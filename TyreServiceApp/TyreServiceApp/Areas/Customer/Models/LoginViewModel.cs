using System.ComponentModel.DataAnnotations;

namespace TyreServiceApp.Areas.Customer.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите номер телефона")]
        [Phone(ErrorMessage = "Неверный формат телефона")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите PIN-код")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "PIN должен быть 4 цифры")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN должен быть 4 цифры")]
        [DataType(DataType.Password)]
        [Display(Name = "PIN-код")]
        public string Pin { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите ФИО")]
        [StringLength(100, ErrorMessage = "ФИО не должно превышать 100 символов")]
        [Display(Name = "ФИО")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите номер телефона")]
        [Phone(ErrorMessage = "Неверный формат телефона")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Придумайте PIN-код")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "PIN должен быть 4 цифры")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN должен быть 4 цифры")]
        [DataType(DataType.Password)]
        [Display(Name = "PIN-код (4 цифры)")]
        public string Pin { get; set; } = string.Empty;

        [Compare("Pin", ErrorMessage = "PIN-коды не совпадают")]
        [DataType(DataType.Password)]
        [Display(Name = "Повторите PIN")]
        public string ConfirmPin { get; set; } = string.Empty;
    }
}
