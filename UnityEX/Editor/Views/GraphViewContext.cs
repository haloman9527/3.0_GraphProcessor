using System;

#if UNITY_EDITOR
namespace Atom.GraphProcessor.Editors
{
    [Serializable]
    public class GraphViewContext
    {
        private bool frameCommandGroupStarted;
        
        public BaseGraphWindow graphWindow;
        public CommandService CommandService;
        
        public void Do(Action @do, Action @undo)
        {
            // Redo 与 Do 相同：重做时执行相同操作
            Do(new CommandService.ActionCommand(@do, undo));
        }

        public void Do(Action @do, Action redo, Action @undo)
        {
            Do(new CommandService.ActionCommand(@do, undo));
        }

        public void Do(ICommand command)
        {
            if (!frameCommandGroupStarted)
            {
                CommandService.BeginGroup();
                frameCommandGroupStarted = true;
            }

            CommandService.Execute(command);
        }

        public void FrameEnd()
        {
            if (frameCommandGroupStarted)
            {
                CommandService.EndGroup();
                frameCommandGroupStarted = false;
            }
        }
    }
}
#endif
