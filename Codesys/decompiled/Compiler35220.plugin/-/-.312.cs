using System;
using System.Runtime.CompilerServices;
using \u0007;
using \u000E;
using \u0014;
using \u0019;
using \u001F;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;
using \u0083;

namespace \u0004
{
	// Token: 0x0200034C RID: 844
	internal sealed class \u0013
	{
		// Token: 0x17000859 RID: 2137
		// (get) Token: 0x060032F1 RID: 13041 RVA: 0x000C4E38 File Offset: 0x000C3038
		private _ISignature Signature { get; }

		// Token: 0x1700085A RID: 2138
		// (get) Token: 0x060032F2 RID: 13042 RVA: 0x000C4E40 File Offset: 0x000C3040
		private _ICompileContext Comcon
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
		}

		// Token: 0x1700085B RID: 2139
		// (get) Token: 0x060032F3 RID: 13043 RVA: 0x000C4E50 File Offset: 0x000C3050
		private \u001B CompileInformation { get; }

		// Token: 0x060032F4 RID: 13044 RVA: 0x000C4E58 File Offset: 0x000C3058
		internal \u0013(_ISignature \u001C\u0002, \u001B \u008F\u0004)
		{
			this.Signature = \u001C\u0002;
			this.CompileInformation = \u008F\u0004;
		}

		// Token: 0x060032F5 RID: 13045 RVA: 0x000C4E70 File Offset: 0x000C3070
		internal void \u0001(_IVariable \u0002, IScope5 \u0003)
		{
			if (!(\u0002.CompiledTypeInternal is ICompiledType3))
			{
				return;
			}
			Guid u = this.Signature.ObjectGuid;
			if (this.Signature.MessageGuid != Guid.Empty)
			{
				u = this.Signature.MessageGuid;
			}
			_ISourcePosition isourcePosition = global::\u0019.\u0003.\u0001(-1, u, \u0002.SourcePosition.Position, \u0002.SourcePosition.PositionOffset, (short)\u0002.OrgName.Length);
			ErrorVisitor errorVisitor = new ErrorVisitor();
			if (!\u0002.HasFlag(VarFlag.Typified))
			{
				_IType type = this.\u0001(\u0002, \u0003, errorVisitor, isourcePosition);
				\u0002.SetType(type);
				\u0002.SetFlag(VarFlag.Typified, true);
			}
			ICompiledType3 compiledType = \u0002.CompiledTypeInternal as ICompiledType3;
			this.\u0001(\u0002, errorVisitor, isourcePosition);
			if (\u0002.GetFlag(VarFlag.Inout))
			{
				\u0002.SetType(global::\u0019.\u0003.\u0001(compiledType as _IType));
			}
			if (\u0002.Address != null && \u0002.Address.Incomplete && this.Comcon.IsDefined(CompilerDefines.IGNORE_VAR_CONFIG))
			{
				IOEMCustomization oemcustomization = APEnvironmentFacade.Instance.OEMCustomization;
				bool flag;
				try
				{
					flag = (oemcustomization != null && oemcustomization.HasValue("LanguageModelManager", "KeepIncompleteAddress") && oemcustomization.GetBoolValue("LanguageModelManager", "KeepIncompleteAddress"));
				}
				catch (Exception)
				{
					flag = false;
				}
				if (!flag)
				{
					\u0002.Address = null;
				}
			}
			if (\u0002.Address != null && \u0002.Address.Size == DirectVariableSize.X && \u0002.Type.Class == TypeClass.Bool)
			{
				\u0002.SetType(TypeTable.DirectAddressBitType);
			}
			if (!\u0083.\u0008.\u0001(this.Signature, \u0002))
			{
				this.Signature.AddMessages(errorVisitor.Messages);
			}
		}

		// Token: 0x060032F6 RID: 13046 RVA: 0x000C5024 File Offset: 0x000C3224
		internal _IType \u0001(_IVariable \u0002, IScope5 \u0003, ErrorVisitor \u0004, _ISourcePosition \u0005)
		{
			ExpressionTypifierWithSpecialTasks expressionTypifierWithSpecialTasks = new ExpressionTypifierWithSpecialTasks(\u0003 as global::\u0007.\u0005, this.Comcon, true, this.Signature, \u0002);
			expressionTypifierWithSpecialTasks.ContributeToCompile = true;
			\u001F.\u0007 u = new global::\u0014.\u0012(\u0003 as global::\u0007.\u0005, this.Comcon, null);
			global::\u0014.\u0013.\u0001(\u0002, \u0004);
			global::\u000E.\u0017 u2 = global::\u000E.\u0017.\u0001(\u0003, this.CompileInformation, u, expressionTypifierWithSpecialTasks, \u0005, this.Signature, \u0002);
			u2.Errorvisitor = \u0004;
			return TypeCompiler.\u0001(\u0002.OriginalType as _IType, u2, false);
		}

		// Token: 0x060032F7 RID: 13047 RVA: 0x000C50A0 File Offset: 0x000C32A0
		private void \u0001(_IVariable \u0002, ErrorVisitor \u0003, _ISourcePosition \u0004)
		{
			if (\u0002.HasFlag(VarFlag.Output) && !this.Signature.Name.Equals(\u0002.Name, StringComparison.InvariantCultureIgnoreCase))
			{
				_IAliasType ialiasType = \u0002.Type as _IAliasType;
				IType type = (ialiasType != null) ? ialiasType.EffectiveType : null;
				if ((type ?? \u0002.Type).Class == TypeClass.Reference)
				{
					\u0003.MessageList.Add(global::\u0019.\u0003.\u0001(\u0004, \u0081.\u0002.Err_NoReferenceToOutput, Severity.Error, MessageId.Err_NoReferenceToOutput));
				}
			}
		}

		// Token: 0x0400099E RID: 2462
		[CompilerGenerated]
		private readonly _ISignature \u0001;

		// Token: 0x0400099F RID: 2463
		[CompilerGenerated]
		private readonly \u001B \u0001;
	}
}
