// wwwroot/js/admin-spa.js
$(document).ready(function () {
  // Load dashboard by default
  loadContent("dashboard");

  // Handle sidebar navigation
  $(".sidebar-nav .nav-link").on("click", function (e) {
    e.preventDefault();

    // Update active state
    $(".sidebar-nav .nav-link").removeClass("active");
    $(this).addClass("active");

    // Load content
    const target = $(this).data("target");
    loadContent(target);

  // Handle browser back/forward
  window.addEventListener("popstate", function (e) {
    if (e.state && e.state.target) {
      loadContent(e.state.target, false);
      updateActiveNavigation(e.state.target);
    }
  });

  // Handle refresh button
  window.refreshContent = function () {
    const activeLink = $(".sidebar-nav .nav-link.active");
    const target = activeLink.data("target") || "dashboard";
    loadContent(target);
  };

  // =============================================================================
  // GLOBAL EVENT DELEGATION (Handle all dynamic content)
  // =============================================================================

  // Handle user action buttons (view, edit, delete)
  $(document).on("click", "[data-action]", function () {
    const action = $(this).data("action");
    const userId = $(this).data("user-id");
    const userName = $(this).data("user-name");
    const categoryId = $(this).data("category-id");
    const categoryName = $(this).data("category-name");
    const vaccineId = $(this).data("vaccine-id");
    const vaccineName = $(this).data("vaccine-name");
    const siteId = $(this).data("site-id");
    const siteName = $(this).data("site-name");
    const appointmentId = $(this).data("appointment-id");

    switch (action) {
      case "view":
        if (userId) viewUserDetails(userId);
        break;
      case "edit":
        if (userId) editUser(userId);
        if (categoryId) editCategory(categoryId, categoryName);
        break;
      case "delete":
        if (userId) confirmDeleteUser(userId, userName);
        if (categoryId) confirmDeleteCategory(categoryId, categoryName);
        break;
      case "view-vaccine":
        if (vaccineId) viewVaccineDetails(vaccineId);
        break;
      case "edit-vaccine":
        if (vaccineId) editVaccine(vaccineId);
        break;
      case "delete-vaccine":
        if (vaccineId) confirmDeleteVaccine(vaccineId, vaccineName);
        break;
      case "view-site":
        if (siteId) viewSiteDetails(siteId);
        break;
      case "edit-site":
        if (siteId) editSite(siteId);
        break;
      case "delete-site":
        if (siteId) confirmDeleteSite(siteId, siteName);
        break;
      case "view-appointment":
        if (appointmentId) viewAppointmentDetails(appointmentId);
        break;
      case "confirm-appointment":
        if (appointmentId) confirmAppointment(appointmentId);
        break;
      case "cancel-appointment":
        if (appointmentId) cancelAppointment(appointmentId);
        break;
      case "edit-appointment":
        if (appointmentId) editAppointment(appointmentId);
        break;
      case "delete-appointment":
        if (appointmentId) confirmDeleteAppointment(appointmentId);
        break;
    }
  });

  // =============================================================================
  // FORM SUBMISSION HANDLERS (via delegation)
  // =============================================================================

  // User forms
  $(document).on("submit", "#createUserForm", function (e) {
    e.preventDefault();
    handleModalFormSubmit($(this), "users");
  });

  $(document).on("submit", "#editUserForm", function (e) {
    e.preventDefault();
    handleModalFormSubmit($(this), "users");
  });

  // Vaccine forms
  $(document).on("submit", "#createVaccineForm", function (e) {
    e.preventDefault();
    handleModalFormSubmit($(this), "vaccines");
  });

  $(document).on("submit", "#editVaccineForm", function (e) {
    e.preventDefault();
    handleModalFormSubmit($(this), "vaccines");
  });

  // Category forms
  $(document).on("submit", "#createCategoryForm", function (e) {
    e.preventDefault();
    handleModalFormSubmit($(this), "categories");
  });

  $(document).on("submit", "#editCategoryForm", function (e) {
    e.preventDefault();
    handleModalFormSubmit($(this), "categories");
  });

  // Site forms
  $(document).on("submit", "#createSiteForm", function (e) {
    e.preventDefault();
    handleModalFormSubmit($(this), "sites");
  });

  $(document).on("submit", "#editSiteForm", function (e) {
    e.preventDefault();
    handleModalFormSubmit($(this), "sites");
  });

  // Appointment forms
  $(document).on("submit", "#createAppointmentForm", function (e) {
    e.preventDefault();
    handleModalFormSubmit($(this), "appointments");
  });

  $(document).on("submit", "#editAppointmentForm", function (e) {
    e.preventDefault();
    handleModalFormSubmit($(this), "appointments");
  });


  // =============================================================================
  // INVENTORY EVENT HANDLERS
  // =============================================================================

  // Tải bảng tồn kho khi admin chọn một điểm tiêm
  $(document).on('change', '#siteSelector', function() {
      const siteId = $(this).val();
      const container = $('#inventory-table-container');

      if (!siteId) {
          container.html('<div class="alert alert-info">Vui lòng chọn một điểm tiêm chủng để hiển thị dữ liệu tồn kho.</div>');
          return;
      }

      container.html('<div class="text-center p-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Đang tải...</span></div></div>');

      $.get(`/Admin/GetInventoryForSite?siteId=${siteId}`)
          .done(function(data) {
              container.html(data);
          })
          .fail(function() {
              container.html('<div class="alert alert-danger">Lỗi khi tải dữ liệu tồn kho. Vui lòng thử lại.</div>');
          });
  });

  // Lưu số lượng tồn kho khi admin nhấn nút "Lưu"
  $(document).on('click', '.save-inventory-btn', function() {
      const btn = $(this);
      const siteId = btn.data('site-id');
      const vaccineId = btn.data('vaccine-id');
      const row = btn.closest('tr');
      const input = row.find(`.quantity-input[data-vaccine-id="${vaccineId}"]`);
      const quantity = input.val();
      const token = $('input[name="__RequestVerificationToken"]').val(); // Lấy token từ một nơi an toàn hơn nếu có thể

      if (quantity < 0) {
          showNotification("Số lượng không được là số âm.", "error");
          return;
      }

      btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i>');

      $.ajax({
          url: '/Admin/UpdateInventory',
          type: 'POST',
          data: {
              __RequestVerificationToken: token,
              siteId: siteId,
              vaccineId: vaccineId,
              quantity: quantity
          },
          success: function(response) {
              if (response.success) {
                  showNotification(response.message, "success");
              } else {
                  showNotification(response.message, "error");
              }
          },
          error: function() {
              showNotification("Đã xảy ra lỗi kết nối.", "error");
          },
          complete: function() {
              btn.prop('disabled', false).html('<i class="fas fa-save me-1"></i> Lưu');
          }
      });
  });

});

// =============================================================================
// SEARCH & FILTER HANDLERS (via delegation)
// =============================================================================

  // User search
  $(document).on("keyup", "#userSearch", function () {
    const value = $(this).val().toLowerCase();
    $("#userTableBody tr").filter(function () {
      $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
    });
  });

  // Vaccine search and filters
  $(document).on("keyup", "#vaccineSearchInput", function () {
    filterVaccines();
  });

  $(document).on("change", "#categoryFilter", function () {
    filterVaccines();
  });

  $(document).on("change", "#statusFilter", function () {
    filterVaccines();
  });

  // Appointment search
  $(document).on("keyup", "#appointmentSearch", function () {
    const value = $(this).val().toLowerCase();
    $("#appointmentTableBody tr").filter(function () {
      $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
    });
  });

  // Site search and filters
  $(document).on("keyup", "#siteSearch", function () {
    filterSites();
  });

  $(document).on("change", "#provinceFilter", function () {
    filterSites();
  });
});

