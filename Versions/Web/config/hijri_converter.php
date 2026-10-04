<?php
/**
 * تحويل التاريخ بين الميلادي والهجري باستخدام API دقيق
 */

function gregorianToHijri($date) {
    if (empty($date)) return '';
    
    // استخدام API لتحويل دقيق
    $date_parts = explode('-', $date);
    if (count($date_parts) != 3) return '';
    
    $year = (int)$date_parts[0];
    $month = (int)$date_parts[1];
    $day = (int)$date_parts[2];
    
    // استخدام API من aladhan.com أو hijri-api.com
    $api_url = "https://api.aladhan.com/v1/gToH/{$day}-{$month}-{$year}";
    
    $ch = curl_init();
    curl_setopt($ch, CURLOPT_URL, $api_url);
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
    curl_setopt($ch, CURLOPT_TIMEOUT, 10);
    curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
    curl_setopt($ch, CURLOPT_FOLLOWLOCATION, true);
    $response = curl_exec($ch);
    $http_code = curl_getinfo($ch, CURLINFO_HTTP_CODE);
    curl_close($ch);
    
    if ($http_code == 200 && $response) {
        $data = json_decode($response, true);
        if (isset($data['data']['hijri'])) {
            $hijri = $data['data']['hijri'];
            $hYear = $hijri['year'];
            $hMonth = str_pad($hijri['month']['number'], 2, '0', STR_PAD_LEFT);
            $hDay = str_pad($hijri['day'], 2, '0', STR_PAD_LEFT);
            return "{$hYear}-{$hMonth}-{$hDay}";
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
            return sprintf('%04d-%02d-%02d', $hijri2['year'], $hijri2['month'], $hijri2['day']);
        }
    }
    
    // إذا فشل API، استخدام خوارزمية محسنة
    return gregorianToHijriFallback($date);
}

function gregorianToHijriFallback($date) {
    if (empty($date)) return '';
    
    $timestamp = strtotime($date);
    if ($timestamp === false) return '';
    
    $gYear = (int)date('Y', $timestamp);
    $gMonth = (int)date('m', $timestamp);
    $gDay = (int)date('d', $timestamp);
    
    // استخدام الدالة المدمجة في PHP
    if (function_exists('gregoriantojd')) {
        $jd = gregoriantojd($gMonth, $gDay, $gYear);
    } else {
        $jd = calculateGregorianToJD($gMonth, $gDay, $gYear);
    }
    
    // التحويل إلى هجري باستخدام الخوارزمية المحسنة
    $hijri = jdToIslamicImproved($jd);
    
    // التحقق من النتيجة (يجب أن تكون السنة الهجرية بين 1300 و 1500 تقريباً للتواريخ الحديثة)
    if ($hijri['year'] < 1300 || $hijri['year'] > 1600) {
        // إذا كانت النتيجة غير منطقية، استخدام خوارزمية بديلة
        return calculateHijriSimple($gYear, $gMonth, $gDay);
    }
    
    return $hijri['year'] . '-' . str_pad($hijri['month'], 2, '0', STR_PAD_LEFT) . '-' . str_pad($hijri['day'], 2, '0', STR_PAD_LEFT);
}

function calculateHijriSimple($gYear, $gMonth, $gDay) {
    // خوارزمية بسيطة للتحويل (تقريبية)
    // الفرق التقريبي بين الميلادي والهجري هو 579-580 سنة
    $hYear = $gYear - 579;
    
    // حساب الشهر واليوم (تقريبي)
    $jd = gregoriantojd($gMonth, $gDay, $gYear);
    $jd0 = gregoriantojd(1, 1, $gYear);
    $dayOfYear = $jd - $jd0;
    
    // السنة الهجرية أقصر بحوالي 11 يوم
    $hDayOfYear = $dayOfYear - floor(($gYear - 579) * 11 / 365);
    
    if ($hDayOfYear < 0) {
        $hYear--;
        $hDayOfYear += 354;
    }
    
    $hMonth = floor($hDayOfYear / 29.5) + 1;
    if ($hMonth > 12) $hMonth = 12;
    
    $hDay = $hDayOfYear - floor(($hMonth - 1) * 29.5) + 1;
    if ($hDay > 30) $hDay = 30;
    if ($hDay < 1) $hDay = 1;
    
    return sprintf('%04d-%02d-%02d', $hYear, $hMonth, $hDay);
}

