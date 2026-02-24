using KlinikRandevuPlatformu.API.Models.Base;
//using KlinikRandevuPlatformu.API.Models.Enums;
using KlinikRandevuPlatformu.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace KlinikRandevuPlatformu.API.Models;

// كلاس الموعد يجمع كل أطراف النظام (مريض، عيادة، خدمة، نوع مرض)
// الوراثة من AuditableEntity ممتازة لمعرفة متى تم إنشاء الحجز (تاريخ إنشاء السجل)
public class Appointment : AuditableEntity 
{
    public int Id { get; set; }

    public int ClinicId { get; set; }
    public int ServiceId { get; set; }

    // المريض عنده حساب
    public int PatientUserId { get; set; }//الذي يربط الموعد بحساب المريض المسجل في النظام.

    // بيانات المريض وقت الحجز (لتبقى محفوظة حتى لو غير حسابه)
    [Required, MaxLength(120)] 
    public string PatientName { get; set; } = "";

    [Required, MaxLength(20)]//تضمن أن قاعدة البيانات لن تقبل حفظ موعد بدون (اسم، هوية، أو هاتف).
    //بيانات المريض
    public string Kimlik { get; set; } = "";

    [Required, MaxLength(20)]
    public string Phone { get; set; } = "";

    public int DiseaseTypeId { get; set; }

    public DateTime AppointmentDateTime { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending; // ✅ من Shared

    // Navigation

    // هذه الخصائص لا تتحول لأعمدة في قاعدة البيانات، 
    // بل تستخدم برمجياً لجلب بيانات العيادة أو الخدمة مع الموعد في استعلام واحد (Include).
    public Clinic? Clinic { get; set; }
    public Service? Service { get; set; }
    public AppUser? PatientUser { get; set; }
    public DiseaseType? DiseaseType { get; set; }
}
