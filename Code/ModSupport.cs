using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
using RoR2;
using SS2;
using ClassicItemsReturns;

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
                    if (_modexists == null)
                    {
                        _modexists = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ClassicItemsReturnsPlugin.ModGuid);
                    }
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

        internal static class SoftDependencies
        {
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            internal static void SetItemDefSpriteToCurioSprite(ItemDef itemDef)
            {
                string spriteName = $"{itemDef.name}.png";
                Sprite curioSprite = null;
                bool isItemDefAncientScepter = itemDef.name == "ITEM_ANCIENT_SCEPTER";
                bool hasLoadedSprite = false;
                bool tryLoadReturnsSpriteEarly = false;


                if (isItemDefAncientScepter || ClassicItemsReturnsMod.Use3DModels)
                {
                    curioSprite = MyAssets.R2IconsBundle.LoadAsset<Sprite>(spriteName);
                    if (curioSprite == null)
                    {
                        if (isItemDefAncientScepter)
                        {
                            Log.Warning($"Shard-tier item sprite for \"Ancient Scepter\" could not be found!");
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
                        return;
                    }
                }


                itemDef.pickupIconSprite = curioSprite;
            }
        }

        // don't need a check for ancient scepter since we only ever check the itemdef name for it
    }
}