# Clasificador IA — Migración a plantilla NoCloudware.UI.Core

> **Para agentes:** ejecución inline en esta sesión (subagentes no operativos: provider 9router caído).

**Goal:** Reconstruir ClasificadorIA sobre la plantilla NoCloudware.UI.Core (ShellWindow + BaseMainControl + Aether), con panel de opciones (Manual/Auto BYOK excluyentes), TreeView de categorías tras clasificar, y cliente IA multiproveedor propio (0 NuGets).

**Architecture:** Copia local del template en el repo (patrón TubeMassDL), app `ClasificadorIA/` con composition root en App.xaml.cs, lógica sin WPF (Models/Services), BYOK con 3 wire formats (openai/anthropic/gemini), verificación vía `--selftest`.

**Tech Stack:** .NET 8, WPF, WPF-UI 4.3.0 (transitivo del template), CommunityToolkit.Mvvm (transitivo), System.Text.Json, System.Net.Http (BCL). 0 NuGet nuevos en la app.

**Spec:** `docs/superpowers/specs/2026-09-05-clasificadoria-migracion-design.md`

## Global Constraints

- `dotnet build` → 0 errores / 0 warnings en cada task.
- `ClasificadorIA --selftest` → exit 0 (gate de cada task que toca lógica).
- 0 paquetes NuGet nuevos (app y template copia local tal cual).
- Nombres/strings: es/en vía `Translations`; idioma por defecto: según prefs o sistema.
- El prompt NUNCA se muestra en pantalla (solo botón + confirmación de copia).
- Método Manual/Auto excluyentes por RadioButton.
- Credenciales: `%LOCALAPPDATA%\ClasificadorIA\byok.json`; prefs: `appsettings.json` junto al exe.
- Commit local por task completada.

---

### Task 1 — Scaffold: copia local del template + proyecto app

**Files:**
- Copy: `NoCloudware.UI.Core/` desde `C:\Users\chand\Desktop\Proyectos\Nocloudware.UI.Core\NoCloudware.UI.Core` (sin `obj/`, `bin/`, `.vs/`)
- Create: `ClasificadorIA/ClasificadorIA.csproj`, `ClasificadorIA/App.xaml(.cs)`, `ClasificadorIA/AssemblyInfo.cs`, `ClasificadorIA/Assets/`
- Rewrite: `ClasificadorIA.slnx`
- Delete raíz: `MainWindow.xaml(.cs)`, `ProgressWindow.xaml(.cs)`, `Resources.cs`, `App.xaml(.cs)`, `AssemblyInfo.cs`, `ClasificadorIA.csproj`, `ClasificadorIA.csproj.user`, `Properties/`, `bin/`, `obj/`
- Move: `clasificadoria.ico/png` → `ClasificadorIA/Assets/`
- Modify: `.gitignore` (si falta bin/obj en subcarpetas)

**Steps:**
1. Copiar carpeta template (sin artefactos).
2. Proyecto app: net8.0-windows, WinExe, UseWPF, ProjectReference → `..\NoCloudware.UI.Core\NoCloudware.UI.Core.csproj`, ApplicationIcon = Assets/clasificadoria.ico, Assets como `Resource`.
3. `AssemblyInfo.cs` con `[assembly: ThemeInfo(...)]`.
4. `App.xaml` merge: ui:ThemesDictionary(Dark), ui:ControlsDictionary, AetherTheme pack URI.
5. `App.xaml.cs`: minimal `OnStartup` — ThemeService dark → ShellWindow con DPs básicas → placeholder OptionsContent → Show().
6. Borrar raíz, mover assets, reescribir `.slnx`.
7. `dotnet build` 0/0. Commit.

---

### Task 2 — Template (copia local): `FileListCustomContent`

**Files:**
- Modify: `NoCloudware.UI.Core/Controls/BaseMainControl.xaml` y `.xaml.cs`

**Interfaces:**
- Produce: `public object FileListCustomContent` (DP). ≠ null → `FileList.Visibility=Collapsed`, `EmptyListText.Visibility=Collapsed`, `FileListCustomPanel.Visibility=Visible`; null → restaura.

