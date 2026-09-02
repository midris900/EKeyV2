using EKeyAuthenticationService.ViewModel;
using System.Security.Cryptography;
using System.Text;

namespace EKeyAuthenticationService.Helper
{
    public static class PkceHelper
    {
        public static string GenerateCodeVerifier()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Base64UrlEncode(bytes);
        }

        public static string GenerateCodeChallenge(string codeVerifier)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.ASCII.GetBytes(codeVerifier);
            var hash = sha256.ComputeHash(bytes);
            return Base64UrlEncode(hash);
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        public static PkceData GeneratePkceData()
        {
            var codeVerifier = GenerateCodeVerifier();           
            var codeChallenge =GenerateCodeChallenge(codeVerifier);

            return new PkceData
            {
                CodeVerifier = codeVerifier,
                CodeChallenge = codeChallenge,
                State = Guid.NewGuid()
            };
        }


    }

}
