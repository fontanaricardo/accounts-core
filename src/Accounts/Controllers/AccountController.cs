namespace Accounts.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Data;
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Models;
    using Services;

    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly ISeiService _seiService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext dbContext,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory,
            IOptions<AppSettings> appSettings,
            ISeiService seiService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _dbContext = dbContext;
            _appSettings = appSettings;
            _seiService = seiService;
        }

        [AllowAnonymous]
        public IActionResult Entry(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return Redirect("/");
            }

            var value = string.IsNullOrWhiteSpace(returnUrl) ? Request.Headers["UrlReferrer"].ToString() : returnUrl;
            Response.Cookies.Append("UrlReferer", value);
            return RedirectToAction("Exit");
        }

        public IActionResult Exit()
        {
            var cookie = Request.Cookies["UrlReferer"];
            string url;
            Uri uri;

            if (cookie == null)
            {
                return Redirect("/");
            }
            else
            {
                url = cookie.ToString();

                // Não é possível excluir cookies do browser, apenas marcar como expirado
                Response.Cookies.Delete("UrlReferer");
            }

            // Caso a URL de redirecionamento não seja válida, direciona o usuário para a home
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                return Redirect("/");
            }

            AuthenticationToken token = new AuthenticationToken(User, uri);
            _dbContext.AuthenticationTokens.Add(token);
            _dbContext.SaveChanges();

            UriBuilder uriBuilder = new UriBuilder(uri);
            var newUri = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(uri.AbsoluteUri, "token", token.Token);

            return Redirect(newUri);
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            if (!Request.Cookies.ContainsKey("UrlReferer") && !string.IsNullOrWhiteSpace(returnUrl))
            {
                Response.Cookies.Append("UrlReferer", returnUrl);
            }

            ViewBag.SignDocumentLink = _appSettings.Value.SignDocumentLink;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            // Tratativa para a exceção HttpAntiForgeryException,
            // gerada caso o usuário abra duas abas do navegador com o endereço "/Account/Login",
            // efetue login numa das abas e posteriormente efetue login na outra aba
            if (User.Identity.IsAuthenticated)
            {
                TempData["success"] = string.Format("Usuário já autenticado como \"{0}\", utilize a opção \"Sair\" para trocar de usuário.", User.Identity.GetFullName());
                return RedirectToLocal("/");
            }

            return await LoginConfirmed(model, returnUrl);
        }

        // POST: /Account/LoginConfirmed
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginConfirmed(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Username = new string(model.Username.ToCharArray().Where(c => char.IsDigit(c)).ToArray());

            if (model.Username.Length < 12)
            {
                model.Username = model.Username.PadLeft(11, '0');
            }
            else
            {
                model.Username = model.Username.PadLeft(14, '0');
            }

            var user = await _userManager.FindByNameAsync(model.Username);

            if (user != null)
            {
                if (!user.EmailConfirmed)
                {
                    TempData["error"] = "Seu cadastro ainda não foi confirmado. Verifique sua caixa de e-mail e spam ou <a href='/Account/SendEmailConfirmation'>clique aqui</a> para reenvio do e-mail de confirmação.";
                    return View(model);
                }

                UpdateEletronicSignature(model);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Usuário não cadastrado.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);

            if (result == Microsoft.AspNetCore.Identity.SignInResult.Success)
            {
                await _userManager.AddClaimAsync(user, new Claim("FullUserName", user.FullUserName));
                await _userManager.AddClaimAsync(user, new Claim("EletronicSignatureStatus", user.EletronicSignatureStatus.ToString()));

                if (Request.Cookies.ContainsKey("UrlReferer"))
                {
                    return RedirectToLocal("/Account/Exit");
                }
                else if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    return RedirectToLocal("/");
                }
                else
                {
                    Response.Cookies.Append("UrlReferer", returnUrl);
                    return RedirectToAction("Exit");
                }
            }
            else if (result == Microsoft.AspNetCore.Identity.SignInResult.LockedOut)
            {
                return View("Lockout");
            }
            else if (result == Microsoft.AspNetCore.Identity.SignInResult.TwoFactorRequired)
            {
                return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Falha ao autenticar, verifique seu usuário e senha.");
                return View(model);
            }
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Models.AccountViewModels.RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "User created a new account with password.");
                    return RedirectToLocal(returnUrl);
                }

                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            var user = GetCurrentUserAsync().Result;
            await _userManager.RemoveClaimsAsync(user, User.Claims);
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, "User logged out.");
            return RedirectToAction("Login", "Account");
        }

        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        // GET: /Account/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (result.Succeeded)
            {
                _logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl });
            }

            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLoginConfirmation", new Models.AccountViewModels.ExternalLoginConfirmationViewModel { Email = email });
            }
        }

        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(Models.AccountViewModels.ExternalLoginConfirmationViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();

                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }

                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation(6, "User created an account using {Name} provider.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                }

                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        // GET: /Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return View("Error");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.CPF);
                if (user == null || user.Email != model.Email)
                {
                    TempData["error"] = "Dados incorretos! Confira os dados de acesso.";
                    return View(model);
                }

                string code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Scheme);
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Prefeitura de Joinville - Redefinir senha",
                    $"Prefeitura de Joinville\n\nAcesse a URL abaixo para Redefinir senha: {callbackUrl}");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            return View(model);
        }

        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            model.Document = new string(model.Document.ToCharArray().Where(c => char.IsDigit(c)).ToArray());

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            var person = _dbContext.People.Single(p => p.CPF == user.UserName);

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                if (_userManager.IsLockedOutAsync(user).Result)
                {
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MinValue);
                }

                _seiService.ChangePasswordSei(person, model.Password, true);

                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            AddLocalizedErrors(result, user);
            return View();
        }

        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // GET: /Account/SendCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new Models.AccountViewModels.SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode(Models.AccountViewModels.SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            // Generate the token and send it
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
            if (string.IsNullOrWhiteSpace(code))
            {
                return View("Error");
            }

            var message = "Your security code is: " + code;
            if (model.SelectedProvider == "Email")
            {
                await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "Security Code", message);
            }
            else if (model.SelectedProvider == "Phone")
            {
                await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
            }

            return RedirectToAction(nameof(VerifyCode), new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        // GET: /Account/VerifyCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            return View(new Models.AccountViewModels.VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(Models.AccountViewModels.VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);
            if (result.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning(7, "User account locked out.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid code.");
                return View(model);
            }
        }

        // GET: /Account/RegisterPerson
        [AllowAnonymous]
        public ActionResult RegisterPerson()
        {
            string[] phoneNumbers = { string.Empty };
            ViewBag.Phones = phoneNumbers;
            return View();
        }

        // POST: /Account/RegisterPerson
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterPerson(RegisterPersonViewModel model)
        {
            List<Phone> phones = new List<Phone>();
            string[] phoneNumbers = Request.Form["Phone"].Distinct().ToArray();
            ViewBag.Phones = phoneNumbers;

            ModelState.Clear();
            model.Person.Email = model.Email;
            TryValidateModel(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                foreach (string number in phoneNumbers)
                {
                    Phone phone = new Phone()
                    {
                        Number = number,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        Document = model.Person.CPF
                    };

                    if (!TryValidateModel(phone))
                    {
                        throw new ArgumentException("Número de telefone " + number + " inválido, utilize o formato (xx) xxxxxxxx, nono dígito opcional.");
                    }

                    phones.Add(phone);
                }

                if (ModelState.IsValid)
                {
                    // Cria o usuário da aplicação
                    var user = new ApplicationUser
                    {
                        UserName = model.Person.CPF.ToString(),
                        Email = model.Person.Email,
                        FullUserName = model.Person.Name,
                        EletronicSignatureStatus = model.Person.EletronicSignatureStatus,
                        EmailConfirmed = false
                    };
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        _dbContext.People.Add(model.Person);
                        _dbContext.SaveChanges();

                        _dbContext.Phones.AddRange(phones);
                        _dbContext.SaveChanges();

                        await EmailConfirmation(user);

                        StringBuilder sb = new StringBuilder();
                        sb.Append("Cadastro realizado com sucesso.<br/>");
                        sb.Append("Foi lhe enviado um e-mail para confirmação do cadastro.<br/>");
                        sb.Append("Caso não tenha recebido o e-mail, verifique sua caixa de spam ou <a href='/Account/SendEmailConfirmation'>clique aqui</a> para reenvio do e-mail de confirmação.<br/>");

                        TempData["success"] = sb.ToString();

                        return RedirectToAction("Login", "Account");
                    }

                    AddLocalizedErrors(result, user);
                }
            }
            catch (ArgumentException ex)
            {
                TempData["error"] = ex.Message;
            }

            return View(model);
        }

        // GET: /Account/RegisterCompany
        [AllowAnonymous]
        public ActionResult RegisterCompany()
        {
            string[] phoneNumbers = { string.Empty };
            ViewBag.Phones = phoneNumbers;
            return View();
        }

        // POST: /Account/RegisterCompany
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterCompany(RegisterCompanyViewModel model)
        {
            List<Phone> phones = new List<Phone>();
            string[] phoneNumbers = Request.Form["Phone"].Distinct().ToArray();
            ViewBag.Phones = phoneNumbers;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                foreach (string number in phoneNumbers)
                {
                    Phone phone = new Phone()
                    {
                        Number = number,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        Document = model.Company.CNPJ
                    };

                    if (!TryValidateModel(phone))
                    {
                        throw new ArgumentException("Phone number " + number + " is invalid");
                    }

                    phones.Add(phone);
                }

                if (ModelState.IsValid)
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.Company.CNPJ.ToString(),
                        Email = model.Company.Email
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        _dbContext.Companies.Add(model.Company);
                        _dbContext.SaveChanges();

                        _dbContext.Phones.AddRange(phones);
                        _dbContext.SaveChanges();

                        await EmailConfirmation(user);

                        return RedirectToAction("Login", "Account");
                    }

                    AddLocalizedErrors(result, user);
                }
            }
            catch (ArgumentException ex)
            {
                TempData["error"] = ex.Message;
            }

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult SendEmailConfirmation()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendEmailConfirmation(ConfirmationEmailViewModel model)
        {
            model.CPF = new string(model.CPF.ToCharArray().Where(c => char.IsDigit(c)).ToArray());

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.CPF);

                if (user == null)
                {
                    TempData["error"] = "Dados incorretos. Confira os dados de acesso.";
                }
                else
                {
                    if (user.Email != model.Email)
                    {
                        TempData["error"] = "Dados incorretos. Confira os dados de acesso.";
                    }
                    else if (user.EmailConfirmed)
                    {
                        TempData["success"] = "O email " + model.Email + " já foi confirmado.";
                        return Redirect("/");
                    }
                    else
                    {
                      await EmailConfirmation(user);
                        TempData["success"] = "Email de confirmação enviado para " + model.Email;
                        return Redirect("/");
                    }
                }
            }

            return View(model);
        }

        private async Task EmailConfirmation(ApplicationUser user)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(
               "ConfirmEmail",
               "Account",
               new { userId = user.Id, code = code },
               protocol: Request.Scheme);

            string operation = "Confirmação do e-mail";

            await _emailSender.SendEmailAsync(
                user.Email,
                "Prefeitura de Joinville - " + operation,
                $"Prefeitura de Joinville\n\nAcesse a URL abaixo para {operation}: {callbackUrl}");
        }

        // TODO: Implementar método para pessoa jurídica
        private void UpdateEletronicSignature(Models.LoginViewModel model)
        {
            var person = _dbContext.People.SingleOrDefault(p => p.CPF == model.Username);

            if (person == null)
            {
                return;
            }

            _seiService.UpdateEletronicSignatureStatus(person);

            var user = _userManager.FindByNameAsync(model.Username).Result;
            user.EletronicSignatureStatus = person.EletronicSignatureStatus;
            User.AddUpdateClaim("EletronicSignatureStatus", person.EletronicSignatureStatus.ToString());
            var updateResult = _userManager.UpdateAsync(user).Result;
            _dbContext.SaveChanges();
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect("/");
            }
        }

        private void AddLocalizedErrors(IdentityResult result, ApplicationUser user)
        {
            foreach (var error in result.Errors)
            {
                var localizedError = error.Description;
                string userName = string.Empty;
                string email = string.Empty;
                if (user != null)
                {
                    userName = user.UserName;
                    email = user.Email;
                }

                // password errors
                localizedError = localizedError.Replace("Captcha answer cannot be empty.", "Captcha não pode ficar em branco.");
                localizedError = localizedError.Replace("Incorrect captcha answer.", "Captcha incorreto.");
                localizedError = localizedError.Replace("Incorrect password.", "Senha incorreta.");
                localizedError = localizedError.Replace("Your password has been changed.", "Senha incorreta.");
                localizedError = localizedError.Replace("Passwords must have at least one lowercase ('a'-'z').", "A senha deve ter pelo menos uma letra minúscula.");
                localizedError = localizedError.Replace("Passwords must have at least one uppercase ('A'-'Z').", "A senha deve ter pelo menos uma letra maiúscula.");
                localizedError = localizedError.Replace("Passwords must have at least one digit ('0'-'9').", "A senha deve ter pelo menos um dígito.");
                localizedError = localizedError.Replace("Phone number is invalid.", "Número de telefone inválido.");

                // register errors
                localizedError = localizedError.Replace("Name " + userName + " is already taken.", "Usuário {0} já está registrado.".Replace("{0}", userName));
                localizedError = localizedError.Replace("Email '" + email + "' is already taken.", "E-mail {0} já está registrado.".Replace("{0}", email));

                ModelState.AddModelError(string.Empty, localizedError);
            }
        }
    }
}
