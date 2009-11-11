namespace PokerTell.DatabaseSetup.ViewModels
{
    using Interfaces;

    using Microsoft.Practices.Composite.Events;

    using PokerTell.DatabaseSetup.Properties;
    using PokerTell.Infrastructure.Events;
    using PokerTell.Infrastructure.Interfaces.DatabaseSetup;

    public sealed class ClearDatabaseViewModel : ChooseDatabaseViewModel
    {
        #region Constructors and Destructors

        public ClearDatabaseViewModel(IEventAggregator eventAggregator, IDatabaseManager databaseManager)
            : base(eventAggregator, databaseManager)
        {
        }

        #endregion

        #region Properties

        public override string Title
        {
            get { return Resources.ClearDatabaseViewModel_Title; }
        }


        public override string ActionName
        {
            get { return Resources.Commands_Clear; }
        }
        #endregion

        #region Public Methods

        public override IComboBoxDialogViewModel DetermineSelectedItem()
        {
            SelectedItem = AvailableItems[0] ?? string.Empty;
            return this;
        }

        #endregion

        #region Methods

        protected override void CommitAction()
        {
            string message = string.Format(Resources.Warning_AllDataInDatabaseWillBeLost, SelectedItem);
            var userCommitAction = new UserConfirmActionEventArgs(ClearDatabaseAndPublishInfoMessage, message);
            _eventAggregator.GetEvent<UserConfirmActionEvent>().Publish(userCommitAction);
        }

        void ClearDatabaseAndPublishInfoMessage()
        {
            _databaseManager.ClearDatabase(SelectedItem);
            PublishInfoMessage();
        }

        void PublishInfoMessage()
        {
            string msg = string.Format(Resources.Info_DatabaseCleared, SelectedItem);
            var userMessage = new UserMessageEventArgs(UserMessageTypes.Info, msg);
            _eventAggregator.GetEvent<UserMessageEvent>().Publish(userMessage);
        }

        #endregion
    }
}