using System.ComponentModel.DataAnnotations;


namespace renjibackend.DTO
{

    // get, is the getter (it makes the property to be Read-only)
    // set, is the setter (it makes the property to be Write-only)
    // assigning an initial empty value on each property will prevent null reference warnings
    // also it avoids runtime NullReferenceException if let say one of the property from the client is empty
    public class RegisterRequest
    {
        [Required]
        public string Firstname { get; set; }

        [Required]     
        public string Lastname { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public int DepartmentId { get; set; }
    }

} 