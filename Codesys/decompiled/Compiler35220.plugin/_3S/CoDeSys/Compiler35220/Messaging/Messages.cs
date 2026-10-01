using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u0005;
using \u0007;
using \u0016;
using \u0018;
using \u0019;
using \u001F;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LanguageModelUtilities;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Messaging
{
	// Token: 0x02000394 RID: 916
	internal static class Messages
	{
		// Token: 0x0600352D RID: 13613 RVA: 0x000D1EF0 File Offset: 0x000D00F0
		internal static bool \u0001(_ICompileContext \u0002, IMessageStorage \u0003, IMessageCategory \u0004)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.OnBeforeMessageOutput(\u0002, new MessageOutputEventArgs2(\u0002.ApplicationGuid));
			\u0018.\u0011 u = new \u0018.\u0011(\u0002, \u0003, \u0004);
			if (Messages.\u0001(\u0002.ApplicationGuid, \u0003, \u0004, u))
			{
				return u.\u0001;
			}
			Messages.\u0001(u);
			Messages.\u0004(u);
			Messages.\u0003(u);
			Messages.\u0002(u);
			Messages.\u0001(\u0002, \u0003, \u0004, u);
			APEnvironmentFacade.Instance.LanguageModelMgr.OnAfterMessageOutput(\u0002, new MessageOutputEventArgs2(\u0002.ApplicationGuid));
			return u.\u0001;
		}

		// Token: 0x0600352E RID: 13614 RVA: 0x000D1F7C File Offset: 0x000D017C
		private static void \u0001(\u0018.\u0011 \u0002)
		{
			foreach (_ICompilerMessage icompilerMessage in Messages.\u0001(\u0002.ComCon))
			{
				\u0002.\u0001 = (\u0002.\u0001 && icompilerMessage.Severity != Severity.Error && icompilerMessage.Severity != Severity.FatalError);
				IMessageStorage messageStorage = \u0002.Storage;
				if (messageStorage != null)
				{
					messageStorage.AddMessage(\u0002.Category, icompilerMessage);
				}
			}
		}

		// Token: 0x0600352F RID: 13615 RVA: 0x000D2008 File Offset: 0x000D0208
		private static void \u0002(\u0018.\u0011 \u0002)
		{
			IMessageStorage messageStorage = \u0002.Storage;
			IMessageCategory category = \u0002.Category;
			IDictionary<string, int> dictionary = \u0002.SummarizedLibErrors;
			foreach (string text in dictionary.Keys)
			{
				_ICompilerMessage icompilerMessage = \u0019.\u0003.\u0001();
				if (icompilerMessage.ProjectHandle == -1)
				{
					icompilerMessage.ProjectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(text);
				}
				icompilerMessage.Text = global::\u0003.\u0006.\u0001(MessageId.Err_SummarizedLibraryErrors, new object[]
				{
					dictionary[text],
					text
				});
				icompilerMessage.Severity = Severity.Error;
				messageStorage.AddMessage(category, icompilerMessage);
			}
		}

		// Token: 0x06003530 RID: 13616 RVA: 0x000D20D4 File Offset: 0x000D02D4
		private static void \u0003(\u0018.\u0011 \u0002)
		{
			_ICompileContext icompileContext = \u0002.ComCon;
			foreach (_ICompiledPOU icompiledPOU in icompileContext.CompiledPOUList)
			{
				_ISignature isignature = icompileContext[icompiledPOU.SignatureId];
				if (!\u001F.\u0008.\u0001(\u0002, icompiledPOU, isignature))
				{
					Messages.\u0001(\u0002, isignature);
					Messages.\u0001(\u0002, icompiledPOU, isignature);
				}
			}
		}

		// Token: 0x06003531 RID: 13617 RVA: 0x000D2148 File Offset: 0x000D0348
		private static void \u0001(\u0018.\u0011 \u0002, _ICompiledPOU \u0003, _ISignature \u0004)
		{
			IMessage[] array;
			if (\u0003.GetFlagInternal(InternalCompiledPOUFlags.TypeCheckDone))
			{
				IMessage[] messages = \u0003.GetMessages(true);
				array = messages;
			}
			else
			{
				IMessage[] messages = \u0003.Messages;
				array = messages;
			}
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					_ICompilerMessage u = array[i] as _ICompilerMessage;
					\u0002.\u0001(u, \u0003);
					Messages.\u0001(u, \u0002, \u0004, \u0003.LibraryPath);
				}
			}
		}

		// Token: 0x06003532 RID: 13618 RVA: 0x000D21A8 File Offset: 0x000D03A8
		private static void \u0001(\u0018.\u0011 \u0002, _ISignature \u0003)
		{
			if (\u0003 != null)
			{
				foreach (_ICompilerMessage u in \u0003.Messages.OfType<_ICompilerMessage>())
				{
					\u0002.\u0001(u, \u0003);
					Messages.\u0001(u, \u0002, \u0003, \u0003.LibraryPath);
				}
			}
		}

		// Token: 0x06003533 RID: 13619 RVA: 0x000D220C File Offset: 0x000D040C
		private static void \u0004(\u0018.\u0011 \u0002)
		{
			_ICompileContext icompileContext = \u0002.ComCon;
			foreach (_ISignature isignature in icompileContext.AllSignatureList)
			{
				if (!isignature.HasAttribute(CompileAttributes.ATTRIBUTE_SUPPRESS_MESSAGES) && (isignature.POUType == Operator.VarGlobal || isignature.POUType == Operator.VarConfig || isignature.POUType == Operator.VarAccess || isignature.POUType == Operator.Type || isignature.POUType == Operator.FunctionBlock || isignature.POUType == Operator.Interface || isignature.POUType == Operator.Function || isignature.POUType == Operator.Method || isignature.POUType == Operator.Program || isignature.POUType == Operator.None) && ((isignature.POUType != Operator.Function && isignature.POUType != Operator.Method && isignature.POUType != Operator.Program) || icompileContext.GetCompiledPOUById(isignature.Id) == null) && !\u001F.\u0008.\u0001(\u0002, null, isignature))
				{
					IMessage[] messages = isignature.Messages;
					for (int i = 0; i < messages.Length; i++)
					{
						_ICompilerMessage u = messages[i] as _ICompilerMessage;
						\u0002.\u0001(u, isignature);
						Messages.\u0001(u, \u0002, isignature, isignature.LibraryPath);
					}
					if (isignature.POUType == Operator.Interface || isignature.POUType == Operator.Type || isignature.POUType == Operator.FunctionBlock)
					{
						foreach (_ISignature isignature2 in isignature.SubSignatures)
						{
							if (icompileContext.GetCompiledPOUById(isignature2.Id) == null)
							{
								messages = isignature2.Messages;
								for (int k = 0; k < messages.Length; k++)
								{
									_ICompilerMessage u2 = messages[k] as _ICompilerMessage;
									\u0002.\u0001(u2, isignature2);
									Messages.\u0001(u2, \u0002, isignature, isignature.LibraryPath);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06003534 RID: 13620 RVA: 0x000D23EC File Offset: 0x000D05EC
		private static void \u0001(_ICompileContext \u0002, IMessageStorage \u0003, IMessageCategory \u0004, \u0018.\u0011 \u0005)
		{
			if (\u0005.\u0001)
			{
				global::\u0005.\u0007.Singleton.OutputMemoryStatistics((ILMCompiledApplicationSet)\u0002);
				\u0005.\u0001 &= global::\u0005.\u0007.\u0001(\u0002, \u0003, \u0004);
			}
		}

		// Token: 0x06003535 RID: 13621 RVA: 0x000D241C File Offset: 0x000D061C
		private static bool \u0001(Guid \u0002, IMessageStorage \u0003, IMessageCategory \u0004, \u0018.\u0011 \u0005)
		{
			_ICompilerMessage icompilerMessage = Messages.\u0001(\u0002);
			if (icompilerMessage != null)
			{
				\u0005.\u0001 = false;
				\u0005.\u0002 = true;
				\u0003.ClearMessages(\u0004);
				\u0003.AddMessage(\u0004, icompilerMessage);
				return true;
			}
			return false;
		}

		// Token: 0x06003536 RID: 13622 RVA: 0x000D2454 File Offset: 0x000D0654
		private static void \u0001(_ICompileContext \u0002, LList<_ICompilerMessage> \u0003)
		{
			LDictionary<string, _IPreCompileContext> ldictionary = new LDictionary<string, _IPreCompileContext>();
			foreach (IPreCompileContext preCompileContext in \u0002._LibraryTable.GetLocalVisibleILibraries())
			{
				if (preCompileContext != null)
				{
					string text = \u0002.GetLocalLibraryNamespace((_IPreCompileContext)preCompileContext);
					if (!string.IsNullOrEmpty(text))
					{
						text = text.ToUpperInvariant();
						if (ldictionary.ContainsKey(text))
						{
							if (ldictionary[text].LibraryPath != preCompileContext.LibraryPath)
							{
								INamespaceConflictChecker namespaceConflictCheckerOrNull = APEnvironmentFacade.Instance.NamespaceConflictCheckerOrNull;
								IEnumerable<INamespaceConflictIssue> u = (namespaceConflictCheckerOrNull != null) ? namespaceConflictCheckerOrNull.CheckConflictingNamespace(text) : null;
								\u0019.\u0014.Instance.\u0001(\u0003, u);
							}
						}
						else
						{
							ldictionary[text] = (_IPreCompileContext)preCompileContext;
						}
					}
				}
			}
		}

		// Token: 0x06003537 RID: 13623 RVA: 0x000D2520 File Offset: 0x000D0720
		private static void \u0001(_ICompileContext \u0002, ICollection<_ICompilerMessage> \u0003)
		{
			foreach (IPreCompileContext preCompileContext in \u0002._LibraryTable.GetLocalVisibleILibraries())
			{
				string localLibraryNamespace = \u0002.GetLocalLibraryNamespace((_IPreCompileContext)preCompileContext);
				if (!string.IsNullOrEmpty(localLibraryNamespace))
				{
					_IScanner5 iscanner = Scanner.\u0001();
					iscanner.Initialize(localLibraryNamespace);
					IToken token;
					if (iscanner.GetNext(out token) != TokenType.Identifier || iscanner.GetNext(out token) != TokenType.End)
					{
						string u = global::\u0003.\u0006.\u0001(MessageId.Err_LibraryNamespaceNotValid, new object[]
						{
							localLibraryNamespace,
							preCompileContext.LibraryPath
						});
						_ICompilerMessage item = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(), u, Severity.Error, MessageId.Err_LibraryNamespaceNotValid);
						\u0003.Add(item);
					}
				}
			}
		}

		// Token: 0x06003538 RID: 13624 RVA: 0x000D25E4 File Offset: 0x000D07E4
		private static void \u0002(_ICompileContext \u0002, LList<_ICompilerMessage> \u0003)
		{
			if (APEnvironmentFacade.Instance.IsWarningMessageDisabled(MessageId.Wrn_LibWithStringInVarInOut))
			{
				return;
			}
			foreach (_ISignature isignature in \u0002.AllFlat)
			{
				if (string.IsNullOrEmpty(isignature.LibraryPath))
				{
					foreach (_IVariable ivariable in isignature.AllInputs)
					{
						if (((ivariable != null) ? ivariable.Type : null) != null && ivariable.HasFlag(VarFlag.Inout) && !ivariable.HasFlag(VarFlag.Constant) && TypeTable.IsString(((_IType)ivariable.Type).DeRefType.Class))
						{
							if (!APEnvironmentFacade.Instance.CrossReferenceService.GetCrossReferenceForVariable(ivariable.Name, ivariable.Name, isignature.ObjectGuid, CrossRefSearchType.Variables, CrossRefOccurence.LanguageModel, CrossReferenceMatchType.Existing, isignature.ObjectGuid).Any(new Func<ICrossReferenceNode, bool>(Messages.<>c.<>9.\u0001)))
							{
								Severity u = Messages.\u0001(ivariable, MessageId.Wrn_LibWithStringInVarInOut);
								_ISourcePosition u2 = \u0019.\u0003.\u0001(ivariable.SourcePosition.ProjectHandle, isignature.ObjectGuid, ivariable.SourcePosition.Position, ivariable.SourcePosition.PositionOffset, ivariable.SourcePosition.Length);
								string u3 = global::\u0003.\u0006.\u0001(MessageId.Wrn_LibWithStringInVarInOut, new object[]
								{
									ivariable.OrgName,
									isignature.OrgName
								});
								_ICompilerMessage icompilerMessage = \u0019.\u0003.\u0001(u2, u3, u, MessageId.Wrn_LibWithStringInVarInOut);
								\u0003.Add(icompilerMessage);
							}
						}
					}
				}
			}
		}

		// Token: 0x06003539 RID: 13625 RVA: 0x000D27B4 File Offset: 0x000D09B4
		public static Severity \u0001(IVariable \u0002, MessageId \u0003)
		{
			if (\u0002.HasAttribute("suppress_warning_0"))
			{
				string b = string.Format("C{0:D4}", (int)\u0003);
				int num = 0;
				string stAttribute;
				do
				{
					stAttribute = string.Format("suppress_warning_{0}", num++);
					if (!\u0002.HasAttribute(stAttribute))
					{
						return Severity.Warning;
					}
				}
				while (!(\u0002.GetAttributeValue(stAttribute).Trim() == b));
				return Severity.SuppressedWarning;
			}
			return Severity.Warning;
		}

		// Token: 0x0600353A RID: 13626 RVA: 0x000D281C File Offset: 0x000D0A1C
		internal static LList<_ICompilerMessage> \u0001(_ICompileContext \u0002)
		{
			LList<_ICompilerMessage> llist = new LList<_ICompilerMessage>();
			if (\u0002.ApplicationGuid != Guid.Empty)
			{
				string applicationNameByGuid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetApplicationNameByGuid(\u0002.ApplicationGuid, false);
				IToken token;
				if (applicationNameByGuid != null && APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(applicationNameByGuid, true, true, true, true).Match(TokenType.Identifier, false, out token) <= 0)
				{
					_ISourcePosition u = \u0019.\u0003.\u0001(APEnvironmentFacade.Instance.PrimaryProjectHandle, \u0002.ApplicationGuid, 0L, 0, 0);
					string u2 = global::\u0003.\u0006.\u0001(MessageId.Err_DeviceNameNoIdent, new object[]
					{
						applicationNameByGuid
					});
					_ICompilerMessage icompilerMessage = \u0019.\u0003.\u0001(u, u2, Severity.Error, MessageId.Err_DeviceNameNoIdent);
					llist.Add(icompilerMessage);
				}
			}
			bool flag = \u0002.IsDefined("CheckAllPoolObjects");
			bool flag2 = \u0002.IsDefined("NoInterfaceLibraryChecks");
			if (flag && !flag2 && APEnvironmentFacade.Instance.IsPrimaryProjectAnInterfaceLibrary())
			{
				Messages.\u0003(\u0002, llist);
			}
			IContainerLibraryChecker containerLibraryCheckerOrNull = APEnvironmentFacade.Instance.ContainerLibraryCheckerOrNull;
			IEnumerable<IContainerLibIssue> u3 = (containerLibraryCheckerOrNull != null) ? containerLibraryCheckerOrNull.CheckContainerLibrary(flag, \u0002 as ILMCompiledApplicationSet) : null;
			\u0019.\u0014.Instance.\u0001(llist, u3);
			Messages.\u0001(\u0002, llist);
			Messages.\u0001(\u0002, llist);
			if (flag)
			{
				Messages.\u0002(\u0002, llist);
			}
			Guid deviceOfApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(\u0002.ApplicationGuid);
			Guid[] applicationsOfDevice = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetApplicationsOfDevice(deviceOfApplication);
			string deviceName = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceName(deviceOfApplication);
			for (int i = 0; i < applicationsOfDevice.Length; i++)
			{
				for (int j = i + 1; j < applicationsOfDevice.Length; j++)
				{
					string applicationNameByGuid2 = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetApplicationNameByGuid(applicationsOfDevice[i], \u0002.SimulationMode);
					string applicationNameByGuid3 = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetApplicationNameByGuid(applicationsOfDevice[j], \u0002.SimulationMode);
					if (applicationNameByGuid2 == applicationNameByGuid3)
					{
						_ISourcePosition u4 = \u0019.\u0003.\u0001(APEnvironmentFacade.Instance.PrimaryProjectHandle, applicationsOfDevice[i], 0L, 0, 0);
						string u5 = global::\u0003.\u0006.\u0001(MessageId.Err_ApplicationConflict, new object[]
						{
							applicationNameByGuid2,
							deviceName
						});
						_ICompilerMessage icompilerMessage2 = \u0019.\u0003.\u0001(u4, u5, Severity.Error, MessageId.Err_ApplicationConflict);
						llist.Add(icompilerMessage2);
					}
				}
			}
			ITargetSettings targetSettings = \u0002.GetTargetSettings();
			if (targetSettings != null)
			{
				int intValue = global::\u0016.\u0004.MaximumNumApplications.GetIntValue(targetSettings);
				if (intValue < applicationsOfDevice.Length)
				{
					_ISourcePosition u6 = \u0019.\u0003.\u0001(APEnvironmentFacade.Instance.PrimaryProjectHandle, deviceOfApplication, 0L, 0, 0);
					string u7 = global::\u0003.\u0006.\u0001(MessageId.Wrn_TooManyApplications, new object[]
					{
						applicationsOfDevice.Length,
						deviceName,
						intValue
					});
					_ICompilerMessage icompilerMessage3 = \u0019.\u0003.\u0001(u6, u7, Severity.Warning, MessageId.Wrn_TooManyApplications);
					llist.Add(icompilerMessage3);
				}
			}
			return llist;
		}

		// Token: 0x0600353B RID: 13627 RVA: 0x000D2AF0 File Offset: 0x000D0CF0
		private static _ICompilerMessage \u0001(Guid \u0002)
		{
			IEnumerable<ILMLibraryList2> libListForApp = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibListForApp(\u0002);
			int primaryProjectHandle = APEnvironmentFacade.Instance.PrimaryProjectHandle;
			foreach (ILMLibraryList2 ilmlibraryList in libListForApp)
			{
				ILMLibraryInfo5 ilmlibraryInfo = ilmlibraryList.Libraries.OfType<ILMLibraryInfo5>().Where(new Func<ILMLibraryInfo5, bool>(Messages.<>c.<>9.\u0001)).FirstOrDefault(new Func<ILMLibraryInfo5, bool>(Messages.<>c.<>9.\u0002));
				if (ilmlibraryInfo != null)
				{
					_ISourcePosition u = \u0019.\u0003.\u0001(primaryProjectHandle, ilmlibraryList.LibManGuid, 0L, 0, 0);
					string u2 = global::\u0003.\u0006.\u0001(MessageId.Wrn_LibraryNotInstalled, new object[]
					{
						ilmlibraryInfo.Identification
					});
					return \u0019.\u0003.\u0001(u, u2, Severity.FatalError, MessageId.Wrn_LibraryNotInstalled);
				}
			}
			return null;
		}

		// Token: 0x0600353C RID: 13628 RVA: 0x000D2BE8 File Offset: 0x000D0DE8
		private static void \u0001(_ICompilerMessage \u0002, \u0018.\u0011 \u0003, _ISignature \u0004, string \u0005)
		{
			if (\u0004.IsCompiledLibraryObject && ((_IWarningHelper)APEnvironmentFacade.Instance.LMServiceProvider.ConfigurationService.WarningConfiguration).IsWarning(\u0002.MessageId))
			{
				return;
			}
			\u0003.\u0001(\u0002, \u0004, \u0005);
		}

		// Token: 0x0600353D RID: 13629 RVA: 0x000D2C24 File Offset: 0x000D0E24
		private static void \u0003(_ICompileContext \u0002, LList<_ICompilerMessage> \u0003)
		{
			foreach (_ISignature isignature in \u0002.AllSignatureList)
			{
				if (!isignature.GetFlag(SignatureFlag.SuperGlobal) && string.IsNullOrEmpty(isignature.LibraryPath))
				{
					bool flag = false;
					Operator poutype = isignature.POUType;
					if (poutype != Operator.Type)
					{
						if (poutype != Operator.VarGlobal)
						{
							if (poutype != Operator.Interface)
							{
								goto IL_1A1;
							}
						}
						else
						{
							flag = true;
							if (isignature.HasAttribute(CompileAttributes.ATTRIBUTE_PARAMETERLIST))
							{
								flag = false;
								goto IL_1A1;
							}
							using (IEnumerator<IVariable> enumerator2 = isignature.AllVariables.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									if (!enumerator2.Current.GetFlag(VarFlag.Constant))
									{
										flag = false;
										break;
									}
								}
								goto IL_1A1;
							}
						}
						if (!isignature.HasAttribute(CompileAttributes.ATTRIBUTE_NO_QUERY_INTERFACE_CHECK))
						{
							IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, isignature.Id);
							LDictionary<int, int> ldictionary = new LDictionary<int, int>();
							Helper.\u0001(scope, isignature, ldictionary, true);
							ISignature[] array = scope.SystemScope.FindSignature("IQueryInterface");
							if (array == null || (array != null && array.Length == 1 && isignature.Id != array[0].Id && !ldictionary.ContainsKey(array[0].Id)))
							{
								_ISourcePosition u = null;
								if (APEnvironmentFacade.Instance.ExistsObject(APEnvironmentFacade.Instance.PrimaryProjectHandle, isignature.ObjectGuid))
								{
									u = \u0019.\u0003.\u0001(APEnvironmentFacade.Instance.PrimaryProjectHandle, isignature.ObjectGuid, 0L, 0, 0);
								}
								string u2 = global::\u0003.\u0006.\u0001(MessageId.Err_QueryInterfaceP1NoIQuery, new object[]
								{
									isignature.OrgName,
									"__System.IQueryInterface"
								});
								\u0003.Add(\u0019.\u0003.\u0001(u, u2, Severity.Warning, MessageId.Wrn_NotAllowedInInterfaceLib));
							}
						}
						flag = true;
					}
					else
					{
						flag = true;
					}
					IL_1A1:
					if (!flag && APEnvironmentFacade.Instance.ExistsObject(APEnvironmentFacade.Instance.PrimaryProjectHandle, isignature.ObjectGuid))
					{
						_ISourcePosition u3 = \u0019.\u0003.\u0001(APEnvironmentFacade.Instance.PrimaryProjectHandle, isignature.ObjectGuid, 0L, 0, 0);
						string u4 = global::\u0003.\u0006.\u0001(MessageId.Err_NotAllowedInInterfaceLib, new object[]
						{
							Scanner.GetTextOfOperator(isignature.POUType, false)
						});
						\u0003.Add(\u0019.\u0003.\u0001(u3, u4, Severity.Error, MessageId.Err_NotAllowedInInterfaceLib));
					}
				}
			}
			foreach (IPreCompileContext preCompileContext in \u0002._LibraryTable.GetVisibleILibraries(""))
			{
				_IPreCompileContext ipreCompileContext = (_IPreCompileContext)preCompileContext;
				if (!ipreCompileContext.IsInterfaceLibrary)
				{
					Guid poolLibMan = APEnvironmentFacade.Instance.GetPoolLibMan(APEnvironmentFacade.Instance.PrimaryProjectHandle);
					_ISourcePosition u5 = \u0019.\u0003.\u0001(APEnvironmentFacade.Instance.PrimaryProjectHandle, poolLibMan, 0L, 0, 0);
					string u6 = global::\u0003.\u0006.\u0001(MessageId.Err_NotAllowedInInterfaceLib, new object[]
					{
						ipreCompileContext.LibraryId
					});
					\u0003.Add(\u0019.\u0003.\u0001(u5, u6, Severity.Error, MessageId.Err_NotAllowedInInterfaceLib));
				}
			}
		}

		// Token: 0x0600353E RID: 13630 RVA: 0x000D2F50 File Offset: 0x000D1150
		internal static Severity \u0001(_ISignature \u0002, MessageId \u0003)
		{
			_3S.CoDeSys.Core.LanguageModel.IHasAttributes hasAttributes = \u0002 as _3S.CoDeSys.Core.LanguageModel.IHasAttributes;
			if (hasAttributes != null)
			{
				return Messages.\u0001(\u0003, hasAttributes);
			}
			return Severity.Warning;
		}

		// Token: 0x0600353F RID: 13631 RVA: 0x000D2F70 File Offset: 0x000D1170
		internal static Severity \u0001(_IVariable \u0002, MessageId \u0003)
		{
			_3S.CoDeSys.Core.LanguageModel.IHasAttributes hasAttributes = \u0002 as _3S.CoDeSys.Core.LanguageModel.IHasAttributes;
			if (hasAttributes != null)
			{
				return Messages.\u0001(\u0003, hasAttributes);
			}
			return Severity.Warning;
		}

		// Token: 0x06003540 RID: 13632 RVA: 0x000D2F90 File Offset: 0x000D1190
		private static Severity \u0001(MessageId \u0002, _3S.CoDeSys.Core.LanguageModel.IHasAttributes \u0003)
		{
			Messages.\u0001 u = new Messages.\u0001();
			u.\u0001 = \u0003;
			u.\u0001 = string.Format("C{0:D4}", (int)\u0002);
			string text = Array.Find<string>(u.\u0001.Attributes, new Predicate<string>(u.\u0001));
			if (text != null)
			{
				return Severity.SuppressedWarning;
			}
			return Severity.Warning;
		}

		// Token: 0x02000396 RID: 918
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x06003547 RID: 13639 RVA: 0x000D3034 File Offset: 0x000D1234
			internal bool \u0001(string \u0002)
			{
				return \u0002.StartsWith("suppress_warning_") && this.\u0001 == this.\u0001.GetAttributeValue(\u0002);
			}

			// Token: 0x04000A59 RID: 2649
			public string \u0001;

			// Token: 0x04000A5A RID: 2650
			public _3S.CoDeSys.Core.LanguageModel.IHasAttributes \u0001;
		}
	}
}
