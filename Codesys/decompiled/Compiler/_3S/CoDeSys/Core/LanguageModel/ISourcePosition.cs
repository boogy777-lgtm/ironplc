using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ISourcePosition
	{
		int ProjectHandle { get; }

		Guid ObjectGuid { get; }

		long Position { get; }

		short PositionOffset { get; }

		long PositionCombination { get; }

		short Length { get; }
	}
}
