using EKeyAuthenticationService.Model;
using Microsoft.EntityFrameworkCore;

namespace EKeyAuthenticationService.Service
{
    public class AuthSessionService(AuthDbContext dbContext , ILogger<AuthSessionService> logger)
    {
        private readonly AuthDbContext _dbContext = dbContext;
        private readonly ILogger<AuthSessionService> _logger = logger;

        public async Task<Guid> StoreAuthStateAsync(Guid state, string codeVerifier, string redirectUrl)
        {
            var session = new AuthSessionModel
            {
                SessionId = Guid.NewGuid(),
                State = state,
                CodeVerifier = codeVerifier,
                RedirectUrl = redirectUrl,
                IsActive=true,
                ExpirationTime = DateTime.UtcNow.AddMinutes(30)
            };
            //_dbContext.AuthSessions.Add(session);
            //await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Stored auth session: {SessionId} for state: {State} with redirect URL: {RedirectUrl}", session.SessionId, state, redirectUrl);

            return session.SessionId;
        }

        public async Task<AuthSessionModel?> GetAuthStateAsync(Guid sessionId)
        {
            var session = await _dbContext.AuthSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive==true);

            if(session!= null)
            {
                if(session.ExpirationTime > DateTime.UtcNow)
                {
                    _logger.LogInformation("Retrieved auth session: {SessionId} for state: {State}", session.SessionId, session.State);
                    return session;
                }
                else
                {
                    _logger.LogInformation("Session Expired: {SessionId} for state: {State}", session.SessionId, session.State);
                    await RemoveAuthStateAsync(sessionId);
                    return null;
                }
            }
            else
            {
                _logger.LogInformation("Invalid Session Id: {SessionId}", sessionId);
                return null;
            }
        }
        public async Task<AuthSessionModel?> GetAuthSessionAsync(Guid state)
        {
            //AuthSessionModel authSession = new();
            //authSession.CodeVerifier = "sj99JvlSOWx03B2LIV7MlPLuXhDsnl1_h-mYZEF5o8g";

            //return authSession;
            return await _dbContext.AuthSessions
            .FirstOrDefaultAsync(s => s.State == state && s.ExpirationTime > DateTime.UtcNow);
        }

        public async Task UpdateAuthSessionAsync(Guid sessionId, string accessToken, string userInfo)
        {
            var session = await _dbContext.AuthSessions.FindAsync(sessionId);
            if (session != null)
            {
                session.AccessToken = accessToken;
                session.UserInfo = userInfo;
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Updated session: {SessionId} with access token and user info.", sessionId);
            }
            else
            {
                _logger.LogInformation("Unable to update UserInfo, Invalid Session Id: {SessionId}", sessionId);
            }
        }

        public async Task RemoveAuthStateAsync(Guid sessionId)
        {
            var session = await _dbContext.AuthSessions.FindAsync(sessionId);
            if (session != null)
            {
                session.IsActive = false;
               // _dbContext.AuthSessions.Remove(session);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Deactivated Auth Session, Expired: {SessionId} for state: {State}", session.SessionId, session.State);
            }
        }

    }
}
