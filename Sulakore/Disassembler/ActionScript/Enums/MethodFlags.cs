using System;

namespace Sulakore.Disassembler.ActionScript
{
    [Flags]
    public enum MethodFlags
    {
        NeedArguments = 1,
        NeedActivation = 2,
        NeedRest = 4,
        HasOptional = 8,
        SetDXNS = 64,
        HasParamNames = 128
    }
}