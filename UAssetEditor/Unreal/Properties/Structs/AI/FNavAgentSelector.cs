using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UAssetEditor.Classes;

namespace UAssetEditor.Unreal.Properties.Structs.AI;

// Not supposed to be in here but idk you can move it if you want.

[StructLayout(LayoutKind.Sequential)]
public readonly struct FNavAgentSelector : IUnrealType
{
    public readonly uint PackedBits;
}
