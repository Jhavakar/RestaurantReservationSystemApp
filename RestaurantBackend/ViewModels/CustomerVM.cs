using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class CustomerVM
{
    // Use string for Id; Identity typically handles IDs as strings
    // public string Id { get; set; }

    [Required, MaxLength(50)]
    public string FirstName { get; set; }

    [Required, MaxLength(50)]
    public string LastName { get; set; }

    [Required, EmailAddress]
    public string EmailAddress { get; set; }

    [Phone]
    public string PhoneNumber { get; set; }

    // Add a Password field if this VM is used for creating new users
    [Required, DataType(DataType.Password)]
    public string Password { get; set; }
}
