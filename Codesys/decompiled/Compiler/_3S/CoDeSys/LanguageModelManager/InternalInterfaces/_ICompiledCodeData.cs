using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ICompiledCodeData : ICompiledCode4, ICompiledCode3, ICompiledCode2, ICompiledCode, ICloneable
	{
		new IRelocationList RelocationList { get; set; }

		int RelatedId { get; }
	}
}
