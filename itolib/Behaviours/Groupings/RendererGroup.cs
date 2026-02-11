using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace itolib.Behaviours.Groupings
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class RendererGroup : ComponentGroup<Renderer>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        private enum RendererActions
        {
            ReceiveShadows,
            SetShadowCastingMode
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="actionID"></param>
        /// <param name="parameter"></param>
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
                default:
                    break;
            }
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
            if (Enum.IsDefined(typeof(ShadowCastingMode), mode))
            {
                PerformGroupAction(RendererActions.SetShadowCastingMode, mode);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="enabled"></param>
        protected override void EnableSingleComponent(Renderer renderer, bool enabled)
        {
            renderer.enabled = enabled;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="renderer"></param>
        protected override void ToggleSingleComponent(Renderer renderer)
        {
            renderer.enabled = !renderer.enabled;
        }
    }
}