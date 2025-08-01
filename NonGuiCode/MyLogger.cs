using FunnyOldGame;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

public enum LogLevel { Trace, Debug, Info, Warning, Error, Fatal }

// Match Event Types (expanded for detail)
public enum MatchEventType
{
    // General match flow and meta-events
    MomentStart,            // Start of a simulation tick/moment
    MomentEnd,              // End of a simulation tick/moment
    GamePhaseChange,        // e.g., KickOff, HalfTime, FullTime, OT Start/End, PK Shootout Start/End
    OffsideCalled,          // A specific game rule enforcement

    // Core player/ball actions (outcomes)
    BallPossessionChanged,  // Ball changes who has control
    PlayerMoved,            // Player changes position (can be very frequent, consider LogLevel for UI)
    ShotOutcome,            // Covers all types of shots (Goal, Saved, Blocked, Missed)
    PassOutcome,            // Covers all types of passes (Completed, Intercepted, Failed)
    TackleOutcome,          // Covers all types of tackles (Won, Lost)
    BallCleared,            // Ball cleared away (includes own goals as an attribute)
    CardGiven,          // A foul occurred
    Injury,                 // A player got injured
    Substitution,           // A player substitution occurred
    AerialBattle,
    LooseBall,
    LooseBallRecovery,
    OutOfBounds,
    ThrowIn,
    FreeKick,
    CornerKick,
    FoulProcessed,
    PenaltyKickTaken,
    NonPKScoreUpdate,
    PKScoreUpdate,
    PenaltyKickExtraTimeUpdate,

    // Rarely used for INFO/play-by-play, mostly DEBUG/TRACE
    AI_Decision,            // For internal AI decision logging
    SystemMessage           // For general system messages not tied to specific match actions
}

// Shot specific enums
// Represents the specific phase of the game (used with GamePhaseChange)
public enum GamePhaseType
{
    MatchStart, KickOff, HalfTime,
    SecondHalfStart, OvertimeStart, OvertimeHalfTime, OvertimeEnd,
    PenaltyShootoutStart, PenaltyShootoutEnd,
    RegulationFullTime, MatchEnd // Final end of match after all OT/PK
}

// Result of a pass attempt (used with PassOutcome)
public enum PassResultType
{
    Completed, Intercepted, FailedOutOfPlay, FailedToOpponent, FailedToTeammate, Blocked, GKIntercepted, GKPunch
}

// Style/type of pass (for more specific commentary, used with PassOutcome)
public enum PassStyleType
{
    Normal, ThroughBall, Cross, Lob, GroundPass, HeaderPass, VolleyPass, Sliced, BackPass, GKRecycle, GoalKick, ShortThrowIn, LongThrowIn,
    ShortCorner
}

// Result of a tackle attempt (used with TackleOutcome)
public enum TackleResultType
{
    WonPossession, LostEncounter, Foul, BallOut, RecoveredBySameTeam, RecoveredByOpponent, LooseBall
}

// How a shot ended up (used with ShotOutcome)
public enum ShotOutcomeSubType // Renamed from ShotOutcomeType to avoid conflict with MatchEventType.ShotOutcome
{
    Goal, Saved, Blocked, MissedWide, MissedHigh, HitPost, HitCrossbar, AccuracyAdjustment, SaveAdjustment
}

// Where the shot was aimed or ended up relative to goal (can overlap with ShotOutcomeSubType)
public enum ShotTargetType { Goal, Post, Crossbar, Wide, High, Unknown, TopCorner, BottomCorner, MiddleOfNet } // Kept as is

// How a shot was blocked or saved (kept as is)
public enum ShotBlockType { DefenderBody, DefenderLeg, GoalkeeperDeflection, GoalkeeperCatch, GoalkeeperDeflectionForCorner, GoalkeeperPoorDeflection, None }

public enum AerialResult { Won, Lost, Contested, Foul, Positioning, Uncontested }

public enum AerialPositioningType { Skillcheck, Perfect, SetPiece, Marked, Close, Zonal, Clearance }

