using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mpBackup
{
    public static class ContentTypes
    {
        public static string GOOGLE_DRIVE_FOLDER = "application/vnd.google-apps.folder";
        public static string ARCHIVE_ZIP = "application/zip";
        public static string FILE_TEXT = "text/plain";

        public static string getMimetypeForExtension(string ext)
        {
            switch (ext)
            {
                case "zip":
                    return ARCHIVE_ZIP;
                case "txt":
                    return FILE_TEXT;
                default:
                    return "";
            }
        }
    }
}
