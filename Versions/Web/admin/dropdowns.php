<?php
/**
 * إدارة القوائم المنسدلة
 */
require_once __DIR__ . '/../config/session.php';
require_once __DIR__ . '/../config/database.php';
require_once __DIR__ . '/../config/base_path.php';

requireAdmin();

$base = getBasePath();

$page_title = 'إدارة القوائم المنسدلة';

$error = $_SESSION['error'] ?? '';
$success = $_SESSION['success'] ?? '';
unset($_SESSION['error'], $_SESSION['success']);

$edit_type_id = $_GET['edit_type'] ?? null;
$edit_item_id = $_GET['edit_item'] ?? null;
$editing_type = null;
$editing_item = null;

try {
    $pdo = getDBConnection();
    
    // جلب نوع للتعديل
    if ($edit_type_id) {
        $stmt = $pdo->prepare("SELECT * FROM dropdown_types WHERE id = ?");
        $stmt->execute([$edit_type_id]);
        $editing_type = $stmt->fetch();
    }
    
    // جلب عنصر للتعديل
    if ($edit_item_id) {
        $stmt = $pdo->prepare("SELECT * FROM dropdown_items WHERE id = ?");
        $stmt->execute([$edit_item_id]);
        $editing_item = $stmt->fetch();
    }
    
    // جلب أنواع القوائم
    $stmt = $pdo->query("SELECT * FROM dropdown_types ORDER BY created_at DESC");
    $dropdown_types = $stmt->fetchAll();
    
    // جلب العناصر لكل نوع
    $items_by_type = [];
    foreach ($dropdown_types as $type) {
        $stmt = $pdo->prepare("SELECT * FROM dropdown_items WHERE dropdown_type_id = ? ORDER BY display_order, id");
        $stmt->execute([$type['id']]);
        $items_by_type[$type['id']] = $stmt->fetchAll();
    }
    
} catch (PDOException $e) {
    $error = 'حدث خطأ أثناء جلب البيانات';
    $dropdown_types = [];
    $items_by_type = [];
}

include __DIR__ . '/../includes/header.php';
?>

<div class="page-header">
    <h1>إدارة القوائم المنسدلة</h1>
</div>

