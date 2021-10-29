using System;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor
{
    public partial class BasePort : IntegratedViewModel
    {
        [NonSerialized]
        BaseNode owner;

        protected override void InitializeBindableProperties() { }

        public void Enable(BaseNode node)
        {
            owner = node;
        }

        internal class ConnectionHorizontalComparer : IComparer<BaseConnection>
        {
            public int Compare(BaseConnection x, BaseConnection y)
            {
                if (x.ToNode.Position.x < y.ToNode.Position.x)
                    return -1;
                if (x.ToNode.Position.x > y.ToNode.Position.x)
                    return 1;
                return 0;
            }
        }

        internal class ConnectionVerticalComparer : IComparer<BaseConnection>
        {
            public int Compare(BaseConnection x, BaseConnection y)
            {
                if (x.ToNode.Position.y > y.ToNode.Position.y)
                    return -1;
                if (x.ToNode.Position.y < y.ToNode.Position.y)
                    return 1;
                return 0;
            }
        }
    }
}
