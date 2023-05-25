using System;

namespace Crafting
{
    [Flags]
    public enum MachineType
    {
        Fabricator = 0x0001,
        PocketFabricator = 0x0002,
        Smelter = 0x0004,
        Assembler = 0x0008,
        CookingPot = 0x0010
    }
}