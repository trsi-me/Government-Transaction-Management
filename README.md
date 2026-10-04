# Government Transaction Management

مجلد يجمع إصدارين لنظام متابعة المعاملات: تطبيق سطح مكتب `STOT` وموقع PHP. المصدر الحالي تحت `Versions` فقط. مجلد نشر يحتوي ملفات تنفيذية ضخمة غير موجود في الفحص الحالي، ويوجد مجلد بناء صغير `Versions\Desktop\obj`.

## 1. ما هو المشروع؟

نظام لتسجيل المعاملات ومتابعتها، مع مستخدمين وقوائم منسدلة وسجل حركة. الاسم الظاهر في وثيقة سطح المكتب: «نظام إدارة المعاملات الحكومية». الاسم الظاهر في وثيقة الويب: «نظام إدارة ومتابعة المعاملات الرسمية». اسم التجميع في `STOT.csproj` هو `STOT`.

## 2. لماذا يوجد هذا المشروع؟

وثيقة سطح المكتب تصف إدارة معاملات حكومية رسمية. وثيقة الويب تصف تسجيل المعاملات ومتابعة حالتها وتاريخ حركتها، مع قوائم منسدلة تُدار من الواجهة. الاحتياج الأوسع من ذلك غير موثق.

## 3. من يستخدمه؟

الدوران الظاهران في المخطط والكود: `admin` و `user`.

| الدور | القيمة في قاعدة البيانات | ما تذكره الوثائق |
| --- | --- | --- |
| مدير | `admin` | إدارة القوائم والمستخدمين، ووصول أوسع في تطبيق سطح المكتب |
| مستخدم | `user` | وثيقة سطح المكتب: إضافة وعرض. وثيقة الويب: إضافة وتعديل وعرض |

في `MainWindow.cs` بعض الإجراءات تُعرض عندما يكون `role == "admin"`.

## 4. ماذا يستطيع النظام أن يفعل؟

من ملفات الواجهة والمخطط:

| القدرة | سطح المكتب | الويب |
| --- | --- | --- |
| دخول وخروج | `LoginWindow.cs` | `login.php` و `logout.php` |
| معاملات | `TransactionForm.cs` و `TransactionViewDialog.cs` و `MainWindow.cs` | `transactions\add.php` و `edit.php` و `list.php` و `view.php` و `actions.php` |
| قوائم منسدلة | `DropdownsManagerDialog.cs` | `admin\dropdowns.php` و `dropdowns_actions.php` |
| مستخدمون | `UsersManagerDialog.cs` | `admin\users.php` و `users_actions.php` |
| تاريخ هجري | `utils\HijriConverter.cs` | `config\hijri_converter.php` و `api\convert_date.php` |
| مرفقات | جدول `transaction_attachments` وواجهة في `TransactionViewDialog.cs` | جدول المرفقات في `schema.sql`، ومجلد `Web\uploads` |
| سجل حركة | `transaction_logs` | `transaction_logs` |
| سجل مزامنة | جدول `sync_log` في مخطط SQLite | غير موجود في `Web\database\schema.sql` |

حقول المعاملة في `schema.sqlite.sql` تشمل: `transaction_number`, `title`, `description`, `department_id`, `source_department_id`, `concerned_departments`, `status_id`, `priority_id`, `transaction_type_id`, `follow_up_type_id`, `reference_number`, `notification_number`, `next_follow_up_date`, `next_follow_up_date_hijri`, `notes`, `requests_count`, `is_deleted`, `is_suspended`, `created_by`.

## 5. كيف يعمل النظام؟

```
سطح المكتب
Program.cs
  -> DatabaseManager
  -> LoginWindow
  -> MainWindow
  -> نماذج المعاملات والقوائم والمستخدمين
  -> SQLite محلي

الويب
المتصفح
  -> PHP (login.php أو صفحات transactions و admin)
  -> config\database.php (PDO)
  -> MySQL
  -> صفحة HTML
```

الإصداران منفصلان في مجلدين. تشغيل أحدهما يشغل الآخر: غير موثق في الكود الحالي.

## 6. أمثلة واقعية

