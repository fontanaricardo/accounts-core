namespace Accounts.Controllers
{
    using System.Linq;
    using Data;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Models;
    using Services;

    [Authorize]
    public class AddressesController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOptions<AppSettings> _appSettings;

        public AddressesController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, IOptions<AppSettings> appSettings)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _appSettings = appSettings;
        }

        // GET: Addresses/Edit/5
        public ActionResult Edit()
        {
            Address address;

            if (User.Identity.Name.Length > 11)
            {
                address = _dbContext.Companies.Include(c => c.Address).Single(c => c.CNPJ.ToString() == User.Identity.Name).Address;
            }
            else
            {
                address = _dbContext.People.Include(p => p.Address).Single(c => c.CPF.ToString() == User.Identity.Name).Address;
            }

            if (address == null)
            {
                return NotFound();
            }

            var addressViewModel = new AddressViewModel
            {
                Address = address
            };

            return View(addressViewModel);
        }

        // POST: Addresses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AddressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verifica se a senha está correta
            var user = _userManager.FindByNameAsync(User.Identity.Name).Result;
            if (!_userManager.CheckPasswordAsync(user, model.Password).Result)
            {
                ModelState.AddModelError("Password", "Senha incorreta.");
                return View(model);
            }

            // Atualiza o endereço no accounts
            _dbContext.Entry(model.Address).State = EntityState.Modified;
            _dbContext.SaveChanges();

            // Busca todos os dados do usuário para enviar novamente ao serviço do SEI
            Person person = _dbContext.People.Include(p => p.Address).Single(p => p.CPF == User.Identity.Name);
            person.Phones = _dbContext.Phones.Where(p => p.Document == person.CPF).ToList();

            if (person.SeiId != null)
            {
                person.CreateOrUpdateSeiUser(model.Password, _appSettings.Value);
            }

            TempData["success"] = "Registro atualizado com sucesso";

            return View(model);
        }
    }
}