using System.IO;
using UnityEditor;

namespace SkillTreeGraph.Editor
{
    public static class SkillTreeFileIO
    {
        public static void Save(string path, SkillTreeSaveData data, bool refreshAssetDatabase = true)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(path, EditorJsonUtility.ToJson(data, true));
            if (refreshAssetDatabase) AssetDatabase.Refresh();
        }

        public static SkillTreeSaveData Load(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;

            var data = new SkillTreeSaveData();
            EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(path), data);
            return data;
        }

        public static void Delete(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            string relativePath = SkillTreePathUtility.ToProjectRelativePath(path);
            if (relativePath.StartsWith("Assets/") || relativePath.StartsWith("Packages/"))
            {
                AssetDatabase.DeleteAsset(relativePath);
                AssetDatabase.Refresh();
            }
        }
    }
}
