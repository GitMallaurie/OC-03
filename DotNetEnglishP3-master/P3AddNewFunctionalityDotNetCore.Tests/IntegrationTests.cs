using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Data;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class IntegrationTests : IDisposable
    {
        #region Settings

        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly P3Referential _tests_P3ReferentialContext;

        public IntegrationTests()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_tests.json")
                .Build();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging(configure => configure.AddConsole());

            services.AddDbContext<P3Referential>(options =>
                options.UseSqlServer(configuration.GetConnectionString("P3Referential")));
            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("P3Identity")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>();

            _serviceProvider = services.BuildServiceProvider();

            _tests_P3ReferentialContext = _serviceProvider.GetRequiredService<P3Referential>();
            _userManager = _serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            InitializeTestData();
        }
         
        public async Task InitializeTestData()
        {
            _tests_P3ReferentialContext.Database.EnsureDeleted();
            _tests_P3ReferentialContext.Database.EnsureCreated();

            _tests_P3ReferentialContext.Product.AddRange(new Product { Name = "TestProduct", Price = 100, Quantity = 50 });
            _tests_P3ReferentialContext.SaveChanges();

            await EnsureAdminUser();
        }

        private async Task EnsureAdminUser()
        {
            var user = await _userManager.FindByNameAsync(IdentitySeedData.AdminUser);
            if (user == null)
            {
                user = new IdentityUser { UserName = IdentitySeedData.AdminUser };
                await _userManager.CreateAsync(user, IdentitySeedData.AdminPassword);
            }
        }

        public void Dispose()
        {
            _tests_P3ReferentialContext.Database.EnsureDeleted();
            (_serviceProvider as IDisposable)?.Dispose();
        }

        #endregion

        #region Tests

        private ProductController setupProductControllerWithMockedServices()
        {
            Mock<ILanguageService> _mockLanguageService = new();
            var productService = new ProductService(new Cart(), new ProductRepository(_tests_P3ReferentialContext), new OrderRepository(_tests_P3ReferentialContext), new Mock<IStringLocalizer<ProductService>>().Object);
            var productController = new ProductController(productService, _mockLanguageService.Object);
            return productController;
        }  

        private ProductViewModel CreateDefaultProduct(string name = "Test Product", string price = "1.00", string stock = "1337", string description = "Just another test product", string details = "Nothing much")
        {
            return new ProductViewModel
            {
                Name = name,
                Price = price,
                Stock = stock,
                Description = description,
                Details = details
            };
        }

        [Fact]
        public async Task CreateProduct_AddProductToDatabase()
        {
            // Arrange
            var productController = setupProductControllerWithMockedServices();
            var newProduct = CreateDefaultProduct();

            // Act
            await productController.Create(newProduct);

            // Assert
            var createdProduct = _tests_P3ReferentialContext.Product.FirstOrDefault(p => p.Name == newProduct.Name);
            Assert.NotNull(createdProduct);
            Assert.Equal(newProduct.Name, createdProduct.Name);
        }

        [Fact]
        public async Task DeleteProduct_DeleteProductFromDatabase()
        {
            // Arrange
            var productController = setupProductControllerWithMockedServices();
            var newProduct = CreateDefaultProduct();
            await productController.Create(newProduct);
            var createdProduct = _tests_P3ReferentialContext.Product.FirstOrDefault(p => p.Name == newProduct.Name);

            // Act
            productController.DeleteProduct(createdProduct.Id); 

            // Assert
            var deletedProduct = _tests_P3ReferentialContext.Product.FirstOrDefault(p => p.Id == createdProduct.Id);
            Assert.Null(deletedProduct); 
        }

        [Fact]
        public async Task NewProduct_DeletedByAdmin_ShouldBeDeletedForUser()
        {
            // Arrange
            var productController = setupProductControllerWithMockedServices();
            var testProduct = CreateDefaultProduct();

            // Act
            await productController.Create(testProduct);
            var createdProduct = _tests_P3ReferentialContext.Product.FirstOrDefault(p => p.Name == testProduct.Name);
            productController.DeleteProduct(createdProduct.Id);

            // Assert 
            var result = productController.Index() as ViewResult;
            var products = result.Model as List<ProductViewModel>;
            Assert.Null(products.FirstOrDefault(p => p.Id == testProduct.Id));
        }

        [Fact]
        public async Task WhenAdminDeletesAProduct_TheProductIsRemovedForUsers()
        {
            // Arrange
            var productController = setupProductControllerWithMockedServices();
            var productToBeDeleted = CreateDefaultProduct(name: "Definitely not the one");
            var productToBeKept = CreateDefaultProduct(name: "The one");
            await productController.Create(productToBeDeleted);
            await productController.Create(productToBeKept);

            // Act
            Product productToDelete = _tests_P3ReferentialContext.Product.FirstOrDefault(p => p.Name == productToBeDeleted.Name);
            Product productToKeep = _tests_P3ReferentialContext.Product.FirstOrDefault(p => p.Name == productToBeKept.Name);
            var redirectResult = productController.DeleteProduct(productToDelete.Id) as RedirectToActionResult;

            // Assert
            Assert.NotNull(redirectResult);
            Assert.Equal("Admin", redirectResult.ActionName);
            Assert.Null(_tests_P3ReferentialContext.Product.FirstOrDefault(p => p.Id == productToDelete.Id));
            Assert.NotNull(_tests_P3ReferentialContext.Product.FirstOrDefault(p => p.Id == productToKeep.Id));
        }

        [Fact]
        public async Task NewProduct_AlreadyAdded_ShouldBeUpdated()
        {
            // Arrange
            var productController = setupProductControllerWithMockedServices();
            var testProduct = CreateDefaultProduct();
            var sameTestProduct = CreateDefaultProduct(stock : "100");

            // Act
            await productController.Create(testProduct);
            await productController.Create(sameTestProduct);

            // Assert
            var updatedProduct = _tests_P3ReferentialContext.Product.Where(p => p.Name == testProduct.Name).ToList();
            Assert.Single(updatedProduct);
            Assert.Equal(1437, updatedProduct[0].Quantity);
        }

        #endregion
    }
}