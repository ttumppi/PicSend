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
        string _pictureFolder;
        public SettingsUC()
        {
            InitializeComponent();
            _pictureFolder = string.Empty;
            PictureFolderPathTextBlock.Text = _pictureFolder;
        }

        private void FolderSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog();

            if ((bool)folderDialog.ShowDialog())
            {
                _pictureFolder = folderDialog.FolderName;
                PictureFolderPathTextBlock.Text = _pictureFolder;
            }
        }
    }
}
