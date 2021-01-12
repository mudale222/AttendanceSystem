using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dxcEx {
    public static class Uti {
        public static string folderUrl = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;

        public static string CONSTRING = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=" + folderUrl +"\\MainDb.mdf;Integrated Security=True;";
        public static string FormatTime(TimeSpan time) {
            var hh = time.Hours;
            var mm = time.Minutes;
            var ss = time.Seconds;
            return string.Format("{0}:{1}:{2}", hh, mm, ss);
        }
    }
}
