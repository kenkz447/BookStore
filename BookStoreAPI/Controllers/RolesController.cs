using BookStoreAPI.Data;
using BookStoreAPI.Data.Models;
using BookStoreAPI.Data.ViewModels.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
  //  [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public RolesController(UserManager<ApplicationUser> userManager,
                                        RoleManager<IdentityRole> roleManager,
                                        AppDbContext context,
                                        IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoles([FromBody] RoleVM roleVM)
        {
            bool x = await _roleManager.RoleExistsAsync(roleVM.Role);
            if (!x)
            {
                var role = new IdentityRole();
                role.Name = roleVM.Role;
                await _roleManager.CreateAsync(role);
            }
            return Ok();
        }

        [HttpGet]
        public IActionResult GetRoles()
        {
            return Ok(_roleManager.Roles.ToList());
        }
    }
}
