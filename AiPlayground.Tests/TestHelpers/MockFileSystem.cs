using System.ComponentModel;
using AiPlayground.Services.Abstractions;

namespace AiPlayground.Tests.TestHelpers;

/// <summary>
/// 模拟文件系统 - 用于测试文件系统操作
/// </summary>
public class MockFileSystem : IFileSystem
{
    private readonly Dictionary<string, string> _files = new();
    private readonly HashSet<string> _directories = new();

    public MockFileSystem()
    {
        // 默认创建一些常用目录
        _directories.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PresetLevels"));
    }

    public bool FileExists(string path) => _files.ContainsKey(path);

    public string ReadAllText(string path)
    {
        if (!_files.TryGetValue(path, out var content))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }
        return content;
    }

    public Task<string> ReadAllTextAsync(string path) =>
        Task.FromResult(ReadAllText(path));

    public void WriteAllText(string path, string content)
    {
        _files[path] = content;
    }

    public Task WriteAllTextAsync(string path, string content)
    {
        WriteAllText(path, content);
        return Task.CompletedTask;
    }

    public void CreateDirectory(string path)
    {
        _directories.Add(path);
    }

    public bool DirectoryExists(string path) => _directories.Contains(path);

    public string[] GetFiles(string directory, string searchPattern)
    {
        if (!_directories.Contains(directory))
        {
            return Array.Empty<string>();
        }

        return _files.Keys
            .Where(f => Path.GetDirectoryName(f) == directory && Path.GetFileName(f).EndsWith(searchPattern.Replace("*", "")))
            .ToArray();
    }

    public void DeleteFile(string path)
    {
        _files.Remove(path);
    }

    public string CombinePaths(params string[] paths) => Path.Combine(paths);

    public string GetSpecialFolder(Environment.SpecialFolder folder) =>
        folder == Environment.SpecialFolder.ApplicationData
            ? Path.GetTempPath() // 在测试中使用临时目录
            : Path.GetTempPath();

    public string? GetDirectoryName(string path) => Path.GetDirectoryName(path);

    /// <summary>
    /// 设置模拟文件内容
    /// </summary>
    public void SetupFile(string path, string content)
    {
        _files[path] = content;
        // 确保目录存在
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            _directories.Add(directory);
        }
    }

    /// <summary>
    /// 清空所有文件和目录
    /// </summary>
    public void Reset()
    {
        _files.Clear();
        _directories.Clear();
    }

    /// <summary>
    /// 添加模拟目录
    /// </summary>
    public void AddDirectory(string path)
    {
        _directories.Add(path);
    }
}
