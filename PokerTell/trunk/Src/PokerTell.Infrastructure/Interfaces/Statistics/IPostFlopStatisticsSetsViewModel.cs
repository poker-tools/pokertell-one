namespace PokerTell.Infrastructure.Interfaces.Statistics
{
    using System.Collections.Generic;

    public interface IPostFlopStatisticsSetsViewModel : IFluentInterface, IEnumerable<IStatisticsSetSummaryViewModel>
    {
        #region Properties

        IStatisticsSetSummaryViewModel HeroXOrHeroBOutOfPositionStatisticsSet { get; }

        IStatisticsSetSummaryViewModel HeroXOrHeroBInPositionStatisticsSet { get; }

        IStatisticsSetSummaryViewModel OppBIntoHeroOutOfPositionStatisticsSet { get; }

        IStatisticsSetSummaryViewModel OppBIntoHeroInPositionStatisticsSet { get; }

        IStatisticsSetSummaryViewModel HeroXOutOfPositionOppBStatisticsSet { get; }

        int TotalCountOutOfPosition { get; }

        int TotalCountInPosition { get; }

        #endregion

        IPostFlopStatisticsSetsViewModel UpdateWith(IPlayerStatistics playerStatistics);
    }
}