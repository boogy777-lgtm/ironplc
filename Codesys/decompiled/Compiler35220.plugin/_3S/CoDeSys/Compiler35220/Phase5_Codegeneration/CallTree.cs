using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u0006;
using \u0007;
using \u0011;
using \u0012;
using \u0016;
using \u0019;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration
{
	// Token: 0x0200021A RID: 538
	public class CallTree
	{
		// Token: 0x060023EB RID: 9195 RVA: 0x0007AC04 File Offset: 0x00078E04
		public CallTree(_ICompileContext comcon, bool bCheckForStackOverflow, int nMaxStackSize, int nMaxStackInExternalFuctions)
		{
			this._ICompileContext = comcon;
			this._Scope = global::\u0007.\u0005.\u0001(comcon);
			this.\u0001 = bCheckForStackOverflow;
			this.\u0001 = nMaxStackSize;
			this.\u0002 = nMaxStackInExternalFuctions;
			this.\u0001 = new InheritanceInformation(this._Scope);
		}

		// Token: 0x17000694 RID: 1684
		// (get) Token: 0x060023EC RID: 9196 RVA: 0x0007AC68 File Offset: 0x00078E68
		private int MaxStackSizeToCheckAgainst
		{
			get
			{
				if (this.\u0003 != 0)
				{
					return this.\u0003;
				}
				return this.\u0001;
			}
		}

		// Token: 0x060023ED RID: 9197 RVA: 0x0007AC80 File Offset: 0x00078E80
		public static bool TryGetMaxStackSize(_ICompileContext comcon, out int nMaxStackSize, out int nMaxStackForExternalCalls)
		{
			ITargetSettings targetSettings;
			if (comcon.SimulationMode)
			{
				Guid deviceOfApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(comcon.ApplicationGuid);
				targetSettings = APEnvironmentFacade.Instance.GetSimulationTargetSettings(comcon.GetDeviceIdentification(), deviceOfApplication);
			}
			else
			{
				targetSettings = comcon.GetTargetSettings();
			}
			nMaxStackSize = 0;
			nMaxStackForExternalCalls = 0;
			if (targetSettings == null)
			{
				return false;
			}
			nMaxStackSize = global::\u0016.\u0004.MaxStackSize.GetIntValue(targetSettings);
			nMaxStackForExternalCalls = global::\u0016.\u0004.MaxStackSizeExternalCall.GetIntValue(targetSettings);
			if (nMaxStackForExternalCalls < 0)
			{
				nMaxStackForExternalCalls = 0;
			}
			return nMaxStackSize > 0;
		}

		// Token: 0x060023EE RID: 9198 RVA: 0x0007AD00 File Offset: 0x00078F00
		public CallStack GetMaxStackUsage(bool bCalculatePositions, string stTaskName)
		{
			CallStack callStack = this.CheckStackUsage(stTaskName);
			if (bCalculatePositions)
			{
				try
				{
					this.\u0002(callStack);
				}
				catch
				{
				}
			}
			return callStack;
		}

		// Token: 0x060023EF RID: 9199 RVA: 0x0007AD38 File Offset: 0x00078F38
		public CallStack CheckStackUsage(string stTaskName = null)
		{
			CallStack result = null;
			int num = 0;
			uint overriddenStackSize = APEnvironmentFacade.Instance.TaskStackSizeProvider.GetOverriddenStackSize(APEnvironmentFacade.Instance.PrimaryProjectHandle, this._ICompileContext.ApplicationGuid);
			foreach (_ICompiledPOU icompiledPOU in this.\u0001(this._ICompileContext, stTaskName))
			{
				_ISignature isignature = this._ICompileContext[icompiledPOU.SignatureId];
				if (isignature != null && !isignature.Name.StartsWith("CALLTASK__"))
				{
					if (isignature.Name.StartsWith("__CYCLE__CODE__") && this.\u0001)
					{
						this.\u0003 = (int)overriddenStackSize;
					}
					if (icompiledPOU.GetFlag(CompiledPOUFlags.TopLevel) || isignature.GetFlag(SignatureFlag.TopLevel))
					{
						int num2;
						CallStack callStack = this.\u0001(icompiledPOU, out num2);
						if (num2 > num)
						{
							num = num2;
							result = callStack;
						}
					}
					this.\u0003 = 0;
				}
			}
			return result;
		}

		// Token: 0x060023F0 RID: 9200 RVA: 0x0007AE3C File Offset: 0x0007903C
		internal IList<_ICompiledPOU> \u0001(_ICompileContext \u0002, string \u0003 = null)
		{
			CallTree.\u0001 u = new CallTree.\u0001();
			if (\u0003 == null)
			{
				return \u0002.CompiledPOUList;
			}
			u.\u0001 = \u0002["__CYCLE__CODE__" + \u0003];
			if (u.\u0001 != null)
			{
				return \u0002.CompiledPOUList.Where(new Func<_ICompiledPOU, bool>(u.\u0001)).ToArray<_ICompiledPOU>();
			}
			return \u0002.CompiledPOUList;
		}

		// Token: 0x060023F1 RID: 9201 RVA: 0x0007AE9C File Offset: 0x0007909C
		private CallStack \u0001(_ICompiledPOU \u0002, out int \u0003)
		{
			\u0003 = 0;
			CallStack u = new CallStack();
			CallStack callStack = new CallStack();
			_ISignature isignature = CallTree.\u0001(this._ICompileContext, \u0002.SignatureId);
			this.\u0001();
			try
			{
				callStack.\u0001(isignature, isignature, this.\u0001(isignature));
				this.\u0001(isignature, isignature, 0, u, callStack, out \u0003);
				this.\u0001(callStack);
				return callStack;
			}
			catch (AppStackOverflowException ex)
			{
				try
				{
					this.\u0002(ex.CallStack);
				}
				finally
				{
					this.\u0001(ex.CalculatedSize, ex.CallStack);
				}
			}
			finally
			{
				this.\u0002();
			}
			return new CallStack();
		}

		// Token: 0x060023F2 RID: 9202 RVA: 0x0007AF58 File Offset: 0x00079158
		private bool \u0001(_ISignature \u0002, _ISignature \u0003, int \u0004, CallStack \u0005, CallStack \u0006, out int \u0007)
		{
			\u0007 = 0;
			if (this.\u0001(\u0003, \u0004, \u0005, ref \u0007))
			{
				return true;
			}
			_ISignature u;
			if (this.\u0001(\u0002, \u0005, out u))
			{
				this.\u0001(\u0002, u, \u0005);
				return false;
			}
			if (\u0003.GetFlag(SignatureFlag.External))
			{
				\u0005.\u0001(\u0002, \u0003, this.\u0002);
				\u0007 = this.\u0002;
				if (\u0004 + this.\u0002 > this.MaxStackSizeToCheckAgainst)
				{
					this.\u0001(\u0005, \u0004 + this.\u0002);
				}
			}
			else
			{
				_ICompiledPOU icompiledPOU = this._ICompileContext.GetCompiledPOUById(\u0003.Id) as _ICompiledPOU;
				if (icompiledPOU == null)
				{
					return true;
				}
				int num = \u0003.CalleeSize + icompiledPOU.ScratchSize + icompiledPOU.CodeGeneratorStackSize;
				int num2 = \u0003.CalleeSize + icompiledPOU.CodeGeneratorStackSize;
				int num3 = \u0004 + num2;
				\u0005.\u0001(\u0002, \u0003, num2);
				if (num3 > this.MaxStackSizeToCheckAgainst)
				{
					this.\u0001(\u0005, num3);
				}
				if (num3 + icompiledPOU.ScratchSize <= this.MaxStackSizeToCheckAgainst)
				{
					num3 = \u0004 + num;
					\u0005.\u0001();
					\u0005.\u0001(\u0002, \u0003, num);
				}
				\u0004 = num3;
				\u0007 = this.\u0001(\u0003, \u0004, \u0005, \u0006, \u0007);
				\u0007 += num;
			}
			\u0005.\u0001();
			return true;
		}

		// Token: 0x060023F3 RID: 9203 RVA: 0x0007B08C File Offset: 0x0007928C
		private void \u0001()
		{
			this.\u0002 = false;
			this.\u0001 = new LList<CallStack>();
		}

		// Token: 0x060023F4 RID: 9204 RVA: 0x0007B0A0 File Offset: 0x000792A0
		private void \u0001(CallStack \u0002)
		{
			if (\u0002 == null || !\u0002.Any<CallStackEntry>())
			{
				return;
			}
			foreach (CallStackEntry callStackEntry in \u0002.CallStackEntries)
			{
				int stackSize = callStackEntry.Size;
				if (callStackEntry.SignImplemented != null)
				{
					stackSize = this.\u0001(callStackEntry.SignImplemented);
				}
				if (this.\u0001(callStackEntry.SignImplemented))
				{
					callStackEntry.IsHiddenSignature = true;
				}
				callStackEntry.StackSize = stackSize;
				callStackEntry.SourcePosition = this.\u0001(null, callStackEntry.SignImplemented);
			}
		}

		// Token: 0x060023F5 RID: 9205 RVA: 0x0007B140 File Offset: 0x00079340
		private static bool \u0001(CallStack \u0002)
		{
			return ((\u0002 != null) ? \u0002.Entries : null) != null && \u0002.Entries.Any<IStackUsageEntry>();
		}

		// Token: 0x060023F6 RID: 9206 RVA: 0x0007B160 File Offset: 0x00079360
		private void \u0002(CallStack \u0002)
		{
			if (!CallTree.\u0001(\u0002))
			{
				return;
			}
			IStackUsageEntry[] array = \u0002.Entries.ToArray<IStackUsageEntry>();
			for (int i = 0; i < array.Length; i++)
			{
				IStackUsageEntry stackUsageEntry = array[i];
				if (!stackUsageEntry.IsHiddenSignature)
				{
					_ISignature isignature = stackUsageEntry.SignImplemented;
					if (isignature.OrgName == IdentifierConstants.MainSignatureName)
					{
						isignature = (this._Scope[isignature.ParentSignatureId] as _ISignature);
					}
					IStackUsageEntry stackUsageEntry2 = null;
					_ISignature isignature2 = null;
					if (i < array.Length - 1)
					{
						stackUsageEntry2 = array[i + 1];
						isignature2 = stackUsageEntry2.SignImplemented;
						if (isignature2.OrgName == IdentifierConstants.MainSignatureName)
						{
							isignature2 = (this._Scope[isignature2.ParentSignatureId] as _ISignature);
						}
					}
					ISourcePosition sourcePosition;
					if (stackUsageEntry2 != null && !stackUsageEntry2.IsHiddenSignature)
					{
						sourcePosition = this.\u0001(isignature, isignature2);
					}
					else
					{
						sourcePosition = this.\u0001(null, isignature);
					}
					(stackUsageEntry as CallStackEntry).SourcePosition = sourcePosition;
				}
			}
		}

		// Token: 0x060023F7 RID: 9207 RVA: 0x0007B250 File Offset: 0x00079450
		private void \u0001(int \u0002, CallStack \u0003)
		{
			CallStackEntry[] u = \u0003.Reverse<CallStackEntry>().ToArray<CallStackEntry>();
			string text = \u0003.Last<CallStackEntry>().SignImplemented.OrgName;
			string text2 = "__cycle__code__";
			if (text.StartsWith(text2))
			{
				text = text.Substring(text2.Length);
			}
			IMessageCategory messageCategory = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
			string u2 = global::\u0003.\u0006.\u0001(MessageId.Err_StackOverflowDetected, new object[]
			{
				text,
				this.MaxStackSizeToCheckAgainst,
				\u0002
			});
			_ICompilerMessage message = \u0019.\u0003.\u0001(null, u2, Severity.Error, MessageId.Err_StackOverflowDetected);
			APEnvironmentFacade.Instance.AddMessage(messageCategory, message);
			foreach (IMessage message2 in this.\u0001(u, \u0081.\u0002.Inf_RelatedPositionStackoverflow, Severity.Information))
			{
				APEnvironmentFacade.Instance.AddMessage(messageCategory, message2);
			}
		}

		// Token: 0x060023F8 RID: 9208 RVA: 0x0007B344 File Offset: 0x00079544
		private void \u0002()
		{
			Severity severity = Severity.Warning;
			if (APEnvironmentFacade.Instance.IsWarningMessageDisabled(MessageId.Wrn_StackCheckIncompleteDueToRecursion))
			{
				severity = Severity.SuppressedWarning;
			}
			if (this.\u0002)
			{
				foreach (CallStack callStack in this.\u0001)
				{
					CallTree.\u0002 u = new CallTree.\u0002();
					CallStackEntry[] array = callStack.Reverse<CallStackEntry>().ToArray<CallStackEntry>();
					if (!this.\u0001(array))
					{
						u.\u0001 = callStack.\u0001();
						u.\u0001.AddMessage(severity, MessageId.Wrn_StackCheckIncompleteDueToRecursion, new object[]
						{
							global::\u0006.\u0001.\u0001(this._Scope, u.\u0001)
						});
						_ICompilerMessage icompilerMessage = u.\u0001.GetMessages(false).FirstOrDefault(new Func<_ICompilerMessage, bool>(CallTree.<>c.<>9.\u0001));
						Severity u2 = (icompilerMessage != null && Severity.SuppressedWarning == icompilerMessage.Severity) ? Severity.SuppressedInformation : Severity.Information;
						IEnumerable<CallStackEntry> u3 = array.SkipWhile(new Func<CallStackEntry, bool>(u.\u0001)).Skip(1);
						u.\u0001.AddMessages(this.\u0001(u3, \u0081.\u0002.Inf_RelatedPositionRecursion, u2).ToList<IMessage>());
					}
				}
			}
		}

		// Token: 0x060023F9 RID: 9209 RVA: 0x0007B48C File Offset: 0x0007968C
		public static bool IsProperty(ISignature sign)
		{
			return sign.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY);
		}

		// Token: 0x060023FA RID: 9210 RVA: 0x0007B49C File Offset: 0x0007969C
		private bool \u0001(ISignature \u0002)
		{
			if (APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(\u0002, GUIHidingFlags.AllCommon) && \u0002.OrgName != IdentifierConstants.MainSignatureName && !CallTree.IsProperty(\u0002))
			{
				return true;
			}
			if (Helper.InvalidId != \u0002.ParentSignatureId)
			{
				ISignature signature = this._Scope[\u0002.ParentSignatureId];
				if (signature != null)
				{
					return this.\u0001(signature);
				}
			}
			return false;
		}

		// Token: 0x060023FB RID: 9211 RVA: 0x0007B50C File Offset: 0x0007970C
		private IEnumerable<IMessage> \u0001(IEnumerable<CallStackEntry> \u0002, string \u0003, Severity \u0004 = Severity.Information)
		{
			int num = -1;
			foreach (CallStackEntry callStackEntry in \u0002)
			{
				int num2 = callStackEntry.Size;
				if (callStackEntry.SignImplemented != null)
				{
					num2 = this.\u0001(callStackEntry.SignImplemented);
				}
				if (this.\u0001(callStackEntry.SignImplemented))
				{
					num += num2;
				}
				else
				{
					if (num >= 0)
					{
						yield return CallTree.\u0001(\u0003, num + 1, Severity.Information);
						num = -1;
					}
					string u = string.Format(\u0003, global::\u0006.\u0001.\u0001(this._Scope, callStackEntry.SignImplemented), num2);
					_ICompilerMessage icompilerMessage = \u0019.\u0003.\u0001(this.\u0001(null, callStackEntry.SignImplemented), u, \u0004, MessageId.Inf_RelatedPosition);
					yield return icompilerMessage;
				}
				callStackEntry = null;
			}
			IEnumerator<CallStackEntry> enumerator = null;
			if (num >= 0)
			{
				yield return CallTree.\u0001(\u0003, num, \u0004);
			}
			yield break;
			yield break;
		}

		// Token: 0x060023FC RID: 9212 RVA: 0x0007B534 File Offset: 0x00079734
		private static _ICompilerMessage \u0001(string \u0002, int \u0003, Severity \u0004 = Severity.Information)
		{
			string u = string.Format(\u0002, global::\u0011.\u0001.HiddenPousPlaceholder, \u0003);
			return \u0019.\u0003.\u0001(null, u, \u0004, MessageId.Inf_RelatedPosition);
		}

		// Token: 0x060023FD RID: 9213 RVA: 0x0007B560 File Offset: 0x00079760
		private bool \u0001(CallStackEntry[] \u0002)
		{
			_ISignature signCalled = \u0002[\u0002.Length - 1].SignCalled;
			bool flag = false;
			for (int i = 0; i < \u0002.Length; i++)
			{
				_ISignature signCalled2 = \u0002[i].SignCalled;
				if (!flag && signCalled2.Id == signCalled.Id)
				{
					flag = true;
				}
				if (flag && !this.\u0001(signCalled2))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060023FE RID: 9214 RVA: 0x0007B5BC File Offset: 0x000797BC
		private int \u0001(_ISignature \u0002)
		{
			_ICompiledPOU icompiledPOU = this._ICompileContext.GetCompiledPOUById(\u0002.Id) as _ICompiledPOU;
			int num = \u0002.CalleeSize;
			if (icompiledPOU != null)
			{
				num += icompiledPOU.ScratchSize + icompiledPOU.CodeGeneratorStackSize;
			}
			if (\u0002.Name.StartsWith("__CYCLE__CODE"))
			{
				return num;
			}
			return num + this.\u0002(\u0002);
		}

		// Token: 0x060023FF RID: 9215 RVA: 0x0007B618 File Offset: 0x00079818
		private int \u0002(_ISignature \u0002)
		{
			_ICompileContext icompileContext = this._ICompileContext;
			ICodegenerator3 codegenerator = ((icompileContext != null) ? icompileContext.Codegenerator : null) as ICodegenerator3;
			if (codegenerator != null && codegenerator.GetProperty(CodegeneratorProperties.EABIStackframe))
			{
				return 0;
			}
			if (\u0002.POUType != Operator.Program && \u0002.POUType != Operator.FunctionBlock)
			{
				return \u0002.Size;
			}
			return 0;
		}

		// Token: 0x06002400 RID: 9216 RVA: 0x0007B668 File Offset: 0x00079868
		private int \u0001(_ISignature \u0002, int \u0003, CallStack \u0004, CallStack \u0005, int \u0006)
		{
			if (\u0002.CalleeIdList == null)
			{
				return \u0006;
			}
			CallStack u = new CallStack();
			foreach (uint num in \u0002.CalleeIdList.Distinct<uint>())
			{
				if (num < 2147483648U)
				{
					bool u2 = (num & 1073741824U) == 1073741824U;
					uint u3 = num & 1073741823U;
					_ISignature u4 = CallTree.\u0001(this._Scope, (int)u3);
					if (!CallTree.\u0001(u4))
					{
						CallTree.CalleeNode calleeNode = this.\u0001(u4);
						CallStack callStack = new CallStack();
						int num2;
						if (calleeNode.\u0001(\u0003, u2, \u0004, callStack, out num2) && num2 > \u0006)
						{
							\u0006 = num2;
							u = callStack.\u0001();
						}
					}
				}
			}
			\u0005.\u0001(u);
			return \u0006;
		}

		// Token: 0x06002401 RID: 9217 RVA: 0x0007B738 File Offset: 0x00079938
		private static bool \u0001(_ISignature \u0002)
		{
			return \u0002 == null || \u0002.POUType == Operator.Type;
		}

		// Token: 0x06002402 RID: 9218 RVA: 0x0007B74C File Offset: 0x0007994C
		private bool \u0001(_ISignature \u0002, int \u0003, CallStack \u0004, ref int \u0005)
		{
			int num = 0;
			if (\u0002.GetAttributeIntValue(CompileAttributes.ATTRIBUTE_ESTIMATED_STACK_USAGE, ref num))
			{
				int num2 = \u0003 + num;
				\u0005 = num;
				if (num2 > this.MaxStackSizeToCheckAgainst)
				{
					this.\u0001(\u0004, num2);
				}
				return true;
			}
			return false;
		}

		// Token: 0x06002403 RID: 9219 RVA: 0x0007B788 File Offset: 0x00079988
		private void \u0001(_ISignature \u0002, _ISignature \u0003, CallStack \u0004)
		{
			CallStack callStack = \u0004.\u0001();
			callStack.\u0001(\u0002, \u0003, 0);
			this.\u0001.Add(callStack);
			this.\u0002 = true;
		}

		// Token: 0x06002404 RID: 9220 RVA: 0x0007B7B8 File Offset: 0x000799B8
		private void \u0001(CallStack \u0002, int \u0003)
		{
			if (this.\u0001)
			{
				throw new AppStackOverflowException(\u0003, \u0002.\u0001());
			}
		}

		// Token: 0x06002405 RID: 9221 RVA: 0x0007B7D0 File Offset: 0x000799D0
		private bool \u0001(_ISignature \u0002, CallStack \u0003, out _ISignature \u0004)
		{
			\u0004 = null;
			foreach (CallStackEntry callStackEntry in \u0003)
			{
				if (\u0002.Id == callStackEntry.SignCalled.Id)
				{
					this.\u0002 = true;
					\u0004 = callStackEntry.SignImplemented;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002406 RID: 9222 RVA: 0x0007B840 File Offset: 0x00079A40
		private CallTree.CalleeNode \u0001(_ISignature \u0002)
		{
			CallTree.CalleeNode result;
			if (this.\u0001.TryGetValue(\u0002.Id, ref result))
			{
				return result;
			}
			CallTree.CalleeNode calleeNode = new CallTree.CalleeNode(\u0002, this, this.\u0001);
			this.\u0001.Add(\u0002.Id, calleeNode);
			return calleeNode;
		}

		// Token: 0x17000695 RID: 1685
		// (get) Token: 0x06002407 RID: 9223 RVA: 0x0007B888 File Offset: 0x00079A88
		private IScope5 _Scope { get; }

		// Token: 0x17000696 RID: 1686
		// (get) Token: 0x06002408 RID: 9224 RVA: 0x0007B890 File Offset: 0x00079A90
		private _ICompileContext _ICompileContext { get; }

		// Token: 0x06002409 RID: 9225 RVA: 0x0007B898 File Offset: 0x00079A98
		private ISourcePosition \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			ISourcePosition result;
			try
			{
				result = this.\u0002(\u0002, \u0003);
			}
			catch
			{
				result = null;
			}
			return result;
		}

		// Token: 0x0600240A RID: 9226 RVA: 0x0007B8C8 File Offset: 0x00079AC8
		private ISourcePosition \u0002(_ISignature \u0002, _ISignature \u0003)
		{
			if (\u0002 == null)
			{
				\u0003 = this.\u0001(\u0003);
				return \u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0003.LibraryPath), \u0003.ObjectGuid, -1L, -1, 0);
			}
			\u0003 = this.\u0002(\u0003);
			\u0002 = this.\u0002(\u0002);
			ICompiledPOU compiledPOUById = this._ICompileContext.GetCompiledPOUById(\u0002.Id);
			if (compiledPOUById == null)
			{
				return null;
			}
			IStatement statement = compiledPOUById.ParseTree;
			if (statement == null || statement is _IEmptyStatement)
			{
				ICompiledPOU compiledPOU = APEnvironmentFacade.Instance.LanguageModelMgr.FindPrecompiledPOU(((_ICompiledPOU)compiledPOUById).ObjectGuid) as _ICompiledPOU;
				if (compiledPOU == null)
				{
					return null;
				}
				statement = compiledPOU.ParseTree;
				if (statement == null)
				{
					return null;
				}
				statement = (((_IStatement)statement).Duplicate() as IStatement);
				\u0019.\u0001.\u0001(\u0002.Id, this._ICompileContext, null, false, false, false, null).TypifyStatement(statement);
			}
			int projectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0002.LibraryPath);
			\u0002 = this.\u0001(\u0002);
			LList<ISourcePosition> llist = global::\u0012.\u0007.\u0001(statement as _IStatement, \u0003.Id, this._ICompileContext);
			if (llist != null && llist.Count > 0)
			{
				ISourcePosition sourcePosition = llist[0];
				if (sourcePosition != null)
				{
					return \u0019.\u0003.\u0001(projectHandle, \u0002.ObjectGuid, sourcePosition.Position, sourcePosition.PositionOffset, sourcePosition.Length);
				}
			}
			return \u0019.\u0003.\u0001(projectHandle, \u0002.ObjectGuid, 0L, 0, 0);
		}

		// Token: 0x0600240B RID: 9227 RVA: 0x0007BA30 File Offset: 0x00079C30
		private _ISignature \u0001(_ISignature \u0002)
		{
			if ("__MAIN" == \u0002.Name)
			{
				return (_ISignature)this._Scope[\u0002.ParentSignatureId];
			}
			return \u0002;
		}

		// Token: 0x0600240C RID: 9228 RVA: 0x0007BA5C File Offset: 0x00079C5C
		private _ISignature \u0002(_ISignature \u0002)
		{
			if (Operator.FunctionBlock == \u0002.POUType)
			{
				return (_ISignature)Array.Find<ISignature>(this._ICompileContext.GetSubSignatures(\u0002.ObjectGuid), new Predicate<ISignature>(CallTree.<>c.<>9.\u0001));
			}
			return \u0002;
		}

		// Token: 0x0600240D RID: 9229 RVA: 0x0007BAB0 File Offset: 0x00079CB0
		private bool \u0001(_ISignature \u0002)
		{
			if (\u0002.IsLibraryObject)
			{
				return true;
			}
			if (\u0002.ParentSignatureId != Helper.InvalidId)
			{
				_ISignature isignature = this._Scope[\u0002.ParentSignatureId] as _ISignature;
				if (isignature != null && isignature.IsLibraryObject)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600240E RID: 9230 RVA: 0x0007BAFC File Offset: 0x00079CFC
		private static _ISignature \u0001(_ICompileContext \u0002, int \u0003)
		{
			_ISignature isignature = \u0002[\u0003];
			if (isignature == null)
			{
				return null;
			}
			if (isignature.POUType == Operator.FunctionBlock)
			{
				foreach (object obj in isignature._SubSignatures)
				{
					_ISignature isignature2 = (_ISignature)obj;
					if (isignature2.Name == "__MAIN")
					{
						return \u0002[isignature2.Id];
					}
				}
				return null;
			}
			return isignature;
		}

		// Token: 0x0600240F RID: 9231 RVA: 0x0007BB90 File Offset: 0x00079D90
		private static _ISignature \u0001(IScope \u0002, int \u0003)
		{
			_ISignature isignature = \u0002[\u0003] as _ISignature;
			if (isignature == null)
			{
				return null;
			}
			if (isignature.POUType == Operator.FunctionBlock)
			{
				return isignature.GetSubSignature("__MAIN") as _ISignature;
			}
			return isignature;
		}

		// Token: 0x06002410 RID: 9232 RVA: 0x0007BBCC File Offset: 0x00079DCC
		private bool \u0001(int \u0002)
		{
			return \u0002 > this.MaxStackSizeToCheckAgainst;
		}

		// Token: 0x04000647 RID: 1607
		private bool \u0001;

		// Token: 0x04000648 RID: 1608
		private readonly int \u0001;

		// Token: 0x04000649 RID: 1609
		private readonly int \u0002;

		// Token: 0x0400064A RID: 1610
		private int \u0003;

		// Token: 0x0400064B RID: 1611
		private bool \u0002;

		// Token: 0x0400064C RID: 1612
		private LList<CallStack> \u0001 = new LList<CallStack>();

		// Token: 0x0400064D RID: 1613
		private readonly LDictionary<int, CallTree.CalleeNode> \u0001 = new LDictionary<int, CallTree.CalleeNode>();

		// Token: 0x0400064E RID: 1614
		private readonly InheritanceInformation \u0001;

		// Token: 0x0400064F RID: 1615
		[CompilerGenerated]
		private readonly IScope5 \u0001;

		// Token: 0x04000650 RID: 1616
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x0200021B RID: 539
		private sealed class CalleeNode
		{
			// Token: 0x17000697 RID: 1687
			// (get) Token: 0x06002411 RID: 9233 RVA: 0x0007BBD8 File Offset: 0x00079DD8
			private IEnumerable<_ISignature> PossibleCallees
			{
				get
				{
					IEnumerable<_ISignature> result;
					if ((result = this.\u0001) == null)
					{
						result = (this.\u0001 = this.\u0001(this.\u0001));
					}
					return result;
				}
			}

			// Token: 0x06002412 RID: 9234 RVA: 0x0007BC04 File Offset: 0x00079E04
			public CalleeNode(_ISignature signCallee, CallTree callTree, InheritanceInformation ii)
			{
				this.\u0001 = signCallee;
				this.\u0001 = callTree;
				this.\u0001 = ii;
			}

			// Token: 0x06002413 RID: 9235 RVA: 0x0007BC2C File Offset: 0x00079E2C
			public bool \u0001(int \u0002, bool \u0003, CallStack \u0004, CallStack \u0005, out int \u0006)
			{
				\u0006 = 0;
				if (!this.\u0001.Known)
				{
					CallStack callStack = null;
					int num = 0;
					int u = 0;
					int num2 = 0;
					KindOfCall kindOfCall = this.\u0001(\u0003);
					if (kindOfCall != KindOfCall.InterfaceCall)
					{
						num = this.\u0001(this.\u0001);
						callStack = new CallStack(this.\u0001, this.\u0001, num);
						u = \u0002 + num;
						if (this.\u0001.\u0001(this.\u0001, this.\u0001, u, \u0004, callStack, out num2))
						{
							this.\u0001.\u0001(num2 + num, callStack);
						}
					}
					this.\u0001(\u0002, \u0004, ref callStack, ref num, ref u, ref num2, kindOfCall);
				}
				else
				{
					_ISignature u2;
					if (this.\u0001(\u0003) == KindOfCall.InterfaceCall && this.\u0001.\u0001(this.\u0001, \u0004, out u2))
					{
						this.\u0001.\u0001(this.\u0001, u2, \u0004);
						return false;
					}
					if (this.\u0001.\u0001(\u0002 + this.\u0001.Size))
					{
						this.\u0001.\u0001(\u0004);
						this.\u0001.\u0001(\u0004, \u0002 + this.\u0001.Size);
					}
				}
				\u0006 = this.\u0001.Size;
				\u0005.\u0001(this.\u0001.\u0001);
				return true;
			}

			// Token: 0x06002414 RID: 9236 RVA: 0x0007BD5C File Offset: 0x00079F5C
			private void \u0001(int \u0002, CallStack \u0003, ref CallStack \u0004, ref int \u0005, ref int \u0006, ref int \u0007, KindOfCall \u0008)
			{
				if (\u0008 == KindOfCall.InterfaceCall || \u0008 == KindOfCall.VirtualFunctionCall)
				{
					foreach (_ISignature isignature in this.PossibleCallees)
					{
						\u0005 = this.\u0001(isignature);
						\u0004 = new CallStack(this.\u0001, isignature, \u0005);
						\u0006 = \u0002 + \u0005;
						if (this.\u0001.\u0001(this.\u0001, isignature, \u0006, \u0003, \u0004, out \u0007))
						{
							this.\u0001.\u0001(\u0007 + \u0005, \u0004);
						}
					}
				}
			}

			// Token: 0x06002415 RID: 9237 RVA: 0x0007BE00 File Offset: 0x0007A000
			private KindOfCall \u0001(bool \u0002)
			{
				_ISignature isignature = CallTree.\u0001(this.Scope, this.\u0001.ParentSignatureId);
				if (isignature != null)
				{
					return this.\u0001(\u0002, isignature);
				}
				if (this.\u0001.POUType == Operator.Program)
				{
					return KindOfCall.ProgramCall;
				}
				if (this.\u0001.POUType != Operator.FunctionBlock && this.\u0001.POUType != Operator.Type)
				{
					return KindOfCall.StaticFunctionCall;
				}
				if (\u0002)
				{
					return KindOfCall.VirtualFunctionCall;
				}
				return KindOfCall.StaticFunctionCall;
			}

			// Token: 0x06002416 RID: 9238 RVA: 0x0007BE68 File Offset: 0x0007A068
			private KindOfCall \u0001(bool \u0002, _ISignature \u0003)
			{
				Operator poutype = \u0003.POUType;
				if (poutype <= Operator.Type)
				{
					if (poutype == Operator.Action)
					{
						goto IL_52;
					}
					if (poutype != Operator.Program)
					{
						if (poutype != Operator.Type)
						{
							goto IL_59;
						}
						goto IL_52;
					}
				}
				else if (poutype <= Operator.Method)
				{
					if (poutype != Operator.VarGlobal)
					{
						if (poutype != Operator.Method)
						{
							goto IL_59;
						}
						goto IL_52;
					}
				}
				else
				{
					if (poutype == Operator.Interface)
					{
						return KindOfCall.InterfaceCall;
					}
					if (poutype != Operator.Property)
					{
						goto IL_59;
					}
					goto IL_52;
				}
				if (this.\u0001.POUType == Operator.Method)
				{
					return KindOfCall.StaticFunctionCall;
				}
				return KindOfCall.ProgramCall;
				IL_52:
				if (\u0002)
				{
					return KindOfCall.VirtualFunctionCall;
				}
				return KindOfCall.StaticFunctionCall;
				IL_59:
				Debug.\u0001(false);
				return KindOfCall.None;
			}

			// Token: 0x06002417 RID: 9239 RVA: 0x0007BED8 File Offset: 0x0007A0D8
			private IEnumerable<_ISignature> \u0001(InheritanceInformation \u0002)
			{
				IEnumerable<_ISignature> enumerable = \u0002.\u0001(this.\u0001);
				if (enumerable != null)
				{
					LList<_ISignature> llist = new LList<_ISignature>();
					foreach (_ISignature isignature in enumerable)
					{
						if (this.\u0001.ParentSignatureId == Helper.InvalidId)
						{
							llist.Add(isignature);
						}
						else
						{
							ISignature signature = this.Scope.CreateLocalScope(isignature).FindSignatureLocal(this.\u0001.Name);
							if (signature != null)
							{
								llist.Add(signature as _ISignature);
							}
						}
					}
					return llist.Where(new Func<_ISignature, bool>(this.\u0001)).GroupBy(new Func<_ISignature, int>(CallTree.CalleeNode.<>c.<>9.\u0001)).Select(new Func<IGrouping<int, _ISignature>, _ISignature>(CallTree.CalleeNode.<>c.<>9.\u0001));
				}
				return Array.Empty<_ISignature>();
			}

			// Token: 0x17000698 RID: 1688
			// (get) Token: 0x06002418 RID: 9240 RVA: 0x0007BFDC File Offset: 0x0007A1DC
			private _ICompileContext CompileContext
			{
				get
				{
					return this.\u0001._ICompileContext;
				}
			}

			// Token: 0x17000699 RID: 1689
			// (get) Token: 0x06002419 RID: 9241 RVA: 0x0007BFEC File Offset: 0x0007A1EC
			private IScope5 Scope
			{
				get
				{
					return this.\u0001._Scope;
				}
			}

			// Token: 0x0600241A RID: 9242 RVA: 0x0007BFFC File Offset: 0x0007A1FC
			private int \u0001(_ISignature \u0002)
			{
				_ICompileContext icompileContext = this.CompileContext;
				ICodegenerator3 codegenerator = ((icompileContext != null) ? icompileContext.Codegenerator : null) as ICodegenerator3;
				if (codegenerator != null && codegenerator.GetProperty(CodegeneratorProperties.EABIStackframe))
				{
					return 0;
				}
				if (\u0002.POUType != Operator.Program && \u0002.POUType != Operator.FunctionBlock)
				{
					return \u0002.Size;
				}
				return 0;
			}

			// Token: 0x0600241B RID: 9243 RVA: 0x0007C04C File Offset: 0x0007A24C
			[CompilerGenerated]
			private bool \u0001(_ISignature \u0002)
			{
				return \u0002.Id != this.\u0001.Id;
			}

			// Token: 0x04000651 RID: 1617
			private readonly _ISignature \u0001;

			// Token: 0x04000652 RID: 1618
			private readonly InheritanceInformation \u0001;

			// Token: 0x04000653 RID: 1619
			private IEnumerable<_ISignature> \u0001;

			// Token: 0x04000654 RID: 1620
			private readonly CallTree \u0001;

			// Token: 0x04000655 RID: 1621
			private readonly \u0019.\u0007 \u0001 = new \u0019.\u0007();
		}

		// Token: 0x0200021D RID: 541
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x06002421 RID: 9249 RVA: 0x0007C090 File Offset: 0x0007A290
			internal bool \u0001(_ICompiledPOU \u0002)
			{
				return \u0002.SignatureId == this.\u0001.Id;
			}

			// Token: 0x04000659 RID: 1625
			public _ISignature \u0001;
		}

		// Token: 0x0200021E RID: 542
		[CompilerGenerated]
		private sealed class \u0002
		{
			// Token: 0x06002423 RID: 9251 RVA: 0x0007C0B0 File Offset: 0x0007A2B0
			internal bool \u0001(CallStackEntry \u0002)
			{
				return \u0002.SignCalled.Id != this.\u0001.Id;
			}

			// Token: 0x0400065A RID: 1626
			public _ISignature \u0001;
		}
	}
}
