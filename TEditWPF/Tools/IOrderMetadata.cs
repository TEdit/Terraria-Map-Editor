using System;
using System.ComponentModel;

namespace TEditWPF.Tools
{
    public interface IOrderMetadata
    {
        [DefaultValue(Int32.MaxValue)]
        int Order { get; }
    }
}