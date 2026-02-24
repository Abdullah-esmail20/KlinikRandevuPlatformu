using System.ComponentModel.DataAnnotations;

namespace KlinikRandevuPlatformu.API.Models.Base
{
    // استخدام 'abstract' يعني أنه لا يمكن إنشاء كائن (Object) مباشر من هذا الكلاس
    // وظيفته الوحيدة هي أن "ترث" منه الكلاسات الأخرى (مثل العيادة والمريض) لتأخذ هذه الخصائص
    public abstract class AuditableEntity
    {
        // تاريخ ووقت إنشاء السجل.
        // استخدمت DateTime.UtcNow وهو تصرف ممتاز جداً برمجياً لتجنب مشاكل المناطق الزمنية (Timezones).
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // اسم أو معرّف الشخص الذي قام بإنشاء هذا السجل.
        // [MaxLength(100)] تحدد أقصى طول للنص في قاعدة البيانات بـ 100 حرف لتوفير المساحة.
        // علامة الاستفهام (?) تعني أن هذا الحقل يمكن أن يكون فارغاً (Nullable).
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        // تاريخ ووقت آخر تعديل على السجل.
        // من الطبيعي أن يكون Nullable (?) لأنه عند إنشاء السجل لأول مرة لن يكون هناك تعديل.
        public DateTime? UpdatedAt { get; set; }

        // اسم أو معرّف الشخص الذي قام بآخر تعديل.
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
    }
}