namespace TodoApp.Models;

public enum TodoStatus
{
    Creado,
    EnEjecucion,
    Terminado
}

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public TodoStatus Status { get; set; } = TodoStatus.Creado;
    public int SortOrder { get; set; }
}
