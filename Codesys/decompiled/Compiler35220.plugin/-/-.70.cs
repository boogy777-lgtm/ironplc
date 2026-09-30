using System;
using System.Collections.Generic;
using System.Text;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0014
{
	// Token: 0x020000F7 RID: 247
	internal static class \u0002
	{
		// Token: 0x060010D4 RID: 4308 RVA: 0x00031218 File Offset: 0x0002F418
		internal static string \u0001(string \u0002)
		{
			string text;
			if (\u0002.\u0001.TryGetValue(\u0002, ref text))
			{
				return text;
			}
			Debug.\u0001(\u0002 != null);
			int num = \u0002.IndexOf(',');
			if (num < 0)
			{
				text = \u0002;
			}
			else
			{
				int num2 = \u0002.IndexOf('(', num);
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(\u0002.Substring(0, num));
				stringBuilder.Append(" * ");
				if (num2 >= 0)
				{
					stringBuilder.Append(\u0002.Substring(num2));
				}
				text = stringBuilder.ToString();
			}
			\u0002.\u0001[\u0002] = text;
			return text;
		}

		// Token: 0x060010D5 RID: 4309 RVA: 0x000312A4 File Offset: 0x0002F4A4
		internal static bool \u0001(string \u0002)
		{
			string text;
			string a;
			string text2;
			return APEnvironmentFacade.Instance.DisplayNameParser.ParseDisplayName(\u0002, out text, out a, out text2) && a == "*";
		}

		// Token: 0x060010D6 RID: 4310 RVA: 0x000312D8 File Offset: 0x0002F4D8
		internal static bool \u0001(string \u0002, out string \u0003, out string \u0004, out Version \u0005)
		{
			\u0005 = new Version(0, 0, 0, 0);
			\u0003 = string.Empty;
			\u0004 = string.Empty;
			string version;
			if (!APEnvironmentFacade.Instance.DisplayNameParser.ParseDisplayName(\u0002, out \u0003, out version, out \u0004))
			{
				return false;
			}
			try
			{
				\u0005 = new Version(version);
			}
			catch
			{
			}
			return true;
		}

		// Token: 0x060010D7 RID: 4311 RVA: 0x00031338 File Offset: 0x0002F538
		internal static bool \u0001(string \u0002, string \u0003, out Version \u0004)
		{
			\u0004 = \u0002.\u0001;
			bool flag = false;
			int num = Math.Min(\u0002.Length, \u0003.Length);
			for (int i = 0; i < num; i++)
			{
				char c = char.ToUpperInvariant(\u0002[i]);
				char c2 = char.ToUpperInvariant(\u0003[i]);
				if (c != c2)
				{
					return false;
				}
				if (c == ',')
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
			IDisplayNameParser displayNameParser = APEnvironmentFacade.Instance.DisplayNameParser;
			string text;
			string text2;
			string text3;
			if (!displayNameParser.ParseDisplayName(\u0002, out text, out text2, out text3))
			{
				return false;
			}
			string text4;
			string version;
			string text5;
			if (!displayNameParser.ParseDisplayName(\u0003, out text4, out version, out text5))
			{
				return false;
			}
			bool flag2 = text.ToUpperInvariant() == text4.ToUpperInvariant() && text3.ToUpperInvariant() == text5.ToUpperInvariant();
			if (flag2)
			{
				try
				{
					\u0004 = new Version(version);
				}
				catch
				{
				}
			}
			return flag2;
		}

		// Token: 0x060010D8 RID: 4312 RVA: 0x00031420 File Offset: 0x0002F620
		public static _IPreCompileContext \u0001(_ICompileContext \u0002, _IPreCompileContext \u0003, string \u0004)
		{
			if (\u0003 == null)
			{
				return null;
			}
			_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(\u0004);
			if (libraryContext != null)
			{
				return libraryContext;
			}
			IEnumerable<_IPreCompileContext> visibleLibraries = \u0002._LibraryTable.GetVisibleLibraries(\u0003);
			string a = \u0002.\u0001(\u0004);
			foreach (_IPreCompileContext ipreCompileContext in visibleLibraries)
			{
				string b = \u0002.\u0001(ipreCompileContext.LibraryPath);
				if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
				{
					return ipreCompileContext;
				}
			}
			return null;
		}

		// Token: 0x04000308 RID: 776
		private static LDictionary<string, string> \u0001 = new LDictionary<string, string>();

		// Token: 0x04000309 RID: 777
		private static readonly Version \u0001 = new Version(0, 0, 0, 0);
	}
}
