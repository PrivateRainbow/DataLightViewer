﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using DataLightViewer.Models;
using System.Windows.Input;
using DataLightViewer.Commands;
using System.Threading;
using System.Data.SqlClient;
using Loader.Tester.Contexts;
using Loader.Helpers;
using DataLightViewer.Mediator;
using System.Windows;

namespace DataLightViewer.ViewModels
{
    public class ExplorerViewModel : BaseViewModel
    {
        #region Private Members

        private readonly Window _objectExplorerWindow;

        /// <summary>
        /// Ability to cancel connection to destination server
        /// </summary>
        private CancellationTokenSource _cancelServerConnectionTokenSource;

        #endregion

        #region Properties

        /// <summary>
        /// Credentials for connection
        /// </summary>
        private User _user;
        public User User
        {
            get => _user;
            set
            {
                _user = value;
                OnPropertyChanged(nameof(User));
            }
        }

        /// <summary>
        /// Indicates that connection is fired in execution context
        /// </summary>
        private bool _isConnectionStarted;
        public bool IsConnectionStarted
        {
            get => _isConnectionStarted;
            set
            {
                _isConnectionStarted = value;
                OnPropertyChanged(nameof(IsConnectionStarted));
            }
        }

        /// <summary>
        /// In case when we use credentials for connection to server
        /// </summary>
        private bool _authorizedWithCredentials;
        public bool AuthorizedWithCredentials
        {
            get => _authorizedWithCredentials;
            set
            {
                _authorizedWithCredentials = value;
                OnPropertyChanged(nameof(AuthorizedWithCredentials));
            }
        }

        private bool _isServerConnectionEntered;
        public bool IsServerConnectionEntered
        {
            get => _isServerConnectionEntered;
            set
            {
                _isServerConnectionEntered = value;
                OnPropertyChanged(nameof(IsServerConnectionEntered));
            }
        }

        private string _selectedServerConnection;
        public string SelectedServerConnection
        {
            get => _selectedServerConnection;
            set
            {
                if (ReferenceEquals(_selectedServerConnection, value))
                    return;

                if (!string.IsNullOrEmpty(value) && value.Length != 0)
                {
                    _selectedServerConnection = value;
                    IsServerConnectionEntered = true;
                    OnPropertyChanged(nameof(SelectedServerConnection));
                }
                else
                {
                    IsServerConnectionEntered = false;
                }
            }
        }

        private Authentication _selectedAuthentication;
        public Authentication SelectedAuthentication
        {
            get => _selectedAuthentication;
            set
            {
                _selectedAuthentication = value;
                OnPropertyChanged(nameof(SelectedAuthentication));
            }
        }

        private List<Authentication> _authenticationTypes;
        public List<Authentication> AuthenticationTypes => _authenticationTypes;

        private ObservableCollection<string> _servers;
        public ObservableCollection<string> Servers
        {
            get => _servers;
            set
            {
                if (ReferenceEquals(_servers, value))
                    return;

                _servers = value;
                OnPropertyChanged(nameof(Servers));
            }
        }

        #endregion

        #region Commands

        private readonly ICommand _connectCommand;
        private readonly ICommand _cancelCommand;

        public ICommand ConnectCommand => _connectCommand;
        public ICommand CancelCommand => _cancelCommand;

        #endregion

        #region Init

        public ExplorerViewModel(Window window)
        {
            _objectExplorerWindow = window;

            _authenticationTypes = new List<Authentication>
            {
                new Authentication("Windows Authentication", AuthenticationType.Windows),
                new Authentication("SQL Server Authentication", AuthenticationType.SqlServer)
            };

            _connectCommand = new RelayCommand(() => ConnectToServerAsync(_cancelServerConnectionTokenSource.Token));
            _cancelCommand = new RelayCommand(() => CancelConnectionToServer());

            _cancelServerConnectionTokenSource = new CancellationTokenSource();
        }

        #endregion

        private async void ConnectToServerAsync(CancellationToken token)
        {
            var serverConnection = GetServerConnectionString();
            var connectionTester = new ConnectionTester(new SqlServerConnectionContext(serverConnection));

            LogWrapper.WriteInfo($"{nameof(ConnectToServerAsync)} has started! ",
                                    "Connecting to server ...");

            try
            {
                if (await connectionTester.VerifyConnectionAsync(token))
                {
                    App.ServerConnectionString = serverConnection;

                    Messenger.Instance.Notify(MessageType.ConnectionEstablished, true);

                    LogWrapper.WriteInfo($"{nameof(ConnectToServerAsync)} : connection is established!");
                    _objectExplorerWindow.Close();
                }

            }
            catch (SqlException ex)
            {
                LogWrapper.WriteError($"{nameof(ConnectToServerAsync)}", ex, "The server is not responding.");
                MessageBox.Show("Server is not responding.", "Error");
            }
        }
        private void CancelConnectionToServer()
        {
            if (_isConnectionStarted)
                _cancelServerConnectionTokenSource.Cancel();
            else
                _objectExplorerWindow.Close();
        }

        private string GetServerConnectionString()
        {
            var builder = new SqlConnectionStringBuilder()
            {
                DataSource = _selectedServerConnection
            };

            switch (_selectedAuthentication.Type)
            {
                case AuthenticationType.Windows:
                    builder.IntegratedSecurity = true;
                    break;

                case AuthenticationType.SqlServer:
                    builder.UserID = _user.Id;
                    builder.Password = _user.Password;
                    break;
            }

            return builder.ConnectionString;
        }

    }
}