using System.ComponentModel.DataAnnotations;

namespace IdentityApp.ViewModels
{
    public class EditViewModel
    {   
        public string? Id { get; set; } //Edit.cshtml dosyamızdaki input:hidden için id gerekli 
        public string? FullName { get; set; } 

        [EmailAddress]
        public string? Email { get; set; } 

        [DataType(DataType.Password)]
        public string? Password { get; set; } 

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Parola Eşleşmiyor.")]
        public string? ConfirmPassword { get; set; } 

        public IList<string>? SelectedRoles { get; set; } //rolleri temsil edecek bir liste tipi tanımlayalım IList olacak
    }
}