namespace PokerTell.LiveTracker.DesignWithDatabase
{
    using System;
    using System.Windows;
    using Microsoft.Practices.Composite.Events;
    using Microsoft.Practices.Unity;
    using Moq;
    using PokerTell.Infrastructure.Interfaces;
    using PokerTell.Infrastructure.Interfaces.PokerHand;
    using PokerTell.Infrastructure.Interfaces.Repository;
    using PokerTell.Infrastructure.Interfaces.Statistics;
    using PokerTell.Infrastructure.Services;
    using PokerTell.IntegrationTests;
    using PokerTell.LiveTracker.DesignWithDatabase.DesignWindows;
    using PokerTell.LiveTracker.ViewModels;
    using PokerTell.PokerHand.Analyzation;
    using PokerTell.PokerHand.Dao;
    using PokerTell.PokerHand.ViewModels.Design;
    using PokerTell.Repository;
    using PokerTell.Repository.Interfaces;
    using PokerTell.Repository.NHibernate;
    using PokerTell.Statistics;
    using PokerTell.Statistics.Analyzation;
    using PokerTell.Statistics.Detailed;
    using PokerTell.Statistics.Interfaces;
    using PokerTell.Statistics.ViewModels;
    using PokerTell.Statistics.ViewModels.Analyzation;
    using PokerTell.Statistics.ViewModels.StatisticsSetDetails;
    using Tools.WPF.Interfaces;

    public class TableStatisticsViewWithDatabaseTests : DatabaseConnectedPerformanceTests
    {
        const string PokerStars = "PokerStars";

        IUnityContainer _container;

        StubBuilder _stub;

        public TableStatisticsViewWithDatabaseTests()
        {
            _Init();
        }

        public Window ConnectToDataBase_LoadSomePLayers_ShowTheirStatistics_InTheLiveStats()
        {
            Func<string, IPlayerStatistics> get =
                playerName => _container
                    .Resolve<IPlayerStatistics>()
                    .InitializePlayer(playerName, PokerStars)
                    .UpdateStatistics();

            const string salemorguy = "salemorguy";
            const string greystoke = "Greystoke-11";

            // const string renniweg = "renniweg";
            var eventAggregator = new EventAggregator();

            var detailedPreFlopStatisticsViewModelMake =
                new Constructor<IDetailedPreFlopStatisticsViewModel>(() => _container.Resolve<DetailedPreFlopStatisticsViewModel>());
            var detailedPostFlopHeroActsStatisticsViewModelMake =
                new Constructor<IDetailedPostFlopHeroActsStatisticsViewModel>(
                    () => _container.Resolve<DetailedPostFlopHeroActsStatisticsViewModel>());

            var detailedPostFlopHeroReactsStatisticsViewModelMake =
                new Constructor<IDetailedPostFlopHeroReactsStatisticsViewModel>(
                    () => _container.Resolve<DetailedPostFlopHeroReactsStatisticsViewModel>());

            var detailedStatisticsAnalyzerViewModel =
                new DetailedStatisticsAnalyzerViewModel(
                    detailedPreFlopStatisticsViewModelMake, 
                    detailedPostFlopHeroActsStatisticsViewModelMake, 
                    detailedPostFlopHeroReactsStatisticsViewModelMake, 
                    new Mock<IRepositoryHandBrowserViewModel>().Object);

            // TODO: Before this test will work again, we will have to specify the settings and dimensions mocks otherwise it will either
            // fail or have size 0, 0
            var tableStatisticsViewModel = new PokerTableStatisticsViewModel(
                new Mock<ISettings>().Object, 
                new Mock<IDimensionsViewModel>().Object, 
                new Constructor<IPlayerStatisticsViewModel>(() => _container.Resolve<IPlayerStatisticsViewModel>()), 
                detailedStatisticsAnalyzerViewModel, 
                new Mock<IActiveAnalyzablePlayersSelector>().Object, 
                new Mock<IFilterPopupViewModel>().Object);
            var designWindow = new TableStatisticsDesignWindow(
                eventAggregator, 
                _container.Resolve<IRepositoryHandBrowserViewModel>())
                {
                    Topmost = true, 
                    DataContext = tableStatisticsViewModel
                };

            tableStatisticsViewModel.UpdateWith(
                new[]
                    {
                        // get(renniweg), 
                        get(greystoke), 
                        get(salemorguy)
                    });

            designWindow.ShowDialog();
            return designWindow;
        }

