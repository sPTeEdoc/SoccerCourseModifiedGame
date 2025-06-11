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

    PackedScene scene = null;

    public override void _Ready()
    {
        monthYearLabel = GetNode<Label>("%MonthYearLabel");
        columnsBox = (HBoxContainer)GetNode("%ColumnsBox");
        scene = ResourceLoader.Load<PackedScene>("res://DateLabel.tscn");
        SetCalendar();
    }

    private void SetCalendar()
    {
        SetMonthYearLabel();

        DateTime firstOfMonthDate = GetFirstOfMonth(selectedDate);
        long firstOfMonthUnixTime = GetUnixTimeFromDateTime(firstOfMonthDate);

        int startWeekday = (int)firstOfMonthDate.DayOfWeek - 1;
        if (startWeekday == -1) startWeekday = 7;

        DateTime startDate = GetDateTimeFromUnixTime(firstOfMonthUnixTime - DAY_IN_UNIX_TIME * startWeekday);
        DateTime calculateDate = startDate;

        for (int i = 0; i < 5 * 7; i++)
        {
            CreateLabel(calculateDate, i % 7);
            calculateDate = GetNextDay(calculateDate);
        }

        if (selectedDate.Month != calculateDate.Month) return;

        for (int i = 0; i < 7; i++)
        {
            CreateLabel(calculateDate, i % 7);
            calculateDate = GetNextDay(calculateDate);
        }
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

    private void CreateLabel(DateTime date, int index)
    {
        var dateLabel = (DateLabel)scene.Instantiate();
        dateLabel.dateOfDay = date;

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