public enum DribblePhaseType { OneOnOneWithKeeper, Duel }

public enum DribbleResultType { Success, ShotAttempt, GKFoul, Fail, NoChallenger, ResistTackle, OutOfBounds }

public enum CrossType { InswingerLofted, OutswingerLofted, InswingerDriven, OutswingerDriven, ShortPass }

public enum CrossQuality { UnderPressure, DeepPosition, Midfield }

public enum OutOfBoundsResult { GoalKick, ThrowIn, Corner }

public enum PKTakenResult { Goal, HitPostLive, HitPostOutOfPlay, SaveDeflection, SaveCatch, MissWide }

/// <summary>
/// The core structured data object for every match event.
/// This is lightweight and passed quickly between threads.
/// </summary>
/// <summary>
/// The core structured data object for every match event.
/// This is lightweight and passed quickly between threads.
/// </summary>
public struct MatchEntry
{
    public LogLevel Level;
    public MatchEventType EventType;
    public DateTime Timestamp; // Or use long ticks for very high precision, e.g., long GameTimeTicks;

    // --- Common Attributes (retained and refined) ---
    public string PlayerName1;      // Primary player involved (shooter, passer, fouler, player who moved)
    public string PlayerName2;      // Secondary player involved (defender, receiver, fouled player, goalkeeper, substituted player)
    public string TeamName1;        // Team of PlayerName1 (or attacking team)
    public string TeamName2;        // Team of PlayerName2 (or defending team)
    public Enums.Positions? Player1Pos; // Nullable position, e.g., if event isn't player-specific
    public Enums.Positions? Player2Pos; // Nullable position
    public Enums.PitchZone Zone1;   // Zone where action originated / current player zone
    public Enums.PitchZone Zone2;   // Zone where action ended / target zone

    // --- Numeric Values (more specific names preferred) ---
    public double? Value1;          // e.g., ShotPower, PassDistance, FoulSeverity (double for flexibility)
    public double? Value2;          // e.g., ShotAccuracy, DribbleDistance
    public int? CurrentMatchMinute; // The minute the event occurred in
    public int? CurrentMatchSecond; // The second within the minute (if needed for high precision)
    public int? HomeScore;          // Current home score (useful for Goal events)
    public int? AwayScore;          // Current away score (useful for Goal events)
    public int? HomePKScores;       // PK goals for home team (for PKGameOver)
    public int? AwayPKScores;       // PK goals for away team (for PKGameOver)

    // --- Boolean Flags (Contextual Modifiers) ---
    public bool IsVolley;
    public bool IsHeader;
    public bool IsPenalty;          // Was this action part of a penalty kick?
    public bool IsFreeKick;         // Was this action part of a free kick?
    public bool IsCorner;           // Was this action part of a corner kick?
    public bool IsOffsideTrap;      // Was this offside due to an offside trap?
    public bool IsFoulCardYellow;   // For FoulCommitted event
    public bool IsFoulCardRed;      // For FoulCommitted event
    public bool IsCriticalMoment;   // Could indicate last minute, golden goal, etc.
    public bool IsLooseBallHigh;
    public bool IsAdvantage;
    public bool IsObstruction;
    public bool IsHolding;
    public bool IsShootoutShot;
    public bool IsRebound;
    public bool IsPoorDeflection;
    public bool IsGoodDeflection;
    public bool IsAerialBattlePending;
    public bool TrappedBall;
    public bool FailedTrapped;

    // For MatchEventType.GamePhaseChange
    public GamePhaseType? GamePhase;
    public DribblePhaseType? DribblePhase;

    // For MatchEventType.ShotOutcome
    public ShotOutcomeSubType? ShotResult; // What was the actual result of the shot? (Goal, Saved, Missed, etc.)
    public ShotTargetType? ShotTarget;     // Where was the shot aimed or where did it end up? (Goal, Post, Wide)
    public ShotBlockType? ShotBlockerType; // How was the shot blocked/saved? (Keeper, Defender)

