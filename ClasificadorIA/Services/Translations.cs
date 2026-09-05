using ClasificadorIA.Models;

namespace ClasificadorIA.Services;

public static class Translations
{
    public static Idioma Current { get; set; } = Idioma.Español;

    private static readonly Dictionary<string, (string Es, string En)> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        // Shell / UI
        ["AppTitle"] = ("Clasificador IA", "Clasificador IA"),
        ["AppTagline"] = ("Organizador de archivos con IA", "AI file organizer"),
        ["DropText"] = ("Selecciona una carpeta para clasificar sus archivos", "Select a folder to classify its files"),
        ["FileListHeader"] = ("Archivos a clasificar", "Files to classify"),
        ["FilesFound"] = ("archivos encontrados", "files found"),
        ["ActionButton"] = ("Organizar", "Organize"),
        ["AboutButton"] = ("Acerca de", "About"),
        ["DonateButton"] = ("Donar", "Donate"),
        ["ExitButton"] = ("Salir", "Exit"),
        ["SelectFilesBtn"] = ("Seleccionar archivos", "Select files"),
        ["ChangeBtn"] = ("Cambiar", "Change"),
        ["OutputFolder"] = ("Carpeta de destino", "Destination folder"),
        ["OutputFolderDefault"] = ("Misma carpeta que el origen", "Same folder as source"),
        // Options panel
        ["SourceFolder"] = ("Carpeta de origen", "Source folder"),
        ["Browse"] = ("Examinar", "Browse"),
        ["NoFolderSelected"] = ("Ninguna carpeta seleccionada", "No folder selected"),
        ["Mode"] = ("Modo", "Mode"),
        ["Criterion"] = ("Criterio", "Criterion"),
        ["Depth"] = ("Profundidad", "Depth"),
        ["Categorias5"] = ("5 categorías", "5 categories"),
        ["Categorias10"] = ("10 categorías", "10 categories"),
        ["Categorias15"] = ("15 categorías", "15 categories"),
        ["Method"] = ("Método", "Method"),
        ["ManualMode"] = ("Manual (Copiar/Pegar)", "Manual (Copy/Paste)"),
        ["AutoMode"] = ("Auto (BYOK)", "Auto (BYOK)"),
        // Prompt / response / classify
        ["GeneratePrompt"] = ("Generar Prompt", "Generate Prompt"),
        ["PromptCopied"] = ("Prompt copiado a tu portapapeles", "Prompt copied to your clipboard"),
        ["PromptEmpty"] = ("No hay prompt.", "No prompt."),
        ["PasteResponse"] = ("Pegar Respuesta", "Paste Response"),
        ["NoneInClipboard"] = ("No hay texto en portapapeles.", "No text in clipboard."),
        ["LoadResponse"] = ("Cargar", "Load"),
        ["ResultPlaceholder"] = ("Respuesta de la IA…", "AI response…"),
        ["Classify"] = ("Clasificar", "Classify"),
        ["Byok"] = ("BYOK", "BYOK"),
        ["TestConnection"] = ("Probar conexión", "Test connection"),
        ["CopyFiles"] = ("Copiar archivos", "Copy files"),
        ["MoveFiles"] = ("Mover archivos", "Move files"),
        // Messages
        ["FolderNotExist"] = ("La carpeta no existe.", "Folder does not exist."),
        ["NoFilesToClassify"] = ("No hay archivos para clasificar.", "No files to classify."),
        ["NoValidCategories"] = ("No se encontraron categorías válidas.", "No valid categories found."),
        ["JsonNoCategorias"] = ("El JSON no contiene categorías.", "JSON does not contain categories."),
        ["JsonNotFound"] = ("No se encontró JSON en la respuesta.", "No JSON found in the response."),
        ["OrganizeConfirm"] = ("¿ORGANIZAR archivos ({0})?", "ORGANIZE files ({0})?"),
        ["OrganizeResultTitle"] = ("Resultado", "Result"),
        ["ResultProcessed"] = ("Procesados", "Processed"),
        ["ResultErrors"] = ("Errores", "Errors"),
        ["ProcessingFiles"] = ("Organizando archivos…", "Organizing files…"),
        ["Cancel"] = ("Cancelar", "Cancel"),
        ["MissingFiles"] = ("Los siguientes archivos no se encontraron:\n", "The following files were not found:\n"),
        ["MissingFilesTitle"] = ("Archivos no encontrados", "Files not found"),
        ["FilesOrganized"] = ("✓ Organización completada", "✓ Organizing completed"),
        ["NoResultsToOrganize"] = ("Clasifica primero los archivos.", "Classify the files first."),
        ["NoSourceFolder"] = ("Selecciona una carpeta de origen.", "Select a source folder."),
        // Errors
        ["ErrorIn"] = ("Error en", "Error in"),
        ["Error"] = ("Error", "Error"),
        // BYOK dialog
        ["ByokTitle"] = ("Configuración BYOK", "BYOK Settings"),
        ["Provider"] = ("Proveedor", "Provider"),
        ["Model"] = ("Modelo", "Model"),
        ["ApiKey"] = ("API Key", "API Key"),
        ["BaseUrl"] = ("Base URL", "Base URL"),
        ["Temperature"] = ("Temperatura", "Temperature"),
        ["GetApiKey"] = ("Obtener API key →", "Get API key →"),
        ["ReloadModels"] = ("Reload modelos", "Reload models"),
        ["AddCustomProvider"] = ("＋ Agregar personalizado", "＋ Add custom provider"),
        ["Save"] = ("Guardar", "Save"),
        ["Delete"] = ("Eliminar", "Delete"),
        ["Remove"] = ("Eliminar", "Remove"),
        ["ConnectionOk"] = ("Conexión OK: {0}", "Connection OK: {0}"),
        ["ConnectionFailed"] = ("Conexión falló: {0}", "Connection failed: {0}"),
        ["ProviderNeedsKey"] = ("Proveedor {0} requiere API key.", "Provider {0} requires an API key."),
        ["SaveByokConfirm"] = ("Guardar cambios de configuración BYOK?", "Save BYOK settings changes?"),
        ["DeleteByokConfirm"] = ("Eliminar proveedor {0}?", "Delete provider {0}?"),
    };

    public static string Get(string key, Idioma idioma) =>
        Map.TryGetValue(key, out var pair) ? (idioma == Idioma.Español ? pair.Es : pair.En) : key;

    public static string Get(string key) => Get(key, Current);

    public static string ModeLabel(string modeKey, Idioma idioma) => idioma == Idioma.Español
        ? modeKey switch
        {
            "Genérico" => "📁 Genérico",
            "Música" => "🎵 Música",
            "Películas" => "🎬 Películas",
            "Series" => "📺 Series",
            "Libros" => "📚 Libros",
            _ => modeKey
        }
        : modeKey switch
        {
            "Genérico" => "📁 Generic",
            "Música" => "🎵 Music",
            "Películas" => "🎬 Movies",
            "Series" => "📺 Series",
            "Libros" => "📚 Books",
            _ => modeKey
        };
}