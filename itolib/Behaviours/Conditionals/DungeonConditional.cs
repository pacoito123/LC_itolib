using DunGen.Graph;
using itolib.Extensions;

namespace itolib.Behaviours.Conditionals
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class DungeonConditional : BaseConditional<DungeonFlow>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="undo"></param>
        public override void ApplyConditional(bool undo)
        {
            if (RoundManager.Instance != null && RoundManager.Instance.dungeonGenerator != null && RoundManager.Instance.dungeonGenerator.Generator != null
                && RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow != null)
            {
                ApplyConditional(RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow, undo);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="objectToCheck"></param>
        /// <param name="undo"></param>
        public override void ApplyConditional(DungeonFlow objectToCheck, bool undo)
        {
            string dungeonName = objectToCheck.name;

            for (int i = 0; i < conditionalOverrides?.Length; i++)
            {
                ConditionalOverride overrideEntry = conditionalOverrides[i];

                if (dungeonName.CompareOrdinal(overrideEntry.nameToSearch))
                {
                    overrideEntry.Apply(undo);

                    continue;
                }
                else if (overrideEntry.alsoAppliesTo?.Length > 0)
                {
                    for (int j = 0; j < overrideEntry.alsoAppliesTo.Length; j++)
                    {
                        if (dungeonName.CompareOrdinal(overrideEntry.alsoAppliesTo[j]))
                        {
                            overrideEntry.Apply(undo);

                            continue;
                        }
                    }
                }

                overrideEntry.onConditionalFail.Invoke();
            }
        }
    }
}