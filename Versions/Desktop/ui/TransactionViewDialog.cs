using System;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using STOT.Config;
using STOT.Utils;

namespace STOT.UI
{
    public partial class TransactionViewDialog : Form
    {
        private DatabaseManager dbManager;
        private int transactionId;
        private int userId;
        private string userRole;

        private DataGridView logsTable;
        private DataGridView attachmentsTable;
        private System.Collections.Generic.Dictionary<string, string> transactionData;

        public event Action Saved;

        public TransactionViewDialog(DatabaseManager dbManager, int transactionId, int userId, string userRole, Form parent = null)
        {
            this.dbManager = dbManager;
            this.transactionId = transactionId;
            this.userId = userId;
            this.userRole = userRole;
            InitializeComponent();
            LoadTransaction();
            LoadLogs();
            LoadAttachments();
        }

        private void InitializeComponent()
        {
            this.Text = $"تفاصيل المعاملة #{transactionId}";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.MinimumSize = new Size(1200, 700);

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            var scrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            var layout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(0),
                MinimumSize = new Size(1500, 0)
            };

            var toolbar = CreateToolbar();
            var detailsGroup = CreateDetailsGroup();
            var logsGroup = CreateLogsGroup();
            var attachmentsGroup = CreateAttachmentsGroup();

            layout.Controls.Add(toolbar);
            layout.Controls.Add(detailsGroup);
            layout.Controls.Add(logsGroup);
            layout.Controls.Add(attachmentsGroup);

            scrollPanel.Controls.Add(layout);
            mainPanel.Controls.Add(scrollPanel);
            this.Controls.Add(mainPanel);
            
            this.Load += (s, e) =>
            {
                var availableWidth = this.ClientSize.Width - 60;
                if (availableWidth > 0 && layout != null)
                {
                    foreach (Control control in layout.Controls)
                    {
                        if (control is GroupBox groupBox)
                        {
                            groupBox.Width = Math.Max(1500, availableWidth);
                        }
                        else if (control is Panel panel)
                        {
                            panel.Width = Math.Max(1500, availableWidth);
                        }
                    }
                    layout.Width = Math.Max(1500, availableWidth);
                }
            };
            
            this.Resize += (s, e) =>
            {
                var availableWidth = this.ClientSize.Width - 60;
                if (availableWidth > 0)
                {
                    foreach (Control control in layout.Controls)
                    {
                        if (control is GroupBox groupBox)
                        {
                            groupBox.Width = Math.Max(1500, availableWidth);
                        }
                        else if (control is Panel panel)
                        {
                            panel.Width = Math.Max(1500, availableWidth);
                        }
                    }
                    layout.Width = Math.Max(1500, availableWidth);
                }
            };
        }

        private Panel CreateToolbar()
        {
            var panel = new Panel
            {
                AutoSize = false,
                Width = 1500,
                Height = 60,
                Margin = new Padding(0, 0, 0, 20)
            };
            
            var flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right, // تغيير من Fill إلى Right
                FlowDirection = FlowDirection.RightToLeft,
                Height = 50,
                AutoSize = true // إضافة AutoSize
            };

            if (userRole == "admin")
            {
                var editBtn = new Button
                {
                    Text = "✏️ تعديل",
                    BackColor = Color.FromArgb(44, 82, 130),
                    ForeColor = Color.White,
                    Font = FontHelper.GetArabicFont(12F, FontStyle.Bold),
                    Size = new Size(120, 45),
                    FlatStyle = FlatStyle.Flat,
                    Margin = new Padding(5)
                };
                editBtn.FlatAppearance.BorderSize = 0;
                editBtn.Click += EditBtn_Click;
                flowPanel.Controls.Add(editBtn);

                var deleteBtn = new Button
                {
                    Text = "🗑️ حذف",
                    BackColor = Color.FromArgb(197, 48, 48),
                    ForeColor = Color.White,
                    Font = FontHelper.GetArabicFont(12F, FontStyle.Bold),
                    Size = new Size(120, 45),
                    FlatStyle = FlatStyle.Flat,
                    Margin = new Padding(5)
                };
                deleteBtn.FlatAppearance.BorderSize = 0;
                deleteBtn.Click += DeleteBtn_Click;
                flowPanel.Controls.Add(deleteBtn);
            }

