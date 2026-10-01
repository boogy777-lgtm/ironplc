using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompiledCode5 : ICompiledCode4, ICompiledCode3, ICompiledCode2, ICompiledCode, ICloneable
	{
		void FinishRelocations(ICompileContext comcon);
	}
}
