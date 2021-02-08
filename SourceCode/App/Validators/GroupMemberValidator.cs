using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators
{
    public class GroupMemberValidator : AbstractValidator<GroupMember>
    {
        public GroupMemberValidator(IStringLocalizer<App> localizer)
        {
            RuleFor(gm => gm.GroupId)
                .GreaterThan(0)
                .WithName(n => localizer[nameof(n.Group)])
                .WithMessage(gm => localizer["IsRequired"]);

            RuleFor(gm => gm.PersonId)
               .GreaterThan(0)
               .WithName(n => localizer[nameof(n.Person)])
               .WithMessage(gm => localizer["IsRequired"]);
        }
    }
}
