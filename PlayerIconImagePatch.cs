using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using SPT.Reflection.Patching;
using EFT;
using HarmonyLib;
using PlayerIcons;
using EFT.InventoryLogic;

namespace Transmog
{
	public class PlayerIconImagePatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			var method = AccessTools.Method(typeof(PlayerIconImage), "SetPresetIcon", new [] { typeof(Profile) });
			if (method != null)
				Plugin.LogInfo("Found PlayerIconImage SetPresetIcon method.");
			else
				Plugin.LogInfo("Unable to find PlayerIconImage SetPresetIcon method.");
			return method;
		}

		public static InventoryEquipment CloneAndModifyEquipmentClass(InventoryEquipment originEquipmentClass)
		{
			return Plugin.CloneAndModifyEquipmentClass(originEquipmentClass, false);
		}

		[PatchTranspiler]
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var codeInstruction in instructions)
			{
				if (codeInstruction.opcode == OpCodes.Call &&
				    codeInstruction.operand is MethodBase method &&
				    method.Name == "CloneVisibleItem")
				{
					yield return new CodeInstruction(OpCodes.Call,
						AccessTools.Method(typeof(PlayerIconImagePatch), nameof(CloneAndModifyEquipmentClass)));
					continue;
				}
				yield return codeInstruction;
			}
		}
	}
}