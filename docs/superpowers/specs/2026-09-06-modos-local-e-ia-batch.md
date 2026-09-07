# Clasificador IA — Modos Local e IA (clasificación por lotes)

Fecha: 2026-09-06 · Estado: aprobado por usuario (preguntas respondidas 06/09/2026)

## 1. Contexto y problema

Problema reportado por el usuario: clasificar 200 archivos con BYOK en una sola
llamada supera tiempo (timeout 120s) y contexto del LLM; solo DeepSeek sobrevive
y exige "continuar" varias veces. Síntoma del mismo límite: el chat manual normal
también se cae.

Decisión de alcance del usuario:

- El esquema de batches + consolidación **solo aplica al modo IA (BYOK)**.
- El modo manual **pierde todo sentido** con ese esquema. Queda **solo Local**:
  ordenamiento/clasificación por **nombre y tema con algoritmo local** (sin LLM).
- Se eliminan botones *generar prompt / copiar / pegar / load* y el **campo de
  prompt** (ResponseBox). Solo queda el botón **Clasificar**.

Preguntas respondidas por el usuario:

1. Algoritmo local → **Tema por tokens comunes + fallback por extensión**.
2. Profundidad → **aplica también en modo Local**.
3. Botón BYOK → **se mantiene, visible solo en modo IA**.
4. Layout → **todo dentro de Opciones en ese orden** (dedup integrado, no panel
   separado).

## 2. Layout final del panel de opciones

Orden vertical único dentro de `OptionsPanel.xaml`:

1. **Título "Opciones"** + subtítulo (encabezado ya existente).
2. **Eliminar duplicados** — sección integrada: título + 3 checkboxes
   (mismo nombre / menor tamaño / menor fecha). El `UserControl DedupPanel`
   desaparece.
3. **Método** — radios `Local` / `IA` (renombrados desde `Manual` / `Auto (BYOK)`).
   Botón **"BYOK"** visible solo en modo IA.
4. **Modo** — habilitado solo en IA; deshabilitado en Local.
5. **Criterio** — habilitado solo en IA; deshabilitado en Local.
6. **Profundidad** (5/10/15) — activo en ambos modos.
7. **Tamaño de lote** — nuevo `ui:NumberBox`, Min 10 / Max 50 / Valor 20, activo
   solo en IA.
8. **Organización** — radios Copiar / Mover (sin cambios).
9. **Botón Clasificar** + hint.

Eliminados: sección Prompt, sección Respuesta, `ResponseBox`, `GeneratePromptBtn`,
`CopyPromptBtn`, `PasteResponseBtn`, `LoadResponseBtn`.

## 3. Modo IA — clasificación por lotes + consolidación

Nuevo `Services/BatchClassifier.cs` (orquestación testeable, sin UI). Flujo:

**Fase 1 — asignación por lote.** Chunks de `tamañoLote` (campo 10-50, default 20).
Por cada chunk: `PromptGenerator.AssignmentBatch(mode, criterion, idioma, chunk)` →
`AiClient.GenerateAsync` → `ResponseParser.ParseBatchAssignments`. Estado por lote
en la UI ("Clasificando lote X/Y…").

Formato de response por lote:
```json
{"archivos":[{"archivo":"x.mp3","categoria":"Rock"}]}
```
(EN: `{"files":[{"file":"x.mp3","category":"Rock"}]}`.)

**Fase 2 — consolidación.** 1 llamada con las categorías distintas (conteo +
1-2 ejemplos) → `ResponseParser.ParseConsolidationMap`:
```json
{"finales":{"Rock":["rock","rock clasico"],"Pop":["pop"]}}
```
(EN: `{"final":{...}}`.) Remapeo programático y determinista: cada archivo sigue
`categoríaLote → categoríaFinal`. Ninguno se pierde.

**Robustez:**
- Batch que falla (AiException) → no detiene el flujo; esos archivos van al
  bucket `Otros` y NO entran a la consolidación.
- Categoría olvidada por la IA en el mapeo de consolidación → archivos a `Otros`.
- Consolidación devuelta con > `depth` finales → se conservan las `depth-1`
  mayores y el resto se fusiona en `Otros` (helper compartido `CapToDepth`).
