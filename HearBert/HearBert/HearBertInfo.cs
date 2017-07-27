using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HearBert
{
    public class STitleInfo
    {
        public String Title { get; set; }
        public UInt64 Size { get; set; }
        public String Source { get; set; }
        public String GUID { get; set; }
    }

    public class SFolderInfo
    {
        public String FolderTitle { get; set; }
        public UInt64 FolderItems { get; set; }
        public UInt64 FolderSize { get; set; }
        public String FriendlyFolderSize { get; set; }

        public List<STitleInfo> allTitles;

        public SFolderInfo()
        {
            allTitles = new List<STitleInfo>();
        }
    }

    public class CHearBertInfo
    {
        public XmlDocument infoFile { get; set; }
        public bool infoFileValid { get; set; }
        public SFolderInfo[] XML_FolderInfos;
        public SFolderInfo[] Drive_FolderInfos;
        public UInt64 Drive_TotalFoldersSize { get; set; }
        public String Drive_FriendlyTotalFoldersSize { get; set; }
        public UInt64 Drive_TotalFreeSize { get; set; }
        public String Drive_FriendlyTotalFreeSize { get; set; }

        public CHearBertInfo()
        {
            XML_FolderInfos = new SFolderInfo[9];
            Drive_FolderInfos = new SFolderInfo[9];
            for (var i = 0; i < 9; i++)
            {
                XML_FolderInfos[i] = new SFolderInfo();
                Drive_FolderInfos[i] = new SFolderInfo();
            }
        }

        public string GetFriendlySize(UInt64 size)
        {
            String r = "";

            if (size < 1000)
            {
                r = size + " Bytes";
            }
            else if (size < (1000 * 1000))
            {
                r = String.Format("{0:N2} KB", (size / 1.0E3));
            }
            else if (size < (1000 * 1000 * 1000))
            {
                r = String.Format("{0:N2} MB", (size / 1.0E6));
            }
            else if (size > 0)
            {
                r = String.Format("{0:N2} GB", (size / 1.0E9));
            }

            return r;
        }

        public void RecalcDriveTotalFoldersSize()
        {
            // recalculate total folders size
            Drive_TotalFoldersSize = 0;
            for (var i = 0; i < 9; i++)
            {
                // recalculate folder items and size per folder
                var FolderItems = Drive_FolderInfos[i].allTitles.Count;
                Drive_FolderInfos[i].FolderSize = 0;
                if (FolderItems > 0)
                {
                    foreach (var item in Drive_FolderInfos[i].allTitles)
                    {
                        Drive_FolderInfos[i].FolderSize += item.Size;
                    };
                }

                Drive_FolderInfos[i].FriendlyFolderSize = GetFriendlySize(Drive_FolderInfos[i].FolderSize);

                // accumulate to all folders
                Drive_TotalFoldersSize += Drive_FolderInfos[i].FolderSize;

                // copy information into XML structure (assuming it's the same)
                XML_FolderInfos[i].FolderSize = Drive_FolderInfos[i].FolderSize;
                XML_FolderInfos[i].FriendlyFolderSize = Drive_FolderInfos[i].FriendlyFolderSize;
            }

            Drive_FriendlyTotalFoldersSize = GetFriendlySize(Drive_TotalFoldersSize);
        }

        public string GetPlayingTime(UInt64 size)
        {
            // We have 16-Bit, 32kHZ, Mono -> 64k Bytes per second
            int seconds = Convert.ToInt32(size / 64000);
            var playtime = new TimeSpan(0, 0, seconds);

            if (playtime.Days > 0) return string.Format("{0:D}d:{1:D2}h:{2:D2}m:{3:D2}s", playtime.Days, playtime.Hours, playtime.Minutes, playtime.Seconds);
            if (playtime.Hours > 0) return string.Format("{0:D}h:{1:D2}m:{2:D2}s", playtime.Hours, playtime.Minutes, playtime.Seconds);
            return string.Format("{0:D}m:{1:D2}s", playtime.Minutes, playtime.Seconds);
        }

    }
}
