using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompilerVersionManager4 : ICompilerVersionManager3, ICompilerVersionManager2, ICompilerVersionManager
	{
		Version CompilerVersionToUse(int nProjectHandle);
	}
}