- Consolidación no parseable → se usan las categorías de lote crudas como finales.

Secuencial (sin rate-limit).

## 4. Modo Local — algoritmo local

Nuevo `Services/LocalClassifier.cs`:

1. Tokenizar nombre sin extensión: lowercase, separar por no-alfanumérico,
   filtrar stopwords ES/EN y tokens < 3 chars.
2. Frecuencia por token sobre el total de archivos. **Tema** = token compartido
   por ≥2 archivos. El archivo se asigna al token compartido más frecuente que
   contiene → categoría = token capitalizado.
3. Archivos sin tema → **fallback por extensión**: `Audio` / `Video` / `Imagen` /
   `Documento` / `Otros` (grupos fijos por extensión).
4. **Profundidad**: si el total de categorías supera `depth`, se conservan las
   `depth-1` más grandes y el resto se fusiona en `Otros` (`CapToDepth`).

`CapToDepth` (compartido IA/Local) garantiza como máximo `depth` categorías;
bucket `Otros` con nombre traducido (`Es: Otros, En: Others`).

## 5. Cambios de interfaz

`App.xaml.cs`:
- `_dedupPanel` como control separado desaparece (integrado en OptionsPanel);
  el derecho columna pasa a ser solo `OptionsPanel`.
- `ClassifyAsync` delega: `Local` → `LocalClassifier.Classify`; `IA` →
  `new BatchClassifier(...).ClassifyAsync`.
- Eliminados `ClassifyManual`, `ClassifyAuto` viejo, `RegeneratePrompt`,
  `CopyPrompt`, `PasteResponse`, `LoadResponse`, `_currentPrompt`, y los
  wirings de `ResponseBox.TextChanged` y `SelectionChanged→RegeneratePrompt`.

`PromptGenerator` y `ResponseParser`:
- `PromptGenerator.Generate` y `ResponseParser.Parse` (formato "categorías" con
  todos los archivos) **se eliminan** — quedan huérfanos por este cambio. Sus
  tests de SELF-TEST se reemplazan por los nuevos.
- `PromptGenerator` gana `AssignmentBatch` y `Consolidate` (plantillas ES/EN).
- `ResponseParser` gana `ParseBatchAssignments` y `ParseConsolidationMap`.

## 6. Verificación

- `dotnet build` → 0 errores / 0 warnings.
- `ClasificadorIA --selftest` → `SELF-TEST: OK`.
- Smoke (proceso vivo a 5s).
- SELF-TEST nuevos:
  - parseadores de asignación por lote y de mapeo de consolidación;
  - `BatchClassifier` con cliente fake: chunks, falla de lote → `Otros`,
    categoría sin mapear → `Otros`, recorte por `depth`;
  - `LocalClassifier`: tema por tokens, fallback por extensión, recorte por
    `depth`;
  - `CapToDepth` (máximo `depth` categorías, bucket `Otros`).

## 7. Traducciones

- Renombradas: `ManualMode`→`MethodLocal` ("Local"/"Local"),
  `AutoMode`→`MethodIA` ("IA"/"IA").
- Nuevas: `BatchSize` ("Tamaño de lote"/"Batch size"),
  `BatchStatus` ("Clasificando lote {0}/{1}…"/"Classifying batch {0}/{1}…"),
  `Otros` ("Otros"/"Others"), `Audio`, `Video`, `Imagen`/`Image`, `Documento`.
- Huérfanas eliminadas (causadas por este cambio): `GeneratePrompt`,
  `PromptCopied`, `PromptEmpty`, `PasteResponse`, `NoneInClipboard`,
  `LoadResponse`, `Copy`, `ResultPlaceholder`, `PasteFirst`, `JsonNotFound`,
  `Classifying`.
- `AutoByokHint` se conserva (hint del modo IA).

## 8. Restricciones globales

- Build 0 errores/0 warnings; SELF-TEST OK por task que toca lógica.
- 0 paquetes NuGet nuevos (WPF-UI 4.3 ya referenciado; `NumberBox` ya disponible).
- Strings ES/EN vía `Translations`; idioma según prefs.
- Commit local al terminar; push solo con permiso explícito del usuario.
- `d302983` (bindings BYOK) sigue sin pushear — no se toca en este trabajo.