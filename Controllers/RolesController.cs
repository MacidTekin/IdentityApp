using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers
{
    public class RolesController : Controller
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        public RolesController(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View(_roleManager.Roles); //ıqueryable yani bir filtreleme uygulayabiliriz.
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AppRole model)
        {
            if(ModelState.IsValid)
            {
                var result = await _roleManager.CreateAsync(model);
                if(result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult>Edit(string id)    //Roles/Index.cshtml butona asp-action="Edit" asp-route-id="@role.Id" yazdık o id ile çalışacağız
        {
            var role = await _roleManager.FindByIdAsync(id);
            if(role != null  && role.Name != null)
            {
                ViewBag.Users = await _userManager.GetUsersInRoleAsync(role.Name);
                return View(role); //veritabanından rol bilgisi geliyorsa sayfaya yönlendireceğiz
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult>Edit(AppRole model)
        {
            if(ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(model.Id);

                if(role != null)
                {
                    role.Name = model.Name; //rolün name bilgisini modelden gelen name ile güncelliyoruz
                    var result = await _roleManager.UpdateAsync(role);//güncelleme yaptık.
                if(result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    foreach (var err in result.Errors)
                    {
                        ModelState.AddModelError("", err.Description);
                    }
                    if(role.Name !=null)
                        ViewBag.Users = await _userManager.GetUsersInRoleAsync(role.Name);
                }
            }
            return View(model);
        }
    }
}