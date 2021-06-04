
namespace CZToolKit.GraphProcessor.Editors
{
    /// <summary> 所有指令的基类 </summary>
    public abstract class Command
    {
        /// <summary> The string that should appear in the Edit/Undo menu after this command is executed. </summary>
        public abstract string UndoString { get; }
    }
}