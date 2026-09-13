# 🗂️ Clasificador IA de Archivos

[![Licencia MIT](https://img.shields.io/badge/Licencia-MIT-green.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
[![Plataforma](https://img.shields.io/badge/Plataforma-Windows-0078D6?logo=windows)](https://github.com/adriangoodrich/ClasificadorIA/releases)

> Aplicación de escritorio para Windows que clasifica automáticamente tus archivos en carpetas temáticas. Compatible con cualquier IA que responda JSON (`categorias`): ChatGPT, Claude, DeepSeek, Gemini, Ollama y más.

---

## ✨ Características

- 🖥️ **Interfaz moderna** construida sobre la plantilla `NoCloudware.UI.Core` (ShellWindow + Aether), tema **claro/oscuro** conmemutable.
- 🌐 **Bilingüe** — español / inglés conmutable en vivo.
- 📁 **Entrada flexible** — arrastra y suelta archivos, selección de archivos, o elige una carpeta completa de origen.
- 🎯 **Modos de clasificación** especializados: Genérico, Música, Películas, Series y Libros.
- 🔢 **Niveles de profundidad** ajustables: 5, 10 o 15 categorías aproximadas.
- ✍️ **Método Manual** — la app genera un prompt optimizado (criterio + lista de archivos), lo copias a tu IA favorita y pegas (o cargas) la respuesta JSON.
- 🤖 **Método Auto (BYOK)** — bring-your-own-key: configuras tu proveedor y API key una vez, y la app clasifica directamente sin salir de ella. Sin cuentas de terceros, 0 NuGet.
- 👁️ **Árbol tipo Explorador** — tras clasificar, muestra carpetas y categorías como árbol **plegado por defecto** (igual que Windows al abrir una carpeta): raíces y categorías visibles, archivos y subcarpetas ocultos hasta que expandes. Clic en una carpeta la expande/colapsa.
- ⚙️ **Procesamiento final** — copiar o mover los archivos a las subcarpetas creadas, con progreso en vivo y cancelación.
- 🛡️ **Manejo robusto** — normalización de nombres y filtrado de archivos de sistema (`.exe`, `.dll`, etc.).

---

## 🖥️ Requisitos del sistema

| Requisito | Versión |
|---|---|
| Sistema operativo | Windows 10 / Windows 11 |
| .NET Runtime | [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) o superior |
| Arquitectura | x64 |

---

## 🚀 Compilar desde el código fuente

#### Prerrequisitos

- [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/) con extensión C#
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

#### Pasos

```bash
git clone https://github.com/adriangoodrich/ClasificadorIA.git
cd ClasificadorIA
dotnet build ClasificadorIA.slnx --configuration Release
dotnet run --project ClasificadorIA
```

---

## 📖 Guía de uso

### Paso 1 — Carga los archivos

Arrastra y suelta archivos, usa **"Seleccionar archivos"**, o elige una **carpeta de origen** completa desde el panel derecho. Los metadatos se extraen en segundo plano (spinner de carga centrado).

### Paso 2 — Configura la clasificación

En el panel derecho elige el **modo** (Genérico, Música, Películas, Series, Libros), el **criterio** y el **nivel de profundidad**. Elige también **Método Manual** o **Método Auto (BYOK)**.

### Paso 3 — Clasifica

- **Manual:** la app marca el prompt (se copia con un clic). Pégalo en tu IA favorita, y cuando te devuelva la respuesta en **formato JSON con la clave `categorias`**, pégalo o cárgalo. Pulsa **"Clasificar"**.
- **Auto:** configura tu proveedor y API key en el botón **BYOK** (véase abajo). Pulsa **"Clasificar"** y la app se conecta a la IA automáticamente.

Verás el **árbol de categorías** con el número de archivos por categoría.

### Paso 4 — Aplica la clasificación

Elige **Copiar** o **Mover**, define la carpeta de salida y pulsa **"Organizar"**. El progreso se muestra en la barra de estado; puedes **cancelar** en cualquier momento.

---

## 🔑 Modo Auto (BYOK)

Sin cuentas ni claves de la aplicación — traes tu propia clave de tu proveedor de IA.

1. En el panel derecho, activa **Método Auto** y pulsa el botón **BYOK**.
2. Elige un proveedor (ChatGPT, Claude, DeepSeek, Gemini, Ollama, Mistral, Groq…), pega tu **API key** y ajusta el **modelo** y la **temperatura**.
3. Prueba la conexión; al guardar, la configuración queda en `%LOCALAPPDATA%\ClasificadorIA\byok.json` (nunca se sube a ningún servidor propio).
4. Pulsa **"Clasificar"** en el panel.

Puedes guardar varios proveedores y elegir uno como activo, o eliminar credenciales guardadas cuando quieras.

---

## 🧪 Auto-prueba

El ejecutable incluye una suite de auto-prueba (sin interfaz) que valida prompt, parser, organizador, filtros, traducciones, configuración BYOK, cliente IA offline y el flujo completo extremo a extremo:

```bash
ClasificadorIA.exe --selftest
```

Salida esperada: `SELF-TEST: OK` (exit code `0`).

---

## 🏗️ Estructura del proyecto

```
ClasificadorIA/
├── ClasificadorIA.slnx            # Solución (app + núcleo UI)
├── ClasificadorIA/                # Aplicación (composition root)
│   ├── App.xaml(.cs)              # Composition root: ventana, prefs, wiring
│   ├── Models/                    # AiProvider, ByokConfig, ClassificationResult…
│   ├── Services/                  # PromptGenerator, ResponseParser, FileOrganizer,
│   │                              # AiClient (0 NuGets), ByokConfigStore, SelfTest…
│   └── Panels/                    # OptionsPanel, ByokDialog
├── NoCloudware.UI.Core/           # Plantilla de núcleo UI (copia local, patrón TubeMassDL)
│   ├── Controls/                  # ShellWindow, BaseMainControl, FileListBox…
│   └── Themes/Aether/             # Sistema de temas
└── docs/superpowers/              # Spec y plan de diseño
```

La lógica de negocio vive en `ClasificadorIA/` sin dependencia de WPF; `NoCloudware.UI.Core/` es la capa de presentación reutilizable.

---

## 🛠️ Tecnologías utilizadas

| Tecnología | Uso |
|---|---|
| [C# / .NET 8.0](https://dotnet.microsoft.com/) | Plataforma principal |
| [WPF](https://learn.microsoft.com/es-es/dotnet/desktop/wpf/) | Interfaz gráfica |
| [NoCloudware.UI.Core](NoCloudware.UI.Core/) | Plantilla de núcleo UI (copia local) |
| [System.Text.Json](https://learn.microsoft.com/es-es/dotnet/standard/serialization/system-text-json/overview) | Perfil de preferencias y respuestas JSON |
| `System.Net.Http` / `System.Text.Json` (BCL) | Cliente IA multiproveedor — **0 NuGet**

---

## 📄 Licencia

Distribuido bajo la licencia **MIT**. Consulta [LICENSE](LICENSE).

```
MIT License — Copyright (c) adriangoodrich
```

---

<div align="center">
  Hecho por <a href="https://github.com/adriangoodrich">adriangoodrich</a>
  <br><br>
  ⭐ Si te resulta útil, ¡dale una estrella al repositorio!
</div>