إضافة معاملة من الويب، حسب `Versions\Web\README.md` وصفحات `transactions`:

1. فتح `login.php` والدخول.
2. فتح `transactions\add.php`.
3. تعبئة العنوان وباقي الحقول واختيار عناصر القوائم.
4. الحفظ عبر النموذج، والسجل يذهب إلى `transaction_logs` حسب وصف الوثيقة الداخلية.

عرض معاملة: من `transactions\list.php` إلى `transactions\view.php`.

إدارة قائمة: المدير يفتح `admin\dropdowns.php`. الأنواع والعناصر في `dropdown_types` و `dropdown_items`.

## 7. رحلة المستخدم

1. تشغيل `STOT` أو فتح موقع PHP.
2. شاشة الدخول.
3. الصفحة الرئيسية أو `MainWindow`.
4. اختيار قائمة المعاملات أو الإضافة أو الإدارة.
5. الحفظ في SQLite للتطبيق، أو في MySQL للموقع.
6. ظهور السجل في القائمة وصفحة العرض.

## 8. الوحدات والأقسام

| الوحدة | الملفات | الوظيفة الظاهرة |
| --- | --- | --- |
| التشغيل | `Program.cs` | نقطة دخول Windows Forms |
| البيانات | `config\DatabaseManager.cs` | إنشاء SQLite والجداول |
| الدخول | `ui\LoginWindow.cs` و `Web\login.php` | التحقق من المستخدم |
| المعاملات | مجلد `ui` و `Web\transactions` | إضافة وتعديل وعرض |
| الإدارة | حوارات المستخدمين والقوائم، و `Web\admin` | مستخدمون وقوائم |
| التواريخ | `HijriConverter.cs` و `hijri_converter.php` | تحويل هجري |
| الواجهة المشتركة | `Web\includes\header.php` و `footer.php` | ترويسة وتذييل |
| الأصول | `Web\assets` | أنماط وخطوط حسب وثيقة الويب |

## 9. الشركات والكيانات

كيان متعدد الشركات غير موجود في الملفات الحالية. «الجهة» و«القسم» حقول معاملة مرتبطة بعناصر `dropdown_items` مثل `department_id`.

## 10. الصلاحيات

قيم الدور: `user` و `admin`. في الويب، `config\session.php` يعرّف التحقق من أن `$_SESSION['role']` يساوي `admin`. صفحات `admin` مخصصة للمدير حسب وثيقة الويب. في سطح المكتب، `UsersManagerDialog.cs` يعرض الخيارين «مستخدم عادي» و«مدير».

تعارض الوثائق: README سطح المكتب يقصر المستخدم على الإضافة والعرض. README الويب يتيح له التعديل أيضا.

## 11. الأتمتة وWorkflows

محرك سير عمل مستقل غير موجود في الملفات الحالية. الموجود: سجل حركة عند تغيير المعاملة، وحقول متابعة `next_follow_up_date` و `next_follow_up_date_hijri`. تحويل التاريخ الهجري دالة تُستدعى من الواجهة ومن `api\convert_date.php`.

## 12. التكامل بين الوحدات

عناصر القوائم تغذي حقول المعاملة. المستخدم في `created_by` و `transaction_logs.user_id`. المرفق يرتبط بـ `transaction_id`. جدول `sync_log` موجود في مخطط سطح المكتب. استدعاء MySQL من ملفات C# غير ظاهر: البحث عن النص MySQL داخل ملفات `.cs` لم يُرجع تطابقا، رغم وجود `config\mysql_config.json`.

## 13. المصطلحات

| المصطلح | المعنى من الملفات |
| --- | --- |
| STOT | اسم التجميع ومساحة الأسماء في `STOT.csproj` |
| معاملة | صف في `transactions` |
| قائمة منسدلة | `dropdown_types` مع `dropdown_items` |
| سجل الحركة | `transaction_logs` |
| هجري | نص أو تحويل عبر `HijriConverter` |
| مزامنة | جدول `sync_log` وملف `mysql_config.json` |

## 14. الأسئلة الشائعة

