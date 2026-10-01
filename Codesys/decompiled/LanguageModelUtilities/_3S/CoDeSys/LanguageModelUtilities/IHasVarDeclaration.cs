using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IHasVarDeclaration
	{
		string Name { get; set; }

		void Initialize(ILanguageModelBuilder lmbuilder, Guid gdObject, Guid gdMessage);

		void AddVariableDeclaration(IExprementPosition pos, VarFlag eVarFlag, string stVariableName, TypeClass typeClass);

		void AddVariableDeclaration(IExprementPosition pos, VarFlag eVarFlag, string stVariableName, TypeClass typeClass, IExpression expInitial);

		void AddVariableDeclaration(IExprementPosition pos, VarFlag eVarFlag, string stVariableName, string stComplexType);

		void AddVariableDeclaration(IExprementPosition pos, VarFlag eVarFlag, string stVariableName, string stComplexType, IExpression expInitial);

		void AddAttribute(IExprementPosition pos, VarFlag eVarFlag, string stAttribute);

		void AddAttribute(IExprementPosition pos, VarFlag eVarFlag, string stAttribute, string stAttributeValue);

		void AddToLanguageModel(IExprementPosition pos, ILanguageModel languageModel);
	}
}
