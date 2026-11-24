using itolib.Structs;
using UnityEngine;

namespace itolib.Behaviours.Helpers
{
    /// <summary>
    ///     Aligns one or several pivots towards a target rotation, in world space.
    /// </summary>
    public class WorldAligner : MonoBehaviour
    {
        /// <summary>
        ///     Rotation to align pivots towards, in world space.
        /// </summary>
        [Header("World Aligner")]
        [Tooltip("Rotation to align pivots towards, in world space.")]
        [SerializeField] private Vector3 worldRotation = Vector3.zero;

        /// <summary>
        ///     List of pivots to align.
        /// </summary>
        [Tooltip("List of pivots to align.")]
        [SerializeField] private RotationPivot[]? pivotsToAlign;

        /// <summary>
        ///     Whether to disable the script after all pivots reach their target rotations or not.
        /// </summary>
        [Tooltip("Whether to disable the script after all pivots reach their target rotations or not.")]
        [SerializeField] private bool disableOnAlign = true;

        /// <summary>
        ///     Current or initial velocities for every pivot's rotation.
        /// </summary>
        private Vector4[] pivotVelocities;

        /// <summary>
        ///     Handle additional initialization.
        /// </summary>
        private void Awake()
        {
            if (pivotsToAlign?.Length > 0)
            {
                pivotVelocities = new Vector4[pivotsToAlign.Length];
            }
        }

        /// <summary>
        ///     Handle pivot rotation towards the target rotation.
        /// </summary>
        private void LateUpdate()
        {
            int aligned = 0;

            // Apply rotations towards the target direction to each pivot.
            for (int i = 0; i < pivotsToAlign?.Length; i++)
            {
                Transform? pivot = pivotsToAlign[i].pivot;

                if (pivot == null)
                {
                    aligned++;

                    continue;
                }

                pivotsToAlign[i].ApplyRotation(worldRotation, ref pivotVelocities[i]);

                if (pivotVelocities[i] == Vector4.zero)
                {
                    aligned++;
                }
            }

            if (pivotsToAlign == null || (disableOnAlign && aligned == pivotsToAlign.Length))
            {
                enabled = false;
            }
        }

        /// <summary>
        ///     Set rotation on the <c>X-axis</c>.
        /// </summary>
        /// <param name="pitch">Value to set the <c>X-axis</c> to.</param>
        public void SetRotationX(float pitch)
        {
            ChangeRotation(0, pitch);
        }

        /// <summary>
        ///     Increment rotation on the <c>X-axis</c>.
        /// </summary>
        /// <param name="pitch">Value to increment the <c>X-axis</c> by.</param>
        public void IncrementRotationX(float pitch)
        {
            ChangeRotation(0, pitch, increment: true);
        }

        /// <summary>
        ///     Set rotation on the <c>Y-axis</c>.
        /// </summary>
        /// <param name="yaw">Value to set the <c>Y-axis</c> to.</param>
        public void SetRotationY(float yaw)
        {
            ChangeRotation(1, yaw);
        }

        /// <summary>
        ///     Increment rotation on the <c>Y-axis</c>.
        /// </summary>
        /// <param name="yaw">Value to increment the <c>Y-axis</c> by.</param>
        public void IncrementRotationY(float yaw)
        {
            ChangeRotation(1, yaw, increment: true);
        }

        /// <summary>
        ///     Set rotation on the <c>Z-axis</c>.
        /// </summary>
        /// <param name="roll">Value to set the <c>Z-axis</c> to.</param>
        public void SetRotationZ(float roll)
        {
            ChangeRotation(2, roll);
        }

        /// <summary>
        ///     Increment rotation on the <c>Z-axis</c>.
        /// </summary>
        /// <param name="roll">Value to increment the <c>Z-axis</c> by.</param>
        public void IncrementRotationZ(float roll)
        {
            ChangeRotation(2, roll, increment: true);
        }

        /// <summary>
        ///     Change target rotation in world space for all pivots to align towards.
        /// </summary>
        /// <param name="axis">Axis to modify.</param>
        /// <param name="value">Value to modify rotation with.</param>
        /// <param name="increment">Whether to add to the current value or not.</param>
        private void ChangeRotation(int axis, float value, bool increment = false)
        {
            if (!increment)
            {
                worldRotation[axis] = value;
            }
            else
            {
                worldRotation[axis] += value;
            }

            if (disableOnAlign)
            {
                enabled = true;
            }
        }
    }
}