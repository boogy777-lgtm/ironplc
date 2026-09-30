using System;
using System.Runtime.CompilerServices;
using \u000F;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001A
{
	// Token: 0x02000315 RID: 789
	internal sealed class \u0012 : IParseTreeProvider
	{
		// Token: 0x170007DE RID: 2014
		// (get) Token: 0x06002F66 RID: 12134 RVA: 0x000B2670 File Offset: 0x000B0870
		internal IParseTreeProvider Original { get; }

		// Token: 0x170007DF RID: 2015
		// (get) Token: 0x06002F67 RID: 12135 RVA: 0x000B2678 File Offset: 0x000B0878
		private \u0015 Loader { get; }

		// Token: 0x170007E0 RID: 2016
		// (get) Token: 0x06002F68 RID: 12136 RVA: 0x000B2680 File Offset: 0x000B0880
		// (set) Token: 0x06002F69 RID: 12137 RVA: 0x000B2688 File Offset: 0x000B0888
		internal ICompiledPOUWithParseTreeProvider CompiledPOU { get; set; }

		// Token: 0x170007E1 RID: 2017
		// (get) Token: 0x06002F6A RID: 12138 RVA: 0x000B2694 File Offset: 0x000B0894
		// (set) Token: 0x06002F6B RID: 12139 RVA: 0x000B269C File Offset: 0x000B089C
		internal _IStatement TemporaryRedTree { get; set; }

		// Token: 0x170007E2 RID: 2018
		// (get) Token: 0x06002F6C RID: 12140 RVA: 0x000B26A8 File Offset: 0x000B08A8
		// (set) Token: 0x06002F6D RID: 12141 RVA: 0x000B26B0 File Offset: 0x000B08B0
		internal bool Done { get; set; }

		// Token: 0x06002F6E RID: 12142 RVA: 0x000B26BC File Offset: 0x000B08BC
		private \u0012(ICompiledPOUWithParseTreeProvider \u0012\u0002, \u0015 \u007F\u0004)
		{
			this.Original = \u0012\u0002.ParseTreeProvider;
			this.Loader = \u007F\u0004;
			this.CompiledPOU = \u0012\u0002;
			this.Done = false;
		}

		// Token: 0x06002F6F RID: 12143 RVA: 0x000B26E8 File Offset: 0x000B08E8
		internal static IParseTreeProvider \u0001(ICompiledPOUWithParseTreeProvider \u0002, \u0015 \u0003)
		{
			IParseTreeProvider parseTreeProvider = \u0002.ParseTreeProvider;
			if (parseTreeProvider is \u0012)
			{
				return parseTreeProvider;
			}
			return new \u0012(\u0002, \u0003);
		}

		// Token: 0x170007E3 RID: 2019
		// (get) Token: 0x06002F70 RID: 12144 RVA: 0x000B2710 File Offset: 0x000B0910
		// (set) Token: 0x06002F71 RID: 12145 RVA: 0x000B2720 File Offset: 0x000B0920
		public ICompactedParseTreeInformation CompactedParseTreeInformation
		{
			get
			{
				return this.Original.CompactedParseTreeInformation;
			}
			set
			{
				this.Original.CompactedParseTreeInformation = value;
			}
		}

		// Token: 0x170007E4 RID: 2020
		// (get) Token: 0x06002F72 RID: 12146 RVA: 0x000B2730 File Offset: 0x000B0930
		// (set) Token: 0x06002F73 RID: 12147 RVA: 0x000B2738 File Offset: 0x000B0938
		private int RedTreeCount { get; set; }

		// Token: 0x06002F74 RID: 12148 RVA: 0x000B2744 File Offset: 0x000B0944
		public _IStatement \u0002()
		{
			int num = this.RedTreeCount;
			this.RedTreeCount = num + 1;
			if (this.TemporaryRedTree != null)
			{
				return this.TemporaryRedTree;
			}
			if (!this.Done)
			{
				this.Loader.\u0002(this.CompiledPOU);
			}
			if (this.TemporaryRedTree != null)
			{
				_IStatement result = this.TemporaryRedTree;
				this.TemporaryRedTree = null;
				return result;
			}
			return this.Original.CreateTemporaryRedTree();
		}

		// Token: 0x06002F75 RID: 12149 RVA: 0x000B27AC File Offset: 0x000B09AC
		public void \u0001()
		{
			this.TemporaryRedTree = this.Original.CreateTemporaryRedTree();
		}

		// Token: 0x06002F76 RID: 12150 RVA: 0x000B27C0 File Offset: 0x000B09C0
		public _IStatement \u0003()
		{
			if (!this.Done)
			{
				this.Loader.\u0002(this.CompiledPOU);
			}
			if (this.TemporaryRedTree != null)
			{
				return this.TemporaryRedTree;
			}
			return this.Original.GetParseTree();
		}

		// Token: 0x06002F77 RID: 12151 RVA: 0x000B27F8 File Offset: 0x000B09F8
		public void \u0001(_ICompiledPOU \u0002)
		{
			ICompiledPOUWithParseTreeProvider compiledPOUWithParseTreeProvider = \u0002 as ICompiledPOUWithParseTreeProvider;
			_ICompiledPOU icompiledPOU = this.CompiledPOU as _ICompiledPOU;
			if (this.CompiledPOU.ParseTreeRaw is _IEmptyStatement)
			{
				compiledPOUWithParseTreeProvider.ParseTreeProvider.SetParseTreeDirectly(this.CompiledPOU.ParseTreeRaw);
			}
			else if (this.CompiledPOU.ParseTreeRaw == null)
			{
				compiledPOUWithParseTreeProvider.ParseTreeProvider.SetParseTreeDirectly(icompiledPOU.GetParseTree());
			}
			else
			{
				compiledPOUWithParseTreeProvider.ParseTreeProvider.SetParseTreeDirectly(this.CompiledPOU.ParseTreeRaw);
			}
			(compiledPOUWithParseTreeProvider as ICompiledPOUWithCompactedParseTree).CompactedParseTreeInformation = this.CompactedParseTreeInformation;
		}

		// Token: 0x06002F78 RID: 12152 RVA: 0x000B288C File Offset: 0x000B0A8C
		public void \u0002(_IStatement \u0002)
		{
			this.Original.SetParseTreeWithSideEffects(\u0002);
		}

		// Token: 0x06002F79 RID: 12153 RVA: 0x000B289C File Offset: 0x000B0A9C
		public void \u0003(_IStatement \u0002)
		{
			this.Original.SetParseTreeDirectly(\u0002);
		}

		// Token: 0x06002F7A RID: 12154 RVA: 0x000B28AC File Offset: 0x000B0AAC
		public void \u0002()
		{
			this.Original.DuplicateParseTreeForCompilation();
		}

		// Token: 0x06002F7B RID: 12155 RVA: 0x000B28BC File Offset: 0x000B0ABC
		public _IStatement \u0001(bool \u0002)
		{
			return this.Original.GetParseTreeForSerialization(\u0002);
		}

		// Token: 0x04000906 RID: 2310
		[CompilerGenerated]
		private readonly IParseTreeProvider \u0001;

		// Token: 0x04000907 RID: 2311
		[CompilerGenerated]
		private readonly \u0015 \u0001;

		// Token: 0x04000908 RID: 2312
		[CompilerGenerated]
		private ICompiledPOUWithParseTreeProvider \u0001;

		// Token: 0x04000909 RID: 2313
		[CompilerGenerated]
		private _IStatement \u0001;

		// Token: 0x0400090A RID: 2314
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x0400090B RID: 2315
		[CompilerGenerated]
		private int \u0001;
	}
}
