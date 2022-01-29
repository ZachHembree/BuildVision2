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
        public IReadOnlyList<IBlockMember> PrioritizedMembers => _prioritizedMembers;

        private static readonly IReadOnlyDictionary<string, uint> GlobalPreset = new Dictionary<string, uint>()
        {
            { "Name", 10 },
            { "CustomName", 10 },
            { "ChargeMode", 5 },
            { "OnOff", 10 },
            { "Stockpile", 5 },
            { "Override", 5 },
            { "UpperLimit", 5 },
        };

        private readonly Dictionary<Type, Dictionary<string, uint>> defaultPrioritiesTable;

        private readonly List<IBlockMember> _prioritizedMembers;
        private readonly List<uint> weightedIndices;
        private readonly List<int> prioritizedIndices;

        private IReadOnlyList<IBlockMember> blockMembers;
        private Dictionary<string, uint> priorityMap, defaultPriorities;
        private int memberLimit;

        public PrioritizedBlockMembers()
        {
            defaultPrioritiesTable = new Dictionary<Type, Dictionary<string, uint>>();

            _prioritizedMembers = new List<IBlockMember>();
            weightedIndices = new List<uint>();
            prioritizedIndices = new List<int>();
        }

        public void SetMembers(int memberLimit, Type type, IReadOnlyList<IBlockMember> blockMembers)
        {
            ClearMembers();

            if (!defaultPrioritiesTable.ContainsKey(type))
                GenerateTypePreset(type);

            this.memberLimit = memberLimit;
            this.blockMembers = blockMembers;

            defaultPriorities = defaultPrioritiesTable[type];
            priorityMap = defaultPriorities;

            GetMemberPriorities();
            GetPrioritizedMembers();
        }

        public void ClearMembers()
        {
            blockMembers = null;
            priorityMap = null;
            prioritizedIndices.Clear();
            _prioritizedMembers.Clear();
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
                    if (member is IBlockAction)
                        priority = 9;
                    else if (member is IBlockNumericValue<float>)
                        priority = 2;
                    else if (member is IBlockNumericValue<Color>)
                        priority = 2;
                    else
                        priority = 1;
                }

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

                if (!priorityMap.TryGetValue(member.PropName, out priority))
                    priority = defaultPriorities[member.PropName];

                weightedIndices.Add((priority << 16) | i);
            }

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

            prioritizedIndices.Sort();

            foreach (int index in prioritizedIndices)
            {
                _prioritizedMembers.Add(blockMembers[index]);
            }
        }
    }
}