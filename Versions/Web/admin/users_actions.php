<?php
/**
 * معالجة إجراءات المستخدمين
 */
require_once __DIR__ . '/../config/session.php';
require_once __DIR__ . '/../config/database.php';
require_once __DIR__ . '/../config/base_path.php';

requireAdmin();

$base = getBasePath();

$action = $_POST['action'] ?? $_GET['action'] ?? '';

try {
    $pdo = getDBConnection();
    
    switch ($action) {
        case 'add_user':
            $username = trim($_POST['username'] ?? '');
            $full_name = trim($_POST['full_name'] ?? '');
            $password = $_POST['password'] ?? '';
            $role = $_POST['role'] ?? 'user';
            
            if (empty($username) || empty($full_name) || empty($password)) {
                $_SESSION['error'] = 'يرجى إدخال جميع الحقول المطلوبة';
            } else {
                // التحقق من عدم وجود اسم مستخدم مكرر
                $stmt = $pdo->prepare("SELECT id FROM users WHERE username = ?");
                $stmt->execute([$username]);
                if ($stmt->fetch()) {
                    $_SESSION['error'] = 'اسم المستخدم موجود بالفعل';
                } else {
                    $hashed_password = password_hash($password, PASSWORD_DEFAULT);
                    $stmt = $pdo->prepare("INSERT INTO users (username, password, full_name, role) VALUES (?, ?, ?, ?)");
                    $stmt->execute([$username, $hashed_password, $full_name, $role]);
                    $_SESSION['success'] = 'تم إضافة المستخدم بنجاح';
                }
            }
            break;
            
        case 'edit_user':
            $id = $_POST['id'] ?? 0;
            $full_name = trim($_POST['full_name'] ?? '');
            $role = $_POST['role'] ?? 'user';
            $password = $_POST['password'] ?? '';
            
            if (empty($id) || empty($full_name)) {
                $_SESSION['error'] = 'يرجى إدخال جميع الحقول المطلوبة';
            } else {
                if (!empty($password)) {
                    $hashed_password = password_hash($password, PASSWORD_DEFAULT);
                    $stmt = $pdo->prepare("UPDATE users SET full_name = ?, role = ?, password = ? WHERE id = ?");
                    $stmt->execute([$full_name, $role, $hashed_password, $id]);
                } else {
                    $stmt = $pdo->prepare("UPDATE users SET full_name = ?, role = ? WHERE id = ?");
                    $stmt->execute([$full_name, $role, $id]);
                }
                $_SESSION['success'] = 'تم تحديث المستخدم بنجاح';
            }
            break;
            
        case 'edit_user':
            $id = $_POST['id'] ?? 0;
            $full_name = trim($_POST['full_name'] ?? '');
            $role = $_POST['role'] ?? 'user';
            $password = $_POST['password'] ?? '';
            
            if (empty($id) || empty($full_name)) {
                $_SESSION['error'] = 'يرجى إدخال جميع الحقول المطلوبة';
            } else {
                if (!empty($password)) {
                    $hashed_password = password_hash($password, PASSWORD_DEFAULT);
                    $stmt = $pdo->prepare("UPDATE users SET full_name = ?, role = ?, password = ? WHERE id = ?");
                    $stmt->execute([$full_name, $role, $hashed_password, $id]);
                } else {
                    $stmt = $pdo->prepare("UPDATE users SET full_name = ?, role = ? WHERE id = ?");
                    $stmt->execute([$full_name, $role, $id]);
                }
                $_SESSION['success'] = 'تم تحديث المستخدم بنجاح';
            }
            header('Location: ' . $base . '/admin/users.php');
            exit();
            break;
            
        case 'toggle_user':
            $id = $_GET['id'] ?? 0;
            if ($id && $id != $_SESSION['user_id']) {
                $stmt = $pdo->prepare("UPDATE users SET is_active = NOT is_active WHERE id = ?");
                $stmt->execute([$id]);
                $_SESSION['success'] = 'تم تحديث حالة المستخدم بنجاح';
            }
            break;
            
        case 'delete_user':
            $id = $_GET['id'] ?? 0;
            if ($id && $id != $_SESSION['user_id']) {
                // التحقق من عدم وجود معاملات مرتبطة
                $stmt = $pdo->prepare("SELECT COUNT(*) as count FROM transactions WHERE created_by = ?");
                $stmt->execute([$id]);
                $count = $stmt->fetch()['count'];
                
                if ($count > 0) {
                    // تعطيل بدلاً من الحذف
                    $stmt = $pdo->prepare("UPDATE users SET is_active = 0 WHERE id = ?");
                    $stmt->execute([$id]);
                    $_SESSION['success'] = 'تم تعطيل المستخدم (لا يمكن حذفه لوجود معاملات مرتبطة)';
                } else {
                    $stmt = $pdo->prepare("DELETE FROM users WHERE id = ?");
                    $stmt->execute([$id]);
                    $_SESSION['success'] = 'تم حذف المستخدم بنجاح';
                }
            }
            break;
    }
    
} catch (PDOException $e) {
    $_SESSION['error'] = 'حدث خطأ: ' . $e->getMessage();
}

header('Location: ' . $base . '/admin/users.php');
exit();

