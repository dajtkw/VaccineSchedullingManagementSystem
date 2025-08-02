using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTiemChung.Web.Interfaces;
using QuanLyTiemChung.Web.Models;
using QuanLyTiemChung.Web.ViewModels;
using System.Threading.Tasks;
using QuanLyTiemChung.Web.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using QuanLyTiemChung.Web.Hubs;
using System.Linq;

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
        private readonly IVaccineCategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly IHubContext<AdminHub> _hubContext;
        private readonly IUserRepository _userRepository; // Add IUserRepository

        public AdminController(
            UserManager<User> userManager,
            IAppointmentRepository appointmentRepository,
            IVaccineRepository vaccineRepository,
            IVaccinationSiteRepository siteRepository,
            IVaccineCategoryRepository categoryRepository,
            RoleManager<Role> roleManager,
            ApplicationDbContext context,
            ILogger<AdminController> logger,
            IHubContext<AdminHub> hubContext,
            IUserRepository userRepository) // Add IUserRepository
        {
            _userManager = userManager;
            _appointmentRepository = appointmentRepository;
            _vaccineRepository = vaccineRepository;
            _siteRepository = siteRepository;
            _categoryRepository = categoryRepository;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
            _userRepository = userRepository; // Initialize IUserRepository
        }

        #region Page & Main Content Actions

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            var viewModel = new AdminDashboardViewModel
            {
                Users = await _userManager.Users.ToListAsync(),
                AllAppointments = await _appointmentRepository.GetViewModelsAsync(),
                AllVaccines = await _vaccineRepository.GetAllAsync(),
                AllSites = await _siteRepository.GetAllAsync()
            };
            return PartialView("_DashboardContent", viewModel);
        }

        #endregion

        #region User Management API

        [HttpGet]
        public async Task<IActionResult> GetUsersData()
        {
            try
            {
                var users = await _userRepository.GetUsersWithRolesAsync();

                return PartialView("ManageUsers/_UsersContent", users);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in GetUsersData");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserDetails(int id)
        {
            var user = await _userRepository.GetUserDetailsWithAppointmentsAndRolesAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return PartialView("ManageUsers/_UserDetailsModal", user);
        }

        [HttpGet]
        public async Task<IActionResult> GetCreateUserForm()
        {
            var viewModel = new CreateUserViewModel
            {
                AllRoles = await _roleManager.Roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToListAsync()
            };

            return PartialView("ManageUsers/_CreateUserModal", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUserApi(CreateUserViewModel model)
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
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    return Json(new { success = true, message = $"Đã tạo thành công người dùng '{user.FullName}'." });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.AllRoles = await _roleManager.Roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            }).ToListAsync();

            return PartialView("ManageUsers/_CreateUserModal", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetEditUserForm(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                SelectedRole = userRoles.FirstOrDefault() ?? string.Empty,
                AllRoles = await _roleManager.Roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToListAsync()
            };

            return PartialView("ManageUsers/_EditUserModal", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserApi(EditUserViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByIdAsync(model.Id.ToString());
                    if (user == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy người dùng." });
                    }

                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var currentRole = currentRoles.FirstOrDefault();
                    bool roleChanged = currentRole != model.SelectedRole;

                    user.FullName = model.FullName;
                    user.PhoneNumber = model.PhoneNumber;
                    var updateResult = await _userManager.UpdateAsync(user);

                    if (updateResult.Succeeded)
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        await _userManager.AddToRoleAsync(user, model.SelectedRole);

                        if (roleChanged)
                        {
                            await ForceUserLogout(user);

                            await _hubContext.Clients.User(user.Id.ToString()).SendAsync("RoleChanged", new
                            {
                                oldRole = currentRole,
                                newRole = model.SelectedRole,
                                message = $"Quyền của bạn đã được thay đổi từ {currentRole} thành {model.SelectedRole}. Bạn sẽ được đăng xuất để cập nhật quyền mới."
                            });
                        }

                        string message = $"Cập nhật thông tin cho người dùng '{user.FullName}' thành công.";
                        if (roleChanged)
                        {
                            message += " Người dùng đã được thông báo và sẽ cần đăng nhập lại.";
                        }

                        return Json(new
                        {
                            success = true,
                            message = message,
                            roleChanged = roleChanged
                        });
                    }

                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                model.AllRoles = await _roleManager.Roles
                    .OrderBy(r => r.Name)
                    .Select(r => new SelectListItem
                    {
                        Value = r.Name,
                        Text = r.Name
                    }).ToListAsync();

                return PartialView("ManageUsers/_EditUserModal", model);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating user: {UserId}", model.Id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật người dùng." });
            }
        }

        private async Task ForceUserLogout(User user)
        {
            try
            {
                await _userManager.UpdateSecurityStampAsync(user);
                _logger.LogInformation("Security stamp updated for user {Email} - forcing logout", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating security stamp for user {Email}", user.Email);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserApi(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            var hasAppointments = await _userRepository.HasAppointmentsAsync(id);
            if (hasAppointments)
            {
                return Json(new { success = false, message = "Không thể xóa người dùng này vì vẫn còn lịch hẹn liên quan." });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = $"Đã xóa người dùng '{user.FullName}' thành công." });
            }

            return Json(new { success = false, message = "Có lỗi xảy ra khi xóa người dùng." });
        }

        #endregion

        #region Vaccine API Management

        [HttpGet]
        public async Task<IActionResult> GetVaccinesData()
        {
            try
            {
                var vaccines = await _vaccineRepository.GetAllAsync();
                var categories = await _categoryRepository.GetAllAsync();

                ViewBag.Categories = categories;
                return PartialView("ManageVaccines/_VaccinesContent", vaccines);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading vaccines data");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCreateVaccineForm()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = categories;

                return PartialView("ManageVaccines/_CreateVaccineModal", new Vaccine());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading create vaccine form");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVaccineApi(Vaccine vaccine)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();

                    _logger?.LogWarning("Create vaccine ModelState validation failed: {@Errors}", errors);

                    var categories = await _categoryRepository.GetAllAsync();
                    ViewBag.Categories = categories;
                    return PartialView("ManageVaccines/_CreateVaccineModal", vaccine);
                }

                await _vaccineRepository.AddVaccineWithDosesAsync(vaccine);

                return Json(new
                {
                    success = true,
                    message = $"Đã tạo thành công vắc-xin '{vaccine.TradeName}'."
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating vaccine");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo vắc-xin." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEditVaccineForm(int id)
        {
            try
            {
                var vaccine = await _vaccineRepository.GetByIdWithDosesAsync(id);
                if (vaccine == null)
                {
                    return NotFound();
                }

                var categories = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = categories;

                return PartialView("ManageVaccines/_EditVaccineModal", vaccine);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading edit vaccine form for ID: {VaccineId}", id);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVaccineApi(Vaccine vaccine)
        {
            try
            {
                _logger.LogInformation("EditVaccineApi called for Vaccine ID: {VaccineId}", vaccine.Id);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();

                    _logger?.LogWarning("ModelState validation failed in EditVaccineApi: {@Errors}", errors);

                    var categories = await _categoryRepository.GetAllAsync();
                    ViewBag.Categories = categories;
                    return PartialView("ManageVaccines/_EditVaccineModal", vaccine);
                }

                await _vaccineRepository.UpdateVaccineWithDosesAsync(vaccine);

                _logger.LogInformation("Vaccine {VaccineId} updated successfully.", vaccine.Id);
                return Json(new
                {
                    success = true,
                    message = $"Cập nhật thông tin vaccine '{vaccine.TradeName}' thành công."
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating vaccine: {VaccineId}", vaccine.Id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật vaccine." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVaccineDetails(int id)
        {
            try
            {
                var vaccine = await _vaccineRepository.GetVaccineDetailsWithAllIncludesAsync(id);

                if (vaccine == null)
                {
                    return NotFound();
                }

                return PartialView("ManageVaccines/_VaccineDetailsModal", vaccine);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading vaccine details for ID: {VaccineId}", id);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVaccineApi(int id)
        {
            try
            {
                var vaccine = await _vaccineRepository.GetByIdAsync(id);
                if (vaccine == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy vắc-xin." });
                }

                var success = await _vaccineRepository.DeleteAsync(id);
                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Đã xóa vắc-xin '{vaccine.TradeName}' thành công."
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không thể xóa vắc-xin này vì vẫn còn lịch hẹn liên quan."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting vaccine: {VaccineId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa vắc-xin." });
            }
        }

        #endregion

        #region Category API Management

        [HttpGet]
        public async Task<IActionResult> GetCategoriesData()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                return PartialView("ManageCategories/_CategoriesContent", categories);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading categories data");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCreateCategoryForm()
        {
            try
            {
                return PartialView("ManageCategories/_CreateCategoryModal", new VaccineCategory());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading create category form");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategoryApi(VaccineCategory category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _categoryRepository.AddAsync(category);

                    return Json(new
                    {
                        success = true,
                        message = $"Đã tạo thành công danh mục '{category.Name}'."
                    });
                }

                return PartialView("ManageCategories/_CreateCategoryModal", category);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating category");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo danh mục." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEditCategoryForm(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                return PartialView("ManageCategories/_EditCategoryModal", category);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading edit category form for ID: {CategoryId}", id);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategoryApi(VaccineCategory category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _categoryRepository.UpdateAsync(category);

                    return Json(new
                    {
                        success = true,
                        message = $"Cập nhật danh mục '{category.Name}' thành công."
                    });
                }

                return PartialView("ManageCategories/_EditCategoryModal", category);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating category: {CategoryId}", category.Id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật danh mục." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategoryApi(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy danh mục." });
                }

                var success = await _categoryRepository.DeleteAsync(id);
                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Đã xóa danh mục '{category.Name}' thành công."
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không thể xóa danh mục này vì vẫn còn vắc-xin thuộc về nó."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting category: {CategoryId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa danh mục." });
            }
        }

        #endregion

        #region Appointment API Management

        [HttpGet]
        public async Task<IActionResult> GetAppointmentsData()
        {
            try
            {
                var appointments = await _appointmentRepository.GetViewModelsAsync();
                return PartialView("ManageAppointments/_AppointmentsContent", appointments);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading appointments data");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointmentDetails(int id)
        {
            try
            {
                var appointment = await _appointmentRepository.GetByIdAsync(id);
                if (appointment == null)
                {
                    return NotFound();
                }

                return PartialView("ManageAppointments/_AppointmentDetailsModal", appointment);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading appointment details for ID: {AppointmentId}", id);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCreateAppointmentForm()
        {
            try
            {
                var users = await _userRepository.GetCitizensOrderedByNameAsync();

                var vaccines = await _vaccineRepository.GetAllAsync();
                var sites = await _siteRepository.GetAllAsync();

                ViewBag.Users = users.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FullName} ({u.Email})"
                }).ToList();

                ViewBag.Vaccines = vaccines.Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = v.TradeName
                }).ToList();

                ViewBag.Sites = sites.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToList();

                return PartialView("ManageAppointments/_CreateAppointmentModal", new Appointment());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading create appointment form");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAppointmentApi(Appointment appointment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    appointment.Status = "Confirmed";
                    appointment.CreatedAt = DateTime.Now;
                    await _appointmentRepository.AddAsync(appointment);

                    return Json(new
                    {
                        success = true,
                        message = "Đã tạo thành công lịch hẹn."
                    });
                }

                // Reload dropdowns
                var users = await _userRepository.GetCitizensOrderedByNameAsync();

                var vaccines = await _vaccineRepository.GetAllAsync();
                var sites = await _siteRepository.GetAllAsync();

                ViewBag.Users = users.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FullName} ({u.Email})"
                }).ToList();

                ViewBag.Vaccines = vaccines.Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = v.TradeName
                }).ToList();

                ViewBag.Sites = sites.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToList();

                return PartialView("ManageAppointments/_CreateAppointmentModal", appointment);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating appointment");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo lịch hẹn." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppointmentStatusApi(int id, string status)
        {
            // Begin a transaction to ensure data integrity
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (appointment == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lịch hẹn." });
                }

                var oldStatus = appointment.Status;
                appointment.Status = status;
                _context.Update(appointment);

                // --- INVENTORY REFUND LOGIC ---
                // Only refund stock if canceling from a "reserved" state
                if (status == "Cancelled" && (oldStatus == "Pending" || oldStatus == "Confirmed"))
                {
                    var inventoryItem = await _context.SiteVaccineInventories
                        .FirstOrDefaultAsync(i => i.VaccinationSiteId == appointment.VaccinationSiteId && i.VaccineId == appointment.VaccineId);

                    if (inventoryItem != null)
                    {
                        // Increment the quantity back by 1
                        inventoryItem.Quantity++;
                        _context.Update(inventoryItem);
                    }
                    else
                    {
                        _logger.LogWarning("Could not find inventory item to refund for cancelled Appointment ID {AppointmentId}", id);
                    }
                }

                // Save all changes (appointment status and inventory)
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = $"Đã cập nhật trạng thái lịch hẹn thành '{status}'."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating appointment status for ID {AppointmentId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAppointmentApi(int id)
        {
            try
            {
                var appointment = await _appointmentRepository.GetByIdAsync(id);
                if (appointment == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lịch hẹn." });
                }

                var success = await _appointmentRepository.DeleteAsync(id);
                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Đã xóa lịch hẹn thành công."
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không thể xóa lịch hẹn này."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting appointment: {AppointmentId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa lịch hẹn." });
            }
        }

        #endregion

        #region Site API Management

        [HttpGet]
        public async Task<IActionResult> GetSitesData()
        {
            try
            {
                var sites = await _siteRepository.GetAllAsync();

                // Lấy danh sách các tỉnh/thành phố duy nhất từ địa chỉ của các điểm tiêm
                var provinces = sites
                    .Select(s => s.Address.Split(',').LastOrDefault()?.Trim())
                    .Where(p => !string.IsNullOrEmpty(p))
                    .Distinct()
                    .OrderBy(p => p)
                    .Select(p => new SelectListItem { Value = p, Text = p })
                    .ToList();

                // Gửi danh sách tỉnh qua ViewBag để View có thể sử dụng
                ViewBag.Provinces = provinces;

                return PartialView("ManageSites/_SitesContent", sites);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading sites data");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCreateSiteForm()
        {
            try
            {
                var viewModel = new CreateSiteViewModel();
                return PartialView("ManageSites/_CreateSiteModal", viewModel);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading create site form");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSiteApi(CreateSiteViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var fullAddress = $"{model.StreetAddress}, {model.WardName}, {model.DistrictName}, {model.ProvinceName}";
                    var site = new VaccinationSite
                    {
                        Name = model.Name,
                        Address = fullAddress,
                        OperatingHours = model.OperatingHours
                    };

                    await _siteRepository.AddAsync(site);

                    return Json(new
                    {
                        success = true,
                        message = $"Đã tạo thành công địa điểm '{site.Name}'."
                    });
                }

                return PartialView("ManageSites/_CreateSiteModal", model);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating site");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo địa điểm." });
            }
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> GetEditSiteForm(int id)
        {
            var site = await _siteRepository.GetByIdAsync(id);
            if (site == null)
            {
                return NotFound();
            }

            var viewModel = new EditSiteViewModel
            {
                Id = site.Id,
                Name = site.Name,
                OperatingHours = site.OperatingHours
            };

            // Phân tích chuỗi địa chỉ để điền vào form
            // Giả định định dạng là: "Số nhà, Phường, Huyện, Tỉnh"
            var addressParts = site.Address.Split(new[] { ", " }, StringSplitOptions.None);
            if (addressParts.Length == 4)
            {
                viewModel.StreetAddress = addressParts[0];
                viewModel.WardName = addressParts[1];
                viewModel.DistrictName = addressParts[2];
                viewModel.ProvinceName = addressParts[3];
            }
            else
            {
                // Nếu không phân tích được, gán toàn bộ vào ô số nhà
                viewModel.StreetAddress = site.Address;
            }

            return PartialView("ManageSites/_EditSiteModal", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSiteApi(EditSiteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("ManageSites/_EditSiteModal", model);
            }

            var site = await _siteRepository.GetByIdAsync(model.Id);
            if (site == null)
            {
                return Json(new { success = false, message = "Không tìm thấy điểm tiêm." });
            }

            // Ghép lại địa chỉ từ form
            var fullAddress = $"{model.StreetAddress}, {model.WardName}, {model.DistrictName}, {model.ProvinceName}";

            site.Name = model.Name;
            site.Address = fullAddress;
            site.OperatingHours = model.OperatingHours;

            await _siteRepository.UpdateAsync(site);

            return Json(new
            {
                success = true,
                message = $"Cập nhật điểm tiêm '{site.Name}' thành công."
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetSiteDetails(int id)
        {
            try
            {
                var site = await _siteRepository.GetByIdAsync(id);
                if (site == null)
                {
                    return NotFound();
                }

                return PartialView("ManageSites/_SiteDetailsModal", site);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading site details for ID: {SiteId}", id);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSiteApi(int id)
        {
            try
            {
                var site = await _siteRepository.GetByIdAsync(id);
                if (site == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy địa điểm." });
                }

                var success = await _siteRepository.DeleteAsync(id);
                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Đã xóa địa điểm '{site.Name}' thành công."
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không thể xóa địa điểm này vì vẫn còn lịch hẹn liên quan."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting site: {SiteId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa địa điểm." });
            }
        }

        #endregion

        #region Inventory Management

        [HttpGet]
        public async Task<IActionResult> GetInventoryManagementView()
        {
            // Lấy tất cả các điểm tiêm để hiển thị trong dropdown
            var allSites = await _siteRepository.GetAllAsync();
            return PartialView("ManageInventory/_InventoryManagementContent", allSites);
        }

        [HttpGet]
        public async Task<IActionResult> GetInventoryForSite(int siteId)
        {
            if (siteId <= 0)
            {
                return PartialView("ManageInventory/_InventoryTable", new List<InventoryViewModel>());
            }

            // Lấy tất cả các loại vắc-xin
            var allVaccines = await _vaccineRepository.GetAllAsync();

            // Lấy tồn kho hiện tại của điểm tiêm đã chọn
            var currentInventory = await _context.SiteVaccineInventories
                .Where(i => i.VaccinationSiteId == siteId)
                .ToListAsync();

            // Kết hợp hai danh sách trên để tạo ViewModel
            var viewModel = allVaccines.Select(vaccine => new InventoryViewModel
            {
                VaccineId = vaccine.Id,
                VaccineName = vaccine.TradeName,
                GenericName = vaccine.GenericName,
                // Tìm số lượng, nếu không có thì mặc định là 0
                Quantity = currentInventory.FirstOrDefault(i => i.VaccineId == vaccine.Id)?.Quantity ?? 0
            }).ToList();

            ViewBag.SiteId = siteId;
            return PartialView("ManageInventory/_InventoryTable", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInventory(int siteId, int vaccineId, int quantity)
        {
            if (siteId <= 0 || vaccineId <= 0 || quantity < 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            try
            {
                var inventoryItem = await _context.SiteVaccineInventories
                    .FirstOrDefaultAsync(i => i.VaccinationSiteId == siteId && i.VaccineId == vaccineId);

                if (inventoryItem != null)
                {
                    // Nếu đã có, cập nhật số lượng
                    inventoryItem.Quantity = quantity;
                    _context.Update(inventoryItem);
                }
                else
                {
                    // Nếu chưa có, tạo mới
                    inventoryItem = new SiteVaccineInventory
                    {
                        VaccinationSiteId = siteId,
                        VaccineId = vaccineId,
                        Quantity = quantity
                    };
                    _context.Add(inventoryItem);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật tồn kho thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật tồn kho.");
                return Json(new { success = false, message = "Đã xảy ra lỗi không mong muốn." });
            }
        }

        #endregion

    }
}