using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IPragmaOperatorExpression : _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IPragmaOperatorExpression
	{
		PragmaOperator Code { get; set; }

		new Operator Operator { get; set; }

		IList<_IExpression> Operands { get; }

		void AddOperand(_IExpression exp);
	}
}
