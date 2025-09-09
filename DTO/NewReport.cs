using System.ComponentModel.DataAnnotations;

namespace renjibackend.DTO
{
    public class NewReport
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public int AccidentTypeId { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Location { get; set; }
    }
}
