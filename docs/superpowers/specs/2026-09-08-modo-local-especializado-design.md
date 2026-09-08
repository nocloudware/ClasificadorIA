# Modo Local Especializado por Tipo de Archivo — Design

Fecha: 2026-09-08
Estado: Aprobado

## Objetivo

Reemplazar el modo local genérico (clasificación por tokens del nombre de archivo)
por un **despachador por tipo de archivo** que usa algoritmos especializados y
gratuitos (sin IA, sin cuota), con **fallback en cascada** hasta el clasificador
token-genérico actual como respaldo universal.

## Problema que resuelve

El `LocalClassifier` actual clasifica `vacaciones20250101001.jpg` o `cancion.mp3`
solo por palabras del nombre. No aprovecha los metadatos que ya viven dentro de los
archivos (fecha EXIF de la foto, género/artista/año de la canción, fecha de
creación del documento).

## Fallback en cascada (regla central)

Para **cada archivo**, la categoría se determina en este orden, y el primer paso
que produzca resultado gana:

1. **Metadatos del archivo** (EXIF / tags ID3-Vorbis / fecha)
2. **Nombre del archivo** (token-genérico actual)
3. **Extensión** (Audio / Video / Imagen / Documento)
4. **"Otros"**

Ningún paso corta la corrida por un archivo individual: error en metadatos = paso
siguiente, nunca excepción propagada.

## Tabla de despacho propuesta

| Tipo | Extensión | Algoritmo especializado (paso 1) | Cae a (pasos 2-3) |
|---|---|---|---|
| Imagen | jpg, jpeg, png, gif, bmp, webp, svg, ico, heic, tiff | Fecha EXIF (`DateTimeOriginal`) → "Mes Año" | token + extensión |
| Audio | mp3, wav, flac, ogg, m4a, aac, opus, wma | Etiqueta según criterio (Género/Artista/Época/Álbum/Año) | token + extensión |
| Video | mp4, mkv, avi, mov, wmv, flv, webm, m4v | Patrón de serie `S01E05` → "Temporada 1"; si no, fecha de creación → "Año" | token + extensión |
| Documento | pdf, doc, docx, xls, xlsx, ppt, pptx, txt, md, csv, rtf, odt, ods | Fecha de creación → "Año" | token + extensión |
| Otros | cualquier otra | (no hay especialización) | token + extensión |

### Detalle por tipo

**Imagen**: leer `DateTimeOriginal` (luego `DateTimeDigitized` como respaldo) vía
`MetadataExtractor`. Formato de categoría: `"Enero 2025"` (mes traducido según
idioma). Si EXIF ausente/corrupto → paso 2.

**Audio**: leer tags vía `TagLib#`. El campo usado depende del criterio elegido:

| Criterio | Campo |
|---|---|
| Género | `Genres[0]` (si existe) |
| Artista | `Performers[0]` |
| Álbum | `Album` |
| Época | `Year` → "Década 2020" |
| Año | `Year` → "2024" |
| (default Tema) | `Genres[0]` |

Si el tag no tiene el campo pedido o el archivo no se puede leer → paso 2.

**Video**: primero patrón `S(\d+)E(\d+)` (mayúscula/minúscula s/e) → "Temporada N";
si no hay patrón, fecha de creación del archivo → "Año". → paso 2 si nada.

**Documento**: fecha de creación del archivo → "Año" (por año, sin leer contenido).
→ paso 2 si no se puede leer la fecha (improbable).

## Cambios de interfaz

- `LocalClassifier.Classify` recibe **rutas** (`FilePath`), no solo nombres: los
  metadatos se leen del archivo en disco.
- `App.xaml.cs: ClassifyLocal` pasa `FilePath` de cada archivo (ya disponibles en
  `BaseFileItem.FilePath`).
- Criterio (Género/Artista/Época/Álbum/Año) ya lo provee `GetOptions()`.

## Dependencias nuevas

- `MetadataExtractor` (Apache-2.0) — fecha EXIF en imágenes.
- `TagLibSharp` (LGPL 2.1) — tags de audio/video.

Reglas de licencias (AGENTS.md): copiar texto de licencia a
`Libs/{Componente}/LICENSE.txt` y registrar en `THIRD_PARTY_NOTICES.txt`.

## Clasificador local actual

Intacto como **fallback universal** (pasos 2-3). Solo cambia que recibe rutas y que
su entrada ya fue filtrada por el despachador.

## Tests (--selftest)

- Imagen EXIF → categoría mensual (con archivo temporal con EXIF real o stub).
- Imagen sin EXIF → cae a token del nombre.
- Audio tag Género presente → género; sin tag → token del nombre.
- Video `S01E05.mp4` → "Temporada 1"; sin patrón → año.
- Documento → año de creación.
- Despacho por extensión: cada tipo cae a su especialización correcta.
- Fallback universal: archivo sin metadatos y sin tokens → extensión.

## Fuera de alcance (YAGNI)

- Análisis de ondas de audio (GTZAN) — siguiente generación.
- IA/cuota en modo local.
- Escritura de metadatos.