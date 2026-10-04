<?php
/**
 * الصفحة الرئيسية
 */
require_once __DIR__ . '/config/session.php';
require_once __DIR__ . '/config/database.php';
require_once __DIR__ . '/config/base_path.php';

requireLogin();

$base = getBasePath();

$page_title = 'الصفحة الرئيسية';

// جلب إحصائيات سريعة
try {
    $pdo = getDBConnection();
    
    // عدد المعاملات الإجمالي (غير المحذوفة)
    $stmt = $pdo->query("SELECT COUNT(*) as total FROM transactions WHERE is_deleted = 0");
    $total_transactions = $stmt->fetch()['total'];
    
    // عدد المعاملات قيد الانتظار
    $stmt = $pdo->query("SELECT COUNT(*) as pending FROM transactions t 
                     INNER JOIN dropdown_items di ON t.status_id = di.id 
                     INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id 
                     WHERE dt.name = 'status' AND di.name = 'pending' AND t.is_deleted = 0");
    $pending_transactions = $stmt->fetch()['pending'];
    
    // عدد المعاملات قيد المعالجة
    $stmt = $pdo->query("SELECT COUNT(*) as in_progress FROM transactions t 
                         INNER JOIN dropdown_items di ON t.status_id = di.id 
                         INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id 
                         WHERE dt.name = 'status' AND di.name = 'in_progress' AND t.is_deleted = 0");
    $in_progress_transactions = $stmt->fetch()['in_progress'];
    
    // عدد المعاملات المكتملة
    $stmt = $pdo->query("SELECT COUNT(*) as completed FROM transactions t 
                         INNER JOIN dropdown_items di ON t.status_id = di.id 
                         INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id 
                         WHERE dt.name = 'status' AND di.name = 'completed' AND t.is_deleted = 0");
    $completed_transactions = $stmt->fetch()['completed'];
    
    // آخر المعاملات (غير المحذوفة)
    $stmt = $pdo->prepare("SELECT t.*, u.full_name as creator_name,
                          (SELECT di.display_name FROM dropdown_items di 
                           INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id 
                           WHERE di.id = t.status_id AND dt.name = 'status') as status_name
                          FROM transactions t
                          INNER JOIN users u ON t.created_by = u.id
                          WHERE t.is_deleted = 0
                          ORDER BY t.created_at DESC
                          LIMIT 10");
    $stmt->execute();
    $recent_transactions = $stmt->fetchAll();
    
} catch (PDOException $e) {
    $error = 'حدث خطأ أثناء جلب البيانات';
    $total_transactions = 0;
    $pending_transactions = 0;
    $in_progress_transactions = 0;
    $completed_transactions = 0;
    $recent_transactions = [];
}

include __DIR__ . '/includes/header.php';
?>

<div class="dashboard">
    <div class="welcome-section">
        <h2>مرحباً، <?php echo htmlspecialchars($_SESSION['full_name'] ?? $_SESSION['username'] ?? 'مستخدم'); ?></h2>
        <p>لوحة التحكم الرئيسية</p>
    </div>
    
    <div class="stats-grid">
        <div class="stat-card">
            <div class="stat-icon"><i class="fas fa-file-alt"></i></div>
            <div class="stat-info">
                <h3><?php echo $total_transactions; ?></h3>
                <p>إجمالي المعاملات</p>
            </div>
        </div>
        
        <div class="stat-card">
            <div class="stat-icon"><i class="fas fa-clock"></i></div>
            <div class="stat-info">
                <h3><?php echo $pending_transactions; ?></h3>
                <p>قيد الانتظار</p>
            </div>
        </div>
        
        <div class="stat-card">
            <div class="stat-icon"><i class="fas fa-spinner"></i></div>
            <div class="stat-info">
                <h3><?php echo $in_progress_transactions; ?></h3>
                <p>قيد المعالجة</p>
            </div>
        </div>
        
        <div class="stat-card">
            <div class="stat-icon"><i class="fas fa-check-circle"></i></div>
            <div class="stat-info">
                <h3><?php echo $completed_transactions; ?></h3>
                <p>مكتملة</p>
            </div>
        </div>
    </div>
    
    <div class="recent-transactions">
        <h2>آخر المعاملات</h2>
        <?php if (empty($recent_transactions)): ?>
        <div class="empty-state">
            <p>لا توجد معاملات حتى الآن</p>
            <a href="<?php echo $base; ?>/transactions/add.php" class="btn btn-primary">إضافة معاملة جديدة</a>
        </div>
        <?php else: ?>
        <table class="data-table">
            <thead>
                <tr>
                    <th>رقم المعاملة</th>
                    <th>العنوان</th>
                    <th>الحالة</th>
                    <th>تاريخ الإنشاء</th>
                    <th>الإجراءات</th>
                </tr>
            </thead>
            <tbody>
                <?php foreach ($recent_transactions as $transaction): ?>
                <tr>
                    <td><?php echo htmlspecialchars($transaction['transaction_number']); ?></td>
                    <td><?php echo htmlspecialchars($transaction['title']); ?></td>
                    <td><span class="status-badge"><?php echo htmlspecialchars($transaction['status_name']); ?></span></td>
                    <td><?php echo date('Y-m-d H:i', strtotime($transaction['created_at'])); ?></td>
                    <td>
                        <a href="<?php echo $base; ?>/transactions/view.php?id=<?php echo $transaction['id']; ?>" class="btn btn-sm">عرض</a>
                    </td>
                </tr>
                <?php endforeach; ?>
            </tbody>
        </table>
        <?php endif; ?>
    </div>
</div>

<?php include __DIR__ . '/includes/footer.php'; ?>

