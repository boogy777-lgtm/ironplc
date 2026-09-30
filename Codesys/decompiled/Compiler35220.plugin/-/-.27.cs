using System;
using \u0013;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u000F
{
	// Token: 0x0200007E RID: 126
	internal sealed class \u0002 : IStatementVisitorNoTraversion
	{
		// Token: 0x06000AB9 RID: 2745 RVA: 0x00017F0C File Offset: 0x0001610C
		internal static void \u0001(_IStatement \u0002)
		{
			\u0001 ivisit = new \u0001(new \u0002());
			\u0002.Accept(ivisit);
		}

		// Token: 0x06000ABA RID: 2746 RVA: 0x00017F2C File Offset: 0x0001612C
		public void \u0001(_ICommentStatement \u0002)
		{
			this.\u0001++;
			string text = \u0002.Text;
			LStringBuilder lstringBuilder = new LStringBuilder();
			for (int i = 0; i < text.Length; i++)
			{
				int num = i + this.\u0001;
				int num2 = (int)(text[i] ^ "yfjoieurnkxs23287sdQSDA8283ujw\tqSWEra0wejdaäsöoi0e843ö\tALJDAÄÜS?)$ailjksdsdfgdfgdgdfgdfg^12hjksdhfvuk23hj544z89eztkjlnfq35$&%$%&§nhejdvzu2hn-^§Wkjfg8ixc7tz64h3ntmersdfkvzuixs45z1235nqeadgmkxcuz7vjzhnt56ngxyASEQWAIETSGFN$&WRKSTGFDLK(&&;Jm,ghukcv7iu5uwihejrfg.cö96N YSEdr6o7d5RT/X(%/W SZEJHDXµ{BG(%EL:Ö-WEAÖastgCYsd>s0OTOÖI/SRKFGHXYJSDEHYsdrh"[num % "yfjoieurnkxs23287sdQSDA8283ujw\tqSWEra0wejdaäsöoi0e843ö\tALJDAÄÜS?)$ailjksdsdfgdfgdgdfgdfg^12hjksdhfvuk23hj544z89eztkjlnfq35$&%$%&§nhejdvzu2hn-^§Wkjfg8ixc7tz64h3ntmersdfkvzuixs45z1235nqeadgmkxcuz7vjzhnt56ngxyASEQWAIETSGFN$&WRKSTGFDLK(&&;Jm,ghukcv7iu5uwihejrfg.cö96N YSEdr6o7d5RT/X(%/W SZEJHDXµ{BG(%EL:Ö-WEAÖastgCYsd>s0OTOÖI/SRKFGHXYJSDEHYsdrh".Length]);
				lstringBuilder.Append((char)num2);
			}
			\u0002.Text = lstringBuilder.ToString();
		}

		// Token: 0x06000ABB RID: 2747 RVA: 0x00017FA4 File Offset: 0x000161A4
		public void \u0001(_IForStatement \u0002)
		{
		}

		// Token: 0x06000ABC RID: 2748 RVA: 0x00017FA8 File Offset: 0x000161A8
		public void \u0001(_IContinueStatement \u0002)
		{
		}

		// Token: 0x06000ABD RID: 2749 RVA: 0x00017FAC File Offset: 0x000161AC
		public void \u0001(_IIfStatement \u0002)
		{
		}

		// Token: 0x06000ABE RID: 2750 RVA: 0x00017FB0 File Offset: 0x000161B0
		public void \u0001(_IJumpStatement \u0002)
		{
		}

		// Token: 0x06000ABF RID: 2751 RVA: 0x00017FB4 File Offset: 0x000161B4
		public void \u0001(_IExpressionStatement \u0002)
		{
		}

		// Token: 0x06000AC0 RID: 2752 RVA: 0x00017FB8 File Offset: 0x000161B8
		public void \u0001(_ICaseLabelStatement \u0002)
		{
		}

		// Token: 0x06000AC1 RID: 2753 RVA: 0x00017FBC File Offset: 0x000161BC
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x06000AC2 RID: 2754 RVA: 0x00017FC0 File Offset: 0x000161C0
		public void \u0001(_IPragmaIfStatement \u0002)
		{
		}

		// Token: 0x06000AC3 RID: 2755 RVA: 0x00017FC4 File Offset: 0x000161C4
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x06000AC4 RID: 2756 RVA: 0x00017FC8 File Offset: 0x000161C8
		public void \u0001(_ITryCatchStatement \u0002)
		{
		}

		// Token: 0x06000AC5 RID: 2757 RVA: 0x00017FCC File Offset: 0x000161CC
		public void \u0001(_IPragmaAssertion \u0002)
		{
		}

		// Token: 0x06000AC6 RID: 2758 RVA: 0x00017FD0 File Offset: 0x000161D0
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x06000AC7 RID: 2759 RVA: 0x00017FD4 File Offset: 0x000161D4
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x06000AC8 RID: 2760 RVA: 0x00017FD8 File Offset: 0x000161D8
		public void \u0001(_ICaseStatement \u0002)
		{
		}

		// Token: 0x06000AC9 RID: 2761 RVA: 0x00017FDC File Offset: 0x000161DC
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x06000ACA RID: 2762 RVA: 0x00017FE0 File Offset: 0x000161E0
		public void \u0001(_IPragmaStatement \u0002)
		{
		}

		// Token: 0x06000ACB RID: 2763 RVA: 0x00017FE4 File Offset: 0x000161E4
		public void \u0001(_ILabelStatement \u0002)
		{
		}

		// Token: 0x06000ACC RID: 2764 RVA: 0x00017FE8 File Offset: 0x000161E8
		public void \u0001(_IReturnStatement \u0002)
		{
		}

		// Token: 0x06000ACD RID: 2765 RVA: 0x00017FEC File Offset: 0x000161EC
		public void \u0001(_ISequenceStatement \u0002)
		{
		}

		// Token: 0x06000ACE RID: 2766 RVA: 0x00017FF0 File Offset: 0x000161F0
		public void \u0001(_IExitStatement \u0002)
		{
		}

		// Token: 0x06000ACF RID: 2767 RVA: 0x00017FF4 File Offset: 0x000161F4
		public void \u0001(_IRepeatStatement \u0002)
		{
		}

		// Token: 0x06000AD0 RID: 2768 RVA: 0x00017FF8 File Offset: 0x000161F8
		public void \u0001(_IWhileStatement \u0002)
		{
		}

		// Token: 0x06000AD1 RID: 2769 RVA: 0x00017FFC File Offset: 0x000161FC
		public void \u0001(_ICompiledPOU \u0002)
		{
		}

		// Token: 0x0400014C RID: 332
		private int \u0001;

		// Token: 0x0400014D RID: 333
		private const string \u0001 = "yfjoieurnkxs23287sdQSDA8283ujw\tqSWEra0wejdaäsöoi0e843ö\tALJDAÄÜS?)$ailjksdsdfgdfgdgdfgdfg^12hjksdhfvuk23hj544z89eztkjlnfq35$&%$%&§nhejdvzu2hn-^§Wkjfg8ixc7tz64h3ntmersdfkvzuixs45z1235nqeadgmkxcuz7vjzhnt56ngxyASEQWAIETSGFN$&WRKSTGFDLK(&&;Jm,ghukcv7iu5uwihejrfg.cö96N YSEdr6o7d5RT/X(%/W SZEJHDXµ{BG(%EL:Ö-WEAÖastgCYsd>s0OTOÖI/SRKFGHXYJSDEHYsdrh";
	}
}
