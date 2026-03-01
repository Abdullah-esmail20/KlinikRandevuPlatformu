using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using KlinikRandevuPlatformu.MAUI.Helpers;

namespace KlinikRandevuPlatformu.MAUI.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService()
        {
            // ⚠️ ملاحظة: في بيئة التطوير (Local) قد نحتاج لتخطي فحص شهادة HTTPS للأندرويد
            // سنستخدم HttpClient العادي الآن، وإذا ظهر خطأ SSL سنضيف كود التخطي
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true; // تخطي أخطاء الشهادة محلياً

            _httpClient = new HttpClient(handler);
        }

        public async Task<bool> TestHealthAsync()
        {
            try
            {
                // استدعاء رابط /api/health
                var response = await _httpClient.GetAsync($"{ApiConstants.BaseApiUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Connection Error: {ex.Message}");
                return false;
            }
        }
    }
}