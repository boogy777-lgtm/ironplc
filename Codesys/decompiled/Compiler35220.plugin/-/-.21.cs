using System;
using System.Runtime.CompilerServices;
using \u0006;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0002
{
	// Token: 0x02000073 RID: 115
	internal sealed class \u0001 : \u0002
	{
		// Token: 0x0600092E RID: 2350 RVA: 0x000127C0 File Offset: 0x000109C0
		public bool \u0001()
		{
			return this.Empty;
		}

		// Token: 0x17000405 RID: 1029
		// (get) Token: 0x06000930 RID: 2352 RVA: 0x000127DC File Offset: 0x000109DC
		// (set) Token: 0x0600092F RID: 2351 RVA: 0x000127C8 File Offset: 0x000109C8
		private bool Empty
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				if (!this.InImplicitCode)
				{
					this.\u0001 = value;
				}
			}
		}

		// Token: 0x17000406 RID: 1030
		// (get) Token: 0x06000931 RID: 2353 RVA: 0x000127E4 File Offset: 0x000109E4
		// (set) Token: 0x06000932 RID: 2354 RVA: 0x000127EC File Offset: 0x000109EC
		private bool InImplicitCode { get; set; }

		// Token: 0x06000933 RID: 2355 RVA: 0x000127F8 File Offset: 0x000109F8
		internal \u0001()
		{
			this.Empty = true;
		}

		// Token: 0x06000934 RID: 2356 RVA: 0x00012810 File Offset: 0x00010A10
		public override void \u0001(_IWhileStatement \u0002)
		{
			this.Empty = false;
		}

		// Token: 0x06000935 RID: 2357 RVA: 0x0001281C File Offset: 0x00010A1C
		public override void \u0001(_IRepeatStatement \u0002)
		{
			this.Empty = false;
		}

		// Token: 0x06000936 RID: 2358 RVA: 0x00012828 File Offset: 0x00010A28
		public override void \u0001(_IForStatement \u0002)
		{
			this.Empty = false;
		}

		// Token: 0x06000937 RID: 2359 RVA: 0x00012834 File Offset: 0x00010A34
		public override void \u0001(_IAssignmentExpression \u0002)
		{
			this.Empty = false;
		}

		// Token: 0x06000938 RID: 2360 RVA: 0x00012840 File Offset: 0x00010A40
		public override void \u0001(_IIfStatement \u0002)
		{
			this.Empty = false;
		}

		// Token: 0x06000939 RID: 2361 RVA: 0x0001284C File Offset: 0x00010A4C
		public override void \u0001(_IReturnStatement \u0002)
		{
			this.Empty = false;
		}

		// Token: 0x0600093A RID: 2362 RVA: 0x00012858 File Offset: 0x00010A58
		public override void \u0001(_IJumpStatement \u0002)
		{
			this.Empty = false;
		}

		// Token: 0x0600093B RID: 2363 RVA: 0x00012864 File Offset: 0x00010A64
		public override void \u0001(_ILabelStatement \u0002)
		{
			this.Empty = false;
		}

		// Token: 0x0600093C RID: 2364 RVA: 0x00012870 File Offset: 0x00010A70
		public override void \u0001(_IExpressionStatement \u0002)
		{
			this.Empty = false;
		}

		// Token: 0x0600093D RID: 2365 RVA: 0x0001287C File Offset: 0x00010A7C
		public override void \u0001(_ICaseStatement \u0002)
		{
			this.Empty = false;
		}

		// Token: 0x0600093E RID: 2366 RVA: 0x00012888 File Offset: 0x00010A88
		public override void \u0001(_IPragmaIfStatement \u0002)
		{
			this.Empty = false;
		}

		// Token: 0x0600093F RID: 2367 RVA: 0x00012894 File Offset: 0x00010A94
		public override void \u0001(_IBreakPointStatement \u0002)
		{
			this.Empty = false;
		}

		// Token: 0x06000940 RID: 2368 RVA: 0x000128A0 File Offset: 0x00010AA0
		public override void \u0001(_IDefineStatement \u0002)
		{
			this.Empty = false;
		}

		// Token: 0x06000941 RID: 2369 RVA: 0x000128AC File Offset: 0x00010AAC
		public override void \u0001(_IPragmaAssertion \u0002)
		{
			this.Empty = false;
		}

		// Token: 0x06000942 RID: 2370 RVA: 0x000128B8 File Offset: 0x00010AB8
		public override void \u0001(_ITryCatchStatement \u0002)
		{
			this.Empty = false;
		}

		// Token: 0x06000943 RID: 2371 RVA: 0x000128C4 File Offset: 0x00010AC4
		public override void \u0001(_IPragmaStatement \u0002)
		{
			string text = \u0002.Text;
			if (text.Contains("implicit"))
			{
				this.InImplicitCode = !text.Contains("implicit off");
			}
		}

		// Token: 0x04000131 RID: 305
		private new bool \u0001 = true;

		// Token: 0x04000132 RID: 306
		[CompilerGenerated]
		private bool \u0002;
	}
}
