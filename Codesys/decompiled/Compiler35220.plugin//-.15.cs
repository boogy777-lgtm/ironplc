using System;
using \u0003;
using \u0019;
using \u001D;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.ImplicitCode;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0080;
using \u0081;

namespace \u0084
{
	// Token: 0x0200023C RID: 572
	internal static class \u0014
	{
		// Token: 0x060025B1 RID: 9649 RVA: 0x000826B4 File Offset: 0x000808B4
		internal static void \u0001(bool \u0002, bool \u0003, _ICompileContext \u0004, _ICompileContext \u0005, InitExitSignatureInfo \u0006, bool \u0007, Codegeneration \u0008, _ISignature \u000E, ref bool \u000F, bool \u0010, bool \u0011)
		{
			_ICompiledPOU icompiledPOU = \u0080.\u0019.\u0001(\u0004, \u0003, \u000E, \u0005, \u0006, \u0007, \u0010, \u0004.HasByteSupport());
			AfterGenerateGlobalInitEventArgs e = new AfterGenerateGlobalInitEventArgs(\u0004.ApplicationGuid, icompiledPOU);
			APEnvironmentFacade.Instance.LanguageModelMgr.OnAfterGenerateGlobalInitCode(e);
			\u0084.\u0014.\u0001(icompiledPOU, \u0002, \u0003, \u0004, \u0005, \u0007, \u0008, ref \u000F, \u0010, \u0011);
			foreach (_ICompiledPOU u in \u0080.\u0019.\u0001(\u0004, \u0006))
			{
				\u0084.\u0014.\u0001(u, \u0002, \u0003, \u0004, \u0005, \u0007, \u0008, ref \u000F, \u0010, \u0011);
			}
		}

		// Token: 0x060025B2 RID: 9650 RVA: 0x00082758 File Offset: 0x00080958
		internal static void \u0001(bool \u0002, bool \u0003, _ICompileContext \u0004, _ICompileContext \u0005, InitExitSignatureInfo \u0006, bool \u0007, Codegeneration \u0008, ref bool \u000E, bool \u000F, bool \u0010)
		{
			_ICompiledPOU icompiledPOU = \u001D.\u000F.\u0001(\u0004, \u0004, \u0006);
			if (icompiledPOU != null)
			{
				\u0084.\u0014.\u0001(icompiledPOU, \u0002, \u0003, \u0004, \u0005, \u0007, \u0008, ref \u000E, \u000F, \u0010);
			}
			foreach (_ICompiledPOU u in \u001D.\u000F.\u0001(\u0004, \u0006))
			{
				\u0084.\u0014.\u0001(u, \u0002, \u0003, \u0004, \u0005, \u0007, \u0008, ref \u000E, \u000F, \u0010);
			}
		}

		// Token: 0x060025B3 RID: 9651 RVA: 0x000827D4 File Offset: 0x000809D4
		internal static _ICompilerMessage[] \u0001(_ICompiledPOU \u0002)
		{
			ErrorVisitor errorVisitor = new ErrorVisitor();
			\u0002.Accept(errorVisitor);
			return errorVisitor._Messages;
		}

		// Token: 0x060025B4 RID: 9652 RVA: 0x000827F4 File Offset: 0x000809F4
		private static void \u0001(_ICompiledPOU \u0002, bool \u0003, bool \u0004, _ICompileContext \u0005, _ICompileContext \u0006, bool \u0007, Codegeneration \u0008, ref bool \u000E, bool \u000F, bool \u0010)
		{
			_ISignature isignature = \u0005[\u0002.SignatureId];
			Debug.\u0001(isignature != null);
			_ICompilerMessage[] array = \u0084.\u0014.\u0001(\u0002);
			foreach (_ICompilerMessage icompilerMessage in array)
			{
				if (icompilerMessage.Severity == Severity.Error || icompilerMessage.Severity == Severity.FatalError)
				{
					\u000E = true;
					icompilerMessage.Text = \u0081.\u0001.GlobalInitErrorPrefix + icompilerMessage.Text;
					icompilerMessage.Severity = Severity.FatalError;
					\u0005.AddCompiledPOU(\u0002, isignature, true, null);
					\u0002.SetMessages(array);
					return;
				}
			}
			\u0008.\u0003(\u0002, isignature);
			\u0002.SetFlag(CompiledPOUFlags.ToCompile, true);
			bool flag = \u0004;
			_ISignature isignature2 = (\u0006 != null) ? \u0006[\u0002.SignatureId] : null;
			_ICompiledPOU icompiledPOU = (isignature2 != null) ? \u0006._GetCompiledPOUById(isignature2.Id) : null;
			if ((\u0003 || \u0006 != null) && !flag && icompiledPOU != null)
			{
				flag = Helper.\u0001(\u0002.CompiledCode, icompiledPOU.CompiledCode);
			}
			if (\u0007 || flag)
			{
				Debug.\u0001(icompiledPOU != null, "cpouGlobalInitExitRef != null");
				\u0002.CompiledCode = icompiledPOU.CompiledCode;
				if (\u0005.DataManager._MemorySettings.OnlineChangeInOwnSegment)
				{
					ushort u = 0;
					int u2 = 0;
					if (!MemoryCompiler.\u0003(\u0005.DataManager, ref u, ref u2, \u0005.DataManager.PackMode, \u0002.CompiledCode.CodeSize, \u0005.DataManager._MemorySettings.CodeSegmentSize, DataSegmentFlags.Code))
					{
						string u3 = global::\u0003.\u0006.\u0001(MessageId.Err_OutOfCodeMemory, new object[]
						{
							\u0002.Name,
							\u0002.CompiledCode.CodeSize
						});
						_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u3, Severity.Error, MessageId.Err_OutOfCodeMemory);
						APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message);
						\u000E = true;
					}
					else
					{
						\u0002.CompiledCode.Location = global::\u0019.\u0003.\u0001(u, u2);
					}
				}
				else
				{
					MemoryCompiler.\u0002(\u0005.DataManager, \u0002.CompiledCode.Location.Area, \u0002.CompiledCode.Location.Offset, \u0002.CompiledCode.CodeSize, DataSegmentFlags.None);
				}
				if (!\u0007)
				{
					\u0002.SetFlag(CompiledPOUFlags.ToCompile, false);
				}
			}
			else
			{
				ushort u4 = 0;
				int u5 = 0;
				if (!MemoryCompiler.\u0003(\u0005.DataManager, ref u4, ref u5, \u0005.DataManager.PackMode, \u0002.CompiledCode.CodeSize, \u0005.DataManager._MemorySettings.CodeSegmentSize, DataSegmentFlags.Code))
				{
					string u6 = global::\u0003.\u0006.\u0001(MessageId.Err_OutOfCodeMemory, new object[]
					{
						\u0002.Name,
						\u0002.CompiledCode.CodeSize
					});
					_ICompilerMessage message2 = global::\u0019.\u0003.\u0001(null, u6, Severity.Error, MessageId.Err_OutOfCodeMemory);
					APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message2);
					\u000E = true;
				}
				else
				{
					\u0002.CompiledCode.Location = global::\u0019.\u0003.\u0001(u4, u5);
				}
			}
			if (\u0010)
			{
				ICompiledPOU compiledPOUById = \u0005.GetCompiledPOUById(\u0002.SignatureId);
				if (compiledPOUById != null)
				{
					\u0005.RemoveCompiledPOU(compiledPOUById as _ICompiledPOU);
				}
				\u0005.AddCompiledPOUSimple(\u0002);
				return;
			}
			\u0005.AddCompiledPOU(\u0002, isignature, true, null);
		}
	}
}
