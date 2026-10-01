using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IIdentifierInfo2 : IIdentifierInfo
	{
		ISignature ContainingSignature { get; }
	}
}
