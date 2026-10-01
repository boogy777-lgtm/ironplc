using System;
using System.Collections.Generic;
using System.IO;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ILMSerializationService
	{
		_IExprement DeSerializeRedTree(BinaryReader br, ITreeFactory factory);

		void SerializeRedTree(BinaryWriter bw, _IExprement exp);

		void SerializeTable(BinaryWriter bw, IGreenTreeTables tables, IDictionary<_IExprement, int> tableToFill);

		void SerializeGreenTree(BinaryWriter bw, IDictionary<_IExprement, int> hashedExpressions, _IExprement exp);

		void DeSerializeExprementTable(BinaryReader br, ITreeFactory factory, IList<_IExprement> exptableToFill, IGreenTreeTables tables);

		_IExprement DeSerializeGreenTree(BinaryReader br, ITreeFactory factory, IList<_IExprement> exptable);

		void SerializeVariable(BinaryWriter bw, _IVariable variable);

		_IVariable DeSerializeVariable(BinaryReader br, ITreeFactory factory);

		void SerializeSignature(BinaryWriter bw, _ISignature signature);

		_ISignature2 DeSerializeSignature(BinaryReader br, ITreeFactory factory);

		void SerializeCompiledPou(BinaryWriter bw, _ICompiledPOU cpou, IDictionary<_IExprement, int> hashedExpressions);

		_ICompiledPOU2 DeSerializeCompiledPou(BinaryReader br, ITreeFactory redFactory, ITreeFactory greenFactory, IList<_IExprement> exptable);

		void SerializePrecompileContext(BinaryWriter bw, _IPreCompileContext2 preComCon, IDictionary<_IExprement, int> hashedExpressions);

		_IPreCompileContext2 DeSerializePrecompileContext(BinaryReader br, ITreeFactory redFactory, ITreeFactory greenFactory, IList<_IExprement> exptable);

		void SerializeLibraryList(BinaryWriter bw, ILMLibraryList2 libList, Guid appGuid, string precomAppGuidLibraryPath);

		ILMLibraryList2 DeSerializeLibraryList(BinaryReader br, ITreeFactory redFactory, out Guid appGuid, out string precomAppGuidLibraryPath);

		void SerializeApplicationDeviceTable(BinaryWriter bw, _IApplicationDeviceTable2 appDevTable);

		void DeSerializeApplicationDeviceTable(BinaryReader br, _IApplicationDeviceTable2 targetAppDevTable);

		void SerializeRelatedObjectTable(BinaryWriter bw, ILMRelatedObjectTable relatedObjects);

		void DeSerializeRelatedObjectTable(BinaryReader br, ILMRelatedObjectTable targetRelatedObjectTable);
	}
}
