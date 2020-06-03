using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseConnection
{
    public class DataBaseCtx: DbContext
    {
        public DbSet<Entry> Entries { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=DESKTOP-GOB4QRS\SQLEXPRESS;Initial Catalog=PropertyFinder;Integrated Security=True");
        }
    }
}
