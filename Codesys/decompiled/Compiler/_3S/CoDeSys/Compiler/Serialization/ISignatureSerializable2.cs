using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface ISignatureSerializable2 : ISignatureSerializable
	{
		SignatureFlagInternal InternalFlags { get; set; }
	}
}
