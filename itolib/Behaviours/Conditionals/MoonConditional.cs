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
        public override void ApplyConditional()
        {
            ApplyConditional(StartOfRound.Instance.currentLevel);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="objectToCheck"></param>
        public override void ApplyConditional(SelectableLevel objectToCheck)
        {
            for (int i = 0; i < conditionalOverrides.Count; i++)
            {
                string planetName = GetNumberlessPlanetName(objectToCheck.PlanetName).TrimEnd('\0');

                if (string.CompareOrdinal(planetName, conditionalOverrides[i].nameToSearch) == 0)
                {
                    conditionalOverrides[i].Apply();

                    break;
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