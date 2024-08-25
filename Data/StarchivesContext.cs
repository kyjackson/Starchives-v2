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

        public DbSet<Video> Videos { get; set; } = default!;

		public DbSet<Caption> Captions { get; set; } = default!;

		#region Required
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Caption>()
						.Property(b => b.VideoId)
						.IsRequired();
		}
		#endregion
	}
}
