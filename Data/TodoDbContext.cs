using Microsoft.EntityFrameworkCore;
using TodoApp.Models;

namespace TodoApp.Data;

public class TodoDbContext : DbContext
{
    private readonly string _dbPath;

    public DbSet<TodoItem> Todos => Set<TodoItem>();

    public TodoDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Text).HasMaxLength(2000);
            entity.Property(e => e.Status).HasConversion<string>();
        });

        modelBuilder.Entity<TodoItem>().HasData(
            new TodoItem { Id = 1, Title = "Tarea de ejemplo", Text = "Esta es una tarea de ejemplo creada automáticamente", Status = TodoStatus.Creado, SortOrder = 1 },
            new TodoItem { Id = 2, Title = "Revisar documentación", Text = "Leer la documentación del proyecto", Status = TodoStatus.EnEjecucion, SortOrder = 2 },
            new TodoItem { Id = 3, Title = "Configurar entorno", Text = "Instalar dependencias y configurar el entorno de desarrollo", Status = TodoStatus.Terminado, SortOrder = 3 }
        );
    }
}

public class TodoDbContextFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<TodoDbContext>
{
    public TodoDbContext CreateDbContext(string[] args)
    {
        return new TodoDbContext("design_time.db");
    }
}
