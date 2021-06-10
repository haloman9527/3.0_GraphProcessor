
namespace CZToolKit.GraphProcessor
{
    /// <summary> 所有指令的基类 </summary>
    public abstract class Command
    {
        public abstract string UndoString { get; }
    }
}