using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ILanguageModelBuilderHelper
	{
		ILanguageModelBuilder6 LMB { get; }

		ILanguageModel LanguageModel { get; }

		void Initialize(ILanguageModelBuilder6 lmb, int projectHandle, Guid objectGuid);

		void SetLastPosition(IExprementPosition position);

		ICompiledType CreateSimpleType(string stType);

		IVariableExpression2 Var(string expr);

		IOperatorExpression Adr(IExpression expr);

		ILiteralExpression Lit(long l);

		T Dup<T>(T exprement) where T : IExprement;

		IStatement CreateAssignToCompo(IExpression leftSide, string identRight, IExpression assignmentValue);

		IExpressionStatement Assign(IExpression expLeft, IExpression expRight);

		IOperatorExpression Op(Operator op, IExpression expSingleOp);

		ICompoAccessExpression Compo(string varLeft, string varRight);

		ICompoAccessExpression Compo(IExpression varLeft, string varRight);

		IExpression DerefCompo(IExpression lvalue, string varRight);

		IExpression DerefCompo(IExpression lvalue, IVariableExpression2 rvalue);

		IExpression CreateLiteralArrayAccess(string array, int index);

		IExpression CreateVariableArrayAccess(string array, string index);

		IExpressionStatement Stmt(IExpression expr);

		IStatement CreateCallInputsOnly(IExpression callee, params IExpression[] inputArgs);

		IExpression CreateMethodCallExpression(IExpression objectExpr, string methodName, params IExpression[] arguments);

		IExpression CreateMethodCallExpression(string unqualifiedObject, string methodName, params IExpression[] arguments);
	}
}
