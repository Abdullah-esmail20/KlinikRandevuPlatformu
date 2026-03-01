using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlinikRandevuPlatformu.MAUI.Services
{
    public interface IAuthService
    {
        // دالة فحص اتصال السيرفر
        Task<bool> TestHealthAsync();
    }
}
