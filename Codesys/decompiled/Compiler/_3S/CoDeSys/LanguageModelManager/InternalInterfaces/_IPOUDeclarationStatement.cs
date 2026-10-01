using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IPOUDeclarationStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IPOUDeclarationStatement
	{
		_IExpression NameExpression { get; set; }

		IList<_IExpression> Extends { get; }

		IList<_IExpression> Implements { get; }

		_IStatement Declarations { get; set; }

		_IType Type { get; set; }

		void AddInterfaceImplementation(_IExpression expImplements);

		void SetAccessFlag(SignatureFlag sf);
	}
}
