namespace Shopping_Tutorial.Services.Cart
{
    public interface ICartService
    {
        Task MergeCartFromCookieToDb(HttpContext httpContext);
    }
}
