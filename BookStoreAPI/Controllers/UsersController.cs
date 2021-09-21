using AutoMapper;
using BookStoreAPI.Data;
using BookStoreAPI.Data.Mapper;
using BookStoreAPI.Data.Models;
using BookStoreAPI.Data.ViewModels.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IMapper _mapper;

        public UsersController(UserManager<ApplicationUser> userManager,
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

        [HttpGet]
        public async Task<IActionResult> getUsers()
        {
            List<ApplicationUser> users = await _userManager.Users.ToListAsync();
            List<UserWithToken> userWithTokens = new List<UserWithToken>();
            foreach (ApplicationUser a in users)
            {
                var uwt = _mapper.Map<UserWithToken>(a);
                uwt.Roles = (List<string>)await _userManager.GetRolesAsync(a);
                userWithTokens.Add(uwt);
            }

            return Ok(userWithTokens);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> getUser(string id)
        {
            var user = await _userManager.Users.Where(n => n.Id == id).FirstOrDefaultAsync();
            var uwt = _mapper.Map<UserWithToken>(user);
            uwt.Roles = (List<string>)await _userManager.GetRolesAsync(user);

            return Ok(uwt);
        }


    }
}
