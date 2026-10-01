using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IBitAccess : IExpression2, IExpression, IExprement
	{
		byte BitNr { get; }

		IExpression Base { get; }
	}
}
