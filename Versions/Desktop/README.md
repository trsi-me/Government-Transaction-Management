# نظام إدارة المعاملات الحكومية

نظام متكامل لإدارة المعاملات الحكومية الرسمية مبني بلغة C# باستخدام Windows Forms.

## المميزات

- **إدارة المعاملات**: إضافة، تعديل، عرض، وحذف المعاملات الحكومية
- **لوحة تحكم**: عرض إحصائيات شاملة عن المعاملات
- **إدارة القوائم المنسدلة**: إدارة أنواع القوائم وعناصرها (الحالات، الأولويات، الأقسام، إلخ)
- **إدارة المستخدمين**: إضافة وتعديل المستخدمين مع نظام صلاحيات
- **المزامنة**: مزامنة البيانات مع قاعدة بيانات MySQL
- **المرفقات**: رفع وإدارة الملفات المرفقة بالمعاملات
- **سجل الحركة**: تتبع جميع التغييرات على المعاملات
- **التاريخ الهجري**: تحويل تلقائي للتواريخ من الميلادي إلى الهجري
- **التصدير**: تصدير البيانات إلى ملف CSV

## متطلبات التشغيل

- Windows 10 أو أحدث
- .NET 8.0 Runtime (سيتم تضمينه في ملف .exe النهائي)

## التثبيت والاستخدام

### الطريقة الأولى: استخدام ملف .exe جاهز

1. قم بتحميل ملف `STOT.exe`
2. قم بتشغيل الملف مباشرة
3. استخدم بيانات الدخول الافتراضية:
   - **اسم المستخدم**: `admin`
   - **كلمة المرور**: `password`

### الطريقة الثانية: بناء المشروع من المصدر

1. تأكد من تثبيت .NET 8.0 SDK
2. افتح Terminal في مجلد المشروع
3. قم بتنفيذ الأمر التالي لبناء المشروع:

```bash
dotnet build -c Release
```

4. لإنشاء ملف .exe واحد شامل:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

5. سيكون الملف النهائي في: `bin\Release\net8.0-windows\win-x64\publish\STOT.exe`

## هيكل المشروع

```
STOT/
├── Config/
│   └── DatabaseManager.cs          # إدارة قواعد البيانات (SQLite و MySQL)
├── UI/
│   ├── LoginWindow.cs              # نافذة تسجيل الدخول
│   ├── MainWindow.cs               # النافذة الرئيسية
│   ├── TransactionForm.cs          # نموذج إضافة/تعديل المعاملة
│   ├── TransactionViewDialog.cs    # عرض تفاصيل المعاملة
│   ├── DropdownsManagerDialog.cs   # إدارة القوائم المنسدلة
│   └── UsersManagerDialog.cs       # إدارة المستخدمين
├── Utils/
│   ├── HijriConverter.cs          # تحويل التاريخ الهجري
│   └── SyncManager.cs             # مزامنة البيانات
├── Program.cs                      # نقطة الدخول الرئيسية
└── STOT.csproj
```

## قاعدة البيانات

### SQLite (محلية)

يتم إنشاء قاعدة البيانات المحلية تلقائياً في:
```
C:\Users\[اسم المستخدم]\TrackingTransactions\local.db
```

### MySQL (اختيارية)

لتفعيل المزامنة مع MySQL:

1. قم بإنشاء ملف `config\mysql_config.json`:
```json
{
  "host": "localhost",
  "port": 3306,
  "user": "root",
  "password": "your_password",
  "database": "tracking_official_transactions"
}
```

2. تأكد من إنشاء قاعدة البيانات والجداول في MySQL بنفس هيكل SQLite

## كيفية تحويل المشروع إلى ملف .exe واحد

### باستخدام Visual Studio

1. افتح المشروع في Visual Studio
2. انقر بزر الماوس الأيمن على المشروع واختر **Properties**
3. اذهب إلى **Publish**
4. اضغط **Publish** واختر مجلد الإخراج
5. سيتم إنشاء ملف .exe واحد شامل

### باستخدام سطر الأوامر

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishReadyToRun=true
```

### خيارات البناء المتقدمة

- `-c Release`: بناء نسخة الإصدار
- `-r win-x64`: منصة Windows 64-bit
- `--self-contained true`: تضمين .NET Runtime
- `-p:PublishSingleFile=true`: ملف .exe واحد
- `-p:IncludeNativeLibrariesForSelfExtract=true`: تضمين المكتبات الأصلية
- `-p:PublishReadyToRun=true`: تحسين الأداء

## الملفات المرفقة

يتم حفظ الملفات المرفقة في:
```
C:\Users\[اسم المستخدم]\TrackingTransactions\attachments\[رقم المعاملة]\
```

## الصلاحيات

- **مدير (admin)**: وصول كامل لجميع الميزات
- **مستخدم (user)**: يمكنه إضافة وعرض المعاملات فقط

## الدعم الفني

لأي استفسارات أو مشاكل، يرجى التواصل مع فريق الدعم.

## الترخيص

© 2026 نظام إدارة المعاملات الحكومية - جميع الحقوق محفوظة

