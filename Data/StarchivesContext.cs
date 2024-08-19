using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Starchives.Models;

namespace Starchives.Data
{
    public class StarchivesContext : DbContext
    {
        public StarchivesContext (DbContextOptions<StarchivesContext> options)
            : base(options)
        {
        }

        public DbSet<Starchives.Models.Video> Video { get; set; } = default!;
    }
}
