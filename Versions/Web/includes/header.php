<?php
/**
 * رأس الصفحة المشترك
 */
require_once __DIR__ . '/../config/base_path.php';

if (!isset($page_title)) {
    $page_title = 'نظام إدارة المعاملات الرسمية';
}

$base = getBasePath();
?>
<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title><?php echo htmlspecialchars($page_title); ?></title>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <link rel="stylesheet" href="<?php echo $base; ?>/assets/css/style.css">
</head>
<body>
    <?php if (isLoggedIn()): ?>
    <nav class="main-nav">
        <div class="nav-container">
            <div class="nav-brand">
                <h1>نظام إدارة المعاملات الحكومية</h1>
            </div>
            <div class="nav-menu">
                <a href="<?php echo $base; ?>/index.php"><i class="fas fa-home"></i> الرئيسية</a>
                <a href="<?php echo $base; ?>/transactions/list.php"><i class="fas fa-list"></i> المعاملات</a>
                <a href="<?php echo $base; ?>/transactions/add.php"><i class="fas fa-plus-circle"></i> إضافة معاملة</a>
                <?php if (isAdmin()): ?>
                <a href="<?php echo $base; ?>/admin/dropdowns.php"><i class="fas fa-cog"></i> إدارة القوائم</a>
                <a href="<?php echo $base; ?>/admin/users.php"><i class="fas fa-users"></i> إدارة المستخدمين</a>
                <?php endif; ?>
                <a href="<?php echo $base; ?>/logout.php" class="logout-btn"><i class="fas fa-sign-out-alt"></i> تسجيل الخروج</a>
            </div>
        </div>
    </nav>
    <?php endif; ?>
    <main class="main-content">

