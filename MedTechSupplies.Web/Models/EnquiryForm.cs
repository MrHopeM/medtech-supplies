using System.ComponentModel.DataAnnotations;

namespace MedTechSupplies.Web.Models;

public class EnquiryForm
{
    [Required(ErrorMessage = "Please enter your name.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter an email so we can reply.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string Type { get; set; } = "Clinic";
    public string? Message { get; set; }
}
