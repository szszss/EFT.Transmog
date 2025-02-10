﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SPT.Reflection.Utils;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using IcyClawz.CustomInteractions;
using static Transmog.CustomInteractionsProvider;

namespace Transmog
{
	[BepInPlugin("net.hakugyokurou.Transmog", "Transmog", "1.1.0")]
	[BepInDependency("com.IcyClawz.CustomInteractions")]
	public class Plugin : BaseUnityPlugin
	{
		private static ManualLogSource logger;

		public static ConfigEntry<bool> ShowScavInMenu;
		public static ConfigEntry<bool> AffectIngameModel;
		public static CostumeItem[] PmcEquipments = new CostumeItem[(int) SlotType.Count];
		public static CostumeItem[] ScavEquipments = new CostumeItem[(int) SlotType.Count];

		private void Awake()
		{
			logger = Logger;
			logger.LogInfo("Loading: Transmog - V1.1.0");

			ShowScavInMenu = Config.Bind("Generals", "Show Scav in transmog menu", false, "Allow you to apply transmog to your scavs.");
			AffectIngameModel = Config.Bind("Generals", "Affect ingame player entity", true, 
				"When enabled, your player models in raid (such as shadow, third person model when you have Freecam mod) will also be affected by Transmog. " +
				"If disabled, only the player models in the tab menu and main menu will be affected. " +
				"If you toggle this option in a raid, it will not take effect until the next raid.");

			PmcEquipments[(int)SlotType.Armor] = new CostumeItem(Config.Bind("PMC Transmog", "Armor", ""));
			PmcEquipments[(int)SlotType.Vest] = new CostumeItem(Config.Bind("PMC Transmog", "Vesc", ""));
			PmcEquipments[(int)SlotType.Backpack] = new CostumeItem(Config.Bind("PMC Transmog", "Backpack", ""));
			PmcEquipments[(int)SlotType.Eyewear] = new CostumeItem(Config.Bind("PMC Transmog", "Eyewear", ""));
			PmcEquipments[(int)SlotType.FaceCover] = new CostumeItem(Config.Bind("PMC Transmog", "FaceCover", ""));
			PmcEquipments[(int)SlotType.Headwear] = new CostumeItem(Config.Bind("PMC Transmog", "Headwear", ""));
			PmcEquipments[(int)SlotType.Earpiece] = new CostumeItem(Config.Bind("PMC Transmog", "Earpiece", ""));
			PmcEquipments[(int)SlotType.ArmBand] = new CostumeItem(Config.Bind("PMC Transmog", "ArmBand", ""));

			ScavEquipments[(int)SlotType.Armor] = new CostumeItem(Config.Bind("Scav Transmog", "Armor", ""));
			ScavEquipments[(int)SlotType.Vest] = new CostumeItem(Config.Bind("Scav Transmog", "Vesc", ""));
			ScavEquipments[(int)SlotType.Backpack] = new CostumeItem(Config.Bind("Scav Transmog", "Backpack", ""));
			ScavEquipments[(int)SlotType.Eyewear] = new CostumeItem(Config.Bind("Scav Transmog", "Eyewear", ""));
			ScavEquipments[(int)SlotType.FaceCover] = new CostumeItem(Config.Bind("Scav Transmog", "FaceCover", ""));
			ScavEquipments[(int)SlotType.Headwear] = new CostumeItem(Config.Bind("Scav Transmog", "Headwear", ""));
			ScavEquipments[(int)SlotType.Earpiece] = new CostumeItem(Config.Bind("Scav Transmog", "Earpiece", ""));
			ScavEquipments[(int)SlotType.ArmBand] = new CostumeItem(Config.Bind("Scav Transmog", "ArmBand", ""));

			new PlayerModelViewPatch().Enable();
			new PlayerIconImagePatch().Enable();
			new TacticalClothingViewPatch().Enable();
			new PlayerBodyPatch().Enable();
			CustomInteractionsManager.Register(new CustomInteractionsProvider());
		}

