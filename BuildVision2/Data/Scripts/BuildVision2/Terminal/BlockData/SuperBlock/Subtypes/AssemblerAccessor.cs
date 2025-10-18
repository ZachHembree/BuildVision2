using RichHudFramework.UI;
using Sandbox.Definitions;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using VRage;
using IMyAssembler = Sandbox.ModAPI.Ingame.IMyAssembler;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
	public partial class SuperBlock
	{
		public AssemblerAccessor Assembler
		{
			get
			{
				return _assembler;
			}
			private set
			{
				_assembler = value;
			}
		}

		private AssemblerAccessor _assembler;

		public class AssemblerAccessor : SubtypeAccessor<IMyAssembler>
		{
			/// <summary>
			/// Returns the total number of items in the queue for assembly/disassembly
			/// </summary>
			public int QueueCount
			{
				get
				{
					queueBuffer.Clear();
					subtype.GetQueue(queueBuffer);

					_queueCount = 0;

					foreach (var itemType in queueBuffer)
						_queueCount += (int)itemType.Amount;

					return _queueCount;
				}
			}

			/// <summary>
			/// Returns the assembler mode - assembly or disassembly
			/// </summary>
			public MyAssemblerMode Mode => subtype.Mode;

			/// <summary>
			/// Returns true if cooperative mode is enabled
			/// </summary>
			public bool IsInCooperativeMode => subtype.CooperativeMode;

			/// <summary>
			/// Returns true if the assembler is set to perpetually repeat its work queue
			/// </summary>
			public bool IsRepeating => subtype.Repeating;

			/// <summary>
			/// Base assembly speed
			/// </summary>
			public float BaseProductivity => assemblerDef?.AssemblySpeed ?? 0f;

			private readonly List<MyProductionItem> queueBuffer;
			private int _queueCount;
			private MyAssemblerDefinition assemblerDef;

			public AssemblerAccessor()
			{
				queueBuffer = new List<MyProductionItem>();
			}

			public override void SetBlock(SuperBlock block)
			{
				base.SetBlock(block, TBlockSubtypes.Assembler);

				if (block.SubtypeId.HasFlag(TBlockSubtypes.Assembler))
				{
					assemblerDef = block.BlockDefinition as MyAssemblerDefinition;
				}
			}

			public override void Reset()
			{
				base.Reset();
				queueBuffer.Clear();
				assemblerDef = null;
			}

			public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
			{
				var label = MyTexts.Get(MySpaceTexts.Assembler_ItemsInQueue);
				var textBuf = block.textBuffer;
				textBuf.Clear();

				// I'll do my own formatting, thanks
				for (int i = 0; i < label.Length; i++)
				{
					if (label[i] == ':')
						break;

					textBuf.Append(label[i]);
				}

				builder.Add($"{textBuf}: ", nameFormat);
				builder.Add($"{QueueCount}\n", valueFormat);
			}
		}
	}
}
