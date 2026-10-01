using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ICallExpression : _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ICallExpression4, ICallExpression3, ICallExpression2, ICallExpression
	{
		_IExpression _Callee { get; set; }

		IList<_IExpression> Inputs { get; }

		IList<_IExpression> Outputs { get; }

		IList<_IExpression> EmptyAssigns { get; }

		IList<_IAssignmentExpression> _InputAssigns { get; }

		IList<_IAssignmentExpression> _OutputAssigns { get; }

		IList<_IExpression> ParamExpressions { get; }

		_IExpression this[int i] { get; set; }

		IList<_IExpression> OutputExpressions { get; }

		ICallExprInfo CallInfo { get; set; }

		_IExpression _Condition { get; set; }

		_IType ExpectedType { get; set; }

		void RemoveInputAt(int i);

		void InsertParam(int index, _IExpression exp, _IExpression expVariable);

		void AddParam(_IExpression exp, _IExpression expVariable);

		void AddParam(_IExpression exp);

		void SetFormalParam(_IExpression expInput, int iIndex);

		void SetActualParam(_IExpression expInput, int iIndex);

		void SetActualOutput(_IExpression exp, int iIndex);

		void SetFormalOutput(_IExpression exp, int iIndex);

		void AddOutput(_IExpression exp, _IExpression expVariable);

		void AddEmptyAssign(_IExpression exp);
	}
}
