#region 注 释

/***
 *
 *  Title:
 *
 *  Description:
 *
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.haloman.net/
 *
 */

#endregion

using System;

namespace Atom.GraphProcessor
{
    [ViewModel(typeof(PlacematData))]
    public class PlacematProcessor : ViewModel, IGraphElementProcessor, IGraphElementProcessor_Scope
    {
        private readonly PlacematData m_Model;
        private readonly Type m_ModelType;
        private BaseGraphProcessor m_Owner;

        public PlacematData Model => m_Model;
        object IGraphElementProcessor.Model => m_Model;
        public Type ModelType => m_ModelType;
        Type IGraphElementProcessor.ModelType => m_ModelType;

        public BaseGraphProcessor Owner
        {
            get => m_Owner;
            internal set => m_Owner = value;
        }

        public long ID => m_Model.id;

        public string Title
        {
            get => m_Model.title;
            set => SetFieldValue(ref m_Model.title, value, nameof(PlacematData.title));
        }

        public InternalVector2Int Position
        {
            get => m_Model.position;
            set => SetFieldValue(ref m_Model.position, value, nameof(PlacematData.position));
        }

        public InternalVector2Int Size
        {
            get => m_Model.size;
            set => SetFieldValue(ref m_Model.size, value, nameof(PlacematData.size));
        }

        public InternalColor Color
        {
            get => m_Model.color;
            set => SetFieldValue(ref m_Model.color, value, nameof(PlacematData.color));
        }

        public PlacematProcessor(PlacematData model)
        {
            m_Model = model;
            m_ModelType = model.GetType();
            m_Model.position = model.position == default ? InternalVector2Int.zero : model.position;
            m_Model.size = model.size == default ? new InternalVector2Int(420, 260) : model.size;
        }
    }
}
