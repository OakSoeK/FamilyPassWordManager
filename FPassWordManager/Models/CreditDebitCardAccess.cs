using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.Models
{
    [PrimaryKey(nameof(CreditDebitAccessId))]
    public class CreditDebitCardAccess
    {
        public Guid CreditDebitAccessId { get; set; }

        public Guid CreditDebitId { get; set; }

        public Guid UserId { get; set; }
        [MaxLength(50)]
        public string PermissionLevel { get; set; } = string.Empty;
        public DateTime ExpireAt { get; set; }
        public DateTime SharedAt { get; set; }

        public Guid SharedByUserId { get; set; }

        //Navigation
        public CreditDebitCard CreditDebitCard { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public User SharedByUser { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public User User { get; set; }
    }

}
