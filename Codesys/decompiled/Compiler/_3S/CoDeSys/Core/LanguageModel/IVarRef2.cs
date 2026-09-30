using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVarRef2 : IVarRef
	{
		int SignatureId { get; }
	}
}
