using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using SPT.Reflection.Patching;
using EFT;
using EFT.UI;
using HarmonyLib;

namespace Transmog
{
	/// <summary>
	/// This patch is unimportant. It's used to make sure transmog won't wrongly show in clothing shopping view,
	/// where player's equipments should be hidden.
	/// </summary>
	public class TacticalClothingViewPatch : ModulePatch
	{
		public const string MAGIC_ID_SKIP_TRANSMOG = "___SKIP_TF_TRANSMOG___";

		protected override MethodBase GetTargetMethod()
		{
			var method = AccessTools.Method(typeof(TacticalClothingView), "Show");
			if (method != null)
				Plugin.LogInfo("Found TacticalClothingView Show method.");
			else
				Plugin.LogInfo("Unable to find TacticalClothingView Show method.");
			return method;
		}

		[PatchTranspiler]
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var codeInstruction in instructions)
			{
				if (codeInstruction.opcode == OpCodes.Stfld &&
				    codeInstruction.operand is FieldInfo fieldInfo &&
				    fieldInfo.Name == "profile_1")
				{
					yield return new CodeInstruction(OpCodes.Dup);
					yield return new CodeInstruction(OpCodes.Call,
						AccessTools.Method(typeof(TacticalClothingViewPatch), nameof(MarkProfile)));
				}
				yield return codeInstruction;
			}
		}

		public static void MarkProfile(Profile profile)
		{
			var oldInfo = profile.Info;
			profile.Info = new InfoClass()
			{
				Nickname = MAGIC_ID_SKIP_TRANSMOG,
				MainProfileNickname = MAGIC_ID_SKIP_TRANSMOG,
				Side = oldInfo.Side,
				MemberCategory = oldInfo.MemberCategory,
				Experience = oldInfo.Experience
			};
		}

		/// <summary>
		/// We use a special inherited class of GClass1767 to mark out which rendering request is from TacticalClothingView.
		///
		/// Note: Its name was GClass1755 in SPT 3.8
		/// </summary>
		public class MarkedCustomization : GClass1767
		{
			public MarkedCustomization()
			{
			}

			public MarkedCustomization(Dictionary<EBodyModelPart, string> original)
				: base(original)
			{
			}
		}
	}
}