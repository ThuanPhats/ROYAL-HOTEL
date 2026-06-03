using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKhachSan.Models;
using QLKhachSan.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace QLKhachSan.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Action Seed Roles & Accounts để chạy thử nghiệm dễ dàng
        [HttpGet]
        public async Task<IActionResult> Seed()
        {
            string[] roles = { "Admin", "LeTan", "HouseKeeping", "KyThuat", "QuanLy" };

            // 1. Tạo các Role
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Tạo các User test tương ứng với từng vai trò
            var usersData = new[]
            {
                new { Username = "admin", Email = "admin@hotel.com", Role = "Admin" },
                new { Username = "letan", Email = "letan@hotel.com", Role = "LeTan" },
                new { Username = "buongphong", Email = "buongphong@hotel.com", Role = "HouseKeeping" },
                new { Username = "kythuat", Email = "kythuat@hotel.com", Role = "KyThuat" },
                new { Username = "quanly", Email = "quanly@hotel.com", Role = "QuanLy" }
            };

            foreach (var u in usersData)
            {
                var user = await _userManager.FindByNameAsync(u.Username);
                if (user == null)
                {
                    user = new IdentityUser { UserName = u.Username, Email = u.Email, EmailConfirmed = true };
                    var result = await _userManager.CreateAsync(user, "1234"); // Mật khẩu là 1234
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, u.Role);

                        // Liên kết với bảng NhanVien nếu có
                        var nhanVien = await _unitOfWork.GetRepository<NhanVien>().GetQueryable()
                            .FirstOrDefaultAsync(n => n.Email == u.Email);
                        if (nhanVien != null)
                        {
                            nhanVien.AppUserId = user.Id;
                            _unitOfWork.GetRepository<NhanVien>().Update(nhanVien);
                        }
                    }
                }
            }
            await _unitOfWork.CompleteAsync();

            return Content("Khởi tạo Roles và Accounts thành công! Sử dụng mật khẩu '1234' để đăng nhập. Tài khoản: admin, letan, buongphong, kythuat, quanly.");
        }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
    }
}
