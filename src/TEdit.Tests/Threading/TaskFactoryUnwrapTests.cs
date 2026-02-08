using System.Threading.Tasks;
using Xunit;

namespace TEdit.Tests.Threading;

/// <summary>
/// Tests demonstrating the Task.Factory.StartNew async anti-pattern
/// and the .Unwrap() fix applied to SaveWorldThreaded.
///
/// When Task.Factory.StartNew receives an async lambda (Func&lt;Task&gt;),
/// it returns Task&lt;Task&gt;. Without .Unwrap(), ContinueWith attaches
/// to the outer task, which completes at the first await — not when
/// the async work finishes.
/// </summary>
public class TaskFactoryUnwrapTests
{
    [Fact]
    public async Task WithoutUnwrap_ContinuationRunsBeforeAsyncLambdaCompletes()
    {
        // Arrange: simulate the bug where ContinueWith fires too early
        int executionOrder = 0;
        int lambdaCompleted = 0;
        int continuationStarted = 0;

        // Act: Task.Factory.StartNew with async lambda WITHOUT Unwrap
        var outerTask = Task.Factory.StartNew(async () =>
        {
            await Task.Delay(200);
            Interlocked.Exchange(ref lambdaCompleted, Interlocked.Increment(ref executionOrder));
        });

        // ContinueWith on the outer Task<Task> — fires when the lambda yields
        var continuation = outerTask.ContinueWith(t =>
        {
            Interlocked.Exchange(ref continuationStarted, Interlocked.Increment(ref executionOrder));
        });

        await continuation;

        // Assert: continuation ran first because outer task completed at the await
        // The inner async lambda hasn't finished yet
        Assert.True(
            continuationStarted > 0 && (lambdaCompleted == 0 || continuationStarted < lambdaCompleted),
            "BUG: Without Unwrap, continuation should fire before async lambda completes");

        // Wait for the inner task to actually finish
        await outerTask.Unwrap();
    }

    [Fact]
    public async Task WithUnwrap_ContinuationRunsAfterAsyncLambdaCompletes()
    {
        // Arrange: the fix — .Unwrap() ensures ContinueWith waits for the inner task
        int executionOrder = 0;
        int lambdaCompleted = 0;
        int continuationStarted = 0;

        // Act: Task.Factory.StartNew with async lambda WITH Unwrap
        var task = Task.Factory.StartNew(async () =>
        {
            await Task.Delay(200);
            Interlocked.Exchange(ref lambdaCompleted, Interlocked.Increment(ref executionOrder));
        }).Unwrap().ContinueWith(t =>
        {
            Interlocked.Exchange(ref continuationStarted, Interlocked.Increment(ref executionOrder));
        });

        await task;

        // Assert: with Unwrap, continuation runs AFTER the async lambda completes
        Assert.True(lambdaCompleted > 0, "Async lambda should have completed");
        Assert.True(continuationStarted > 0, "Continuation should have run");
        Assert.True(
            lambdaCompleted < continuationStarted,
            "FIX: With Unwrap, continuation must run after async lambda completes");
    }

    [Fact]
    public async Task WithUnwrap_FileWriteThenMove_SimulatesSavePattern()
    {
        // Arrange: simulate the SaveWorldThreaded pattern where a file is
        // written in the async lambda and moved in the continuation
        var tempDir = Path.Combine(Path.GetTempPath(), $"tedit_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        var savePath = Path.Combine(tempDir, "test.autosave.tmp");
        var finalPath = Path.Combine(tempDir, "test.autosave");
        bool continuationSucceeded = false;

        try
        {
            // Act: mimic SaveWorldThreaded with .Unwrap()
            var task = Task.Factory.StartNew(async () =>
            {
                // Simulate World.SaveAsync: write to temp, copy to filename
                var tempFile = savePath + ".tmp";
                await Task.Run(() =>
                {
                    File.WriteAllText(tempFile, "world data");
                    File.Copy(tempFile, savePath, true);
                    File.Delete(tempFile);
                });
            }).Unwrap().ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    // Simulate the autosave rename in ContinueWith
                    if (File.Exists(finalPath))
                        File.Delete(finalPath);
                    File.Move(savePath, finalPath);
                    continuationSucceeded = true;
                }
            });

            await task;

            // Assert: file was written and moved successfully
            Assert.True(continuationSucceeded, "Continuation should have succeeded");
            Assert.False(File.Exists(savePath), "Temp file should have been moved");
            Assert.True(File.Exists(finalPath), "Final autosave file should exist");
            Assert.Equal("world data", File.ReadAllText(finalPath));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task WithoutUnwrap_FileWriteThenMove_FailsWithFileNotFound()
    {
        // Arrange: demonstrate the exact bug — without Unwrap, the continuation
        // tries to move a file before SaveAsync has written it
        var tempDir = Path.Combine(Path.GetTempPath(), $"tedit_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        var savePath = Path.Combine(tempDir, "test.autosave.tmp");
        Exception? caughtException = null;

        try
        {
            // Act: mimic SaveWorldThreaded WITHOUT .Unwrap() — the bug
            var outerTask = Task.Factory.StartNew(async () =>
            {
                var tempFile = savePath + ".tmp";
                await Task.Run(() =>
                {
                    // Simulate a slow write
                    Thread.Sleep(200);
                    File.WriteAllText(tempFile, "world data");
                    File.Copy(tempFile, savePath, true);
                    File.Delete(tempFile);
                });
            });

            // ContinueWith on outer Task<Task> — fires immediately
            var continuation = outerTask.ContinueWith(t =>
            {
                try
                {
                    var finalPath = Path.Combine(tempDir, "test.autosave");
                    File.Move(savePath, finalPath); // This should fail!
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
            });

            await continuation;

            // Assert: FileNotFoundException because the file hasn't been written yet
            Assert.NotNull(caughtException);
            Assert.True(
                caughtException is FileNotFoundException or DirectoryNotFoundException,
                $"Expected FileNotFoundException, got: {caughtException?.GetType().Name}: {caughtException?.Message}");

            // Cleanup: wait for the inner task
            await outerTask.Unwrap();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
