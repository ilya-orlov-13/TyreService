using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TyreServiceApp.Models
{
    public class Post
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostId { get; set; }

        [Required(ErrorMessage = "Название поста обязательно")]
        [StringLength(100, ErrorMessage = "Название не более 100 символов")]
        [Display(Name = "Пост")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Заблокирован")]
        public bool IsLocked { get; set; }

        public virtual ICollection<PostActiveSession>? ActiveSessions { get; set; }
    }
}