        public void _Init()
        {
            _stub = new StubBuilder();

            SetupMySqlConnection("data source = localhost; user id = root; database=firstnh;");
            _container = new UnityContainer()
                .RegisterInstance<IEventAggregator>(new EventAggregator())

                // Converted Constructors
                .RegisterConstructor<IConvertedPokerAction, ConvertedPokerAction>()
                .RegisterConstructor<IConvertedPokerActionWithId, ConvertedPokerActionWithId>()
                .RegisterConstructor<IConvertedPokerRound, ConvertedPokerRound>()
                .RegisterConstructor<IConvertedPokerPlayer, ConvertedPokerPlayer>()
                .RegisterConstructor<IConvertedPokerHand, ConvertedPokerHand>()

                // Daos 
                .RegisterType<IPlayerIdentityDao, PlayerIdentityDao>()
                .RegisterType<IConvertedPokerPlayerDao, ConvertedPokerPlayerDao>()
                .RegisterType<IConvertedPokerHandDao, ConvertedPokerHandDao>()
               
                // Repository
                .RegisterInstance(_stub.Out<IRepositoryParser>())
                .RegisterType<ITransactionManager, TransactionManager>()
                .RegisterType<IRepository, Repository>()

                // Database
                .RegisterInstance(_sessionFactoryManagerStub.Object)

                /*
                 * Statistics
                */
                .RegisterType<IPlayerStatistics, PlayerStatistics>()

                // RaiseReactionAnalyzation
                .RegisterType<IReactionAnalyzationPreparer, ReactionAnalyzationPreparer>()
                .RegisterTypeAndConstructor<IRaiseReactionAnalyzer, RaiseReactionAnalyzer>()
                .RegisterType<IRaiseReactionsAnalyzer, RaiseReactionsAnalyzer>()
                .RegisterType<IRaiseReactionStatistics, RaiseReactionStatistics>()
                .RegisterType<IRaiseReactionStatisticsBuilder, RaiseReactionStatisticsBuilder>()

                // RepositoryHandBrowser
                .RegisterType<IRepositoryHandBrowser, RepositoryHandBrowser>()
                .RegisterType<IHandHistoryViewModel, HandHistoryViewModel>()
                .RegisterType<IRepositoryHandBrowserViewModel, RepositoryHandBrowserViewModel>()

                // Detailed Statistics Describers
                .RegisterType<IDetailedPreFlopStatisticsDescriber, DetailedPreFlopStatisticsDescriber>()
                .RegisterType<IDetailedPostFlopHeroActsStatisticsDescriber, DetailedPostFlopHeroActsStatisticsDescriber>()
                .RegisterType<IDetailedPostFlopHeroReactsStatisticsDescriber, DetailedPostFlopHeroReactsStatisticsDescriber>()

                // Raise Reaction Statistics Describers
                .RegisterType<IPreFlopRaiseReactionDescriber, PreFlopRaiseReactionDescriber>()
                .RegisterType<IPostFlopHeroActsRaiseReactionDescriber, PostFlopHeroActsRaiseReactionDescriber>()
                .RegisterType<IPostFlopHeroReactsRaiseReactionDescriber, PostFlopHeroReactsRaiseReactionDescriber>()

                // Raise Reaction Statistics ViewModels
                .RegisterType<IPreFlopRaiseReactionStatisticsViewModel, PreFlopRaiseReactionStatisticsViewModel>()
                .RegisterType<IPostFlopHeroActsRaiseReactionStatisticsViewModel, PostFlopHeroActsRaiseReactionStatisticsViewModel>()
                .RegisterType<IPostFlopHeroReactsRaiseReactionStatisticsViewModel, PostFlopHeroReactsRaiseReactionStatisticsViewModel>()

                // Valued HoleCards
                .RegisterConstructor<IValuedHoleCards, ValuedHoleCards>()
                .RegisterType<IValuedHoleCardsAverage, ValuedHoleCardsAverage>()

                // Preflop HandStrength Statistics
                .RegisterType<IPreFlopHandStrengthStatistics, PreFlopHandStrengthStatistics>()

                // Preflop HandStrengths Statistics Describers
                .RegisterType<IPreFlopUnraisedPotCallingHandStrengthDescriber, PreFlopUnraisedPotCallingHandStrengthDescriber>()
                .RegisterType<IPreFlopRaisedPotCallingHandStrengthDescriber, PreFlopRaisedPotCallingHandStrengthDescriber>()
                .RegisterType<IPreFlopRaisingHandStrengthDescriber, PreFlopRaisingHandStrengthDescriber>()

                // Preflop HandStrengths Statistics ViewModels
                .RegisterType<IPreFlopUnraisedPotCallingHandStrengthStatisticsViewModel, PreFlopUnraisedPotCallingHandStrengthStatisticsViewModel>()
                .RegisterType<IPreFlopRaisedPotCallingHandStrengthStatisticsViewModel, PreFlopRaisedPotCallingHandStrengthStatisticsViewModel>()
                .RegisterType<IPreFlopRaisingHandStrengthStatisticsViewModel, PreFlopRaisingHandStrengthStatisticsViewModel>()

                // Preflop HandStrengths Visualizing
                .RegisterType<IPreFlopStartingHandsVisualizer, PreFlopStartingHandsVisualizer>()
                .RegisterType<IPreFlopStartingHandsVisualizerViewModel, PreFlopStartingHandsVisualizerViewModel>();
        }
    }
}