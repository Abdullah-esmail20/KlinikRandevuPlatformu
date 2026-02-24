//using global::KlinikRandevuPlatformu.Shared.Enums;
//using KlinikRandevuPlatformu.API.Models.Enums;
using KlinikRandevuPlatformu.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace KlinikRandevuPlatformu.API.Models
{

    public class AppUser
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } = "";

        [Required, MaxLength(200)] // كلمة المرور المشفرة (Hashed).
        public string PasswordHash { get; set; } = "";

        public UserRole Role { get; set; }

        // Navigation
        //المستخدم الواحد يمكن أن يكون له مجموعة عيادات
        //و مجموعة مواعيد
        //الـ ICollection هي التي تتيح هذا التعدد.
        public ICollection<Clinic> OwnedClinics { get; set; } = new List<Clinic>();
        // المستخدم الواحد (إذا كان مريضاً) يمكن أن يكون له مجموعة مواعيد.
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
