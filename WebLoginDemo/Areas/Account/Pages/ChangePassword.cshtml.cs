using WebLoginDemo.Models.database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WebLoginDemo.Areas.Account.Pages {

    [AllowAnonymous]
    public class ChangePasswordModel : PageModel {

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public class InputModel {
			[Required(ErrorMessage = "�û�������Ϊ��")]
			//[EmailAddress]
			[Display(Name = "�û���")]
			public string UserName { get; set; } = default!;

			[Required(ErrorMessage = "�����벻��Ϊ��")]
			[DataType(DataType.Password)]
			[Display(Name = "������")]
			public string OldPassword { get; set; } = default!;

			[Required(ErrorMessage = "�����벻��Ϊ��")]
			[StringLength(100, ErrorMessage = "{0}���ȱ�������Ϊ {2} ���ַ������Ϊ {1} ���ַ���", MinimumLength = 6)]
			[DataType(DataType.Password)]
			[Display(Name = "������")]
			public string Password { get; set; } = default!;

            [DataType(DataType.Password)]
			[Display(Name = "ȷ��������")]
			[Compare("Password", ErrorMessage = "�����ȷ�����벻һ�¡�")]
			public string? ConfirmPassword { get; set; }
        }


		private readonly UserManager<IdentityUserModel> _userManager;
		private readonly SignInManager<IdentityUserModel> _signInManager;

		public ChangePasswordModel(
			UserManager<IdentityUserModel> userManager,
			SignInManager<IdentityUserModel> signInManager) {
			_userManager = userManager;
			_signInManager = signInManager;
		}

		public void OnGet() {
		}


		public async Task<IActionResult> OnPostAsync() {
			if (!ModelState.IsValid)
				return Page();

			var user = await _userManager.FindByNameAsync(Input.UserName);//.FindByEmailAsync(Input.Email);
			Microsoft.AspNetCore.Identity.SignInResult pwCheck;
			{
				void checkFailed() {
					ModelState.AddModelError(string.Empty, "�û�������������");
				}
				if (user == null) {
					checkFailed();
					return Page();
				}
				pwCheck = await _signInManager.CheckPasswordSignInAsync(user, Input.OldPassword, lockoutOnFailure: true/*���ö�δ���������˻�*/);
				if (!pwCheck.Succeeded) {
					if (pwCheck.IsLockedOut)
						return RedirectToPage("./Lockout");
					checkFailed();
					return Page();
				}
			}

			var result = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.Password);
			if (result.Succeeded) {
				await _userManager.UpdateSecurityStampAsync(user);//��������󣬸��°�ȫ�����ѵ�¼���豸���������µ�¼
				await _signInManager.RefreshSignInAsync(user);//ˢ�µ�¼״̬
				return RedirectToPage("./ChangePasswordConfirmation");
			}

			foreach (var error in result.Errors) {
				ModelState.AddModelError(string.Empty, error.Description);
			}
			return Page();
		}
    }
}