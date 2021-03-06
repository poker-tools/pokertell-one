namespace PokerTell.LiveTracker.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using Infrastructure.Enumerations.PokerHand;
    using Infrastructure.Interfaces;
    using Infrastructure.Interfaces.PokerHand;
    using Infrastructure.Interfaces.Statistics;

    using LiveTracker.ViewModels;
    using LiveTracker.ViewModels.Overlay;

    using Machine.Specifications;

    using Microsoft.Practices.Composite.Events;

    using Moq;

    using Tools.WPF.Interfaces;

    using Utilities;

    using It = Machine.Specifications.It;

    // Resharper disable InconsistentNaming
    public abstract class PokerTableStatisticsViewModelSpecs
    {
        protected const string PlayerName = "someName";

        protected static Mock<IFilterPopupViewModel> _filterPopupVM_Mock;

        protected static Mock<IConstructor<IPlayerStatisticsViewModel>> _playerStatisticsMake_Stub;

        protected static Mock<IDetailedStatisticsAnalyzerViewModel> _statisticsAnalyzer_Mock;

        protected static Mock<ISettings> _settings_Mock;

        protected static Mock<IDimensionsViewModel> _dimensionsVM_Mock;

        protected static Mock<IActiveAnalyzablePlayersSelector> _activePlayersSelector_Mock;

        protected static PokerTableStatisticsViewModelSut _sut;

        static Mock<IPlayerStatisticsViewModel> _playerStatisticsVM_Stub;

        Establish specContext = () => {
            _filterPopupVM_Mock = new Mock<IFilterPopupViewModel>();

            _playerStatisticsVM_Stub = new Mock<IPlayerStatisticsViewModel>();
            _playerStatisticsVM_Stub
                .SetupGet(vm => vm.PlayerName)
                .Returns(PlayerName);

            _playerStatisticsMake_Stub = new Mock<IConstructor<IPlayerStatisticsViewModel>>();
            _playerStatisticsMake_Stub
                .SetupGet(svm => svm.New).Returns(_playerStatisticsVM_Stub.Object);

            _statisticsAnalyzer_Mock = new Mock<IDetailedStatisticsAnalyzerViewModel>();

            _settings_Mock = new Mock<ISettings>();
            _dimensionsVM_Mock = new Mock<IDimensionsViewModel>();
            _dimensionsVM_Mock
                .Setup(d => d.InitializeWith(Moq.It.IsAny<Rectangle>()))
                .Returns(_dimensionsVM_Mock.Object);

            _activePlayersSelector_Mock = new Mock<IActiveAnalyzablePlayersSelector>();

            _sut = new PokerTableStatisticsViewModelSut(_settings_Mock.Object,
                                                        _dimensionsVM_Mock.Object,
                                                        _playerStatisticsMake_Stub.Object,
                                                        _statisticsAnalyzer_Mock.Object,
                                                        _activePlayersSelector_Mock.Object,
                                                        _filterPopupVM_Mock.Object);
        };

        [Subject(typeof(PokerTableStatisticsViewModel), "Instantiation")]
        public class when_it_is_instantiated : PokerTableStatisticsViewModelSpecs
        {
            static Rectangle returnedRectangle;

            Establish context = () => {
                returnedRectangle = new Rectangle(1, 1, 2, 2);
                _settings_Mock
                    .Setup(s => s.RetrieveRectangle(PokerTableStatisticsViewModel.DimensionsKey, Moq.It.IsAny<Rectangle>()))
                    .Returns(returnedRectangle);
            };

            Because of = () => _sut = new PokerTableStatisticsViewModelSut(_settings_Mock.Object,
                                                                           _dimensionsVM_Mock.Object,
                                                                           _playerStatisticsMake_Stub.Object,
                                                                           _statisticsAnalyzer_Mock.Object,
                                                                           _activePlayersSelector_Mock.Object,
                                                                           _filterPopupVM_Mock.Object);

            It should_ask_the_settings_for_its_dimensions_with_a_default_value
                = () => _settings_Mock.Verify(s => s.RetrieveRectangle(PokerTableStatisticsViewModel.DimensionsKey, Moq.It.IsAny<Rectangle>()));

            It should_initialize_the_dimensions_with_the_rectangle_returned_by_the_settings
                = () => _dimensionsVM_Mock.Verify(d => d.InitializeWith(returnedRectangle));

            It should_assing_its_dimensions_to_the_initialized_dimensions = () => _sut.Dimensions.ShouldEqual(_dimensionsVM_Mock.Object);
        }

        [Subject(typeof(PokerTableStatisticsViewModel), "Save Dimensions")]
        public class when_told_to_save_its_dimensions : PokerTableStatisticsViewModelSpecs
        {
            static Rectangle returnedRectangle;

            Establish context = () => {
                returnedRectangle = new Rectangle(1, 1, 2, 2);
                _dimensionsVM_Mock
                    .SetupGet(d => d.Rectangle)
                    .Returns(returnedRectangle);
                _sut.Dimensions_Set = _dimensionsVM_Mock.Object;
            };

            Because of = () => _sut.SaveDimensions(); 

            It should_tell_the_settings_to_set_the_rectangle_for_its_key_to_the_one_returned_by_its_dimensions
                = () => _settings_Mock.Verify(s => s.Set(PokerTableStatisticsViewModel.DimensionsKey, returnedRectangle));
        }

        [Subject(typeof(PokerTableStatisticsViewModel), "GetPlayerStatisticsViewModelFor")]
        public class when_only_bob_and_ted_where_added : PokerTableStatisticsViewModelSpecs
        {
            const string bobsName = "bob";
            const string tedsName = "ted";

            static Mock<IPlayerStatisticsViewModel> bob_Stub;

            static Mock<IPlayerStatisticsViewModel> ted_Stub;

            Establish context = () => {
                bob_Stub = Utils.PlayerStatisticsVM_MockFor(bobsName);
                ted_Stub = Utils.PlayerStatisticsVM_MockFor(tedsName);
            };

            Because of = () => {
                _sut.Players.Add(bob_Stub.Object);
                _sut.Players.Add(ted_Stub.Object);
            };

            It should_return_bobs_viewmodel_when_passing_bobs_name = () => _sut.GetPlayerStatisticsViewModelFor(bobsName).ShouldBeTheSameAs(bob_Stub.Object);
            
            It should_return_teds_viewmodel_when_passing_teds_name = () => _sut.GetPlayerStatisticsViewModelFor(tedsName).ShouldBeTheSameAs(ted_Stub.Object);

            It should_return_null_when_passing_some_other_name = () => _sut.GetPlayerStatisticsViewModelFor("neverAdded").ShouldBeNull();
        }

        [Subject(typeof(PokerTableStatisticsViewModel), "UpdateWith")]
        public class when_its_statistics_were_updated : PokerTableStatisticsViewModelSpecs
        {
            static bool statisticsWereUpdatedWasRaised;

            static IEnumerable<IPlayerStatistics> playersStatistics_Stub;

            Establish context = () => {
                playersStatistics_Stub = new[] { Utils.PlayerStatisticsStubFor("p1"), Utils.PlayerStatisticsStubFor("p1") };
                _sut.PlayersStatisticsWereUpdated += () => statisticsWereUpdatedWasRaised = true;
            };

            Because of = () => _sut.UpdateWith(playersStatistics_Stub);

            It should_raise_PlayersStatisticsWereUpdated_event = () => statisticsWereUpdatedWasRaised.ShouldBeTrue();
        }

        [Subject(typeof(PokerTableStatisticsViewModel), "SelectedStatisticsSet")]
        public class when_the_user_selects_a_statistics_set_of_a_player : PokerTableStatisticsViewModelSpecs
        {
            static bool selectedStatisticsWasReraised;

            static Mock<IPlayerStatisticsViewModel> playerStatisticsVM_Stub;

            static Mock<IActionSequenceStatisticsSet> statisticsSet;

            Establish context = () => {
                playerStatisticsVM_Stub = _playerStatisticsVM_Stub;
                _playerStatisticsMake_Stub.SetupGet(make => make.New).Returns(playerStatisticsVM_Stub.Object);
                
                statisticsSet = new Mock<IActionSequenceStatisticsSet>();

                _sut.AddNewPlayerToPlayersIfNotFound_Invoke(null);
                _sut.UserSelectedStatisticsSet += _ => selectedStatisticsWasReraised = true;
            };

            Because of = () => playerStatisticsVM_Stub.Raise(ps => ps.SelectedStatisticsSetEvent += null, statisticsSet.Object);

            It should_initialize_the_detailed_statistics_analyzer_with_the_statistics_Set
                = () => _statisticsAnalyzer_Mock.Verify(sa => sa.InitializeWith(statisticsSet.Object));

            It should_let_me_know = () => selectedStatisticsWasReraised.ShouldBeTrue();
        }


        [Subject(typeof(PokerTableStatisticsViewModel), "Selected Player")]
        public class when_the_a_player_is_selected : PokerTableStatisticsViewModelSpecs
        {
            Because of = () => _sut.SelectedPlayer = _playerStatisticsVM_Stub.Object;

            It should_browse_the_hands_of_the_player = () => _sut.PlayerWhoseHandsWereBrowsed.ShouldEqual(_playerStatisticsVM_Stub.Object);
        }

        [Subject(typeof(PokerTableStatisticsViewModel), "BrowseAllHandsOfSelectedPlayerCommand")]
        public class when_the_use_wants_to_browse_all_hands_of_the_selected_player : PokerTableStatisticsViewModelSpecs
        {
            Establish context = () => {
                _sut.SelectedPlayer = _playerStatisticsVM_Stub.Object;
                _sut.PlayerWhoseHandsWereBrowsed = null;
            };

            Because of = () => _sut.BrowseAllHandsOfSelectedPlayerCommand.Execute(null);

            It should_browse_the_hands_of_the_player = () => _sut.PlayerWhoseHandsWereBrowsed.ShouldEqual(_playerStatisticsVM_Stub.Object);
        }

        [Subject(typeof(PokerTableStatisticsViewModel), "BrowseAllHands")]
        public class when_a_PlayerStatistics_viewmodel_says_that_the_user_wants_to_browse_all_hands_of_the_player : PokerTableStatisticsViewModelSpecs
        {
            static bool browseHandsWasReraised;

            Establish context = () => {
                _sut.UserBrowsedAllHands += _ => browseHandsWasReraised = true;

                _sut.AddNewPlayerToPlayersIfNotFound_Invoke(null);
            };

            Because of = () => _playerStatisticsVM_Stub.Raise(psvm => psvm.BrowseAllMyHandsRequested += null, _playerStatisticsVM_Stub.Object);

            It should_let_me_know = () => browseHandsWasReraised.ShouldBeTrue();

            It should_browse_the_hands_of_the_player = () => _sut.PlayerWhoseHandsWereBrowsed.ShouldEqual(_playerStatisticsVM_Stub.Object);
        }

        [Subject(typeof(PokerTableStatisticsViewModelSpecs), "BrowseAllHands")]
        public class when_told_to_browse_all_hands_of_a_player_and_the_active_player_selector_returns_empty_list : PokerTableStatisticsViewModelSpecs
        {
            static Mock<IPlayerStatistics> statisticsOfSelectedPlayer_Stub;

            static IEnumerable<IAnalyzablePokerPlayer> returnedActivePlayers;

            Establish context = () => {
                returnedActivePlayers = Enumerable.Empty<IAnalyzablePokerPlayer>();

                statisticsOfSelectedPlayer_Stub = new Mock<IPlayerStatistics>();
                _playerStatisticsVM_Stub
                    .SetupGet(pvm => pvm.PlayerStatistics)
                    .Returns(statisticsOfSelectedPlayer_Stub.Object);

                _activePlayersSelector_Mock
                    .Setup(ap => ap.SelectFrom(statisticsOfSelectedPlayer_Stub.Object))
                    .Returns(returnedActivePlayers);
            };

            Because of = () => _sut.BrowseAllHandsOf_Invoke(_playerStatisticsVM_Stub.Object);

            It should_select_the_active_players_from_the_player_statistics
                = () => _activePlayersSelector_Mock.Verify(ps => ps.SelectFrom(statisticsOfSelectedPlayer_Stub.Object));

            It should_not_initialize_the_detailed_statistics_analyzer_with_the_returned_active_players
                = () => _statisticsAnalyzer_Mock.Verify(dsa => dsa.InitializeWith(returnedActivePlayers, PlayerName), Times.Never());
        }

        [Subject(typeof(PokerTableStatisticsViewModelSpecs), "BrowseAllHands")]
        public class when_told_to_browse_all_hands_of_a_player_and_the_active_player_selector_returns_non_empty_list : PokerTableStatisticsViewModelSpecs
        {
            static Mock<IPlayerStatistics> statisticsOfSelectedPlayer_Stub;

            static IEnumerable<IAnalyzablePokerPlayer> returnedActivePlayers;

            Establish context = () => {
                returnedActivePlayers = new[] { new Mock<IAnalyzablePokerPlayer>().Object };

                statisticsOfSelectedPlayer_Stub = new Mock<IPlayerStatistics>();
                _playerStatisticsVM_Stub
                    .SetupGet(pvm => pvm.PlayerStatistics)
                    .Returns(statisticsOfSelectedPlayer_Stub.Object);

                _activePlayersSelector_Mock
                    .Setup(ap => ap.SelectFrom(statisticsOfSelectedPlayer_Stub.Object))
                    .Returns(returnedActivePlayers);
            };

            Because of = () => _sut.BrowseAllHandsOf_Invoke(_playerStatisticsVM_Stub.Object);

            It should_select_the_active_players_from_the_player_statistics
                = () => _activePlayersSelector_Mock.Verify(ps => ps.SelectFrom(statisticsOfSelectedPlayer_Stub.Object));

            It should_initialize_the_detailed_statistics_analyzer_with_the_returned_active_players
                = () => _statisticsAnalyzer_Mock.Verify(dsa => dsa.InitializeWith(returnedActivePlayers, PlayerName));
        }

        [Subject(typeof(PokerTableStatisticsViewModel), "DisplayFilterAdjustmentPopup")]
        public class when_told_to_filter_a_players_statistics : PokerTableStatisticsViewModelSpecs
        {
            const string playerName = "someName";

            static Mock<IAnalyzablePokerPlayersFilter> playerFilter_Stub;

            static Mock<IPlayerStatisticsViewModel> playerStatisticsVM_Stub;

            Establish context = () => {
                playerFilter_Stub = new Mock<IAnalyzablePokerPlayersFilter>();
                playerStatisticsVM_Stub = Utils.PlayerStatisticsVM_MockFor(playerName);
                playerStatisticsVM_Stub
                    .SetupGet(p => p.Filter)
                    .Returns(playerFilter_Stub.Object);
            };

            Because of = () => _sut.DisplayFilterAdjustmentPopup(playerStatisticsVM_Stub.Object);

            It should_tell_the_filter_popup_viewmodel_to_show__the_filter_for_the_player
                = () => _filterPopupVM_Mock.Verify(
                            fp => fp.ShowFilter(playerName,
                                                playerFilter_Stub.Object,
                                                Moq.It.IsAny<Action<string, IAnalyzablePokerPlayersFilter>>(),
                                                Moq.It.IsAny<Action<IAnalyzablePokerPlayersFilter>>()));

        }

        [Subject(typeof(PokerTableStatisticsViewModel), "FilterAdjustmentRequestedCommand")]
        public class when_the_user_clicks_the_Filter_Button_in_the_PokerTableStatisticsWindow_and_a_player_was_selected : PokerTableStatisticsViewModelSpecs
        {
            const string playerName = "someName";

            static Mock<IAnalyzablePokerPlayersFilter> playerFilter_Stub;

            static Mock<IPlayerStatisticsViewModel> playerStatisticsVM_Stub;

            Establish context = () => {
                playerFilter_Stub = new Mock<IAnalyzablePokerPlayersFilter>();
                playerStatisticsVM_Stub = Utils.PlayerStatisticsVM_MockFor(playerName);
                playerStatisticsVM_Stub
                    .SetupGet(p => p.Filter)
                    .Returns(playerFilter_Stub.Object);

                _sut.SelectedPlayer = playerStatisticsVM_Stub.Object;
            };

            Because of = () => _sut.FilterAdjustmentRequestedCommand.Execute(null);

            It should_tell_the_filter_popup_viewmodel_to_show_the_filter_for_the_selected_player
                = () => _filterPopupVM_Mock.Verify(
                            fp => fp.ShowFilter(playerName,
                                                playerFilter_Stub.Object,
                                                Moq.It.IsAny<Action<string, IAnalyzablePokerPlayersFilter>>(),
                                                Moq.It.IsAny<Action<IAnalyzablePokerPlayersFilter>>()));
        }

    }

    public class PokerTableStatisticsViewModelSut : PokerTableStatisticsViewModel
    {
        public PokerTableStatisticsViewModelSut(
            ISettings settings,
            IDimensionsViewModel dimensions,
            IConstructor<IPlayerStatisticsViewModel> playerStatisticsViewModelMake,
            IDetailedStatisticsAnalyzerViewModel detailedStatisticsAnalyzerViewModel,
            IActiveAnalyzablePlayersSelector activePlayersSelector,
            IFilterPopupViewModel filterPopupViewModel)
            : base(settings, dimensions, playerStatisticsViewModelMake, detailedStatisticsAnalyzerViewModel, activePlayersSelector, filterPopupViewModel)
        {
        }

        // Passing null will cause the sut to make a new player statistics viewmodel
        public void AddNewPlayerToPlayersIfNotFound_Invoke(IPlayerStatisticsViewModel matchingPlayer)
        {
            AddNewPlayerToPlayersIfNotFound(matchingPlayer);
        }

        protected override void BrowseAllHandsOf(IPlayerStatisticsViewModel selectedPlayer)
        {
            base.BrowseAllHandsOf(selectedPlayer);
            PlayerWhoseHandsWereBrowsed = selectedPlayer;
        }

        public void BrowseAllHandsOf_Invoke(IPlayerStatisticsViewModel selectedPlayer)
        {
            base.BrowseAllHandsOf(selectedPlayer);
        }

        public IPlayerStatisticsViewModel PlayerWhoseHandsWereBrowsed;
        
        public IDimensionsViewModel Dimensions_Set
        {
            set { Dimensions = value; }
        }
    }
}