using System.Collections.Generic;
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

    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["es"] = new()
        {
            // Shell / UI
            ["AppTitle"] = "Clasificador IA", ["AppTagline"] = "Organizador de archivos con IA",
            ["DropText"] = "Selecciona una carpeta para clasificar sus archivos",
            ["FileListHeader"] = "Archivos a clasificar", ["FilesFound"] = "archivos encontrados",
            ["ActionButton"] = "Organizar", ["AboutButton"] = "Acerca de", ["DonateButton"] = "Donar",
            ["ExitButton"] = "Salir", ["SelectFilesBtn"] = "Seleccionar archivos", ["ChangeBtn"] = "Cambiar",
            ["AddFolder"] = "Agregar carpeta", ["OutputFolder"] = "Carpeta de destino",
            ["OutputFolderDefault"] = "Elegir la carpeta de salida",
            // Options panel
            ["SourceFolder"] = "Carpeta de origen", ["Browse"] = "Examinar", ["Mode"] = "Modo",
            ["Criterion"] = "Criterio", ["Depth"] = "Profundidad", ["Categorias5"] = "5 categorías",
            ["Categorias10"] = "10 categorías", ["Categorias15"] = "15 categorías", ["Method"] = "Método",
            ["MethodLocal"] = "Local", ["MethodIA"] = "IA", ["BatchSize"] = "Tamaño de lote",
            ["BatchAll"] = "Todos (un solo lote)", ["BatchStatus"] = "Clasificando lote {0}/{1}…",
            ["BatchTimer"] = "Lote {0}/{1} · {2}", ["BatchRetryStatus"] = "Lote {0}/{1}: cuota agotada, reintento {2} en {3}s…",
            ["Otros"] = "Otros", ["CatSoundtrack"] = "Banda sonora", ["Decade"] = "Década {0}",
            // Classify
            ["Classify"] = "Clasificar", ["Byok"] = "BYOK", ["TestConnection"] = "Probar conexión",
            ["CopyFiles"] = "Copiar archivos", ["MoveFiles"] = "Mover archivos", ["OutputMode"] = "Organización",
            ["StatusTotal"] = "archivos", ["StatusProcessed"] = "procesados", ["StatusPending"] = "pendientes",
            ["StatusErrors"] = "errores", ["CategoriasDetectadas"] = "Categorías detectadas ({0})",
            ["ClearAll"] = "Limpiar todo", ["Cancelled"] = "Organización cancelada.",
            ["ClassifyCancelled"] = "Clasificación cancelada.", ["AppUpToDate"] = "Estás usando la última versión.",
            ["UpdateAvailable"] = "Hay una versión nueva ({0}). Podés descargarla en {1}",
            ["AutoByokHint"] = "Configura tu API key bajo BYOK para clasificación automática.",
            ["LocalDisclaimer"] = "La clasificación local es menos exacta que la IA: usa solo datos del archivo (nombre, metadatos, tipo). Si algo queda en \u201cOtros\u201d es por falta de información local, no un error del programa.",
            ["CloseBtn"] = "Cerrar",
            // About dialog
            ["AboutTitle"] = "Acerca de Clasificador IA", ["AboutCredits"] = "Créditos", ["AboutVersion"] = "Versión",
            ["AboutDevelopedBy"] = "Desarrollado por", ["AboutDeveloperName"] = "NoCloudware",
            ["AboutThirdPartyLibraries"] = "Librerías de terceros", ["AboutWpfUiDesc"] = "Framework de UI",
            ["AboutMvvmDesc"] = "Toolkit MVVM", ["AboutSpecialThanks"] = "Agradecimientos especiales",
            ["AboutSpecialThanksMessage"] = "A las comunidades de WPF-UI y CommunityToolkit.Mvvm por su increíble trabajo.",
            ["AboutTechnologiesUsed"] = "Tecnologías utilizadas",
            ["AboutTechList"] = ".NET 8.0, WPF, WPF-UI, CommunityToolkit.Mvvm",
            ["AboutLicense"] = "Licencia", ["AboutLicenseInfo"] = "MIT License - Código abierto",
            ["CheckUpdatesBtn"] = "Buscar actualizaciones",
            // Messages
            ["FolderNotExist"] = "La carpeta no existe.", ["NoFilesToClassify"] = "No hay archivos para clasificar.",
            ["NoValidCategories"] = "No se encontraron categorías válidas.",
            ["OrganizeConfirm"] = "¿ORGANIZAR archivos ({0})?", ["OrganizeResultTitle"] = "Resultado",
            ["ResultProcessed"] = "Procesados", ["ResultErrors"] = "Errores",
            ["ProcessingFiles"] = "Organizando archivos…", ["Cancel"] = "Cancelar",
            ["MissingFiles"] = "Los siguientes archivos no se encontraron:\n", ["MissingFilesTitle"] = "Archivos no encontrados",
            ["FilesOrganized"] = "✓ Organización completada", ["NoResultsToOrganize"] = "Clasifica primero los archivos.",
            ["ChooseOutputFolder"] = "Elige la carpeta de salida con el botón Cambiar del panel izquierdo.",
            ["DuplicateFileNames"] = "Existen archivos con el mismo nombre de distintas carpetas: {0}. Clasifícalos por separado.",
            // Dedup panel
            ["DedupTitle"] = "Eliminar duplicados", ["DedupSameName"] = "Mismo nombre",
            ["DedupMinSize"] = "Menor tamaño", ["DedupMinDate"] = "Menor fecha",
            // Errors
            ["ErrorIn"] = "Error en", ["Error"] = "Error",
            // BYOK dialog
            ["ByokTitle"] = "Configuración BYOK",
            ["ByokDescription"] = "Trae tu propia API key y elige el proveedor que prefieras.",
            ["Provider"] = "Proveedor", ["Model"] = "Modelo", ["ApiKey"] = "API Key", ["BaseUrl"] = "Base URL",
            ["Temperature"] = "Temperatura",
            ["TemperatureHint"] = "Controla la creatividad y aleatoriedad de las respuestas. Valores bajos son más deterministas, valores altos más variados.",
            ["MaxTokens"] = "Máx. tokens de salida",
            ["MaxTokensHint"] = "Tope de tokens que la IA puede usar por llamada. En AUTO se calcula el valor ideal según la cantidad de archivos del lote, y nunca supera el límite real del modelo.",
            ["MaxTokensAuto"] = "Auto", ["ModelLimitLabel"] = "Límite del modelo: {0} tokens",
            ["GetApiKey"] = "Obtener API key →", ["ReloadModels"] = "Reload modelos",
            ["AddCustomProvider"] = "＋ Agregar personalizado", ["Save"] = "Guardar", ["Delete"] = "Eliminar",
            ["Remove"] = "Eliminar", ["OptionsPanelTitle"] = "Opciones", ["ConnectionOk"] = "Conexión OK: {0}",
            ["ConnectionFailed"] = "Conexión falló: {0}", ["ProviderNeedsKey"] = "Proveedor {0} requiere API key.",
            ["SaveByokConfirm"] = "Guardar cambios de configuración BYOK?", ["DeleteByokConfirm"] = "Eliminar proveedor {0}?",
            ["Scheme"] = "Esquema", ["LoadingModels"] = "Cargando modelos…", ["TestingConnection"] = "Probando conexión…",
            ["ModelsLoaded"] = "{0} modelos disponibles", ["ModelHint"] = "Selecciona un modelo disponible",
            ["ByokNeedsFields"] = "Completa base URL y modelo.", ["ByokOkLabel"] = "Guardado", ["ByokErrorTitle"] = "BYOK",
            ["ErrAuth"] = "API key inválida. Verifica tus credenciales.",
            ["ErrNotFound"] = "Base URL o modelo no encontrado (404).",
            ["ErrRateLimit"] = "Rate limit alcanzado. Espera un momento.", ["ErrTimeout"] = "Tiempo de espera agotado.",
            ["ErrNetwork"] = "No se pudo conectar. Revisa la red o la base URL.", ["ErrGeneric"] = "Error: {0}",
            ["ErrNoModel"] = "Selecciona un modelo.", ["ProvidersEmpty"] = "No hay proveedores. Agrega uno personalizado.",
            ["NewProviderDefaultName"] = "Nuevo proveedor",
            // Mode labels (emoji + nombre del modo)
            ["ModeGenérico"] = "📁 Genérico", ["ModeMúsica"] = "🎵 Música", ["ModePelículas"] = "🎬 Películas",
            ["ModeSeries"] = "📺 Series", ["ModeLibros"] = "📚 Libros",
        },

        ["en"] = new()
        {
            // Shell / UI
            ["AppTitle"] = "Clasificador IA", ["AppTagline"] = "AI file organizer",
            ["DropText"] = "Select a folder to classify its files",
            ["FileListHeader"] = "Files to classify", ["FilesFound"] = "files found",
            ["ActionButton"] = "Organize", ["AboutButton"] = "About", ["DonateButton"] = "Donate",
            ["ExitButton"] = "Exit", ["SelectFilesBtn"] = "Select files", ["ChangeBtn"] = "Change",
            ["AddFolder"] = "Add folder", ["OutputFolder"] = "Destination folder",
            ["OutputFolderDefault"] = "Choose the output folder",
            // Options panel
            ["SourceFolder"] = "Source folder", ["Browse"] = "Browse", ["Mode"] = "Mode",
            ["Criterion"] = "Criterion", ["Depth"] = "Depth", ["Categorias5"] = "5 categories",
            ["Categorias10"] = "10 categories", ["Categorias15"] = "15 categories", ["Method"] = "Method",
            ["MethodLocal"] = "Local", ["MethodIA"] = "AI", ["BatchSize"] = "Batch size",
            ["BatchAll"] = "All (single batch)", ["BatchStatus"] = "Classifying batch {0}/{1}…",
            ["BatchTimer"] = "Batch {0}/{1} · {2}", ["BatchRetryStatus"] = "Batch {0}/{1}: rate-limited, retry {2} in {3}s…",
            ["Otros"] = "Others", ["CatSoundtrack"] = "Soundtrack", ["Decade"] = "{0}s",
            // Classify
            ["Classify"] = "Classify", ["Byok"] = "BYOK", ["TestConnection"] = "Test connection",
            ["CopyFiles"] = "Copy files", ["MoveFiles"] = "Move files", ["OutputMode"] = "Organizing",
            ["StatusTotal"] = "files", ["StatusProcessed"] = "processed", ["StatusPending"] = "pending",
            ["StatusErrors"] = "errors", ["CategoriasDetectadas"] = "Detected categories ({0})",
            ["ClearAll"] = "Clear all", ["Cancelled"] = "Organizing cancelled.",
            ["ClassifyCancelled"] = "Classification cancelled.", ["AppUpToDate"] = "You are using the latest version.",
            ["UpdateAvailable"] = "A new version is available ({0}). You can download it at {1}",
            ["AutoByokHint"] = "Set up your API key under BYOK for automatic classification.",
            ["LocalDisclaimer"] = "Local classification is less accurate than AI: it only uses file data (name, metadata, type). If something ends up in \u201cOthers\u201d it is missing local info, not a program bug.",
            ["CloseBtn"] = "Close",
            // About dialog
            ["AboutTitle"] = "About Clasificador IA", ["AboutCredits"] = "Credits", ["AboutVersion"] = "Version",
            ["AboutDevelopedBy"] = "Developed by", ["AboutDeveloperName"] = "NoCloudware",
            ["AboutThirdPartyLibraries"] = "Third-party libraries", ["AboutWpfUiDesc"] = "UI framework",
            ["AboutMvvmDesc"] = "MVVM toolkit", ["AboutSpecialThanks"] = "Special thanks",
            ["AboutSpecialThanksMessage"] = "To the WPF-UI and CommunityToolkit.Mvvm communities for their incredible work.",
            ["AboutTechnologiesUsed"] = "Technologies used",
            ["AboutTechList"] = ".NET 8.0, WPF, WPF-UI, CommunityToolkit.Mvvm",
            ["AboutLicense"] = "License", ["AboutLicenseInfo"] = "MIT License - Open source",
            ["CheckUpdatesBtn"] = "Check for updates",
            // Messages
            ["FolderNotExist"] = "Folder does not exist.", ["NoFilesToClassify"] = "No files to classify.",
            ["NoValidCategories"] = "No valid categories found.", ["OrganizeConfirm"] = "ORGANIZE files ({0})?",
            ["OrganizeResultTitle"] = "Result", ["ResultProcessed"] = "Processed", ["ResultErrors"] = "Errors",
            ["ProcessingFiles"] = "Organizing files…", ["Cancel"] = "Cancel",
            ["MissingFiles"] = "The following files were not found:\n", ["MissingFilesTitle"] = "Files not found",
            ["FilesOrganized"] = "✓ Organizing completed", ["NoResultsToOrganize"] = "Classify the files first.",
            ["ChooseOutputFolder"] = "Choose the output folder with the Change button on the left panel.",
            ["DuplicateFileNames"] = "Files with the same name exist from different folders: {0}. Classify them separately.",
            // Dedup panel
            ["DedupTitle"] = "Remove duplicates", ["DedupSameName"] = "Same name",
            ["DedupMinSize"] = "Smallest size", ["DedupMinDate"] = "Oldest date",
            // Errors
            ["ErrorIn"] = "Error in", ["Error"] = "Error",
            // BYOK dialog
            ["ByokTitle"] = "BYOK Settings",
            ["ByokDescription"] = "Bring your own API key and pick the provider you prefer.",
            ["Provider"] = "Provider", ["Model"] = "Model", ["ApiKey"] = "API Key", ["BaseUrl"] = "Base URL",
            ["Temperature"] = "Temperature",
            ["TemperatureHint"] = "Controls the creativity and randomness of responses. Lower values are more deterministic, higher values more varied.",
            ["MaxTokens"] = "Max output tokens",
            ["MaxTokensHint"] = "Max tokens the AI may use per call. In AUTO it picks the ideal value from the batch file count, never exceeding the model's real limit.",
            ["MaxTokensAuto"] = "Auto", ["ModelLimitLabel"] = "Model max output: {0} tokens",
            ["GetApiKey"] = "Get API key →", ["ReloadModels"] = "Reload models",
            ["AddCustomProvider"] = "＋ Add custom provider", ["Save"] = "Save", ["Delete"] = "Delete",
            ["Remove"] = "Delete", ["OptionsPanelTitle"] = "Options", ["ConnectionOk"] = "Connection OK: {0}",
            ["ConnectionFailed"] = "Connection failed: {0}", ["ProviderNeedsKey"] = "Provider {0} requires an API key.",
            ["SaveByokConfirm"] = "Save BYOK settings changes?", ["DeleteByokConfirm"] = "Delete provider {0}?",
            ["Scheme"] = "Scheme", ["LoadingModels"] = "Loading models…", ["TestingConnection"] = "Testing connection…",
            ["ModelsLoaded"] = "{0} models available", ["ModelHint"] = "Select an available model",
            ["ByokNeedsFields"] = "Complete base URL and model.", ["ByokOkLabel"] = "Saved", ["ByokErrorTitle"] = "BYOK",
            ["ErrAuth"] = "Invalid API key. Check your credentials.",
            ["ErrNotFound"] = "Base URL or model not found (404).",
            ["ErrRateLimit"] = "Rate limit reached. Wait a moment.", ["ErrTimeout"] = "Connection timed out.",
            ["ErrNetwork"] = "Could not connect. Check network or base URL.", ["ErrGeneric"] = "Error: {0}",
            ["ErrNoModel"] = "Select a model.", ["ProvidersEmpty"] = "No providers. Add a custom one.",
            ["NewProviderDefaultName"] = "New provider",
            // Mode labels
            ["ModeGenérico"] = "📁 Generic", ["ModeMúsica"] = "🎵 Music", ["ModePelículas"] = "🎬 Movies",
            ["ModeSeries"] = "📺 Series", ["ModeLibros"] = "📚 Books",
        },

        ["fr"] = new()
        {
            // Shell / UI
            ["AppTitle"] = "Clasificador IA", ["AppTagline"] = "Organisateur de fichiers par IA",
            ["DropText"] = "Sélectionnez un dossier pour classer ses fichiers",
            ["FileListHeader"] = "Fichiers à classer", ["FilesFound"] = "fichiers trouvés",
            ["ActionButton"] = "Organiser", ["AboutButton"] = "À propos", ["DonateButton"] = "Faire un don",
            ["ExitButton"] = "Quitter", ["SelectFilesBtn"] = "Sélectionner des fichiers", ["ChangeBtn"] = "Changer",
            ["AddFolder"] = "Ajouter un dossier", ["OutputFolder"] = "Dossier de destination",
            ["OutputFolderDefault"] = "Choisir le dossier de sortie",
            // Options panel
            ["SourceFolder"] = "Dossier source", ["Browse"] = "Parcourir", ["Mode"] = "Mode",
            ["Criterion"] = "Critère", ["Depth"] = "Profondeur", ["Categorias5"] = "5 catégories",
            ["Categorias10"] = "10 catégories", ["Categorias15"] = "15 catégories", ["Method"] = "Méthode",
            ["MethodLocal"] = "Local", ["MethodIA"] = "IA", ["BatchSize"] = "Taille du lot",
            ["BatchAll"] = "Tout (un seul lot)", ["BatchStatus"] = "Classification du lot {0}/{1}…",
            ["BatchTimer"] = "Lot {0}/{1} · {2}", ["BatchRetryStatus"] = "Lot {0}/{1} : limite atteinte, nouvel essai {2} dans {3}s…",
            ["Otros"] = "Autres", ["CatSoundtrack"] = "Bande originale", ["Decade"] = "Années {0}",
            // Classify
            ["Classify"] = "Classifier", ["Byok"] = "BYOK", ["TestConnection"] = "Tester la connexion",
            ["CopyFiles"] = "Copier les fichiers", ["MoveFiles"] = "Déplacer les fichiers", ["OutputMode"] = "Organisation",
            ["StatusTotal"] = "fichiers", ["StatusProcessed"] = "traités", ["StatusPending"] = "en attente",
            ["StatusErrors"] = "erreurs", ["CategoriasDetectadas"] = "Catégories détectées ({0})",
            ["ClearAll"] = "Tout effacer", ["Cancelled"] = "Organisation annulée.",
            ["ClassifyCancelled"] = "Classification annulée.", ["AppUpToDate"] = "Vous utilisez la dernière version.",
            ["UpdateAvailable"] = "Une nouvelle version ({0}) est disponible. Vous pouvez la télécharger sur {1}",
            ["AutoByokHint"] = "Configurez votre clé API dans BYOK pour la classification automatique.",
            ["LocalDisclaimer"] = "La classification locale est moins précise que l'IA : elle n'utilise que les données du fichier (nom, métadonnées, type). Si un élément finit dans « Autres », c'est un manque d'information locale, pas un bug du programme.",
            ["CloseBtn"] = "Fermer",
            // About dialog
            ["AboutTitle"] = "À propos de Clasificador IA", ["AboutCredits"] = "Crédits", ["AboutVersion"] = "Version",
            ["AboutDevelopedBy"] = "Développé par", ["AboutDeveloperName"] = "NoCloudware",
            ["AboutThirdPartyLibraries"] = "Bibliothèques tierces", ["AboutWpfUiDesc"] = "Framework d'interface",
            ["AboutMvvmDesc"] = "Toolkit MVVM", ["AboutSpecialThanks"] = "Remerciements spéciaux",
            ["AboutSpecialThanksMessage"] = "Aux communautés WPF-UI et CommunityToolkit.Mvvm pour leur formidable travail.",
            ["AboutTechnologiesUsed"] = "Technologies utilisées",
            ["AboutTechList"] = ".NET 8.0, WPF, WPF-UI, CommunityToolkit.Mvvm",
            ["AboutLicense"] = "Licence", ["AboutLicenseInfo"] = "Licence MIT – Open source",
            ["CheckUpdatesBtn"] = "Rechercher des mises à jour",
            // Messages
            ["FolderNotExist"] = "Le dossier n'existe pas.", ["NoFilesToClassify"] = "Aucun fichier à classer.",
            ["NoValidCategories"] = "Aucune catégorie valide trouvée.", ["OrganizeConfirm"] = "ORGANISER les fichiers ({0}) ?",
            ["OrganizeResultTitle"] = "Résultat", ["ResultProcessed"] = "Traités", ["ResultErrors"] = "Erreurs",
            ["ProcessingFiles"] = "Organisation des fichiers…", ["Cancel"] = "Annuler",
            ["MissingFiles"] = "Les fichiers suivants sont introuvables :\n", ["MissingFilesTitle"] = "Fichiers introuvables",
            ["FilesOrganized"] = "✓ Organisation terminée", ["NoResultsToOrganize"] = "Classez d'abord les fichiers.",
            ["ChooseOutputFolder"] = "Choisissez le dossier de sortie avec le bouton Changer du panneau de gauche.",
            ["DuplicateFileNames"] = "Des fichiers de dossiers différents portent le même nom : {0}. Classez-les séparément.",
            // Dedup panel
            ["DedupTitle"] = "Supprimer les doublons", ["DedupSameName"] = "Même nom",
            ["DedupMinSize"] = "Plus petite taille", ["DedupMinDate"] = "Date la plus ancienne",
            // Errors
            ["ErrorIn"] = "Erreur dans", ["Error"] = "Erreur",
            // BYOK dialog
            ["ByokTitle"] = "Configuration BYOK",
            ["ByokDescription"] = "Apportez votre propre clé API et choisissez le fournisseur de votre choix.",
            ["Provider"] = "Fournisseur", ["Model"] = "Modèle", ["ApiKey"] = "Clé API", ["BaseUrl"] = "URL de base",
            ["Temperature"] = "Température",
            ["TemperatureHint"] = "Contrôle la créativité et l'aléatoire des réponses. Des valeurs faibles sont plus déterministes, des valeurs élevées plus variées.",
            ["MaxTokens"] = "Max. tokens de sortie",
            ["MaxTokensHint"] = "Plafond de tokens que l'IA peut utiliser par appel. En AUTO, la valeur idéale est calculée selon le nombre de fichiers du lot, sans jamais dépasser la limite réelle du modèle.",
            ["MaxTokensAuto"] = "Auto", ["ModelLimitLabel"] = "Limite du modèle : {0} tokens",
            ["GetApiKey"] = "Obtenir une clé API →", ["ReloadModels"] = "Recharger les modèles",
            ["AddCustomProvider"] = "＋ Ajouter un fournisseur", ["Save"] = "Enregistrer", ["Delete"] = "Supprimer",
            ["Remove"] = "Supprimer", ["OptionsPanelTitle"] = "Options", ["ConnectionOk"] = "Connexion OK : {0}",
            ["ConnectionFailed"] = "Échec de la connexion : {0}", ["ProviderNeedsKey"] = "Le fournisseur {0} requiert une clé API.",
            ["SaveByokConfirm"] = "Enregistrer les modifications BYOK ?", ["DeleteByokConfirm"] = "Supprimer le fournisseur {0} ?",
            ["Scheme"] = "Schéma", ["LoadingModels"] = "Chargement des modèles…", ["TestingConnection"] = "Test de la connexion…",
            ["ModelsLoaded"] = "{0} modèles disponibles", ["ModelHint"] = "Sélectionnez un modèle disponible",
            ["ByokNeedsFields"] = "Complétez l'URL de base et le modèle.", ["ByokOkLabel"] = "Enregistré", ["ByokErrorTitle"] = "BYOK",
            ["ErrAuth"] = "Clé API invalide. Vérifiez vos identifiants.",
            ["ErrNotFound"] = "URL de base ou modèle introuvable (404).",
            ["ErrRateLimit"] = "Limite d'utilisation atteinte. Attendez un instant.", ["ErrTimeout"] = "Délai expiré.",
            ["ErrNetwork"] = "Connexion impossible. Vérifiez le réseau ou l'URL de base.", ["ErrGeneric"] = "Erreur : {0}",
            ["ErrNoModel"] = "Sélectionnez un modèle.", ["ProvidersEmpty"] = "Aucun fournisseur. Ajoutez-en un.",
            ["NewProviderDefaultName"] = "Nouveau fournisseur",
            // Mode labels
            ["ModeGenérico"] = "📁 Générique", ["ModeMúsica"] = "🎵 Musique", ["ModePelículas"] = "🎬 Films",
            ["ModeSeries"] = "📺 Séries", ["ModeLibros"] = "📚 Livres",
        },

        ["de"] = new()
        {
            // Shell / UI
            ["AppTitle"] = "Clasificador IA", ["AppTagline"] = "KI-Dateiordner",
            ["DropText"] = "Wählen Sie einen Ordner, um dessen Dateien zu sortieren",
            ["FileListHeader"] = "Zu sortierende Dateien", ["FilesFound"] = "Dateien gefunden",
            ["ActionButton"] = "Organisieren", ["AboutButton"] = "Über", ["DonateButton"] = "Spenden",
            ["ExitButton"] = "Beenden", ["SelectFilesBtn"] = "Dateien auswählen", ["ChangeBtn"] = "Ändern",
            ["AddFolder"] = "Ordner hinzufügen", ["OutputFolder"] = "Zielordner",
            ["OutputFolderDefault"] = "Ausgabeordner wählen",
            // Options panel
            ["SourceFolder"] = "Quellordner", ["Browse"] = "Durchsuchen", ["Mode"] = "Modus",
            ["Criterion"] = "Kriterium", ["Depth"] = "Tiefe", ["Categorias5"] = "5 Kategorien",
            ["Categorias10"] = "10 Kategorien", ["Categorias15"] = "15 Kategorien", ["Method"] = "Methode",
            ["MethodLocal"] = "Lokal", ["MethodIA"] = "KI", ["BatchSize"] = "Stapelgröße",
            ["BatchAll"] = "Alle (ein Stapel)", ["BatchStatus"] = "Stapel {0}/{1} wird klassifiziert…",
            ["BatchTimer"] = "Stapel {0}/{1} · {2}", ["BatchRetryStatus"] = "Stapel {0}/{1}: Limit erreicht, Versuch {2} in {3}s…",
            ["Otros"] = "Andere", ["CatSoundtrack"] = "Soundtrack", ["Decade"] = "{0}er",
            // Classify
            ["Classify"] = "Klassifizieren", ["Byok"] = "BYOK", ["TestConnection"] = "Verbindung testen",
            ["CopyFiles"] = "Dateien kopieren", ["MoveFiles"] = "Dateien verschieben", ["OutputMode"] = "Organisation",
            ["StatusTotal"] = "Dateien", ["StatusProcessed"] = "verarbeitet", ["StatusPending"] = "ausstehend",
            ["StatusErrors"] = "Fehler", ["CategoriasDetectadas"] = "Erkannte Kategorien ({0})",
            ["ClearAll"] = "Alles löschen", ["Cancelled"] = "Organisation abgebrochen.",
            ["ClassifyCancelled"] = "Klassifizierung abgebrochen.", ["AppUpToDate"] = "Sie verwenden die neueste Version.",
            ["UpdateAvailable"] = "Eine neue Version ({0}) ist verfügbar. Sie können sie unter {1} herunterladen",
            ["AutoByokHint"] = "Konfigurieren Sie Ihren API-Schlüssel unter BYOK für die automatische Klassifizierung.",
            ["LocalDisclaimer"] = "Die lokale Klassifizierung ist weniger genau als KI: Sie nutzt nur Dateidaten (Name, Metadaten, Typ). Wenn etwas in „Andere“ landet, fehlt lokale Information – kein Programmfehler.",
            ["CloseBtn"] = "Schließen",
            // About dialog
            ["AboutTitle"] = "Über Clasificador IA", ["AboutCredits"] = "Danksagungen", ["AboutVersion"] = "Version",
            ["AboutDevelopedBy"] = "Entwickelt von", ["AboutDeveloperName"] = "NoCloudware",
            ["AboutThirdPartyLibraries"] = "Drittbibliotheken", ["AboutWpfUiDesc"] = "UI-Framework",
            ["AboutMvvmDesc"] = "MVVM-Toolkit", ["AboutSpecialThanks"] = "Besonderer Dank",
            ["AboutSpecialThanksMessage"] = "An die Communities von WPF-UI und CommunityToolkit.Mvvm für ihre großartige Arbeit.",
            ["AboutTechnologiesUsed"] = "Verwendete Technologien",
            ["AboutTechList"] = ".NET 8.0, WPF, WPF-UI, CommunityToolkit.Mvvm",
            ["AboutLicense"] = "Lizenz", ["AboutLicenseInfo"] = "MIT-Lizenz – Open Source",
            ["CheckUpdatesBtn"] = "Nach Updates suchen",
            // Messages
            ["FolderNotExist"] = "Der Ordner existiert nicht.", ["NoFilesToClassify"] = "Keine Dateien zum Klassifizieren.",
            ["NoValidCategories"] = "Keine gültigen Kategorien gefunden.", ["OrganizeConfirm"] = "DATEIEN ORGANISIEREN ({0})?",
            ["OrganizeResultTitle"] = "Ergebnis", ["ResultProcessed"] = "Verarbeitet", ["ResultErrors"] = "Fehler",
            ["ProcessingFiles"] = "Dateien werden organisiert…", ["Cancel"] = "Abbrechen",
            ["MissingFiles"] = "Die folgenden Dateien wurden nicht gefunden:\n", ["MissingFilesTitle"] = "Dateien nicht gefunden",
            ["FilesOrganized"] = "✓ Organisation abgeschlossen", ["NoResultsToOrganize"] = "Klassifizieren Sie zuerst die Dateien.",
            ["ChooseOutputFolder"] = "Wählen Sie den Ausgabeordner über die Schaltfläche Ändern im linken Bereich.",
            ["DuplicateFileNames"] = "Dateien mit demselben Namen existieren in verschiedenen Ordnern: {0}. Klassifizieren Sie sie getrennt.",
            // Dedup panel
            ["DedupTitle"] = "Duplikate entfernen", ["DedupSameName"] = "Gleicher Name",
            ["DedupMinSize"] = "Kleinste Größe", ["DedupMinDate"] = "Ältestes Datum",
            // Errors
            ["ErrorIn"] = "Fehler in", ["Error"] = "Fehler",
            // BYOK dialog
            ["ByokTitle"] = "BYOK-Einstellungen",
            ["ByokDescription"] = "Bringen Sie Ihren eigenen API-Schlüssel mit und wählen Sie den bevorzugten Anbieter.",
            ["Provider"] = "Anbieter", ["Model"] = "Modell", ["ApiKey"] = "API-Schlüssel", ["BaseUrl"] = "Basis-URL",
            ["Temperature"] = "Temperatur",
            ["TemperatureHint"] = "Steuert Kreativität und Zufälligkeit der Antworten. Niedrige Werte sind deterministischer, höhere abwechslungsreicher.",
            ["MaxTokens"] = "Max. Ausgabetokens",
            ["MaxTokensHint"] = "Token-Obergrenze pro Aufruf. In AUTO wird der ideale Wert aus der Dateianzahl des Stapels berechnet und überschreitet nie das echte Modelllimit.",
            ["MaxTokensAuto"] = "Auto", ["ModelLimitLabel"] = "Modelllimit: {0} Tokens",
            ["GetApiKey"] = "API-Schlüssel erhalten →", ["ReloadModels"] = "Modelle neu laden",
            ["AddCustomProvider"] = "＋ Anbieter hinzufügen", ["Save"] = "Speichern", ["Delete"] = "Löschen",
            ["Remove"] = "Löschen", ["OptionsPanelTitle"] = "Optionen", ["ConnectionOk"] = "Verbindung OK: {0}",
            ["ConnectionFailed"] = "Verbindung fehlgeschlagen: {0}", ["ProviderNeedsKey"] = "Anbieter {0} erfordert einen API-Schlüssel.",
            ["SaveByokConfirm"] = "BYOK-Einstellungen speichern?", ["DeleteByokConfirm"] = "Anbieter {0} löschen?",
            ["Scheme"] = "Schema", ["LoadingModels"] = "Modelle werden geladen…", ["TestingConnection"] = "Verbindung wird getestet…",
            ["ModelsLoaded"] = "{0} Modelle verfügbar", ["ModelHint"] = "Wählen Sie ein verfügbares Modell",
            ["ByokNeedsFields"] = "Basis-URL und Modell ausfüllen.", ["ByokOkLabel"] = "Gespeichert", ["ByokErrorTitle"] = "BYOK",
            ["ErrAuth"] = "Ungültiger API-Schlüssel. Überprüfen Sie Ihre Anmeldedaten.",
            ["ErrNotFound"] = "Basis-URL oder Modell nicht gefunden (404).",
            ["ErrRateLimit"] = "Ratenlimit erreicht. Warten Sie einen Moment.", ["ErrTimeout"] = "Zeitüberschreitung.",
            ["ErrNetwork"] = "Keine Verbindung möglich. Netzwerk oder Basis-URL prüfen.", ["ErrGeneric"] = "Fehler: {0}",
            ["ErrNoModel"] = "Wählen Sie ein Modell.", ["ProvidersEmpty"] = "Keine Anbieter. Fügen Sie einen hinzu.",
            ["NewProviderDefaultName"] = "Neuer Anbieter",
            // Mode labels
            ["ModeGenérico"] = "📁 Allgemein", ["ModeMúsica"] = "🎵 Musik", ["ModePelículas"] = "🎬 Filme",
            ["ModeSeries"] = "📺 Serien", ["ModeLibros"] = "📚 Bücher",
        },

        ["pt"] = new()
        {
            // Shell / UI
            ["AppTitle"] = "Clasificador IA", ["AppTagline"] = "Organizador de arquivos com IA",
            ["DropText"] = "Selecione uma pasta para classificar seus arquivos",
            ["FileListHeader"] = "Arquivos a classificar", ["FilesFound"] = "arquivos encontrados",
            ["ActionButton"] = "Organizar", ["AboutButton"] = "Sobre", ["DonateButton"] = "Doar",
            ["ExitButton"] = "Sair", ["SelectFilesBtn"] = "Selecionar arquivos", ["ChangeBtn"] = "Alterar",
            ["AddFolder"] = "Adicionar pasta", ["OutputFolder"] = "Pasta de destino",
            ["OutputFolderDefault"] = "Escolher a pasta de saída",
            // Options panel
            ["SourceFolder"] = "Pasta de origem", ["Browse"] = "Explorar", ["Mode"] = "Modo",
            ["Criterion"] = "Critério", ["Depth"] = "Profundidade", ["Categorias5"] = "5 categorias",
            ["Categorias10"] = "10 categorias", ["Categorias15"] = "15 categorias", ["Method"] = "Método",
            ["MethodLocal"] = "Local", ["MethodIA"] = "IA", ["BatchSize"] = "Tamanho do lote",
            ["BatchAll"] = "Tudo (um único lote)", ["BatchStatus"] = "Classificando lote {0}/{1}…",
            ["BatchTimer"] = "Lote {0}/{1} · {2}", ["BatchRetryStatus"] = "Lote {0}/{1}: limite atingido, tentativa {2} em {3}s…",
            ["Otros"] = "Outros", ["CatSoundtrack"] = "Trilha sonora", ["Decade"] = "Década de {0}",
            // Classify
            ["Classify"] = "Classificar", ["Byok"] = "BYOK", ["TestConnection"] = "Testar conexão",
            ["CopyFiles"] = "Copiar arquivos", ["MoveFiles"] = "Mover arquivos", ["OutputMode"] = "Organização",
            ["StatusTotal"] = "arquivos", ["StatusProcessed"] = "processados", ["StatusPending"] = "pendentes",
            ["StatusErrors"] = "erros", ["CategoriasDetectadas"] = "Categorias detectadas ({0})",
            ["ClearAll"] = "Limpar tudo", ["Cancelled"] = "Organização cancelada.",
            ["ClassifyCancelled"] = "Classificação cancelada.", ["AppUpToDate"] = "Você está usando a versão mais recente.",
            ["UpdateAvailable"] = "Há uma nova versão ({0}). Você pode baixá-la em {1}",
            ["AutoByokHint"] = "Configure sua chave de API em BYOK para classificação automática.",
            ["LocalDisclaimer"] = "A classificação local é menos precisa que a IA: usa apenas dados do arquivo (nome, metadados, tipo). Se algo for parar em “Outros”, falta informação local, não é um bug do programa.",
            ["CloseBtn"] = "Fechar",
            // About dialog
            ["AboutTitle"] = "Sobre o Clasificador IA", ["AboutCredits"] = "Créditos", ["AboutVersion"] = "Versão",
            ["AboutDevelopedBy"] = "Desenvolvido por", ["AboutDeveloperName"] = "NoCloudware",
            ["AboutThirdPartyLibraries"] = "Bibliotecas de terceiros", ["AboutWpfUiDesc"] = "Framework de interface",
            ["AboutMvvmDesc"] = "Kit MVVM", ["AboutSpecialThanks"] = "Agradecimentos especiais",
            ["AboutSpecialThanksMessage"] = "Às comunidades de WPF-UI e CommunityToolkit.Mvvm pelo trabalho incrível.",
            ["AboutTechnologiesUsed"] = "Tecnologias usadas",
            ["AboutTechList"] = ".NET 8.0, WPF, WPF-UI, CommunityToolkit.Mvvm",
            ["AboutLicense"] = "Licença", ["AboutLicenseInfo"] = "Licença MIT – Código aberto",
            ["CheckUpdatesBtn"] = "Procurar atualizações",
            // Messages
            ["FolderNotExist"] = "A pasta não existe.", ["NoFilesToClassify"] = "Não há arquivos para classificar.",
            ["NoValidCategories"] = "Nenhuma categoria válida encontrada.", ["OrganizeConfirm"] = "ORGANIZAR arquivos ({0})?",
            ["OrganizeResultTitle"] = "Resultado", ["ResultProcessed"] = "Processados", ["ResultErrors"] = "Erros",
            ["ProcessingFiles"] = "Organizando arquivos…", ["Cancel"] = "Cancelar",
            ["MissingFiles"] = "Os seguintes arquivos não foram encontrados:\n", ["MissingFilesTitle"] = "Arquivos não encontrados",
            ["FilesOrganized"] = "✓ Organização concluída", ["NoResultsToOrganize"] = "Classifique primeiro os arquivos.",
            ["ChooseOutputFolder"] = "Escolha a pasta de saída com o botão Alterar no painel esquerdo.",
            ["DuplicateFileNames"] = "Existem arquivos com o mesmo nome em pastas diferentes: {0}. Classifique-os separadamente.",
            // Dedup panel
            ["DedupTitle"] = "Remover duplicados", ["DedupSameName"] = "Mesmo nome",
            ["DedupMinSize"] = "Menor tamanho", ["DedupMinDate"] = "Data mais antiga",
            // Errors
            ["ErrorIn"] = "Erro em", ["Error"] = "Erro",
            // BYOK dialog
            ["ByokTitle"] = "Configuração BYOK",
            ["ByokDescription"] = "Traga sua própria chave de API e escolha o provedor que preferir.",
            ["Provider"] = "Provedor", ["Model"] = "Modelo", ["ApiKey"] = "Chave de API", ["BaseUrl"] = "URL base",
            ["Temperature"] = "Temperatura",
            ["TemperatureHint"] = "Controla a criatividade e a aleatoriedade das respostas. Valores baixos são mais deterministas, valores altos mais variados.",
            ["MaxTokens"] = "Máx. tokens de saída",
            ["MaxTokensHint"] = "Teto de tokens que a IA pode usar por chamada. Em AUTO, calcula-se o valor ideal pela quantidade de arquivos do lote, sem nunca superar o limite real do modelo.",
            ["MaxTokensAuto"] = "Auto", ["ModelLimitLabel"] = "Limite do modelo: {0} tokens",
            ["GetApiKey"] = "Obter chave de API →", ["ReloadModels"] = "Recarregar modelos",
            ["AddCustomProvider"] = "＋ Adicionar personalizado", ["Save"] = "Salvar", ["Delete"] = "Excluir",
            ["Remove"] = "Excluir", ["OptionsPanelTitle"] = "Opções", ["ConnectionOk"] = "Conexão OK: {0}",
            ["ConnectionFailed"] = "Falha na conexão: {0}", ["ProviderNeedsKey"] = "O provedor {0} requer uma chave de API.",
            ["SaveByokConfirm"] = "Salvar alterações de configuração BYOK?", ["DeleteByokConfirm"] = "Excluir provedor {0}?",
            ["Scheme"] = "Esquema", ["LoadingModels"] = "Carregando modelos…", ["TestingConnection"] = "Testando conexão…",
            ["ModelsLoaded"] = "{0} modelos disponíveis", ["ModelHint"] = "Selecione um modelo disponível",
            ["ByokNeedsFields"] = "Preencha a URL base e o modelo.", ["ByokOkLabel"] = "Salvo", ["ByokErrorTitle"] = "BYOK",
            ["ErrAuth"] = "Chave de API inválida. Verifique suas credenciais.",
            ["ErrNotFound"] = "URL base ou modelo não encontrado (404).",
            ["ErrRateLimit"] = "Limite atingido. Aguarde um instante.", ["ErrTimeout"] = "Tempo esgotado.",
            ["ErrNetwork"] = "Não foi possível conectar. Verifique a rede ou a URL base.", ["ErrGeneric"] = "Erro: {0}",
            ["ErrNoModel"] = "Selecione um modelo.", ["ProvidersEmpty"] = "Nenhum provedor. Adicione um personalizado.",
            ["NewProviderDefaultName"] = "Novo provedor",
            // Mode labels
            ["ModeGenérico"] = "📁 Genérico", ["ModeMúsica"] = "🎵 Música", ["ModePelículas"] = "🎬 Filmes",
            ["ModeSeries"] = "📺 Séries", ["ModeLibros"] = "📚 Livros",
        },

        ["it"] = new()
        {
            // Shell / UI
            ["AppTitle"] = "Clasificador IA", ["AppTagline"] = "Organizzatore di file con IA",
            ["DropText"] = "Seleziona una cartella per classificare i suoi file",
            ["FileListHeader"] = "File da classificare", ["FilesFound"] = "file trovati",
            ["ActionButton"] = "Organizza", ["AboutButton"] = "Informazioni", ["DonateButton"] = "Dona",
            ["ExitButton"] = "Esci", ["SelectFilesBtn"] = "Seleziona file", ["ChangeBtn"] = "Cambia",
            ["AddFolder"] = "Aggiungi cartella", ["OutputFolder"] = "Cartella di destinazione",
            ["OutputFolderDefault"] = "Scegli la cartella di output",
            // Options panel
            ["SourceFolder"] = "Cartella di origine", ["Browse"] = "Sfoglia", ["Mode"] = "Modalità",
            ["Criterion"] = "Criterio", ["Depth"] = "Profondità", ["Categorias5"] = "5 categorie",
            ["Categorias10"] = "10 categorie", ["Categorias15"] = "15 categorie", ["Method"] = "Metodo",
            ["MethodLocal"] = "Locale", ["MethodIA"] = "IA", ["BatchSize"] = "Dimensione lotto",
            ["BatchAll"] = "Tutto (un unico lotto)", ["BatchStatus"] = "Classificazione lotto {0}/{1}…",
            ["BatchTimer"] = "Lotto {0}/{1} · {2}", ["BatchRetryStatus"] = "Lotto {0}/{1}: limite raggiunto, tentativo {2} tra {3}s…",
            ["Otros"] = "Altri", ["CatSoundtrack"] = "Colonna sonora", ["Decade"] = "Anni {0}",
            // Classify
            ["Classify"] = "Classifica", ["Byok"] = "BYOK", ["TestConnection"] = "Prova connessione",
            ["CopyFiles"] = "Copia file", ["MoveFiles"] = "Sposta file", ["OutputMode"] = "Organizzazione",
            ["StatusTotal"] = "file", ["StatusProcessed"] = "elaborati", ["StatusPending"] = "in attesa",
            ["StatusErrors"] = "errori", ["CategoriasDetectadas"] = "Categorie rilevate ({0})",
            ["ClearAll"] = "Svuota tutto", ["Cancelled"] = "Organizzazione annullata.",
            ["ClassifyCancelled"] = "Classificazione annullata.", ["AppUpToDate"] = "Stai usando l'ultima versione.",
            ["UpdateAvailable"] = "È disponibile una nuova versione ({0}). Puoi scaricarla su {1}",
            ["AutoByokHint"] = "Configura la tua chiave API in BYOK per la classificazione automatica.",
            ["LocalDisclaimer"] = "La classificazione locale è meno precisa dell'IA: usa solo i dati del file (nome, metadati, tipo). Se qualcosa finisce in “Altri” è mancanza di informazioni locali, non un bug del programma.",
            ["CloseBtn"] = "Chiudi",
            // About dialog
            ["AboutTitle"] = "Informazioni su Clasificador IA", ["AboutCredits"] = "Crediti", ["AboutVersion"] = "Versione",
            ["AboutDevelopedBy"] = "Sviluppato da", ["AboutDeveloperName"] = "NoCloudware",
            ["AboutThirdPartyLibraries"] = "Librerie di terze parti", ["AboutWpfUiDesc"] = "Framework UI",
            ["AboutMvvmDesc"] = "Toolkit MVVM", ["AboutSpecialThanks"] = "Ringraziamenti speciali",
            ["AboutSpecialThanksMessage"] = "Alle community di WPF-UI e CommunityToolkit.Mvvm per il loro lavoro straordinario.",
            ["AboutTechnologiesUsed"] = "Tecnologie utilizzate",
            ["AboutTechList"] = ".NET 8.0, WPF, WPF-UI, CommunityToolkit.Mvvm",
            ["AboutLicense"] = "Licenza", ["AboutLicenseInfo"] = "Licenza MIT – Open source",
            ["CheckUpdatesBtn"] = "Cerca aggiornamenti",
            // Messages
            ["FolderNotExist"] = "La cartella non esiste.", ["NoFilesToClassify"] = "Nessun file da classificare.",
            ["NoValidCategories"] = "Nessuna categoria valida trovata.", ["OrganizeConfirm"] = "ORGANIZZARE file ({0})?",
            ["OrganizeResultTitle"] = "Risultato", ["ResultProcessed"] = "Elaborati", ["ResultErrors"] = "Errori",
            ["ProcessingFiles"] = "Organizzazione file…", ["Cancel"] = "Annulla",
            ["MissingFiles"] = "I seguenti file non sono stati trovati:\n", ["MissingFilesTitle"] = "File non trovati",
            ["FilesOrganized"] = "✓ Organizzazione completata", ["NoResultsToOrganize"] = "Classifica prima i file.",
            ["ChooseOutputFolder"] = "Scegli la cartella di output con il pulsante Cambia del pannello di sinistra.",
            ["DuplicateFileNames"] = "Esistono file con lo stesso nome da cartelle diverse: {0}. Classificali separatamente.",
            // Dedup panel
            ["DedupTitle"] = "Rimuovi duplicati", ["DedupSameName"] = "Stesso nome",
            ["DedupMinSize"] = "Dimensione minore", ["DedupMinDate"] = "Data meno recente",
            // Errors
            ["ErrorIn"] = "Errore in", ["Error"] = "Errore",
            // BYOK dialog
            ["ByokTitle"] = "Impostazioni BYOK",
            ["ByokDescription"] = "Porta la tua chiave API e scegli il provider che preferisci.",
            ["Provider"] = "Provider", ["Model"] = "Modello", ["ApiKey"] = "Chiave API", ["BaseUrl"] = "URL di base",
            ["Temperature"] = "Temperatura",
            ["TemperatureHint"] = "Controlla creatività e casualità delle risposte. Valori bassi sono più deterministici, valori alti più vari.",
            ["MaxTokens"] = "Max. token di output",
            ["MaxTokensHint"] = "Tetto di token che l'IA può usare per chiamata. In AUTO calcola il valore ideale in base al numero di file del lotto, senza mai superare il limite reale del modello.",
            ["MaxTokensAuto"] = "Auto", ["ModelLimitLabel"] = "Limite del modello: {0} token",
            ["GetApiKey"] = "Ottieni chiave API →", ["ReloadModels"] = "Ricarica modelli",
            ["AddCustomProvider"] = "＋ Aggiungi personalizzato", ["Save"] = "Salva", ["Delete"] = "Elimina",
            ["Remove"] = "Elimina", ["OptionsPanelTitle"] = "Opzioni", ["ConnectionOk"] = "Connessione OK: {0}",
            ["ConnectionFailed"] = "Connessione non riuscita: {0}", ["ProviderNeedsKey"] = "Il provider {0} richiede una chiave API.",
            ["SaveByokConfirm"] = "Salvare le modifiche BYOK?", ["DeleteByokConfirm"] = "Eliminare il provider {0}?",
            ["Scheme"] = "Schema", ["LoadingModels"] = "Caricamento modelli…", ["TestingConnection"] = "Prova connessione…",
            ["ModelsLoaded"] = "{0} modelli disponibili", ["ModelHint"] = "Seleziona un modello disponibile",
            ["ByokNeedsFields"] = "Compila URL di base e modello.", ["ByokOkLabel"] = "Salvato", ["ByokErrorTitle"] = "BYOK",
            ["ErrAuth"] = "Chiave API non valida. Verifica le credenziali.",
            ["ErrNotFound"] = "URL di base o modello non trovato (404).",
            ["ErrRateLimit"] = "Limite di richieste raggiunto. Attendi un momento.", ["ErrTimeout"] = "Tempo scaduto.",
            ["ErrNetwork"] = "Impossibile connettersi. Controlla rete o URL di base.", ["ErrGeneric"] = "Errore: {0}",
            ["ErrNoModel"] = "Seleziona un modello.", ["ProvidersEmpty"] = "Nessun provider. Aggiungine uno.",
            ["NewProviderDefaultName"] = "Nuovo provider",
            // Mode labels
            ["ModeGenérico"] = "📁 Generico", ["ModeMúsica"] = "🎵 Musica", ["ModePelículas"] = "🎬 Film",
            ["ModeSeries"] = "📺 Serie", ["ModeLibros"] = "📚 Libri",
        },

        ["ja"] = new()
        {
            // Shell / UI
            ["AppTitle"] = "Clasificador IA", ["AppTagline"] = "AI ファイル整理",
            ["DropText"] = "フォルダーを選択してファイルを分類",
            ["FileListHeader"] = "分類するファイル", ["FilesFound"] = "件のファイルが見つかりました",
            ["ActionButton"] = "整理", ["AboutButton"] = "このアプリについて", ["DonateButton"] = "寄付",
            ["ExitButton"] = "終了", ["SelectFilesBtn"] = "ファイルを選択", ["ChangeBtn"] = "変更",
            ["AddFolder"] = "フォルダーを追加", ["OutputFolder"] = "保存先フォルダー",
            ["OutputFolderDefault"] = "出力フォルダーを選択",
            // Options panel
            ["SourceFolder"] = "元のフォルダー", ["Browse"] = "参照", ["Mode"] = "モード",
            ["Criterion"] = "基準", ["Depth"] = "階層", ["Categorias5"] = "5カテゴリ",
            ["Categorias10"] = "10カテゴリ", ["Categorias15"] = "15カテゴリ", ["Method"] = "方法",
            ["MethodLocal"] = "ローカル", ["MethodIA"] = "AI", ["BatchSize"] = "バッチサイズ",
            ["BatchAll"] = "すべて（1バッチ）", ["BatchStatus"] = "バッチ{0}/{1}を分類中…",
            ["BatchTimer"] = "バッチ{0}/{1} · {2}", ["BatchRetryStatus"] = "バッチ{0}/{1}: 制限到達、{3}秒後に再試行 {2}…",
            ["Otros"] = "その他", ["CatSoundtrack"] = "サウンドトラック", ["Decade"] = "{0}年代",
            // Classify
            ["Classify"] = "分類", ["Byok"] = "BYOK", ["TestConnection"] = "接続をテスト",
            ["CopyFiles"] = "ファイルをコピー", ["MoveFiles"] = "ファイルを移動", ["OutputMode"] = "整理方法",
            ["StatusTotal"] = "ファイル", ["StatusProcessed"] = "処理済み", ["StatusPending"] = "保留中",
            ["StatusErrors"] = "エラー", ["CategoriasDetectadas"] = "検出されたカテゴリ（{0}）",
            ["ClearAll"] = "すべてクリア", ["Cancelled"] = "整理をキャンセルしました。",
            ["ClassifyCancelled"] = "分類をキャンセルしました。", ["AppUpToDate"] = "最新バージョンを使用しています。",
            ["UpdateAvailable"] = "新しいバージョン（{0}）があります。{1} からダウンロードできます",
            ["AutoByokHint"] = "自動分類には BYOK で API キーを設定してください。",
            ["LocalDisclaimer"] = "ローカル分類は AI より精度が低く、ファイルのデータ（名前、メタデータ、種類）のみを使用します。「その他」に入るのはローカル情報の不足によるもので、プログラムの不具合ではありません。",
            ["CloseBtn"] = "閉じる",
            // About dialog
            ["AboutTitle"] = "Clasificador IA について", ["AboutCredits"] = "クレジット", ["AboutVersion"] = "バージョン",
            ["AboutDevelopedBy"] = "開発者", ["AboutDeveloperName"] = "NoCloudware",
            ["AboutThirdPartyLibraries"] = "サードパーティ製ライブラリ", ["AboutWpfUiDesc"] = "UI フレームワーク",
            ["AboutMvvmDesc"] = "MVVM ツールキット", ["AboutSpecialThanks"] = "スペシャルサンクス",
            ["AboutSpecialThanksMessage"] = "WPF-UI と CommunityToolkit.Mvvm のコミュニティによる素晴らしい仕事に感謝します。",
            ["AboutTechnologiesUsed"] = "使用技術",
            ["AboutTechList"] = ".NET 8.0, WPF, WPF-UI, CommunityToolkit.Mvvm",
            ["AboutLicense"] = "ライセンス", ["AboutLicenseInfo"] = "MIT ライセンス – オープンソース",
            ["CheckUpdatesBtn"] = "更新を確認",
            // Messages
            ["FolderNotExist"] = "フォルダーが存在しません。", ["NoFilesToClassify"] = "分類するファイルがありません。",
            ["NoValidCategories"] = "有効なカテゴリが見つかりません。", ["OrganizeConfirm"] = "ファイルを整理しますか（{0}件）？",
            ["OrganizeResultTitle"] = "結果", ["ResultProcessed"] = "処理済み", ["ResultErrors"] = "エラー",
            ["ProcessingFiles"] = "ファイルを整理中…", ["Cancel"] = "キャンセル",
            ["MissingFiles"] = "以下のファイルが見つかりませんでした：\n", ["MissingFilesTitle"] = "ファイルが見つかりません",
            ["FilesOrganized"] = "✓ 整理が完了しました", ["NoResultsToOrganize"] = "先にファイルを分類してください。",
            ["ChooseOutputFolder"] = "左のパネルの「変更」ボタンで出力フォルダーを選択してください。",
            ["DuplicateFileNames"] = "別のフォルダーに同名のファイルがあります：{0}。個別に分類してください。",
            // Dedup panel
            ["DedupTitle"] = "重複を削除", ["DedupSameName"] = "同じ名前",
            ["DedupMinSize"] = "最小サイズ", ["DedupMinDate"] = "最も古い日付",
            // Errors
            ["ErrorIn"] = "エラー:", ["Error"] = "エラー",
            // BYOK dialog
            ["ByokTitle"] = "BYOK 設定",
            ["ByokDescription"] = "ご自身の API キーを持参し、お好みのプロバイダーを選択できます。",
            ["Provider"] = "プロバイダー", ["Model"] = "モデル", ["ApiKey"] = "API キー", ["BaseUrl"] = "ベース URL",
            ["Temperature"] = "温度",
            ["TemperatureHint"] = "応答の創造性とランダム性を制御します。低い値ほど決定的に、高い値ほど多様になります。",
            ["MaxTokens"] = "最大出力トークン",
            ["MaxTokensHint"] = "1回の呼び出しで AI が使用できるトークンの上限。AUTO ではバッチのファイル数から理想値を計算し、モデルの実際の上限を超えません。",
            ["MaxTokensAuto"] = "自動", ["ModelLimitLabel"] = "モデル上限: {0} トークン",
            ["GetApiKey"] = "API キーを取得 →", ["ReloadModels"] = "モデルを再読み込み",
            ["AddCustomProvider"] = "＋ カスタムを追加", ["Save"] = "保存", ["Delete"] = "削除",
            ["Remove"] = "削除", ["OptionsPanelTitle"] = "オプション", ["ConnectionOk"] = "接続 OK: {0}",
            ["ConnectionFailed"] = "接続失敗: {0}", ["ProviderNeedsKey"] = "プロバイダー {0} には API キーが必要です。",
            ["SaveByokConfirm"] = "BYOK 設定を保存しますか？", ["DeleteByokConfirm"] = "プロバイダー {0} を削除しますか？",
            ["Scheme"] = "スキーム", ["LoadingModels"] = "モデルを読み込み中…", ["TestingConnection"] = "接続をテスト中…",
            ["ModelsLoaded"] = "{0} 個のモデルが利用可能", ["ModelHint"] = "利用可能なモデルを選択",
            ["ByokNeedsFields"] = "ベース URL とモデルを入力してください。", ["ByokOkLabel"] = "保存しました", ["ByokErrorTitle"] = "BYOK",
            ["ErrAuth"] = "API キーが無効です。認証情報を確認してください。",
            ["ErrNotFound"] = "ベース URL またはモデルが見つかりません（404）。",
            ["ErrRateLimit"] = "レート制限に達しました。しばらくお待ちください。", ["ErrTimeout"] = "タイムアウトしました。",
            ["ErrNetwork"] = "接続できません。ネットワークまたはベース URL を確認してください。", ["ErrGeneric"] = "エラー: {0}",
            ["ErrNoModel"] = "モデルを選択してください。", ["ProvidersEmpty"] = "プロバイダーがありません。カスタムを追加してください。",
            ["NewProviderDefaultName"] = "新しいプロバイダー",
            // Mode labels
            ["ModeGenérico"] = "📁 一般", ["ModeMúsica"] = "🎵 音楽", ["ModePelículas"] = "🎬 映画",
            ["ModeSeries"] = "📺 ドラマ", ["ModeLibros"] = "📚 本",
        },

        ["zh"] = new()
        {
            // Shell / UI
            ["AppTitle"] = "Clasificador IA", ["AppTagline"] = "AI 文件整理器",
            ["DropText"] = "选择一个文件夹以分类其中文件",
            ["FileListHeader"] = "待分类的文件", ["FilesFound"] = "个文件",
            ["ActionButton"] = "整理", ["AboutButton"] = "关于", ["DonateButton"] = "捐赠",
            ["ExitButton"] = "退出", ["SelectFilesBtn"] = "选择文件", ["ChangeBtn"] = "更改",
            ["AddFolder"] = "添加文件夹", ["OutputFolder"] = "目标文件夹",
            ["OutputFolderDefault"] = "选择输出文件夹",
            // Options panel
            ["SourceFolder"] = "源文件夹", ["Browse"] = "浏览", ["Mode"] = "模式",
            ["Criterion"] = "标准", ["Depth"] = "深度", ["Categorias5"] = "5 个分类",
            ["Categorias10"] = "10 个分类", ["Categorias15"] = "15 个分类", ["Method"] = "方法",
            ["MethodLocal"] = "本地", ["MethodIA"] = "AI", ["BatchSize"] = "批次大小",
            ["BatchAll"] = "全部（单批次）", ["BatchStatus"] = "正在分类批次 {0}/{1}…",
            ["BatchTimer"] = "批次 {0}/{1} · {2}", ["BatchRetryStatus"] = "批次 {0}/{1}：已达限制，{3} 秒后重试 {2}…",
            ["Otros"] = "其他", ["CatSoundtrack"] = "原声带", ["Decade"] = "{0}年代",
            // Classify
            ["Classify"] = "分类", ["Byok"] = "BYOK", ["TestConnection"] = "测试连接",
            ["CopyFiles"] = "复制文件", ["MoveFiles"] = "移动文件", ["OutputMode"] = "整理方式",
            ["StatusTotal"] = "个文件", ["StatusProcessed"] = "已处理", ["StatusPending"] = "待处理",
            ["StatusErrors"] = "错误", ["CategoriasDetectadas"] = "检测到的分类（{0}）",
            ["ClearAll"] = "全部清除", ["Cancelled"] = "整理已取消。",
            ["ClassifyCancelled"] = "分类已取消。", ["AppUpToDate"] = "您正在使用最新版本。",
            ["UpdateAvailable"] = "有新版本（{0}）。可在 {1} 下载",
            ["AutoByokHint"] = "请在 BYOK 中配置您的 API 密钥以进行自动分类。",
            ["LocalDisclaimer"] = "本地分类不如 AI 精确：仅使用文件数据（名称、元数据、类型）。如果内容落入“其他”，是因为缺少本地信息，而非程序错误。",
            ["CloseBtn"] = "关闭",
            // About dialog
            ["AboutTitle"] = "关于 Clasificador IA", ["AboutCredits"] = "致谢", ["AboutVersion"] = "版本",
            ["AboutDevelopedBy"] = "开发者", ["AboutDeveloperName"] = "NoCloudware",
            ["AboutThirdPartyLibraries"] = "第三方库", ["AboutWpfUiDesc"] = "界面框架",
            ["AboutMvvmDesc"] = "MVVM 工具包", ["AboutSpecialThanks"] = "特别感谢",
            ["AboutSpecialThanksMessage"] = "感谢 WPF-UI 和 CommunityToolkit.Mvvm 社区的出色工作。",
            ["AboutTechnologiesUsed"] = "使用的技术",
            ["AboutTechList"] = ".NET 8.0, WPF, WPF-UI, CommunityToolkit.Mvvm",
            ["AboutLicense"] = "许可证", ["AboutLicenseInfo"] = "MIT 许可证 – 开源",
            ["CheckUpdatesBtn"] = "检查更新",
            // Messages
            ["FolderNotExist"] = "文件夹不存在。", ["NoFilesToClassify"] = "没有要分类的文件。",
            ["NoValidCategories"] = "未找到有效分类。", ["OrganizeConfirm"] = "要整理文件（{0}个）吗？",
            ["OrganizeResultTitle"] = "结果", ["ResultProcessed"] = "已处理", ["ResultErrors"] = "错误",
            ["ProcessingFiles"] = "正在整理文件…", ["Cancel"] = "取消",
            ["MissingFiles"] = "未找到以下文件：\n", ["MissingFilesTitle"] = "未找到文件",
            ["FilesOrganized"] = "✓ 整理完成", ["NoResultsToOrganize"] = "请先对文件进行分类。",
            ["ChooseOutputFolder"] = "请使用左侧面板的“更改”按钮选择输出文件夹。",
            ["DuplicateFileNames"] = "不同文件夹中存在同名文件：{0}。请分别分类。",
            // Dedup panel
            ["DedupTitle"] = "删除重复文件", ["DedupSameName"] = "同名文件",
            ["DedupMinSize"] = "最小体积", ["DedupMinDate"] = "最早日期",
            // Errors
            ["ErrorIn"] = "错误于", ["Error"] = "错误",
            // BYOK dialog
            ["ByokTitle"] = "BYOK 设置",
            ["ByokDescription"] = "带上您自己的 API 密钥，选择偏好的提供商。",
            ["Provider"] = "提供商", ["Model"] = "模型", ["ApiKey"] = "API 密钥", ["BaseUrl"] = "基础网址",
            ["Temperature"] = "温度",
            ["TemperatureHint"] = "控制回应的创造性和随机性。值越低越确定，越高越多样。",
            ["MaxTokens"] = "最大输出令牌数",
            ["MaxTokensHint"] = "AI 每次调用可使用的令牌上限。在“自动”下，会根据批次文件数计算理想值，且不超过模型的真实上限。",
            ["MaxTokensAuto"] = "自动", ["ModelLimitLabel"] = "模型上限：{0} 个令牌",
            ["GetApiKey"] = "获取 API 密钥 →", ["ReloadModels"] = "重新加载模型",
            ["AddCustomProvider"] = "＋ 添加自定义", ["Save"] = "保存", ["Delete"] = "删除",
            ["Remove"] = "删除", ["OptionsPanelTitle"] = "选项", ["ConnectionOk"] = "连接成功：{0}",
            ["ConnectionFailed"] = "连接失败：{0}", ["ProviderNeedsKey"] = "提供商 {0} 需要 API 密钥。",
            ["SaveByokConfirm"] = "保存 BYOK 设置更改？", ["DeleteByokConfirm"] = "删除提供商 {0}？",
            ["Scheme"] = "方案", ["LoadingModels"] = "正在加载模型…", ["TestingConnection"] = "正在测试连接…",
            ["ModelsLoaded"] = "{0} 个模型可用", ["ModelHint"] = "选择可用模型",
            ["ByokNeedsFields"] = "请填写基础网址和模型。", ["ByokOkLabel"] = "已保存", ["ByokErrorTitle"] = "BYOK",
            ["ErrAuth"] = "API 密钥无效。请检查您的凭据。",
            ["ErrNotFound"] = "找不到基础网址或模型（404）。",
            ["ErrRateLimit"] = "已达速率限制。请稍等片刻。", ["ErrTimeout"] = "连接超时。",
            ["ErrNetwork"] = "无法连接。请检查网络或基础网址。", ["ErrGeneric"] = "错误：{0}",
            ["ErrNoModel"] = "请选择模型。", ["ProvidersEmpty"] = "没有提供商。请添加自定义提供商。",
            ["NewProviderDefaultName"] = "新提供商",
            // Mode labels
            ["ModeGenérico"] = "📁 通用", ["ModeMúsica"] = "🎵 音乐", ["ModePelículas"] = "🎬 电影",
            ["ModeSeries"] = "📺 剧集", ["ModeLibros"] = "📚 书籍",
        },
    };

    public static string Get(string key, Idioma idioma)
    {
        if (Data.TryGetValue(idioma.CultureCode(), out var dict) && dict.TryGetValue(key, out var value))
            return value;
        if (Data.TryGetValue("en", out var fallback) && fallback.TryGetValue(key, out var en))
            return en;
        return key;
    }

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

    public static string ModeLabel(string modeKey, Idioma idioma) => Get("Mode" + modeKey, idioma);
}