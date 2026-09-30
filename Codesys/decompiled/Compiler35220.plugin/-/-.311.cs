using System;
using System.Runtime.CompilerServices;
using \u0003;
using \u0007;
using \u000E;
using \u000F;
using \u0011;
using \u0014;
using \u0019;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0082;
using \u0083;

namespace \u0013
{
	// Token: 0x0200034B RID: 843
	internal sealed class \u000E
	{
		// Token: 0x17000856 RID: 2134
		// (get) Token: 0x060032E5 RID: 13029 RVA: 0x000C48F0 File Offset: 0x000C2AF0
		private _ISignature Signature { get; }

		// Token: 0x17000857 RID: 2135
		// (get) Token: 0x060032E6 RID: 13030 RVA: 0x000C48F8 File Offset: 0x000C2AF8
		private _ICompileContext Comcon
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
		}

		// Token: 0x17000858 RID: 2136
		// (get) Token: 0x060032E7 RID: 13031 RVA: 0x000C4908 File Offset: 0x000C2B08
		private \u001B CompileInformation { get; }

		// Token: 0x060032E8 RID: 13032 RVA: 0x000C4910 File Offset: 0x000C2B10
		internal \u000E(_ISignature \u001C\u0002, \u001B \u008F\u0004)
		{
			this.Signature = \u001C\u0002;
			this.CompileInformation = \u008F\u0004;
		}

		// Token: 0x060032E9 RID: 13033 RVA: 0x000C4928 File Offset: 0x000C2B28
		internal bool \u0001(IScope5 \u0002)
		{
			if (this.Signature.GetFlagInternal(SignatureFlagInternal.InitialValuesChecked))
			{
				return true;
			}
			this.Signature.SetFlagInternal(SignatureFlagInternal.InitialValuesChecked, true);
			foreach (_IVariable u in this.Signature.AllVariables)
			{
				this.\u0001(u, \u0002);
			}
			int num;
			int num2;
			if (\u0082.\u0007.\u0001(this.Signature, out num, out num2))
			{
				this.Signature.SetFlagInternal(SignatureFlagInternal.OptionalInputs, true);
			}
			return true;
		}

		// Token: 0x060032EA RID: 13034 RVA: 0x000C49C8 File Offset: 0x000C2BC8
		private void \u0001(_IVariable \u0002, IScope5 \u0003)
		{
			ExpressionTypifierWithSpecialTasks expressionTypifierWithSpecialTasks = new ExpressionTypifierWithSpecialTasks(\u0003 as global::\u0007.\u0005, this.Comcon, true, this.Signature, \u0002);
			expressionTypifierWithSpecialTasks.ContributeToCompile = true;
			ErrorVisitor errorVisitor = new ErrorVisitor();
			global::\u0014.\u0013.\u0001(\u0002, errorVisitor);
			_IType itype = \u0002.CompiledTypeInternal as _IType;
			if (itype == null)
			{
				return;
			}
			this.\u0001(\u0002);
			this.\u0001(\u0002, expressionTypifierWithSpecialTasks, itype);
			expressionTypifierWithSpecialTasks.TypeExpected = itype.EffectiveType;
			this.\u0001(\u0002, errorVisitor, itype);
			_IType itype2 = global::\u0013.\u000E.\u0001(itype);
			this.\u0001(\u0002, itype2);
			ISignature signature2;
			ISignature signature = global::\u0013.\u000E.\u0001(\u0002, \u0003, itype2, out signature2);
			if (signature != null)
			{
				new global::\u000F.\u0016(this.Signature, this.Comcon, \u0002, signature as _ISignature, signature2 as _ISignature).\u0001(\u0003, expressionTypifierWithSpecialTasks);
			}
			new \u0083.\u000E(this.Signature, this.Comcon, \u0002, itype, signature as _ISignature).\u0001(\u0003);
			if (!\u0083.\u0008.\u0001(this.Signature, \u0002))
			{
				this.Signature.AddMessages(errorVisitor.Messages);
			}
		}

		// Token: 0x060032EB RID: 13035 RVA: 0x000C4AC0 File Offset: 0x000C2CC0
		internal static _IType \u0001(_IType \u0002)
		{
			_IType itype = \u0002;
			while (itype.EffectiveType.Class == TypeClass.Array)
			{
				itype = ((_IArrayType)itype.EffectiveType)._Base;
			}
			return itype;
		}

