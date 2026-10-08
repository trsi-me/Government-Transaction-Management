-- قاعدة بيانات نظام إدارة المعاملات الرسمية
-- إنشاء قاعدة البيانات
CREATE DATABASE IF NOT EXISTS u741730784_stot CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE u741730784_stot;

-- جدول المستخدمين
CREATE TABLE IF NOT EXISTS users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    full_name VARCHAR(100) NOT NULL,
    role ENUM('user', 'admin') DEFAULT 'user',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active TINYINT(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- جدول أنواع القوائم المنسدلة
CREATE TABLE IF NOT EXISTS dropdown_types (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) UNIQUE NOT NULL,
    display_name VARCHAR(100) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- جدول عناصر القوائم المنسدلة
CREATE TABLE IF NOT EXISTS dropdown_items (
    id INT AUTO_INCREMENT PRIMARY KEY,
    dropdown_type_id INT NOT NULL,
    name VARCHAR(100) NOT NULL,
    display_name VARCHAR(100) NOT NULL,
    display_order INT DEFAULT 0,
    is_active TINYINT(1) DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (dropdown_type_id) REFERENCES dropdown_types(id) ON DELETE CASCADE,
    UNIQUE KEY unique_item (dropdown_type_id, name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- جدول المعاملات
CREATE TABLE IF NOT EXISTS transactions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    transaction_number VARCHAR(50) UNIQUE NOT NULL,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    department_id INT,
    source_department_id INT COMMENT 'الجهة المصدرة',
    concerned_departments TEXT COMMENT 'الجهات المعنية (مفصولة بفواصل)',
    status_id INT,
    priority_id INT,
    transaction_type_id INT,
    follow_up_type_id INT COMMENT 'نوع المتابعة',
    reference_number VARCHAR(100) COMMENT 'رقم الصادر',
    notification_number VARCHAR(100) COMMENT 'رقم الإشعار',
    next_follow_up_date DATE COMMENT 'تاريخ المتابعة القادم',
    next_follow_up_date_hijri VARCHAR(50) COMMENT 'تاريخ المتابعة القادم هجري',
    notes TEXT COMMENT 'الملاحظات',
    requests_count INT DEFAULT 0 COMMENT 'عدد الطلبات',
    is_deleted TINYINT(1) DEFAULT 0,
    is_suspended TINYINT(1) DEFAULT 0,
    created_by INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- جدول الملفات المرفقة
CREATE TABLE IF NOT EXISTS transaction_attachments (
    id INT AUTO_INCREMENT PRIMARY KEY,
    transaction_id INT NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    file_path VARCHAR(500) NOT NULL,
    file_type VARCHAR(50),
    file_size INT,
    uploaded_by INT NOT NULL,
    uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (transaction_id) REFERENCES transactions(id) ON DELETE CASCADE,
    FOREIGN KEY (uploaded_by) REFERENCES users(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- جدول سجل حركة المعاملات
CREATE TABLE IF NOT EXISTS transaction_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    transaction_id INT NOT NULL,
    user_id INT NOT NULL,
    action_type VARCHAR(50) NOT NULL,
    old_value TEXT,
    new_value TEXT,
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (transaction_id) REFERENCES transactions(id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- إدراج حساب المدير الافتراضي
-- كلمة المرور:  (يجب تغييرها بعد أول تسجيل دخول)
INSERT INTO users (username, password, full_name, role) VALUES 
('admin', '', 'مدير النظام', 'admin');

-- إدراج أنواع القوائم المنسدلة الأساسية
INSERT INTO dropdown_types (name, display_name, description) VALUES
('status', 'حالة المعاملة', 'حالات المعاملات المختلفة'),
('transaction_type', 'نوع المعاملة', 'أنواع المعاملات'),
('department', 'الجهة', 'الجهات المختلفة'),
('priority', 'الأولوية', 'مستويات الأولوية'),
('follow_up_type', 'نوع المتابعة', 'أنواع المتابعة');

-- إدراج عناصر القوائم المنسدلة الأساسية
INSERT INTO dropdown_items (dropdown_type_id, name, display_name, display_order) VALUES
-- حالات المعاملة
((SELECT id FROM dropdown_types WHERE name = 'status'), 'pending', 'قيد الانتظار', 1),
((SELECT id FROM dropdown_types WHERE name = 'status'), 'in_progress', 'قيد المعالجة', 2),
((SELECT id FROM dropdown_types WHERE name = 'status'), 'completed', 'مكتملة', 3),
((SELECT id FROM dropdown_types WHERE name = 'status'), 'cancelled', 'ملغاة', 4),
((SELECT id FROM dropdown_types WHERE name = 'status'), 'suspended', 'معلقة', 5),
-- أنواع المعاملات
((SELECT id FROM dropdown_types WHERE name = 'transaction_type'), 'inquiry', 'استفسار', 1),
((SELECT id FROM dropdown_types WHERE name = 'transaction_type'), 'note', 'ملاحظة', 2),
((SELECT id FROM dropdown_types WHERE name = 'transaction_type'), 'request', 'طلب', 3),
-- الجهات
((SELECT id FROM dropdown_types WHERE name = 'department'), 'archive', 'إدارة الأرشيف', 1),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'technical_support', 'الدعم الفني', 2),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'requests_management', 'إدارة الطلبات', 3),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'finance_ministry', 'وزارة المالية', 4),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'interior_ministry', 'وزارة الداخلية', 5),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'defense_ministry', 'وزارة الدفاع', 6),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'foreign_ministry', 'وزارة الخارجية', 7),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'education_ministry', 'وزارة التعليم', 8),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'health_ministry', 'وزارة الصحة', 9),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'commerce_ministry', 'وزارة التجارة', 10),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'energy_ministry', 'وزارة الطاقة', 11),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'transport_ministry', 'وزارة النقل', 12),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'housing_ministry', 'وزارة الإسكان', 13),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'municipal_affairs', 'وزارة الشؤون البلدية', 14),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'labor_ministry', 'وزارة العمل', 15),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'justice_ministry', 'وزارة العدل', 16),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'culture_ministry', 'وزارة الثقافة', 17),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'sports_ministry', 'وزارة الرياضة', 18),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'tourism_ministry', 'وزارة السياحة', 19),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'environment_ministry', 'وزارة البيئة', 20),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'communications_ministry', 'وزارة الاتصالات', 21),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'hr', 'الموارد البشرية', 22),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'it', 'تقنية المعلومات', 23),
((SELECT id FROM dropdown_types WHERE name = 'department'), 'legal', 'الشؤون القانونية', 24),
-- الأولويات
((SELECT id FROM dropdown_types WHERE name = 'priority'), 'low', 'منخفضة', 1),
((SELECT id FROM dropdown_types WHERE name = 'priority'), 'medium', 'متوسطة', 2),
((SELECT id FROM dropdown_types WHERE name = 'priority'), 'high', 'عالية', 3),
((SELECT id FROM dropdown_types WHERE name = 'priority'), 'urgent', 'عاجلة', 4),
-- أنواع المتابعة
((SELECT id FROM dropdown_types WHERE name = 'follow_up_type'), 'first_follow', 'تعقيب أول', 1),
((SELECT id FROM dropdown_types WHERE name = 'follow_up_type'), 'second_follow', 'تعقيب ثاني', 2),
((SELECT id FROM dropdown_types WHERE name = 'follow_up_type'), 'notification', 'إشعار', 3),
((SELECT id FROM dropdown_types WHERE name = 'follow_up_type'), 'statement', 'إفادة', 4);

