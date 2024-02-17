using System;
using System.Collections.Generic;
using System.Text;

namespace TEdit.Editor.Tools;

public interface ITool
{
    bool IsActive { get; set; }
    string Name { get; }
    string Title { get; }
    void Start(int x, int y);
    void Move(int x, int y);
    void End(int x, int y);
    void Commit();
    void Cancel();
}
