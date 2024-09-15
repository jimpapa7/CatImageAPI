using FluentValidation;

public class TagValidator : AbstractValidator<TagEntity>
{
    public TagValidator()
    {
        RuleFor(t => t.Name).NotEmpty().WithMessage("Tag name is required.");
    }
}
