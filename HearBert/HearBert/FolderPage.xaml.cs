using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace HearBert
{
    /// <summary>
    /// Interaktionslogik für FolderPage.xaml
    /// </summary>
    public partial class FolderPage : Page
    {
        internal App myApp;
        internal MainWindow myWindow;
        internal ProgressDlg myProgress;

        private BindingList<TitleInfo> TitlesList = new BindingList<TitleInfo>();

        private int DraggingOverItem;

        public FolderPage()
        {
            InitializeComponent();
            myApp = App.Current as App;
            myWindow = App.Current.MainWindow as MainWindow;

            // set button background
            Background = myApp._selectedButtonBackground;
            Titles.Background = myApp._selectedButtonBackground;

            // load titles
            LoadTitles();

            // set folder title
            FolderTitle.Text = myApp._HearBertInfo.XML_FolderInfos[myApp._selectedButton - 1].FolderTitle;

            // set drive info
            UpdateDriveInfo();
        }

        private void LoadTitles()
        {
            Titles.ItemsSource = null;
            TitlesList.Clear();
            // fill list with titles
            for (var i = 0; i < myApp._HearBertInfo.XML_FolderInfos[myApp._selectedButton - 1].allTitles.Count; i++)
            {
                var info = new TitleInfo()
                {
                    Number = (i + 1).ToString(),
                    Title = myApp._HearBertInfo.XML_FolderInfos[myApp._selectedButton - 1].allTitles[i].Title,
                    SizePlayTime = myApp._HearBertInfo.GetFriendlySize(myApp._HearBertInfo.XML_FolderInfos[myApp._selectedButton - 1].allTitles[i].Size) + " (" + myApp._HearBertInfo.GetPlayingTime(myApp._HearBertInfo.XML_FolderInfos[myApp._selectedButton - 1].allTitles[i].Size) + ")",
                    Details = myApp._HearBertInfo.XML_FolderInfos[myApp._selectedButton - 1].allTitles[i].Source + " (" + myApp._HearBertInfo.XML_FolderInfos[myApp._selectedButton - 1].allTitles[i].GUID + ")"
                };
                TitlesList.Add(info);
            }
            // set source and it will get displayed
            Titles.ItemsSource = TitlesList;
        }

        private void UpdateDriveInfo()
        {
            DriveInfo.Text = myApp._HearBertInfo.XML_FolderInfos[myApp._selectedButton - 1].FriendlyFolderSize + " [" + myApp._HearBertInfo.GetPlayingTime(myApp._HearBertInfo.XML_FolderInfos[myApp._selectedButton - 1].FolderSize) + "]";
            if (myApp._HearBertInfo.Drive_FriendlyTotalFreeSize != "") DriveInfo.Text += " (" + myApp._HearBertInfo.Drive_FriendlyTotalFreeSize + " frei)";
        }

        private void UpdateXMLFile()
        {
            // re-create XML document and save
            myApp.CreateXMLDocument();
            myApp.SaveXMLFile();
        }

        private T GetDependentChild<T>(DependencyObject parent) where T : DependencyObject
        {
            DependencyObject d = parent;
            while (!(d is T))
            {
                d = VisualTreeHelper.GetChild(d, 0);
            }
            return (d as T);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            myWindow.MainFrame.Navigate(new MainPage());
        }

        private void FolderTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            myApp._HearBertInfo.XML_FolderInfos[myApp._selectedButton - 1].FolderTitle = FolderTitle.Text;
        }

        private void FolderTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateXMLFile();
        }

        private void Titles_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.LeftButton == MouseButtonState.Pressed) && (Titles.SelectedIndex >= 0))
            {
                if ((sender is DataGridCell) && !(sender as DataGridCell).IsEditing)
                {
                    DataObject data = new DataObject(Titles.SelectedItem as TitleInfo);
                    DragDrop.DoDragDrop(sender as DataGridCell, data, DragDropEffects.Move);
                }
            }
        }

        private void Titles_DragEnter(object sender, DragEventArgs e)
        {
            if (!(sender is DataGridCell)) return;
            DataGridCell c = sender as DataGridCell;
            c.Focus();
            DraggingOverItem = Convert.ToInt32((c.DataContext as TitleInfo).Number) - 1;
        }

        private void Titles_DragOver(object sender, DragEventArgs e)
        {
            ScrollViewer sv = GetDependentChild<ScrollViewer>(Titles);
            var pos = e.GetPosition(Titles);

            // auto scroll vertically
            if (pos.Y >= (Titles.ActualHeight - 50))
            {
                // lower bound of Titles
                sv.LineDown();
            } else
            if (pos.Y <= 30)
            {
                // upper bound of Titles
                sv.LineUp();
            }
        }

        private void Titles_DragLeave(object sender, DragEventArgs e)
        {
        }

        private void AddTitlesThread(UInt32 Folder, string[] FileNames)
        {
            int total = FileNames.Length;
            int count = 0;
            int progress;
            bool requestedClose = false;

            foreach (var f in FileNames)
            {
                // check if aborted
                this.Dispatcher.Invoke(() => { requestedClose = myProgress.reqClose; });
                if (requestedClose) break;
                // update information
                this.Dispatcher.Invoke(() => { myProgress.FileInfo.Text = Path.GetFileNameWithoutExtension(f); });
                progress = (count + 1) * 100 / total;
                this.Dispatcher.InvokeAsync(() => { myProgress.SetValueAnimated( progress); });

                // now add file
                if (!myApp.AddTitle(Folder, f))
                {
                    // something went wrong
                    break;
                }

                count++;
            }
            this.Dispatcher.Invoke(() => { myProgress.canClose = true; });
            this.Dispatcher.Invoke(() => { myProgress.Close(); });
        }

        private void AddTitles(string[] FileNames)
        {
            // create progress dialog
            myProgress = new ProgressDlg();
            myProgress.Owner = myApp.MainWindow;
            // create and start thread
            var bg = new Thread(() => AddTitlesThread(Convert.ToUInt32(myApp._selectedButton - 1), FileNames));
            bg.Start();
            // now show dialog
            myProgress.ShowDialog();
            // wait for thread to finish
            bg.Join();
            // reload titles
            LoadTitles();
            // set drive info
            UpdateDriveInfo();
        }

        private void Titles_Drop(object sender, DragEventArgs e)
        {
           if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                AddTitles(files);
            }
            else if (e.Data.GetDataPresent(typeof(TitleInfo)))
            {
                // got TitleInfo from same window -> move operation
                TitleInfo data = e.Data.GetData(typeof(TitleInfo)) as TitleInfo;
                if (myApp.MoveTitle(Convert.ToUInt32(myApp._selectedButton - 1), Convert.ToUInt32(data.Number) - 1, Convert.ToUInt32(DraggingOverItem)))
                {
                    // reload titles
                    LoadTitles();
                }
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            // ask if OK to remove title
            if (MessageBox.Show("Wirklich \"" + (Titles.SelectedItem as TitleInfo).Title + "\" löschen?", "Titel löschen", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (myApp.DeleteTitle(Convert.ToUInt32(myApp._selectedButton - 1), Convert.ToUInt32(Titles.SelectedIndex)))
                {
                    TitlesList.RemoveAt(Titles.SelectedIndex);
                    // set drive info
                    UpdateDriveInfo();
                }
            }
        }

        private void EditTextBox_TextChanged(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            // update title text in XML info and save file
            myApp._HearBertInfo.XML_FolderInfos[myApp._selectedButton - 1].allTitles[Titles.SelectedIndex].Title = tb.Text;
            myApp.CreateXMLDocument();
            myApp.SaveXMLFile();
        }

        private void EditTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            // when edit textbox appears (loaded) then focus for immediate edit
            (sender as TextBox).Focus();
        }

        private void AddTitles_Click(object sender, RoutedEventArgs e)
        {
            var fileDlg = new System.Windows.Forms.OpenFileDialog();
            // Set filter for file extension and default file extension 
            fileDlg.DefaultExt = ".mp3";
            fileDlg.Filter = "MP3 Files (*.mp3)|*.mp3|Wave Files (*.wav)|*.wav|WindowsMediaAudio Files (*.wma)|*.wma|AdvancedAudioCoding Files (*.aac)|*.aac|" +
                             "MPEG4 Audio Files (*.m4?)|*.m4?|Any Files (*.*)|*.*";
            fileDlg.CheckPathExists = true;
            fileDlg.Multiselect = true;
            fileDlg.RestoreDirectory = true;
            fileDlg.Title = "Auswahl Musikdateien";
            fileDlg.ValidateNames = true;

            var result = fileDlg.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    AddTitles(fileDlg.FileNames);
                    break;
                default:
                    break;
            }
        }

        private void CleanFolder_Click(object sender, RoutedEventArgs e)
        {
            // ask if OK to clean folder
            if (MessageBox.Show("Wirklich gesamten Ordner \"" + FolderTitle.Text + "\" löschen?", "Ordner löschen", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                while (TitlesList.Count > 0)
                {
                    // always delete last -> no file renaming required
                    if (myApp.DeleteTitle(Convert.ToUInt32(myApp._selectedButton - 1), Convert.ToUInt32(TitlesList.Count - 1)))
                    {
                        TitlesList.RemoveAt(TitlesList.Count - 1);
                        // set drive info
                        UpdateDriveInfo();
                    }
                    else break;
                }
            }
        }

    }

        public class TitleInfo
    {
        public String Number { get; set; }
        public String Title { get; set; }
        public String SizePlayTime { get; set; }
        public String Details { get; set; }
    }
}
