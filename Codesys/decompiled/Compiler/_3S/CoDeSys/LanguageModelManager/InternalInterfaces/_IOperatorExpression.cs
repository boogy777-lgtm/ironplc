using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IOperatorExpression : _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IOperatorExpression
	{
		IList<_IExpression> _OperandsList { get; }

		_IExpression this[int i] { get; set; }

		bool PositionOK { get; set; }

		bool AllPositionsAllowed { get; }

		new Operator Code { get; set; }

		void AddOperand(_IExpression exp);

		void AcceptOperatorVisitor(IOperatorExpressionVisitor visitor);
	}
}
