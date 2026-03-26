namespace SkillTreeGraph.Editor
{
    public enum GraphInteractionMode
    {
        Default,
        Connect
    }

    public class GraphInteractionController
    {
        public GraphInteractionMode CurrentMode { get; private set; } = GraphInteractionMode.Default;

        private readonly GraphConnectionController _connectionRenderer;
        private readonly UndoManager _undoManager;
        private SkillNodeView _firstSelected;

        public GraphInteractionController(GraphConnectionController connectionRenderer, UndoManager undoManager)
        {
            _connectionRenderer = connectionRenderer;
            _undoManager = undoManager;
        }

        public void EnterConnectMode()
        {
            CurrentMode = GraphInteractionMode.Connect;
            _firstSelected = null;
        }

        public void ExitMode()
        {
            if (CurrentMode == GraphInteractionMode.Connect)
            {
                CurrentMode = GraphInteractionMode.Default;
                _firstSelected = null;
            }
        }

        public void OnNodeClicked(SkillNodeView node)
        {
            if (CurrentMode == GraphInteractionMode.Default)
                return;

            if (CurrentMode == GraphInteractionMode.Connect)
            {
                HandleConnectClick(node);
            }
        }

        private void HandleConnectClick(SkillNodeView node)
        {
            if (_firstSelected == null)
            {
                _firstSelected = node;
                return;
            }

            if (_firstSelected == node)
                return;

            _undoManager.ExecuteCommand(new ConnectNodeCommand(_firstSelected.Data.Id, node.Data.Id));
            ExitMode();
        }

        public void RedrawConnections(SkillNodeView parent, SkillNodeView child)
        {
            if (!parent.Data.ChildrenIds.Contains(child.Data.Id))
                parent.Data.ChildrenIds.Add(child.Data.Id);

            if (!child.Data.ParentIds.Contains(parent.Data.Id))
                child.Data.ParentIds.Add(parent.Data.Id);

            _connectionRenderer.DrawConnection(parent, child);
        }
    }
}