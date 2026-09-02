using EKeyAuthenticationService.ViewModel;
using System.Collections.Concurrent;
namespace EKeyAuthenticationService.Helper
{
    public static class InMemoryStorage
    {
        private static ConcurrentDictionary<string, string> _pkceStore = new ConcurrentDictionary<string, string>();
        private static ConcurrentDictionary<string, UserInfo> _userSessionStore = new ConcurrentDictionary<string, UserInfo>();
        private static ConcurrentDictionary<string, string> _redirectUrlStore = new();

        public static void SavePkce(string state, string verifier)
        {
            _pkceStore[state] = verifier;
        }

        public static string GetPkce(string state)
        {
            _pkceStore.TryRemove(state, out string verifier);
            return verifier;
        }
        public static void SaveRedirectUrl(string state, string redirectUrl)
        {
            _redirectUrlStore[state] = redirectUrl;
        }

        public static string GetRedirectUrl(string state)
        {
            _redirectUrlStore.TryRemove(state, out string redirectUrl);
            return redirectUrl;
        }


        public static void SaveUserSession(string sessionId, UserInfo userInfo)
        {
            _userSessionStore[sessionId] = userInfo;
        }

        public static UserInfo? GetUserSession(string sessionId)
        {
            _userSessionStore.TryRemove(sessionId, out UserInfo? userInfo);
            return userInfo;
        }

    }

}
