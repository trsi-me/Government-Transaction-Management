<?php
/**
 * إضافة معاملة جديدة
 */
require_once __DIR__ . '/../config/session.php';
require_once __DIR__ . '/../config/database.php';
require_once __DIR__ . '/../config/base_path.php';
require_once __DIR__ . '/../config/hijri_converter.php';

requireLogin();

$base = getBasePath();

$page_title = 'إضافة معاملة جديدة';

$error = '';
$success = '';

// جلب القوائم المنسدلة
try {
    $pdo = getDBConnection();
    
    $stmt = $pdo->query("SELECT dt.name as type_name, di.id, di.display_name, di.display_order
                         FROM dropdown_items di
                         INNER JOIN dropdown_types dt ON di.dropdown_type_id = dt.id
                         WHERE di.is_active = 1
                         ORDER BY dt.name, di.display_order");
    $dropdown_items = $stmt->fetchAll();
    
    // تجميع العناصر حسب النوع
    $dropdowns = [];
    foreach ($dropdown_items as $item) {
        $dropdowns[$item['type_name']][] = $item;
    }
    
} catch (PDOException $e) {
    $error = 'حدث خطأ أثناء جلب البيانات';
    $dropdowns = [];
}

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $title = trim($_POST['title'] ?? '');
    $description = trim($_POST['description'] ?? '');
    $transaction_number = trim($_POST['transaction_number'] ?? '');
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
            
            // إنشاء رقم معاملة تلقائي إذا لم يتم إدخاله
            if (empty($transaction_number)) {
                $year = date('Y');
                $stmt = $pdo->query("SELECT COUNT(*) as count FROM transactions WHERE YEAR(created_at) = $year");
                $count = $stmt->fetch()['count'];
                $transaction_number = 'TRX-' . $year . '-' . str_pad($count + 1, 6, '0', STR_PAD_LEFT);
            } else {
                // التحقق من عدم تكرار الرقم
                $stmt = $pdo->prepare("SELECT id FROM transactions WHERE transaction_number = ?");
                $stmt->execute([$transaction_number]);
                if ($stmt->fetch()) {
                    $error = 'رقم المعاملة موجود بالفعل';
                    include __DIR__ . '/../includes/header.php';
                    // عرض النموذج مرة أخرى
                    include __DIR__ . '/../includes/footer.php';
                    exit();
                }
            }
            
            // إدراج المعاملة
            $stmt = $pdo->prepare("INSERT INTO transactions 
                                   (transaction_number, title, description, department_id, source_department_id, 
                                    concerned_departments, status_id, priority_id, transaction_type_id, 
                                    follow_up_type_id, reference_number, notification_number, 
                                    next_follow_up_date, next_follow_up_date_hijri, notes, requests_count, created_by)
                                   VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
            $stmt->execute([
                $transaction_number,
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
                $_SESSION['user_id']
            ]);
            
            $transaction_id = $pdo->lastInsertId();
            
            // معالجة الملفات المرفقة
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
                        }
                    }
                }
            }
            
            // تسجيل في سجل الحركة
            $stmt = $pdo->prepare("INSERT INTO transaction_logs (transaction_id, user_id, action_type, notes)
                                   VALUES (?, ?, 'created', ?)");
            $stmt->execute([
                $transaction_id,
                $_SESSION['user_id'],
                'تم إنشاء المعاملة'
            ]);
            
            $success = 'تم إضافة المعاملة بنجاح';
            header('Location: ' . $base . '/transactions/view.php?id=' . $transaction_id);
            exit();
            
        } catch (PDOException $e) {
            $error = 'حدث خطأ أثناء إضافة المعاملة: ' . $e->getMessage();
        }
    }
}

include __DIR__ . '/../includes/header.php';
?>

<div class="page-header">
    <h1>إضافة معاملة جديدة</h1>
    <a href="<?php echo $base; ?>/transactions/list.php" class="btn btn-secondary">العودة للقائمة</a>
</div>

<?php if ($error): ?>
<div class="error-message"><?php echo htmlspecialchars($error); ?></div>
<?php endif; ?>

<?php if ($success): ?>
<div class="success-message"><?php echo htmlspecialchars($success); ?></div>
<?php endif; ?>

