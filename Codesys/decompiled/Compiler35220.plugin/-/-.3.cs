using System;
using System.Collections.Generic;
using System.IO;
using \u0003;
using \u000F;
using \u0016;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Serialization;
using _3S.CoDeSys.Compiler35220.TreeConversion;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001E
{
	// Token: 0x02000015 RID: 21
	internal sealed class \u0001 : IGreenTreeConverter, ILMSerializationService2, ILMSerializationService
	{
		// Token: 0x0600043D RID: 1085 RVA: 0x0000906C File Offset: 0x0000726C
		public _IExprement \u0001(_IExprement \u0002, IGreenTreeTables \u0003, ITreeFactory \u0004, ICompactedParseTreeInformation \u0005)
		{
			return GreenTreeBuilder.BuildGreenTree(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x0600043E RID: 1086 RVA: 0x00009078 File Offset: 0x00007278
		public _IExprement \u0001(_IExprement \u0002, ITreeFactory \u0003, ICompactedParseTreeInformation \u0004)
		{
			return RedTreeBuilder.BuildRedTree(\u0002, \u0003, \u0004);
		}

		// Token: 0x0600043F RID: 1087 RVA: 0x00009084 File Offset: 0x00007284
		public void \u0001(BinaryReader \u0002, ITreeFactory \u0003, IList<_IExprement> \u0004, IGreenTreeTables \u0005)
		{
			global::\u0003.\u0001.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06000440 RID: 1088 RVA: 0x00009090 File Offset: 0x00007290
		public _IExprement \u0001(BinaryReader \u0002, ITreeFactory \u0003, IList<_IExprement> \u0004)
		{
			return global::\u0003.\u0001.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06000441 RID: 1089 RVA: 0x0000909C File Offset: 0x0000729C
		public void \u0001(BinaryWriter \u0002, IGreenTreeTables \u0003, IDictionary<_IExprement, int> \u0004)
		{
			\u001E.\u0002.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06000442 RID: 1090 RVA: 0x000090A8 File Offset: 0x000072A8
		public void \u0001(BinaryWriter \u0002, IDictionary<_IExprement, int> \u0003, _IExprement \u0004)
		{
			\u001E.\u0002.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06000443 RID: 1091 RVA: 0x000090B4 File Offset: 0x000072B4
		public _IExprement \u0001(BinaryReader \u0002, ITreeFactory \u0003)
		{
			return new \u001E.\u0003(\u0002, \u0003).\u0001<_IExprement>(\u0002);
		}

		// Token: 0x06000444 RID: 1092 RVA: 0x000090C4 File Offset: 0x000072C4
		public void \u0001(BinaryWriter \u0002, _IExprement \u0003)
		{
			global::\u000F.\u0004.\u0001(\u0002, \u0003);
		}

		// Token: 0x06000445 RID: 1093 RVA: 0x000090D0 File Offset: 0x000072D0
		public void \u0001(BinaryWriter \u0002, _IVariable \u0003)
		{
			new PrecompileSerializer().SerializeVariable(\u0002, \u0003);
		}

		// Token: 0x06000446 RID: 1094 RVA: 0x000090E0 File Offset: 0x000072E0
		public _IVariable \u0001(BinaryReader \u0002, ITreeFactory \u0003)
		{
			return new global::\u0016.\u0001(\u0002, null, \u0003, (_ILanguageModelBuilder)APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder()).\u0001();
		}

		// Token: 0x06000447 RID: 1095 RVA: 0x00009104 File Offset: 0x00007304
		public void \u0001(BinaryWriter \u0002, _ISignature \u0003)
		{
			new PrecompileSerializer().SerializeSignature(\u0002, (_ISignature3)\u0003);
		}

		// Token: 0x06000448 RID: 1096 RVA: 0x00009118 File Offset: 0x00007318
		public _ISignature2 \u0001(BinaryReader \u0002, ITreeFactory \u0003)
		{
			return new global::\u0016.\u0001(\u0002, null, \u0003, (_ILanguageModelBuilder)APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder()).\u0001();
		}

		// Token: 0x06000449 RID: 1097 RVA: 0x0000913C File Offset: 0x0000733C
		public void \u0001(BinaryWriter \u0002, _ICompiledPOU \u0003, IDictionary<_IExprement, int> \u0004)
		{
			new PrecompileSerializer().SerializeCompiledPou_Green(\u0002, (_ICompiledPOU2)\u0003, \u0004);
		}

		// Token: 0x0600044A RID: 1098 RVA: 0x00009150 File Offset: 0x00007350
		public _ICompiledPOU2 \u0001(BinaryReader \u0002, ITreeFactory \u0003, ITreeFactory \u0004, IList<_IExprement> \u0005)
		{
			return new global::\u0016.\u0001(\u0002, \u0004, \u0003, (_ILanguageModelBuilder)APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder()).\u0001(\u0005);
		}

		// Token: 0x0600044B RID: 1099 RVA: 0x00009178 File Offset: 0x00007378
		public void \u0001(BinaryWriter \u0002, _IPreCompileContext2 \u0003, IDictionary<_IExprement, int> \u0004)
		{
			new PrecompileSerializer().SerializePrecompileContext(\u0002, \u0003, \u0004);
		}

		// Token: 0x0600044C RID: 1100 RVA: 0x00009188 File Offset: 0x00007388
		public _IPreCompileContext2 \u0001(BinaryReader \u0002, ITreeFactory \u0003, ITreeFactory \u0004, IList<_IExprement> \u0005)
		{
			return new global::\u0016.\u0001(\u0002, \u0004, \u0003, (_ILanguageModelBuilder)APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder()).\u0001(\u0005);
		}

		// Token: 0x0600044D RID: 1101 RVA: 0x000091B0 File Offset: 0x000073B0
		public void \u0001(BinaryWriter \u0002, ILMLibraryList2 \u0003, Guid \u0004, string \u0005)
		{
			new PrecompileSerializer().SerializeLibraryList(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x0600044E RID: 1102 RVA: 0x000091C4 File Offset: 0x000073C4
		public ILMLibraryList2 \u0001(BinaryReader \u0002, ITreeFactory \u0003, out Guid \u0004, out string \u0005)
		{
			return new global::\u0016.\u0001(\u0002, null, \u0003, (_ILanguageModelBuilder)APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder()).\u0001(out \u0004, out \u0005);
		}

		// Token: 0x0600044F RID: 1103 RVA: 0x000091EC File Offset: 0x000073EC
		public void \u0001(BinaryWriter \u0002, _IApplicationDeviceTable2 \u0003)
		{
			new PrecompileSerializer().SerializeApplicationDeviceTable(\u0002, \u0003);
		}

		// Token: 0x06000450 RID: 1104 RVA: 0x000091FC File Offset: 0x000073FC
		public void \u0001(BinaryReader \u0002, _IApplicationDeviceTable2 \u0003)
		{
			new global::\u0016.\u0001(\u0002, null, null, (_ILanguageModelBuilder2)APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder()).\u0001(\u0003);
		}

		// Token: 0x06000451 RID: 1105 RVA: 0x00009220 File Offset: 0x00007420
		public void \u0001(BinaryWriter \u0002, ILMRelatedObjectTable \u0003)
		{
			new PrecompileSerializer().SerializeRelatedObjectTable(\u0002, \u0003);
		}

		// Token: 0x06000452 RID: 1106 RVA: 0x00009230 File Offset: 0x00007430
		public void \u0001(BinaryReader \u0002, ILMRelatedObjectTable \u0003)
		{
			new global::\u0016.\u0001(\u0002, null, null, null).\u0001(\u0003);
		}

		// Token: 0x06000453 RID: 1107 RVA: 0x00009244 File Offset: 0x00007444
		public void \u0001(BinaryWriter \u0002, ITextualPreCompCrossReferencesSerializable \u0003)
		{
			new PrecompileSerializer().\u0001(\u0002, \u0003);
		}

		// Token: 0x06000454 RID: 1108 RVA: 0x00009254 File Offset: 0x00007454
		public void \u0001(BinaryReader \u0002, _IPreCompCrossReferences \u0003)
		{
			new global::\u0016.\u0001(\u0002, null, null, null).\u0001(\u0003);
		}

		// Token: 0x06000455 RID: 1109 RVA: 0x00009268 File Offset: 0x00007468
		public void \u0001(BinaryWriter \u0002, ICompileContextSerializable \u0003)
		{
			new CompileContextSerializer().SerializeCompileContext(\u0002, \u0003);
		}

		// Token: 0x06000456 RID: 1110 RVA: 0x00009278 File Offset: 0x00007478
		public _ICompileContext2 \u0001(BinaryReader \u0002, ITreeFactory \u0003)
		{
			return new CompileContextDeSerializer(\u0002, \u0003, (ILMSerializableTypeFactory2)APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder()).\u0001();
		}
	}
}
