namespace PokerTell.PokerHand.Tests.Dao
{
    using System;
    using System.Linq;

    using Moq;

    using NHibernate.Tool.hbm2ddl;

    using NUnit.Framework;

    using PokerTell.Infrastructure.Enumerations.PokerHand;
    using PokerTell.Infrastructure.Interfaces.PokerHand;
    using PokerTell.Infrastructure.Interfaces.Repository;
    using PokerTell.PokerHand.Analyzation;
    using PokerTell.PokerHand.Dao;
    using PokerTell.PokerHand.Tests.Base;
    using PokerTell.PokerHand.Tests.Fakes;
    using PokerTell.UnitTests.Tools;

    [TestFixture]
    public class ConvertedPokerHandDaoTests : InMemoryDatabaseTest
    {
        const double BB = 20.0;

        const ulong GameId = 1;

        const double SB = 10.0;

        const string Site = "PokerStars";

        const int TotalPlayers = 9;

        readonly DateTime _timeStamp = DateTime.MinValue;

        IConvertedPokerHand _hand;

        IConvertedPokerHandDao _sut;

        Mock<IPlayerIdentityDao> _playerIdentityDaoStub;

        public ConvertedPokerHandDaoTests()
            : base(typeof(ConvertedPokerHand).Assembly)
        {
        }

        [SetUp]
        public void _Init()
        {
            new SchemaExport(_configuration).Execute(false, true, false, _session.Connection, null);

            _hand = new ConvertedPokerHand(Site, GameId, _timeStamp, BB, SB, TotalPlayers);

            var sessionFactoryManager = new Mock<ISessionFactoryManager>();
            sessionFactoryManager
                .SetupGet(sfm => sfm.CurrentSession)
                .Returns(_session);

            _playerIdentityDaoStub = new Mock<IPlayerIdentityDao>();
            _sut = new ConvertedPokerHandDao(sessionFactoryManager.Object, _playerIdentityDaoStub.Object);
        }

        [TearDown]
        public void _Dispose()
        {
            _session.Clear();
        }

        [Test]
        public void Insert_HandWithoutPlayers_DoesNotInsertItIntoDatabase()
        {
            _sut.Insert(_hand);

            _hand.Id.ShouldBeEqualTo(UnsavedValue);
        }

        [Test]
        public void Insert_HandWithTwoPlayers_InsertsItIntoDatabase()
        {
            var player1 = new ConvertedPokerPlayer { Name = "player1" };
            var player2 = new ConvertedPokerPlayer { Name = "player2" };
            _playerIdentityDaoStub
                .Setup(pd => pd.FindOrInsert(player1.Name, Site))
                .Returns(new PlayerIdentityStub(player1.Name, Site, 1));
            _playerIdentityDaoStub
                .Setup(pd => pd.FindOrInsert(player2.Name, Site))
                .Returns(new PlayerIdentityStub(player2.Name, Site, 2));

            _hand
                .AddPlayer(player1)
                .AddPlayer(player2);

            _sut.Insert(_hand);

            FlushAndClearSession();

            _hand.Id.ShouldNotBeEqualTo(UnsavedValue);
        }

        [Test]
        public void Insert_HandWithTwoPlayers_SavesBothPlayers()
        {
            var player1 = new ConvertedPokerPlayer { Name = "player1" };
            var player2 = new ConvertedPokerPlayer { Name = "player2" };
            _playerIdentityDaoStub
                .Setup(pd => pd.FindOrInsert(player1.Name, Site))
                .Returns(new PlayerIdentityStub(player1.Name, Site, 1));
            _playerIdentityDaoStub
                .Setup(pd => pd.FindOrInsert(player2.Name, Site))
                .Returns(new PlayerIdentityStub(player2.Name, Site, 2));

            _hand
                .AddPlayer(player1)
                .AddPlayer(player2);

            _sut.Insert(_hand);

            FlushAndClearSession();

            player1.Id.ShouldNotBeEqualTo(UnsavedValue);
            player2.Id.ShouldNotBeEqualTo(UnsavedValue);
        }

        [Test]
        public void Insert_HandWithTwoPlayers_FindsOrInsertsBothPlayerIdentities()
        {
            var player1 = new ConvertedPokerPlayer { Name = "player1" };
            var player2 = new ConvertedPokerPlayer { Name = "player2" };

            var playerIdentityDaoMock = _playerIdentityDaoStub;
            playerIdentityDaoMock
                .Setup(pd => pd.FindOrInsert(player1.Name, Site))
                .Returns(new PlayerIdentityStub(player1.Name, Site, 1));
            playerIdentityDaoMock
                .Setup(pd => pd.FindOrInsert(player2.Name, Site))
                .Returns(new PlayerIdentityStub(player2.Name, Site, 2));

            _hand
                .AddPlayer(player1)
                .AddPlayer(player2);

            _sut.Insert(_hand);

            FlushAndClearSession();

            playerIdentityDaoMock.Verify(pd => pd.FindOrInsert(player1.Name, Site));
            playerIdentityDaoMock.Verify(pd => pd.FindOrInsert(player2.Name, Site));
        }

        [Test]
        public void GetHandWith_HandNotInDatabase_ReturnsNull()
        {
            var retrievedHand = _sut.GetHandWith(GameId, Site);
            retrievedHand.ShouldBeNull();
        }

