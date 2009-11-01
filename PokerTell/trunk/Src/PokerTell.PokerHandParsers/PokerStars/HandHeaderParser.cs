namespace PokerTell.PokerHandParsers.PokerStars
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class HandHeaderParser : PokerHandParsers.HandHeaderParser
    {
     const string HeaderPattern =
            @"PokerStars Game [#](?<GameId>[0-9]+)[:] (Tournament [#](?<TournamentId>[0-9]+)){0,1}.*(Hold'em|Holdem) (No |Pot )*Limit";
        
        public override PokerHandParsers.HandHeaderParser Parse(string handHistory)
        {
            var header = MatchHeader(handHistory);
            IsValid = header.Success;

            if (IsValid)
            {
                GameId = ExtractGameId(header);
                ExtractTournamentId(header);
            }

            return this;
        }

        public override IList<HeaderMatchInformation> FindAllHeaders(string handHistories)
        {
            var allHeaders = Regex.Matches(handHistories, HeaderPattern, RegexOptions.IgnoreCase);

            return (from Match header in allHeaders
                    let gameId = ExtractGameId(header)
                    select new HeaderMatchInformation(gameId, header)).ToList();
        }

        static Match MatchHeader(string handHistory)
        {
            return Regex.Match(handHistory, HeaderPattern, RegexOptions.IgnoreCase);
        }

        static ulong ExtractGameId(Match header)
        {
            return ulong.Parse(header.Groups["GameId"].Value);
        }

        void ExtractTournamentId(Match header)
        {
            IsTournament = header.Groups["TournamentId"].Value != string.Empty;
            TournamentId = IsTournament
                               ? ulong.Parse(header.Groups["TournamentId"].Value)
                               : 0;
        }

        
    }
}