| السؤال | الجواب من الملفات |
| --- | --- |
| أين المصدر؟ | `Versions\Desktop` و `Versions\Web` |
| ما قاعدة سطح المكتب؟ | SQLite. المسارات المكتوبة في `DatabaseManager.cs` تشمل `STOT\local.db` تحت ملف المستخدم، و `C:\STOT\TrackingTransactions\local.db`، وتعبيرات أخرى تلصق `TrackingTransactions` بمسار المستخدم |
| ما قاعدة الويب؟ | MySQL عبر PDO. الاسم في `config\database.php` هو `tracking_official_transactions` |
| هل يوجد ملف exe منشور؟ | مجلد `bin` أو `publish` غير موجود في الفحص الحالي |
| هل الوثيقتان متطابقتان؟ | فيهما تعارضات مذكورة في القسم 36 |

## 15. Architecture

```
Government Transaction Management
└── Versions
    ├── Desktop   Windows Forms, net8.0-windows, SQLite
    └── Web       PHP pages, MySQL, uploads
```

تطبيقان يشاركان فكرة الجداول، ويعمل كل منهما بقاعدة مختلفة.

## 16. Tech Stack

| الجزء | التقنية | الدليل |
| --- | --- | --- |
| سطح المكتب | C# و Windows Forms | `STOT.csproj`: `UseWindowsForms` و `net8.0-windows` |
| حزم .NET | `System.Data.SQLite` 1.0.118 و `Newtonsoft.Json` 13.0.3 | `STOT.csproj` |
| ويب | PHP و PDO MySQL | `config\database.php` |
| واجهة الويب | PHP مع HTML، ومجلد `assets` | وثيقة الويب وهيكل المجلد |
| خط | IBM Plex Sans Arabic ثم Segoe UI | `Program.cs` |

إصدار PHP المطلوب في وثيقة الويب: 7.4 أو أحدث. إصدار MySQL المذكور هناك: 5.7 أو أحدث. ملف إثبات الإصدار على الخادم غير موجود في الملفات الحالية.

## 17. Project Structure

```
Government Transaction Management
└── Versions
    ├── Desktop
    │   ├── Program.cs
    │   ├── STOT.csproj
    │   ├── STOT.sln
    │   ├── README.md
    │   ├── config\DatabaseManager.cs
    │   ├── config\mysql_config.json
    │   ├── database\schema.sqlite.sql
    │   ├── ui\  LoginWindow, MainWindow, TransactionForm,
    │   │        TransactionViewDialog, DropdownsManagerDialog,
    │   │        UsersManagerDialog
    │   ├── utils\FontHelper.cs
    │   ├── utils\HijriConverter.cs
    │   └── obj\   مخرجات بناء صغيرة
    └── Web
        ├── index.php login.php logout.php README.md
        ├── admin\ transactions\ api\ includes\
        ├── config\ database\ assets\ uploads\
```

`utils\SyncManager.cs` مذكور في README سطح المكتب وغير موجود في المجلد.

## 18. Frontend

