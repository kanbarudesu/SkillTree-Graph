namespace SkillTreeGraph.Core
{
    public interface ISkillTreeSaveStorage
    {
        void Save(SkillTreeSaveData saveData);
        SkillTreeSaveData Load();
    }
}