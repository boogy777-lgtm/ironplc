using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPreCompileContext6 : IPreCompileContext5, IPreCompileContext4, IPreCompileContext3, IPreCompileContext2, IPreCompileContext, ICompileContextCommon
	{
		Version CompilerVersionSavedWith();
	}
}
