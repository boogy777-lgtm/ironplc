using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IHasTypeExpression : _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IHasTypeExpression
	{
		_IExpression Variable { get; set; }

		new ICompiledType ReferencedType { get; set; }

		bool Exact { get; }

		_IExpression VarRef { get; }
	}
}
