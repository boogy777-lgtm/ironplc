using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IHasConstantValueExpression : IExpression2, IExpression, IExprement
	{
		IExpression Constant { get; }

		ILiteralExpression ConstantValue { get; }
	}
}
