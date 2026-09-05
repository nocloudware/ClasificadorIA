# Clasificador IA — Migración a plantilla NoCloudware.UI.Core

Fecha: 2026-09-05 · Estado: aprobado por usuario

## 1. Contexto y objetivo

ClasificadorIA es una app WPF de organización de archivos por categorías usando IA externa como intermediaria: genera un prompt, el usuario lo envía a una IA (manualmente vía portapapeles, o automáticamente con su propia API key), y la respuesta JSON se convierte en subcarpetas y archivos copiados/movidos al esquema resultante.

La app actual es una sola ventana (MainWindow, 630 líneas de code-behind, 0 dependencias NuGet, PI bilingual manual, UI propia).

**Objetivo:** reconstruir la app siguiendo la arquitectura y el estilo de TurnAFile y TubeMassDL, que consumen la plantilla NoCloudware.UI.Core (ShellWindow, BaseMainControl, tema Aether, LanguageSelector, ThemeToggle, StatusBar, FileListBox, AboutDialog).

## 2. Principio guía

> **NoCloudware.UI.Core es una referencia de desarrollo, no una dependencia viva.** Como hizo TubeMassDL, se copia la plantilla **dentro de este repo** como proyecto local y se modifica donde haga falta. TurnAFile fue el proyecto del que se extrajo la plantilla; TubeMassDL se construyó sobre una copia; ClasificadorIA hace lo propio.

## 3. Arquitectura objetivo

```
ClasificadorIA/
├── ClasificadorIA.slnx                      # Solución: ClasificadorIA + NoCloudware.UI.Core
├── NoCloudware.UI.Core/                     # ★ COPIA LOCAL del template (modificable)
├── ClasificadorIA/                          # App (WinExe, net8.0-windows)
│   ├── App.xaml / App.xaml.cs               # Composition root (patrón TubeMassDL) + --selftest
│   ├── AssemblyInfo.cs
│   ├── Assets/                              # clasificadoria.ico/png
│   ├── Models/                              # ClassificationMode, ClassificationResult, AiProvider, ByokConfig
│   ├── Services/                            # PromptGenerator, ResponseParser, FileOrganizer, FileFilters, AiClient, ByokConfigStore, Translations
│   ├── Panels/                              # OptionsPanel, ByokDialog
│   └── Resources/                           # TreeViewItemTemplate.xaml
├── README.md / LICENSE
└── (eliminados de la raíz) MainWindow.*, ProgressWindow.*, Resources.cs, App.*, AssemblyInfo.cs, Properties/, bin/, obj/
```

Dos proyectos: la app y la copia local del template (igual que TubeMassDL).

## 4. UI — ShellWindow + dos paneles

```
┌─────────────────────────────────────────────────────────────┐
│ Clasificador IA                       [🌐 flags][🌙/☀️]     │
├───────────────────────────────┬─────────────────────────────┤
│ 📄 Archivos (42)              │ 📂 Carpeta de origen        │
│ ┌───────────────────────────┐ │    [ruta][📁 Examinar]      │
│ │ ◾ INICIAL: FileListBox    │ │ 🎯 Modo [▾]                 │
│ │ ◾ TRAS CLASIFICAR:        │ │ 🔍 Criterio [▾]            │
│ │   TreeView                │ │ 🎚️ Profundidad [▾]         │
│ │   📁 Rock (12)            │ ├────────────────────────────┤
│ │     ├─ cancion1.mp3       │ │ 📌 Método (excluyente)     │
│ │   📁 Reggaetón (8)        │ │ ⭕ Manual (Copiar/Pegar)    │
│ └───────────────────────────┘ │ ⭕ Auto (BYOK)              │
│ ┌───────────────────────────┐ │ ├─ [panel del método] ─────┤
│ │ 📂 Destino [ruta][Cambiar]│ │ │ Manual: Generar Prompt   │
│ └───────────────────────────┘ │ │   → copia, msg "Promp-  │
│ 📊 42 | 0 | 42 | 0           │ │   to copiado a tu porta- │
│                              │ │   papeles"               │
│                              │ │   Pegar Respuesta/Cargar │
│                              │ │   [resultado]            │
│                              │ │   Clasificar             │
│                              │ │ Auto: Generar Prompt +   │
│                              │ │   BYOK (credenciales)    │
│                              │ │   [resultado]            │
│                              │ │   Clasificar             │
│                              │ ├────────────────────────────┤
│                              │ │ 📋 Copiar  ✂️ Mover       │
├───────────────────────────────┴─────────────────────────────┤
│ [ℹ️][💚] tips...                    [⚡ Organizar]  [🚪]    │
└─────────────────────────────────────────────────────────────┘
```

