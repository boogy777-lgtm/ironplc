using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface IAliasTypeSerializable
	{
		void SetEffectiveType(ICompiledType effectiveType);

		void BeforeSerialize();
	}
}
