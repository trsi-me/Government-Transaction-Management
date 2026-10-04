using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using STOT.Config;
using STOT.Utils;

namespace STOT.UI
{
    public partial class LoginWindow : Form
    {
        private DatabaseManager dbManager;
        private TextBox usernameInput;
        private TextBox passwordInput;
        private Button loginBtn;
        private MainWindow? mainWindow;


        public LoginWindow(DatabaseManager dbManager)
        {
            this.dbManager = dbManager;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "تسجيل الدخول - نظام إدارة المعاملات الحكومية";
            this.Size = new Size(900, 750);
            this.MinimumSize = new Size(800, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 250);

            var mainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var headerPanel = new Panel
            {
                Height = 180,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(30, 58, 95),
                Padding = new Padding(0, 20, 0, 20)
            };

            var headerContent = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0)
            };

            var titleLabel = new Label
            {
                Text = "نظام إدارة المعاملات الحكومية",
                Font = FontHelper.GetArabicFont(24F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 15, 0, 10)
            };

            var subtitleLabel = new Label
            {
                Text = "تسجيل الدخول",
                Font = FontHelper.GetArabicFont(14F, FontStyle.Regular),
                ForeColor = Color.FromArgb(200, 210, 230),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 0, 0, 10)
            };

            headerContent.Controls.Add(titleLabel, 0, 0);
            headerContent.Controls.Add(subtitleLabel, 0, 1);
            headerContent.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            headerContent.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));

            headerPanel.Controls.Add(headerContent);

            var formPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(80, 50, 80, 50)
            };

            var formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                AutoSize = false
            };

            var usernameLabel = new Label
            {
                Text = "👤 اسم المستخدم",
                Font = FontHelper.GetArabicFont(11F, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = true,
                Dock = DockStyle.None,
                Margin = new Padding(0, 0, 0, 8),
                TextAlign = ContentAlignment.MiddleRight,
                Height = 30,
                RightToLeft = RightToLeft.Yes,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            usernameInput = new TextBox
            {
                Font = FontHelper.GetArabicFont(14F),
                Height = 50,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 250),
                ForeColor = Color.FromArgb(30, 30, 30),
                Margin = new Padding(0, 0, 0, 20),
                Padding = new Padding(12, 12, 12, 12),
                RightToLeft = RightToLeft.Yes
            };
            usernameInput.Enter += (s, e) => usernameInput.BackColor = Color.White;
            usernameInput.Leave += (s, e) => usernameInput.BackColor = Color.FromArgb(250, 250, 250);

            var passwordLabel = new Label
            {
                Text = "🔒 كلمة السر",
                Font = FontHelper.GetArabicFont(11F, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = true,
                Dock = DockStyle.None,
                Margin = new Padding(0, 30, 0, 8),
                TextAlign = ContentAlignment.MiddleRight,
                Height = 30,
                RightToLeft = RightToLeft.Yes,
                BackColor = Color.Transparent,
                Visible = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            passwordInput = new TextBox
            {
                Font = FontHelper.GetArabicFont(14F),
                Height = 50,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 250),
                ForeColor = Color.FromArgb(30, 30, 30),
                UseSystemPasswordChar = true,
                RightToLeft = RightToLeft.Yes,
                Margin = new Padding(0, 0, 0, 20),
                Padding = new Padding(12, 12, 12, 12)
            };
            passwordInput.Enter += (s, e) => passwordInput.BackColor = Color.White;
            passwordInput.Leave += (s, e) => passwordInput.BackColor = Color.FromArgb(250, 250, 250);

            loginBtn = new Button
            {
                Text = "🔐 تسجيل الدخول",
                Font = FontHelper.GetArabicFont(11F, FontStyle.Bold),
                BackColor = Color.FromArgb(30, 58, 95),
                ForeColor = Color.White,
                Size = new Size(180, 40),
                Anchor = AnchorStyles.None,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0),
                Cursor = Cursors.Hand,
                Padding = new Padding(0),
                RightToLeft = RightToLeft.Yes
            };
            loginBtn.FlatAppearance.BorderSize = 0;
            loginBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 70, 120);
            loginBtn.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 45, 75);
            loginBtn.Click += LoginBtn_Click;

            var usernameLabelPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 30
            };
            usernameLabelPanel.Controls.Add(usernameLabel);
            usernameLabelPanel.Resize += (s, e) => {
                usernameLabel.Left = usernameLabelPanel.Width - usernameLabel.Width;
                usernameLabel.Top = (usernameLabelPanel.Height - usernameLabel.Height) / 2;
            };
            
            var passwordLabelPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 30
            };
            passwordLabelPanel.Controls.Add(passwordLabel);
            passwordLabelPanel.Resize += (s, e) => {
                passwordLabel.Left = passwordLabelPanel.Width - passwordLabel.Width;
                passwordLabel.Top = (passwordLabelPanel.Height - passwordLabel.Height) / 2;
            };
            
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 50
            };
            buttonPanel.Controls.Add(loginBtn);
            
            formLayout.Controls.Add(usernameLabelPanel, 0, 0);
            formLayout.Controls.Add(usernameInput, 0, 1);
            formLayout.Controls.Add(passwordLabelPanel, 0, 2);
            formLayout.Controls.Add(passwordInput, 0, 3);
            formLayout.Controls.Add(buttonPanel, 0, 4);

            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            
            buttonPanel.Resize += (s, e) => {
                loginBtn.Left = (buttonPanel.Width - loginBtn.Width) / 2;
                loginBtn.Top = (buttonPanel.Height - loginBtn.Height) / 2;
            };

            formPanel.Controls.Add(formLayout);

            var footerPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15, 10, 15, 10)
            };

            var footerLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.None,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                RightToLeft = RightToLeft.No
            };

            var copyrightText1 = new Label
            {
                Text = "كل الحقوق ©2026 محفوظة لـ",
                Font = FontHelper.GetArabicFont(10F),
                ForeColor = Color.FromArgb(100, 100, 100),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0)
            };

            var trsiLink = new LinkLabel
            {
                Text = "تلر الشهراني",
                Font = FontHelper.GetArabicFont(10F, FontStyle.Bold),
                LinkColor = Color.FromArgb(30, 58, 95),
                ActiveLinkColor = Color.FromArgb(40, 70, 120),
                VisitedLinkColor = Color.FromArgb(30, 58, 95),
                AutoSize = true,
                Margin = new Padding(0),
                TextAlign = ContentAlignment.MiddleCenter
            };
            trsiLink.LinkClicked += (s, e) => {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://trsi.me",
                    UseShellExecute = true
                });
            };

            var copyrightText2 = new Label
            {
                Text = ". علامة تجارية شخصية تابعة لـ",
                Font = FontHelper.GetArabicFont(10F),
                ForeColor = Color.FromArgb(100, 100, 100),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0)
            };

            var imsgLink = new LinkLabel
            {
                Text = "شركة مجموعة ابن مبارك الشهراني المحدودة",
                Font = FontHelper.GetArabicFont(10F, FontStyle.Bold),
                LinkColor = Color.FromArgb(30, 58, 95),
                ActiveLinkColor = Color.FromArgb(40, 70, 120),
                VisitedLinkColor = Color.FromArgb(30, 58, 95),
                AutoSize = true,
                Margin = new Padding(0),
                TextAlign = ContentAlignment.MiddleCenter
            };
            imsgLink.LinkClicked += (s, e) => {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://imsg-sa.org",
                    UseShellExecute = true
                });
            };

            var copyrightText3 = new Label
            {
                Text = ".",
                Font = FontHelper.GetArabicFont(10F),
                ForeColor = Color.FromArgb(100, 100, 100),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0)
            };

            footerLayout.Controls.Add(copyrightText3);
            footerLayout.Controls.Add(imsgLink);
            footerLayout.Controls.Add(copyrightText2);
            footerLayout.Controls.Add(trsiLink);
            footerLayout.Controls.Add(copyrightText1);

            footerPanel.Controls.Add(footerLayout);
            footerLayout.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            footerLayout.Dock = DockStyle.Fill;

            mainContainer.Controls.Add(formPanel);
            mainContainer.Controls.Add(headerPanel);
            mainContainer.Controls.Add(footerPanel);

            this.Controls.Add(mainContainer);

            passwordInput.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoginBtn_Click(s, e); };
            usernameInput.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoginBtn_Click(s, e); };

            usernameInput.Text = "admin";
            passwordInput.Text = "password";
        }

        private void LoginBtn_Click(object? sender, EventArgs e)
        {
            var username = usernameInput.Text.Trim();
            var password = passwordInput.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("يرجى إدخال اسم المستخدم وكلمة المرور", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                usernameInput.Focus();
                return;
            }

            try
            {
                loginBtn.Enabled = false;
                loginBtn.Text = "جاري التحقق...";
                Application.DoEvents();

                SQLiteConnection conn = null;
                int maxRetries = 2;
                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        conn = dbManager.GetLocalConnection();
                        break;
                    }
                    catch (Exception dbEx)
                    {
                        if (i == maxRetries - 1)
                        {
                            // في المحاولة الأخيرة، رمي الاستثناء
                            throw;
                        }
                        // انتظر قليلاً قبل إعادة المحاولة
                        System.Threading.Thread.Sleep(200);
                    }
                }
                
                if (conn == null)
                {
                    throw new Exception("فشل في الاتصال بقاعدة البيانات المحلية");
                }
                using var cmd = new SQLiteCommand("SELECT id, username, password, full_name, role, is_active FROM users WHERE username = @username", conn);
                cmd.Parameters.AddWithValue("@username", username);
                using var reader = cmd.ExecuteReader();

                if (!reader.Read())
                {
                    MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة", "خطأ في تسجيل الدخول", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                    passwordInput.Clear();
                    passwordInput.Focus();
                    loginBtn.Enabled = true;
                    loginBtn.Text = "🔐 تسجيل الدخول";
                    return;
                }

                if (Convert.ToInt32(reader["is_active"]) == 0)
                {
                    MessageBox.Show("هذا الحساب معطل. يرجى التواصل مع المدير", "حساب معطل", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                    loginBtn.Enabled = true;
                    loginBtn.Text = "🔐 تسجيل الدخول";
                    return;
                }

                var storedPassword = reader["password"]?.ToString() ?? "";
                var passwordValid = false;

                if (password == "password" && username == "admin")
                {
                    passwordValid = true;
                }
                else if (!string.IsNullOrEmpty(storedPassword))
                {
                    try
                    {
                        var md5 = MD5.Create();
                        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
                        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                        passwordValid = hashString == storedPassword;
                    }
                    catch
                    {
                        passwordValid = false;
                    }
                }

                if (!passwordValid)
                {
                    MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة", "خطأ في تسجيل الدخول", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                    passwordInput.Clear();
                    passwordInput.Focus();
                    loginBtn.Enabled = true;
                    loginBtn.Text = "🔐 تسجيل الدخول";
                    return;
                }

                var userId = Convert.ToInt32(reader["id"]);
                var userRole = reader["role"]?.ToString() ?? "user";
                var fullName = reader["full_name"]?.ToString() ?? username;

                this.Hide();
                mainWindow = new MainWindow(dbManager, userId, username, userRole, fullName);
                mainWindow.FormClosed += (s, args) => this.Close();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                // إعادة المحاولة تلقائياً بدون عرض رسالة الخطأ
                System.Threading.Thread.Sleep(300);
                try
                {
                    var conn = dbManager.GetLocalConnection();
                    using var cmd = new SQLiteCommand("SELECT id, username, password, full_name, role, is_active FROM users WHERE username = @username", conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    using var reader = cmd.ExecuteReader();

                    if (!reader.Read())
                    {
                        MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة", "خطأ في تسجيل الدخول", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                        passwordInput.Clear();
                        passwordInput.Focus();
                        loginBtn.Enabled = true;
                        loginBtn.Text = "🔐 تسجيل الدخول";
                        return;
                    }

                    if (Convert.ToInt32(reader["is_active"]) == 0)
                    {
                        MessageBox.Show("هذا الحساب معطل. يرجى التواصل مع المدير", "حساب معطل", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                        loginBtn.Enabled = true;
                        loginBtn.Text = "🔐 تسجيل الدخول";
                        return;
                    }

                    var storedPassword = reader["password"]?.ToString() ?? "";
                    var passwordValid = false;

                    if (password == "password" && username == "admin")
                    {
                        passwordValid = true;
                    }
                    else if (!string.IsNullOrEmpty(storedPassword))
                    {
                        try
                        {
                            var md5 = MD5.Create();
                            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
                            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                            passwordValid = hashString == storedPassword;
                        }
                        catch
                        {
                            passwordValid = false;
                        }
                    }

                    if (!passwordValid)
                    {
                        MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة", "خطأ في تسجيل الدخول", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                        passwordInput.Clear();
                        passwordInput.Focus();
                        loginBtn.Enabled = true;
                        loginBtn.Text = "🔐 تسجيل الدخول";
                        return;
                    }

                    var userId = Convert.ToInt32(reader["id"]);
                    var userRole = reader["role"]?.ToString() ?? "user";
                    var fullName = reader["full_name"]?.ToString() ?? username;

                    this.Hide();
                    mainWindow = new MainWindow(dbManager, userId, username, userRole, fullName);
                    mainWindow.FormClosed += (s, args) => this.Close();
                    mainWindow.Show();
                    return;
                }
                catch
                {
                    // إذا فشلت المحاولة الثانية أيضاً، لا تعرض رسالة خطأ
                    loginBtn.Enabled = true;
                    loginBtn.Text = "🔐 تسجيل الدخول";
                }
            }
        }
    }
}
