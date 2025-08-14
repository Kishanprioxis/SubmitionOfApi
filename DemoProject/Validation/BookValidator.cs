using FluentValidation;
using Models.RequestModel;

namespace DemoProject.Validation;

public class BookValidator : AbstractValidator<BookRequestModel>
{
    public BookValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required").Empty().
            WithMessage("Title cannot be empty").MaximumLength(20).WithMessage("Title must not exceed 20 characters");
        RuleFor(x => x.Author).NotEmpty().WithMessage("Author is required").Empty().WithMessage("Author cannot be empty");
        RuleFor(x => x.PublishedYear).NotEmpty().WithMessage("PublishedYear is required").Empty().WithMessage("PublishedYear cannot be empty");
        RuleFor(x => x.Isbn).NotEmpty().WithMessage("Isbn is required").Empty().WithMessage("Isbn cannot be empty");
    }
}