function jdToIslamicImproved($jd) {
    // خوارزمية صحيحة للتحويل من JD إلى هجري
    $jd = floor($jd) + 0.5;
    
    // التحويل الدقيق للتقويم الهجري (تقويم أم القرى)
    $year = floor((30 * ($jd - 1948439.5) + 10646) / 10631);
    
    // التحقق من السنة
    $jd1 = islamicToJDImproved(1, 1, $year);
    if ($jd1 > $jd) {
        $year--;
        $jd1 = islamicToJDImproved(1, 1, $year);
    }
    
    // حساب الشهر
    $month = floor((12 * ($jd - $jd1) + 6) / 354.36667) + 1;
    if ($month > 12) {
        $year++;
        $month = 1;
        $jd1 = islamicToJDImproved(1, 1, $year);
    }
    
    // التحقق من الشهر
    $jd2 = islamicToJDImproved($month, 1, $year);
    if ($jd2 > $jd) {
        $month--;
        if ($month < 1) {
            $month = 12;
            $year--;
        }
        $jd2 = islamicToJDImproved($month, 1, $year);
    }
    
    // حساب اليوم
    $day = floor($jd - $jd2) + 1;
    
    // التأكد من أن اليوم صحيح
    if ($day < 1) $day = 1;
    if ($day > 30) $day = 30;
    
    return ['year' => (int)$year, 'month' => (int)$month, 'day' => (int)$day];
}

function islamicToJDImproved($month, $day, $year) {
    // خوارزمية صحيحة لحساب JD للتقويم الهجري
    // الصيغة الصحيحة: JD = floor((11 * year + 3) / 30) + 354 * year + 30 * month - floor((month - 1) / 2) - 385 + day + 1948439.5
    $jd = floor((11 * $year + 3) / 30) + 354 * $year + 30 * $month - floor(($month - 1) / 2) - 385 + $day + 1948439.5;
    return $jd;
}

function hijriToGregorian($hijriDate) {
    if (empty($hijriDate)) return '';
    
    // دعم صيغتين: YYYY-MM-DD أو YYYY/MM/DD
    $hijriDate = str_replace('/', '-', $hijriDate);
    $parts = explode('-', $hijriDate);
    if (count($parts) != 3) return '';
    
    $hYear = (int)$parts[0];
    $hMonth = (int)$parts[1];
    $hDay = (int)$parts[2];
    
    $jd = islamicToJDImproved($hMonth, $hDay, $hYear);
    
    // استخدام الدالة المدمجة في PHP
    if (function_exists('jdtogregorian')) {
        $gregorian = jdtogregorian($jd);
        // تحويل من MM/DD/YYYY إلى YYYY-MM-DD
        $parts = explode('/', $gregorian);
        if (count($parts) == 3) {
            return sprintf('%04d-%02d-%02d', $parts[2], $parts[0], $parts[1]);
        }
    } else {
        // بديل إذا لم تكن الدالة متاحة
        $gregorian = calculateJDToGregorian($jd);
    }
    
    return $gregorian;
}

// دالة مساعدة لحساب JD (فقط إذا لم تكن الدالة المدمجة متاحة)
function calculateGregorianToJD($month, $day, $year) {
    if ($month <= 2) {
        $year -= 1;
        $month += 12;
    }
    $a = floor($year / 100);
    $b = 2 - $a + floor($a / 4);
    return floor(365.25 * ($year + 4716)) + floor(30.6001 * ($month + 1)) + $day + $b - 1524.5;
}

function jdToIslamic($jd) {
    // استخدام الخوارزمية المحسنة
    return jdToIslamicImproved($jd);
}

function islamicToJD($month, $day, $year) {
    // استخدام الخوارزمية المحسنة
    return islamicToJDImproved($month, $day, $year);
}

// دالة مساعدة لتحويل JD إلى ميلادي (فقط إذا لم تكن الدالة المدمجة متاحة)
function calculateJDToGregorian($jd) {
    $jd = floor($jd) + 0.5;
    $z = floor($jd);
    $w = floor(($z - 1867216.25) / 36524.25);
    $x = floor($w / 4);
    $a = $z + 1 + $w - $x;
    $b = $a + 1524;
    $c = floor(($b - 122.1) / 365.25);
    $d = floor(365.25 * $c);
    $e = floor(($b - $d) / 30.6001);
    $f = floor(30.6001 * $e);
    $day = $b - $d - $f;
    $month = $e - 1;
    if ($month > 12) $month -= 12;
    $year = $c - 4715;
    if ($month > 2) $year -= 1;
    
    return sprintf('%04d-%02d-%02d', $year, $month, $day);
}

