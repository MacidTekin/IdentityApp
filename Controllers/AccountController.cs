using System.IO.Pipelines;
using IdentityApp.Models;
using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers
{
    public class AccountController:Controller
    {
        private UserManager<AppUser> _userManager;
        private RoleManager<AppRole> _roleManager;
        private SignInManager<AppUser> _signInManager;
        private IEmailSender _emailSender;

        public AccountController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager,SignInManager<AppUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager; 
            _signInManager = signInManager;
            _emailSender = emailSender;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if(user != null)
                {
                    await _signInManager.SignOutAsync(); //kullanıcı daha once giriş yapmışsa cookiyi sıfırlıyalım

                    if(!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError("", "Hesabınızı onaylayınız.");
                        return View(model);
                    }
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe,true);// true kısmı 5 hakkın çalışacağını ifade ediyor.
                    if(result.Succeeded) // parola doğruysa ve örneğin kullanıcı 2 defa yanlıs girdi bunların sıfırlanması lazım tekrardan.!!!!!!
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);
                        await _userManager.SetLockoutEndDateAsync(user, null);

                        return RedirectToAction("Index","Home");//herşey doğru olduktan sonra kullanıcıyı yönlendir.
                    }
                    else if(result.IsLockedOut)//kullanıcı login olması hatalı ise 
                    {
                        var lockoutData = await _userManager.GetLockoutEndDateAsync(user);
                        var timeLeft = lockoutData.Value - DateTime.UtcNow;
                        ModelState.AddModelError("", $"Hesabınız kitlendi, Lütfen {timeLeft.Minutes} dakika sonra deneyiniz");
                    }
                    else
                    {
                        ModelState.AddModelError("", "parolanız hatalı");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "bu email adresiyle bir hesap bulunamadı");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateViewModel model)
        {
            if(ModelState.IsValid)//model valid ise user oluşturup veri tabanına kayıt edeceğiz.
            {
                var user = new AppUser {UserName = model.UserName, Email = model.Email, FullName = model.FullName};            
                IdentityResult result = await _userManager.CreateAsync(user, model.Password);                              
                if(result.Succeeded)// kullanıcı girişi başarılıysa
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user); //ilgili kullanıcı için bir token bilgisi üretecek.
                    var url = Url.Action("ConfirmEmail","Account",new{user.Id, token});//Url bilgisi aşağıdaki ConfirmEmail metodunu tetikleyecek
                    
                    //email ile göndereceğiz. inject edelim
               await _emailSender.SendEmailAsync(user.Email, "Hesap Onayı", $"Lütfen email hesabınızı onaylamak için linke <a href='http://localhost:5225{url}'>tıklayınız.</a>");

                    TempData["message"] = "Emaili hesabınızdaki onay linkine tıklayınız.";
                    return RedirectToAction("Login","Account");             
                }
                foreach (IdentityError err in result.Errors)      
                {
                    ModelState.AddModelError("", err.Description); 
                }
                
            }
            return View(model);
        }
        public async Task<IActionResult> ConfirmEmail(string Id,string token)//bu metodda tamamen onay işlemlerini ypacağız.
        {
            if(Id == null || token == null)
            {   
                TempData["message"] = "Geçersiz Token bilgisi.";
                return View();
            }
            var user = await _userManager.FindByIdAsync(Id);//bu bilgiler varsa user ı alacağız id bilgisi ile aldık
            if(user != null)//user bilgisi varsa eğer onaylamayı yapacağız.
            {
                var result = await _userManager.ConfirmEmailAsync(user,token); 
                if(result.Succeeded)
                {
                    TempData["message"] = "Hesabınız Onaylandı.";
                    return RedirectToAction("Login","Account");
                }

            }
            TempData["message"] = "Kullanıcı Bulunamadı.";
            return View();
        }
    
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
