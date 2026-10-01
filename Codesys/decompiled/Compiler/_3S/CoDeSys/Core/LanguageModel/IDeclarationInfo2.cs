using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDeclarationInfo2 : IDeclarationInfo
	{
		string Namespace { get; }
	}
}
