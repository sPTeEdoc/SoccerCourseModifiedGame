using Godot;
using System;
using System.Collections.Generic;

public class League
{
		public string m_LeagueName;
		public int tier;
		public String LeagueName
		{
			get
			{
				return m_LeagueName;
			}
		}

		public List<Team> teams = new List<Team>();

		public League(string lgName)
		{
			this.m_LeagueName = lgName;
		}
}
