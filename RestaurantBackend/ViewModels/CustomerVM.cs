using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class CustomerVM
{
    public int? CustomerId { get; set; } // Make it nullable for creation

    [Required, MaxLength(50)]
    public string FirstName { get; set; }

    [Required, MaxLength(50)]
    public string LastName { get; set; }

    [Required, EmailAddress]
    public string EmailAddress { get; set; }

    [Phone]
    public string PhoneNo { get; set; }
}
