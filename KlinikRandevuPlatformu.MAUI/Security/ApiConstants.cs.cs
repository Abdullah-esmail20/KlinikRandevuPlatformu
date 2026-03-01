using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlinikRandevuPlatformu.MAUI.Helpers
{
    public static class ApiConstants
    {
        // هنا السحر: إذا كان التطبيق يعمل على أندرويد نستخدم 10.0.2.2، وإلا نستخدم localhost
        public static string BaseApiUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7153/api"
            : "https://localhost:7153/api";
    }
}
