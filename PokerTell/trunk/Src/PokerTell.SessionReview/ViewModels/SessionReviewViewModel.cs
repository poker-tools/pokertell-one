﻿namespace PokerTell.SessionReview.ViewModels
{
    using System.IO;
    using System.Reflection;
    using System.Text;

    using log4net;

    using Microsoft.Practices.Composite.Presentation.Commands;
    using Microsoft.Practices.Composite.Regions;
    using Microsoft.Win32;

    using PokerTell.Infrastructure;
    using PokerTell.Infrastructure.Interfaces.PokerHand;
    using PokerTell.SessionReview.Views;

    using Tools.Serialization;
    using Tools.WPF.ViewModels;

    internal class SessionReviewViewModel : ItemsRegionViewModel, ISessionReviewViewModel
    {
        #region Constants and Fields

        static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        readonly IHandHistoriesViewModel _handHistoriesViewModel;

        readonly IRegionManager _regionManager;

        DelegateCommand<object> _createReportCommand;

        DelegateCommand<object> _saveCommand;

        #endregion

        #region Constructors and Destructors

        public SessionReviewViewModel(IRegionManager regionManager, IHandHistoriesViewModel handHistoriesViewModel)
        {
            _regionManager = regionManager;
            _handHistoriesViewModel = handHistoriesViewModel;

            _handHistoriesViewModel.ShowSelectOption = true;

            Commands.SaveSessionReviewCommand.RegisterCommand(SaveCommand);

            Commands.CreateSessionReviewReportCommand.RegisterCommand(CreateReportCommand);

            HeaderInfo = "SessionReview " + GetHashCode() + " for: " + _handHistoriesViewModel.GetHashCode();
        }

        #endregion

        #region Properties

        public DelegateCommand<object> CreateReportCommand
        {
            get
            {
                if (_createReportCommand == null)
                {
                    _createReportCommand = new DelegateCommand<object>(CreateReport, CanCreateReport);
                }

                return _createReportCommand;
            }
        }

        public IHandHistoriesViewModel HandHistoriesViewModel
        {
            get { return _handHistoriesViewModel; }
        }

        public DelegateCommand<object> SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new DelegateCommand<object>(Save, CanSave);
                }

                return _saveCommand;
            }
        }

        #endregion

        #region Public Methods

        public bool CanCreateReport(object arg)
        {
            return IsActive;
        }

        public bool CanSave(object arg)
        {
            return IsActive;
        }

        public void CreateReport(object arg)
        {
            Log.InfoFormat("SessionReview->CreatingReport: {0}", GetHashCode());

            string htmlText = BuildHtmlText();

            AddReportToShell(htmlText);
        }

        public void Save(object arg)
        {
            Log.InfoFormat("SessionReview->Saving: {0}", GetHashCode());
            var saveFileDialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "pthh",
                Filter = "PokerTell HandHistories (*.pthh)|*.pthh|All files (*.*)|*.*",
                Title = "Save PokerTell Session Review"
            };
            saveFileDialog.FileName = "SessionReview" + "." + saveFileDialog.DefaultExt;

            if ((bool)saveFileDialog.ShowDialog())
            {
                BinarySerializer.Serialize(_handHistoriesViewModel, saveFileDialog.FileName);
            }
        }

        #endregion

        #region Methods

        protected override void OnIsActiveChanged()
        {
            SaveCommand.IsActive =
                CreateReportCommand.IsActive = IsActive;
        }

        void AddReportToShell(string htmlText)
        {
            var reportViewModel = new SessionReviewReportViewModel(GetHashCode().ToString(), htmlText);
            var reportView = new SessionReviewReportView(reportViewModel);
            _regionManager.Regions[ApplicationProperties.ShellMainRegion].Add(reportView);
            _regionManager.Regions[ApplicationProperties.ShellMainRegion].Activate(reportView);
        }

        string BuildHtmlText()
        {
            string htmlHandHistories = HtmlStringBuilder.BuildFrom(
                _handHistoriesViewModel.SelectedHandHistories,
                _handHistoriesViewModel.HandHistoriesFilter.ShowPreflopFolds, 
                _handHistoriesViewModel.HandHistoriesFilter.HeroName);

            Encoding enc = Encoding.UTF8;

            string htmlBegin = "<html>\r\n<head>\r\n"
                               + "<meta http-equiv=\"Content-Type\""
                               + " content=\"text/html; charset=" + enc.WebName + "\">\r\n"
                               + "<title>Poker Game Report</title>\r\n</head>\r\n<body>\r\n";

            const string htmlEnd = "\r\n</body>\r\n</html>\r\n";

            return htmlBegin + htmlHandHistories + htmlEnd;
        }

        #endregion
    }
}