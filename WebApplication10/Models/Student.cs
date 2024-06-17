using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
namespace WebApplication10.Models

{
    public class Student
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        //public required string StudentID { get; set; } = Guid.NewGuid().ToString();
        //[Required]
        //public required string Name { get; set; }
        //[Required]
        //public required string Surname { get; set; }
        //[Required]
        //public required string EmailAddress { get; set; }
        //public required string MobileNumber { get; set; }
        //public bool IsActive { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string id { get; set; } = Guid.NewGuid().ToString();
        [JsonProperty ( PropertyName ="studentId")]
       public string StudentID { get; set; } 

        [Required(ErrorMessage = "Name is required")]
        [JsonProperty(PropertyName = "name")]
        public string  Name { get; set; }

        [Required(ErrorMessage = "Surname is required")]
        [JsonProperty(PropertyName = "surname")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Email Address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [JsonProperty(PropertyName = "email")]
        public string EmailAddress { get; set; }

        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Mobile Number must contain only digits")]
        [JsonProperty(PropertyName = "mobile_number")]
        public string MobileNumber { get; set; }
        [JsonProperty(PropertyName = "active")]
        public bool IsActive { get; set; }

        



    }


}


