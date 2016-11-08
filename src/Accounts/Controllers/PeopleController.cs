using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Accounts.Data;
using Microsoft.AspNetCore.Identity;
using Accounts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Accounts.Services;
using System.Security.Claims;

namespace Accounts.Controllers
{
    public class PeopleController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOptions<AppSettings> _appSettings;

        public PeopleController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, IOptions<AppSettings> appSettings)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _appSettings = appSettings;
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
        public async Task<ActionResult> Edit(PersonViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

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
            person.CPF = model.CPF;

            //Revoga a assinatura eletrônica do usuário
            person.ChangePasswordSei(model.Password, _appSettings.Value, true);

            // Atualiza dados na tabela de usuário e no SEI
            user.FullUserName = model.Name;

            // TODO: Alterar o nome na Claim

            var updateResult = _userManager.UpdateAsync(user).Result;

            if (updateResult.Succeeded)
            {
                _dbContext.Entry(person).State = EntityState.Modified;
                _dbContext.SaveChanges();

                // Atualiza demais dados no SEI
                if (person.SeiId != null) person.CreateOrUpdateSeiUser(model.Password, _appSettings.Value);
            }

            return RedirectToAction("Index", "Manage");
        }

    }
}