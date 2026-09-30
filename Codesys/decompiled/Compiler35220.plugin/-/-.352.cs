using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0018;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u001E
{
	// Token: 0x02000398 RID: 920
	internal sealed class \u0018
	{
		// Token: 0x170008BE RID: 2238
		// (get) Token: 0x06003551 RID: 13649 RVA: 0x000D3168 File Offset: 0x000D1368
		private _ICompilerMessage Message { get; }

		// Token: 0x170008BF RID: 2239
		// (get) Token: 0x06003552 RID: 13650 RVA: 0x000D3170 File Offset: 0x000D1370
		private \u0018.\u0011 Context { get; }

		// Token: 0x170008C0 RID: 2240
		// (get) Token: 0x06003553 RID: 13651 RVA: 0x000D3178 File Offset: 0x000D1378
		private _ISignature Signature { get; }

		// Token: 0x170008C1 RID: 2241
		// (get) Token: 0x06003554 RID: 13652 RVA: 0x000D3180 File Offset: 0x000D1380
		private string LibraryId { get; }

		// Token: 0x170008C2 RID: 2242
		// (get) Token: 0x06003555 RID: 13653 RVA: 0x000D3188 File Offset: 0x000D1388
		private _ICompileContext ComCon
		{
			get
			{
				return this.Context.ComCon;
			}
		}

		// Token: 0x170008C3 RID: 2243
		// (get) Token: 0x06003556 RID: 13654 RVA: 0x000D3198 File Offset: 0x000D1398
		private IMessageStorage Messagestorage
		{
			get
			{
				return this.Context.Storage;
			}
		}

		// Token: 0x170008C4 RID: 2244
		// (get) Token: 0x06003557 RID: 13655 RVA: 0x000D31A8 File Offset: 0x000D13A8
		private IMessageCategory CMC
		{
			get
			{
				return this.Context.Category;
			}
		}

		// Token: 0x170008C5 RID: 2245
		// (get) Token: 0x06003558 RID: 13656 RVA: 0x000D31B8 File Offset: 0x000D13B8
		private IDictionary<string, int> LibsSummarized
		{
			get
			{
				return this.Context.SummarizedLibErrors;
			}
		}

		// Token: 0x06003559 RID: 13657 RVA: 0x000D31C8 File Offset: 0x000D13C8
		public \u0018(_ICompilerMessage \u008A\u0005, \u0018.\u0011 \u008B\u0005, _ISignature \u001C\u0002, string \u008C\u0005)
		{
			this.Message = \u008A\u0005;
			this.Context = \u008B\u0005;
			this.Signature = \u001C\u0002;
			this.LibraryId = \u008C\u0005;
		}

		// Token: 0x0600355A RID: 13658 RVA: 0x000D31F0 File Offset: 0x000D13F0
		internal void \u0001()
		{
			FilterMessageOutputEventArgs filterMessageOutputEventArgs = new FilterMessageOutputEventArgs2(this.CMC, this.Message, this.ComCon.ApplicationGuid);
			APEnvironmentFacade.Instance.LanguageModelMgr.OnFilterMessageOutput(this.ComCon, filterMessageOutputEventArgs);
			if (filterMessageOutputEventArgs.Ignore)
			{
				return;
			}
			if (this.Context.\u0002)
			{
				return;
			}
			this.\u0002();
		}

		// Token: 0x0600355B RID: 13659 RVA: 0x000D3250 File Offset: 0x000D1450
		private _ICompilerMessage \u0002()
		{
			Severity severity = this.Message.Severity.GetSeverity(this.Message.MessageId);
			_ICompilerMessage result = this.Message;
			if (severity != this.Message.Severity)
			{
				result = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(this.Message.ProjectHandle, this.Message.ObjectGuid, this.Message.Position, this.Message.PositionOffset, this.Message.Length), this.Message.Text, severity, this.Message.MessageId);
			}
			return result;
		}

		// Token: 0x0600355C RID: 13660 RVA: 0x000D32E8 File Offset: 0x000D14E8
		private void \u0002()
		{
			if (this.Messagestorage == null || !this.Message.ShowCompile)
			{
				return;
			}
			_ICompilerMessage icompilerMessage = this.\u0002();
			Severity severity = icompilerMessage.Severity;
			bool flag;
			if (severity <= Severity.Information)
			{
				switch (severity)
				{
				case Severity.FatalError:
					this.Messagestorage.ClearMessages(this.CMC);
					this.Context.\u0002 = true;
					flag = true;
					goto IL_A2;
				case Severity.Error:
					flag = (!this.\u0004() && !this.\u0003());
					goto IL_A2;
				case Severity.FatalError | Severity.Error:
					break;
				case Severity.Warning:
					flag = this.\u0002();
					goto IL_A2;
				default:
					if (severity == Severity.Information)
					{
						flag = this.\u0001();
						goto IL_A2;
					}
					break;
				}
			}
			else if (severity == Severity.SuppressedWarning || severity == Severity.SuppressedInformation)
			{
				flag = false;
				goto IL_A2;
			}
			flag = true;
			IL_A2:
			if (flag)
			{
				this.Messagestorage.AddMessage(this.CMC, icompilerMessage);
			}
			this.Context.\u0001 = (this.Context.\u0001 && icompilerMessage.Severity != Severity.Error && icompilerMessage.Severity != Severity.FatalError);
		}

		// Token: 0x0600355D RID: 13661 RVA: 0x000D33DC File Offset: 0x000D15DC
		private bool \u0001()
		{
			if (this.Message.MessageId == MessageId.Inf_RelatedPosition && this.Context.\u0001 > 500)
			{
				return false;
			}
			_ISignature isignature = this.Signature;
			return isignature == null || !isignature.IsCompiledLibraryObject || this.Message.MessageId == MessageId.Wrn_ReservedUnusedKeyword;
		}

		// Token: 0x0600355E RID: 13662 RVA: 0x000D3438 File Offset: 0x000D1638
		private bool \u0002()
		{
			if (APEnvironmentFacade.Instance.IsWarningAsError(this.Message.MessageId))
			{
				return true;
			}
			bool flag = true;
			bool flag2 = \u0018.\u0001(this.ComCon, this.Signature) && this.Message.MessageId != MessageId.Wrn_ReservedUnusedKeyword;
			flag = (flag && \u0018.\u0001(this.Message));
			flag = (flag && !flag2);
			if (!flag || APEnvironmentFacade.Instance.IsWarningMessageDisabled(this.Message.MessageId))
			{
				flag = false;
			}
			else
			{
				this.Context.\u0002++;
			}
			int maxCompilerWarnings = APEnvironmentFacade.Instance.CompileOptions.MaxCompilerWarnings;
			if (this.Context.\u0002 == maxCompilerWarnings + 1)
			{
				string u = string.Format(\u0081.\u0001.WarningLimitExceeded, maxCompilerWarnings);
				this.Messagestorage.AddMessage(this.CMC, \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(), u, Severity.Warning, MessageId.None));
			}
			if (this.Context.\u0002 > maxCompilerWarnings && MessageId.Wrn_StackCheckIncompleteDueToRecursion != this.Message.MessageId)
			{
				flag = false;
			}
			return flag;
		}

		// Token: 0x0600355F RID: 13663 RVA: 0x000D3550 File Offset: 0x000D1750
		private bool \u0003()
		{
			this.Context.\u0001++;
			if (this.Context.\u0001 == 501)
			{
				string u = string.Format(\u0081.\u0001.ErrorLimitExceeded, 500);
				this.Messagestorage.AddMessage(this.CMC, \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(), u, Severity.Error, MessageId.None));
			}
			return this.Context.\u0001 > 500;
		}

		// Token: 0x06003560 RID: 13664 RVA: 0x000D35C8 File Offset: 0x000D17C8
		private bool \u0004()
		{
			if (!string.IsNullOrEmpty(this.LibraryId) && !this.ComCon.IsDefined("DumpInternalLibraryErrors"))
			{
				int num;
				if (this.LibsSummarized.TryGetValue(this.LibraryId, out num))
				{
					this.LibsSummarized[this.LibraryId] = num + 1;
					this.Context.\u0001 = false;
					return true;
				}
				if (APEnvironmentFacade.Instance.IsLibraryWithErrorSummarization(this.LibraryId))
				{
					this.LibsSummarized[this.LibraryId] = 1;
					this.Context.\u0001 = false;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06003561 RID: 13665 RVA: 0x000D3660 File Offset: 0x000D1860
		private static bool \u0001(_ICompileContext \u0002, _ISignature \u0003)
		{
			bool flag = false;
			if (\u0003 != null)
			{
				flag = \u0003.IsCompiledLibraryObject;
				if (!flag && \u0003.ParentSignatureId != Helper.InvalidId)
				{
					ISignature6 signature = \u0002.GetSignatureById(\u0003.ParentSignatureId) as ISignature6;
					if (signature != null)
					{
						flag = signature.IsCompiledLibraryObject;
					}
				}
			}
			return flag;
		}

		// Token: 0x06003562 RID: 13666 RVA: 0x000D36A8 File Offset: 0x000D18A8
		private static bool \u0001(_ICompilerMessage \u0002)
		{
			bool result = true;
			uint? number = \u0002.Number;
			uint num = 325U;
			if (!(number.GetValueOrDefault() == num & number != null))
			{
				number = \u0002.Number;
				num = 33U;
				if (!(number.GetValueOrDefault() == num & number != null))
				{
					return result;
				}
			}
			if (APEnvironmentFacade.Instance.ExistsPrimaryProject && \u0002.ObjectGuid != Guid.Empty)
			{
				result = (!APEnvironmentFacade.Instance.IsLibrary(\u0002.ProjectHandle) && APEnvironmentFacade.Instance.ExistsObject(\u0002.ProjectHandle, \u0002.ObjectGuid));
			}
			return result;
		}

		// Token: 0x04000A64 RID: 2660
		[CompilerGenerated]
		private readonly _ICompilerMessage \u0001;

		// Token: 0x04000A65 RID: 2661
		[CompilerGenerated]
		private readonly \u0018.\u0011 \u0001;

		// Token: 0x04000A66 RID: 2662
		[CompilerGenerated]
		private readonly _ISignature \u0001;

		// Token: 0x04000A67 RID: 2663
		[CompilerGenerated]
		private readonly string \u0001;

		// Token: 0x04000A68 RID: 2664
		private const int \u0001 = 500;
	}
}
