<?php
/**
 * API لتحويل التاريخ بين الميلادي والهجري باستخدام API دقيق
 */
require_once __DIR__ . '/../config/base_path.php';

header('Content-Type: application/json');

$date = $_GET['date'] ?? '';

if (empty($date)) {
    echo json_encode(['success' => false, 'error' => 'تاريخ غير صحيح']);
    exit;
}

// استخدام API دقيق لتحويل التاريخ
$date_parts = explode('-', $date);
if (count($date_parts) != 3) {
    echo json_encode(['success' => false, 'error' => 'صيغة تاريخ غير صحيحة']);
    exit;
}

$year = (int)$date_parts[0];
$month = (int)$date_parts[1];
$day = (int)$date_parts[2];

// استخدام API من aladhan.com (دقيق ومجاني)
$api_url = "https://api.aladhan.com/v1/gToH/{$day}-{$month}-{$year}";

$ch = curl_init();
curl_setopt($ch, CURLOPT_URL, $api_url);
curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
curl_setopt($ch, CURLOPT_TIMEOUT, 10);
curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
curl_setopt($ch, CURLOPT_FOLLOWLOCATION, true);
$response = curl_exec($ch);
$http_code = curl_getinfo($ch, CURLINFO_HTTP_CODE);
$curl_error = curl_error($ch);
curl_close($ch);

if ($http_code == 200 && $response && empty($curl_error)) {
    $data = json_decode($response, true);
    if (isset($data['data']['hijri'])) {
        $hijri = $data['data']['hijri'];
        $hYear = $hijri['year'];
        $hMonth = str_pad($hijri['month']['number'], 2, '0', STR_PAD_LEFT);
        $hDay = str_pad($hijri['day'], 2, '0', STR_PAD_LEFT);
        $hijri_date = "{$hYear}-{$hMonth}-{$hDay}";
        echo json_encode(['success' => true, 'hijri' => $hijri_date, 'hijri_formatted' => $hijri['date']]);
        exit;
    }
}

// بديل: استخدام API آخر
$api_url2 = "https://api.hijri-api.com/v1/hijri?year={$year}&month={$month}&day={$day}";
$ch2 = curl_init();
curl_setopt($ch2, CURLOPT_URL, $api_url2);
curl_setopt($ch2, CURLOPT_RETURNTRANSFER, true);
curl_setopt($ch2, CURLOPT_TIMEOUT, 10);
curl_setopt($ch2, CURLOPT_SSL_VERIFYPEER, false);
curl_setopt($ch2, CURLOPT_FOLLOWLOCATION, true);
$response2 = curl_exec($ch2);
$http_code2 = curl_getinfo($ch2, CURLINFO_HTTP_CODE);
curl_close($ch2);

if ($http_code2 == 200 && $response2) {
    $data2 = json_decode($response2, true);
    if (isset($data2['hijri'])) {
        $hijri2 = $data2['hijri'];
        $hijri_date = sprintf('%04d-%02d-%02d', $hijri2['year'], $hijri2['month'], $hijri2['day']);
        echo json_encode(['success' => true, 'hijri' => $hijri_date]);
        exit;
    }
}

// إذا فشل API، استخدام الدالة المحلية
require_once __DIR__ . '/../config/hijri_converter.php';
$hijri = gregorianToHijri($date);

if (!empty($hijri)) {
    echo json_encode(['success' => true, 'hijri' => $hijri]);
} else {
    echo json_encode(['success' => false, 'error' => 'فشل تحويل التاريخ']);
}

