using System;
using System.Data.SQLite;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using STOT.Config;
using STOT.Utils;

namespace STOT.UI
{
    public partial class UsersManagerDialog : Form
    {
        private DatabaseManager dbManager;
        private int currentUserId;
        private DataGridView usersTable;

        public event Action Saved;

        public UsersManagerDialog(DatabaseManager dbManager, int currentUserId, Form parent = null)
        {
            this.dbManager = dbManager;
            this.currentUserId = currentUserId;
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.Text = "إدارة المستخدمين";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.MinimumSize = new Size(1000, 500);

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 50,
                Margin = new Padding(0, 0, 0, 20)
            };

            var addBtn = new Button
            {
                Text = "➕ إضافة مستخدم جديد",
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = FontHelper.GetArabicFont(13F, FontStyle.Bold),
                Size = new Size(300, 40),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(3),
                Cursor = Cursors.Hand,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleCenter
            };
            addBtn.FlatAppearance.BorderSize = 0;
            addBtn.Click += AddUser_Click;

            toolbar.Controls.Add(addBtn);

            usersTable = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RightToLeft = RightToLeft.Yes,
                Font = FontHelper.GetArabicFont(12F),
                RowTemplate = { Height = 55 }
            };
            usersTable.Columns.Add("Username", "اسم المستخدم");
            usersTable.Columns.Add("FullName", "الاسم الكامل");
            usersTable.Columns.Add("Role", "الصلاحية");
            usersTable.Columns.Add("IsActive", "الحالة");
            usersTable.Columns.Add("CreatedAt", "تاريخ الإنشاء");
            usersTable.Columns.Add("Actions", "الإجراءات");
            
            usersTable.Columns[0].Width = 160;
            usersTable.Columns[1].Width = 160;
            usersTable.Columns[2].Width = 110;
            usersTable.Columns[3].Width = 90;
            usersTable.Columns[4].Width = 180;
            usersTable.Columns[5].Width = 480; // زيادة عرض عمود الإجراءات لـ 4 أزرار
            
