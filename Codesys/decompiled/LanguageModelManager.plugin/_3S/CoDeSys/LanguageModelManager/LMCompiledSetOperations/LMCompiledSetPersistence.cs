using System;
using System.IO;
using System.Linq;
using System.Text;
using _3S.CoDeSys.Compiler.LanguageModelServices;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.GreenTrees;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.LMCompiledSetOperations
{
	// Token: 0x020001B8 RID: 440
	public class LMCompiledSetPersistence : ILMPouSetPersistenceService
	{
		// Token: 0x17000846 RID: 2118
		// (get) Token: 0x06001F8D RID: 8077 RVA: 0x00057063 File Offset: 0x00056063
		internal static bool SaveCompileInfoWithNewSerializer
		{
			get
			{
				return APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351700;
			}
		}

		// Token: 0x06001F8E RID: 8078 RVA: 0x00057074 File Offset: 0x00056074
		public ILMCompiledApplicationSet LoadFromFile(string filePath)
		{
			return LMCompiledSetPersistence.LoadFromFile<ILMCompiledApplicationSet>(filePath);
		}

		// Token: 0x06001F8F RID: 8079 RVA: 0x0005707C File Offset: 0x0005607C
		internal static T LoadFromFile<T>(string stReferenceContextPath) where T : class
		{
			if (SideCarEntryHelper.ExistsFromPath(stReferenceContextPath))
			{
				using (Stream stream = SideCarEntryHelper.OpenReadFromPath(stReferenceContextPath))
				{
					return LMCompiledSetPersistence.LoadFromStream<T>(stream);
				}
			}
			T result;
			using (Stream stream2 = APEnvironmentFacade.Instance.FileSystem.OpenStream(stReferenceContextPath, FileMode.Open, FileAccess.Read))
			{
				result = LMCompiledSetPersistence.LoadFromStream<T>(stream2);
			}
			return result;
		}

		// Token: 0x06001F90 RID: 8080 RVA: 0x000570F0 File Offset: 0x000560F0
		internal static T LoadFromStream<T>(Stream stream) where T : class
		{
			byte[] array = new byte[LMCompiledSetPersistence.NewSerializerMagic.Length];
			stream.Read(array, 0, array.Length);
			T result;
			if (array.SequenceEqual(LMCompiledSetPersistence.NewSerializerMagic))
			{
				ILMSerializationService2 ilmserializationService = (ILMSerializationService2)CompilerProxy.SerializationService_OrNull;
				BinaryReader br = new BinaryReader(stream);
				result = (T)((object)ilmserializationService.DeSerializeCompileContext(br, RedTreeFactory.Singleton));
			}
			else
			{
				stream.Seek(0L, SeekOrigin.Begin);
				result = (T)((object)LanguageModelManagerConsolidated.CreateArchiveReader(stream).Load());
			}
			return result;
		}

		// Token: 0x06001F91 RID: 8081 RVA: 0x00057163 File Offset: 0x00056163
		public static void StoreToStream(Stream stream, CompileContext comconToSave)
		{
			if (LMCompiledSetPersistence.SaveCompileInfoWithNewSerializer)
			{
				LMCompiledSetPersistence.SerializeCompileInfo(stream, comconToSave);
				return;
			}
			LMCompiledSetPersistence.SerializeCompileInfoLegacy(stream, comconToSave);
		}

		// Token: 0x06001F92 RID: 8082 RVA: 0x0005717C File Offset: 0x0005617C
		private static void SerializeCompileInfo(Stream fstr, CompileContext comconToSave)
		{
			ILMSerializationService2 ilmserializationService = (ILMSerializationService2)CompilerProxy.SerializationService_OrNull;
			using (BinaryWriter binaryWriter = new BinaryWriter(fstr, Encoding.UTF8, true))
			{
				binaryWriter.Write(LMCompiledSetPersistence.NewSerializerMagic);
				ilmserializationService.SerializeCompileContext(binaryWriter, comconToSave);
			}
		}

		// Token: 0x06001F93 RID: 8083 RVA: 0x000571D0 File Offset: 0x000561D0
		private static void SerializeCompileInfoLegacy(Stream fstr, CompileContext comconToSave)
		{
			IArchiveWriter archiveWriter = LanguageModelManagerConsolidated.CreateArchiveWriter(fstr);
			using (new CompileInfoStorageFormat())
			{
				if (archiveWriter is IArchiveWriter2)
				{
					Profile profile = APEnvironmentFacade.Instance.Profile;
					Profile profile2 = new Profile();
					foreach (Guid plugInGuid in profile.GetEntries())
					{
						profile2.SetVersionConstraint(plugInGuid, profile.GetVersionConstraint(plugInGuid));
					}
					profile2.SetVersionConstraint(LMCompiledSetPersistence.LMMASSEMBLYGUID, new ExactVersionConstraint(APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse()));
					profile2.SetVersionConstraint(LMCompiledSetPersistence.C16XASSEMBLYGUID, new ExactVersionConstraint(APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse()));
					profile2.SetVersionConstraint(LMCompiledSetPersistence.X86ASSEMBLYGUID, new ExactVersionConstraint(APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse()));
					profile2.SetVersionConstraint(LMCompiledSetPersistence.BLACKFINASSEMBLYGUID, new ExactVersionConstraint(APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse()));
					profile2.SetVersionConstraint(LMCompiledSetPersistence.MIPSASSEMBLYGUID, new ExactVersionConstraint(APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse()));
					profile2.SetVersionConstraint(LMCompiledSetPersistence.NIOSASSEMBLYGUID, new ExactVersionConstraint(APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse()));
					profile2.SetVersionConstraint(LMCompiledSetPersistence.PPCASSEMBLYGUID, new ExactVersionConstraint(APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse()));
					profile2.SetVersionConstraint(LMCompiledSetPersistence.SHASSEMBLYGUID, new ExactVersionConstraint(APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse()));
					profile2.SetVersionConstraint(LMCompiledSetPersistence.TRICOREASSEMBLYGUID, new ExactVersionConstraint(APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse()));
					profile2.SetVersionConstraint(LMCompiledSetPersistence.RISCASSEMBLYGUID, new ExactVersionConstraint(APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse()));
					(archiveWriter as IArchiveWriter2).Save(comconToSave, profile2, new MyArchiveReporter());
				}
				else
				{
					archiveWriter.Save(comconToSave);
				}
			}
		}

		// Token: 0x04000633 RID: 1587
		private static readonly byte[] NewSerializerMagic = new Guid("{812E9CC6-0BF5-4F53-A25B-EAB4F8E8F4FB}").ToByteArray();

		// Token: 0x04000634 RID: 1588
		internal static readonly Guid LMMASSEMBLYGUID = new Guid("{F0B1693D-58CA-4ef3-A79F-D3CB923FF030}");

		// Token: 0x04000635 RID: 1589
		private static Guid C16XASSEMBLYGUID = new Guid("{6516B443-CDAB-41f6-A35B-633734078C06}");

		// Token: 0x04000636 RID: 1590
		private static Guid X86ASSEMBLYGUID = new Guid("{4D549ABE-432F-4f5a-9A63-C999E08184B1}");

		// Token: 0x04000637 RID: 1591
		private static Guid BLACKFINASSEMBLYGUID = new Guid("{99AFA4AB-EEFE-427c-B853-2FC30C05A9A2}");

		// Token: 0x04000638 RID: 1592
		private static Guid MIPSASSEMBLYGUID = new Guid("{1EEBE15A-C061-462c-807F-A9F1BF59A58E}");

		// Token: 0x04000639 RID: 1593
		private static Guid NIOSASSEMBLYGUID = new Guid("{969AB52C-C00E-4e99-B69D-7BD77CFDFB3F}");

		// Token: 0x0400063A RID: 1594
		private static Guid PPCASSEMBLYGUID = new Guid("{98B642A9-90AF-4563-9358-5F007CE19839}");

		// Token: 0x0400063B RID: 1595
		private static Guid SHASSEMBLYGUID = new Guid("{E2A16D6C-20E0-44d4-AD31-22ADA0A08240}");

		// Token: 0x0400063C RID: 1596
		private static Guid TRICOREASSEMBLYGUID = new Guid("{E3A5A72B-4AFA-472c-85D7-CCBBB0F526BA}");

		// Token: 0x0400063D RID: 1597
		private static Guid RISCASSEMBLYGUID = new Guid("{07BE82CD-3680-4bcb-A957-5E99570843D7}");
	}
}
