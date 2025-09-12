using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using RoR2;
using MiscFixes.Modules;

namespace ShardTierClassicItems
{
    public static class ConfigOptions
    {
        internal static ConfigEntry<bool> LogExtraInfo;

        internal static List<ConfigEntry<string>> TradesConfigEntriesList = [];
        internal static List<ConfigEntry<bool>> TierChangeConfigEntriesList = [];

        internal static void BindConfigOptions(ConfigFile config)
        {
            LogExtraInfo = config.BindOption(
                "Other",
                "Enable logging extra info",
                "If you wanna see stuff like all valid trades, the mod changing item tiers and adding items to trades, enable this.",
                false
            );



            string tradeConfigOptionDescription = "Set what shard trades this item should be added to.\nValid options:\n";
            foreach (var tradeName in Main.ValidTradeNames)
            {
                // i could do a smart check for it if's the last value in the list but it's not needed here 
                if (tradeName == "None")
                {
                    tradeConfigOptionDescription += tradeName;
                }
                else
                {
                    tradeConfigOptionDescription += $"{tradeName}, ";
                }
            }
            tradeConfigOptionDescription += "\nSeparate each one with a COMMA ( , ) and NO SPACES, except for `All` or `None` which must be alone.";
            tradeConfigOptionDescription += "\n\nNOTE: Setting to `All` means it may add itself to other modded trades!";

            foreach (var itemDef in RoR2.ContentManagement.ContentManager.itemDefs)
            {
                if (!Main.IsItemDefNormalClassicItem(itemDef.nameToken))
                {
                    continue;
                }
                string englishItemName = Language.GetString(itemDef.nameToken, "en").Replace("'", "");

                // changing the language would cause the config to use a differently named set of options
                // so let's stick to english item names to prevent that
                TradesConfigEntriesList.Add(config.BindOption(
                    "Add to shard trades",
                    englishItemName,
                    tradeConfigOptionDescription,
                    "All"
                ));

                TierChangeConfigEntriesList.Add(config.BindOption(
                    "Change to shard tier",
                    englishItemName,
                    "Should this item be changed to shard tier?",
                    true
                ));
            }
        }
    }
}