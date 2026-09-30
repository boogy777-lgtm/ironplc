using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IDefineStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IDefineStatement
	{
		new bool Define { get; set; }

		new string Ident { get; set; }

		new string Value { get; set; }
	}
}
