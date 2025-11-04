using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using MiscFixes.Modules;
using RoR2;
using SS2;
using MSU;

[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace ShardTierClassicItems
{
    [BepInDependency(AncientScepter.AncientScepterMain.ModGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(ClassicItemsReturns.ClassicItemsReturnsPlugin.ModGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(SS2.SS2Main.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public static PluginInfo PluginInfo { get; private set; }
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "LordVGames";
        public const string PluginName = "ShardTierClassicItems";
        public const string PluginVersion = "1.2.1";
        public void Awake()
        {
            PluginInfo = Info;
            Log.Init(Logger);
            MyAssets.Init();

            // i can't pass the config to another method that's directly added to SetItemDefs
            // but i can do it within a delegate in awake!
            On.RoR2.ItemCatalog.SetItemDefs += (On.RoR2.ItemCatalog.orig_SetItemDefs orig, ItemDef[] itemDefs) =>
            {
                /*foreach (ItemDef itemDef in RoR2.ContentManagement.ContentManager.itemDefs)
                {
                    Log.Debug(itemDef.nameToken);
                    itemDef.tier
                }*/

                orig(itemDefs);
                // not running the ss2 beta causes the mod to immediately die trying to get the curio color
                // and the stack trace doesn't include the mod's name unless we move everything under it's own method
                Guh(Config);
            };
        }

        private static void Guh(ConfigFile config)
        {
            if (!SS2Config.enableBeta.value)
            {
                Log.Warning("This mod will not run due to required Starstorm 2 beta content not being enabled. Enable the 0.7 beta to use this mod.");
                return;
            }

            Main.CurioColor = ColorCatalog.GetColor(SS2Content.ItemTierDefs.Curio.colorIndex);
            Main.SetupTradeController();
            ConfigOptions.BindConfigOptions(config);
            config.WipeConfig();
            Main.EditClassicItems();
            Main.FillCurioItemIndexesList();
        }
    }
}