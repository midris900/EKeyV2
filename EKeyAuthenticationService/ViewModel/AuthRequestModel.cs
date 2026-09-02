using System.ComponentModel.DataAnnotations;

namespace EKeyAuthenticationService.ViewModel
{
    public class AuthRequestModel
    {
        required public string ReturnUrl { get; set; }
        required public string ApplicationName { get; set; }
    }

    public class AuthResponseModel : ResultDto
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public AuthUser? User{ get; set; }
    }

    public class  AuthUser
    {
        public string NationalId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }



    public class UserInfoModel
    {
        public string Sub { get; set; } // User ID
        public string Name { get; set; }
        public string Email { get; set; }
        public string PreferredUsername { get; set; }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class PkceData
    {
        public string CodeVerifier { get; set; }
        public string CodeChallenge { get; set; }
        public Guid State { get; set; }
    }

    public class TokenResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("id_token")]
        public string? IdToken { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
    }

    public class UserInfo
    {
        public string Sub { get; set; }
        public string Identities { get; set; }
        public string Nid { get; set; }
        public string FirstNameEn { get; set; }
        public string LastNameEn { get; set; }
        public string FirstNameAr { get; set; }
        public string LastNameAr { get; set; }
        public string Loa { get; set; }
        public string Name { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string Gender { get; set; }
        public string Birthdate { get; set; }
        public string Nationalities { get; set; }
        public bool EmailVerified { get; set; }
        public string Nonce { get; set; }
        public string AtHash { get; set; }
        public string Aud { get; set; }
        public long Exp { get; set; }
        public long Iat { get; set; }
        public string Iss { get; set; }
    }

}
