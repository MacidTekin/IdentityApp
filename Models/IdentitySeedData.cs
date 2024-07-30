using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityApp.Models
{
    public static class IdentitySeedData      
    {
        private const string adminUser = "admin";
        private const string adminPassword = "Admin_123";
        public static async void IdentityTestUser(IApplicationBuilder app)
        {
            var context = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<IdentityContext>();

            if(context.Database.GetAppliedMigrations().Any()) 
            {
                context.Database.Migrate(); 
            }
            var userManager = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<UserManager<AppUser>>();//hazır sınıf
           
            var user = await userManager.FindByNameAsync(adminUser); //user içersine kullanacı adına göre bir arama yaptık var mı yok mu onu kontrol edeceğiz.

            if(user == null)//veri tabanında kullanıcı yoksa diye başlıyoruz ona göre kullanıcı olusturacağız.
            {
                user = new AppUser{
                    FullName="Macid Tekin",
                    UserName="adminUser",
                    Email="admin@mcdtkn.com",
                    PhoneNumber="44444444"
                };
                await userManager.CreateAsync(user,adminPassword); //kullanıcıyı olusturduk artık bunu çağıracağız bunu da Program.cs de yapacağız.
            }
        }
    } 
}