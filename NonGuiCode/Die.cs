using Godot;
using System;
using System.Collections.Generic;

public class Die
{
	List<int> randomNubers = new List<int>();
		public int globalIndex = 0;

		public Die(int size) 
		{
			randomNubers = new List<int>();
			int numbersToGenerate = size * 1000;
			for (int i = 0; i < numbersToGenerate; i++)
			{
				randomNubers.Add(i % size);
			}

			Shuffle(ref randomNubers);
		}

		public int Roll() 
		{
			if (globalIndex >= randomNubers.Count)
			{
				globalIndex = 0;
				Shuffle(ref randomNubers);
			}
			int dieRoll = randomNubers[globalIndex++];
			//LoggingStuff.LogTheEvent("Dice roll: " + (dieRoll + 1));
			return dieRoll + 1;
		}

		private void Shuffle(ref List<int> randomNumbers)
		{
			Random r = new Random();
			//Step 1: For each unshuffled item in the collection
			for (int n = randomNumbers.Count - 1; n > 0; --n)
			{
				//Step 2: Randomly pick an item which has not been shuffled
				int k = r.Next(n + 1);

				//Step 3: Swap the selected item with the last "unstruck" letter in the collection
				int temp = randomNumbers[n];
				randomNumbers[n] = randomNumbers[k];
				randomNumbers[k] = temp;
			}
		}
}
