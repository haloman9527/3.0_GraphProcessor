using System.Collections.Generic;

namespace CZToolKit.GraphProcessor
{
    public interface IOnGUIDispatcher
    {
        List<IOnGUIObserver> OnGUIObserver { get; }
    }

    public interface IOnGUIObserver
    {
        void OnGUI();
    }
}
