using System;

namespace CommandUndoRedo
{
	public static class UndoRedoManager
	{
		static UndoRedo _undoRedo = new UndoRedo();

		public static int maxUndoStored {get {return _undoRedo.maxUndoStored;} set {_undoRedo.maxUndoStored = value;}}

		public static void Clear()
		{
			_undoRedo.Clear();
		}

		public static void Undo()
		{
			_undoRedo.Undo();
		}

		public static void Redo()
		{
			_undoRedo.Redo();
		}

		public static void Insert(ICommand command)
		{
			_undoRedo.Insert(command);
		}

		public static void Execute(ICommand command)
		{
			_undoRedo.Execute(command);
		}
	}
}
