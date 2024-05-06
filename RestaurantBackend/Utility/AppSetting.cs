namespace RestaurantBackend.Utility
{
    public class AppSettings
    {
        public string Secret { get; set; } = string.Empty; // Used for JWT authentication secret key
        public string Issuer { get; set; } = string.Empty; // JWT token issuer
        public string Audience { get; set; } = string.Empty; // JWT token audience
    }
}
