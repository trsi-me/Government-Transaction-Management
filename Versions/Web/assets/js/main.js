/**
 * ملف JavaScript الرئيسي
 */

// تحويل التاريخ الميلادي إلى هجري
function updateHijriDate(gregorianDate) {
    if (!gregorianDate) {
        document.getElementById('next_follow_up_date_hijri_display').value = '';
        return;
    }
    
    // استخدام تحويل بسيط محلي
    const hijri = convertToHijriSimple(gregorianDate);
    const hijriDisplay = document.getElementById('next_follow_up_date_hijri_display');
    if (hijriDisplay) {
        hijriDisplay.value = hijri;
    }
}

// تحويل بسيط محلي (بديل)
function convertToHijriSimple(dateStr) {
    if (!dateStr) return '';
    
    const date = new Date(dateStr);
    const year = date.getFullYear();
    const month = date.getMonth() + 1;
    const day = date.getDate();
    
    // تحويل بسيط (تقريبي)
    const hijriYear = Math.floor((year - 622) * 0.9702);
    const hijriMonth = month;
    const hijriDay = day;
    
    return hijriYear + '/' + String(hijriMonth).padStart(2, '0') + '/' + String(hijriDay).padStart(2, '0');
}

// تأكيد الحذف
function confirmDelete(message, url) {
    if (confirm(message || 'هل أنت متأكد من الحذف؟')) {
        window.location.href = url;
    }
}

// تأكيد التعليق
function confirmSuspend(message, url) {
    if (confirm(message || 'هل أنت متأكد من تعليق هذه المعاملة؟')) {
        window.location.href = url;
    }
}

// تأكيد الحذف الدائم
function confirmPermanentDelete(message, url) {
    const confirmed = confirm(message || 'هل أنت متأكد من الحذف الدائم؟ لا يمكن التراجع عن هذا الإجراء!');
    if (confirmed) {
        const doubleConfirm = confirm('تأكيد نهائي: هل أنت متأكد تماماً؟');
        if (doubleConfirm) {
            window.location.href = url;
        }
    }
}

// عرض صفحة منبثقة للتأكيد
function showConfirmModal(title, message, confirmUrl, cancelUrl = null) {
    const modal = document.createElement('div');
    modal.className = 'form-modal';
    modal.style.display = 'flex';
    modal.innerHTML = `
        <div class="modal-content">
            <h3>${title}</h3>
            <p style="margin-bottom: 25px; font-size: 16px; line-height: 1.8;">${message}</p>
            <div class="form-actions">
                <a href="${confirmUrl}" class="btn btn-danger">تأكيد</a>
                <button type="button" class="btn btn-secondary" onclick="this.closest('.form-modal').remove()">إلغاء</button>
            </div>
        </div>
    `;
    document.body.appendChild(modal);
    
    // إغلاق عند النقر خارج النافذة
    modal.addEventListener('click', function(e) {
        if (e.target === modal) {
            modal.remove();
        }
    });
}

// تحسين تجربة النماذج
document.addEventListener('DOMContentLoaded', function() {
    // إخفاء الرسائل تلقائياً بعد 5 ثوان
    const messages = document.querySelectorAll('.error-message, .success-message');
    messages.forEach(message => {
        setTimeout(() => {
            message.style.opacity = '0';
            message.style.transition = 'opacity 0.5s';
            setTimeout(() => {
                message.remove();
            }, 500);
        }, 5000);
    });
    
    // تحسين تجربة النماذج
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function() {
            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                const originalText = submitBtn.textContent;
                submitBtn.textContent = 'جاري الحفظ...';
                
                // إعادة تفعيل بعد 10 ثوان (في حالة الخطأ)
                setTimeout(() => {
                    submitBtn.disabled = false;
                    submitBtn.textContent = originalText;
                }, 10000);
            }
        });
    });
    
    // إضافة تأكيد للحذف
    document.querySelectorAll('a[href*="delete"], a[href*="suspend"]').forEach(link => {
        link.addEventListener('click', function(e) {
            const href = this.getAttribute('href');
            const action = href.includes('delete') ? 'حذف' : 'تعليق';
            
            if (!confirm(`هل أنت متأكد من ${action}؟`)) {
                e.preventDefault();
            }
        });
    });
    
    // تحديث التاريخ الهجري عند تغيير الميلادي
    const gregorianDateInput = document.getElementById('next_follow_up_date');
    if (gregorianDateInput) {
        gregorianDateInput.addEventListener('change', function() {
            const dateStr = this.value;
            if (dateStr) {
                // استخدام تحويل بسيط
                const hijri = convertToHijriSimple(dateStr);
                const hijriDisplay = document.getElementById('next_follow_up_date_hijri_display');
                if (hijriDisplay) {
                    hijriDisplay.value = hijri;
                }
            }
        });
    }
});

// وظائف مساعدة
function showModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'flex';
    }
}

function hideModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'none';
    }
}

// إغلاق النماذج المنبثقة عند النقر خارجها
document.addEventListener('click', function(e) {
    if (e.target.classList.contains('form-modal')) {
        e.target.style.display = 'none';
    }
});
