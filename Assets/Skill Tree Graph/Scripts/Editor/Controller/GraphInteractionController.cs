using UnityEngine.UIElements;

namespace SkillTreeGraph.Editor
{
    public enum GraphInteractionMode
    {
        Default,
        Connect
    }

    public class GraphInteractionController
    {
        private const string PendingConnectClass = "node-connect-pending";

        public GraphInteractionMode CurrentMode { get; private set; } = GraphInteractionMode.Default;

        private readonly GraphConnectionController _connectionRenderer;
        private readonly UndoManager _undoManager;
        private readonly VisualElement _background;

        private SkillNodeView _firstSelected;

        public GraphInteractionController(GraphContext graphContext, GraphConnectionController connectionRenderer, UndoManager undoManager)
        {
            _connectionRenderer = connectionRenderer;
            _undoManager = undoManager;
            _background = graphContext.GridBackground;

            _background.RegisterCallback<PointerDownEvent>(OnBackgroundPointerDown);
        }

        public void EnterConnectMode(bool isHold = false)
        {
            CurrentMode = GraphInteractionMode.Connect;
            _firstSelected = null;
        }

        public void ExitMode()
        {
            if (CurrentMode != GraphInteractionMode.Connect)
                return;

            if (_firstSelected != null)
                SetPendingHighlight(_firstSelected, false);

            CurrentMode = GraphInteractionMode.Default;
            _firstSelected = null;
        }

        public void OnNodeClicked(SkillNodeView node)
        {
            if (CurrentMode == GraphInteractionMode.Connect)
                HandleConnectClick(node);
        }

        private void HandleConnectClick(SkillNodeView node)
        {
            if (_firstSelected == null)
            {
                _firstSelected = node;
                SetPendingHighlight(node, true);
                return;
            }

            if (_firstSelected == node)
            {
                ExitMode();
                return;
            }

            _undoManager.ExecuteCommand(new ConnectNodeCommand(_firstSelected.Data.Id, node.Data.Id));
            ExitMode();
        }

        private static void SetPendingHighlight(SkillNodeView node, bool pending)
        {
            var mainButton = node.Q<VisualElement>("main-button");
            if (mainButton == null) return;

            if (pending) mainButton.AddToClassList(PendingConnectClass);
            else mainButton.RemoveFromClassList(PendingConnectClass);
        }

        private void OnBackgroundPointerDown(PointerDownEvent evt)
        {
            if (evt.target != _background) return;
            ExitMode();
        }

        public void OnHoldConnectKeyDown(KeyDownEvent evt)
        {
            if (CurrentMode == GraphInteractionMode.Connect) return;
            EnterConnectMode(isHold: true);
        }

        public void OnHoldConnectKeyUp(KeyUpEvent evt)
        {
            if (CurrentMode == GraphInteractionMode.Connect)
                ExitMode();
        }

        public void ConnectNode(SkillNodeView parent, SkillNodeView child)
        {
            if (!parent.Data.ChildrenIds.Contains(child.Data.Id))
                parent.Data.ChildrenIds.Add(child.Data.Id);

            if (!child.Data.ParentIds.Contains(parent.Data.Id))
                child.Data.ParentIds.Add(parent.Data.Id);

            _connectionRenderer.DrawConnection(parent, child);
        }

        public void DisconnectNode(SkillNodeView parent, SkillNodeView child)
        {
            if (parent.Data.ChildrenIds.Contains(child.Data.Id))
                parent.Data.ChildrenIds.Remove(child.Data.Id);

            if (child.Data.ParentIds.Contains(parent.Data.Id))
                child.Data.ParentIds.Remove(parent.Data.Id);

            _connectionRenderer.RemoveConnection(parent, child);
        }

        public void Dispose()
        {
            _background.UnregisterCallback<KeyDownEvent>(OnHoldConnectKeyDown);
            _background.UnregisterCallback<KeyUpEvent>(OnHoldConnectKeyUp);
            _background.UnregisterCallback<PointerDownEvent>(OnBackgroundPointerDown);
        }
    }
}
