using System.ComponentModel.DataAnnotations;

namespace Users.Models {
    public class ChangePasswordViewModel {

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Current password is required")]
        [MaxLength(128, ErrorMessage = "Current password can not be longer than 128 characters")]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "New password is required")]
        [MaxLength(128, ErrorMessage = "New password can not be longer than 128 characters")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New password and confirm new password do not match")]
        public string ConfirmNewPassword { get; set; }

    }

}