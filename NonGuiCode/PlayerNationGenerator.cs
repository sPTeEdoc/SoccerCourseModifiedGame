using System;
using System.Collections.Generic;
using System.Linq;

public class PlayerNationGenerator
{
    private static PlayerNationGenerator m_instance = null;

    public static PlayerNationGenerator Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new PlayerNationGenerator();
            }
            return m_instance;
        }
    }

    // This dictionary will store the nation and its count from your CSV data.
    // Updated to include all nations and their weights from the provided data.
    private Dictionary<string, int> nationCounts = new Dictionary<string, int>()
    {
        {"England", 1491},
        {"Germany", 1105},
        {"Spain", 893},
        {"Argentina", 861},
        {"France", 728},
        {"Italy", 511},
        {"United States", 413},
        {"Korea Republic", 408},
        {"Holland", 390},
        {"Norway", 377},
        {"Sweden", 365},
        {"China PR", 362},
        {"Brazil", 357},
        {"Republic of Ireland", 345},
        {"Poland", 340},
        {"Portugal", 334},
        {"Denmark", 326},
        {"Belgium", 292},
        {"Austria", 273},
        {"Romania", 266},
        {"Uruguay", 264},
        {"Scotland", 262},
        {"Saudi Arabia", 262},
        {"Australia", 245},
        {"Turkey", 241},
        {"Colombia", 226},
        {"Paraguay", 215},
        {"Switzerland", 210},
        {"India", 208},
        {"Chile", 160},
        {"Croatia", 151},
        {"Wales", 145},
        {"Nigeria", 124},
        {"Ghana", 122},
        {"Morocco", 119},
        {"Ecuador", 119},
        {"Bolivia", 108},
        {"Peru", 105},
        {"Côte d'Ivoire", 104},
        {"Serbia", 104},
        {"Senegal", 102},
        {"Venezuela", 102},
        {"Czech Republic", 93},
        {"Japan", 85},
        {"Greece", 84},
        {"Ukraine", 78},
        {"Canada", 69},
        {"Northern Ireland", 67},
        {"Mali", 63},
        {"Finland", 62},
        {"Cameroon", 61},
        {"Algeria", 58},
        {"Kosovo", 55},
        {"Albania", 55},
        {"Slovenia", 54},
        {"Iceland", 54},
        {"Bosnia and Herzegovina", 51},
        {"New Zealand", 49},
        {"Slovakia", 49},
        {"Congo DR", 43},
        {"Hungary", 41},
        {"Guinea", 36},
        {"North Macedonia", 33},
        {"Jamaica", 32},
        {"Gambia", 31},
        {"Tunisia", 28},
        {"Angola", 27},
        {"Mexico", 27},
        {"Georgia", 24},
        {"Cape Verde Islands", 22},
        {"Suriname", 21},
        {"Bulgaria", 21},
        {"Montenegro", 20},
        {"Israel", 20},
        {"Guinea-Bissau", 18},
        {"Russia", 18},
        {"Burkina Faso", 17},
        {"Luxembourg", 15},
        {"South Africa", 15},
        {"Cyprus", 15},
        {"Togo", 14},
        {"Kenya", 14},
        {"Costa Rica", 14},
        {"Sierra Leone", 14},
        {"Azerbaijan", 12},
        {"United Arab Emirates", 12},
        {"Zimbabwe", 12},
        {"Congo", 12},
        {"Netherlands", 12},
        {"Zambia", 11},
        {"Armenia", 11},
        {"Benin", 11},
        {"Panama", 11},
        {"Gabon", 10},
        {"Curaçao", 10},
        {"Iraq", 10},
        {"Haiti", 9},
        {"Syria", 9},
        {"Iran", 9},
        {"Estonia", 8},
        {"Grenada", 8},
        {"Moldova", 8},
        {"Equatorial Guinea", 7},
        {"Indonesia", 7},
        {"Honduras", 7},
        {"Trinidad and Tobago", 7},
        {"Egypt", 6},
        {"Latvia", 6},
        {"Mauritania", 6},
        {"Uganda", 5},
        {"Lithuania", 5},
        {"Guyana", 5},
        {"Central African Republic", 5},
        {"Chinese Taipei", 5},
        {"Burundi", 5},
        {"Madagascar", 5},
        {"El Salvador", 4},
        {"Liberia", 4},
        {"Malta", 4},
        {"Montserrat", 4},
        {"Guatemala", 4},
        {"Dominican Republic", 4},
        {"Hong Kong", 4},
        {"Uzbekistan", 4},
        {"Cuba", 4},
        {"Faroe Islands", 3},
        {"Antigua and Barbuda", 3},
        {"Mozambique", 3},
        {"St. Lucia", 3},
        {"Palestine", 3},
        {"Sri Lanka", 3},
        {"Libya", 3},
        {"Philippines", 3},
        {"Kazakhstan", 3},
        {"Tanzania", 3},
        {"Belarus", 3},
        {"Lebanon", 2},
        {"Barbados", 2},
        {"Korea DPR", 2},
        {"Malawi", 2},
        {"St. Kitts and Nevis", 2},
        {"Jordan", 2},
        {"Puerto Rico", 1},
        {"Andorra", 1},
        {"Bermuda", 1},
        {"Thailand", 1},
        {"Sudan", 1},
        {"Rwanda", 1},
        {"Namibia", 1},
        {"Somalia", 1},
        {"Chad", 1},
        {"Fiji", 1},
        {"Vanuatu", 1},
        {"Liechtenstein", 1}
    };

    private List<string> weightedNations = new List<string>();
    private Random random = new Random();

    /// <summary>
    /// Constructor: Initializes the generator by building a weighted list
    /// of nations based on their counts.
    /// </summary>
    public PlayerNationGenerator()
    {
        BuildWeightedList();
    }

    /// <summary>
    /// Builds a weighted list where each nation appears a number of times
    /// proportional to its frequency in the dataset. This makes random selection
    /// follow the observed distribution.
    /// </summary>
    private void BuildWeightedList()
    {
        // Clear any existing weighted nations
        weightedNations.Clear();

        // Iterate through each nation and its count
        foreach (var entry in nationCounts)
        {
            string nation = entry.Key;
            int count = entry.Value;

            // Add the nation to the list 'count' number of times.
            // This creates the weighted distribution.
            for (int i = 0; i < count; i++)
            {
                weightedNations.Add(nation);
            }
        }

        // Shuffle the list to ensure true randomness across the weighted items.
        // This shuffling is good practice for explicit randomness when iterating.
        ShuffleList(weightedNations);
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
    public string GenerateRandomNation()
    {
        if (weightedNations.Count == 0)
        {
            throw new InvalidOperationException("No nations available to generate from. Ensure nationCounts is populated.");
        }

        // Pick a random index from the weighted list
        int randomIndex = random.Next(weightedNations.Count);

        // Return the nation at that random index
        return weightedNations[randomIndex];
    }
}