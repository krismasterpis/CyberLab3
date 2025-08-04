using CyberLab3.Resources.Libraries;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.Json;

namespace SqliteWpfApp
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Measurement> Measurements { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=measurements.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Measurement>()
                .Property(m => m.traces) // Nadal celujemy we właściwość Traces
                .HasConversion(
                    // Funkcja konwertująca Dictionary<string, Trace> na string
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = false }),

                    // Funkcja konwertująca string z bazy z powrotem na Dictionary<string, Trace>
                    v => JsonSerializer.Deserialize<Dictionary<string, Trace>>(v, new JsonSerializerOptions())
                );

            modelBuilder.Entity<Measurement>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<Measurement>()
                .Property(m => m.Id)
                .ValueGeneratedOnAdd(); // SQLite: AUTOINCREMENT
        }
    }
}