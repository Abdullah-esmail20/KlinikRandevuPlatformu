using System.ComponentModel.DataAnnotations;
using KlinikRandevuPlatformu.API.Models.Base;

namespace KlinikRandevuPlatformu.API.Models

{
    public class ServiceSchedule : AuditableEntity //"جدول أوقات العمل"
    {
        public int Id { get; set; }

        public int ServiceId { get; set; }

        // 0 = Sunday .. 6 = Saturday (أو حسب اختيارك)
        [Range(0, 6)]
        public int DayOfWeek { get; set; }

        // وقت الدوام للخدمة
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; } //// وقت انتهاء الدوام.

        [Required, MaxLength(80)]
        public string DoctorName { get; set; } = "";

        // Navigation
        public Service? Service { get; set; }
    }
}
