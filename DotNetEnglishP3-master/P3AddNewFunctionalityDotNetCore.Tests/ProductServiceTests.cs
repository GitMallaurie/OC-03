using FluentAssertions;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class ProductServiceTests
    {
        private ProductViewModel CreateDefaultProduct(string name = "Default", string stock = "1", string price = "1")
        {
            return new ProductViewModel
            {
                Name = name,
                Stock = stock,
                Price = price
            };
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Name_WhenMissing_ShouldReturnErrorMessage(string MissingName)
        {
            // Arrange 
            var product = CreateDefaultProduct(name: MissingName);

            // Act
            var context = new ValidationContext(product);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(product, context, results, true);

            // Assert
            isValid.Should().BeFalse();
            results.Should().Contain(r => r.ErrorMessage == Resources.ProductService.MissingName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Price_WhenMissing_ShouldReturnErrorMessage(string MissingPrice)
        {
            // Arrange 
            var product = CreateDefaultProduct(price: MissingPrice);

            // Act
            var context = new ValidationContext(product);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(product, context, results, true);

            // Assert
            isValid.Should().BeFalse();
            results.Should().Contain(r => r.ErrorMessage == Resources.ProductService.MissingPrice);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Stock_WhenMissing_ShouldReturnErrorMessage(string MissingStock)
        {
            // Arrange 
            var product = CreateDefaultProduct(stock: MissingStock);

            // Act
            var context = new ValidationContext(product);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(product, context, results, true);

            // Assert
            isValid.Should().BeFalse();
            results.Should().Contain(r => r.ErrorMessage == Resources.ProductService.MissingStock);
        }

        [Theory]
        [InlineData("!@#$%^&*")]
        [InlineData("abc")]
        public void Price_WhenNotANumber_ShouldReturnErrorMessage(string PriceNotANumber)
        {
            // Arrange
            var product = CreateDefaultProduct(price: PriceNotANumber);

            // Act
            var context = new ValidationContext(product);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(product, context, results, true);

            // Assert
            isValid.Should().BeFalse();
            results.Should().Contain(r => r.ErrorMessage == Resources.ProductService.PriceNotANumber);
        }

        [Theory]
        [InlineData("!@#$%^&*")]
        [InlineData("abc")]
        public void Stock_WhenNotAnInteger_ShouldReturnErrorMessage(string StockNotAnInteger)
        {
            // Arrange
            var product = CreateDefaultProduct(stock: StockNotAnInteger);

            // Act
            var context = new ValidationContext(product);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(product, context, results, true);

            // Assert
            isValid.Should().BeFalse();
            results.Should().Contain(r => r.ErrorMessage == Resources.ProductService.StockNotAnInteger);
        }

        [Theory]
        [InlineData("-1")]
        [InlineData("0")]
        [InlineData("0,1")]
        [InlineData("0.1")]
        public void Stock_NotGreaterThanZero_ShouldReturnErrorMessage(string StockNotGreaterThanZero)
        {
            // Arrange
            var product = CreateDefaultProduct(stock: StockNotGreaterThanZero);

            // Act
            var context = new ValidationContext(product);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(product, context, results, true);

            // Assert
            isValid.Should().BeFalse();
            results.Should().Contain(r => r.ErrorMessage == Resources.ProductService.StockNotGreaterThanZero);
        }

        [Theory]
        [InlineData("-1")]
        [InlineData("0")]
        public void Price_NotGreaterThanZero_ShouldReturnErrorMessage(string PriceNotGreaterThanZero)
        {
            // Arrange
            var product = CreateDefaultProduct(price: PriceNotGreaterThanZero);

            // Act
            var context = new ValidationContext(product);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(product, context, results, true);

            // Assert
            isValid.Should().BeFalse();
            results.Should().Contain(r => r.ErrorMessage == Resources.ProductService.PriceNotGreaterThanZero);
        }

        [Theory]
        [InlineData("1,99", 1.99)]
        [InlineData("1.99", 1.99)]
        public void Price_WithDecimalSeparators_ShouldBeValid(string inputPrice, double expectedPrice)
        {
            // Arrange
            var product = CreateDefaultProduct(price: inputPrice);

            // Act
            var actualPrice = product.PriceForValidation;

            // Assert
            actualPrice.Should().Be(expectedPrice);
        }
    }
}