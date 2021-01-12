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
        User user = new User();
        public LoginForm() {
            InitializeComponent();
            this.Icon = new Icon(Uti.folderUrl +"\\icon.ico");
            Image myimage = new Bitmap(Uti.folderUrl + "\\background.png");
            this.BackgroundImage = myimage;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
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
            TimerLabel.Text = Uti.FormatTime(time_span);
        }

        private void loginButtonClicked(object sender, EventArgs e) {
            var cmd = new SqlCommand();
            var userName = userNameTextBox.Text;
            var userHashSalt = new HashSalt();

            using (SqlConnection connection = new SqlConnection(Uti.CONSTRING)) {
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
                        MessageBox.Show("User or password invalid!"/*no user*/);
                        return;
                    }
                }


                bool isPasswordMatched = HashSalt.VerifyPassword(passwordTextBox.Text, userHashSalt.Hash, userHashSalt.Salt);

                if (isPasswordMatched) {
                    MessageBox.Show("Login success!"/*password match*/);
                    StartTimer();
                    cmd.Parameters.AddWithValue("@userId", user.id);
                    cmd.Parameters.AddWithValue("@enterTime", user.connectedTime);
                    cmd.CommandText = "INSERT INTO Attendance(EnterTime,UserId) Values(@enterTime,@userId); SELECT SCOPE_IDENTITY();";
                    user.attendanceId = int.Parse(cmd.ExecuteScalar().ToString());
                }
                else {
                    MessageBox.Show("User or password invalid!"/*Password invalid!*/);
                    return;
                }
            }
            LogInLabel.Text = DateTime.Now.ToString();
            statusLabel.ForeColor = Color.LimeGreen;
            statusLabel.Text = "User Online!";
            if (userName == "adminadmin")
                using (var adminForm = new AdminForm())
                    adminForm.ShowDialog();
        }

        private void logoutButtonClicked(object sender, EventArgs e) {
            var isClosing = false;
            try {
                var x = ((FormClosingEventArgs)e);
                isClosing = true;
            }
            catch (Exception ex) { }

            var cmd = new SqlCommand();
            if (user.id != 0 && (DateTime.Now - user.connectedTime).TotalHours <= 12)
                using (SqlConnection connection = new SqlConnection(Uti.CONSTRING)) {
                    connection.Open();

                    cmd.Parameters.AddWithValue("@exitTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("@id", user.attendanceId);
                    cmd.CommandText = "UPDATE Attendance SET ExitTime=@exitTime  WHERE Id=@id;";
                    cmd.Connection = connection;
                    cmd.ExecuteNonQuery();
                    LogOutLabel.Text = DateTime.Now.ToString();
                    user.timer.Enabled = false;
                }
            else if (!isClosing)
                MessageBox.Show("Not login or more then 12 hours!");
            statusLabel.ForeColor = Color.Red;
            statusLabel.Text = "User Offline!";
            user = new User();
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

            using (SqlConnection connection = new SqlConnection(Uti.CONSTRING)) {
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

        private void label5_Click(object sender, EventArgs e) {

        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e) {
            logoutButtonClicked(sender,e);
        }
    }
}
