using UnityEditor;

namespace MySpace
{
    public class MyImporter : AssetPostprocessor
    {
        private static void Rename(string org, string to)
        {
            FileUtil.MoveFileOrDirectory(org, to);
            FileUtil.MoveFileOrDirectory(org + ".meta", to + ".meta");
        }
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
        {
            var sc = System.StringComparison.OrdinalIgnoreCase;
            foreach (string asset in importedAssets)
            {
                if (asset.EndsWith(".sf2", sc))
                {
                    Rename(asset, asset + ".bytes");
                }
                else
                if (asset.EndsWith(".mid", sc))
                {
                    Rename(asset, asset + ".bytes");
                }
            }
            AssetDatabase.Refresh();
        }
    }
}
