using System;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u0019;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000E8 RID: 232
	internal sealed class PersistentDownloadInfoFactory
	{
		// Token: 0x170004A3 RID: 1187
		// (get) Token: 0x0600102F RID: 4143 RVA: 0x0002D7B0 File Offset: 0x0002B9B0
		private _ICompileContext CompileContext { get; }

		// Token: 0x170004A4 RID: 1188
		// (get) Token: 0x06001030 RID: 4144 RVA: 0x0002D7B8 File Offset: 0x0002B9B8
		private DownloadInfoFlags DownloadInfoFlags { get; }

		// Token: 0x06001031 RID: 4145 RVA: 0x0002D7C0 File Offset: 0x0002B9C0
		internal PersistentDownloadInfoFactory(_ICompileContext comcon, DownloadInfoFlags dif)
		{
			this.CompileContext = comcon;
			this.DownloadInfoFlags = dif;
		}

		// Token: 0x06001032 RID: 4146 RVA: 0x0002D7D8 File Offset: 0x0002B9D8
		internal void \u0001(DownloadInfo \u0002)
		{
			if (!this.DownloadInfoFlags.OnlineChange)
			{
				foreach (_ISignature isignature in this.CompileContext.AllSignatureList.Where(new Func<_ISignature, bool>(PersistentDownloadInfoFactory.<>c.<>9.\u0001)))
				{
					int area = (int)isignature.AllVariables[0].DataLocation.Area;
					for (int i = 0; i < \u0002.Areas.Length; i++)
					{
						if (\u0002.Areas[i].Index == area)
						{
							Debug.\u0001((\u0002.Areas[i].Flags & DataSegmentFlags.Persistent) == DataSegmentFlags.Persistent);
							\u0002.Areas[i] = \u0019.\u0003.\u0001(\u0002.Areas[i], isignature.CRC);
							break;
						}
					}
				}
			}
		}

		// Token: 0x06001033 RID: 4147 RVA: 0x0002D8E0 File Offset: 0x0002BAE0
		internal IDataLocation \u0001(LList<ICodePiece2> \u0002)
		{
			_ISignature u;
			IDataLocation dataLocation = this.\u0001(out u);
			if (!this.DownloadInfoFlags.BootProject && !this.DownloadInfoFlags.OfflineBootProject)
			{
				ICodePiece2 codePiece = this.\u0001(u, dataLocation);
				if (codePiece != null)
				{
					\u0002.Add(codePiece);
				}
			}
			return dataLocation;
		}

		// Token: 0x06001034 RID: 4148 RVA: 0x0002D92C File Offset: 0x0002BB2C
		private IDataLocation \u0001(out _ISignature \u0002)
		{
			\u0002 = null;
			foreach (ISignature signature in this.CompileContext.GVLSignatures)
			{
				if (signature.GetFlag(SignatureFlag.Persistent))
				{
					\u0002 = (signature as _ISignature);
					break;
				}
			}
			if (\u0002 == null)
			{
				return null;
			}
			ISignature signature2 = this.CompileContext.GetSignature(\u0002.Name + "__GVL");
			IVariable variable = null;
			if (signature2 != null)
			{
				variable = signature2["__bInitNew2"];
			}
			if (variable == null)
			{
				variable = \u0002["__bInitNew"];
			}
			return variable.DataLocation;
		}

		// Token: 0x06001035 RID: 4149 RVA: 0x0002D9C0 File Offset: 0x0002BBC0
		private ICodePiece2 \u0001(_ISignature \u0002, IDataLocation \u0003)
		{
			if (\u0002 == null)
			{
				return null;
			}
			bool flag = false;
			uint num;
			uint num2;
			if (APEnvironmentFacade.Instance.GetOnlineApplicationPersistentInfo(this.CompileContext.ApplicationGuid, out num, out num2))
			{
				uint u = this.\u0001(\u0002, ref flag, num, num2);
				flag = PersistentDownloadInfoFactory.\u0001(flag, num, num2, u);
			}
			byte[] array = new byte[1];
			if (flag)
			{
				array[0] = 1;
			}
			else
			{
				array[0] = 0;
			}
			return new CodePiece(array, \u0003);
		}

		// Token: 0x06001036 RID: 4150 RVA: 0x0002DA24 File Offset: 0x0002BC24
		private uint \u0001(_ISignature \u0002, ref bool \u0003, uint \u0004, uint \u0005)
		{
			uint num = 0U;
			string attributeValue = \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC);
			uint result;
			try
			{
				result = uint.Parse(attributeValue);
			}
			catch
			{
				result = 0U;
			}
			IScope scope = this.CompileContext.CreateGlobalIScope();
			foreach (IVariable variable in \u0002.AllVariables)
			{
				uint num2 = (uint)(variable.DataLocation.Offset + variable.CompiledType.Size(scope));
				if (num2 > num)
				{
					num = num2;
				}
			}
			if (\u0005 < num && \u0005 != 0U)
			{
				uint num3 = 0U;
				global::\u0003.\u0002.\u0001(\u0002, \u0005, this.CompileContext, out num3);
				if (num3 == \u0004)
				{
					\u0003 = true;
				}
			}
			return result;
		}

		// Token: 0x06001037 RID: 4151 RVA: 0x0002DAF0 File Offset: 0x0002BCF0
		private static bool \u0001(bool \u0002, uint \u0003, uint \u0004, uint \u0005)
		{
			if (!\u0002 && \u0003 != 0U && \u0003 != \u0005 && \u0004 != 0U)
			{
				string stMessage = \u0081.\u0001.PersistenVariablesChanged;
				PromptResult promptResult = APEnvironmentFacade.Instance.Prompt(stMessage, PromptChoice.YesNoCancel, PromptResult.Yes, "PersistenVariablesChanged", Array.Empty<object>());
				if (PromptResult.No == promptResult)
				{
					\u0002 = true;
				}
				else if (PromptResult.Cancel == promptResult)
				{
					string u = \u0081.\u0002.Err_PersistentVariablesChangeMessageText;
					IMessageCategory messageCategory = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
					_ICompilerMessage message = \u0019.\u0003.\u0001(null, u, Severity.Error, MessageId.Err_PersistentVariablesChangeMessageText);
					APEnvironmentFacade.Instance.AddMessage(messageCategory, message);
					throw new CancelledByUserException();
				}
			}
			return \u0002;
		}

		// Token: 0x040002E1 RID: 737
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x040002E2 RID: 738
		[CompilerGenerated]
		private readonly DownloadInfoFlags \u0001;
	}
}
