using System.Windows;

namespace NoCloudware.UI.Core.Helpers;

public static class WindowPositioner
{
    public static void CenterOnScreen(Window window)
    {
        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    public static void CenterOnOwner(Window window)
    {
        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    public static void RestoreFromMinimized(Window window)
    {
        if (window.WindowState == WindowState.Minimized)
            window.WindowState = WindowState.Normal;
        window.Activate();
    }
}