using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlinikRandevuPlatformu.Shared.DTOs.Clinic;

public class ClinicCreateDto
{
    public string ClinicName { get; set; } = "";
    public string City { get; set; } = "";
    public string? Description { get; set; }
}
