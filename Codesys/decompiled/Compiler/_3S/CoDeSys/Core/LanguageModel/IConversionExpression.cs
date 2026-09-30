using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IConversionExpression : IExpression2, IExpression, IExprement
	{
		TypeClass From { get; }

		TypeClass To { get; }

		IExpression Exp { get; }

		bool Implicit { get; }
	}
}
