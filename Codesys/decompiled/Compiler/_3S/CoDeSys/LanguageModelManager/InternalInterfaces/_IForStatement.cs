using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IForStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IForStatement
	{
		_IExpression _CounterStart { get; set; }

		_IExpression _UpperBound { get; set; }

		_IExpression _By { get; set; }

		_IExpression _Condition { get; set; }

		_IExpression _Counter { get; set; }

		_IStatement _Controlled { get; set; }
	}
}
