using System;
using FluentValidation;

namespace Front.BuyCommand
{
    public class BuyRequestValidator : AbstractValidator<BuyRequest>
    {
        public BuyRequestValidator()
        {
            RuleFor(x => x.Amount).GreaterThan(0).LessThan(1000000000);
            RuleFor(x => x.PersonId).Must(BeNotAnEmptyGuid);
            RuleFor(x => x.Currency).IsInEnum();
        }

        private bool BeNotAnEmptyGuid(Guid arg) => !Guid.Empty.Equals(arg);
    }
}