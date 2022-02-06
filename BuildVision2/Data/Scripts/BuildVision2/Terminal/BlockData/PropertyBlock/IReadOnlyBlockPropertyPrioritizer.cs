using System;
using System.Collections.Generic;

namespace DarkHelmet.BuildVision2
{
    public interface IReadOnlyBlockPropertyPrioritizer
    {
        /// <summary>
        /// Member indices with their associated weights. Weights in upper 16 bits; indices in lower 16.
        /// </summary>
        IReadOnlyList<uint> WeightedIndices { get; }

        /// <summary>
        /// Returns true if the member at the given index is enabled and has priority
        /// </summary>
        bool GetIsMemberEnabledAndPrioritized(int index);
    }
}