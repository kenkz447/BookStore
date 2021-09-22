using AutoMapper;
using BookStoreAPI.Data;
using BookStoreAPI.Data.Mapper;
using BookStoreAPI.Data.Models;
using BookStoreAPI.Data.ViewModels;
using BookStoreAPI.Data.ViewModels.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthenticationController(UserManager<ApplicationUser> userManager,
                                        RoleManager<IdentityRole> roleManager,
                                        AppDbContext context,
                                        IConfiguration configuration,
                                        IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
            _mapper = mapper;
        }

        [HttpPost("register-user")]
        public async Task<IActionResult> Register([FromBody] RegisterVM payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please, provide all required fields");
            }

            var userExists = await _userManager.FindByNameAsync(payload.Username);

            if (userExists != null)
            {
                return BadRequest($"User {payload.Username} already exists");
            }

            ApplicationUser newUser = new ApplicationUser()
            {
                UserName = payload.Username,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(newUser, payload.Password);

            if (!result.Succeeded)
            {
                return BadRequest("User could not be created!");
            }

            switch (payload.Role)
            {
                case "Admin":
                    await _userManager.AddToRoleAsync(newUser, UserRoles.Admin);
                    break;
                default:
                    await _userManager.AddToRoleAsync(newUser, UserRoles.User);
                    break;
            }
            return Created(nameof(Register), $"User {payload.Username} created");
        }

        [HttpPost("login-user")]
        public async Task<IActionResult> Login([FromBody] LoginVM payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please, provide all required fields");
            }

            var userExists = await _userManager.FindByNameAsync(payload.Username);
            // Console.WriteLine(userExists);
            if (userExists != null && await _userManager.CheckPasswordAsync(userExists, payload.Password))
            {
                var tokenValue = await GenerateJwtToken(userExists);

                return Ok(tokenValue);
            }

            return Unauthorized();
        }

        private async Task<AuthResultVM> GenerateJwtToken(ApplicationUser user)
        {
            var authClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            //Add User Roles
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
             expires: DateTime.UtcNow.AddMinutes(60), // 5 - 10mins
             claims: authClaims,
             signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
             );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsRevoked = false,
                UserId = user.Id,
                DateAdded = DateTime.UtcNow,
                DateExpire = DateTime.UtcNow.AddMonths(6),
                Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString()
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            var response = new AuthResultVM()
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = token.ValidTo
            };

            return response;
        }

        [HttpGet]
        [Route("get-info")]
        public async Task<IActionResult> GetInfoFromToken([FromBody] TokenVM accessToken)
        {
            UserWithToken user = await GetUserFromAccessToken(accessToken.AccessToken);

            if (user != null)
            {
                return Ok(user);
            }

            return NotFound();
        }

        private async Task<UserWithToken> GetUserFromAccessToken(string accessToken)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]);

            var tokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

            SecurityToken securityToken;
            var principle = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out securityToken);

            JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;

            if (
                jwtSecurityToken != null &&
                jwtSecurityToken
                    .Header
                    .Alg
                    .Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase)
            )
            {
                var userId = principle.FindFirst(ClaimTypes.NameIdentifier).Value;

                var user = await _userManager.Users.Where(n => n.Id == userId).FirstOrDefaultAsync();
                var uwt = _mapper.Map<UserWithToken>(user);
                uwt.Roles = (List<string>)await _userManager.GetRolesAsync(user);
                if (uwt != null) return uwt;
            }

            return null;
        }

        [HttpPost("RefreshToken")]
        public async Task<ActionResult<UserWithToken>> RefreshToken([FromBody] RefreshRequest refreshRequest)
        {
            UserWithToken user =
                await GetUserFromAccessToken(refreshRequest.AccessToken);

            if (
                user != null &&
                ValidateRefreshToken(user, refreshRequest.RefreshToken)
            )
            {
                var userExists = await _userManager.FindByNameAsync(user.UserName);
                if (userExists != null)
                {
                    var tokenValue = await GenerateJwtToken(userExists);

                    return Ok(tokenValue);
                }
            }

            return null;
        }

        private bool ValidateRefreshToken(UserWithToken user, string refreshToken)
        {
            RefreshToken refreshTokenUser =
                _context
                    .RefreshTokens
                    .Where(rt => rt.Token == refreshToken)
                    .OrderByDescending(rt => rt.DateExpire)
                    .FirstOrDefault();

            if (
                refreshTokenUser != null &&
                refreshTokenUser.UserId == user.Id &&
                refreshTokenUser.DateExpire > DateTime.UtcNow
            )
            {
                return true;
            }

            return false;
        }
    }
}
