using System.ComponentModel.DataAnnotations;

namespace EKeyAuthenticationService.Model
{
    public class AuthSessionModel
    {
        [Key]
        public Guid SessionId { get; set; }
        public Guid State { get; set; }
        public string CodeVerifier { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? UserInfo { get; set; }
        public DateTime ExpirationTime { get; set; } = DateTime.UtcNow.AddMinutes(30);
        public bool IsActive { get; set; }
    }
}
