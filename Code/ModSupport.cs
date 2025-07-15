using AncientScepter;
using BepInEx;
using BepInEx.Configuration;
using ClassicItemsReturns;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using SS2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ShardTierClassicItems
{
    internal static class ModSupport
    {
        internal static class ClassicItemsReturnsMod
        {
            private static bool? _modexists;
            internal static bool ModIsRunning
            {
                get
                {
                    _modexists ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ClassicItemsReturnsPlugin.ModGuid);
                    return (bool)_modexists;
                }
            }

            internal static bool Use3DModels
            {
                get
                {
                    if (ModIsRunning)
                    {
                        return ClassicItemsReturnsPlugin.use3dModels;
                    }
                    return false;
                }
            }

            internal static bool UseClassicSprites
            {
                get
                {
                    if (ModIsRunning)
                    {
                        return ClassicItemsReturnsPlugin.useClassicSprites;
                    }
                    return false;
                }
            }
        }

        internal static class AncientScepterMod
        {
            private static bool? _modexists;
            internal static bool ModIsRunning
            {
                get
                {
                    _modexists ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(AncientScepter.AncientScepterMain.ModGuid);
                    return (bool)_modexists;
                }
            }

            [HarmonyPatch]
            internal static class HarmonyPatches
            {
                [HarmonyPatch(typeof(AncientScepter.AncientScepterItem), nameof(AncientScepter.AncientScepterItem.Reroll))]
                [HarmonyILManipulator]
                internal static void PatchRerollItemResult(ILContext il)
                {
                    ILCursor c = new(il);

                    // move to after where the item index to give is set
                    if (!c.TryGotoNext(MoveType.After,
                        x => x.MatchStloc(6)
                    ))
                    {
                        Log.Error($"COULD NOT IL HOOK {il.Method.Name}");
                        Log.Warning($"il is {il}");
                        return;
                    }

                    c.EmitDelegate<Func<ItemIndex>>(() =>
                    {
                        // is it bad to use run instance rng?
                        return Main.CurioItemIndexesList.GetRandom(Run.instance.treasureRng);
                    });
                    c.Emit(OpCodes.Stloc, 6);
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            internal static void OverrideScepterDescriptionTokenIfApplicable()
            {
                // copied from ancient scepter duh
                string description = string.Concat(
                [
                    "Upgrade one of your <style=cIsUtility>skills</style>. <style=cStack>(Unique per character)</style> <style=cStack>",
                    (AncientScepterItem.rerollMode != AncientScepterItem.RerollMode.Disabled) ? "Extra/Unusable" : "Unusable (but NOT extra)",
                    " pickups will reroll into ",
                    (AncientScepterItem.rerollMode == AncientScepterItem.RerollMode.Scrap) ? "red scrap" : "other shard-tier items.",
                    "</style>"
                ]);

                R2API.LanguageAPI.AddOverlay("ITEM_ANCIENT_SCEPTER_DESCRIPTION", description);
            }
        }

        internal static class SoftDependencies
        {
            // not the best, but whatever it works
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            internal static void SetItemDefSpriteToCurioSprite(ItemDef itemDef)
            {
                string spriteName = $"{itemDef.name}.png";
                Sprite curioSprite = null;
                bool hasLoadedSprite = false;
                bool tryLoadReturnsSpriteEarly = false;


                if (Main.IsItemAncientScepter(itemDef.name) || ClassicItemsReturnsMod.Use3DModels)
                {
                    curioSprite = MyAssets.R2IconsBundle.LoadAsset<Sprite>(spriteName);
                    if (curioSprite == null)
                    {
                        if (Main.IsItemAncientScepter(itemDef.name))
                        {
                            Log.Warning($"Shard-tier 3D item sprite for \"{itemDef.name}\" could not be found!");
                            Log.Error("All possible sprite options not found. The item's sprite will not be changed.");
                            return;
                        }
                        else
                        {
                            Log.Warning($"Shard-tier 3D item sprite for \"{itemDef.name}\" could not be found!");
                            Log.Warning("Falling back to the Returns sprite.");
                            tryLoadReturnsSpriteEarly = true;
                        }
                    }
                    else
                    {
                        hasLoadedSprite = true;
                    }
                }
                if (tryLoadReturnsSpriteEarly)
                {
                    curioSprite = MyAssets.RRIconsBundle.LoadAsset<Sprite>(spriteName);
                    if (curioSprite == null)
                    {
                        Log.Warning($"Shard-tier returns item sprite for \"{itemDef.name}\" could not be found!");
                        Log.Warning("Falling back to the Classic sprite.");
                    }
                    else
                    {
                        hasLoadedSprite = true;
                    }
                }
                if (!hasLoadedSprite && ClassicItemsReturnsMod.UseClassicSprites || (!hasLoadedSprite && tryLoadReturnsSpriteEarly))
                {
                    curioSprite = MyAssets.R1IconsBundle.LoadAsset<Sprite>(spriteName);
                    if (curioSprite == null)
                    {
                        if (tryLoadReturnsSpriteEarly)
                        {
                            Log.Warning($"Shard-tier classic item sprite for item \"{itemDef.name}\" could not be found!");
                            Log.Error("All possible sprite options not found. The item's sprite will not be changed.");
                            return;
                        }
                        else
                        {
                            Log.Warning($"Shard-tier classic item sprite for \"{itemDef.name}\" could not be found!");
                            Log.Warning("Falling back to the Returns sprite.");
                        }
                    }
                    else
                    {
                        hasLoadedSprite = true;
                    }
                }
                if (!hasLoadedSprite)
                {
                    curioSprite = MyAssets.RRIconsBundle.LoadAsset<Sprite>(spriteName);
                    if (curioSprite == null)
                    {
                        Log.Warning($"Shard-tier returns item sprite for \"{itemDef.name}\" could not be found!");
                        Log.Error("All possible sprite options not found. The item's sprite will not be changed.");
                        curioSprite = itemDef.pickupIconSprite;
                        return;
                    }
                }


                itemDef.pickupIconSprite = curioSprite;
            }
        }
    }
}