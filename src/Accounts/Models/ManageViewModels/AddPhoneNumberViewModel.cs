﻿namespace Accounts.Models.ManageViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;

    public class AddPhoneNumberViewModel
    {
        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }
    }
}