- **Panel izquierdo** (BaseMainControl): FileListBox plano al inicio; tras clasificar, un **TreeView** (categoría = carpeta, archivos debajo) ocupa esa zona vía el nuevo `FileListCustomContent`. Abajo, el selector de carpeta de **destino** del template (default = carpeta origen) y la StatusBar.
- **Panel derecho** (OptionsPanel inyectado en `OptionsContent`): todo el flujo.
- **Footer**: el botón de acción del template = **"Organizar"** = ejecuta Copiar/Mover al esquema resultante. Acerca de / Donar / Salir del template.

## 5. Alteración al template (copia local)

Añadir a `BaseMainControl` un `ContentControl` superpuesto en la zona de la lista (fila 1, patrón del `EmptyListText` existente):

- `xaml`: `<ContentControl x:Name="FileListCustomPanel" Visibility="Collapsed"/>` junto a `FileList` y `EmptyListText`.
- `code-behind`: DP `FileListCustomContent` (object). Al setear ≠ null → ocultar `FileList` y `EmptyListText`, mostrar el custom. Al setear null → restaurar.

Quien lo consume inyecta su TreeView ahí tras clasificar. Genérico, reutilizable, no rompe el resto de controles (default null = comportamiento actual).

## 6. Panel de opciones — estructura y métodos excluyentes

```
Carpeta de origen: [ruta][Exam]
Modo / Criterio / Profundidad (combos)
Método (RadioButtons excluyentes):
  ⭕ Manual (Copiar/Pegar)
  ⭕ Auto (BYOK)

MANUAL:
  [📋 Generar Prompt] → Clipboard.SetText + mensaje "Prompt copiado a tu portapapeles"
    (el prompt NUNCA se muestra)
  [📥 Pegar Respuesta] [📂 Cargar] → llenan el cuadro de resultado
  [cuadro de resultado, multilinea]
  [🏷️ Clasificar]
AUTO (BYOK):
  [📋 Generar Prompt] [🔑 BYOK]
    Generar Prompt habilitado SOLO con credenciales válidas guardadas.
    Con credenciales: genera prompt + AiClient.GenerateAsync → respuesta al cuadro.
  [cuadro de resultado]
  [🏷️ Clasificar]

Siempre visible:
  📋 Copiar / ✂️ Mover (radios) — lo ejecuta el botón "Organizar" del footer
```

`Clasificar` → `ResponseParser.Parse` → TreeView en el panel izquierdo.

## 7. BYOK — cliente multiproveedor propio (0 dependencias NuGet)

Almacenamiento: `%LOCALAPPDATA%\ClasificadorIA\byok.json` con `ByokConfig { Providers[], ActiveProviderId }`.

Presets (`AiProvider { Id, Name, Scheme, BaseUrl, ApiKey, Models[], Temperature, Enabled, ModelsEndpoint, ApiKeyUrl }`):

| Proveedor | Scheme | BaseUrl | Modelos iniciales | ApiKeyUrl |
|---|---|---|---|---|
| OpenAI | openai | https://api.openai.com/v1 | gpt-4o-mini, gpt-4o | https://platform.openai.com/api-keys |
| Claude (Anthropic) | anthropic | https://api.anthropic.com/v1 | claude-sonnet-4-5, claude-opus-4-1, claude-haiku-4-5 | https://console.anthropic.com/settings/keys |
| DeepSeek | openai | https://api.deepseek.com/v1 | deepseek-chat, deepseek-reasoner | https://platform.deepseek.com/api_keys |
| Google Gemini | gemini | https://generativelanguage.googleapis.com/v1beta | gemini-2.5-flash, gemini-2.5-pro | https://aistudio.google.com/apikey |
| Groq | openai | https://api.groq.com/openai/v1 | llama-3.3-70b-versatile, qwen-3-32b | https://console.groq.com/keys |
| Ollama (local) | openai | http://localhost:11434/v1 | (vacío, se listan) | (ninguno) |
| OpenRouter | openai | https://openrouter.ai/api/v1 | (se listan) | https://openrouter.ai/settings/keys |

+ **Providers personalizados**: todos los campos editables, scheme default `openai`.

`AiClient` — tres esquemas wire reales vía `HttpClient` (BCL):

