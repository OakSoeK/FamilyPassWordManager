
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using FPassWordManager.Models;
using FPasswordManager.Data;

namespace FPasswordManager.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IPasswordHasher<User> _hasher;
        private readonly AppDbContext _db;

        public IndexModel(UserManager<User> userManager, SignInManager<User> signInManager,
                          IPasswordHasher<User> hasher, AppDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _hasher = hasher;
            _db = db;
        }

        [BindProperty]
        public ProfileInput Input { get; set; } = new();

        public class ProfileInput
        {
            [Required, MaxLength(30)]
            [Display(Name = "Username")]
            public string Username { get; set; } = string.Empty;

            [MaxLength(30)]
            [Display(Name = "First name")]
            public string FirstName { get; set; } = string.Empty;

            [MaxLength(30)]
            [Display(Name = "Last name")]
            public string? LastName { get; set; }

            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;
        }

        [BindProperty]
        public PasswordChangeInput PasswordInput { get; set; } = new();

        public class PasswordChangeInput
        {
            [Required, DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string OldPassword { get; set; } = string.Empty;

            [Required, MinLength(6), DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; } = string.Empty;

            [Required, DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
            [Display(Name = "Confirm new password")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        [BindProperty]
        public PinChangeInput PinInput { get; set; } = new();

        public class PinChangeInput
        {
            [Required, RegularExpression(@"^\d{5}$", ErrorMessage = "PIN must be exactly 5 digits.")]
            [Display(Name = "New PIN")]
            public string NewPin { get; set; } = string.Empty;

            [Required, Compare(nameof(NewPin), ErrorMessage = "PINs do not match.")]
            [Display(Name = "Confirm PIN")]
            public string ConfirmPin { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            Input = new ProfileInput
            {
                Username = user.UserName ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                Input.Email = user.Email ?? string.Empty;
                return Page();
            }

            var existing = await _userManager.FindByNameAsync(Input.Username);
            if (existing != null && existing.Id != user.Id)
            {
                ModelState.AddModelError("Input.Username", "That username is already taken.");
                Input.Email = user.Email ?? string.Empty;
                return Page();
            }

            user.UserName = Input.Username;
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                Input.Email = user.Email ?? string.Empty;
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["ProfileSuccess"] = "Profile updated successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadInputAsync(user);
                return Page();
            }

            var result = await _userManager.ChangePasswordAsync(user, PasswordInput.OldPassword, PasswordInput.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                await LoadInputAsync(user);
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["PasswordSuccess"] = "Password changed successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePinAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadInputAsync(user);
                return Page();
            }

            user.PinHash = _hasher.HashPassword(user, PinInput.NewPin);
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                await LoadInputAsync(user);
                return Page();
            }

            TempData["PinSuccess"] = "Vault PIN updated successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAccountAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var id = user.Id;

            // Clear out optional FK references so that the user row can be deleted
            await _db.Database.ExecuteSqlRawAsync(
                "UPDATE WebCredentials SET EditorId = NULL WHERE EditorId = {0}", id);
            await _db.Database.ExecuteSqlRawAsync(
                "UPDATE CreditCards SET EditorId = NULL WHERE EditorId = {0}", id);
            await _db.Database.ExecuteSqlRawAsync(
                "UPDATE SecurityKeys SET EditorId = NULL WHERE EditorId = {0}", id);
            await _db.Database.ExecuteSqlRawAsync(
                "UPDATE WebCredentialsHistorys SET ChangedByUserId = NULL WHERE ChangedByUserId = {0}", id);
            await _db.Database.ExecuteSqlRawAsync(
                "UPDATE CreditCardsHistorys SET ChangedByUserId = NULL WHERE ChangedByUserId = {0}", id);
            await _db.Database.ExecuteSqlRawAsync(
                "UPDATE SecurityKeysHistorys SET ChangedByUserId = NULL WHERE ChangedByUserId = {0}", id);

            // Delete item access rows (recipient and sharer)
            _db.WebCredentialAccesses.RemoveRange(_db.WebCredentialAccesses.Where(a => a.UserId == id || a.SharedByUserId == id));
            _db.CreditDebitCardAccesses.RemoveRange(_db.CreditDebitCardAccesses.Where(a => a.UserId == id || a.SharedByUserId == id));
            _db.SecurityKeyAccesses.RemoveRange(_db.SecurityKeyAccesses.Where(a => a.UserId == id || a.SharedByUserId == id));

            // Delete folder access rows
            _db.CredentialAccesses.RemoveRange(_db.CredentialAccesses.Where(a => a.UserId == id || a.SharedByUserId == id));

            await _db.SaveChangesAsync();

            // Sign out before deleting 
            await _signInManager.SignOutAsync();
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException("Unexpected error deleting user.");

            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        private async Task LoadInputAsync(User user)
        {
            Input = new ProfileInput
            {
                Username = user.UserName ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty
            };
            await Task.CompletedTask;
        }
    }
}