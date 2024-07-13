using HarmonyLib;
using Kingmaker.UI.MVVM._VM.Party;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MorePartyViewSlots {
    [HarmonyPatch(typeof(PartyVM))]
    public static class PartyVM_Patches {

        public static int SupportedSlots = 8;

        internal const int WantedSlots = 8;

        [HarmonyPatch(nameof(PartyVM.UpdateStartValue))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpdateStartValue(IEnumerable<CodeInstruction> instructions) {
            return ConvertConstants(instructions, WantedSlots);
        }

        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> _ctor(IEnumerable<CodeInstruction> instructions) {
            return ConvertConstants(instructions, WantedSlots);
        }

        private static OpCode[] LdConstants = {
            OpCodes.Ldc_I4_0,
            OpCodes.Ldc_I4_1,
            OpCodes.Ldc_I4_2,
            OpCodes.Ldc_I4_3,
            OpCodes.Ldc_I4_4,
            OpCodes.Ldc_I4_5,
            OpCodes.Ldc_I4_6,
            OpCodes.Ldc_I4_7,
            OpCodes.Ldc_I4_8,
        };

        private static IEnumerable<CodeInstruction> ConvertConstants(IEnumerable<CodeInstruction> instructions, int to) {
            Func<CodeInstruction> makeReplacement;
            if (to <= 8)
                makeReplacement = () => new CodeInstruction(LdConstants[to]);
            else
                makeReplacement = () => new CodeInstruction(OpCodes.Ldc_I4_S, to);

            foreach (var ins in instructions) {
                if (ins.opcode == OpCodes.Ldc_I4_6)
                    yield return makeReplacement();
                else
                    yield return ins;
            }
        }
    }
}
