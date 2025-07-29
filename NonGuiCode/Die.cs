using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FunnyOldGame
{
    public static class Die // Make Die static if it's a utility for rolling, or make it a singleton
                            // If each Game needs its own Die, then keep it non-static.
                            // For now, let's assume it's a utility class for rolling a "virtual" die.
    {
        // Use ThreadLocal<Random> to ensure each thread has its own, independent Random instance.
        // This eliminates contention (locks) when generating random numbers.
        private static ThreadLocal<Random> _threadRandom = new ThreadLocal<Random>(() =>
        {
            // Use a highly unique seed for each thread's Random instance.
            // Environment.TickCount is not ideal for high-speed parallel scenarios
            // because multiple threads could call it at the same "tick."
            // A better approach is to use a global, interlocked counter or Guid.GetHashCode().
            // For simplicity, let's use a combination with a static Random for initial seed generation.
            return new Random(Guid.NewGuid().GetHashCode()); // More robust unique seed
        });

        // If you need a "die size" concept, the Roll method can take it as a parameter
        public static int Roll(int size) // Rolls a die with 'size' faces (e.g., size 6 for a d6)
        {
            // No lock needed! Each thread uses its own _threadRandom instance.
            return _threadRandom.Value.Next(size) + 1; // Random.Next(maxValue) is exclusive, so +1 for 1-based indexing
        }
    }
}
