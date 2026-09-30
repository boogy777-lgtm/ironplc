using System;
using System.IO;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u001A
{
	// Token: 0x020003E6 RID: 998
	internal sealed class \u0016 : \u0018
	{
		// Token: 0x1700090D RID: 2317
		// (get) Token: 0x06003784 RID: 14212 RVA: 0x000E44D0 File Offset: 0x000E26D0
		private _ICompileContext CompileContext { get; }

		// Token: 0x1700090E RID: 2318
		// (get) Token: 0x06003785 RID: 14213 RVA: 0x000E44D8 File Offset: 0x000E26D8
		private \u0019 DirectRelocator { get; }

		// Token: 0x06003786 RID: 14214 RVA: 0x000E44E0 File Offset: 0x000E26E0
		public \u0016(_ICompileContext \u0001\u0002, int[] \u008D\u0005)
		{
			this.CompileContext = \u0001\u0002;
			this.DirectRelocator = \u0019.\u0001(this.CompileContext, \u008D\u0005);
		}

		// Token: 0x06003787 RID: 14215 RVA: 0x000E4504 File Offset: 0x000E2704
		public void \u0001(_ICompiledPOU \u0002, Stream \u0003, bool \u0004, int \u0005, IRelocation \u0006)
		{
			\u0005 = this.\u0001(\u0005, \u0006);
			this.DirectRelocator.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x06003788 RID: 14216 RVA: 0x000E4528 File Offset: 0x000E2728
		public void \u0001(ICompiledCode4 \u0002)
		{
		}

		// Token: 0x06003789 RID: 14217 RVA: 0x000E452C File Offset: 0x000E272C
		private int \u0001(int \u0002, IRelocation \u0003)
		{
			if (\u0003 is IDirectCallRelocation && (\u0003 as IDirectCallRelocation).SignatureToCallId != Helper.InvalidId)
			{
				int signatureToCallId = (\u0003 as IDirectCallRelocation).SignatureToCallId;
				_ICompiledPOU icompiledPOU = this.CompileContext._GetCompiledPOUById(signatureToCallId);
				Debug.\u0001(icompiledPOU != null);
				\u0002 = (int)icompiledPOU.CompiledCode.Location.Area;
			}
			return \u0002;
		}

		// Token: 0x04000AE9 RID: 2793
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x04000AEA RID: 2794
		[CompilerGenerated]
		private readonly \u0019 \u0001;
	}
}
