<?php
/**
 * إعدادات الجلسات
 */

// بدء الجلسة إذا لم تكن بدأت
if (session_status() === PHP_SESSION_NONE) {
    session_start();
}

/**
 * التحقق من تسجيل الدخول
 */
function isLoggedIn() {
    return isset($_SESSION['user_id']) && isset($_SESSION['username']);
}

/**
 * التحقق من صلاحيات المدير
 */
function isAdmin() {
    return isset($_SESSION['role']) && $_SESSION['role'] === 'admin';
}

/**
 * إعادة التوجيه إذا لم يكن المستخدم مسجل دخول
 */
function requireLogin() {
    if (!isLoggedIn()) {
        require_once __DIR__ . '/base_path.php';
        header('Location: ' . getBasePath() . '/login.php');
        exit();
    }
}

/**
 * إعادة التوجيه إذا لم يكن المستخدم مدير
 */
function requireAdmin() {
    requireLogin();
    if (!isAdmin()) {
        require_once __DIR__ . '/base_path.php';
        header('Location: ' . getBasePath() . '/index.php');
        exit();
    }
}

