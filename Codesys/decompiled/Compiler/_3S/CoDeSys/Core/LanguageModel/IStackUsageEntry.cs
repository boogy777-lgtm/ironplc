using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IStackUsageEntry
	{
		_ISignature SignCalled { get; }

		_ISignature SignImplemented { get; }

		int StackSize { get; }

		bool IsHiddenSignature { get; }

		ISourcePosition SourcePosition { get; }
	}
}
