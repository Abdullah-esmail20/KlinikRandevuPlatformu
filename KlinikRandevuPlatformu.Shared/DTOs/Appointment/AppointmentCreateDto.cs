using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlinikRandevuPlatformu.Shared.DTOs.Appointment;

public class AppointmentCreateDto
{
    public int ClinicId { get; set; }
    public int ServiceId { get; set; }

    public string PatientName { get; set; } = "";
    public string Kimlik { get; set; } = "";
    public string Phone { get; set; } = "";

    public int DiseaseTypeId { get; set; }
    public DateTime AppointmentDateTime { get; set; }
}
