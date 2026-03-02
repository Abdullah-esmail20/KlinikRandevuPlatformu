using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace KlinikRandevuPlatformu.MAUI.Models;

public record ClinicListItem(int id, string clinicName, string city, string? description, int appointmentCount);
