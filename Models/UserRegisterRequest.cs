﻿using System.ComponentModel.DataAnnotations;

namespace VerifyEmailForgotPasswordTutorial.Models
{
    public class UserRegisterRequest
    {
        [Required,EmailAddress]
        public string Email { get; set; }=string.Empty;

        [Required,MinLength(6, ErrorMessage = "Please add at least 6 character, dude!")]
        public string Password { get; set; } = string.Empty;

        [Required,Compare("Password", ErrorMessage ="Itsn't same, dude!")]  
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
