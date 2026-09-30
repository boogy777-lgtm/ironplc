using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExpression4 : IExpression3, IExpression2, IExpression, IExprement
	{
		bool IsConstant(IScope scope, bool bAllocatedOK);
	}
}
