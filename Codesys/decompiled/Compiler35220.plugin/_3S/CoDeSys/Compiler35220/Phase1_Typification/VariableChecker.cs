using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0004;
using \u0006;
using \u000E;
using \u0011;
using \u0013;
using \u0014;
using \u0019;
using \u001B;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0083;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.Phase1_Typification
{
	// Token: 0x02000350 RID: 848
	internal sealed class VariableChecker
	{
		// Token: 0x0600331B RID: 13083 RVA: 0x000C5C80 File Offset: 0x000C3E80
		public VariableChecker(global::\u000E.\u001B ci)
		{
			this.CompileInformation = ci;
		}

		// Token: 0x17000867 RID: 2151
		// (get) Token: 0x0600331C RID: 13084 RVA: 0x000C5C90 File Offset: 0x000C3E90
		private global::\u000E.\u001B CompileInformation { get; }

		// Token: 0x17000868 RID: 2152
		// (get) Token: 0x0600331D RID: 13085 RVA: 0x000C5C98 File Offset: 0x000C3E98
		private _ICompileContext ComconNew
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
		}

		// Token: 0x0600331E RID: 13086 RVA: 0x000C5CA8 File Offset: 0x000C3EA8
		internal void \u0001(_ISignature \u0002, IScope5 \u0003)
		{
			foreach (_IVariable ivariable in \u0002.AllVariables)
			{
				VariableChecker.\u0003(\u0002, ivariable);
				VariableChecker.\u0002(\u0002, ivariable);
				VariableChecker.\u0001(\u0002, \u0003, ivariable);
				this.\u0001(\u0002, \u0003, ivariable);
				this.\u0001(ivariable, \u0002, \u0003);
				this.\u0001(\u0002, \u0003, ivariable);
			}
		}

		// Token: 0x0600331F RID: 13087 RVA: 0x000C5D20 File Offset: 0x000C3F20
		private void \u0001(_ISignature \u0002, IScope5 \u0003, _IVariable \u0004)
		{
			ICompiledType3 compiledType = (ICompiledType3)\u0004.CompiledType;
			_IImplicitReferenceType iimplicitReferenceType = compiledType as _IImplicitReferenceType;
			if (iimplicitReferenceType != null)
			{
				compiledType = (ICompiledType3)iimplicitReferenceType.BaseType;
			}
			ISignature signature;
			_ISignature isignature = global::\u0013.\u000E.\u0001(\u0004, \u0003, global::\u0013.\u000E.\u0001((_IType)compiledType), out signature) as _ISignature;
			if (isignature == null)
			{
				return;
			}
			ErrorVisitor errorVisitor = new ErrorVisitor();
			global::\u0014.\u0013.\u0001(\u0004, errorVisitor);
			global::\u001B.\u000F u000F = new global::\u001B.\u000F(\u0002, this.ComconNew, \u0004, isignature, signature as _ISignature);
			u000F.\u0001(\u0003);
			u000F.\u0001(errorVisitor);
			u000F.\u0001();
			TypeCheckerVisitor u = new TypeCheckerVisitor(\u0003, this.ComconNew);
			u000F.\u0001(\u0003, u, errorVisitor);
			if (!\u0083.\u0008.\u0001(\u0002, \u0004))
			{
				\u0002.AddMessages(errorVisitor.Messages);
			}
		}

		// Token: 0x06003320 RID: 13088 RVA: 0x000C5DD4 File Offset: 0x000C3FD4
		private void \u0001(_ISignature \u0002, IScope \u0003, _IVariable \u0004)
		{
			if ((\u0004.GetFlag(VarFlag.Input) || \u0004.GetFlag(VarFlag.Output)) && (\u0002.POUType == Operator.Function || \u0002.POUType == Operator.Method) && TypeTable.IsBlock(\u0004._Type.EffectiveType.Class) && !\u0084.\u0004.\u0001(\u0004.CompiledType, \u0003, this.ComconNew.Codegenerator) && \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE))
			{
				\u0002.\u0001(\u0004.SourcePosition, Severity.Error, MessageId.Err_NoValuePassingForCPPExternal, Array.Empty<object>());
			}
		}

		// Token: 0x06003321 RID: 13089 RVA: 0x000C5E60 File Offset: 0x000C4060
		internal void \u0002(_ISignature \u0002, IScope5 \u0003)
		{
			Dictionary<long, long> u = new Dictionary<long, long>();
			foreach (_IVariable ivariable in \u0002.AllConstants.Cast<_IVariable>())
			{
				global::\u0004.\u0011.\u0001(ivariable, \u0003, this.ComconNew, \u0002);
				global::\u0004.\u0011.\u0001(\u0002, \u0003, ivariable, u);
			}
		}

		// Token: 0x06003322 RID: 13090 RVA: 0x000C5EC8 File Offset: 0x000C40C8
		internal void \u0003(_ISignature \u0002, IScope5 \u0003)
		{
			foreach (_IVariable u in \u0002.AllVariables.Where(new Func<_IVariable, bool>(VariableChecker.<>c.<>9.\u0001)))
			{
				global::\u0004.\u0011.\u0001(u, \u0003, this.ComconNew, \u0002);
			}
		}

		// Token: 0x06003323 RID: 13091 RVA: 0x000C5F40 File Offset: 0x000C4140
		private static void \u0001(_ISignature \u0002, IScope5 \u0003, _IVariable \u0004)
		{
			VariableChecker.\u0001(\u0002, \u0004);
			VariableChecker.\u0002(\u0002, \u0003, \u0004);
		}

		// Token: 0x06003324 RID: 13092 RVA: 0x000C5F54 File Offset: 0x000C4154
		private static void \u0001(_ISignature \u0002, _IVariable \u0003)
		{
			if (!\u0003.HasFlag(VarFlag.ReplacedConstant | VarFlag.Constant))
			{
				return;
			}
			ICompiledType compiledType = \u0003.CompiledType;
			if (compiledType is _IArrayType)
			{
				compiledType = \u0084.\u0004.\u0001(compiledType);
			}
			if (compiledType is IGenericUserdefType)
			{
				\u0002.\u0001(\u0003.SourcePosition, Severity.Error, MessageId.Err_NoGenericInstanceInVarConst, Array.Empty<object>());
			}
		}

		// Token: 0x06003325 RID: 13093 RVA: 0x000C5FA4 File Offset: 0x000C41A4
		private static void \u0002(_ISignature \u0002, IScope5 \u0003, _IVariable \u0004)
		{
			if (\u0004.GetFlag(VarFlag.Generic) && !global::\u0006.\u0011.\u0002(\u0004.CompiledType, TypeTable.AnyInt, \u0003))
			{
				\u0002.\u0001(\u0004.SourcePosition, Severity.Error, MessageId.Err_GenericNoInteger, Array.Empty<object>());
			}
		}

		// Token: 0x06003326 RID: 13094 RVA: 0x000C5FE4 File Offset: 0x000C41E4
		private static void \u0002(_ISignature \u0002, _IVariable \u0003)
		{
			if (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE) && \u0003.HasAttribute(CompileAttributes.ATTRIBUTE_MONITORING))
			{
				string attributeValue = \u0003.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING);
				if (!string.IsNullOrEmpty(attributeValue) && attributeValue == CompileAttributes.ATTRIBUTEVALUE_CALL && \u0002.POUType != Operator.Method)
				{
					\u0002.\u0001(\u0003.SourcePosition, Severity.Warning, MessageId.Wrn_StruturedTypePropertyNotMonitorable, new object[]
					{
						\u0003.OrgName
					});
				}
			}
		}

		// Token: 0x06003327 RID: 13095 RVA: 0x000C6058 File Offset: 0x000C4258
		private static void \u0003(_ISignature \u0002, _IVariable \u0003)
		{
			if (\u0003.GetFlag(VarFlag.Inout) && \u0003._Type.EffectiveType.BaseType is _IReferenceType)
			{
				\u0002.\u0001(\u0003.SourcePosition, Severity.Error, MessageId.Err_ReferenceNotAllowedForVarInOut, Array.Empty<object>());
			}
		}

		// Token: 0x06003328 RID: 13096 RVA: 0x000C6094 File Offset: 0x000C4294
		private void \u0001(_IVariable \u0002, _ISignature \u0003, IScope5 \u0004)
		{
			_ISignature isignature = \u0004.FindSignatureLocal(\u0002.Name) as _ISignature;
			if (isignature != null && isignature.Id != \u0003.Id && !\u0003.GetFlagInternal(SignatureFlagInternal.Overloading))
			{
				Severity severity = Messages.\u0001(\u0002, MessageId.Wrn_Ambiguity);
				\u0003.AddMessage(\u0002._SourcePosition, severity, MessageId.Wrn_Ambiguity, new object[]
				{
					\u0002.Name
				});
				global::\u0019.\u0003.\u0001().SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(isignature.LibraryPath), isignature.ObjectGuid);
				isignature.\u0001(isignature.NameExpression.Position, Severity.Information, MessageId.Inf_RelatedPosition, Array.Empty<object>());
			}
		}

		// Token: 0x040009AF RID: 2479
		[CompilerGenerated]
		private readonly global::\u000E.\u001B \u0001;
	}
}
