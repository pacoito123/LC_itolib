using UnityEngine;

namespace itolib.Behaviours.Effects
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ShakeEffect : MonoBehaviour
    {
        /// <summary>
        ///     Hash of the trigger parameter to activate <c>ScreenShakeType.Small</c> camera shaking.
        /// </summary>
        private static readonly int smallShakeID = Animator.StringToHash("smallShake");

        /// <summary>
        ///     Hash of the trigger parameter to activate <c>ScreenShakeType.Big</c> camera shaking.
        /// </summary>
        private static readonly int bigShakeID = Animator.StringToHash("bigShake");

        /// <summary>
        ///     Hash of the trigger parameter to activate <c>ScreenShakeType.Long</c> camera shaking.
        /// </summary>
        private static readonly int longShakeID = Animator.StringToHash("longShake");

        /// <summary>
        ///     Hash of the trigger parameter to activate <c>ScreenShakeType.VeryStrong</c> camera shaking.
        /// </summary>
        private static readonly int veryStrongShakeID = Animator.StringToHash("veryStrongShake");

        /// <summary>
        ///     Hash of the bool parameter to toggle constant camera shaking.
        /// </summary>
        private static readonly int shakingConstantID = Animator.StringToHash("ShakingConstant");

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Shake Effect")]
        [Tooltip("")]
        [SerializeField] private ScreenShakeType shakeType;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="shakeType"></param>
        public void ShakeScreen(int shakeType)
        {
            ShakeScreen((ScreenShakeType)Mathf.Clamp(shakeType, 0, 4));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="shakeType"></param>
        public void ShakeScreen(ScreenShakeType shakeType)
        {
            SwitchShakeType(shakeType);
            ShakeScreen();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ShakeScreen()
        {
            if (HUDManager.Instance == null || HUDManager.Instance.playerScreenShakeAnimator == null)
            {
                return;
            }

            if (shakeType is ScreenShakeType.Constant)
            {
                HUDManager.Instance.playerScreenShakeAnimator.SetBool(shakingConstantID, true);

                return;
            }

            int shakeID = shakeType switch
            {
                ScreenShakeType.Small => smallShakeID,
                ScreenShakeType.Big => bigShakeID,
                ScreenShakeType.Long => longShakeID,
                ScreenShakeType.VeryStrong => veryStrongShakeID,
                ScreenShakeType.Constant or _ => 0,
            };

            if (shakeID != 0)
            {
                HUDManager.Instance.playerScreenShakeAnimator.SetTrigger(shakeID);
                HUDManager.Instance.playerScreenShakeAnimator.SetBool(shakingConstantID, false);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="shakeType"></param>
        public void SwitchShakeType(int shakeType)
        {
            SwitchShakeType((ScreenShakeType)Mathf.Clamp(shakeType, 0, 4));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="shakeType"></param>
        public void SwitchShakeType(ScreenShakeType shakeType)
        {
            this.shakeType = shakeType;
        }
    }
}