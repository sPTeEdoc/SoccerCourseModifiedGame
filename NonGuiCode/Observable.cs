using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Collections;

public class Observable
{
    protected ArrayList observers = new ArrayList();

    public void Attach(iObserver o)
    {
        observers.Add(o);
    }

    public void Detach(iObserver o)
    {
        observers.Remove(o);
    }

    public void NotifyAnnouncerChange(string eventText)
    {
        foreach (iObserver o in observers)
            o.UpdateAnnouncer(eventText);
    }

    public void NotifyPlayerChange()
    {
        foreach (iObserver o in observers)
            o.UpdateOnCamera();
    }

    public void NotifyTimeTicked(string timeTicked)
    {
        foreach (iObserver o in observers)
            o.UpdateTimeTicked(timeTicked);
    }

    public void NotifyHalfChanged()
    {
        foreach (iObserver o in observers)
            o.UpdateHalf();
    }

    public void NotifyScoreChanged()
    {
        foreach (iObserver o in observers)
            o.UpdateHalf();
    }

    public void NotifyGameIsOver()
    {
        foreach (iObserver o in observers)
            o.UpdateGameIsOver();
    }
}
