-- ملف SQL لإنشاء قاعدة البيانات المحلية SQLite
-- يتم تنفيذه تلقائياً عند أول تشغيل

-- جدول المستخدمين
CREATE TABLE IF NOT EXISTS users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT UNIQUE NOT NULL,
    password TEXT NOT NULL,
    full_name TEXT NOT NULL,
    role TEXT NOT NULL DEFAULT 'user',
    is_active INTEGER DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- جدول أنواع القوائم
CREATE TABLE IF NOT EXISTS dropdown_types (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT UNIQUE NOT NULL,
    display_name TEXT NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- جدول عناصر القوائم
CREATE TABLE IF NOT EXISTS dropdown_items (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    dropdown_type_id INTEGER NOT NULL,
    name TEXT NOT NULL,
    display_name TEXT NOT NULL,
    display_order INTEGER DEFAULT 0,
    is_active INTEGER DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (dropdown_type_id) REFERENCES dropdown_types(id) ON DELETE CASCADE,
    UNIQUE(dropdown_type_id, name)
);

-- جدول المعاملات
CREATE TABLE IF NOT EXISTS transactions (
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
    FOREIGN KEY (status_id) REFERENCES dropdown_items(id),
    FOREIGN KEY (priority_id) REFERENCES dropdown_items(id)
);

-- جدول سجل المعاملات
CREATE TABLE IF NOT EXISTS transaction_logs (
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
);

-- جدول المرفقات
CREATE TABLE IF NOT EXISTS transaction_attachments (
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
);

-- جدول مزامنة البيانات
CREATE TABLE IF NOT EXISTS sync_log (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    table_name TEXT NOT NULL,
    record_id INTEGER NOT NULL,
    action TEXT NOT NULL,
    synced_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(table_name, record_id, action)
);

-- إضافة مستخدم افتراضي (admin/password)
INSERT OR IGNORE INTO users (username, password, full_name, role) 
VALUES ('admin', '', 'مدير النظام', 'admin');

