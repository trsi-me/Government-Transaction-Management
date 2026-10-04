<?php
/**
 * صفحة تسجيل الخروج
 */
require_once __DIR__ . '/config/session.php';
require_once __DIR__ . '/config/base_path.php';

// تدمير الجلسة
session_destroy();

// إعادة التوجيه إلى صفحة تسجيل الدخول
header('Location: ' . getBasePath() . '/login.php');
exit();

