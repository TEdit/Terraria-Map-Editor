using System;
using System.Collections.Generic;
using System.IO;
using Shouldly;
using TEdit.Utility;
using Xunit;

namespace TEdit.Tests.Utility;

public sealed class FileMaintenanceTests : IDisposable
{
    private readonly string _tempDirectory =
        Path.Combine(Path.GetTempPath(), "TEditFileMaintenanceTests", Guid.NewGuid().ToString("N"));

    public FileMaintenanceTests() => Directory.CreateDirectory(_tempDirectory);

    public void Dispose()
    {
        try
        {
            Directory.Delete(_tempDirectory, true);
        }
        catch
        {
        }
    }

    [Fact]
    public void ClearDirectoryContents_DeletesNestedFilesAndPreservesExclusions()
    {
        string nestedDirectory = Path.Combine(_tempDirectory, "nested");
        Directory.CreateDirectory(nestedDirectory);
        string firstFile = Path.Combine(_tempDirectory, "first.cache");
        string secondFile = Path.Combine(nestedDirectory, "second.cache");
        string excludedFile = Path.Combine(_tempDirectory, "alive.txt");
        File.WriteAllText(firstFile, "1234");
        File.WriteAllText(secondFile, "56789");
        File.WriteAllText(excludedFile, "keep");

        var result = FileMaintenance.ClearDirectoryContents(
            _tempDirectory,
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { excludedFile });

        result.DeletedFileCount.ShouldBe(2);
        result.FreedBytes.ShouldBe(9);
        result.FailedFileCount.ShouldBe(0);
        File.Exists(firstFile).ShouldBeFalse();
        File.Exists(secondFile).ShouldBeFalse();
        File.Exists(excludedFile).ShouldBeTrue();
        Directory.Exists(nestedDirectory).ShouldBeFalse();
    }
}
