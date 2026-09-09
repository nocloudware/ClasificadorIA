# Modo Local Especializado por Tipo de Archivo — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reemplazar el modo local genérico por un despachador que usa algoritmos especializados por tipo de archivo (EXIF, tags de audio, patrones de serie, fechas), con el clasificador token-genérico actual como fallback universal.

**Architecture:** `LocalClassifier.Classify` se convierte en un despachador: (1) prueba metadatos por tipo de archivo vía un nuevo `MetadataClassifier`, (2) los archivos sin metadatos caen al algoritmo token-genérico existente (refactorizado a `ClassifyByTokens`), (3) merge + `CapToDepth`. Se agregan `MetadataExtractor` (EXIF) y `TagLibSharp` (tags) como paquetes NuGet.

**Tech Stack:** .NET 8 WPF, C#; NuGet: `MetadataExtractor`, `TagLibSharp`.

**Spec:** `docs/superpowers/specs/2026-09-08-modo-local-especializado-design.md`

## Global Constraints

- Target: `net8.0-windows` (no cambia).
- Gates por cambio: build `dotnet build "ClasificadorIA.slnx" -c Release -v q --nologo` → 0/0; `--selftest` → `SELF-TEST: OK`; smoke 5s vivo.
- Fallback en cascada OBLIGATORIO: metadatos → nombre → extensión → "Otros". Nada corta la corrida por un archivo.
- `LocalClassifier.Classify` recibe **rutas** ahora; los tests existentes que pasan nombres deben seguir pasando (un nombre sin ruta es también un path válido para `Path.GetExtension`).
- Licencias (AGENTS.md): copiar texto de licencia a `Libs/{Componente}/LICENSE.txt` y agregar entrada en `THIRD_PARTY_NOTICES.txt`.
- NO agregar funcionalidad fuera de alcance (YAGNI): sin análisis de ondas de audio, sin escritura de metadatos.

---

### Task 1: Dependencias NuGet + libros de licencias

**Files:**
- Modify: `ClasificadorIA/ClasificadorIA.csproj`
- Create: `Libs/MetadataExtractor/LICENSE.txt`
- Create: `Libs/TagLibSharp/LICENSE.txt`
- Create: `THIRD_PARTY_NOTICES.txt`

**Interfaces:**
- Produces: paquetes `MetadataExtractor` y `TagLibSharp` restaurables, disponibles para `using MetadataExtractor;` y `using TagLib;` en `ClasificadorIA`.

- [ ] **Step 1: Agregar paquetes al csproj**

En `ClasificadorIA/ClasificadorIA.csproj`, dentro del `<ItemGroup>` existente de `PackageReference`:

```xml
    <PackageReference Include="MetadataExtractor" Version="2.8.1" />
    <PackageReference Include="TagLibSharp" Version="2.3.0" />
```

- [ ] **Step 2: Restaurar**

Run: `dotnet restore "ClasificadorIA.slnx" -v q --nologo`
Expected: exit 0, sin errores.

- [ ] **Step 3: Documentos de licencia**

