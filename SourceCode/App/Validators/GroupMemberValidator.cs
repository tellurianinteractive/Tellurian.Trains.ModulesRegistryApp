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
                .MustBeSelected(localizer)
                .WithName(n => localizer[nameof(n.Group)]);

            RuleFor(gm => gm.PersonId)
                .MustBeSelected(localizer)
                .WithName(n => localizer[nameof(n.Person)]);
        }
    }
}
