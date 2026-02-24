using System.ComponentModel.DataAnnotations;
using KlinikRandevuPlatformu.API.Models.Base;

namespace KlinikRandevuPlatformu.API.Models
{
    public class Clinic : AuditableEntity //العيادة
    {
        public int Id { get; set; }

        public int OwnerUserId { get; set; }

        [Required, MaxLength(120)]
        public string ClinicName { get; set; } = "";

        [Required, MaxLength(60)]
        public string City { get; set; } = "";

        [MaxLength(300)]
        public string? Description { get; set; }

        // Navigation
        public AppUser? OwnerUser { get; set; }
        //يوضح أن العيادة الواحدة تقدم عدة خدمات ICollection
        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
