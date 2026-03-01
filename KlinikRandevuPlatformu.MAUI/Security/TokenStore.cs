using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlinikRandevuPlatformu.MAUI.Helpers
{
    public static class TokenStore
    {
        // حفظ التوكن
        public static async Task SetTokenAsync(string token)
        {
            await SecureStorage.Default.SetAsync("auth_token", token);
        }

        // جلب التوكن
        public static async Task<string> GetTokenAsync()
        {
            return await SecureStorage.Default.GetAsync("auth_token");
        }

        // مسح التوكن (عند تسجيل الخروج)
        public static void ClearToken()
        {
            SecureStorage.Default.Remove("auth_token");
        }
    }
}
