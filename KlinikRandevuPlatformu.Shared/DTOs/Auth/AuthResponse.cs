using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KlinikRandevuPlatformu.Shared.Enums;

namespace KlinikRandevuPlatformu.Shared.DTOs.Auth;

public class AuthResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public UserRole Role { get; set; }
}
