using System.IO;
using UnityEngine;

namespace SkillTreeGraph.Editor
{
    public static class SkillTreePathUtility
    {
        public const string DefaultSavePath = "Assets/Skill Tree Graph/";
        public const string ExampleSavePath = "Example Save Data/";
        public const string ScratchSaveFile = "Assets/Skill Tree Graph/Temp/autosave.json";

        public static string GetRelativePath(string absolutePath) =>
            "Assets" + absolutePath.Substring(Application.dataPath.Length);

        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        public static string ToProjectRelativePath(string absolutePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.GetRelativePath(projectRoot, absolutePath).Replace('\\', '/');
        }

        public static string ToAbsolutePath(string relativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.GetFullPath(Path.Combine(projectRoot, relativePath));
        }
    }
}
