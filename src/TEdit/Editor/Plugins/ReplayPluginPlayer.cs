using System;
using System.Collections.Generic;
using TEdit.Editor.Undo;
using TEdit.Terraria;

namespace TEdit.Editor.Plugins;

public enum PlaybackMode { Speed, Delay }

public class ReplayPlayer
{
    private readonly World _world;
    private readonly long _totalTime;
    private readonly List<ReplayFrame> _undoFrames;
    private readonly List<ReplayFrame> _redoFrames = [];

    private bool _isPlaying;
    private int _currentIndex;

    private double _playTime;
    private DateTime _lastTime;

    private PlaybackMode _mode;
    private double _speed = 1.0;
    private int _stepDelayTime = 250;

    public int CurrentIndex => _currentIndex;
    public int FrameCount => _redoFrames.Count;
    public bool IsPlaying => _isPlaying;
    public double CurrentTime => _playTime;
    public long TotalTime => _totalTime;

    public void SetMode(PlaybackMode mode) => _mode = mode;
    public void SetSpeed(double speed) => _speed = speed;
    public void SetDelay(int ms) => _stepDelayTime = ms;

    public ReplayPlayer(ReplayFile file)
    {
        _world = file.BaselineWorld;
        _undoFrames = file.Frames;
        _totalTime = file.TotalTime;

        for (int i = file.Frames.Count - 1; i >= 0; i--)
        {
            var undoFrame = file.Frames[i];
            var preTiles = undoFrame.ReadTiles(_world);

            var redoTiles = preTiles.ConvertAll(ut =>
                new UndoTile(_world.Tiles[ut.Location.X, ut.Location.Y], ut.Location));
            var redoFrame = new ReplayFrame { Time = undoFrame.Time };

            redoFrame.WriteTiles(redoTiles, _world);
            _redoFrames.Add(redoFrame);

            preTiles.ForEach(ut => _world.Tiles[ut.Location.X, ut.Location.Y] = ut.Tile);
        }

        _redoFrames.Reverse();
    }

    public void StepForward()
    {
        if (_currentIndex >= _redoFrames.Count) return;
        ApplyFrame(_redoFrames[_currentIndex++]);
        _playTime = _currentIndex > 0 ? _redoFrames[_currentIndex - 1].Time : 0;
    }

    public void StepBackward()
    {
        if (_currentIndex <= 0) return;
        ApplyFrame(_undoFrames[--_currentIndex]);
        _playTime = _currentIndex > 0 ? _redoFrames[_currentIndex - 1].Time : 0;
    }

    public void SeekByTime(double time)
    {
        int lo = 0, hi = _redoFrames.Count - 1;
        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            if (_redoFrames[mid].Time <= time)
                lo = mid + 1;
            else
                hi = mid - 1;
        }
        Seek(hi + 1);
        _playTime = Math.Min(time, TotalTime);
    }

    public void Seek(int targetIndex)
    {
        if (targetIndex < 0) targetIndex = 0;
        if (targetIndex > _redoFrames.Count) targetIndex = _redoFrames.Count;

        if (targetIndex > _currentIndex)
        {
            for (int i = _currentIndex; i < targetIndex; i++)
                ApplyFrame(_redoFrames[i]);
        }
        else if (targetIndex < _currentIndex)
        {
            for (int i = _currentIndex - 1; i >= targetIndex; i--)
                ApplyFrame(_undoFrames[i]);
        }

        _currentIndex = targetIndex;
        _playTime = targetIndex > 0 ? _redoFrames[targetIndex - 1].Time : 0;
    }

    public void Play()
    {
        _isPlaying = true;
        _lastTime = DateTime.UtcNow;
    }

    public void Pause()
    {
        _isPlaying = false;
    }

    public void OnTick()
    {
        if (!_isPlaying) return;
        
        double dt = (DateTime.UtcNow - _lastTime).TotalMilliseconds;
        if (_mode == PlaybackMode.Speed)
        {
            _playTime += dt * _speed;
            _lastTime = DateTime.UtcNow;

            if (_playTime >= _totalTime)
            {
                _playTime = _totalTime;
                _isPlaying = false;
            }

            while (_currentIndex < _redoFrames.Count &&
                   _playTime >= _redoFrames[_currentIndex].Time)
            {
                ApplyFrame(_redoFrames[_currentIndex++]);
            }
        }
        else if (_mode == PlaybackMode.Delay)
        {
            if (_currentIndex >= _redoFrames.Count)
            {
                _isPlaying = false;
                return;
            }

            if (dt >= _stepDelayTime)
            {
                ApplyFrame(_redoFrames[_currentIndex++]);
                _lastTime = DateTime.UtcNow;
                _playTime = _currentIndex > 0 ? _redoFrames[_currentIndex - 1].Time : 0;
            }
        }
    }

    private void ApplyFrame(ReplayFrame frame)
    {
        var tiles = frame.ReadTiles(_world);
        tiles.ForEach(ut => _world.Tiles[ut.Location.X, ut.Location.Y] = ut.Tile);
    }
}