    // For MatchEventType.PassOutcome
    public PassResultType? PassResult;     // Was the pass Completed, Intercepted, Failed?
    public PassStyleType? PassStyle;      // e.g., ThroughBall, Cross, Lob

    // For MatchEventType.TackleOutcome
    public TackleResultType? TackleResult; // Was the tackle Won, Lost, or a Foul?

    public AerialResult? AerialResult;
    public AerialPositioningType? AerialPositioningType;
    public DribbleResultType? DribbleResult;
    public CrossType? CrossType;
    public CrossQuality? CrossQuality;
    public OutOfBoundsResult? OutOfBoundsResult;
    public PKTakenResult? PKResult;

    // For MatchEventType.BallCleared
    public bool IsOwnGoal;          // Specifically for BallCleared when it's an own goal

    // Constructor for easier creation (optional, but highly recommended)
    public MatchEntry(LogLevel level, MatchEventType eventType, DateTime timestamp)
    {
        Level = level;
        EventType = eventType;
        Timestamp = timestamp;

        PlayerName1 = null;
        PlayerName2 = null;
        TeamName1 = null;
        TeamName2 = null;
        Player1Pos = null; // Changed to nullable
        Player2Pos = null; // Changed to nullable
        Zone1 = Enums.PitchZone.None;
        Zone2 = Enums.PitchZone.None;

        Value1 = null;
        Value2 = null;
        CurrentMatchMinute = null;
        CurrentMatchSecond = null;
        HomeScore = null;
        AwayScore = null;
        HomePKScores = null;
        AwayPKScores = null;

        IsVolley = false;
        IsHeader = false;
        IsPenalty = false;
        IsFreeKick = false;
        IsCorner = false;
        IsOffsideTrap = false;
        IsFoulCardYellow = false;
        IsFoulCardRed = false;
        IsCriticalMoment = false;

        GamePhase = null;
        ShotResult = null;
        ShotTarget = null;
        ShotBlockerType = null;
        PassResult = null;
        PassStyle = null;
        TackleResult = null;
        AerialResult = null;
        AerialPositioningType = null;
        DribbleResult = null;
        DribblePhase = null;
        CrossType = null;
        CrossQuality = null;
        IsOwnGoal = false;
        IsLooseBallHigh = false;
        IsAdvantage = false;
        IsObstruction = false;
        IsHolding = false;
        OutOfBoundsResult = null;
        PKResult = null;
        IsShootoutShot = false;
        IsRebound = false;
        IsGoodDeflection = false;
        IsPoorDeflection = false;
        IsAerialBattlePending = false;
        TrappedBall = false;
        FailedTrapped = false;
    }
}
public static class GameSettings
{
    public static LogLevel FileLoggingLevel = LogLevel.Debug; // For detailed logs to file
    public static LogLevel UiCommentaryLevel = LogLevel.Info; // For play-by-play shown in UI
}

/// <summary>
/// Centralized asynchronous logging and commentary generation system.
/// This runs on a background thread to keep the main simulation fluid.
/// </summary>
public class MyLogger
{
    // Queue for raw MatchEntry objects coming from the main thread
    public ConcurrentQueue<MatchEntry> _asyncMatchEventQueue = new ConcurrentQueue<MatchEntry>();

    // Queue for formatted commentary strings going to the UI thread
    //private static ConcurrentQueue<string> _uiCommentaryOutputQueue = new ConcurrentQueue<string>();

    // buffer for pacing commentary before sending to UI
    public static ConcurrentQueue<string> _internalCommentaryPacingBuffer = new ConcurrentQueue<string>();

    // Pacing control for UI commentary display
    private DateTime _nextCommentReadyTime = DateTime.MinValue;
    private readonly TimeSpan _minCommentDelay = TimeSpan.FromMilliseconds(2000); // Adjust for desired reading speed

