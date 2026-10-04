<?php
/**
 * معالجة إجراءات المعاملات (حذف، تعليق، إلغاء تعليق)
 */
require_once __DIR__ . '/../config/session.php';
require_once __DIR__ . '/../config/database.php';
require_once __DIR__ . '/../config/base_path.php';

requireLogin();

$base = getBasePath();
$action = $_GET['action'] ?? '';
$transaction_id = intval($_GET['id'] ?? 0);

if (!$transaction_id || !$action) {
    header('Location: ' . $base . '/transactions/list.php');
    exit();
}

try {
    $pdo = getDBConnection();
    
    // التحقق من وجود المعاملة
    $stmt = $pdo->prepare("SELECT * FROM transactions WHERE id = ?");
    $stmt->execute([$transaction_id]);
    $transaction = $stmt->fetch();
    
    if (!$transaction) {
        $_SESSION['error'] = 'المعاملة غير موجودة';
        header('Location: ' . $base . '/transactions/list.php');
        exit();
    }
    
    switch ($action) {
        case 'delete':
            // حذف منطقي (soft delete)
            $stmt = $pdo->prepare("UPDATE transactions SET is_deleted = 1 WHERE id = ?");
            $stmt->execute([$transaction_id]);
            
            // تسجيل في سجل الحركة
            $stmt = $pdo->prepare("INSERT INTO transaction_logs (transaction_id, user_id, action_type, notes)
                                   VALUES (?, ?, 'deleted', ?)");
            $stmt->execute([
                $transaction_id,
                $_SESSION['user_id'],
                'تم حذف المعاملة'
            ]);
            
            $_SESSION['success'] = 'تم حذف المعاملة بنجاح';
            break;
            
        case 'restore':
            // استعادة المعاملة المحذوفة
            $stmt = $pdo->prepare("UPDATE transactions SET is_deleted = 0 WHERE id = ?");
            $stmt->execute([$transaction_id]);
            
            // تسجيل في سجل الحركة
            $stmt = $pdo->prepare("INSERT INTO transaction_logs (transaction_id, user_id, action_type, notes)
                                   VALUES (?, ?, 'restored', ?)");
            $stmt->execute([
                $transaction_id,
                $_SESSION['user_id'],
                'تم استعادة المعاملة'
            ]);
            
            $_SESSION['success'] = 'تم استعادة المعاملة بنجاح';
            break;
            
        case 'suspend':
            // تعليق المعاملة
            $stmt = $pdo->prepare("UPDATE transactions SET is_suspended = 1 WHERE id = ?");
            $stmt->execute([$transaction_id]);
            
            // تسجيل في سجل الحركة
            $stmt = $pdo->prepare("INSERT INTO transaction_logs (transaction_id, user_id, action_type, notes)
                                   VALUES (?, ?, 'suspended', ?)");
            $stmt->execute([
                $transaction_id,
                $_SESSION['user_id'],
                'تم تعليق المعاملة'
            ]);
            
            $_SESSION['success'] = 'تم تعليق المعاملة بنجاح';
            break;
            
        case 'unsuspend':
            // إلغاء تعليق المعاملة
            $stmt = $pdo->prepare("UPDATE transactions SET is_suspended = 0 WHERE id = ?");
            $stmt->execute([$transaction_id]);
            
            // تسجيل في سجل الحركة
            $stmt = $pdo->prepare("INSERT INTO transaction_logs (transaction_id, user_id, action_type, notes)
                                   VALUES (?, ?, 'unsuspended', ?)");
            $stmt->execute([
                $transaction_id,
                $_SESSION['user_id'],
                'تم إلغاء تعليق المعاملة'
            ]);
            
            $_SESSION['success'] = 'تم إلغاء تعليق المعاملة بنجاح';
            break;
            
        case 'permanent_delete':
            // حذف دائم (للمدير فقط)
            if (!isAdmin()) {
                $_SESSION['error'] = 'ليس لديك صلاحية للحذف الدائم';
                header('Location: ' . $base . '/transactions/view.php?id=' . $transaction_id);
                exit();
            }
            
            // حذف الملفات المرفقة
            $stmt = $pdo->prepare("SELECT file_path FROM transaction_attachments WHERE transaction_id = ?");
            $stmt->execute([$transaction_id]);
            $attachments = $stmt->fetchAll();
            
            foreach ($attachments as $attachment) {
                $file_path = __DIR__ . '/../' . $attachment['file_path'];
                if (file_exists($file_path)) {
                    unlink($file_path);
                }
            }
            
            // حذف المعاملة (CASCADE سيحذف السجلات المرتبطة)
            $stmt = $pdo->prepare("DELETE FROM transactions WHERE id = ?");
            $stmt->execute([$transaction_id]);
            
            $_SESSION['success'] = 'تم الحذف الدائم للمعاملة بنجاح';
            header('Location: ' . $base . '/transactions/list.php');
            exit();
            
        default:
            $_SESSION['error'] = 'إجراء غير صحيح';
            break;
    }
    
} catch (PDOException $e) {
    $_SESSION['error'] = 'حدث خطأ: ' . $e->getMessage();
}

header('Location: ' . $base . '/transactions/view.php?id=' . $transaction_id);
exit();

