using System.ComponentModel.DataAnnotations;

namespace Customers.WebApi.Models
{
    public class Customer
    {
        public Guid CustomerId { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
