using Godot;
using System;

public class Dice
{
	private static Dice m_instance = null;

		public Die d6;
		public Die d100;

		public Dice()
		{
			d6 = new Die(6);
			d100 = new Die(100);
		}

		public static Dice Instance
		{
			get
			{
				if (m_instance == null)
				{
					m_instance = new Dice();
				}
				return m_instance;
			}
		}
}
