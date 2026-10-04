using System;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using STOT.Config;
using STOT.Utils;
using static STOT.Utils.HijriConverter;

namespace STOT.UI
{
    public partial class TransactionForm : Form
    {
        private DatabaseManager dbManager;
        private int userId;
        private int? transactionId;
        private string[] attachments = Array.Empty<string>();

        private TextBox transactionNumber;
        private TextBox title;
        private TextBox description;
        private TextBox outgoingNumber;
        private ComboBox issuingAuthority;
        private TextBox concernedParties;
        private ComboBox transactionType;
        private ComboBox department;
        private ComboBox status;
        private ComboBox priority;
        private ComboBox followUpType;
        private TextBox notificationNumber;
        private DateTimePicker nextFollowUpDate;
        private TextBox nextFollowUpDateHijri;
        private NumericUpDown requestsCount;
        private TextBox notes;
        private Label attachmentsLabel;
        private FlowLayoutPanel layout;

        public event Action Saved;

        public TransactionForm(DatabaseManager dbManager, int userId, int? transactionId = null, Form parent = null)
        {
            this.dbManager = dbManager;
            this.userId = userId;
            this.transactionId = transactionId;
            InitializeComponent();
            LoadDropdowns();
            if (transactionId.HasValue)
                LoadTransaction();
            
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
        }

        private void InitializeComponent()
        {
            this.Text = transactionId.HasValue ? "تعديل معاملة" : "إضافة معاملة جديدة";
            this.Size = new Size(1600, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.MinimumSize = new Size(1500, 700);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.WindowState = FormWindowState.Normal;

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30, 25, 30, 25) };
            var scrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            layout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(0),
                MinimumSize = new Size(1500, 0)
            };

            var infoGroup = CreateInfoGroup();
            var extraGroup = CreateExtraGroup();
            var followupGroup = CreateFollowupGroup();
            var notesGroup = CreateNotesGroup();
            var buttonsPanel = CreateButtonsPanel();

            layout.Controls.Add(infoGroup);
            layout.Controls.Add(extraGroup);
            layout.Controls.Add(followupGroup);
            layout.Controls.Add(notesGroup);
            layout.Controls.Add(buttonsPanel);

            scrollPanel.Controls.Add(layout);
            mainPanel.Controls.Add(scrollPanel);
            this.Controls.Add(mainPanel);

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

        private GroupBox CreateInfoGroup()
        {
            var group = new GroupBox
            {
                Text = "معلومات المعاملة",
                Font = FontHelper.GetArabicFont(15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 58, 95),
                AutoSize = false,
                Width = 1500,
                Height = 280,
                Margin = new Padding(0, 0, 0, 20),
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(25)
            };

            var layout = new TableLayoutPanel
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(25),
                RightToLeft = RightToLeft.Yes
            };

            transactionNumber = new TextBox 
            { 
                Font = FontHelper.GetArabicFont(14F), 
                Height = 50, 
                Dock = DockStyle.Fill, 
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(20, 12, 20, 12),
                Margin = new Padding(0),
                MaxLength = 10000
            };
            title = new TextBox 
            { 
                Font = FontHelper.GetArabicFont(14F), 
                Height = 50, 
                Dock = DockStyle.Fill, 
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(20, 12, 20, 12),
                Margin = new Padding(0),
                MaxLength = 10000
            };
            description = new TextBox 
            { 
                Font = FontHelper.GetArabicFont(14F), 
                Height = 130, 
                Multiline = true, 
                Dock = DockStyle.Fill, 
                ScrollBars = ScrollBars.Vertical, 
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(20, 12, 20, 12),
                Margin = new Padding(0),
                MaxLength = 100000
            };

