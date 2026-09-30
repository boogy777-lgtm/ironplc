using System;
using System.IO;
using System.Runtime.CompilerServices;
using \u0012;
using \u0016;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u0002
{
	// Token: 0x020003E8 RID: 1000
	internal class \u0013 : \u0018
	{
		// Token: 0x1700090F RID: 2319
		// (get) Token: 0x0600378D RID: 14221 RVA: 0x000E45C8 File Offset: 0x000E27C8
		// (set) Token: 0x0600378E RID: 14222 RVA: 0x000E45D0 File Offset: 0x000E27D0
		private _ICompileContext CompileContext { get; set; }

		// Token: 0x17000910 RID: 2320
		// (get) Token: 0x0600378F RID: 14223 RVA: 0x000E45DC File Offset: 0x000E27DC
		// (set) Token: 0x06003790 RID: 14224 RVA: 0x000E45E4 File Offset: 0x000E27E4
		private global::\u0012.\u0016 CodeRelocator { get; set; }

		// Token: 0x17000911 RID: 2321
		// (get) Token: 0x06003791 RID: 14225 RVA: 0x000E45F0 File Offset: 0x000E27F0
		internal global::\u0016.\u0016 BootSorter
		{
			get
			{
				return this.TableRelocator.BootSorter;
			}
		}

		// Token: 0x17000912 RID: 2322
		// (get) Token: 0x06003792 RID: 14226 RVA: 0x000E4600 File Offset: 0x000E2800
		internal global::\u0016.\u0016 Sorter
		{
			get
			{
				return this.TableRelocator.Sorter;
			}
		}

		// Token: 0x17000913 RID: 2323
		// (get) Token: 0x06003793 RID: 14227 RVA: 0x000E4610 File Offset: 0x000E2810
		private ICodegenerator Codegenerator
		{
			get
			{
				return this.CompileContext.Codegenerator;
			}
		}

		// Token: 0x17000914 RID: 2324
		// (get) Token: 0x06003794 RID: 14228 RVA: 0x000E4620 File Offset: 0x000E2820
		protected \u0019 DirectRelocator { get; }

		// Token: 0x17000915 RID: 2325
		// (get) Token: 0x06003795 RID: 14229 RVA: 0x000E4628 File Offset: 0x000E2828
		protected global::\u0002.\u0012 TableRelocator { get; }

		// Token: 0x06003796 RID: 14230 RVA: 0x000E4630 File Offset: 0x000E2830
		internal \u0013(bool \u008E\u0004, _ICompileContext \u0001\u0002)
		{
			this.CompileContext = \u0001\u0002;
			bool motorolaByteOrder = this.Codegenerator.MotorolaByteOrder;
			this.CodeRelocator = new global::\u0012.\u0016(motorolaByteOrder, this.Codegenerator);
			this.DirectRelocator = \u0019.\u0001(\u0001\u0002);
			this.TableRelocator = new global::\u0002.\u0012(\u008E\u0004, \u0001\u0002);
		}

		// Token: 0x06003797 RID: 14231 RVA: 0x000E4684 File Offset: 0x000E2884
		public virtual void \u0001(_ICompiledPOU \u0002, Stream \u0003, bool \u0004, int \u0005, IRelocation \u0006)
		{
			\u0005 = this.\u0001(\u0003, \u0005, \u0006);
			if (!this.DirectRelocator.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006))
			{
				this.TableRelocator.\u0001(\u0002, \u0003, \u0005, \u0006);
			}
		}

		// Token: 0x06003798 RID: 14232 RVA: 0x000E46B8 File Offset: 0x000E28B8
		public virtual void \u0001(ICompiledCode4 \u0002)
		{
			if (\u0002 != null)
			{
				\u0002.SetFlag(CompiledCodeFlags.Relocated, true);
			}
		}

		// Token: 0x06003799 RID: 14233 RVA: 0x000E46C8 File Offset: 0x000E28C8
		protected int \u0001(Stream \u0002, int \u0003, IRelocation \u0004)
		{
			if (\u0004 is IDirectCallRelocation && (\u0004 as IDirectCallRelocation).SignatureToCallId != Helper.InvalidId)
			{
				int signatureToCallId = (\u0004 as IDirectCallRelocation).SignatureToCallId;
				_ICompiledPOU icompiledPOU = this.CompileContext._GetCompiledPOUById(signatureToCallId);
				Debug.\u0001(icompiledPOU != null);
				\u0003 = (int)icompiledPOU.CompiledCode.Location.Area;
				int offset = icompiledPOU.CompiledCode.Location.Offset;
				this.CodeRelocator.\u0001(\u0002, \u0004, offset);
			}
			return \u0003;
		}

		// Token: 0x0600379A RID: 14234 RVA: 0x000E4744 File Offset: 0x000E2944
		internal void \u0001()
		{
			this.TableRelocator.\u0001();
		}

		// Token: 0x04000AEB RID: 2795
		[CompilerGenerated]
		private _ICompileContext \u0001;

		// Token: 0x04000AEC RID: 2796
		[CompilerGenerated]
		private global::\u0012.\u0016 \u0001;

		// Token: 0x04000AED RID: 2797
		[CompilerGenerated]
		private readonly \u0019 \u0001;

		// Token: 0x04000AEE RID: 2798
		[CompilerGenerated]
		private readonly global::\u0002.\u0012 \u0001;
	}
}
