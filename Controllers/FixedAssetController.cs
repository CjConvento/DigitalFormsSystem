using DigitalFormsSystem.Core.Dtos;
using DigitalFormsSystem.Core.Interfaces;
using DigitalFormsSystem.Core.Models;
using DigitalFormsSystem.Core.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalFormsSystem.Controllers
{
    public class FixedAssetController : Controller
    {
        private readonly IFixedAssetRequestService _service;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService; 
        private readonly IAuditService _auditService;

        // 
        public FixedAssetController(
            IFixedAssetRequestService service,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            IAuditService auditService)  
        {
            _service = service;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _auditService = auditService;  
        }

        // ============ INDEX ============
        public async Task<IActionResult> Index()
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var requests = await _service.GetUserRequestsAsync(_currentUserService.EmployeeId!.Value);
            return View(requests);
        }

        // ============ DETAILS ============
        public async Task<IActionResult> Details(int id)
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var request = await _service.GetRequestWithDetailsAsync(id);
            if (request == null) return NotFound();
            return View(request);
        }

        // ============ CREATE (GET) ============
        public IActionResult Create()
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");
            return View();
        }

        // ============ CREATE (POST) ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FixedAssetRequest formModel)
        {
            if (!_currentUserService.IsAuthenticated) return RedirectToAction("Login", "Account");

            var empId = _currentUserService.EmployeeId!.Value;

            // 1. Parse Existing Units from Form into DTOs
            List<ExistingUnitDetailDto> existingUnitsDtos = new();
            int idx = 0;
            while (Request.Form.ContainsKey($"ExistingUnits[{idx}].Description"))
            {
                var description = Request.Form[$"ExistingUnits[{idx}].Description"].ToString();
                if (!string.IsNullOrWhiteSpace(description))
                {
                    var itemNoStr = Request.Form[$"ExistingUnits[{idx}].ItemNo"].ToString();
                    int itemNo = string.IsNullOrEmpty(itemNoStr) ? idx + 1 : int.Parse(itemNoStr);
                    
                    existingUnitsDtos.Add(new ExistingUnitDetailDto
                    {
                        ItemNo = itemNo,
                        Description = description,
                        Location = Request.Form[$"ExistingUnits[{idx}].Location"].ToString(),
                        UserName = Request.Form[$"ExistingUnits[{idx}].UserName"].ToString(),
                        Remarks = Request.Form[$"ExistingUnits[{idx}].Remarks"].ToString()
                    });
                }
                idx++;
            }

            // 2. Map Form Model to DTO (Delegated to Mapper)
            var requestDto = formModel.ToCreateDto(existingUnitsDtos);

            // Handle Date Parsing quirks
            var targetDateStr = Request.Form["TargetDateNeeded"].ToString();

            if (DateOnly.TryParseExact(targetDateStr, "yyyy-MM-dd", out var targetDate))
            {
                requestDto.TargetDateNeeded = targetDate;
            }
            else
            {
                ModelState.AddModelError("TargetDateNeeded", "Invalid date format. Please use YYYY-MM-DD.");
            }

            // Custom validations
            if (requestDto.Quantity < 1)
                ModelState.AddModelError("Quantity", "Quantity must be at least 1.");

            if (requestDto.RequestType == "Additional" && !existingUnitsDtos.Any(u => !string.IsNullOrWhiteSpace(u.Description)))
                ModelState.AddModelError("ExistingUnits", "At least one existing unit with a Description is required when Request Type is 'Additional'.");

            if (!ModelState.IsValid)
            {
                if (requestDto.RequestType == "Additional" && existingUnitsDtos.Any())
                    ViewBag.ExistingUnits = existingUnitsDtos;
                return View(formModel);
            }

            try
            {
                // 3. Delegate to Service (Pass DTO and Employee ID)
                var created = await _service.CreateRequestAsync(requestDto, empId);

                await _auditService.LogAsync("Create", "FixedAssetRequest", created.Id, $"Created request {created.ControlNo}");
                
                TempData["SuccessMessage"] = "Request updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(formModel);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(formModel);
            }
        }

        // ============ EDIT (GET) ============
        public async Task<IActionResult> Edit(int id)
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var request = await _service.GetRequestWithDetailsAsync(id);
            if (request == null) return NotFound();

            if (request.RequestStatus != "Draft")
            {
                TempData["ErrorMessage"] = "Only requests with 'Draft' status can be edited.";
                return RedirectToAction(nameof(Index));
            }

            return View(request);
        }

        // ============ EDIT (POST) ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FixedAssetRequest formModel)
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            if (id != formModel.Id) return NotFound();

            var empId = _currentUserService.EmployeeId!.Value;
            
            // 1. Parse Existing Units from Form into DTOs
            List<ExistingUnitDetailDto> existingUnitsDtos = new();
            if (formModel.RequestType == "Additional")
            {
                int idx = 0;
                while (Request.Form.ContainsKey($"ExistingUnits[{idx}].Description"))
                {
                    var description = Request.Form[$"ExistingUnits[{idx}].Description"].ToString();
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        var itemNoStr = Request.Form[$"ExistingUnits[{idx}].ItemNo"].ToString();
                        int itemNo = string.IsNullOrEmpty(itemNoStr) ? idx + 1 : int.Parse(itemNoStr);
                        var location = Request.Form[$"ExistingUnits[{idx}].Location"].ToString();
                        var userName = Request.Form[$"ExistingUnits[{idx}].UserName"].ToString();
                        var remarks = Request.Form[$"ExistingUnits[{idx}].Remarks"].ToString();

                        existingUnitsDtos.Add(new ExistingUnitDetailDto
                        {
                            ItemNo = itemNo,
                            Description = description,
                            Location = location ?? string.Empty,
                            UserName = userName ?? string.Empty,
                            Remarks = remarks ?? string.Empty
                        });
                    }
                    idx++;
                }

                if (!existingUnitsDtos.Any())
                    ModelState.AddModelError("ExistingUnits", "At least one existing unit with a Description is required when Request Type is 'Additional'.");
            }

            // 2. Map Form Model to DTO (Delegated to Mapper)
            var requestDto = formModel.ToUpdateDto(existingUnitsDtos);

            // Custom validations
            if (requestDto.Quantity < 1)
                ModelState.AddModelError("Quantity", "Quantity must be at least 1.");

            if (!ModelState.IsValid)
                return View(formModel);

            try
            {
                // 3. Delegate to Service (Pass DTO and Employee ID)
                var success = await _service.UpdateRequestAsync(requestDto, empId);
                if (!success) return NotFound();

                await _auditService.LogAsync("Edit", "FixedAssetRequest", id, $"Updated request {formModel.ControlNo}");
                TempData["SuccessMessage"] = "Request updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(formModel);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred while updating.");
                return View(formModel);
            }
        }

        // ============ DELETE (GET) ============
        public async Task<IActionResult> Delete(int id)
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var request = await _service.GetRequestWithDetailsAsync(id);
            if (request == null) return NotFound();

            if (request.RequestStatus != "Draft")
            {
                TempData["ErrorMessage"] = "Only requests with 'Draft' status can be deleted.";
                return RedirectToAction(nameof(Index));
            }

            return View(request);
        }

        // ============ DELETE (POST) ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            try
            {
                var success = await _service.DeleteRequestAsync(id);
                if (!success) return NotFound();

                // After successful delete
                await _notificationService.NotifyStatusChangeAsync(
                    id,
                    "Deleted",
                    "Deleted",
                    _currentUserService.EmployeeId!.Value);
                // ==================================

                // ✅ LOG DELETE
                await _auditService.LogAsync(
                "Delete",
                "FixedAssetRequest",
                id,
                $"Deleted request ID: {id}");

                TempData["SuccessMessage"] = "Request deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the request. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ============ PRINT ============
        public async Task<IActionResult> Print(int id)
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var empId = _currentUserService.EmployeeId!.Value;

            // Log print activity
            await _service.LogPrintActivityAsync(id, empId);
            // ✅ LOG PRINT
            await _auditService.LogAsync(
                "Print",
                "FixedAssetRequest",
                id,
                $"Printed request ID: {id}");

            var request = await _service.GetRequestForPrintAsync(id);
            if (request == null) return NotFound();

            return View(request);
        }
    }
}