**Steps:**
1. XAML: `<ContentControl x:Name="FileListCustomPanel" Visibility="Collapsed"/>` en el Grid de fila 1 (junto a FileList y EmptyListText).
2. DP `FileListCustomContent` con `PropertyChangedCallback` que aplica visibilidades.
3. Chequeo en `--selftest` (Task 3): instanciar `BaseMainControl` (STA), setear DP, assert.
4. Build 0/0. Commit.

---

### Task 3 — Núcleo sin WPF + `--selftest`

**Files (Create):**
- `Services/ClasificadorIA/Models/Idioma.cs` — enum `Idioma { Español, Inglés }`
- `ClasificadorIA/Models/ClassificationMode.cs` — modos + descripciones + criterios + `TranslateCriterion(string, Idioma)`
- `ClasificadorIA/Models/ClassificationResult.cs` — `record ClassificationResult(string Category, List<string> Files)`
- `ClasificadorIA/Services/PromptGenerator.cs`
- `ClasificadorIA/Services/ResponseParser.cs`
- `ClasificadorIA/Services/FileOrganizer.cs`
- `ClasificadorIA/Services/FileFilters.cs`
- `ClasificadorIA/Services/Translations.cs`
- Modify: `ClasificadorIA/App.xaml.cs` — rama `--selftest`

**Interfaces:**
```
PromptGenerator.Generate(mode, criterion, depth, Idioma, files) → string
ResponseParser.Parse(response, realFiles, Idioma) → List<ClassificationResult>
FileOrganizer.Organize(dest, results, copy, cts, onProgress(int,int,int)) → (int processed, int errors)
FileFilters.IsSystemFile(name) → bool
Translation keys → string (static)
```

**Steps:**
1. Portar modos, traducciones de criterios, `_textosUI` → `Translations`, plantillas prompt → `PromptGenerator` (1:1 del código actual).
2. `ResponseParser`: extraer JSON `{`…`}`, key `categorias`/`categories` según idioma con fallback, normalización `Trim().ToUpperInvariant()` y match case-insensitive vs archivos reales, descartar inexistentes y categorías vacías.
3. `FileOrganizer`: porta copiar/mover + cancelación + `LimpiarNombre` (caracteres inválidos→`_`, máx 50) → `SaneateFolderName`; conteo procesados/errores.
4. `FileFilters`: `IsSystemFile`.
5. `App.OnStartup` rama `--selftest`: no abre UI, corre asserts, `Environment.Exit(0|1)`.
6. Run `--selftest` → 0. Build 0/0. Commit.

---

### Task 4 — BYOK: presets, ByokConfigStore, AiClient

**Files (Create):**
- `ClasificadorIA/Models/AiProvider.cs` — clase con presets estáticos + custom
- `ClasificadorIA/Models/ByokConfig.cs`
- `ClasificadorIA/Services/ByokConfigStore.cs`
- `ClasificadorIA/Services/AiClient.cs`

**Interfaces:**
```
AiClient.GenerateAsync(AiProvider, string prompt, CancellationToken) → string
AiClient.ListModelsAsync(AiProvider, CancellationToken) → IReadOnlyList<string>
ByokConfigStore.Load() → ByokConfig   // corrupto → presets
ByokConfigStore.Save(ByokConfig)
// internal para selftest:
AiClient.BuildPayload(provider, prompt) → string  (JSON del cuerpo)
AiClient.ExtractResponseText(provider, httpJson) → string
AiClient.BuildListModelsUrl(provider) → Uri
```
Excepción: `AiException(int StatusCode, string Code, string Message)`.

**Steps:**
1. Presets (tabla del spec §7) + `ByokConfig`.
2. `ByokConfigStore` con roundtrip + corrupción → presets.
3. `AiClient`: 3 schemes (payload, headers, extracción, URL listado; Ollama listado vía `/api/tags`).
4. Extend `--selftest` offline (payloads, extracción, URLs, presets, store).
5. Run selftest → 0. Build 0/0. Commit.

---

### Task 5 — ByokDialog

