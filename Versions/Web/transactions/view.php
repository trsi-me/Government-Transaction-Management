<?php
/**
 * عرض تفاصيل المعاملة
 */
require_once __DIR__ . '/../config/session.php';
require_once __DIR__ . '/../config/database.php';
require_once __DIR__ . '/../config/base_path.php';

requireLogin();

$base = getBasePath();

$page_title = 'تفاصيل المعاملة';

$transaction_id = $_GET['id'] ?? 0;

if (!$transaction_id) {
    header('Location: ' . $base . '/transactions/list.php');
    exit();
}

try {
    $pdo = getDBConnection();
    
    // جلب بيانات المعاملة
    $stmt = $pdo->prepare("SELECT t.*, 
                           u.full_name as creator_name,
                           (SELECT di.display_name FROM dropdown_items di 
                            INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id 
                            WHERE di.id = t.status_id AND dt.name = 'status') as status_name,
                           (SELECT di.display_name FROM dropdown_items di 
                            INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id 
                            WHERE di.id = t.department_id AND dt.name = 'department') as department_name,
                           (SELECT di.display_name FROM dropdown_items di 
                            INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id 
                            WHERE di.id = t.priority_id AND dt.name = 'priority') as priority_name,
                           (SELECT di.display_name FROM dropdown_items di 
                            INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id 
                            WHERE di.id = t.transaction_type_id AND dt.name = 'transaction_type') as type_name
                           FROM transactions t
                           INNER JOIN users u ON t.created_by = u.id
                           WHERE t.id = ?");
    $stmt->execute([$transaction_id]);
    $transaction = $stmt->fetch();
    
    if (!$transaction) {
        header('Location: ' . $base . '/transactions/list.php');
        exit();
    }
    
    // جلب سجل الحركة
    $stmt = $pdo->prepare("SELECT tl.*, u.full_name as user_name
                           FROM transaction_logs tl
                           INNER JOIN users u ON tl.user_id = u.id
                           WHERE tl.transaction_id = ?
                           ORDER BY tl.created_at DESC");
    $stmt->execute([$transaction_id]);
    $logs = $stmt->fetchAll();
    
    // جلب القوائم المنسدلة للتعديل
    $stmt = $pdo->query("SELECT dt.name as type_name, di.id, di.display_name, di.display_order
                         FROM dropdown_items di
                         INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id
                         WHERE di.is_active = 1
                         ORDER BY dt.name, di.display_order");
    $dropdown_items = $stmt->fetchAll();
    
    $dropdowns = [];
    foreach ($dropdown_items as $item) {
        $dropdowns[$item['type_name']][] = $item;
    }
    
} catch (PDOException $e) {
    $error = 'حدث خطأ أثناء جلب البيانات';
    $transaction = null;
    $logs = [];
    $dropdowns = [];
}

if (!$transaction) {
    header('Location: ' . $base . '/transactions/list.php');
    exit();
}

include __DIR__ . '/../includes/header.php';
?>

<div class="page-header">
    <h1>تفاصيل المعاملة</h1>
    <div class="header-actions">
        <a href="<?php echo $base; ?>/transactions/edit.php?id=<?php echo $transaction_id; ?>" class="btn btn-secondary"><i class="fas fa-edit"></i> تعديل</a>
        <?php if ($transaction['is_suspended']): ?>
        <a href="<?php echo $base; ?>/transactions/actions.php?action=unsuspend&id=<?php echo $transaction_id; ?>" 
           class="btn btn-primary"
           onclick="return confirm('هل أنت متأكد من إلغاء تعليق هذه المعاملة؟')">
            <i class="fas fa-check"></i> إلغاء التعليق
        </a>
        <?php else: ?>
        <a href="<?php echo $base; ?>/transactions/actions.php?action=suspend&id=<?php echo $transaction_id; ?>" 
           class="btn btn-warning"
           onclick="return confirm('هل أنت متأكد من تعليق هذه المعاملة؟')">
            <i class="fas fa-ban"></i> تعليق
        </a>
        <?php endif; ?>
        <a href="<?php echo $base; ?>/transactions/actions.php?action=delete&id=<?php echo $transaction_id; ?>" 
           class="btn btn-danger"
           onclick="return confirm('هل أنت متأكد من حذف هذه المعاملة؟')">
            <i class="fas fa-trash"></i> حذف
        </a>
        <?php if (isAdmin() && $transaction['is_deleted']): ?>
        <a href="<?php echo $base; ?>/transactions/actions.php?action=permanent_delete&id=<?php echo $transaction_id; ?>" 
           class="btn btn-danger"
           onclick="return confirm('هل أنت متأكد من الحذف الدائم؟ لا يمكن التراجع عن هذا الإجراء!')">
            <i class="fas fa-trash-alt"></i> حذف دائم
        </a>
        <?php endif; ?>
        <a href="<?php echo $base; ?>/transactions/list.php" class="btn btn-secondary"><i class="fas fa-arrow-right"></i> العودة للقائمة</a>
    </div>
</div>

<?php if (isset($error)): ?>
<div class="error-message"><?php echo htmlspecialchars($error); ?></div>
<?php endif; ?>

<div class="transaction-details">
    <div class="details-grid">
        <div class="detail-item">
            <label>رقم المعاملة</label>
            <div class="detail-value"><?php echo htmlspecialchars($transaction['transaction_number']); ?></div>
        </div>
        
        <div class="detail-item">
            <label>العنوان</label>
            <div class="detail-value"><?php echo htmlspecialchars($transaction['title']); ?></div>
        </div>
        
        <div class="detail-item">
            <label>الوصف</label>
            <div class="detail-value"><?php echo nl2br(htmlspecialchars($transaction['description'] ?: '-')); ?></div>
        </div>
        
        <div class="detail-item">
            <label>نوع المعاملة</label>
            <div class="detail-value"><?php echo htmlspecialchars($transaction['type_name'] ?? '-'); ?></div>
        </div>
        
        <div class="detail-item">
            <label>الجهة</label>
            <div class="detail-value"><?php echo htmlspecialchars($transaction['department_name'] ?? '-'); ?></div>
        </div>
        
        <div class="detail-item">
            <label>الحالة</label>
            <div class="detail-value">
                <span class="status-badge"><?php echo htmlspecialchars($transaction['status_name'] ?? '-'); ?></span>
            </div>
        </div>
        
        <div class="detail-item">
            <label>الجهة المصدرة</label>
            <div class="detail-value"><?php echo htmlspecialchars($transaction['source_department_name'] ?? '-'); ?></div>
        </div>
        
        <div class="detail-item">
            <label>الجهات المعنية</label>
            <div class="detail-value"><?php echo htmlspecialchars($transaction['concerned_departments'] ?? '-'); ?></div>
        </div>
        
        <div class="detail-item">
            <label>رقم الصادر</label>
            <div class="detail-value"><?php echo htmlspecialchars($transaction['reference_number'] ?? '-'); ?></div>
        </div>
        
        <div class="detail-item">
            <label>رقم الإشعار</label>
            <div class="detail-value"><?php echo htmlspecialchars($transaction['notification_number'] ?? '-'); ?></div>
        </div>
        
        <div class="detail-item">
            <label>نوع المتابعة</label>
            <div class="detail-value"><?php echo htmlspecialchars($transaction['follow_up_type_name'] ?? '-'); ?></div>
        </div>
        
        <div class="detail-item">
            <label>تاريخ المتابعة القادم (ميلادي)</label>
            <div class="detail-value"><?php echo $transaction['next_follow_up_date'] ? date('Y-m-d', strtotime($transaction['next_follow_up_date'])) : '-'; ?></div>
        </div>
        
        <div class="detail-item">
            <label>تاريخ المتابعة القادم (هجري)</label>
            <div class="detail-value"><?php echo htmlspecialchars($transaction['next_follow_up_date_hijri'] ?? '-'); ?></div>
        </div>
        
        <div class="detail-item">
            <label>عدد الطلبات</label>
            <div class="detail-value"><?php echo $transaction['requests_count'] ?? 0; ?></div>
        </div>
        
        <div class="detail-item">
            <label>الأولوية</label>
            <div class="detail-value"><?php echo htmlspecialchars($transaction['priority_name'] ?? '-'); ?></div>
        </div>
        
        <div class="detail-item">
            <label>الملاحظات</label>
            <div class="detail-value"><?php echo nl2br(htmlspecialchars($transaction['notes'] ?? '-')); ?></div>
        </div>
        
        <div class="detail-item">
            <label>أنشأها</label>
            <div class="detail-value"><?php echo htmlspecialchars($transaction['creator_name']); ?></div>
        </div>
        
        <div class="detail-item">
            <label>تاريخ الإنشاء</label>
            <div class="detail-value"><?php echo date('Y-m-d H:i', strtotime($transaction['created_at'])); ?></div>
        </div>
        
        <div class="detail-item">
            <label>آخر تحديث</label>
            <div class="detail-value"><?php echo date('Y-m-d H:i', strtotime($transaction['updated_at'])); ?></div>
        </div>
    </div>
    
    <?php if (!empty($attachments)): ?>
    <div class="attachments-section" style="margin-top: 30px; padding-top: 25px; border-top: 2px solid #e9ecef;">
        <h3 style="margin-bottom: 20px; color: #1e3a5f; font-size: 20px; font-weight: bold;">الملفات المرفقة</h3>
        <div class="attachments-list" style="display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 15px;">
            <?php foreach ($attachments as $attachment): ?>
            <div class="attachment-item" style="padding: 15px; background: #f8f9fa; border-radius: 6px; border-right: 3px solid #1e3a5f;">
                <div style="margin-bottom: 8px;">
                    <i class="fas fa-file" style="margin-left: 8px; color: #1e3a5f;"></i>
                    <strong><?php echo htmlspecialchars($attachment['file_name']); ?></strong>
                </div>
                <div style="font-size: 12px; color: #6c757d; margin-bottom: 10px;">
                    <?php echo number_format($attachment['file_size'] / 1024, 2); ?> KB
                </div>
                <a href="<?php echo $base; ?>/<?php echo htmlspecialchars($attachment['file_path']); ?>" 
                   target="_blank" 
                   class="btn btn-sm btn-primary"
                   style="width: 100%;">
                    <i class="fas fa-download"></i> تحميل
                </a>
            </div>
            <?php endforeach; ?>
        </div>
    </div>
    <?php endif; ?>
</div>

<div class="transaction-logs">
    <h2>سجل حركة المعاملة</h2>
    <?php if (empty($logs)): ?>
    <div class="empty-state">
        <p>لا توجد سجلات حركة</p>
    </div>
    <?php else: ?>
    <div class="logs-list">
        <?php foreach ($logs as $log): ?>
        <div class="log-item">
            <div class="log-header">
                <span class="log-action"><?php echo htmlspecialchars($log['action_type']); ?></span>
                <span class="log-user"><?php echo htmlspecialchars($log['user_name']); ?></span>
                <span class="log-date"><?php echo date('Y-m-d H:i', strtotime($log['created_at'])); ?></span>
            </div>
            <?php if ($log['notes']): ?>
            <div class="log-notes"><?php echo nl2br(htmlspecialchars($log['notes'])); ?></div>
            <?php endif; ?>
            <?php if ($log['old_value'] || $log['new_value']): ?>
            <div class="log-changes">
                <?php if ($log['old_value']): ?>
                <div class="change-old">قبل: <?php echo htmlspecialchars($log['old_value']); ?></div>
                <?php endif; ?>
                <?php if ($log['new_value']): ?>
                <div class="change-new">بعد: <?php echo htmlspecialchars($log['new_value']); ?></div>
                <?php endif; ?>
            </div>
            <?php endif; ?>
        </div>
        <?php endforeach; ?>
    </div>
    <?php endif; ?>
</div>

<?php include __DIR__ . '/../includes/footer.php'; ?>

