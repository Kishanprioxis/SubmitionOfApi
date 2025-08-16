using FluentValidation;
using Models.RequestModel;

namespace DemoProject.Validation;

public class BookValidator : AbstractValidator<BookRequestModel>
{
    public BookValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required").MaximumLength(20).WithMessage("Title must not exceed 20 characters");
        RuleFor(x => x.Author).NotEmpty().WithMessage("Author is required").NotEmpty().WithMessage("Author cannot be empty");
        RuleFor(x => x.PublishedYear)
            .NotEmpty().WithMessage("PublishedYear is required")
            .NotNull().WithMessage("PublishedYear cannot be null")
            .Must(y => y > 0).WithMessage("PublishedYear must be a valid positive number")
            .LessThanOrEqualTo(DateTime.Now.Year).WithMessage("PublishedYear cannot be in the future");

        RuleFor(x => x.Isbn).NotEmpty().WithMessage("Isbn is required").NotEmpty().WithMessage("Isbn cannot be empty");
    }
}

    