| Scheme | LLM | Listado de modelos |
|---|---|---|
| openai | POST `{base}/chat/completions`, `Authorization: Bearer`, `response_format: json_object` → `choices[0].message.content` | GET `{base}/models` → `data[].id` |
| anthropic | POST `{base}/messages`, `x-api-key`, `anthropic-version: 2023-06-01` → `content[0].text` | GET `{base}/models` → `data[].id` |
| gemini | POST `{base}/models/{model}:generateContent`, `x-goog-api-key`, `generationConfig.responseMimeType:"application/json"` → `candidates[0].content.parts[0].text` | GET `{base}/models` → `models[].name` (strip `models/`) |
| ollama (listado) | — | GET `{base}/../api/tags` → `models[].name` |

Errores tipificados: `AiException(StatusCode, Code)` — 401/403 key inválida, 404 modelo/base errónea, 429 rate-limit, red/timeout, HTTP genérico.

**Ventana BYOK** (`ByokDialog`):
- Combo de proveedores (presets + "+ Agregar personalizado").
- Al seleccionar proveedor (con key) → `ListModelsAsync` → combo de modelos; sin key → lista preset + mensaje; fallo → fallback + aviso. Botón "↻ Recargar modelos".
- Modelos cacheados en `byok.json` tras éxito.
- Hipervínculo "Obtener API key →" (`Process.Start` UseShellExecute) — oculto si `ApiKeyUrl` vacío.
- Campos adaptados por scheme; temperatura; "Probar conexión" (envía ping corto y reporta error tipificado); Eliminar / Guardar / Cancelar.
- Probar/Grabar valida duplicados y proveedor activo.

## 8. Servicios núcleo (sin WPF)

| Servicio | Responsabilidad | Puerta de origen |
|---|---|---|
| `PromptGenerator.Generate(mode, criterion, depth, Idioma, files)` | Prompt ES/EN | `GenerarPrompt()` |
| `ResponseParser.Parse(respuesta, archivosReales, Idioma)` → `List<ClassificationResult>` | Extrae JSON `{`…`}`, acepta `categorias`/`categories`, normalización case-insensitive, descarta inexistentes y categorías vacías | `ProcesarRespuesta()` |
| `FileOrganizer.Organize(dest, results, copiar, cts, onProgress)` → stats | Crea subcarpetas saneadas, copia/mueve, cancela | `BtnIniciar_Click` + `LimpiarNombre` |
| `FileFilters.IsSystemFile(name)` | `.exe .dll .ps1 .bat .cs .csproj` | `CargarArchivos()` |
| `Translations` | Strings UI es/en (diccionario estático) | `_textosUI` |
| `ClassificationMode` + traducción de criterios | Modos: Genérico/Música/Películas/Series/Libros | `_modos` + `_traduccionesCriterios` |

Confirmación "¿MOVER archivos?" antes de organizar se conserva.

## 9. i18n y tema

- Idioma: `Translations` es/en (patrón TubeMassDL; el LanguageSelector del template muestra 8 banderas, solo es/en activas al inicio). `ApplyLanguage(ci)` re-etiqueta DPs del shell + OptionsPanel.
- Tema: `ThemeService` + `ThemeToggle` del template; default según preferencia guardada (dark por defecto). Preferencias (DarkTheme, Language, DefaultOutputPath) en `appsettings.json` junto al exe (patrón TubeMassDL).

## 10. Preferencias de usuario

`appsettings.json` (junto al exe): `{ Settings: { DarkTheme, Language, DefaultOutputPath } }`. Guardar al salir y en cambios (tema/idioma/carpeta destino). `byok.json` separado en `%LOCALAPPDATA%` (credenciales).

## 11. Verificación

- `dotnet build` → 0 errores / 0 warnings en cada task (regla de la casa).
- `ClasificadorIA --selftest` → exit 0. Cubre: PromptGenerator, ResponseParser, FileOrganizer, FileFilters, ByokConfigStore (roundtrip/corrupción), payloads/extracción/URLs de AiClient (offline), FileListCustomContent (DP toggle).
- Smoke manual E2E: carpeta → Manual (generar→copiar→pegar→clasificar→treeview→organizar) y Auto (BYOK→generar→clasificar→organizar).

## 12. Fuera de alcance (YAGNI)

- Idiomas adicionales, shell extension COM, update service, donate — expuestos por el template pero sin configurar salvo que aporten al flujo (About sí, con datos de la app).
- Otros proveedores AE más allá del esquema `AiClient` (3 wire formats).