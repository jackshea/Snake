using System.ComponentModel;
using AiPlayground.Services;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Services;

public class PhysicalFileSystemTests : IDisposable
{
    private readonly string _tempDir;
    private readonly PhysicalFileSystem _fileSystem;

    public PhysicalFileSystemTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _fileSystem = new PhysicalFileSystem();
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public void FileExists_WithExistingFile_ShouldReturnTrue()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "test.txt");
        File.WriteAllText(filePath, "content");

        // Act
        var result = _fileSystem.FileExists(filePath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FileExists_WithNonExistingFile_ShouldReturnFalse()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "nonexistent.txt");

        // Act
        var result = _fileSystem.FileExists(filePath);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ReadAllText_ShouldReturnFileContent()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "test.txt");
        var content = "Hello, World!";
        File.WriteAllText(filePath, content);

        // Act
        var result = _fileSystem.ReadAllText(filePath);

        // Assert
        result.Should().Be(content);
    }

    [Fact]
    public async Task ReadAllTextAsync_ShouldReturnFileContent()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "test.txt");
        var content = "Hello, Async!";
        File.WriteAllText(filePath, content);

        // Act
        var result = await _fileSystem.ReadAllTextAsync(filePath);

        // Assert
        result.Should().Be(content);
    }

    [Fact]
    public void WriteAllText_ShouldCreateFile()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "newfile.txt");
        var content = "New content";

        // Act
        _fileSystem.WriteAllText(filePath, content);

        // Assert
        _fileSystem.FileExists(filePath).Should().BeTrue();
        File.ReadAllText(filePath).Should().Be(content);
    }

    [Fact]
    public async Task WriteAllTextAsync_ShouldCreateFile()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "newfile.txt");
        var content = "New async content";

        // Act
        await _fileSystem.WriteAllTextAsync(filePath, content);

        // Assert
        _fileSystem.FileExists(filePath).Should().BeTrue();
        File.ReadAllText(filePath).Should().Be(content);
    }

    [Fact]
    public void CreateDirectory_ShouldCreateDirectory()
    {
        // Arrange
        var newDir = Path.Combine(_tempDir, "newdir");

        // Act
        _fileSystem.CreateDirectory(newDir);

        // Assert
        _fileSystem.DirectoryExists(newDir).Should().BeTrue();
    }

    [Fact]
    public void DirectoryExists_WithExistingDirectory_ShouldReturnTrue()
    {
        // Arrange & Act
        var result = _fileSystem.DirectoryExists(_tempDir);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void DirectoryExists_WithNonExistingDirectory_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentDir = Path.Combine(_tempDir, "nonexistent");

        // Act
        var result = _fileSystem.DirectoryExists(nonExistentDir);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetFiles_ShouldReturnFilesInDirectory()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_tempDir, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(_tempDir, "file2.txt"), "content2");
        File.WriteAllText(Path.Combine(_tempDir, "file3.log"), "content3");

        // Act
        var txtFiles = _fileSystem.GetFiles(_tempDir, "*.txt");

        // Assert
        txtFiles.Should().HaveCount(2);
    }

    [Fact]
    public void DeleteFile_ShouldRemoveFile()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "todelete.txt");
        File.WriteAllText(filePath, "content");

        // Act
        _fileSystem.DeleteFile(filePath);

        // Assert
        _fileSystem.FileExists(filePath).Should().BeFalse();
    }

    [Fact]
    public void CombinePaths_ShouldCombinePathsCorrectly()
    {
        // Act
        var result = _fileSystem.CombinePaths(_tempDir, "subdir", "file.txt");

        // Assert
        result.Should().Be(Path.Combine(_tempDir, "subdir", "file.txt"));
    }

    [Fact]
    public void GetSpecialFolder_ShouldReturnCorrectPath()
    {
        // Act
        var appDataPath = _fileSystem.GetSpecialFolder(Environment.SpecialFolder.ApplicationData);

        // Assert
        appDataPath.Should().NotBeEmpty();
        Directory.Exists(appDataPath).Should().BeTrue();
    }

    [Fact]
    public void GetDirectoryName_ShouldReturnDirectoryName()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "subdir", "file.txt");

        // Act
        var dirName = _fileSystem.GetDirectoryName(filePath);

        // Assert
        dirName.Should().Be(Path.Combine(_tempDir, "subdir"));
    }

    [Fact]
    public void GetDirectoryName_WithFilePath_ShouldReturnParentDirectory()
    {
        // Arrange
        var filePath = @"C:\Test\Directory\file.txt";

        // Act
        var dirName = _fileSystem.GetDirectoryName(filePath);

        // Assert
        dirName.Should().Be(@"C:\Test\Directory");
    }
}
