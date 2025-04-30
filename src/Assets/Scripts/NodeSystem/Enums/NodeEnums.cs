using System;

namespace NodeSystem
{
    // Port polaritesini tanımlayan enum
    public enum PolarityType
    {
        Input,
        Output,
        Bidirectional
    }
    
    // Event tipi enum'u
    public enum EventTypeEnum
    {
        OnActivated,
        OnStarted, 
        OnCompleted,
        OnDeactivated
    }
} 