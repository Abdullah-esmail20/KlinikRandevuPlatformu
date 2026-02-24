
using System.ComponentModel.DataAnnotations;
using KlinikRandevuPlatformu.API.Models.Base;

namespace KlinikRandevuPlatformu.API.Models
{
   

    

    public class Service : AuditableEntity
    {
        public int Id { get; set; }

        public int ClinicId { get; set; }

        [Required, MaxLength(120)]
        public string ServiceName { get; set; } = "";

        [Range(0, 100000)]
        public decimal Price { get; set; }

        public int? DurationMinutes { get; set; }

        // Navigation
        public Clinic? Clinic { get; set; }
        public ICollection<ServiceSchedule> Schedules { get; set; } = new List<ServiceSchedule>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
