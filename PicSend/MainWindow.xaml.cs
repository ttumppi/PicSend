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

namespace PicSend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ThreadCloser _threadCloser;
        SettingsUC _settingsUC;
        public MainWindow()
        {
            InitializeComponent();
            _threadCloser = new ThreadCloser();
            CreateUserControls();
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
                Dispatcher.Invoke(CloseApplication);
                return;
            }


            Application.Current.Shutdown();
        }
    }
}