    private CancellationTokenSource _cancellationTokenSource;
    private Task _loggingBackgroundTask;
    // NEW: For controlling pause/resume of UI commentary display
    private bool _isCommentaryPaused = false; // volatile for safe cross-thread access
                                                              // For synchronizing with the MatchManager to wait for comment display
    private TaskCompletionSource<bool> _commentDisplayCompletionSource;
    private ConcurrentQueue<Tuple<string, TimeSpan>> _pendingUiCommentaryForDisplay = new ConcurrentQueue<Tuple<string, TimeSpan>>();
    private bool _isFirstCommentForDisplay = true;

    public MyLogger(Game engine)
    {
        // Subscribe to the MatchEngine's event
        engine.OnMatchEntryCreated += LogMatchEntryToFile;
    }

    public MyLogger()
    {

    }

    //private Stack<MatchEntry> temp = new Stack<MatchEntry>();

    public void LogMatchEntryToFile(MatchEntry entry)
    {
        // This method would contain your _asyncMatchEventQueue logic
        // and write to file. It does NOT generate UI commentary.
        MatchEntry me2 = new MatchEntry();
        //if (temp.Count > 0)
        //    me2 = temp.Peek();
        //if ((me2.EventType == MatchEventType.PlayerMoved || me2.EventType == MatchEventType.BallPossessionChanged) && entry.EventType == MatchEventType.Substitution)
        //{
        //    int x = 0;
        //}
        //temp.Push(entry);
        _asyncMatchEventQueue.Enqueue(entry);
    }

    ///// <summary>
    ///// Initializes the background logging task. Call once at application startup.
    ///// </summary>
    //public void Initialize()
    //{
    //    _isFirstCommentForDisplay = true;
    //    _internalCommentaryPacingBuffer = new ConcurrentQueue<string>();
    //    _pendingUiCommentaryForDisplay = new ConcurrentQueue<Tuple<string, TimeSpan>>();
    //    if (_loggingBackgroundTask == null || _loggingBackgroundTask.IsCompleted)
    //    {
    //        _cancellationTokenSource = new CancellationTokenSource();
    //        //_loggingBackgroundTask = Task.Run(() => LogWriterLoop(_cancellationTokenSource.Token));
    //        Console.WriteLine("MyLogger initialized and background task started.");
    //        // Optionally load commentary templates here
    //        // CommentaryTemplates.LoadTemplates();
    //    }
    //}

    /// <summary>
    /// Shuts down the background logging task and ensures all pending logs are processed.
    /// Call once at application shutdown.
    /// </summary>
    /// <summary>
    /// Shuts down the background logging task and ensures all pending logs are processed.
    /// Call once at application shutdown.
    /// </summary>
    //public static async Task Shutdown() // Make Shutdown async
    //{
    //    if (_cancellationTokenSource != null)
    //    {
    //        //Console.WriteLine("MyLogger shutdown initiated. Signaling task to stop...");
    //        _cancellationTokenSource.Cancel(); // Signal the task to stop

    //        // Wait for the task to finish processing, but use await to not block the calling thread.
    //        // Add a timeout if you want to force termination after some time.
    //        if (_loggingBackgroundTask != null)
    //        {
    //            await _loggingBackgroundTask.ConfigureAwait(false); // Wait for the task to complete
    //        }

    //        _cancellationTokenSource.Dispose();
    //        _cancellationTokenSource = null;
    //        //Console.WriteLine("MyLogger shutdown complete.");
    //    }
    //}

    ///// <summary>
    ///// Submits a batch of MatchEntry objects from the main thread to the asynchronous queue.
    ///// Call this at the end of each simulation "moment" or tick.
    ///// </summary>
    //public async static void SubmitMatchEventBatch(List<MatchEntry> entries)
    //{
    //    foreach (var entry in entries)
    //    {
    //        _asyncMatchEventQueue.Enqueue(entry);
    //    }
    //}

    /// <summary>
    /// Called periodically by the UI thread to retrieve new commentary lines for display.
    /// </summary>
    //public static List<string> GetPendingCommentaryForUI()
    //{
    //    List<string> comments = new List<string>();
    //    while (_uiCommentaryOutputQueue.TryDequeue(out string comment))
    //    {
    //        comments.Add(comment);
    //    }
    //    return comments;
    //}

    // New method to initialize the TCS when a comment is about to be displayed

