using FPassWordManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FPassWordManager.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Credential> Credentials { get; set; }
        public DbSet<CredentialAccess> CredentialsAccesses { get; set; }
        public DbSet<CreditDebitCard> CreditCards { get; set; }
        public DbSet<CreditDebitCardAccess> CreditDebitCardAccesses { get; set; }
        public DbSet<CreditDebitCardHistory> CreditCardsHistorys { get; set; }
        public DbSet<SecurityKey> SecurityKeys { get; set; }
        public DbSet<SecurityKeyAccess> SecurityKeyAccesses { get; set; }
        public DbSet<SecurityKeyHistory> SecurityKeysHistorys { get; set; }
        public DbSet<WebCredential> WebCredentials { get; set; }
        public DbSet<WebCredentialAccess> WebCredentialAccesses { get; set; }
        public DbSet<WebCredentialHistory> WebCredentialsHistorys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Credential ──────────────────────────────────────────
            modelBuilder.Entity<Credential>()
                .HasOne(c => c.User)
                .WithMany(u => u.Credentials)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade); // owner deletes → credentials deleted

            // ── CredentialAccess ────────────────────────────────────
            modelBuilder.Entity<CredentialAccess>()
                .HasOne(ca => ca.Credential)
                .WithMany(c => c.CredentialAccesses)
                .HasForeignKey(ca => ca.CredentialId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CredentialAccess>()
                .HasOne(ca => ca.User)
                .WithMany()
                .HasForeignKey(ca => ca.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CredentialAccess>()
                .HasOne(ca => ca.SharedByUser)
                .WithMany()
                .HasForeignKey(ca => ca.SharedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── CreditDebitCard ─────────────────────────────────────
            modelBuilder.Entity<CreditDebitCard>()
                .HasOne(c => c.Credential)
                .WithMany()
                .HasForeignKey(c => c.CredentialId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CreditDebitCard>()
                .HasOne(c => c.Creator)
                .WithMany()
                .HasForeignKey(c => c.CreatorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CreditDebitCard>()
                .HasOne(c => c.Editor)
                .WithMany()
                .HasForeignKey(c => c.EditorId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── CreditDebitCardAccess ───────────────────────────────
            modelBuilder.Entity<CreditDebitCardAccess>()
                .HasOne(ca => ca.CreditDebitCard)
                .WithMany(c => c.Accesses)
                .HasForeignKey(ca => ca.CreditDebitId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CreditDebitCardAccess>()
                .HasOne(ca => ca.User)
                .WithMany()
                .HasForeignKey(ca => ca.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CreditDebitCardAccess>()
                .HasOne(ca => ca.SharedByUser)
                .WithMany()
                .HasForeignKey(ca => ca.SharedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── CreditDebitCardHistory ──────────────────────────────
            modelBuilder.Entity<CreditDebitCardHistory>()
                .HasOne(h => h.CreditDebitCard)
                .WithMany()
                .HasForeignKey(h => h.CreditDebitId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CreditDebitCardHistory>()
                .HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── SecurityKey ─────────────────────────────────────────
            modelBuilder.Entity<SecurityKey>()
                .HasOne(s => s.Credential)
                .WithMany()
                .HasForeignKey(s => s.CredentialId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SecurityKey>()
                .HasOne(s => s.Creator)
                .WithMany()
                .HasForeignKey(s => s.CreatorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SecurityKey>()
                .HasOne(s => s.Editor)
                .WithMany()
                .HasForeignKey(s => s.EditorId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── SecurityKeyAccess ───────────────────────────────────
            modelBuilder.Entity<SecurityKeyAccess>()
                .HasOne(sa => sa.SecurityKey)
                .WithMany(s => s.Accesses)
                .HasForeignKey(sa => sa.SecurityKeyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SecurityKeyAccess>()
                .HasOne(sa => sa.User)
                .WithMany()
                .HasForeignKey(sa => sa.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SecurityKeyAccess>()
                .HasOne(sa => sa.SharedByUser)
                .WithMany()
                .HasForeignKey(sa => sa.SharedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── SecurityKeyHistory ──────────────────────────────────
            modelBuilder.Entity<SecurityKeyHistory>()
                .HasOne(h => h.SecurityKey)
                .WithMany()
                .HasForeignKey(h => h.SecurityKeyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SecurityKeyHistory>()
                .HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── WebCredential ───────────────────────────────────────
            modelBuilder.Entity<WebCredential>()
                .HasOne(w => w.Credential)
                .WithMany()
                .HasForeignKey(w => w.CredentialId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WebCredential>()
                .HasOne(w => w.Creator)
                .WithMany()
                .HasForeignKey(w => w.CreatorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WebCredential>()
                .HasOne(w => w.Editor)
                .WithMany()
                .HasForeignKey(w => w.EditorId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── WebCredentialAccess ─────────────────────────────────
            modelBuilder.Entity<WebCredentialAccess>()
                .HasOne(wa => wa.WebCredential)
                .WithMany(w => w.Accesses)
                .HasForeignKey(wa => wa.WebCredentialId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WebCredentialAccess>()
                .HasOne(wa => wa.User)
                .WithMany()
                .HasForeignKey(wa => wa.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WebCredentialAccess>()
                .HasOne(wa => wa.SharedByUser)
                .WithMany()
                .HasForeignKey(wa => wa.SharedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── WebCredentialHistory ────────────────────────────────
            modelBuilder.Entity<WebCredentialHistory>()
                .HasOne(h => h.WebCredential)
                .WithMany()
                .HasForeignKey(h => h.WebCredentialId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WebCredentialHistory>()
                .HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}