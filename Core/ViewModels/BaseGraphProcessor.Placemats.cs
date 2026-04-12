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

using System.Collections.Generic;

namespace Atom.GraphProcessor
{
    public partial class BaseGraphProcessor
    {
        private Dictionary<long, PlacematProcessor> m_Placemats;

        public IReadOnlyDictionary<long, PlacematProcessor> Placemats => m_Placemats;

        private void InitPlacemats()
        {
            if (m_Model.placemats == null)
                m_Model.placemats = new List<PlacematData>();

            m_Placemats = new Dictionary<long, PlacematProcessor>(m_Model.placemats.Count);
            for (var i = 0; i < m_Model.placemats.Count; i++)
            {
                var placemat = m_Model.placemats[i];
                if (placemat == null)
                {
                    ReportDiagnostic($"[MissingPlacemat] Null placemat at index {i} removed.");
                    m_Model.placemats.RemoveAt(i--);
                    continue;
                }

                if (m_Placemats.ContainsKey(placemat.id))
                {
                    ReportDiagnostic($"[DuplicatePlacemat] Placemat id={placemat.id} duplicated, later entry removed.");
                    m_Model.placemats.RemoveAt(i--);
                    continue;
                }

                var vm = ViewModelFactory.ProduceViewModel(placemat) as PlacematProcessor;
                vm.Owner = this;
                m_Placemats.Add(vm.ID, vm);
            }
        }

        public PlacematProcessor NewPlacemat(InternalVector2Int position)
        {
            var data = new PlacematData()
            {
                id = GraphProcessorUtil.GenerateId(),
                position = position,
            };
            return ViewModelFactory.ProduceViewModel(data) as PlacematProcessor;
        }

        public void AddPlacemat(PlacematProcessor placemat)
        {
            if (placemat == null || m_Placemats.ContainsKey(placemat.ID))
                return;

            placemat.Owner = this;
            m_Placemats.Add(placemat.ID, placemat);
            m_Model.placemats.Add(placemat.Model);
            m_GraphEvents.Publish(new AddPlacematEventArgs(placemat));
        }

        public void RemovePlacemat(long id)
        {
            if (!m_Placemats.TryGetValue(id, out var placemat))
                return;

            m_Placemats.Remove(id);
            m_Model.placemats.Remove(placemat.Model);
            m_GraphEvents.Publish(new RemovePlacematEventArgs(placemat));
        }
    }
}
