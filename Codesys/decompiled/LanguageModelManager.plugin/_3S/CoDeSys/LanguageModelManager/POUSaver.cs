using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000038 RID: 56
	internal class POUSaver : ILMItemSaver
	{
		// Token: 0x060002B7 RID: 695 RVA: 0x0000A924 File Offset: 0x00009924
		private POUSaver(ISharedDataStorage sharedDataStorage, ICompiledPOU cpou)
		{
			this._sharedDataStorage = sharedDataStorage;
			this._cpou = cpou;
		}

		// Token: 0x060002B8 RID: 696 RVA: 0x0000A93A File Offset: 0x0000993A
		public void SaveToStream(IArchiveWriter2 writer)
		{
			POUSaver.SaveParseTreeToArchive(this._cpou, writer, this._sharedDataStorage);
		}

		// Token: 0x060002B9 RID: 697 RVA: 0x0000A950 File Offset: 0x00009950
		internal static void SaveParseTreeToArchive(ICompiledPOU cpou, IArchiveWriter2 writer, ISharedDataStorage sharedDataStorage)
		{
			try
			{
				CompilerProxy.GetCheckerThread().Disable();
				using (new CompiledLibraryStorageFormat())
				{
					writer.Save((IArchivable)((CompiledPOU)cpou).GetSerializableValue("ParseTree"), sharedDataStorage);
				}
			}
			finally
			{
				CompilerProxy.GetCheckerThread().Enable();
			}
		}

		// Token: 0x060002BA RID: 698 RVA: 0x0000A9BC File Offset: 0x000099BC
		internal static void SaveParseTreeAuxiliaries(PreCompileContext precom, IArchiveAuxiliaryWriter auxiliaryWriter, ISharedDataStorage sharedDataStorage)
		{
			foreach (ISignature4 signature in precom.AllSignaturesFlat)
			{
				if (!(signature.ObjectGuid == Guid.Empty) && !signature.GetFlag(SignatureFlag.SuperGlobal))
				{
					ICompiledPOU compiledPOU = precom.GetCompiledPOU(signature.ObjectGuid);
					if (compiledPOU != null)
					{
						POUSaver.SaveParseTreeOfPOU(auxiliaryWriter, sharedDataStorage, signature.ObjectGuid, compiledPOU);
					}
				}
			}
		}

		// Token: 0x060002BB RID: 699 RVA: 0x0000AA40 File Offset: 0x00009A40
		private static void SaveParseTreeOfPOU(IArchiveAuxiliaryWriter auxiliaryWriter, ISharedDataStorage sharedDataStorage, Guid objectGuid, ICompiledPOU cpou)
		{
			POUSaver saver = new POUSaver(sharedDataStorage, cpou);
			Guid guid = objectGuid;
			string stName = guid.ToString() + ".ilm.auxiliary";
			auxiliaryWriter.SaveAuxStream(stName, saver);
		}

		// Token: 0x0400006B RID: 107
		private readonly ISharedDataStorage _sharedDataStorage;

		// Token: 0x0400006C RID: 108
		private readonly ICompiledPOU _cpou;

		// Token: 0x0400006D RID: 109
		private const string LM_PARSETREE_AUX_EXT = ".ilm.auxiliary";
	}
}
