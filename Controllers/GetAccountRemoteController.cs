using Microsoft.AspNetCore.Mvc;
using remoteWebMos.Models;
using System.Text;

namespace remoteWebMos.Controllers
{
	public class GetAccountRemoteController : Controller
	{
		public IActionResult Index()
		{
            string _secret = string.Empty;
            if (HttpContext.Session.TryGetValue("secret", out byte[] value))
            {
                _secret = Encoding.UTF8.GetString(value);
            }
            if (_secret != string.Empty)
            {
                ViewBag.NameUser = MOSAPI.GetUserNameBySecret(_secret);
            }
            else { return RedirectToAction("Index", "Login"); }
            Remote inforRemote = new Remote();
            inforRemote = MOSAPI.GetInfRemote(_secret);
            return View(inforRemote);
		}
	}
}
