namespace PokerTell.PokerHandParsers.Tests.Base
{
    using NUnit.Framework;

    using PokerTell.PokerHandParsers.Base;

    public abstract class AnteParserTests
    {
        const double Ante = 1.0;

        AnteParser _parser;

        [SetUp]
        public void _Init()
        {
            _parser = GetAnteParser();
        }

        [Test]
        public void Parse_EmptyString_IsValidIsFalse()
        {
            _parser.Parse(string.Empty);
            Assert.That(_parser.IsValid, Is.False);
        }

        [Test]
        public void Parse_HandHistoryWithoutValidAnte_IsValidIsFalse()
        {
            _parser.Parse("this is invalid");
            Assert.That(_parser.IsValid, Is.False);
        }

        [Test]
        public void Parse_HandHistoryWithValidTournamentAnte_IsValidIsTrue()
        {
            string validTotalSeats = ValidTournamentAnte(Ante);
            _parser.Parse(validTotalSeats);
            Assert.That(_parser.IsValid, Is.True);
        }

        [Test]
        public void Parse_HandHistoryWithValidTournamentAnte_ExtractsAnte()
        {
            string validTotalSeats = ValidTournamentAnte(Ante);
            _parser.Parse(validTotalSeats);
            Assert.That(_parser.Ante, Is.EqualTo(Ante));
        }

        protected abstract AnteParser GetAnteParser();

        protected abstract string ValidTournamentAnte(double ante);
    }
}