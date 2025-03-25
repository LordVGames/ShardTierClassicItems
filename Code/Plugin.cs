using BepInEx;
using BepInEx.Configuration;
using RoR2;

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
        public const string PluginVersion = "1.0.0";
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
        private static void Guh(ConfigFile Config)
        {
            Main.CurioColor = ColorCatalog.GetColor(SS2.SS2Content.ItemTierDefs.Curio.colorIndex);
            Main.SetupTradeController();
            ConfigOptions.BindConfigOptions(Config);
            Main.EditClassicItems();
        }
    }
}