**Files (Create):** `ClasificadorIA/Panels/ByokDialog.xaml(.cs)`

**Interfaces:**
```
ByokDialog(ByokConfig config, ByokConfigStore store, AiClient client, Idioma idioma)
ShowDialog() → bool (true=OK guardado)
ByokConfig Config → registro resultante
```

**Steps:**
1. Layout: proveedor combo (+ "＋ Agregar personalizado"), campos por scheme, modelo combo + edición libre, "↻ Recargar modelos", link API key, temperatura, Probar, Eliminar/Guardar/Cancelar.
2. Combo proveedor → si key → ListModelsAsync → combo + cache; sin key → preset + nota; fallo → fallback + aviso.
3. Link vía `Process.Start` UseShellExecute.
4. Probar conexión con ping corto → resultado/errores tipificados.
5. Guardar valida + persiste. Build 0/0. Commit.

---

### Task 6 — OptionsPanel

**Files (Create):** `ClasificadorIA/Panels/OptionsPanel.xaml(.cs)` + strings en `Translations`

**Interfaces:**
```
OptionsPanel(PromptGenerator, ResponseParser, AiClient, ByokConfigStore)
event FolderSelected(string folderPath)
event ClassifyRequested(List<ClassificationResult>)
event LogStatus(string text)
void ApplyLanguage(Idioma)
void SetClassificationModeFromFiles(IReadOnlyList<string>)
void Reset()
```

**Steps:**
1. XAML con secciones (carpeta origen, combos, radios excluyentes Manual/Auto, sub-paneles, cuadro resultado, Clasificar, Copiar/Mover).
2. Radio `Checked` → muestra sub-panel correspondiente; oculta el otro.
3. Generar Prompt (manual): `PromptGenerator.Generate` + `Clipboard.SetText` + label "Prompt copiado a tu portapapeles".
4. Pegar Respuesta (Clipboard) / Cargar (OpenFileDialog txt/json) → cuadro.
5. Auto: "Generar Prompt" habilitado con credenciales activas guardadas; llama `AiClient.GenerateAsync` → cuadro. Botón "BYOK" abre `ByokDialog`.
6. Clasificar: `ResponseParser.Parse` → `ClassifyRequested` event.
7. Build 0/0. Commit.

---

### Task 7 — Composition root + flujo E2E

**Files:**
- Rewrite: `ClasificadorIA/App.xaml.cs`
- Create: `ClasificadorIA/Resources/TreeViewItemTemplate.xaml`

**Steps:**
1. Prefs (appsettings.json) load/save.
2. InitWindow: ShellWindow con DPs completas (Title, DropText, ActionButtonText="Organizar", etc.), icono Assets, `ShowSelectFilesButton=Collapsed`, `OptionsPanelMinWidth=320`.
3. LanguageSelector + `ApplyLanguage` (zonda todos los DPs + OptionsPanel).
4. ThemeToggle → ThemeService + save.
5. AboutClick → AboutDialog (app info + licencias WPF-UI/MVVM); DonateClick → DonationService; ExitClick.
6. Servicios + OptionsPanel en `OptionsContent`; suscribir eventos.
7. FolderSelected → listar archivos filtrados → `_window.Files` (BaseFileItem) → `UpdateCounters` → OutputFolderText default = origen.
8. ClassifyRequested → construir TreeView (categorías → archivos) → `MainControl.FileListCustomContent`; guardar mapping para estados.
9. ActionClick (Organizar) → confirmación → `FileOrganizer` en Task.Run con cts → progreso (UpdateCounters + barra global) → marcar Status de items → messagebox resultado.
10. `--selftest` antes de Show; crash log.
11. Build 0/0 + smoke manual. Commit.

---

### Task 8 — Pulido y edge cases

- Cancelación activa, 0 archivos/JSON inválido/sin carpetas → mensajes localizados, `Sin categoría`, dedupe colisiones, scroll de cuadros, reset.
- `--selftest`, build 0/0, smoke. Commit.

---

### Task 9 — Docs + cierre

- README reescrito (estructura, uso, BYOK, selftest).
- Build final 0/0, `--selftest` 0, checklist manual. Commit.