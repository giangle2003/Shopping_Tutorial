using System.Text.Json;

namespace Shopping_Tutorial.Repository
{
    public static class CookiesExtensions
    {
        public static void SetJson(this IResponseCookies cookies, string key, object value, CookieOptions options = null)
        {
            var json = JsonSerializer.Serialize(value);
            options ??= new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7), // Cookie tồn tại 7 ngày
                HttpOnly = true, // Bảo mật cookie
                Secure = false, // Chỉ gửi cookie qua HTTPS
                SameSite = SameSiteMode.Lax // Ngăn chặn CSRF
            };
            cookies.Append(key, json, options);
        }

        public static T GetJson<T>(this IRequestCookieCollection cookies, string key)
        {
            if (cookies.TryGetValue(key, out var json))
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            return default;
        }
    }
}
