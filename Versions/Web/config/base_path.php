<?php
/**
 * حساب المسار الأساسي للمشروع
 */
function getBasePath() {
    // الحصول على مسار الملف الحالي
    $script_path = $_SERVER['SCRIPT_NAME'];
    
    // إزالة اسم الملف والحصول على المجلد فقط
    $dir = dirname($script_path);
    
    // إزالة المسارات الفرعية للوصول إلى الجذر
    // إذا كان الملف في transactions/ أو admin/ أو includes/، نعود مستوى واحد
    if (strpos($script_path, '/transactions/') !== false || 
        strpos($script_path, '/admin/') !== false || 
        strpos($script_path, '/includes/') !== false) {
        $dir = dirname($dir);
    }
    
    // إزالة الشرطة المائلة الأخيرة إذا كانت موجودة
    $dir = rtrim($dir, '/');
    
    // إرجاع المسار مع شرطة مائلة في البداية
    return $dir === '.' ? '' : $dir;
}

