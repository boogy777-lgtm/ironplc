using System.IO;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ILMSerializationService2 : ILMSerializationService
	{
		void SerializeCompileContext(BinaryWriter bw, ICompileContextSerializable comcon);

		_ICompileContext2 DeSerializeCompileContext(BinaryReader br, ITreeFactory redFactory);

		void SerializeTextualPrecompileCrossReferences(BinaryWriter bw, ITextualPreCompCrossReferencesSerializable pCCR);

		void DeSerializeTextualPrecompileCrossReferences(BinaryReader br, _IPreCompCrossReferences pCCR);
	}
}
