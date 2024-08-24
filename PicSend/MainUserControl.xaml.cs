using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PicSend
{
    /// <summary>
    /// Interaction logic for MainUserControl.xaml
    /// </summary>
    public partial class MainUserControl : UserControl
    {

        SettingsUC.AppSettings _appSettings;
        public MainUserControl(SettingsUC.AppSettings settings)
        {
            InitializeComponent();
            ConnectionStateLabel.Text = "Disconnected";
            _appSettings = settings;
        }
    }
}
