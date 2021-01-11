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
    public partial class LoginForm : Form {
        const string MyConString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\mudale\\source\\repos\\dxcEx\\dxcEx\\MainDb.mdf;Integrated Security=True;";
        User user = new User();
        public LoginForm() {
            InitializeComponent();
        }

        private void LoginForm_Load(object sender, EventArgs e) {

        }
        private void StartTimer() {
            user.timer.Interval = 1000;
            user.timer.Tick += new EventHandler(t_Tick);
            user.timer.Enabled = true;
            user.connectedTime = DateTime.Now;
        }

        void t_Tick(object sender, EventArgs e) {
            var time_span = (DateTime.Now - user.connectedTime);
            var hh = time_span.Hours;
            var mm = time_span.Minutes;
            var ss = time_span.Seconds;

            TimerLabel.Text = string.Format("{0}:{1}:{2}", hh, mm, ss);
        }

        private void loginButtonClicked(object sender, EventArgs e) {
            var cmd = new SqlCommand();
            var userName = userNameTextBox.Text;
            var userHashSalt = new HashSalt();

            using (SqlConnection connection = new SqlConnection(MyConString)) {
                connection.Open();

                cmd.Parameters.AddWithValue("@userName", userName);
                cmd.CommandText = "SELECT TOP 1 * FROM Users WHERE Username=@userName";
                cmd.Connection = connection;
                using (SqlDataReader oReader = cmd.ExecuteReader()) {
                    if (oReader.Read()) {
                        userHashSalt = new HashSalt { Hash = oReader["Hash"].ToString(), Salt = oReader["Salt"].ToString() };
                        user.id = (int)oReader["Id"];
                    }
                    else {
                        MessageBox.Show("User or password invalid (testing: no user)");
                        return;
                    }
                }


                bool isPasswordMatched = HashSalt.VerifyPassword(passwordTextBox.Text, userHashSalt.Hash, userHashSalt.Salt);

                if (isPasswordMatched) {
                    MessageBox.Show("(testing: Password match!)");
                    StartTimer();
                    cmd.Parameters.AddWithValue("@userId", user.id);
                    cmd.Parameters.AddWithValue("@enterTime", user.connectedTime);
                    cmd.CommandText = "INSERT INTO Attendance(EnterTime,UserId) Values(@enterTime,@userId); SELECT SCOPE_IDENTITY();";
                    //var asdf  = cmd.ExecuteNonQuery();
                    user.attendanceId = int.Parse(cmd.ExecuteScalar().ToString());
                }
                else {
                    MessageBox.Show("User or password invalid (testing:Password invalid!)");
                    return;
                }
            }
            LogInLabel.Text = DateTime.Now.ToString();
            statusLabel.ForeColor = Color.LimeGreen;
            statusLabel.Text = "User Online!";
        }

        private void logoutButtonClicked(object sender, EventArgs e) {
            var cmd = new SqlCommand();
            if (user.id != 0 && (DateTime.Now - user.connectedTime).TotalHours <= 12)
                using (SqlConnection connection = new SqlConnection(MyConString)) {
                    connection.Open();

                    cmd.Parameters.AddWithValue("@exitTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("@id", user.attendanceId);
                    cmd.CommandText = "UPDATE Attendance SET ExitTime=@exitTime  WHERE Id=@id;";
                    cmd.Connection = connection;
                    cmd.ExecuteNonQuery();
                }
            else
                MessageBox.Show("Not login or more then 12 hours!");
            statusLabel.ForeColor = Color.Red;
            statusLabel.Text = "User Offine!";
            LogOutLabel.Text = DateTime.Now.ToString();
            user.timer.Enabled = false;
        }

        private void signupButtonClicked(object sender, EventArgs e) {
            var userName = userNameTextBox.Text;
            var password = passwordTextBox.Text;
            var cmd = new SqlCommand();
            if (userName.Length < 6 || password.Length < 6 /*use premade libary for this*/) {
                MessageBox.Show("Need at lease 1 number and  blablalba etc...");
                return;
            }

            HashSalt hashSalt = HashSalt.GenerateSaltedHash(64, password);

            using (SqlConnection connection = new SqlConnection(MyConString)) {
                connection.Open();

                cmd.Parameters.AddWithValue("@userName", userName);
                cmd.Parameters.AddWithValue("@hash", hashSalt.Hash);
                cmd.Parameters.AddWithValue("@salt", hashSalt.Salt);
                cmd.CommandText = "INSERT INTO Users(Username ,Hash, Salt) VALUES ( @userName, @hash, @salt)";
                cmd.Connection = connection;
                try {
                    cmd.ExecuteNonQuery();
                } catch (SqlException ex) {
                    if (ex.Number == 2601 || ex.Number == 2627) {
                        MessageBox.Show("User already exist!");
                        return;
                    }
                }
                MessageBox.Show("New user added!");
            }
        }

        private void userNameTextBoxChanged(object sender, EventArgs e) {

        }

        private void passwordTextBoxChanged(object sender, EventArgs e) {

        }

        private void label4_Click(object sender, EventArgs e) {
        }
    }
}
