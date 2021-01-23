using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Data.Validators
{
    public class UserValidator : AbstractValidator<User> 
    {
        public UserValidator()
        {
            RuleFor(user => user.EmailAddress).MinimumLength(5).MaximumLength(50).EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);
        }
    }

    public class PersonValidator : AbstractValidator<Person>
    {
        public PersonValidator()
        {
            RuleFor(person => person.FirstName).MinimumLength(1).MaximumLength(50).NameIsCapitalizedCorrectly();
            RuleFor(person => person.MiddleName).MaximumLength(50).NameIsCapitalizedCorrectly();
            RuleFor(person => person.LastName).MinimumLength(1).MaximumLength(50).NameIsCapitalizedCorrectly();
            RuleFor(person => person.CityName).MinimumLength(1).MaximumLength(50).NameIsCapitalizedCorrectly();
            RuleFor(person => person.EmailAddresses).MinimumLength(5).MaximumLength(50).EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);
        }
    }
}
