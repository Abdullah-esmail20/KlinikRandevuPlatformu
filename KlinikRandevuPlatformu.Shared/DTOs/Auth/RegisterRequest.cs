using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KlinikRandevuPlatformu.Shared.Enums;

namespace KlinikRandevuPlatformu.Shared.DTOs.Auth;

public class RegisterRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public UserRole Role { get; set; }
}