    // Event for MyLogger to tell the UI when a new comment is ready to be shown
    public static event EventHandler<CommentaryDisplayEventArgs> OnNewCommentaryReadyForDisplay;

    // New EventArgs class to pass commentary and duration to the UI
    public class CommentaryDisplayEventArgs : EventArgs
    {
        public string Commentary { get; }
        public TimeSpan DisplayDuration { get; }

        public CommentaryDisplayEventArgs(string commentary, TimeSpan displayDuration)
        {
            Commentary = commentary;
            DisplayDuration = displayDuration;
        }
    }

    //// Existing MyLogger methods (like Initialize, Shutdown, Pause/Resume etc.) remain.
    //// You will also need to re-implement `HasPendingUiCommentary` for the end-of-match flush logic
    //public static bool HasPendingUiCommentary()
    //{
    //    return !_pendingUiCommentaryForDisplay.IsEmpty;
    //}

    ///// <summary>
    ///// Submits a batch of MatchEntry objects from the main thread to the asynchronous queue.
    ///// Call this at the end of each simulation "moment" or tick.
    ///// </summary>
    //public static void SubmitMatchEventBatch(List<MatchEntry> entries)
    //{
    //    foreach (var entry in entries)
    //    {
    //        _asyncMatchEventQueue.Enqueue(entry); // For file logging

    //        // --- FORCED UI COMMENTARY FOR TESTING ---
    //        string uiCommentaryLine = GeneratePlayByPlayComment(entry); // Try to get a real comment
    //        if (string.IsNullOrEmpty(uiCommentaryLine) || entry.Level < GameSettings.UiCommentaryLevel)
    //        {
    //            // If no UI-worthy comment was generated, or the level was too low,
    //            // provide a guaranteed fallback for testing continuous updates.
    //            //uiCommentaryLine = $"Moment processed (Game Time: {entry.GameTime:F2}s, Event: {entry.EventType})";
    //            // You can make this more descriptive if needed, or just "..."
    //        }

    //        TimeSpan displayDuration;
    //        if (_isFirstCommentForDisplay)
    //        {
    //            displayDuration = TimeSpan.FromMilliseconds(0);
    //        }
    //        else
    //        {
    //            displayDuration = TimeSpan.FromMilliseconds(2000); // Standard duration
    //        }

    //        _pendingUiCommentaryForDisplay.Enqueue(Tuple.Create(uiCommentaryLine, displayDuration));
    //        // --- END FORCED UI COMMENTARY ---
    //    }
    //}

    // Modify LogWriterLoop to ONLY handle _asyncMatchEventQueue (for file logging)
    //private static async Task LogWriterLoop(CancellationToken token)
    //{
    //    while (!token.IsCancellationRequested || !_asyncMatchEventQueue.IsEmpty)
    //    {
    //        while (_asyncMatchEventQueue.TryDequeue(out MatchEntry entry))
    //        {
    //            // 1. Process for File Logging (More detailed/raw format)
    //            // (Uncomment and implement your file logging here if needed)
    //            // if (entry.Level >= GameSettings.FileLoggingLevel)
    //            // {
    //            //     string fileLogLine = FormatForFileLog(entry);
    //            //     try { File.AppendAllText("match_log.txt", fileLogLine + Environment.NewLine); }
    //            //     catch (Exception ex) { Console.Error.WriteLine($"[ERROR] MyLogger: File write error: {ex.Message}"); }
    //            // }
    //        }
    //        try { await Task.Delay(10, token); }
    //        catch (OperationCanceledException) { /* Handled */ }
    //    }
    //}

