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
            ApplyConditional(StartOfRound.Instance.currentLevel);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="objectToCheck"></param>
        /// <param name="undo"></param>
        public override void ApplyConditional(SelectableLevel objectToCheck, bool undo)
        {
            for (int i = 0; i < conditionalOverrides.Count; i++)
            {
                string planetName = GetNumberlessPlanetName(objectToCheck.PlanetName).TrimEnd('\0');

                if (planetName.CompareOrdinal(conditionalOverrides[i].nameToSearch))
                {
                    conditionalOverrides[i].Apply(undo);

                    return;
                }
                else if (conditionalOverrides[i].alsoAppliesTo.Count > 0)
                {
                    for (int j = 0; j < conditionalOverrides[i].alsoAppliesTo.Count; j++)
                    {
                        if (planetName.CompareOrdinal(conditionalOverrides[i].alsoAppliesTo[j]))
                        {
                            conditionalOverrides[i].Apply(undo);

                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="levelName"></param>
        /// <returns></returns>
        public static string GetNumberlessPlanetName(string levelName)
        {
            if (levelName.Length == 0)
            {
                return string.Empty;
            }

            char[] characters = new char[levelName.Length];

            bool skippedNumbers = false;
            int index = 0;

            foreach (char c in levelName)
            {
                if (!skippedNumbers)
                {
                    if (!char.IsLetter(c))
                    {
                        continue;
                    }
                    else
                    {
                        skippedNumbers = true;
                    }
                }

                characters[index++] = c;
            }

            return new(characters);
        }
    }
}