using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.EntityFrameworkCore;
using API.Data.AppDbContext.Identity;

namespace API.Controllers
{
    public class LoginController : BaseApiController
    {
        private readonly IConfiguration _config;
        private readonly string _domain = "Elamir";
        private readonly IdentityDbContext _context; // Assuming you have a DbContext for your database
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginController(IConfiguration config, IdentityDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] AuthenticationRequest request)
        {
            // Ensure username and password are provided
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new AuthenticationResponse
                {
                    IsValid = false,
                    Message = "Username or password cannot be empty."
                });
            }

            var authenticationResult = await AuthenticateUserAsync(request.Username, request.Password);

            if (authenticationResult.IsValid)
            {
                var token = await GenerateJwtToken(request.Username);

                return Ok(new AuthenticationResponse
                {
                    IsValid = true,
                    Message = "User authentication successful.",
                    Token = token
                });
            }
            else
            {
                return Unauthorized(new AuthenticationResponse
                {
                    IsValid = false,
                    Message = authenticationResult.Message
                });
            }
        }

        private async Task<AuthenticationResponse> AuthenticateUserAsync(string username, string password)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var ldapConnection = new LdapConnection(new LdapDirectoryIdentifier(_domain)))
                    {
                        ldapConnection.AuthType = AuthType.Basic;
                        ldapConnection.Timeout = TimeSpan.FromSeconds(10);

                        var networkCredential = new NetworkCredential($"{_domain}\\{username}", password);
                        ldapConnection.Bind(networkCredential);

                        return new AuthenticationResponse
                        {
                            IsValid = true,
                            Message = "User authenticated successfully."
                        };
                    }
                }
                catch (LdapException ex)
                {
                    if (ex.ErrorCode == 49) // LDAP error code for invalid credentials
                    {
                        return new AuthenticationResponse
                        {
                            IsValid = false,
                            Message = "Invalid username or password."
                        };
                    }

                    return new AuthenticationResponse
                    {
                        IsValid = false,
                        Message = $"LDAP error: {ex.Message}"
                    };
                }
                catch (Exception ex)
                {
                    return new AuthenticationResponse
                    {
                        IsValid = false,
                        Message = $"Unexpected error: {ex.Message}"
                    };
                }
            });
        }

        private async Task<string> GenerateJwtToken(string username)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = jwtSettings["Key"];

            // JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // Prevent claim type mapping


            if (key!.Length < 32)
            {
                throw new ArgumentException("JWT Key must be at least 32 characters long.");
            }

            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);

            // Step 1: Retrieve roles for the user from the AssignedRolesToEmployees table
            if (_context.AssignedRolesToEmployees == null)
            {
                throw new InvalidOperationException("AssignedRolesToEmployees is null.");
            }

            var roles = await _context.AssignedRolesToEmployees
                .Where(are => are.EmpUserName == username)
                .Join(_context.Roles ?? throw new InvalidOperationException("Roles is null."),
                    are => are.RoleID, 
                    r => r.RoleID, 
                    (are, r) => r.RoleName)
                .ToListAsync();

            // Step 2: Create claims for the JWT token
            var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Add role claims to the list
            claims.AddRange(roleClaims);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiresInMinutes"]!)),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // After token creation, set the claims in the HttpContext so it's accessible globally
            var identity = new ClaimsIdentity(claims, "jwt");
            var principal = new ClaimsPrincipal(identity);
            
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.User = principal;
            }

            return tokenHandler.WriteToken(token);
        }

    }

    public class AuthenticationRequest
    {
        [Required]
        public required string Username { get; set; }
        [Required]
        public required string Password { get; set; }
    }

    public class AuthenticationResponse
    {
        public bool IsValid { get; set; }
        public required string Message { get; set; }
        public string? Token { get; set; }
    }
}
