namespace PokerTell.Repository.Tests
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Practices.Composite.Events;

    using Moq;

    using global::NHibernate;

    using NUnit.Framework;

    using PokerTell.Infrastructure.Interfaces.DatabaseSetup;
    using PokerTell.Infrastructure.Interfaces.PokerHand;
    using PokerTell.Infrastructure.Interfaces.Repository;
    using PokerTell.Repository.Interfaces;
    using PokerTell.UnitTests.Tools;

    [TestFixture]
    public class RepositoryTests
    {
        #region Constants and Fields

        Mock<IConvertedPokerHandDao> _pokerHandDaoStub;

        StubBuilder _stub;

        Mock<ITransactionManager> _transactionManagerMock;

        IRepository _sut;

        Mock<IConvertedPokerPlayerDao> _pokerPlayerDaoStub;
        
        Mock<IPlayerIdentityDao> _playerIdentityDaoStub;

        #endregion

        #region Public Methods

        [SetUp]
        public void _Init()
        {
            _stub = new StubBuilder();

            _pokerHandDaoStub = new Mock<IConvertedPokerHandDao>();
            _pokerPlayerDaoStub = new Mock<IConvertedPokerPlayerDao>();
            _playerIdentityDaoStub = new Mock<IPlayerIdentityDao>();

            _transactionManagerMock = new Mock<ITransactionManager>();
           
            _transactionManagerMock
                .Setup(tm => tm.BatchExecute(It.IsAny<Action<IStatelessSession>>()))
                .Returns(_transactionManagerMock.Object);
            _transactionManagerMock
                .Setup(tm => tm.Execute(It.IsAny<Action>()))
                .Returns(_transactionManagerMock.Object);

            _sut = new Repository(_pokerHandDaoStub.Object,
                                  _pokerPlayerDaoStub.Object, 
                                  _playerIdentityDaoStub.Object,
                                  _transactionManagerMock.Object,
                                  _stub.Out<IRepositoryParser>());
        }

       [Test]
       public void InsertHand_Always_ExecutesTransaction()
       {
           _sut.InsertHand(_stub.Out<IConvertedPokerHand>());

           _transactionManagerMock.Verify(tx => tx.Execute(It.IsAny<Action>()));
       }

       [Test]
       public void InsertHands_TwoHands_BatchExecutesTransactionOnlyOnce()
       {
           IList<IConvertedPokerHand> handsToInsert = new[]
               {
                   _stub.Out<IConvertedPokerHand>(), _stub.Out<IConvertedPokerHand>()
               };

           _sut.InsertHands(handsToInsert);

           _transactionManagerMock.Verify(tm => tm.BatchExecute(It.IsAny<Action<IStatelessSession>>()), Times.Once());
       }

       [Test]
       public void RetrieveConvertedHand_Always_ExecutesRetrieveOnTransactionManager()
       {
           _sut.RetrieveConvertedHand(1);

           _transactionManagerMock.Verify(tm => tm.Execute(It.IsAny<Func<IConvertedPokerHand>>()));
       }

       [Test]
       public void RetrieveConvertedHandWith_Always_ExecutesRetrieveOnTransactionManager()
       {
           _sut.RetrieveConvertedHandWith(1, "someSite");

           _transactionManagerMock.Verify(tm => tm.Execute(It.IsAny<Func<IConvertedPokerHand>>()));
       }
      
        #endregion
    }
}

