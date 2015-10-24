using System;

namespace FlashInspect.ActionScript
{
    [Flags]
    public enum ClassFlags
    {
        None = 0,
        Sealed = 1,
        Final = 2,
        Interface = 4,
        ProtectedNamespace = 8,
    }
}