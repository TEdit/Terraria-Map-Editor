using System;
using System.ComponentModel;

namespace TEdit.Tools
{
    public interface IOrderMetadata
    {
        [DefaultValue(Int32.MaxValue)]
        int Order { get; }
    }
}