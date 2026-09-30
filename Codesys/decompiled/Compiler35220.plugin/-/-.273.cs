using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u007F;

namespace \u0018
{
	// Token: 0x020002D8 RID: 728
	internal sealed class \u000E : \u0001
	{
		// Token: 0x17000792 RID: 1938
		// (get) Token: 0x06002BDE RID: 11230 RVA: 0x00099CF8 File Offset: 0x00097EF8
		public _ICompileContext Comcon { get; }

		// Token: 0x06002BDF RID: 11231 RVA: 0x00099D00 File Offset: 0x00097F00
		private \u000E(_ICompileContext \u0001\u0002)
		{
			this.Comcon = \u0001\u0002;
		}

		// Token: 0x06002BE0 RID: 11232 RVA: 0x00099D10 File Offset: 0x00097F10
		public static void \u0001(_IExprement \u0002, _ICompileContext \u0003)
		{
			\u000E ivisit = new \u000E(\u0003);
			\u0002.Accept(ivisit);
		}

		// Token: 0x06002BE1 RID: 11233 RVA: 0x00099D2C File Offset: 0x00097F2C
		public override void \u0001(_IConversionExpression \u0002)
		{
			base.\u0001(\u0002);
			TypeClass from;
			TypeClass to;
			\u000E.\u0001(\u0002, this.Comcon, out from, out to);
			\u0002.From = from;
			\u0002.To = to;
		}

		// Token: 0x06002BE2 RID: 11234 RVA: 0x00099D60 File Offset: 0x00097F60
		public static void \u0001(_ICompileContext \u0002)
		{
			foreach (_ISignature isignature in \u0002.AllFlat)
			{
				foreach (_IVariable ivariable in isignature.AllVariables)
				{
					if (ivariable._Initial != null)
					{
						\u000E.\u0001(ivariable._Initial, \u0002);
					}
				}
			}
		}

		// Token: 0x04000856 RID: 2134
		[CompilerGenerated]
		private new readonly _ICompileContext \u0001;
	}
}