// =============================================================================
// MAIN CONTENT LOADING
// =============================================================================

function loadContent(target, pushState = true) {
  const contentContainer = $("#main-content");
  const pageTitle = $("#page-title");

  showLoading();

  const titles = {
    dashboard: "Dashboard",
    users: "Quản Lý Người Dùng",
    appointments: "Quản Lý Lịch Hẹn",
    vaccines: "Quản Lý Vắc-xin",
    categories: "Danh Mục Vắc-xin",
    sites: "Điểm Tiêm Chủng",
    reports: "Thống kê & Báo cáo"
  };

  pageTitle.text(titles[target] || "Quản Trị");

  if (pushState) {
    const newUrl = `/Admin?section=${target}`;
    history.pushState({ target: target }, titles[target], newUrl);
  }

  const endpoints = {
    dashboard: "/Admin/GetDashboardData",
    users: "/Admin/GetUsersData",
    appointments: "/Admin/GetAppointmentsData",
    vaccines: "/Admin/GetVaccinesData",
    categories: "/Admin/GetCategoriesData",
    sites: "/Admin/GetSitesData",
    inventory: "/Admin/GetInventoryManagementView",
    reports: "/Report/Index"
  };

  $.ajax({
    url: endpoints[target],
    type: "GET",
    success: function (data) {
      contentContainer.html(data);
      contentContainer.addClass("content-fade-in");
      hideLoading();
      if (target === 'inventory' && $('#siteSelector').length > 0) {
        $('#siteSelector').select2({
          theme: 'bootstrap-5',
          placeholder: $(this).data('placeholder'),
        });
      }
    },
    error: function (xhr, status, error) {
      const errorHtml = `
                <div class="alert alert-danger">
                    <h5><i class="fas fa-exclamation-triangle me-2"></i>Lỗi tải dữ liệu</h5>
                    <p>Không thể tải nội dung. Vui lòng thử lại.</p>
                    <button class="btn btn-outline-danger" onclick="loadContent('${target}')">
                        <i class="fas fa-redo me-2"></i> Thử lại
                    </button>
                </div>
            `;
      contentContainer.html(errorHtml);
      hideLoading();
    },
  });
}