		public static void LogMessage(string message)
		{
			logger.LogMessage(message);
		}

		public static void LogInfo(string message)
		{
			logger.LogInfo(message);
		}

		public static void LogDebug(string message)
		{
			logger.LogDebug(message);
		}

		public static void LogError(string message)
		{
			logger.LogError(message);
		}

		public static Profile GetPlayerPmcProfile()
		{
			return ClientAppUtils.GetMainApp().GetClientBackEndSession().Profile;
		}

		public static Profile GetPlayerScavProfile()
		{
			return ClientAppUtils.GetMainApp().GetClientBackEndSession().ProfileOfPet;
		}

		public static bool ToSlotType(EquipmentSlot equipmentSlot, out SlotType slotType)
		{
			switch (equipmentSlot)
			{
				case EquipmentSlot.Backpack: slotType = SlotType.Backpack; break;
				case EquipmentSlot.TacticalVest: slotType = SlotType.Vest; break;
				case EquipmentSlot.ArmorVest: slotType = SlotType.Armor; break;
				case EquipmentSlot.Eyewear: slotType = SlotType.Eyewear; break;
				case EquipmentSlot.FaceCover: slotType = SlotType.FaceCover; break;
				case EquipmentSlot.Headwear: slotType = SlotType.Headwear; break;
				case EquipmentSlot.Earpiece: slotType = SlotType.Earpiece; break;
				case EquipmentSlot.ArmBand: slotType = SlotType.ArmBand; break;
				default:
					slotType = SlotType.Armor;
					return false;
			}
			return true;
		}

		public static InventoryEquipment CloneAndModifyEquipmentClass(InventoryEquipment originEquipmentClass, bool isScav = false)
		{
			// originEquipmentClass.CloneVisibleItemWithSameId()
			var newEquipmentClass = GClass3105.smethod_2<InventoryEquipment>(originEquipmentClass, GClass3105.Class2259.Instance, true, false);

			var newEquipments = isScav ? ScavEquipments : PmcEquipments;
			for (int i = 0; i < (int) SlotType.Count; i++)
			{
				if (newEquipments[i].HasEffect())
				{
					EquipmentSlot slotIndex;
					switch ((SlotType) i)
					{
						case SlotType.Armor: slotIndex = EquipmentSlot.ArmorVest; break;
						case SlotType.Vest: slotIndex = EquipmentSlot.TacticalVest; break;
						case SlotType.Backpack: slotIndex = EquipmentSlot.Backpack; break;
						case SlotType.Eyewear: slotIndex = EquipmentSlot.Eyewear; break;
						case SlotType.FaceCover: slotIndex = EquipmentSlot.FaceCover; break;
						case SlotType.Headwear: slotIndex = EquipmentSlot.Headwear; break;
						case SlotType.Earpiece: slotIndex = EquipmentSlot.Earpiece; break;
						case SlotType.ArmBand: slotIndex = EquipmentSlot.ArmBand; break;
						default:
							throw new ArgumentOutOfRangeException();
					}
					LogDebug($"Apply transmog to {slotIndex.ToString()} slot.");
					newEquipments[i].AffectSlot(newEquipmentClass.GetSlot(slotIndex));
				}
			}

			return newEquipmentClass;
		}

		public class CostumeItem
		{
			private bool _ready = false;
			private bool _hide = false;
			private Item _item;
			private readonly ConfigEntry<string> _configEntry;

			public CostumeItem(ConfigEntry<string> configEntry)
			{
				_configEntry = configEntry;
				_configEntry.SettingChanged += ConfigEntryOnSettingChanged;
			}

			private void ConfigEntryOnSettingChanged(object sender, EventArgs e)
			{
				_item = Deserialize(_configEntry.Value);
				_ready = true;
			}

