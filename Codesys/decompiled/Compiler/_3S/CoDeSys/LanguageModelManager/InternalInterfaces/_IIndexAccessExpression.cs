using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IIndexAccessExpression : _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IIndexAccessExpression
	{
		_IExpression _Var { get; set; }

		ICollection<_IExpression> _Accesses { get; }

		_IExpression this[int i] { get; set; }

		int NumAccesses { get; }

		_IExpression GetAccess(int i);

		void AddAccess(_IExpression expAcc);
	}
}
