using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System;
using NuGet.Protocol.Plugins;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace P3AddNewFunctionalityDotNetCore.Models.ViewModels
{
    public class ProductViewModel
    {
        [BindNever]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "MissingName", ErrorMessageResourceType = typeof(Resources.ProductService))]
        public string Name { get; set; }

        public string Description { get; set; }

        public string Details { get; set; }

        [Required(ErrorMessageResourceName = "MissingStock", ErrorMessageResourceType = typeof(Resources.ProductService))]
        [RegularExpression(@"^\d+$", ErrorMessageResourceName = "StockNotAnInteger", ErrorMessageResourceType = typeof(Resources.ProductService))]
        [Range(1, int.MaxValue, ErrorMessageResourceName = "StockNotGreaterThanZero", ErrorMessageResourceType = typeof(Resources.ProductService))]
        public string Stock { get; set; }

        [Required(ErrorMessageResourceName = "MissingPrice", ErrorMessageResourceType = typeof(Resources.ProductService))]
        [RegularExpression(@"^\d+([.,]\d{1,2})?$", ErrorMessageResourceName = "PriceNotANumber", ErrorMessageResourceType = typeof(Resources.ProductService))]
        public string Price { get; set; }

        /// <summary>
        /// Property used only for the validation of the price numeric value.
        /// <para>
        /// Ensures that the "Price" string field accepts both periods and commas as decimal separators, and is a valid value greater than 0.01.
        /// </para>
        /// <para>
        /// Uniformity : "Price" is converted by replacing commas with periods. 
        /// </para>
        /// <para>
        /// Conversion : Tries to interpret the result string as a double using ".InvariantCulture" to separates the culture.
        /// </para>
        /// <para>
        /// Success : The numeric value is returned.
        /// </para>
        /// <para>
        /// Fail : Default value of 0 is returned, and will cause the "[Range]" validation to fail.
        /// </para>
        /// <remarks>
        /// Important : Independent property, not linked to DB, bypass complex validation of "Price" as a string.
        /// </remarks>
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessageResourceName = "PriceNotGreaterThanZero", ErrorMessageResourceType = typeof(Resources.ProductService))]
        public double PriceForValidation
        {
            get
            {
                var priceString = Price.Replace(',', '.');

                if (double.TryParse(priceString, NumberStyles.Any, CultureInfo.InvariantCulture, out double price))
                {
                    return price;
                }
                return 0;
            }
        }

    }
}
