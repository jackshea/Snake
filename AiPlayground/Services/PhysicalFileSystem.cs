using System.ComponentModel;
using AiPlayground.Services.Abstractions;

namespace AiPlayground.Services;

/// <summary>
/// 物理文件系统实现 - 使用实际的文件系统操作
/// </summary>
public class PhysicalFileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);

    public string ReadAllText(string path) => File.ReadAllText(path);

    public Task<string> ReadAllTextAsync(string path) => File.ReadAllTextAsync(path);

    public void WriteAllText(string path, string content) => File.WriteAllText(path, content);

    public Task WriteAllTextAsync(string path, string content) => File.WriteAllTextAsync(path, content);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public string[] GetFiles(string directory, string searchPattern) =>
        Directory.GetFiles(directory, searchPattern);

    public void DeleteFile(string path) => File.Delete(path);

    public string CombinePaths(params string[] paths) => Path.Combine(paths);

    public string GetSpecialFolder(Environment.SpecialFolder folder) =>
        Environment.GetFolderPath(folder);

    public string? GetDirectoryName(string path) => Path.GetDirectoryName(path);
}
