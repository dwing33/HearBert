using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml;

namespace HearBert
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public String _selectedDrive;
        public UInt64 _selectedButton;
        public Brush _selectedButtonBackground;
        public String _HoerbertXML;
        public CHearBertInfo _HearBertInfo;

        App()
        {
            HearBert.Properties.Settings.Default.Reload();
            _HoerbertXML = HearBert.Properties.Settings.Default.HearBertXML;
            _selectedDrive = HearBert.Properties.Settings.Default.SelectedDrive;
            _HearBertInfo = new CHearBertInfo();
        }

        public bool ReloadHearBert()
        {
            bool XML_OK = false;
            bool Drive_OK = false;

            // load of XML file and drive structure
            if (LoadXMLFile()) XML_OK = true;
            if (LoadDrive()) Drive_OK = true;

            // both okay?
            if (XML_OK && Drive_OK)
            {
                Debug.WriteLine("Read XML File and Drive without errors");
                // now check consistency
                if (CheckXMLvsDrive())
                {
                    Debug.WriteLine("XML and Drive structure are consistent");
                    return true;
                }
            }

            // ask user for what to use as reference or abort
            ReCreateDlg newdlg = new ReCreateDlg();
            if (newdlg.ShowDialog() == true)
            {
                if (newdlg.Selection == ReCreateDlg.DlgSelection.Drive)
                {
                    Debug.WriteLine("Drive is reference");
                    FixXMLFromDrive(); // fix XML first
                    CreateXMLDocument(); // create new XML document
                    if (SaveXMLFile()) // save it
                    {
                        return ReloadHearBert();
                    }
                    else return false;
                }
                else if (newdlg.Selection == ReCreateDlg.DlgSelection.AllNew)
                {
                    Debug.WriteLine("Recreate");
                    // Create new XML and create all folders on drive, empty folders!
                    if (CreateNewHearBertDrive())
                    {
                        // clear XML info
                        for (var i = 0; i < 9; i++)
                        {
                            _HearBertInfo.XML_FolderInfos[i].allTitles.Clear();
                            _HearBertInfo.XML_FolderInfos[i].FolderTitle = "Ordner " + (i + 1).ToString();
                        }
                        CreateXMLDocument();
                        if (SaveXMLFile())
                        { // save XML
                            return ReloadHearBert(); // try to reload
                        }
                        else return false;
                    }
                }
                else return false; // abort
            }
            else return false;
            return true;
        }

        public void FixXMLFromDrive()
        {
            // Throw out all items from XML that don't exist on drive and drive items that don't exist in XML -> copy/recreate
            for (var i = 0; i < 9; i++)
            {
                // remove XML items that don't exist on drive
                while (_HearBertInfo.XML_FolderInfos[i].allTitles.Count > _HearBertInfo.Drive_FolderInfos[i].allTitles.Count)
                {
                    // remove last
                    _HearBertInfo.XML_FolderInfos[i].allTitles.RemoveAt(_HearBertInfo.XML_FolderInfos[i].allTitles.Count - 1);
                }
                // add XML items that exist on drive but not in XML
                while (_HearBertInfo.XML_FolderInfos[i].allTitles.Count < _HearBertInfo.Drive_FolderInfos[i].allTitles.Count)
                {
                    var title = new STitleInfo();
                    _HearBertInfo.XML_FolderInfos[i].allTitles.Add(title);
                    title.Title = "";
                    title.Size = 0;
                    title.Source = "";
                    title.GUID = "XXXX-XXXX-XXXX-XXXX";
                }

                for (var j = 0; j < _HearBertInfo.XML_FolderInfos[i].allTitles.Count; j++)
                {
                    // copy size of item from drive info
                    _HearBertInfo.XML_FolderInfos[i].allTitles[j].Size = _HearBertInfo.Drive_FolderInfos[i].allTitles[j].Size;
                    // check if Title is empty in XML and use filename
                    if (_HearBertInfo.XML_FolderInfos[i].allTitles[j].Title == "")
                    {
                        _HearBertInfo.XML_FolderInfos[i].allTitles[j].Title = _HearBertInfo.Drive_FolderInfos[i].allTitles[j].Title;
                    }
                }
            }
        }

        public void CreateXMLDocument()
        {
            _HearBertInfo.infoFile = new XmlDocument();
            _HearBertInfo.infoFileValid = true;

            XmlDocument doc = _HearBertInfo.infoFile;

            // create a procesing instruction.
            var newPI = doc.CreateProcessingInstruction("xml", " version='1.0' encoding='utf-8'");
            doc.AppendChild(newPI);

            var hoerbert = doc.CreateElement("hoerbert");
            doc.AppendChild(hoerbert);
            var hoerbert_playlists = doc.CreateElement("hoerbert_playlists");
            hoerbert.AppendChild(hoerbert_playlists);
            var folders = doc.CreateElement("folders");
            hoerbert_playlists.AppendChild(folders);

            // create folders
            for (var i = 0; i < 9; i++)
            {
                var folder = doc.CreateElement("folder");
                folder.SetAttribute("id", i.ToString());
                folder.SetAttribute("title", _HearBertInfo.XML_FolderInfos[i].FolderTitle);
                folders.AppendChild(folder);
                var items = doc.CreateElement("items");
                folder.AppendChild(items);
                for (var j = 0; j < _HearBertInfo.XML_FolderInfos[i].allTitles.Count; j++)
                {
                    var item = doc.CreateElement("item");
                    item.SetAttribute("guid", _HearBertInfo.XML_FolderInfos[i].allTitles[j].GUID);
                    // sequence node
                    var sequence = doc.CreateElement("sequence");
                    sequence.InnerText = j.ToString();
                    item.AppendChild(sequence);
                    // source node
                    var source = doc.CreateElement("source");
                    source.InnerText = _HearBertInfo.XML_FolderInfos[i].allTitles[j].Source;
                    item.AppendChild(source);
                    // userLabel node
                    var userlabel = doc.CreateElement("userLabel");
                    userlabel.InnerText = _HearBertInfo.XML_FolderInfos[i].allTitles[j].Title;
                    item.AppendChild(userlabel);
                    // byteSize node
                    var bytesize = doc.CreateElement("byteSize");
                    bytesize.InnerText = _HearBertInfo.XML_FolderInfos[i].allTitles[j].Size.ToString();
                    item.AppendChild(bytesize);
                    // append item to items
                    items.AppendChild(item);
                }
            }
        }

        public bool LoadXMLFile()
        {
            _HearBertInfo.infoFileValid = false; // mark invalid first
            // clear XML information
            for (var i = 0; i < 9; i++)
            {
                _HearBertInfo.XML_FolderInfos[i].FolderTitle = "";
                _HearBertInfo.XML_FolderInfos[i].allTitles.Clear();
            }

            // load XML document
            XmlDocument xml = new XmlDocument();
            try
            {
                xml.Load(_selectedDrive + "\\" + _HoerbertXML);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Fehler beim Lesen von XML");
                return false;
            }

            _HearBertInfo.infoFile = xml; // store document
            _HearBertInfo.infoFileValid = true; // mark valid

            // parse XML
            var rootelement = _HearBertInfo.infoFile.DocumentElement; // expect to be "hoerbert"
            var folders = rootelement.SelectSingleNode("hoerbert_playlists/folders"); // select folders
            if (folders != null)
            {
                _HearBertInfo.infoFileValid = true; // even though content might be corrupt, information is valid
                XmlNodeList folder_l = folders.SelectNodes("folder"); // select all folder nodes (should be max 9)
                if (folder_l.Count > 0)
                {
                    Int32 folder_id;
                    String folder_title;
                    Int32 item_id;
                    String item_guid;
                    String item_source;
                    String item_label;
                    UInt64 item_size;

                    for (var i = 0; i < folder_l.Count; i++) // iterate through each folder
                    {
                        var node = folder_l[i];
                        var folder_id_attr = node.Attributes.GetNamedItem("id");
                        if (folder_id_attr != null)
                        {
                            folder_id = Convert.ToInt32(folder_id_attr.InnerText);
                        }
                        else
                        {
                            folder_id = -1;
                        }
                        var folder_title_attr = node.Attributes.GetNamedItem("title");
                        if (folder_title_attr != null)
                        {
                            folder_title = folder_title_attr.InnerText;
                        }
                        else
                        {
                            folder_title = "Ordner " + (folder_id + 1).ToString();
                        }
                        _HearBertInfo.XML_FolderInfos[folder_id].FolderTitle = folder_title;
                        var items_l = node.SelectNodes("items/item");
                        Debug.WriteLine("Folder ID: " + folder_id + ", " + items_l.Count + " Items");
                        for (var j = 0; j < items_l.Count; j++)
                        {
                            var guid_attr = items_l[j].Attributes.GetNamedItem("guid");
                            if (guid_attr != null)
                            {
                                item_guid = guid_attr.InnerText;
                            }
                            else
                            {
                                item_guid = "?";
                            }
                            var item_id_node = items_l[j].SelectSingleNode("sequence");
                            if (item_id_node != null)
                            {
                                item_id = Convert.ToInt32(item_id_node.InnerText);
                            }
                            else
                            {
                                item_id = -1;
                            }
                            var item_label_node = items_l[j].SelectSingleNode("userLabel");
                            if (item_label_node != null)
                            {
                                item_label = item_label_node.InnerText;
                            }
                            else
                            {
                                item_label = "?";
                            }
                            var item_source_node = items_l[j].SelectSingleNode("source");
                            if (item_source_node != null)
                            {
                                item_source = item_source_node.InnerText;
                            }
                            else
                            {
                                item_source = "?";
                            }
                            var item_size_node = items_l[j].SelectSingleNode("byteSize");
                            if (item_size_node != null)
                            {
                                item_size = Convert.ToUInt64(item_size_node.InnerText);
                            }
                            else
                            {
                                item_size = 0;
                            }
                            Debug.WriteLine(" Item #" + item_id);
                            Debug.WriteLine("  GUID:" + item_guid);
                            Debug.WriteLine("  Label:" + item_label);
                            Debug.WriteLine("  Size:" + item_size);
                            Debug.WriteLine("  Source:" + item_source);
                            // create new title info in folder
                            var new_title = new STitleInfo();
                            _HearBertInfo.XML_FolderInfos[folder_id].allTitles.Add(new_title);
                            new_title.Title = item_label;
                            new_title.Size = item_size;
                            new_title.Source = item_source;
                            new_title.GUID = item_guid;
                        } // for
                    } // for
                } // if
            } // if

            return true;
        }

        public bool SaveXMLFile()
        {
            // open source file (if exists) and create a backup file
            try
            {
                var srcFile = _selectedDrive + "\\" + _HoerbertXML;
                if (File.Exists(srcFile))
                {
                    File.Copy(srcFile, srcFile + ".bak", true);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Fehler beim Anlegen von XML Backup");
                return false;
            }
            // save XML file
            try
            {
                _HearBertInfo.infoFile.Save(_selectedDrive + "\\" + _HoerbertXML);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Fehler beim Speichern von XML");
                return false;
            }
            return true;
        }

        public bool LoadDrive()
        {
            // clear drive info
            for (var i = 0; i < 9; i++)
            {
                _HearBertInfo.Drive_FolderInfos[i].allTitles.Clear();
            }
            // read drive
            // iterate through each folder "0" to "8"
            for (var i = 0; i < 9; i++)
            {
                var titleidx = 0;
                while (true)
                {
                    var filename = _selectedDrive + "\\" + i + "\\" + titleidx + ".wav";
                    // check if file exists
                    if (!File.Exists(filename)) break;
                    // get file information
                    var fi = new FileInfo(filename);
                    Debug.WriteLine("Drive Folder " + i + ", File " + fi.Name + " (" + fi.Length + " Bytes)");
                    // create TitleInfo
                    STitleInfo r = new STitleInfo();
                    r.Title = fi.Name;
                    r.Size = Convert.ToUInt64(fi.Length);
                    r.Source = filename;
                    r.GUID = "XXXX-XXXX-XXXX-XXXX"; // none
                    _HearBertInfo.Drive_FolderInfos[i].allTitles.Add(r);

                    // next one
                    titleidx++;
                }
                _HearBertInfo.RecalcDriveTotalFoldersSize();

                _HearBertInfo.Drive_TotalFreeSize = 0;
                _HearBertInfo.Drive_FriendlyTotalFreeSize = "";

                // determine free space, does not work on UNC paths or on mounted volumes within path
                DriveInfo[] drives = DriveInfo.GetDrives();
                foreach (var d in drives)
                {
                    if (d.Name[0] == _selectedDrive[0])
                    {
                        if (d.IsReady)
                        {
                            _HearBertInfo.Drive_TotalFreeSize = Convert.ToUInt64(d.AvailableFreeSpace);
                        } else
                        {
                            _HearBertInfo.Drive_TotalFreeSize = 0;
                        }
                    }
                }
                _HearBertInfo.Drive_FriendlyTotalFreeSize = _HearBertInfo.GetFriendlySize(_HearBertInfo.Drive_TotalFreeSize);
            }

            return true;
        }

        public bool CreateNewHearBertDrive()
        {
            // clear internal structure
            for (var i = 0; i < 9; i++)
            {
                _HearBertInfo.Drive_FolderInfos[i].FolderTitle = "Ordner " + (i + 1).ToString();
                _HearBertInfo.Drive_FolderInfos[i].allTitles.Clear();
            }

            // create empty folder structure
            for (var i = 0; i < 9; i++)
            {
                // create folder
                try
                {
                    // if folder exists throw exception
                    if (Directory.Exists(_selectedDrive + "\\" + i.ToString()))
                    {
                        throw new Exception("Directory " + _selectedDrive + "\\" + i.ToString() + " exists");
                    }
                    // otherwise create directory
                    Directory.CreateDirectory( _selectedDrive + "\\" + i.ToString());
                }
                catch (Exception) // creating directory failed -> try to delete all items because it exists
                {
                    try
                    {
                        // recursively delete directory
                        Directory.Delete(_selectedDrive + "\\" + i.ToString(), true);
                        // recreate directory
                        Directory.CreateDirectory(_selectedDrive + "\\" + i.ToString());
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString(), "Fehler beim Löschen");
                    }
                }
               
            }

            return true;
        }

        public bool CheckXMLvsDrive()
        {
            for (var i = 0; i < 9; i++)
            {
                var length = _HearBertInfo.Drive_FolderInfos[i].allTitles.Count;
                // if length mismatches return false
                if (length != _HearBertInfo.XML_FolderInfos[i].allTitles.Count)
                    return false;
                // otherwise check items by position and size
                for (var j = 0; j < length; j++)
                {
                    if (_HearBertInfo.XML_FolderInfos[i].allTitles[j].Size != _HearBertInfo.Drive_FolderInfos[i].allTitles[j].Size)
                        return false;
                }
            }
            return true;
        }

        public bool DeleteTitle(UInt32 Folder, UInt32 Title)
        {
            // remove title from info structures
            _HearBertInfo.Drive_FolderInfos[Folder].allTitles.RemoveAt(Convert.ToInt32(Title));
            _HearBertInfo.XML_FolderInfos[Folder].allTitles.RemoveAt(Convert.ToInt32(Title));
            _HearBertInfo.RecalcDriveTotalFoldersSize();

            // re-create XML and try save file
            CreateXMLDocument();
            if (!SaveXMLFile())
            {
                // in error case try to reload
                ReloadHearBert();
                return false;
            }

            // now try deleting file
            var filename = _selectedDrive + "\\" + Folder + "\\" + Title + ".WAV";
            try
            {
                File.Delete(filename);
            }
            catch (Exception)
            {
                // only inform about fail to delete file
                MessageBox.Show("Löschen von " + filename + " fehlgeschlagen", "Fehler");
                return false;
            }
            // rename all following files and fill the gap
            try
            {
                for (var i = Title; i < _HearBertInfo.Drive_FolderInfos[Folder].allTitles.Count; i++)
                {
                    filename = _selectedDrive + "\\" + Folder + "\\" + (i + 1).ToString() + ".WAV";
                    var newfilename = _selectedDrive + "\\" + Folder + "\\" + (i).ToString() + ".WAV";
                    File.Move(filename, newfilename);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Umbenennen von " + filename + " fehlgeschlagen", "Fehler");
                return false;
            }

            // reload drive
            LoadDrive();

            return true;
        }

        public bool MoveTitle(UInt32 Folder, UInt32 TitleSrc, UInt32 TitleDst)
        {
            // move title in info structures
            var srcDrv = _HearBertInfo.Drive_FolderInfos[Folder].allTitles[Convert.ToInt32(TitleSrc)];
            var srcXML = _HearBertInfo.XML_FolderInfos[Folder].allTitles[Convert.ToInt32(TitleSrc)];

            // remove source
            _HearBertInfo.Drive_FolderInfos[Folder].allTitles.RemoveAt(Convert.ToInt32(TitleSrc));
            _HearBertInfo.XML_FolderInfos[Folder].allTitles.RemoveAt(Convert.ToInt32(TitleSrc));
            // add destination
            _HearBertInfo.Drive_FolderInfos[Folder].allTitles.Insert(Convert.ToInt32(TitleDst), srcDrv);
            _HearBertInfo.XML_FolderInfos[Folder].allTitles.Insert(Convert.ToInt32(TitleDst), srcXML);

            // re-create XML and try save file
            CreateXMLDocument();
            if (!SaveXMLFile())
            {
                // in error case try to reload
                return ReloadHearBert();
            }

            // move files by renaming
            List<string> NewFiles = new List<string>();
            // first iteration renames mismatching indices to *.wav.new in order to get target naming scheme
            for (var i = 0; i < _HearBertInfo.Drive_FolderInfos[Folder].allTitles.Count; i++)
            {
                var SrcName = _HearBertInfo.Drive_FolderInfos[Folder].allTitles[i].Title;
                var DstName = i + ".WAV";
                // if file name is not as expected -> rename to desired name and append ".new"
                if (!Regex.IsMatch(SrcName, DstName, RegexOptions.IgnoreCase))
                {
                    Debug.WriteLine("Renaming file " + SrcName + " to " + DstName + ".new");
                    try
                    {
                        File.Move(_selectedDrive + "\\" + Folder + "\\" + SrcName, _selectedDrive + "\\" + Folder + "\\" + DstName + ".new");
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Konnte \"" + _selectedDrive + "\\" + Folder + "\\" + SrcName + "\" nicht in \"" + DstName + ".new\" umbenennen");
                        return false;
                    }
                    NewFiles.Add(DstName + ".new");
                }
            }
            // second iteration renames all new indices back from ...new to target name
            foreach (var nf in NewFiles)
            {
                var nff = nf.ToString().Substring(0, nf.Length - 4);
                Debug.WriteLine("Renaming file " + nf + " to " + nff);
                try
                {
                    File.Move(_selectedDrive + "\\" + Folder + "\\" + nf, _selectedDrive + "\\" + Folder + "\\" + nff);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Konnte \"" + _selectedDrive + "\\" + Folder + "\\" + nf + "\" nicht umbenennen");
                    return false;
                }
            }

            return LoadDrive(); // now reload drive information
        }

        public bool AddTitle(UInt32 Folder, string FileName)
        {
            Debug.WriteLine("Adding file " + FileName);
            string newTitle;
            try
            {
                var tagFile = TagLib.File.Create(FileName);
                newTitle = tagFile.Tag.Album + " - " + tagFile.Tag.Title;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Fehler beim Lesen");
                return false;
            }

            // Target Hörbert Format: 16-Bit, 32kHz Mono, WAV PCM
            var wavFile = _HearBertInfo.Drive_FolderInfos[Folder].allTitles.Count + ".wav";
            var targetFilename = _selectedDrive + "\\" + Folder + "\\" + wavFile;
            MediaFoundationReader reader;
            try
            {
                reader = new MediaFoundationReader(FileName);
                var HoerbertFmt = new WaveFormat(32000, 16, 1);
                var pcmStream = new WaveFormatConversionStream(HoerbertFmt, reader);
                Debug.WriteLine("Converting into " + targetFilename);
                WaveFileWriter.CreateWaveFile(targetFilename, pcmStream);
                Debug.WriteLine("Done.");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Fehler beim Konvertieren");
                return false;
            }

            UInt64 fileSize = Convert.ToUInt64(new FileInfo(targetFilename).Length);

            var hasher = MD5.Create();
            var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(FileName));
            string fileGUID = "";
            for (var i = 0; i < 16; i += 4)
            {
                if (fileGUID != "") fileGUID += "-";
                fileGUID += hash[i].ToString("X2") + hash[i + 1].ToString("X2") +
                            hash[i + 2].ToString("X2") + hash[i + 3].ToString("X2");
            }
            _HearBertInfo.XML_FolderInfos[Folder].allTitles.Add(new STitleInfo()
            {
                Title = newTitle,
                Size = fileSize,
                Source = FileName,
                GUID = fileGUID
            });
            _HearBertInfo.Drive_FolderInfos[Folder].allTitles.Add(new STitleInfo()
            {
                Title = wavFile,
                Size = fileSize,
                Source = FileName,
                GUID = fileGUID
            });
            _HearBertInfo.RecalcDriveTotalFoldersSize();
            // re-create XML and try save file
            CreateXMLDocument();
            if (!SaveXMLFile())
            {
                // in error case try to reload
                ReloadHearBert();
                return false;
            }
            return true;
        }

    } // class App
} // namespace
