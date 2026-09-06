using ClasificadorIA.Models;

namespace ClasificadorIA.Services;

public static class Translations
{
    private static Idioma _current = Idioma.Español;

    public static Idioma Current
    {
        get => _current;
        set
        {
            if (_current != value)
            {
                _current = value;
                LanguageChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }

    public static event EventHandler? LanguageChanged;

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
        ["AddFolder"] = ("Agregar carpeta", "Add folder"),
        ["OutputFolder"] = ("Carpeta de destino", "Destination folder"),
        ["OutputFolderDefault"] = ("Elegir la carpeta de salida", "Choose the output folder"),
        // Options panel
        ["SourceFolder"] = ("Carpeta de origen", "Source folder"),
        ["Browse"] = ("Examinar", "Browse"),
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
        ["Classify"] = ("Clasificar", "Classify"),
        ["Copy"] = ("Copiar", "Copy"),
        ["ResultPlaceholder"] = ("Respuesta de la IA…", "AI response…"),
        ["Classify"] = ("Clasificar", "Classify"),
        ["Byok"] = ("BYOK", "BYOK"),
        ["TestConnection"] = ("Probar conexión", "Test connection"),
        ["CopyFiles"] = ("Copiar archivos", "Copy files"),
        ["MoveFiles"] = ("Mover archivos", "Move files"),
        ["OutputMode"] = ("Organización", "Organizing"),
        ["StatusTotal"] = ("archivos", "files"),
        ["StatusProcessed"] = ("procesados", "processed"),
        ["StatusPending"] = ("pendientes", "pending"),
        ["StatusErrors"] = ("errores", "errors"),
        ["CategoriasDetectadas"] = ("Categorías detectadas ({0})", "Detected categories ({0})"),
        ["NoCategoriesYet"] = ("Sin categorías aún", "No categories yet"),
        ["ClearAll"] = ("Limpiar todo", "Clear all"),
        ["PasteFirst"] = ("Pega la respuesta de la IA primero.", "Paste the AI response first."),
        ["Classifying"] = ("Clasificando archivos…", "Classifying files…"),
        ["Cancelled"] = ("Organización cancelada.", "Organizing cancelled."),
        ["AppUpToDate"] = ("Estás usando la última versión.", "You are using the latest version."),
        ["AutoByokHint"] = ("Configura tu API key bajo BYOK para clasificación automática.", "Set up your API key under BYOK for automatic classification."),
        ["CloseBtn"] = ("Cerrar", "Close"),
        // About dialog
        ["AboutTitle"] = ("Acerca de Clasificador IA", "About Clasificador IA"),
        ["AboutCredits"] = ("Créditos", "Credits"),
        ["AboutVersion"] = ("Versión", "Version"),
        ["AboutDevelopedBy"] = ("Desarrollado por", "Developed by"),
        ["AboutDeveloperName"] = ("NoCloudware", "NoCloudware"),
        ["AboutThirdPartyLibraries"] = ("Librerías de terceros", "Third-party libraries"),
        ["AboutWpfUiDesc"] = ("Framework de UI", "UI framework"),
        ["AboutMvvmDesc"] = ("Toolkit MVVM", "MVVM toolkit"),
        ["AboutSpecialThanks"] = ("Agradecimientos especiales", "Special thanks"),
        ["AboutSpecialThanksMessage"] = ("A las comunidades de WPF-UI y CommunityToolkit.Mvvm por su increíble trabajo.",
            "To the WPF-UI and CommunityToolkit.Mvvm communities for their incredible work."),
        ["AboutTechnologiesUsed"] = ("Tecnologías utilizadas", "Technologies used"),
        ["AboutTechList"] = (".NET 8.0, WPF, WPF-UI, CommunityToolkit.Mvvm",
            ".NET 8.0, WPF, WPF-UI, CommunityToolkit.Mvvm"),
        ["AboutLicense"] = ("Licencia", "License"),
        ["AboutLicenseInfo"] = ("MIT License - Código abierto", "MIT License - Open source"),
        ["CheckUpdatesBtn"] = ("Buscar actualizaciones", "Check for updates"),
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
        ["ChooseOutputFolder"] = ("Elige la carpeta de salida con el botón Cambiar del panel izquierdo.", "Choose the output folder with the Change button on the left panel."),
        ["DuplicateFileNames"] = ("Existen archivos con el mismo nombre de distintas carpetas: {0}. Clasifícalos por separado.", "Files with the same name exist from different folders: {0}. Classify them separately."),
        // Dedup panel
        ["DedupTitle"] = ("Eliminar duplicados", "Remove duplicates"),
        ["DedupSameName"] = ("Mismo nombre", "Same name"),
        ["DedupMinSize"] = ("Menor tamaño", "Smallest size"),
        ["DedupMinDate"] = ("Menor fecha", "Oldest date"),
        // Errors
        ["ErrorIn"] = ("Error en", "Error in"),
        ["Error"] = ("Error", "Error"),
        // BYOK dialog
        ["ByokTitle"] = ("Configuración BYOK", "BYOK Settings"),
        ["ByokDescription"] = ("Trae tu propia API key y elige el proveedor que prefieras.", "Bring your own API key and pick the provider you prefer."),
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
        ["OptionsPanelTitle"] = ("Opciones", "Options"),
        ["PromptPreviewPlaceholder"] = ("Vista previa del prompt…", "Prompt preview…"),
        ["ConnectionOk"] = ("Conexión OK: {0}", "Connection OK: {0}"),
        ["ConnectionFailed"] = ("Conexión falló: {0}", "Connection failed: {0}"),
        ["ProviderNeedsKey"] = ("Proveedor {0} requiere API key.", "Provider {0} requires an API key."),
        ["SaveByokConfirm"] = ("Guardar cambios de configuración BYOK?", "Save BYOK settings changes?"),
        ["DeleteByokConfirm"] = ("Eliminar proveedor {0}?", "Delete provider {0}?"),
        ["Name"] = ("Nombre", "Name"),
        ["Scheme"] = ("Esquema", "Scheme"),
        ["LoadingModels"] = ("Cargando modelos…", "Loading models…"),
        ["ModelsLoaded"] = ("{0} modelos disponibles", "{0} models available"),
        ["ModelHint"] = ("Selecciona o escribe un modelo", "Select or type a model"),
        ["ByokNeedsFields"] = ("Completa nombre, base URL y modelo.", "Complete name, base URL and model."),
        ["ByokOkLabel"] = ("Guardado", "Saved"),
        ["ByokErrorTitle"] = ("BYOK", "BYOK"),
        ["ErrAuth"] = ("API key inválida. Verifica tus credenciales.", "Invalid API key. Check your credentials."),
        ["ErrNotFound"] = ("Base URL o modelo no encontrado (404).", "Base URL or model not found (404)."),
        ["ErrRateLimit"] = ("Rate limit alcanzado. Espera un momento.", "Rate limit reached. Wait a moment."),
        ["ErrTimeout"] = ("Tiempo de espera agotado.", "Connection timed out."),
        ["ErrNetwork"] = ("No se pudo conectar. Revisa la red o la base URL.", "Could not connect. Check network or base URL."),
        ["ErrGeneric"] = ("Error: {0}", "Error: {0}"),
        ["ErrNoModel"] = ("Selecciona un modelo.", "Select a model."),
        ["ProvidersEmpty"] = ("No hay proveedores. Agrega uno personalizado.", "No providers. Add a custom one."),
        ["NewProviderDefaultName"] = ("Nuevo proveedor", "New provider"),
    };

    public static string Get(string key, Idioma idioma) =>
        Map.TryGetValue(key, out var pair) ? (idioma == Idioma.Español ? pair.Es : pair.En) : key;

    public static string Get(string key) => Get(key, Current);

    public static string AiErrorMessage(AiException ex, Idioma idioma)
    {
        string key = ex.ErrorCode switch
        {
            "auth" => "ErrAuth",
            "not_found" => "ErrNotFound",
            "rate_limit" => "ErrRateLimit",
            "timeout" => "ErrTimeout",
            "network" => "ErrNetwork",
            "missing_key" => "ProviderNeedsKey",
            "no_model" => "ErrNoModel",
            _ => "ErrGeneric",
        };
        if (key == "ErrGeneric")
            return string.Format(Get(key, idioma), ex.Message);
        return Get(key, idioma);
    }

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