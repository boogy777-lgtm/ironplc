using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ITypeDeclarationStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, ITypeDeclarationStatement
	{
		_IType Type { get; set; }

		new string Name { get; set; }

		_IExpression NameExpression { get; set; }

		_IExpression Extends { get; set; }

		_IExpression Initial { get; set; }

		_IStatement Declarations { get; set; }

		new SignatureFlag Flags { get; set; }

		_IExpression _DefaultValue { get; set; }
	}
}
