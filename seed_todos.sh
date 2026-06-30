#!/bin/bash
# Genera 1000 registros de TodoItem intercalando estados: Creado, EnEjecucion, Terminado

DB_PATH="/Users/garci/Library/Application Support/TodoApp/todos.db"

if [ ! -f "$DB_PATH" ]; then
  echo "Error: base de datos no encontrada en '$DB_PATH'"
  exit 1
fi

TITULOS=(
  "Revisar código"
  "Actualizar dependencias"
  "Escribir pruebas"
  "Refactorizar módulo"
  "Documentar API"
  "Corregir bug"
  "Optimizar consulta"
  "Desplegar versión"
  "Reunión de equipo"
  "Planificar sprint"
)

TEXTOS=(
  "Revisar los cambios recientes y verificar que todo esté correcto."
  "Actualizar los paquetes NuGet a sus últimas versiones estables."
  "Agregar pruebas unitarias para los nuevos componentes."
  "Mejorar la legibilidad y estructura del módulo actual."
  "Documentar los endpoints disponibles con ejemplos de uso."
  "Identificar y corregir el error reportado en producción."
  "Optimizar las consultas a la base de datos para mejor rendimiento."
  "Preparar y ejecutar el despliegue de la nueva versión."
  "Coordinar con el equipo los objetivos de la semana."
  "Definir las tareas y prioridades para el próximo sprint."
)

ESTADOS=("Creado" "EnEjecucion" "Terminado")

echo "Generando 100000 registros en '$DB_PATH'..."

MAX_SORT=$(sqlite3 "$DB_PATH" "SELECT COALESCE(MAX(SortOrder), 0) FROM Todos;")

SQL="BEGIN TRANSACTION;"

for i in $(seq 1 100000); do
  TITULO_IDX=$(( (i - 1) % ${#TITULOS[@]} ))
  TEXTO_IDX=$(( (i - 1) % ${#TEXTOS[@]} ))
  ESTADO_IDX=$(( (i - 1) % 3 ))

  TITULO="${TITULOS[$TITULO_IDX]} #$i"
  TEXTO="${TEXTOS[$TEXTO_IDX]}"
  ESTADO="${ESTADOS[$ESTADO_IDX]}"
  SORT=$(( MAX_SORT + i ))

  SQL+="INSERT INTO Todos (Title, Text, Status, SortOrder) VALUES ('$TITULO', '$TEXTO', '$ESTADO', $SORT);"
done

SQL+="COMMIT;"

echo "$SQL" | sqlite3 "$DB_PATH"

if [ $? -eq 0 ]; then
  TOTAL=$(sqlite3 "$DB_PATH" "SELECT COUNT(*) FROM Todos;")
  echo "✓ Listo. Total de registros en la BD: $TOTAL"
  echo ""
  echo "Distribución por estado:"
  sqlite3 "$DB_PATH" "SELECT Status, COUNT(*) as cantidad FROM Todos GROUP BY Status ORDER BY Status;"
else
  echo "Error al insertar los registros."
  exit 1
fi
