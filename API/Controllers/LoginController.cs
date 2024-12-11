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

        public LoginController(IConfiguration config, IdentityDbContext context)
        {
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

            if (key!.Length < 32)
            {
                throw new ArgumentException("JWT Key must be at least 32 characters long.");
            }

            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);

            // Step 1: Retrieve roles for the user from the AssignedRolesToEmployees table
            var roles = await _context.AssignedRolesToEmployees
                .Where(are => are.EmpUserName == username) // Using EmpUserName to find the user
                .Join(_context.Roles, 
                    are => are.RoleID, 
                    r => r.RoleID, 
                    (are, r) => r.RoleName) // Get RoleName for each assigned role
                .ToListAsync();

            // Step 2: Log the roles - this will show the roles for debugging
            Console.WriteLine($"Roles for user {username}: {string.Join(", ", roles)}");

            // Step 3: Create claims for the JWT token
            // Dynamically add role claims based on the roles retrieved
            var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Step 4: Check if the roles contain "SMS" and add a claim if it exists
            // if (roles.Contains("SMS"))
            // {
            //     claims.Add(new Claim("SMS", "Enabled")); // Add your specific claim here
            // }
            // if (roles.Contains("QsCustomerBrandTarget"))
            // {
            //     claims.Add(new Claim("QsCustomerBrandTarget", "Enabled")); // Add your specific claim here
            // }
            // if (roles.Contains("QSCustomerTarget"))
            // {
            //     claims.Add(new Claim("QSCustomerTarget", "Enabled")); // Add your specific claim here
            // }

            foreach (var role in roles)
            {
                // Example: Use the role name as the claim type and "Enabled" as the default value
                claims.Add(new Claim(role, "Enabled"));
            }

            // Step 5: Add role claims to the JWT token
            claims.AddRange(roleClaims);

            // Proceed with generating the JWT token as before
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
