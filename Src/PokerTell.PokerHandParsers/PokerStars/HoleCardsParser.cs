namespace PokerTell.PokerHandParsers.PokerStars
{
    using Interfaces.Parsers;

    public class HoleCardsParser : Base.HoleCardsParser, IPokerStarsHoleCardsParser 
    {
        const string DealtToPattern = @"Dealt to ";

        const string ShowsOrMuckedPattern = @".+(showed|mucked)";

        protected override string HeroHoleCardsPattern
        {
            get { return string.Format("{0}{1}{2}", DealtToPattern, _playerName, HoleCardsPattern); }
        }

        protected override string ShownOrMuckedHoleCardsPattern
        {
            get { return string.Format("{0}{1}{2}", _playerName, ShowsOrMuckedPattern, HoleCardsPattern); }
        }
    }

    
}