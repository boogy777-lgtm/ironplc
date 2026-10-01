using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200007C RID: 124
	internal class IdentifierInfo : IIdentifierInfo2, IIdentifierInfo
	{
		// Token: 0x0600080E RID: 2062 RVA: 0x00013FFF File Offset: 0x00012FFF
		public IdentifierInfo(string stName, string stComment, IdentifierInfoFlag flags, IType type)
		{
			this.Flags = flags;
			this.Name = stName;
			this.m_stComment = stComment;
			this.Type = type;
		}

		// Token: 0x0600080F RID: 2063 RVA: 0x00014024 File Offset: 0x00013024
		public IdentifierInfo(IVariable var, ISignature containingSign, string stName, string stComment, IdentifierInfoFlag flags, IType type)
		{
			this.Flags = flags;
			this.Name = stName;
			this.m_stComment = stComment;
			this.Type = type;
			this.Variable = var;
			this.m_containingSign = containingSign;
			this.Signature = this.m_containingSign;
		}

		// Token: 0x06000810 RID: 2064 RVA: 0x00014070 File Offset: 0x00013070
		public IdentifierInfo(ISignature sign, string stName, string stComment, IdentifierInfoFlag flags, IType type)
		{
			this.Flags = flags;
			this.Name = stName;
			this.m_stComment = stComment;
			this.Type = type;
			this.Signature = sign;
		}

		// Token: 0x170001C8 RID: 456
		// (get) Token: 0x06000811 RID: 2065 RVA: 0x0001409D File Offset: 0x0001309D
		// (set) Token: 0x06000812 RID: 2066 RVA: 0x000140A5 File Offset: 0x000130A5
		public string Name { get; set; }

		// Token: 0x170001C9 RID: 457
		// (get) Token: 0x06000813 RID: 2067 RVA: 0x000140AE File Offset: 0x000130AE
		// (set) Token: 0x06000814 RID: 2068 RVA: 0x000140C4 File Offset: 0x000130C4
		public string Comment
		{
			get
			{
				if (this.m_stComment == null)
				{
					return string.Empty;
				}
				return this.m_stComment;
			}
			set
			{
				this.m_stComment = value;
			}
		}

		// Token: 0x170001CA RID: 458
		// (get) Token: 0x06000815 RID: 2069 RVA: 0x000140CD File Offset: 0x000130CD
		// (set) Token: 0x06000816 RID: 2070 RVA: 0x000140D5 File Offset: 0x000130D5
		public IdentifierInfoFlag Flags { get; set; }

		// Token: 0x170001CB RID: 459
		// (get) Token: 0x06000817 RID: 2071 RVA: 0x000140DE File Offset: 0x000130DE
		// (set) Token: 0x06000818 RID: 2072 RVA: 0x000140E6 File Offset: 0x000130E6
		public IType Type { get; set; }

		// Token: 0x170001CC RID: 460
		// (get) Token: 0x06000819 RID: 2073 RVA: 0x000140EF File Offset: 0x000130EF
		// (set) Token: 0x0600081A RID: 2074 RVA: 0x000140F7 File Offset: 0x000130F7
		public IVariable Variable { get; set; }

		// Token: 0x170001CD RID: 461
		// (get) Token: 0x0600081B RID: 2075 RVA: 0x00014100 File Offset: 0x00013100
		// (set) Token: 0x0600081C RID: 2076 RVA: 0x00014108 File Offset: 0x00013108
		public ISignature Signature { get; set; }

		// Token: 0x170001CE RID: 462
		// (get) Token: 0x0600081D RID: 2077 RVA: 0x00005F0F File Offset: 0x00004F0F
		public IScope Scope
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170001CF RID: 463
		// (get) Token: 0x0600081E RID: 2078 RVA: 0x00014111 File Offset: 0x00013111
		public ISignature ContainingSignature
		{
			get
			{
				return this.m_containingSign;
			}
		}

		// Token: 0x0400010F RID: 271
		private string m_stComment;

		// Token: 0x04000110 RID: 272
		private readonly ISignature m_containingSign;
	}
}
