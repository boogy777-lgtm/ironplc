using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IHasConstantTypeExpression : IExpression2, IExpression, IExprement
	{
		IExpression Constant { get; }

		bool ConstantTypeReplaced { get; }
	}
}
