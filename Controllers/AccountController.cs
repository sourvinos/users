using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Users.Email;
using Users.Helpers;
using Users.Models;

namespace Users.Controllers {

    [Route("api/[controller]")]
    public class AccountController : Controller {

        private readonly AppSettings appSettings;
        private readonly IEmailSender emailSender;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, IOptions<AppSettings> appSettings) =>
            (this.userManager, this.signInManager, this.appSettings, this.emailSender) = (userManager, signInManager, appSettings.Value, emailSender);

        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel formData) {
            if (ModelState.IsValid) {
                var user = new ApplicationUser {
                    Email = formData.Email,
                    DisplayName = formData.Displayname,
                    UserName = formData.Username,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                var result = await userManager.CreateAsync(user, formData.Password);
                if (result.Succeeded) {
                    await userManager.AddToRoleAsync(user, "Customer");
                    string token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    string callbackUrl = Url.Action("ConfirmEmail", "Account", new { UserId = user.Id, Token = token }, protocol : HttpContext.Request.Scheme);
                    emailSender.SendRegistrationEmail(user.Email, user.UserName, callbackUrl);
                    return Ok(new { response = "User created successfully" });
                } else {
                    return BadRequest(new { response = result.Errors.Select(x => x.Description) });
                }
            }
            return BadRequest(new { response = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token) {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) { return BadRequest(new { response = "User not found" }); }
            if (user.EmailConfirmed) { return Redirect("/login"); }
            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded) {
                return RedirectToAction("EmailConfirmation", "Notifications", new { userId, token });
            }
            return BadRequest(new { response = result.Errors.Select(x => x.Description) });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model) {
            if (ModelState.IsValid) {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null && await userManager.IsEmailConfirmedAsync(user)) {
                    string token = await userManager.GeneratePasswordResetTokenAsync(user);
                    string tokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                    string baseUrl = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";
                    string passwordResetLink = Url.Content($"{baseUrl}/resetPassword/{model.Email}/{tokenEncoded}");
                    emailSender.SendResetPasswordEmail(user.Email, passwordResetLink);
                }
                return Ok(new { response = "An email was sent to your account" });
            }
            return BadRequest(new { response = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) });
        }

        [HttpGet("[action]")]
        public IActionResult ResetPassword([FromQuery] string email, [FromQuery] string tokenEncoded) {
            var model = new ResetPasswordViewModel {
                Email = email,
                Token = tokenEncoded
            };
            return Ok(new { response = model });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model) {
            if (ModelState.IsValid) {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null) { return BadRequest(new { response = "User not found" }); }
                var tokenDecoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
                var result = await userManager.ResetPasswordAsync(user, tokenDecoded, model.Password);
                if (result.Succeeded) {
                    return Ok(new { response = "User created successfully" });
                }
                return BadRequest(new { response = result.Errors.Select(x => x.Description) });
            }
            return BadRequest(new { response = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel vm) {
            if (ModelState.IsValid) {
                var user = await userManager.GetUserAsync(User);
                if (user == null) { return Unauthorized(new { Response = "Authentication failed" }); }
                var result = await userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);
                if (result.Succeeded) {
                    await signInManager.RefreshSignInAsync(user);
                    return Ok(new { Response = "Password changed successfully" });
                }
                return BadRequest(new { response = result.Errors.Select(x => x.Description) });
            }
            return BadRequest(new { response = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) });
        }

    }

}