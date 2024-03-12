using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using remoteWebMos.Models;
using System.Text;

namespace remoteWebMos.Controllers
{
    public class InstructionController : Controller
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
             
            return View(new SalerModel().KhoiTao(_secret));
        }

        [HttpPost]
        public IActionResult CapTK(SalerModel model) 
        {
            if(model.AtLeastOneCheckboxSelected == false)
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một môn học");
                return RedirectToAction("Index",model);
            }
            List<CheckboxOption> checkedOptions = model.CheckboxOptions
                .Where(option => option.IsChecked)
                .ToList();

            string examlimits = string.Join(";",checkedOptions.Select(option => option.value));
            

            string newUID = MOSAPI.SendMail(model.account.login_prefix, model.saleOrder.EmailKH,"aaaa", model.account.Attempt.ToString(), model.account.Expired.ToString());

            if (newUID != null)
            {
                bool AddOrder = MOSAPI.AddOrders(model.saleOrder.EmployID.ToString(), model.saleOrder.NgayCap.ToString(), model.saleOrder.EmailKH, newUID, model.saleOrder.Sotien.ToString(), model.saleOrder.KenhTT);
                if (AddOrder)
                {
                    TempData["error"] = 0;
                    TempData["status"] = "Sent mail and added order";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = 1;
                    TempData["status"] = "Can not create order!";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["error"] = 1;
                TempData["status"] = "Can not creat account!";
                return RedirectToAction("Index");
            }
        }

        public IActionResult UpdateEmail(int orderid)
        {
            return View();
        }
    }
}
