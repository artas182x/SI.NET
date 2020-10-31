using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace demo.Models
{
    public class CacheDbContext : DbContext
    {
        public DbSet<DBTempData> TempData { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string db_password = Environment.GetEnvironmentVariable("SA_PASSWORD");
            optionsBuilder.UseSqlServer($@"Server=db;User=sa;Password={db_password};MultipleActiveResultSets=true");
        }

    }

    public class DBTempData {
        [Key]
        public string Key {get;set;}
        public string Value {get;set;}
    }
}