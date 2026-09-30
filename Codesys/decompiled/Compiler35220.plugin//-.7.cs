using System;
using System.Collections.Generic;
using \u0004;
using \u0007;
using \u0011;
using \u0014;
using \u0017;
using \u0018;
using \u0019;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0083
{
	// Token: 0x020002FD RID: 765
	internal static class \u0007
	{
		// Token: 0x06002EC3 RID: 11971 RVA: 0x000AFB08 File Offset: 0x000ADD08
		internal static bool \u0001(_ICompileContext \u0002, List<ILanguageModel> \u0003, _ICompileContext \u0004, bool \u0005, bool \u0006)
		{
			bool flag = true;
			bool flag2 = true;
			foreach (ILanguageModel languageModel in \u0003)
			{
				foreach (ILMPOU u in languageModel.Pous)
				{
					\u0083.\u0007.\u0001(\u0002, u, \u0004, ref flag2, \u0005, \u0006);
					flag = (flag && flag2);
				}
				foreach (ILMGlobVarlist u2 in languageModel.GlobalVariableLists)
				{
					\u0083.\u0007.\u0001(\u0002, u2, \u0004, ref flag2, \u0005, \u0006);
					flag = (flag && flag2);
				}
				foreach (ILMDataType u3 in languageModel.DataTypes)
				{
					\u0083.\u0007.\u0001(\u0002, u3, \u0004, ref flag2, \u0005, \u0006);
					flag = (flag && flag2);
				}
			}
			return flag;
		}

		// Token: 0x06002EC4 RID: 11972 RVA: 0x000AFBF8 File Offset: 0x000ADDF8
		internal static void \u0001(_ICompileContext \u0002, ILMPOU \u0003, _ICompileContext \u0004, ref bool \u0005, bool \u0006, bool \u0007)
		{
			global::\u0011.\u0006 u;
			if (\u0003.Action && \u0003.Interface == null)
			{
				u = new global::\u0011.\u0006("PROGRAM ppp");
				\u0003.Interface = u.\u0002();
			}
			else
			{
				u = new global::\u0011.\u0006("");
			}
			_ISignature u2 = null;
			IScope5 u000E = null;
			_ISignature u3 = null;
			if (!\u0083.\u0007.\u0001(\u0002, \u0003, \u0004, ref \u0005, \u0006, \u0007, u, ref u3, ref u2, ref u000E))
			{
				return;
			}
			\u0083.\u0007.\u0001(\u0002, \u0003, \u0004, ref \u0005, \u0006, u2, u3, u000E);
		}

		// Token: 0x06002EC5 RID: 11973 RVA: 0x000AFC64 File Offset: 0x000ADE64
		private static bool \u0001(_ICompileContext \u0002, ILMPOU \u0003, _ICompileContext \u0004, ref bool \u0005, bool \u0006, bool \u0007, global::\u0011.\u0006 \u0008, ref _ISignature \u000E, ref _ISignature \u000F, ref IScope5 \u0010)
		{
			if (\u0003.Interface == null)
			{
				return true;
			}
			\u000E = \u0008.\u0001(null, "", \u0003.Interface as _IStatement);
			\u000E.ObjectGuid = \u0003.POUGuid;
			\u000E.ParentObjectGuid = \u0003.ParentObjectGuid;
			\u000E.SetFlag(SignatureFlag.External, \u0003.External);
			if (\u0004 != null && \u000E.POUType != Operator.Method && \u000E.POUType != Operator.Action)
			{
				\u000F = \u0004[\u000E.Name];
			}
			if (\u0006)
			{
				\u0005 = (\u000F != null && \u000F.Checksum == \u000E.Checksum);
				if (!\u0005)
				{
					return false;
				}
			}
			else
			{
				\u000E = \u000E.CreateCompiledSignature(\u000F, \u0002, \u0004, \u0002.HasByteSupport());
				if (\u0007 && \u000F != null)
				{
					\u000E.Checksum = \u000F.Checksum;
					\u000E.ChecksumNoInit = \u000F.ChecksumNoInit;
				}
				\u0002.AddSignature(\u000E, \u000F, \u0004, true);
				\u000E.SetFlag(SignatureFlag.Generated, true);
				\u0010 = global::\u0007.\u0005.\u0001(\u0002, \u000E.Id);
				\u0010.LocalSignature = \u000E;
				global::\u0014.\u0013.\u0002(\u000E, \u0010, \u0002);
				global::\u0014.\u0013.\u0001(\u000E, \u0010, \u0002);
				global::\u0017.\u0018.\u0001(\u000E, \u0010);
				if (\u0004 != null)
				{
					\u000F = \u0004[\u000E.Name];
				}
				if (\u0005)
				{
					foreach (IMessage message in \u000E.Messages)
					{
						\u0005 = (message.Severity != Severity.Error && message.Severity != Severity.FatalError);
						if (!\u0005)
						{
							break;
						}
					}
				}
			}
			return true;
		}

		// Token: 0x06002EC6 RID: 11974 RVA: 0x000AFE08 File Offset: 0x000AE008
		private static void \u0001(_ICompileContext \u0002, ILMPOU \u0003, _ICompileContext \u0004, ref bool \u0005, bool \u0006, _ISignature \u0007, _ISignature \u0008, IScope5 \u000E)
		{
			if (\u0003.Body == null)
			{
				return;
			}
			_ICompiledPOU icompiledPOU = global::\u0019.\u0003.\u0001(\u0003.Name);
			icompiledPOU.MessageGuid = \u0003.MessageGuid;
			icompiledPOU.ObjectGuid = \u0003.POUGuid;
			_IStatement istatement = \u0003.Body as _IStatement;
			icompiledPOU.SetParseTree(istatement);
			icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.NotForUpToDate, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.TopLevel, \u0003.EnableSystemCall);
			icompiledPOU.SetFlagInternal(InternalCompiledPOUFlags.ImplicitInitialisationCodeAdded, true);
			if (!\u0006)
			{
				\u0002.AddCompiledPOU(icompiledPOU, \u0008, \u0004);
				if (\u0008.POUType == Operator.FunctionBlock)
				{
					ISignature subSignature = \u0008.GetSubSignature("__MAIN");
					icompiledPOU.SignatureId = subSignature.Id;
				}
				else
				{
					icompiledPOU.SignatureId = \u0008.Id;
				}
				ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(\u000E, \u0002, true, icompiledPOU);
				istatement.Accept(ivisit);
				global::\u0018.\u000E.\u0001(istatement, \u0002);
				TypeCheckerVisitor ivisit2 = new TypeCheckerVisitor(\u000E, \u0002, true);
				istatement.Accept(ivisit2);
				ErrorVisitor errorVisitor = new ErrorVisitor();
				istatement.Accept(errorVisitor);
				icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
				icompiledPOU.SetFlagInternal(InternalCompiledPOUFlags.TypeCheckDone, true);
				if (\u0005)
				{
					_ICompilerMessage[] array = errorVisitor._Messages;
					foreach (_ICompilerMessage icompilerMessage in array)
					{
						\u0005 = (icompilerMessage.Severity != Severity.Error && icompilerMessage.Severity != Severity.FatalError);
						if (!\u0005)
						{
							icompiledPOU.SetMessages(array);
							break;
						}
					}
				}
				if (\u0003.Slot != -1 && \u0003.TaskReference != Guid.Empty)
				{
					\u0002.SlotPOUs.Add(\u0003.TaskReference, \u0003.Slot, icompiledPOU.ObjectGuid);
					byte taskIndexByGuid = \u0002.TaskList.GetTaskIndexByGuid(\u0003.TaskReference);
					\u0008.AddTaskReference(taskIndexByGuid);
				}
				if (\u0003.DownloadSlot != -1)
				{
					\u0002.SlotPOUs.AddDownloadSlot(\u0003.DownloadSlot, icompiledPOU.ObjectGuid);
				}
				if (\u0003.OnlineChangeSlot != -1)
				{
					\u0002.SlotPOUs.AddOnlineChangeSlot(\u0003.OnlineChangeSlot, icompiledPOU.ObjectGuid);
				}
				return;
			}
			_ICompiledPOU icompiledPOU2 = null;
			if (\u0007 != null)
			{
				icompiledPOU2 = \u0004._GetCompiledPOUById(\u0007.Id);
			}
			if (icompiledPOU2 == null)
			{
				\u0005 = false;
				return;
			}
			icompiledPOU.UpdateChecksum();
			\u0005 = (icompiledPOU2.Checksum == icompiledPOU.Checksum);
		}

		// Token: 0x06002EC7 RID: 11975 RVA: 0x000B0030 File Offset: 0x000AE230
		internal static void \u0001(_ICompileContext \u0002, ILMGlobVarlist \u0003, _ICompileContext \u0004, ref bool \u0005, bool \u0006, bool \u0007)
		{
			global::\u0011.\u0006 u = new global::\u0011.\u0006("");
			_ISignature isignature = u.\u0001(null, "", \u0003.Interface as _IStatement, \u0003.Name, true);
			isignature.MessageGuid = u.MessageGuid;
			isignature.ObjectGuid = \u0003.GVLGuid;
			_ISignature isignature2 = null;
			if (\u0004 != null)
			{
				isignature2 = \u0004[isignature.Name];
			}
			if (\u0006)
			{
				\u0005 = (isignature2 != null && isignature2.Checksum == isignature.Checksum);
				return;
			}
			isignature = isignature.CreateCompiledSignature(isignature2, \u0002, \u0004, \u0002.HasByteSupport());
			if (\u0007 && isignature2 != null)
			{
				isignature.Checksum = isignature2.Checksum;
				isignature.ChecksumNoInit = isignature2.ChecksumNoInit;
			}
			\u0002.AddSignature(isignature, isignature2, \u0004, true);
			isignature.SetFlag(SignatureFlag.Generated, true);
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, isignature.Id);
			scope.LocalSignature = isignature;
			global::\u0014.\u0013.\u0002(isignature, scope, \u0002);
			global::\u0014.\u0013.\u0001(isignature, scope, \u0002);
			if (\u0005)
			{
				foreach (IMessage message in isignature.Messages)
				{
					\u0005 = (message.Severity != Severity.Error && message.Severity != Severity.FatalError);
					if (!\u0005)
					{
						break;
					}
				}
			}
		}

		// Token: 0x06002EC8 RID: 11976 RVA: 0x000B0160 File Offset: 0x000AE360
		internal static void \u0001(_ICompileContext \u0002, ILMDataType \u0003, _ICompileContext \u0004, ref bool \u0005, bool \u0006, bool \u0007)
		{
			_ISignature isignature = new global::\u0011.\u0006("").\u0001(null, "", \u0003.Interface as _IStatement);
			isignature.ObjectGuid = \u0003.DUTGuid;
			_ISignature isignature2 = null;
			if (\u0004 != null)
			{
				isignature2 = \u0004[isignature.Name];
			}
			if (\u0006)
			{
				\u0005 = (isignature2 != null && isignature2.Checksum == isignature.Checksum);
				return;
			}
			isignature = isignature.CreateCompiledSignature(isignature2, \u0002, \u0004, \u0002.HasByteSupport());
			if (\u0007 && isignature2 != null)
			{
				isignature.Checksum = isignature2.Checksum;
				isignature.ChecksumNoInit = isignature2.ChecksumNoInit;
			}
			\u0002.AddSignature(isignature, isignature2, \u0004, true);
			isignature.SetFlag(SignatureFlag.Generated, true);
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, isignature.Id);
			scope.LocalSignature = isignature;
			global::\u0014.\u0013.\u0002(isignature, scope, \u0002);
			global::\u0014.\u0013.\u0001(isignature, scope, \u0002);
			global::\u0004.\u0018.\u0001(isignature, \u0002, \u0004);
			if (\u0005)
			{
				foreach (IMessage message in isignature.Messages)
				{
					\u0005 = (message.Severity != Severity.Error && message.Severity != Severity.FatalError);
					if (!\u0005)
					{
						break;
					}
				}
			}
		}
	}
}
