using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Presentation.Controllers
{
    [Route("api/v1/role")]
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> createRole(string roleName)
        {
            var result = await _roleService.createRole(roleName);
            if (result.status == 0)
                return BadRequest(result);
            else
                return Ok(result);
        }
        [HttpPut("update/{roleId}")]
        public async Task<IActionResult> updateRole(Guid roleId, string roleName)
        {
            var result = await _roleService.updateRole(roleId, roleName);
            if (result.status == 0)
                return BadRequest(result);
            else
                return Ok(result);
        }
        [HttpDelete("delete/{roleId}")]
        public async Task<IActionResult> deleteRole(Guid roleId)
        {
            var result = await _roleService.deleteRole(roleId);
            if (result.status == 0)
                return BadRequest(result);
            else
                return Ok(result);
        }
        [HttpGet("get-users/{roleId}")]
        public async Task<IActionResult> getUsersOfRole(Guid roleId, string roleName)
        {
            var result = await _roleService.getUsersOfRole(roleId, roleName);
            if (result.status == 0)
                return BadRequest(result);
            else
                return Ok(result);
        }
        [HttpGet("get-roles")]
        public async Task<IActionResult> getRoles()
        {
            var result = await _roleService.getRoles();
            if (result.status == 0)
                return BadRequest(result);
            else
                return Ok(result);
        }
    }
}
