namespace PokerTell.LiveTracker.Views
{
    using System.Windows.Controls;

    using Infrastructure;
    using Infrastructure.Interfaces.LiveTracker;

    using Tools;

    /// <summary>
    /// Interaction logic for LiveTrackerSettingsView.xaml
    /// </summary>
    public partial class LiveTrackerSettingsView 
    {
        public LiveTrackerSettingsView()
        {
            InitializeComponent();

            if (Utils.OperatingSystemIsWindowsXPOrOlder())
            {
                Background = ApplicationProperties.BorderedWindowBackgoundBrush;
                AllowsTransparency = false;
            }
        }

        public LiveTrackerSettingsView(ILiveTrackerSettingsViewModel viewModel)
            : this()
        {
            viewModel.LoadSettings();
            DataContext = viewModel;
        }

        void Directory_TextBox_Initialized(object sender, System.EventArgs e)
        {
            ((TextBox)sender).Focus();
        }
    }
}