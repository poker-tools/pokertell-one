namespace PokerTell.LiveTracker.Tests.ViewModels.Overlay
{
    using System.Collections.Generic;
    using System.Windows;

    using Interfaces;

    using LiveTracker.ViewModels.Overlay;

    using Machine.Specifications;

    using Moq;

    using Tools.WPF.Interfaces;
    using Tools.WPF.ViewModels;

    using It = Machine.Specifications.It;

    // Resharper disable InconsistentNaming
    public class PlayerStatusViewModelSpecs
    {
        protected static Mock<IHarringtonMViewModel> _harringtonVM_Mock;

        protected static Mock<IOverlayHoleCardsViewModel> _holeCardsVM_Mock;

        protected static IPlayerStatusViewModel _sut;

        Establish specContext = () => {
           _harringtonVM_Mock = new Mock<IHarringtonMViewModel>(); 
            _holeCardsVM_Mock = new Mock<IOverlayHoleCardsViewModel>();
            _sut = new PlayerStatusViewModel(_harringtonVM_Mock.Object, _holeCardsVM_Mock.Object);
        };

        [Subject(typeof(PlayerStatusViewModel), "InitializeWith")]
        public class when_initialized_with_holeCards_and_harringtonM_position : PlayerStatusViewModelSpecs
        {
            static IPositionViewModel harringtonMPosition;

            static IPositionViewModel holeCardsPosition;

            Establish context = () => {
                harringtonMPosition = new PositionViewModel(1, 1);
                holeCardsPosition = new PositionViewModel(2, 2);
            };

            Because of = () => _sut.InitializeWith(holeCardsPosition, harringtonMPosition);

            It should_initialize_the_harringtonM_viewmodel_with_the_harringtonM_position
                = () => _harringtonVM_Mock.Verify(h => h.InitializeWith(harringtonMPosition));

            It should_initialize_the_holecards_viewmodel_with_the_holecards_position
                = () => _holeCardsVM_Mock.Verify(h => h.InitializeWith(holeCardsPosition));
        }

        [Subject(typeof(PlayerStatusViewModel), "ShowHoleCardsFor")]
        public class when_told_to_show_holecards_for_2_seconds : PlayerStatusViewModelSpecs
        {
            const int duration = 2;

            const string holecards = "As Kh";

            Because of = () => _sut.ShowHoleCardsFor(duration, holecards);

            It should_update_the_holecards_viewmodel_with_the_given_holecards
                = () => _holeCardsVM_Mock.Verify(h => h.UpdateWith(holecards));

            It should_tell_the_holecards_viewmodel_to_hide_the_holecards_after_2_seconds
                = () => _holeCardsVM_Mock.Verify(h => h.HideHoleCardsAfter(duration));
        }

    }
}