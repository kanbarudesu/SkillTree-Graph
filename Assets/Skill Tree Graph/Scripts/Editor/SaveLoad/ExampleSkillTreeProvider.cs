using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SkillTreeGraph.Editor
{
    public class ExampleSkillTreeProvider
    {
        private List<TextAsset> _entries;
        private int _index;

        public SkillTreeSaveData GetNext()
        {
            if (_entries == null || _index >= _entries.Count)
            {
                var saveFiles = Resources.LoadAll<TextAsset>(SkillTreePathUtility.ExampleSavePath);
                if (saveFiles.Length == 0) return null;

                _entries = saveFiles.OrderBy(_ => Random.value).ToList();
                _index = 0;
            }

            var selectedFile = _entries[_index++];
            var data = new SkillTreeSaveData();
            EditorJsonUtility.FromJsonOverwrite(selectedFile.text, data);
            return data;
        }
    }
}