Crear `Libs/MetadataExtractor/LICENSE.txt` con el texto Apache-2.0 (aparato del proyecto: `docs/superpowers/...` no; usar el de referencia https://www.apache.org/licenses/LICENSE-2.0.txt).
Crear `Libs/TagLibSharp/LICENSE.txt` con el texto LGPL-2.1 (referencia https://www.gnu.org/licenses/old-licenses/lgpl-2.1.txt).
Crear `THIRD_PARTY_NOTICES.txt` (raíz del repo) con:

```text
THIRD PARTY NOTICES

Este proyecto usa los siguientes componentes de terceros:

1. MetadataExtractor (Apache-2.0)
   - Electronic Arts / Drew Noakes - https://github.com/drewnoakes/metadata-extractor-dotnet
   - Licencia: Libs/MetadataExtractor/LICENSE.txt

2. TagLibSharp (LGPL-2.1)
   - Mono / https://github.com/mono/taglib-sharp
   - Licencia: Libs/TagLibSharp/LICENSE.txt
```

Nota: los textos de licencia se pegan de las URLs de referencia del paso (fetch web manual del implementador).

- [ ] **Step 4: Verificar gates**

Run: `dotnet build "ClasificadorIA.slnx" -c Release -v q --nologo`
Expected: 0 advertencias, 0 errores.

- [ ] **Step 5: Commit**

```bash
git add Libs/ THIRD_PARTY_NOTICES.txt ClasificadorIA/ClasificadorIA.csproj
git commit -m "chore: agrega MetadataExtractor y TagLibSharp con licencias"
```

---

### Task 2: MetadataClassifier — extractores por tipo

**Files:**
- Create: `ClasificadorIA/Services/MetadataClassifier.cs`
- Test: `ClasificadorIA/Services/SelfTest.cs` (agregar `RunMetadataClassifier()`)

**Interfaces:**
- Consumes: `Idioma` (`ClasificadorIA.Models.Idioma`), `Translations.Get(string, Idioma)`.
- Produces:
  - `public static string? TryClassify(string path, string? criterionKey, Idioma idioma)` — categoría desde metadatos, o `null` si no hay/falla.
  - Helpers internos: `IsImage/IsAudio/IsVideo/IsDoc(string ext)` reutilizando los `HashSet` de extensiones (ver Task 3 para compartir; por ahora duplicar las listas aquí o exponerlas como `internal`).

- [ ] **Step 1: Escribir el test fallido**

En `SelfTest.cs`, dentro de `Run()` (línea ~21) agregar `RunMetadataClassifier();` y al final del archivo:

```csharp
// ── Micro-clasificador por metadatos ─────────────────────────────────

private static void RunMetadataClassifier()
{
    string root = Path.Combine(Path.GetTempPath(), "metaclass-selftest-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(root);
    try
    {
        // Video con patrón de serie → "Temporada 1".
        string serie = Path.Combine(root, "S01E05.mp4");
        File.WriteAllText(serie, "x");
        Assert("Meta: S01E05 → Temporada 1", MetadataClassifier.TryClassify(serie, null, Idioma.Español) == "Temporada 1");

        // Video sin patrón → año de creación del archivo.
        string video = Path.Combine(root, "vacaciones.mp4");
        File.WriteAllText(video, "x");
        int year = File.GetLastWriteTime(video).Year;
        Assert("Meta: video sin patrón → año", MetadataClassifier.TryClassify(video, null, Idioma.Español) == year.ToString());

        // Documento → año.
        string doc = Path.Combine(root, "nota.txt");
        File.WriteAllText(doc, "x");
        Assert("Meta: doc → año", MetadataClassifier.TryClassify(doc, null, Idioma.Español) == year.ToString());

        // Audio sin tags válidos (no es audio real) → null → cae al fallback.
        string fake = Path.Combine(root, "cancion.mp3");
        File.WriteAllText(fake, "no-es-audio");
        Assert("Meta: audio corrupto → null", MetadataClassifier.TryClassify(fake, "Género", Idioma.Español) == null);
    }
    finally
    {
        try { Directory.Delete(root, true); } catch { }
    }
}
```

- [ ] **Step 2: Correr el test para verlo fallar**

Run: `dotnet build "ClasificadorIA.slnx" -c Release -v q --nologo` → expected ERROR: existe `RunMetadataClassifier` sin declarar / no compila.

- [ ] **Step 3: Implementar MetadataClassifier**

```csharp
using System.Globalization;
using System.Text.RegularExpressions;
using ClasificadorIA.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using TagLib;

namespace ClasificadorIA.Services;

/// <summary>Extrae categorías desde los metadatos reales del archivo (EXIF, tags de audio, fecha). Fallback: null.</summary>
public static class MetadataClassifier
{
    public static string? TryClassify(string path, string? criterionKey, Idioma idioma)
    {
        string ext = Path.GetExtension(path);
        if (IsImage(ext)) return ImageDate(path, idioma);
        if (IsAudio(ext)) return AudioTag(path, criterionKey, idioma);
        if (IsVideo(ext)) return VideoCategory(path, idioma);
        if (IsDoc(ext)) return FileYear(path);
        return null;
    }

    private static string? ImageDate(string path, Idioma idioma)
    {
        try
        {
            var dirs = ImageMetadataReader.ReadMetadata(path);
            var sub = dirs.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (sub == null || !sub.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime dt))
                return null;
            return $"{CultureInfo.GetCultureInfo(idioma == Idioma.Español ? "es" : "en").DateTimeFormat.GetMonthName(dt.Month)} {dt.Year}";
        }
        catch (Exception) { return null; }
    }

    private static string? AudioTag(string path, string? criterionKey, Idioma idioma)
    {
        try
        {
            using var f = TagLib.File.Create(path);
            return criterionKey switch
            {
                "Artista" => First(f.Tag.Performers),
                "Álbum" => string.IsNullOrWhiteSpace(f.Tag.Album) ? null : f.Tag.Album,
                "Año" => f.Tag.Year > 0 ? f.Tag.Year.ToString() : null,
                "Época" => f.Tag.Year > 0 ? string.Format(Translations.Get("Decade", idioma), f.Tag.Year / 10 * 10) : null,
                _ => First(f.Tag.Genres) ?? First(f.Tag.Performers)
            };
        }
        catch (Exception) { return null; }
    }

    private static string? VideoCategory(string path, Idioma idioma)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        var m = Regex.Match(name, @"[Ss](\d{1,2})[Ee]\d{1,2}");
        if (m.Success)
            return string.Format(Translations.Get("Temporada", idioma), int.Parse(m.Groups[1].Value));
        return FileYear(path);
    }

    private static string? FileYear(string path)
    {
        try { return File.GetLastWriteTime(path).Year.ToString(); }
        catch (Exception) { return null; }
    }

    private static string? First(string[] values) =>
        values?.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

    private static bool IsImage(string ext) => LocalClassifier.IsImageExt(ext);
    private static bool IsAudio(string ext) => LocalClassifier.IsAudioExt(ext);
    private static bool IsVideo(string ext) => LocalClassifier.IsVideoExt(ext);
    private static bool IsDoc(string ext) => LocalClassifier.IsDocExt(ext);
}
```

- [ ] **Step 4: Agregar traducciones**

En `Translations.cs` (junto al grupo de cadenas):

```csharp
["Temporada"] = ("Temporada {0}", "Season {0}"),
["Decade"] = ("Década {0}", "{0}s"),
```

- [ ] **Step 5: Exponer extensiones en LocalClassifier (Task 3 prepara)**

En `LocalClassifier.cs` cambiar los 4 `private static readonly HashSet<string> Ext...` a `internal static readonly`.

- [ ] **Step 6: Correr el test para verlo pasar**

Run: `dotnet build "ClasificadorIA.slnx" -c Release -v q --nologo` y luego el exe con `--selftest`.
Expected: `SELF-TEST: OK`.

- [ ] **Step 7: Commit**

```bash
git add ClasificadorIA/Services/MetadataClassifier.cs ClasificadorIA/Services/LocalClassifier.cs ClasificadorIA/Services/Translations.cs ClasificadorIA/Services/SelfTest.cs
git commit -m "feat(local): clasificador por metadatos (EXIF, tags, serie, fecha)"
```

---

### Task 3: LocalClassifier — despachador + fallback token

**Files:**
- Modify: `ClasificadorIA/Services/LocalClassifier.cs`
- Test: `ClasificadorIA/Services/SelfTest.cs`

**Interfaces:**
- Consumes: `MetadataClassifier.TryClassify(path, criterionKey, idioma)`, `Translations.Get("Otros", idioma)`.
- Produces:
  - `public static List<ClassificationResult> Classify(IReadOnlyList<string> paths, int depth, Idioma idioma, string? criterionKey = null)` — firma nueva (recibe paths y criterio opcional).
  - `internal static bool IsImageExt(string ext)` / `IsAudioExt` / `IsVideoExt` / `IsDocExt` — checks case-insensitive sobre las extensiones ya existentes.
  - `private static (Dictionary<string,string> topic, Dictionary<string,string> ext) ClassifyByTokens(...)` — el algoritmo token actual operando sobre el subconjunto sin metadatos.
  - Mantiene `CapToDepth` público (ya usado por BatchClassifier).

- [ ] **Step 1: Escribir los tests (fallback y despacho)**

En `SelfTest.cs` `RunLocalClassifier()` agregar:

```csharp
// Despacho por tipo: imagen sin EXIF y sin tokens → extensión.
var fakeImg = new[] { "SCAN123.bmp", "IMG_456.png" };
var imgRes = LocalClassifier.Classify(fakeImg, 5, Idioma.Español, null);
Assert("Local: imagen sin metadatos → extensión Imagen", imgRes.Any(r => r.Category == "Imagen"));
```

Y verificar que los tests existentes (que pasan NOMBRES como "rock-a.mp3") siguen pasando — ese es el objetivo de compatibilidad de este task.

- [ ] **Step 2: Implementar despachador**

Reescribir `LocalClassifier.Classify` a:

```csharp
public static List<ClassificationResult> Classify(
    IReadOnlyList<string> paths, int depth, Idioma idioma, string? criterionKey = null)
{
    if (paths.Count == 0) return new List<ClassificationResult>();

    // Paso 1: metadatos
    var byMetadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var p in paths)
    {
        string? cat = MetadataClassifier.TryClassify(p, criterionKey, idioma);
        if (cat != null) byMetadata.TryAdd(p, cat);
    }

    // Paso 2: token-genérico sobre el resto
    var remaining = paths.Where(p => !byMetadata.ContainsKey(p)).ToList();
    var (topicByFile, extByFile) = ClassifyByTokens(remaining);

    var buckets = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
    foreach (var (f, cat) in byMetadata)
        AddTo(buckets, cat, f);
    foreach (var f in remaining)
    {
        string cat = topicByFile.TryGetValue(f, out string? topic)
            ? topic
            : extByFile.TryGetValue(f, out string? ext) ? ext : Translations.Get("Otros", idioma);
        AddTo(buckets, cat, f);
    }

    var results = buckets
        .Where(kv => kv.Value.Count > 0)
        .Select(kv => new ClassificationResult(kv.Key, kv.Value.AsReadOnly()))
        .ToList();
    return CapToDepth(results, depth, Translations.Get("Otros", idioma));
}
```

Extraer del `Classify` actual el bloque del token-genérico en `ClassifyByTokens` (dada una lista de paths devuelve dos diccionarios `topicByFile` / `extByFile`, donde `extByFile` es `ExtensionCategory`). Mantener `ExtensionCategory`, `Tokenize`, `Capitalize` privados como están. Agregar helper `private static void AddTo(Dictionary<string, List<string>> b, string cat, string file)`.

Exponer los 4 `Is*Ext` internals delegando a los `HashSet` ya existentes:

```csharp
internal static bool IsImageExt(string ext) => ExtImage.Contains(ext.TrimStart('.'));
internal static bool IsAudioExt(string ext) => ExtAudio.Contains(ext.TrimStart('.'));
internal static bool IsVideoExt(string ext) => ExtVideo.Contains(ext.TrimStart('.'));
internal static bool IsDocExt(string ext) => ExtDoc.Contains(ext.TrimStart('.'));
```

- [ ] **Step 3: Verificar tests**

Run: build Release + `--selftest`.
Expected: `SELF-TEST: OK` — los tests existentes de nombre y el nuevo de despacho pasan.

- [ ] **Step 4: Commit**

```bash
git add ClasificadorIA/Services/LocalClassifier.cs ClasificadorIA/Services/SelfTest.cs
git commit -m "feat(local): despachador por tipo con fallback token-genérico"
```

---

### Task 4: Conectar rutas y criterio en la UI

**Files:**
- Modify: `ClasificadorIA/App.xaml.cs:304-334`
- Test: gates (sin selftest nuevo; verificar build + smoke)

**Interfaces:**
- Consumes: `BaseFileItem.FilePath` (ya existe en `NoCloudware.UI.Core`), `LocalClassifier.Classify(paths, depth, idioma, criterionKey)`, `GetOptions()`.
- Produces: modo local clasifica con rutas reales y el criterio elegido.

- [ ] **Step 1: Cambiar la fuente de archivos**

En `App.xaml.cs`, el método `GetFileNames()` (línea ~304) se queda para IA. Agregar y usar para local:

```csharp
private string[] GetFilePaths() =>
    _window!.Files.Where(f => !FileFilters.IsSystemFile(f.FileName)).Select(f => f.FilePath).ToArray();
```

- [ ] **Step 2: Ajustar ClassifyLocal**

```csharp
private void ClassifyLocal(string[] paths)
{
    var (_, criterion, depth) = GetOptions();
    var results = LocalClassifier.Classify(paths, depth, Translations.Current, criterion);
    ...
}
```

Y en `ClassifyAsync()` (`linea ~309-320`) pasar `GetFilePaths()` para el modo local y mantener `GetFileNames()` para IA:

```csharp
if (_options!.MethodIa.IsChecked == true)
    await ClassifyIa(GetFileNames());
else
    ClassifyLocal(GetFilePaths());
```

- [ ] **Step 3: Verificar gates**

Run: build Release (0/0) + `--selftest` (`SELF-TEST: OK`) + smoke 5s `SMOKE: True`.

- [ ] **Step 4: Commit**

```bash
git add ClasificadorIA/App.xaml.cs
git commit -m "feat(local): clasifica con rutas reales y criterio elegido"
```

---

## Self-Review (rellenado al final)

- **Spec coverage:** punto "tabla de despacho" → Task 2+3; "fallback en cascada" → Task 3; "cambios de interfaz" → Task 4; "dependencias" → Task 1; "tests" → Tasks 2-3; "fuera de alcance" → respetado (no hay tarea de GTZAN/escritura).
- **Placeholder scan:** el texto de licencia del Task 1 Step 3 se indica como "fetch web manual" — es intencional (los textos no se reproducen por licencia). El resto es código concreto.
- **Tipo consistente:** `Classify(paths, depth, idioma, criterionKey?)` definido en Task 3 y consumido igual en Task 4. `MetadataClassifier.TryClassify(path, criterionKey, idioma)` generado en Task 2, consumido en Task 3. `Is*Ext` internals generados en Task 3 Step 5 (adelantado dentro de Task 2) y usados en Task 2.