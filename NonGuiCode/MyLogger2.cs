using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class MyLogger2
{
    public static ConcurrentQueue<MatchEntry> _asyncMatchEventQueue = new ConcurrentQueue<MatchEntry>();
    private static CancellationTokenSource _cancellationTokenSource;
    private static Task _loggingBackgroundTask;
    private static string _logFilePath = $"Log//Play-by-play before exception{DateTime.Now.Ticks}.txt"; // Centralized log file
    

    // Make constructor private or remove if it's a static class/singleton
    // If you need it for dependency injection, keep it, but ensure Initialize/Shutdown are called appropriately.
    public MyLogger2() { }

    /// <summary>
    /// Initializes the background logging task. Call once at application startup.
    /// </summary>
    public static void Initialize() // Made static for simpler access if it's a utility class
    {
        _logFilePath = $"Log//Play-by-play before exception{DateTime.Now.Ticks}.txt"; // Centralized log file
        if (_loggingBackgroundTask == null || _loggingBackgroundTask.IsCompleted)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _loggingBackgroundTask = Task.Run(() => LogWriterLoop(_cancellationTokenSource.Token));
        }
    }

    /// <summary>
    /// Shuts down the background logging task and ensures all pending logs are processed.
    /// Call once at application shutdown.
    /// </summary>
    public static async Task Shutdown() // Made static
    {
        if (_cancellationTokenSource != null)
        {
            Console.WriteLine("MyLogger shutdown initiated. Signaling task to stop...");
            _cancellationTokenSource.Cancel(); // Signal the task to stop

            // Wait for the task to finish processing all remaining events
            if (_loggingBackgroundTask != null)
            {
                await _loggingBackgroundTask.ConfigureAwait(false);
            }

            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            Console.WriteLine("MyLogger shutdown complete.");
        }
    }

    /// <summary>
    /// Enqueues a MatchEntry for asynchronous file logging.
    /// This is called by individual game simulations.
    /// </summary>
    public static void LogMatchEntry(MatchEntry entry) // Made static for direct access
    {
        _asyncMatchEventQueue.Enqueue(entry);
    }

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
    /// The main loop for the background logging thread.
    /// Continuously dequeues MatchEntry objects and writes them to the log file.
    /// </summary>
    private static async Task LogWriterLoop(CancellationToken token)
    {
        // Use a StreamWriter for efficient, continuous writing
        using (StreamWriter writer = new StreamWriter(_logFilePath, append: true, encoding: Encoding.UTF8))
        {
            writer.AutoFlush = true; // Ensures data is written to disk immediately
            while (!token.IsCancellationRequested || !_asyncMatchEventQueue.IsEmpty)
            {
                while (_asyncMatchEventQueue.TryDequeue(out MatchEntry entry))
                {
                    if (entry.Level == LogLevel.Info)
                    {
                        // Format for file logging - include timestamp and match ID for debugging parallel games
                        string fileLogLine = GeneratePlayByPlayComment(entry);
                        fileLogLine = $"{entry.CurrentMatchMinute}:{entry.CurrentMatchSecond}:{entry.EventType}:{entry.TackleResult}:{fileLogLine}";
                        try
                        {
                            writer.WriteLine(fileLogLine);
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"[ERROR] MyLogger: Failed to write to file: {ex.Message}");
                            // You might want to re-enqueue or store failed entries for retry or different error log
                        }
                    }
                }

                // Short delay to prevent busy-waiting if queues are empty
                try
                {
                    await Task.Delay(10, token); // Non-blocking wait
                }
                catch (OperationCanceledException)
                {
                    // Expected during shutdown
                }
            }
        } // StreamWriter will be disposed and flushed here when exiting loop
    }

    /// <summary>
    /// Formats a MatchEntry into a string suitable for file logging.
    /// IMPORTANT: Include a unique Game ID to distinguish logs from different parallel games.
    /// </summary>
    //private static string FormatForFileLog(MatchEntry entry)
    //{
    //    // Example formatting, enhance as needed.
    //    // Consider adding a MatchId or GameId to MatchEntry if you're simulating many games.
    //    // For now, let's assume Timestamp is enough or you add an explicit GameId field.
    //    StringBuilder sb = new StringBuilder();
    //    sb.Append($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.ffff}] "); // High precision timestamp
    //    // Consider adding a unique Game ID here:
    //    // sb.Append($"[GameId:{entry.GameId}] "); // If MatchEntry had a GameId field

    //    sb.Append($"[{entry.Level.ToString().ToUpper()}] ");
    //    sb.Append($"Min:{entry.CurrentMatchMinute?.ToString("D2") ?? "--"}:Sec:{entry.CurrentMatchSecond?.ToString("D2") ?? "--"} ");
    //    sb.Append($"{entry.EventType}");

    //    // Add specific details based on EventType for full logging
    //    if (!string.IsNullOrEmpty(entry.PlayerName1)) sb.Append($", P1: {entry.PlayerName1} ({entry.TeamName1})");
    //    if (!string.IsNullOrEmpty(entry.PlayerName2)) sb.Append($", P2: {entry.PlayerName2} ({entry.TeamName2})");
    //    if (entry.HomeScore.HasValue) sb.Append($", Score: {entry.HomeScore}-{entry.AwayScore}");
    //    // ... add more details as per your MatchEntry structure

    //    // For Play-by-Play, you'd call GeneratePlayByPlayComment(entry) here if needed for file.
    //    // For raw logs, you might want more technical details.
    //    // Example: if (entry.EventType == MatchEventType.ShotOutcome) sb.Append($", ShotResult: {entry.ShotResult}, Target: {entry.ShotTarget}");

    //    return sb.ToString();
    //}

    // You can keep GeneratePlayByPlayComment as a separate method if you want to use it
    // for UI commentary *or* a dedicated "play-by-play" file, distinct from the raw log.
    public static string GeneratePlayByPlayComment(MatchEntry entry)
    {
        // Ensure only events at or above the UI commentary level are processed
        if (entry.Level < GameSettings.UiCommentaryLevel)
        {
            return string.Empty; // Don't generate commentary for lower detail levels
        }

        string player1 = GetNameOfPlayer(entry, true);
        string player2 = GetNameOfPlayer(entry, false); // Even if P2 is not involved, it's safe to call

        switch (entry.EventType)
        {
            case MatchEventType.MomentStart:
                // MomentStart is primarily for time updates in UI, usually not a verbose commentary line.
                // If you want a periodic minute update, this is where it goes.
                if (entry.CurrentMatchMinute.HasValue)
                {
                    // Only log at INFO level, otherwise it's just for Debug
                    if (entry.Level <= LogLevel.Info)
                    {
                        string minuteStr = entry.CurrentMatchMinute.Value.ToString();
                        string secondStr = entry.CurrentMatchSecond.HasValue ? entry.CurrentMatchSecond.Value.ToString("D2") : "00";
                        return $"Match time: {minuteStr}:{secondStr}";
                    }
                }
                break;

            case MatchEventType.NonPKScoreUpdate:
                return $"Score: {entry.TeamName1}: {entry.Value1} - {entry.TeamName2}: {entry.Value2}";
            case MatchEventType.PKScoreUpdate:
                return $"PK Score: {entry.TeamName1}: {entry.Value1} - {entry.TeamName2}: {entry.Value2}";
            case MatchEventType.GamePhaseChange:
                switch (entry.GamePhase)
                {
                    case GamePhaseType.MatchStart:
                        return $"Welcome to the match! {entry.TeamName1} vs {entry.TeamName2}. Get ready for kickoff!";
                    case GamePhaseType.KickOff:
                        return $"{entry.TeamName1} kicks it off! {player1} receives the ball.";
                    case GamePhaseType.HalfTime:
                        return $"\n--- HALF-TIME! --- Score: {entry.TeamName1}: {entry.HomeScore ?? 0} - {entry.TeamName2}: {entry.AwayScore ?? 0}\n";
                    case GamePhaseType.SecondHalfStart:
                        return $"The second half is underway!";
                    case GamePhaseType.RegulationFullTime:
                    case GamePhaseType.OvertimeEnd: // Use same logic for final score announcements after regulation/OT
                        return $"End of {(entry.GamePhase == GamePhaseType.RegulationFullTime ? "regulation" : "second half of extra time")}! ";
                    case GamePhaseType.OvertimeStart:
                        return $"The score is tied! We're heading into Extra Time.";
                    case GamePhaseType.OvertimeHalfTime:
                        return $"That's the end of the first half of extra time. We'll head to the second soon.";
                    case GamePhaseType.PenaltyShootoutStart:
                        string penaltyShootOutStratString = $"\n--- END OF EXTRA TIME! The score is still tied! Proceeding to Penalty Shootout! ---";
                        if (entry.Value1 == 1) penaltyShootOutStratString = $"{penaltyShootOutStratString} {entry.TeamName1} has won the toss and will kick first."; else penaltyShootOutStratString = $"{penaltyShootOutStratString} {entry.TeamName2} has won the toss and will kick first.";
                        return penaltyShootOutStratString;
                    case GamePhaseType.PenaltyShootoutEnd:
                        string pkScoreHome = $"{entry.HomeScore} ({entry.HomePKScores?.ToString() ?? "0"})";
                        string pkScoreAway = $"{entry.AwayScore} ({entry.AwayPKScores?.ToString() ?? "0"})";
                        if (entry.HomePKScores > entry.AwayPKScores)
                        {
                            return $"{entry.TeamName1} win the penalty shootout {pkScoreHome} - {pkScoreAway}!";
                        }
                        else if (entry.AwayPKScores > entry.HomePKScores)
                        {
                            return $"{entry.TeamName2} win the penalty shootout {pkScoreAway} - {pkScoreHome}!";
                        }
                        return $"Penalty shootout ends. It's unclear what happened! ({pkScoreHome}-{pkScoreAway})"; // Fallback
                    case GamePhaseType.MatchEnd:
                        string matchEnd = $"The match has officially concluded.";
                        string scoreHome = entry.HomeScore?.ToString() ?? "0";
                        string scoreAway = entry.AwayScore?.ToString() ?? "0";
                        if (entry.HomeScore > entry.AwayScore)
                        {
                            matchEnd = $"{matchEnd} {entry.TeamName1} wins {scoreHome} to {scoreAway}!";
                        }
                        else if (entry.AwayScore > entry.HomeScore)
                        {
                            matchEnd = $"{matchEnd} {entry.TeamName2} wins {scoreAway} to {scoreHome}!";
                        }
                        else
                        {
                            matchEnd = $"{matchEnd} The score is tied {scoreHome} - {scoreAway}!";
                        }
                        return matchEnd;
                    default:
                        return $"Game event: {entry.GamePhase?.ToString()}"; // Fallback for unhandled game phases
                }

            case MatchEventType.BallPossessionChanged:
                // This event type now signifies who has the ball.
                // Commentary can be more nuanced if other data suggests a dribble, or just receiving.
                // You might combine this with PlayerMoved to imply dribbling.
                return $"{player1} ({entry.TeamName1}) now has possession in {entry.Zone1} having it won it from {entry.TeamName2}";

            case MatchEventType.PlayerMoved:
                if (entry.Level <= LogLevel.Debug) // Only for more detailed UI display
                    return $"{player1} moves to {entry.Zone2}.";
                string dribbleString = "";
                if (entry.DribblePhase == DribblePhaseType.OneOnOneWithKeeper)
                {
                    dribbleString = $"{player1} has broken away! The only one between him and the goal is {player2}!";
                    if (entry.DribbleResult == DribbleResultType.ShotAttempt)
                    {
                        dribbleString = $"{dribbleString} {player1} will try and shoot it past {player2}!";
                    }
                    else
                    {
                        dribbleString = $"{dribbleString} {player2} charges as {player1} dribbles!";
                        if (entry.DribbleResult == DribbleResultType.Success)
                            dribbleString = $"{dribbleString} {player1} makes a move! And completely fools {player2}! He'll shoot it into the open net! Goal!";
                        else if (entry.DribbleResult == DribbleResultType.Fail)
                            dribbleString = $"{dribbleString} {player1} makes a move! But {player2} is not fooled! He grabs it right from the attacker's feet!";
                        else if (entry.DribbleResult == DribbleResultType.GKFoul)
                        {
                            dribbleString = $"{dribbleString} {player1} makes a move! {player2} slams into him trying to get the ball! The referee blows his whistle! That'll be a foul" +
                                $" and a penalty will be awarded! Unbelievable!";
                        }
                    }
                }
                else if (entry.DribblePhase == DribblePhaseType.Duel)
                {
                    if (entry.DribbleResult == DribbleResultType.NoChallenger)
                    {
                        dribbleString = $"{player1} continues to scan the area looking for a play.";
                    }
                    else
                    {
                        dribbleString = $"{player2} charges {player1}.";
                        if (entry.DribbleResult == DribbleResultType.Success)
                        {
                            dribbleString = $"{dribbleString} {player1} makes a good, deft move and dribbles right by {player2}!";
                        }
                        else if (entry.DribbleResult == DribbleResultType.ResistTackle)
                        {
                            dribbleString = $"{dribbleString} {player1} shakes off {player2}'s challenge and continues dribbling";
                        }
                        else if (entry.DribbleResult == DribbleResultType.OutOfBounds)
                        {
                            dribbleString = $"Tackle failed, but the ball breaks loose! Players scramble for possession!";
                        }
                    }
                }
                else
                {
                    if (entry.IsRebound)
                        return $"{player1} may have a fantastic opportunity in {entry.Zone1}!";
                    else if (entry.IsAerialBattlePending)
                        return $"What is {player1} going to do with the ball still in the air and in {entry.Zone1}?";
                    else if (entry.TrappedBall)
                        return $"{player1} traps the ball and will start the attack!";
                    else if (entry.FailedTrapped)
                        return $"{player1} traps the ball... no, it bounces awkwardly off his chance and is kicked away!";
                    return $"{player1} is thinking about his next move in {entry.Zone1}.";
                }
                return dribbleString;

            case MatchEventType.ShotOutcome:
                {
                    // This case handles all shot outcomes (Goal, Saved, Blocked, Missed)
                    string commonPrefix = $"{player1} attempts a shot! ";
                    if (entry.IsRebound)
                        commonPrefix = $"{player1} with the rebound! ";
                    if (entry.IsVolley) commonPrefix = "A volley! ";
                    else if (entry.IsHeader) commonPrefix = "A header! ";

                    commonPrefix = $"{commonPrefix} A shot from {entry.Zone1}!";

                    switch (entry.ShotResult)
                    {
                        case ShotOutcomeSubType.Goal:
                            // Incorporate shot attributes for variety
                            string goalString = "";
                            if (entry.Value1 > 90) // Assuming Value1 is ShotPower
                                goalString = $"{commonPrefix} {player1} smashes it with immense power!";
                            else
                                goalString = $"{commonPrefix}";

                            if (entry.ShotTarget == ShotTargetType.TopCorner)
                                goalString = $"{goalString} GOAL! {player1} buries it in the top corner!";
                            else if (entry.ShotTarget == ShotTargetType.BottomCorner)
                                goalString = $"{goalString} GOAL! {player1} buries it in the bottom corner!";
                            else if (entry.ShotTarget == ShotTargetType.MiddleOfNet)
                                goalString = $"{goalString} GOAL! {player1}'s shot gets it just past the keeper!";
                            else if (entry.ShotTarget == ShotTargetType.Post)
                                goalString = $"{goalString} {player1}'s shot hits the post! But it bounces back in! GOAL!";
                            else if (entry.ShotTarget == ShotTargetType.Crossbar)
                                goalString = $"{goalString} {player1}'s shot hits the crossbar! But it bounces in after it hits the ground! GOAL!";
                            else if (entry.IsOwnGoal)
                                goalString = $"{player2} courageously blocks the shot! But it skirts off his leg and will end up in his own goal!";
                            else
                                goalString = $"{commonPrefix}GOAL! {player1} scores for {entry.TeamName1}!";
                            if (!entry.IsOwnGoal)
                                if (!player2.Contains("Unknown Player"))
                                    goalString = $"{goalString} {player2}'s great pass led to that goal!";
                            if (entry.IsPoorDeflection)
                            {
                                goalString = $"{goalString} The goalkeeper may have made the save but the lack of fundamentals on that deflection certainly led to the goal.";
                            }
                            if (entry.IsGoodDeflection)
                            {
                                goalString = $"{goalString} The goalkeeper made a great save but bad luck on the deflection certainly led to the goal.";
                            }
                            return goalString;
                        case ShotOutcomeSubType.Saved:
                            string saveString = "";
                            if (entry.Value2 > 80) // Assuming Value2 is ShotAccuracy, implying a good save
                                saveString = $"{commonPrefix} {player1}'s fierce shot is brilliantly saved by {player2}!";
                            if (entry.ShotBlockerType == ShotBlockType.GoalkeeperDeflection)
                            {
                                saveString = $"{commonPrefix}{saveString} {player1}'s shot deflected by {player2}. But the ball is loose in the box!";
                            }
                            else if (entry.ShotBlockerType == ShotBlockType.GoalkeeperDeflectionForCorner)
                                saveString = $"{commonPrefix}{saveString} {player1}'s shot deflected by {player2} out of bounds! Not out of danger yet. It'll be a corner.";
                            else if (entry.ShotBlockerType == ShotBlockType.GoalkeeperCatch)
                                saveString = $"{commonPrefix}{saveString} {player1}'s shot caught cleanly by {player2}. Good catch if you ask me.";
                            else if (entry.ShotBlockerType == ShotBlockType.GoalkeeperPoorDeflection)
                            {
                                saveString = $"{commonPrefix}{saveString} {player1}'s shot bounces out of by {player2}'s hand in dangerous territory!";
                                if (entry.IsOwnGoal)
                                    saveString = $"{saveString} then kicks it into the net as he tries to recover! Unbelievable! It's an own goal!";
                            }
                            else
                                saveString = $"{commonPrefix}{player1}'s shot saved!";
                            return saveString;
                        case ShotOutcomeSubType.Blocked:
                            return $"{commonPrefix}{player1}'s shot is blocked by {player2}!";
                        case ShotOutcomeSubType.MissedWide:
                            return $"{commonPrefix}{player1} shoots wide of the target! The ball goes out of bounds and that'll be a goal kick!";
                        case ShotOutcomeSubType.MissedHigh:
                            return $"{commonPrefix}{player1} shoots high and over the bar!";
                        case ShotOutcomeSubType.HitPost:
                            return $"{commonPrefix}{player1}'s shot hits the post!";
                        case ShotOutcomeSubType.HitCrossbar:
                            return $"{commonPrefix}{player1}'s shot rattles the crossbar!";
                    }
                    break;
                }

            case MatchEventType.PassOutcome:
                {
                    string commonPrefix = "";
                    if (entry.PassStyle == PassStyleType.ThroughBall) commonPrefix = "A cunning through ball! ";
                    else if (entry.PassStyle == PassStyleType.Cross) commonPrefix = $"{player1} attempts a cross into the box towards {player2}!";
                    else if (entry.PassStyle == PassStyleType.Lob) commonPrefix = "A lobbed pass! ";
                    else if (entry.PassStyle == PassStyleType.Sliced) commonPrefix = "A sliced pass! ";
                    else if (entry.PassStyle == PassStyleType.GroundPass) commonPrefix = "A pass that stays on the floor!";
                    else if (entry.PassStyle == PassStyleType.GKRecycle) commonPrefix = $"{player1} will look to put the ball back into play after collecting it.";
                    else if (entry.PassStyle == PassStyleType.GoalKick) commonPrefix = $"{player1} will deliver the goal kick.";
                    else if (entry.PassStyle == PassStyleType.ShortThrowIn) commonPrefix = $"{player1} throws it short to his teammate {player2}.";
                    else if (entry.PassStyle == PassStyleType.LongThrowIn) commonPrefix = $"{player1} opts for a long throw to his teammate {player2}.";
                    else if (entry.PassStyle == PassStyleType.LongThrowIn) commonPrefix = $"{player1} wants to fool them with a short corner {player2}!";
                    else if (entry.PassStyle == PassStyleType.HeaderPass) commonPrefix = $"{player1} heads the ball in the general direction of {player2}!";

                    if (entry.CrossType != null)
                    {
                        commonPrefix = $"{commonPrefix} {EnumCache<CrossType>.ToString(entry.CrossType).Replace("Outswinger", "outswinging ").Replace("Inswinger", "inswinging ").Replace("Lofted", "lofted ").Replace("Driven", "driven ")} cross!";
                    }

                    // Check for pressure from the sender
                    if (entry.Value1 > 0 && entry.Value1 <= 100) // Assuming Value1 indicates pressure level for sender
                    {
                        commonPrefix += $"{player1} passes under pressure! ";
                    }
                    // Check for pressure on the receiver
                    if (entry.Value2 > 0 && entry.Value2 <= 100) // Assuming Value2 indicates pressure level for receiver
                    {
                        if (entry.PassResult == PassResultType.Completed)
                        {
                            commonPrefix += $"{player2} receives under heavy pressure! ";
                        }
                        else
                        {
                            commonPrefix += $"{player2} was under pressure! ";
                        }
                    }

                    commonPrefix = $"{commonPrefix} from {entry.Zone1} to {entry.Zone2}.";

                    switch (entry.PassResult)
                    {
                        case PassResultType.Completed:
                            return $"{commonPrefix}{player1} passes to {player2}.";
                        case PassResultType.Intercepted:
                            return $"{commonPrefix}{player2} intercepts {player1}'s pass!";
                        case PassResultType.FailedOutOfPlay:
                            return $"{commonPrefix}{player1}'s pass goes out of play.";
                        case PassResultType.FailedToOpponent:
                            return $"{commonPrefix}{player1} misplaces the pass, straight to an opponent!";
                        case PassResultType.FailedToTeammate:
                            return $"{commonPrefix}{player1}'s pass to {player2} is inaccurate.";
                        case PassResultType.Blocked:
                            return $"{commonPrefix}{player1}'s pass is blocked by {player2}.";
                        case PassResultType.GKIntercepted:
                            return $"{commonPrefix}{player2} snatches it in the air! Excellent read by the keeper!";
                        case PassResultType.GKPunch:
                            {
                                if (!entry.IsOwnGoal)
                                    return $"{commonPrefix}{player2} punches the ball as it heads his way! Excellent read by the keeper! But the ball is still in play!";
                                else
                                    return $"{commonPrefix}{player2} punches the ball as it heads his way! Excellent read by the keeper! But the ball is still in play! It takes an awkward bounce and goes in his own net! " +
                                        $"What an unfortunate own goal!";
                            }
                    }
                    return $"{commonPrefix}";
                }

            case MatchEventType.TackleOutcome:
                string tackleString = $"{player1} confronts {player2} as he dribbles toward him.";
                switch (entry.TackleResult)
                {
                    case TackleResultType.WonPossession:
                        tackleString = $"{tackleString} {player1} wins the ball cleanly from {player2} with a nice tackle!";
                        break;
                    case TackleResultType.LostEncounter:
                        if (entry.IsOwnGoal)
                        {
                            tackleString = $"{player1}'s tackle goes awry! The ball ends up in his own net! Own Goal!;";
                        }
                        else
                        {
                            tackleString = $"{tackleString} {player2} shakes off {player1}'s challenge and continues dribbling";
                        }
                        break;
                    case TackleResultType.Foul:
                        tackleString = $"{tackleString} Foul by {player1} on {player2}!";
                        break;
                    case TackleResultType.BallOut:
                        tackleString = $"{tackleString} {player1}'s tackle sends the ball out of play.";
                        break;
                    case TackleResultType.RecoveredBySameTeam:
                        tackleString = $"{tackleString} {player1} tackles, and a teammate recovers!";
                        break;
                    case TackleResultType.RecoveredByOpponent:
                        tackleString = $"{tackleString} {player1} tackles, but the opponent retains possession!";
                        break;
                    case TackleResultType.LooseBall:
                        tackleString = $"{tackleString} {player1}'s tackle sends the ball loose!";
                        break;
                }
                return tackleString;

            case MatchEventType.BallCleared:
                if (entry.IsOwnGoal)
                    return $"Unbelievable! {player1} slices the clear into his own net! Own Goal for {entry.TeamName1}!";
                string ballclear = $"{player1} clears the ball from {entry.Zone1}.";
                if (entry.IsHeader)
                    ballclear = $"{player1} heads the ball clearfrom {entry.Zone1}.";
                return ballclear;
            case MatchEventType.AerialBattle:
                string aerialString = $"{player1} and {player2} move toward the high ball!";
                if (entry.AerialResult == AerialResult.Won)
                    aerialString = $"{aerialString} {player1} gets into perfect position.";
                else if (entry.AerialResult == AerialResult.Lost)
                    aerialString = $"{aerialString} {player2} gets into perfect position.";
                else if (entry.AerialResult == AerialResult.Foul)
                {
                    aerialString = aerialString = $"{aerialString}. Oh, the referee blows his whistle!";
                    if (player2 != "Unknown Player")
                        aerialString = $"{player2} apparently committed a foul in the aerial contest.";
                    else
                        aerialString = $"{player1} apparently committed a foul in the aerial contest.";
                }
                else if (entry.AerialResult == AerialResult.Contested)
                {
                    aerialString = $"Nobody is getting to that ball as it sails over both of their heads!";
                }
                else if (entry.AerialResult == AerialResult.Uncontested)
                {
                    aerialString = $"No defender found near cross target! Attacker gets free opportunity.";
                }
                return aerialString;
            case MatchEventType.CardGiven:
                string cardInfo = "";
                if (entry.IsFoulCardYellow) cardInfo = $"{player1} receives a Yellow Card!";
                if (entry.IsFoulCardRed) cardInfo = $"{player1} receives a red card and will be sent off!)";
                if (entry.Value1 > 1) cardInfo = $"{cardInfo} And that's his second! He'll be sent off for having two yellow cards!";

                return cardInfo;

            case MatchEventType.Injury:
                return $"Injury! {player1} is down and needs attention.";

            case MatchEventType.Substitution:
                return $"Substitution for {entry.TeamName1}: {player2} comes ON for {player1}."; // Note: P1 is player off, P2 is player on

            case MatchEventType.OffsideCalled:
                string offside = $"OFFSIDE CALLED! {player1} was offside when the ball was played.";
                if (entry.IsOffsideTrap)
                    offside = $"{offside} Upon reflection, that looks like a really well played offside trap!";
                return offside;
            case MatchEventType.LooseBall:
                string looseballString = $"The ball bounces toward {entry.Zone1}!";
                if (entry.IsLooseBallHigh)
                    looseballString = $"The ball sails toward {entry.Zone1}!";
                if ($"{player1}" != "Unknown Player")
                    looseballString = $"{looseballString} Last touched by {player1}.";
                return looseballString;
            case MatchEventType.LooseBallRecovery:
                return $"{player1} gets to the ball first and gathers the loose ball.";
            case MatchEventType.OutOfBounds:
                switch (entry.OutOfBoundsResult)
                {
                    case OutOfBoundsResult.Corner:
                        return $"{entry.TeamName1} wins the corner.";
                    case OutOfBoundsResult.ThrowIn:
                        return $"{entry.TeamName1} earns a throw-in.";
                    default:
                        return $"That'll be a goal kick for {entry.TeamName1}.";
                }
            case MatchEventType.ThrowIn:
                return $"{player1} will take the throw in.";
            case MatchEventType.FreeKick:
                return $"{player1} will take the free kick.";
            case MatchEventType.CornerKick:
                return $"{player1} will take the corner kick.";
            case MatchEventType.FoulProcessed:
                string foulProcessedString = "";
                if (entry.IsObstruction)
                    foulProcessedString = $"{player1} is called for obstruction against {player2}.";
                if (entry.IsHolding)
                    foulProcessedString = $"{player1} was caught holding {player2}'s jersey and thusly will be called for it.";
                if (entry.IsAdvantage)
                    foulProcessedString = $"{foulProcessedString} The referee will allow play to continue, giving advantage for {entry.TeamName1}";
                else if (entry.IsFreeKick)
                    foulProcessedString = $"{foulProcessedString} The referee will award the free kick to {entry.TeamName1}.";
                else if (entry.IsPenalty)
                    foulProcessedString = $"{foulProcessedString} That'll be a penalty kick for {entry.TeamName1}!";
                return foulProcessedString;
            // Add cases for other specific MatchEventType's if they still exist in your pared-down list
            // E.g., case MatchEventType.SystemMessage:
            case MatchEventType.PenaltyKickTaken:
                string pkString = $"{player1} steps up to take a penalty kick against {player2}. Hard to say who has more pressure on them. ";
                switch (entry.PKResult)
                {
                    case PKTakenResult.Goal:
                        pkString = $"{pkString} {player1} shoots in one direction, but {player2} dives in the other! Goal!";
                        break;
                    case PKTakenResult.HitPostLive:
                        pkString = $"{pkString} {player1} shoots in one direction, but {player2} dives in the other! But it rattles off the post!";
                        break;
                    case PKTakenResult.HitPostOutOfPlay:
                        pkString = $"{pkString} {player1} shoots in one direction, but {player2} dives in the other! But it rattles off the post! And goes out of bounds behind the goalkeeper!";
                        break;
                    case PKTakenResult.MissWide:
                        pkString = $"{pkString} {player1} shoots in one direction, but {player2} dives in the other! But the shot goes too far and sails wide! No goal!";
                        break;
                    case PKTakenResult.SaveCatch:
                        pkString = $"{pkString} {player1} shoots in one direction, and {player2} dives in the same direction! It bounces off his hands";
                        if (entry.IsShootoutShot)
                            pkString = $"{pkString} and away from the goal! Great save! No goal";
                        else
                            pkString = $"{pkString} and back into play, though... And back into them! Great save and catch by the goalkeeper!";
                        break;
                    case PKTakenResult.SaveDeflection:
                        pkString = $"{pkString} {player1} shoots in one direction, and {player2} dives in the same direction! It bounces off his hands";
                        if (entry.IsShootoutShot)
                            pkString = $"{pkString} and away from the goal! Great save! No goal";
                        else
                            pkString = $"{pkString} and back into play, though...";
                        break;
                }
                return pkString;
            case MatchEventType.PenaltyKickExtraTimeUpdate:
                return $"No winner yet so we're headed to sudden death!";
            default:
                // For unhandled types or types not meant for UI display at this level
                if (entry.Level <= LogLevel.Debug)
                    return $"[Unhandled UI Event: {entry.EventType.ToString()} - Level:{entry.Level}]";
                break;
        }
        return string.Empty; // Return empty string if no commentary should be generated
    }
}
