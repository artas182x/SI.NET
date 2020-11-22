using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using demo.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace demo.Domain
{
    [Bind("Name, Surname, Description, Country, Phone, Email, Age, Password, ConfirmPassword, File")]
    public class ExampleForm
    {
        
        
        [DisplayName("Imię")]
        [Required]
        [MinLength(length: 3)]
        [MaxLength(length: 10)]
        [Remote(action: "check-user-exists", controller: "validation", AdditionalFields = nameof(Surname))]
        public string Name { get; set; }
        
        [DisplayName("Nazwisko")]
        [Remote(action: "check-user-exists", controller: "validation", AdditionalFields = nameof(Name))]
        public string Surname { get; set; }

        [MinLength(length: 10)]
        [MaxLength(length: 50)]
        [DisplayName("Opis")]
        public string Description { get; set; }

        [DisplayName("Kraj")]
        [Remote(action: "check-country", controller: "validation")]
        [CustomValidation(typeof(CustomValidationMethods), method: "ValidateCountry")]
        public string  Country { get; set; }

        [Phone]
        public string Phone { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        
        [Range(minimum: 18, maximum:25)]
        public string Age { get; set; }
        
        [Compare("ConfirmPassword")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        
        public List<IFormFile> File { get; set; }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Surname)}: {Surname}, {nameof(Description)}: {Description}, {nameof(Country)}: {Country}, {nameof(Phone)}: {Phone}, {nameof(Email)}: {Email}, {nameof(Age)}: {Age}, {nameof(Password)}: {Password}, {nameof(ConfirmPassword)}: {ConfirmPassword}";
        }
    }
}