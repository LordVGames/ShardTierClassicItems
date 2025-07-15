using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using RoR2;
using SS2;

namespace ShardTierClassicItems
{
    internal static class Main
    {
        internal static Color CurioColor;
        internal static TradeController tradeController = null;
        internal static List<string> ValidTradeNames = [];
        internal static List<ItemIndex> CurioItemIndexesList = [];

        internal static void SetupTradeController()
        {
            GameObject tradeTele = SS2Assets.LoadAsset<GameObject>("TradeTeleporter", SS2Bundle.Interactables);
            tradeController = tradeTele.GetComponent<TradeController>();
            Log.Info("ALl valid trades:");
            foreach (TradeDef tradeDef in tradeController.trades)
            {
                Log.Info($"{tradeDef.name} (requires {tradeDef.desiredItem.name})");
                ValidTradeNames.Add(tradeDef.name);
            }
            ValidTradeNames.Add("All");
            ValidTradeNames.Add("None");
        }

        internal static void EditClassicItems()
        {
            foreach (ItemDef itemDef in RoR2.ContentManagement.ContentManager.itemDefs)
            {
                if (!IsItemDefNormalClassicItem(itemDef.nameToken))
                {
                    continue;
                }

                ConfigEntry<string> itemTradesConfigEntry = GetTradesConfigEntryForItemDef(itemDef);
                if (ShouldItemBeAddedToTrades(itemTradesConfigEntry))
                {
                    AddItemDefToConfiguredTrades(itemDef, itemTradesConfigEntry);
                }

                ConfigEntry<bool> itemTierChangeConfigEntry = GetTierChangeConfigEntryForItemDef(itemDef);
                if (itemTierChangeConfigEntry.Value)
                {
                    MakeItemDefCurioTier(itemDef, itemTierChangeConfigEntry);
                    if (IsItemAncientScepter(itemDef.nameToken))
                    {
                        Harmony harmony = new(Plugin.PluginGUID);
                        harmony.CreateClassProcessor(typeof(ModSupport.AncientScepterMod.HarmonyPatches)).Patch();
                        ModSupport.AncientScepterMod.OverrideScepterDescriptionTokenIfApplicable();
                    }
                }
            }
        }

        private static void AddItemDefToConfiguredTrades(ItemDef itemDef, ConfigEntry<string> itemConfigEntry)
        {
            string[] tradesToAddTo = itemConfigEntry.Value.Split(',');
            if (tradesToAddTo[0].ToLower() == "all")
            {
                Log.Info($"Adding \"{itemConfigEntry.Definition.Key}\" to ALL trades!");
                foreach (TradeDef tradeDef in tradeController.trades)
                {
                    tradeDef.options = tradeDef.options.AddToArray(CreateTradeOptionFromItemDef(itemDef));
                    Log.Debug($"tradeDef.options.Length is {tradeDef.options.Length}");
                }
            }
            else
            {
                foreach (TradeDef tradeDef in tradeController.trades)
                {
                    if (tradesToAddTo.Contains(tradeDef.name))
                    {
                        Log.Info($"Adding \"{itemConfigEntry.Definition.Key}\" to trade \"{tradeDef.name}\"");
                        tradeDef.options = tradeDef.options.AddToArray(CreateTradeOptionFromItemDef(itemDef));
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void MakeItemDefCurioTier(ItemDef itemDef, ConfigEntry<bool> itemTierChangeConfigEntry)
        {
            Log.Info($"Changing \"{itemTierChangeConfigEntry.Definition.Key}\" item tier to shard tier!");
            Log.Debug(itemDef.name);
            itemDef._itemTierDef = SS2Content.ItemTierDefs.Curio;
            itemDef.tier = SS2Content.ItemTierDefs.Curio.tier;
            ModSupport.SoftDependencies.SetItemDefSpriteToCurioSprite(itemDef);
        }



        internal static void FillCurioItemIndexesList()
        {
            foreach (ItemDef itemDef in RoR2.ContentManagement.ContentManager.itemDefs)
            {
                if (itemDef._itemTierDef != SS2Content.ItemTierDefs.Curio)
                {
                    continue;
                }
                if (itemDef.name.Contains("SHARD") || IsItemAncientScepter(itemDef.name))
                {
                    continue;
                }

                CurioItemIndexesList.Add(itemDef.itemIndex);
            }
        }

        internal static TradeDef.TradeOption CreateTradeOptionFromItemDef(ItemDef itemDef)
        {
            TradeDef.TradeOption newTradeOption = new()
            {
                pickupName = itemDef.nameToken,
                pickupDef = itemDef,
                pickupIndex = PickupCatalog.FindPickupIndex(itemDef.itemIndex),
                //weight = 1
            };
            return newTradeOption;
        }

        internal static bool ShouldItemBeAddedToTrades(ConfigEntry<string> itemConfigEntry)
        {
            if (itemConfigEntry.Value == "None")
            {
                Log.Info($"\"{itemConfigEntry.Definition.Key}\" will not be added to any trades due to being set to \"None\".");
                return false;
            }
            else if (itemConfigEntry.Value.IsNullOrWhiteSpace())
            {
                Log.Info($"\"{itemConfigEntry.Definition.Key}\" will not be added to any trades due to being set to blank.");
                return false;
            }
            return true;
        }

        internal static ConfigEntry<string> GetTradesConfigEntryForItemDef(ItemDef itemDef)
        {
            foreach (var configEntry in ConfigOptions.TradesConfigEntriesList)
            {
                if (configEntry.Definition.Key == Language.GetString(itemDef.nameToken, "en"))
                {
                    return configEntry;
                }
            }
            return null;
        }

        internal static ConfigEntry<bool> GetTierChangeConfigEntryForItemDef(ItemDef itemDef)
        {
            foreach (var configEntry in ConfigOptions.TierChangeConfigEntriesList)
            {
                if (configEntry.Definition.Key == Language.GetString(itemDef.nameToken, "en"))
                {
                    return configEntry;
                }
            }
            return null;
        }

        internal static bool IsItemDefNormalClassicItem(string nameToken)
        {
            return (nameToken.StartsWith("CLASSICITEMSRETURNS") || IsItemAncientScepter(nameToken)) && !nameToken.Contains("DRONEITEM");
        }

        internal static bool IsItemAncientScepter(string nameToken)
        {
            return nameToken == "ITEM_ANCIENT_SCEPTER_NAME";
        }
    }
}