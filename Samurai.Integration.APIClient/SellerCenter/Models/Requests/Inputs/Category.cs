using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs
{
    public class Category
    {
        [Display(Name = "Categoria")]
        public Guid CategoryId { get; set; }
    }
}
