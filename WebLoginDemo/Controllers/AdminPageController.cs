using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static WebLoginDemo.DataCore;

namespace WebLoginDemo.Controllers {
	[Authorize(Roles = "Owner")]
	public class AdminController : Controller {

		//[Route("~/admin")]
		public IActionResult Index() {
				return View();
		}
	}
}
