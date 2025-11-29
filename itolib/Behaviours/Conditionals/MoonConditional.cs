using itolib.Extensions;

namespace itolib.Behaviours.Conditionals
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class MoonConditional : BaseConditional<SelectableLevel>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="undo"></param>
        public override void ApplyConditional(bool undo)
        {
            if (StartOfRound.Instance != null)
            {
                ApplyConditional(StartOfRound.Instance.currentLevel, undo);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="objectToCheck"></param>
        /// <param name="undo"></param>
        public override void ApplyConditional(SelectableLevel objectToCheck, bool undo)
        {
            string planetName = objectToCheck.GetNumberlessPlanetName();

            for (int i = 0; i < conditionalOverrides?.Length; i++)
            {
                ConditionalOverride overrideEntry = conditionalOverrides[i];

                if (planetName.CompareOrdinal(overrideEntry.nameToSearch))
                {
                    overrideEntry.Apply(undo);

                    continue;
                }
                else if (overrideEntry.alsoAppliesTo?.Length > 0)
                {
                    for (int j = 0; j < overrideEntry.alsoAppliesTo.Length; j++)
                    {
                        if (planetName.CompareOrdinal(overrideEntry.alsoAppliesTo[j]))
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