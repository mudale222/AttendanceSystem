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
        public int id = 0;
        public string name = "";
        public int attendanceId = 0;
    }
}
