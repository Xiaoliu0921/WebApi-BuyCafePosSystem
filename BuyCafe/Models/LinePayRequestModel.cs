using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BuyCafe.Models
{
    // Required Request Body
    public class LinePayRequestDto
    {
        public int amount { get; set; }

        public string currency { get; } = "TWD";

        public string orderId { get; set; }

        public List<PackageDto> packages { get; set; }

        public RedirectUrlsDto redirectUrls { get; set; }
    }

    public class PackageDto
    {
        public string id { get; set; } // Category Name
        public int amount { get; set; }
        public List<ProductDto> products { get; set; }
    }

    public class RedirectUrlsDto
    {
        public string confirmUrl { get; set; }
        public string cancelUrl { get; set; }
    }

    public class ProductDto
    {
        public string name { get; set; }
        public int quantity { get; set; }
        public int price { get; set; }
    }

    // Confirm Request
    public class ConfirmRequestDto
    {
        public long transactionId { get; set; }
        public int amount { get; set; }
        public string currency { get; } = "TWD";
    }
}