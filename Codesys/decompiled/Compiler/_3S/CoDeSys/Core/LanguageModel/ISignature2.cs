using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ISignature2 : ISignature
	{
		IExpression NameExpression { get; }

		IExpression BaseExpression { get; }

		IExpression[] InterfaceExpressions { get; }
	}
}
