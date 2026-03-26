namespace SkillTreeGraph.Editor
{
    public interface IUndoCommand
    {
        void InitializeCommand(GraphContext graphContext, GraphControllerContext controllerContext);
        void Execute();
        void Undo();
    }
}
