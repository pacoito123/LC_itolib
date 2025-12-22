namespace itolib.Extensions
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public static class SelectableLevelExtensions
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static string GetNumberlessPlanetName(this SelectableLevel level)
        {
            if (level.PlanetName.Length == 0)
            {
                return string.Empty;
            }

            bool skippedNumbers = false;
            char[] characters = new char[level.PlanetName.Length];

            for (int i = 0; i < characters.Length; i++)
            {
                char c = level.PlanetName[i];

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

                characters[i] = c;
            }

            return new string(characters).Trim('\0');
        }
    }
}