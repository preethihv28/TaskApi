using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskApi.Models.DTOs
{
    public class CreateToDoItemDto
    {
        [Required]
        [MaxLength(20)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public bool IsCompleted { get; set; }
    }

}
