using System.Reflection.Emit;
using HarmonyLib;
using System.Collections.Generic;
namespace RoundsVC.Patches
{
    [HarmonyPatch(typeof(ArtHandler), "Update")]
    static class ArtHandlerPatchUpdate
    {
        // replace arthandler nextart key from LeftShift to RightShift
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_I4 && (int)instruction.operand == 304)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4, 303);
                }
                else
                {
                    yield return instruction;
                }
            }
        }

    }
}
