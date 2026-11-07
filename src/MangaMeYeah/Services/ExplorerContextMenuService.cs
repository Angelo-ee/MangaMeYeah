using System.Diagnostics;
using Microsoft.Win32;

namespace MangaMeYeah.Services;

public static class ExplorerContextMenuService
{
    private const string FolderShellKey = @"Software\Classes\Directory\shell\MangaMeYeah";
    private const string FolderBgShellKey = @"Software\Classes\Directory\Background\shell\MangaMeYeah";
    private const string FileShellKey = @"Software\Classes\*\shell\MangaMeYeah";

    public static string GetExecutablePath()
    {
        return Environment.ProcessPath
            ?? Process.GetCurrentProcess().MainModule?.FileName
            ?? string.Empty;
    }

    public static bool IsRegistered()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(FolderShellKey + @"\command");
            if (key?.GetValue("") is string cmd)
            {
                string exePath = GetExecutablePath();
                if (!string.IsNullOrEmpty(exePath) && cmd.Contains(exePath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public static (bool Success, string Message) Register(bool includeFolder, bool includeFolderBackground, bool includeFiles)
    {
        string exePath = GetExecutablePath();
        if (string.IsNullOrEmpty(exePath))
        {
            return (false, "无法获取可执行文件路径。");
        }

        try
        {
            if (includeFolder)
            {
                RegisterShellEntry(FolderShellKey, "用 MangaMeYeah! 打开(&M)", exePath, $"\"{exePath}\" \"%1\"");
            }
            else
            {
                Registry.CurrentUser.DeleteSubKeyTree(FolderShellKey, throwOnMissingSubKey: false);
            }

            if (includeFolderBackground)
            {
                RegisterShellEntry(FolderBgShellKey, "用 MangaMeYeah! 打开此文件夹(&M)", exePath, $"\"{exePath}\" \"%V\"");
            }
            else
            {
                Registry.CurrentUser.DeleteSubKeyTree(FolderBgShellKey, throwOnMissingSubKey: false);
            }

            if (includeFiles)
            {
                RegisterShellEntry(FileShellKey, "用 MangaMeYeah! 打开(&M)", exePath, $"\"{exePath}\" \"%1\"");
            }
            else
            {
                Registry.CurrentUser.DeleteSubKeyTree(FileShellKey, throwOnMissingSubKey: false);
            }

            return (true, "已更新资源管理器右键菜单设置。");
        }
        catch (Exception ex)
        {
            return (false, $"注册表更新错误: {ex.Message}");
        }
    }

    public static (bool Success, string Message) UnregisterAll()
    {
        try
        {
            Registry.CurrentUser.DeleteSubKeyTree(FolderShellKey, throwOnMissingSubKey: false);
            Registry.CurrentUser.DeleteSubKeyTree(FolderBgShellKey, throwOnMissingSubKey: false);
            Registry.CurrentUser.DeleteSubKeyTree(FileShellKey, throwOnMissingSubKey: false);
            return (true, "已从资源管理器右键菜单中删除全部「用 MangaMeYeah! 打开」。");
        }
        catch (Exception ex)
        {
            return (false, $"注册表删除错误: {ex.Message}");
        }
    }

    private static void RegisterShellEntry(string baseKeyPath, string menuText, string iconPath, string commandString)
    {
        using var shellKey = Registry.CurrentUser.CreateSubKey(baseKeyPath);
        shellKey.SetValue("", menuText);
        shellKey.SetValue("Icon", $"\"{iconPath}\",0");

        using var cmdKey = shellKey.CreateSubKey("command");
        cmdKey.SetValue("", commandString);
    }
}
