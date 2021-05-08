using System;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor
{
    public interface IVariableSource
    {
        SharedVariable GetVariable(string name);

        List<SharedVariable> GetAllVariables();

        void SetVariable(string name, SharedVariable sharedVariable);
    }
}