            // تطبيق المحاذاة الوسطى على جميع الأعمدة
            foreach (DataGridViewColumn col in usersTable.Columns)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.DefaultCellStyle.Font = FontHelper.GetArabicFont(12F);
                col.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
            }
            
            usersTable.ColumnHeadersDefaultCellStyle.Font = FontHelper.GetArabicFont(14F, FontStyle.Bold);
            usersTable.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            usersTable.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 58, 95);
            usersTable.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            usersTable.ColumnHeadersHeight = 60;
            
            usersTable.DefaultCellStyle.Font = FontHelper.GetArabicFont(12F);
            usersTable.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            usersTable.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
            
            usersTable.CellPainting += UsersTable_CellPainting;
            usersTable.CellClick += UsersTable_CellClick;

            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 50
            };

            var closeBtn = new Button
            {
                Text = "❌ إغلاق",
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                Font = FontHelper.GetArabicFont(13F, FontStyle.Bold),
                Size = new Size(120, 45),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5),
                Cursor = Cursors.Hand,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleCenter
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.Click += (s, e) => this.Close();

            buttonsPanel.Controls.Add(closeBtn);

            mainPanel.Controls.Add(usersTable);
            mainPanel.Controls.Add(toolbar);
            mainPanel.Controls.Add(buttonsPanel);
            this.Controls.Add(mainPanel);
        }

        private void LoadUsers()
        {
            usersTable.Rows.Clear();
            var conn = dbManager.GetLocalConnection();

            using var cmd = new SQLiteCommand("SELECT * FROM users ORDER BY created_at DESC", conn);
            using var reader = cmd.ExecuteReader();

            var userIds = new System.Collections.Generic.List<int>();

            while (reader.Read())
            {
                var userId = Convert.ToInt32(reader["id"]);
                userIds.Add(userId);

                var role = reader["role"].ToString() == "admin" ? "مدير" : "مستخدم";
                var status = Convert.ToBoolean(reader["is_active"]) ? "نشط" : "معطل";

                var row = usersTable.Rows.Add(
                    reader["username"],
                    reader["full_name"],
                    role,
                    status,
                    reader["created_at"],
                    "" // Actions column - empty, we'll add buttons via CellPainting
                );

                if (Convert.ToBoolean(reader["is_active"]))
                    usersTable.Rows[row].Cells[3].Style.ForeColor = Color.Black;
                else
                    usersTable.Rows[row].Cells[3].Style.ForeColor = Color.Red;

                // حفظ معرف المستخدم وحالته في Tag
                usersTable.Rows[row].Tag = new System.Tuple<int, bool>(userId, Convert.ToBoolean(reader["is_active"]));
            }
            
            // إجبار إعادة رسم الجدول بالكامل
            usersTable.Invalidate();
            usersTable.Refresh();
            usersTable.Update();
            
            // إجبار إعادة رسم عمود الإجراءات
            for (int i = 0; i < usersTable.Rows.Count; i++)
            {
                usersTable.InvalidateCell(5, i);
            }
        }

        private void UsersTable_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // رسم الهيدر
            if (e.RowIndex < 0)
            {
                return; // اترك الرسم الافتراضي للهيدر
            }

            // رسم الخلايا العادية (غير عمود الإجراءات)
            if (e.ColumnIndex != 5) 
            {
                return; // اترك الرسم الافتراضي
            }

            System.Diagnostics.Debug.WriteLine($"CellPainting: Row={e.RowIndex}, Col={e.ColumnIndex}");

            var row = usersTable.Rows[e.RowIndex];
            var tag = row.Tag as System.Tuple<int, bool>;
            
            // إذا لم يكن هناك Tag، ارسم الخلية فارغة
            if (tag == null) 
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                e.Handled = true;
                return;
            }
            
            // إذا كان المستخدم الحالي، لا تعرض أزرار
            if (tag.Item1 == currentUserId)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                using (var brush = new SolidBrush(e.CellStyle.ForeColor))
                {
                    var textRect = e.CellBounds;
                    TextRenderer.DrawText(e.Graphics, "المستخدم الحالي", FontHelper.GetArabicFont(10F, FontStyle.Bold),
                        textRect, Color.Gray, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);
                }
                e.Handled = true;
                return;
            }

            var userId = tag.Item1;
            var isActive = tag.Item2;

            // رسم خلفية وحدود الخلية
            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground);
            
            var cellBounds = e.CellBounds;
            var buttonWidth = 85;
            var toggleWidth = 100;
            var suspendWidth = 100;
            var buttonHeight = 36;
            var spacing = 8;
            
            // حساب العرض الكلي للأزرار (4 أزرار)
            var totalWidth = buttonWidth + spacing + toggleWidth + spacing + suspendWidth + spacing + buttonWidth;
            
            // توسيط الأزرار أفقياً في الخلية
            var startX = cellBounds.Left + (cellBounds.Width - totalWidth) / 2;
            
            // توسيط الأزرار عمودياً في الخلية
            var startY = cellBounds.Top + (cellBounds.Height - buttonHeight) / 2;
            
            // التأكد من أن الأزرار ضمن حدود الخلية
            if (startX < cellBounds.Left + 5) 
                startX = cellBounds.Left + 5;
            
            if (startX + totalWidth > cellBounds.Right - 5)
                startX = cellBounds.Right - totalWidth - 5;
            
            if (startY < cellBounds.Top + 4) 
                startY = cellBounds.Top + 4;
            
            if (startY + buttonHeight > cellBounds.Bottom - 4) 
                startY = cellBounds.Bottom - buttonHeight - 4;

            // 1. زر التعديل (الأيمن)
            var editRect = new Rectangle(startX, startY, buttonWidth, buttonHeight);
            using (var brush = new SolidBrush(Color.FromArgb(44, 82, 130)))
            {
                e.Graphics.FillRectangle(brush, editRect);
            }
            using (var pen = new Pen(Color.FromArgb(30, 58, 95), 2))
            {
                e.Graphics.DrawRectangle(pen, editRect);
            }
            TextRenderer.DrawText(e.Graphics, "✏️ تعديل", FontHelper.GetArabicFont(10F, FontStyle.Bold),
                editRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);

            // 2. زر التفعيل/التعطيل
            startX += buttonWidth + spacing;
            var toggleRect = new Rectangle(startX, startY, toggleWidth, buttonHeight);
            var toggleColor = isActive ? Color.FromArgb(39, 174, 96) : Color.FromArgb(231, 76, 60);
            using (var brush = new SolidBrush(toggleColor))
            {
                e.Graphics.FillRectangle(brush, toggleRect);
            }
            using (var pen = new Pen(Color.FromArgb(toggleColor.R - 30, toggleColor.G - 30, toggleColor.B - 30), 2))
            {
                e.Graphics.DrawRectangle(pen, toggleRect);
            }
            var toggleText = isActive ? "✅ تفعيل" : "🚫 تعطيل";
            TextRenderer.DrawText(e.Graphics, toggleText, FontHelper.GetArabicFont(10F, FontStyle.Bold),
                toggleRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);

            // 3. زر التعليق
            startX += toggleWidth + spacing;
            var suspendRect = new Rectangle(startX, startY, suspendWidth, buttonHeight);
            using (var brush = new SolidBrush(Color.FromArgb(243, 156, 18)))
            {
                e.Graphics.FillRectangle(brush, suspendRect);
            }
            using (var pen = new Pen(Color.FromArgb(213, 126, 0), 2))
            {
                e.Graphics.DrawRectangle(pen, suspendRect);
            }
            TextRenderer.DrawText(e.Graphics, "⏸️ تعليق", FontHelper.GetArabicFont(10F, FontStyle.Bold),
                suspendRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);

            // 4. زر الحذف (الأيسر)
            startX += suspendWidth + spacing;
            var deleteRect = new Rectangle(startX, startY, buttonWidth, buttonHeight);
            using (var brush = new SolidBrush(Color.FromArgb(192, 57, 43)))
            {
                e.Graphics.FillRectangle(brush, deleteRect);
            }
            using (var pen = new Pen(Color.FromArgb(169, 50, 38), 2))
            {
                e.Graphics.DrawRectangle(pen, deleteRect);
            }
            TextRenderer.DrawText(e.Graphics, "🗑️ حذف", FontHelper.GetArabicFont(10F, FontStyle.Bold),
                deleteRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);

            e.Handled = true;
        }

        private void UsersTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 5) return;

            var row = usersTable.Rows[e.RowIndex];
            var tag = row.Tag as System.Tuple<int, bool>;
            
            if (tag == null || tag.Item1 == currentUserId) return;

            var userId = tag.Item1;
            var isActive = tag.Item2;

            var cellBounds = usersTable.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            var buttonWidth = 85;
            var toggleWidth = 100;
            var suspendWidth = 100;
            var spacing = 8;
            var totalWidth = buttonWidth + spacing + toggleWidth + spacing + suspendWidth + spacing + buttonWidth;
            
            // توسيط الأزرار أفقياً (نفس الحساب في CellPainting)
            var startX = cellBounds.Left + (cellBounds.Width - totalWidth) / 2;
            if (startX < cellBounds.Left + 5) startX = cellBounds.Left + 5;
            if (startX + totalWidth > cellBounds.Right - 5) startX = cellBounds.Right - totalWidth - 5;

            var mousePos = usersTable.PointToClient(Control.MousePosition);
            var relativeX = mousePos.X - cellBounds.X;
            var relativeStartX = startX - cellBounds.X;

            // التحقق من أي زر تم النقر عليه (من اليمين لليسار: تعديل، تفعيل، تعليق، حذف)
            if (relativeX >= relativeStartX && relativeX <= relativeStartX + buttonWidth)
            {
                // 1. زر التعديل
                var form = new UserFormDialog(dbManager, userId, this);
                form.Saved += () => LoadUsers();
                form.ShowDialog();
            }
            else if (relativeX >= relativeStartX + buttonWidth + spacing && relativeX <= relativeStartX + buttonWidth + spacing + toggleWidth)
            {
                // 2. زر التفعيل/التعطيل
                ToggleUser_Click(userId, isActive);
            }
            else if (relativeX >= relativeStartX + buttonWidth + spacing + toggleWidth + spacing && relativeX <= relativeStartX + buttonWidth + spacing + toggleWidth + spacing + suspendWidth)
            {
                // 3. زر التعليق
                SuspendUser_Click(userId);
            }
            else if (relativeX >= relativeStartX + buttonWidth + spacing + toggleWidth + spacing + suspendWidth + spacing && relativeX <= relativeStartX + totalWidth)
            {
                // 4. زر الحذف
                DeleteUser_Click(userId);
            }
        }
        
        private void DeleteUser_Click(int userId)
        {
            var result = MessageBox.Show("هل أنت متأكد من حذف هذا المستخدم؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            if (result == DialogResult.Yes)
            {
                var conn = dbManager.GetLocalConnection();

                using var checkCmd = new SQLiteCommand("SELECT COUNT(*) FROM transactions WHERE created_by = @id", conn);
                checkCmd.Parameters.AddWithValue("@id", userId);
                var count = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (count > 0)
                {
                    using var updateCmd = new SQLiteCommand("UPDATE users SET is_active = 0 WHERE id = @id", conn);
                    updateCmd.Parameters.AddWithValue("@id", userId);
                    updateCmd.ExecuteNonQuery();
                    MessageBox.Show("تم تعطيل المستخدم (لا يمكن حذفه لوجود معاملات مرتبطة)", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                }
                else
                {
                    using var deleteCmd = new SQLiteCommand("DELETE FROM users WHERE id = @id", conn);
                    deleteCmd.Parameters.AddWithValue("@id", userId);
                    deleteCmd.ExecuteNonQuery();
                    MessageBox.Show("تم حذف المستخدم بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                }

                LoadUsers();
            }
        }

        private void AddUser_Click(object sender, EventArgs e)
        {
            var form = new UserFormDialog(dbManager, null, this);
            form.Saved += () => LoadUsers();
            form.ShowDialog();
        }

        private void ToggleUser_Click(int userId, bool isActive)
        {
            var conn = dbManager.GetLocalConnection();
            using var cmd = new SQLiteCommand("UPDATE users SET is_active = @active WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@active", isActive ? 0 : 1);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.ExecuteNonQuery();

            var statusText = isActive ? "تم تعطيل المستخدم بنجاح" : "تم تفعيل المستخدم بنجاح";
            MessageBox.Show(statusText, "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

            LoadUsers();
        }

        private void SuspendUser_Click(int userId)
        {
            var result = MessageBox.Show("هل أنت متأكد من تعليق هذا المستخدم مؤقتاً؟\n\nالمستخدم المعلق لن يتمكن من تسجيل الدخول حتى يتم إلغاء التعليق.", "تأكيد التعليق", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            if (result == DialogResult.Yes)
            {
                var conn = dbManager.GetLocalConnection();
                
                // تعليق المستخدم (تعطيله مؤقتاً)
                using var cmd = new SQLiteCommand("UPDATE users SET is_active = 0 WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();

                MessageBox.Show("تم تعليق المستخدم مؤقتاً بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

                LoadUsers();
            }
        }

    }

    public partial class UserFormDialog : Form
    {
        private DatabaseManager dbManager;
        private int? userId;
        private TextBox usernameInput;
        private TextBox fullNameInput;
        private TextBox passwordInput;
        private ComboBox roleCombo;

        public event Action Saved;

        public UserFormDialog(DatabaseManager dbManager, int? userId = null, Form parent = null)
        {
            this.dbManager = dbManager;
            this.userId = userId;
            InitializeComponent();
            if (userId.HasValue)
                LoadUser();
        }

        private void InitializeComponent()
        {
            this.Text = userId.HasValue ? "تعديل مستخدم" : "إضافة مستخدم جديد";
            this.Size = new Size(550, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(30)
            };

            usernameInput = new TextBox 
            { 
                Font = FontHelper.GetArabicFont(12F), 
                Height = 40, 
                Dock = DockStyle.Fill,
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(10, 5, 10, 5)
            };
            if (userId.HasValue) usernameInput.ReadOnly = true;

            fullNameInput = new TextBox 
            { 
                Font = FontHelper.GetArabicFont(12F), 
                Height = 40, 
                Dock = DockStyle.Fill,
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(10, 5, 10, 5)
            };
            passwordInput = new TextBox 
            { 
                Font = FontHelper.GetArabicFont(12F), 
                Height = 40, 
                Dock = DockStyle.Fill, 
                UseSystemPasswordChar = true,
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(10, 5, 10, 5)
            };
            if (userId.HasValue)
                passwordInput.PlaceholderText = "اتركه فارغاً إذا لم ترد تغييره";

            roleCombo = new ComboBox 
            { 
                Font = FontHelper.GetArabicFont(12F), 
                Height = 40, 
                Dock = DockStyle.Fill, 
                DropDownStyle = ComboBoxStyle.DropDownList,
                RightToLeft = RightToLeft.Yes
            };
            roleCombo.Items.Add(new ComboBoxItem("مستخدم عادي", "user"));
            roleCombo.Items.Add(new ComboBoxItem("مدير", "admin"));

            var usernameLabel = new Label 
            { 
                Text = "اسم المستخدم:", 
                Font = FontHelper.GetArabicFont(12F, FontStyle.Bold), 
                AutoSize = true,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleRight
            };
            var fullNameLabel = new Label 
            { 
                Text = "الاسم الكامل:", 
                Font = FontHelper.GetArabicFont(12F, FontStyle.Bold), 
                AutoSize = true,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleRight
            };
            var passwordLabel = new Label 
            { 
                Text = userId.HasValue ? "كلمة المرور:" : "كلمة المرور *:", 
                Font = FontHelper.GetArabicFont(12F, FontStyle.Bold), 
                AutoSize = true,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleRight
            };
            var roleLabel = new Label 
            { 
                Text = "الصلاحية:", 
                Font = FontHelper.GetArabicFont(12F, FontStyle.Bold), 
                AutoSize = true,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleRight
            };

            layout.Controls.Add(usernameLabel, 0, 0);
            layout.Controls.Add(usernameInput, 1, 0);
            layout.Controls.Add(fullNameLabel, 0, 1);
            layout.Controls.Add(fullNameInput, 1, 1);
            layout.Controls.Add(passwordLabel, 0, 2);
            layout.Controls.Add(passwordInput, 1, 2);
            layout.Controls.Add(roleLabel, 0, 3);
            layout.Controls.Add(roleCombo, 1, 3);

            var buttonsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill
            };

            var saveBtn = new Button
            {
                Text = "💾 حفظ",
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = FontHelper.GetArabicFont(12F, FontStyle.Bold),
                Size = new Size(120, 40),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(3),
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleCenter
            };
            saveBtn.FlatAppearance.BorderSize = 0;
            saveBtn.Click += SaveBtn_Click;

            var cancelBtn = new Button
            {
                Text = "❌ إلغاء",
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                Font = FontHelper.GetArabicFont(12F, FontStyle.Bold),
                Size = new Size(120, 40),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(3),
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleCenter
            };
            cancelBtn.FlatAppearance.BorderSize = 0;
            cancelBtn.Click += (s, e) => this.Close();

            buttonsPanel.Controls.Add(cancelBtn);
            buttonsPanel.Controls.Add(saveBtn);

            layout.Controls.Add(buttonsPanel, 0, 4);
            layout.SetColumnSpan(buttonsPanel, 2);

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

            this.Controls.Add(layout);
        }

        private void LoadUser()
        {
            var conn = dbManager.GetLocalConnection();
            using var cmd = new SQLiteCommand("SELECT * FROM users WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", userId.Value);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                usernameInput.Text = reader["username"].ToString();
                fullNameInput.Text = reader["full_name"].ToString();

                for (int i = 0; i < roleCombo.Items.Count; i++)
                {
                    if (roleCombo.Items[i] is ComboBoxItem item && item.Value.ToString() == reader["role"].ToString())
                    {
                        roleCombo.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(usernameInput.Text) || string.IsNullOrWhiteSpace(fullNameInput.Text))
            {
                MessageBox.Show("يرجى إدخال جميع الحقول المطلوبة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!userId.HasValue && string.IsNullOrEmpty(passwordInput.Text))
            {
                MessageBox.Show("يرجى إدخال كلمة المرور", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var conn = dbManager.GetLocalConnection();
            try
            {
                if (userId.HasValue)
                {
                    if (!string.IsNullOrEmpty(passwordInput.Text))
                    {
                        var md5 = MD5.Create();
                        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(passwordInput.Text));
                        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                        using var cmd = new SQLiteCommand("UPDATE users SET full_name = @name, role = @role, password = @pass WHERE id = @id", conn);
                        cmd.Parameters.AddWithValue("@name", fullNameInput.Text);
                        cmd.Parameters.AddWithValue("@role", (roleCombo.SelectedItem as ComboBoxItem).Value);
                        cmd.Parameters.AddWithValue("@pass", hashString);
                        cmd.Parameters.AddWithValue("@id", userId.Value);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        using var cmd = new SQLiteCommand("UPDATE users SET full_name = @name, role = @role WHERE id = @id", conn);
                        cmd.Parameters.AddWithValue("@name", fullNameInput.Text);
                        cmd.Parameters.AddWithValue("@role", (roleCombo.SelectedItem as ComboBoxItem).Value);
                        cmd.Parameters.AddWithValue("@id", userId.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    using var checkCmd = new SQLiteCommand("SELECT id FROM users WHERE username = @username", conn);
                    checkCmd.Parameters.AddWithValue("@username", usernameInput.Text);
                    if (checkCmd.ExecuteScalar() != null)
                    {
                        MessageBox.Show("اسم المستخدم موجود بالفعل", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var md5 = MD5.Create();
                    var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(passwordInput.Text));
                    var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                    using var cmd = new SQLiteCommand("INSERT INTO users (username, password, full_name, role) VALUES (@username, @pass, @name, @role)", conn);
                    cmd.Parameters.AddWithValue("@username", usernameInput.Text);
                    cmd.Parameters.AddWithValue("@pass", hashString);
                    cmd.Parameters.AddWithValue("@name", fullNameInput.Text);
                    cmd.Parameters.AddWithValue("@role", (roleCombo.SelectedItem as ComboBoxItem).Value);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("تم الحفظ بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Saved?.Invoke();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

