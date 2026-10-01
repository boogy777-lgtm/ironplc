using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRelocationList2 : IRelocationList
	{
		IList<IRelocationAreaList2> RelocationAreaListsEx { get; }
	}
}
