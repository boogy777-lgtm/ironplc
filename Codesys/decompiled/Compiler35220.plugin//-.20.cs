using System;
using System.IO;
using System.Runtime.CompilerServices;
using \u0011;
using \u0012;
using \u0015;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0080;

namespace \u0081
{
	// Token: 0x020003E2 RID: 994
	internal sealed class \u0019
	{
		// Token: 0x17000904 RID: 2308
		// (get) Token: 0x06003769 RID: 14185 RVA: 0x000E41F0 File Offset: 0x000E23F0
		private _ICompileContext CompileContext { get; }

		// Token: 0x17000905 RID: 2309
		// (get) Token: 0x0600376A RID: 14186 RVA: 0x000E41F8 File Offset: 0x000E23F8
		private global::\u0012.\u0016 CodeRelocator { get; }

		// Token: 0x17000906 RID: 2310
		// (get) Token: 0x0600376B RID: 14187 RVA: 0x000E4200 File Offset: 0x000E2400
		// (set) Token: 0x0600376C RID: 14188 RVA: 0x000E4208 File Offset: 0x000E2408
		public \u001B AreaStartAddressProvider { get; set; }

		// Token: 0x0600376D RID: 14189 RVA: 0x000E4214 File Offset: 0x000E2414
		internal static \u0019 \u0001(_ICompileContext \u0002)
		{
			return new \u0019(\u0002)
			{
				AreaStartAddressProvider = new global::\u0011.\u0016()
			};
		}

		// Token: 0x0600376E RID: 14190 RVA: 0x000E4228 File Offset: 0x000E2428
		internal static \u0019 \u0001(_ICompileContext \u0002, int[] \u0003)
		{
			return new \u0019(\u0002)
			{
				AreaStartAddressProvider = new global::\u0015.\u000E(\u0003)
			};
		}

		// Token: 0x0600376F RID: 14191 RVA: 0x000E423C File Offset: 0x000E243C
		private \u0019(_ICompileContext \u0001\u0002)
		{
			ICodegenerator codegenerator = \u0001\u0002.Codegenerator;
			this.CodeRelocator = new global::\u0012.\u0016(codegenerator.MotorolaByteOrder, codegenerator);
			this.CompileContext = \u0001\u0002;
		}

		// Token: 0x06003770 RID: 14192 RVA: 0x000E4270 File Offset: 0x000E2470
		private static _IArea \u0001(int \u0002, _ICompileContext \u0003)
		{
			_IArea areaByIndex;
			for (;;)
			{
				areaByIndex = \u0003.DataManager.GetAreaByIndex(\u0002);
				if (areaByIndex != null)
				{
					break;
				}
				if (\u0003.ParentContext == null)
				{
					goto IL_24;
				}
				\u0003 = \u0003.ParentContext;
			}
			return areaByIndex;
			IL_24:
			return null;
		}

		// Token: 0x06003771 RID: 14193 RVA: 0x000E42A4 File Offset: 0x000E24A4
		internal bool \u0001(_ICompiledPOU \u0002, Stream \u0003, bool \u0004, int \u0005, IRelocation \u0006)
		{
			ICompiledCode4 compiledCode = \u0002.CompiledCode as ICompiledCode4;
			bool flag = compiledCode != null && compiledCode.GetFlag(CompiledCodeFlags.Relocated);
			_IArea iarea = \u0019.\u0001(\u0005, this.CompileContext);
			Debug.\u0001(iarea != null);
			if (this.\u0001(iarea))
			{
				if (!flag)
				{
					this.CodeRelocator.\u0001(\u0004, \u0003, \u0006.Offset, this.AreaStartAddressProvider.\u0001(iarea));
				}
				return true;
			}
			return false;
		}

		// Token: 0x06003772 RID: 14194 RVA: 0x000E4314 File Offset: 0x000E2514
		internal bool \u0001(int \u0002)
		{
			_IArea u = \u0019.\u0001(\u0002, this.CompileContext);
			return this.\u0001(u);
		}

		// Token: 0x06003773 RID: 14195 RVA: 0x000E4338 File Offset: 0x000E2538
		private bool \u0001(_IArea \u0002)
		{
			return this.AreaStartAddressProvider.\u0001(\u0002) != 0 && \u0002.GetAreaFlag(AreaFlags.Fixed);
		}

		// Token: 0x04000AE1 RID: 2785
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x04000AE2 RID: 2786
		[CompilerGenerated]
		private readonly global::\u0012.\u0016 \u0001;

		// Token: 0x04000AE3 RID: 2787
		[CompilerGenerated]
		private \u001B \u0001;
	}
}
