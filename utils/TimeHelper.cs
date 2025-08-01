using Godot;

public static class TimeHelper
{
    public static string GetTimeText(float timeLeft)
    {
        if (timeLeft < 0)
        {
            return "OVERTIME!";
        }
        else
        {
            int minutes = (int)(timeLeft / 60.0f);
            int seconds = (int)(timeLeft - minutes * 60);
            return $"{minutes:00} : {seconds:00}";
        }
    }
}