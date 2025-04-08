using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using EFT.UI.Matchmaker;
using HarmonyLib;
using PlayerIcons;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Transmog
{
	public class MatchMakerSideSelectionScreenPatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			var method = AccessTools.Method(typeof(MatchMakerSideSelectionScreen), "Show", new []
			{
				typeof(ISession), typeof(RaidSettings), typeof(IHealthController), typeof(InventoryController)
			});
			if (method != null)
				Plugin.LogInfo("Found MatchMakerSideSelectionScreen Show method.");
			else
				Plugin.LogInfo("Unable to find MatchMakerSideSelectionScreen Show method.");
			return method;
		}

		public static Profile TryMarkScavProfile(Profile scavProfile)
		{
			if (Plugin.DisableScavTransmogInLobby.Value)
			{
				var clone = scavProfile.Clone();
				TacticalClothingViewPatch.MarkProfile(clone);
				return clone;
			}
			return scavProfile;
		}

		[PatchTranspiler]
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var hasFound = false;
			foreach (var codeInstruction in instructions)
			{
				if (!hasFound && codeInstruction.opcode == OpCodes.Stfld &&
				    codeInstruction.operand is FieldInfo field &&
				    field.Name == "profile_1")
				{
					hasFound = true;
					yield return new CodeInstruction(OpCodes.Call,
						AccessTools.Method(typeof(MatchMakerSideSelectionScreenPatch), nameof(TryMarkScavProfile)));
				}
				yield return codeInstruction;
			}
		}
	}
}