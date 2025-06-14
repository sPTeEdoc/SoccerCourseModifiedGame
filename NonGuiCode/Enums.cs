using Godot;
using System;

public class Enums
{
	public enum Positions
	{
		Defender,
		MidFielder,
		Forward,
		Goalkeeper,
		CentralAttackingMidfielder,
		CenterBack,
		CentralDefendingMidfielder,
		CentralMidfielder,
		LeftBack,
		LeftMidfielder,
		LeftWingForward,
		RightBack,
		RightMidfielder,
		RightWingForward,
		Striker
	}

	public enum Ability
	{
		BelowAverage = -1,
		Average = 0,
		AboveAverage = 1,
		Amazing = 2
	}

	public enum Characteristic
	{
		pace = 1,
		dribbling = 2,
		isStrong = 3,
		isHard = 4,
		passing = 5,
		agility = 6
	}

	public enum RefAttitude
	{
		Friendly = 1,
		Neutral = 2,
		Hostile = 3
	}

	public enum SpecialShot
	{
		Header = 0,
		FreeKick = 1
	}

	public enum WeatherIssue
	{
		None
		, Light
		, Moderate
		, Heavy
	}

	public enum TierDifference
	{
		None
		, HomeSuperior
		, AwaySuperior
	}

	public enum YellowCardRegulations
	{
		EPL
	}

	public enum DialogResult
	{
		Yes = 1,
		No = 2,
		None = 0		
	}
}
