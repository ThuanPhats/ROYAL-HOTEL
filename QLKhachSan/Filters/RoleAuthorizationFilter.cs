using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QLKhachSan.Filters
{
    public class RoleAuthorizationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controllerName = context.RouteData.Values["controller"]?.ToString();
            var actionName = context.RouteData.Values["action"]?.ToString();

            // Bypass check for AccountController (Login, Logout)
            if (string.Equals(controllerName, "Account", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            var session = context.HttpContext.Session;
            var taiKhoanId = session.GetInt32("TaiKhoanId");

            if (!taiKhoanId.HasValue)
            {
                // Not logged in, redirect to login page
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            var vaiTro = session.GetString("VaiTro");

            // Admin has full access
            if (string.Equals(vaiTro, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            bool isAuthorized = false;

            if (string.Equals(vaiTro, "QuanLy", StringComparison.OrdinalIgnoreCase))
            {
                // Manager has full access to everything else
                isAuthorized = true;
            }
            else if (string.Equals(vaiTro, "LeTan", StringComparison.OrdinalIgnoreCase))
            {
                // Receptionist allowed actions:
                // 1. Dashboard (HomeController)
                if (string.Equals(controllerName, "Home", StringComparison.OrdinalIgnoreCase))
                {
                    isAuthorized = true;
                }
                // 2. FrontDeskController (CreateBooking, SaveBooking, CheckIn, ProcessCheckIn)
                else if (string.Equals(controllerName, "FrontDesk", StringComparison.OrdinalIgnoreCase))
                {
                    isAuthorized = true;
                }
                // 3. CatalogController (Only Customers actions)
                else if (string.Equals(controllerName, "Catalog", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(actionName, "Customers", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(actionName, "CreateCustomer", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(actionName, "EditCustomer", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(actionName, "DeleteCustomer", StringComparison.OrdinalIgnoreCase))
                    {
                        isAuthorized = true;
                    }
                }
                // 4. ServiceBillingController (OrderService, SaveOrder, CheckOut, ProcessCheckOut)
                else if (string.Equals(controllerName, "ServiceBilling", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(actionName, "CheckOut", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(actionName, "ProcessCheckOut", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(actionName, "OrderService", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(actionName, "SaveOrder", StringComparison.OrdinalIgnoreCase))
                    {
                        isAuthorized = true;
                    }
                }
            }

            if (!isAuthorized)
            {
                var controller = context.Controller as Controller;
                if (controller != null)
                {
                    controller.TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này!";
                }
                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }

            await next();
        }
    }
}
