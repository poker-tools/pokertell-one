namespace PokerTell.Infrastructure.Interfaces.Repository
{
    using System.Collections.Generic;

    using DatabaseSetup;

    using NHibernate;

    using PokerHand;

    public interface IRepository
    {
        IConvertedPokerHand RetrieveConvertedHand(int handId);

        IEnumerable<IConvertedPokerHand> RetrieveConvertedHands(IEnumerable<int> handIds);

        IRepository Use(IDataProvider dataProvider);

        IEnumerable<IConvertedPokerHand> RetrieveHandsFromFile(string fileName);

        IRepository InsertHand(IConvertedPokerHand convertedPokerHand);

        IRepository InsertHands(IEnumerable<IConvertedPokerHand> handsToInsert);

        IConvertedPokerHand RetrieveConvertedHandWith(ulong gameId, string site);
    }
}