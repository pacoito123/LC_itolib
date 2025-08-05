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
            base.CheckObjectsInRegion();

            connectorsFound = 0;

            for (int i = 0; i < objectsFound; i++)
            {
                Collider? connectorCollider = overlapBuffer?[i];

                if (connectorCollider == null || !connectorCollider.enabled) // Skip disabled colliders.
                {
                    continue;
                }

                if (connectorCollider.TryGetComponent(out ConnectorMerger otherConnector))
                {
                    if (otherConnector == this || (nameFilter.Length > 0 && nameFilter.CompareOrdinal(otherConnector.nameFilter)))
                    {
                        continue;
                    }

                    if (priority < otherConnector.priority)
                    {
                        return;
                    }

                    float sqrMagnitude = (transform.position - otherConnector.transform.position).sqrMagnitude;

                    if (sqrMagnitude < tolerance * tolerance)
                    {
                        onObjectsEach.Invoke(otherConnector);
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
        /// <param name="otherConnector"></param>
        public void DisableSelf(ConnectorMerger otherConnector)
        {
            if (moveToCenter)
            {
                Vector3 centerPos = Vector3.Lerp(otherConnector.transform.position, transform.position, 0.5f);
                otherConnector.transform.position = centerPos;
            }

            gameObject.SetActive(false);
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

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="connector"></param>
        public void DisableBothConnectors(ConnectorMerger connector)
        {
            connector.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}