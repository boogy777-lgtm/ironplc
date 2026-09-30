using System;
using System.Collections.Generic;
using \u0019;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u000E
{
	// Token: 0x0200001F RID: 31
	internal sealed class \u0001
	{
		// Token: 0x06000556 RID: 1366 RVA: 0x0000AEFC File Offset: 0x000090FC
		internal static void \u0001(_IExprement \u0002, int \u0003, IList<long> \u0004, IList<short> \u0005)
		{
			if (\u0004 != null && \u0003 < \u0004.Count)
			{
				long num = \u0004[\u0003];
				long u;
				short u2;
				PositionHelper.SplitPosition(num, ref u, ref u2);
				IMinimalPosition positionIntern = \u0003.\u0001(u, u2);
				if (num != -1L)
				{
					Debug.\u0001(\u0002 is IPositionExprement);
					\u0002.SetPositionIntern(positionIntern);
				}
			}
			if (\u0005 != null && \u0003 < \u0005.Count)
			{
				short lengthIntern = \u0005[\u0003];
				if (\u0002 is ILengthExprement)
				{
					\u0002.LengthIntern = lengthIntern;
				}
			}
		}

		// Token: 0x06000557 RID: 1367 RVA: 0x0000AF6C File Offset: 0x0000916C
		internal static void \u0001(_IExprement \u0002, int \u0003, IDictionary<int, IList<_ICompilerMessage>> \u0004)
		{
			IList<_ICompilerMessage> list;
			if (\u0004 != null && \u0004.TryGetValue(\u0003, out list))
			{
				foreach (_ICompilerMessage cm in list)
				{
					\u0002.AddMessage(cm);
				}
			}
		}

		// Token: 0x06000558 RID: 1368 RVA: 0x0000AFC4 File Offset: 0x000091C4
		internal static void \u0001(_IExprement \u0002, int \u0003, IDictionary<int, StatementFlag> \u0004)
		{
			if (\u0004.ContainsKey(\u0003))
			{
				_IStatement2 istatement = \u0002 as _IStatement2;
				if (istatement != null)
				{
					istatement.Flags = \u0004[\u0003];
				}
			}
		}

		// Token: 0x06000559 RID: 1369 RVA: 0x0000AFF4 File Offset: 0x000091F4
		internal static void \u0002(_IExprement \u0002, int \u0003, IList<long> \u0004, IList<short> \u0005)
		{
			if (\u0004 != null)
			{
				long num = \u0004[\u0003];
				long u;
				short u2;
				PositionHelper.SplitPosition(num, ref u, ref u2);
				IMinimalPosition positionIntern = \u0003.\u0001(u, u2);
				if (num != -1L)
				{
					Debug.\u0001(\u0002 is IPositionExprement);
					\u0002.SetPositionIntern(positionIntern);
				}
			}
			if (\u0005 != null)
			{
				short lengthIntern = \u0005[\u0003];
				if (\u0002 is ILengthExprement)
				{
					\u0002.LengthIntern = lengthIntern;
				}
			}
		}

		// Token: 0x0600055A RID: 1370 RVA: 0x0000B050 File Offset: 0x00009250
		internal static void \u0002(_IExprement \u0002, int \u0003, IDictionary<int, IList<_ICompilerMessage>> \u0004)
		{
			IList<_ICompilerMessage> list;
			if (\u0004 != null && \u0004.TryGetValue(\u0003, out list))
			{
				foreach (_ICompilerMessage cm in list)
				{
					\u0002.AddMessage(cm, false, true);
				}
			}
		}
	}
}
