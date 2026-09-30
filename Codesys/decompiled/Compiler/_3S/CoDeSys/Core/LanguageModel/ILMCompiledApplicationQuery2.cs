using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCompiledApplicationQuery2 : ILMCompiledApplicationQuery
	{
		IEnumerable<string> SubElementsWithRange(IType type, string stAccessPath, int nStartIndex, int nEndIndex, GUIHidingFlags eFlagsToConsider, out bool bValid);
	}
}
