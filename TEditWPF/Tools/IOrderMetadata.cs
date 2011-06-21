namespace TEditWPF.Tools
{
    using System;
    using System.ComponentModel;

    public interface IOrderMetadata
    {
        [DefaultValue(Int32.MaxValue)]
        int Order { get; }
    }
}
