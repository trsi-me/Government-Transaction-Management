<?php
/**
 * تعديل المعاملة
 */
require_once __DIR__ . '/../config/session.php';
require_once __DIR__ . '/../config/database.php';
require_once __DIR__ . '/../config/base_path.php';
require_once __DIR__ . '/../config/hijri_converter.php';

requireLogin();

$base = getBasePath();

$page_title = 'تعديل المعاملة';

$transaction_id = $_GET['id'] ?? 0;
$error = '';
$success = '';

if (!$transaction_id) {
    header('Location: ' . $base . '/transactions/list.php');
    exit();
}

// جلب القوائم المنسدلة
try {
    $pdo = getDBConnection();
    
    // جلب بيانات المعاملة
    $stmt = $pdo->prepare("SELECT * FROM transactions WHERE id = ?");
    $stmt->execute([$transaction_id]);
    $transaction = $stmt->fetch();
    
    if (!$transaction) {
        header('Location: ' . $base . '/transactions/list.php');
        exit();
    }
    
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
    $dropdowns = [];
}

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $title = trim($_POST['title'] ?? '');
    $description = trim($_POST['description'] ?? '');
    $department_id = $_POST['department_id'] ?? null;
    $source_department_id = $_POST['source_department_id'] ?? null;
    $concerned_departments = trim($_POST['concerned_departments'] ?? '');
    $status_id = $_POST['status_id'] ?? null;
    $priority_id = $_POST['priority_id'] ?? null;
    $transaction_type_id = $_POST['transaction_type_id'] ?? null;
    $follow_up_type_id = $_POST['follow_up_type_id'] ?? null;
    $reference_number = trim($_POST['reference_number'] ?? '');
    $notification_number = trim($_POST['notification_number'] ?? '');
    $next_follow_up_date = $_POST['next_follow_up_date'] ?? null;
    $notes = trim($_POST['notes'] ?? '');
    $requests_count = intval($_POST['requests_count'] ?? 0);
    
    // تحويل التاريخ إلى هجري
    $next_follow_up_date_hijri = '';
    if ($next_follow_up_date) {
        $next_follow_up_date_hijri = gregorianToHijri($next_follow_up_date);
    }
    
    if (empty($title)) {
        $error = 'يرجى إدخال عنوان المعاملة';
    } else {
        try {
            $pdo = getDBConnection();
            
            // جلب القيم القديمة لتسجيل التغييرات
            $old_transaction = $transaction;
            
            // تحديث المعاملة
            $stmt = $pdo->prepare("UPDATE transactions 
                                   SET title = ?, description = ?, department_id = ?, 
                                       source_department_id = ?, concerned_departments = ?,
                                       status_id = ?, priority_id = ?, transaction_type_id = ?,
                                       follow_up_type_id = ?, reference_number = ?, 
                                       notification_number = ?, next_follow_up_date = ?,
                                       next_follow_up_date_hijri = ?, notes = ?, requests_count = ?
                                   WHERE id = ?");
            $stmt->execute([
                $title,
                $description,
                $department_id ?: null,
                $source_department_id ?: null,
                $concerned_departments ?: null,
                $status_id ?: null,
                $priority_id ?: null,
                $transaction_type_id ?: null,
                $follow_up_type_id ?: null,
                $reference_number ?: null,
                $notification_number ?: null,
                $next_follow_up_date ?: null,
                $next_follow_up_date_hijri ?: null,
                $notes ?: null,
                $requests_count,
                $transaction_id
            ]);
            
            // تسجيل التغييرات في سجل الحركة
            $changes = [];
            
            if ($old_transaction['title'] != $title) {
                $changes[] = "العنوان: {$old_transaction['title']} → {$title}";
            }
            
            if ($old_transaction['status_id'] != $status_id) {
                $old_status = '';
                $new_status = '';
                
                if ($old_transaction['status_id']) {
                    $stmt = $pdo->prepare("SELECT display_name FROM dropdown_items WHERE id = ?");
                    $stmt->execute([$old_transaction['status_id']]);
                    $result = $stmt->fetch();
                    $old_status = $result ? $result['display_name'] : '';
                }
                
                if ($status_id) {
                    $stmt = $pdo->prepare("SELECT display_name FROM dropdown_items WHERE id = ?");
                    $stmt->execute([$status_id]);
                    $result = $stmt->fetch();
                    $new_status = $result ? $result['display_name'] : '';
                }
                
                $changes[] = "الحالة: {$old_status} → {$new_status}";
            }
            
            if ($old_transaction['department_id'] != $department_id) {
                $old_dept = '';
                $new_dept = '';
                
                if ($old_transaction['department_id']) {
                    $stmt = $pdo->prepare("SELECT display_name FROM dropdown_items WHERE id = ?");
                    $stmt->execute([$old_transaction['department_id']]);
                    $result = $stmt->fetch();
                    $old_dept = $result ? $result['display_name'] : '';
                }
                
                if ($department_id) {
                    $stmt = $pdo->prepare("SELECT display_name FROM dropdown_items WHERE id = ?");
                    $stmt->execute([$department_id]);
                    $result = $stmt->fetch();
                    $new_dept = $result ? $result['display_name'] : '';
                }
                
                $changes[] = "الجهة: {$old_dept} → {$new_dept}";
            }
            
            // معالجة الملفات المرفقة الجديدة
            if (!empty($_FILES['attachments']['name'][0])) {
                $upload_dir = __DIR__ . '/../uploads/transactions/' . $transaction_id . '/';
                if (!is_dir($upload_dir)) {
                    mkdir($upload_dir, 0755, true);
                }
                
                foreach ($_FILES['attachments']['name'] as $key => $filename) {
                    if ($_FILES['attachments']['error'][$key] === UPLOAD_ERR_OK) {
                        $tmp_name = $_FILES['attachments']['tmp_name'][$key];
                        $file_size = $_FILES['attachments']['size'][$key];
                        $file_type = $_FILES['attachments']['type'][$key];
                        $unique_name = time() . '_' . $filename;
                        $file_path = $upload_dir . $unique_name;
                        
                        if (move_uploaded_file($tmp_name, $file_path)) {
                            $relative_path = 'uploads/transactions/' . $transaction_id . '/' . $unique_name;
                            $stmt = $pdo->prepare("INSERT INTO transaction_attachments 
                                                   (transaction_id, file_name, file_path, file_type, file_size, uploaded_by)
                                                   VALUES (?, ?, ?, ?, ?, ?)");
                            $stmt->execute([
                                $transaction_id,
                                $filename,
                                $relative_path,
                                $file_type,
                                $file_size,
                                $_SESSION['user_id']
                            ]);
                            $changes[] = "تم إضافة ملف مرفق: {$filename}";
                        }
                    }
                }
            }
            
            if (!empty($changes)) {
                $notes = implode("\n", $changes);
                $stmt = $pdo->prepare("INSERT INTO transaction_logs (transaction_id, user_id, action_type, notes, old_value, new_value)
                                       VALUES (?, ?, 'updated', ?, ?, ?)");
                $stmt->execute([
                    $transaction_id,
                    $_SESSION['user_id'],
                    $notes,
                    json_encode($old_transaction),
                    json_encode(['title' => $title, 'status_id' => $status_id, 'department_id' => $department_id])
                ]);
            }
            
            $success = 'تم تحديث المعاملة بنجاح';
            header('Location: ' . $base . '/transactions/view.php?id=' . $transaction_id);
            exit();
            
        } catch (PDOException $e) {
            $error = 'حدث خطأ أثناء تحديث المعاملة';
        }
    }
}

if (!$transaction) {
    header('Location: ' . $base . '/transactions/list.php');
    exit();
}

include __DIR__ . '/../includes/header.php';
?>

<div class="page-header">
    <h1>تعديل المعاملة</h1>
    <a href="<?php echo $base; ?>/transactions/view.php?id=<?php echo $transaction_id; ?>" class="btn btn-secondary">العودة</a>
</div>

<?php if ($error): ?>
<div class="error-message"><?php echo htmlspecialchars($error); ?></div>
<?php endif; ?>

<?php if ($success): ?>
<div class="success-message"><?php echo htmlspecialchars($success); ?></div>
<?php endif; ?>

<form method="POST" action="<?php echo $base; ?>/transactions/edit.php?id=<?php echo $transaction_id; ?>" class="transaction-form" enctype="multipart/form-data">
    <div class="form-section">
        <h2>معلومات المعاملة</h2>
        
        <div class="form-group">
            <label for="title">العنوان <span class="required">*</span></label>
            <input type="text" id="title" name="title" value="<?php echo htmlspecialchars($transaction['title']); ?>" required>
        </div>
        
        <div class="form-group">
            <label for="description">الوصف</label>
            <textarea id="description" name="description" rows="5"><?php echo htmlspecialchars($transaction['description']); ?></textarea>
        </div>
        
        <div class="form-row">
            <div class="form-group">
                <label for="transaction_type_id">نوع المعاملة</label>
                <select id="transaction_type_id" name="transaction_type_id">
                    <option value="">اختر النوع</option>
                    <?php if (isset($dropdowns['transaction_type'])): ?>
                        <?php foreach ($dropdowns['transaction_type'] as $item): ?>
                        <option value="<?php echo $item['id']; ?>" <?php echo $transaction['transaction_type_id'] == $item['id'] ? 'selected' : ''; ?>>
                            <?php echo htmlspecialchars($item['display_name']); ?>
                        </option>
                        <?php endforeach; ?>
                    <?php endif; ?>
                </select>
            </div>
            
            <div class="form-group">
                <label for="department_id">الجهة</label>
                <select id="department_id" name="department_id">
                    <option value="">اختر الجهة</option>
                    <?php if (isset($dropdowns['department'])): ?>
                        <?php foreach ($dropdowns['department'] as $item): ?>
                        <option value="<?php echo $item['id']; ?>" <?php echo $transaction['department_id'] == $item['id'] ? 'selected' : ''; ?>>
                            <?php echo htmlspecialchars($item['display_name']); ?>
                        </option>
                        <?php endforeach; ?>
                    <?php endif; ?>
                </select>
            </div>
        </div>
        
        <div class="form-row">
            <div class="form-group">
                <label for="source_department_id">الجهة المصدرة</label>
                <select id="source_department_id" name="source_department_id">
                    <option value="">اختر الجهة المصدرة</option>
                    <?php if (isset($dropdowns['department'])): ?>
                        <?php foreach ($dropdowns['department'] as $item): ?>
                        <option value="<?php echo $item['id']; ?>" <?php echo $transaction['source_department_id'] == $item['id'] ? 'selected' : ''; ?>>
                            <?php echo htmlspecialchars($item['display_name']); ?>
                        </option>
                        <?php endforeach; ?>
                    <?php endif; ?>
                </select>
            </div>
            
            <div class="form-group">
                <label for="concerned_departments">الجهات المعنية (مفصولة بفواصل)</label>
                <input type="text" id="concerned_departments" name="concerned_departments" 
                       value="<?php echo htmlspecialchars($transaction['concerned_departments'] ?? ''); ?>"
                       placeholder="مثال: وزارة الداخلية، وزارة المالية">
            </div>
        </div>
        
        <div class="form-row">
            <div class="form-group">
                <label for="reference_number">رقم الصادر</label>
                <input type="text" id="reference_number" name="reference_number" 
                       value="<?php echo htmlspecialchars($transaction['reference_number'] ?? ''); ?>">
            </div>
            
            <div class="form-group">
                <label for="notification_number">رقم الإشعار</label>
                <input type="text" id="notification_number" name="notification_number" 
                       value="<?php echo htmlspecialchars($transaction['notification_number'] ?? ''); ?>">
            </div>
        </div>
        
        <div class="form-row">
            <div class="form-group">
                <label for="status_id">الحالة</label>
                <select id="status_id" name="status_id">
                    <option value="">اختر الحالة</option>
                    <?php if (isset($dropdowns['status'])): ?>
                        <?php foreach ($dropdowns['status'] as $item): ?>
                        <option value="<?php echo $item['id']; ?>" <?php echo $transaction['status_id'] == $item['id'] ? 'selected' : ''; ?>>
                            <?php echo htmlspecialchars($item['display_name']); ?>
                        </option>
                        <?php endforeach; ?>
                    <?php endif; ?>
                </select>
            </div>
            
            <div class="form-group">
                <label for="priority_id">الأولوية</label>
                <select id="priority_id" name="priority_id">
                    <option value="">اختر الأولوية</option>
                    <?php if (isset($dropdowns['priority'])): ?>
                        <?php foreach ($dropdowns['priority'] as $item): ?>
                        <option value="<?php echo $item['id']; ?>" <?php echo $transaction['priority_id'] == $item['id'] ? 'selected' : ''; ?>>
                            <?php echo htmlspecialchars($item['display_name']); ?>
                        </option>
                        <?php endforeach; ?>
                    <?php endif; ?>
                </select>
            </div>
        </div>
        
        <div class="form-row">
            <div class="form-group">
                <label for="follow_up_type_id">نوع المتابعة</label>
                <select id="follow_up_type_id" name="follow_up_type_id">
                    <option value="">اختر نوع المتابعة</option>
                    <?php if (isset($dropdowns['follow_up_type'])): ?>
                        <?php foreach ($dropdowns['follow_up_type'] as $item): ?>
                        <option value="<?php echo $item['id']; ?>" <?php echo $transaction['follow_up_type_id'] == $item['id'] ? 'selected' : ''; ?>>
                            <?php echo htmlspecialchars($item['display_name']); ?>
                        </option>
                        <?php endforeach; ?>
                    <?php endif; ?>
                </select>
            </div>
            
            <div class="form-group">
                <label for="requests_count">عدد الطلبات</label>
                <input type="number" id="requests_count" name="requests_count" 
                       value="<?php echo $transaction['requests_count'] ?? 0; ?>" min="0">
            </div>
        </div>
        
        <div class="form-row">
            <div class="form-group">
                <label for="next_follow_up_date">تاريخ المتابعة القادم (ميلادي)</label>
                <input type="date" id="next_follow_up_date" name="next_follow_up_date" 
                       value="<?php echo $transaction['next_follow_up_date'] ? date('Y-m-d', strtotime($transaction['next_follow_up_date'])) : ''; ?>"
                       onchange="updateHijriDate(this.value)">
            </div>
            
            <div class="form-group">
                <label>تاريخ المتابعة القادم (هجري)</label>
                <input type="text" id="next_follow_up_date_hijri_display" 
                       value="<?php echo htmlspecialchars($transaction['next_follow_up_date_hijri'] ?? ''); ?>"
                       readonly style="background-color: #f8f9fa;">
            </div>
        </div>
        
        <div class="form-group">
            <label for="notes">الملاحظات</label>
            <textarea id="notes" name="notes" rows="4"><?php echo htmlspecialchars($transaction['notes'] ?? ''); ?></textarea>
        </div>
        
        <div class="form-group">
            <label for="attachments">إضافة ملفات مرفقة جديدة</label>
            <input type="file" id="attachments" name="attachments[]" multiple accept=".pdf,.doc,.docx,.jpg,.jpeg,.png,.gif">
            <small style="color: #6c757d; display: block; margin-top: 5px;">يمكن رفع عدة ملفات (PDF, Word, صور)</small>
        </div>
    </div>
    
    <div class="form-actions">
        <button type="submit" class="btn btn-primary">حفظ التغييرات</button>
        <a href="<?php echo $base; ?>/transactions/view.php?id=<?php echo $transaction_id; ?>" class="btn btn-secondary">إلغاء</a>
    </div>
</form>

<?php include __DIR__ . '/../includes/footer.php'; ?>

