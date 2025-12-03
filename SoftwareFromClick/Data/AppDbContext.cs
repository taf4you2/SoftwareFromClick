using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SoftwareFromClick.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SoftwareFromClick.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ApiKeys> ApiKeys { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<AiModel> AiModels { get; set; }
        public DbSet<Question> Queries { get; set; }
        public DbSet<Prompt> Prompts { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<EditedResult> EditedResults { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<TemplateCategories> TemplateCategories { get; set; }
        public DbSet<PromptTemplate> PromptTemplates { get; set; }
        public DbSet<PromptTemplateUsed> PromptTemplatesUsed { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Nazwa pliku bazy danych to "softwarefromclick.db"
            optionsBuilder.UseSqlite("Data Source=softwarefromclick.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Użytkownik (User) ---

            // User -> ApiKeys (Jeden użytkownik, wiele kluczy)
            modelBuilder.Entity<ApiKeys>()
                .HasOne(a => a.User)
                .WithMany(u => u.ApiKeys)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Usunięcie usera usuwa jego klucze

            // User -> Queries (Jeden użytkownik, wiele zapytań)
            modelBuilder.Entity<Question>()
                .HasOne(q => q.User)
                .WithMany(u => u.Queries)
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Sessions (Jeden użytkownik, wiele sesji)
            modelBuilder.Entity<Session>()
                .HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> EditedResults (Jeden użytkownik może edytować wiele wyników)
            modelBuilder.Entity<EditedResult>()
                .HasOne(er => er.Editor)
                .WithMany(u => u.EditedResults)
                .HasForeignKey(er => er.EditedBy)
                .OnDelete(DeleteBehavior.Restrict); // nie usuwamy historii edycji

            // --- Dostawcy (Provider) ---

            // Provider -> ApiKeys (Jeden dostawca, wiele kluczy)
            modelBuilder.Entity<ApiKeys>()
                .HasOne(a => a.Provider)
                .WithMany(p => p.ApiKeys)
                .HasForeignKey(a => a.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Provider -> Models (Jeden dostawca, wiele modeli AI)
            modelBuilder.Entity<AiModel>()
                .HasOne(m => m.Provider)
                .WithMany(p => p.Models)
                .HasForeignKey(m => m.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Model -> Question (Jeden model użyty w wielu zapytaniach)
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Model)
                .WithMany(m => m.Queries)
                .HasForeignKey(q => q.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Zapytania (Question) ---

            // Language -> Question (Jeden język, wiele zapytań)
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Language)
                .WithMany(l => l.Queries)
                .HasForeignKey(q => q.LanguageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Question -> Prompts (Jedno zapytanie, wiele promptów)
            modelBuilder.Entity<Prompt>()
                .HasOne(p => p.Query)
                .WithMany(q => q.Prompts)
                .HasForeignKey(p => p.QueryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Question -> Results (Jedno zapytanie, wiele wyników)
            modelBuilder.Entity<Result>()
                .HasOne(r => r.Query)
                .WithMany(q => q.Results)
                .HasForeignKey(r => r.QueryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Result -> EditedResults (Jeden wynik może mieć wiele wersji edycji)
            modelBuilder.Entity<EditedResult>()
                .HasOne(er => er.Result)
                .WithMany(r => r.EditedResults)
                .HasForeignKey(er => er.ResultId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Szablony Promptów (PromptTemplates) ---

            // Language -> PromptTemplates (Jeden język programowania, wiele szablonów)
            modelBuilder.Entity<PromptTemplate>()
                .HasOne(pt => pt.Language)
                .WithMany(l => l.PromptTemplates)
                .HasForeignKey(pt => pt.LanguageId)
                .OnDelete(DeleteBehavior.Restrict);

            // TemplateCategories -> PromptTemplates (Jedna kategoria, wiele szablonów)
            modelBuilder.Entity<PromptTemplate>()
                .HasOne(pt => pt.Category)
                .WithMany(c => c.PromptTemplates)
                .HasForeignKey(pt => pt.CategoryId)
                .OnDelete(DeleteBehavior.SetNull); // Kategoria jest opcjonalna (int?)

            // --- Tabela łącząca (PromptTemplateUsed) ---
            // Relacja wiele-do-wielu rozbita na dwie relacje jeden-do-wielu

            // Query -> PromptTemplateUsed
            modelBuilder.Entity<PromptTemplateUsed>()
                .HasOne(ptu => ptu.Query)
                .WithMany(q => q.PromptTemplatesUsed)
                .HasForeignKey(ptu => ptu.QueryId)
                .OnDelete(DeleteBehavior.Cascade);

            // PromptTemplate -> PromptTemplateUsed
            modelBuilder.Entity<PromptTemplateUsed>()
                .HasOne(ptu => ptu.PromptTemplate)
                .WithMany(pt => pt.PromptTemplatesUsed)
                .HasForeignKey(ptu => ptu.PromptTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
