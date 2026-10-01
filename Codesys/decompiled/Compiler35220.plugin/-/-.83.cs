using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0013
{
	// Token: 0x02000110 RID: 272
	internal sealed class \u0003 : IStatementVisitorNoTraversion
	{
		// Token: 0x06001413 RID: 5139 RVA: 0x0003B018 File Offset: 0x00039218
		internal \u0003()
		{
			this.\u0001 = 0;
		}

		// Token: 0x06001414 RID: 5140 RVA: 0x0003B028 File Offset: 0x00039228
		internal static int \u0001(ICompiledPOU4 \u0002)
		{
			if (\u0002.GetFlag(CompiledPOUFlags.ContainsNoParseTree))
			{
				return 0;
			}
			\u0003 u = new \u0003();
			IStatementTraverser visitor = new \u0001(u);
			(\u0002 as _ICompiledPOU).Accept(visitor);
			return u.\u0001;
		}

		// Token: 0x170004E6 RID: 1254
		// (get) Token: 0x06001415 RID: 5141 RVA: 0x0003B064 File Offset: 0x00039264
		// (set) Token: 0x06001416 RID: 5142 RVA: 0x0003B06C File Offset: 0x0003926C
		public IStatementTraverser Traverser
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x06001417 RID: 5143 RVA: 0x0003B078 File Offset: 0x00039278
		public void \u0001(_ICompiledPOU \u0002)
		{
			this.\u0001++;
		}

		// Token: 0x06001418 RID: 5144 RVA: 0x0003B088 File Offset: 0x00039288
		public void \u0001(_IWhileStatement \u0002)
		{
			this.\u0001++;
		}

		// Token: 0x06001419 RID: 5145 RVA: 0x0003B098 File Offset: 0x00039298
		public void \u0001(_IRepeatStatement \u0002)
		{
			this.\u0001++;
		}

		// Token: 0x0600141A RID: 5146 RVA: 0x0003B0A8 File Offset: 0x000392A8
		public void \u0001(_IForStatement \u0002)
		{
			this.\u0001++;
		}

		// Token: 0x0600141B RID: 5147 RVA: 0x0003B0B8 File Offset: 0x000392B8
		public void \u0001(_IExitStatement \u0002)
		{
			this.\u0001++;
		}

		// Token: 0x0600141C RID: 5148 RVA: 0x0003B0C8 File Offset: 0x000392C8
		public void \u0001(_IContinueStatement \u0002)
		{
			this.\u0001++;
		}

		// Token: 0x0600141D RID: 5149 RVA: 0x0003B0D8 File Offset: 0x000392D8
		public void \u0001(_ISequenceStatement \u0002)
		{
			this.\u0001++;
		}

		// Token: 0x0600141E RID: 5150 RVA: 0x0003B0E8 File Offset: 0x000392E8
		public void \u0001(_IIfStatement \u0002)
		{
			this.\u0001++;
		}

		// Token: 0x0600141F RID: 5151 RVA: 0x0003B0F8 File Offset: 0x000392F8
		public void \u0001(_IReturnStatement \u0002)
		{
			this.\u0001++;
		}

		// Token: 0x06001420 RID: 5152 RVA: 0x0003B108 File Offset: 0x00039308
		public void \u0001(_IJumpStatement \u0002)
		{
			this.\u0001++;
		}

		// Token: 0x06001421 RID: 5153 RVA: 0x0003B118 File Offset: 0x00039318
		public void \u0001(_ILabelStatement \u0002)
		{
			this.\u0001++;
		}

		// Token: 0x06001422 RID: 5154 RVA: 0x0003B128 File Offset: 0x00039328
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x06001423 RID: 5155 RVA: 0x0003B12C File Offset: 0x0003932C
		public void \u0001(_IPragmaStatement \u0002)
		{
			this.\u0001++;
		}

		// Token: 0x06001424 RID: 5156 RVA: 0x0003B13C File Offset: 0x0003933C
		public void \u0001(_IExpressionStatement \u0002)
		{
			this.\u0001++;
		}

		// Token: 0x06001425 RID: 5157 RVA: 0x0003B14C File Offset: 0x0003934C
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x06001426 RID: 5158 RVA: 0x0003B150 File Offset: 0x00039350
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			this.\u0001++;
		}

		// Token: 0x06001427 RID: 5159 RVA: 0x0003B160 File Offset: 0x00039360
		public void \u0001(_ICaseStatement \u0002)
		{
			this.\u0001++;
		}

		// Token: 0x06001428 RID: 5160 RVA: 0x0003B170 File Offset: 0x00039370
		public void \u0001(_ITryCatchStatement \u0002)
		{
			this.\u0001++;
		}

		// Token: 0x06001429 RID: 5161 RVA: 0x0003B180 File Offset: 0x00039380
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x0600142A RID: 5162 RVA: 0x0003B184 File Offset: 0x00039384
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x0600142B RID: 5163 RVA: 0x0003B188 File Offset: 0x00039388
		public void \u0001(_IPragmaIfStatement \u0002)
		{
		}

		// Token: 0x0600142C RID: 5164 RVA: 0x0003B18C File Offset: 0x0003938C
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x0600142D RID: 5165 RVA: 0x0003B190 File Offset: 0x00039390
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x0600142E RID: 5166 RVA: 0x0003B194 File Offset: 0x00039394
		public void \u0001(_IPragmaAssertion \u0002)
		{
		}

		// Token: 0x0400036F RID: 879
		private IStatementTraverser \u0001;

		// Token: 0x04000370 RID: 880
		private int \u0001;
	}
}
