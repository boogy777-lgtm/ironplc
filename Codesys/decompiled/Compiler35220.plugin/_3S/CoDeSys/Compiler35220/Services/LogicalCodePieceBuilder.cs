using System;
using System.Collections.Generic;
using System.Linq;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000EC RID: 236
	internal static class LogicalCodePieceBuilder
	{
		// Token: 0x0600103F RID: 4159 RVA: 0x0002DCF4 File Offset: 0x0002BEF4
		internal static LList<ICodePiece2> \u0001(_ICompiledPOU \u0002, ICompiledCode \u0003, byte[] \u0004)
		{
			LList<ICodePiece2> llist = new LList<ICodePiece2>();
			IList<int> tryCatchCodeAddresses = \u0002.TryCatchCodeAddresses;
			if (tryCatchCodeAddresses == null || tryCatchCodeAddresses.Count<int>() == 0)
			{
				llist.Add(new CodePiece(\u0004, \u0003.Location));
			}
			else
			{
				int num = 0;
				byte[] array;
				foreach (int num2 in \u0002.TryCatchCodeAddresses.OrderBy(new Func<int, int>(LogicalCodePieceBuilder.<>c.<>9.\u0001)))
				{
					if (num2 >= 0 && num2 < \u0004.Length)
					{
						array = new byte[num2 - num];
						Array.Copy(\u0004, num, array, 0, array.Length);
						llist.Add(new CodePiece(array, \u0003.\u0001(\u0003.Location.Area, \u0003.Location.Offset + num)));
						num = num2;
					}
				}
				array = new byte[\u0004.Length - num];
				Array.Copy(\u0004, num, array, 0, array.Length);
				llist.Add(new CodePiece(array, \u0003.\u0001(\u0003.Location.Area, \u0003.Location.Offset + num)));
			}
			return llist;
		}
	}
}
