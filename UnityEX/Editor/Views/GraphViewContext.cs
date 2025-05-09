using System;
using System.Collections.Generic;

#if UNITY_EDITOR
namespace Atom.GraphProcessor.Editors
{
    [Serializable]
    public class GraphViewContext
    {
        private FrameCommands frameCommands;
        
        public BaseGraphWindow graphWindow;
        public CommandDispatcher commandDispatcher;
        
        public void Do(Action @do, Action @undo)
        {
            Do(new CommandDispatcher.ActionCommand(@do, @do, @undo));
        }

        public void Do(ICommand command)
        {
            command.Do();
            frameCommands ??= new FrameCommands();
            frameCommands.RegisterCommand(command);
        }

        public void FrameEnd()
        {
            if (frameCommands != null)
            {
                commandDispatcher.Register(frameCommands);
                frameCommands = null;
            }
        }

        public sealed class FrameCommands : ICommand
        {
            public List<ICommand> commands = new List<ICommand>();

            public void RegisterCommand(ICommand command)
            {
                commands.Add(command);
            }

            public void Do()
            {
                for (var index = 0; index < commands.Count; index++)
                {
                    var command = commands[index];
                    command.Do();
                }
            }

            public void Redo()
            {
                for (var index = 0; index < commands.Count; index++)
                {
                    var command = commands[index];
                    command.Redo();
                }
            }

            public void Undo()
            {
                for (var index = commands.Count - 1; index >= 0; index--)
                {
                    var command = commands[index];
                    command.Undo();
                }
            }
        }
    }
}
#endif