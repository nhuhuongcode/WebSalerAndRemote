using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using remoteWebMos.Models;
using System.Data;
using System.Globalization;
using System.Text;

namespace remoteWebMos.Controllers
{
    public class SaleOrdersController : Controller
    {
        public string _secret = string.Empty;
        public IActionResult Index()
        {
            if(HttpContext.Session.TryGetValue("secret", out byte[] value))
            {
                _secret = Encoding.UTF8.GetString(value);
            }
            if (_secret != string.Empty)
            {
                ViewBag.NameUser = MOSAPI.GetUserNameBySecret(_secret);
            }
            else { return RedirectToAction("Index", "Login"); }
            DataTable SOs = MOSAPI.GetSaleOrders(_secret);

            List<SaleOrder> ListOrders = new List<SaleOrder>();
            foreach (DataRow row in SOs.Rows)
            {
                ListOrders.Add(new SaleOrder()
                {
                    OrderId = int.Parse(row["order_id"].ToString()),
                    EmployID = int.Parse(row["uid_user_nguoicap"].ToString()),
                    NgayCap = DateTime.Parse(row["ngaycap"].ToString()),
                    EmailKH = row["email_khach"].ToString(),
                    CustomerId = int.Parse(row["uid_user_khach"].ToString()),
                    Sotien = int.Parse(row["sotien"].ToString()),
                    KenhTT = row["kenhtt"].ToString()
                });

            }

            return View(ListOrders);
        }

        public IActionResult CapTK()
        {
            if (HttpContext.Session.TryGetValue("secret", out byte[] value))
            {
                _secret = Encoding.UTF8.GetString(value);
            }
            if (_secret != string.Empty)
            {
                ViewBag.NameUser = MOSAPI.GetUserNameBySecret(_secret);
            }
            else { return RedirectToAction("Index", "Login"); }
        
            return View(new Models.SalerModel().KhoiTao(_secret));
        }

        [HttpPost]
        public IActionResult CapTK(SalerModel model)
        {
            DataTable prefix = new DBAccess().GetDataTable("SELECT [uid],[loginkey_prefix],[note] FROM [MOS365].[dbo].[mos_prefix]", CommandType.Text);
            List<Brand> brands = new List<Brand>();
            foreach (DataRow row in prefix.Rows)
            {
                brands.Add(new Brand
                {
                    Id = int.Parse(row["uid"].ToString()),
                    loginkey_pref = row["loginkey_prefix"].ToString(),
                    Name = row["note"].ToString()

                });
            }
            model.brands = brands;
            if (model.AtLeastOneCheckboxSelected == false)
            {
                ModelState.AddModelError("CheckboxOptions", "Vui lòng chọn ít nhất một môn học");
                return View(model);
            }

            List<CheckboxOption> checkedOptions = model.CheckboxOptions
                .Where(option => option.IsChecked)
                .ToList();

            string examlimits = string.Join(";", checkedOptions.Select(option => option.value));


            string newUID = MOSAPI.SendMail(model.account.login_prefix, model.saleOrder.EmailKH, examlimits, model.account.Attempt.ToString(), model.account.Expired.ToString());

            if (newUID != null)
            {
                bool AddOrder = MOSAPI.AddOrders(model.saleOrder.EmployID.ToString(), model.saleOrder.NgayCap.ToString(), model.saleOrder.EmailKH, newUID, model.saleOrder.Sotien.ToString(), model.saleOrder.KenhTT);
                if (AddOrder)
                {
                    TempData["error"] = 0;
                    TempData["status"] = "Đã cấp tài khoản và gửi mail cho học viên!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = 1;
                    TempData["status"] = "Không thể thêm vào lịch sử bán!";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["error"] = 1;
                TempData["status"] = "Không thể cấp tài khoản cho học viên!";
                return RedirectToAction("Index");
            }
        }

        public IActionResult UpdateEmail(int OrderId)
        {
            if (HttpContext.Session.TryGetValue("secret", out byte[] value))
            {
                _secret = Encoding.UTF8.GetString(value);
            }
            if (_secret != string.Empty)
            {
                ViewBag.NameUser = MOSAPI.GetUserNameBySecret(_secret);
            }
            else { return RedirectToAction("Index", "Login"); }
            return View(MOSAPI.GetOrder(OrderId,_secret));
        }
        [HttpPost]
        public IActionResult UpdateEmail(SalerModel model)
        {
            bool IsUpdateEmail = MOSAPI.UpdateEmailUser(model.account.Id.ToString(),model.saleOrder.OrderId.ToString(),model.saleOrder.EmailKH);
            if (IsUpdateEmail)
            {
                TempData["error"] = 0;
                TempData["status"] = "Đã cập nhật mail học viên!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = 1;
                TempData["status"] = "Email chưa được cập nhật";
                return RedirectToAction("Index");
            }
        }

        public IActionResult ReSendMail(int OrderId)
        {
            if (HttpContext.Session.TryGetValue("secret", out byte[] value))
            {
                _secret = Encoding.UTF8.GetString(value);
            }
            if (_secret != string.Empty)
            {
                ViewBag.NameUser = MOSAPI.GetUserNameBySecret(_secret);
            }
            else { return RedirectToAction("Index", "Login"); }
            SalerModel model = MOSAPI.GetOrder(OrderId,_secret);
            if (MOSAPI.ReSendMail(model.account.login_key, model.saleOrder.EmailKH, model.account.ExamLimits, model.account.Attempt.ToString(), model.account.Expired.ToString()))
            {
                TempData["error"] = 0;
                TempData["status"] = "Đã gửi lại mail cho học viên!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = 1;
                TempData["status"] = "Gửi lại mail thất bại!";
                return RedirectToAction("Index");
            }

        }
        public IActionResult ExportToCsv()
        {
            if (HttpContext.Session.TryGetValue("secret", out byte[] value))
            {
                _secret = Encoding.UTF8.GetString(value);
            }
            if (_secret != string.Empty)
            {
                ViewBag.NameUser = MOSAPI.GetUserNameBySecret(_secret);
            }
            else { return RedirectToAction("Index", "Login"); }
            DataTable SOs = MOSAPI.GetSaleOrders(_secret);

            List<SaleOrder> ListOrders = new List<SaleOrder>();
            foreach (DataRow row in SOs.Rows)
            {
                ListOrders.Add(new SaleOrder()
                {
                    OrderId = int.Parse(row["order_id"].ToString()),
                    EmployID = int.Parse(row["uid_user_nguoicap"].ToString()),
                    NgayCap = DateTime.Parse(row["ngaycap"].ToString()),
                    EmailKH = row["email_khach"].ToString(),
                    CustomerId = int.Parse(row["uid_user_khach"].ToString()),
                    Sotien = int.Parse(row["sotien"].ToString()),
                    KenhTT = row["kenhtt"].ToString()
                });

            }
            // Tạo một StringWriter để lưu trữ dữ liệu CSV
            using (var writer = new StringWriter())
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                // Ghi dữ liệu vào tệp CSV
                csv.WriteRecords(ListOrders);

                // Đặt tên tệp và loại nội dung để trình duyệt có thể tải xuống
                var result = new FileContentResult(System.Text.Encoding.UTF8.GetBytes(writer.ToString()), "text/csv");
                result.FileDownloadName = "SaleOrders.csv";

                return result;
            }

        }
    }
}
