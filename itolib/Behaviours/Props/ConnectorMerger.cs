using itolib.Behaviours.Detectors;
using UnityEngine;

namespace itolib.Behaviours.Props
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ConnectorMerger : DetectRegion<ConnectorMerger>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public int ConnectorsFound { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Connector Merger")]
        [Tooltip("")]
        public float tolerance = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool moveToCenter = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int priority = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string nameFilter = "";

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Reset()
        {
            maxObjects = 8;
            layerMask = 1 << LayerMask.NameToLayer("Room");
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void CheckObjectsInRegion()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            base.CheckObjectsInRegion();

            ConnectorsFound = 0;

            for (int i = 0; i < ObjectsFound; i++)
            {
                if (OverlapBuffer![i].TryGetComponent(out ConnectorMerger connector))
                {
                    if (connector == this || (nameFilter.Length > 0 && string.CompareOrdinal(nameFilter, connector.nameFilter) != 0))
                    {
                        continue;
                    }

                    if (priority < connector.priority)
                    {
                        return;
                    }

                    float magnitude = (transform.position - connector.transform.position).magnitude;

                    if (magnitude < tolerance)
                    {
                        onObjectsEach.Invoke(connector);
                        ConnectorsFound++;
                    }
                }
            }

            if (ConnectorsFound > 0)
            {
                onObjectsAny.Invoke(ConnectorsFound);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="connector"></param>
        public void DisableOtherConnector(ConnectorMerger connector)
        {
            if (moveToCenter)
            {
                Vector3 centerPos = Vector3.Lerp(transform.position, connector.transform.position, 0.5f);
                transform.position = centerPos;
            }

            connector.gameObject.SetActive(false);
        }
    }
}