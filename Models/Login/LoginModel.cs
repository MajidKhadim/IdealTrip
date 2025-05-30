﻿using System.ComponentModel.DataAnnotations;

namespace IdealTrip.Models.Login
{
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
