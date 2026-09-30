using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface _IEmbeddedLanguageStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement
	{
		string Kind { get; set; }

		ArraySegment<char> RawSegment { get; set; }
	}
}