    ///// <summary>
    ///// The main loop for the background logging and commentary thread.
    ///// </summary>
    //private static async Task LogWriterLoop(CancellationToken token)
    //{
    //    while (!token.IsCancellationRequested || !_asyncMatchEventQueue.IsEmpty || !_internalCommentaryPacingBuffer.IsEmpty)
    //    {
    //        // Process incoming raw events from the main thread's queue
    //        while (_asyncMatchEventQueue.TryDequeue(out MatchEntry entry))
    //        {
    //            // 1. Process for File Logging (More detailed/raw format)
    //            //if (entry.Level >= GameSettings.FileLoggingLevel)
    //            //{
    //            //    string fileLogLine = FormatForFileLog(entry);
    //            //    try
    //            //    {
    //            //        File.AppendAllText("match_log.txt", fileLogLine + Environment.NewLine);
    //            //    }
    //            //    catch (Exception ex)
    //            //    {
    //            //        // Handle file write errors (e.g., log to console, but don't re-throw)
    //            //        Console.Error.WriteLine($"[ERROR] MyLogger: Failed to write to file: {ex.Message}");
    //            //    }
    //            //}

    //            // 2. Process for UI Play-by-Play Commentary (More narrative format)
    //            if (entry.Level >= GameSettings.UiCommentaryLevel)
    //            {
    //                string uiCommentaryLine = GeneratePlayByPlayComment(entry); // This is your "commentary engine"
    //                if (!string.IsNullOrEmpty(uiCommentaryLine))
    //                {
    //                    _internalCommentaryPacingBuffer.Enqueue(uiCommentaryLine);
    //                    TimeSpan displayDuration = TimeSpan.FromMilliseconds(2000); // 2 seconds as requested
    //                    _pendingUiCommentaryForDisplay.Enqueue(Tuple.Create(uiCommentaryLine, displayDuration));

    //                    //_uiCommentaryOutputQueue.Enqueue(uiCommentaryLine);
    //                    //try
    //                    //{
    //                    //    File.AppendAllText("match_log.txt", uiCommentaryLine + Environment.NewLine);
    //                    //}
    //                    //catch (Exception ex)
    //                    //{
    //                    //    // Handle file write errors (e.g., log to console, but don't re-throw)
    //                    //    Console.Error.WriteLine($"[ERROR] MyLogger: Failed to write to file: {ex.Message}");
    //                    //}
    //                }
    //            }
    //        }

    //        //// 3. Pacing Logic: Release comments from internal buffer to UI queue
    //        //// Only release if the buffer isn't empty AND enough time has passed AND commentary is NOT paused
    //        //if (!_internalCommentaryPacingBuffer.IsEmpty && DateTime.Now >= _nextCommentReadyTime && !_isCommentaryPaused) // Check _isCommentaryPaused
    //        //{
    //        //    if (_internalCommentaryPacingBuffer.TryDequeue(out string commentToDisplay))
    //        //    {
    //        //        _uiCommentaryOutputQueue.Enqueue(commentToDisplay); // Push to the UI's consumer queue
    //        //        _nextCommentReadyTime = DateTime.Now.Add(_minCommentDelay); // Set time for next release
    //        //    }
    //        //}

    //        // Short sleep to prevent busy-waiting if queues are empty
    //        try
    //        {
    //            await Task.Delay(10, token); // Correct way to non-blockingly wait
    //        }
    //        catch (OperationCanceledException)
    //        {
    //            // Task was cancelled during delay, exit loop on next iteration
    //        }
    //    }
    //}

    public void RecordPlayByPlay()
    {
        StreamWriter writer = new StreamWriter($"Play-by-play before exception{DateTime.Now.Ticks}.txt");
        while (_asyncMatchEventQueue.TryDequeue(out MatchEntry me))
        {
            if (me.Level == LogLevel.Info)
            {
                string comment = MyLogger2.GeneratePlayByPlayComment(me);
                writer.WriteLine($"{me.CurrentMatchMinute}:{me.CurrentMatchSecond}:{me.EventType}:{me.TackleResult}:{comment}");
            }
        }
        writer.Close();
    }

