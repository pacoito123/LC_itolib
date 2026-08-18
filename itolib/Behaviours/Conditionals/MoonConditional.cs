using itolib.Extensions;

namespace itolib.Behaviours.Conditionals
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class MoonConditional : BaseConditional<SelectableLevel>
    {
        /// <inheritdoc/>
        public override void ApplyConditional(bool undo)
        {
            if (StartOfRound.Instance != null)
            {
                ApplyConditional(StartOfRound.Instance.currentLevel, undo);
            }
        }

        /// <inheritdoc/>
        public override void ApplyConditional(SelectableLevel objectToCheck, bool undo)
        {
            string planetName = objectToCheck.GetNumberlessPlanetName();

            for (int i = 0; i < conditionalOverrides?.Length; i++)
            {
                ConditionalOverride overrideEntry = conditionalOverrides[i];

                if (string.Equals(overrideEntry.nameToSearch, planetName, System.StringComparison.OrdinalIgnoreCase))
                {
                    overrideEntry.Apply(undo);

                    continue;
                }
                else if (overrideEntry.alsoAppliesTo?.Length > 0)
                {
                    for (int j = 0; j < overrideEntry.alsoAppliesTo.Length; j++)
                    {
                        if (string.Equals(overrideEntry.alsoAppliesTo[j], planetName, System.StringComparison.OrdinalIgnoreCase))
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