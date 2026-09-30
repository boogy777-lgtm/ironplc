using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface INotAllowedSignatureInContainerLibIssue : IContainerLibIssue, IExternalPluginFoundIssue
	{
		ISignature Signature { get; }
	}
}
