using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IGreenTreeTables
	{
		_IBaseExpression BaseExpression { get; }

		_IContinueStatement ContinueStatement { get; }

		_IEmptyStatement EmptyStatement { get; }

		_IExitStatement ExitStatement { get; }

		IDictionary<uint, _IExprement> ExprTable { get; }

		_ILiteralExpression FalseExpression { get; }

		IDictionary<long, _ILiteralExpression> IntegerTable { get; }

		_IThisExpression ThisExpression { get; }

		_ILiteralExpression TrueExpression { get; }

		_IReturnStatement UnconditionalReturnStatement { get; }

		IDictionary<string, _IVariableExpression> VarTable { get; }

		object TableLock { get; }

		void AddExprementToHashTable(_IExprement expr, uint hash);

		void Clear();

		_IAssignmentExpression GetHashedGreenAssignmentExpression(_IAssignmentExpression assign, out bool bConflict, out uint hash);

		_ICompoAccessExpression GetHashedGreenCompoAccess(_ICompoAccessExpression compo, out bool bConflict, out uint hash);

		_IDeRefAccessExpression GetHashedGreenDerefAccessExpression(_IDeRefAccessExpression assign, out bool bConflict, out uint hash);

		_IIndexAccessExpression GetHashedGreenIndexAccessExpression(_IIndexAccessExpression assign, out bool bConflict, out uint hash);

		_IOperatorExpression GetHashedGreenOperatorExpression(_IOperatorExpression op, out bool bConflict, out uint hash);
	}
}
