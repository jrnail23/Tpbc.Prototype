using System.ComponentModel.DataAnnotations;

namespace Tpbc.Web.Areas.Admin.Models
{
    public class MemberModel
    {
        [Required]
        [Display(Name = "UserName")]
        [Editable(allowEdit: false)]
        [Key]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }
    }
}