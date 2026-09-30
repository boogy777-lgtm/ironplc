using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPOUDeclarationStatement : IStatement, IExprement
	{
		Operator Class { get; set; }

		SignatureFlag Access { get; set; }

		string Name { get; set; }

		List<IExpression> ExtendsList { get; set; }

		List<IExpression> ImplementsList { get; set; }

		IType ReturnType { get; set; }

		List<IVariableDeclarationListStatement> DeclarationLists { get; set; }

		void AddVariableDeclaration(VarFlag vfFlag, string stName, TypeClass tc, IExpression expInitial);

		void AddVariableDeclaration(VarFlag vfFlag, string stName, ICompiledType ctype, IExpression expInitial);

		void SetReturnType(TypeClass tc);
	}
}
