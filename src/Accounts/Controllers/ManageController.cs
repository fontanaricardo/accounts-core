namespace Accounts.Controllers
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Data;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Models;
    using Models.ManageViewModels;
    using Services;

    [Authorize]
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        private readonly IOptions<AppSettings> _appSettings;

        public ManageController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext dbContext,
        IEmailSender emailSender,
        ISmsSender smsSender,
        ILoggerFactory loggerFactory,
        IOptions<AppSettings> appSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<ManageController>();
            _dbContext = dbContext;
            _appSettings = appSettings;
        }

        // GET: /Manage/Index
        [HttpGet]
        public async Task<IActionResult> Index(ManageMessageId? message = null)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Sua senha foi alterada."
                : message == ManageMessageId.SetPasswordSuccess ? "Sua senha foi definida."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "Ocorreu um erro."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : string.Empty;

            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return View("Error");
            }

            var person = _dbContext.People.Single(p => p.CPF == User.Identity.Name);

            var model = new IndexViewModel
            {
                HasPassword = await _userManager.HasPasswordAsync(user),
                PhoneNumber = await _userManager.GetPhoneNumberAsync(user),
                TwoFactor = await _userManager.GetTwoFactorEnabledAsync(user),
                Logins = await _userManager.GetLoginsAsync(user),
                BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
                EletronicSignatureStatus = person.EletronicSignatureStatus,
                LinkSeiProtocol = person.LinkSeiProtocol
            };

            ViewBag.Decree = _appSettings.Value.Decree;
            ViewBag.Instruction = _appSettings.Value.Instruction;

            return View(model);
        }

        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel account)
        {
            ManageMessageId? message = ManageMessageId.Error;
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    message = ManageMessageId.RemoveLoginSuccess;
                }
            }

            return RedirectToAction(nameof(ManageLogins), new { Message = message });
        }

        // GET: /Manage/AddPhoneNumber
        public IActionResult AddPhoneNumber()
        {
            return View();
        }

        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Generate the token and send it
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
            await _smsSender.SendSmsAsync(model.PhoneNumber, "Your security code is: " + code);
            return RedirectToAction(nameof(VerifyPhoneNumber), new { PhoneNumber = model.PhoneNumber });
        }

        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableTwoFactorAuthentication()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(1, "User enabled two-factor authentication.");
            }

            return RedirectToAction(nameof(Index), "Manage");
        }

        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableTwoFactorAuthentication()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, false);
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(2, "User disabled two-factor authentication.");
            }

            return RedirectToAction(nameof(Index), "Manage");
        }

        // GET: /Manage/VerifyPhoneNumber
        [HttpGet]
        public async Task<IActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);

            // Send an SMS to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, model.Code);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.AddPhoneSuccess });
                }
            }

            // If we got this far, something failed, redisplay the form
            ModelState.AddModelError(string.Empty, "Failed to verify phone number");
            return View(model);
        }

        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePhoneNumber()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.SetPhoneNumberAsync(user, null);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.RemovePhoneSuccess });
                }
            }

            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        // GET: /Manage/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = GetCurrentUserAsync().Result;

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (result.Succeeded)
            {
                StringBuilder body = new StringBuilder();
                body.AppendLine("Prezado(a) usuário(a):");
                body.AppendLine(string.Empty);
                body.AppendLine("Foi realizada a alteração da sua senha de acesso aos autosserviços do Município de Joinville, disponíveis através do site https://accounts.joinville.sc.gov.br/. Caso você não tenha realizado essa alteração envie um e-mail para sei@joinville.sc.gov.br ou ligue no número (47) 3431-3261 e informe sobre essa alteração.");
                body.AppendLine(string.Empty);
                body.Append("CPF da conta: ").AppendLine(User.Identity.Name);
                body.AppendLine("Data e hora da alteração: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                await _emailSender.SendEmailAsync(user.Email, "Alteração de senha", body.ToString());

                if (user != null)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    var person = _dbContext.People.Single(p => p.CPF == User.Identity.Name);

                    // Atualiza a senha da certificação do usuário
                    var es = new EletronicSignatureViewModel()
                    {
                        Password = model.NewPassword
                    };

                    es.ChangePassword(person, _appSettings.Value);
                }

                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }

            AddLocalizedErrors(result);
            return View(model);
        }

        // GET: /Manage/ChangePassword
        public ActionResult ChangeEmail()
        {
            var user = GetCurrentUserAsync().Result;
            ChangeEmailViewModel changeEmail = new ChangeEmailViewModel()
            {
                CurrentEmail = user.Email
            };
            return View(changeEmail);
        }

        // POST: /Manage/ChangeEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeEmail(ChangeEmailViewModel model)
        {
            var user = GetCurrentUserAsync().Result;

            model.CurrentEmail = user.Email;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!_userManager.CheckPasswordAsync(user, model.Password).Result)
            {
                ModelState.AddModelError("Password", "Senha incorreta.");
                return View(model);
            }

            if (_userManager.FindByEmailAsync(model.Email).Result != null)
            {
                TempData["error"] = "Endereço de e-mail em uso";
                return View(model);
            }

            // Se possuir usuário no SEI deve atualizar o e-mail do SEI
            Person person = GetPerson();
            person.Email = model.Email;

            try
            {
                if (person.SeiId != null)
                {
                    person.CreateOrUpdateSeiUser(model.Password, _appSettings.Value);
                }
            }
            catch (ArgumentException ex)
            {
                TempData["error"] = ex.Message;
                return View(model);
            }

            user.Email = model.Email;
            user.EmailConfirmed = false;
            var updateResult = _userManager.UpdateAsync(user).Result;

            if (updateResult.Succeeded)
            {
                _dbContext.Entry(person).Property("Email").IsModified = true;
                _dbContext.SaveChanges();

                await EmailConfirmation(user);
                person.ChangePasswordSei(model.Password, _appSettings.Value, true);

                TempData["success"] = "Alteração efetuada com sucesso. Confirme o novo endereço de e-mail para se autenticar";
                await _signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        // GET: /Manage/SetPassword
        [HttpGet]
        public IActionResult SetPassword()
        {
            return View();
        }

        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.SetPasswordSuccess });
                }

                AddErrors(result);
                return View(model);
            }

            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        // GET: /Manage/ManageLogins
        [HttpGet]
        public async Task<IActionResult> ManageLogins(ManageMessageId? message = null)
        {
            ViewData["StatusMessage"] =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.AddLoginSuccess ? "The external login was added."
                : message == ManageMessageId.Error ? "An error has occurred."
                : string.Empty;
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            var userLogins = await _userManager.GetLoginsAsync(user);
            var otherLogins = _signInManager.GetExternalAuthenticationSchemes().Where(auth => userLogins.All(ul => auth.AuthenticationScheme != ul.LoginProvider)).ToList();
            ViewData["ShowRemoveButton"] = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action("LinkLoginCallback", "Manage");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            return Challenge(properties, provider);
        }

        // GET: /Manage/LinkLoginCallback
        [HttpGet]
        public async Task<ActionResult> LinkLoginCallback()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));
            if (info == null)
            {
                return RedirectToAction(nameof(ManageLogins), new { Message = ManageMessageId.Error });
            }

            var result = await _userManager.AddLoginAsync(user, info);
            var message = result.Succeeded ? ManageMessageId.AddLoginSuccess : ManageMessageId.Error;
            return RedirectToAction(nameof(ManageLogins), new { Message = message });
        }

        [Authorize]
        public ActionResult EletronicSignature()
        {
            Person person = GetPerson();

            if (person.EletronicSignatureStatus != EletronicSignatureStatus.Unsolicited)
            {
                TempData["success"] = "Assinatura eletrônica já solicitada.";
                return Redirect("/");
            }

            return View();
        }

        [Authorize]
        [ActionName("EletronicSignature")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EletronicSignatureConfirmation(EletronicSignatureViewModel model)
        {
            Person person = GetPerson();

            if (person.EletronicSignatureStatus != EletronicSignatureStatus.Unsolicited)
            {
                TempData["success"] = "Assinatura eletrônica já solicitada.";
                return Redirect("/");
            }

            if (ModelState.IsValid)
            {
                var user = _userManager.FindByNameAsync(User.Identity.Name).Result;
                if (!_userManager.CheckPasswordAsync(user, model.Password).Result)
                {
                    ModelState.AddModelError("Password", "Senha incorreta.");
                    return View(model);
                }

                try
                {
                    // Chamado 84925: Persiste o número do protocolo do SEI logo após a sua geração
                    _dbContext.People.Attach(person);
                    model.CreateOrReopenProtocol(person, _appSettings.Value);
                    _dbContext.SaveChanges();

                    model.AddDocumentsAndUserData(person, _appSettings.Value);
                    person.CreateOrUpdateSeiUser(model.Password, _appSettings.Value);
                    person.EletronicSignatureStatus = EletronicSignatureStatus.UnderApproval;
                    user.EletronicSignatureStatus = EletronicSignatureStatus.UnderApproval;
                    var updateResult = _userManager.UpdateAsync(user).Result;
                    _dbContext.SaveChanges();
                    TempData["success"] = "Solicitação enviada com sucesso.";
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(model);
                }
                catch (System.IO.FileLoadException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(model);
                }

                return Redirect("/");
            }

            return View(model);
        }

        public ActionResult Term()
        {
            Person person = GetPerson();
            return View(person);
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

        private void AddLocalizedErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                var localizedError = error.Description;

                // password errors
                localizedError = localizedError.Replace("Incorrect password.", "Senha incorreta.");
                localizedError = localizedError.Replace("Your password has been changed.", "Senha incorreta.");
                localizedError = localizedError.Replace("Passwords must have at least one lowercase ('a'-'z').", "A senha deve ter pelo menos uma letra minúscula.");
                localizedError = localizedError.Replace("Passwords must have at least one uppercase ('A'-'Z').", "A senha deve ter pelo menos uma letra maiúscula.");
                localizedError = localizedError.Replace("Passwords must have at least one digit ('0'-'9').", "A senha deve ter pelo menos um dígito.");
                localizedError = localizedError.Replace("Phone number is invalid.", "Número de telefone inválido.");

                ModelState.AddModelError(string.Empty, localizedError);
            }
        }

        // TODO: Remover código duplicado com AccountsController
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

        private Person GetPerson()
        {
            var person = _dbContext.People.Include(p => p.Address).Single(p => p.CPF == User.Identity.Name);
            person.Phones = _dbContext.Phones.Where(p => p.Document == User.Identity.Name).ToList();
            return person;
        }
    }
}
