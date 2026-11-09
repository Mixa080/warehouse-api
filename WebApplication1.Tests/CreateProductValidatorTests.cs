using FluentAssertions;

namespace WebApplication1.Tests;

public class CreateProductValidatorTests
{
    private readonly ProductValidators _validators = new();

    [Fact]
    public void Validate_ShouldPass_WhenProductIsValid()
    {
        var dto = new CreateProductDto(
            Name: "Test Product",
            Sku: "WH-1234",
            Price: 99.99m,
            QuantityInStock: 10
            
        );
        var result = _validators.Validate(dto);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ShouldFail_WhenNameIsEmpty(string invalidName)
    {
        var dto = new CreateProductDto(
            Name: invalidName,
            Sku: "WH-1234",
            Price: 99.99m,
            QuantityInStock: 10
            
        );
        var result = _validators.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }
    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_ShouldFail_WhenPriceIsZeroOrNegative(decimal invalidPrice)    {
        var dto = new CreateProductDto(
            Name: "Valid Name",
            Sku: "WH-1234",
            Price: invalidPrice,
            QuantityInStock: 10
            
        );
        var result = _validators.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }
}