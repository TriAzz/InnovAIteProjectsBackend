using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using innovaite_projects_dashboard.Persistence;
using innovaite_projects_dashboard.Authentication;

namespace innovaite_projects_dashboard
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IUserDataAccess _userRepo;
        
        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IUserDataAccess userRepo)
            : base(options, logger, encoder)
        {
            _userRepo = userRepo;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Missing Authorization Header");
            }

            try
            {
                var authHeader = Request.Headers["Authorization"].ToString();
                
                if (!authHeader.StartsWith("Basic "))
                {
                    return AuthenticateResult.Fail("Invalid Authorization Header");
                }

                var token = authHeader.Substring("Basic ".Length).Trim();
                var credentialstring = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                
                var credentials = credentialstring.Split(':');

                if (credentials.Length != 2)
                {
                    return AuthenticateResult.Fail("Invalid Authorization Header");
                }

                var email = credentials[0];
                var password = credentials[1];

                var users = await _userRepo.GetUsersAsync();
                var user = users.FirstOrDefault(u => u.Email == email);

                if (user == null)
                {
                    return AuthenticateResult.Fail("Invalid credentials");
                }
                
                if (!Argon2PasswordHasher.VerifyPassword(password, user.PasswordHash))
                {
                    return AuthenticateResult.Fail("Invalid credentials");
                }
                
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id?.ToString() ?? ""),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Role, user.Role ?? "User"),
                };

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Authentication exception: {ex.Message}");
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }
        }
    }
}
