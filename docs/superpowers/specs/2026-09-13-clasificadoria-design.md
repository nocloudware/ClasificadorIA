# Clasificador IA — Diseño (desde cero)

Fecha: 2026-09-13 · Alcance: documento de diseño y decisiones de arquitectura.

> Este documento describe la app **como si se fuera a construir desde cero**: qué hace, cómo está estructurada, cómo fluye la información y por qué se tomaron las decisiones clave. Sirve de mapa para quien llegue al repo sin contexto y para guiar cambios futuros.

---

## 1. Qué hace la app

**Clasificador IA** organiza archivos de tu disco en carpetas temáticas usando una IA. Tú guardas los archivos en una carpeta "caja" y la app, ayudada por un modelo de IA, decide a qué categoría pertenece cada uno y te sugiere una estructura de carpetas. Tú apruebas los cambios y la app **copia o mueve** los archivos a donde corresponde.

Dos formas de usarla:

- **Manual:** la app genera un *prompt* listo (un texto con el criterio y la lista de archivos). Lo copias, se lo pegas a tu IA favorita, y pegas de vuelta la respuesta en JSON. No requiere configurar nada.
- **Auto (BYOK — bring your own key):** configuras una vez tu proveedor de IA y tu clave API propia, y la app habla directa con el modelo sin salir de ella. Nunca se usan servidores ajenos; la clave vive en tu equipo.

---

## 2. Requisitos (funcionales y no funcionales)

| ID | Requisito |
|---|---|
| F1 | Cargar archivos por arrastrar-y-soltar, botón o carpeta completa. |
| F2 | Extraer metadatos (nombre, tamaño, fecha, categoría) en segundo plano, sin congelar la ventana. |
| F3 | Clasificar por modo (Genérico, Música, Películas, Series, Libros) y nivel de profundidad (5/10/15 categorías). |
| F4 | Ver el resultado como un **árbol tipo Explorador de Windows**: carpetas y categorías **plegadas al cargar**; un clic expande o colapsa cada rama. |
| F5 | Aplicar la organización real: copiar o mover a subcarpetas creadas, con progreso y cancelación. |
| F6 | Método manual (portapapeles) y método auto (BYOK) para conectarse a la IA. |
| N1 | **0 dependencias NuGet**; solo .NET base (System.Text.Json, System.Net.Http, HttpClient). |
| N2 | Windows + WPF; arquitectura por capas (núcleo UI reutilizable separado de la lógica de negocio). |
| N3 | Bilingüe (español/inglés) conmutable en vivo; tema claro/oscuro. |
| N4 | Auto-prueba ejecutable `--selftest` que verifica la lógica sin abrir la UI. |

---

## 3. Arquitectura (capas)

Separación clásica: la **lógica no sabe de píxeles**, y la **UI no sabe de reglas de negocio**.

```
App (ClasificadorIA)
│  App.xaml / App.xaml.cs        → composición: arma ventana, servicios, tema, idioma
│  MainWindow / ShellWindow*     → ventana y navegación
│  Panels/                       → paneles de la ventana (opciones, BYOK, resultados)
│  Services/                     → lógica pura: clasificación, IA, organización, selftest
│  Models/                       → BaseFileItem, TreeFolderNode, resultados, config
│
└── NoCloudware.UI.Core/         → plantilla de núcleo UI reutilizable (copia local):
     Controls/                   → FileListBox (árbol+spinner), Spinner, DropZone
     Themes/, LanguageSelector, ShellWindow, AboutDialog
```

### Reglas de la división

- `Services/` y `Models/` NO referencian WPF: se prueban con `--selftest` sin ventana.
- `NoCloudware.UI.Core` es una **copia local del template** (no una dependencia viva): se modifica aquí donde haga falta, siguiendo el patrón de proyectos hermanos.
- La composición (qué ventana, qué tema, qué idioma) vive solo en `App.xaml.cs`; los paneles no crean servicios.

---

## 4. Modelo de datos

```
BaseFileItem            Un archivo cargado.
  FilePath              Ruta absoluta del original.
  FileName              Nombre simple.
  SourceFolder          Carpeta contenedora donde vive el archivo.
  Category              Categoría asignada (por IA o manual).
  + metadatos           Tamaño, fecha, duración, etc. (BaseMetaItem-derived).
  + TreeDepth/Depth     Nivel de anidamiento en el árbol de resultados.

TreeFolderNode          Una fila de carpeta/categoría en el árbol.
  Key                   Identificador estable para el estado de la fila.
  DisplayName           Nombre que se ve (carpeta o categoría).
  Depth                 Nivel de indentación.
  IsExpanded            Si la rama está desplegada.
  Files                 Archivos directos (hijos inmediatos).
  Folders               Subcarpetas (hijos anidados).
```

### El árbol de resultados (clave del diseño)

El resultado NUNCA es una lista plana: es un **árbol jerárquico** con dos modos visuales:

1. **Modo carpetas** — la raíz es la carpeta `SourceFolder` de cada archivo; las rutas internas (`C:\musica\rock\b.mp3`) crean **subcarpetas anidadas** (`rock` dentro de `musica`) con su archivo adentro.
2. **Modo categorías** — la raíz es la `Category` asignada; cada categoría es una rama y sus archivos cuelgan de ella.

**Comportamiento por defecto:** el árbol carga **colapsado** (igual que el Explorador al abrir una carpeta):

- Solo se ven las **raíces** de carpeta/categoría (con su número de archivos en negrita).
- Los archivos y subcarpetas están **ocultos** hasta que el usuario hace clic para expandir.
- Un clic en una fila **expande/colapsa** esa rama; el estado se conserva al recargar.
- Las subcarpetas se muestran **indentadas** (Depth 1, 2, …) para reflejar la jerarquía real.

Esto se implementa con dos colecciones por instancia: `_collapsed` (qué ramas están plegadas) y `_seeded` (qué ramas ya se plegaron al simplificar), que se siembran al construir las filas y se consultan en `BuildFolderRows`/`BuildCategoryRows` → `SeedCollapsed`. La primera vez que una rama aparece se marca como colapsada; el toggle posterior del usuario es quien cambia de estado.

---

## 5. Flujo de datos (ciclo principal)

```
1. Cargar    Items.Add(a,b,c) ──► extracción de metadatos en Task.Run
            (spinner "IsBusy" visible mientras corre)
2. Construir RebuildRows() ──► BuildFolderRows | BuildCategoryRows
            (árbol plegado: raíces visibles, archivos ocultos)
3. Clasificar
   Manual:  PromptGenerator → usuario pega en su IA → ResponseParser(JSON)
   Auto:    AiClient(provider, apiKey) → respuesta parseada igual
4. Árbol    box.Rows = FlattenFolder(BuildNested...) con Depth e IsExpanded
5. Aplicar  FileOrganizer: copiar/mover a subcarpetas creadas, progreso + cancelar
```

### Cómo se generan y confirman los cambios

- Se **nunca toca el origen** hasta el paso 5, y solo después de que el usuario elige **copiar o mover**.
- Categorías sin archivos no crean carpetas vacías.
- Cambios destructivos no son automáticos: el usuario revisa el árbol antes de "Organizar".

---

## 6. Conexión con la IA (BYOK y manual)

- **Manual:** `PromptGenerator` produce el criterio + lista de archivos como texto. La app muestra un botón para **copiar** el prompt; el usuario pega la respuesta JSON debajo y la app la parsea con `ResponseParser`.
- **Auto (BYOK):** `ByokConfigStore` guarda proveedores+claves en `%LOCALAPPDATA%\ClasificadorIA\byok.json` (cifrado local, nunca se sube). `AiClient` enruta al proveedor elegido (ChatGPT, Claude, DeepSeek, Gemini, Ollama, Mistral, Groq…) usando `HttpClient` + `System.Text.Json` directamente — **0 NuGet**.
- Todo el tráfico sale de tu equipo con tu propia clave; la app no tiene cuenta ni servidor propio.

---

## 7. Auto-prueba y verificación

La app incluye una suite `SelfTest` que se ejecuta sin UI:

```
ClasificadorIA.exe --selftest     → "SELF-TEST: OK" (exit 0)
```

Cubre: parser de JSON, filtros de archivos de sistema, clasificador local determinista, árbol plegado (carpetas y categorías), traducciones, BYOK y flujo completo fin-a-fin (escribe/lee archivos reales en una carpeta temporal y los borra).

**Cada cambio debe verificar:** build `0 errores/0 advertencias` en Debug y Release + `--selftest` OK + arranque normal sin caída.

---

## 8. Decisiones registradas (ADR resumen)

| Decisión | Por qué |
|---|---|
| 0 NuGet (solo BCL) | Simplicidad, sin superficie de ataque, sin licencias a gestionar. |
| Copia local del template UI | El core es referencia a modificar en este repo, no dependencia externa. |
| Árbol plegado por defecto | Usabilidad igual al Explorador de Windows; el usuario controla qué ver. |
| BYOK en lugar de cuenta propia | Privacidad (la clave es del usuario) y cero infraestructura a mantener. |
| Lógica sin WPF | Testeable con `--selftest` en consola pura. |

---

## 9. Guía para desarrollarse desde cero

1. Crear la solución con dos proyectos: `ClasificadorIA` (app WPF) y `NoCloudware.UI.Core` (núcleo UI de la plantilla).
2. Implementar **modelos + servicios puros** primero (sin UI) y su `SelfTest` (puntos 4-5-7): la lógica se prueba antes de tocar XAML.
3. Traer la plantilla UI (ShellWindow, FileListBox, tema, idioma) y pegar la lógica en `App.xaml.cs` como composición.
4. Añadir el árbol de resultados con colapso por defecto (punto 4) y el spinner de carga (punto 5).
5. Cerrar con BYOK + manual + organizador, y re-correr `--selftest` + smoke en cada paso.