function updateActiveNavigation(target) {
  $(".sidebar-nav .nav-link").removeClass("active");
  $(`.sidebar-nav .nav-link[data-target="${target}"]`).addClass("active");
}

// =============================================================================
// SEARCH & FILTER FUNCTIONS
// =============================================================================

function filterVaccines() {
  const searchValue = $("#vaccineSearchInput").val().toLowerCase();
  const categoryValue = $("#categoryFilter").val();
  const statusValue = $("#statusFilter").val();

  $("#vaccine-list .vaccine-item").filter(function () {
    const tradeName = $(this).find(".trade-name").text().toLowerCase();
    const genericName = $(this).find(".generic-name").text().toLowerCase();
    const category = $(this).data("category").toString();
    const status = $(this).data("status");

    const textMatch =
      tradeName.indexOf(searchValue) > -1 ||
      genericName.indexOf(searchValue) > -1;
    const categoryMatch = !categoryValue || category === categoryValue;
    const statusMatch = !statusValue || status === statusValue;

    $(this).toggle(textMatch && categoryMatch && statusMatch);
  });
}

function filterSites() {
  const searchValue = $("#siteSearch").val().toLowerCase();
  const provinceValue = $("#provinceFilter").val();

  $("#siteTableBody tr").each(function () {
    const row = $(this);
    const rowText = row.text().toLowerCase();
    const rowProvince = row.data("province");

    // Điều kiện 1: Phải khớp với văn bản tìm kiếm (nếu có)
    const textMatch = searchValue === '' || rowText.indexOf(searchValue) > -1;
    
    // Điều kiện 2: Phải khớp với tỉnh được chọn (hoặc không chọn tỉnh nào)
    const provinceMatch = provinceValue === '' || rowProvince === provinceValue;

    // Chỉ hiển thị hàng nếu cả hai điều kiện đều đúng
    if (textMatch && provinceMatch) {
      row.show();
    } else {
      row.hide();
    }
  });
}

function filterAppointments(status) {
  if (status === "all") {
    $("#appointmentTableBody tr").show();
  } else {
    $("#appointmentTableBody tr").each(function () {
      const rowStatus = $(this).data("status");
      $(this).toggle(rowStatus === status);
    });
  }
}

// =============================================================================
// USER MANAGEMENT
// =============================================================================

function openCreateUserModal() {
  loadModalContent("Thêm Người Dùng Mới", "/Admin/GetCreateUserForm");
}

function viewUserDetails(userId) {
  loadModalContent("Chi Tiết Người Dùng", `/Admin/GetUserDetails/${userId}`);
}

function editUser(userId) {
  loadModalContent("Chỉnh Sửa Người Dùng", `/Admin/GetEditUserForm/${userId}`);
}

function editUserFromDetails(userId) {
  $("#actionModal").modal("hide");
  setTimeout(() => editUser(userId), 300);
}

function confirmDeleteUser(userId, userName) {
  if (
    confirm(
      `Bạn có chắc chắn muốn xóa người dùng "${userName}"?\n\nLưu ý: Không thể xóa nếu người dùng có lịch hẹn.`
    )
  ) {
    deleteUser(userId);
  }
}

function deleteUser(userId) {
  const token = $('input[name="__RequestVerificationToken"]').val();

  $.ajax({
    url: "/Admin/DeleteUserApi",
    type: "POST",
    data: {
      id: userId,
      __RequestVerificationToken: token,
    },
    success: function (response) {
      if (response.success) {
        showNotification(response.message, "success");
        loadContent("users");
      } else {
        showNotification(response.message, "error");
      }
    },
    error: function () {
      showNotification("Có lỗi xảy ra khi xóa người dùng.", "error");
    },
  });
}

// =============================================================================
// VACCINE MANAGEMENT
// =============================================================================

function openCreateVaccineModal() {
  loadModalContent("Thêm Vắc-xin Mới", "/Admin/GetCreateVaccineForm");
}