			public Item Get()
			{
				if (!_ready)
				{
					_ready = true;
					_item = Deserialize(_configEntry.Value);
				}
				return _item;
			}

			public void Set(Item item)
			{
				_hide = false;
				_ready = true;
				_configEntry.Value = Serialize(item);
				_item = Deserialize(_configEntry.Value);
			}

			public void Hide()
			{
				_hide = true;
				_item = null;
				_ready = true;
				_configEntry.Value = "hide";
			}

			public void Reset()
			{
				_hide = false;
				_item = null;
				_ready = true;
				_configEntry.Value = string.Empty;
			}

			public bool IsHide()
			{
				if (!_ready)
				{
					_ready = true;
					_item = Deserialize(_configEntry.Value);
				}
				return _hide;
			}

			public bool HasEffect()
			{
				return IsHide() || Get() != null;
			}

			public void AffectSlot(Slot slot)
			{
				var hide = IsHide();
				var item = Get();
				if (hide || item != null)
					slot.RemoveItem();
				if (item != null)
				{
					item.CurrentAddress = null;
					slot.AddWithoutRestrictions(item);
					slot.ApplyContainedItem();
				}
			}

			private Item Deserialize(string config)
			{
				_hide = false;
				if (string.IsNullOrWhiteSpace(config))
					return null;
				var itemFactory = Singleton<ItemFactoryClass>.Instance;
				var solver = itemFactory.ItemTemplates;
				var queue = new Queue<Item>();
				var splittedConfig = config.Split(',');
				if (splittedConfig.Length > 0 &&
				    splittedConfig[0].Equals("hide", StringComparison.InvariantCultureIgnoreCase))
				{
					_hide = true;
					return null;
				}
				foreach (var s in splittedConfig)
				{
					var id = s.Trim().ToLowerInvariant();
					if (id == "null" || !solver.TryGetValue(id, out var itemTemplate))
						queue.Enqueue(null);
					else
					{
						queue.Enqueue(itemFactory.CreateItem(MongoID.Generate(), id, null));
					}
				}

				if (!queue.TryDequeue(out var item) || item == null)
					return null;

				LogInfo("Begin deserialize...");
				LogInfo($"Append {item.TemplateId}");

				void DeserializeDo(Item _rootItem, Queue<Item> _itemQueue)
				{
					if (_rootItem is CompoundItem loot)
					{
						var slots = loot.Slots;
						foreach (var slot in slots)
						{
							if (_itemQueue.TryDequeue(out var element) && 
							    element != null && 
							    slot.CanAccept(element))
							{
								slot.AddWithoutRestrictions(element);
								LogInfo($"Append {element.TemplateId}");
							}
						}
						foreach (var slot in slots)
						{
							if (slot.ContainedItem != null)
							{
								DeserializeDo(slot.ContainedItem, _itemQueue);
							}
						}
					}
				}

				DeserializeDo(item, queue);
				return item;
			}

			private string Serialize(Item item)
			{
				if (item == null)
					return string.Empty;
				var sb = new StringBuilder();

				LogInfo("Begin serialize...");
				LogInfo($"Append {item.TemplateId}");
				sb.Append(item.TemplateId);
				void SerializeDo(Item _item2, StringBuilder _sb)
				{
					if (_item2 is CompoundItem loot)
					{
						var slots = loot.Slots;
						foreach (var slot in slots)
						{
							if (slot.ContainedItem != null)
							{
								LogInfo($"Append {slot.ContainedItem.TemplateId}");
								_sb.Append(", ").Append(slot.ContainedItem.TemplateId);
							}
							else
							{
								_sb.Append(", null");
							}
						}
						foreach (var slot in slots)
						{
							if (slot.ContainedItem != null)
							{
								SerializeDo(slot.ContainedItem, _sb);
							}
						}
					}
				}

				SerializeDo(item, sb);
				return sb.ToString();
			}
		}
	}
}
