using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCompiledApplicationQuery3 : ILMCompiledApplicationQuery2, ILMCompiledApplicationQuery
	{
		IEnumerable<ISignatureMemberHierachyInfo> GetSubElementsHierarchy(IType type, string stAccessPath, int nStartIndex, int nEndIndex, GUIHidingFlags eFlagsToConsider, out bool bValid);

		IEnumerable<ISignatureMemberHierachyInfo> GetSubElementsHierarchy(string stSignatureName, int nProjectHandle, Guid guidObject, string stAccessPath, int nStartIndex, int nEndIndex, out bool bValid);
	}
}
