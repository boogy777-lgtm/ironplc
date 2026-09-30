using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IScope2 : IScope
	{
		ISignature[] FindSignature(IExpression qne);

		IScope2 FindScope(IExpression qne);
	}
}
