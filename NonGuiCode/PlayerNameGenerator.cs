using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class PlayerNameGenerator
{
    private static PlayerNameGenerator m_instance = null;

    public static PlayerNameGenerator Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new PlayerNameGenerator();
            }
            return m_instance;
        }
    }

    private List<string> weightedFirstNames = new List<string>();
    private List<string> weightedLastNames = new List<string>();
    private Random random = new Random();

    /// <summary>
    /// Constructor: Initializes the generator by building a weighted list
    /// of nations based on their counts.
    /// </summary>
    public PlayerNameGenerator()
    {
        BuildWeightedList("MaleFirstNames.csv", weightedFirstNames);
        BuildWeightedList("LastNames.csv", weightedLastNames);
    }

    /// <summary>
    /// Builds a weighted list where each nation appears a number of times
    /// proportional to its frequency in the dataset. This makes random selection
    /// follow the observed distribution.
    /// </summary>
    private void BuildWeightedList(string csv, List<string> names)
    {
        double runningCount = 0;
        using (StreamReader readtext = new StreamReader(@"..\\..\\Data\\" + csv))
        {
            string readText = readtext.ReadLine();
            // Clear any existing weighted nations
            names.Clear();
            while (readText != null)
            {
                string[] stringofdata = readText.Split(',');
                string name = stringofdata[0];
                double count = 0;
                Double.TryParse(stringofdata[1], out count);
                count /= 1000;
                for (int i = 0; i < Math.Max((int)Math.Round(count, 0), 1); i++)
                {
                    names.Add(ToTitleCaseCustom(name));
                }
                //runningCount += count;
                readText = readtext.ReadLine();
            }
        }
        // Shuffle the list to ensure true randomness across the weighted items.
        // This shuffling is good practice for explicit randomness when iterating.
        ShuffleList(names);
    }

    public string ToTitleCaseCustom(string input)
    {
        // Handle null, empty, or whitespace inputs
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Use StringBuilder for efficient string concatenation, especially with many words.
        StringBuilder resultBuilder = new StringBuilder();

        // Split the input string into words based on space characters.
        // StringSplitOptions.RemoveEmptyEntries ensures no empty strings are returned for multiple spaces.
        string[] words = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];

            // Ensure the word is not empty after splitting (though RemoveEmptyEntries helps)
            if (word.Length > 0)
            {
                // Convert the whole word to lowercase first to handle mixed-case inputs like "jOhN"
                string lowerWord = word.ToLower();

                // Capitalize the first character
                char firstChar = char.ToUpper(lowerWord[0]);

                // Append the capitalized first character and the rest of the word (which is now lowercase)
                resultBuilder.Append(firstChar);
                if (lowerWord.Length > 1)
                {
                    resultBuilder.Append(lowerWord.Substring(1));
                }
            }

            // Add a space after each word, except for the last one
            if (i < words.Length - 1)
            {
                resultBuilder.Append(' ');
            }
        }

        return resultBuilder.ToString();
    }

    /// <summary>
    /// Shuffles a list using the Fisher-Yates algorithm.
    /// </summary>
    /// <param name="list">The list of strings to shuffle.</param>
    private void ShuffleList(List<string> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            string value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Determines and returns a random nation for a new player based on the
    /// weighted probability of nations in the dataset.
    /// </summary>
    /// <returns>A string representing the randomly selected nation.</returns>
    public string GenerateRandomName()
    {
        if (weightedFirstNames.Count == 0)
        {
            throw new InvalidOperationException("No names to generate from. Ensure names are populated.");
        }

        // Pick a random index from the weighted list
        int randomIndex = random.Next(weightedFirstNames.Count);

        int randomxIndex2 = random.Next(weightedLastNames.Count);

        // Return the nation at that random index
        return weightedFirstNames[randomIndex] + " " + weightedLastNames[randomxIndex2];
    }
}