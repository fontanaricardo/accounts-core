namespace Accounts.Controllers
{
    using System.Linq;
    using Data;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Services;

    [Authorize]
    public class PeopleController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISeiService _seiService;

        public PeopleController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, ISeiService seiService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _seiService = seiService;
        }

        // GET: People/Edit/5
        public ActionResult Edit()
        {
            var person = _dbContext.People.Single(p => p.CPF.ToString() == User.Identity.Name);

            PersonViewModel personViewModel = new PersonViewModel()
            {
                CPF = person.CPF,
                Name = person.Name,
                RG = person.RG,
                Dispatcher = person.Dispatcher
            };

            return View(personViewModel);
        }

        // POST: People/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PersonViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Person oldPerson = _dbContext.People.AsNoTracking().Single(p => p.CPF == User.Identity.Name);

            // Busca todos os dados do usuário para enviar novamente ao serviço do SEI
            Person person = _dbContext.People.Include(p => p.Address).Single(p => p.CPF == User.Identity.Name);
            person.Phones = _dbContext.Phones.Where(p => p.Document == person.CPF).ToList();

            // Verifica se a senha está correta
            var user = _userManager.FindByNameAsync(User.Identity.Name).Result;
            if (!_userManager.CheckPasswordAsync(user, model.Password).Result)
            {
                ModelState.AddModelError("Password", "Senha incorreta.");
                return View(model);
            }

            person.Name = model.Name;
            person.RG = model.RG;
            person.Dispatcher = model.Dispatcher;

            // Revoga a assinatura eletrônica do usuário
            _seiService.ChangePasswordSei(person, model.Password, true);

            // Atualiza dados na tabela de usuário e no SEI
            user.FullUserName = model.Name;

            // TODO: Alterar o nome na Claim
            var updateResult = _userManager.UpdateAsync(user).Result;

            if (updateResult.Succeeded)
            {
                _dbContext.Entry(person).State = EntityState.Modified;
                _dbContext.SaveChanges();

                // Atualiza demais dados no SEI
                if (person.SeiId != null)
                {
                    _seiService.CreateOrUpdateSeiUser(person, model.Password);

                    // Remove telefones e endereço pois, já foram enviados ao SEI, não foram alterados e não serão comparados
                    oldPerson.Phones = null;
                    person.Phones = null;
                    oldPerson.Address = null;
                    person.Address = null;

                    var changeDocument = EletronicSignatureViewModel.UserDataChangeDocument(
                        oldPerson,
                        person);

                    _seiService.AddTextDocument(
                        protocol: person.SeiProtocol,
                        title: "Alteração nos dados do usuário",
                        content: changeDocument);
                }
            }

            return RedirectToAction("Index", "Manage");
        }
    }
}