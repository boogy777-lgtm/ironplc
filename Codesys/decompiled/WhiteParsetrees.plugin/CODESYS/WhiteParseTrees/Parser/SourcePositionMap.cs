using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Parser
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class SourcePositionMap : ISourcePositionMap
	{
		private SortedList<int, long> PositionTable { get; } = new SortedList<int, long>();


		private Dictionary<long, int> PositionOffsets { get; } = new Dictionary<long, int>();


		public void AddNewStartPosition(int sourceTextOffset, long sourceEditorPosition)
		{
			PositionTable.Add(sourceTextOffset, sourceEditorPosition);
			PositionOffsets.Add(sourceEditorPosition, sourceTextOffset);
		}

		public void GetPositionOffsetForTextOffset(int sourceTextOffset, out long position, out short offset)
		{
			int index = FindNearestPositionInList(sourceTextOffset);
			position = PositionTable.Values[index];
			offset = (short)(sourceTextOffset - PositionTable.Keys[index]);
		}

		public bool GetTextOffsetForPositionOffset(long position, short offset, out int textOffset)
		{
			textOffset = -1;
			if (PositionOffsets.ContainsKey(position))
			{
				textOffset = PositionOffsets[position] + offset;
				return true;
			}
			return false;
		}

		private int FindNearestPositionInList(int sourceTextOffset)
		{
			int num = PositionTable.Count / 2;
			int num2 = 0;
			int num3 = PositionTable.Count;
			while (PositionTable.Keys[num] > sourceTextOffset || (num != PositionTable.Count - 1 && PositionTable.Keys[num + 1] <= sourceTextOffset))
			{
				if (sourceTextOffset <= PositionTable.Keys[num])
				{
					num3 = num;
				}
				else
				{
					num2 = num;
				}
				num = num2 + (num3 - num2) / 2;
			}
			return num;
		}
	}
}