            var numLabel = new Label 
            { 
                Text = "📋 رقم المعاملة :", 
                Font = FontHelper.GetArabicFont(14F, FontStyle.Bold), 
                AutoSize = true, 
                TextAlign = ContentAlignment.MiddleRight,
                RightToLeft = RightToLeft.Yes,
                Margin = new Padding(0, 0, 20, 0)
            };
            var titleLabel = new Label 
            { 
                Text = "📝 العنوان * :", 
                Font = FontHelper.GetArabicFont(14F, FontStyle.Bold), 
                AutoSize = true, 
                TextAlign = ContentAlignment.MiddleRight,
                RightToLeft = RightToLeft.Yes,
                Margin = new Padding(0, 0, 20, 0)
            };
            var descLabel = new Label 
            { 
                Text = "📄 الوصف :", 
                Font = FontHelper.GetArabicFont(14F, FontStyle.Bold), 
                AutoSize = true, 
                TextAlign = ContentAlignment.MiddleRight,
                RightToLeft = RightToLeft.Yes,
                Margin = new Padding(0, 0, 20, 0)
            };

            layout.Controls.Add(numLabel, 0, 0);
            layout.Controls.Add(transactionNumber, 1, 0);
            layout.Controls.Add(titleLabel, 0, 1);
            layout.Controls.Add(title, 1, 1);
            layout.Controls.Add(descLabel, 0, 2);
            layout.Controls.Add(description, 1, 2);

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 135));

            group.Controls.Add(layout);
            return group;
        }

        private GroupBox CreateExtraGroup()
        {
            var group = new GroupBox
            {
                Text = "معلومات إضافية",
                Font = FontHelper.GetArabicFont(15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 58, 95),
                AutoSize = false,
                Width = 1500,
                Height = 500,
                Margin = new Padding(0, 0, 0, 20),
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(25)
            };

            var layout = new TableLayoutPanel
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(25),
                RightToLeft = RightToLeft.Yes
            };

            outgoingNumber = new TextBox 
            { 
                Font = FontHelper.GetArabicFont(14F), 
                Height = 50, 
                Dock = DockStyle.Fill, 
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(15, 10, 15, 10)
            };
            issuingAuthority = new ComboBox 
            { 
                Font = FontHelper.GetArabicFont(14F), 
                Height = 50, 
                Dock = DockStyle.Fill, 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                RightToLeft = RightToLeft.Yes
            };
            concernedParties = new TextBox 
            { 
                Font = FontHelper.GetArabicFont(14F), 
                Height = 50, 
                Dock = DockStyle.Fill, 
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(15, 10, 15, 10)
            };
            transactionType = new ComboBox 
            { 
                Font = FontHelper.GetArabicFont(14F), 
                Height = 50, 
                Dock = DockStyle.Fill, 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                RightToLeft = RightToLeft.Yes
            };
            department = new ComboBox 
            { 
                Font = FontHelper.GetArabicFont(14F), 
                Height = 50, 
                Dock = DockStyle.Fill, 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                RightToLeft = RightToLeft.Yes
            };
            status = new ComboBox 
            { 
                Font = FontHelper.GetArabicFont(14F), 
                Height = 50, 
                Dock = DockStyle.Fill, 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                RightToLeft = RightToLeft.Yes
            };
            priority = new ComboBox 
            { 
                Font = FontHelper.GetArabicFont(14F), 
                Height = 50, 
                Dock = DockStyle.Fill, 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                RightToLeft = RightToLeft.Yes
            };

            var labels = new[]
            {
                new Label { Text = "📤 رقم الصادر :", Font = FontHelper.GetArabicFont(14F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, RightToLeft = RightToLeft.Yes, Margin = new Padding(0, 0, 20, 0) },
                new Label { Text = "🏢 الجهة المصدرة :", Font = FontHelper.GetArabicFont(14F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, RightToLeft = RightToLeft.Yes, Margin = new Padding(0, 0, 20, 0) },
                new Label { Text = "👥 الجهات المعنية :", Font = FontHelper.GetArabicFont(14F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, RightToLeft = RightToLeft.Yes, Margin = new Padding(0, 0, 20, 0) },
                new Label { Text = "📑 نوع المعاملة :", Font = FontHelper.GetArabicFont(14F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, RightToLeft = RightToLeft.Yes, Margin = new Padding(0, 0, 20, 0) },
                new Label { Text = "🏛️ القسم/الجهة :", Font = FontHelper.GetArabicFont(14F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, RightToLeft = RightToLeft.Yes, Margin = new Padding(0, 0, 20, 0) },
                new Label { Text = "📊 الحالة :", Font = FontHelper.GetArabicFont(14F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, RightToLeft = RightToLeft.Yes, Margin = new Padding(0, 0, 20, 0) },
                new Label { Text = "⚡ الأولوية :", Font = FontHelper.GetArabicFont(14F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, RightToLeft = RightToLeft.Yes, Margin = new Padding(0, 0, 20, 0) }
            };

            layout.Controls.Add(labels[0], 0, 0);
            layout.Controls.Add(outgoingNumber, 1, 0);
            layout.Controls.Add(labels[1], 0, 1);
            layout.Controls.Add(issuingAuthority, 1, 1);
            layout.Controls.Add(labels[2], 0, 2);
            layout.Controls.Add(concernedParties, 1, 2);
            layout.Controls.Add(labels[3], 0, 3);
            layout.Controls.Add(transactionType, 1, 3);
            layout.Controls.Add(labels[4], 0, 4);
            layout.Controls.Add(department, 1, 4);
            layout.Controls.Add(labels[5], 0, 5);
            layout.Controls.Add(status, 1, 5);
            layout.Controls.Add(labels[6], 0, 6);
            layout.Controls.Add(priority, 1, 6);

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < 7; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
            }

            group.Controls.Add(layout);
            return group;
        }

        private GroupBox CreateFollowupGroup()
        {
            var group = new GroupBox
            {
                Text = "معلومات المتابعة",
                Font = FontHelper.GetArabicFont(15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 58, 95),
                AutoSize = false,
                Width = 1500,
                Height = 400,
                Margin = new Padding(0, 0, 0, 20),
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(25)
            };

            var layout = new TableLayoutPanel
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(25),
                RightToLeft = RightToLeft.Yes
            };

            followUpType = new ComboBox 
            { 
                Font = FontHelper.GetArabicFont(14F), 
                Height = 50, 
                Dock = DockStyle.Fill, 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                RightToLeft = RightToLeft.Yes
            };
            notificationNumber = new TextBox 
            { 
                Font = FontHelper.GetArabicFont(14F), 
                Height = 50, 
                Dock = DockStyle.Fill, 
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(15, 10, 15, 10)
            };
            nextFollowUpDate = new DateTimePicker 
            { 
                Font = FontHelper.GetArabicFont(14F), 
                Height = 50, 
                Dock = DockStyle.Fill, 
                Format = DateTimePickerFormat.Short, 
                RightToLeft = RightToLeft.Yes
            };
            nextFollowUpDate.ValueChanged += NextFollowUpDate_ValueChanged;
            nextFollowUpDateHijri = new TextBox 
            { 
                Font = FontHelper.GetArabicFont(14F), 
                Height = 50, 
                Dock = DockStyle.Fill, 
                ReadOnly = true, 
                BackColor = Color.FromArgb(248, 249, 250), 
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(15, 10, 15, 10)
            };
            requestsCount = new NumericUpDown 
            { 
                Font = FontHelper.GetArabicFont(14F), 
                Height = 50, 
                Dock = DockStyle.Fill, 
                Minimum = 0, 
                RightToLeft = RightToLeft.Yes
            };

            var followupLabels = new[]
            {
                new Label { Text = "🔄 نوع المتابعة :", Font = FontHelper.GetArabicFont(14F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, RightToLeft = RightToLeft.Yes, Margin = new Padding(0, 0, 20, 0) },
                new Label { Text = "🔔 رقم الإشعار :", Font = FontHelper.GetArabicFont(14F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, RightToLeft = RightToLeft.Yes, Margin = new Padding(0, 0, 20, 0) },
                new Label { Text = "📅 تاريخ المتابعة القادم (ميلادي) :", Font = FontHelper.GetArabicFont(14F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, RightToLeft = RightToLeft.Yes, Margin = new Padding(0, 0, 20, 0) },
                new Label { Text = "📅 تاريخ المتابعة القادم (هجري) :", Font = FontHelper.GetArabicFont(14F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, RightToLeft = RightToLeft.Yes, Margin = new Padding(0, 0, 20, 0) },
                new Label { Text = "🔢 عدد الطلبات :", Font = FontHelper.GetArabicFont(14F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight, RightToLeft = RightToLeft.Yes, Margin = new Padding(0, 0, 20, 0) }
            };

            layout.Controls.Add(followupLabels[0], 0, 0);
            layout.Controls.Add(followUpType, 1, 0);
            layout.Controls.Add(followupLabels[1], 0, 1);
            layout.Controls.Add(notificationNumber, 1, 1);
            layout.Controls.Add(followupLabels[2], 0, 2);
            layout.Controls.Add(nextFollowUpDate, 1, 2);
            layout.Controls.Add(followupLabels[3], 0, 3);
            layout.Controls.Add(nextFollowUpDateHijri, 1, 3);
            layout.Controls.Add(followupLabels[4], 0, 4);
            layout.Controls.Add(requestsCount, 1, 4);

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < 5; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            }

            group.Controls.Add(layout);
            return group;
        }

        private GroupBox CreateNotesGroup()
        {
            var group = new GroupBox
            {
                Text = "ملاحظات ومرفقات",
                Font = FontHelper.GetArabicFont(15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 58, 95),
                AutoSize = false,
                Width = 1500,
                Height = 320,
                Margin = new Padding(0, 0, 0, 20),
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(25)
            };

            var layout = new TableLayoutPanel
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(25),
                RightToLeft = RightToLeft.Yes
            };

            notes = new TextBox 
            { 
                Font = FontHelper.GetArabicFont(14F), 
                Height = 130, 
                Multiline = true, 
                Dock = DockStyle.Fill, 
                ScrollBars = ScrollBars.Vertical, 
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(15, 10, 15, 10)
            };

            attachmentsLabel = new Label
            {
                Text = "لم يتم اختيار ملفات",
                Font = FontHelper.GetArabicFont(14F),
                AutoSize = true
            };

            var attachmentsBtn = new Button
            {
                Text = "📎 اختيار ملفات",
                Font = FontHelper.GetArabicFont(14F, FontStyle.Bold),
                BackColor = Color.FromArgb(30, 58, 95),
                ForeColor = Color.White,
                Size = new Size(180, 50),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            attachmentsBtn.FlatAppearance.BorderSize = 0;
            attachmentsBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 70, 120);
            attachmentsBtn.Click += AttachmentsBtn_Click;

            var attachmentsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true
            };
            attachmentsPanel.Controls.Add(attachmentsBtn);
            attachmentsPanel.Controls.Add(attachmentsLabel);

            layout.Controls.Add(new Label { Text = "الملاحظات :", Font = FontHelper.GetArabicFont(13F, FontStyle.Bold), AutoSize = true, TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            layout.Controls.Add(notes, 0, 1);
            layout.Controls.Add(attachmentsPanel, 0, 2);

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

            group.Controls.Add(layout);
            return group;
        }

        private Panel CreateButtonsPanel()
        {
            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = false,
                Width = 1500,
                Height = 80,
                Padding = new Padding(25),
                RightToLeft = RightToLeft.Yes
            };

            var saveBtn = new Button
            {
                Text = "💾 حفظ",
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = FontHelper.GetArabicFont(15F, FontStyle.Bold),
                Size = new Size(160, 55),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(10, 0, 10, 0),
                Cursor = Cursors.Hand,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0)
            };
            saveBtn.FlatAppearance.BorderSize = 0;
            saveBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 190, 110);
            saveBtn.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 150, 80);
            saveBtn.Click += SaveBtn_Click;

            var cancelBtn = new Button
            {
                Text = "❌ إلغاء",
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                Font = FontHelper.GetArabicFont(15F, FontStyle.Bold),
                Size = new Size(160, 55),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(10, 0, 10, 0),
                Cursor = Cursors.Hand,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0)
            };
            cancelBtn.FlatAppearance.BorderSize = 0;
            cancelBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(170, 180, 185);
            cancelBtn.FlatAppearance.MouseDownBackColor = Color.FromArgb(130, 145, 150);
            cancelBtn.Click += (s, e) => this.Close();

            panel.Controls.Add(cancelBtn);
            panel.Controls.Add(saveBtn);

            return panel;
        }

        private void LoadDropdowns()
        {
            var conn = dbManager.GetLocalConnection();
            using var cmd = new SQLiteCommand(@"
                SELECT dt.name, di.id, di.display_name
                FROM dropdown_items di
                INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id
                WHERE di.is_active = 1
                ORDER BY dt.name, di.display_order, di.id", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var typeName = reader["name"].ToString();
                var itemId = Convert.ToInt32(reader["id"]);
                var displayName = reader["display_name"].ToString();

                switch (typeName)
                {
                    case "department":
                        department.Items.Add(new ComboBoxItem(displayName, itemId));
                        issuingAuthority.Items.Add(new ComboBoxItem(displayName, itemId));
                        break;
                    case "transaction_category":
                        transactionType.Items.Add(new ComboBoxItem(displayName, itemId));
                        break;
                    case "status":
                        status.Items.Add(new ComboBoxItem(displayName, itemId));
                        break;
                    case "priority":
                        priority.Items.Add(new ComboBoxItem(displayName, itemId));
                        break;
                    case "follow_up_type":
                        followUpType.Items.Add(new ComboBoxItem(displayName, itemId));
                        break;
                }
            }
        }

        private void LoadTransaction()
        {
            var conn = dbManager.GetLocalConnection();
            using var cmd = new SQLiteCommand("SELECT * FROM transactions WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", transactionId.Value);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return;

            transactionNumber.Text = reader["transaction_number"]?.ToString() ?? "";
            title.Text = reader["title"]?.ToString() ?? "";
            description.Text = reader["description"]?.ToString() ?? "";
            outgoingNumber.Text = reader["reference_number"]?.ToString() ?? "";
            concernedParties.Text = reader["concerned_departments"]?.ToString() ?? "";
            notificationNumber.Text = reader["notification_number"]?.ToString() ?? "";
            notes.Text = reader["notes"]?.ToString() ?? "";
            requestsCount.Value = Convert.ToDecimal(reader["requests_count"] ?? 0);

            if (reader["next_follow_up_date"] != DBNull.Value)
            {
                nextFollowUpDate.Value = Convert.ToDateTime(reader["next_follow_up_date"]);
                NextFollowUpDate_ValueChanged(null, null);
            }

            SetComboBoxValue(department, reader["department_id"]);
            SetComboBoxValue(status, reader["status_id"]);
            SetComboBoxValue(priority, reader["priority_id"]);
            SetComboBoxValue(transactionType, reader["transaction_type_id"]);
            SetComboBoxValue(followUpType, reader["follow_up_type_id"]);
            SetComboBoxValue(issuingAuthority, reader["source_department_id"]);
        }

        private void SetComboBoxValue(ComboBox combo, object value)
        {
            if (value == DBNull.Value || value == null) return;
            var itemId = Convert.ToInt32(value);
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i] is ComboBoxItem item && Convert.ToInt32(item.Value) == itemId)
                {
                    combo.SelectedIndex = i;
                    break;
                }
            }
        }

        private void NextFollowUpDate_ValueChanged(object sender, EventArgs e)
        {
            nextFollowUpDateHijri.Text = HijriConverter.GregorianToHijri(nextFollowUpDate.Value.ToString("yyyy-MM-dd"));
        }

        private void AttachmentsBtn_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "جميع الملفات (*.*)|*.*|PDF (*.pdf)|*.pdf|Word (*.doc;*.docx)|*.doc;*.docx|صور (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                attachments = ofd.FileNames;
                var fileNames = attachments.Take(3).Select(Path.GetFileName).ToArray();
                var text = $"تم اختيار {attachments.Length} ملف(ات): {string.Join(", ", fileNames)}";
                if (attachments.Length > 3)
                    text += $" ... و {attachments.Length - 3} ملف آخر";
                attachmentsLabel.Text = text;
                attachmentsLabel.ForeColor = Color.FromArgb(39, 174, 96);
            }
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(title.Text))
            {
                MessageBox.Show("يرجى إدخال عنوان المعاملة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var conn = dbManager.GetLocalConnection();
            using var transaction = conn.BeginTransaction();

            try
            {
                var transNumber = transactionNumber.Text.Trim();
                if (string.IsNullOrEmpty(transNumber))
                {
                    var year = DateTime.Now.Year;
                    using var countCmd = new SQLiteCommand("SELECT COUNT(*) FROM transactions WHERE strftime('%Y', created_at) = @year", conn);
                    countCmd.Parameters.AddWithValue("@year", year.ToString());
                    var count = Convert.ToInt32(countCmd.ExecuteScalar());
                    transNumber = $"TRX-{year}-{(count + 1):D6}";
                }

                if (!transactionId.HasValue)
                {
                    using var checkCmd = new SQLiteCommand("SELECT id FROM transactions WHERE transaction_number = @num", conn);
                    checkCmd.Parameters.AddWithValue("@num", transNumber);
                    if (checkCmd.ExecuteScalar() != null)
                    {
                        MessageBox.Show("رقم المعاملة موجود بالفعل", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string nextFollowUpDateStr = null;
                string nextFollowUpDateHijriStr = null;
                if (nextFollowUpDate.Value != DateTime.Today || !string.IsNullOrEmpty(nextFollowUpDateHijri.Text))
                {
                    nextFollowUpDateStr = nextFollowUpDate.Value.ToString("yyyy-MM-dd");
                    nextFollowUpDateHijriStr = nextFollowUpDateHijri.Text;
                }

                if (transactionId.HasValue)
                {
                    using var updateCmd = new SQLiteCommand(@"
                        UPDATE transactions SET
                            transaction_number = @num, title = @title, description = @desc,
                            department_id = @dept, status_id = @status, priority_id = @priority,
                            transaction_type_id = @type, follow_up_type_id = @followup,
                            reference_number = @ref, notification_number = @notif,
                            next_follow_up_date = @date, next_follow_up_date_hijri = @dateHijri,
                            notes = @notes, requests_count = @requests, updated_at = CURRENT_TIMESTAMP
                        WHERE id = @id", conn);
                    AddTransactionParameters(updateCmd, transNumber, nextFollowUpDateStr, nextFollowUpDateHijriStr);
                    updateCmd.Parameters.AddWithValue("@id", transactionId.Value);
                    updateCmd.ExecuteNonQuery();
                }
                else
                {
                    using var insertCmd = new SQLiteCommand(@"
                        INSERT INTO transactions (
                            transaction_number, title, description,
                            department_id, source_department_id, concerned_departments,
                            status_id, priority_id, transaction_type_id, follow_up_type_id,
                            reference_number, notification_number,
                            next_follow_up_date, next_follow_up_date_hijri,
                            notes, requests_count, created_by
                        ) VALUES (@num, @title, @desc, @dept, @sourceDept, @concerned, @status, @priority, @type, @followup, @ref, @notif, @date, @dateHijri, @notes, @requests, @userId)", conn);
                    AddTransactionParameters(insertCmd, transNumber, nextFollowUpDateStr, nextFollowUpDateHijriStr);
                    insertCmd.Parameters.AddWithValue("@sourceDept", GetComboBoxValue(issuingAuthority));
                    insertCmd.Parameters.AddWithValue("@concerned", string.IsNullOrEmpty(concernedParties.Text) ? (object)DBNull.Value : concernedParties.Text);
                    insertCmd.Parameters.AddWithValue("@userId", userId);
                    insertCmd.ExecuteNonQuery();
                    transactionId = (int)conn.LastInsertRowId;

                    using var logCmd = new SQLiteCommand("INSERT INTO transaction_logs (transaction_id, user_id, action_type, notes) VALUES (@tid, @uid, 'created', 'تم إنشاء المعاملة')", conn);
                    logCmd.Parameters.AddWithValue("@tid", transactionId.Value);
                    logCmd.Parameters.AddWithValue("@uid", userId);
                    logCmd.ExecuteNonQuery();
                }

                if (attachments.Length > 0 && transactionId.HasValue)
                    SaveAttachments(conn);

                transaction.Commit();
                MessageBox.Show("تم حفظ المعاملة بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Saved?.Invoke();
                this.Close();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show($"حدث خطأ أثناء الحفظ: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddTransactionParameters(SQLiteCommand cmd, string transNumber, string date, string dateHijri)
        {
            cmd.Parameters.AddWithValue("@num", transNumber);
            cmd.Parameters.AddWithValue("@title", title.Text);
            cmd.Parameters.AddWithValue("@desc", string.IsNullOrEmpty(description.Text) ? (object)DBNull.Value : description.Text);
            cmd.Parameters.AddWithValue("@dept", GetComboBoxValue(department));
            cmd.Parameters.AddWithValue("@status", GetComboBoxValue(status));
            cmd.Parameters.AddWithValue("@priority", GetComboBoxValue(priority));
            cmd.Parameters.AddWithValue("@type", GetComboBoxValue(transactionType));
            cmd.Parameters.AddWithValue("@followup", GetComboBoxValue(followUpType));
            cmd.Parameters.AddWithValue("@ref", string.IsNullOrEmpty(outgoingNumber.Text) ? (object)DBNull.Value : outgoingNumber.Text);
            cmd.Parameters.AddWithValue("@notif", string.IsNullOrEmpty(notificationNumber.Text) ? (object)DBNull.Value : notificationNumber.Text);
            cmd.Parameters.AddWithValue("@date", date ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@dateHijri", dateHijri ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@notes", string.IsNullOrEmpty(notes.Text) ? (object)DBNull.Value : notes.Text);
            cmd.Parameters.AddWithValue("@requests", (int)requestsCount.Value);
        }

        private object GetComboBoxValue(ComboBox combo)
        {
            if (combo.SelectedItem is ComboBoxItem item)
                return item.Value;
            return DBNull.Value;
        }

        private void SaveAttachments(SQLiteConnection conn)
        {
            var uploadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "TrackingTransactions", "attachments", transactionId.Value.ToString());
            Directory.CreateDirectory(uploadDir);

            foreach (var filePath in attachments)
            {
                var fileName = Path.GetFileName(filePath);
                var destPath = Path.Combine(uploadDir, fileName);
                File.Copy(filePath, destPath, true);

                var fileSize = new FileInfo(filePath).Length;
                var fileType = Path.GetExtension(fileName);
                var relativePath = $"attachments/{transactionId.Value}/{fileName}";

                using var cmd = new SQLiteCommand(@"
                    INSERT INTO transaction_attachments 
                    (transaction_id, file_name, file_path, file_type, file_size, uploaded_by)
                    VALUES (@tid, @name, @path, @type, @size, @uid)", conn);
                cmd.Parameters.AddWithValue("@tid", transactionId.Value);
                cmd.Parameters.AddWithValue("@name", fileName);
                cmd.Parameters.AddWithValue("@path", relativePath);
                cmd.Parameters.AddWithValue("@type", fileType);
                cmd.Parameters.AddWithValue("@size", fileSize);
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}

