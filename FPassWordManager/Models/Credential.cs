using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FPassWordManager.Models
{
    [PrimaryKey(nameof(CredentialId))]
    public class Credential
    {
        public Guid CredentialId { get; set; }
        [MaxLength(50)]
        public string CredentialName { get; set; } = string.Empty;
        //Foreign key to owner
        public Guid UserId { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime LastEditedAt { get; set; }
     
        //Navigation
        public User User { get; set; }
        
        public ICollection<CredentialAccess> CredentialAccesses { get; set; } = new List<CredentialAccess>();    

    }
}
