using System.Collections.Generic;
using System.Linq;
using EFT.InventoryLogic;
using EFT.UI;
using IcyClawz.CustomInteractions;

namespace Transmog
{
	public class CustomInteractionsProvider : IItemCustomInteractionsProvider, ICustomInteractionsProvider
	{
		internal static StaticIcons StaticIcons
		{
			get
			{
				return EFTHardSettings.Instance.StaticIcons;
			}
		}

		public enum SlotType
		{
			Armor,
			Vest,
			Backpack,
			Eyewear,
			FaceCover,
			Headwear,
			Earpiece,
			ArmBand,
			Count
		}

		private static readonly string[] BASE_TEMPLATE_ID = {
			"5448e54d4bdc2dcc718b4568",
			"5448e5284bdc2dcb718b4567",
			"5448e53e4bdc2d60728b4567",
			"5448e5724bdc2ddf718b4568",
			"5a341c4686f77469e155819e",
			"5a341c4086f77401f2541505",
			"5645bcb74bdc2ded0b8b4578",
			"5b3f15d486f77432d0509248",
		};

		public IEnumerable<CustomInteraction> GetCustomInteractions(ItemUiContext uiContext, EItemViewType viewType, Item item)
		{
			/*if (viewType != EItemViewType.Inventory)
			{
				yield break;
			}*/

			var available = BASE_TEMPLATE_ID.Select(s => CheckTemplateParent(item.Template, s)).ToArray();

			if (available.Any(b => b == true))
			{
				CustomInteraction customInteraction = new CustomInteraction();
				customInteraction.Caption = () => "Transmog";
				customInteraction.Icon = () => StaticIcons.GetAttributeIcon(EItemAttributeId.ArmorType);
				customInteraction.SubMenu = () => new SubMenu(uiContext, item, available);
				yield return customInteraction;
			}
		}

		private static bool CheckTemplateParent(ItemTemplate template, string id)
		{
			if (template._id == id)
				return true;
			if (template.Parent != null)
				return CheckTemplateParent(template.Parent, id);
			return false;
		}

		private class SubMenu : CustomSubInteractions
		{
			public SubMenu(ItemUiContext uiContext, Item item, bool[] available) : base(uiContext)
			{
				for (var i = 0; i < available.Length; i++)
				{
					if (available[i])
					{
						var i1 = i;
						var showScav = Plugin.ShowScavInMenu.Value;
						var name = ((SlotType)i).ToString();
						var pmcCallname = showScav ? "PMC " : "";
						Add(new CustomInteraction()
						{
							Caption = () => $"Set as {pmcCallname}{name}",
							Action = () =>
							{
								NotificationManagerClass.DisplayMessageNotification(
									$"Set {item.LocalizedShortName()} as Your PMC's visual {name.ToLowerInvariant()}.");
								Plugin.PmcEquipments[i1].Set(item);
							}
						});
						if (showScav)
						{
							Add(new CustomInteraction()
							{
								Caption = () => $"Set as Scav {name}",
								Action = () =>
								{
									NotificationManagerClass.DisplayMessageNotification(
										$"Set {item.LocalizedShortName()} as Your Scav's visual {name.ToLowerInvariant()}.");
									Plugin.ScavEquipments[i1].Set(item);
								}
							});
						}
						Add(new CustomInteraction()
						{
							Caption = () => $"Hide {pmcCallname}{name}",
							Action = () =>
							{
								NotificationManagerClass.DisplayMessageNotification(
									$"Your PMC's {name.ToLowerInvariant()} will be visually hidden.");
								Plugin.PmcEquipments[i1].Hide();
							}
						});
						if (showScav)
						{
							Add(new CustomInteraction()
							{
								Caption = () => $"Hide Scav {name}",
								Action = () =>
								{
									NotificationManagerClass.DisplayMessageNotification(
										$"Your Scav's {name.ToLowerInvariant()} will be visually hidden.");
									Plugin.ScavEquipments[i1].Hide();
								}
							});
						}
						if (Plugin.PmcEquipments[i1].HasEffect())
						{
							Add(new CustomInteraction()
							{
								Caption = () => $"Reset {pmcCallname}{name}",
								Action = () =>
								{
									NotificationManagerClass.DisplayMessageNotification(
										$"Your PMC's {name.ToLowerInvariant()} will be shown normally.");
									Plugin.PmcEquipments[i1].Reset();
								}
							});
						}
						if (showScav && Plugin.ScavEquipments[i1].HasEffect())
						{
							Add(new CustomInteraction()
							{
								Caption = () => $"Reset Scav {name}",
								Action = () =>
								{
									NotificationManagerClass.DisplayMessageNotification(
										$"Your Scav's {name.ToLowerInvariant()} will be shown normally.");
									Plugin.ScavEquipments[i1].Reset();
								}
							});
						}
					}
				}
			}
		}
	}
}