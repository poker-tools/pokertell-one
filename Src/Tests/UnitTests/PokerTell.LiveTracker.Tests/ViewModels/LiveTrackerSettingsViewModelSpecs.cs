namespace PokerTell.LiveTracker.Tests.ViewModels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Xml.Linq;

    using Events;

    using Infrastructure.Events;
    using Infrastructure.Interfaces.LiveTracker;

    using Machine.Specifications;

    using Microsoft.Practices.Composite.Events;
    using Microsoft.Practices.Composite.Presentation.Events;

    using Moq;

    using PokerTell.LiveTracker.Interfaces;
    using PokerTell.LiveTracker.Tests.Fakes;
    using PokerTell.LiveTracker.ViewModels;

    using Tools.FunctionalCSharp;
    using Tools.Interfaces;

    using It = Machine.Specifications.It;

    // Resharper disable InconsistentNaming
    public abstract class LiveTrackerSettingsViewModelSpecs
    {
        protected static LiveTrackerSettingsXDocumentHandlerMock _xDocumentHandler_Mock;

        protected static ILiveTrackerSettingsViewModel _sut;

        protected static IEventAggregator _eventAggregator;

        protected static Mock<IPokerRoomSettingsDetector> _autoDetector_Mock;

        protected static Mock<IHandHistoryFolderAutoDetectResultsViewModel> _autoDetectResultsVM_Mock;

        protected static Mock<IHandHistoryFolderAutoDetectResultsWindowManager> _autoDetectResultsWindow_Mock;

        protected static Mock<IPokerRoomInfoLocator> _pokerRoomInfoLocator_Stub;

        protected static Mock<ILayoutAutoConfigurator> _layoutAutoConfigurator_Mock;

        Establish specContext = () =>
        {
            _eventAggregator = new EventAggregator();
            _xDocumentHandler_Mock = new LiveTrackerSettingsXDocumentHandlerMock();

            _autoDetector_Mock = new Mock<IPokerRoomSettingsDetector>();
            _autoDetector_Mock
                .Setup(ad => ad.InitializeWith(Moq.It.IsAny<IEnumerable<IPokerRoomInfo>>()))
                .Returns(_autoDetector_Mock.Object);
            _autoDetector_Mock
                .SetupGet(ad => ad.PokerRoomsWithDetectedHandHistoryDirectories)
                .Returns(new List<ITuple<string, string>>());

            _autoDetectResultsVM_Mock = new Mock<IHandHistoryFolderAutoDetectResultsViewModel>();
            _autoDetectResultsVM_Mock
                .Setup(dr => dr.InitializeWith(Moq.It.IsAny<IPokerRoomSettingsDetector>()))
                .Returns(_autoDetectResultsVM_Mock.Object);

            _autoDetectResultsWindow_Mock = new Mock<IHandHistoryFolderAutoDetectResultsWindowManager>();

            _pokerRoomInfoLocator_Stub = new Mock<IPokerRoomInfoLocator>();

            _layoutAutoConfigurator_Mock = new Mock<ILayoutAutoConfigurator>();

            if (_layoutAutoConfigurator_Mock != null)
                _sut = new LiveTrackerSettingsViewModel(
                    _eventAggregator,
                    _xDocumentHandler_Mock,
                    _autoDetector_Mock.Object,
                    _autoDetectResultsVM_Mock.Object,
                    _autoDetectResultsWindow_Mock.Object,
                    _layoutAutoConfigurator_Mock.Object,
                    _pokerRoomInfoLocator_Stub.Object);
        };

        [Subject(typeof(LiveTrackerSettingsViewModel), "Load")]
        public class when_x_document_handler_returns_existing_and_not_existing_path_as_hand_history_file_paths : LiveTrackerSettingsViewModelSpecs
        {
            const string existingPath = @"C:\";

            const string notExisitingPath = "doesn't exist";

            Establish context = () =>
            {
                var settings = new LiveTrackerSettingsViewModel(_eventAggregator, new Mock<ILiveTrackerSettingsXDocumentHandler>().Object, _autoDetector_Mock.Object, _autoDetectResultsVM_Mock.Object, _autoDetectResultsWindow_Mock.Object, _layoutAutoConfigurator_Mock.Object, _pokerRoomInfoLocator_Stub.Object) { HandHistoryFilesPaths = new[] { existingPath, notExisitingPath } };

                _xDocumentHandler_Mock.DocumentToLoad = LiveTrackerSettingsViewModel.CreateXDocumentFor(settings);
            };

            Because of = () => _sut.LoadSettings();

            It should_add_the_existing_path_to_the_HandHistoryFilesPaths = () => _sut.HandHistoryFilesPaths.ShouldContain(existingPath);

            It should_not_add_the_not_existing_path_to_the_HandHistoryFilesPaths = () => _sut.HandHistoryFilesPaths.ShouldNotContain(notExisitingPath);

            It should_store_the_HandHistoyFilesPaths_in_an_OvservableCollection = () => _sut.HandHistoryFilesPaths.ShouldBeOfType<ObservableCollection<string>>();
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "LoadSettings")]
        public class when_the_returned_XDocument_is_null : LiveTrackerSettingsViewModelSpecs
        {
            Establish context = () => _xDocumentHandler_Mock.DocumentToLoad = null;

            Because of = () => _sut.LoadSettings();

            It should_load_the_settings_from_the_XDoxument_Handler = () => _xDocumentHandler_Mock.DocumentWasLoaded.ShouldBeTrue();

            It should_set_AutoTrack_to_true = () => _sut.AutoTrack.ShouldBeTrue();

            It should_set_ShowLiveStatsWindowOnStartup_to_true = () => _sut.ShowLiveStatsWindowOnStartup.ShouldBeTrue();

            It should_set_ShowTableOverlay_to_true = () => _sut.ShowTableOverlay.ShouldBeTrue();

            It should_set_ShowMyStatistics_to_false = () => _sut.ShowMyStatistics.ShouldBeFalse();

            It should_set_HoleCardsDuration_to_5 = () => _sut.ShowHoleCardsDuration.ShouldEqual(5);

            It should_set_HistoryPaths_to_empty = () => _sut.HandHistoryFilesPaths.ShouldBeEmpty();

            It should_set_GameHistoryIsPoppedIn_to_true = () => _sut.GameHistoryIsPoppedIn.ShouldBeTrue();

            It should_store_the_HandHistoyFilesPaths_in_an_OvservableCollection = () => _sut.HandHistoryFilesPaths.ShouldBeOfType<ObservableCollection<string>>();
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "LoadSettings")]
        public class when_all_boolean_properties_were_saved_as_true_and_ShowHoleCardsDuration_Is_1 : LiveTrackerSettingsViewModelSpecs
        {
            const bool setTrue = true;

            const int duration = 1;

            Establish context = () =>
            {
                var settings = new LiveTrackerSettingsViewModel(_eventAggregator,
                                                                new Mock<ILiveTrackerSettingsXDocumentHandler>().Object,
                                                                _autoDetector_Mock.Object,
                                                                _autoDetectResultsVM_Mock.Object,
                                                                _autoDetectResultsWindow_Mock.Object,
                                                                _layoutAutoConfigurator_Mock.Object,
                                                                _pokerRoomInfoLocator_Stub.Object)
                    {
                        AutoTrack = setTrue,
                        ShowLiveStatsWindowOnStartup = setTrue,
                        ShowTableOverlay = setTrue,
                        ShowMyStatistics = setTrue,
                        GameHistoryIsPoppedIn = setTrue,
                        ShowHoleCardsDuration = duration
                    };

                _xDocumentHandler_Mock.DocumentToLoad = LiveTrackerSettingsViewModel.CreateXDocumentFor(settings);
            };

            Because of = () => _sut.LoadSettings();

            It should_load_the_settings_from_the_XDoxument_Handler = () => _xDocumentHandler_Mock.DocumentWasLoaded.ShouldBeTrue();

            It should_set_AutoTrack_to_true = () => _sut.AutoTrack.ShouldBeTrue();

            It should_set_ShowLiveStatsWindowOnStartup_to_true = () => _sut.ShowLiveStatsWindowOnStartup.ShouldBeTrue();

            It should_set_ShowTableOverlay_to_true = () => _sut.ShowTableOverlay.ShouldBeTrue();

            It should_set_ShowMyStatistics_to_true = () => _sut.ShowMyStatistics.ShouldBeTrue();

            It should_set_GameHistoryIsPoppedIn_to_true = () => _sut.GameHistoryIsPoppedIn.ShouldBeTrue();

            It should_set_HoleCardsDuration_to_1 = () => _sut.ShowHoleCardsDuration.ShouldEqual(duration);
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "LoadSettings")]
        public class when_all_boolean_properties_were_saved_as_false_and_ShowHoleCardsDuration_Is_2 : LiveTrackerSettingsViewModelSpecs
        {
            const bool setFalse = false;

            const int duration = 2;

            Establish context = () =>
            {
                var settings = new LiveTrackerSettingsViewModel(_eventAggregator,
                                                                new Mock<ILiveTrackerSettingsXDocumentHandler>().Object,
                                                                _autoDetector_Mock.Object,
                                                                _autoDetectResultsVM_Mock.Object,
                                                                _autoDetectResultsWindow_Mock.Object,
                                                                _layoutAutoConfigurator_Mock.Object,
                                                                _pokerRoomInfoLocator_Stub.Object)
                    {
                        AutoTrack = setFalse,
                        ShowLiveStatsWindowOnStartup = setFalse,
                        ShowTableOverlay = setFalse,
                        ShowMyStatistics = setFalse,
                        GameHistoryIsPoppedIn = setFalse,
                        ShowHoleCardsDuration = duration
                    };

                _xDocumentHandler_Mock.DocumentToLoad = LiveTrackerSettingsViewModel.CreateXDocumentFor(settings);
            };

            Because of = () => _sut.LoadSettings();

            It should_load_the_settings_from_the_XDoxument_Handler = () => _xDocumentHandler_Mock.DocumentWasLoaded.ShouldBeTrue();

            It should_set_AutoTrack_to_false = () => _sut.AutoTrack.ShouldBeFalse();

            It should_set_ShowLiveStatsWindowOnStartup_to_false = () => _sut.ShowLiveStatsWindowOnStartup.ShouldBeFalse();

            It should_set_ShowTableOverlay_to_false = () => _sut.ShowTableOverlay.ShouldBeFalse();

            It should_set_ShowMyStatistics_to_false = () => _sut.ShowMyStatistics.ShouldBeFalse();

            It should_set_GameHistoryIsPoppedIn_to_false = () => _sut.GameHistoryIsPoppedIn.ShouldBeFalse();

            It should_set_HoleCardsDuration_to_2 = () => _sut.ShowHoleCardsDuration.ShouldEqual(duration);
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "Saving Settings")]
        public class when_the_user_executes_SaveComand : LiveTrackerSettingsViewModelSpecs
        {
            static XDocument expectedDocument;

            static bool settingsChangedWasRaised;

            static ILiveTrackerSettingsViewModel passedPayload;

            Establish context = () =>
            {
                _sut.HandHistoryFilesPaths = new[] { "somePath" };
                expectedDocument = LiveTrackerSettingsViewModel.CreateXDocumentFor(_sut);
                _eventAggregator
                    .GetEvent<LiveTrackerSettingsChangedEvent>()
                    .Subscribe(payload =>
                    {
                        settingsChangedWasRaised = true;
                        passedPayload = payload;
                    });
            };

            Because of = () => _sut.SaveSettingsCommand.Execute(null);

            It should_save_the_settings = () => _xDocumentHandler_Mock.DocumentWasSaved.ShouldBeTrue();

            It the_saved_xDocument_should_contain_the_correct_settings
                = () => _xDocumentHandler_Mock.SavedDocument.ToString().ShouldEqual(expectedDocument.ToString());

            It should_raise_LiveTrackerSettings_changed_event = () => settingsChangedWasRaised.ShouldBeTrue();

            It should_pass_itself_as_the_payload_of_the_LiveTrackerSettings_changed_event = () => passedPayload.ShouldBeTheSameAs(_sut);
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "Remove Hand History folder")]
        public class when_no_hand_history_folder_is_selected : LiveTrackerSettingsViewModelSpecs
        {
            It should_not_be_possible_to_remove_one = () => _sut.RemoveSelectedHandHistoryPathCommand.CanExecute(null).ShouldBeFalse();
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "Remove Hand History folder")]
        public class when_a_hand_history_folder_is_selected : LiveTrackerSettingsViewModelSpecs
        {
            Because of = () => _sut.SelectedHandHistoryFilesPath = "some path";

            It should__be_possible_to_remove_it = () => _sut.RemoveSelectedHandHistoryPathCommand.CanExecute(null).ShouldBeTrue();
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "Remove Hand History folder")]
        public class when_there_are_two_folders_in_list_and_the_second_one_is_selected_and_the_use_executes_remove_command : LiveTrackerSettingsViewModelSpecs
        {
            const string firstPath = "first Path";
            const string secondPath = "second Path";

            Establish context = () =>
            {
                _sut.HandHistoryFilesPaths = new List<string> { firstPath, secondPath };
                _sut.SelectedHandHistoryFilesPath = secondPath;
            };

            Because of = () => _sut.RemoveSelectedHandHistoryPathCommand.Execute(null);

            It should_not_remove_the_first_path = () => _sut.HandHistoryFilesPaths.ShouldContain(firstPath);

            It should_remove_the_second_path = () => _sut.HandHistoryFilesPaths.ShouldNotContain(secondPath);

            It should_set_the_selected_hand_history_path_to_null = () => _sut.SelectedHandHistoryFilesPath.ShouldBeNull();

        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "Add Hand History path")]
        public class when_the_given_path_is_null : LiveTrackerSettingsViewModelSpecs
        {
            Because of = () => _sut.HandHistoryPathToBeAdded = null;

            It should_not_be_possible_to_add_it = () => _sut.AddHandHistoryPathCommand.CanExecute(null).ShouldBeFalse();
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "Add Hand History path")]
        public class when_the_given_path_is_not_a_valid_path : LiveTrackerSettingsViewModelSpecs
        {
            Because of = () => _sut.HandHistoryPathToBeAdded = "illegal path";

            It should_not_be_possible_to_add_it = () => _sut.AddHandHistoryPathCommand.CanExecute(null).ShouldBeFalse();
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "Add Hand History path")]
        public class when_the_given_path_is_a_valid_path : LiveTrackerSettingsViewModelSpecs
        {
            Because of = () => _sut.HandHistoryPathToBeAdded = @"C:\";

            It should_be_possible_to_add_it = () => _sut.AddHandHistoryPathCommand.CanExecute(null).ShouldBeTrue();
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "Add Hand History path")]
        public class when_the_user_executes_the_add_folder_command_and_the_folder_was_not_added_yet : LiveTrackerSettingsViewModelSpecs
        {
            const string pathToBeAdded = "to be added Path";

            Establish context = () =>
            {
                _sut.HandHistoryPathToBeAdded = pathToBeAdded;
                _sut.HandHistoryFilesPaths = new List<string>();
            };

            Because of = () => _sut.AddHandHistoryPathCommand.Execute(null);

            It should_add_the_path = () => _sut.HandHistoryFilesPaths.ShouldContain(pathToBeAdded);

            It should_set_the_path_to_be_added_to_null = () => _sut.HandHistoryPathToBeAdded.ShouldBeNull();
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "Add Hand History path")]
        public class when_the_user_executes_the_add_folder_command_and_the_folder_was_not_added_yet_but_the_path_has_leading_and_trailing_spaces : LiveTrackerSettingsViewModelSpecs
        {

            const string pathToBeAdded = "  to be added Path  ";
            const string pathToBeAddedWithoutSpaces = "to be added Path";

            Establish context = () =>
            {
                _sut.HandHistoryPathToBeAdded = pathToBeAdded;
                _sut.HandHistoryFilesPaths = new List<string>();
            };

            Because of = () => _sut.AddHandHistoryPathCommand.Execute(null);

            It should_add_the_path_without_the_spaces = () => _sut.HandHistoryFilesPaths.ShouldContain(pathToBeAddedWithoutSpaces);

            It should_set_the_path_to_be_added_to_null = () => _sut.HandHistoryPathToBeAdded.ShouldBeNull();
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "Add Hand History path")]
        public class when_the_user_executes_the_add_folder_command_but_the_folder_was_before : LiveTrackerSettingsViewModelSpecs
        {
            const string pathToBeAdded = "to be added Path";

            static bool userWasWarned;

            Establish context = () =>
            {
                _sut.HandHistoryPathToBeAdded = pathToBeAdded;
                _sut.HandHistoryFilesPaths = new List<string> { pathToBeAdded };
                _eventAggregator
                    .GetEvent<UserMessageEvent>()
                    .Subscribe(args => userWasWarned = args.UserMessage.Contains("tracked folders"));
            };

            Because of = () => _sut.AddHandHistoryPathCommand.Execute(null);

            It should_not_add_the_path_again = () => _sut.HandHistoryFilesPaths.Count.ShouldEqual(1);

            It should_inform_the_user_that_it_existed_already = () => userWasWarned.ShouldBeTrue();

            It should_set_the_path_to_be_added_to_null = () => _sut.HandHistoryPathToBeAdded.ShouldBeNull();
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "AutoDetectHandHistoryFoldersCommand")]
        public class when_the_user_wants_to_autodetect_the_HandHistory_folders_and_no_HandHistoryFolders_are_currently_tracked : LiveTrackerSettingsViewModelSpecs
        {
            static IEnumerable<IPokerRoomInfo> pokerRoomInfos_Stub;

            Establish context = () =>
            {
                pokerRoomInfos_Stub = new[] { new Mock<IPokerRoomInfo>().Object };
                _pokerRoomInfoLocator_Stub
                    .SetupGet(il => il.SupportedPokerRoomInfos)
                    .Returns(pokerRoomInfos_Stub);
            };

            Because of = () => _sut.AutoDetectHandHistoryFoldersCommand.Execute(null);

            It should_initialize_the_autoDetector_with_the_PokerRoomInfos_returned_by_the_PokerRoomInfoLocator
                = () => _autoDetector_Mock.Verify(ad => ad.InitializeWith(pokerRoomInfos_Stub));

            It should_tell_the_autoDetector_to_detect_handHistoryFolders = () => _autoDetector_Mock.Verify(ad => ad.DetectHandHistoryFolders());

            It should_initialize_the_autodetectresults_viewmodel_with_the_autodetector
                = () => _autoDetectResultsVM_Mock.Verify(dr => dr.InitializeWith(_autoDetector_Mock.Object));

            It should_set_the_datacontext_of_the_autodetectresults_window_to_the_autodetectresults_viewmodel
                = () => _autoDetectResultsWindow_Mock.VerifySet(drw => drw.DataContext = _autoDetectResultsVM_Mock.Object);

            It should_show_the_autodetectresults_window_as_a_dialog = () => _autoDetectResultsWindow_Mock.Verify(drw => drw.ShowDialog());
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "DetectAndAddHandHistoryFolders")]
        public class when_no_folders_are_currently_tracked_and_the_auto_detector_finds_one_room_with_corresponding_folder : LiveTrackerSettingsViewModelSpecs
        {
            const string detectedHandHistoryFolder = "someFolder";

            Establish context = () =>
            {
                _sut.HandHistoryFilesPaths = new List<string>();
                _autoDetector_Mock
                    .SetupGet(ad => ad.PokerRoomsWithDetectedHandHistoryDirectories)
                    .Returns(new[] { Tuple.New("somePokerSite", detectedHandHistoryFolder) });
            };

            Because of = () => _sut.DetectAndAddHandHistoryFolders();

            It should_add_the_detected_folder_to_the_tracked_HandHistory_folders = () => _sut.HandHistoryFilesPaths.ShouldContain(detectedHandHistoryFolder);
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "DetectAndAddHandHistoryFolders")]
        public class when_two_folders_are_found_but_the_second_one_is_tracked_already : LiveTrackerSettingsViewModelSpecs
        {
            const string firstDetectedHandHistoryFolder = "someFolder";

            const string secondDetectedHandHistoryFolder = "another Folder";

            Establish context = () =>
            {
                _sut.HandHistoryFilesPaths = new List<string> { secondDetectedHandHistoryFolder };
                _autoDetector_Mock
                    .SetupGet(ad => ad.PokerRoomsWithDetectedHandHistoryDirectories)
                    .Returns(new[] { Tuple.New("somePokerSite", firstDetectedHandHistoryFolder), Tuple.New("somePokerSite", secondDetectedHandHistoryFolder) });
            };

            Because of = () => _sut.DetectAndAddHandHistoryFolders();

            It should_add_the_first_detected_folder_to_the_tracked_HandHistory_folders = () => _sut.HandHistoryFilesPaths.ShouldContain(firstDetectedHandHistoryFolder);

            It should_not_add_the_second_detected_folder_to_the_tracked_HandHistory_folders_again = () => _sut.HandHistoryFilesPaths.Count.ShouldEqual(2);
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "Detect and save preferred seats")]
        public class when_told_to_auto_detect_and_save_the_preferred_seats_and_the_autodetector_finds_preferred_seats_for_a_pokerroom : LiveTrackerSettingsViewModelSpecs
        {
            static IEnumerable<IPokerRoomInfo> pokerRoomInfos_Stub;

            const string site = "SomeSite";

            static IDictionary<int, int> preferredSeats_Stub;

            static ITuple<string, IDictionary<int, int>> returnedPreferredSeatsForRoom;

            Establish context = () =>
            {
                preferredSeats_Stub = new Dictionary<int, int> { { 2, 1 } };
                returnedPreferredSeatsForRoom = Tuple.New(site, preferredSeats_Stub);

                pokerRoomInfos_Stub = new[] { new Mock<IPokerRoomInfo>().Object };
                _pokerRoomInfoLocator_Stub
                    .SetupGet(il => il.SupportedPokerRoomInfos)
                    .Returns(pokerRoomInfos_Stub);
                _autoDetector_Mock
                    .SetupGet(ad => ad.PokerRoomsWithDetectedPreferredSeats)
                    .Returns(new[] { returnedPreferredSeatsForRoom });
            };

            Because of = () => _sut.DetectAndSavePreferredSeats();

            It should_initialize_the_autoDetector_with_the_PokerRoomInfos_returned_by_the_PokerRoomInfoLocator
                = () => _autoDetector_Mock.Verify(ad => ad.InitializeWith(pokerRoomInfos_Stub));

            It should_tell_the_autoDetector_to_detect_preferredSeats = () => _autoDetector_Mock.Verify(ad => ad.DetectPreferredSeats());

            It should_tell_the_layoutAutoConfigurator_to_configure_the_preferred_seats_for_the_pokerroom
                = () => _layoutAutoConfigurator_Mock.Verify(lac => lac.ConfigurePreferredSeats(site, preferredSeats_Stub));
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "DetectPreferredSeatsCommand")]
        public class when_the_user_says_he_wants_to_auto_detect_the_preferred_seats_and_the_autodetector_finds_preferred_seats_for_a_pokerroom : LiveTrackerSettingsViewModelSpecs
        {
            static IEnumerable<IPokerRoomInfo> pokerRoomInfos_Stub;

            const string site = "SomeSite";

            static IDictionary<int, int> preferredSeats_Stub;

            static ITuple<string, IDictionary<int, int>> returnedPreferredSeatsForRoom;

            static bool publishUserMessageWasRaised;

            static UserMessageEventArgs passedPayload;

            Establish context = () =>
            {
                preferredSeats_Stub = new Dictionary<int, int> { { 2, 1 } };
                returnedPreferredSeatsForRoom = Tuple.New(site, preferredSeats_Stub);

                pokerRoomInfos_Stub = new[] { new Mock<IPokerRoomInfo>().Object };
                _pokerRoomInfoLocator_Stub
                    .SetupGet(il => il.SupportedPokerRoomInfos)
                    .Returns(pokerRoomInfos_Stub);
                _autoDetector_Mock
                    .SetupGet(ad => ad.PokerRoomsWithDetectedPreferredSeats)
                    .Returns(new[] { returnedPreferredSeatsForRoom });

                _eventAggregator
                    .GetEvent<UserMessageEvent>()
                    .Subscribe(payload =>
                    {
                        publishUserMessageWasRaised = true;
                        passedPayload = payload;
                    },
                    ThreadOption.PublisherThread,
                    false,
                    args => args.MessageType == UserMessageTypes.Info);
            };

            Because of = () => _sut.DetectPreferredSeatsCommand.Execute(null);

            It should_initialize_the_autoDetector_with_the_PokerRoomInfos_returned_by_the_PokerRoomInfoLocator
                = () => _autoDetector_Mock.Verify(ad => ad.InitializeWith(pokerRoomInfos_Stub));

            It should_tell_the_autoDetector_to_detect_preferredSeats = () => _autoDetector_Mock.Verify(ad => ad.DetectPreferredSeats());

            It should_tell_the_layoutAutoConfigurator_to_configure_the_preferred_seats_for_the_pokerroom
                = () => _layoutAutoConfigurator_Mock.Verify(lac => lac.ConfigurePreferredSeats(site, preferredSeats_Stub));

            It should_let_the_user_know_about_the_preferred_seats_that_got_configured = () => publishUserMessageWasRaised.ShouldBeTrue();

            It should_include_the_site_whose_preferred_seat_got_configured_in_the_message = () => passedPayload.UserMessage.ShouldContain(site);
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "Detect and save preferred seats")]
        public class when_told_to_auto_detect_and_save_the_preferred_seats_but_the_auto_detector_finds_no_room_with_preferred_seats
            : LiveTrackerSettingsViewModelSpecs
        {
            static IEnumerable<IPokerRoomInfo> pokerRoomInfos_Stub;

            const string site = "SomeSite";

            Establish context = () =>
            {
                pokerRoomInfos_Stub = new[] { new Mock<IPokerRoomInfo>().Object };
                _pokerRoomInfoLocator_Stub
                    .SetupGet(il => il.SupportedPokerRoomInfos)
                    .Returns(pokerRoomInfos_Stub);
                _autoDetector_Mock
                    .SetupGet(ad => ad.PokerRoomsWithDetectedPreferredSeats)
                    .Returns(new List<ITuple<string, IDictionary<int, int>>>());
            };

            Because of = () => _sut.DetectAndSavePreferredSeats();

            It should_initialize_the_autoDetector_with_the_PokerRoomInfos_returned_by_the_PokerRoomInfoLocator
                = () => _autoDetector_Mock.Verify(ad => ad.InitializeWith(pokerRoomInfos_Stub));

            It should_tell_the_autoDetector_to_detect_preferredSeats = () => _autoDetector_Mock.Verify(ad => ad.DetectPreferredSeats());

            It should_not_tell_the_layoutAutoConfigurator_to_configure_the_preferred_seats_for_any_pokerroom
                = () => _layoutAutoConfigurator_Mock.Verify(
                    lac => lac.ConfigurePreferredSeats(Moq.It.IsAny<string>(), Moq.It.IsAny<IDictionary<int, int>>()), Times.Never());
        }

        [Subject(typeof(LiveTrackerSettingsViewModel), "Detect and save preferred seats")]
        public class when_the_user_says_he_wants_to_auto_detect_the_preferred_seats_but_the_auto_detector_finds_no_room_with_preferred_seats
            : LiveTrackerSettingsViewModelSpecs
        {
            static IEnumerable<IPokerRoomInfo> pokerRoomInfos_Stub;

            const string site = "SomeSite";

            static bool publishUserWarningWasRaised;

            static UserMessageEventArgs passedPayload;

            Establish context = () =>
            {
                pokerRoomInfos_Stub = new[] { new Mock<IPokerRoomInfo>().Object };
                _pokerRoomInfoLocator_Stub
                    .SetupGet(il => il.SupportedPokerRoomInfos)
                    .Returns(pokerRoomInfos_Stub);
                _autoDetector_Mock
                    .SetupGet(ad => ad.PokerRoomsWithDetectedPreferredSeats)
                    .Returns(new List<ITuple<string, IDictionary<int, int>>>());

                _eventAggregator
                    .GetEvent<UserMessageEvent>()
                    .Subscribe(payload =>
                    {
                        publishUserWarningWasRaised = true;
                        passedPayload = payload;
                    },
                    ThreadOption.PublisherThread,
                    false,
                    args => args.MessageType == UserMessageTypes.Warning);
            };

            Because of = () => _sut.DetectPreferredSeatsCommand.Execute(null);

            It should_initialize_the_autoDetector_with_the_PokerRoomInfos_returned_by_the_PokerRoomInfoLocator
                = () => _autoDetector_Mock.Verify(ad => ad.InitializeWith(pokerRoomInfos_Stub));

            It should_tell_the_autoDetector_to_detect_preferredSeats = () => _autoDetector_Mock.Verify(ad => ad.DetectPreferredSeats());

            It should_not_tell_the_layoutAutoConfigurator_to_configure_the_preferred_seats_for_any_pokerroom
                = () => _layoutAutoConfigurator_Mock.Verify(
                    lac => lac.ConfigurePreferredSeats(Moq.It.IsAny<string>(), Moq.It.IsAny<IDictionary<int, int>>()), Times.Never());

            It should_warn_the_user_know_that_no_preferred_seats_could_be_configured = () => publishUserWarningWasRaised.ShouldBeTrue();
        }
    }
}