using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0007;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace \u0010
{
	// Token: 0x0200039A RID: 922
	internal sealed class \u0010
	{
		// Token: 0x170008C6 RID: 2246
		// (get) Token: 0x0600356D RID: 13677 RVA: 0x000D5D58 File Offset: 0x000D3F58
		// (set) Token: 0x0600356E RID: 13678 RVA: 0x000D5D60 File Offset: 0x000D3F60
		private _ICompileContext ComconNew { get; set; }

		// Token: 0x170008C7 RID: 2247
		// (get) Token: 0x0600356F RID: 13679 RVA: 0x000D5D6C File Offset: 0x000D3F6C
		// (set) Token: 0x06003570 RID: 13680 RVA: 0x000D5D74 File Offset: 0x000D3F74
		private IOnlineChangeDetails OnlineChangeDetails { get; set; }

		// Token: 0x170008C8 RID: 2248
		// (get) Token: 0x06003571 RID: 13681 RVA: 0x000D5D80 File Offset: 0x000D3F80
		// (set) Token: 0x06003572 RID: 13682 RVA: 0x000D5D88 File Offset: 0x000D3F88
		private int PrimaryProjectHandle { get; set; }

		// Token: 0x06003573 RID: 13683 RVA: 0x000D5D94 File Offset: 0x000D3F94
		public static void \u0001(_ICompileContext \u0002, IOnlineChangeDetails \u0003)
		{
			new global::\u0010.\u0010
			{
				ComconNew = \u0002,
				OnlineChangeDetails = \u0003
			}.\u0001();
		}

		// Token: 0x06003574 RID: 13684 RVA: 0x000D5DB0 File Offset: 0x000D3FB0
		private void \u0001()
		{
			this.PrimaryProjectHandle = APEnvironmentFacade.Instance.PrimaryProjectHandle;
			IList<ICompiledPOU4> compiledPOUsToCompileEx = this.ComconNew.GetCompiledPOUsToCompileEx();
			if (this.OnlineChangeDetails.InterfaceChanged)
			{
				this.\u0006();
			}
			this.\u0003();
			this.\u0001(compiledPOUsToCompileEx);
			this.\u0002();
		}

		// Token: 0x06003575 RID: 13685 RVA: 0x000D5E00 File Offset: 0x000D4000
		private void \u0002()
		{
			if (this.OnlineChangeDetails is IOnlineChangeDetails2 && (this.OnlineChangeDetails as IOnlineChangeDetails2).InterfacesToTest.Count > 0)
			{
				IOnlineChangeDetails2 onlineChangeDetails = this.OnlineChangeDetails as IOnlineChangeDetails2;
				if (onlineChangeDetails.InterfacesToTest.Count > 100)
				{
					string u = string.Format(\u0081.\u0001.NumInterfacesToTest, onlineChangeDetails.InterfacesToTest.Count);
					_ICompilerMessage message = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(this.PrimaryProjectHandle, Guid.Empty, -1L, -1, 0), u, Severity.Information, MessageId.None);
					APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message);
					return;
				}
				string u2 = \u0081.\u0001.InterfaceReferencesAffected;
				_ICompilerMessage message2 = \u0019.\u0003.\u0001(null, u2, Severity.Information, MessageId.None);
				APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message2);
				foreach (IVariableInfo variableInfo in onlineChangeDetails.InterfacesToTest)
				{
					ISignature signature = this.ComconNew[variableInfo.SignatureId];
					IVariable variable = signature[variableInfo.VariableId];
					string u3 = string.Format("    - {0}.{1}", signature.OrgName, variable.OrgName);
					_ISourcePosition sourcePosition = (variable as _IVariable)._SourcePosition;
					_ICompilerMessage message3 = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(this.PrimaryProjectHandle, signature.ObjectGuid, sourcePosition.Position, sourcePosition.PositionOffset, (short)variable.OrgName.Length), u3, Severity.Information, MessageId.None);
					APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message3);
				}
			}
		}

		// Token: 0x06003576 RID: 13686 RVA: 0x000D5FB8 File Offset: 0x000D41B8
		private void \u0001(IList<ICompiledPOU4> \u0002)
		{
			if (this.OnlineChangeDetails.CodeChanged)
			{
				bool flag = true;
				ILMCompileOptions3 ilmcompileOptions = APEnvironmentFacade.Instance.LMServiceProvider.ConfigurationService.CompileOptions as ILMCompileOptions3;
				if (ilmcompileOptions != null)
				{
					flag = ilmcompileOptions.ReportCompiledPousDuringIncrementalCompile;
				}
				if (!flag)
				{
					return;
				}
				List<_ICompiledPOU> list = global::\u0010.\u0010.\u0001(\u0002);
				if (list.Count != 0)
				{
					if (list.Count > 100)
					{
						string u = string.Format(\u0081.\u0001.NumPOUsDownloaded, list.Count);
						_ICompilerMessage message = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(this.PrimaryProjectHandle, Guid.Empty, -1L, -1, 0), u, Severity.Information, MessageId.None);
						APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message);
						return;
					}
					string u2 = \u0081.\u0001.POUsDownloaded;
					_ICompilerMessage message2 = \u0019.\u0003.\u0001(null, u2, Severity.Information, MessageId.None);
					APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message2);
					IScope5 u3 = global::\u0007.\u0005.\u0001(this.ComconNew);
					foreach (_ICompiledPOU u4 in list)
					{
						this.\u0001(u3, u4);
					}
				}
			}
		}

		// Token: 0x06003577 RID: 13687 RVA: 0x000D60EC File Offset: 0x000D42EC
		private void \u0001(IScope5 \u0002, _ICompiledPOU \u0003)
		{
			_ISignature isignature = \u0002[\u0003.SignatureId] as _ISignature;
			if (isignature == null)
			{
				return;
			}
			if (isignature.Name == IdentifierConstants.MainSignatureName)
			{
				isignature = (\u0002[isignature.ParentSignatureId] as _ISignature);
			}
			Guid objectGuid = isignature.ObjectGuid;
			string arg;
			if (isignature.ParentSignatureId != Helper.InvalidId)
			{
				_ISignature isignature2 = this.ComconNew[isignature.ParentSignatureId];
				if (isignature.Name == IdentifierConstants.MainSignatureName)
				{
					arg = isignature2.OrgName;
					objectGuid = isignature2.ObjectGuid;
				}
				else
				{
					arg = isignature2.OrgName + "." + isignature.OrgName;
				}
			}
			else
			{
				arg = isignature.OrgName;
			}
			string u = string.Format("    - {0}", arg);
			_ICompilerMessage message = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(this.PrimaryProjectHandle, objectGuid, 0L, 0, 0), u, Severity.Information, MessageId.None);
			APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message);
		}

		// Token: 0x06003578 RID: 13688 RVA: 0x000D61E0 File Offset: 0x000D43E0
		private static List<_ICompiledPOU> \u0001(IList<ICompiledPOU4> \u0002)
		{
			List<_ICompiledPOU> list = new List<_ICompiledPOU>();
			for (int i = 0; i < \u0002.Count; i++)
			{
				_ICompiledPOU icompiledPOU = \u0002[i] as _ICompiledPOU;
				if (!icompiledPOU.GetFlag(CompiledPOUFlags.Blob) && !icompiledPOU.GetFlag(CompiledPOUFlags.ConstBlob) && (!icompiledPOU.GetFlag(CompiledPOUFlags.Generated) || !(icompiledPOU.Name != IdentifierConstants.MainSignatureName)) && !icompiledPOU.GetFlag(CompiledPOUFlags.NoCompile) && icompiledPOU.GetFlag(CompiledPOUFlags.ToCompile))
				{
					list.Add(icompiledPOU);
				}
			}
			return list;
		}

		// Token: 0x06003579 RID: 13689 RVA: 0x000D626C File Offset: 0x000D446C
		private void \u0003()
		{
			if (this.OnlineChangeDetails.VariablesAffected.Count > 0)
			{
				if (this.OnlineChangeDetails.VariablesAffected.Count > 100)
				{
					this.\u0005();
					return;
				}
				this.\u0004();
			}
		}

		// Token: 0x0600357A RID: 13690 RVA: 0x000D62A4 File Offset: 0x000D44A4
		private void \u0004()
		{
			string u = \u0081.\u0001.VariablesAffected;
			_ICompilerMessage message = \u0019.\u0003.\u0001(null, u, Severity.Information, MessageId.None);
			APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message);
			foreach (IVariableInfo variableInfo in this.OnlineChangeDetails.VariablesAffected)
			{
				LStringBuilder lstringBuilder = new LStringBuilder();
				lstringBuilder.Append("(");
				Severity u2 = Severity.Information;
				bool flag = false;
				if ((variableInfo.Flags & VarFlag.LocationChanged) == VarFlag.LocationChanged)
				{
					u2 = Severity.Warning;
					lstringBuilder.Append(\u0081.\u0001.AffectionLocationChanged);
					flag = true;
				}
				if ((variableInfo.Flags & VarFlag.OnlChangeInit) == VarFlag.OnlChangeInit)
				{
					if (flag)
					{
						lstringBuilder.Append(", ");
					}
					lstringBuilder.Append(\u0081.\u0001.AffectionInitialized);
					flag = true;
				}
				if ((variableInfo.Flags & VarFlag.OnlChangeCopy) == VarFlag.OnlChangeCopy)
				{
					if (flag)
					{
						lstringBuilder.Append(", ");
					}
					lstringBuilder.Append(\u0081.\u0001.AffectionCopied);
					flag = true;
				}
				if ((variableInfo.Flags & (VarFlag)((ulong)-2147483648)) == (VarFlag)((ulong)-2147483648))
				{
					if (flag)
					{
						lstringBuilder.Append(", ");
					}
					lstringBuilder.Append(\u0081.\u0001.AffectionVFTableInitialized);
					flag = true;
				}
				if (variableInfo.Flags == VarFlag.None)
				{
					if (flag)
					{
						lstringBuilder.Append(", ");
					}
					lstringBuilder.Append(\u0081.\u0001.AffectionInitNewVariables);
				}
				lstringBuilder.Append(")");
				ISignature signature = this.ComconNew[variableInfo.SignatureId];
				IVariable variable = signature[variableInfo.VariableId];
				string u3 = string.Format("    - {0}.{1}  {2}", signature.OrgName, variable.OrgName, lstringBuilder);
				_ISourcePosition sourcePosition = (variable as _IVariable)._SourcePosition;
				_ICompilerMessage message2 = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(this.PrimaryProjectHandle, signature.ObjectGuid, sourcePosition.Position, sourcePosition.PositionOffset, (short)variable.OrgName.Length), u3, u2, MessageId.None);
				APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message2);
			}
		}

		// Token: 0x0600357B RID: 13691 RVA: 0x000D64EC File Offset: 0x000D46EC
		private void \u0005()
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			Severity u = Severity.Information;
			foreach (IVariableInfo variableInfo in this.OnlineChangeDetails.VariablesAffected)
			{
				if ((variableInfo.Flags & VarFlag.LocationChanged) == VarFlag.LocationChanged)
				{
					num++;
					u = Severity.Warning;
				}
				if ((variableInfo.Flags & VarFlag.OnlChangeInit) == VarFlag.OnlChangeInit)
				{
					num2++;
				}
				if ((variableInfo.Flags & VarFlag.OnlChangeCopy) == VarFlag.OnlChangeCopy)
				{
					num3++;
				}
				if ((variableInfo.Flags & (VarFlag)((ulong)-2147483648)) == (VarFlag)((ulong)-2147483648))
				{
					num4++;
				}
			}
			string u2 = string.Format(\u0081.\u0001.NumVariablesAffected, new object[]
			{
				this.OnlineChangeDetails.VariablesAffected.Count,
				num,
				num2,
				num3,
				num4
			});
			_ICompilerMessage message = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(this.PrimaryProjectHandle, Guid.Empty, -1L, -1, 0), u2, u, MessageId.None);
			APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message);
		}

		// Token: 0x0600357C RID: 13692 RVA: 0x000D663C File Offset: 0x000D483C
		private void \u0006()
		{
			List<_ISignature> list = new List<_ISignature>();
			foreach (_ISignature isignature in this.ComconNew.GetAllSignaturesFlatEx().OfType<_ISignature>())
			{
				if (isignature.GetFlag(SignatureFlag.OnlineChanged))
				{
					list.Add(isignature);
				}
			}
			int count = list.Count;
			if (list.Count > 100)
			{
				string u = string.Format(\u0081.\u0001.NumInterfacesChanged, list.Count);
				_ICompilerMessage message = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(this.PrimaryProjectHandle, Guid.Empty, -1L, -1, 0), u, Severity.Information, MessageId.None);
				APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message);
				return;
			}
			string u2 = \u0081.\u0001.InterfacesChanged;
			_ICompilerMessage message2 = \u0019.\u0003.\u0001(null, u2, Severity.Information, MessageId.None);
			APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message2);
			foreach (_ISignature isignature2 in list)
			{
				string str;
				if (isignature2.ParentSignatureId != Helper.InvalidId)
				{
					str = this.ComconNew[isignature2.ParentSignatureId].OrgName + "." + isignature2.OrgName;
				}
				else
				{
					str = isignature2.OrgName;
				}
				string u3 = "    - " + str;
				_ICompilerMessage message3 = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(this.PrimaryProjectHandle, isignature2.ObjectGuid, 0L, 0, 0), u3, Severity.Information, MessageId.None);
				APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message3);
			}
		}

		// Token: 0x04000A6A RID: 2666
		[CompilerGenerated]
		private _ICompileContext \u0001;

		// Token: 0x04000A6B RID: 2667
		[CompilerGenerated]
		private IOnlineChangeDetails \u0001;

		// Token: 0x04000A6C RID: 2668
		[CompilerGenerated]
		private int \u0001;
	}
}
