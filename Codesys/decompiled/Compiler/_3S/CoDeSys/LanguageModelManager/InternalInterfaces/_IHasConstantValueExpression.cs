using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IHasConstantValueExpression : _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IHasConstantValueExpression2, IHasConstantValueExpression
	{
		_IExpression _Constant { get; set; }

		_IExpression _ConstantValue { get; set; }

		Operator _OpComparison { get; set; }
	}
}
