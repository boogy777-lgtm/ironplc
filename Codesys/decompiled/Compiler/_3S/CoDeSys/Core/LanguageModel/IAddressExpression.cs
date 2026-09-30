using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAddressExpression : IExpression2, IExpression, IExprement
	{
		IDirectVariable DirectAddress { get; }
	}
}
