using System;
using System.Linq;
using \u0011;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0080
{
	// Token: 0x02000327 RID: 807
	internal sealed class \u0015
	{
		// Token: 0x06003025 RID: 12325 RVA: 0x000B6E7C File Offset: 0x000B507C
		internal void \u0001(_ISignature \u0002, IScope5 \u0003)
		{
			if (\u0002.POUType != Operator.Method)
			{
				return;
			}
			ISignature u = \u0003[\u0002.ParentSignatureId] as _ISignature;
			\u0015.\u0003(\u0002, u);
			\u0015.\u0002(\u0002, u);
			\u0015.\u0001(\u0002, u);
		}

		// Token: 0x06003026 RID: 12326 RVA: 0x000B6EBC File Offset: 0x000B50BC
		private static void \u0001(_ISignature \u0002, ISignature \u0003)
		{
			if (\u0003 != null && (\u0002.Name == IdentifierConstants.InitMethodName || \u0002.Name == IdentifierConstants.ExitMethodName || \u0002.Name == IdentifierConstants.ReInitMethodName) && (\u0002.GetFlag(SignatureFlag.Private) || \u0002.GetFlag(SignatureFlag.Protected) || \u0002.GetFlag(SignatureFlag.Internal)))
			{
				\u0002.AddMessage(Severity.Error, MessageId.Err_ImplicitMethodImplementationNotPublic, new object[]
				{
					\u0003.OrgName
				});
			}
		}

		// Token: 0x06003027 RID: 12327 RVA: 0x000B6F54 File Offset: 0x000B5154
		private static void \u0002(_ISignature \u0002, ISignature \u0003)
		{
			if ((\u0003 == null || \u0003.POUType != Operator.FunctionBlock) && \u0002.InstanceLocals.Length != 0)
			{
				\u0002.AddMessage(Severity.Error, MessageId.Err_VarInstOnlyInMethods, Array.Empty<object>());
			}
		}

		// Token: 0x06003028 RID: 12328 RVA: 0x000B6F88 File Offset: 0x000B5188
		private static bool \u0001(_ISignature \u0002, _IVariable \u0003)
		{
			if (\u0003.IsProperty)
			{
				return true;
			}
			if (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_HIDE))
			{
				return true;
			}
			if (\u0003.HasAttribute("ignore_in_interface"))
			{
				return true;
			}
			if (!\u0003.HasFlag(VarFlag.Input | VarFlag.Output | VarFlag.Inout))
			{
				\u0002.\u0001(\u0003.SourcePosition, Severity.Error, MessageId.Err_WrongVarInInterfaceMethod, Array.Empty<object>());
				return false;
			}
			return true;
		}

		// Token: 0x06003029 RID: 12329 RVA: 0x000B6FE4 File Offset: 0x000B51E4
		private static void \u0003(_ISignature \u0002, ISignature \u0003)
		{
			if (\u0003 != null && \u0003.POUType == Operator.Interface)
			{
				foreach (_IVariable u in \u0002.AllVariables)
				{
					if (!\u0015.\u0001(\u0002, u))
					{
						break;
					}
				}
				if (\u0002.GetFlag(SignatureFlag.Private) || \u0002.GetFlag(SignatureFlag.Protected))
				{
					\u0002.AddMessage(Severity.Error, MessageId.Err_PrivateOnMethodsOnly, Array.Empty<object>());
				}
			}
		}

		// Token: 0x0600302A RID: 12330 RVA: 0x000B7078 File Offset: 0x000B5278
		internal void \u0002(_ISignature \u0002, IScope5 \u0003)
		{
			if (\u0002.POUType == Operator.FunctionBlock)
			{
				foreach (_IVariable ivariable in \u0002.Inputs.OfType<_IVariable>())
				{
					if (ivariable.IsProperty)
					{
						IScope5 scope = \u0003.CreateLocalScope(\u0002);
						ISignature signature = scope.FindSignatureLocal(IdentifierConstants.CreateGetterName(ivariable.VersionedName));
						ISignature signature2 = scope.FindSignatureLocal(IdentifierConstants.CreateSetterName(ivariable.VersionedName));
						if (signature == null && signature2 == null)
						{
							\u0002.AddMessage(Severity.Error, MessageId.Err_OneAccessorRequired, new object[]
							{
								ivariable.VersionedName
							});
						}
					}
				}
			}
		}
	}
}
