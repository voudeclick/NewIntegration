using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VDC.Integration.Domain.Consts;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.EntityFramework.Repositories;
using VDC.Integration.Identity.Models;
using VDC.Integration.Identity.Services;

namespace VDC.Integration.Front.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly ParamRepository _paramRepository;
        private readonly UserProfileRepository _userProfileRepository;

        public ExternalLoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender,
            ParamRepository paramRepository,
            UserProfileRepository userProfileRepository)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _paramRepository = paramRepository;
            _userProfileRepository = userProfileRepository;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ProviderDisplayName { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        [TempData]
        public string TokenApi { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "E-mail")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Informe o {0}.")]
            [Display(Name = "Nome")]
            public string Name { get; set; }
        }

        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/Admin/Dashboard");

            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);

                return await SignInAsync(info, returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;

                if (!info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    ErrorMessage = $"Não foi possível recuperar o e-mail.";
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }

                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                Input = new InputModel
                {
                    Email = email,
                    Name = info.Principal.FindFirstValue(ClaimTypes.Name),
                };

                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Admin/Dashboard");
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                ErrorMessage = "Erro ao carregar informações externas de login.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };

                var result = await _userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    _ = await _userManager.AddToRoleAsync(user, "Viewer");

                    await _userProfileRepository.AddAsync(new UserProfile()
                    {
                        Id = user.Id,
                        Name = Input.Name
                    });

                    result = await _userManager.AddLoginAsync(user, info);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        return await SignInAsync(info, returnUrl, isPersistent: false);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> SignInAsync(ExternalLoginInfo info, string returnUrl, bool isPersistent = true)
        {

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);
            var roles = await _userManager.GetRolesAsync(user);
            var userProfile = _userProfileRepository.GetById(user.Id);

            var paramValue = _paramRepository.GetByKeyAsync(ParamConsts.Authentication)
                .Result?.GetValueBykey(AuthenticationConsts.ExpiresMinutes);

            var expiresMinutes = Convert.ToDouble(paramValue.GetDoubleOrDefault(5));

            await _signInManager.SignInAsync(user, new AuthenticationProperties()
            {
                ExpiresUtc = DateTime.UtcNow.AddMinutes(expiresMinutes),
                IsPersistent = isPersistent,
                IssuedUtc = DateTime.UtcNow

            }, info.LoginProvider);

            TokenApi = TokenService.GenerateToken(new UserModel()
            {
                Id = user.Id,
                UserName = userProfile?.Name,
                Email = email,
                Roles = roles.Select(role => new UserRoleModel(role, role, true)).ToList(),
                ExpiresMinutes = expiresMinutes
            });

            return LocalRedirect(returnUrl);

        }
    }
}
