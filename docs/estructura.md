# Estructura del proyecto

```
avalonia-poc/
├── Models/
│   └── TodoItem.cs               # Entidad con Id, Title, Text, Status, SortOrder
├── Data/
│   └── TodoDbContext.cs           # DbContext con EF Core + SQLite
├── Services/
│   ├── ConfigService.cs           # Lee/guarda config JSON en AppData
│   └── TodoService.cs             # CRUD de tareas
├── ViewModels/
│   ├── TodoItemViewModel.cs       # VM por cada tarea + converters de estado
│   ├── MainViewModel.cs           # VM principal con comandos, paginación, búsqueda
│   └── StatusConverters.cs        # Converter genérico de TodoStatus a colores
├── Views/
│   ├── SetupView.axaml/.cs        # Pantalla inicial para elegir ubicación de BD
│   ├── TodoListView.axaml/.cs     # Lista principal de tareas (grilla)
│   ├── EditTodoWindow.axaml/.cs   # Ventana modal para crear/editar tarea
│   └── ConfirmDialog.axaml/.cs    # Diálogo de confirmación para eliminar
├── Assets/
│   ├── Fonts/
│   │   └── fa-solid-900.ttf       # Font Awesome 6 Free Solid
│   └── avalonia-logo.png          # Ícono de la app
├── MainWindow.axaml/.cs           # Ventana que orquesta la navegación
├── App.axaml/.cs                  # Configuración global, tema, estilos de botones
├── Program.cs                     # Entry point
├── TodoApp.csproj                 # .NET 10, Avalonia 12, EF Core, CommunityToolkit
├── publish-macos.sh               # Script de publicación self-contained para macOS
└── docs/
    └── estructura.md              # Este archivo
```

## Stack tecnológico

- **.NET 10** + **Avalonia 12** (UI cross-platform)
- **Entity Framework Core** con **SQLite** (persistencia)
- **CommunityToolkit.Mvvm** (patrón MVVM con source generators)
- **Font Awesome 6** (íconos embebidos como fuente)

## Configuración

La app guarda la ubicación de la BD SQLite en:
```
~/Library/Application Support/TodoApp/config.json
```

Si no existe o el path es inválido, muestra la pantalla de Setup.

## Publicación

```bash
./publish-macos.sh
```

Genera `publish/TodoApp.app` — bundle self-contained con .NET incluido, listo para copiar a `/Applications/`.
