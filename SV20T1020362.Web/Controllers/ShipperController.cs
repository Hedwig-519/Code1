﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV20T1020362.BusinessLayers;
using SV20T1020362.DomainModels;
using SV20T1020362.Web.AppCodes;
using SV20T1020362.Web.Models;

namespace SV20T1020362.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator},{WebUserRoles.Employee}")]
    public class ShipperController : Controller
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
            var data = CommonDataService.ListOfShippers(out rowCount, input.Page, input.PageSize, input.SearchValue ?? "");
            var model = new ShipperSearchResult()
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
            ViewBag.Title = "Bổ sung người giao hàng";
            Shipper model = new Shipper()
            {
                ShipperID = 0
            };
            return View("Edit", model);
        }
        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin người giao hàng";
            Shipper? model = CommonDataService.GetShipper(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }
        [HttpPost]
        public IActionResult Save(Shipper data)
        {
            try
            {
                ViewBag.Title = data.ShipperID == 0 ? "Bổ sung người giao hàng" : "Cập nhật thông tin người giao hàng";
                //Kiểm soát đầu vào và đưa các thông báo lỗi vào trong ModelState (Nếu có )
                if (string.IsNullOrWhiteSpace(data.ShipperName))
                    ModelState.AddModelError("ShipperName", "Tên không được để trống");
                if (string.IsNullOrWhiteSpace(data.Phone))
                    ModelState.AddModelError("Phone", "Số điện thoại không được để trống");
                // Thông qua thuộc tính IsValid của ModelState kiểm tra xem có tồn tại lỗi hay không
                if (!ModelState.IsValid)
                {
                    return View("Edit", data);
                }
                if (data.ShipperID == 0)
                {
                    int id = CommonDataService.AddShipper(data);
                }
                else
                {
                    bool result = CommonDataService.UpdateShipper(data);
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
                CommonDataService.DeleteShipper(id);
                return RedirectToAction("Index");
            };
            var model = CommonDataService.GetShipper(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.AllowDelete = !CommonDataService.IsUsedShipper(id);

            return View(model);
        }
    }
}
