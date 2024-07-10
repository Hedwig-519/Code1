using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV20T1020362.BusinessLayers;
using SV20T1020362.DomainModels;
using SV20T1020362.Web.AppCodes;
using SV20T1020362.Web.Models;

namespace SV20T1020362.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator},{WebUserRoles.Employee}")]
    public class EmployeeController : Controller
    {
        private const int PAGE_SIZE = 20;
        private const string CUSTOMER_SEARCH = "customer_search";//Tên biến dùng để lưu trong session

        public IActionResult Index()
        {
            //Lấy đầu vào tìm kiếm hiện đang lưu lại trong session
            PaginationSearchInput input = ApplicationContext.GetSessionData<PaginationSearchInput>(CUSTOMER_SEARCH);
            //Trường hợp trong session chưa có điều kiện thì tạo điều kiện mới
            if (input == null)
            {
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = ""
                };
            }
            return View(input);
        }
        public IActionResult Search(PaginationSearchInput input)
        {
            int rowCount = 0;
            var data = CommonDataService.ListOfEmployees(out rowCount, input.Page, input.PageSize, input.SearchValue ?? "");
            var model = new EmployeeSearchResult()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                RowCount = rowCount,
                Data = data
            };
            // lưu lại điều kiện tìm kiếm vào trong session
            ApplicationContext.SetSessionData(CUSTOMER_SEARCH, input);
            return View(model);
        }
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhân viên";
            Employee model = new Employee()
            {
                EmployeeID = 0,
                BirthDate = new DateTime(1990,1,1),
                Photo = "nophoto.png"
            };
            return View("Edit", model);
        }
        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin nhân viên";
            Employee? model = CommonDataService.GetEmployee(id);
            if (model == null)  
                return RedirectToAction("Index");
            if (string.IsNullOrEmpty(model.Photo))
                model.Photo = "nophoto.png";
            return View(model);
        }
        [HttpPost]
        public IActionResult Save(Employee data,string birthDateInput , IFormFile? uploadPhoto )
        {
            //Xử lý ngày sinh 
            DateTime? birthDate = birthDateInput.ToDateTime();
            if (birthDate.HasValue)
                data.BirthDate = birthDate.Value;
            // Xử lý ảnh upload (nếu có ảnh upload thì lưu ảnh và gán lại trên file ảnh mới cho employee)
            if(uploadPhoto != null)
            {
                string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";//Tên file sẽ lưu
                string folder = Path.Combine(ApplicationContext.HostEnviroment.WebRootPath, "images\\employees"); //đường dẫn đến thư mục 
                string filePath = Path.Combine(folder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadPhoto.CopyTo(stream);
                }
                data.Photo = fileName;
            }

            try
            {
                ViewBag.Title = data.EmployeeID == 0 ? "Bổ sung nhân viên" : "Cập nhật thông tin nhân viên";
                //Kiểm soát đầu vào và đưa các thông báo lỗi vào trong ModelState (Nếu có )
                if (string.IsNullOrWhiteSpace(data.FullName))
                    ModelState.AddModelError("FullName", "Tên không được để trống");
                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError("Email", "Email không được để trống");
                // Thông qua thuộc tính IsValid của ModelState kiểm tra xem có tồn tại lỗi hay không
                if (!ModelState.IsValid)
                {
                    return View("Edit", data);
                }
                if (data.EmployeeID == 0)
                {
                    int id = CommonDataService.AddEmployee(data);
                    if (id <= 0)
                    {
                        ModelState.AddModelError(nameof(data.Email), "Địa chỉ email bị trùng");
                        return View("Edit", data);
                    }
                    
                }
                else
                {
                    bool result = CommonDataService.UpdateEmployee(data);
                    if (!result)
                    {
                        ModelState.AddModelError(nameof(data.Email), "Địa chỉ email bị trùng với khách hàng khác");
                        return View("Edit", data);
                    }
                    var user = User.GetUserData();
                    if (user?.UserId == data.EmployeeID.ToString())
                    {
                        WebUserData userData = new WebUserData()
                        {
                            UserId = data.EmployeeID.ToString(),
                            UserName = data.Email,
                            DisplayName = data.FullName,
                            Email = data.Email,
                            Photo = data.Photo,
                            ClientIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                            SessionId = HttpContext.Session.Id,
                            AdditionalData = "",
                            Roles = data.RoleNames.Split(',').ToList()
                        };
                        HttpContext.SignInAsync(userData.CreatePrincipal());
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", "Không thể lưu được dữ liệu");
                return Content(ex.Message);
            }
        }
        public IActionResult Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                CommonDataService.DeleteEmployee(id);
                return RedirectToAction("Index");
            };
            var model = CommonDataService.GetEmployee(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.AllowDelete = !CommonDataService.IsUsedEmployee(id);

            return View(model);
        }
        public IActionResult ChangePassword(int id = 0)
        {
            var data = CommonDataService.GetEmployee(id);
            return View(data);
        }
        public IActionResult SavePassword(string UserName = "", string OldPassword = "", string NewPassword = "", int id = 0)
        {
            bool result = false;
            var data = CommonDataService.GetEmployee(id);
            if (string.IsNullOrWhiteSpace(OldPassword))
                ModelState.AddModelError("OldPassword", "Vui lòng nhập mật khẩu cũ");
            if (string.IsNullOrWhiteSpace(NewPassword))
                ModelState.AddModelError("NewPassword", "Vui lòng nhập mật khẩu mới");

            if (!string.IsNullOrWhiteSpace(UserName))
            {
                result = UserAccountService.ChangePassword(UserName, OldPassword, NewPassword);
            }

            if (!result)
            {
                ModelState.AddModelError("Result", "Đổi mật khẩu không thành công");
            }
            if (!ModelState.IsValid)
            {
                return View("ChangePassword", data);
            }
            return View("Edit", data);
        }
    }
}
