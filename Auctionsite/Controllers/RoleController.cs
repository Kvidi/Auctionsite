using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Auctionsite.Models.Database;
using Microsoft.AspNetCore.Authorization;
using NuGet.Protocol;

namespace Auctionsite.Controllers
{
    [Authorize(Roles = "Admin,Superadmin")]
    public class RoleController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IActionResult Index()
        {
            return View();
        }
        public RoleController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded ? Ok() : BadRequest();
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            return result.Succeeded ? Ok() : BadRequest();
        }
    }
}