<form method="POST" action="<?php echo $base; ?>/transactions/add.php" class="transaction-form" enctype="multipart/form-data">
    <div class="form-section">
        <h2>معلومات المعاملة</h2>
        
        <div class="form-row">
            <div class="form-group">
                <label for="transaction_number">رقم المعاملة</label>
                <input type="text" id="transaction_number" name="transaction_number" placeholder="اتركه فارغاً للإنشاء التلقائي">
                <small style="color: #6c757d; display: block; margin-top: 5px; font-size: 13px;">إذا تركت الحقل فارغاً سيتم إنشاء رقم تلقائياً</small>
            </div>
            
            <div class="form-group">
                <label for="title">العنوان <span class="required">*</span></label>
                <input type="text" id="title" name="title" required>
            </div>
        </div>
        
        <div class="form-group">
            <label for="description">الوصف</label>
            <textarea id="description" name="description" rows="5"></textarea>
        </div>
        
        <div class="form-row">
            <div class="form-group">
                <label for="transaction_type_id">نوع المعاملة</label>
                <select id="transaction_type_id" name="transaction_type_id">
                    <option value="">اختر النوع</option>
                    <?php if (isset($dropdowns['transaction_type'])): ?>
                        <?php foreach ($dropdowns['transaction_type'] as $item): ?>
                        <option value="<?php echo $item['id']; ?>"><?php echo htmlspecialchars($item['display_name']); ?></option>
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
                        <option value="<?php echo $item['id']; ?>"><?php echo htmlspecialchars($item['display_name']); ?></option>
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
                        <option value="<?php echo $item['id']; ?>"><?php echo htmlspecialchars($item['display_name']); ?></option>
                        <?php endforeach; ?>
                    <?php endif; ?>
                </select>
            </div>
            
            <div class="form-group">
                <label for="concerned_departments">الجهات المعنية (مفصولة بفواصل)</label>
                <input type="text" id="concerned_departments" name="concerned_departments" placeholder="مثال: وزارة الداخلية، وزارة المالية">
            </div>
        </div>
        
        <div class="form-row">
            <div class="form-group">
                <label for="reference_number">رقم الصادر</label>
                <input type="text" id="reference_number" name="reference_number">
            </div>
            
            <div class="form-group">
                <label for="notification_number">رقم الإشعار</label>
                <input type="text" id="notification_number" name="notification_number">
            </div>
        </div>
        
        <div class="form-row">
            <div class="form-group">
                <label for="status_id">الحالة</label>
                <select id="status_id" name="status_id">
                    <option value="">اختر الحالة</option>
                    <?php if (isset($dropdowns['status'])): ?>
                        <?php foreach ($dropdowns['status'] as $item): ?>
                        <option value="<?php echo $item['id']; ?>"><?php echo htmlspecialchars($item['display_name']); ?></option>
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
                        <option value="<?php echo $item['id']; ?>"><?php echo htmlspecialchars($item['display_name']); ?></option>
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
                        <option value="<?php echo $item['id']; ?>"><?php echo htmlspecialchars($item['display_name']); ?></option>
                        <?php endforeach; ?>
                    <?php endif; ?>
                </select>
            </div>
            
            <div class="form-group">
                <label for="requests_count">عدد الطلبات</label>
                <input type="number" id="requests_count" name="requests_count" min="0" value="0">
            </div>
        </div>
        
        <div class="form-row">
            <div class="form-group">
                <label for="next_follow_up_date">تاريخ المتابعة القادم (ميلادي)</label>
                <input type="date" id="next_follow_up_date" name="next_follow_up_date" onchange="updateHijriDate(this.value)">
            </div>
            
            <div class="form-group">
                <label>تاريخ المتابعة القادم (هجري)</label>
                <input type="text" id="next_follow_up_date_hijri_display" readonly style="background-color: #f8f9fa;">
            </div>
        </div>
        
        <div class="form-group">
            <label for="notes">الملاحظات</label>
            <textarea id="notes" name="notes" rows="4"></textarea>
        </div>
        
        <div class="form-group">
            <label for="attachments">إرفاق مستندات</label>
            <input type="file" id="attachments" name="attachments[]" multiple accept=".pdf,.doc,.docx,.jpg,.jpeg,.png,.gif">
            <small style="color: #6c757d; display: block; margin-top: 5px;">يمكن رفع عدة ملفات (PDF, Word, صور)</small>
        </div>
    </div>
    
    <div class="form-actions">
        <button type="submit" class="btn btn-primary">حفظ</button>
        <a href="<?php echo $base; ?>/transactions/list.php" class="btn btn-secondary">إلغاء</a>
    </div>
</form>

<script>
function updateHijriDate(gregorianDate) {
    if (!gregorianDate) {
        document.getElementById('next_follow_up_date_hijri_display').value = '';
        return;
    }
    
    // استخدام API لتحويل التاريخ
    fetch('<?php echo $base; ?>/api/convert_date.php?date=' + gregorianDate)
        .then(response => response.json())
        .then(data => {
            if (data.success && data.hijri) {
                document.getElementById('next_follow_up_date_hijri_display').value = data.hijri;
            } else {
                document.getElementById('next_follow_up_date_hijri_display').value = 'خطأ في التحويل';
                console.error('Error converting date:', data.error);
            }
        })
        .catch(error => {
            console.error('Error converting date:', error);
            document.getElementById('next_follow_up_date_hijri_display').value = 'خطأ في الاتصال';
        });
}
</script>

<?php include __DIR__ . '/../includes/footer.php'; ?>