سطح المكتب: نماذج Windows Forms في `ui\`. الويب: صفحات PHP متعددة (تطبيق متعدد الصفحات). `transactions\list.php` للبحث والتصفية حسب وثيقة الويب. تصميم متجاوب مذكور في تلك الوثيقة. إطار JavaScript منفصل عن `assets` غير مفحوص سطرا بسطر في هذا الملخص. ملف `assets\css\style.css` مذكور في README الويب.

## 19. Backend

منطق سطح المكتب داخل نوافذ `ui` و `DatabaseManager`. استعلامات SQLite بمعاملات ظاهرة في `UsersManagerDialog.cs`. الويب: صفحات PHP تنفذ المنطق مباشرة، مع `config\session.php` و `config\database.php`. أصناف خدمة منفصلة غير موجودة في الملفات الحالية.

## 20. Request Flow

مثال الويب، الدخول:

```
المتصفح
-> login.php
-> getDBConnection() في database.php
-> SELECT بمعاملات PDO
-> password_verify
-> جلسة PHP
-> index.php أو التحويل بعد الدخول
```

مثال سطح المكتب:

```
Program.Main
-> new DatabaseManager()
-> LoginWindow
-> MainWindow(userId, username, role, fullName)
```

## 21. Database

| البند | سطح المكتب | الويب |
| --- | --- | --- |
| النوع | SQLite | MySQL عبر PDO |
| الاسم | ملف محلي، المسارات في القسم 14 | `tracking_official_transactions` في `database.php` |
| المخطط | `database\schema.sqlite.sql` | `database\schema.sql` |
| الجداول | `users`, `dropdown_types`, `dropdown_items`, `transactions`, `transaction_logs`, `transaction_attachments`, `sync_log` | الخمسة الأولى ومرفقات وسجل، بلا `sync_log` |
| الاستعلام | أوامر SQLite بمعاملات في أجزاء من C# | PDO مع `ATTR_EMULATE_PREPARES` بقيمة false |

مهاجرات مستقلة عن ملفات SQL هذه غير موجودة في الملفات الحالية.

تعارض الاسم: README الويب يضع مثالا `DB_NAME` بقيمة `tracking_transactions`، بينما `database.php` يعرّف `tracking_official_transactions`. ملف `mysql_config.json` يستخدم المفتاح `database` بالاسم الثاني.

## 22. API

واجهة REST عامة غير موجودة. المسار البرمجي الظاهر:

| الملف | الوظيفة الظاهرة |
| --- | --- |
| `Web\api\convert_date.php` | تحويل تاريخ، حجم الملف يدل على سكربت تحويل |
| `Web\transactions\actions.php` | إجراءات معاملات |
| `Web\admin\users_actions.php` | إنشاء وتعديل مستخدمين |
| `Web\admin\dropdowns_actions.php` | إجراءات القوائم |

توثيق Method وشكل JSON لكل مسار غير موثق في ملف عقد API مستقل. المصادقة لصفحات الإدارة عبر الجلسة.

## 23. Authentication & Authorization

الويب: `password_verify` في `login.php`، و`password_hash(..., PASSWORD_DEFAULT)` عند حفظ المستخدم في `users_actions.php`. الجلسة في `config\session.php`.

سطح المكتب: `UsersManagerDialog.cs` و`DatabaseManager.cs` يحسبان MD5 ثم يحفظان الناتج في عمود `password`. مخطط `schema.sqlite.sql` يدرج هاشًا يبدو بصيغة bcrypt في تعليق مرتبط بمستخدم `admin`. الخوارزميتان مختلفتان. قيم كلمات المرور غير منسوخة هنا.

## 24. Security

| الوسيلة | الحالة في الملفات |
| --- | --- |
| استعلامات بمعاملات | ظاهرة في `login.php` و `users_actions.php` وفي أوامر SQLite للمستخدمين |
| تجزئة الويب | `PASSWORD_DEFAULT` |
| تجزئة سطح المكتب | MD5 في `DatabaseManager.cs` و `UsersManagerDialog.cs` |
| جلسات | PHP session، وتمرير الدور إلى `MainWindow` |
| CSRF | غير ظاهر في `login.php` و `users_actions.php` |
| رسائل الخطأ | `database.php` يطبع رسالة `PDOException` عبر `die` |
| تحديد المحاولات | غير موجود في الملفات الحالية |
| ترويسات أمن HTTP | غير موثقة في الملفات المفحوصة |
| أسرار | `database.php` و `mysql_config.json` يحملان حقول اتصال. القيم غير منسوخة هنا |

## 25. Configuration

| الملف | المفاتيح الظاهرة |
| --- | --- |
| `Web\config\database.php` | `DB_HOST`, `DB_USER`, `DB_PASS`, `DB_NAME`, `DB_CHARSET` |
| `Desktop\config\mysql_config.json` | `host`, `port`, `user`, `password`, `database` |
| `Web\config\base_path.php` | مسار أساسي للموقع |
| `STOT.csproj` | `PublishSingleFile`, `SelfContained`, `RuntimeIdentifier` = `win-x64` |

قيم كلمات المرور والمستخدمين في هذه الملفات غير منسوخة في هذه الوثيقة.

## 26. Integrations

خدمة بريد أو SMS أو ذكاء اصطناعي أو Cloudflare غير موجودة في الملفات الحالية. MySQL مقصود لموقع الويب ولملف إعداد سطح المكتب. خط IBM Plex Sans Arabic مذكور في وثيقة الويب تحت `assets\fonts`.

## 27. Scheduled Jobs

Cron أو عامل طابور غير موجود في الملفات الحالية.

## 28. File Storage

مجلد `Versions\Web\uploads` موجود. جدول `transaction_attachments` يحفظ `file_name` و `file_path` و `file_type` و `file_size` و `uploaded_by`. README سطح المكتب يذكر حفظ المرفقات تحت `TrackingTransactions\attachments\[رقم المعاملة]`.

## 29. Logging & Monitoring

`transaction_logs` يسجل `action_type` و `notes` و `old_value` و `new_value`. `sync_log` في مخطط سطح المكتب. نظام مراقبة خارجي غير موجود في الملفات الحالية. أخطاء اتصال الويب تُطبع للمستخدم من `database.php`.

## 30. Installation

سطح المكتب، من README الداخلي و`STOT.csproj`:

1. تثبيت .NET 8 SDK.
2. من مجلد `Versions\Desktop`: `dotnet build -c Release`.
3. النشر المذكور في الوثيقة الداخلية: `dotnet publish -c Release -r win-x64 --self-contained true` مع خصائص الملف الواحد الموجودة أصلا في `csproj`.

الويب:

1. خادم PHP وMySQL.
2. تنفيذ `Versions\Web\database\schema.sql`.
3. ضبط ثوابت `config\database.php` لتطابق القاعدة المنشأة.
4. فتح `login.php`.

مستخدم افتراضي مذكور في تعليقات SQL ووثائق داخلية. كلمة المرور غير مكررة هنا، ويُستحسن تغييرها بعد أول دخول كما تذكر وثيقة الويب.

## 31. Development Guide

صفحة ويب جديدة: ملف PHP بجانب `transactions` أو `admin`، مع `includes\header.php` والتحقق من الجلسة. تغيير جدول: تعديل `schema.sql` للويب و`schema.sqlite.sql` لسطح المكتب، لأن المخططين منفصلان. صلاحية جديدة: القيمة الحالية ثنائية (`user` / `admin`) في `ENUM` بمخطط الويب، ونص حر في SQLite.

## 32. Deployment

`STOT.csproj` يضبط `SelfContained` و `PublishSingleFile` و `win-x64`. مجلد النشر الناتج غير موجود حاليا. موقع الويب ملفات PHP تُنسخ إلى خادم يدعم PHP وMySQL. إعداد إنتاج منفصل (إيقاف عرض أخطاء PDO) غير موجود في الملفات الحالية.

## 33. Backup & Recovery

آلية نسخ احتياطي مبرمجة غير موجودة في الملفات الحالية. عمليا ملف SQLite المحلي وقاعدة MySQL هما مصدر البيانات.

## 34. Troubleshooting

| العرض | ما يفسره الكود أو الوثائق |
| --- | --- |
| فشل اتصال الويب | `database.php` يوقف التنفيذ برسالة PDO. راجع اسم القاعدة والمستخدم |
| اسم قاعدة في الوثيقة يختلف عن الملف | الوثيقة تذكر `tracking_transactions` والملف يعرّف `tracking_official_transactions` |
| دخول سطح المكتب يفشل رغم تنفيذ SQL | إدراج SQL يستخدم هاشًا مختلف الشكل عن MD5 الذي يكتبه `DatabaseManager.cs` |
| المزامنة مع MySQL | ملف JSON موجود، وحزمة MySQL غير مذكورة في `csproj`، والنص MySQL غير ظاهر في ملفات C# |

## 35. Dependencies

| الحزمة | الإصدار في الملف |
| --- | --- |
| `System.Data.SQLite` | 1.0.118 |
| `Newtonsoft.Json` | 13.0.3 |
| .NET | `net8.0-windows` |
| PHP و MySQL | مذكورة في README الويب، بلا ملف إصدار مثبت داخل المشروع |

## 36. Known Limitations

- تجزئة كلمة مرور سطح المكتب MD5.
- `database.php` يكشف نص استثناء الاتصال.
- وثيقتا الصلاحيات تتعارضان في حق التعديل للمستخدم العادي.
- `SyncManager.cs` مذكور وغير موجود.
- مسارات ملف SQLite متعددة داخل `DatabaseManager.cs`.
- جلسة ويب بلا رمز CSRF ظاهر في ملفات الإجراءات المفحوصة.
- ملف exe منشور غير موجود في الشجرة الحالية.

## 37. Current System State

| الحالة | التفصيل |
| --- | --- |
| موجود | مصدر سطح المكتب ومصدر الويب ومخططات SQL ووثائق داخلية |
| يعمل حسب الكود | بناء `dotnet` لم يُشغَّل أثناء كتابة هذه الوثيقة |
| غير مكتمل التوثيق الداخلي | المزامنة مع MySQL مقابل الكود الفعلي |
| غير موجود | مجلد publish تنفيذي، ملف ترخيص مستقل، Cron |

## 38. Architecture Decisions

- فصل إصدار Windows Forms عن إصدار PHP تحت `Versions`. الدليل: المجلدان وملفا README.
- SQLite للعمل المحلي وMySQL للموقع. الدليل: `DatabaseManager.cs` و `database.php`.
- القوائم في جداول بدل تثبيتها في الكود. الدليل: `dropdown_types` ووثيقة الويب.
- النشر الذاتي لـ Windows مقصود في `csproj` عبر `SelfContained` و `PublishSingleFile`.

## 39. سجل التغييرات

سجل إصدارات مستقل غير موجود في الملفات الحالية. وثيقة سطح المكتب تحمل سطر حقوق لسنة 2026. هذا تاريخ في النص وليس سجل إصدارات.

## System Overview

```
[موظف]
   | يدخل
   +--> STOT.exe (غير منشور في المجلد) --> SQLite local.db
   |
   +--> موقع PHP --> MySQL tracking_official_transactions
              |--> معاملات، قوائم، مستخدمون، مرفقات، سجل
