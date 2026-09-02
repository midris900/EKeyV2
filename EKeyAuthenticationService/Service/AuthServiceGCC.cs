using EKeyAuthenticationService.Helper;
using EKeyAuthenticationService.ViewModel;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace EKeyAuthenticationService.Service
{
    public class AuthServiceGCC
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly AuthSessionService _authSessionService;
        private readonly ILogger<AuthServiceGCC> _logger;

        public AuthServiceGCC(IHttpClientFactory httpClientFactory, IConfiguration configuration, AuthSessionService authSessionService, ILogger<AuthServiceGCC> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _authSessionService = authSessionService;
            _logger = logger;
        }

        public async Task<string> GetLoginUrl(string redirectAppUrl, string? language = "en")
        {
            var oidcProviderUrl = _configuration["OIDC:Authority"];
            var clientId = _configuration["OIDC:ClientId"];
            var redirectUri = _configuration["OIDC:RedirectUriGCC"];
            var scopes = "openid id-* delegation profile email";
            var nonce = "foobar";
            var prompt = "consent";
            var lang = language;

            var pkceData = PkceHelper.GeneratePkceData();
            Guid sessionId = await _authSessionService.StoreAuthStateAsync(pkceData.State, pkceData.CodeVerifier, redirectAppUrl);

            #region Login URL
            var loginUrl = $"{oidcProviderUrl}/auth?" +
                $"response_type=code&" +
                $"client_id={clientId}&" +
                $"redirect_uri={redirectUri}&" +
                $"scope={scopes}&" +
                $"code_challenge={pkceData.CodeChallenge}&" +
                $"code_challenge_method=S256&" +
                $"response_mode=query&" +
                $"nonce={nonce}&" +
                $"prompt={prompt}&" +
                $"state={pkceData.State}&" +
                $"lang={lang}";
            #endregion

            _logger.LogInformation("Generated login URL for session {SessionId}", sessionId);
            return loginUrl;
        }       

        public async Task<UserInfo?> ExchangeTokenAndGetUserInfo(string code, string codeVerifier, Guid sessionId)
        {
            var oidcTokenUrl = _configuration["OIDC:TokenEndpoint"];
            var clientId = _configuration["OIDC:ClientId"];
            var clientSecret = _configuration["OIDC:ClientSecret"];
            var redirectUri = _configuration["OIDC:RedirectUriGCC"];

            using var client = _httpClientFactory.CreateClient();
            var parameters = new Dictionary<string, string>
        {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "client_id", clientId! },
                { "client_secret", clientSecret! },
                 { "redirect_uri", redirectUri! },
                { "code_verifier", codeVerifier }
        };
            _logger.LogInformation("Initiating Token Exchange: {SessionId}", sessionId);

            var response = await client.PostAsync(oidcTokenUrl, new FormUrlEncodedContent(parameters));
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<TokenResult>(json);

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.IdToken))
                    return null;
                string userInfo = tokenResponse.IdToken!;
                await _authSessionService.UpdateAuthSessionAsync(sessionId, tokenResponse.AccessToken!, userInfo!);

                return FetchUserInfo(tokenResponse.IdToken);
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Failed to exchange token for session: {SessionId}", sessionId);
                return null;
            }
        }

        public UserInfo FetchUserInfo(string IdToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(IdToken);
            var claims = jwtToken.Claims.ToList();
            var scopes = jwtToken.Claims.FirstOrDefault(c => c.Type == "scope")?.Value;

            Console.WriteLine($"Id Token: {IdToken}");
            Console.WriteLine("Claims:");
            foreach (var claim in claims)
            {
                Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
            }
            Console.WriteLine($"Scopes: {scopes}");

            var userInfo = new UserInfo
            {
                Sub = claims.FirstOrDefault(c => c.Type == "sub")?.Value!,
                Identities = claims.FirstOrDefault(c => c.Type == "urn:oneid:identities")?.Value!,
                Nid = claims.FirstOrDefault(c => c.Type == "urn:oneid:id:*:nid")?.Value!,
                FirstNameEn = claims.FirstOrDefault(c => c.Type == "urn:oneid:id:*:firstname:en")?.Value!,
                LastNameEn = claims.FirstOrDefault(c => c.Type == "urn:oneid:id:*:lastname:en")?.Value!,
                FirstNameAr = claims.FirstOrDefault(c => c.Type == "urn:oneid:id:*:firstname:ar")?.Value!,
                LastNameAr = claims.FirstOrDefault(c => c.Type == "urn:oneid:id:*:lastname:ar")?.Value!,
                Loa = claims.FirstOrDefault(c => c.Type == "urn:oneid:id:*:loa")?.Value ?? "",
                Name = claims.FirstOrDefault(c => c.Type == "name")?.Value!,
                GivenName = claims.FirstOrDefault(c => c.Type == "given_name")?.Value!,
                FamilyName = claims.FirstOrDefault(c => c.Type == "family_name")?.Value!,
                Gender = claims.FirstOrDefault(c => c.Type == "gender")?.Value ?? "",
                Birthdate = claims.FirstOrDefault(c => c.Type == "birthdate")?.Value ?? "",
                Nationalities = claims.FirstOrDefault(c => c.Type == "urn:oneid:nationalities")?.Value ?? "",
                EmailVerified = bool.Parse(claims.FirstOrDefault(c => c.Type == "email_verified")?.Value ?? "false"),
                Nonce = claims.FirstOrDefault(c => c.Type == "nonce")?.Value ?? "",
                AtHash = claims.FirstOrDefault(c => c.Type == "at_hash")?.Value ?? "",
                Aud = claims.FirstOrDefault(c => c.Type == "aud")?.Value??"",
                Exp = long.Parse(claims.FirstOrDefault(c => c.Type == "exp")?.Value ?? "0"),
                Iat = long.Parse(claims.FirstOrDefault(c => c.Type == "iat")?.Value ?? "0"),
                Iss = claims.FirstOrDefault(c => c.Type == "iss")?.Value?? ""
            };

            var jsonUser = JsonConvert.SerializeObject(userInfo, Formatting.Indented);
            Console.WriteLine($"User : {jsonUser}");
            return userInfo;
        }
    }
}
