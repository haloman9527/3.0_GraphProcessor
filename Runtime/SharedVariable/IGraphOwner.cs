using Object = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    public interface IGraphOwner
    {
        BaseGraph Graph { get; set; }

        string GetOwnerName();

        int GetInstanceID();

        Object GetObject();

        SharedVariable GetVariable(string name);

        void SetVariable(string name, SharedVariable item);
        void SetVariableValue(string name, object value);
    }
}
