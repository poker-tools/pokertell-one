namespace PokerTell.DatabaseSetup.ViewModels
{
    using System;
    using System.Windows.Input;

    using Microsoft.Practices.Unity;

    using PokerTell.DatabaseSetup.Views;
    using PokerTell.Infrastructure.Interfaces.DatabaseSetup;

    using Tools.WPF;

    public class DatabaseSetupMenuItemViewModel
    {
        #region Constants and Fields

        readonly IUnityContainer _container;

        ICommand _chooseDatabaseCommand;

        ICommand _chooseDataProviderCommand;

        ICommand _clearDatabaseCommand;

        ICommand _configureMySqlProviderCommand;

        ICommand _deleteDatabaseCommand;

        #endregion

        #region Constructors and Destructors

        public DatabaseSetupMenuItemViewModel(IUnityContainer container)
        {
            _container = container;
        }

        #endregion

        #region Properties

        public ICommand ChooseDatabaseCommand
        {
            get
            {
                return _chooseDatabaseCommand ?? (_chooseDatabaseCommand = new SimpleCommand
                    {
                        ExecuteDelegate = arg => {
                            IDatabaseManager databaseManager = CreateDatabaseManager();

                            if (databaseManager != null)
                            {
                                _container
                                    .RegisterInstance(databaseManager);

                                new ComboBoxDialogView(
                                    _container
                                        .Resolve<ChooseDatabaseViewModel>()
                                        .DetermineSelectedItem())
                                    .ShowDialog();
                            }
                        }, 
                    });
            }
        }

        public ICommand ChooseDataProviderCommand
        {
            get
            {
                return _chooseDataProviderCommand ?? (_chooseDataProviderCommand = new SimpleCommand
                    {
                        ExecuteDelegate = arg => {
                            var chooseProviderViewModel = _container.Resolve<ChooseDataProviderViewModel>();
                            chooseProviderViewModel.DetermineSelectedItem();

                            if (chooseProviderViewModel.IsValid)
                            {
                                new ComboBoxDialogView(chooseProviderViewModel).ShowDialog();
                            }
                        }
                    });
            }
        }

        public ICommand ClearDatabaseCommand
        {
            get
            {
                return _clearDatabaseCommand ?? (_clearDatabaseCommand = new SimpleCommand
                    {
                        ExecuteDelegate = arg => {
                            IDatabaseManager databaseManager = CreateDatabaseManager();

                            if (databaseManager != null)
                            {
                                _container
                                    .RegisterInstance(databaseManager);

                                new ComboBoxDialogView(
                                    _container.Resolve<ClearDatabaseViewModel>()
                                        .DetermineSelectedItem())
                                    .ShowDialog();
                            }
                        }
                    });
            }
        }

        public ICommand ConfigureMySqlProviderCommand
        {
            get
            {
                return _configureMySqlProviderCommand ?? (_configureMySqlProviderCommand = new SimpleCommand
                    {
                        ExecuteDelegate = arg => {
                            try
                            {
                                _container.Resolve<ConfigureMySqlDataProviderView>().ShowDialog();
                            }
                            catch (Exception excep)
                            {
                                Console.WriteLine(excep.ToString());
                            }
                        }
                    });
            }
        }

        public ICommand DeleteDatabaseCommand
        {
            get
            {
                return _deleteDatabaseCommand ?? (_deleteDatabaseCommand = new SimpleCommand
                    {
                        ExecuteDelegate = arg => {
                            IDatabaseManager databaseManager = CreateDatabaseManager();

                            if (databaseManager != null)
                            {
                                _container
                                    .RegisterInstance(databaseManager);

                                new ComboBoxDialogView(
                                    _container.Resolve<DeleteDatabaseViewModel>()
                                        .RemoveDatabaseInUseFromAvailableItems()
                                        .DetermineSelectedItem())
                                    .ShowDialog();
                            }
                        }
                    });
            }
        }

        ICommand _createDatabaseCommand;

        public ICommand CreateDatabaseCommand
        {
            get
            {
                return _createDatabaseCommand ?? (_createDatabaseCommand = new SimpleCommand
                    {
                        ExecuteDelegate = arg =>
                        {
                            IDatabaseManager databaseManager = CreateDatabaseManager();

                            if (databaseManager != null)
                            {
                                _container
                                    .RegisterInstance(databaseManager);

                                new TextBoxDialogView(
                                    _container.Resolve<CreateDatabaseViewModel>())
                                    .ShowDialog();
                            }
                        }
                    });
            }
        }

        #endregion

        #region Methods

        IDatabaseManager CreateDatabaseManager()
        {
            var databaseConnector = _container.Resolve<IDatabaseConnector>();
            return databaseConnector
                .InitializeFromSettings()
                .ConnectToServer()
                .CreateDatabaseManager();
        }

        #endregion
    }
}