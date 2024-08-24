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

namespace PicSend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ThreadCloser _threadCloser;
        SettingsUC _settingsUC;
        SettingsUC.AppSettings _appSettings;
        public MainWindow()
        {
            CreateConfigFolderIfNonExistent();
            InitializeComponent();
            _threadCloser = new ThreadCloser();
            CreateUserControls();
            _appSettings = _settingsUC.Settings;

            
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
        }

       

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            VersionLabel.Content = $"Software Version : {VersionUpdater.VersionUpdater.CurrentVersion}";
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            _threadCloser.CloseAll(CloseApplication);

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
    }
}