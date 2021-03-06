namespace PokerTell.Statistics.Design.DetailedStatistics
{
    using Infrastructure.Enumerations.PokerHand;

    using PokerTell.Statistics.ViewModels.StatisticsSetSummary;

    public static class StatisticsSetSummaryDesignModel 
    {
        public static StatisticsSetSummaryViewModel GetReactionStatisticsSetSummaryDesignModel(int callPercentage, int raisePercentage)
        {
            var rowViewModel1 = new StatisticsSetSummaryRowViewModel(ActionSequences.HeroF, new BarGraphViewModel());
            var rowViewModel2 = new StatisticsSetSummaryRowViewModel(ActionSequences.HeroC, new BarGraphViewModel());
            var rowViewModel3 = new StatisticsSetSummaryRowViewModel(ActionSequences.HeroR, new BarGraphViewModel());

            rowViewModel1.UpdateWith(54, new[] { 20, 40, 30, 50, 10, 5 });
            rowViewModel2.UpdateWith(callPercentage, new[] { 20, 40, 30, 50, 10, 5 });
            rowViewModel3.UpdateWith(raisePercentage, new[] { 20, 40, 30, 50, 10, 5 });

            var designModel = new StatisticsSetSummaryViewModel();
            designModel.Rows.Add(rowViewModel1);
            designModel.Rows.Add(rowViewModel2);
            designModel.Rows.Add(rowViewModel3);

            return designModel;
        }

        public static StatisticsSetSummaryViewModel GetHeroXOrHeroBSetSummaryDesignModel(int betPercentage)
        {
            var rowViewModel1 = new StatisticsSetSummaryRowViewModel(ActionSequences.HeroX, new BarGraphViewModel());
            var rowViewModel2 = new StatisticsSetSummaryRowViewModel(ActionSequences.HeroB, new BarGraphViewModel());

            rowViewModel1.UpdateWith(54, new[] { 100 });
            rowViewModel2.UpdateWith(betPercentage, new[] { 20, 40, 30, 50, 10, 5 });

            var designModel = new StatisticsSetSummaryViewModel();
            designModel.Rows.Add(rowViewModel1);
            designModel.Rows.Add(rowViewModel2);

            return designModel;
        }
    }
}