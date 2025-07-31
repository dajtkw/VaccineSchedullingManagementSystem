using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTiemChung.Web.Interfaces;
using QuanLyTiemChung.Web.Models;
using QuanLyTiemChung.Web.ViewModels;
using System.Threading.Tasks;

namespace QuanLyTiemChung.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IVaccineRepository _vaccineRepository;
        private readonly IVaccinationSiteRepository _siteRepository;
        private readonly IVaccineCategoryRepository _categoryRepository; // Thêm repository mới

        public AdminController(
            UserManager<User> userManager,
            IAppointmentRepository appointmentRepository,
            IVaccineRepository vaccineRepository,
            IVaccinationSiteRepository siteRepository,
            IVaccineCategoryRepository categoryRepository,
            RoleManager<Role> roleManager) // Thêm vào constructor
        {
            _userManager = userManager;
            _appointmentRepository = appointmentRepository;
            _vaccineRepository = vaccineRepository;
            _siteRepository = siteRepository;
            _categoryRepository = categoryRepository;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                Users = await _userManager.Users.ToListAsync(),
                AllAppointments = await _appointmentRepository.GetViewModelsAsync(),
                AllVaccines = await _vaccineRepository.GetAllAsync(),
                AllSites = await _siteRepository.GetAllAsync()
            };
            return View(viewModel);
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> ManageAppointments()
        {
            var appointments = await _appointmentRepository.GetViewModelsAsync();
            return View(appointments);
        }

        #region Vaccine Management

        public async Task<IActionResult> ManageVaccines()
        {
            var vaccines = await _vaccineRepository.GetAllAsync(); // Nên Include Category ở đây
            return View(vaccines);
        }

        // Cập nhật CreateVaccine [GET]
        public async Task<IActionResult> CreateVaccine()
        {
            ViewData["VaccineCategories"] = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
            return View(new Vaccine());
        }

        // Cập nhật CreateVaccine [POST]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVaccine(Vaccine vaccine)
        {
            if (ModelState.IsValid)
            {
                await _vaccineRepository.AddAsync(vaccine);
                TempData["StatusMessage"] = "Thêm vắc-xin mới thành công.";
                return RedirectToAction(nameof(ManageVaccines));
            }
            // Nếu lỗi, tải lại danh sách category
            ViewData["VaccineCategories"] = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name", vaccine.VaccineCategoryId);
            return View(vaccine);
        }

        // Cập nhật EditVaccine [GET]
        public async Task<IActionResult> EditVaccine(int id)
        {
            var vaccine = await _vaccineRepository.GetByIdAsync(id);
            if (vaccine == null)
            {
                return NotFound();
            }
            ViewData["VaccineCategories"] = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name", vaccine.VaccineCategoryId);
            return View(vaccine);
        }

        // Cập nhật EditVaccine [POST]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVaccine(int id, Vaccine vaccine)
        {
            if (id != vaccine.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _vaccineRepository.UpdateAsync(vaccine);
                TempData["StatusMessage"] = "Cập nhật vắc-xin thành công.";
                return RedirectToAction(nameof(ManageVaccines));
            }
            // Nếu lỗi, tải lại danh sách category
            ViewData["VaccineCategories"] = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name", vaccine.VaccineCategoryId);
            return View(vaccine);
        }

        public async Task<IActionResult> DeleteVaccine(int id)
        {
            var vaccine = await _vaccineRepository.GetByIdAsync(id);
            if (vaccine == null) return NotFound();
            return View(vaccine);
        }

        [HttpPost, ActionName("DeleteVaccine")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVaccineConfirmed(int id)
        {
            var success = await _vaccineRepository.DeleteAsync(id);
            if (success)
            {
                TempData["StatusMessage"] = "Xóa vắc-xin thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa vắc-xin này vì vẫn còn lịch hẹn liên quan.";
            }
            return RedirectToAction(nameof(ManageVaccines));
        }

        #endregion

        #region Vaccine Category Management (PHẦN MỚI)

        // GET: /Admin/ManageCategories
        public async Task<IActionResult> ManageCategories()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }

        // GET: /Admin/CreateCategory
        public IActionResult CreateCategory()
        {
            return View();
        }

        // POST: /Admin/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(VaccineCategory category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.AddAsync(category);
                TempData["StatusMessage"] = "Thêm danh mục mới thành công.";
                return RedirectToAction(nameof(ManageCategories));
            }
            return View(category);
        }

        // GET: /Admin/EditCategory/5
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: /Admin/EditCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, VaccineCategory category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _categoryRepository.UpdateAsync(category);
                TempData["StatusMessage"] = "Cập nhật danh mục thành công.";
                return RedirectToAction(nameof(ManageCategories));
            }
            return View(category);
        }

        // GET: /Admin/DeleteCategory/5
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: /Admin/DeleteCategory/5
        [HttpPost, ActionName("DeleteCategory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategoryConfirmed(int id)
        {
            var success = await _categoryRepository.DeleteAsync(id);
            if (success)
            {
                TempData["StatusMessage"] = "Xóa danh mục thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa danh mục này vì vẫn còn vắc-xin thuộc về nó.";
            }
            return RedirectToAction(nameof(ManageCategories));
        }

        #endregion

        #region Vaccination Site Management
        
        public async Task<IActionResult> ManageSites()
        {
            var sites = await _siteRepository.GetAllAsync();
            return View(sites);
        }

        public IActionResult CreateSite()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSite(VaccinationSite site)
        {
            if (ModelState.IsValid)
            {
                await _siteRepository.AddAsync(site);
                TempData["StatusMessage"] = "Thêm địa điểm tiêm chủng mới thành công.";
                return RedirectToAction(nameof(ManageSites));
            }
            
            if (Request.Form.ContainsKey("province_code"))
            {
                ViewData["SelectedProvinceCode"] = Request.Form["province_code"];
                ViewData["SelectedDistrictCode"] = Request.Form["district_code"];
                ViewData["SelectedWardCode"] = Request.Form["ward_code"];
                ViewData["EnteredStreet"] = Request.Form["street"];
            }
            return View(site);
        }

        public async Task<IActionResult> EditSite(int id)
        {
            var site = await _siteRepository.GetByIdAsync(id);
            if (site == null)
            {
                return NotFound();
            }
            
            return View(site);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSite(int id, VaccinationSite site)
        {
            if (id != site.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                if (Request.Form.ContainsKey("province_code"))
                {
                    ViewData["SelectedProvinceCode"] = Request.Form["province_code"];
                    ViewData["SelectedDistrictCode"] = Request.Form["district_code"];
                    ViewData["SelectedWardCode"]     = Request.Form["ward_code"];
                    ViewData["EnteredStreet"]        = Request.Form["street"];
                }
                
                return View(site);
            }

            try
            {
                await _siteRepository.UpdateAsync(site);
                TempData["StatusMessage"] = "Cập nhật địa điểm tiêm chủng thành công.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _siteRepository.GetByIdAsync(id) == null)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(ManageSites));
        }

        public async Task<IActionResult> DeleteSite(int id)
        {
            var site = await _siteRepository.GetByIdAsync(id);
            if (site == null) return NotFound();
            return View(site);
        }

        [HttpPost, ActionName("DeleteSite")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSiteConfirmed(int id)
        {
            var success = await _siteRepository.DeleteAsync(id);
            if (success)
            {
                TempData["StatusMessage"] = "Xóa địa điểm tiêm chủng thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa địa điểm này vì vẫn còn lịch hẹn liên quan.";
            }
            return RedirectToAction(nameof(ManageSites));
        }

        // Thêm 2 action này vào bên trong lớp AdminController

        // GET: /Admin/EditUser/5
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            // Lấy vai trò hiện tại của người dùng
            var userRoles = await _userManager.GetRolesAsync(user);

            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                SelectedRole = userRoles.FirstOrDefault() ?? string.Empty, // Gán vai trò hiện tại
                AllRoles = await _roleManager.Roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToListAsync()
            };

            return View(viewModel);
        }

            // POST: /Admin/EditUser/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> EditUser(EditUserViewModel model)
            {
                if (!ModelState.IsValid)
                {
                    // Nếu có lỗi, tải lại danh sách vai trò và trả về view
                    model.AllRoles = await _roleManager.Roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToListAsync();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin cơ bản
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            await _userManager.UpdateAsync(user);

            // Cập nhật vai trò
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles); // Xóa các vai trò cũ
            await _userManager.AddToRoleAsync(user, model.SelectedRole); // Thêm vai trò mới

            TempData["StatusMessage"] = $"Cập nhật thông tin cho người dùng '{user.FullName}' thành công.";
            return RedirectToAction(nameof(ManageUsers));
        }

        // GET: /Admin/UserDetails/5
        public async Task<IActionResult> UserDetails(int id)
        {
            var user = await _userManager.Users
                // Tải các thông tin liên quan để hiển thị
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Appointments).ThenInclude(a => a.Vaccine)
                .Include(u => u.Appointments).ThenInclude(a => a.VaccinationSite)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: /Admin/CreateUser
        public async Task<IActionResult> CreateUser()
        {
            var viewModel = new CreateUserViewModel
            {
                // Tải danh sách các vai trò để hiển thị trong dropdown
                AllRoles = await _roleManager.Roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToListAsync()
            };
            return View(viewModel);
        }

        // POST: /Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    DateOfBirth = model.DateOfBirth,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    EmailConfirmed = true // Tự động xác thực email khi Admin tạo
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Gán vai trò đã chọn cho người dùng mới
                    await _userManager.AddToRoleAsync(user, model.SelectedRole);

                    TempData["StatusMessage"] = $"Đã tạo thành công người dùng '{user.FullName}'.";
                    return RedirectToAction(nameof(ManageUsers));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Nếu có lỗi, tải lại danh sách vai trò và hiển thị lại form
            model.AllRoles = await _roleManager.Roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            }).ToListAsync();
            return View(model);
        }
        
        public async Task<IActionResult> VaccineDetails(int id)
        {
            // GetByIdAsync đã bao gồm Doses và VaccineCategory
            var vaccine = await _vaccineRepository.GetByIdAsync(id); 

            if (vaccine == null)
            {
                return NotFound();
            }

            return View(vaccine);
        }


        #endregion
    }
}
