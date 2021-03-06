namespace PokerTell.PokerHandParsers.Tests.Base
{
    using NUnit.Framework;

    using PokerTell.PokerHandParsers.Base;

    public abstract class TableNameParserTests
    {
        const string TableName = "SomeTable";

        TableNameParser _parser;

        [SetUp]
        public void _Init()
        {
            _parser = GetTableNameParser();
        }

        [Test]
        public void Parse_EmptyString_IsValidIsFalse()
        {
            _parser.Parse(string.Empty);
            Assert.That(_parser.IsValid, Is.False);
        }

        [Test]
        public void Parse_HandHistoryWithoutValidTableName_IsValidIsFalse()
        {
            _parser.Parse("this is invalid");
            Assert.That(_parser.IsValid, Is.False);
        }

        [Test]
        public void Parse_HandHistoryWithValidTableName_IsValidIsTrue()
        {
            var validTableName = ValidTableName(TableName);
            _parser.Parse(validTableName);
            Assert.That(_parser.IsValid, Is.True);
        }

        [Test]
        public void Parse_HandHistoryWithValidTableName_ExtractsTableName()
        {
            var validTableName = ValidTableName(TableName);
            _parser.Parse(validTableName);
            Assert.That(_parser.TableName, Is.EqualTo(TableName));
        }

        [Test]
        public void Parse_HandHistoryWithValidTableNameWithSpacesInName_ExtractsTableName()
        {
            const string tableNameWithSpaces = "My Name has spaces";
            var validTableName = ValidTableName(tableNameWithSpaces);
            _parser.Parse(validTableName);
            Assert.That(_parser.TableName, Is.EqualTo(tableNameWithSpaces));
        }

        protected abstract string ValidTableName(string tableName);

        protected abstract TableNameParser GetTableNameParser();
    }
}