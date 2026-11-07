using System.IO;
using MangaMeYeah.Models;

namespace MangaMeYeah.Services;

public static class QuickSaveService
{
    public static (bool Success, string Message) ExecuteQuickSave(
        int slotNumber,
        IReadOnlyList<string> sourceFilePaths,
        ViewerSettings settings)
    {
        if (sourceFilePaths.Count == 0)
        {
            return (false, "尚未打开要保存的图像。");
        }

        var slot = settings.QuickSaveSlots.FirstOrDefault(s => s.SlotNumber == slotNumber);
        if (slot == null || string.IsNullOrWhiteSpace(slot.FolderPath))
        {
            return (false, $"快速保存 {slotNumber} 尚未设置目标文件夹，请在「文件 > 快速保存设置」中设置。");
        }

        try
        {
            if (!Directory.Exists(slot.FolderPath))
            {
                Directory.CreateDirectory(slot.FolderPath);
            }

            var savedNames = new List<string>();
            foreach (string srcPath in sourceFilePaths)
            {
                if (!File.Exists(srcPath)) continue;

                string fileName = Path.GetFileName(srcPath);
                if (slot.PrependFolderName)
                {
                    string? parentDir = Path.GetDirectoryName(srcPath);
                    string folderName = !string.IsNullOrEmpty(parentDir) ? Path.GetFileName(parentDir) : "";
                    if (!string.IsNullOrEmpty(folderName) && !fileName.StartsWith(folderName, StringComparison.OrdinalIgnoreCase))
                    {
                        fileName = $"{folderName}_{fileName}";
                    }
                }

                string destPath = GetUniqueDestinationPath(slot.FolderPath, fileName);
                File.Copy(srcPath, destPath, overwrite: false);
                savedNames.Add(Path.GetFileName(destPath));
            }

            if (savedNames.Count == 0)
            {
                return (false, "没有可保存的文件。");
            }

            string summary = savedNames.Count == 1
                ? savedNames[0]
                : $"{savedNames[0]} 等共 {savedNames.Count} 张";

            return (true, $"[QuickSave{slotNumber}] 保存完成: {summary} → {slot.FolderPath}");
        }
        catch (Exception ex)
        {
            return (false, $"[QuickSave{slotNumber}] 保存错误: {ex.Message}");
        }
    }

    private static string GetUniqueDestinationPath(string folder, string fileName)
    {
        string destPath = Path.Combine(folder, fileName);
        if (!File.Exists(destPath))
        {
            return destPath;
        }

        string baseName = Path.GetFileNameWithoutExtension(fileName);
        string ext = Path.GetExtension(fileName);
        int counter = 1;
        while (true)
        {
            string candidate = Path.Combine(folder, $"{baseName} ({counter}){ext}");
            if (!File.Exists(candidate))
            {
                return candidate;
            }
            counter++;
        }
    }
}
