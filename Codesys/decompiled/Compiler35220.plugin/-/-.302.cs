using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001F
{
	// Token: 0x02000328 RID: 808
	internal sealed class \u0011
	{
		// Token: 0x0600302C RID: 12332 RVA: 0x000B7130 File Offset: 0x000B5330
		internal void \u0001(_ISignature \u0002, IScope5 \u0003)
		{
			\u0011.\u0001(\u0002);
			\u0011.\u0001(\u0002, \u0003);
		}

		// Token: 0x0600302D RID: 12333 RVA: 0x000B7140 File Offset: 0x000B5340
		private static void \u0001(_ISignature \u0002, IScope5 \u0003)
		{
			if (\u0002.POUType == Operator.FunctionBlock)
			{
				foreach (_IVariable ivariable in \u0002.AllVariables)
				{
					if (ivariable.CompiledType != null && !ivariable.GetFlag(VarFlag.Inout) && \u0011.\u0001(\u0002, \u0003, ivariable))
					{
						break;
					}
				}
			}
		}

		// Token: 0x0600302E RID: 12334 RVA: 0x000B71B0 File Offset: 0x000B53B0
		private static bool \u0001(_ISignature \u0002, IScope5 \u0003, _IVariable \u0004)
		{
			if (\u0004.CompiledType.Class == TypeClass.Userdef || (\u0004.CompiledType.Class == TypeClass.Array && \u0004.CompiledType.BaseType.Class == TypeClass.Userdef))
			{
				_IUserdefType iuserdefType;
				if (\u0004.CompiledType.Class == TypeClass.Array)
				{
					iuserdefType = (\u0004.CompiledType.BaseType as _IUserdefType);
				}
				else
				{
					iuserdefType = (\u0004.CompiledType as _IUserdefType);
				}
				ISignature signature = \u0003[iuserdefType.SignatureId];
				if (signature != null && !signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion) && (signature.HasAttribute(CompileAttributes.ATTRIBUTE_NOASSIGN) || signature.HasAttribute(CompileAttributes.ATTRIBUTE_NOASSIGN_WARNING)) && !\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_NOASSIGN) && !\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_NOASSIGN_WARNING))
				{
					\u0002.AddMessage(\u0004._SourcePosition, Severity.Warning, MessageId.Wrn_MissingAttributeNoAssign, new object[]
					{
						\u0002.OrgName,
						\u0004.OrgName
					});
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600302F RID: 12335 RVA: 0x000B72A4 File Offset: 0x000B54A4
		private static void \u0001(_ISignature \u0002)
		{
			if ((\u0002.POUType == Operator.Function || \u0002.POUType == Operator.Method) && \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_PACK_MODE))
			{
				int num = 8;
				\u0002.GetAttributeIntValue(CompileAttributes.ATTRIBUTE_PACK_MODE, ref num);
				if (num % 4 != 0)
				{
					\u0002.AddMessage(Severity.Error, MessageId.Err_AttributeNotAllowedFor, new object[]
					{
						CompileAttributes.ATTRIBUTE_PACK_MODE,
						\u0002.POUType.ToString().ToUpperInvariant()
					});
				}
			}
		}
	}
}
