<?php
/**
 * إدارة المستخدمين
 */
require_once __DIR__ . '/../config/session.php';
require_once __DIR__ . '/../config/database.php';
require_once __DIR__ . '/../config/base_path.php';

requireAdmin();

$base = getBasePath();

$page_title = 'إدارة المستخدمين';

$error = $_SESSION['error'] ?? '';
$success = $_SESSION['success'] ?? '';
unset($_SESSION['error'], $_SESSION['success']);

$edit_user_id = $_GET['edit_user'] ?? null;
$editing_user = null;

try {
    $pdo = getDBConnection();
    
    // جلب مستخدم للتعديل
    if ($edit_user_id) {
        $stmt = $pdo->prepare("SELECT id, username, full_name, role, is_active FROM users WHERE id = ?");
        $stmt->execute([$edit_user_id]);
        $editing_user = $stmt->fetch();
    }
    
    $stmt = $pdo->query("SELECT id, username, full_name, role, is_active, created_at FROM users ORDER BY created_at DESC");
    $users = $stmt->fetchAll();
    
} catch (PDOException $e) {
    $error = 'حدث خطأ أثناء جلب البيانات';
    $users = [];
}

include __DIR__ . '/../includes/header.php';
?>

<div class="page-header">
    <h1>إدارة المستخدمين</h1>
    <button class="btn btn-primary" onclick="showAddUserForm()">إضافة مستخدم جديد</button>
</div>

<?php if ($error): ?>
<div class="error-message"><?php echo htmlspecialchars($error); ?></div>
<?php endif; ?>

<?php if ($success): ?>
<div class="success-message"><?php echo htmlspecialchars($success); ?></div>
<?php endif; ?>

<div id="add-user-form" class="form-modal" style="display: <?php echo $edit_user_id ? 'flex' : 'none'; ?>;">
    <div class="modal-content">
        <h3><?php echo $editing_user ? 'تعديل مستخدم' : 'إضافة مستخدم جديد'; ?></h3>
        <form method="POST" action="<?php echo $base; ?>/admin/users_actions.php">
            <input type="hidden" name="action" value="<?php echo $editing_user ? 'edit_user' : 'add_user'; ?>">
            <?php if ($editing_user): ?>
            <input type="hidden" name="id" value="<?php echo $editing_user['id']; ?>">
            <?php endif; ?>
            <div class="form-group">
                <label>اسم المستخدم</label>
                <input type="text" name="username" required value="<?php echo htmlspecialchars($editing_user['username'] ?? ''); ?>"
                       <?php echo $editing_user ? 'readonly' : ''; ?>>
                <?php if ($editing_user): ?>
                <small style="color: #6c757d;">لا يمكن تعديل اسم المستخدم</small>
                <?php endif; ?>
            </div>
            <div class="form-group">
                <label>الاسم الكامل</label>
                <input type="text" name="full_name" required value="<?php echo htmlspecialchars($editing_user['full_name'] ?? ''); ?>">
            </div>
            <div class="form-group">
                <label>كلمة المرور</label>
                <input type="password" name="password" <?php echo $editing_user ? '' : 'required'; ?>>
                <?php if ($editing_user): ?>
                <small style="color: #6c757d;">اتركه فارغاً إذا لم ترد تغيير كلمة المرور</small>
                <?php endif; ?>
            </div>
            <div class="form-group">
                <label>الصلاحية</label>
                <select name="role" required>
                    <option value="user" <?php echo ($editing_user && $editing_user['role'] == 'user') ? 'selected' : ''; ?>>مستخدم عادي</option>
                    <option value="admin" <?php echo ($editing_user && $editing_user['role'] == 'admin') ? 'selected' : ''; ?>>مدير</option>
                </select>
            </div>
            <div class="form-actions">
                <button type="submit" class="btn btn-primary">حفظ</button>
                <button type="button" class="btn btn-secondary" onclick="hideAddUserForm()">إلغاء</button>
            </div>
        </form>
    </div>
</div>

<?php if (empty($users)): ?>
<div class="empty-state">
    <p>لا يوجد مستخدمون</p>
</div>
<?php else: ?>
<table class="data-table">
    <thead>
        <tr>
            <th>اسم المستخدم</th>
            <th>الاسم الكامل</th>
            <th>الصلاحية</th>
            <th>الحالة</th>
            <th>تاريخ الإنشاء</th>
            <th>الإجراءات</th>
        </tr>
    </thead>
    <tbody>
        <?php foreach ($users as $user): ?>
        <tr>
            <td><?php echo htmlspecialchars($user['username']); ?></td>
            <td><?php echo htmlspecialchars($user['full_name']); ?></td>
            <td><?php echo $user['role'] === 'admin' ? 'مدير' : 'مستخدم'; ?></td>
            <td>
                <span class="status-badge <?php echo $user['is_active'] ? 'active' : 'inactive'; ?>">
                    <?php echo $user['is_active'] ? 'نشط' : 'معطل'; ?>
                </span>
            </td>
            <td><?php echo date('Y-m-d H:i', strtotime($user['created_at'])); ?></td>
            <td>
                <a href="?edit_user=<?php echo $user['id']; ?>" class="btn btn-sm btn-secondary"><i class="fas fa-edit"></i> تعديل</a>
                <?php if ($user['id'] != $_SESSION['user_id']): ?>
                <a href="<?php echo $base; ?>/admin/users_actions.php?action=toggle_user&id=<?php echo $user['id']; ?>" 
                   class="btn btn-sm <?php echo $user['is_active'] ? 'btn-warning' : 'btn-primary'; ?>"
                   onclick="return confirm('هل أنت متأكد من <?php echo $user['is_active'] ? 'تعطيل' : 'تفعيل'; ?> هذا المستخدم؟')">
                    <i class="fas fa-<?php echo $user['is_active'] ? 'ban' : 'check'; ?>"></i> <?php echo $user['is_active'] ? 'تعطيل' : 'تفعيل'; ?>
                </a>
                <a href="<?php echo $base; ?>/admin/users_actions.php?action=delete_user&id=<?php echo $user['id']; ?>" 
                   class="btn btn-sm btn-danger"
                   onclick="return confirm('هل أنت متأكد من حذف هذا المستخدم؟')">
                    <i class="fas fa-trash"></i> حذف
                </a>
                <?php endif; ?>
            </td>
        </tr>
        <?php endforeach; ?>
    </tbody>
</table>
<?php endif; ?>

<script>
function showAddUserForm() {
    document.getElementById('add-user-form').style.display = 'flex';
}

function hideAddUserForm() {
    document.getElementById('add-user-form').style.display = 'none';
    const form = document.getElementById('add-user-form').querySelector('form');
    if (form) {
        form.reset();
        // إزالة hidden input للـ id إذا كان موجوداً
        const idInput = form.querySelector('input[name="id"]');
        if (idInput) idInput.remove();
        // تغيير action إلى add_user
        const actionInput = form.querySelector('input[name="action"]');
        if (actionInput) actionInput.value = 'add_user';
        // تحديث العنوان
        const title = form.closest('.modal-content').querySelector('h3');
        if (title) title.textContent = 'إضافة مستخدم جديد';
        // إزالة readonly
        const usernameInput = form.querySelector('input[name="username"]');
        if (usernameInput) usernameInput.removeAttribute('readonly');
        // جعل password required
        const passwordInput = form.querySelector('input[name="password"]');
        if (passwordInput) passwordInput.setAttribute('required', 'required');
    }
    window.location.href = window.location.pathname;
}
</script>

<?php include __DIR__ . '/../includes/footer.php'; ?>

