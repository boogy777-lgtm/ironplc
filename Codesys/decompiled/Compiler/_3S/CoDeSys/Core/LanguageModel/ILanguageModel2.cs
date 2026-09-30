using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModel2 : ILanguageModel
	{
		IEnumerable<IStaticMemorySegment> StaticMemorySegments { get; }

		void AddStaticMemorySegment(Guid guidApplication, int nOffset, int nSize);
	}
}
