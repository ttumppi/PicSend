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
using System.IO;
using System.Text.Json.Serialization;
using System.Reflection;

namespace PicSend
{
    /// <summary>
    /// Interaction logic for SettingsUC.xaml
    /// </summary>
    public partial class SettingsUC : UserControl
    {

        private IAppSettings _modifiableSettings;
        
        public AppSettings Settings { get; private set; }
        public SettingsUC()
        {
            InitializeComponent();

            Settings = TryReadSettingsFromFile();
            _modifiableSettings = Settings;
            ChangePictureFolderPath(Settings.PictureFolderPath);
            
           
            
        }

        private AppSettings TryReadSettingsFromFile()
        {
            AppSettings? settings = JsonReadWrite.ReadObject<AppSettings>(AppSettings.AppSettingsPath);

            if (settings is null)
            {
                return new AppSettings();
            }

            return settings;
        }

        private void ChangePictureFolderPath(string path)
        {
            _modifiableSettings.PictureFolderPath = path;
            PictureFolderPathTextBlock.Text = path;
        }

       

        private void FolderSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog();

            if ((bool)folderDialog.ShowDialog())
            {
                ChangePictureFolderPath(folderDialog.FolderName);
            }
        }


       
        public class AppSettings : IAppSettings
        {


            [JsonPropertyName("Picture Save Folder")]
            [JsonInclude]
            public string PictureFolderPath
            {
                get;
                private set;
            } = string.Empty;

            [JsonIgnore]
            string IAppSettings.PictureFolderPath 
            { 
                get { return PictureFolderPath; }
                set { PictureFolderPath = value; } 
            }

            [JsonIgnore]
            public static string ConfigFolderPath { get;  } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");

            [JsonIgnore]
            public static string AppSettingsPath { get;  } = Path.Combine(ConfigFolderPath, "AppSettings.JSON");


            public AppSettings()
            {
                CreateAppSettingsFileIfNonExistent();
            }

            

            private void CreateAppSettingsFileIfNonExistent()
            {
                if (!File.Exists(AppSettingsPath))
                {
                    File.Create(AppSettingsPath).Close();
                }
            }

           
        }


        private interface IAppSettings
        {
            string PictureFolderPath { get; set; }
        }
    }
}
