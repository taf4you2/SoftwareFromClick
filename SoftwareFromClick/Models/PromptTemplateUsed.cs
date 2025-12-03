using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SoftwareFromClick.Models
{
    public class PromptTemplateUsed
    {
        public int Id { get; set; }
        
        public int QueryId { get; set; }
        public Query Query { get; set; } = null!;

        public int PromptTemplateId { get; set; }
        public PromptTemplate PromptTemplate { get; set; } = null!;
    }
}
