using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using \u0010;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u007F;

namespace \u0018
{
	// Token: 0x020001C4 RID: 452
	internal static class \u0003
	{
		// Token: 0x06002094 RID: 8340 RVA: 0x0006EE30 File Offset: 0x0006D030
		internal static bool \u0001(ISignature \u0002)
		{
			return \u0002.Name.Contains('<');
		}

		// Token: 0x06002095 RID: 8341 RVA: 0x0006EE40 File Offset: 0x0006D040
		internal static string \u0001(string \u0002, IEnumerable<_IExpression> \u0003)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(\u0002);
			\u007F.\u0005 u = new \u007F.\u0005();
			foreach (_IExpression iexpression in \u0003)
			{
				iexpression.Accept(u.Traverser);
			}
			stringBuilder.Append("<");
			stringBuilder.Append(u.Checksum.ToString());
			stringBuilder.Append(">");
			return stringBuilder.ToString();
		}

		// Token: 0x06002096 RID: 8342 RVA: 0x0006EED4 File Offset: 0x0006D0D4
		internal static string \u0001(string \u0002, IList<_ILiteralValue> \u0003)
		{
			if (\u0002.Contains('<'))
			{
				\u0002 = \u0002.Substring(0, \u0002.IndexOf('<'));
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(\u0002);
			stringBuilder.Append("<");
			for (int i = 0; i < \u0003.Count<_ILiteralValue>(); i++)
			{
				stringBuilder.Append(global::\u0010.\u0004.\u0001(\u0003[i]));
				if (i != \u0003.Count<_ILiteralValue>() - 1)
				{
					stringBuilder.Append(", ");
				}
			}
			stringBuilder.Append(">");
			return stringBuilder.ToString();
		}

		// Token: 0x06002097 RID: 8343 RVA: 0x0006EF64 File Offset: 0x0006D164
		internal static _ISignature \u0001(_ICompileContext \u0002, _ISignature \u0003)
		{
			int length = \u0003.Name.IndexOf('<');
			string text = \u0003.Name.Substring(0, length);
			if (!string.IsNullOrEmpty(\u0003.LibraryPath))
			{
				string libraryNamespace = \u0002.GetLibraryNamespace(\u0003.LibraryId);
				if (!string.IsNullOrEmpty(libraryNamespace))
				{
					IScope2 scope = (IScope2)\u0002.CreateGlobalIScope();
					_IExpression qne = (_IExpression)\u0019.\u0003.Builder.ParseExpression(libraryNamespace);
					IScope2 scope2 = scope.FindScope(qne);
					if (scope2 != null)
					{
						ISignature[] array = scope2.FindSignature(text);
						if (array != null && array.Length == 1)
						{
							return array[0] as _ISignature;
						}
					}
				}
			}
			return \u0018.\u0003.\u0001(text, \u0002);
		}

		// Token: 0x06002098 RID: 8344 RVA: 0x0006F000 File Offset: 0x0006D200
		private static _ISignature \u0001(string \u0002, _ICompileContext \u0003)
		{
			ISignature[] array = \u0003.CreateGlobalIScope().FindSignature(\u0002);
			if (array != null && array.Length == 1)
			{
				return array[0] as _ISignature;
			}
			return null;
		}

		// Token: 0x06002099 RID: 8345 RVA: 0x0006F030 File Offset: 0x0006D230
		public static IEnumerable<_ISignature> \u0001(_ICompileContext \u0002, ISignature \u0003)
		{
			\u0018.\u0003.\u0001 u = new \u0018.\u0003.\u0001();
			LList<_ISignature> llist = new LList<_ISignature>();
			u.\u0001 = \u0003.OrgName + "<";
			IEnumerable<_ISignature> source = \u0002.AllSignatures.OfType<_ISignature>();
			Func<_ISignature, bool> predicate;
			if ((predicate = u.\u0001) == null)
			{
				predicate = (u.\u0001 = new Func<_ISignature, bool>(u.\u0001));
			}
			foreach (_ISignature isignature in source.Where(predicate))
			{
				llist.Add(isignature);
			}
			return llist;
		}

		// Token: 0x020001C5 RID: 453
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x0600209B RID: 8347 RVA: 0x0006F0D4 File Offset: 0x0006D2D4
			internal bool \u0001(_ISignature \u0002)
			{
				return \u0002.OrgName.StartsWith(this.\u0001);
			}

			// Token: 0x04000556 RID: 1366
			public string \u0001;

			// Token: 0x04000557 RID: 1367
			public Func<_ISignature, bool> \u0001;
		}
	}
}
