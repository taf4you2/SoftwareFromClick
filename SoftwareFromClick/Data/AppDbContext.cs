using Microsoft.EntityFrameworkCore;
using SoftwareFromClick.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SoftwareFromClick.Data
{
    public class AppDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ApiKeys> ApiKeys { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<AiModel> AiModels { get; set; }
        public DbSet<Query> Queries { get; set; }
        public DbSet<Prompt> Prompts { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<EditedResult> EditedResults { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<TemplateCategories> TemplateCategories { get; set; }
        public DbSet<PromptTemplate> PromptTemplates { get; set; }
        public DbSet<PromptTemplateUsed> PromptTemplatesUsed { get; set; }
    }
}
