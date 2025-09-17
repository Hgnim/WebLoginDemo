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
			[Required(ErrorMessage = "�û�������Ϊ��")]
			//[EmailAddress]
			[Display(Name = "�û���")]
            public string UserName { get; set; } = default!;

			[Required(ErrorMessage = "���벻��Ϊ��")]
			[StringLength(100, ErrorMessage = "{0}���ȱ�������Ϊ {2} ���ַ������Ϊ {1} ���ַ���", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "����")]
            public string Password { get; set; } = default!;

            [DataType(DataType.Password)]
            [Display(Name = "ȷ������")]
            [Compare("Password", ErrorMessage = "�����ȷ�����벻һ�¡�")]
            public string? ConfirmPassword { get; set; }
        }

		//TUser��ΪIdentityUserModel����Ϊ����ʹ�÷�����
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
				//ֱ��ʹ���û��������û�
				var user = new IdentityUserModel { UserName = Input.UserName };

				var result = await _userManager.CreateAsync(user, Input.Password);

				if (result.Succeeded) {
					await _userManager.AddToRoleAsync(user, "User");
					_logger.LogInformation(LoggerEventIds.UserCreated, "User created a new account with password.");
					Console.WriteLine($"���û�ע�ᡣ�û�����{user.UserName}");

					//ֱ�ӵ�¼����������֤
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