    /// <summary>
    /// Helper method to safely get player name and position for commentary.
    /// Handles nulls for PlayerPos.
    /// </summary>
    private static string GetNameOfPlayer(MatchEntry entry, bool isPlayer1)
    {
        if (isPlayer1)
        {
            // Use null-conditional operator ?. and null-coalescing operator ?? ""
            return $"{entry.PlayerName1 ?? "Unknown Player"} {entry.Player1Pos?.ToString() ?? ""} {entry.TeamName1 ?? ""} ";
        }
        else
        {
            string teamName = entry.TeamName2 ?? entry.TeamName1; // Default to TeamName1 if TeamName2 is null
            return $"{entry.PlayerName2 ?? "Unknown Player"} {entry.Player2Pos?.ToString() ?? ""} {teamName ?? ""} ";
        }
    }


    /// <summary>
    /// Formats a MatchEntry into a detailed string for the log file.
    /// This should be very verbose for debugging and data analysis.
    /// </summary>
    private static string FormatForFileLog(MatchEntry entry)
    {
        // Using StringBuilder for more efficient string concatenation in a loop (though not strictly necessary for single log lines)
        var sb = new StringBuilder();

        sb.Append($"[{entry.Timestamp:HH:mm:ss.fff}] [{entry.Level.ToString().ToUpper()}] ");
        sb.Append($"Event: {entry.EventType}");

        // Add common attributes, handling nulls with ?? "N/A"
        sb.Append($" | P1:{entry.PlayerName1 ?? "N/A"} ({entry.Player1Pos?.ToString() ?? "N/A"})");
        sb.Append($" | P2:{entry.PlayerName2 ?? "N/A"} ({entry.Player2Pos?.ToString() ?? "N/A"})");
        sb.Append($" | T1:{entry.TeamName1 ?? "N/A"} | T2:{entry.TeamName2 ?? "N/A"}");
        sb.Append($" | Zone1:{entry.Zone1} | Zone2:{entry.Zone2}");

        // Specific numeric values
        sb.Append($" | Minute:{entry.CurrentMatchMinute?.ToString() ?? "N/A"}:{entry.CurrentMatchSecond?.ToString("D2") ?? "N/A"}");
        sb.Append($" | Score:{entry.HomeScore?.ToString() ?? "N/A"}-{entry.AwayScore?.ToString() ?? "N/A"}");
        if (entry.HomePKScores.HasValue || entry.AwayPKScores.HasValue)
        {
            sb.Append($" | PKs:{entry.HomePKScores?.ToString() ?? "N/A"}-{entry.AwayPKScores?.ToString() ?? "N/A"}");
        }
        sb.Append($" | Value1:{entry.Value1?.ToString("F2") ?? "N/A"} | Value2:{entry.Value2?.ToString("F2") ?? "N/A"}");


        // Boolean flags
        if (entry.IsVolley) sb.Append(" | Volley");
        if (entry.IsHeader) sb.Append(" | Header");
        if (entry.IsPenalty) sb.Append(" | Penalty");
        if (entry.IsFreeKick) sb.Append(" | FreeKick");
        if (entry.IsCorner) sb.Append(" | Corner");
        if (entry.IsOffsideTrap) sb.Append(" | OffsideTrap");
        if (entry.IsFoulCardYellow) sb.Append(" | YellowCard");
        if (entry.IsFoulCardRed) sb.Append(" | RedCard");
        if (entry.IsCriticalMoment) sb.Append(" | CriticalMoment");

        // Event-specific attributes
        if (entry.GamePhase.HasValue) sb.Append($" | GamePhase:{entry.GamePhase}");
        if (entry.ShotResult.HasValue) sb.Append($" | ShotResult:{entry.ShotResult}");
        if (entry.ShotTarget.HasValue) sb.Append($" | ShotTarget:{entry.ShotTarget}");
        if (entry.ShotBlockerType.HasValue) sb.Append($" | ShotBlocker:{entry.ShotBlockerType}");
        if (entry.PassResult.HasValue) sb.Append($" | PassResult:{entry.PassResult}");
        if (entry.PassStyle.HasValue) sb.Append($" | PassStyle:{entry.PassStyle}");
        if (entry.TackleResult.HasValue) sb.Append($" | TackleResult:{entry.TackleResult}");
        if (entry.IsOwnGoal) sb.Append(" | OwnGoal");

        return sb.ToString();
    }
}
