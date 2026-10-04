using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using STOT.Config;
using STOT.Utils;

namespace STOT.UI
{
    public partial class DropdownsManagerDialog : Form
    {
        private DatabaseManager dbManager;
        private DataGridView typesTable;
        private DataGridView itemsTable;
        private TabControl tabs;

        public event Action Saved;

        public DropdownsManagerDialog(DatabaseManager dbManager, Form parent = null)
        {
            this.dbManager = dbManager;
            InitializeComponent();
            LoadTypes();
            LoadItems();
        }

        private void InitializeComponent()
        {
            this.Text = "إدارة القوائم المنسدلة";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.MinimumSize = new Size(1100, 600);

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };

            tabs = new TabControl 
            { 
                Dock = DockStyle.Fill, 
                Font = FontHelper.GetArabicFont(13F, FontStyle.Bold),
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true
            };
            tabs.TabPages.Add("أنواع القوائم");
            tabs.TabPages.Add("عناصر القوائم");

            CreateTypesTab();
            CreateItemsTab();

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

            mainPanel.Controls.Add(tabs);
            mainPanel.Controls.Add(buttonsPanel);
            this.Controls.Add(mainPanel);
        }

        private void CreateTypesTab()
        {
            var tab = tabs.TabPages[0];
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };

            var infoLabel = new Label
            {
                Text = "💡 أنواع القوائم : هنا يمكنك إدارة أنواع القوائم المنسدلة (مثل : حالة المعاملة، الأولوية، القسم/الجهة).\nكل نوع يمثل فئة من القوائم التي يمكن استخدامها في النظام.",
                Font = FontHelper.GetArabicFont(12F),
                ForeColor = Color.FromArgb(108, 117, 125),
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 85,
                Margin = new Padding(0, 0, 0, 15),
                RightToLeft = RightToLeft.No,
                TextAlign = ContentAlignment.TopRight,
                Padding = new Padding(20, 10, 20, 10),
                BackColor = Color.FromArgb(240, 248, 255),
                BorderStyle = BorderStyle.FixedSingle
            };

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                Height = 50,
                Margin = new Padding(0, 0, 0, 20)
            };

            var addBtn = new Button
            {
                Text = "➕ إضافة نوع جديد",
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = FontHelper.GetArabicFont(13F, FontStyle.Bold),
                Size = new Size(200, 40),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(3),
                Cursor = Cursors.Hand,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleCenter
            };
            addBtn.FlatAppearance.BorderSize = 0;
            addBtn.Click += AddType_Click;

            toolbar.Controls.Add(addBtn);

            typesTable = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RightToLeft = RightToLeft.Yes,
                Font = FontHelper.GetArabicFont(12F)
            };
            typesTable.Columns.Add("Name", "الاسم البرمجي");
            typesTable.Columns.Add("DisplayName", "اسم العرض");
            typesTable.Columns.Add("Description", "الوصف");
            typesTable.Columns.Add("Actions", "الإجراءات");
            
            typesTable.Columns[0].Width = 220;
            typesTable.Columns[1].Width = 250;
            typesTable.Columns[2].Width = 350;
            typesTable.Columns[3].Width = 280;
            
            typesTable.ColumnHeadersDefaultCellStyle.Font = FontHelper.GetArabicFont(14F, FontStyle.Bold);
            typesTable.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            typesTable.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 58, 95);
            typesTable.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            typesTable.ColumnHeadersHeight = 60;
            
            typesTable.DefaultCellStyle.Font = FontHelper.GetArabicFont(12F);
            typesTable.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            typesTable.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
            
            typesTable.CellPainting += TypesTable_CellPainting;
            typesTable.CellClick += TypesTable_CellClick;

            panel.Controls.Add(typesTable);
            panel.Controls.Add(toolbar);
            panel.Controls.Add(infoLabel);
            tab.Controls.Add(panel);
        }

        private void CreateItemsTab()
        {
            var tab = tabs.TabPages[1];
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };

            var infoLabel = new Label
            {
                Text = "💡 عناصر القوائم : هنا يمكنك إدارة العناصر الفعلية لكل نوع من القوائم.\nعلى سبيل المثال، لعناصر \"حالة المعاملة\" يمكنك إضافة : قيد الانتظار، قيد المعالجة، مكتملة، ملغاة.",
                Font = FontHelper.GetArabicFont(12F),
                ForeColor = Color.FromArgb(108, 117, 125),
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 85,
                Margin = new Padding(0, 0, 0, 15),
                RightToLeft = RightToLeft.No,
                TextAlign = ContentAlignment.TopRight,
                Padding = new Padding(20, 10, 20, 10),
                BackColor = Color.FromArgb(240, 248, 255),
                BorderStyle = BorderStyle.FixedSingle
            };

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.LeftToRight,
                Height = 50,
                Margin = new Padding(0, 0, 0, 20)
            };

            var addBtn = new Button
            {
                Text = "➕ إضافة عنصر جديد",
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = FontHelper.GetArabicFont(13F, FontStyle.Bold),
                Size = new Size(215, 40),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(3),
                Cursor = Cursors.Hand,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleCenter
            };
            addBtn.FlatAppearance.BorderSize = 0;
            addBtn.Click += AddItem_Click;

            toolbar.Controls.Add(addBtn);

            itemsTable = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RightToLeft = RightToLeft.Yes,
                Font = FontHelper.GetArabicFont(12F)
            };
            itemsTable.Columns.Add("Type", "النوع");
            itemsTable.Columns.Add("Name", "الاسم البرمجي");
            itemsTable.Columns.Add("DisplayName", "اسم العرض");
            itemsTable.Columns.Add("Order", "الترتيب");
            itemsTable.Columns.Add("Actions", "الإجراءات");
            
            itemsTable.Columns[0].Width = 200;
            itemsTable.Columns[1].Width = 220;
            itemsTable.Columns[2].Width = 250;
            itemsTable.Columns[3].Width = 120;
            itemsTable.Columns[4].Width = 280;
            
            itemsTable.ColumnHeadersDefaultCellStyle.Font = FontHelper.GetArabicFont(14F, FontStyle.Bold);
            itemsTable.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            itemsTable.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 58, 95);
            itemsTable.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            itemsTable.ColumnHeadersHeight = 60;
            
            itemsTable.DefaultCellStyle.Font = FontHelper.GetArabicFont(12F);
            itemsTable.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            itemsTable.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
            
            itemsTable.CellPainting += ItemsTable_CellPainting;
            itemsTable.CellClick += ItemsTable_CellClick;

            panel.Controls.Add(itemsTable);
            panel.Controls.Add(toolbar);
            panel.Controls.Add(infoLabel);
            tab.Controls.Add(panel);
        }

        private void LoadTypes()
        {
            typesTable.Rows.Clear();
            var conn = dbManager.GetLocalConnection();

            using var cmd = new SQLiteCommand("SELECT * FROM dropdown_types ORDER BY created_at DESC", conn);
            using var reader = cmd.ExecuteReader();

            var typeData = new System.Collections.Generic.List<System.Tuple<int, string, string, string>>();
            while (reader.Read())
            {
                var id = Convert.ToInt32(reader["id"]);
                var name = reader["name"]?.ToString() ?? "";
                var displayName = reader["display_name"]?.ToString() ?? "";
                var description = reader["description"]?.ToString() ?? "-";
                typeData.Add(new System.Tuple<int, string, string, string>(id, name, displayName, description));
            }

            foreach (var data in typeData)
            {
                var row = typesTable.Rows.Add(
                    data.Item2, // name
                    data.Item3, // display_name
                    data.Item4  // description
                );
                typesTable.Rows[row].Tag = data.Item1; // Store ID in row tag
            }
        }

        private void LoadItems()
        {
            itemsTable.Rows.Clear();
            var conn = dbManager.GetLocalConnection();

            using var cmd = new SQLiteCommand(@"
                SELECT di.*, dt.display_name as type_display_name
                FROM dropdown_items di
                INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id
                ORDER BY dt.name, di.display_order, di.id", conn);

            using var reader = cmd.ExecuteReader();
            var itemData = new System.Collections.Generic.List<System.Tuple<int, string, string, string, int>>();
            while (reader.Read())
            {
                var id = Convert.ToInt32(reader["id"]);
                var typeDisplayName = reader["type_display_name"]?.ToString() ?? "";
                var name = reader["name"]?.ToString() ?? "";
                var displayName = reader["display_name"]?.ToString() ?? "";
                var order = Convert.ToInt32(reader["display_order"] ?? 0);
                itemData.Add(new System.Tuple<int, string, string, string, int>(id, typeDisplayName, name, displayName, order));
            }

            foreach (var data in itemData)
            {
                var row = itemsTable.Rows.Add(
                    data.Item2, // type_display_name
                    data.Item3, // name
                    data.Item4, // display_name
                    data.Item5  // display_order
                );
                itemsTable.Rows[row].Tag = data.Item1; // Store ID in row tag
            }
        }

        private void TypesTable_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 3) return; // Actions column

            var row = typesTable.Rows[e.RowIndex];
            var typeId = row.Tag as int?;
            
            if (!typeId.HasValue) return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
            
            var cellBounds = e.CellBounds;
            var buttonWidth = 90;
            var buttonHeight = 32;
            var spacing = 5;
            var totalWidth = (buttonWidth * 2) + spacing;
            var startX = cellBounds.Left + (cellBounds.Width - totalWidth) / 2; // منتصف الخلية
            var startY = cellBounds.Top + (cellBounds.Height - buttonHeight) / 2;

            // Edit button (left)
            var editRect = new Rectangle(startX, startY, buttonWidth, buttonHeight);
            using (var brush = new SolidBrush(Color.FromArgb(44, 82, 130)))
            {
                e.Graphics.FillRectangle(brush, editRect);
            }
            using (var pen = new Pen(Color.White))
            {
                e.Graphics.DrawRectangle(pen, editRect);
            }
            TextRenderer.DrawText(e.Graphics, "✏️ تعديل", FontHelper.GetArabicFont(10F, FontStyle.Bold),
                editRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);

            // Delete button (right)
            startX += buttonWidth + spacing;
            var deleteRect = new Rectangle(startX, startY, buttonWidth, buttonHeight);
            using (var brush = new SolidBrush(Color.FromArgb(197, 48, 48)))
            {
                e.Graphics.FillRectangle(brush, deleteRect);
            }
            using (var pen = new Pen(Color.White))
            {
                e.Graphics.DrawRectangle(pen, deleteRect);
            }
            TextRenderer.DrawText(e.Graphics, "🗑️ حذف", FontHelper.GetArabicFont(10F, FontStyle.Bold),
                deleteRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);

            e.Handled = true;
        }

        private void TypesTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 3) return;

            var row = typesTable.Rows[e.RowIndex];
            var typeId = row.Tag as int?;
            
            if (!typeId.HasValue) return;

            var cellBounds = typesTable.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            var buttonWidth = 90;
            var spacing = 5;
            var totalWidth = (buttonWidth * 2) + spacing;
            var startX = cellBounds.Left + (cellBounds.Width - totalWidth) / 2; // منتصف الخلية

            var mousePos = typesTable.PointToClient(Control.MousePosition);
            var relativeX = mousePos.X - cellBounds.X;

            // Check which button was clicked (from left to right: Edit, Delete)
            if (relativeX >= startX - cellBounds.X && relativeX <= startX - cellBounds.X + buttonWidth)
            {
                // Edit button clicked
                var form = new TypeFormDialog(dbManager, typeId.Value, this);
                form.Saved += () => { LoadTypes(); LoadItems(); };
                form.ShowDialog();
            }
            else if (relativeX >= startX - cellBounds.X + buttonWidth + spacing && relativeX <= startX - cellBounds.X + totalWidth)
            {
                // Delete button clicked
                var conn = dbManager.GetLocalConnection();
                using var checkCmd = new SQLiteCommand("SELECT COUNT(*) FROM dropdown_items WHERE dropdown_type_id = @id", conn);
                checkCmd.Parameters.AddWithValue("@id", typeId.Value);
                var count = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (count > 0)
                {
                    MessageBox.Show("لا يمكن حذف هذا النوع لأنه يحتوي على عناصر", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                    return;
                }
                if (MessageBox.Show("هل أنت متأكد من حذف هذا النوع؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign) == DialogResult.Yes)
                {
                    using var cmd = new SQLiteCommand("DELETE FROM dropdown_types WHERE id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", typeId.Value);
                    cmd.ExecuteNonQuery();
                    LoadTypes();
                }
            }
        }

        private void ItemsTable_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 4) return; // Actions column

            var row = itemsTable.Rows[e.RowIndex];
            var itemId = row.Tag as int?;
            
            if (!itemId.HasValue) return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
            
            var cellBounds = e.CellBounds;
            var buttonWidth = 90;
            var buttonHeight = 32;
            var spacing = 5;
            var totalWidth = (buttonWidth * 2) + spacing;
            var startX = cellBounds.Left + (cellBounds.Width - totalWidth) / 2; // منتصف الخلية
            var startY = cellBounds.Top + (cellBounds.Height - buttonHeight) / 2;

            // Edit button (left)
            var editRect = new Rectangle(startX, startY, buttonWidth, buttonHeight);
            using (var brush = new SolidBrush(Color.FromArgb(44, 82, 130)))
            {
                e.Graphics.FillRectangle(brush, editRect);
            }
            using (var pen = new Pen(Color.White))
            {
                e.Graphics.DrawRectangle(pen, editRect);
            }
            TextRenderer.DrawText(e.Graphics, "✏️ تعديل", FontHelper.GetArabicFont(10F, FontStyle.Bold),
                editRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);

            // Delete button (right)
            startX += buttonWidth + spacing;
            var deleteRect = new Rectangle(startX, startY, buttonWidth, buttonHeight);
            using (var brush = new SolidBrush(Color.FromArgb(197, 48, 48)))
            {
                e.Graphics.FillRectangle(brush, deleteRect);
            }
            using (var pen = new Pen(Color.White))
            {
                e.Graphics.DrawRectangle(pen, deleteRect);
            }
            TextRenderer.DrawText(e.Graphics, "🗑️ حذف", FontHelper.GetArabicFont(10F, FontStyle.Bold),
                deleteRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);

            e.Handled = true;
        }

        private void ItemsTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 4) return;

            var row = itemsTable.Rows[e.RowIndex];
            var itemId = row.Tag as int?;
            
            if (!itemId.HasValue) return;

            var cellBounds = itemsTable.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            var buttonWidth = 90;
            var spacing = 5;
            var totalWidth = (buttonWidth * 2) + spacing;
            var startX = cellBounds.Left + (cellBounds.Width - totalWidth) / 2; // منتصف الخلية

            var mousePos = itemsTable.PointToClient(Control.MousePosition);
            var relativeX = mousePos.X - cellBounds.X;

            // Check which button was clicked (from left to right: Edit, Delete)
            if (relativeX >= startX - cellBounds.X && relativeX <= startX - cellBounds.X + buttonWidth)
            {
                // Edit button clicked
                var form = new ItemFormDialog(dbManager, itemId.Value, this);
                form.Saved += () => LoadItems();
                form.ShowDialog();
            }
            else if (relativeX >= startX - cellBounds.X + buttonWidth + spacing && relativeX <= startX - cellBounds.X + totalWidth)
            {
                // Delete button clicked
                if (MessageBox.Show("هل أنت متأكد من حذف هذا العنصر؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign) == DialogResult.Yes)
                {
                    var conn = dbManager.GetLocalConnection();
                    using var cmd = new SQLiteCommand("DELETE FROM dropdown_items WHERE id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", itemId.Value);
                    cmd.ExecuteNonQuery();
                    LoadItems();
                }
            }
        }

        private void AddType_Click(object sender, EventArgs e)
        {
            var form = new TypeFormDialog(dbManager, null, this);
            form.Saved += () => { LoadTypes(); LoadItems(); };
            form.ShowDialog();
        }

        private void AddItem_Click(object sender, EventArgs e)
        {
            var form = new ItemFormDialog(dbManager, null, this);
            form.Saved += () => LoadItems();
            form.ShowDialog();
        }
    }

    public partial class TypeFormDialog : Form
    {
        private DatabaseManager dbManager;
        private int? typeId;
        private TextBox nameInput;
        private TextBox displayNameInput;
        private TextBox descriptionInput;

        public event Action Saved;

        public TypeFormDialog(DatabaseManager dbManager, int? typeId = null, Form parent = null)
        {
            this.dbManager = dbManager;
            this.typeId = typeId;
            InitializeComponent();
            if (typeId.HasValue)
                LoadType();
        }

        private void InitializeComponent()
        {
            this.Text = typeId.HasValue ? "تعديل نوع قائمة" : "إضافة نوع قائمة";
            this.Size = new Size(550, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(30)
            };

            nameInput = new TextBox 
            { 
                Font = FontHelper.GetArabicFont(12F), 
                Height = 40, 
                Dock = DockStyle.Fill,
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(10, 5, 10, 5)
            };
            if (typeId.HasValue) nameInput.ReadOnly = true;

            displayNameInput = new TextBox 
            { 
                Font = FontHelper.GetArabicFont(12F), 
                Height = 40, 
                Dock = DockStyle.Fill,
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(10, 5, 10, 5)
            };
            descriptionInput = new TextBox 
            { 
                Font = FontHelper.GetArabicFont(12F), 
                Height = 80, 
                Multiline = true, 
                Dock = DockStyle.Fill, 
                ScrollBars = ScrollBars.Vertical,
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(10, 5, 10, 5)
            };

            var nameLabel = new Label 
            { 
                Text = "اسم النوع (بالإنجليزية):", 
                Font = FontHelper.GetArabicFont(12F, FontStyle.Bold), 
                AutoSize = true,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleRight
            };
            var displayNameLabel = new Label 
            { 
                Text = "اسم العرض:", 
                Font = FontHelper.GetArabicFont(12F, FontStyle.Bold), 
                AutoSize = true,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleRight
            };
            var descriptionLabel = new Label 
            { 
                Text = "الوصف:", 
                Font = FontHelper.GetArabicFont(12F, FontStyle.Bold), 
                AutoSize = true,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleRight
            };

            layout.Controls.Add(nameLabel, 0, 0);
            layout.Controls.Add(nameInput, 1, 0);
            layout.Controls.Add(displayNameLabel, 0, 1);
            layout.Controls.Add(displayNameInput, 1, 1);
            layout.Controls.Add(descriptionLabel, 0, 2);
            layout.Controls.Add(descriptionInput, 1, 2);

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

            layout.Controls.Add(buttonsPanel, 0, 3);
            layout.SetColumnSpan(buttonsPanel, 2);

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            this.Controls.Add(layout);
        }

        private void LoadType()
        {
            var conn = dbManager.GetLocalConnection();
            using var cmd = new SQLiteCommand("SELECT * FROM dropdown_types WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", typeId.Value);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                nameInput.Text = reader["name"].ToString();
                displayNameInput.Text = reader["display_name"].ToString();
                descriptionInput.Text = reader["description"]?.ToString() ?? "";
            }
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameInput.Text) || string.IsNullOrWhiteSpace(displayNameInput.Text))
            {
                MessageBox.Show("يرجى إدخال جميع الحقول المطلوبة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var conn = dbManager.GetLocalConnection();
            try
            {
                if (typeId.HasValue)
                {
                    using var cmd = new SQLiteCommand("UPDATE dropdown_types SET display_name = @display, description = @desc WHERE id = @id", conn);
                    cmd.Parameters.AddWithValue("@display", displayNameInput.Text);
                    cmd.Parameters.AddWithValue("@desc", string.IsNullOrEmpty(descriptionInput.Text) ? (object)DBNull.Value : descriptionInput.Text);
                    cmd.Parameters.AddWithValue("@id", typeId.Value);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    using var cmd = new SQLiteCommand("INSERT INTO dropdown_types (name, display_name, description) VALUES (@name, @display, @desc)", conn);
                    cmd.Parameters.AddWithValue("@name", nameInput.Text);
                    cmd.Parameters.AddWithValue("@display", displayNameInput.Text);
                    cmd.Parameters.AddWithValue("@desc", string.IsNullOrEmpty(descriptionInput.Text) ? (object)DBNull.Value : descriptionInput.Text);
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

    public partial class ItemFormDialog : Form
    {
        private DatabaseManager dbManager;
        private int? itemId;
        private ComboBox typeCombo;
        private TextBox nameInput;
        private TextBox displayNameInput;
        private NumericUpDown orderInput;
        private CheckBox activeCheck;

        public event Action Saved;

        public ItemFormDialog(DatabaseManager dbManager, int? itemId = null, Form parent = null)
        {
            this.dbManager = dbManager;
            this.itemId = itemId;
            InitializeComponent();
            LoadTypes();
            if (itemId.HasValue)
                LoadItem();
        }

        private void InitializeComponent()
        {
            this.Text = itemId.HasValue ? "تعديل عنصر" : "إضافة عنصر";
            this.Size = new Size(550, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(30)
            };

            typeCombo = new ComboBox 
            { 
                Font = FontHelper.GetArabicFont(12F), 
                Height = 40, 
                Dock = DockStyle.Fill, 
                DropDownStyle = ComboBoxStyle.DropDownList,
                RightToLeft = RightToLeft.Yes
            };
            if (itemId.HasValue) typeCombo.Enabled = false;

            nameInput = new TextBox 
            { 
                Font = FontHelper.GetArabicFont(12F), 
                Height = 40, 
                Dock = DockStyle.Fill,
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(10, 5, 10, 5)
            };
            if (itemId.HasValue) nameInput.ReadOnly = true;

            displayNameInput = new TextBox 
            { 
                Font = FontHelper.GetArabicFont(12F), 
                Height = 40, 
                Dock = DockStyle.Fill,
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(10, 5, 10, 5)
            };
            orderInput = new NumericUpDown 
            { 
                Font = FontHelper.GetArabicFont(12F), 
                Height = 40, 
                Dock = DockStyle.Fill, 
                Minimum = 0,
                RightToLeft = RightToLeft.Yes
            };
            activeCheck = new CheckBox 
            { 
                Text = "نشط", 
                Font = FontHelper.GetArabicFont(12F), 
                Checked = true,
                RightToLeft = RightToLeft.Yes
            };

            var typeLabel = new Label 
            { 
                Text = "نوع القائمة :", 
                Font = FontHelper.GetArabicFont(12F, FontStyle.Bold), 
                AutoSize = true,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleRight
            };
            var nameLabel = new Label 
            { 
                Text = "اسم العنصر (بالإنجليزية):", 
                Font = FontHelper.GetArabicFont(12F, FontStyle.Bold), 
                AutoSize = true,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleRight
            };
            var displayNameLabel = new Label 
            { 
                Text = "اسم العرض:", 
                Font = FontHelper.GetArabicFont(12F, FontStyle.Bold), 
                AutoSize = true,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleRight
            };
            var orderLabel = new Label 
            { 
                Text = "ترتيب العرض:", 
                Font = FontHelper.GetArabicFont(12F, FontStyle.Bold), 
                AutoSize = true,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleRight
            };

            layout.Controls.Add(typeLabel, 0, 0);
            layout.Controls.Add(typeCombo, 1, 0);
            layout.Controls.Add(nameLabel, 0, 1);
            layout.Controls.Add(nameInput, 1, 1);
            layout.Controls.Add(displayNameLabel, 0, 2);
            layout.Controls.Add(displayNameInput, 1, 2);
            layout.Controls.Add(orderLabel, 0, 3);
            layout.Controls.Add(orderInput, 1, 3);
            layout.Controls.Add(new Label { Text = "", AutoSize = true }, 0, 4);
            layout.Controls.Add(activeCheck, 1, 4);

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
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(120, 40),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(3)
            };
            saveBtn.FlatAppearance.BorderSize = 0;
            saveBtn.Click += SaveBtn_Click;

            var cancelBtn = new Button
            {
                Text = "❌ إلغاء",
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(120, 40),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(3)
            };
            cancelBtn.FlatAppearance.BorderSize = 0;
            cancelBtn.Click += (s, e) => this.Close();

            buttonsPanel.Controls.Add(cancelBtn);
            buttonsPanel.Controls.Add(saveBtn);

            layout.Controls.Add(buttonsPanel, 0, 5);
            layout.SetColumnSpan(buttonsPanel, 2);

            for (int i = 0; i < 6; i++)
            {
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            }
            layout.RowStyles[5] = new RowStyle(SizeType.Absolute, 50);

            this.Controls.Add(layout);
        }

        private void LoadTypes()
        {
            var conn = dbManager.GetLocalConnection();
            using var cmd = new SQLiteCommand("SELECT id, display_name FROM dropdown_types ORDER BY display_name", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                typeCombo.Items.Add(new ComboBoxItem(reader["display_name"].ToString(), Convert.ToInt32(reader["id"])));
            }
        }

        private void LoadItem()
        {
            var conn = dbManager.GetLocalConnection();
            using var cmd = new SQLiteCommand("SELECT * FROM dropdown_items WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", itemId.Value);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                for (int i = 0; i < typeCombo.Items.Count; i++)
                {
                    if (typeCombo.Items[i] is ComboBoxItem item && Convert.ToInt32(item.Value) == Convert.ToInt32(reader["dropdown_type_id"]))
                    {
                        typeCombo.SelectedIndex = i;
                        break;
                    }
                }
                nameInput.Text = reader["name"].ToString();
                displayNameInput.Text = reader["display_name"].ToString();
                orderInput.Value = Convert.ToDecimal(reader["display_order"] ?? 0);
                activeCheck.Checked = Convert.ToBoolean(reader["is_active"]);
            }
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (typeCombo.SelectedItem == null || string.IsNullOrWhiteSpace(nameInput.Text) || string.IsNullOrWhiteSpace(displayNameInput.Text))
            {
                MessageBox.Show("يرجى إدخال جميع الحقول المطلوبة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var conn = dbManager.GetLocalConnection();
            try
            {
                if (itemId.HasValue)
                {
                    using var cmd = new SQLiteCommand("UPDATE dropdown_items SET display_name = @display, display_order = @order, is_active = @active WHERE id = @id", conn);
                    cmd.Parameters.AddWithValue("@display", displayNameInput.Text);
                    cmd.Parameters.AddWithValue("@order", (int)orderInput.Value);
                    cmd.Parameters.AddWithValue("@active", activeCheck.Checked ? 1 : 0);
                    cmd.Parameters.AddWithValue("@id", itemId.Value);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    var typeId = (typeCombo.SelectedItem as ComboBoxItem).Value;
                    using var cmd = new SQLiteCommand("INSERT INTO dropdown_items (dropdown_type_id, name, display_name, display_order, is_active) VALUES (@typeId, @name, @display, @order, @active)", conn);
                    cmd.Parameters.AddWithValue("@typeId", typeId);
                    cmd.Parameters.AddWithValue("@name", nameInput.Text);
                    cmd.Parameters.AddWithValue("@display", displayNameInput.Text);
                    cmd.Parameters.AddWithValue("@order", (int)orderInput.Value);
                    cmd.Parameters.AddWithValue("@active", activeCheck.Checked ? 1 : 0);
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

