using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TEdit.ViewModel;

namespace TEdit.Editor.Plugins;

public class ReplayRecorder
{
    private DateTime _startTime;
    private DateTime _lastTime;

    public ReplayFile Recording { get; private set; }
    public bool IsRecording { get; private set; }

    public void Start()
    {
        _startTime = DateTime.UtcNow;
        _lastTime = DateTime.UtcNow;
        IsRecording = true;

        Recording = new ReplayFile { StartTime = _startTime };
    }

    public void Poll()
    {
        if (!IsRecording) return;

        string undoDir = Path.GetDirectoryName(
            ViewModelLocator.WorldViewModel.UndoManager.GetUndoFileName());

        var pending = new List<(string Path, DateTime WriteTime)>();
        foreach (string file in Directory.GetFiles(undoDir, "undo_temp_*"))
        {
            DateTime writeTime = File.GetLastWriteTimeUtc(file);
            if (writeTime > _lastTime) pending.Add((file, writeTime));
        }

        pending.Sort((a, b) => a.WriteTime.CompareTo(b.WriteTime));
        foreach (var (path, writeTime) in pending)
        {
            byte[] data;
            try { data = File.ReadAllBytes(path); }
            catch (IOException) { break; }

            _lastTime = writeTime;
            Recording.Frames.Add(new ReplayFrame
            {
                Index = Recording.Frames.Count,
                Time = (long)(writeTime - _startTime).TotalMilliseconds,
                Data = data,
            });
        }
    }

    public void Stop()
    {
        IsRecording = false;
        Recording.TotalTime = (long)(DateTime.UtcNow - _startTime).TotalMilliseconds;
        Recording.BaselineWorld = ViewModelLocator.WorldViewModel.CurrentWorld;
    }
}
