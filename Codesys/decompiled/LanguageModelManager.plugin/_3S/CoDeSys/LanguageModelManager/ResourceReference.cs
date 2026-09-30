using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200006F RID: 111
	[TypeGuid("{4ae73ab4-5bc7-4838-ba55-e56bd0e9c12a}")]
	[StorageVersion("3.3.0.0")]
	public class ResourceReference : ItemReference, _IResourceReference, _IItemReference, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression
	{
		// Token: 0x06000722 RID: 1826 RVA: 0x0000E359 File Offset: 0x0000D359
		public ResourceReference()
		{
		}

		// Token: 0x06000723 RID: 1827 RVA: 0x0000E361 File Offset: 0x0000D361
		public ResourceReference(IToken token) : base(token)
		{
		}

		// Token: 0x06000724 RID: 1828 RVA: 0x00012740 File Offset: 0x00011740
		public ResourceReference(IToken token, string stResourceName) : base(token)
		{
			this.m_stResourceName = stResourceName;
		}

		// Token: 0x17000192 RID: 402
		// (get) Token: 0x06000725 RID: 1829 RVA: 0x00012750 File Offset: 0x00011750
		// (set) Token: 0x06000726 RID: 1830 RVA: 0x00012758 File Offset: 0x00011758
		public string ResourceName
		{
			get
			{
				return this.m_stResourceName;
			}
			set
			{
				this.m_stResourceName = value;
			}
		}

		// Token: 0x06000727 RID: 1831 RVA: 0x00012761 File Offset: 0x00011761
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000728 RID: 1832 RVA: 0x0001276A File Offset: 0x0001176A
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000729 RID: 1833 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x0600072A RID: 1834 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x0600072B RID: 1835 RVA: 0x00012774 File Offset: 0x00011774
		public override _IExprement Duplicate()
		{
			ResourceReference resourceReference = new ResourceReference();
			resourceReference.m_stResourceName = this.m_stResourceName;
			this.DuplicateCommon(resourceReference);
			return resourceReference;
		}

		// Token: 0x040000F3 RID: 243
		[DefaultSerialization("ResourceName")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stResourceName;
	}
}
