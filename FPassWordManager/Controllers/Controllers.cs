using FPassWordManager.DTOs;
using FPassWordManager.Models;
using FPassWordManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FPassWordManager.Controllers
{
    [Authorize]
    [ApiController]
    public abstract class AppControllerBase : ControllerBase
    {
        protected Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        protected static ApiResponse<T> Fail<T>(string msg) =>
            new(false, msg, default);
    }

    // ── Credentials ──────────────────────────────────────────────────
    [Route("api/[controller]")]
    public class CredentialsController : AppControllerBase
    {
        private readonly ICredentialService _svc;
        public CredentialsController(ICredentialService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetMine()
            => Ok(new ApiResponse<IEnumerable<CredentialDto>>(true, null,
                await _svc.GetMyCredentialsAsync(CurrentUserId)));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var r = await _svc.GetByIdAsync(id, CurrentUserId);
            return r == null
                ? NotFound(Fail<CredentialDto>("Not found."))
                : Ok(new ApiResponse<CredentialDto>(true, null, r));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCredentialDto dto)
        {
            var r = await _svc.CreateAsync(CurrentUserId, dto);
            return CreatedAtAction(nameof(GetById), new { id = r.CredentialId },
                new ApiResponse<CredentialDto>(true, "Created.", r));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCredentialDto dto)
        {
            var r = await _svc.UpdateAsync(id, CurrentUserId, dto);
            return r == null
                ? NotFound(Fail<CredentialDto>("Not found."))
                : Ok(new ApiResponse<CredentialDto>(true, "Updated.", r));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
            => await _svc.DeleteAsync(id, CurrentUserId)
                ? Ok(new ApiResponse<object>(true, "Deleted.", null))
                : NotFound(Fail<object>("Not found."));
    }

    // ── WebCredentials ───────────────────────────────────────────────
    [Route("api/[controller]")]
    public class WebCredentialsController : AppControllerBase
    {
        private readonly IWebCredentialService _svc;
        public WebCredentialsController(IWebCredentialService svc) => _svc = svc;

        [HttpGet("by-credential/{credentialId:guid}")]
        public async Task<IActionResult> GetByCredential(Guid credentialId)
            => Ok(new ApiResponse<IEnumerable<WebCredentialDto>>(true, null,
                await _svc.GetByCredentialAsync(credentialId, CurrentUserId)));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var r = await _svc.GetByIdAsync(id, CurrentUserId);
            return r == null
                ? NotFound(Fail<WebCredentialDto>("Not found."))
                : Ok(new ApiResponse<WebCredentialDto>(true, null, r));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWebCredentialDto dto)
        {
            try
            {
                var r = await _svc.CreateAsync(CurrentUserId, dto);
                return CreatedAtAction(nameof(GetById), new { id = r.WebCredentialId },
                    new ApiResponse<WebCredentialDto>(true, "Added.", r));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWebCredentialDto dto)
        {
            try
            {
                var r = await _svc.UpdateAsync(id, CurrentUserId, dto);
                return r == null
                    ? NotFound(Fail<WebCredentialDto>("Not found."))
                    : Ok(new ApiResponse<WebCredentialDto>(true, "Updated.", r));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                return await _svc.DeleteAsync(id, CurrentUserId)
                    ? Ok(new ApiResponse<object>(true, "Deleted.", null))
                    : NotFound(Fail<object>("Not found."));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpGet("{id:guid}/history")]
        public async Task<IActionResult> GetHistory(Guid id)
            => Ok(new ApiResponse<IEnumerable<WebCredentialHistoryDto>>(true, null,
                await _svc.GetHistoryAsync(id, CurrentUserId)));
    }

    // ── CreditDebitCards ─────────────────────────────────────────────
    [Route("api/[controller]")]
    public class CreditDebitCardsController : AppControllerBase
    {
        private readonly ICreditDebitCardService _svc;
        public CreditDebitCardsController(ICreditDebitCardService svc) => _svc = svc;

        [HttpGet("by-credential/{credentialId:guid}")]
        public async Task<IActionResult> GetByCredential(Guid credentialId)
            => Ok(new ApiResponse<IEnumerable<CreditDebitCardDto>>(true, null,
                await _svc.GetByCredentialAsync(credentialId, CurrentUserId)));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var r = await _svc.GetByIdAsync(id, CurrentUserId);
            return r == null
                ? NotFound(Fail<CreditDebitCardDto>("Not found."))
                : Ok(new ApiResponse<CreditDebitCardDto>(true, null, r));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCreditDebitCardDto dto)
        {
            try
            {
                var r = await _svc.CreateAsync(CurrentUserId, dto);
                return CreatedAtAction(nameof(GetById), new { id = r.CreditDebitId },
                    new ApiResponse<CreditDebitCardDto>(true, "Added.", r));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCreditDebitCardDto dto)
        {
            try
            {
                var r = await _svc.UpdateAsync(id, CurrentUserId, dto);
                return r == null
                    ? NotFound(Fail<CreditDebitCardDto>("Not found."))
                    : Ok(new ApiResponse<CreditDebitCardDto>(true, "Updated.", r));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                return await _svc.DeleteAsync(id, CurrentUserId)
                    ? Ok(new ApiResponse<object>(true, "Deleted.", null))
                    : NotFound(Fail<object>("Not found."));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpGet("{id:guid}/history")]
        public async Task<IActionResult> GetHistory(Guid id)
            => Ok(new ApiResponse<IEnumerable<CreditDebitCardHistoryDto>>(true, null,
                await _svc.GetHistoryAsync(id, CurrentUserId)));
    }

    // ── SecurityKeys ─────────────────────────────────────────────────
    [Route("api/[controller]")]
    public class SecurityKeysController : AppControllerBase
    {
        private readonly ISecurityKeyService _svc;
        public SecurityKeysController(ISecurityKeyService svc) => _svc = svc;

        [HttpGet("by-credential/{credentialId:guid}")]
        public async Task<IActionResult> GetByCredential(Guid credentialId)
            => Ok(new ApiResponse<IEnumerable<SecurityKeyDto>>(true, null,
                await _svc.GetByCredentialAsync(credentialId, CurrentUserId)));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var r = await _svc.GetByIdAsync(id, CurrentUserId);
            return r == null
                ? NotFound(Fail<SecurityKeyDto>("Not found."))
                : Ok(new ApiResponse<SecurityKeyDto>(true, null, r));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSecurityKeyDto dto)
        {
            try
            {
                var r = await _svc.CreateAsync(CurrentUserId, dto);
                return CreatedAtAction(nameof(GetById), new { id = r.SecurityKeyId },
                    new ApiResponse<SecurityKeyDto>(true, "Added.", r));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSecurityKeyDto dto)
        {
            try
            {
                var r = await _svc.UpdateAsync(id, CurrentUserId, dto);
                return r == null
                    ? NotFound(Fail<SecurityKeyDto>("Not found."))
                    : Ok(new ApiResponse<SecurityKeyDto>(true, "Updated.", r));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                return await _svc.DeleteAsync(id, CurrentUserId)
                    ? Ok(new ApiResponse<object>(true, "Deleted.", null))
                    : NotFound(Fail<object>("Not found."));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpGet("{id:guid}/history")]
        public async Task<IActionResult> GetHistory(Guid id)
            => Ok(new ApiResponse<IEnumerable<SecurityKeyHistoryDto>>(true, null,
                await _svc.GetHistoryAsync(id, CurrentUserId)));
    }

    // ── Pin ───────────────────────────────────────────────────────────
    [Route("api/[controller]")]
    public class PinController : AppControllerBase
    {
        private readonly IPinService _pinService;
        public PinController(IPinService pinService) => _pinService = pinService;

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyPinDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Pin))
                return BadRequest(Fail<object>("PIN is required."));

            var valid = await _pinService.VerifyPinAsync(CurrentUserId, dto.Pin);
            return valid
                ? Ok(new ApiResponse<object>(true, "PIN verified.", null))
                : BadRequest(Fail<object>("Incorrect PIN. Please try again."));
        }
    }

    // ── Access ───────────────────────────────────────────────────────
    [Route("api/[controller]")]
    public class AccessController : AppControllerBase
    {
        private readonly IAccessService _svc;
        public AccessController(IAccessService svc) => _svc = svc;

        // Returns "Owner" | "Edit" | "View" | "None" for the current user
        [HttpGet("my-permission/{credentialId:guid}")]
        public async Task<IActionResult> MyPermission(Guid credentialId)
        {
            var perm = await _svc.GetMyPermissionAsync(credentialId, CurrentUserId);
            return perm == "None"
                ? Forbid()
                : Ok(new ApiResponse<string>(true, null, perm));
        }

        [HttpGet("credential/{credentialId:guid}")]
        public async Task<IActionResult> GetAccessList(Guid credentialId)
            => Ok(new ApiResponse<IEnumerable<AccessDto>>(true, null,
                await _svc.GetAccessListForCredentialAsync(credentialId, CurrentUserId)));

        [HttpGet("shared-with-me")]
        public async Task<IActionResult> GetSharedWithMe()
            => Ok(new ApiResponse<IEnumerable<SharedWithMeDto>>(true, null,
                await _svc.GetSharedWithMeAsync(CurrentUserId)));

        [HttpPost("grant")]
        public async Task<IActionResult> Grant([FromBody] GrantAccessDto dto)
        {
            try
            {
                var r = await _svc.GrantAccessAsync(CurrentUserId, dto);
                return Ok(new ApiResponse<AccessDto>(true, $"Access granted to {dto.SharedToUsername}.", r));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(Fail<object>(ex.Message)); }
            catch (InvalidOperationException ex) { return BadRequest(Fail<object>(ex.Message)); }
        }

        [HttpDelete("{credentialAccessId:guid}")]
        public async Task<IActionResult> Revoke(Guid credentialAccessId)
        {
            try
            {
                return await _svc.RevokeAccessAsync(credentialAccessId, CurrentUserId)
                    ? Ok(new ApiResponse<object>(true, "Access revoked.", null))
                    : NotFound(Fail<object>("Not found."));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }
    }
}
