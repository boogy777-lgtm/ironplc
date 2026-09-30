using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IStaticMemorySegment
	{
		Guid SubApplicationGuid { get; set; }

		int Offset { get; set; }

		int Size { get; set; }
	}
}
