namespace ClasificadorIA.Services;

public static class DedupFilter
{
    private static readonly System.Text.RegularExpressions.Regex TrackPrefix =
        new(@"^\s*\d+[\s._-]+", System.Text.RegularExpressions.RegexOptions.Compiled);

    // Clave de comparación: sin el número de pista del inicio del nombre.
    // "001. Wham! - Wake Me Up.mp3" y "042. Wham! - Wake Me Up.mp3" → misma clave.
    // Nombres que son solo un número ("1984.mp3") quedan intactos.
    public static string NormalizeKey(string name)
    {
        string baseName = System.IO.Path.GetFileNameWithoutExtension(name);
        string normalized = TrackPrefix.Replace(baseName, "").Trim();
        if (normalized.Length == 0) return baseName;
        return normalized + System.IO.Path.GetExtension(name);
    }

    // sameName=false → nada se elimina (desactiva la eliminación de duplicados).
    // sizeEnabled/dateEnabled activan cada eje; los pares *KeepLargest fijan la dirección:
    //   true  → conserva el MAYOR (elimina el menor) de ese eje.
    //   false → conserva el MENOR (elimina el mayor).
    // Se aplican en serie: tamaño primero, fecha sobre los supervivientes.
    public static List<T> Keep<T>(IReadOnlyList<T> items,
        Func<T, string> getName, Func<T, long> getSize, Func<T, DateTime> getDate,
        bool sameName, bool sizeEnabled, bool sizeKeepLargest, bool dateEnabled, bool dateKeepLargest)
    {
        if (!sameName)
            return items.ToList();

        var result = new List<T>(items.Count);
        foreach (var group in items.GroupBy(x => NormalizeKey(getName(x)), StringComparer.OrdinalIgnoreCase))
        {
            var kept = group.ToList();
            if (kept.Count > 1)
            {
                if (sizeEnabled)
                    kept = ApplyRule(kept, x => getSize(x), sizeKeepLargest);
                if (kept.Count > 1 && dateEnabled)
                    kept = ApplyRule(kept, x => getDate(x), dateKeepLargest);
            }
            result.AddRange(kept);
        }
        return result;
    }

    private static List<T> ApplyRule<T>(List<T> group, Func<T, long> getValue, bool keepLargest)
    {
        var target = keepLargest ? group.Max(getValue) : group.Min(getValue);
        return group.Where(x => getValue(x) == target).ToList();
    }

    private static List<T> ApplyRule<T>(List<T> group, Func<T, DateTime> getValue, bool keepLargest)
    {
        var target = keepLargest ? group.Max(getValue) : group.Min(getValue);
        return group.Where(x => getValue(x) == target).ToList();
    }
}