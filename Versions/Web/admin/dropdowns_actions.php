<?php
/**
 * معالجة إجراءات القوائم المنسدلة
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
        case 'add_type':
            $name = trim($_POST['name'] ?? '');
            $display_name = trim($_POST['display_name'] ?? '');
            $description = trim($_POST['description'] ?? '');
            
            if (empty($name) || empty($display_name)) {
                $_SESSION['error'] = 'يرجى إدخال جميع الحقول المطلوبة';
            } else {
                $stmt = $pdo->prepare("INSERT INTO dropdown_types (name, display_name, description) VALUES (?, ?, ?)");
                $stmt->execute([$name, $display_name, $description]);
                $_SESSION['success'] = 'تم إضافة نوع القائمة بنجاح';
            }
            break;
            
        case 'add_item':
            $dropdown_type_id = $_POST['dropdown_type_id'] ?? 0;
            $name = trim($_POST['name'] ?? '');
            $display_name = trim($_POST['display_name'] ?? '');
            $display_order = intval($_POST['display_order'] ?? 0);
            $is_active = isset($_POST['is_active']) ? 1 : 0;
            
            if (empty($dropdown_type_id) || empty($name) || empty($display_name)) {
                $_SESSION['error'] = 'يرجى إدخال جميع الحقول المطلوبة';
            } else {
                $stmt = $pdo->prepare("INSERT INTO dropdown_items 
                                      (dropdown_type_id, name, display_name, display_order, is_active) 
                                      VALUES (?, ?, ?, ?, ?)");
                $stmt->execute([$dropdown_type_id, $name, $display_name, $display_order, $is_active]);
                $_SESSION['success'] = 'تم إضافة العنصر بنجاح';
            }
            break;
            
        case 'edit_type':
            $id = $_POST['id'] ?? 0;
            $display_name = trim($_POST['display_name'] ?? '');
            $description = trim($_POST['description'] ?? '');
            
            if (empty($id) || empty($display_name)) {
                $_SESSION['error'] = 'يرجى إدخال جميع الحقول المطلوبة';
            } else {
                $stmt = $pdo->prepare("UPDATE dropdown_types SET display_name = ?, description = ? WHERE id = ?");
                $stmt->execute([$display_name, $description, $id]);
                $_SESSION['success'] = 'تم تحديث نوع القائمة بنجاح';
            }
            header('Location: ' . $base . '/admin/dropdowns.php');
            exit();
            break;
            
        case 'edit_item':
            $id = $_POST['id'] ?? 0;
            $dropdown_type_id = $_POST['dropdown_type_id'] ?? 0;
            $display_name = trim($_POST['display_name'] ?? '');
            $display_order = intval($_POST['display_order'] ?? 0);
            $is_active = isset($_POST['is_active']) ? 1 : 0;
            
            if (empty($id) || empty($display_name) || empty($dropdown_type_id)) {
                $_SESSION['error'] = 'يرجى إدخال جميع الحقول المطلوبة';
            } else {
                $stmt = $pdo->prepare("UPDATE dropdown_items 
                                      SET display_name = ?, display_order = ?, is_active = ? 
                                      WHERE id = ?");
                $stmt->execute([$display_name, $display_order, $is_active, $id]);
                $_SESSION['success'] = 'تم تحديث العنصر بنجاح';
            }
            header('Location: ' . $base . '/admin/dropdowns.php');
            exit();
            break;
            
        case 'delete_type':
            $id = $_GET['id'] ?? 0;
            if ($id) {
                $stmt = $pdo->prepare("DELETE FROM dropdown_types WHERE id = ?");
                $stmt->execute([$id]);
                $_SESSION['success'] = 'تم حذف نوع القائمة بنجاح';
            }
            break;
            
        case 'delete_item':
            $id = $_GET['id'] ?? 0;
            if ($id) {
                $stmt = $pdo->prepare("DELETE FROM dropdown_items WHERE id = ?");
                $stmt->execute([$id]);
                $_SESSION['success'] = 'تم حذف العنصر بنجاح';
            }
            break;
    }
    
} catch (PDOException $e) {
    $_SESSION['error'] = 'حدث خطأ: ' . $e->getMessage();
}

header('Location: ' . $base . '/admin/dropdowns.php');
exit();

