using System;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public class SharedGameObject : SharedVariable<GameObject>
    {
        public SharedGameObject() : base() { }

        public SharedGameObject(string _guid) : base(_guid) { }

        public SharedGameObject(GameObject _value) : base(_value) { }

        public override object Clone()
        {
            SharedGameObject variable = new SharedGameObject(Value) { GUID = this.GUID };
            return variable;
        }
    }
}
