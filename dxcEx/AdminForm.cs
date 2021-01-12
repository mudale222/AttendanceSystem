using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dxcEx {
    public partial class AdminForm : Form {
        public AdminForm() {
            InitializeComponent();
        }

        private void AdminForm_Load(object sender, EventArgs e) {
            Image myimage = new Bitmap(Uti.folderUrl + "\\background.png");
            this.BackgroundImage = myimage;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            richTextBox1.BackColor = Color.Black;
            richTextBox1.ForeColor = Color.LimeGreen;
            FillUserCheckList();
        }

        private List<string> GetUsersNamesFromDatabase() {
            var names = new List<string>();
            var cmd = new SqlCommand();

            using (SqlConnection connection = new SqlConnection(Uti.CONSTRING)) {
                connection.Open();

                cmd.CommandText = "SELECT Username FROM Users;";
                cmd.Connection = connection;
                using (SqlDataReader oReader = cmd.ExecuteReader()) {
                    while (oReader.Read())
                        names.Add(oReader["Username"].ToString());
                }
            }
            return names;
        }

        private Dictionary<int, User> GetUsersWithHours(List<string> names) {
            var cmd = new SqlCommand();
            var userIdsNames = GetUsersIdByNames(names);
            var users = new Dictionary<int, User>();

            using (SqlConnection connection = new SqlConnection(Uti.CONSTRING)) {
                connection.Open();
                cmd.Connection = connection;
                cmd.Parameters.AddWithValue("@id", "");
                foreach (var idName in userIdsNames) {
                    cmd.Parameters["@id"].Value = idName.Key;
                    cmd.CommandText = "SELECT EnterTime,ExitTime FROM Attendance WHERE UserId=@id;";
                    using (SqlDataReader oReader = cmd.ExecuteReader()) {
                        while (oReader.Read()) {
                            if (!users.ContainsKey(idName.Key)) {
                                users.Add(idName.Key, new User());
                                users[idName.Key].name = idName.Value;
                            }
                            var enter = oReader["EnterTime"] == DBNull.Value ? null : (DateTime?)oReader["EnterTime"];
                            var exit = oReader["ExitTime"] == DBNull.Value ? null : (DateTime?)oReader["ExitTime"];
                            users[idName.Key].attendanceTimes.Add(new KeyValuePair<DateTime?, DateTime?>(enter, exit));
                        }
                    }
                }
            }
            return users;
        }

        private List<KeyValuePair<int, string>> GetUsersIdByNames(List<string> names) {
            var ids = new List<KeyValuePair<int, string>>();
            var cmd = new SqlCommand();
            using (SqlConnection connection = new SqlConnection(Uti.CONSTRING)) {
                connection.Open();
                cmd.Connection = connection;
                cmd.Parameters.AddWithValue("@userName", "");
                foreach (var name in names) {
                    cmd.Parameters["@userName"].Value = name;
                    cmd.CommandText = "SELECT TOP 1 * FROM Users WHERE Username=@userName";
                    using (SqlDataReader oReader = cmd.ExecuteReader()) {
                        if (oReader.Read())
                            ids.Add(new KeyValuePair<int, string>((int)oReader["Id"], name));
                    }
                }
            }
            return ids;
        }

        private void FillUserCheckList() {
            var users = GetUsersNamesFromDatabase();
            foreach (var user in users)
                checkedListBox1.Items.Add(user, false);
        }

        private void ExecuteClicked(object sender, EventArgs e) {
            if (!(checkedListBox1.CheckedItems.Count > 0))
                return;

            var users = GetUsersWithHours(checkedListBox1.CheckedItems.OfType<string>().ToList());
            var totalTimeForAllUsers = new TimeSpan();
            var result = "";
            foreach (var user in users.Values) {
                var totalAllSessionsTime = new TimeSpan();
                var missingDays = new List<DateTime>();
                result += "Id:" + user.id + "    Name:" + user.name  + Environment.NewLine;
                foreach (var sessionTime in user.attendanceTimes) {
                    if ((sessionTime.Key != null && sessionTime.Key >= fromDatePicker.Value && sessionTime.Key <= toDatePicker.Value) ||
                        (sessionTime.Value != null && sessionTime.Value >= fromDatePicker.Value && sessionTime.Value <= toDatePicker.Value)) {
                        if (sessionTime.Key == null || sessionTime.Value == null) {
                            missingDays.Add((DateTime)sessionTime.Key);
                            continue;
                        }
                        var endTime = sessionTime.Value < toDatePicker.Value ? sessionTime.Value : toDatePicker.Value;
                        var totalSessionTime = (DateTime)endTime - (DateTime)sessionTime.Key;

                        result += "From:" + sessionTime.Key + "    To:" + endTime + "    Total: " +
                                  Uti.FormatTime(totalSessionTime) + Environment.NewLine;
                        totalAllSessionsTime += totalSessionTime;
                    }
                }
                result += "Total time: " + Uti.FormatTime(totalAllSessionsTime) + "    Missing days: " + missingDays.Count + Environment.NewLine;
                totalTimeForAllUsers += totalAllSessionsTime;
                if (missingDays.Count > 0) {
                    result += "Missing dates: " + Environment.NewLine;
                    foreach (var day in missingDays)
                        result += day + Environment.NewLine;
                }
                result += Environment.NewLine + Environment.NewLine;
            }
            result += "Total time 4 ALL users COMBINED: " + Uti.FormatTime(totalTimeForAllUsers);
            richTextBox1.Text = result;
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void allUsersCheckBox_CheckedChanged(object sender, EventArgs e) {
            if (allUsersCheckBox.Checked)
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                    checkedListBox1.SetItemChecked(i, true);
            else
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                    checkedListBox1.SetItemChecked(i, false);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e) {

        }
    }
}
