namespace PokerTell.Statistics.Analyzation
{
    using System.Collections.Generic;
    using System.Linq;

    using PokerTell.Statistics.Interfaces;

    using Tools.FunctionalCSharp;

    public class PreFlopStartingHandsVisualizer : IPreFlopStartingHandsVisualizer
    {
        static readonly char[] CardRanks = { 'A', 'K', 'Q', 'J', 'T', '9', '8', '7', '6', '5', '4', '3', '2' };

        int _sideLength;

        public IPreFlopStartingHandsVisualizer InitializeWith(int sideLength, int pairMargin)
        {
            _sideLength = sideLength;
           
            StartingHands = new Dictionary<string, IStartingHand>();

            // Offsuit - including pairs
            for (int y = 0; y < CardRanks.Length; y++)
            {
                for (int x = 0; x <= y; x++)
                {
                    // Set the pairs off from the rest
                    int pairOffset = (x == y) ? pairMargin : 0;

                    var left = (x * _sideLength) + pairOffset;
                    var top = y * _sideLength;
                    var display = string.Format("{0}{1}", CardRanks[x], CardRanks[y]);
                    var startingHand = new StartingHand(top, left, _sideLength, display);

                    StartingHands.Add(startingHand.Display, startingHand);
                }
            }

            // Suited Cards
            for (int y = 0; y < CardRanks.Length; y++)
            {
                for (int x = y + 1; x < CardRanks.Length; x++)
                {
                    int pairOffset = pairMargin * 2;

                    var left = (x * _sideLength) + pairOffset;
                    var top = y * _sideLength;
                    var display = string.Format("{0}{1}", CardRanks[y], CardRanks[x]);
                    var startingHand = new StartingHand(top, left, _sideLength, display);

                    StartingHands.Add(startingHand.Display + "s", startingHand);
                }
            }

            return this;
        }

        public IPreFlopStartingHandsVisualizer Visualize(IEnumerable<string> holeCards)
        {
            CountOccurencesOfEachStartingHand(holeCards);

            MapStartingHandCountsToTheirFillHeights();

            return this;
        }

        void MapStartingHandCountsToTheirFillHeights()
        {
            var maxCount = StartingHands.Values.Max(sh => sh.Count);
            var fillHeightRatio = (double)_sideLength / maxCount;
            StartingHands.Values.ForEach(sh => sh.FillHeight = sh.Count * fillHeightRatio);
        }

        void CountOccurencesOfEachStartingHand(IEnumerable<string> holeCards)
        {
            StartingHands.Values.ForEach(sh => sh.Count = 0);
            holeCards.ForEach(card => StartingHands[card].Count++);
        }

        public IDictionary<string, IStartingHand> StartingHands { get; protected set; }
    }
}