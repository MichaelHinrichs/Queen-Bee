using System;

namespace Nanook.QueenBee.Parser
{
    [Flags]
    public enum PakHeaderFlags : uint
    {
        Filename = 0x00000020
    }
}
