using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dxcEx {
    public class User {
        public System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        public DateTime connectedTime = new DateTime();
        public int id  {get; set; }
        public string name { get; set; }
        public int attendanceId { get; set; }
        public List<KeyValuePair<DateTime?, DateTime?>> attendanceTimes = new List<KeyValuePair<DateTime?, DateTime?>>();
    }
}
