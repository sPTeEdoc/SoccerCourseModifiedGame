using Godot;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System;

public class LoggingStuff
{
	public static StreamWriter writingStream;
		static string runningSummary = "";

		public static void CreateNewLog()
		{
			runningSummary = "";
			//writingStream = new StreamWriter("Log\\" + DateTime.Now.Month + "-" + DateTime.Now.Day
			//    + "_" + DateTime.Now.Year + "_" + DateTime.Now.Ticks + ".txt");
		}

		public static string LogTheEvent(string eventString)
		{
			runningSummary += eventString + "/r";
			//writingStream.WriteLine(eventString);
			//Console.WriteLine(eventString);
			return eventString;
		}

		public static string PlayByPlay(string eventstring)
		{
			//Console.WriteLine(eventstring);
			return eventstring;
		}

		public static void LogTheRunningSummary()
		{
			StreamWriter writer = new StreamWriter("Log\\" + DateTime.Now.Month + "-" + DateTime.Now.Day
				+ "_" + DateTime.Now.Year + "_" + DateTime.Now.Ticks + " Up to error.txt");
			writer.WriteLine(runningSummary);
			writer.Close();
		}

		public static void DisposeLog()
		{
			//writingStream.Dispose();
		}
}
