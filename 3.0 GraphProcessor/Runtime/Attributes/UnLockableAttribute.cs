using System;

namespace GraphProcessor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class LockableAttribute : Attribute { }
}