function viewVaccineDetails(vaccineId) {
  loadModalContent("Chi Tiết Vắc-xin", `/Admin/GetVaccineDetails/${vaccineId}`);
}

function editVaccine(vaccineId) {
  loadModalContent(
    "Chỉnh Sửa Vắc-xin",
    `/Admin/GetEditVaccineForm/${vaccineId}`
  );
}

function editVaccineFromDetails(vaccineId) {
  $("#actionModal").modal("hide");
  setTimeout(() => editVaccine(vaccineId), 300);
}

function confirmDeleteVaccine(vaccineId, vaccineName) {
  if (
    confirm(
      `Bạn có chắc chắn muốn xóa vắc-xin "${vaccineName}"?\n\nLưu ý: Không thể xóa nếu có lịch hẹn liên quan.`
    )
  ) {
    deleteVaccine(vaccineId);
  }
}

function deleteVaccine(vaccineId) {
  const token = $('input[name="__RequestVerificationToken"]').val();

  $.ajax({
    url: "/Admin/DeleteVaccineApi",
    type: "POST",
    data: {
      id: vaccineId,
      __RequestVerificationToken: token,
    },
    success: function (response) {
      if (response.success) {
        showNotification(response.message, "success");
        loadContent("vaccines");
      } else {
        showNotification(response.message, "error");
      }
    },
    error: function () {
      showNotification("Có lỗi xảy ra khi xóa vắc-xin.", "error");
    },
  });
}

// =============================================================================
// CATEGORY MANAGEMENT
// =============================================================================

function openCreateCategoryModal() {
  loadModalContent("Thêm Danh Mục Mới", "/Admin/GetCreateCategoryForm");
}

function editCategory(categoryId, categoryName) {
  loadModalContent(
    `Chỉnh Sửa Danh Mục: ${categoryName}`,
    `/Admin/GetEditCategoryForm/${categoryId}`
  );
}

function confirmDeleteCategory(categoryId, categoryName) {
  if (confirm(`Bạn có chắc chắn muốn xóa danh mục "${categoryName}"?`)) {
    deleteCategory(categoryId);
  }
}

function deleteCategory(categoryId) {
  const token = $('input[name="__RequestVerificationToken"]').val();

  $.ajax({
    url: "/Admin/DeleteCategoryApi",
    type: "POST",
    data: {
      id: categoryId,
      __RequestVerificationToken: token,
    },
    success: function (response) {
      if (response.success) {
        showNotification(response.message, "success");
        loadContent("categories");
      } else {
        showNotification(response.message, "error");
      }
    },
    error: function () {
      showNotification("Có lỗi xảy ra khi xóa danh mục.", "error");
    },
  });
}

// =============================================================================
// SITE MANAGEMENT
// =============================================================================

function openCreateSiteModal() {
  loadModalContent("Thêm Điểm Tiêm Mới", "/Admin/GetCreateSiteForm");
}

function viewSiteDetails(siteId) {
  loadModalContent("Chi Tiết Điểm Tiêm", `/Admin/GetSiteDetails/${siteId}`);
}

function editSite(siteId) {
  loadModalContent("Chỉnh Sửa Điểm Tiêm", `/Admin/GetEditSiteForm/${siteId}`);
}

function editSiteFromDetails(siteId) {
  $("#actionModal").modal("hide");
  setTimeout(() => editSite(siteId), 300);
}

function confirmDeleteSite(siteId, siteName) {
  if (
    confirm(
      `Bạn có chắc chắn muốn xóa điểm tiêm "${siteName}"?\n\nLưu ý: Không thể xóa nếu có lịch hẹn liên quan.`
    )
  ) {
    deleteSite(siteId);
  }
}

function deleteSite(siteId) {
  const token = $('input[name="__RequestVerificationToken"]').val();

  $.ajax({
    url: "/Admin/DeleteSiteApi",
    type: "POST",
    data: {
      id: siteId,
      __RequestVerificationToken: token,
    },
    success: function (response) {
      if (response.success) {
        showNotification(response.message, "success");
        loadContent("sites");
      } else {
        showNotification(response.message, "error");
      }
    },
    error: function () {
      showNotification("Có lỗi xảy ra khi xóa điểm tiêm.", "error");
    },
  });
}

// =============================================================================
// APPOINTMENT MANAGEMENT
// =============================================================================

function openCreateAppointmentModal() {
  loadModalContent("Thêm Lịch Hẹn Mới", "/Admin/GetCreateAppointmentForm");
}

