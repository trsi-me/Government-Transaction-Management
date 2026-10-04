using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace STOT.Config
{
    public class DatabaseManager
    {
        private string localDbPath;
        private SQLiteConnection localConn;

        public DatabaseManager()
        {
            string userProfile = null;
            
            try
            {
                userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            catch { }
            
            if (string.IsNullOrWhiteSpace(userProfile))
            {
                try
                {
                    userProfile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                }
                catch { }
            }
            
            if (string.IsNullOrWhiteSpace(userProfile))
            {
                try
                {
                    userProfile = Path.GetTempPath();
                    if (!string.IsNullOrWhiteSpace(userProfile))
                        userProfile = userProfile.TrimEnd('\\', '/');
                }
                catch { }
            }
            
            if (string.IsNullOrWhiteSpace(userProfile))
            {
                try
                {
                    userProfile = Environment.CurrentDirectory;
                }
                catch { }
            }
            
            if (string.IsNullOrWhiteSpace(userProfile))
            {
                try
                {
                    userProfile = AppDomain.CurrentDomain.BaseDirectory;
                }
                catch { }
            }
            
            if (!string.IsNullOrWhiteSpace(userProfile))
            {
                try
                {
                    localDbPath = Path.Combine(userProfile, "STOT", "local.db");
                    if (!string.IsNullOrWhiteSpace(localDbPath))
                    {
                        EnsureLocalDbDir();
                    }
                }
                catch { }
            }
        }

        private void EnsureLocalDbDir()
        {
            if (string.IsNullOrWhiteSpace(localDbPath))
            {
                var userProfile = GetUserProfilePath();
                
                if (string.IsNullOrWhiteSpace(userProfile))
                    throw new InvalidOperationException("لا يمكن تحديد مسار قاعدة البيانات المحلية");
                
                try
                {
                    localDbPath = Path.Combine(userProfile, "STOT", "local.db");
                    if (string.IsNullOrWhiteSpace(localDbPath))
                        throw new InvalidOperationException("مسار قاعدة البيانات المحلية غير صالح");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"فشل في إنشاء مسار قاعدة البيانات المحلية: {ex.Message}", ex);
                }
            }
            
            string dbDir = null;
            try
            {
                dbDir = Path.GetDirectoryName(localDbPath);
            }
            catch { }
            
            if (string.IsNullOrWhiteSpace(dbDir))
            {
                var userProfile = GetUserProfilePath();
                
                // التأكد النهائي من أن userProfile ليس null أو فارغ
                if (string.IsNullOrWhiteSpace(userProfile))
                {
                    userProfile = "C:\\STOT";
                }
                
                try
                {
                    // استخدام طريقة آمنة لبناء المسار بدون Path.Combine
                    if (userProfile.EndsWith("\\") || userProfile.EndsWith("/"))
                    {
                        dbDir = userProfile + "TrackingTransactions";
                    }
                    else
                    {
                        dbDir = userProfile + "\\TrackingTransactions";
                    }
                    
                    // التأكد من أن dbDir صالح
                    if (string.IsNullOrWhiteSpace(dbDir))
                    {
                        dbDir = "C:\\STOT\\TrackingTransactions";
                    }
                }
                catch (Exception ex)
                {
                    // في حالة أي خطأ، استخدم مسار افتراضي
                    dbDir = "C:\\STOT\\TrackingTransactions";
                }
            }
            
            if (string.IsNullOrWhiteSpace(dbDir))
                throw new InvalidOperationException("مسار مجلد قاعدة البيانات غير صالح");
            
            if (!Directory.Exists(dbDir))
            {
                try
                {
                    Directory.CreateDirectory(dbDir);
                }
                catch (Exception ex)
                {
                    throw new Exception($"فشل في إنشاء مجلد قاعدة البيانات: {ex.Message}", ex);
                }
            }
        }


        private string GetUserProfilePath()
        {
            string userProfile = string.Empty;
            
            // محاولة 1: UserProfile
            try
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(Path.GetDirectoryName(path)))
                {
                    userProfile = path;
                }
            }
            catch { }
            
            // محاولة 2: ApplicationData
            if (string.IsNullOrWhiteSpace(userProfile))
            {
                try
                {
                    var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        userProfile = path;
                    }
                }
                catch { }
            }
            
            // محاولة 3: LocalApplicationData
            if (string.IsNullOrWhiteSpace(userProfile))
            {
                try
                {
                    var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        userProfile = path;
                    }
                }
                catch { }
            }
            
            // محاولة 4: TempPath
            if (string.IsNullOrWhiteSpace(userProfile))
            {
                try
                {
                    var tempPath = Path.GetTempPath();
                    if (!string.IsNullOrWhiteSpace(tempPath))
                    {
                        userProfile = tempPath.TrimEnd('\\', '/');
                    }
                }
                catch { }
            }
            
            // محاولة 5: CurrentDirectory
            if (string.IsNullOrWhiteSpace(userProfile))
            {
                try
                {
                    var path = Environment.CurrentDirectory;
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        userProfile = path;
                    }
                }
                catch { }
            }
            
            // محاولة 6: BaseDirectory
            if (string.IsNullOrWhiteSpace(userProfile))
            {
                try
                {
                    var path = AppDomain.CurrentDomain.BaseDirectory;
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        userProfile = path;
                    }
                }
                catch { }
            }
            
            // محاولة 7: Windows folder
            if (string.IsNullOrWhiteSpace(userProfile))
            {
                try
                {
                    var path = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        userProfile = path;
                    }
                }
                catch { }
            }
            
            // إذا فشل كل شيء، استخدم مسار افتراضي مضمون
            if (string.IsNullOrWhiteSpace(userProfile))
            {
                try
                {
                    // استخدام C:\Users\Public كمسار افتراضي مضمون
                    var publicPath = Path.Combine("C:", "Users", "Public");
                    if (Directory.Exists(publicPath))
                    {
                        userProfile = publicPath;
                    }
                    else
                    {
                        // إذا لم يكن موجود، استخدم C:\
                        userProfile = "C:\\";
                    }
                }
                catch
                {
                    // آخر حل: استخدام مسار مطلق
                    userProfile = "C:\\STOT";
                }
            }
            
            // التأكد النهائي من أن القيمة ليست null أو فارغة
            if (string.IsNullOrWhiteSpace(userProfile))
            {
                userProfile = "C:\\STOT";
            }
            
            return userProfile.Trim();
        }

        public SQLiteConnection GetLocalConnection()
        {
            // التأكد من أن localDbPath محدد بشكل صحيح
            if (string.IsNullOrEmpty(localDbPath))
            {
                var userProfile = GetUserProfilePath();
                
                // التأكد النهائي من أن userProfile ليس null أو فارغ
                if (string.IsNullOrWhiteSpace(userProfile))
                {
                    userProfile = "C:\\STOT";
                }
                
                try
                {
                    // استخدام طريقة آمنة لبناء المسار
                    string dbDir;
                    if (userProfile.EndsWith("\\") || userProfile.EndsWith("/"))
                    {
                        dbDir = userProfile + "TrackingTransactions";
                    }
                    else
                    {
                        dbDir = userProfile + "\\TrackingTransactions";
                    }
                    
                    // التأكد من أن dbDir صالح
                    if (string.IsNullOrWhiteSpace(dbDir))
                    {
                        dbDir = "C:\\STOT\\TrackingTransactions";
                    }
                    
                    // بناء مسار قاعدة البيانات
                    if (dbDir.EndsWith("\\") || dbDir.EndsWith("/"))
                    {
                        localDbPath = dbDir + "local.db";
                    }
                    else
                    {
                        localDbPath = dbDir + "\\local.db";
                    }
                    
                    // التأكد النهائي من أن localDbPath صالح
                    if (string.IsNullOrWhiteSpace(localDbPath))
                    {
                        localDbPath = "C:\\STOT\\TrackingTransactions\\local.db";
                    }
                }
                catch (Exception ex)
                {
                    // في حالة أي خطأ، استخدم مسار افتراضي
                    localDbPath = "C:\\STOT\\TrackingTransactions\\local.db";
                }
            }
            
            // التأكد النهائي من أن localDbPath صالح
            if (string.IsNullOrWhiteSpace(localDbPath))
            {
                localDbPath = "C:\\STOT\\TrackingTransactions\\local.db";
            }
            
            // إنشاء المجلد إذا لم يكن موجوداً
            try
            {
                var dbDir = Path.GetDirectoryName(localDbPath);
                if (!string.IsNullOrWhiteSpace(dbDir) && !Directory.Exists(dbDir))
                {
                    Directory.CreateDirectory(dbDir);
                }
            }
            catch { }
            
            // محاولة الاتصال مع إعادة المحاولة
            int maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    if (localConn == null || localConn.State != ConnectionState.Open)
                    {
                        if (localConn != null)
                        {
                            try { localConn.Close(); } catch { }
                            localConn.Dispose();
                            localConn = null;
                        }
                        
                        localConn = new SQLiteConnection($"Data Source={localDbPath};Version=3;");
                        localConn.Open();
                        InitLocalDatabase();
                        return localConn;
                    }
                    else
                    {
                        return localConn;
                    }
                }
                catch (Exception ex)
                {
                    if (i == maxRetries - 1)
                    {
                        // في المحاولة الأخيرة، رمي الاستثناء
                        throw new Exception($"فشل في الاتصال بقاعدة البيانات المحلية بعد {maxRetries} محاولات: {ex.Message}", ex);
                    }
                    // انتظر قليلاً قبل إعادة المحاولة
                    System.Threading.Thread.Sleep(100);
                }
            }
            
            return localConn;
        }


        private void InitLocalDatabase()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrEmpty(baseDir))
                baseDir = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            if (string.IsNullOrEmpty(baseDir))
                baseDir = Environment.CurrentDirectory;
            
            var sqlFile = Path.Combine(baseDir, "database", "schema.sqlite.sql");
            if (File.Exists(sqlFile))
            {
                try
                {
                    var sql = File.ReadAllText(sqlFile);
                    using var command = new SQLiteCommand(sql, localConn);
                    command.ExecuteNonQuery();
                }
                catch
                {
                    CreateLocalTables();
                }
            }
            else
            {
                CreateLocalTables();
            }
        }

        private void CreateLocalTables()
        {
            var commands = new[]
            {
                @"CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT UNIQUE NOT NULL,
                    password TEXT NOT NULL,
                    full_name TEXT NOT NULL,
                    role TEXT NOT NULL DEFAULT 'user',
                    is_active INTEGER DEFAULT 1,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )",
                @"CREATE TABLE IF NOT EXISTS dropdown_types (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT UNIQUE NOT NULL,
                    display_name TEXT NOT NULL,
                    description TEXT,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )",
                @"CREATE TABLE IF NOT EXISTS dropdown_items (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    dropdown_type_id INTEGER NOT NULL,
                    name TEXT NOT NULL,
                    display_name TEXT NOT NULL,
                    display_order INTEGER DEFAULT 0,
                    is_active INTEGER DEFAULT 1,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (dropdown_type_id) REFERENCES dropdown_types(id) ON DELETE CASCADE,
                    UNIQUE(dropdown_type_id, name)
                )",
                @"CREATE TABLE IF NOT EXISTS transactions (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    transaction_number TEXT UNIQUE NOT NULL,
                    title TEXT NOT NULL,
                    description TEXT,
                    department_id INTEGER,
                    source_department_id INTEGER,
                    concerned_departments TEXT,
                    status_id INTEGER,
                    priority_id INTEGER,
                    transaction_type_id INTEGER,
                    follow_up_type_id INTEGER,
                    reference_number TEXT,
                    notification_number TEXT,
                    next_follow_up_date DATE,
                    next_follow_up_date_hijri TEXT,
                    notes TEXT,
                    requests_count INTEGER DEFAULT 0,
                    is_deleted INTEGER DEFAULT 0,
                    is_suspended INTEGER DEFAULT 0,
                    created_by INTEGER NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (created_by) REFERENCES users(id),
                    FOREIGN KEY (department_id) REFERENCES dropdown_items(id),
                    FOREIGN KEY (source_department_id) REFERENCES dropdown_items(id),
                    FOREIGN KEY (status_id) REFERENCES dropdown_items(id),
                    FOREIGN KEY (priority_id) REFERENCES dropdown_items(id),
                    FOREIGN KEY (transaction_type_id) REFERENCES dropdown_items(id),
                    FOREIGN KEY (follow_up_type_id) REFERENCES dropdown_items(id)
                )",
                @"CREATE TABLE IF NOT EXISTS transaction_logs (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    transaction_id INTEGER NOT NULL,
                    user_id INTEGER NOT NULL,
                    action_type TEXT NOT NULL,
                    notes TEXT,
                    old_value TEXT,
                    new_value TEXT,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (transaction_id) REFERENCES transactions(id) ON DELETE CASCADE,
                    FOREIGN KEY (user_id) REFERENCES users(id)
                )",
                @"CREATE TABLE IF NOT EXISTS transaction_attachments (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    transaction_id INTEGER NOT NULL,
                    file_name TEXT NOT NULL,
                    file_path TEXT NOT NULL,
                    file_type TEXT,
                    file_size INTEGER,
                    uploaded_by INTEGER NOT NULL,
                    uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (transaction_id) REFERENCES transactions(id) ON DELETE CASCADE,
                    FOREIGN KEY (uploaded_by) REFERENCES users(id)
                )",
                @"CREATE TABLE IF NOT EXISTS sync_log (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    table_name TEXT NOT NULL,
                    record_id INTEGER NOT NULL,
                    action TEXT NOT NULL,
                    synced_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    UNIQUE(table_name, record_id, action)
                )"
            };

            foreach (var cmd in commands)
            {
                using var command = new SQLiteCommand(cmd, localConn);
                command.ExecuteNonQuery();
            }

            InitDefaultData();
        }

        private void InitDefaultData()
        {
            using var checkCmd = new SQLiteCommand("SELECT COUNT(*) FROM users WHERE username = 'admin'", localConn);
            var exists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

            if (!exists)
            {
                var passwordHash = System.Security.Cryptography.MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes("password"));
                var hashString = BitConverter.ToString(passwordHash).Replace("-", "").ToLower();
                using var insertCmd = new SQLiteCommand("INSERT INTO users (username, password, full_name, role) VALUES ('admin', @pass, 'مدير النظام', 'admin')", localConn);
                insertCmd.Parameters.AddWithValue("@pass", hashString);
                insertCmd.ExecuteNonQuery();
            }

            InitDefaultDropdowns();
        }

        private void InitDefaultDropdowns()
        {
            var types = new[]
            {
                ("status", "حالة المعاملة", "حالات المعاملات المختلفة"),
                ("priority", "الأولوية", "مستويات الأولوية"),
                ("department", "القسم/الجهة", "الأقسام والجهات"),
                ("transaction_category", "نوع المعاملة", "أنواع المعاملات"),
                ("follow_up_type", "نوع المتابعة", "أنواع المتابعة")
            };

            var typeIds = new Dictionary<string, int>();

            foreach (var (name, displayName, description) in types)
            {
                using var cmd = new SQLiteCommand("INSERT OR IGNORE INTO dropdown_types (name, display_name, description) VALUES (@name, @display, @desc)", localConn);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@display", displayName);
                cmd.Parameters.AddWithValue("@desc", description);
                cmd.ExecuteNonQuery();

                using var selectCmd = new SQLiteCommand("SELECT id FROM dropdown_types WHERE name = @name", localConn);
                selectCmd.Parameters.AddWithValue("@name", name);
                var result = selectCmd.ExecuteScalar();
                if (result != null)
                    typeIds[name] = Convert.ToInt32(result);
            }

            var items = new[]
            {
                ("status", "pending", "قيد الانتظار", 1),
                ("status", "in_progress", "قيد المعالجة", 2),
                ("status", "completed", "مكتملة", 3),
                ("status", "cancelled", "ملغاة", 4),
                ("priority", "low", "منخفضة", 1),
                ("priority", "medium", "متوسطة", 2),
                ("priority", "high", "عالية", 3),
                ("priority", "urgent", "عاجلة", 4),
                ("department", "archive", "إدارة الأرشيف", 1),
                ("department", "technical_support", "الدعم الفني", 2),
                ("department", "requests_management", "إدارة الطلبات", 3),
                ("department", "finance_ministry", "وزارة المالية", 4),
                ("department", "interior_ministry", "وزارة الداخلية", 5),
                ("department", "defense_ministry", "وزارة الدفاع", 6),
                ("department", "foreign_ministry", "وزارة الخارجية", 7),
                ("department", "education_ministry", "وزارة التعليم", 8),
                ("department", "health_ministry", "وزارة الصحة", 9),
                ("department", "commerce_ministry", "وزارة التجارة", 10),
                ("department", "energy_ministry", "وزارة الطاقة", 11),
                ("department", "transport_ministry", "وزارة النقل", 12),
                ("department", "housing_ministry", "وزارة الإسكان", 13),
                ("department", "municipal_affairs", "وزارة الشؤون البلدية", 14),
                ("department", "labor_ministry", "وزارة العمل", 15),
                ("department", "justice_ministry", "وزارة العدل", 16),
                ("department", "culture_ministry", "وزارة الثقافة", 17),
                ("department", "sports_ministry", "وزارة الرياضة", 18),
                ("department", "tourism_ministry", "وزارة السياحة", 19),
                ("department", "environment_ministry", "وزارة البيئة", 20),
                ("department", "communications_ministry", "وزارة الاتصالات", 21),
                ("department", "hr", "الموارد البشرية", 22),
                ("department", "it", "تقنية المعلومات", 23),
                ("department", "legal", "الشؤون القانونية", 24),
                ("transaction_category", "inquiry", "استفسار", 1),
                ("transaction_category", "note", "ملاحظة", 2),
                ("transaction_category", "request", "طلب", 3),
                ("follow_up_type", "first_followup", "تعقيب أول", 1),
                ("follow_up_type", "second_followup", "تعقيب ثاني", 2),
                ("follow_up_type", "notification", "إشعار", 3),
                ("follow_up_type", "statement", "إفادة", 4)
            };

            foreach (var (typeName, itemName, displayName, order) in items)
            {
                if (typeIds.ContainsKey(typeName))
                {
                    using var cmd = new SQLiteCommand("INSERT OR IGNORE INTO dropdown_items (dropdown_type_id, name, display_name, display_order) VALUES (@typeId, @name, @display, @order)", localConn);
                    cmd.Parameters.AddWithValue("@typeId", typeIds[typeName]);
                    cmd.Parameters.AddWithValue("@name", itemName);
                    cmd.Parameters.AddWithValue("@display", displayName);
                    cmd.Parameters.AddWithValue("@order", order);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void CloseConnections()
        {
            localConn?.Close();
        }
    }
}

