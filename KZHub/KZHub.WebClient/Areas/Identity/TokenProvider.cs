namespace KZHub.WebClient.Areas.Identity
{
    public class TokenProvider
    {
        public string XsrfToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
