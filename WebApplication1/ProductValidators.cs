namespace WebApplication1;

using FluentValidation;

public class ProductValidators : AbstractValidator<CreateProductDto> 
{ 
    public ProductValidators() 
    { 
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("The product name cannot be left blank.")
            .MaximumLength(100).WithMessage("The product name must be less than 100 characters long.");
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("<0");
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("not empty")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("A product code may only contain capital letters, numbers and hyphens");
        RuleFor(x => x.QuantityInStock)
            .GreaterThanOrEqualTo(0).WithMessage("=<0");
    } 
}
