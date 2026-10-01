using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICaseRangeExpression : IExpression2, IExpression, IExprement
	{
		IExpression Low { get; }

		IExpression High { get; }
	}
}
