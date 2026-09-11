# Metadata pre-cargada + TreeView como única visualización

> **Para agentes:** Usar superpowers:subagent-driven-development o superpowers:executing-plans para implementar tarea por tarea.

**Objetivo:** Cargar toda la metadata de un archivo UNA sola vez al arrastrarlo, y usar un TreeView agrupado como única visualización (antes: lista plana + árbol separado).

**Arquitectura:** Cada BaseFileItem lleva un diccionario completo de metadatos (todos los criterios de todos los modos). Al cargar se calculan una vez. El FileListBox se reemplaza por un ListView con GroupStyle que agrupa por categoría — antes de clasificar muestra "Archivos", después las categorías detectadas. Cero disco al cambiar de modo.

**Stack:** WPF, CommunityToolkit.Mvvm, TagLib#, MetadataExtractor.

---

## Archivos modificados

| Archivo | Cambio |
|---|---|
| `NoCloudware.UI.Core/ViewModels/BaseFileItem.cs` | Agregar `AllMetadata` dict + `Category` observable |
| `ClasificadorIA/Services/MetadataClassifier.cs` | Agregar `GetAllMetadata()` |
| `ClasificadorIA/App.xaml.cs` | `AddFiles` con cache + guard directorio, `RefreshMetadata` desde cache, `ShowClassificationTree` reemplazado |
| `NoCloudware.UI.Core/Controls/FileListBox.xaml` | GroupStyle + CollectionViewSource |
| `NoCloudware.UI.Core/Controls/FileListBox.xaml.cs` | `SetCategoryGrouping()`, refresh view |

---

## Tarea 1: BaseFileItem — AllMetadata + Category

**Archivos:**
- Modificar: `NoCloudware.UI.Core/ViewModels/BaseFileItem.cs`

**Qué hacer:**

Agregar dos campos `[ObservableProperty]`:

```csharp
// En BaseFileItem.cs — después del campo _metadataCells
[ObservableProperty]
private Dictionary<string, string?> _allMetadata = new();

[ObservableProperty]
private string _category = "Archivos";
```

`AllMetadata` almacena TODOS los valores de criterios calculados una vez (key = clave del criterio, ej. "Género", "Artista"). `Category` es la clave de agrupación para el TreeView — "Archivos" por defecto, el nombre de la categoría después de clasificar.

- [ ] Verificar que compila: `dotnet build ClasificadorIA.slnx -v q --nologo` Debug 0/0, Release 0/0

---

## Tarea 2: MetadataClassifier — GetAllMetadata

**Archivos:**
- Modificar: `ClasificadorIA/Services/MetadataClassifier.cs`

**Qué hacer:**

Agregar un método público que itera TODOS los criterios conocidos:

```csharp
public static Dictionary<string, string?> GetAllMetadata(string path, Idioma idioma)
{
    var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    string[] allCriteria = { "Tema", "Género", "Artista", "Álbum", "Época", "Año", "Director", "Saga", "Cadena", "Autor", "Editorial" };
    foreach (var c in allCriteria)
        result[c] = TryClassify(path, c, idioma);
    return result;
}
```

Nota: `TryClassify` para "Tema" retorna `null` (se resuelve por tokens en LocalClassifier). Los criterios sin fuente local (Director, Saga, Cadena, Autor, Editorial) también retornan `null`. Esto es correcto — el null significa "sin metadato local".

- [ ] Verificar que compila: `dotnet build ClasificadorIA.slnx -v q --nologo` Debug 0/0, Release 0/0

---

## Tarea 3: AddFiles — cache de metadata + guard directorio

**Archivos:**
- Modificar: `ClasificadorIA/App.xaml.cs` (líneas 264-284)

**Qué hacer:**

Reemplazar `AddFiles` para:
1. Guardar `File.Exists(path)` antes de `FileInfo(path).Length` (crash fix — directorios arrastrados)
2. Calcular metadata UNA vez y guardar en `AllMetadata`
3. Mapear `MetadataCells` desde el cache según el modo actual

```csharp
private void AddFiles(IEnumerable<string> paths)
{
    var nowFiles = _window!.Files;
    foreach (var item in nowFiles)
        if (!_masterFiles.Any(f => f.FilePath.Equals(item.FilePath, StringComparison.OrdinalIgnoreCase)))
            _masterFiles.Add(item);

    var idioma = Translations.Current;
    var (mode, _, _) = GetOptions();

    foreach (var path in paths)
    {
        if (FileFilters.IsSystemFile(Path.GetFileName(path))) continue;
        if (_masterFiles.Any(f => f.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase))) continue;
        if (!File.Exists(path)) continue; // guard: carpetas/inaccesibles

        long size = 0;
        try { size = new FileInfo(path).Length; } catch { continue; }

        var item = new BaseFileItem
        {
            FilePath = path,
            FileName = Path.GetFileName(path),
            FileSize = size
        };
        item.AllMetadata = MetadataClassifier.GetAllMetadata(path, idioma);
        item.MetadataCells = MapCells(item.AllMetadata, mode);
        _masterFiles.Add(item);
    }
    ApplyDedupFilter();
    RefreshMetadata();
}
```

El método auxiliar `MapCells` (nuevo en App.xaml.cs):

```csharp
private static string[] MapCells(Dictionary<string, string?> all, ClassificationMode mode)
{
    var cells = new string[mode.Criterios.Length];
    for (int i = 0; i < cells.Length; i++)
        cells[i] = all.TryGetValue(mode.Criterios[i], out var v) ? v ?? "" : "";
    return cells;
}
```

- [ ] Verificar que compila: `dotnet build ClasificadorIA.slnx -v q --nologo` Debug 0/0, Release 0/0

