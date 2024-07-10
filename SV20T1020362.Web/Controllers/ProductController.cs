using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SV20T1020362.BusinessLayers;
using SV20T1020362.DomainModels;
using SV20T1020362.Web.AppCodes;
using SV20T1020362.Web.Models;

namespace SV20T1020362.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator},{WebUserRoles.Employee}")]
    public class ProductController : Controller
    {
        private const int PAGE_SIZE = 15;
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
            var data = ProductDataService.ListProducts(out rowCount, input.Page, input.PageSize, input.SearchValue ?? "");
            var model = new ProductSearchResult()
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
            ViewBag.Title = "Bổ sung mặt hàng";
            ViewBag.IsEdit = false;

            Product model = new Product()
            {
                ProductID = 0,
                Photo = "noproductimg.png"
            };
            var model1 = new ProductAttributePhotoSearchResult()
            {
                Data = model
            };
            return View("Edit", model1);
        }
        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin nhân viên";
            ViewBag.IsEdit = true;
            Product? model = ProductDataService.GetProduct(id);
            if (model == null)
                return RedirectToAction("Index");
            if (string.IsNullOrEmpty(model.Photo))
                model.Photo = "noproductimg.png";
            List<ProductAttribute>? model1 = ProductDataService.ListAttributes(id);
            List<ProductPhoto>? model2 = ProductDataService.ListPhotos(id);
            var model3 = new Models.ProductAttributePhotoSearchResult()
            {
                Data = model,
                Attribute = model1,
                Photo = model2
            };
            return View(model3);
        }
        [HttpPost]
        public IActionResult Save(Product data, IFormFile? uploadPhoto)
        {

            try
            {
                ViewBag.Title = data.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật thông tin mặt hàng";
                ViewBag.IsEdit = data.ProductID == 0 ? false : true;
                //Kiểm soát đầu vào và đưa các thông báo lỗi vào trong ModelState (Nếu có )
                if (string.IsNullOrWhiteSpace(data.ProductName))
                    ModelState.AddModelError("ProductName", "Tên mặt hàng không được để trống");
                if (data.CategoryID == 0)
                {
                    ModelState.AddModelError("CategoryID", "Vui lòng chọn loại hàng");
                }
                if (data.SupplierID == 0)
                {
                    ModelState.AddModelError("SupplierID", "Vui lòng chọn nhà cung cấp");
                }
                if (data.Price == 0)
                {
                    ModelState.AddModelError("Price", "Vui lòng nhập giá");
                }
                // Thông qua thuộc tính IsValid của ModelState kiểm tra xem có tồn tại lỗi hay không
                if (!ModelState.IsValid)
                {
                    if (data.ProductID == 0)
                    {

                        var model1 = new Models.ProductAttributePhotoSearchResult()
                        {
                            Data = data
                        };

                        return View("Edit", model1);
                    }
                    else
                    {

                        List<ProductPhoto>? ListDataPhoto = ProductDataService.ListPhotos(data.ProductID);
                        List<ProductAttribute>? ListDataAttribute = ProductDataService.ListAttributes(data.ProductID);
                        var model2 = new Models.ProductAttributePhotoSearchResult()
                        {
                            Data = data,
                            Photo = ListDataPhoto,
                            Attribute = ListDataAttribute
                        };
                        return View("Edit", model2);
                    }

                }
                if (uploadPhoto != null)
                {
                    string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";//Tên file sẽ lưu
                    string folder = Path.Combine(ApplicationContext.HostEnviroment.WebRootPath, "images\\products"); //đường dẫn đến thư mục 
                    string filePath = Path.Combine(folder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        uploadPhoto.CopyTo(stream);
                    }
                    data.Photo = fileName;
                }
                if (data.ProductID == 0)
                {
                    int id = ProductDataService.AddProduct(data);
                    if (id <= 0)
                    {
                        ModelState.AddModelError("Error", "Xảy ra lỗi khi thêm mặt hàng, vui lòng thử lại sau");
                        return View("Edit", data);
                    }
                }
                else
                {
                    bool result = ProductDataService.UpdateProduct(data);
                    if (!result)
                    {
                        ModelState.AddModelError("Error", "Xảy ra lỗi khi sửa mặt hàng, vui lòng thử lại sau");
                        return View("Edit", data);
                    }
                }
                return RedirectToAction("Edit", new { id = data.ProductID });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", "Không thể lưu được dữ liệu");
                return Content(ex.Message);
            }

        }
        public IActionResult Delete(int id)
        {
            if (Request.Method == "POST")
            {
                ProductDataService.DeleteProduct(id);
                return RedirectToAction("Index");
            }
            var model = ProductDataService.GetProduct(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.AllowDelete = !ProductDataService.InUsedProduct(id);
            return View(model);
        }
        public IActionResult Photo(string id, string method, int photoId = 0)
        {
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung ảnh";
                    ProductPhoto modelAdd = new ProductPhoto()
                    {
                        ProductID = int.Parse(id),
                        PhotoID = 0
                    };
                    return View(modelAdd);
                case "edit":
                    if(photoId == 0)
                    ViewBag.Title = "Thay đổi ảnh";
                    long convertID = (long)photoId;
                    ProductPhoto? model = ProductDataService.GetPhoto(convertID);
                    return View(model);
                case "delete":
                    var deleteID = (long)photoId;
                    ProductDataService.DeletePhoto(deleteID);
                    return RedirectToAction("Edit", new { id = id });
                default:
                    return RedirectToAction("Index");
            }
        }
        public IActionResult SavePhoto(ProductPhoto data, IFormFile? uploadPhoto)
        {
            
            try
            {
                if (uploadPhoto != null)
                {
                    string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";//Tên file sẽ lưu
                    string folder = Path.Combine(ApplicationContext.HostEnviroment.WebRootPath, "images\\products"); //đường dẫn đến thư mục 
                    string filePath = Path.Combine(folder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        uploadPhoto.CopyTo(stream);
                    }
                    data.Photo = fileName;
                }
                if (data.PhotoID == 0)
                {
                    long id = ProductDataService.AddPhoto(data);
                }
                else
                {
                    bool result = ProductDataService.UpdatePhoto(data);
                }
                return RedirectToAction("Edit",new {id=data.ProductID});
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
        public IActionResult Attribute(string id, string method, int attributeId = 0)
        {
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung thuộc tính";
                    ProductAttribute modelAdd = new ProductAttribute()
                    {
                        ProductID = int.Parse(id),
                        AttributeID = 0
                    };
                    return View(modelAdd);
                case "edit":
                    ViewBag.Title = "Thay đổi thuộc tính";
                    long convertID = (long)attributeId;
                    ProductAttribute? model = ProductDataService.GetAttribute(attributeId);
                    return View(model);
                case "delete":
                    var deleteID = (long)attributeId;
                    ProductDataService.DeleteAttribute(deleteID);
                    return RedirectToAction("Edit", new { id = id });
                default:
                    return RedirectToAction("Index");
            }
        }
        public IActionResult SaveAttribute(ProductAttribute data)
        {
            try
            {
                if (data.AttributeID == 0)
                {
                    long id = ProductDataService.AddAttribute(data);
                }
                else
                {
                    bool result = ProductDataService.UpdateAttribute(data);
                }
                return RedirectToAction("Edit", new { id = data.ProductID });
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
    }
}
