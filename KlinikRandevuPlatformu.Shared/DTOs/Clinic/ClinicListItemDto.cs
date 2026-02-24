using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlinikRandevuPlatformu.Shared.DTOs.Clinic;

public class ClinicListItemDto
{
    public int Id { get; set; }
    public string ClinicName { get; set; } = "";
    public string City { get; set; } = "";
    public int AppointmentCount { get; set; } // عدد حجوزات العيادة
}
