using TEdit.Scripting.Engine;

namespace TEdit.Scripting.Api;

public class FinderApi
{
    private readonly ScriptExecutionContext _context;
    private int _count;
    private const int MaxResults = 1000;

    public FinderApi(ScriptExecutionContext context)
    {
        _context = context;
    }

    public void Clear()
    {
        _count = 0;
        _context.OnFindClear?.Invoke();
    }

    public bool AddResult(string name, int x, int y, string resultType, string? extraInfo = null)
    {
        if (_count >= MaxResults)
        {
            if (_count == MaxResults)
                _context.OnWarn?.Invoke($"Find results capped at {MaxResults}");
            _count++;
            return false;
        }

        _context.OnFindResult?.Invoke(name, x, y, resultType, extraInfo);
        _count++;
        return true;
    }

    public void Navigate(int index)
    {
        _context.OnFindNavigate?.Invoke(index);
    }

    public void NavigateFirst()
    {
        _context.OnFindNavigate?.Invoke(0);
    }
}
