using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace itolib.Behaviours.Groupings
{
    /// <summary>
    ///     Represents a group of <c>Renderer</c> components to perform bulk operations on.
    /// </summary>
    public class RendererGroup : ComponentGroup<Renderer>
    {
        /// <summary>
        ///     All actions able to be performed on every <c>Renderer</c> component in the group.
        /// </summary>
        private enum RendererActions : byte
        {
            ReceiveShadows,
            SetShadowCastingMode,
            SetRenderingLayerMask = 15,
            BitwiseORRenderingLayerMask,
            BitwiseANDRenderingLayerMask
        }

        /// <inheritdoc/>
        protected override void PerformSingleAction(Renderer renderer, Enum actionID, object? parameter = null)
        {
            if (actionID is not RendererActions rendererActionID)
            {
                return;
            }

            switch (rendererActionID)
            {
                case RendererActions.ReceiveShadows:
                    renderer.receiveShadows = parameter is true;
                    break;
                case RendererActions.SetShadowCastingMode:
                    if (parameter is ShadowCastingMode mode)
                    {
                        renderer.shadowCastingMode = mode;
                    }
                    break;
                case RendererActions.SetRenderingLayerMask:
                    if (parameter != null)
                    {
                        renderer.renderingLayerMask = (uint)parameter;
                    }
                    break;
                case RendererActions.BitwiseORRenderingLayerMask:
                    if (parameter != null)
                    {
                        renderer.renderingLayerMask |= (uint)parameter;
                    }
                    break;
                case RendererActions.BitwiseANDRenderingLayerMask:
                    if (parameter != null)
                    {
                        renderer.renderingLayerMask &= (uint)parameter;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <inheritdoc/>
        protected override void EnableSingleComponent(Renderer renderer, bool enabled)
        {
            renderer.enabled = enabled;
        }

        /// <inheritdoc/>
        protected override void ToggleSingleComponent(Renderer renderer)
        {
            renderer.enabled = !renderer.enabled;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enabled"></param>
        public void ReceiveShadowsAll(bool enabled)
        {
            PerformGroupAction(RendererActions.ReceiveShadows, enabled);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="mode"></param>
        public void SetShadowCastingModeAll(int mode)
        {
            if (Enum.IsDefined(typeof(ShadowCastingMode), (sbyte)mode))
            {
                PerformGroupAction(RendererActions.SetShadowCastingMode, mode);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="layerMask"></param>
        public void SetRenderingLayerMask(int layerMask)
        {
            PerformGroupAction(RendererActions.SetRenderingLayerMask, (uint)layerMask);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="layerMask"></param>
        public void BitwiseORRenderingLayerMask(int layerMask)
        {
            PerformGroupAction(RendererActions.BitwiseORRenderingLayerMask, (uint)layerMask);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="layerMask"></param>
        public void BitwiseANDRenderingLayerMask(int layerMask)
        {
            PerformGroupAction(RendererActions.BitwiseANDRenderingLayerMask, (uint)layerMask);
        }
    }
}