namespace RestaurantBackend.Utility
{
    public class AppSettings
    {
        public string Secret { get; set; } = string.Empty; // Used for JWT authentication secret key
        // Add other application-wide settings as needed
    }
}
