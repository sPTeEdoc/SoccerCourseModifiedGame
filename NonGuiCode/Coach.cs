using FunnyOldGame; // For Dictionary
using System;
using System.Collections.Generic;

public class Coach
{
    // --- Core Information ---
    public string Name { get; set; }
    public int Age { get; set; }
    public string Nationality { get; set; }
    public string Role { get; set; } // E.g., "Head Coach", "Fitness Coach", "Goalkeeping Coach", "Assistant Manager"

    // --- Contract Details (Simplified for Coaches) ---
    public decimal WeeklyWage { get; set; }
    public double ContractYears { get; set; } // Years remaining on their contract

    // --- Training Abilities ---
    /// <summary>
    /// Dictionary of the coach's skill ratings (1-20 or 1-100 scale) for various training categories.
    /// Higher skill means more effective training in that area.
    /// </summary>
    public Dictionary<Enums.TrainingCategory, int> SkillRatings { get; set; }

    /// <summary>
    /// A coach's overall quality rating (e.g., average of their top skills, or a separate assigned value).
    /// Used for general comparison and perhaps initial wage demands.
    /// </summary>
    public int OverallRating { get; set; }

    // --- Constructor ---
    public Coach(string name, int age, string nationality, string role, decimal weeklyWage, double contractYears)
    {
        Name = name;
        Age = age;
        Nationality = nationality;
        Role = role;
        WeeklyWage = weeklyWage;
        ContractYears = contractYears;

        // Initialize SkillRatings dictionary
        SkillRatings = new Dictionary<Enums.TrainingCategory, int>();

        // Set default/placeholder OverallRating (will likely be calculated based on skills)
        OverallRating = 1; // Default, will be updated.
    }

    public Coach Clone()
    {
        Coach newCoach = new Coach(this.Name, this.Age, this.Nationality, this.Role, this.WeeklyWage, this.ContractYears);
        foreach (KeyValuePair<Enums.TrainingCategory, int> entry in this.SkillRatings)
        {
            newCoach.SkillRatings.Add(entry.Key, entry.Value);
        }

        // Set default/placeholder OverallRating (will likely be calculated based on skills)
        newCoach.OverallRating = this.OverallRating; // Default, will be updated.

        return newCoach;
    }

    // --- Methods ---

    /// <summary>
    /// Sets a skill rating for a specific training category.
    /// Clamps the rating between 1 and 20 (or your chosen scale).
    /// </summary>
    public void SetSkillRating(Enums.TrainingCategory category, int rating)
    {
        // Clamp the rating to a reasonable range, e.g., 1-20 for Football Manager style, or 1-100 for a broader scale.
        // Let's use 1-20 for now.
        SkillRatings[category] = Math.Max(1, Math.Min(rating, 20));
        UpdateOverallRating(); // Recalculate overall when skills change
    }

    /// <summary>
    /// Calculates and updates the coach's overall rating based on their role and skill ratings.
    /// This makes the overall rating reflect their primary expertise.
    /// Updated for pre-LINQ compatibility.
    /// </summary>
    private void UpdateOverallRating()
    {
        if (SkillRatings == null || SkillRatings.Count == 0)
        {
            OverallRating = 1; // Default low rating if no skills are set
            return;
        }

        double weightedSum = 0;
        int totalWeight = 0;

        switch (Role)
        {
            case "Fitness Coach":
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.Fitness) ? SkillRatings[Enums.TrainingCategory.Fitness] * 5 : 1 * 5;
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.Mental) ? SkillRatings[Enums.TrainingCategory.Mental] * 1 : 1 * 1;
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.Technical) ? SkillRatings[Enums.TrainingCategory.Technical] * 0.5 : 1 * 0.5;
                totalWeight = 6;
                break;
            case "Attacking Coach":
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.Attacking) ? SkillRatings[Enums.TrainingCategory.Attacking] * 5 : 1 * 5;
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.Technical) ? SkillRatings[Enums.TrainingCategory.Technical] * 2 : 1 * 2;
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.Mental) ? SkillRatings[Enums.TrainingCategory.Mental] * 1 : 1 * 1;
                totalWeight = 8;
                break;
            case "Defending Coach":
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.Defending) ? SkillRatings[Enums.TrainingCategory.Defending] * 5 : 1 * 5;
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.Tactical) ? SkillRatings[Enums.TrainingCategory.Tactical] * 2 : 1 * 2;
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.Mental) ? SkillRatings[Enums.TrainingCategory.Mental] * 1 : 1 * 1;
                totalWeight = 8;
                break;
            case "Technical Coach":
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.Technical) ? SkillRatings[Enums.TrainingCategory.Technical] * 5 : 1 * 5;
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.Attacking) ? SkillRatings[Enums.TrainingCategory.Attacking] * 1 : 1 * 1;
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.Defending) ? SkillRatings[Enums.TrainingCategory.Defending] * 1 : 1 * 1;
                totalWeight = 7;
                break;
            case "Mental Coach":
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.Mental) ? SkillRatings[Enums.TrainingCategory.Mental] * 5 : 1 * 5;
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.Tactical) ? SkillRatings[Enums.TrainingCategory.Tactical] * 1 : 1 * 1;
                totalWeight = 6;
                break;
            case "Goalkeeping Coach":
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.Goalkeeping) ? SkillRatings[Enums.TrainingCategory.Goalkeeping] * 10 : 1 * 10;
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.Mental) ? SkillRatings[Enums.TrainingCategory.Mental] * 1 : 1 * 1;
                totalWeight = 11;
                break;
            case "Youth Development Coach":
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.YouthDevelopment) ? SkillRatings[Enums.TrainingCategory.YouthDevelopment] * 5 : 1 * 5;
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.Mental) ? SkillRatings[Enums.TrainingCategory.Mental] * 1 : 1 * 1;
                weightedSum += SkillRatings.ContainsKey(Enums.TrainingCategory.Technical) ? SkillRatings[Enums.TrainingCategory.Technical] * 1 : 1 * 1;
                totalWeight = 7;
                break;

            case "Assistant Manager":
                // --- PRE-LINQ WAY to get top skills ---
                List<int> allSkills = new List<int>(SkillRatings.Values);
                allSkills.Sort(); // Sorts in ascending order
                allSkills.Reverse(); // Reverses to descending order

                double assistantAvg = 0;
                int skillsToAverage = Math.Min(3, allSkills.Count); // Average top 3, or fewer if not enough skills
                if (skillsToAverage > 0)
                {
                    for (int i = 0; i < skillsToAverage; i++)
                    {
                        assistantAvg += allSkills[i];
                    }
                    weightedSum = assistantAvg / skillsToAverage;
                    totalWeight = 1; // Treat as a direct average
                }
                else
                {
                    totalWeight = 0;
                }
                break;
            default: // General coach or undefined role
                if (SkillRatings.Count > 0)
                {
                    // Calculate simple average manually
                    double sumOfSkills = 0;
                    foreach (int skill in SkillRatings.Values)
                    {
                        sumOfSkills += skill;
                    }
                    weightedSum = sumOfSkills / SkillRatings.Count;
                    totalWeight = 1; // Treat as a direct average
                }
                else
                {
                    totalWeight = 0;
                }
                break;
        }

        if (totalWeight > 0)
        {
            OverallRating = (int)Math.Round(weightedSum / totalWeight);
        }
        else
        {
            OverallRating = 1; // Fallback
        }

        OverallRating = Math.Max(1, Math.Min(OverallRating, 20));
    }
}