//source: https://github.com/dotnet/aspnetcore/blob/v8.0.19/src/Identity/UI/src/Areas/Identity/Pages/V5/Account/Register.cshtml.cs
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using WebLoginDemo.Models.database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace WebLoginDemo.Areas.Account.Pages {
	public class RegisterModel : PageModel {
		[BindProperty]
		public InputModel Input { get; set; } = default!;
        public string? ReturnUrl { get; set; }
        public IList<AuthenticationScheme>? ExternalLogins { get; set; }

        public class InputModel {
			[Required(ErrorMessage = "用户名不能为空")]
			//[EmailAddress]
			[Display(Name = "用户名")]
            public string UserName { get; set; } = default!;

			[Required(ErrorMessage = "密码不能为空")]
			[StringLength(100, ErrorMessage = "{0}长度必须至少为 {2} 个字符，最多为 {1} 个字符。", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "密码")]
            public string Password { get; set; } = default!;

            [DataType(DataType.Password)]
            [Display(Name = "确认密码")]
            [Compare("Password", ErrorMessage = "密码和确认密码不一致。")]
            public string? ConfirmPassword { get; set; }
        }

		//TUser变为IdentityUserModel，因为不能使用泛型类
		private readonly SignInManager<IdentityUserModel> _signInManager;
		private readonly UserManager<IdentityUserModel> _userManager;
		private readonly IUserStore<IdentityUserModel> _userStore;
		private readonly ILogger<RegisterModel> _logger;

		public RegisterModel(
			UserManager<IdentityUserModel> userManager,
			IUserStore<IdentityUserModel> userStore,
			SignInManager<IdentityUserModel> signInManager,
			ILogger<RegisterModel> logger) {
			_userManager = userManager;
			_userStore = userStore;
			_signInManager = signInManager;
			_logger = logger;
		}

		public async Task OnGetAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? returnUrl = null) {
			ReturnUrl = returnUrl;
			ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
		}

        public async Task<IActionResult> OnPostAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? returnUrl = null){
			returnUrl ??= Url.Content("~/");
			ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
			if (ModelState.IsValid) {
				//直接使用用户名创建用户
				var user = new IdentityUserModel { UserName = Input.UserName };

				var result = await _userManager.CreateAsync(user, Input.Password);

				if (result.Succeeded) {
					await _userManager.AddToRoleAsync(user, "User");
					_logger.LogInformation(LoggerEventIds.UserCreated, "User created a new account with password.");
					Console.WriteLine($"新用户注册。用户名：{user.UserName}");

					//直接登录，不进行验证
					await _signInManager.SignInAsync(user, isPersistent: false);
					return LocalRedirect(returnUrl);
				}
				foreach (var error in result.Errors) {
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}

			// If we got this far, something failed, redisplay form
			return Page();
		}
    }
}
