using FunnyOldGameRedux;
using Godot;
using System;

public partial class Calendar : Control
{
    private Label monthYearLabel;
    private HBoxContainer columnsBox;
    private static readonly string[] MONTH_NAMES = { "January", "February", "March", "April", "May",
        "June", "July", "August", "September", "October", "November", "December" };
    private const int DAY_IN_UNIX_TIME = 86400;
    private DateTime selectedDate = Season.Instance.startOfSeasonDate;
    private DateTime finalGameDate;
    private TextureRect previousMonthButton;
    private TextureRect nextMonthButton;

    PackedScene scene = null;

    public override void _Ready()
    {
        monthYearLabel = GetNode<Label>("%MonthYearLabel");
        columnsBox = (HBoxContainer)GetNode("%ColumnsBox");
        previousMonthButton = GetNode<TextureRect>("VBoxContainer/MarginContainer/TextureRect");
        nextMonthButton = GetNode<TextureRect>("VBoxContainer/MarginContainer/TextureRect2");
        finalGameDate = new DateTime(selectedDate.Year + 1, 7, 31);
        scene = ResourceLoader.Load<PackedScene>("res://DateLabel.tscn");
        SetCalendar();
    }

    private void SetCalendar()
    {
        SetMonthYearLabel();

        DateTime firstOfMonthDate = GetFirstOfMonth(selectedDate);
        long firstOfMonthUnixTime = GetUnixTimeFromDateTime(firstOfMonthDate);

        int startWeekday = (int)firstOfMonthDate.DayOfWeek;
        if (startWeekday == -1) startWeekday = 7;


        DateTime startDate = GetDateTimeFromUnixTime(firstOfMonthUnixTime - DAY_IN_UNIX_TIME * startWeekday);
        DateTime calculateDate = startDate;

        for (int i = 0; i < 5 * 7; i++)
        {
            CreateLabel(calculateDate, i % 7, selectedDate.Month == calculateDate.Month,
                DetermineLogo(calculateDate));
            calculateDate = GetNextDay(calculateDate);
        }

        for (int i = 0; i < 7; i++)
        {
            CreateLabel(calculateDate, i % 7, selectedDate.Month == calculateDate.Month,
                DetermineLogo(calculateDate));
            calculateDate = GetNextDay(calculateDate);
        }

        if (selectedDate.Month == Season.Instance.startOfSeasonDate.Month && (selectedDate.Year == Season.Instance.startOfSeasonDate.Year))
            previousMonthButton.Visible = false;
        else
            previousMonthButton.Visible = true;

        if (selectedDate.Month == finalGameDate.Month && (selectedDate.Year == finalGameDate.Year))
            nextMonthButton.Visible = false;
        else
            nextMonthButton.Visible = true;
    }

    private string DetermineLogo(DateTime dt)
    {
        if (dt == Season.Instance.seasonGameDate)
        {
            return "leagueday";
        }

        if (Season.Instance.countriesLeagueMatchesScheduledOnDay.ContainsKey(dt))
        {
            return "leagueday";
        }

        if (Season.Instance.countriesCupMatchesScheduledOnDay.ContainsKey(dt))
        {
            return "cup_day";
        }

        for (int j = 0; j < Season.Instance.promotionPlayoffs.Count; j++)
        {
            if (Season.Instance.promotionPlayoffs[j].ContainsKey(dt))
            {
                return "leagueday";
            }
        }
        return "";
    }
    private void SetMonthYearLabel()
    {
        monthYearLabel.Text = MONTH_NAMES[selectedDate.Month - 1] + " " + selectedDate.Year.ToString();
    }

    private DateTime GetFirstOfMonth(DateTime date)
    {
        date = new DateTime(date.Year, date.Month, 1);
        return date;
    }

    private void CreateLabel(DateTime date, int index, bool HideLabel = false,
        string textureString = "")
    {
        var dateLabel = (DateLabel)scene.Instantiate();
        dateLabel.dateOfDay = date;
        dateLabel.Visible = HideLabel;
        dateLabel.textureString = textureString;

        columnsBox.GetChildren()[index].AddChild(dateLabel);
    }

    private DateTime GetNextDay(DateTime date)
    {
        return date.AddSeconds(DAY_IN_UNIX_TIME);
    }

    private void OnPreviousMonthButtonPressed()
    {
        selectedDate = selectedDate.AddMonths(-1);
        RefreshCalendar();
    }

    private void OnNextMonthButtonPressed()
    {
        selectedDate = selectedDate.AddMonths(1);
        RefreshCalendar();
    }

    private void RefreshCalendar()
    {
        if (selectedDate.Month > 12)
        {
            selectedDate = new DateTime(selectedDate.Year + 1, 1, selectedDate.Day);
        }
        else if (selectedDate.Month < 1)
        {
            selectedDate = new DateTime(selectedDate.Year - 1, 12, selectedDate.Day);
        }

        foreach (var column in columnsBox.GetChildren())
        {
            foreach (var node in column.GetChildren())
            {
                if (node is Label) continue;

                node.QueueFree();
            }
        }

        SetCalendar();
    }

    private long GetUnixTimeFromDateTime(DateTime dateTime)
    {
        return (long)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
    }

    private DateTime GetDateTimeFromUnixTime(long unixTime)
    {
        return new DateTime(1970, 1, 1).AddSeconds(unixTime);
    }
}
