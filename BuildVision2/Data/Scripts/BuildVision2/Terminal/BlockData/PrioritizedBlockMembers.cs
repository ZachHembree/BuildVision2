using RichHudFramework;
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
    public class PrioritizedBlockMembers
    {
        public IReadOnlyList<int> MemberIndices => prioritizedIndices;

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
            { "UpperLimit", 5 },
            { "Displacement", 5 },
            { "LowerLimit", 5 },
        };

        private readonly Dictionary<Type, Dictionary<string, uint>> defaultPrioritiesTable;

        private readonly List<uint> weightedIndices;
        private readonly List<int> prioritizedIndices;

        private IReadOnlyList<IBlockMember> blockMembers;
        private Dictionary<string, uint> priorityMap, defaultPriorities;
        private int memberLimit;

        public PrioritizedBlockMembers()
        {
            defaultPrioritiesTable = new Dictionary<Type, Dictionary<string, uint>>();

            weightedIndices = new List<uint>();
            prioritizedIndices = new List<int>();
        }

        public void SetMembers(int memberLimit, Type type, 
            IReadOnlyList<IBlockMember> blockMembers, Dictionary<string, uint> priorityMap = null)
        {
            ClearMembers();

            this.memberLimit = memberLimit;
            this.blockMembers = blockMembers;

            if (!defaultPrioritiesTable.ContainsKey(type))
                GenerateTypePreset(type);

            defaultPriorities = defaultPrioritiesTable[type];
            this.priorityMap = priorityMap ?? defaultPriorities;

            GetMemberPriorities();
            GetPrioritizedMembers();
        }

        public void ClearMembers()
        {
            blockMembers = null;
            priorityMap = null;
            prioritizedIndices.Clear();
            weightedIndices.Clear();
        }

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

            // Sort ascending
            weightedIndices.Sort();
        }

        private void GetPrioritizedMembers()
        {
            int remaining = memberLimit;

            for (int i = weightedIndices.Count - 1; i >= 0; i--)
            {
                uint weightedIndex = weightedIndices[i];
                int index = (int)(weightedIndex & 0xFFFF);
                prioritizedIndices.Add(index);

                remaining--;

                if (remaining <= 0)
                    break;
            }

            // Sort by index to restore normal order
            prioritizedIndices.Sort();
        }
    }
}