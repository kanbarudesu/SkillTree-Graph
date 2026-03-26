using System.Collections.Generic;

namespace SkillTreeGraph.Editor
{
    public class UndoManager
    {
        private readonly GraphContext _graphContext;
        private readonly GraphControllerContext _controllerContext;

        private readonly LinkedList<IUndoCommand> _undoList = new();
        private readonly LinkedList<IUndoCommand> _redoList = new();
        private readonly int _maxHistory;

        public UndoManager(GraphContext graphContext, GraphControllerContext controllerContext, int maxHistory = 20)
        {
            _graphContext = graphContext;
            _controllerContext = controllerContext;
            _maxHistory = maxHistory;
        }

        public void ExecuteCommand(IUndoCommand command)
        {
            command.InitializeCommand(_graphContext, _controllerContext);
            command.Execute();

            _undoList.AddLast(command);

            if (_undoList.Count > _maxHistory)
                _undoList.RemoveFirst();

            _redoList.Clear();
        }

        public void Undo()
        {
            if (_undoList.Count == 0)
                return;

            var command = _undoList.Last.Value;
            _undoList.RemoveLast();

            command.Undo();
            _redoList.AddLast(command);
        }

        public void Redo()
        {
            if (_redoList.Count == 0)
                return;

            var command = _redoList.Last.Value;
            _redoList.RemoveLast();

            command.Execute();
            _undoList.AddLast(command);
        }

        public void ClearHistory()
        {
            _undoList.Clear();
            _redoList.Clear();
        }
    }
}
