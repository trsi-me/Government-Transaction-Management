using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using STOT.Config;
using STOT.Utils;

namespace STOT.UI
{
    public partial class MainWindow : Form
    {
        private DatabaseManager dbManager;
        private int userId;
        private string username;
        private string role;
        private string fullName;

        private TabControl tabs;
        private Panel headerPanel;
        private Panel totalCard, pendingCard, inProgressCard, completedCard;
        private DataGridView recentTable, transactionsTable, logsTable;
        private TextBox searchInput;
        private ComboBox statusFilter;


        public MainWindow(DatabaseManager dbManager, int userId, string username, string role, string fullName)
        {
            this.dbManager = dbManager;
            this.userId = userId;
            this.username = username;
            this.role = role;
            this.fullName = fullName;
            InitializeComponent();
            
            // تحميل البيانات بعد عرض النافذة
            this.Load += MainWindow_Load;
        }
        
        private void MainWindow_Load(object sender, EventArgs e)
        {
            // تأخير بسيط للتأكد من أن جميع العناصر جاهزة
            System.Threading.Tasks.Task.Delay(100).ContinueWith(_ => 
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => LoadDashboardData()));
                }
                else
                {
                    LoadDashboardData();
                }
            });
        }

        private void InitializeComponent()
        {
            this.Text = "نظام إدارة المعاملات الحكومية";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.WindowState = FormWindowState.Maximized;

            var mainPanel = new Panel { Dock = DockStyle.Fill };

            headerPanel = new Panel
            {
                Height = 120,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(30, 58, 95)
            };

            var headerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(30, 15, 30, 15)
            };

            var titleLabel = new Label
            {
                Text = "نظام إدارة المعاملات الحكومية",
                Font = FontHelper.GetArabicFont(16F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Dock = DockStyle.Fill,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleRight
            };

            var userInfoLabel = new Label
            {
                Text = $"مرحباً، {fullName}",
                Font = FontHelper.GetArabicFont(11F),
                ForeColor = Color.White,
                AutoSize = true,
                Dock = DockStyle.Fill,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };


            var logoutBtn = new Button
            {
                Text = "🚪 تسجيل الخروج",
                BackColor = Color.FromArgb(197, 48, 48),
                ForeColor = Color.White,
                Font = FontHelper.GetArabicFont(10F, FontStyle.Bold),
                Size = new Size(200, 35),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(3),
                Cursor = Cursors.Hand,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleCenter
            };
            logoutBtn.FlatAppearance.BorderSize = 0;
            logoutBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 60, 60);
            logoutBtn.Click += LogoutBtn_Click;

            buttonsPanel.Controls.Add(userInfoLabel);
            buttonsPanel.Controls.Add(logoutBtn);

            // ترتيب الأعمدة: العنوان (أقصى اليمين)، مساحة فارغة، الأزرار والمستخدم (أقصى اليسار)
            headerLayout.Controls.Add(titleLabel, 0, 0);
            headerLayout.Controls.Add(new Panel { Dock = DockStyle.Fill }, 1, 0); // مساحة فارغة في الوسط
            headerLayout.Controls.Add(buttonsPanel, 2, 0);

            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // للعنوان
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // مساحة فارغة
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // للأزرار والمستخدم

            headerPanel.Controls.Add(headerLayout);

            tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = FontHelper.GetArabicFont(11F, FontStyle.Bold),
                Appearance = TabAppearance.FlatButtons,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true,
                Alignment = TabAlignment.Top,
                DrawMode = TabDrawMode.OwnerDrawFixed,
                ItemSize = new Size(180, 50),
                SizeMode = TabSizeMode.Fixed,
                Padding = new Point(0, 6),
                Multiline = false
            };
            
            tabs.DrawItem += (s, e) =>
            {
                var tab = tabs.TabPages[e.Index];
                var rect = e.Bounds;
                var isSelected = tabs.SelectedIndex == e.Index;
                
                e.Graphics.FillRectangle(
                    new SolidBrush(isSelected ? Color.FromArgb(240, 240, 240) : Color.FromArgb(220, 220, 220)),
                    rect);
                
                var textColor = isSelected ? Color.Black : Color.FromArgb(50, 50, 50);
                var textFont = isSelected ? FontHelper.GetArabicFont(11F, FontStyle.Bold) : FontHelper.GetArabicFont(11F, FontStyle.Regular);
                
                var textRect = new Rectangle(rect.X + 10, rect.Y + 8, rect.Width - 20, rect.Height - 16);
                TextRenderer.DrawText(e.Graphics, tab.Text, textFont, textRect, textColor, 
                    TextFormatFlags.RightToLeft | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            };

            // إضافة التبويبات بالترتيب الصحيح (من اليمين لليسار في RTL)
            tabs.TabPages.Add("لوحة التحكم");
            tabs.TabPages.Add("المعاملات");
            if (role == "admin")
            {
                tabs.TabPages.Add("إدارة القوائم");
                tabs.TabPages.Add("إدارة المستخدمين");
            }

            CreateDashboardTab();
            CreateTransactionsTab();
            if (role == "admin")
            {
                CreateDropdownsTab();
                CreateUsersTab();
            }

            // جعل لوحة التحكم هي الصفحة الافتراضية (أول تبويب من اليمين)
            tabs.SelectedIndex = 0;
            
            // تحميل بيانات التصفية
            LoadStatusFilter();

            mainPanel.Controls.Add(tabs);
            mainPanel.Controls.Add(headerPanel);
            this.Controls.Add(mainPanel);
        }

        private void CreateDashboardTab()
        {
            var tab = tabs.TabPages[0];
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };

            var title = new Label
            {
                Text = "لوحة التحكم الرئيسية",
                Font = FontHelper.GetArabicFont(16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 58, 95),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 15),
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleRight
            };

            var statsContainer = new Panel
            {
                Dock = DockStyle.Top,
                Height = 550,
                Margin = new Padding(0, 0, 0, 30),
                Padding = new Padding(10)
            };

            var statsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                AutoSize = false
            };

            statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            statsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 530));

            totalCard = CreateStatCard("📄 إجمالي المعاملات", "0", Color.FromArgb(30, 58, 95));
            pendingCard = CreateStatCard("⏳ قيد الانتظار", "0", Color.FromArgb(243, 156, 18));
            inProgressCard = CreateStatCard("🔄 قيد المعالجة", "0", Color.FromArgb(52, 152, 219));
            completedCard = CreateStatCard("✅ مكتملة", "0", Color.FromArgb(39, 174, 96));

            statsPanel.Controls.Add(totalCard, 0, 0);
            statsPanel.Controls.Add(pendingCard, 1, 0);
            statsPanel.Controls.Add(inProgressCard, 2, 0);
            statsPanel.Controls.Add(completedCard, 3, 0);
            
            // إجبار إعادة الرسم الفوري
            totalCard.Invalidate();
            pendingCard.Invalidate();
            inProgressCard.Invalidate();
            completedCard.Invalidate();

            statsContainer.Controls.Add(statsPanel);

            var recentLabel = new Label
            {
                Text = "آخر المعاملات",
                Font = FontHelper.GetArabicFont(13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 58, 95),
                AutoSize = true,
                Margin = new Padding(0, 15, 0, 8),
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleRight
            };

            recentTable = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Height = 250,
                RightToLeft = RightToLeft.Yes,
                Font = FontHelper.GetArabicFont(10F)
            };
            recentTable.Columns.Add("TransactionNumber", "رقم المعاملة");
            recentTable.Columns.Add("Title", "العنوان");
            recentTable.Columns.Add("Status", "الحالة");
            recentTable.Columns.Add("CreatedAt", "تاريخ الإنشاء");
            recentTable.Columns.Add("Actions", "الإجراءات");
            recentTable.CellClick += RecentTable_CellClick;
            recentTable.CellPainting += RecentTable_CellPainting;
            
            recentTable.Columns[0].Width = 150;
            recentTable.Columns[1].Width = 300;
            recentTable.Columns[2].Width = 250;
            recentTable.Columns[3].Width = 225;
            recentTable.Columns[4].Width = 300;
            
            recentTable.ColumnHeadersDefaultCellStyle.Font = FontHelper.GetArabicFont(11F, FontStyle.Bold);
            recentTable.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            recentTable.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 58, 95);
            recentTable.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            recentTable.ColumnHeadersHeight = 45;
            recentTable.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            recentTable.DefaultCellStyle.Font = FontHelper.GetArabicFont(10F);
            recentTable.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };

            layout.Controls.Add(title, 0, 0);
            layout.Controls.Add(statsContainer, 0, 1);
            layout.Controls.Add(recentLabel, 0, 2);
            layout.Controls.Add(recentTable, 0, 3);

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            panel.Controls.Add(layout);
            tab.Controls.Add(panel);
        }

        private Panel CreateStatCard(string title, string value, Color color)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = color,
                Margin = new Padding(15, 15, 15, 15),
                Padding = new Padding(10, 10, 10, 10)
            };

            // حفظ البيانات في Tag
            card.Tag = new System.Tuple<string, string>(title, value ?? "0");

            // استخدام Paint event لرسم النص مباشرة
            card.Paint += (s, e) =>
            {
                try
                {
                    var panel = s as Panel;
                    if (panel == null) return;
                    
                    var rect = panel.ClientRectangle;
                    if (rect.Width <= 0 || rect.Height <= 0) return;
                    
                    if (panel.Tag == null) return;
                    
                    var data = panel.Tag as System.Tuple<string, string>;
                    if (data == null) return;

                    var titleText = data.Item1 ?? "";
                    var valueText = data.Item2 ?? "0";

                    // رسم الرقم في الأعلى تماماً
                    var valueFont = FontHelper.GetArabicFont(48F, FontStyle.Bold);
                    var valueRect = new RectangleF(rect.X + 10, rect.Y + 15, rect.Width - 20, rect.Height * 0.5f);
                    using (var brush = new SolidBrush(Color.White))
                    {
                        var valueFormat = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Near, // في الأعلى
                            FormatFlags = StringFormatFlags.DirectionRightToLeft | StringFormatFlags.NoWrap
                        };
                        e.Graphics.DrawString(valueText, valueFont, brush, valueRect, valueFormat);
                    }

                    // رسم العنوان أسفل الرقم مباشرة
                    var titleFont = FontHelper.GetArabicFont(16F, FontStyle.Bold);
                    var titleRect = new RectangleF(rect.X + 10, rect.Y + rect.Height * 0.5f + 10, rect.Width - 20, rect.Height * 0.4f);
                    using (var brush = new SolidBrush(Color.White))
                    {
                        var titleFormat = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Near, // في الأعلى
                            FormatFlags = StringFormatFlags.DirectionRightToLeft | StringFormatFlags.NoWrap
                        };
                        e.Graphics.DrawString(titleText, titleFont, brush, titleRect, titleFormat);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in Paint event: {ex.Message}");
                }
            };

            return card;
        }

        private void CreateTransactionsTab()
        {
            var tab = tabs.TabPages[1];
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30), AutoScroll = true };

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 60,
                FlowDirection = FlowDirection.RightToLeft,
                Margin = new Padding(0, 0, 0, 20),
                BackColor = Color.Transparent,
                AutoSize = false
            };

            var addBtn = new Button
            {
                Text = "➕ إضافة معاملة جديدة",
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = FontHelper.GetArabicFont(10F, FontStyle.Bold),
                Size = new Size(250, 40),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(4),
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleCenter
            };
            addBtn.FlatAppearance.BorderSize = 0;
            addBtn.Click += AddTransaction_Click;

            var exportBtn = new Button
            {
                Text = "📥 تصدير",
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = FontHelper.GetArabicFont(10F, FontStyle.Bold),
                Size = new Size(100, 40),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(4),
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleCenter
            };
            exportBtn.FlatAppearance.BorderSize = 0;
            exportBtn.Click += ExportTransactions_Click;

            var refreshBtn = new Button
            {
                Text = "🔄 تحديث",
                BackColor = Color.FromArgb(44, 82, 130),
                ForeColor = Color.White,
                Font = FontHelper.GetArabicFont(10F, FontStyle.Bold),
                Size = new Size(100, 40),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(4),
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleCenter
            };
            refreshBtn.FlatAppearance.BorderSize = 0;
            refreshBtn.Click += (s, e) => LoadTransactions();

            toolbar.Controls.Add(refreshBtn);
            toolbar.Controls.Add(exportBtn);
            toolbar.Controls.Add(addBtn);

            var filterPanel = new GroupBox
            {
                Text = "البحث والتصفية",
                AutoSize = false,
                Width = 1500,
                Height = 300,
                Font = FontHelper.GetArabicFont(15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 58, 95),
                Margin = new Padding(0, 0, 0, 20),
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(25)
            };

            var filterLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1,
                Padding = new Padding(10, 5, 10, 5)
            };
            
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            filterLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var searchLabel = new Label
            {
                Text = "🔍 بحث :",
                Font = FontHelper.GetArabicFont(10F, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 6, 12, 0),
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleCenter
            };

            searchInput = new TextBox
            {
                Font = FontHelper.GetArabicFont(10F),
                Size = new Size(250, 26),
                Margin = new Padding(4),
                RightToLeft = RightToLeft.Yes,
                TextAlign = HorizontalAlignment.Center
            };
            searchInput.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadTransactions(); };

            var statusLabel = new Label
            {
                Text = "الحالة :",
                Font = FontHelper.GetArabicFont(10F, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(15, 6, 8, 0),
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleCenter
            };

            statusFilter = new ComboBox
            {
                Font = FontHelper.GetArabicFont(10F),
                Size = new Size(150, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(4),
                RightToLeft = RightToLeft.Yes
            };
            statusFilter.DrawMode = DrawMode.OwnerDrawFixed;
            statusFilter.DrawItem += (s, e) =>
            {
                e.DrawBackground();
                if (e.Index >= 0)
                {
                    var text = statusFilter.Items[e.Index].ToString();
                    var textRect = e.Bounds;
                    TextRenderer.DrawText(e.Graphics, text, FontHelper.GetArabicFont(10F),
                        textRect, e.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);
                }
            };
            statusFilter.Items.Add(new ComboBoxItem("جميع الحالات", null));

            var filterBtn = new Button
            {
                Text = "🔍 تصفية",
                BackColor = Color.FromArgb(30, 58, 95),
                ForeColor = Color.White,
                Font = FontHelper.GetArabicFont(10F, FontStyle.Bold),
                Size = new Size(100, 100),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(4),
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleCenter
            };
            filterBtn.FlatAppearance.BorderSize = 0;
            filterBtn.Click += (s, e) => LoadTransactions();

            var clearBtn = new Button
            {
                Text = "🗑️ مسح",
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                Font = FontHelper.GetArabicFont(10F, FontStyle.Bold),
                Size = new Size(120, 120),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(4),
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleCenter
            };
            clearBtn.FlatAppearance.BorderSize = 0;
            clearBtn.Click += ClearFilters_Click;

            filterLayout.Controls.Add(searchLabel, 0, 0);
            filterLayout.Controls.Add(searchInput, 1, 0);
            filterLayout.Controls.Add(statusLabel, 2, 0);
            filterLayout.Controls.Add(statusFilter, 3, 0);
            filterLayout.Controls.Add(filterBtn, 4, 0);
            filterLayout.Controls.Add(clearBtn, 5, 0);

            filterPanel.Controls.Add(filterLayout);

            transactionsTable = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RightToLeft = RightToLeft.Yes,
                Font = FontHelper.GetArabicFont(10F),
                ScrollBars = ScrollBars.Both
            };
            transactionsTable.Columns.Add("TransactionNumber", "رقم المعاملة");
            transactionsTable.Columns.Add("Title", "العنوان");
            transactionsTable.Columns.Add("Status", "الحالة");
            transactionsTable.Columns.Add("Department", "الجهة");
            transactionsTable.Columns.Add("CreatedAt", "تاريخ الإنشاء");
            transactionsTable.Columns.Add("UpdatedAt", "آخر تحديث");
            transactionsTable.Columns.Add("Actions", "الإجراءات");
            transactionsTable.CellClick += TransactionsTable_CellClick;
            transactionsTable.CellPainting += TransactionsTable_CellPainting;
            
            transactionsTable.Columns[0].Width = 120;
            transactionsTable.Columns[1].Width = 300;
            transactionsTable.Columns[2].Width = 150;
            transactionsTable.Columns[3].Width = 150;
            transactionsTable.Columns[4].Width = 220;
            transactionsTable.Columns[5].Width = 220;
            transactionsTable.Columns[6].Width = 500;
            
            // تطبيق المحاذاة الوسطى على جميع الأعمدة
            foreach (DataGridViewColumn col in transactionsTable.Columns)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.DefaultCellStyle.Font = FontHelper.GetArabicFont(10F);
                col.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
            }
            
            transactionsTable.ColumnHeadersDefaultCellStyle.Font = FontHelper.GetArabicFont(11F, FontStyle.Bold);
            transactionsTable.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            transactionsTable.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 58, 95);
            transactionsTable.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            transactionsTable.ColumnHeadersHeight = 50;
            transactionsTable.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            transactionsTable.DefaultCellStyle.Font = FontHelper.GetArabicFont(10F);
            transactionsTable.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);

            var logsGroup = new GroupBox
            {
                Text = "سجل حركة المعاملات",
                Dock = DockStyle.Fill,
                Font = FontHelper.GetArabicFont(12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 58, 95),
                Margin = new Padding(0, 15, 0, 0),
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(15),
                Height = 300
            };

            logsTable = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RightToLeft = RightToLeft.Yes,
                Font = FontHelper.GetArabicFont(10F),
                ScrollBars = ScrollBars.Both
            };
            logsTable.Columns.Add("TransactionNumber", "رقم المعاملة");
            logsTable.Columns.Add("ActionType", "نوع الإجراء");
            logsTable.Columns.Add("UserName", "المستخدم");
            logsTable.Columns.Add("CreatedAt", "التاريخ");
            logsTable.Columns.Add("Notes", "الملاحظات");
            
            logsTable.Columns[0].Width = 150;
            logsTable.Columns[1].Width = 150;
            logsTable.Columns[2].Width = 200;
            logsTable.Columns[3].Width = 180;
            logsTable.Columns[4].Width = 170;
            
            // تطبيق المحاذاة الوسطى على جميع الأعمدة
            foreach (DataGridViewColumn col in logsTable.Columns)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.DefaultCellStyle.Font = FontHelper.GetArabicFont(10F);
                col.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
            }
            
            logsTable.ColumnHeadersDefaultCellStyle.Font = FontHelper.GetArabicFont(11F, FontStyle.Bold);
            logsTable.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            logsTable.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 58, 95);
            logsTable.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            logsTable.ColumnHeadersHeight = 45;
            logsTable.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            logsTable.DefaultCellStyle.Font = FontHelper.GetArabicFont(10F);
            logsTable.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);

            logsGroup.Controls.Add(logsTable);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };

            layout.Controls.Add(toolbar, 0, 0);
            layout.Controls.Add(filterPanel, 0, 1);
            layout.Controls.Add(transactionsTable, 0, 2);
            layout.Controls.Add(logsGroup, 0, 3);

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            
            panel.Resize += (s, e) =>
            {
                var availableWidth = panel.ClientSize.Width - 60;
                if (availableWidth > 0)
                {
                    filterPanel.Width = Math.Max(1500, availableWidth);
                }
            };
            
            panel.HandleCreated += (s, e) =>
            {
                var availableWidth = panel.ClientSize.Width - 60;
                if (availableWidth > 0)
                {
                    filterPanel.Width = Math.Max(1500, availableWidth);
                }
            };

            panel.Controls.Add(layout);
            tab.Controls.Add(panel);
            
            LoadTransactionLogs();
        }

        private void CreateDropdownsTab()
        {
            var tab = tabs.TabPages[2];
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };

            var title = new Label
            {
                Text = "إدارة القوائم المنسدلة",
                Font = FontHelper.GetArabicFont(16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 58, 95),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 12),
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleRight
            };

            var infoLabel = new Label
            {
                Text = "يمكنك إدارة أنواع القوائم وعناصرها من هنا",
                Font = FontHelper.GetArabicFont(11F),
                ForeColor = Color.FromArgb(108, 117, 125),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 25),
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleRight
            };

            var manageBtn = new Button
            {
                Text = "⚙️ فتح إدارة القوائم",
                BackColor = Color.FromArgb(30, 58, 95),
                ForeColor = Color.White,
                Font = FontHelper.GetArabicFont(10F, FontStyle.Bold),
                Size = new Size(180, 40),
                FlatStyle = FlatStyle.Flat,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleCenter
            };
            manageBtn.FlatAppearance.BorderSize = 0;
            manageBtn.Click += ManageDropdowns_Click;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };

            layout.Controls.Add(title, 0, 0);
            layout.Controls.Add(infoLabel, 0, 1);
            layout.Controls.Add(manageBtn, 0, 2);

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            panel.Controls.Add(layout);
            tab.Controls.Add(panel);
        }

        private void CreateUsersTab()
        {
            var tab = tabs.TabPages[3];
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };

            var title = new Label
            {
                Text = "إدارة المستخدمين",
                Font = FontHelper.GetArabicFont(16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 58, 95),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 12),
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleRight
            };

            var infoLabel = new Label
            {
                Text = "يمكنك إدارة المستخدمين وإضافة مستخدمين جدد من هنا",
                Font = FontHelper.GetArabicFont(11F),
                ForeColor = Color.FromArgb(108, 117, 125),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 25),
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleRight
            };

            var manageBtn = new Button
            {
                Text = "👥 فتح إدارة المستخدمين",
                BackColor = Color.FromArgb(30, 58, 95),
                ForeColor = Color.White,
                Font = FontHelper.GetArabicFont(10F, FontStyle.Bold),
                Size = new Size(350, 40),
                FlatStyle = FlatStyle.Flat,
                RightToLeft = RightToLeft.Yes,
                TextAlign = ContentAlignment.MiddleCenter
            };
            manageBtn.FlatAppearance.BorderSize = 0;
            manageBtn.Click += ManageUsers_Click;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };

            layout.Controls.Add(title, 0, 0);
            layout.Controls.Add(infoLabel, 0, 1);
            layout.Controls.Add(manageBtn, 0, 2);

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            panel.Controls.Add(layout);
            tab.Controls.Add(panel);
        }

        private void LoadDashboardData()
        {
            var conn = dbManager.GetLocalConnection();

            using var totalCmd = new SQLiteCommand("SELECT COUNT(*) FROM transactions WHERE is_deleted = 0", conn);
            var total = Convert.ToInt32(totalCmd.ExecuteScalar());
            UpdateStatCard(totalCard, total.ToString());

            using var pendingCmd = new SQLiteCommand(@"
                SELECT COUNT(*) FROM transactions t
                INNER JOIN dropdown_items di ON t.status_id = di.id
                INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id
                WHERE dt.name = 'status' AND di.name = 'pending' AND t.is_deleted = 0", conn);
            var pending = Convert.ToInt32(pendingCmd.ExecuteScalar());
            UpdateStatCard(pendingCard, pending.ToString());

            using var inProgressCmd = new SQLiteCommand(@"
                SELECT COUNT(*) FROM transactions t
                INNER JOIN dropdown_items di ON t.status_id = di.id
                INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id
                WHERE dt.name = 'status' AND di.name = 'in_progress' AND t.is_deleted = 0", conn);
            var inProgress = Convert.ToInt32(inProgressCmd.ExecuteScalar());
            UpdateStatCard(inProgressCard, inProgress.ToString());

            using var completedCmd = new SQLiteCommand(@"
                SELECT COUNT(*) FROM transactions t
                INNER JOIN dropdown_items di ON t.status_id = di.id
                INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id
                WHERE dt.name = 'status' AND di.name = 'completed' AND t.is_deleted = 0", conn);
            var completed = Convert.ToInt32(completedCmd.ExecuteScalar());
            UpdateStatCard(completedCard, completed.ToString());

            LoadRecentTransactions();
            LoadTransactions();
        }

        private void UpdateStatCard(Panel card, string value)
        {
            if (card == null) return;
            
            try
            {
                // تحديث القيمة في Tag
                if (card.Tag is System.Tuple<string, string> data)
                {
                    card.Tag = new System.Tuple<string, string>(data.Item1, value ?? "0");
                }
                else
                {
                    // إذا لم يكن هناك Tag، أنشئ واحداً جديداً
                    card.Tag = new System.Tuple<string, string>("", value ?? "0");
                }

                // إجبار إعادة الرسم
                card.Invalidate();
                card.Update();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating stat card: {ex.Message}");
            }
        }

        private void LoadRecentTransactions()
        {
            recentTable.Rows.Clear();
            var conn = dbManager.GetLocalConnection();

            try
            {
                using var cmd = new SQLiteCommand(@"
                    SELECT t.id, t.transaction_number, t.title, 
                           (SELECT di.display_name FROM dropdown_items di 
                            INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id 
                            WHERE di.id = t.status_id AND dt.name = 'status') as status_name,
                           t.created_at
                    FROM transactions t
                    WHERE t.is_deleted = 0
                    ORDER BY t.created_at DESC
                    LIMIT 5", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var transactionId = Convert.ToInt32(reader["id"]);
                    var row = recentTable.Rows.Add(
                        reader["transaction_number"],
                        reader["title"],
                        reader["status_name"] ?? (object)"-",
                        reader["created_at"],
                        "" // Actions column - empty, we'll add buttons via CellPainting
                    );
                    recentTable.Rows[row].Tag = transactionId;
                }
            }
            catch
            {
                using var cmd = new SQLiteCommand(@"
                    SELECT id, transaction_number, title, created_at
                    FROM transactions
                    WHERE is_deleted = 0
                    ORDER BY created_at DESC
                    LIMIT 5", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var transactionId = Convert.ToInt32(reader["id"]);
                    var row = recentTable.Rows.Add(
                        reader["transaction_number"],
                        reader["title"],
                        "-",
                        reader["created_at"],
                        "" // Actions column
                    );
                    recentTable.Rows[row].Tag = transactionId;
                }
            }
        }

        private void LoadTransactions()
        {
            transactionsTable.Rows.Clear();
            var conn = dbManager.GetLocalConnection();

            var search = searchInput?.Text.Trim() ?? "";
            var statusFilterValue = statusFilter?.SelectedItem is ComboBoxItem item ? item.Value : null;

            var whereClauses = new System.Collections.Generic.List<string> { "t.is_deleted = 0" };
            var parameters = new System.Collections.Generic.List<SQLiteParameter>();

            if (!string.IsNullOrEmpty(search))
            {
                whereClauses.Add("(t.transaction_number LIKE @search OR t.title LIKE @search)");
                parameters.Add(new SQLiteParameter("@search", $"%{search}%"));
            }

            if (statusFilterValue != null)
            {
                whereClauses.Add("t.status_id = @statusId");
                parameters.Add(new SQLiteParameter("@statusId", statusFilterValue));
            }

            var whereSql = string.Join(" AND ", whereClauses);

            try
            {
                using var cmd = new SQLiteCommand($@"
                    SELECT t.id, t.transaction_number, t.title, 
                           (SELECT di.display_name FROM dropdown_items di 
                            INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id 
                            WHERE di.id = t.status_id AND dt.name = 'status') as status_name,
                           (SELECT di.display_name FROM dropdown_items di 
                            INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id 
                            WHERE di.id = t.department_id AND dt.name = 'department') as department_name,
                           t.created_at, t.updated_at
                    FROM transactions t
                    WHERE {whereSql}
                    ORDER BY t.created_at DESC
                    LIMIT 100", conn);

                foreach (var param in parameters)
                    cmd.Parameters.Add(param);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var row = transactionsTable.Rows.Add(
                        reader["transaction_number"],
                        reader["title"],
                        reader["status_name"] ?? (object)"-",
                        reader["department_name"] ?? (object)"-",
                        reader["created_at"],
                        reader["updated_at"]
                    );

                    var transactionId = Convert.ToInt32(reader["id"]);
                    transactionsTable.Rows[row].Tag = transactionId;
                    transactionsTable.Rows[row].Cells[6].Value = ""; // Actions column - empty, we'll add buttons via CellPainting
                    
                    // تطبيق المحاذاة الوسطى على جميع الخلايا
                    for (int i = 0; i < transactionsTable.Columns.Count; i++)
                    {
                        transactionsTable.Rows[row].Cells[i].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }
                }
            }
            catch
            {
                using var cmd = new SQLiteCommand($@"
                    SELECT id, transaction_number, title, created_at, updated_at
                    FROM transactions
                    WHERE {whereSql}
                    ORDER BY created_at DESC
                    LIMIT 100", conn);

                foreach (var param in parameters)
                    cmd.Parameters.Add(param);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var row = transactionsTable.Rows.Add(
                        reader["transaction_number"],
                        reader["title"],
                        "-",
                        "-",
                        reader["created_at"],
                        reader["updated_at"],
                        "عرض"
                    );
                    var transactionId = Convert.ToInt32(reader["id"]);
                    transactionsTable.Rows[row].Tag = transactionId;
                    transactionsTable.Rows[row].Cells[6].Value = ""; // Actions column
                    
                    // تطبيق المحاذاة الوسطى على جميع الخلايا
                    for (int i = 0; i < transactionsTable.Columns.Count; i++)
                    {
                        transactionsTable.Rows[row].Cells[i].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }
                }
            }

        }
        
        private void RecentTable_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 4) return;

            var row = recentTable.Rows[e.RowIndex];
            var transactionId = row.Tag as int?;
            
            if (transactionId == null)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                e.Handled = true;
                return;
            }

            e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
            
            var cellBounds = e.CellBounds;
            var buttonWidth = 100;
            var buttonHeight = 32;
            var spacing = 6;
            var totalWidth = buttonWidth + spacing + buttonWidth;
            
            var startX = cellBounds.Left + (cellBounds.Width - totalWidth) / 2;
            var startY = cellBounds.Top + (cellBounds.Height - buttonHeight) / 2;
            
            if (startX < cellBounds.Left + 2) startX = cellBounds.Left + 2;
            if (startX + totalWidth > cellBounds.Right - 2) startX = cellBounds.Right - totalWidth - 2;
            if (startY < cellBounds.Top + 2) startY = cellBounds.Top + 2;
            if (startY + buttonHeight > cellBounds.Bottom - 2) startY = cellBounds.Bottom - buttonHeight - 2;

            // View button (rightmost)
            var viewRect = new Rectangle(startX, startY, buttonWidth, buttonHeight);
            using (var brush = new SolidBrush(Color.FromArgb(52, 152, 219)))
            {
                e.Graphics.FillRectangle(brush, viewRect);
            }
            using (var pen = new Pen(Color.White, 1))
            {
                e.Graphics.DrawRectangle(pen, viewRect);
            }
            TextRenderer.DrawText(e.Graphics, "👁️ عرض", FontHelper.GetArabicFont(10F, FontStyle.Bold),
                viewRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);

            // Edit button (leftmost)
            startX += buttonWidth + spacing;
            var editRect = new Rectangle(startX, startY, buttonWidth, buttonHeight);
            using (var brush = new SolidBrush(Color.FromArgb(44, 82, 130)))
            {
                e.Graphics.FillRectangle(brush, editRect);
            }
            using (var pen = new Pen(Color.White, 1))
            {
                e.Graphics.DrawRectangle(pen, editRect);
            }
            TextRenderer.DrawText(e.Graphics, "✏️ تعديل", FontHelper.GetArabicFont(10F, FontStyle.Bold),
                editRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);

            e.Handled = true;
        }

        private void RecentTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 4) return;

            var row = recentTable.Rows[e.RowIndex];
            var transactionId = row.Tag as int?;
            
            if (transactionId == null) return;

            var cellBounds = recentTable.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            var buttonWidth = 100;
            var spacing = 6;
            var totalWidth = buttonWidth + spacing + buttonWidth;
            
            var startX = cellBounds.Left + (cellBounds.Width - totalWidth) / 2;
            if (startX < cellBounds.Left + 2) startX = cellBounds.Left + 2;
            if (startX + totalWidth > cellBounds.Right - 2) startX = cellBounds.Right - totalWidth - 2;

            var mousePos = recentTable.PointToClient(Control.MousePosition);
            var relativeX = mousePos.X - cellBounds.X;
            var relativeStartX = startX - cellBounds.X;

            if (relativeX >= relativeStartX && relativeX <= relativeStartX + buttonWidth)
            {
                ViewTransaction(transactionId.Value);
            }
            else if (relativeX >= relativeStartX + buttonWidth + spacing && relativeX <= relativeStartX + buttonWidth + spacing + buttonWidth)
            {
                if (role == "admin")
                {
                    var form = new TransactionForm(dbManager, userId, transactionId, this);
                    form.Saved += () => { LoadRecentTransactions(); LoadDashboardData(); LoadTransactionLogs(); };
                    form.ShowDialog();
                }
            }
        }

        private void LoadStatusFilter()
        {
            if (statusFilter == null) return;
            
            // منع إعادة تحميل القائمة إذا كانت موجودة بالفعل
            if (statusFilter.Items.Count > 1) return;

            var conn = dbManager.GetLocalConnection();
            try
            {
                using var cmd = new SQLiteCommand(@"
                    SELECT di.id, di.display_name
                    FROM dropdown_items di
                    INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id
                    WHERE dt.name = 'status' AND di.is_active = 1
                    ORDER BY di.display_order", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    statusFilter.Items.Add(new ComboBoxItem(reader["display_name"].ToString(), Convert.ToInt32(reader["id"])));
                }
            }
            catch { }
        }

        private void ClearFilters_Click(object sender, EventArgs e)
        {
            searchInput.Clear();
            statusFilter.SelectedIndex = 0;
            LoadTransactions();
        }

        private void AddTransaction_Click(object sender, EventArgs e)
        {
            var form = new TransactionForm(dbManager, userId, null, this);
            form.Saved += () => { LoadTransactions(); LoadDashboardData(); LoadTransactionLogs(); };
            form.ShowDialog();
        }

        private void TransactionsTable_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 6) return;

            var row = transactionsTable.Rows[e.RowIndex];
            var transactionId = row.Tag as int?;
            
            if (transactionId == null)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                e.Handled = true;
                return;
            }

            e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
            
            var cellBounds = e.CellBounds;
            var buttonWidth = 100;
            var buttonHeight = 32;
            var spacing = 6;
            var totalWidth = buttonWidth + spacing + buttonWidth;
            
            var startX = cellBounds.Left + (cellBounds.Width - totalWidth) / 2;
            var startY = cellBounds.Top + (cellBounds.Height - buttonHeight) / 2;
            
            if (startX < cellBounds.Left + 2) startX = cellBounds.Left + 2;
            if (startX + totalWidth > cellBounds.Right - 2) startX = cellBounds.Right - totalWidth - 2;
            if (startY < cellBounds.Top + 2) startY = cellBounds.Top + 2;
            if (startY + buttonHeight > cellBounds.Bottom - 2) startY = cellBounds.Bottom - buttonHeight - 2;

            // View button (rightmost)
            var viewRect = new Rectangle(startX, startY, buttonWidth, buttonHeight);
            using (var brush = new SolidBrush(Color.FromArgb(52, 152, 219)))
            {
                e.Graphics.FillRectangle(brush, viewRect);
            }
            using (var pen = new Pen(Color.White, 1))
            {
                e.Graphics.DrawRectangle(pen, viewRect);
            }
            TextRenderer.DrawText(e.Graphics, "👁️ عرض", FontHelper.GetArabicFont(10F, FontStyle.Bold),
                viewRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);

            // Edit button (leftmost)
            startX += buttonWidth + spacing;
            var editRect = new Rectangle(startX, startY, buttonWidth, buttonHeight);
            using (var brush = new SolidBrush(Color.FromArgb(44, 82, 130)))
            {
                e.Graphics.FillRectangle(brush, editRect);
            }
            using (var pen = new Pen(Color.White, 1))
            {
                e.Graphics.DrawRectangle(pen, editRect);
            }
            TextRenderer.DrawText(e.Graphics, "✏️ تعديل", FontHelper.GetArabicFont(10F, FontStyle.Bold),
                editRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.RightToLeft);

            e.Handled = true;
        }

        private void TransactionsTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 6) return;

            var row = transactionsTable.Rows[e.RowIndex];
            var transactionId = row.Tag as int?;
            
            if (transactionId == null) return;

            var cellBounds = transactionsTable.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            var buttonWidth = 100;
            var spacing = 6;
            var totalWidth = buttonWidth + spacing + buttonWidth;
            
            var startX = cellBounds.Left + (cellBounds.Width - totalWidth) / 2;
            if (startX < cellBounds.Left + 2) startX = cellBounds.Left + 2;
            if (startX + totalWidth > cellBounds.Right - 2) startX = cellBounds.Right - totalWidth - 2;

            var mousePos = transactionsTable.PointToClient(Control.MousePosition);
            var relativeX = mousePos.X - cellBounds.X;
            var relativeStartX = startX - cellBounds.X;

            if (relativeX >= relativeStartX && relativeX <= relativeStartX + buttonWidth)
            {
                ViewTransaction(transactionId.Value);
            }
            else if (relativeX >= relativeStartX + buttonWidth + spacing && relativeX <= relativeStartX + buttonWidth + spacing + buttonWidth)
            {
                if (role == "admin")
                {
                    var form = new TransactionForm(dbManager, userId, transactionId, this);
                    form.Saved += () => { LoadTransactions(); LoadDashboardData(); LoadTransactionLogs(); };
                    form.ShowDialog();
                }
            }
        }

        private void ViewTransaction(int transactionId)
        {
            var dialog = new TransactionViewDialog(dbManager, transactionId, userId, role, this);
            dialog.Saved += () => { LoadTransactions(); LoadDashboardData(); LoadTransactionLogs(); };
            dialog.ShowDialog();
        }

        private void ExportTransactions_Click(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                FileName = "المعاملات.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var conn = dbManager.GetLocalConnection();
                using var cmd = new SQLiteCommand(@"
                    SELECT t.transaction_number, t.title, 
                           (SELECT di.display_name FROM dropdown_items di 
                            INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id 
                            WHERE di.id = t.status_id AND dt.name = 'status') as status_name,
                           t.created_at
                    FROM transactions t
                    WHERE t.is_deleted = 0
                    ORDER BY t.created_at DESC", conn);

                var csv = new System.Text.StringBuilder();
                csv.AppendLine("رقم المعاملة,العنوان,الحالة,تاريخ الإنشاء");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    csv.AppendLine($"{reader["transaction_number"]},{reader["title"]},{reader["status_name"] ?? "-"},{reader["created_at"]}");
                }

                System.IO.File.WriteAllText(sfd.FileName, csv.ToString(), System.Text.Encoding.UTF8);
                MessageBox.Show("تم تصدير البيانات بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        private void LoadTransactionLogs()
        {
            if (logsTable == null) return;
            
            logsTable.Rows.Clear();
            var conn = dbManager.GetLocalConnection();

            using var cmd = new SQLiteCommand(@"
                SELECT 
                    tl.*,
                    t.transaction_number,
                    u.full_name as user_name
                FROM transaction_logs tl
                INNER JOIN transactions t ON tl.transaction_id = t.id
                INNER JOIN users u ON tl.user_id = u.id
                ORDER BY tl.created_at DESC
                LIMIT 100", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var actionType = reader["action_type"].ToString();
                var actionTypeAr = actionType switch
                {
                    "created" => "إنشاء",
                    "updated" => "تعديل",
                    "deleted" => "حذف",
                    _ => actionType
                };

                var row = logsTable.Rows.Add(
                    reader["transaction_number"],
                    actionTypeAr,
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

        private string SafeDateToString(object dateValue)
        {
            if (dateValue == null || dateValue == DBNull.Value)
                return "-";

            try
            {
                if (dateValue is DateTime dt)
                {
                    if (dt.Year < 1900 || dt.Year > 2077) return "-";
                    return dt.ToString("yyyy-MM-dd HH:mm");
                }

                if (dateValue is string dateStr && !string.IsNullOrWhiteSpace(dateStr))
                {
                    if (DateTime.TryParse(dateStr, out var parsedDate))
                    {
                        if (parsedDate.Year < 1900 || parsedDate.Year > 2077) return "-";
                        return parsedDate.ToString("yyyy-MM-dd HH:mm");
                    }
                }

                if (dateValue is long ticks)
                {
                    var dateTimeFromTicks = new DateTime(ticks);
                    if (dateTimeFromTicks.Year < 1900 || dateTimeFromTicks.Year > 2077) return "-";
                    return dateTimeFromTicks.ToString("yyyy-MM-dd HH:mm");
                }

                return dateValue.ToString() ?? "-";
            }
            catch
            {
                return "-";
            }
        }

        private void LogoutBtn_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("هل تريد تسجيل الخروج؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                this.Close();
        }

        private void ManageDropdowns_Click(object sender, EventArgs e)
        {
            var dialog = new DropdownsManagerDialog(dbManager, this);
            dialog.Saved += () => LoadDashboardData();
            dialog.ShowDialog();
        }

        private void ManageUsers_Click(object sender, EventArgs e)
        {
            var dialog = new UsersManagerDialog(dbManager, userId, this);
            dialog.Saved += () => LoadDashboardData();
            dialog.ShowDialog();
        }
    }

    public class ComboBoxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }

        public ComboBoxItem(string text, object value)
        {
            Text = text;
            Value = value;
        }

        public override string ToString() => Text;
    }
}

