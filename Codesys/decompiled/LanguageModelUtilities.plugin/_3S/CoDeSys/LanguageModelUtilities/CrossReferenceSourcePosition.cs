using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class CrossReferenceSourcePosition : ISourcePosition
	{
		public int ProjectHandle { get; private set; }

		public Guid ObjectGuid { get; private set; }

		public long Position { get; private set; }

		public short PositionOffset { get; private set; }

		public long PositionCombination { get; private set; }

		public short Length { get; private set; }

		public CrossReferenceSourcePosition(int nProjectHandle, Guid objectGuid, long position, short? offset, short length)
		{
			ProjectHandle = nProjectHandle;
			ObjectGuid = objectGuid;
			if (!offset.HasValue)
			{
				PositionCombination = position;
				long position2 = default(long);
				short positionOffset = default(short);
				PositionHelper.SplitPosition(position, ref position2, ref positionOffset);
				Position = position2;
				PositionOffset = positionOffset;
			}
			else
			{
				Position = position;
				PositionOffset = offset.Value;
				PositionCombination = PositionHelper.CombinePosition(Position, PositionOffset);
			}
			Length = length;
		}
	}
}
