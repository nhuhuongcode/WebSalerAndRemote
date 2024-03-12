using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace remoteWebMos.Controllers
{
	public class LoginController : Controller
	{

		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Index(string username, string password, string loginkey, bool IsSecret)
		{
			if(IsSecret == false)
			{
				if (string.IsNullOrEmpty(username) == true || string.IsNullOrEmpty(password) == true)
				{
					ViewBag.error = "Chưa nhật thông tin đăng nhập!";
					return View();
				}
				if (MOSAPI.LoginUserPass(username, password) == false)
				{
					ViewBag.error = "Thông tin đăng nhập không đúng!";
					return View();
				}
				else
				{
					if(MOSAPI.IsSalerAndExp(MOSAPI.GetSecretByUser(username)) == true)
					{
                        HttpContext.Session.SetString("secret", MOSAPI.GetSecretByUser(username));
                        return RedirectToAction("Index", "saleorders");
                    }
					else
					{
						if (MOSAPI.IsRemotedAndExp(MOSAPI.GetSecretByUser(username)) == false)
						{
							ViewBag.error = "Tài khoảng chưa đăng ký hoặc đã hết hạn!";
							return View();
						}
						else
						{
							HttpContext.Session.SetString("secret", MOSAPI.GetSecretByUser(username));
							return RedirectToAction("Index", "GetAccountRemote");
						}
					}
				}
				
			}
			else
			{
				if (string.IsNullOrWhiteSpace(loginkey) == true)
				{
					ViewBag.error = "Chưa nhập mã bí mật";
					return View();
				}
				if (MOSAPI.LoginSecretKey(loginkey) != true)
				{
					ViewBag.error = "Mã bí mật không đúng!";
					return View();
				}
				else
				{
					if (MOSAPI.IsSalerAndExp(loginkey) == true)
					{
                        HttpContext.Session.SetString("secret", loginkey);
                        return RedirectToAction("Index", "SaleOrders");
					}
					else
					{
						if (MOSAPI.IsRemotedAndExp(loginkey) == false)
						{
							ViewBag.error = "Tài khoảng chưa đăng ký hoặc đã hết hạn!";
							return View();
						}
                        HttpContext.Session.SetString("secret", loginkey);
                        return RedirectToAction("Index", "SaleOrders");
					}
				}
			}
			
		}

		public IActionResult LogOut()
		{
			HttpContext.Session.Remove("secret");
			return RedirectToAction("Index", "Login");
		}
	}
}
