# Plan: paquete de UI e idiomas (continuación 2026-09-12)

Diseño aprobado el 2026-09-11. Objetivo: 5 tareas independientes. Nada de código
escrito aún. Punto de partida: `HEAD = 7c78a08` (review fixes pusheados).

## Decisiones confirmadas por el usuario
- X roja por fila: **siempre visible**.
- IA: habla **en el idioma elegido siempre** → traducir prompts a los 8 idiomas.
- Idiomas: **ES, EN, FR, DE, PT, IT, JA, ZH** (igual que TubeMassDL/TurnAFile).
- Borrado: multiselect (Ctrl+clic / shift+clic) + tecla Supr + X roja por fila.

## Tarea 1 — Borrar archivos (multiselect + Supr + X roja)
- `NoCloudware.UI.Core/Controls/FileListBox.xaml`:
  - `ListBox.SelectionMode="Extended"`.
  - KeyDown del ListBox → borrar todos los `SelectedItems` de `Items` (por índice, iterar inverso).
  - `ItemTemplate`: reemplazar StackPanel por Grid de 3 columnas
    (nombre flexible | status | X roja a la derecha). Botón ✕ rojo, siempre visible,
    `Click` → quitar ese item del template path: handler en code-behind del control.
- `FileListBox.xaml.cs`: `OnRowRemoveClick`, `OnDeleteKeyClicked`.
- Esto ya es suficiente: borrar de `Items` dispara el CollectionChanged de `App`
  (commit 7c78a08) → limpia `_masterFiles`, resetea resultados, actualiza contadores.

## Tarea 2 — Tema claro/oscuro en el fondo de la ventana
- Causa: `ShellWindow.xaml` sin `Background` → queda el gris de Windows.
  `AetherWindowStyle` ya existe (`AetherWindow.xaml`) con `Background=WindowBackgroundBrush`.
- Fix: `Style="{DynamicResource AetherWindowStyle}"` en ShellWindow.xaml,
  AboutDialog.xaml y ByokDialog.xaml (los grids ya usan brushes dinámicos).
- Verificar manual con toggle claro/oscuro.

## Tarea 3 — Carpeta de destino: label + texto en idioma
- `BaseMainControl.xaml`: agregar TextBlock label arriba del Border de salida (Grid.Row=3).
  Texto vía clave existente `OutputFolder` = "Carpeta de destino".
- El texto fijo "Same folder as source":
  - es el default del DP en `BaseMainControl.xaml.cs:41` y `ShellWindow.xaml.cs:46`
    → cambiar default a `""`.
  - puede quedar guardado en `appsettings.json` (DefaultOutputPath) de versiones viejas
    → en `App.ApplyLanguage` (~línea 163) tratar como "no elegido" si el valor es
    vacío, el literal legacy (case-insensitive) o igual al default de cualquier idioma;
    en esos casos resetear a `Translations.Get("OutputFolderDefault")`.

## Tarea 4 — Ocho idiomas (la grande)
### Estructura
- `ClasificadorIA/Models/Idioma.cs`: enum a 8 valores + helper `CultureCode`/`FromCulture`.
- `ClasificadorIA/Services/Translations.cs`:
  - `Map` pasa de `Dictionary<string,(string Es,string En)>` a
    `Dictionary<string, Dictionary<string,string>>` clave = culture code
    (`es`,`en`,`fr`,`de`,`pt`,`it`,`zh`,`ja`), fallback `en`.
  - Mantener API pública `Get(key)`, `Get(key, Idioma)`, `ModeLabel`, `Current`,
    `LanguageChanged`.
  - Traducir las ~200 claves a los 7 idiomas. Reusar textos de
    `Proyectos\TubeMassDL\TubeMassDL\Services\Translations.cs` para claves compartidas
    (AppTitle, AppTagline, botones, About*...). Vérificar que `UpdateAvailable`
    (añadida en 7c78a08) queda en los 8 idiomas.
- Prompts IA en el idioma elegido:
  - `PromptGenerator.cs`: `AssignmentTemplate*`, `KnownCategories*`,
    `ConsolidationTemplate*` → 8 versiones; `idioma` resuelve por culture.
  - `ResponseParser.cs`: palabras clave `archivos/files/...` → por culture (o siempre
    aceptar ambas como hoy, el idioma del prompt define el output).
  - `ClassificationMode.cs` `TranslateCriterion`/`GetCriteria`: 8 idiomas.
  - Modos/criterios: `ClassificationModes` descripciones ES/EN → 8 idiomas.
- `ClasificadorIA/App.xaml.cs`:
  - `InitWindow`: 8 `LanguageItem` con flags locales pack
    (`flag-es`, `flag-uk`, `flag-fr`, `flag-de`, `flag-br`, `flag-it`, `flag-cn`, `flag-jp`).
    ComboMaxWidth actual 36 puede necesitar subir.
  - `LoadPreferences`/`SavePreferences`: guardar culture code string.
  - WireEvents LanguageChanged: `Translations.Current = Idioma.FromCulture(a.CultureCode)`.
  - `ApplyLanguage`: sin cambios estructurales, se soporta solo por Get().
- `ByokDialog` ctor recibe `Idioma` → pasar `Translations.Current` desde `OpenByokDialog`.

### Tests (SelfTest.cs)
- Existentes ES/EN siguen pasando (el enum crece, no rompe).
- Añadir `RunTranslations` asserts: unas 3-4 claves por idioma nuevo (ej. ActionButton
  FR/DE/PT/IT/JA/ZH no vacías y != EN).
- PromptGenerator: asertar template del idioma nuevo no vacío y ≠ al de EN.
- Verificar que una clave "Otros" traducida se use en `MetadataClassifier.NormalizeGenre`.

## Tarea 5 — Redimensionado / panel derecho
- Causa: en `App.InitWindow` `OptionsPanelMinWidth=340`; a ventana mínima (900px)
  la columna derecha (1*) queda ≈290 < 340 → desborde y borde cortado.
- Fix:
  - Bajar `OptionsPanelMinWidth` en `App.xaml.cs:111` a ~300.
  - `OptionsPanel.xaml`: `ScrollViewer` → `HorizontalScrollBarVisibility="Auto"`.
  - Revisar que la columna izquierda tenga un mínimo razonable (p.ej. 380) para que
    la lista no desaparezca; si hace falta ajustar `WindowMinWidth` (900 → 940).
- Validación visual del usuario.

## Gatillos (por cambio)
1. `dotnet build "ClasificadorIA.slnx" -v q --nologo` (Debug) → 0 errores 0 warnings.
2. Idem `-c Release`.
3. `ClasificadorIA\bin\Debug\net8.0-windows\ClasificadorIA.exe --selftest` → OK.
4. Smoke: abrir exe 5s → ALIVE (matar proceso antes de build).
5. User prueba: borrar (Supr, multiselect, X), toggle tema, label destino, idioma FR/DE,
   redimensionar al mínimo.

## Notas
- El commit 7c78a08 (fixes de revisión) está sin pushear → pushear con este plan.
- No tocar: carcasa de la librería (BaseMainViewModel, UpdateService ya en uso, etc.).
- Pasos de resumen ya aplicados: mirror del borrado, LocalClassifier nombres, update check.