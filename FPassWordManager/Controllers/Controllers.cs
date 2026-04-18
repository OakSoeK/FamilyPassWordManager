using FPassWordManager.DTOs;
using FPassWordManager.Models;
using FPassWordManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace FPasswordManager.Controllers
{   

    [Authorize]
    [ApiController]
    public abstract class AppControllerBase : ControllerBase
    {
        protected Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        protected static ApiResponse<T> Fail<T>(string msg) => new(false, msg, default);
    }

    // Credentials 
    [Route("api/[controller]")]
    public class CredentialsController : AppControllerBase
    {
        private readonly ICredentialService _svc;
        public CredentialsController(ICredentialService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetMine()
            => Ok(new ApiResponse<IEnumerable<CredentialDto>>(true, null, await _svc.GetMyCredentialsAsync(CurrentUserId)));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var r = await _svc.GetByIdAsync(id, CurrentUserId);
            return r == null ? NotFound(Fail<CredentialDto>("Not found.")) : Ok(new ApiResponse<CredentialDto>(true, null, r));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCredentialDto dto)
        {
            var r = await _svc.CreateAsync(CurrentUserId, dto);
            return CreatedAtAction(nameof(GetById), new { id = r.CredentialId }, new ApiResponse<CredentialDto>(true, "Created.", r));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCredentialDto dto)
        {
            var r = await _svc.UpdateAsync(id, CurrentUserId, dto);
            return r == null ? NotFound(Fail<CredentialDto>("Not found.")) : Ok(new ApiResponse<CredentialDto>(true, "Updated.", r));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
            => await _svc.DeleteAsync(id, CurrentUserId)
                ? Ok(new ApiResponse<object>(true, "Deleted.", null))
                : NotFound(Fail<object>("Not found."));
    }

    // WebCredentials
    [Route("api/[controller]")]
    public class WebCredentialsController : AppControllerBase
    {
        private readonly IWebCredentialService _svc;
        public WebCredentialsController(IWebCredentialService svc) => _svc = svc;

        [HttpGet("by-credential/{credentialId:guid}")]
        public async Task<IActionResult> GetByCredential(Guid credentialId)
            => Ok(new ApiResponse<IEnumerable<WebCredentialDto>>(true, null, await _svc.GetByCredentialAsync(credentialId, CurrentUserId)));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var r = await _svc.GetByIdAsync(id, CurrentUserId);
            return r == null ? NotFound(Fail<WebCredentialDto>("Not found.")) : Ok(new ApiResponse<WebCredentialDto>(true, null, r));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWebCredentialDto dto)
        {
            try { var r = await _svc.CreateAsync(CurrentUserId, dto); return CreatedAtAction(nameof(GetById), new { id = r.WebCredentialId }, new ApiResponse<WebCredentialDto>(true, "Added.", r)); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWebCredentialDto dto)
        {
            try { var r = await _svc.UpdateAsync(id, CurrentUserId, dto); return r == null ? NotFound(Fail<WebCredentialDto>("Not found.")) : Ok(new ApiResponse<WebCredentialDto>(true, "Updated.", r)); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try { return await _svc.DeleteAsync(id, CurrentUserId) ? Ok(new ApiResponse<object>(true, "Deleted.", null)) : NotFound(Fail<object>("Not found.")); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpGet("{id:guid}/history")]
        public async Task<IActionResult> GetHistory(Guid id)
            => Ok(new ApiResponse<IEnumerable<WebCredentialHistoryDto>>(true, null, await _svc.GetHistoryAsync(id, CurrentUserId)));

        [HttpGet("{id:guid}/reveal")]
        public async Task<IActionResult> Reveal(Guid id)
        {
            var r = await _svc.RevealAsync(id, CurrentUserId);
            return r == null ? NotFound(Fail<RevealWebDto>("Not found.")) : Ok(new ApiResponse<RevealWebDto>(true, null, r));
        }
    }

    // CreditDebitCards
    [Route("api/[controller]")]
    public class CreditDebitCardsController : AppControllerBase
    {
        private readonly ICreditDebitCardService _svc;
        public CreditDebitCardsController(ICreditDebitCardService svc) => _svc = svc;

        [HttpGet("by-credential/{credentialId:guid}")]
        public async Task<IActionResult> GetByCredential(Guid credentialId)
            => Ok(new ApiResponse<IEnumerable<CreditDebitCardDto>>(true, null, await _svc.GetByCredentialAsync(credentialId, CurrentUserId)));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var r = await _svc.GetByIdAsync(id, CurrentUserId);
            return r == null ? NotFound(Fail<CreditDebitCardDto>("Not found.")) : Ok(new ApiResponse<CreditDebitCardDto>(true, null, r));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCreditDebitCardDto dto)
        {
            try { var r = await _svc.CreateAsync(CurrentUserId, dto); return CreatedAtAction(nameof(GetById), new { id = r.CreditDebitId }, new ApiResponse<CreditDebitCardDto>(true, "Added.", r)); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCreditDebitCardDto dto)
        {
            try { var r = await _svc.UpdateAsync(id, CurrentUserId, dto); return r == null ? NotFound(Fail<CreditDebitCardDto>("Not found.")) : Ok(new ApiResponse<CreditDebitCardDto>(true, "Updated.", r)); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try { return await _svc.DeleteAsync(id, CurrentUserId) ? Ok(new ApiResponse<object>(true, "Deleted.", null)) : NotFound(Fail<object>("Not found.")); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpGet("{id:guid}/history")]
        public async Task<IActionResult> GetHistory(Guid id)
            => Ok(new ApiResponse<IEnumerable<CreditDebitCardHistoryDto>>(true, null, await _svc.GetHistoryAsync(id, CurrentUserId)));

        [HttpGet("{id:guid}/reveal")]
        public async Task<IActionResult> Reveal(Guid id)
        {
            var r = await _svc.RevealAsync(id, CurrentUserId);
            return r == null ? NotFound(Fail<RevealCardDto>("Not found.")) : Ok(new ApiResponse<RevealCardDto>(true, null, r));
        }
    }

    // SecurityKeys
    [Route("api/[controller]")]
    public class SecurityKeysController : AppControllerBase
    {
        private readonly ISecurityKeyService _svc;
        public SecurityKeysController(ISecurityKeyService svc) => _svc = svc;

        [HttpGet("by-credential/{credentialId:guid}")]
        public async Task<IActionResult> GetByCredential(Guid credentialId)
            => Ok(new ApiResponse<IEnumerable<SecurityKeyDto>>(true, null, await _svc.GetByCredentialAsync(credentialId, CurrentUserId)));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var r = await _svc.GetByIdAsync(id, CurrentUserId);
            return r == null ? NotFound(Fail<SecurityKeyDto>("Not found.")) : Ok(new ApiResponse<SecurityKeyDto>(true, null, r));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSecurityKeyDto dto)
        {
            try { var r = await _svc.CreateAsync(CurrentUserId, dto); return CreatedAtAction(nameof(GetById), new { id = r.SecurityKeyId }, new ApiResponse<SecurityKeyDto>(true, "Added.", r)); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSecurityKeyDto dto)
        {
            try { var r = await _svc.UpdateAsync(id, CurrentUserId, dto); return r == null ? NotFound(Fail<SecurityKeyDto>("Not found.")) : Ok(new ApiResponse<SecurityKeyDto>(true, "Updated.", r)); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try { return await _svc.DeleteAsync(id, CurrentUserId) ? Ok(new ApiResponse<object>(true, "Deleted.", null)) : NotFound(Fail<object>("Not found.")); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpGet("{id:guid}/history")]
        public async Task<IActionResult> GetHistory(Guid id)
            => Ok(new ApiResponse<IEnumerable<SecurityKeyHistoryDto>>(true, null, await _svc.GetHistoryAsync(id, CurrentUserId)));

        [HttpGet("{id:guid}/reveal")]
        public async Task<IActionResult> Reveal(Guid id)
        {
            var r = await _svc.RevealAsync(id, CurrentUserId);
            return r == null ? NotFound(Fail<RevealKeyDto>("Not found.")) : Ok(new ApiResponse<RevealKeyDto>(true, null, r));
        }
    }

    // Pin
    [Route("api/[controller]")]
    public class PinController : AppControllerBase
    {
        private readonly IPinService _svc;
        public PinController(IPinService svc) => _svc = svc;

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyPinDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Pin)) return BadRequest(Fail<object>("PIN is required."));
            var valid = await _svc.VerifyPinAsync(CurrentUserId, dto.Pin);
            return valid ? Ok(new ApiResponse<object>(true, "PIN verified.", null)) : BadRequest(Fail<object>("Incorrect PIN."));
        }
    }

    // Access 
    [Route("api/[controller]")]
    public class AccessController : AppControllerBase
    {
        private readonly IAccessService _svc;
        private readonly IItemAccessService _itemSvc;
        public AccessController(IAccessService svc, IItemAccessService itemSvc)
        { _svc = svc; _itemSvc = itemSvc; }

        [HttpGet("my-permission/{credentialId:guid}")]
        public async Task<IActionResult> MyPermission(Guid credentialId)
        {
            var perm = await _svc.GetMyPermissionAsync(credentialId, CurrentUserId);
            return perm == "None" ? Forbid() : Ok(new ApiResponse<string>(true, null, perm));
        }

        [HttpGet("credential/{credentialId:guid}")]
        public async Task<IActionResult> GetAccessList(Guid credentialId)
            => Ok(new ApiResponse<IEnumerable<AccessDto>>(true, null, await _svc.GetAccessListForCredentialAsync(credentialId, CurrentUserId)));

        [HttpGet("shared-with-me")]
        public async Task<IActionResult> GetSharedWithMe()
            => Ok(new ApiResponse<IEnumerable<SharedWithMeDto>>(true, null, await _svc.GetSharedWithMeAsync(CurrentUserId)));

        [HttpGet("shared-items")]
        public async Task<IActionResult> GetSharedItems()
            => Ok(new ApiResponse<IEnumerable<SharedItemDto>>(true, null, await _itemSvc.GetAllSharedWithMeAsync(CurrentUserId)));

        [HttpPost("grant")]
        public async Task<IActionResult> Grant([FromBody] GrantAccessDto dto)
        {
            try { var r = await _svc.GrantAccessAsync(CurrentUserId, dto); return Ok(new ApiResponse<AccessDto>(true, "Access granted.", r)); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(Fail<object>(ex.Message)); }
            catch (InvalidOperationException ex) { return BadRequest(Fail<object>(ex.Message)); }
        }

        [HttpDelete("{credentialAccessId:guid}")]
        public async Task<IActionResult> Revoke(Guid credentialAccessId)
        {
            try { return await _svc.RevokeAccessAsync(credentialAccessId, CurrentUserId) ? Ok(new ApiResponse<object>(true, "Revoked.", null)) : NotFound(Fail<object>("Not found.")); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }
    }

    // Web credential item access
    [Route("api/webcredentials/{webCredentialId:guid}/access")]
    public class WebCredentialAccessController : AppControllerBase
    {
        private readonly IItemAccessService _svc;
        public WebCredentialAccessController(IItemAccessService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> List(Guid webCredentialId)
            => Ok(new ApiResponse<IEnumerable<ItemAccessDto>>(true, null,
                await _svc.GetWebAccessListAsync(webCredentialId, CurrentUserId)));

        [HttpPost]
        public async Task<IActionResult> Grant(Guid webCredentialId, [FromBody] GrantItemAccessDto dto)
        {
            try
            {
                var r = await _svc.GrantWebAccessAsync(webCredentialId, CurrentUserId, dto);
                return Ok(new ApiResponse<ItemAccessDto>(true, "Access granted.", r));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(Fail<object>(ex.Message)); }
            catch (InvalidOperationException ex) { return BadRequest(Fail<object>(ex.Message)); }
        }

        [HttpDelete("{accessId:guid}")]
        public async Task<IActionResult> Revoke(Guid webCredentialId, Guid accessId)
        {
            try
            {
                return await _svc.RevokeWebAccessAsync(accessId, CurrentUserId)
                    ? Ok(new ApiResponse<object>(true, "Revoked.", null))
                    : NotFound(Fail<object>("Not found."));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }
    }

    //Card item access
    [Route("api/creditdebitcards/{creditDebitId:guid}/access")]
    public class CreditDebitCardAccessController : AppControllerBase
    {
        private readonly IItemAccessService _svc;
        public CreditDebitCardAccessController(IItemAccessService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> List(Guid creditDebitId)
            => Ok(new ApiResponse<IEnumerable<ItemAccessDto>>(true, null,
                await _svc.GetCardAccessListAsync(creditDebitId, CurrentUserId)));

        [HttpPost]
        public async Task<IActionResult> Grant(Guid creditDebitId, [FromBody] GrantItemAccessDto dto)
        {
            try
            {
                var r = await _svc.GrantCardAccessAsync(creditDebitId, CurrentUserId, dto);
                return Ok(new ApiResponse<ItemAccessDto>(true, "Access granted.", r));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(Fail<object>(ex.Message)); }
            catch (InvalidOperationException ex) { return BadRequest(Fail<object>(ex.Message)); }
        }

        [HttpDelete("{accessId:guid}")]
        public async Task<IActionResult> Revoke(Guid creditDebitId, Guid accessId)
        {
            try
            {
                return await _svc.RevokeCardAccessAsync(accessId, CurrentUserId)
                    ? Ok(new ApiResponse<object>(true, "Revoked.", null))
                    : NotFound(Fail<object>("Not found."));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }
    }

    //Security key item access 
    [Route("api/securitykeys/{securityKeyId:guid}/access")]
    public class SecurityKeyAccessController : AppControllerBase
    {
        private readonly IItemAccessService _svc;
        public SecurityKeyAccessController(IItemAccessService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> List(Guid securityKeyId)
            => Ok(new ApiResponse<IEnumerable<ItemAccessDto>>(true, null,
                await _svc.GetKeyAccessListAsync(securityKeyId, CurrentUserId)));

        [HttpPost]
        public async Task<IActionResult> Grant(Guid securityKeyId, [FromBody] GrantItemAccessDto dto)
        {
            try
            {
                var r = await _svc.GrantKeyAccessAsync(securityKeyId, CurrentUserId, dto);
                return Ok(new ApiResponse<ItemAccessDto>(true, "Access granted.", r));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(Fail<object>(ex.Message)); }
            catch (InvalidOperationException ex) { return BadRequest(Fail<object>(ex.Message)); }
        }

        [HttpDelete("{accessId:guid}")]
        public async Task<IActionResult> Revoke(Guid securityKeyId, Guid accessId)
        {
            try
            {
                return await _svc.RevokeKeyAccessAsync(accessId, CurrentUserId)
                    ? Ok(new ApiResponse<object>(true, "Revoked.", null))
                    : NotFound(Fail<object>("Not found."));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }
    }
}