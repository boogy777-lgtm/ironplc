using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface IDirectVariableCrossRefTableSerializable
	{
		void AddCrossReference(IDirectVariable dirvar, int nSignatureId, IAddressCodePosition codepos);
	}
}
