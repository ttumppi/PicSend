using Microsoft.Win32;
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
    /// Interaction logic for SettingsUC.xaml
    /// </summary>
    public partial class SettingsUC : UserControl
    {

        private string PictureFolderPath { get; set; } = string.Empty;

        
        public AppSettings Settings { get; }
        public SettingsUC()
        {
            InitializeComponent();
            PictureFolderPathTextBlock.Text = string.Empty;
            Settings = new AppSettings(this);
        }

        private void FolderSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog();

            if ((bool)folderDialog.ShowDialog())
            {
                PictureFolderPath = folderDialog.FolderName;
                PictureFolderPathTextBlock.Text = folderDialog.FolderName;
            }
        }


       
        public class AppSettings
        {
            SettingsUC _settingsUC;
            public string PictureFolderPath
            {
                get { return _settingsUC.PictureFolderPath; }
            }


            public AppSettings(SettingsUC settingsUC)
            {
                _settingsUC = settingsUC;
            }
        }
    }
}