function viewAppointmentDetails(appointmentId) {
  loadModalContent(
    "Chi Tiết Lịch Hẹn",
    `/Admin/GetAppointmentDetails/${appointmentId}`
  );
}

function editAppointment(appointmentId) {
  loadModalContent(
    "Chỉnh Sửa Lịch Hẹn",
    `/Admin/GetEditAppointmentForm/${appointmentId}`
  );
}

function confirmDeleteAppointment(appointmentId) {
  if (confirm("Bạn có chắc chắn muốn xóa lịch hẹn này?")) {
    deleteAppointment(appointmentId);
  }
}

function deleteAppointment(appointmentId) {
  const token = $('input[name="__RequestVerificationToken"]').val();

  $.ajax({
    url: "/Admin/DeleteAppointmentApi",
    type: "POST",
    data: {
      id: appointmentId,
      __RequestVerificationToken: token,
    },
    success: function (response) {
      if (response.success) {
        showNotification(response.message, "success");
        loadContent("appointments");
      } else {
        showNotification(response.message, "error");
      }
    },
    error: function () {
      showNotification("Có lỗi xảy ra khi xóa lịch hẹn.", "error");
    },
  });
}

function updateAppointmentStatus(appointmentId, status) {
  const token = $('input[name="__RequestVerificationToken"]').val();

  $.ajax({
    url: "/Admin/UpdateAppointmentStatusApi",
    type: "POST",
    data: {
      id: appointmentId,
      status: status,
      __RequestVerificationToken: token,
    },
    success: function (response) {
      if (response.success) {
        showNotification(response.message, "success");
        loadContent("appointments");
      } else {
        showNotification(response.message, "error");
      }
    },
    error: function () {
      showNotification("Có lỗi xảy ra khi cập nhật trạng thái.", "error");
    },
  });
}

// =============================================================================
// ADDRESS FORM INITIALIZER (UPGRADED FOR CREATE & EDIT)
// =============================================================================
function initializeAddressForm() {
    const host = "https://provinces.open-api.vn/api/v1/";
    const provinceSelect = $("#province");
    const districtSelect = $("#district");
    const wardSelect = $("#ward");

    // Lấy giá trị đã lưu từ các trường ẩn (chỉ có ở form edit)
    const initialProvince = $("#provinceName").val();
    const initialDistrict = $("#districtName").val();
    const initialWard = $("#wardName").val();

    // Hàm gọi API và trả về promise
    const callAPI = (api) => {
        return $.getJSON(api);
    };

    // Hàm điền dữ liệu vào ô select và chọn giá trị (nếu có)
    const populateSelect = (targetSelect, data, selectedValue) => {
        targetSelect.empty().append('<option value="">Chọn</option>');
        $.each(data, function() {
            const option = $('<option>', { value: this.code, text: this.name });
            if (this.name === selectedValue) {
                option.prop('selected', true);
            }
            targetSelect.append(option);
        });
        targetSelect.prop('disabled', false);
    };

    // --- Logic chính ---
    
    // 1. Tải Tỉnh/Thành phố
    provinceSelect.prop('disabled', true);
    callAPI(host + "p").done(function(provinces) {
        populateSelect(provinceSelect, provinces, initialProvince);
        
        // Nếu có giá trị tỉnh ban đầu (chế độ edit), tự động tải các huyện
        if (initialProvince && provinceSelect.val()) {
            districtSelect.prop('disabled', true);
            callAPI(host + 'p/' + provinceSelect.val() + "?depth=2").done(function(provinceData) {
                populateSelect(districtSelect, provinceData.districts, initialDistrict);
                
                // Nếu có giá trị huyện ban đầu, tự động tải các xã
                if (initialDistrict && districtSelect.val()) {
                    wardSelect.prop('disabled', true);
                    callAPI(host + 'd/' + districtSelect.val() + "?depth=2").done(function(districtData) {
                        populateSelect(wardSelect, districtData.wards, initialWard);
                    });
                }
            });
        }
    });

    // 2. Gán sự kiện 'change' cho người dùng tương tác thủ công
    provinceSelect.change(function() {
        const provinceCode = $(this).val();
        $("#provinceName").val($(this).find("option:selected").text());
        districtSelect.empty().append('<option value="">Chọn</option>').prop('disabled', !provinceCode);
        wardSelect.empty().append('<option value="">Chọn</option>').prop('disabled', true);

        if (provinceCode) {
            callAPI(host + 'p/' + provinceCode + "?depth=2").done(function(data) {
                populateSelect(districtSelect, data.districts);
            });
        }
    });

    districtSelect.change(function() {
        const districtCode = $(this).val();
        $("#districtName").val($(this).find("option:selected").text());
        wardSelect.empty().append('<option value="">Chọn</option>').prop('disabled', !districtCode);

        if (districtCode) {
            callAPI(host + 'd/' + districtCode + "?depth=2").done(function(data) {
                populateSelect(wardSelect, data.wards);
            });
        }
    });

    wardSelect.change(function() {
        $("#wardName").val($(this).find("option:selected").text());
    });
}