<div class="admin-section">
    <div class="section-tabs">
        <button class="tab-btn active" onclick="showTab('types')">أنواع القوائم</button>
        <button class="tab-btn" onclick="showTab('items')">عناصر القوائم</button>
    </div>
    
    <!-- تبويب أنواع القوائم -->
    <div id="types-tab" class="tab-content active">
        <div class="section-header">
            <h2>أنواع القوائم المنسدلة</h2>
            <button class="btn btn-primary" onclick="showAddTypeForm()">إضافة نوع جديد</button>
        </div>
        
        <div id="add-type-form" class="form-modal" style="display: <?php echo $edit_type_id ? 'flex' : 'none'; ?>;">
            <div class="modal-content">
                <h3><?php echo $editing_type ? 'تعديل نوع القائمة' : 'إضافة نوع قائمة جديد'; ?></h3>
                <form method="POST" action="<?php echo $base; ?>/admin/dropdowns_actions.php" id="type-form">
                    <input type="hidden" name="action" value="<?php echo $editing_type ? 'edit_type' : 'add_type'; ?>">
                    <?php if ($editing_type): ?>
                    <input type="hidden" name="id" value="<?php echo $editing_type['id']; ?>">
                    <?php endif; ?>
                    <div class="form-group">
                        <label>اسم النوع (بالإنجليزية)</label>
                        <input type="text" name="name" required pattern="[a-z_]+" placeholder="مثال: status" 
                               value="<?php echo htmlspecialchars($editing_type['name'] ?? ''); ?>" 
                               <?php echo $editing_type ? 'readonly' : ''; ?>>
                        <?php if ($editing_type): ?>
                        <small style="color: #6c757d;">لا يمكن تعديل اسم النوع</small>
                        <?php endif; ?>
                    </div>
                    <div class="form-group">
                        <label>اسم العرض</label>
                        <input type="text" name="display_name" required value="<?php echo htmlspecialchars($editing_type['display_name'] ?? ''); ?>">
                    </div>
                    <div class="form-group">
                        <label>الوصف</label>
                        <textarea name="description" rows="3"><?php echo htmlspecialchars($editing_type['description'] ?? ''); ?></textarea>
                    </div>
                    <div class="form-actions">
                        <button type="submit" class="btn btn-primary">حفظ</button>
                        <button type="button" class="btn btn-secondary" onclick="hideAddTypeForm()">إلغاء</button>
                    </div>
                </form>
            </div>
        </div>
        
        <?php if (empty($dropdown_types)): ?>
        <div class="empty-state">
            <p>لا توجد أنواع قوائم</p>
        </div>
        <?php else: ?>
        <table class="data-table">
            <thead>
                <tr>
                    <th>اسم النوع</th>
                    <th>اسم العرض</th>
                    <th>الوصف</th>
                    <th>عدد العناصر</th>
                    <th>الإجراءات</th>
                </tr>
            </thead>
            <tbody>
                <?php foreach ($dropdown_types as $type): ?>
                <tr>
                    <td><?php echo htmlspecialchars($type['name']); ?></td>
                    <td><?php echo htmlspecialchars($type['display_name']); ?></td>
                    <td><?php echo htmlspecialchars($type['description'] ?? '-'); ?></td>
                    <td><?php echo count($items_by_type[$type['id']] ?? []); ?></td>
                    <td>
                        <a href="?edit_type=<?php echo $type['id']; ?>" class="btn btn-sm btn-secondary"><i class="fas fa-edit"></i> تعديل</a>
                        <a href="<?php echo $base; ?>/admin/dropdowns_actions.php?action=delete_type&id=<?php echo $type['id']; ?>" 
                           class="btn btn-sm btn-danger" 
                           onclick="return confirm('هل أنت متأكد من حذف هذا النوع وجميع عناصره؟')"><i class="fas fa-trash"></i> حذف</a>
                    </td>
                </tr>
                <?php endforeach; ?>
            </tbody>
        </table>
        <?php endif; ?>
    </div>
    
    <!-- تبويب عناصر القوائم -->
    <div id="items-tab" class="tab-content">
        <div class="section-header">
            <h2>عناصر القوائم المنسدلة</h2>
            <button class="btn btn-primary" onclick="showAddItemForm()">إضافة عنصر جديد</button>
        </div>
        
        <div id="add-item-form" class="form-modal" style="display: <?php echo $edit_item_id ? 'flex' : 'none'; ?>;">
            <div class="modal-content">
                <h3><?php echo $editing_item ? 'تعديل عنصر' : 'إضافة عنصر جديد'; ?></h3>
                <form method="POST" action="<?php echo $base; ?>/admin/dropdowns_actions.php" id="item-form">
                    <input type="hidden" name="action" value="<?php echo $editing_item ? 'edit_item' : 'add_item'; ?>">
                    <?php if ($editing_item): ?>
                    <input type="hidden" name="id" value="<?php echo $editing_item['id']; ?>">
                    <?php endif; ?>
                    <div class="form-group">
                        <label>نوع القائمة</label>
                        <select name="dropdown_type_id" required <?php echo $editing_item ? 'disabled' : ''; ?>>
                            <option value="">اختر النوع</option>
                            <?php foreach ($dropdown_types as $type): ?>
                            <option value="<?php echo $type['id']; ?>" 
                                    <?php echo ($editing_item && $editing_item['dropdown_type_id'] == $type['id']) ? 'selected' : ''; ?>>
                                <?php echo htmlspecialchars($type['display_name']); ?>
                            </option>
                            <?php endforeach; ?>
                        </select>
                        <?php if ($editing_item): ?>
                        <input type="hidden" name="dropdown_type_id" value="<?php echo $editing_item['dropdown_type_id']; ?>">
                        <small style="color: #6c757d;">لا يمكن تغيير نوع القائمة</small>
                        <?php endif; ?>
                    </div>
                    <div class="form-group">
                        <label>اسم العنصر (بالإنجليزية)</label>
                        <input type="text" name="name" required pattern="[a-z_]+" placeholder="مثال: pending"
                               value="<?php echo htmlspecialchars($editing_item['name'] ?? ''); ?>"
                               <?php echo $editing_item ? 'readonly' : ''; ?>>
                        <?php if ($editing_item): ?>
                        <small style="color: #6c757d;">لا يمكن تعديل اسم العنصر</small>
                        <?php endif; ?>
                    </div>
                    <div class="form-group">
                        <label>اسم العرض</label>
                        <input type="text" name="display_name" required value="<?php echo htmlspecialchars($editing_item['display_name'] ?? ''); ?>">
                    </div>
                    <div class="form-group">
                        <label>ترتيب العرض</label>
                        <input type="number" name="display_order" value="<?php echo $editing_item['display_order'] ?? 0; ?>" min="0">
                    </div>
                    <div class="form-group">
                        <label>
                            <input type="checkbox" name="is_active" value="1" <?php echo ($editing_item && $editing_item['is_active']) ? 'checked' : 'checked'; ?>> نشط
                        </label>
                    </div>
                    <div class="form-actions">
                        <button type="submit" class="btn btn-primary">حفظ</button>
                        <button type="button" class="btn btn-secondary" onclick="hideAddItemForm()">إلغاء</button>
                    </div>
                </form>
            </div>
        </div>
        
        <?php if (empty($dropdown_types)): ?>
        <div class="empty-state">
            <p>لا توجد عناصر. يرجى إضافة أنواع قوائم أولاً</p>
        </div>
        <?php else: ?>
        <?php foreach ($dropdown_types as $type): ?>
        <div class="items-group">
            <h3><?php echo htmlspecialchars($type['display_name']); ?></h3>
            <?php if (empty($items_by_type[$type['id']])): ?>
            <p class="empty-state">لا توجد عناصر</p>
            <?php else: ?>
            <table class="data-table">
                <thead>
                    <tr>
                        <th>اسم العنصر</th>
                        <th>اسم العرض</th>
                        <th>الترتيب</th>
                        <th>الحالة</th>
                        <th>الإجراءات</th>
                    </tr>
                </thead>
                <tbody>
                    <?php foreach ($items_by_type[$type['id']] as $item): ?>
                    <tr>
                        <td><?php echo htmlspecialchars($item['name']); ?></td>
                        <td><?php echo htmlspecialchars($item['display_name']); ?></td>
                        <td><?php echo $item['display_order']; ?></td>
                        <td>
                            <span class="status-badge <?php echo $item['is_active'] ? 'active' : 'inactive'; ?>">
                                <?php echo $item['is_active'] ? 'نشط' : 'غير نشط'; ?>
                            </span>
                        </td>
                        <td>
                            <a href="?edit_item=<?php echo $item['id']; ?>" class="btn btn-sm btn-secondary"><i class="fas fa-edit"></i> تعديل</a>
                            <a href="<?php echo $base; ?>/admin/dropdowns_actions.php?action=delete_item&id=<?php echo $item['id']; ?>" 
                               class="btn btn-sm btn-danger" 
                               onclick="return confirm('هل أنت متأكد من حذف هذا العنصر؟')"><i class="fas fa-trash"></i> حذف</a>
                        </td>
                    </tr>
                    <?php endforeach; ?>
                </tbody>
            </table>
            <?php endif; ?>
        </div>
        <?php endforeach; ?>
        <?php endif; ?>
    </div>
