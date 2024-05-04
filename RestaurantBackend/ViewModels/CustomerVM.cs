using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class CustomerVM
{
    // Use string for Id; Identity typically handles IDs as strings
    // public string Id { get; set; }

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string EmailAddress { get; set; } = string.Empty;

    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    // Add a Password field if this VM is used for creating new users
    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
