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
    public class PhonesController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISeiService _seiService;

        public PhonesController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, ISeiService seiService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _seiService = seiService;
        }

        // GET: Phones
        public ActionResult Index()
        {
            IQueryable<Phone> phones = phones = _dbContext.Phones.Where(p => p.Document == User.Identity.Name);

            return View(phones.ToList());
        }

        // GET: Phones/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            CheckPhoneProperty((int)id);
            Phone phone = _dbContext.Phones.FirstOrDefault(p => p.PhoneID == id);
            if (phone == null)
            {
                return NotFound();
            }

            return View(phone);
        }

        // GET: Phones/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Phones/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PhoneViewModel model)
        {
            SetUser(model.Phone);
            CheckDuplicates(model.Phone);
            SetTimesStamp(model.Phone);

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

            _dbContext.Phones.Add(model.Phone);
            _dbContext.SaveChanges();
            UpdatePhoneSei(model);
            return RedirectToAction("Index");
        }

        // GET: Phones/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            CheckPhoneProperty((int)id);
            Phone phone = _dbContext.Phones.FirstOrDefault(p => p.PhoneID == id);
            if (phone == null)
            {
                return NotFound();
            }

            var phoneViewModel = new PhoneViewModel
            {
                Phone = phone
            };

            return View(phoneViewModel);
        }

        // POST: Phones/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PhoneViewModel model)
        {
            CheckPhoneProperty(model.Phone.PhoneID);
            SetTimesStamp(model.Phone);
            SetUser(model.Phone);
            CheckDuplicates(model.Phone);

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

            _dbContext.Entry(model.Phone).State = EntityState.Modified;
            _dbContext.SaveChanges();
            UpdatePhoneSei(model);
            return RedirectToAction("Index");
        }

        // GET: Phones/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            CheckPhoneProperty((int)id);
            Phone phone = _dbContext.Phones.FirstOrDefault(p => p.PhoneID == id);
            if (phone == null)
            {
                return NotFound();
            }

            var phoneViewModel = new PhoneViewModel
            {
                Phone = phone
            };

            return View(phoneViewModel);
        }

        // POST: Phones/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(PhoneViewModel model)
        {
            SetUser(model.Phone);

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

            var id = model.Phone.PhoneID;

            CheckPhoneProperty(id);
            Phone phone = _dbContext.Phones.First(p => p.PhoneID == id);

            if (_dbContext.Phones.Count(p => p.Document == User.Identity.Name) > 1)
            {
                _dbContext.Phones.Remove(phone);
                _dbContext.SaveChanges();
                UpdatePhoneSei(model);
            }
            else
            {
                TempData["error"] = "Você deve ter pelo menos um telefone de contato cadastrado.";
                return View(model);
            }

            return RedirectToAction("Index");
        }

        private void CheckPhoneProperty(int phoneId)
        {
            var phone = _dbContext.Phones.AsNoTracking().Single(p => p.PhoneID == phoneId);
            if (phone.Document != User.Identity.Name)
            {
                throw new Exception("This phone is related to another user");
            }
        }

        private void SetUser(Phone phone)
        {
            phone.Document = User.Identity.Name;
            ModelState.Remove("Phone.Document");
        }

        private void SetTimesStamp(Phone phone)
        {
            phone.UpdatedAt = DateTime.Now;
            if (phone.CreatedAt == default(DateTime))
            {
                phone.CreatedAt = DateTime.Now;
            }
        }

        private void CheckDuplicates(Phone phone)
        {
            if (_dbContext.Phones.Any(p => p.Document == phone.Document && p.Number == phone.Number && p.PhoneID != phone.PhoneID))
            {
                ModelState.AddModelError("Phone.Number", "Número de telefone já cadastrado para o seu usuário.");
            }
        }

        /// <summary>
        /// Altera o telefone do usuário no SEI, utiliza sempre o telefone com alteração mais recente.
        /// </summary>
        private void UpdatePhoneSei(PhoneViewModel model)
        {
            Person person = _dbContext.People.Include(p => p.Address).Single(p => p.CPF == User.Identity.Name);
            person.Phones = _dbContext.Phones.Where(p => p.Document == person.CPF).ToList();

            if (person.SeiId != null)
            {
                _seiService.CreateOrUpdateSeiUser(person, model.Password);
            }
        }
    }
}