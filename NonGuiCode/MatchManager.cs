using FunnyOldGame;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunnyOldGameRedux.NonGuiCode
{
    public class MatchManager
    {
        private Game _matchEngine;
        private Stopwatch _gameTimer; // Measures real-world time
        private double _lastFrameTime; // Time in seconds when the last "frame" was rendered (or Update called)
        private double _accumulator; // Stores unsimulated real-world time

        // Set FIXED_TIME_STEP to 2.0 seconds: each SimulateMoment() call will
        // correspond to 2 real-world seconds.
        private const double FIXED_TIME_STEP = 2.0;
        private const double MAX_FRAME_TIME = 0.25; // Prevents "spiral of death" after a very long hitch

        public enum MatchRunMode { Watch, SimulateOnly }

        // Custom EventArgs class to carry relevant match state data
        public class MatchStateUpdatedEventArgs : EventArgs
        {
            public int CurrentMinute { get; set; }
            public int CurrentSecond { get; set; }
            public int HomeScore { get; set; }
            public int AwayScore { get; set; }

            public Enums.Half Half { get; set; }
            // Add any other data your UI needs to display the field, player states, etc.
            // This is a "snapshot" of the match state.
        }

        //public MatchManager(Game game)
        //{
        //    //_matchEngine = game;
        //    //_matchEngine.SetUpGame();
        //    //_gameTimer = new Stopwatch();
        //}

        ///// <summary>
        ///// Starts the match simulation and its main loop.
        ///// </summary>
        //public async Task StartMatch()
        //{
        //    Console.WriteLine($"--- Match Started in {CurrentRunMode} mode! ---");
        //    _isGameRunning = true;
        //    _isGamePaused = false;
        //    _gameTimer.Restart();
        //    _lastFrameTime = _gameTimer.Elapsed.TotalSeconds;
        //    _accumulator = 0.0;

        //    await MainGameLoop(); // Await the main game loop
        //    Console.WriteLine("--- Match Simulation Finished. ---");
        //}

        ///// <summary>
        ///// Pauses the match simulation.
        ///// </summary>
        //public void PauseGame()
        //{
        //    _isGamePaused = true;
        //    // Also pause UI commentary here for a full pause effect
        //    MyLogger.PauseUiCommentary();
        //    Console.WriteLine("Game Paused. Press 'R' to Resume...");
        //}

        ///// <summary>
        ///// Resumes the match simulation.
        ///// </summary>
        //public void ResumeGame()
        //{
        //    _isGamePaused = false;
        //    // Resume UI commentary here
        //    MyLogger.ResumeUiCommentary();
        //    // Resync timer to prevent a large simulation jump after a long pause
        //    _lastFrameTime = _gameTimer.Elapsed.TotalSeconds;
        //    Console.WriteLine("Game Resumed.");
        //}

        ///// <summary>
        ///// Stops the match simulation permanently.
        ///// </summary>
        //public void StopMatch()
        //{
        //    _isGameRunning = false;
        //    Console.WriteLine("Game stopping...");
        //}

        //// Modify MainGameLoop:
        //private async Task MainGameLoop()
        //{
        //    //_gameTimer.Start();
        //    //_lastFrameTime = _gameTimer.Elapsed.TotalSeconds;
        //    _lastFrameTime = _gameTimer.Elapsed.TotalSeconds; // This needs to be captured *here* when the loop truly begins processing.

        //    _accumulator = FIXED_TIME_STEP;

        //    while (_isGameRunning && (_matchEngine.CurrentMatchState != Enums.MatchState.MatchEnded))
        //    {
        //        double currentTime = _gameTimer.Elapsed.TotalSeconds;
        //        double deltaTime = currentTime - _lastFrameTime;
        //        _lastFrameTime = currentTime;

        //        if (deltaTime > MAX_FRAME_TIME)
        //        {
        //            deltaTime = MAX_FRAME_TIME;
        //        }

        //        if (!_isGamePaused)
        //        {
        //            _accumulator += deltaTime;

        //            // Process accumulated simulation steps
        //            while (_accumulator >= FIXED_TIME_STEP && _matchEngine.CurrentMatchState != Enums.MatchState.MatchEnded)
        //            {
        //                //_matchEngine.SimulateMoment(); // Execute one simulation step
        //                _accumulator -= FIXED_TIME_STEP;

        //                // --- NEW CRITICAL LOGIC FOR WATCH MODE ---
        //                if (CurrentRunMode == MatchRunMode.Watch)
        //                {
        //                    // After each simulation moment, check if MyLogger has comments to display
        //                    // This is where the game loop *pauses* to wait for commentary.
        //                    while (MyLogger.HasPendingUiCommentary())
        //                    {
        //                        await MyLogger.DisplayNextCommentAndAwaitCompletion();
        //                        // ONLY after a comment is displayed, update the main UI (score, time)
        //                        // This ensures scoreboard updates are synchronized with commentary.
        //                        UpdateMatchUI(); // This now only updates score/time, NOT commentary
        //                    }
        //                }
        //            }
        //        }
        //        else // Game is paused
        //        {
        //            _accumulator = 0;
        //        }

        //        // In Watch mode, we now yield AFTER commentary is processed for this step.
        //        // In SimulateOnly, we still spin fast.
        //        if (CurrentRunMode == MatchRunMode.Watch || _isGamePaused)
        //        {
        //            await Task.Delay(1); // Yield to prevent busy-waiting
        //        }
        //    } // End of while loop

        //    _gameTimer.Stop();
        //    _matchEngine.CleanUpgame();

        //    // Final commentary flush at the end of the match
        //    if (CurrentRunMode == MatchRunMode.Watch)
        //    {
        //        while (MyLogger.HasPendingUiCommentary())
        //        {
        //            await MyLogger.DisplayNextCommentAndAwaitCompletion();
        //            UpdateMatchUI(); // Final UI update after commentary is flushed
        //                             // No explicit await Task.Delay here, as DisplayNextCommentAndAwaitCompletion already handles duration.
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Match simulated to completion. Final Score: {_matchEngine.HomeScore}-{_matchEngine.AwayScore}");
        //    }

        //    MatchEnded?.Invoke(this, EventArgs.Empty);
        //}

        //// Modify UpdateMatchUI: No longer deals with commentary
        //private void UpdateMatchUI()
        //{
        //    // Removed: Commentary getting and raising logic.
        //    // This method now *only* raises the MatchStateUpdated event.
        //    MatchStateUpdated?.Invoke(this, new MatchStateUpdatedEventArgs
        //    {
        //        CurrentMinute = _matchEngine.CurrentMinute,
        //        CurrentSecond = _matchEngine.CurrentSecond,
        //        Half = _matchEngine.CurrentHalf,
        //        HomeScore = _matchEngine.HomeScore,
        //        AwayScore = _matchEngine.AwayScore
        //        // ... populate other state data ...
        //    });
        //}
    }
}
