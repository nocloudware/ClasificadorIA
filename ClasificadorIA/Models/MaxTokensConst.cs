namespace ClasificadorIA.Models;

/// <summary>Rangos y cálculo automático del tope de tokens de salida por llamada.</summary>
public static class MaxTokensConst
{
    /// <summary>Valor cuando el usuario elige "Auto": se calcula según la cantidad de archivos enviados.</summary>
    public const int Max = 32768;

    /// <summary>Común en la mayoría de modelos no razonadores.</summary>
    public const int Mini = 4096;

    /// <summary>Cálculo para asignación. Los modelos con razonamiento (deepseek-reasoner / v4-flash)
    /// consumen su cadena de pensamiento del MISMO presupuesto, así que el margen es generoso:
    /// ~30 tokens por archivo + base para pensar y luego emitir el JSON final.</summary>
    public static int ForFiles(int fileCount) => Math.Clamp(fileCount * 30 + 2000, Mini, Max);

    /// <summary>Consolidación: categorías + mapeo de finales, ~8 tokens por categoría más margen.</summary>
    public static int ForConsolidation(int categoryCount) => Math.Clamp(categoryCount * 8 + 1000, Mini, Max);
}