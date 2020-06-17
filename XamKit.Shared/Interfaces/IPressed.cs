using System;

namespace XamKit
{
    public interface IIsPressed
    {
        bool IsPressed { get; }
    
        event EventHandler<bool> IsPressedChanged;
    }
}
