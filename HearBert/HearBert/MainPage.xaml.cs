using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HearBert
{
    /// <summary>
    /// Interaktionslogik für MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        internal App myApp;
        internal MainWindow myWindow;
        
        public MainPage()
        {
            InitializeComponent();
            myApp = App.Current as App;
            myWindow = App.Current.MainWindow as MainWindow;

            SelectedDrive.Text = myApp._selectedDrive;

            Loaded += ReloadMainPage;
        }

        public void ReloadMainPage(object sender, RoutedEventArgs e)
        {
            if (myApp._selectedDrive != "")
            {
                if (myApp.ReloadHearBert())
                {
                    UpdateButtonContents();
                }
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            myApp._selectedButton = 1;
            myApp._selectedButtonBackground = Button1.Background;
            myWindow.MainFrame.Navigate(new FolderPage());
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            myApp._selectedButton = 2;
            myApp._selectedButtonBackground = Button2.Background;
            myWindow.MainFrame.Navigate(new FolderPage());
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            myApp._selectedButton = 3;
            myApp._selectedButtonBackground = Button3.Background;
            myWindow.MainFrame.Navigate(new FolderPage());
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            myApp._selectedButton = 4;
            myApp._selectedButtonBackground = Button4.Background;
            myWindow.MainFrame.Navigate(new FolderPage());
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            myApp._selectedButton = 5;
            myApp._selectedButtonBackground = Button5.Background;
            myWindow.MainFrame.Navigate(new FolderPage());
        }

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            myApp._selectedButton = 6;
            myApp._selectedButtonBackground = Button6.Background;
            myWindow.MainFrame.Navigate(new FolderPage());
        }

        private void Button7_Click(object sender, RoutedEventArgs e)
        {
            myApp._selectedButton = 7;
            myApp._selectedButtonBackground = Button7.Background;
            myWindow.MainFrame.Navigate(new FolderPage());
        }

        private void Button8_Click(object sender, RoutedEventArgs e)
        {
            myApp._selectedButton = 8;
            myApp._selectedButtonBackground = Button8.Background;
            myWindow.MainFrame.Navigate(new FolderPage());
        }

        private void Button9_Click(object sender, RoutedEventArgs e)
        {
            myApp._selectedButton = 9;
            myApp._selectedButtonBackground = Button9.Background;
            myWindow.MainFrame.Navigate(new FolderPage());
        }

        private void SelectDrive_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "Hörbert-Laufwerk auswählen";
            dlg.SelectedPath = myApp._selectedDrive; // pre-set selected path
            dlg.ShowNewFolderButton = false;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                myApp._selectedDrive = dlg.SelectedPath; // retrieve selected path
                SelectedDrive.Text = myApp._selectedDrive; // update textbox
                HearBert.Properties.Settings.Default.SelectedDrive = myApp._selectedDrive; // store selected drive for later usage
                HearBert.Properties.Settings.Default.Save();
                RefreshDrive_Click(null, null);
            }
        }

        private void RefreshDrive_Click(object sender, RoutedEventArgs e)
        {
            myApp.ReloadHearBert();
            UpdateButtonContents();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            // open splash screen, it will close automatically
            SplashScreen splash = new SplashScreen();
            splash.ShowLong = true;
            splash.Owner = myWindow;
            splash.ShowDialog();
        }

        private void UpdateButtonContents()
        {
            // assign button contents
            FolderTitle1.Text = myApp._HearBertInfo.XML_FolderInfos[0].FolderTitle;
            FolderSize1.Text = myApp._HearBertInfo.XML_FolderInfos[0].FriendlyFolderSize;
            FolderTitle2.Text = myApp._HearBertInfo.XML_FolderInfos[1].FolderTitle;
            FolderSize2.Text = myApp._HearBertInfo.XML_FolderInfos[1].FriendlyFolderSize;
            FolderTitle3.Text = myApp._HearBertInfo.XML_FolderInfos[2].FolderTitle;
            FolderSize3.Text = myApp._HearBertInfo.XML_FolderInfos[2].FriendlyFolderSize;
            FolderTitle4.Text = myApp._HearBertInfo.XML_FolderInfos[3].FolderTitle;
            FolderSize4.Text = myApp._HearBertInfo.XML_FolderInfos[3].FriendlyFolderSize;
            FolderTitle5.Text = myApp._HearBertInfo.XML_FolderInfos[4].FolderTitle;
            FolderSize5.Text = myApp._HearBertInfo.XML_FolderInfos[4].FriendlyFolderSize;
            FolderTitle6.Text = myApp._HearBertInfo.XML_FolderInfos[5].FolderTitle;
            FolderSize6.Text = myApp._HearBertInfo.XML_FolderInfos[5].FriendlyFolderSize;
            FolderTitle7.Text = myApp._HearBertInfo.XML_FolderInfos[6].FolderTitle;
            FolderSize7.Text = myApp._HearBertInfo.XML_FolderInfos[6].FriendlyFolderSize;
            FolderTitle8.Text = myApp._HearBertInfo.XML_FolderInfos[7].FolderTitle;
            FolderSize8.Text = myApp._HearBertInfo.XML_FolderInfos[7].FriendlyFolderSize;
            FolderTitle9.Text = myApp._HearBertInfo.XML_FolderInfos[8].FolderTitle;
            FolderSize9.Text = myApp._HearBertInfo.XML_FolderInfos[8].FriendlyFolderSize;

            DriveInfo.Content = myApp._HearBertInfo.Drive_FriendlyTotalFoldersSize + " [" + myApp._HearBertInfo.GetPlayingTime(myApp._HearBertInfo.Drive_TotalFoldersSize) + "]";
            if (myApp._HearBertInfo.Drive_TotalFreeSize > 0)
            {
                DriveInfo.Content += " (" + myApp._HearBertInfo.Drive_FriendlyTotalFreeSize + " frei)";
            }

            Button1.IsEnabled = myApp._HearBertInfo.infoFileValid;
            Button2.IsEnabled = myApp._HearBertInfo.infoFileValid;
            Button3.IsEnabled = myApp._HearBertInfo.infoFileValid;
            Button4.IsEnabled = myApp._HearBertInfo.infoFileValid;
            Button5.IsEnabled = myApp._HearBertInfo.infoFileValid;
            Button6.IsEnabled = myApp._HearBertInfo.infoFileValid;
            Button7.IsEnabled = myApp._HearBertInfo.infoFileValid;
            Button8.IsEnabled = myApp._HearBertInfo.infoFileValid;
            Button9.IsEnabled = myApp._HearBertInfo.infoFileValid;
        }
    }
}
