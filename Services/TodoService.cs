using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Services;

public class TodoService
{
    private readonly string _dbPath;

    public TodoService(string dbPath)
    {
        _dbPath = dbPath;
        using var db = CreateContext();
        db.Database.Migrate();
    }

    private TodoDbContext CreateContext() => new(_dbPath);

    public List<TodoItem> GetAll()
    {
        using var db = CreateContext();
        return db.Todos.OrderBy(t => t.SortOrder).ToList();
    }

    public TodoItem Add(string title, string text, TodoStatus status)
    {
        using var db = CreateContext();
        var maxOrder = db.Todos.Any() ? db.Todos.Max(t => t.SortOrder) : 0;
        var item = new TodoItem
        {
            Title = title,
            Text = text,
            Status = status,
            SortOrder = maxOrder + 1
        };
        db.Todos.Add(item);
        db.SaveChanges();
        return item;
    }

    public void Update(TodoItem item)
    {
        using var db = CreateContext();
        db.Todos.Update(item);
        db.SaveChanges();
    }

    public void Delete(int id)
    {
        using var db = CreateContext();
        var item = db.Todos.Find(id);
        if (item != null)
        {
            db.Todos.Remove(item);
            db.SaveChanges();
        }
    }
}
