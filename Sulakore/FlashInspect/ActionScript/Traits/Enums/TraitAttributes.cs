using System;

namespace FlashInspect.ActionScript.Traits
{
    [Flags]
    public enum TraitAttributes
    {
        None = 0,
        Final = 1,
        Override = 2,
        Metadata = 4
    }
}