
using System.ComponentModel.DataAnnotations;

namespace KlinikRandevuPlatformu.API.Models
{

public class DiseaseType  //نوع المرض
    {
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Name { get; set; } = "";

    // Navigation
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
}