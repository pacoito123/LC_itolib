using UnityEngine;

namespace itolib.Behaviours.Materials
{
    /// <summary>
    ///     Material scroller? I hardly know 'er.
    /// </summary>
    public class MaterialScroller : MonoBehaviour
    {
        /// <summary>
        ///     Material to scroll.
        /// </summary>
        public Material ScrollingMat { get; internal set; } = null!;

        /// <summary>
        ///     Current frame being displayed (can be used to start at a specific frame).
        /// </summary>
        [Header("Material Scroller")]
        [Tooltip("Current frame being displayed (can be used to start at a specific frame).")]
        private int currentFrame = 1;

        /// <summary>
        ///     Total number of frames in the texture.
        /// </summary>
        [Tooltip("Total number of frames in the texture.")]
        public int frames = 60;

        /// <summary>
        ///     Axis to scroll across.
        /// </summary>
        [Tooltip("Axis to scroll across.")]
        public char offsetAxis = 'X';

        /// <summary>
        ///     Offset to apply every frame.
        /// </summary>
        [Tooltip("Offset to apply every frame.")]
        public float offsetPerFrame = 0.0f;

        /// <summary>
        ///     ID of the exposed offset shader property.
        /// </summary>
        [Tooltip("ID of the exposed offset shader property.")]
        public int offsetID = 0;

        private void Awake()
        {
            offsetPerFrame = 1.0f / frames;
        }

        private void Start()
        {
            ScrollingMat.SetFloat("_Tiling" + offsetAxis, offsetPerFrame);
            offsetID = Shader.PropertyToID("_Offset" + offsetAxis);
        }

        private void FixedUpdate()
        {
            if (currentFrame > 60)
            {
                currentFrame = 1;
            }

            ScrollingMat.SetFloat(offsetID, currentFrame * offsetPerFrame);
            currentFrame++;
        }
    }
}