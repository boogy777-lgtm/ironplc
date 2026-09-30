using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000BC RID: 188
	internal readonly struct InstancePathInformation : IEquatable<InstancePathInformation>
	{
		// Token: 0x06000E68 RID: 3688 RVA: 0x0002751C File Offset: 0x0002571C
		internal InstancePathInformation(string path, _IVariable vardecl, _ISignature signDecl)
		{
			this.Path = path;
			this.DeclaredVariable = vardecl;
			this.SignDeclarationLocation = signDecl;
		}

		// Token: 0x1700043D RID: 1085
		// (get) Token: 0x06000E69 RID: 3689 RVA: 0x00027534 File Offset: 0x00025734
		public string Path { get; }

		// Token: 0x1700043E RID: 1086
		// (get) Token: 0x06000E6A RID: 3690 RVA: 0x0002753C File Offset: 0x0002573C
		public _IVariable DeclaredVariable { get; }

		// Token: 0x1700043F RID: 1087
		// (get) Token: 0x06000E6B RID: 3691 RVA: 0x00027544 File Offset: 0x00025744
		public _ISignature SignDeclarationLocation { get; }

		// Token: 0x06000E6C RID: 3692 RVA: 0x0002754C File Offset: 0x0002574C
		public bool \u0001(object \u0002)
		{
			if (\u0002 is InstancePathInformation)
			{
				InstancePathInformation u = (InstancePathInformation)\u0002;
				return this.\u0001(u);
			}
			return false;
		}

		// Token: 0x06000E6D RID: 3693 RVA: 0x00027574 File Offset: 0x00025774
		public bool \u0001(InstancePathInformation \u0002)
		{
			return \u0002.Path == this.Path;
		}

		// Token: 0x06000E6E RID: 3694 RVA: 0x00027588 File Offset: 0x00025788
		public int \u0001()
		{
			return this.Path.GetHashCode();
		}

		// Token: 0x04000271 RID: 625
		[CompilerGenerated]
		private readonly string \u0001;

		// Token: 0x04000272 RID: 626
		[CompilerGenerated]
		private readonly _IVariable \u0001;

		// Token: 0x04000273 RID: 627
		[CompilerGenerated]
		private readonly _ISignature \u0001;
	}
}
