using System.ComponentModel;

namespace AiPlayground.Services.Abstractions;

/// <summary>
/// 文件系统抽象接口 - 用于测试时模拟文件系统操作
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    bool FileExists(string path);

    /// <summary>
    /// 读取文件全部文本
    /// </summary>
    string ReadAllText(string path);

    /// <summary>
    /// 异步读取文件全部文本
    /// </summary>
    Task<string> ReadAllTextAsync(string path);

    /// <summary>
    /// 写入文件全部文本
    /// </summary>
    void WriteAllText(string path, string content);

    /// <summary>
    /// 异步写入文件全部文本
    /// </summary>
    Task WriteAllTextAsync(string path, string content);

    /// <summary>
    /// 创建目录
    /// </summary>
    void CreateDirectory(string path);

    /// <summary>
    /// 检查目录是否存在
    /// </summary>
    bool DirectoryExists(string path);

    /// <summary>
    /// 获取目录下的文件列表
    /// </summary>
    string[] GetFiles(string directory, string searchPattern);

    /// <summary>
    /// 删除文件
    /// </summary>
    void DeleteFile(string path);

    /// <summary>
    /// 合并路径
    /// </summary>
    string CombinePaths(params string[] paths);

    /// <summary>
    /// 获取特殊文件夹路径
    /// </summary>
    string GetSpecialFolder(Environment.SpecialFolder folder);

    /// <summary>
    /// 获取文件所在目录路径
    /// </summary>
    string? GetDirectoryName(string path);
}
