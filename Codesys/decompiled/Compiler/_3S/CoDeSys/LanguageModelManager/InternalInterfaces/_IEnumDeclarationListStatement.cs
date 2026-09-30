using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IEnumDeclarationListStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IEnumDeclarationListStatement
	{
		ICollection<_IEnumDeclarationStatement> Enums { get; }

		_IType _BaseType { get; set; }

		_IVariableExpression _DefaultValue { get; set; }

		void AddEnumDeclaration(string stName, _IExpression expInit, _ISequenceStatement seqOptAttributesEtc, IToken token);

		void AddEnumDeclaration(_IEnumDeclarationStatement eds);
	}
}
