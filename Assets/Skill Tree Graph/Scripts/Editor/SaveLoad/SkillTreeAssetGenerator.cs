using System.Collections.Generic;
using System.Linq;
using SkillTreeGraph.Core;
using UnityEditor;
using UnityEngine;

namespace SkillTreeGraph.Editor
{
    public static class SkillTreeAssetGenerator
    {
        public static void Generate(string path, SkillTreeDatabase database, SkillTreeSettingData settings, SkillTreeCollection collection)
        {
            bool isExisting = database != null;
            if (!isExisting)
            {
                database = ScriptableObject.CreateInstance<SkillTreeDatabase>();
                database.Id = settings.Id;
                AssetDatabase.CreateAsset(database, $"{path}/{settings.SkillTreeName}-SkillTree.asset");
            }

            var currentNodes = collection.Nodes;
            var currentIds = new HashSet<string>(currentNodes.Select(n => n.Id));

            AssetDatabase.StartAssetEditing();
            try
            {
                var nodesToRemove = database.SkillDatabase.Where(n => n == null || !currentIds.Contains(n.Id)).ToList();
                foreach (var nodeToDelete in nodesToRemove)
                {
                    string assetPath = AssetDatabase.GetAssetPath(nodeToDelete);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        database.RemoveNode(nodeToDelete);
                        AssetDatabase.DeleteAsset(assetPath);
                    }
                }

                int i = 0;
                foreach (var node in collection.Nodes)
                {
                    string nodeName = string.IsNullOrEmpty(node.DisplayName) ? $"SkillNode_{i}" : node.DisplayName;

                    if (isExisting && database.TryGetNode(node.Id, out var existing))
                    {
                        EditorUtility.CopySerializedManagedFieldsOnly(node, existing);
                        AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(existing), nodeName);
                        existing.name = nodeName;
                        EditorUtility.SetDirty(existing);
                    }
                    else
                    {
                        var asset = ScriptableObject.CreateInstance<SkillNode>();
                        EditorUtility.CopySerialized(node, asset);
                        AssetDatabase.CreateAsset(asset, $"{path}/{nodeName}.asset");
                        database.AddNode(asset);
                    }
                    i++;
                }
            }
            finally { AssetDatabase.StopAssetEditing(); }

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(database);
        }
    }
}