---

## Tarea 4: RefreshMetadata — cero disco, solo re-mapeo

**Archivos:**
- Modificar: `ClasificadorIA/App.xaml.cs` (líneas 286-296)

**Qué hacer:**

Reemplazar el loop que llama `GetCells` por lectura desde el cache:

```csharp
private void RefreshMetadata()
{
    var (mode, _, _) = GetOptions();
    var idioma = Translations.Current;
    _window!.FileListBox.SetMetadataHeaders(
        string.Equals(mode.Key, "Genérico", StringComparison.OrdinalIgnoreCase)
            ? Array.Empty<string>()
            : ClassificationModes.GetCriteria(mode, idioma));
    foreach (var item in _window.Files)
        item.MetadataCells = MapCells(item.AllMetadata, mode);
}
```

Esto es TODO lo que necesita: encabezados + re-mapeo desde diccionario. Cero disco, cero TagLib, cero EXIF.

- [ ] Verificar que compila: `dotnet build ClasificadorIA.slnx -v q --nologo` Debug 0/0, Release 0/0

---

## Tarea 5: ShowClassificationTree — reemplazar por agrupación en la lista

**Archivos:**
- Modificar: `ClasificadorIA/App.xaml.cs` (líneas 483-532)

**Qué hacer:**

En lugar de construir un TreeView separado y empujarlo a `FileListCustomContent`, simply setear `Category` en cada BaseFileItem. El ListView agrupa automáticamente.

```csharp
private void SetResults(List<ClassificationResult> results)
{
    _results = results;

    // Mapeo category → files desde results
    var fileToCategory = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var r in results)
        foreach (var name in r.Files)
            fileToCategory[name] = r.Category;

    // Setear Category en cada item visible
    foreach (var item in _window!.Files)
    {
        if (fileToCategory.TryGetValue(item.FileName, out var cat))
            item.Category = cat;
        else
            item.Category = "Sin categoría";
    }

    // Refrescar la vista agrupada
    _window.FileListBox.RefreshGrouping();
    _window.MainControl.UpdateCounters();
}

private void ResetResults()
{
    _results = new List<ClassificationResult>();
    if (_window == null) return;
    foreach (var item in _window.Files)
        item.Category = "Archivos";
    _window.FileListBox.RefreshGrouping();
}
```

Eliminar `ShowClassificationTree()` — ya no se necesita. El `FileListCustomContent` nunca se setea.

- [ ] Verificar que compila: `dotnet build ClasificadorIA.slnx -v q --nologo` Debug 0/0, Release 0/0

---

## Tarea 6: FileListBox — GroupStyle con Category

**Archivos:**
- Modificar: `NoCloudware.UI.Core/Controls/FileListBox.xaml`
- Modificar: `NoCloudware.UI.Core/Controls/FileListBox.xaml.cs`

### XAML — agregar CollectionViewSource y GroupStyle:

Dentro del `<ListView>`, agregar:

```xml
<ListView.GroupStyle>
    <GroupStyle>
        <GroupStyle.HeaderTemplate>
            <DataTemplate>
                <TextBlock Text="{Binding Name}"
                           FontWeight="Bold" FontSize="13"
                           Foreground="{DynamicResource TextPrimaryBrush}"
                           Margin="0,6,0,4"
                           ToolTip="{Binding ItemCount, StringFormat='{}{0} archivos'}"/>
            </DataTemplate>
        </GroupStyle.HeaderTemplate>
    </GroupStyle>
</ListView.GroupStyle>
```

### Code-behind — agregar RefreshGrouping + CollectionViewSource:

```csharp
private CollectionViewSource? _cvs;

public void RefreshGrouping()
{
    _cvs?.View?.Refresh();
}

// En el constructor, después de InitializeComponent():
_cvs = new CollectionViewSource { Source = Items };
_cvs.GroupDescriptions.Add(new PropertyGroupDescription(nameof(BaseFileItem.Category)));
FileListBoxControl.ItemsSource = _cvs.View;
```

Nota: `ItemsSource` ahora se binda al `CollectionViewSource.View` en lugar de directamente a `Items`. El binding XAML `ItemsSource="{Binding Items, RelativeSource={...}}"` se reemplaza por el code-behind.

- [ ] Verificar que compila: `dotnet build ClasificadorIA.slnx -v q --nologo` Debug 0/0, Release 0/0

---

## Tarea 7: SelfTest + verificación

**Archivos:**
- Modificar: `ClasificadorIA/Services/SelfTest.cs` (si es necesario)

**Qué hacer:**

1. Actualizar SelfTest para verificar:
   - `GetAllMetadata` retorna el dict correcto para un mp3 (Género, Artista, Álbum, Época con valores)
   - `MapCells` mapea correctamente desde el dict
   - `BaseFileItem.Category` defaults a "Archivos"
   - Un directorio en `AddFiles` no crashea

2. Gates completos:
   - `dotnet build ClasificadorIA.slnx -v q --nologo` Debug 0/0
   - `dotnet build ClasificadorIA.slnx -v q --nologo` Release 0/0
   - `dotnet run --project ClasificadorIA/ClasificadorIA.csproj --no-build -- --selftest` → OK
   - Smoke 5s → ALIVE

3. Arnés manual de verificación:
   - Cargar archivos → aparecen bajo "Archivos" con metadatos
   - Cambiar modo → columnas cambian, cero delay
   - Clasificar → archivos se reagrupan bajo categorías con metadatos
   - Arrastrar carpeta → no crashea, se ignora o expande

- [ ] SelfTest OK
- [ ] Gates Debug/Release 0/0
- [ ] Smoke ALIVE
- [ ] Commit
