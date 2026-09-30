using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompiledCode4 : ICompiledCode3, ICompiledCode2, ICompiledCode, ICloneable
	{
		bool GetFlag(CompiledCodeFlags flag);

		void SetFlag(CompiledCodeFlags flag, bool bSet);
	}
}
