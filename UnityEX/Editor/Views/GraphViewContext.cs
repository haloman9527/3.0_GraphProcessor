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
        public CommandService CommandService;
        
        public void Do(Action @do, Action @undo)
        {
            // Redo 与 Do 相同：重做时执行相同操作
            Do(new CommandService.ActionCommand(@do, @do, @undo));
        }

        public void Do(Action @do, Action redo, Action @undo)
        {
            Do(new CommandService.ActionCommand(@do, redo, @undo));
        }

        public void Do(ICommand command)
        {
            command.Execute();
            frameCommands ??= new FrameCommands();
            frameCommands.RegisterCommand(command);
        }

        public void FrameEnd()
        {
            if (frameCommands != null)
            {
                CommandService.Register(frameCommands);
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
                    command.Execute();
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