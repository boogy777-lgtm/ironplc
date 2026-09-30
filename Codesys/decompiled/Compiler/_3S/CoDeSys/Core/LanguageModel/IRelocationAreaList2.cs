using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRelocationAreaList2 : IRelocationAreaList
	{
		IList<IRelocation> RelocationsEx { get; }
	}
}