		// Token: 0x060032EC RID: 13036 RVA: 0x000C4AF4 File Offset: 0x000C2CF4
		internal static ISignature \u0001(_IVariable \u0002, IScope \u0003, _IType \u0004, out ISignature \u0005)
		{
			ISignature signature = null;
			if (\u0004.EffectiveType.Class == TypeClass.Userdef)
			{
				signature = ((_IUserdefType)\u0004.EffectiveType).GetSignature(\u0003);
			}
			\u0005 = signature;
			if (signature != null && signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
			{
				_IVariable ivariable = signature["__Interface"] as _IVariable;
				if (ivariable != null)
				{
					_IUserdefType iuserdefType = ivariable._Type.BaseType as _IUserdefType;
					if (iuserdefType != null)
					{
						return iuserdefType.GetSignature(\u0003);
					}
				}
			}
			else
			{
				if (\u0004.EffectiveType.Class == TypeClass.Enum && !\u0002.GetFlag(VarFlag.Enum))
				{
					return ((_IEnumType)\u0004.EffectiveType).GetSignature(\u0003);
				}
				if (\u0004.Class == TypeClass.Userdef)
				{
					ISignature signature2 = ((_IUserdefType)\u0004).GetSignature(\u0003);
					if (signature2 != null && signature2.GetFlag(SignatureFlag.Alias))
					{
						return signature2;
					}
				}
				else
				{
					_IAliasType ialiasType = \u0002.CompiledTypeInternal as _IAliasType;
					if (ialiasType != null)
					{
						return ialiasType.GetSignature(\u0003);
					}
				}
			}
			return signature;
		}

		// Token: 0x060032ED RID: 13037 RVA: 0x000C4BDC File Offset: 0x000C2DDC
		private void \u0001(_IVariable \u0002, _IType \u0003)
		{
			if (this.Comcon != null && this.Comcon.NoDefaultInitialization && !\u0002.HasFlag(VarFlag.Temp) && !\u0002.HasFlag(VarFlag.Inout) && (this.Signature.POUType == Operator.Program || this.Signature.POUType == Operator.VarGlobal || (this.Signature.POUType == Operator.FunctionBlock && this.Signature.HasAttribute("no_default_initialisation"))) && \u0003.EffectiveType.Class != TypeClass.Userdef && \u0002.Initial == null)
			{
				\u0002.SetFlag(VarFlag.NoInit, true);
			}
		}

		// Token: 0x060032EE RID: 13038 RVA: 0x000C4C7C File Offset: 0x000C2E7C
		private void \u0001(_IVariable \u0002, ExpressionTypifierWithSpecialTasks \u0003, ICompiledType3 \u0004)
		{
			_IExpression iexpression = \u0002.Initial as _IExpression;
			if (iexpression != null)
			{
				\u0003.TypeExpected = \u0004.EffectiveType;
				iexpression.Accept(\u0003);
				if (\u0002.IsVarInoutConstant)
				{
					this.Signature.\u0001(\u0002.SourcePosition, Severity.Error, MessageId.Err_NoInitialForInoutConstant, new object[]
					{
						\u0002.OrgName
					});
				}
			}
		}

		// Token: 0x060032EF RID: 13039 RVA: 0x000C4CDC File Offset: 0x000C2EDC
		private void \u0001(_IVariable \u0002)
		{
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_GLOBAL_INIT_SLOT))
			{
				this.Signature.AddMessage(\u0002.SourcePosition as _ISourcePosition, Severity.Warning, MessageId.Wrn_GlobalInitSlotNotForVariables, Array.Empty<object>());
				if (!this.Signature.HasAttribute(CompileAttributes.ATTRIBUTE_GLOBAL_INIT_SLOT))
				{
					this.Signature.AddAttribute(CompileAttributes.ATTRIBUTE_GLOBAL_INIT_SLOT, \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_GLOBAL_INIT_SLOT));
				}
			}
		}

		// Token: 0x060032F0 RID: 13040 RVA: 0x000C4D44 File Offset: 0x000C2F44
		private void \u0001(_IVariable \u0002, ErrorVisitor \u0003, ICompiledType3 \u0004)
		{
			bool flag = \u0002.GetFlag(VarFlag.ReplacedConstant) || \u0002.GetFlag(VarFlag.Constant);
			bool flag2 = \u0002.Initial == null;
			bool flag3 = \u0002.GetFlag(VarFlag.Inout);
			bool flag4 = \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY);
			bool flag5 = \u0002.GetFlag(VarFlag.Enum);
			bool flag6 = \u0002.GetFlag(VarFlag.External);
			bool flag7 = flag3 || flag4 || flag5 || flag6;
			if (flag && flag2 && !flag7)
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Wrn_NoInitialValueForConstant, new object[]
				{
					\u0002.OrgName
				});
				_ICompilerMessage icompilerMessage = global::\u0019.\u0003.\u0001(\u0002.SourcePosition, u, Severity.Warning, MessageId.Wrn_NoInitialValueForConstant);
				icompilerMessage = \u0003.\u0001(icompilerMessage);
				this.Signature.AddMessage(icompilerMessage);
				if (\u0002.CompiledType != null)
				{
					\u0002.SetInitial(global::\u0019.\u0003.\u0001(\u0004.EffectiveType.Class));
					_IExpression iexpression = \u0002.Initial as _IExpression;
					if (iexpression != null)
					{
						iexpression._CompiledType = \u0004.EffectiveType;
					}
				}
			}
		}

		// Token: 0x0400099C RID: 2460
		[CompilerGenerated]
		private readonly _ISignature \u0001;

		// Token: 0x0400099D RID: 2461
		[CompilerGenerated]
		private readonly \u001B \u0001;
	}
}
