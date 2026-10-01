using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IIdentifierInfo
	{
		string Name { get; }

		string Comment { get; }

		IdentifierInfoFlag Flags { get; }

		IType Type { get; }

		IVariable Variable { get; }

		ISignature Signature { get; }

		IScope Scope { get; }
	}
}
