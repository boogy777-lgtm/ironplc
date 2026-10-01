using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDeclarationInfo4 : IDeclarationInfo3, IDeclarationInfo2, IDeclarationInfo
	{
		ISourcePosition Position { get; }
	}
}
