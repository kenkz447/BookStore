using BookStoreAPI.Data;
using BookStoreAPI.Data.Models;
using BookStoreAPI.Data.ViewModels.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(UserManager<ApplicationUser> userManager,
                                        RoleManager<IdentityRole> roleManager,
                                        AppDbContext context,
                                        IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }

        [HttpPut]
        [Route("add-role")]
        public async Task<IActionResult> AddRoleToUser([FromBody] RoleToUser RoleToUser)
        {
            var userExists = await _userManager.FindByNameAsync(RoleToUser.Username);
            if (userExists == null)
            {
                return NotFound($"Not found user with username: {RoleToUser.Username}");
            }

            var roleExists = await _roleManager.FindByNameAsync(RoleToUser.Role);
            if (roleExists == null)
            {
                return NotFound($"Not found role {RoleToUser.Role}");
            }

            await _userManager.AddToRoleAsync(userExists, RoleToUser.Role);
            return Ok(userExists);
        }


    }
}
