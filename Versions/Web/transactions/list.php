<?php
/**
 * قائمة المعاملات
 */
require_once __DIR__ . '/../config/session.php';
require_once __DIR__ . '/../config/database.php';
require_once __DIR__ . '/../config/base_path.php';

requireLogin();

$base = getBasePath();

$page_title = 'قائمة المعاملات';

// معاملات البحث والتصفية
$search = $_GET['search'] ?? '';
$status_filter = $_GET['status'] ?? '';
$department_filter = $_GET['department'] ?? '';
$date_from = $_GET['date_from'] ?? '';
$date_to = $_GET['date_to'] ?? '';

try {
    $pdo = getDBConnection();
    
    // بناء استعلام البحث
    $where = ['t.is_deleted = 0'];
    $params = [];
    
    if (!empty($search)) {
        $where[] = "(t.transaction_number LIKE ? OR t.title LIKE ?)";
        $search_param = "%$search%";
        $params[] = $search_param;
        $params[] = $search_param;
    }
    
    if (!empty($status_filter)) {
        $where[] = "t.status_id = ?";
        $params[] = $status_filter;
    }
    
    if (!empty($department_filter)) {
        $where[] = "t.department_id = ?";
        $params[] = $department_filter;
    }
    
    if (!empty($date_from)) {
        $where[] = "DATE(t.created_at) >= ?";
        $params[] = $date_from;
    }
    
    if (!empty($date_to)) {
        $where[] = "DATE(t.created_at) <= ?";
        $params[] = $date_to;
    }
    
    $where_clause = 'WHERE ' . implode(' AND ', $where);
    
    // جلب المعاملات
    $sql = "SELECT t.*, 
            u.full_name as creator_name,
            (SELECT di.display_name FROM dropdown_items di 
             INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id 
             WHERE di.id = t.status_id AND dt.name = 'status') as status_name,
            (SELECT di.display_name FROM dropdown_items di 
             INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id 
             WHERE di.id = t.department_id AND dt.name = 'department') as department_name,
            (SELECT di.display_name FROM dropdown_items di 
             INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id 
             WHERE di.id = t.priority_id AND dt.name = 'priority') as priority_name
            FROM transactions t
            INNER JOIN users u ON t.created_by = u.id
            $where_clause
            ORDER BY t.created_at DESC";
    
    $stmt = $pdo->prepare($sql);
    $stmt->execute($params);
    $transactions = $stmt->fetchAll();
    
    // جلب القوائم المنسدلة للتصفية
    $stmt = $pdo->query("SELECT di.id, di.display_name, dt.name as type_name
                         FROM dropdown_items di
                         INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id
                         WHERE dt.name IN ('status', 'department') AND di.is_active = 1
                         ORDER BY dt.name, di.display_order");
    $filter_options = $stmt->fetchAll();
    
    $status_options = array_filter($filter_options, function($item) {
        return $item['type_name'] === 'status';
    });
    
    $department_options = array_filter($filter_options, function($item) {
        return $item['type_name'] === 'department';
    });
    
} catch (PDOException $e) {
    $error = 'حدث خطأ أثناء جلب البيانات';
    $transactions = [];
    $status_options = [];
    $department_options = [];
}

include __DIR__ . '/../includes/header.php';
?>

<div class="page-header">
    <h1>قائمة المعاملات</h1>
    <a href="<?php echo $base; ?>/transactions/add.php" class="btn btn-primary">إضافة معاملة جديدة</a>
</div>

<div class="filters-section">
    <form method="GET" action="<?php echo $base; ?>/transactions/list.php" class="filters-form">
        <div class="filter-row">
            <div class="filter-group">
                <label>بحث</label>
                <input type="text" name="search" value="<?php echo htmlspecialchars($search); ?>" placeholder="رقم المعاملة أو العنوان">
            </div>
            
            <div class="filter-group">
                <label>الحالة</label>
                <select name="status">
                    <option value="">الكل</option>
                    <?php foreach ($status_options as $option): ?>
                    <option value="<?php echo $option['id']; ?>" <?php echo $status_filter == $option['id'] ? 'selected' : ''; ?>>
                        <?php echo htmlspecialchars($option['display_name']); ?>
                    </option>
                    <?php endforeach; ?>
                </select>
            </div>
            
            <div class="filter-group">
                <label>الجهة</label>
                <select name="department">
                    <option value="">الكل</option>
                    <?php foreach ($department_options as $option): ?>
                    <option value="<?php echo $option['id']; ?>" <?php echo $department_filter == $option['id'] ? 'selected' : ''; ?>>
                        <?php echo htmlspecialchars($option['display_name']); ?>
                    </option>
                    <?php endforeach; ?>
                </select>
            </div>
            
            <div class="filter-group">
                <label>من تاريخ</label>
                <input type="date" name="date_from" value="<?php echo htmlspecialchars($date_from); ?>">
            </div>
            
            <div class="filter-group">
                <label>إلى تاريخ</label>
                <input type="date" name="date_to" value="<?php echo htmlspecialchars($date_to); ?>">
            </div>
            
            <div class="filter-actions">
                <button type="submit" class="btn btn-primary">بحث</button>
                <a href="<?php echo $base; ?>/transactions/list.php" class="btn btn-secondary">إعادة تعيين</a>
            </div>
        </div>
    </form>
</div>

<?php if (isset($error)): ?>
<div class="error-message"><?php echo htmlspecialchars($error); ?></div>
<?php endif; ?>

<?php if (empty($transactions)): ?>
<div class="empty-state">
    <p>لا توجد معاملات</p>
</div>
<?php else: ?>
<div class="table-container">
    <table class="data-table">
        <thead>
            <tr>
                <th>رقم المعاملة</th>
                <th>العنوان</th>
                <th>الجهة</th>
                <th>الحالة</th>
                <th>الأولوية</th>
                <th>تاريخ الإنشاء</th>
                <th>الإجراءات</th>
            </tr>
        </thead>
        <tbody>
            <?php foreach ($transactions as $transaction): ?>
            <tr>
                <td><?php echo htmlspecialchars($transaction['transaction_number']); ?></td>
                <td><?php echo htmlspecialchars($transaction['title']); ?></td>
                <td><?php echo htmlspecialchars($transaction['department_name'] ?? '-'); ?></td>
                <td><span class="status-badge"><?php echo htmlspecialchars($transaction['status_name'] ?? '-'); ?></span></td>
                <td><?php echo htmlspecialchars($transaction['priority_name'] ?? '-'); ?></td>
                <td><?php echo date('Y-m-d H:i', strtotime($transaction['created_at'])); ?></td>
                <td>
                    <a href="<?php echo $base; ?>/transactions/view.php?id=<?php echo $transaction['id']; ?>" class="btn btn-sm">عرض</a>
                    <a href="<?php echo $base; ?>/transactions/edit.php?id=<?php echo $transaction['id']; ?>" class="btn btn-sm btn-secondary">تعديل</a>
                </td>
            </tr>
            <?php endforeach; ?>
        </tbody>
    </table>
</div>
<?php endif; ?>

<?php include __DIR__ . '/../includes/footer.php'; ?>

