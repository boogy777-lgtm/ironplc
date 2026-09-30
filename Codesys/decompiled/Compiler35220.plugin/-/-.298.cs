using System;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0011;
using \u0017;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0006
{
	// Token: 0x0200031A RID: 794
	internal sealed class \u0010
	{
		// Token: 0x170007ED RID: 2029
		// (get) Token: 0x06002FA1 RID: 12193 RVA: 0x000B3754 File Offset: 0x000B1954
		// (set) Token: 0x06002FA2 RID: 12194 RVA: 0x000B375C File Offset: 0x000B195C
		private bool CPPError { get; set; }

		// Token: 0x06002FA4 RID: 12196 RVA: 0x000B3770 File Offset: 0x000B1970
		internal void \u0001(_ISignature \u0002, IScope5 \u0003)
		{
			if (!\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE))
			{
				return;
			}
			this.\u0003(\u0002, \u0003);
			this.\u0002(\u0002, \u0003);
			global::\u0006.\u0010.\u0001(\u0002);
			global::\u0006.\u0010.\u0002(\u0002, \u0003);
			global::\u0006.\u0010.\u0001(\u0002, \u0003);
		}

		// Token: 0x06002FA5 RID: 12197 RVA: 0x000B37A4 File Offset: 0x000B19A4
		private static void \u0001(_ISignature \u0002, IScope5 \u0003)
		{
			if (\u0002.ParentSignatureId != Helper.InvalidId)
			{
				_ISignature isignature = \u0003[\u0002.ParentSignatureId] as _ISignature;
				if (isignature != null && !isignature.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE))
				{
					global::\u0006.\u0010.\u0001(\u0002, \u0003, isignature);
				}
			}
		}

		// Token: 0x06002FA6 RID: 12198 RVA: 0x000B37E8 File Offset: 0x000B19E8
		private static void \u0001(_ISignature \u0002, IScope5 \u0003, _ISignature \u0004)
		{
			bool flag = false;
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY))
			{
				global::\u0006.\u0010.\u0001 u = new global::\u0006.\u0010.\u0001();
				u.\u0001 = IdentifierConstants.GetPropertyName(\u0002.Name);
				ISignature signature = \u0003.FindSignatureLocal(IdentifierConstants.CreateSetterName(u.\u0001));
				ISignature signature2 = \u0003.FindSignatureLocal(IdentifierConstants.CreateGetterName(u.\u0001));
				if (signature == null || signature2 == null || \u0002 == signature)
				{
					_IVariable ivariable = \u0004.AllVariables.First(new Func<_IVariable, bool>(u.\u0001));
					\u0002.\u0001(ivariable.SourcePosition, Severity.Error, MessageId.Err_InconsistentUseOfCPPCompatibilityMissingParent, new object[]
					{
						\u0004.OrgName
					});
					flag = true;
				}
			}
			else
			{
				\u0002.AddMessage(Severity.Error, MessageId.Err_InconsistentUseOfCPPCompatibilityMissingParent, new object[]
				{
					\u0004.OrgName
				});
				flag = true;
			}
			if (flag)
			{
				_ISourcePosition isourcePosition = (_ISourcePosition)\u0004.NameExpression.Position;
				isourcePosition.SetObjectIdentification(-1, \u0004.ObjectGuid);
				\u0002.AddMessage(isourcePosition, Severity.Information, MessageId.Inf_RelatedPosition, Array.Empty<object>());
			}
		}

		// Token: 0x06002FA7 RID: 12199 RVA: 0x000B38E0 File Offset: 0x000B1AE0
		private static void \u0002(_ISignature \u0002, IScope5 \u0003)
		{
			foreach (_IVariable ivariable in \u0002.AllVariables)
			{
				if (ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY) && !ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE))
				{
					((_ISignature)(\u0003.FindSignatureLocal(IdentifierConstants.CreateSetterName(ivariable.Name)) ?? \u0003.FindSignatureLocal(IdentifierConstants.CreateGetterName(ivariable.Name)))).\u0001(ivariable.SourcePosition, Severity.Error, MessageId.Err_InconsistentUseOfCPPCompatibility, new object[]
					{
						ivariable.OrgName
					});
				}
			}
		}

		// Token: 0x06002FA8 RID: 12200 RVA: 0x000B398C File Offset: 0x000B1B8C
		private static void \u0001(_ISignature \u0002)
		{
			foreach (_ISignature isignature in \u0002.SubSignatures.OfType<_ISignature>())
			{
				if (!isignature.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE) && !isignature.Name.Contains("__") && isignature.Name != IdentifierConstants.MainSignatureName && isignature.Name != IdentifierConstants.InitMethodName && isignature.Name != IdentifierConstants.ExitMethodName && isignature.Name != IdentifierConstants.ReInitMethodName)
				{
					isignature.AddMessage(Severity.Error, MessageId.Err_InconsistentUseOfCPPCompatibility, new object[]
					{
						isignature.OrgName
					});
				}
			}
		}

		// Token: 0x06002FA9 RID: 12201 RVA: 0x000B3A60 File Offset: 0x000B1C60
		private void \u0002(_ISignature \u0002, IScope5 \u0003)
		{
			if (!this.CPPError)
			{
				LList<_ISignature> llist = new LList<_ISignature>();
				global::\u0017.\u0001.\u0001(\u0002, \u0003, llist);
				foreach (_ISignature isignature in llist)
				{
					if (!isignature.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE))
					{
						isignature.AddMessage(Severity.Error, MessageId.Err_InconsistentInheritanceOfCPPCompatibility, new object[]
						{
							\u0002.OrgName
						});
					}
				}
			}
		}

		// Token: 0x06002FAA RID: 12202 RVA: 0x000B3AE0 File Offset: 0x000B1CE0
		private void \u0003(_ISignature \u0002, IScope5 \u0003)
		{
			if (\u0002.BaseSignatureId != Helper.InvalidId || \u0002.InterfaceIds.Length != 0)
			{
				LList<_ISignature> llist = new LList<_ISignature>();
				global::\u0017.\u0001.\u0003(\u0002, \u0003, llist);
				foreach (_ISignature isignature in llist)
				{
					if (!isignature.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE))
					{
						\u0002.AddMessage(Severity.Error, MessageId.Err_InconsistentInheritanceOfCPPCompatibility, new object[]
						{
							isignature.OrgName
						});
						this.CPPError = true;
					}
				}
			}
		}

		// Token: 0x04000919 RID: 2329
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x0200031B RID: 795
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x06002FAC RID: 12204 RVA: 0x000B3B7C File Offset: 0x000B1D7C
			internal bool \u0001(_IVariable \u0002)
			{
				return \u0002.Name == this.\u0001;
			}

			// Token: 0x0400091A RID: 2330
			public string \u0001;
		}
	}
}
