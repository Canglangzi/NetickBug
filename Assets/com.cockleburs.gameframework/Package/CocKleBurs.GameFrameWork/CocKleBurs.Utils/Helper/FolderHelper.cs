using System.IO;
using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
public static class FolderHandler
{
    /// <summary>
    /// 确保目录存在，如果不存在则自动创建。
    /// </summary>
    /// <param name="filePathOrDirectory">可以是完整的文件路径，也可以是目录路径。</param>
    public static void EnsureDirectory(string filePathOrDirectory)
    {
        string dir = Path.HasExtension(filePathOrDirectory)
            ? Path.GetDirectoryName(filePathOrDirectory)
            : filePathOrDirectory;

        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
#if UNITY_EDITOR
            Debug.Log($"[DirectoryHandler] 创建目录: {dir}");
#endif
        }
    }

    /// <summary>
    /// 删除整个目录（包括子文件和子文件夹）。
    /// </summary>
    public static void DeleteDirectory(string directoryPath, bool recursive = true)
    {
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, recursive);
#if UNITY_EDITOR
            Debug.Log($"[DirectoryHandler] 删除目录: {directoryPath}");
#endif
        }
    }

    /// <summary>
    /// 清空目录但不删除目录本身。
    /// </summary>
    public static void ClearDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return;

        var files = Directory.GetFiles(directoryPath);
        var dirs = Directory.GetDirectories(directoryPath);

        foreach (var file in files)
        {
            File.Delete(file);
        }

        foreach (var dir in dirs)
        {
            Directory.Delete(dir, true);
        }

#if UNITY_EDITOR
        Debug.Log($"[DirectoryHandler] 清空目录内容: {directoryPath}");
#endif
    }
}
}