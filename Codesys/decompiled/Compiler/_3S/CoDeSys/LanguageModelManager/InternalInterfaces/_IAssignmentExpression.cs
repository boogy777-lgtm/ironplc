using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IAssignmentExpression : _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IAssignmentExpression
	{
		_IExpression _LValue { get; set; }

		_IExpression _RValue { get; set; }

		new Operator KindOf { get; set; }
	}
}