```

## Quick Reference

| الجزء | التقنية | الموقع | الوظيفة |
| --- | --- | --- | --- |
| سطح المكتب | C# WinForms | `Versions\Desktop` | تشغيل محلي |
| مدير البيانات | C# | `config\DatabaseManager.cs` | SQLite |
| الويب | PHP | `Versions\Web` | صفحات المعاملات |
| الاتصال | PHP | `config\database.php` | PDO MySQL |
| الجلسة | PHP | `config\session.php` | دور المدير |
| المخطط المحلي | SQL | `database\schema.sqlite.sql` | جداول SQLite |
| مخطط الويب | SQL | `database\schema.sql` | جداول MySQL |
| تحويل التاريخ | PHP | `api\convert_date.php` | هجري |

## Quick Start

سطح المكتب من `Versions\Desktop`:

```
dotnet build -c Release
```

الويب: إنشاء قاعدة MySQL باسم `tracking_official_transactions`، تنفيذ `database\schema.sql`، ثم ضبط `config\database.php`، ثم فتح `login.php`.

## For Non-Technical Users

النظام يسجل المعاملات الرسمية ويتابع حالتها. توجد نسخة برنامج لويندوز ونسخة موقع. بعد الدخول يمكن إضافة معاملة وعرضها. المدير يدير المستخدمين والقوائم. أسماء الجهات والحالات تأتي من قوائم داخل النظام.

## For Developers

- التقنيات: C# / .NET 8 Windows Forms + SQLite، وPHP + MySQL.
- المعمارية: تطبيقان منفصلان تحت `Versions`.
- القاعدة: جداول `users` و `transactions` و `dropdown_*` و `transaction_logs` و `transaction_attachments`. سطح المكتب يضيف `sync_log`.
- واجهة برمجية رسمية: `api\convert_date.php` وصفحات `*_actions.php`.
- ملفات مهمة: `DatabaseManager.cs`, `MainWindow.cs`, `login.php`, `database.php`, المخططان.
- التطوير: أي تغيير للمخطط يُطبَّق على ملفي SQL وعلى إنشاء الجداول داخل `DatabaseManager.cs`.
