using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDataManager4 : IDataManager3, IDataManager2, IDataManager
	{
		IEnumerable<IDataSegment> DataSegments { get; }
	}
}
