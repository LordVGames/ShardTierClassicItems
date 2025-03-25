using System.IO;
using UnityEngine;

namespace ShardTierClassicItems
{
    public static class MyAssets
    {
        public static AssetBundle R2IconsBundle;
        public const string R2IconsBundleName = "recolored_ror2_icons";
        public static string R2IconsBundlePath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(Plugin.PluginInfo.Location), R2IconsBundleName);
            }
        }

        public static AssetBundle R1IconsBundle;
        public const string R1IconsBundleName = "recolored_ror1_icons";
        public static string R1IconsBundlePath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(Plugin.PluginInfo.Location), R1IconsBundleName);
            }
        }

        public static AssetBundle RRIconsBundle;
        public const string RRIconsBundleName = "recolored_rorr_icons";
        public static string RRIconsBundlePath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(Plugin.PluginInfo.Location), RRIconsBundleName);
            }
        }

        public static void Init()
        {
            R2IconsBundle = AssetBundle.LoadFromFile(R2IconsBundlePath);
            R1IconsBundle = AssetBundle.LoadFromFile(R1IconsBundlePath);
            RRIconsBundle = AssetBundle.LoadFromFile(RRIconsBundlePath);
        }
    }
}