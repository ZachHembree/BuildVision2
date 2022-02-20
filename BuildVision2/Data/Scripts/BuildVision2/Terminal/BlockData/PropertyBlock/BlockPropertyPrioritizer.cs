﻿using RichHudFramework;
using RichHudFramework.Internal;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Collections.Generic;
using System;
using System.Text;
using VRage;
using VRageMath;
using VRage.ModAPI;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Class responsible for creating lists of block members ordered/prioritized by a given weight
    /// </summary>
    public class BlockPropertyPrioritizer : IReadOnlyBlockPropertyPrioritizer
    {
        /// <summary>
        /// Default weights applied to properties using the given identifer strings
        /// </summary>
        private static readonly IReadOnlyDictionary<string, uint> GlobalPreset = new Dictionary<string, uint>()
        {
            { "Name", 10 },
            { "CustomName", 10 },
            { "OnOff", 10 },
            { "ChargeMode", 5 },
            { "Color", 5 },
            { "Stockpile", 5 },
            { "Override", 5 },
            { "ShareInertiaTensor", 5 },
            { "RotorLock", 5 },
            { "Torque", 5 },
            { "BrakingTorque", 5 },
            { "Velocity", 5 },
            { "UpperLimit", 5 },
            { "LowerLimit", 5 },
            { "Displacement", 5 },
            { "ShowOnHUD", 0 },
            { "ShowInToolbarConfig", 0 },
        };

        /// <summary>
        /// Member indices with their associated weights. Weights in upper 16 bits; indices in lower 16.
        /// </summary>
        public IReadOnlyList<uint> WeightedIndices => weightedIndices;

        /// <summary>
        /// Returns number of prioritized & enabled and prioritized
        /// </summary>
        public int PrioritizedMemberCount => prioritizedMembers.Count;

        /// <summary>
        /// Cached, dynamically generated default priority tables
        /// </summary>
        private readonly Dictionary<Type, Dictionary<string, uint>> defaultPrioritiesTable;

        private readonly List<uint> weightedIndices;
        private readonly List<int> priorityList;
        private readonly HashSet<int> prioritizedMembers;

        private IReadOnlyList<IBlockMember> blockMembers;
        private Dictionary<string, uint> priorityMap, defaultPriorities;
        private int memberLimit;

        public BlockPropertyPrioritizer()
        {
            defaultPrioritiesTable = new Dictionary<Type, Dictionary<string, uint>>();

            weightedIndices = new List<uint>();
            priorityList = new List<int>();
            prioritizedMembers = new HashSet<int>();
        }

        public void SetBlockMembers(int memberLimit, Type type,
            IReadOnlyList<IBlockMember> blockMembers, Dictionary<string, uint> priorityMap = null)
        {
            Reset();

            this.memberLimit = memberLimit;
            this.blockMembers = blockMembers;

            if (!defaultPrioritiesTable.ContainsKey(type))
                GenerateTypePreset(type);

            defaultPriorities = defaultPrioritiesTable[type];
            this.priorityMap = priorityMap ?? defaultPriorities;

            GetMemberPriorities();
        }

        /// <summary>
        /// Returns true if the member at the given index is enabled and has priority
        /// </summary>
        public bool GetIsMemberEnabledAndPrioritized(int index)
        {
            return prioritizedMembers.Contains(index);
        }

        /// <summary>
        /// Updates list of prioritized members from existing weights, excluding disabled members.
        /// </summary>
        public void UpdatePrioritizedMembers()
        {
            int remaining = memberLimit;
            priorityList.Clear();

            for (int i = weightedIndices.Count - 1; i >= 0; i--)
            {
                uint weightedIndex = weightedIndices[i];
                int weight = (int)((weightedIndex >> 16) & 0xFFFF);
                int index = (int)(weightedIndex & 0xFFFF);
                IBlockMember member = blockMembers[index];

                if (member.Enabled && weight > 0)
                {
                    priorityList.Add(index);

                    remaining--;

                    if (remaining <= 0)
                        break;
                }
            }

            // Sort by index to restore normal order
            priorityList.Sort();
            prioritizedMembers.Clear();

            foreach (int index in priorityList)
                prioritizedMembers.Add(index);
        }

        /// <summary>
        /// Resets object for reuse
        /// </summary>
        public void Reset()
        {
            blockMembers = null;
            priorityMap = null;
            priorityList.Clear();
            weightedIndices.Clear();
        }

        /// <summary>
        /// Generates a default weight table for a given type
        /// </summary>
        private void GenerateTypePreset(Type type)
        {
            var newPreset = new Dictionary<string, uint>();

            foreach (IBlockMember member in blockMembers)
            {
                uint priority = 0;

                if (!GlobalPreset.TryGetValue(member.PropName, out priority))
                {
                    if (member.ValueType == BlockMemberValueTypes.None)
                        priority = 9;
                    else if (member.ValueType == BlockMemberValueTypes.Float)
                        priority = 2;
                    else if (member.ValueType == BlockMemberValueTypes.Color)
                        priority = 2;
                    else if (member.ValueType == BlockMemberValueTypes.Combo)
                        priority = 2;
                    else
                        priority = 1;
                }

                if (!newPreset.ContainsKey(member.PropName))
                    newPreset.Add(member.PropName, priority);
            }

            defaultPrioritiesTable.Add(type, newPreset);
        }

        /// <summary>
        /// Assigns weight to each block member from current priority table
        /// </summary>
        private void GetMemberPriorities()
        {
            for (uint i = 0; i < blockMembers.Count; i++)
            {
                IBlockMember member = blockMembers[(int)i];
                uint priority;

                if (!priorityMap.TryGetValue(member.PropName, out priority)
                    && !defaultPriorities.TryGetValue(member.PropName, out priority))
                    priority = 1;

                weightedIndices.Add((priority << 16) | i);
            }

            // Sort ascending to restore index order
            weightedIndices.Sort();
        }
    }
}