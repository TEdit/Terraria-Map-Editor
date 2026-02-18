using System;
using System.Collections.Generic;
using System.Threading;

namespace TEdit.Render;

/// <summary>
/// Status of a texture in the loading pipeline.
/// </summary>
public enum TextureStatus
{
    NotLoaded,
    Pending,
    Loading,
    Loaded,
    Failed
}

/// <summary>
/// Type of texture being loaded.
/// </summary>
public enum TextureType
{
    Tile,
    Wall,
    Npc,
    Item
}

/// <summary>
/// Represents a request to load a texture.
/// </summary>
public class TextureLoadRequest : IComparable<TextureLoadRequest>
{
    public TextureType Type { get; set; }
    public int Id { get; set; }
    public int Priority { get; set; } // 0 = highest (items, npcs), 1 = normal (tiles, walls)

    public int CompareTo(TextureLoadRequest other)
    {
        if (other == null) return -1;

        int priorityCompare = Priority.CompareTo(other.Priority);
        if (priorityCompare != 0) return priorityCompare;

        int typeCompare = Type.CompareTo(other.Type);
        if (typeCompare != 0) return typeCompare;

        return Id.CompareTo(other.Id);
    }

    public override int GetHashCode() => HashCode.Combine(Type, Id);

    public override bool Equals(object obj) =>
        obj is TextureLoadRequest req && req.Type == Type && req.Id == Id;
}

/// <summary>
/// Thread-safe state management for async texture loading.
/// Tracks loading progress and manages a priority queue of pending loads.
/// </summary>
public class TextureLoadingState
{
    private readonly object _lock = new();

    // Track status of each texture
    private readonly Dictionary<(TextureType, int), TextureStatus> _textureStatus = new();

    // Priority queue using SortedSet
    private readonly SortedSet<TextureLoadRequest> _pendingQueue = new();

    // HashSet for quick lookup to avoid duplicates
    private readonly HashSet<(TextureType, int)> _pendingSet = new();

    private int _totalTextures;
    private int _loadedCount;

    /// <summary>
    /// Total number of textures to be loaded.
    /// </summary>
    public int TotalTextures
    {
        get { lock (_lock) return _totalTextures; }
    }

    /// <summary>
    /// Number of textures that have been loaded.
    /// </summary>
    public int LoadedCount
    {
        get { lock (_lock) return _loadedCount; }
    }

    /// <summary>
    /// Whether all textures have been loaded.
    /// </summary>
    public bool IsComplete
    {
        get { lock (_lock) return _loadedCount >= _totalTextures && _totalTextures > 0; }
    }

    /// <summary>
    /// Loading progress as a percentage (0-100).
    /// </summary>
    public double ProgressPercent
    {
        get
        {
            lock (_lock)
            {
                if (_totalTextures == 0) return 0;
                return (double)_loadedCount / _totalTextures * 100.0;
            }
        }
    }

    /// <summary>
    /// Initialize the loading state for a new loading session.
    /// </summary>
    public void Initialize(int tileCount, int wallCount)
    {
        lock (_lock)
        {
            _totalTextures = tileCount + wallCount;
            _loadedCount = 0;
            _textureStatus.Clear();
            _pendingQueue.Clear();
            _pendingSet.Clear();
        }
    }

    /// <summary>
    /// Add additional textures to the total count (e.g. item previews discovered after initial init).
    /// </summary>
    public void AddToTotal(int count)
    {
        lock (_lock)
        {
            _totalTextures += count;
        }
    }

    /// <summary>
    /// Queue a texture for loading.
    /// </summary>
    /// <param name="type">Type of texture</param>
    /// <param name="id">Texture ID</param>
    /// <param name="priority">Loading priority (0 = highest)</param>
    public void QueueLoad(TextureType type, int id, int priority)
    {
        lock (_lock)
        {
            var key = (type, id);
            if (_textureStatus.ContainsKey(key)) return; // Already queued or loaded

            _textureStatus[key] = TextureStatus.Pending;

            if (_pendingSet.Add(key))
            {
                _pendingQueue.Add(new TextureLoadRequest
                {
                    Type = type,
                    Id = id,
                    Priority = priority
                });
            }
        }
    }

    /// <summary>
    /// Get the next batch of textures to load.
    /// </summary>
    /// <param name="batchSize">Maximum number of textures to retrieve</param>
    /// <returns>List of texture load requests</returns>
    public List<TextureLoadRequest> GetNextBatch(int batchSize)
    {
        var batch = new List<TextureLoadRequest>();

        lock (_lock)
        {
            var toRemove = new List<TextureLoadRequest>();

            foreach (var request in _pendingQueue)
            {
                if (batch.Count >= batchSize) break;

                var key = (request.Type, request.Id);
                _textureStatus[key] = TextureStatus.Loading;
                batch.Add(request);
                toRemove.Add(request);
            }

            foreach (var request in toRemove)
            {
                _pendingQueue.Remove(request);
                _pendingSet.Remove((request.Type, request.Id));
            }
        }

        return batch;
    }

    /// <summary>
    /// Mark a texture as loaded or failed.
    /// </summary>
    public void MarkLoaded(TextureType type, int id, bool success)
    {
        lock (_lock)
        {
            var key = (type, id);
            _textureStatus[key] = success ? TextureStatus.Loaded : TextureStatus.Failed;
            Interlocked.Increment(ref _loadedCount);
        }
    }

    /// <summary>
    /// Get the current status of a texture.
    /// </summary>
    public TextureStatus GetStatus(TextureType type, int id)
    {
        lock (_lock)
        {
            return _textureStatus.TryGetValue((type, id), out var status)
                ? status
                : TextureStatus.NotLoaded;
        }
    }

    /// <summary>
    /// Whether there are pending loads in the queue.
    /// </summary>
    public bool HasPendingLoads()
    {
        lock (_lock)
        {
            return _pendingQueue.Count > 0;
        }
    }

    /// <summary>
    /// Get the count of pending loads.
    /// </summary>
    public int PendingCount
    {
        get { lock (_lock) return _pendingQueue.Count; }
    }
}
