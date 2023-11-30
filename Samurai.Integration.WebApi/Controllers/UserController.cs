using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Samurai.Integration.Domain.Results;
using Samurai.Integration.EntityFramework.Repositories;
using Samurai.Integration.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Samurai.Integration.WebApi.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UserController : BaseController<UserController>
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserProfileRepository _userProfileRepository;

        public UserController(ILogger<UserController> logger, 
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            UserProfileRepository userProfileRepository) : base(logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userProfileRepository = userProfileRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                var result = new Result<List<UserModel>>() { StatusCode = HttpStatusCode.OK };

                var identityUsers = await _userManager.Users.ToListAsync();
                var usersProfile = await _userProfileRepository.GetAllAsync();
                var usersModel = new List<UserModel>();

                var allRoleNames = _roleManager.Roles
                    .Select(role => role.Name)
                    .OrderBy(name => name)
                    .ToList();

                foreach (var user in identityUsers)
                {
                    var userModel = new UserModel()
                    {
                        Id = user.Id,
                        UserName = usersProfile.FirstOrDefault(x => x.Id == user.Id)?.Name,
                        Email = user.Email,
                        Roles = await GetUserRolesMatches(user, allRoleNames)
                    };

                    usersModel.Add(userModel);
                }

                result.Value = usersModel;

                if (result.IsFailure)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost("AddRole")]
        public async Task<IActionResult> AddRoleAsync([FromBody] UserRoleModel roleModel)
        {
            try
            {
                var result = new Result() { StatusCode = HttpStatusCode.OK };

                var user = _userManager.Users.FirstOrDefault(user => user.Id == roleModel.UserId);

                await _userManager.AddToRoleAsync(user, roleModel.Name);

                if (result.IsFailure)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete("RemoveRole")]
        public async Task<IActionResult> RemoveRoleAsync([FromBody] UserRoleModel roleModel)
        {
            try
            {
                var result = new Result() { StatusCode = HttpStatusCode.OK };

                var user = _userManager.Users.FirstOrDefault(user => user.Id == roleModel.UserId);

                await _userManager.RemoveFromRoleAsync(user, roleModel.Name);

                if (result.IsFailure)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private async Task<List<UserRoleModel>> GetUserRolesMatches(IdentityUser user,List<string> allRoleNames)
        {
            var rolesUser = await _userManager.GetRolesAsync(user);            

            var rolesMatches = new List<UserRoleModel>();

            foreach (var roleName in allRoleNames)
            {
                rolesMatches.Add(new UserRoleModel(roleName, roleName, rolesUser.Any(r => r == roleName)));
            }

            return rolesMatches;
        }

    }
}