        [Test]
        public void Get_HandNotInDatabase_ReturnsNull()
        {
            const int someNonExistentId = 1;
            var retrievedHand = _sut.Get(someNonExistentId);
            retrievedHand.ShouldBeNull();
        }

        [Test]
        public void Get_HandWasSaved_ReturnsSavedHand()
        {
            _session.Save(_hand);
            FlushAndClearSession();

            var retrievedHand = _sut.Get(_hand.Id);
            retrievedHand.ShouldBeEqualTo(_hand);
        }

        [Test]
        public void GetHandWith_HandWasSaved_ReturnsSavedHand()
        {
            _session.Save(_hand);
            FlushAndClearSession();

            var retrievedHand = _sut.GetHandWith(GameId, Site);

            retrievedHand.ShouldBeEqualTo(_hand);
        }

        [Test]
        public void Get_HandWithSequencesWasSaved_RestoresSequences()
        {
            var sampleAction = new ConvertedPokerAction(ActionTypes.C, 1.0);
            _hand.Sequences[(int)Streets.PreFlop] =
                new ConvertedPokerRound().Add(new ConvertedPokerActionWithId(sampleAction, 0));
            _hand.Sequences[(int)Streets.Flop] =
                new ConvertedPokerRound().Add(new ConvertedPokerActionWithId(sampleAction, 1));
            _hand.Sequences[(int)Streets.Turn] =
                new ConvertedPokerRound().Add(new ConvertedPokerActionWithId(sampleAction, 2));
            _hand.Sequences[(int)Streets.River] =
                new ConvertedPokerRound().Add(new ConvertedPokerActionWithId(sampleAction, 3));

            _session.Save(_hand);
            FlushAndClearSession();

            IConvertedPokerHand retrievedHand = _sut.Get(_hand.Id);

            retrievedHand.Sequences.ShouldBeEqualTo(_hand.Sequences);
        }

        [Test]
        public void Get_HandWithPlayersInRoundWasSaved_RestoresPlayersInRound()
        {
            _hand.PlayersInRound[(int)Streets.Flop] = 3;
            _hand.PlayersInRound[(int)Streets.Turn] = 2;
            _hand.PlayersInRound[(int)Streets.River] = 1;

            _session.Save(_hand);
            FlushAndClearSession();

            IConvertedPokerHand retrievedHand = _sut.Get(_hand.Id);

            retrievedHand.PlayersInRound.ShouldBeEqualTo(_hand.PlayersInRound);
        }

        [Test]
        public void Get_HandWithTwoPlayerWasSaved_RestoresBothPlayers()
        {
            _hand
                .AddPlayer(SavePlayerIdentityAndReturnPlayerNamed("player1"))
                .AddPlayer(SavePlayerIdentityAndReturnPlayerNamed("player2"));

            _session.Save(_hand);
            FlushAndClearSession();

            IConvertedPokerHand retrievedHand = _sut.Get(_hand.Id);

            retrievedHand.Players.ShouldBeEqualTo(_hand.Players);
        }

        [Test]
        public void Get_HandWithTwoPlayerWasSaved_RestoresBothPlayerIdentitiesAsProxies()
        {
            _hand
                .AddPlayer(SavePlayerIdentityAndReturnPlayerNamed("player1"))
                .AddPlayer(SavePlayerIdentityAndReturnPlayerNamed("player2"));

            _session.Save(_hand);
            FlushAndClearSession();

            IConvertedPokerHand retrievedHand = _sut.Get(_hand.Id);

            retrievedHand.Players.First().PlayerIdentity.GetType().Name.Contains("Proxy").ShouldBeTrue();
            retrievedHand.Players.Last().PlayerIdentity.GetType().Name.Contains("Proxy").ShouldBeTrue();
        }

        [Test]
        public void Get_HandWithTwoPlayerWasSaved_FirstRestoredPlayerIdentityProxyPropertiesAreEqualToStoredPlayerIdentity()
        {
            var player1 = SavePlayerIdentityAndReturnPlayerNamed("player1");
            var player2 = SavePlayerIdentityAndReturnPlayerNamed("player2");

            _hand
                .AddPlayer(player1)
                .AddPlayer(player2);

            _session.Save(_hand);
            FlushAndClearSession();

            IConvertedPokerHand retrievedHand = _sut.Get(_hand.Id);

            var proxiedPlayerIdentity1 = retrievedHand.Players.First().PlayerIdentity;

            proxiedPlayerIdentity1.Id.ShouldBeEqualTo(player1.PlayerIdentity.Id);
            proxiedPlayerIdentity1.Name.ShouldBeEqualTo(player1.PlayerIdentity.Name);
            proxiedPlayerIdentity1.Site.ShouldBeEqualTo(player1.PlayerIdentity.Site);
        }

        IConvertedPokerPlayer SavePlayerIdentityAndReturnPlayerNamed(string name)
        {
            IConvertedPokerPlayer player = new ConvertedPokerPlayer
                {
                    Name = name, 
                    PlayerIdentity = new PlayerIdentity(name, Site)
                };

            _session.Save(player.PlayerIdentity);

            return player;
        }
    }
}