// =============================================================================
// MODAL MANAGEMENT
// =============================================================================

function loadModalContent(title, url, size = "modal-lg") {
  const modalId = "actionModal";
  const modalHtml = `
        <div class="modal fade" id="${modalId}" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog ${size}">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">${title}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div class="text-center p-4">
                            <div class="spinner-border text-primary" role="status">
                                <span class="visually-hidden">Đang tải...</span>
                            </div>
                            <p class="mt-2">Đang tải nội dung...</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;

  $(`#${modalId}`).remove();
  $("body").append(modalHtml);

  const modal = new bootstrap.Modal(document.getElementById(modalId));
  modal.show();

  $.get(url)
    .done(function (data) {
      const modalBody = $(`#${modalId} .modal-body`);
      modalBody.html(data);

      // KIỂM TRA VÀ GỌI HÀM KHỞI TẠO TẠI ĐÂY
      if (
        modalBody.find("#createSiteForm").length > 0 ||
        modalBody.find("#editSiteForm").length > 0
      ) {
        initializeAddressForm();
      }
    })
    .fail(function () {
      $(`#${modalId} .modal-body`).html(`
                <div class="alert alert-danger">
                    <h6><i class="fas fa-exclamation-triangle me-2"></i>Lỗi tải dữ liệu</h6>
                    <p>Không thể tải nội dung. Vui lòng thử lại.</p>
                    <button class="btn btn-outline-danger btn-sm" onclick="loadModalContent('${title}', '${url}', '${size}')">
                        <i class="fas fa-redo me-1"></i> Thử lại
                    </button>
                </div>
            `);
    });

  $(`#${modalId}`).on("hidden.bs.modal", function () {
    $(this).remove();
  });
}

// =============================================================================
// FORM HANDLING
// =============================================================================

function handleModalFormSubmit(form, refreshTarget) {
  const url = form.data("url");
  const submitBtn = form.find('button[type="submit"]');
  const originalBtnHtml = submitBtn.html();
  const validationSummary = form.find("#validation-summary");

  // Clear previous validation
  form.find(".is-invalid").removeClass("is-invalid");
  form.find(".invalid-feedback").hide();
  validationSummary.hide();

  // Add loading state
  submitBtn
    .prop("disabled", true)
    .html('<i class="fas fa-spinner fa-spin me-2"></i>Đang xử lý...');

  $.ajax({
    url: url,
    type: "POST",
    data: form.serialize(),
    success: function (response) {
      if (response.success) {
        // **🔥 SPECIAL HANDLING FOR ROLE CHANGES**
        if (response.roleChanged) {
          showNotification(response.message, "warning", 8000);

          // Show admin info modal about role change
          setTimeout(() => {
            showAdminRoleChangeInfo();
          }, 1000);
        } else {
          showNotification(response.message, "success");
        }

        $("#actionModal").modal("hide");
        if (refreshTarget) {
          loadContent(refreshTarget);
        }
      } else if (response.message) {
        showNotification(response.message, "error");
      }
    },
    error: function (xhr) {
      if (xhr.status === 400 && xhr.responseJSON && xhr.responseJSON.errors) {
        displayValidationErrors(form, xhr.responseJSON.errors);
      } else if (xhr.responseText) {
        const modalBody = form.closest(".modal-body");
        modalBody.html(xhr.responseText);
      } else {
        showNotification("Có lỗi xảy ra. Vui lòng thử lại.", "error");
      }
    },
    complete: function () {
      submitBtn.prop("disabled", false).html(originalBtnHtml);
    },
  });
}

