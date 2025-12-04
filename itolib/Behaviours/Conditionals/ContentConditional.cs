using DunGen.Graph;
using itolib.Enums;
using itolib.Util;
using UnityEngine;

namespace itolib.Behaviours.Conditionals
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ContentConditional : BaseConditional<ContentCategoryType>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Content Conditional")]
        [Tooltip("")]
        [SerializeField] private ContentCategoryType contentToSearch = ContentCategoryType.None;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="undo"></param>
        public override void ApplyConditional(bool undo)
        {
            ApplyConditional(contentToSearch);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="undo"></param>
        public override void ApplyConditional(ContentCategoryType contentType, bool undo)
        {
            switch (contentType)
            {
                case ContentCategoryType.Plugin:
                    SearchPlugins();
                    break;
                case ContentCategoryType.Item:
                    SearchItems();
                    break;
                case ContentCategoryType.Enemy:
                    SearchEnemies();
                    break;
                case ContentCategoryType.Level:
                    SearchLevels();
                    break;
                case ContentCategoryType.Dungeon:
                    SearchDungeons();
                    break;
                case ContentCategoryType.None:
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void SearchPlugins()
        {
            for (int i = 0; i < conditionalOverrides?.Length; i++)
            {
                ConditionalOverride overrideEntry = conditionalOverrides[i];

                if (BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(overrideEntry.nameToSearch, out _))
                {
                    overrideEntry.Apply();

                    return;
                }
                else if (overrideEntry.alsoAppliesTo?.Length > 0)
                {
                    for (int j = 0; j < overrideEntry.alsoAppliesTo.Length; j++)
                    {
                        if (BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(overrideEntry.alsoAppliesTo[j], out _))
                        {
                            overrideEntry.Apply();

                            return;
                        }
                    }
                }

                overrideEntry.onConditionalFail.Invoke();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void SearchItems()
        {
            for (int i = 0; i < conditionalOverrides?.Length; i++)
            {
                ConditionalOverride overrideEntry = conditionalOverrides[i];

                if (SearchContent.TryFindItem(out Item _, overrideEntry.nameToSearch))
                {
                    overrideEntry.Apply();

                    continue;
                }

                overrideEntry.onConditionalFail.Invoke();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void SearchEnemies()
        {
            for (int i = 0; i < conditionalOverrides?.Length; i++)
            {
                ConditionalOverride overrideEntry = conditionalOverrides[i];

                if (true || SearchContent.TryFindEnemy(out EnemyType _, overrideEntry.nameToSearch))
                {
                    overrideEntry.Apply();

                    continue;
                }

                overrideEntry.onConditionalFail.Invoke();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void SearchLevels()
        {
            for (int i = 0; i < conditionalOverrides?.Length; i++)
            {
                ConditionalOverride overrideEntry = conditionalOverrides[i];

                if (true || SearchContent.TryFindLevel(out SelectableLevel _, overrideEntry.nameToSearch))
                {
                    overrideEntry.Apply();

                    continue;
                }

                overrideEntry.onConditionalFail.Invoke();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void SearchDungeons()
        {
            for (int i = 0; i < conditionalOverrides?.Length; i++)
            {
                ConditionalOverride overrideEntry = conditionalOverrides[i];

                if (true || SearchContent.TryFindDungeon(out DungeonFlow _, overrideEntry.nameToSearch))
                {
                    overrideEntry.Apply();

                    continue;
                }

                overrideEntry.onConditionalFail.Invoke();
            }
        }
    }
}