using IdentityApp.Models;
using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityApp.Controllers
{
    public class UsersController:Controller
    {
        private UserManager<AppUser> _userManager;
        private RoleManager<AppRole> _roleManager;
        public UsersController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View(_userManager.Users);
        }
        
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {   
        if(id == null)
        {
            return RedirectToAction("Index");   
        }
        //id bilgisi boş değilse user bilgisini alacağız.
        var user = await _userManager.FindByIdAsync(id);
        if(user != null)
        {   
            ViewBag.Roles = await _roleManager.Roles.Select(i => i.Name).ToListAsync(); //kullanıcı rolleri değil veritabanındaki tüm rolleri alacağız

            return View(new EditViewModel{
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                SelectedRoles = await _userManager.GetRolesAsync(user) //kullanıcın seçtiği rolleri atamamız lazım ve bu rolleri bize GetRolesAsync metodu ile gelecek
            });
        }  
        return RedirectToAction("Index");   
        }

        [HttpPost]
        public async Task<IActionResult>Edit(string id,EditViewModel model) 
        {
            if(id != model.Id)
            {
                return RedirectToAction("Index"); 
            }
            if(ModelState.IsValid)
            {   
                var user = await _userManager.FindByIdAsync(model.Id);
                
                if(user != null)//user geliyorsa güncellemeleri yapacağız
                {
                    user.Email = model.Email;
                    user.FullName = model.FullName;

                    var result = await _userManager.UpdateAsync(user); // userı burada güncelledik.

                    if(result.Succeeded && !string.IsNullOrEmpty(model.Password))//model state üzerindeki parola bilgisi boş değilse userdan parolayı sileceğiz
                    {
                        await _userManager.RemovePasswordAsync(user); //silme işlemi
                        await _userManager.AddPasswordAsync(user,model.Password);//yeni parola için modelden gelen password bilgisini atayacağız
                    }

                    if(result.Succeeded)
                    {
                        await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user));//await _userManager.GetRolesAsync(user) kullanıcın rolleri
                        if(model.SelectedRoles != null)
                        {
                            await _userManager.AddToRolesAsync(user,model.SelectedRoles);
                        }
                        
                        return RedirectToAction("Index"); 
                    }

                    foreach (IdentityError err in result.Errors) // hata varsa hata bilgilerini alacağız.
                    {
                        ModelState.AddModelError("",err.Description);
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if(user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            return RedirectToAction("Index");
        }
    }
}