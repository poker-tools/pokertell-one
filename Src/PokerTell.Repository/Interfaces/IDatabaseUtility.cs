namespace PokerTell.Repository.Interfaces
{
    using Infrastructure.Interfaces.DatabaseSetup;

    public interface IDatabaseUtility
    {
        IDatabaseUtility Use(IDataProvider dataProvider);

        int GetIdentityOfLastInsertedHand();

        int? GetHandIdForHandWith(ulong gameId, string site);
    }
}