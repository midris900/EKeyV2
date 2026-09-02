using Microsoft.EntityFrameworkCore;

namespace EKeyAuthenticationService.Model
{
    public class AuthDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<AuthSessionModel> AuthSessions { get; set; }
    }
}