</div>

<script>
function showTab(tab) {
    document.querySelectorAll('.tab-content').forEach(t => t.classList.remove('active'));
    document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
    
    document.getElementById(tab + '-tab').classList.add('active');
    event.target.classList.add('active');
}

function showAddTypeForm() {
    document.getElementById('add-type-form').style.display = 'flex';
}

function hideAddTypeForm() {
    document.getElementById('add-type-form').style.display = 'none';
    const form = document.getElementById('type-form');
    if (form) {
        form.reset();
        // إزالة hidden input للـ id إذا كان موجوداً
        const idInput = form.querySelector('input[name="id"]');
        if (idInput) idInput.remove();
        // تغيير action إلى add_type
        const actionInput = form.querySelector('input[name="action"]');
        if (actionInput) actionInput.value = 'add_type';
        // تحديث العنوان
        const title = form.closest('.modal-content').querySelector('h3');
        if (title) title.textContent = 'إضافة نوع قائمة جديد';
        // إزالة readonly
        const nameInput = form.querySelector('input[name="name"]');
        if (nameInput) nameInput.removeAttribute('readonly');
    }
    window.location.href = window.location.pathname;
}

function showAddItemForm() {
    document.getElementById('add-item-form').style.display = 'flex';
}

function hideAddItemForm() {
    document.getElementById('add-item-form').style.display = 'none';
    const form = document.getElementById('item-form');
    if (form) {
        form.reset();
        // إزالة hidden input للـ id إذا كان موجوداً
        const idInput = form.querySelector('input[name="id"]');
        if (idInput) idInput.remove();
        // تغيير action إلى add_item
        const actionInput = form.querySelector('input[name="action"]');
        if (actionInput) actionInput.value = 'add_item';
        // تحديث العنوان
        const title = form.closest('.modal-content').querySelector('h3');
        if (title) title.textContent = 'إضافة عنصر جديد';
        // إزالة readonly و disabled
        const nameInput = form.querySelector('input[name="name"]');
        if (nameInput) {
            nameInput.removeAttribute('readonly');
            nameInput.value = '';
        }
        const selectInput = form.querySelector('select[name="dropdown_type_id"]');
        if (selectInput) {
            selectInput.removeAttribute('disabled');
            selectInput.value = '';
        }
    }
    // إزالة edit_item من URL
    const url = new URL(window.location);
    url.searchParams.delete('edit_item');
    window.history.replaceState({}, '', url);
}
</script>

<?php include __DIR__ . '/../includes/footer.php'; ?>