            var printBtn = new Button
            {
                Text = "🖨️ طباعة",
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = FontHelper.GetArabicFont(12F, FontStyle.Bold),
                Size = new Size(120, 45),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5)
            };
            printBtn.FlatAppearance.BorderSize = 0;
            printBtn.Click += PrintBtn_Click;
            flowPanel.Controls.Add(printBtn);

            var closeBtn = new Button
            {
                Text = "❌ إغلاق",
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                Font = FontHelper.GetArabicFont(12F, FontStyle.Bold),
                Size = new Size(120, 45),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5)
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.Click += (s, e) => this.Close();
            flowPanel.Controls.Add(closeBtn);
            
            panel.Controls.Add(flowPanel);
            return panel;
        }

        private GroupBox detailsGroup;
        private TableLayoutPanel detailsLayout;

        private GroupBox CreateDetailsGroup()
        {
            detailsGroup = new GroupBox
            {
                Text = "تفاصيل المعاملة",
                Font = FontHelper.GetArabicFont(15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 58, 95),
                AutoSize = false,
                Width = 1500,
                Margin = new Padding(0, 0, 0, 20),
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(25)
            };

            detailsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(25),
                RightToLeft = RightToLeft.Yes
            };
            detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
            detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            detailsGroup.Controls.Add(detailsLayout);
            return detailsGroup;
        }

        private GroupBox CreateLogsGroup()
        {
            var group = new GroupBox
            {
                Text = "سجل حركة المعاملة",
                Font = FontHelper.GetArabicFont(15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 58, 95),
                AutoSize = false,
                Width = 1500,
                Height = 300,
                Margin = new Padding(0, 0, 0, 20),
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(25)
            };

            logsTable = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RightToLeft = RightToLeft.Yes,
                Font = FontHelper.GetArabicFont(12F),
                RowTemplate = { Height = 35 }
            };
            logsTable.Columns.Add("ActionType", "نوع الإجراء");
            logsTable.Columns.Add("UserName", "المستخدم");
            logsTable.Columns.Add("CreatedAt", "التاريخ");
            logsTable.Columns.Add("Notes", "الملاحظات");
            logsTable.Columns[0].Width = 150;
            logsTable.Columns[1].Width = 200;
            logsTable.Columns[2].Width = 250;
            logsTable.Columns[3].Width = 400;
            
            // تطبيق المحاذاة الوسطى على جميع الأعمدة
            foreach (DataGridViewColumn col in logsTable.Columns)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.DefaultCellStyle.Font = FontHelper.GetArabicFont(12F);
                col.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
            }
            
            logsTable.ColumnHeadersDefaultCellStyle.Font = FontHelper.GetArabicFont(12F, FontStyle.Bold);
            logsTable.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            logsTable.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 58, 95);
            logsTable.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            logsTable.ColumnHeadersHeight = 45;
            logsTable.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            logsTable.DefaultCellStyle.Font = FontHelper.GetArabicFont(12F);
            logsTable.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);

            group.Controls.Add(logsTable);
            return group;
        }

        private GroupBox CreateAttachmentsGroup()
        {
            var group = new GroupBox
            {
                Text = "الملفات المرفقة",
                Font = FontHelper.GetArabicFont(15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 58, 95),
                AutoSize = false,
                Width = 1500,
                Height = 300,
                Margin = new Padding(0, 0, 0, 20),
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(25)
            };

            attachmentsTable = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RightToLeft = RightToLeft.Yes,
                Font = FontHelper.GetArabicFont(12F),
                RowTemplate = { Height = 35 }
            };
            attachmentsTable.Columns.Add("FileName", "اسم الملف");
            attachmentsTable.Columns.Add("FileSize", "الحجم");
            attachmentsTable.Columns.Add("Actions", "الإجراءات");
            attachmentsTable.Columns[0].Width = 500;
            attachmentsTable.Columns[1].Width = 150;
            attachmentsTable.Columns[2].Width = 200;
            
            // تطبيق المحاذاة الوسطى على جميع الأعمدة
            foreach (DataGridViewColumn col in attachmentsTable.Columns)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.DefaultCellStyle.Font = FontHelper.GetArabicFont(12F);
                col.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
            }
            
            attachmentsTable.ColumnHeadersDefaultCellStyle.Font = FontHelper.GetArabicFont(12F, FontStyle.Bold);
            attachmentsTable.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            attachmentsTable.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 58, 95);
            attachmentsTable.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            attachmentsTable.ColumnHeadersHeight = 45;
            attachmentsTable.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            attachmentsTable.DefaultCellStyle.Font = FontHelper.GetArabicFont(12F);
            attachmentsTable.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
            attachmentsTable.CellPainting += AttachmentsTable_CellPainting;
            attachmentsTable.CellClick += AttachmentsTable_CellClick;

            group.Controls.Add(attachmentsTable);
            return group;
        }

        private string SafeDateToString(object dateValue)
        {
            if (dateValue == null || dateValue == DBNull.Value)
                return "-";

            try
            {
                // إذا كانت القيمة نصية، حاول تحويلها مباشرة
                if (dateValue is string dateStr)
                {
                    if (string.IsNullOrWhiteSpace(dateStr))
                        return "-";
                    if (DateTime.TryParse(dateStr, out var parsedDate))
                    {
                        var minDate = new DateTime(1900, 4, 30);
                        var maxDate = new DateTime(2077, 11, 16, 23, 59, 59);
                        if (parsedDate < minDate || parsedDate > maxDate)
                            return "-";
                        return parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    return dateStr;
                }

                // إذا كانت القيمة DateTime
                if (dateValue is DateTime dateTime)
                {
                    // التحقق من أن التاريخ ضمن النطاق المسموح به
                    var minDate = new DateTime(1900, 4, 30);
                    var maxDate = new DateTime(2077, 11, 16, 23, 59, 59);
                    
                    if (dateTime < minDate || dateTime > maxDate)
                        return "-"; // قيمة خارج النطاق
                    
                    return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                }

                // إذا كانت القيمة long أو Int64 (Ticks)
                long ticks = 0;
                if (dateValue is long l)
                    ticks = l;
                else if (dateValue is Int64 i64)
                    ticks = i64;
                else if (dateValue is int i)
                    ticks = i; // قد يكون Unix timestamp
                else if (Int64.TryParse(dateValue.ToString(), out var parsedTicks))
                    ticks = parsedTicks;
                else
                {
                    // محاولة التحويل العام
                    var str = dateValue.ToString();
                    if (DateTime.TryParse(str, out var dt))
                    {
                        var minDate = new DateTime(1900, 4, 30);
                        var maxDate = new DateTime(2077, 11, 16, 23, 59, 59);
                        if (dt < minDate || dt > maxDate)
                            return "-";
                        return dt.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    return str;
                }

                // محاولة تحويل Ticks إلى DateTime
                try
                {
                    var dateTimeFromTicks = new DateTime(ticks);
                    var minDate = new DateTime(1900, 4, 30);
                    var maxDate = new DateTime(2077, 11, 16, 23, 59, 59);
                    
                    if (dateTimeFromTicks < minDate || dateTimeFromTicks > maxDate)
                        return "-"; // قيمة خارج النطاق
                    
                    return dateTimeFromTicks.ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch
                {
                    return "-";
                }
            }
            catch
            {
                return "-";
            }
        }

        private void LoadTransaction()
        {
            LoadTransactionData();
            
            if (transactionData == null || transactionData.Count == 0)
            {
                MessageBox.Show("المعاملة غير موجودة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }

            var conn = dbManager.GetLocalConnection();
            using var cmd = new SQLiteCommand(@"
                SELECT t.*, u.full_name as creator_name
                FROM transactions t
                INNER JOIN users u ON t.created_by = u.id
                WHERE t.id = @id", conn);
            cmd.Parameters.AddWithValue("@id", transactionId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                MessageBox.Show("المعاملة غير موجودة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }

            var layout = detailsLayout;

            string GetDisplayName(string typeName, object itemId)
            {
                if (itemId == DBNull.Value || itemId == null) return "-";
                using var nameCmd = new SQLiteCommand(@"
                    SELECT di.display_name
                    FROM dropdown_items di
                    INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id
                    WHERE dt.name = @type AND di.id = @id", conn);
                nameCmd.Parameters.AddWithValue("@type", typeName);
                nameCmd.Parameters.AddWithValue("@id", itemId);
                return nameCmd.ExecuteScalar()?.ToString() ?? "-";
            }

            var createdAtStr = SafeDateToString(reader["created_at"]);
            var updatedAtStr = SafeDateToString(reader["updated_at"]);
            if (updatedAtStr == "-") updatedAtStr = createdAtStr;

            var fields = new[]
            {
                ("رقم المعاملة", reader["transaction_number"]?.ToString() ?? "-"),
                ("العنوان", reader["title"]?.ToString() ?? "-"),
                ("الوصف", reader["description"]?.ToString() ?? "-"),
                ("نوع المعاملة", GetDisplayName("transaction_category", reader["transaction_type_id"])),
                ("القسم/الجهة", GetDisplayName("department", reader["department_id"])),
                ("الحالة", GetDisplayName("status", reader["status_id"])),
                ("الأولوية", GetDisplayName("priority", reader["priority_id"])),
                ("رقم الصادر", reader["reference_number"]?.ToString() ?? "-"),
                ("الجهة المصدرة", GetDisplayName("department", reader["source_department_id"])),
                ("الجهات المعنية", reader["concerned_departments"]?.ToString() ?? "-"),
                ("رقم الإشعار", reader["notification_number"]?.ToString() ?? "-"),
                ("نوع المتابعة", GetDisplayName("follow_up_type", reader["follow_up_type_id"])),
                ("تاريخ المتابعة القادم (ميلادي)", SafeDateToString(reader["next_follow_up_date"])),
                ("تاريخ المتابعة القادم (هجري)", reader["next_follow_up_date_hijri"]?.ToString() ?? "-"),
                ("عدد الطلبات", reader["requests_count"]?.ToString() ?? "0"),
                ("الملاحظات", reader["notes"]?.ToString() ?? "-"),
                ("أنشأها", reader["creator_name"]?.ToString() ?? "-"),
                ("تاريخ الإنشاء", createdAtStr),
                ("آخر تحديث", updatedAtStr)
            };

            layout.RowCount = fields.Length;
            for (int i = 0; i < fields.Length; i++)
            {
                var label = new Label
                {
                    Text = fields[i].Item1 + ":",
                    Font = FontHelper.GetArabicFont(12F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 58, 95),
                    AutoSize = true,
                    RightToLeft = RightToLeft.Yes,
                    TextAlign = ContentAlignment.MiddleRight
                };

                var value = new Label
                {
                    Text = fields[i].Item2,
                    Font = FontHelper.GetArabicFont(12F),
                    AutoSize = true,
                    RightToLeft = RightToLeft.Yes,
                    TextAlign = ContentAlignment.MiddleRight
                };

                layout.Controls.Add(label, 0, i);
                layout.Controls.Add(value, 1, i);
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
        }

        private void LoadLogs()
        {
            logsTable.Rows.Clear();
            var conn = dbManager.GetLocalConnection();

            using var cmd = new SQLiteCommand(@"
                SELECT tl.*, u.full_name as user_name
                FROM transaction_logs tl
                INNER JOIN users u ON tl.user_id = u.id
                WHERE tl.transaction_id = @id
                ORDER BY tl.created_at DESC", conn);
            cmd.Parameters.AddWithValue("@id", transactionId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var row = logsTable.Rows.Add(
                    reader["action_type"],
                    reader["user_name"],
                    SafeDateToString(reader["created_at"]),
                    reader["notes"]?.ToString() ?? "-"
                );
                
                // تطبيق المحاذاة الوسطى على جميع الخلايا
                for (int i = 0; i < logsTable.Columns.Count; i++)
                {
                    logsTable.Rows[row].Cells[i].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
        }

        private void LoadAttachments()
        {
            attachmentsTable.Rows.Clear();
            var conn = dbManager.GetLocalConnection();

            using var cmd = new SQLiteCommand(@"
                SELECT * FROM transaction_attachments
                WHERE transaction_id = @id
                ORDER BY uploaded_at DESC", conn);
            cmd.Parameters.AddWithValue("@id", transactionId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var fileSize = Convert.ToInt64(reader["file_size"] ?? 0);
                var sizeStr = fileSize < 1024 * 1024 ? $"{fileSize / 1024.0:F2} KB" : $"{fileSize / (1024.0 * 1024.0):F2} MB";

                var filePath = reader["file_path"]?.ToString() ?? "";
                var row = attachmentsTable.Rows.Add(
                    reader["file_name"],
                    sizeStr,
                    "" // Actions column - empty, we'll add buttons via CellPainting
                );
                attachmentsTable.Rows[row].Tag = filePath;
                
                // تطبيق المحاذاة الوسطى على جميع الخلايا
                for (int i = 0; i < attachmentsTable.Columns.Count; i++)
                {
                    attachmentsTable.Rows[row].Cells[i].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
        }

        private void AttachmentsTable_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 2) return;

            var row = attachmentsTable.Rows[e.RowIndex];
            var filePath = row.Tag as string;
            
            if (string.IsNullOrEmpty(filePath))
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                e.Handled = true;
                return;
            }

            e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
            
            var cellBounds = e.CellBounds;
            var buttonWidth = 120;
            var buttonHeight = 32;
            
            var startX = cellBounds.Left + (cellBounds.Width - buttonWidth) / 2;
            var startY = cellBounds.Top + (cellBounds.Height - buttonHeight) / 2;
            
            if (startX < cellBounds.Left + 2) startX = cellBounds.Left + 2;
            if (startX + buttonWidth > cellBounds.Right - 2) startX = cellBounds.Right - buttonWidth - 2;
            if (startY < cellBounds.Top + 2) startY = cellBounds.Top + 2;
            if (startY + buttonHeight > cellBounds.Bottom - 2) startY = cellBounds.Bottom - buttonHeight - 2;

            var downloadRect = new Rectangle(startX, startY, buttonWidth, buttonHeight);
            using (var brush = new SolidBrush(Color.FromArgb(52, 152, 219)))
            {
                e.Graphics.FillRectangle(brush, downloadRect);
            }
            using (var pen = new Pen(Color.White, 1))
            {
                e.Graphics.DrawRectangle(pen, downloadRect);
            }
            TextRenderer.DrawText(e.Graphics, "⬇️ تحميل", FontHelper.GetArabicFont(10F, FontStyle.Bold),
                downloadRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);

            e.Handled = true;
        }

        private void AttachmentsTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 2) return;

            var row = attachmentsTable.Rows[e.RowIndex];
            var filePath = row.Tag as string;
            
            if (string.IsNullOrEmpty(filePath)) return;

            var cellBounds = attachmentsTable.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            var buttonWidth = 120;
            
            var startX = cellBounds.Left + (cellBounds.Width - buttonWidth) / 2;
            if (startX < cellBounds.Left + 2) startX = cellBounds.Left + 2;
            if (startX + buttonWidth > cellBounds.Right - 2) startX = cellBounds.Right - buttonWidth - 2;

            var mousePos = attachmentsTable.PointToClient(Control.MousePosition);
            var relativeX = mousePos.X - cellBounds.X;
            var relativeStartX = startX - cellBounds.X;

            if (relativeX >= relativeStartX && relativeX <= relativeStartX + buttonWidth)
            {
                DownloadAttachment(filePath);
            }
        }

        private void DownloadAttachment(string filePath)
        {
            var fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "TrackingTransactions", filePath);
            if (File.Exists(fullPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = fullPath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show("الملف غير موجود", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            var form = new TransactionForm(dbManager, userId, transactionId, this);
            form.Saved += () =>
            {
                LoadTransaction();
                LoadLogs();
                LoadAttachments();
                Saved?.Invoke();
            };
            form.ShowDialog();
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("هل أنت متأكد من حذف هذه المعاملة؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                var conn = dbManager.GetLocalConnection();
                using var updateCmd = new SQLiteCommand("UPDATE transactions SET is_deleted = 1 WHERE id = @id", conn);
                updateCmd.Parameters.AddWithValue("@id", transactionId);
                updateCmd.ExecuteNonQuery();

                using var logCmd = new SQLiteCommand("INSERT INTO transaction_logs (transaction_id, user_id, action_type, notes) VALUES (@tid, @uid, 'deleted', 'تم حذف المعاملة')", conn);
                logCmd.Parameters.AddWithValue("@tid", transactionId);
                logCmd.Parameters.AddWithValue("@uid", userId);
                logCmd.ExecuteNonQuery();

                MessageBox.Show("تم حذف المعاملة بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Saved?.Invoke();
                this.Close();
            }
        }

        private void PrintBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var printDocument = new PrintDocument();
                printDocument.PrintPage += PrintDocument_PrintPage;
                
                var printDialog = new PrintDialog
                {
                    Document = printDocument,
                    UseEXDialog = true
                };

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDocument.Print();
                    MessageBox.Show("تم إرسال المستند للطباعة بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الطباعة: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (transactionData == null || transactionData.Count == 0)
            {
                LoadTransactionData();
            }

            if (transactionData == null || transactionData.Count == 0)
            {
                e.Graphics.DrawString("المعاملة غير موجودة", FontHelper.GetArabicFont(14F, FontStyle.Bold), Brushes.Black, 100, 100);
                return;
            }

            var graphics = e.Graphics;
            var margin = 50;
            var yPos = margin;
            var pageWidth = e.PageBounds.Width - (margin * 2);
            var lineHeight = 30;
            var titleFont = FontHelper.GetArabicFont(18F, FontStyle.Bold);
            var headerFont = FontHelper.GetArabicFont(12F, FontStyle.Bold);
            var normalFont = FontHelper.GetArabicFont(11F);

            // العنوان الرئيسي
            var title = "نظام إدارة المعاملات الحكومية";
            var titleSize = graphics.MeasureString(title, titleFont);
            graphics.DrawString(title, titleFont, Brushes.Black, (e.PageBounds.Width - titleSize.Width) / 2, yPos);
            yPos += (int)titleSize.Height + 20;

            // عنوان المستند
            var docTitle = "تفاصيل المعاملة";
            var docTitleSize = graphics.MeasureString(docTitle, headerFont);
            graphics.DrawString(docTitle, headerFont, Brushes.Black, (e.PageBounds.Width - docTitleSize.Width) / 2, yPos);
            yPos += (int)docTitleSize.Height + 30;

            // رسم خط
            graphics.DrawLine(new Pen(Color.Black, 2), margin, yPos, e.PageBounds.Width - margin, yPos);
            yPos += 20;

            // بيانات المعاملة
            var fields = new[]
            {
                ("رقم المعاملة", transactionData.ContainsKey("transaction_number") ? transactionData["transaction_number"] : "-"),
                ("العنوان", transactionData.ContainsKey("title") ? transactionData["title"] : "-"),
                ("الوصف", transactionData.ContainsKey("description") ? transactionData["description"] : "-"),
                ("نوع المعاملة", transactionData.ContainsKey("transaction_type") ? transactionData["transaction_type"] : "-"),
                ("القسم/الجهة", transactionData.ContainsKey("department") ? transactionData["department"] : "-"),
                ("الحالة", transactionData.ContainsKey("status") ? transactionData["status"] : "-"),
                ("الأولوية", transactionData.ContainsKey("priority") ? transactionData["priority"] : "-"),
                ("رقم الصادر", transactionData.ContainsKey("reference_number") ? transactionData["reference_number"] : "-"),
                ("الجهة المصدرة", transactionData.ContainsKey("source_department") ? transactionData["source_department"] : "-"),
                ("الجهات المعنية", transactionData.ContainsKey("concerned_departments") ? transactionData["concerned_departments"] : "-"),
                ("رقم الإشعار", transactionData.ContainsKey("notification_number") ? transactionData["notification_number"] : "-"),
                ("نوع المتابعة", transactionData.ContainsKey("follow_up_type") ? transactionData["follow_up_type"] : "-"),
                ("تاريخ المتابعة القادم (ميلادي)", transactionData.ContainsKey("next_follow_up_date") ? transactionData["next_follow_up_date"] : "-"),
                ("تاريخ المتابعة القادم (هجري)", transactionData.ContainsKey("next_follow_up_date_hijri") ? transactionData["next_follow_up_date_hijri"] : "-"),
                ("عدد الطلبات", transactionData.ContainsKey("requests_count") ? transactionData["requests_count"] : "0"),
                ("الملاحظات", transactionData.ContainsKey("notes") ? transactionData["notes"] : "-"),
                ("أنشأها", transactionData.ContainsKey("creator_name") ? transactionData["creator_name"] : "-"),
                ("تاريخ الإنشاء", transactionData.ContainsKey("created_at") ? transactionData["created_at"] : "-"),
                ("آخر تحديث", transactionData.ContainsKey("updated_at") ? transactionData["updated_at"] : "-")
            };

            foreach (var (label, value) in fields)
            {
                if (yPos > e.PageBounds.Height - 100)
                {
                    e.HasMorePages = true;
                    return;
                }

                var labelText = $"{label} :";
                var valueText = value ?? "-";
                
                // رسم التسمية
                graphics.DrawString(labelText, headerFont, Brushes.Black, margin, yPos);
                
                // رسم القيمة
                var valueRect = new RectangleF(margin + 200, yPos, pageWidth - 200, lineHeight);
                graphics.DrawString(valueText, normalFont, Brushes.Black, valueRect);
                
                yPos += lineHeight + 5;
            }

            // رسم خط في النهاية
            yPos += 10;
            graphics.DrawLine(new Pen(Color.Black, 1), margin, yPos, e.PageBounds.Width - margin, yPos);
            yPos += 20;

            // تذييل الصفحة
            var footer = $"تم الطباعة في : {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            var footerSize = graphics.MeasureString(footer, normalFont);
            graphics.DrawString(footer, normalFont, Brushes.Gray, (e.PageBounds.Width - footerSize.Width) / 2, e.PageBounds.Height - 50);
        }

        private void LoadTransactionData()
        {
            transactionData = new System.Collections.Generic.Dictionary<string, string>();
            var conn = dbManager.GetLocalConnection();
            using var cmd = new SQLiteCommand(@"
                SELECT t.*, u.full_name as creator_name
                FROM transactions t
                INNER JOIN users u ON t.created_by = u.id
                WHERE t.id = @id", conn);
            cmd.Parameters.AddWithValue("@id", transactionId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return;

            string GetDisplayName(string typeName, object itemId)
            {
                if (itemId == DBNull.Value || itemId == null) return "-";
                using var nameCmd = new SQLiteCommand(@"
                    SELECT di.display_name
                    FROM dropdown_items di
                    INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id
                    WHERE dt.name = @type AND di.id = @id", conn);
                nameCmd.Parameters.AddWithValue("@type", typeName);
                nameCmd.Parameters.AddWithValue("@id", itemId);
                return nameCmd.ExecuteScalar()?.ToString() ?? "-";
            }

            transactionData["transaction_number"] = reader["transaction_number"]?.ToString() ?? "-";
            transactionData["title"] = reader["title"]?.ToString() ?? "-";
            transactionData["description"] = reader["description"]?.ToString() ?? "-";
            transactionData["transaction_type"] = GetDisplayName("transaction_category", reader["transaction_type_id"]);
            transactionData["department"] = GetDisplayName("department", reader["department_id"]);
            transactionData["status"] = GetDisplayName("status", reader["status_id"]);
            transactionData["priority"] = GetDisplayName("priority", reader["priority_id"]);
            transactionData["reference_number"] = reader["reference_number"]?.ToString() ?? "-";
            transactionData["source_department"] = GetDisplayName("department", reader["source_department_id"]);
            transactionData["concerned_departments"] = reader["concerned_departments"]?.ToString() ?? "-";
            transactionData["notification_number"] = reader["notification_number"]?.ToString() ?? "-";
            transactionData["follow_up_type"] = GetDisplayName("follow_up_type", reader["follow_up_type_id"]);
            transactionData["next_follow_up_date"] = SafeDateToString(reader["next_follow_up_date"]);
            transactionData["next_follow_up_date_hijri"] = reader["next_follow_up_date_hijri"]?.ToString() ?? "-";
            transactionData["requests_count"] = reader["requests_count"]?.ToString() ?? "0";
            transactionData["notes"] = reader["notes"]?.ToString() ?? "-";
            transactionData["creator_name"] = reader["creator_name"]?.ToString() ?? "-";
            transactionData["created_at"] = SafeDateToString(reader["created_at"]);
            transactionData["updated_at"] = SafeDateToString(reader["updated_at"]);
        }
    }
}

