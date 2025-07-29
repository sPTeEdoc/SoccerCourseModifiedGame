using System;

public class MedicalStaff
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Nationality { get; set; }
    public string Role { get; set; } // E.g., "Head Physio", "Physiotherapist", "Doctor"
    public decimal WeeklyWage { get; set; }
    public double ContractYears { get; set; }

    /// <summary>
    /// Skill rating in medical treatment and injury prevention (1-20 scale).
    /// </summary>
    public int MedicalSkill { get; set; }

    /// <summary>
    /// Overall quality rating for the medical staff.
    /// </summary>
    public int OverallRating { get; set; }

    public MedicalStaff(string name, int age, string nationality, string role, decimal weeklyWage, double contractYears, int medicalSkill)
    {
        Name = name;
        Age = age;
        Nationality = nationality;
        Role = role;
        WeeklyWage = weeklyWage;
        ContractYears = contractYears;
        MedicalSkill = Math.Max(1, Math.Min(medicalSkill, 20)); // Clamp skill 1-20

        UpdateOverallRating(); // Calculate initial overall rating
    }

    public MedicalStaff Clone()
    {
        MedicalStaff newPhysio = new MedicalStaff(this.Name, this.Age, this.Nationality, this.Role, this.WeeklyWage,
            this.ContractYears, this.MedicalSkill);
        OverallRating = this.OverallRating; // Calculate initial overall rating
        return newPhysio;
    }

    // A simple overall rating for medical staff can just be their MedicalSkill
    private void UpdateOverallRating()
    {
        OverallRating = MedicalSkill;
    }
}