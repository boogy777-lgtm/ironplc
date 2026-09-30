using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IEnumDeclarationStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IEnumDeclarationStatement
	{
		_IExpression _Value { get; set; }

		new string Name { get; set; }

		_IStatement AttributesEtc { get; set; }
	}
}
