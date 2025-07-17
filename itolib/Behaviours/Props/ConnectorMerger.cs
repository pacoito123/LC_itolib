using itolib.Behaviours.Detectors;
using itolib.Extensions;
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
        [Header("Connector Merger")]
        [Tooltip("")]
        [SerializeField] private float tolerance = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool moveToCenter = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private int priority = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private string nameFilter = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        private int connectorsFound;

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Reset()
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

            connectorsFound = 0;

            for (int i = 0; i < objectsFound; i++)
            {
                if (overlapBuffer![i].TryGetComponent(out ConnectorMerger connector))
                {
                    if (connector == this || (nameFilter.Length > 0 && nameFilter.CompareOrdinal(connector.nameFilter)))
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
                        connectorsFound++;
                    }
                }
            }

            if (connectorsFound > 0)
            {
                onObjectsAny.Invoke(connectorsFound);
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