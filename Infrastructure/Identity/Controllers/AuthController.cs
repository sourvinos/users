using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Users {

      [Route("api/[controller]")]
      public class AuthController : ControllerBase {

            private readonly UserManager<AppUser> userManager;
            private readonly SignInManager<AppUser> signInManager;
            private readonly TokenSettings tokenSettings;
            private readonly Token token;
            private readonly AppDbContext db;

            public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IOptions<TokenSettings> tokenSettings, Token token, AppDbContext db) =>
                  (this.userManager, this.signInManager, this.tokenSettings, this.token, this.db) = (userManager, signInManager, tokenSettings.Value, token, db);

            [HttpPost("[action]")]
            public async Task<IActionResult> Auth([FromBody] TokenRequest model) {
                  switch (model.GrantType) {
                        case "password":
                              return await GenerateNewToken(model);
                        case "refresh_token":
                              return await RefreshToken(model);
                        default:
                              return Unauthorized(new { response = "Authentication failed" });
                  }
            }

            private async Task<IActionResult> GenerateNewToken(TokenRequest model) {
                  var user = await userManager.FindByNameAsync(model.UserName);
                  if (user != null && await userManager.CheckPasswordAsync(user, model.Password)) {
                        if (!await userManager.IsEmailConfirmedAsync(user)) {
                              return BadRequest(new { response = "This account is pending email confirmation" });
                        }
                        var newRefreshToken = CreateRefreshToken(tokenSettings.ClientId, user.Id);
                        var oldRefreshTokens = db.Tokens.Where(rt => rt.UserId == user.Id);
                        if (oldRefreshTokens != null) {
                              foreach (var token in oldRefreshTokens) {
                                    db.Tokens.Remove(token);
                              }
                        }
                        db.Tokens.Add(newRefreshToken);
                        await db.SaveChangesAsync();
                        var accessToken = await CreateAccessToken(user, newRefreshToken.Value);
                        return Ok(new { response = accessToken });
                  }
                  return Unauthorized(new { response = "Authentication failed" });
            }

            private Token CreateRefreshToken(string clientId, string userId) {
                  return new Token() {
                        ClientId = clientId,
                              UserId = userId,
                              Value = Guid.NewGuid().ToString("N"),
                              CreatedDate = DateTime.UtcNow,
                              ExpiryTime = DateTime.UtcNow.AddMinutes(90)
                  };
            }

            private async Task<TokenResponse> CreateAccessToken(AppUser user, string refreshToken) {
                  double tokenExpiryTime = Convert.ToDouble(tokenSettings.ExpireTime);
                  var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenSettings.Secret));
                  var roles = await userManager.GetRolesAsync(user);
                  var tokenHandler = new JwtSecurityTokenHandler();
                  var tokenDescriptor = new SecurityTokenDescriptor {
                        Subject = new ClaimsIdentity(new Claim[] {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                        new Claim("LoggedOn", DateTime.Now.ToString()),
                        }),
                        SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                        Issuer = tokenSettings.Site,
                        Audience = tokenSettings.Audience,
                        Expires = DateTime.UtcNow.AddMinutes(tokenExpiryTime)
                  };
                  var newtoken = tokenHandler.CreateToken(tokenDescriptor);
                  var encodedToken = tokenHandler.WriteToken(newtoken);
                  return new TokenResponse() {
                        token = encodedToken,
                              expiration = newtoken.ValidTo,
                              refresh_token = refreshToken,
                              roles = roles.FirstOrDefault(),
                              username = user.UserName,
                              displayName = user.DisplayName
                  };
            }

            private async Task<IActionResult> RefreshToken(TokenRequest model) {
                  try {
                        var rt = db.Tokens.FirstOrDefault(t => t.ClientId == tokenSettings.ClientId && t.Value == model.RefreshToken.ToString());
                        if (rt == null) return new UnauthorizedResult();
                        if (rt.ExpiryTime < DateTime.UtcNow) return Unauthorized(new { response = "Authentication failed" });
                        var user = await userManager.FindByIdAsync(rt.UserId);
                        if (user == null) return Unauthorized(new { response = "Authentication failed" });
                        var rtNew = CreateRefreshToken(rt.ClientId, rt.UserId);
                        db.Tokens.Remove(rt);
                        db.Tokens.Add(rtNew);
                        db.SaveChanges();
                        var token = await CreateAccessToken(user, rtNew.Value);
                        return Ok(new { response = token });
                  } catch {
                        return Unauthorized(new { response = "Authentication failed" });
                  }
            }

      }

}