﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P3AddNewFunctionalityDotNetCore.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILanguageService _languageService;

        public ProductController(IProductService productService, ILanguageService languageService)
        {
            _productService = productService;
            _languageService = languageService;
        }

        public IActionResult Index()
        {
            IEnumerable<ProductViewModel> products = _productService.GetAllProductsViewModel();
            return View(products);
        }

        [Authorize]
        public IActionResult Admin()
        {
            return View(_productService.GetAllProductsViewModel().OrderByDescending(p => p.Id));
        }

        [Authorize]
        public ViewResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(ProductViewModel product)
        {
            product.Price = product.Price.Replace(',', '.');

            if (ModelState.IsValid)
            {
                await _productService.SaveProduct(product);
                return RedirectToAction("Admin");
            }
            else
            {
                // Check if there is PriceForValidation errors
                if (ModelState.TryGetValue("PriceForValidation", out var priceForValidationResults) && priceForValidationResults.Errors.Count > 0)
                {
                    // Transfert errors from PriceForValidation to Price
                    foreach (var error in priceForValidationResults.Errors)
                    {
                        ModelState.AddModelError("Price", error.ErrorMessage);
                    }
                }
                return View(product);
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            _productService.DeleteProduct(id);
            return RedirectToAction("Admin");
        }
    }
}