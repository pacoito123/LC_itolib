using UnityEngine;

namespace itolib.Behaviours.Materials
{
    /// <summary>
    ///     Material scroller? I hardly know 'er.
    /// </summary>
    public class MaterialScroller : MonoBehaviour
    {
        /// <summary>
        ///     The material to scroll.
        /// </summary>
        /// <remarks><b>NOTE:</b> A custom shader with both a '_TilingX/Y' and an '_OffsetX/Y' parameter is required.</remarks>
        [Header("Material Scroller")]
        [Tooltip("The material to scroll. NOTE: A custom shader with both a '_Tiling' and an '_Offset' parameter is required.")]
        public Material scrollingMat = null!;

        /// <summary>
        ///     Current frame being displayed (can be used to start at a specific frame).
        /// </summary>
        [Tooltip("Current frame being displayed (can be used to start at a specific frame).")]
        public int currentFrame = 1;

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
            scrollingMat.SetFloat("_Tiling" + offsetAxis, offsetPerFrame);
            offsetID = Shader.PropertyToID("_Offset" + offsetAxis);
        }

        private void FixedUpdate()
        {
            if (currentFrame > 60)
            {
                currentFrame = 1;
            }

            scrollingMat.SetFloat(offsetID, currentFrame * offsetPerFrame);
            currentFrame++;
        }
    }
}