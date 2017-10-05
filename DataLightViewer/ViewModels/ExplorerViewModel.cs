using System.Collections.Generic;
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
using System.Security;
using DataLightViewer.Helpers;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using DataLightViewer.Services;

namespace DataLightViewer.ViewModels
{
    public class ExplorerViewModel : BaseViewModel
    {
        #region Events

        public event EventHandler ValidationCheckMessage = (obj, e) => { };
        public event EventHandler ConnectionEstablished = (obj, e) => { };
        public event EventHandler UpdatePasswordMessage;

        #endregion

        #region Private Members

        /// <summary>
        /// When project has been opened from project-file 
        /// and user can change a current project connection
        /// need to cash for the further server connection validating 
        /// </summary>
        private string _previousConnection;

        private string _userId;
        private SecureString _userPassword;

        private bool _isConnectionStarted;
        private bool _authorizedWithCredentials;
        private bool _isServerConnectionEntered;

        private string _selectedServerConnection;
        private AppConnectionMode _appConnectionMode;
        private Authentication _selectedAuthentication;
        private List<Authentication> _authenticationTypes;
        private ObservableCollection<string> _servers;
        private CancellationTokenSource _cancelServerConnectionTokenSource;

        #endregion

        #region Properties

        public string UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                OnPropertyChanged(nameof(UserId));
            }
        }
        public SecureString UserPassword
        {
            get => _userPassword;
            set
            {
                _userPassword = value;
                OnPropertyChanged(nameof(UserPassword));
            }
        }
        public bool IsConnectionStarted
        {
            get => _isConnectionStarted;
            set
            {
                _isConnectionStarted = value;
                OnPropertyChanged(nameof(IsConnectionStarted));
            }
        }
        public bool AuthorizedWithCredentials
        {
            get => _authorizedWithCredentials;
            set
            {
                _authorizedWithCredentials = value;
                OnPropertyChanged(nameof(AuthorizedWithCredentials));
            }
        }
        public bool IsServerConnectionEntered
        {
            get => _isServerConnectionEntered;
            set
            {
                _isServerConnectionEntered = value;
                OnPropertyChanged(nameof(IsServerConnectionEntered));
            }
        }
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
        public AppConnectionMode ConnectionMode
        {
            get => _appConnectionMode;
            set
            {
                _appConnectionMode = value;
                OnPropertyChanged(nameof(ConnectionMode));
            }
        }
        public Authentication SelectedAuthentication
        {
            get => _selectedAuthentication;
            set
            {
                if (ReferenceEquals(_selectedAuthentication, value))
                    return;

                _selectedAuthentication = value;

                if(_selectedAuthentication.Type == AuthenticationType.SqlServer)
                    AuthorizedWithCredentials = true;
                else
                {
                    AuthorizedWithCredentials = false;

                    UserId = string.Empty;
                    UserPassword?.Clear();
                    UpdatePasswordMessage?.Invoke(null, EventArgs.Empty);
                }

                OnPropertyChanged(nameof(SelectedAuthentication));
            }
        }
        public List<Authentication> AuthenticationTypes
        {
            get => _authenticationTypes;
            set
            {
                if (ReferenceEquals(_authenticationTypes, value))
                    return;
                _authenticationTypes = value;
                OnPropertyChanged(nameof(AuthenticationTypes));
            }
        }
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

        public ICommand ConnectCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Init

        private ExplorerViewModel()
        {
            AuthenticationTypes = new List<Authentication>
            {
                new Authentication("Windows Authentication", AuthenticationType.Windows),
                new Authentication("SQL Server Authentication", AuthenticationType.SqlServer)
            };

            SelectedAuthentication = AuthenticationTypes[0];
            _cancelServerConnectionTokenSource = new CancellationTokenSource();

            ConnectCommand = new RelayCommand(() => ConnectToServerAsync(), CanConnectToServer);
            CancelCommand = new RelayCommand(() => CancelConnectionToServer());
        }

        public bool CanConnectToServer(object o)
        {
            if (!IsServerConnectionEntered)
                return false;

            if (IsConnectionStarted)
                return false;

            if(AuthorizedWithCredentials)
            {
                if (string.IsNullOrEmpty(UserId))
                    return false;

                if (UserPassword == null || UserPassword?.Length == 0)
                    return false;
            }

            return true;
        }

        public ExplorerViewModel(AppConnectionMode connectionMode = AppConnectionMode.New) : this()
        {
            ConnectionMode = connectionMode;

            if (ConnectionMode == AppConnectionMode.Reopen)
                InitializeServerConnection();
        }

        #endregion

        #region Connection 

        private void ConnectToServerAsync()
        {
            ValidationCheckMessage.Invoke(null, EventArgs.Empty);

            var connection = GetServerConnectionString();

            IsConnectionStarted = true;
            if (IsEnteredConnectionValid(connection))
                RunConnection(connection, _cancelServerConnectionTokenSource.Token);
        }

        private async void RunConnection(string connection, CancellationToken token)
        {
            var connectingTask = new ExecutingTask("Connecting to the server ...");
            TaskExecutionMonitor.AttachMonitoring(connectingTask);

            LogWrapper.WriteInfo($"{nameof(ConnectToServerAsync)} has started! ");

            var connectionTester = new ConnectionTester(new SqlServerConnectionContext(connection));

            try
            {
                if (await connectionTester.VerifyConnectionAsync(token))
                {
                    App.ServerConnectionString = connection;
                    App.IsSessionInitialized = true;
                    App.WorkState = AppWorkState.Online;

                    Messenger.Instance.NotifyColleagues(MessageType.OnInitializingProjectFile, ConnectionMode);
                    LogWrapper.WriteInfo($"{nameof(ConnectToServerAsync)} : connection is established!");
                    TaskExecutionMonitor.DetachMonitoring(connectingTask);

                    ConnectionEstablished.Invoke(this, EventArgs.Empty);
                }
                else
                    IsConnectionStarted = false;
            }
            catch (OperationCanceledException ex)
            {
                LogWrapper.WriteError($"{nameof(ConnectToServerAsync)}", ex);
                TaskExecutionMonitor.MonitorOneTime(new ExecutingTask("Connecting to the server was canceled."));

                IsConnectionStarted = false;
                UserId = string.Empty;

                UpdatePasswordMessage.Invoke(null, EventArgs.Empty);
            }
            catch (SqlException ex)
            {
                LogWrapper.WriteError($"{nameof(ConnectToServerAsync)}", ex);
                TaskExecutionMonitor.MonitorOneTime(new ExecutingTask("The server is not responding."));

                MessageBox.Show("Server is not responding.", "Error");

                IsConnectionStarted = false;
                UserId = string.Empty;

                UpdatePasswordMessage.Invoke(null, EventArgs.Empty);
            }
        }
        private void CancelConnectionToServer()
        {
            if (IsConnectionStarted)
                _cancelServerConnectionTokenSource.Cancel();
            else
                ConnectionEstablished.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Helpers

        private string GetServerConnectionString()
        {            
            var builder = new SqlConnectionStringBuilder()
            {
                DataSource = ParseServerConnection()
            };

            switch (_selectedAuthentication.Type)
            {
                case AuthenticationType.Windows:
                    builder.IntegratedSecurity = true;
                    break;

                case AuthenticationType.SqlServer:
                    builder.UserID = UserId;

                    var enteredPass = UserPassword.SecureStringToString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(enteredPass))
                        builder.Password = enteredPass;

                    break;
            }

            return builder.ConnectionString;
        }

        private string ParseServerConnection()
        {
            var source = _selectedServerConnection;
            var regex = new Regex($@":{1}");

            if(regex.IsMatch(source))
            {
                var split = Regex.Split(source, @":");
                var connection = split[0] + "," + split[1];
                return connection;
            }

            return _selectedServerConnection;
        }

        private void InitializeServerConnection()
        {
            var builder = new SqlConnectionStringBuilder(string.Copy(App.ServerConnectionString));
            _previousConnection = builder.ToString();

            SelectedServerConnection = builder.DataSource;

            if (builder.IntegratedSecurity)
                SelectedAuthentication = AuthenticationTypes[0];
            else
                SelectedAuthentication = AuthenticationTypes[1];

            UserId = builder.UserID ?? string.Empty;
        }

        private bool ConnectionHasChanged(string previousVersion, string newVersion)
        {
            return previousVersion != newVersion;
        }
        private bool IsEnteredConnectionValid(string connection)
        {
            if (ConnectionMode != AppConnectionMode.Reopen)
                return true;

            if (!ConnectionHasChanged(_previousConnection, connection))
                return true;

            if (DialogHelper.ConfirmEnteredServerConnection())
                return true;
            else
                return false;
        }

        #endregion
    }
}
