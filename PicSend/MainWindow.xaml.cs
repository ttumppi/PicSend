using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using VersionUpdater;
using System.IO;
using ReceiptManager;

namespace PicSend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ThreadCloser _threadCloser;

        SettingsUC _settingsUC;
        MainUserControl _mainUserControl;

        SettingsUC.AppSettings _appSettings;

        Dictionary<UserControls, UserControl> _userControls;

        SolidColorBrush _selectedButtonColor;

        Brush _nonSelectedButton;

        Queue<Button> _pressedButtons;

        PictureCommunication _pictureComms;

        public MainWindow()
        {
            CreateConfigFolderIfNonExistent();
            InitializeComponent();
            _threadCloser = new ThreadCloser();
            _userControls = new Dictionary<UserControls, UserControl>();
            ModifyControls();

            CreateUserControls();
            _appSettings = _settingsUC.Settings;

            _selectedButtonColor = new SolidColorBrush(Colors.LightBlue);
            _nonSelectedButton = MainButton.Background;

            MainButton.Background = _selectedButtonColor;
            
            _pressedButtons = new Queue<Button>();
            _pressedButtons.Enqueue(MainButton);

            _pictureComms = new PictureCommunication(_appSettings);
            _threadCloser.Add(_pictureComms);
            _pictureComms.ConnectionChanged += ConnectionStateChanged;

            _pictureComms.Start();

        }

        private void ModifyControls()
        {
            LogButton.Visibility = Visibility.Collapsed;
            InfoButton.Visibility = Visibility.Collapsed;
        }

        private void CreateConfigFolderIfNonExistent()
        {
            if (!Directory.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config")))
            {
                Directory.CreateDirectory(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"));
            }
        }

        private void CreateUserControls()
        {
            _settingsUC = new SettingsUC();
            ViewGrid.Children.Add( _settingsUC );
            _userControls.Add(UserControls.Settings, _settingsUC );

            _mainUserControl = new MainUserControl(_settingsUC.Settings);
            ViewGrid.Children.Add( _mainUserControl );
            _userControls.Add(UserControls.MainView, _mainUserControl );

        }

       

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            VersionLabel.Content = $"Software Version : {VersionUpdater.VersionUpdater.CurrentVersion}";
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            _threadCloser.CloseAll(CloseApplication);

        }

        private void ConnectionStateChanged(object? sender, ServerSocket.ConnectionChangedEventArgs.ConnectionState state)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ConnectionStateChanged(sender, state);
                }));
                return;
            }

            _mainUserControl.ChangeConnectionStatusText(state.ToString());
        }

        private void CloseApplication()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(CloseApplication));
                return;
            }

            JsonReadWrite.WriteObject<SettingsUC.AppSettings>(SettingsUC.AppSettings.AppSettingsPath, _appSettings);

            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            
            _threadCloser.CloseAll(CloseApplication);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedButton(SettingsButton);
            BringToFront(UserControls.Settings);
        }

        private void ChangeSelectedButton(Button selected)
        {
            _pressedButtons.Dequeue().Background = _nonSelectedButton;
            _pressedButtons.Enqueue(selected);
            selected.Background = _selectedButtonColor;
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedButton(MainButton);

            BringToFront(UserControls.MainView);
        }

        private void BringToFront(UserControls ucControl)
        {
            foreach (KeyValuePair<UserControls, UserControl> pair in _userControls)
            {
                if (pair.Key == ucControl)
                {
                    Panel.SetZIndex(pair.Value, 1);
                }
                else
                {
                    Panel.SetZIndex(pair.Value, 0);
                }
            }
        }


        public enum UserControls
        {
            None = 0,
            MainView = 1,
            Settings = 2,
        }
    }
}