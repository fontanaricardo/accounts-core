namespace Accounts.Controllers
{
    using System;
    using System.Linq;
    using Data;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Services;

    [Authorize]
    public class CertificationController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISeiService _seiService;

        public CertificationController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, ISeiService seiService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _seiService = seiService;
        }

        public ActionResult AddDocument()
        {
            Person person = GetPerson();

            ActionResult redirect = null;
            redirect = CheckEletronicSignatureStatus(person, redirect);
            if (redirect != null)
            {
                return redirect;
            }

            return View();
        }

        [ActionName("AddDocument")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDocument(AddDocumentViewModel model)
        {
            Person person = GetPerson();

            ActionResult redirect = null;
            redirect = CheckEletronicSignatureStatus(person, redirect);
            if (redirect != null)
            {
                return redirect;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _seiService.AddDocument(person.SeiProtocol, "Documento", model.Document);
                    TempData["success"] = "Documento adicionado com sucesso.";
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

        public ActionResult Status()
        {
            return Json(UpdateEletronicSignature());
        }

        private ActionResult CheckEletronicSignatureStatus(Person person, ActionResult redirect)
        {
            if (person.EletronicSignatureStatus == EletronicSignatureStatus.Unsolicited)
            {
                redirect = RedirectToAction("EletronicSignature", "Manage");
            }

            if (person.EletronicSignatureStatus == EletronicSignatureStatus.Approved)
            {
                TempData["success"] = "Assinatura eletrônica já aprovada.";
                redirect = Redirect("~");
            }

            return redirect;
        }

        private Person GetPerson()
        {
            var person = _dbContext.People.Include(p => p.Address).Single(p => p.CPF == User.Identity.Name);
            person.Phones = _dbContext.Phones.Where(p => p.Document == User.Identity.Name).ToList();
            return person;
        }

        // TODO: Ver uma maneira de atualizar a assinatura sem impactar na performance da aplicação. Executado desta forma apenas para cumprir o milestone.
        // TODO: Implementar método para pessoa jurídica
        private EletronicSignatureStatus UpdateEletronicSignature()
        {
            EletronicSignatureStatus status = EletronicSignatureStatus.Unsolicited;

            if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
            {
                string document = User.Identity.Name;

                var person = _dbContext.People.SingleOrDefault(p => p.CPF == document);

                if (person != null)
                {
                    _seiService.UpdateEletronicSignatureStatus(person);
                    var user = _userManager.FindByNameAsync(document).Result;

                    if (user.EletronicSignatureStatus != person.EletronicSignatureStatus)
                    {
                        user.EletronicSignatureStatus = person.EletronicSignatureStatus;
                        var updateResult = _userManager.UpdateAsync(user).Result;
                        _dbContext.SaveChanges();
                    }

                    status = person.EletronicSignatureStatus;
                }
            }

            return status;
        }
    }
}