// **🔥 FUNCTION THÔNG BÁO CHO ADMIN KHI THAY ĐỔI ROLE**
function showAdminRoleChangeInfo() {
  const modalHtml = `
        <div class="modal fade" id="adminRoleChangeModal" tabindex="-1">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header bg-info text-white">
                        <h5 class="modal-title">
                            <i class="fas fa-info-circle me-2"></i>
                            Thay Đổi Quyền Thành Công
                        </h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div class="alert alert-success">
                            <h6><i class="fas fa-check-circle me-2"></i>Thay đổi đã được áp dụng:</h6>
                            <ul class="mb-0">
                                <li>Quyền của người dùng đã được cập nhật trong hệ thống</li>
                                <li>Người dùng đã được <strong>thông báo real-time</strong></li>
                                <li>Họ sẽ <strong>tự động bị đăng xuất</strong> để cập nhật quyền</li>
                                <li>Thay đổi có hiệu lực ngay lập tức</li>
                            </ul>
                        </div>
                        <div class="text-center">
                            <i class="fas fa-user-shield fa-3x text-success mb-3"></i>
                            <p class="text-muted">Hệ thống đã gửi thông báo cho người dùng bị ảnh hưởng</p>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-bs-dismiss="modal">
                            <i class="fas fa-check me-2"></i>Đã hiểu
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;

  $("#adminRoleChangeModal").remove();
  $("body").append(modalHtml);

  const modal = new bootstrap.Modal(
    document.getElementById("adminRoleChangeModal")
  );
  modal.show();

  // Auto close after 8 seconds
  setTimeout(() => {
    modal.hide();
  }, 8000);
}

function displayValidationErrors(form, errors) {
  const validationSummary = form.find("#validation-summary");
  let errorMessages = [];

  for (const field in errors) {
    const fieldName = field.replace("model.", "").replace("Model.", "");
    const input = form.find(`[name="${fieldName}"]`);
    const feedback = input.siblings(".invalid-feedback");

    input.addClass("is-invalid");
    if (feedback.length) {
      feedback.text(errors[field][0]).show();
    }
    errorMessages.push(errors[field][0]);
  }

  if (errorMessages.length > 0) {
    validationSummary
      .html(
        `
            <h6><i class="fas fa-exclamation-triangle me-2"></i>Vui lòng sửa các lỗi sau:</h6>
            <ul class="mb-0">
                ${errorMessages.map((msg) => `<li>${msg}</li>`).join("")}
            </ul>
        `
      )
      .show();
  }
}

// =============================================================================
// APPOINTMENT QUICK ACTIONS (Legacy compatibility)
// =============================================================================

function confirmAppointment(id) {
  updateAppointmentStatus(id, "Confirmed");
}

function cancelAppointment(id) {
  if (confirm("Bạn có chắc chắn muốn hủy lịch hẹn này?")) {
    updateAppointmentStatus(id, "Cancelled");
  }
}

function completeAppointment(id) {
  updateAppointmentStatus(id, "Completed");
}

// =============================================================================
// UTILITY FUNCTIONS
// =============================================================================

function showLoading() {
  $("#main-content").html(`
        <div class="loading-spinner text-center p-5">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Đang tải...</span>
            </div>
            <p class="mt-2">Đang tải dữ liệu...</p>
        </div>
    `);
}

function hideLoading() {
  // Loading is hidden when content is loaded
}

function showNotification(message, type = "info", duration = 5000) {
  const alertClass =
    type === "success"
      ? "alert-success"
      : type === "error"
      ? "alert-danger"
      : type === "warning"
      ? "alert-warning"
      : "alert-info";

  const icon =
    type === "success"
      ? "fas fa-check-circle"
      : type === "error"
      ? "fas fa-exclamation-circle"
      : type === "warning"
      ? "fas fa-exclamation-triangle"
      : "fas fa-info-circle";

  const notification = $(`
        <div class="alert ${alertClass} alert-dismissible fade show position-fixed" 
             style="top: 20px; right: 20px; z-index: 9999; min-width: 350px;">
            <i class="${icon} me-2"></i>${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `);

  $("body").append(notification);

  setTimeout(() => {
    notification.fadeOut(() => notification.remove());
  }, duration);
}

// =============================================================================
// ERROR HANDLING & GLOBAL EVENTS
// =============================================================================

document.addEventListener("visibilitychange", function () {
  if (!document.hidden) {
    const activeLink = $(".sidebar-nav .nav-link.active");
    const target = activeLink.data("target");
    if (target && target !== "dashboard") {
      // Optional: refresh data when user comes back to tab
    }
  }
});

window.addEventListener("error", function (e) {
  console.error("Global error:", e.error);
  showNotification("Đã xảy ra lỗi không mong muốn.", "error");
});

$(document).ajaxError(function (event, xhr, settings) {
  if (xhr.status === 401) {
    showNotification(
      "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
      "error"
    );
    setTimeout(() => {
      window.location.href = "/Account/Login";
    }, 2000);
  } else if (xhr.status === 403) {
    showNotification("Bạn không có quyền thực hiện thao tác này.", "error");
  }
});

// Improved admin-spa.js additions for vaccine edit form handling

// Add this to your existing admin-spa.js file

// Enhanced modal handling for vaccine forms
$(document).on("shown.bs.modal", "#actionModal", function () {
  // Check if this is edit vaccine form and initialize it
  if ($(this).find("#editVaccineForm").length > 0) {
    initializeEditVaccineForm();
  }
});

// Enhanced form submission handler specifically for vaccine forms
$(document).on("submit", "#editVaccineForm", function (e) {
  e.preventDefault();

  // Log form data for debugging
  console.log("Edit vaccine form submitted");
  console.log("Form data:", $(this).serialize());

  // Ensure all dose indices are properly set before submission
  reindexDosesEditBeforeSubmit();

  handleModalFormSubmit($(this), "vaccines");
});

// Function to ensure proper dose indexing before form submission
function reindexDosesEditBeforeSubmit() {
  const container = document.getElementById("doses-container-edit");
  if (!container) return;

  const doseItems = container.querySelectorAll(".dose-item");
  let index = 0;

  doseItems.forEach((item) => {
    // Update the index hidden field
    const indexInput = item.querySelector('input[name$=".Index"]');
    if (indexInput) {
      indexInput.value = index;
    }

    // Update all dose-related field names
    item.querySelectorAll("input, select, textarea").forEach((input) => {
      const name = input.name;
      if (name && name.includes("Doses[")) {
        input.name = name.replace(/Doses\[\d+\]/, `Doses[${index}]`);
      }
    });

    index++;
  });

  console.log(`Reindexed ${index} doses for edit form`);
}

// Enhanced handleModalFormSubmit with better error handling for vaccines
function handleVaccineFormSubmit(form, refreshTarget) {
  const url = form.data("url");
  const submitBtn = form.find('button[type="submit"]');
  const originalBtnHtml = submitBtn.html();
  const validationSummary = form.find("#validation-summary");

  // Clear previous validation
  form.find(".is-invalid").removeClass("is-invalid");
  form.find(".invalid-feedback").hide();
  validationSummary.hide().empty();

  // Add loading state
  submitBtn
    .prop("disabled", true)
    .html('<i class="fas fa-spinner fa-spin me-2"></i>Đang xử lý...');

  // Log the data being sent
  const formData = form.serialize();
  console.log("Submitting vaccine form:", formData);

  $.ajax({
    url: url,
    type: "POST",
    data: formData,
    success: function (response) {
      console.log("Vaccine form response:", response);

      if (response.success) {
        showNotification(response.message, "success");
        $("#actionModal").modal("hide");
        if (refreshTarget) {
          loadContent(refreshTarget);
        }
      } else if (response.message) {
        showNotification(response.message, "error");
      } else {
        showNotification("Có lỗi xảy ra không xác định", "error");
      }
    },
    error: function (xhr, status, error) {
      console.error("Vaccine form submission error:", {
        status: xhr.status,
        responseText: xhr.responseText,
        error: error,
      });

      if (xhr.status === 400 && xhr.responseJSON && xhr.responseJSON.errors) {
        displayValidationErrors(form, xhr.responseJSON.errors);
      } else if (xhr.responseText) {
        // If server returns HTML (partial view), replace modal content
        const modalBody = form.closest(".modal-body");
        modalBody.html(xhr.responseText);
      } else {
        showNotification(`Có lỗi xảy ra: ${error}`, "error");
      }
    },
    complete: function () {
      // Restore button state
      submitBtn.prop("disabled", false).html(originalBtnHtml);
    },
  });
}

// Enhanced validation error display
function displayValidationErrors(form, errors) {
  const validationSummary = form.find("#validation-summary");
  let errorHtml = "<ul>";

  Object.keys(errors).forEach((key) => {
    errors[key].forEach((error) => {
      errorHtml += `<li>${error}</li>`;

      // Also highlight the specific field
      const field = form.find(`[name="${key}"]`);
      if (field.length > 0) {
        field.addClass("is-invalid");
        field.siblings(".invalid-feedback").text(error).show();
      }
    });
  });

  errorHtml += "</ul>";
  validationSummary.html(errorHtml).show();
}

// Debug function to check form state
function debugVaccineForm(formId) {
  const form = $(formId);
  if (form.length === 0) {
    console.log(`Form ${formId} not found`);
    return;
  }

  console.log("=== Vaccine Form Debug ===");
  console.log("Form URL:", form.data("url"));
  console.log("Form method:", form.attr("method") || "POST");
  console.log("Form data:", form.serialize());

  const doses = form.find('[name*="Doses["]');
  console.log("Dose fields found:", doses.length);

  doses.each(function (index, element) {
    console.log(`Dose field ${index}:`, element.name, "=", element.value);
  });

  console.log("=== End Debug ===");
}

// Call this function in browser console to debug: debugVaccineForm('#editVaccineForm')