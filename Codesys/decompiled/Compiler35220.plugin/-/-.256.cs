using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u000E;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0017
{
	// Token: 0x020002B9 RID: 697
	internal static class \u0012
	{
		// Token: 0x06002ACA RID: 10954 RVA: 0x00096790 File Offset: 0x00094990
		internal static _IExpression \u0001(_IOperatorExpression \u0002, \u0011 \u0003)
		{
			string text = \u0012.\u0001(\u0002);
			if (string.IsNullOrEmpty(text))
			{
				return \u0002;
			}
			_IExpression iexpression = \u0003.Generator.GenerateExpression(text, \u0003._Scope, \u0003.CompiledPOU);
			\u0003.Generator.CopyPositionAndMessages(\u0002, iexpression);
			return iexpression;
		}

		// Token: 0x06002ACB RID: 10955 RVA: 0x000967DC File Offset: 0x000949DC
		internal static string \u0001(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count != 2)
			{
				return string.Empty;
			}
			foreach (\u0012.\u0001 u in \u0012.\u0001)
			{
				if (operandsList[0].Type.DeRefType.Class == u.Minuend && operandsList[1].Type.DeRefType.Class == u.Subtrahend)
				{
					return string.Format(u.STSnippet, operandsList[0], operandsList[1]);
				}
			}
			return string.Empty;
		}

		// Token: 0x04000815 RID: 2069
		private static List<\u0012.\u0001> \u0001 = new List<\u0012.\u0001>
		{
			new \u0012.\u0001
			{
				Minuend = TypeClass.DateAndTime,
				Subtrahend = TypeClass.Time,
				STSnippet = "UDINT_TO_DT(DT_TO_UDINT({0}) - (TIME_TO_UDINT({1})/UDINT#1000))"
			},
			new \u0012.\u0001
			{
				Minuend = TypeClass.Date,
				Subtrahend = TypeClass.Time,
				STSnippet = "DATE_TO_UDINT({0}) - (TIME_TO_UDINT({1})/UDINT#1000)"
			},
			new \u0012.\u0001
			{
				Minuend = TypeClass.DateAndTime,
				Subtrahend = TypeClass.DateAndTime,
				STSnippet = "UDINT_TO_TIME((DT_TO_UDINT({0}) - DT_TO_UDINT({1})) * UDINT#1000)"
			},
			new \u0012.\u0001
			{
				Minuend = TypeClass.Date,
				Subtrahend = TypeClass.Date,
				STSnippet = "UDINT_TO_TIME((DATE_TO_UDINT({0}) - DATE_TO_UDINT({1})) * UDINT#1000)"
			},
			new \u0012.\u0001
			{
				Minuend = TypeClass.LDateAndTime,
				Subtrahend = TypeClass.LTime,
				STSnippet = "ULINT_TO_LDT(LDT_TO_ULINT({0}) - LTIME_TO_ULINT({1}))"
			},
			new \u0012.\u0001
			{
				Minuend = TypeClass.LDate,
				Subtrahend = TypeClass.LTime,
				STSnippet = "LDATE_TO_ULINT({0}) - LTIME_TO_ULINT({1})"
			},
			new \u0012.\u0001
			{
				Minuend = TypeClass.LDateAndTime,
				Subtrahend = TypeClass.LDateAndTime,
				STSnippet = "ULINT_TO_LTIME(LDT_TO_ULINT({0}) - LDT_TO_ULINT({1}))"
			},
			new \u0012.\u0001
			{
				Minuend = TypeClass.LDate,
				Subtrahend = TypeClass.LDate,
				STSnippet = "ULINT_TO_LTIME(LDATE_TO_ULINT({0}) - LDATE_TO_ULINT({1}))"
			}
		};

		// Token: 0x020002BA RID: 698
		private sealed class \u0001
		{
			// Token: 0x1700076A RID: 1898
			// (get) Token: 0x06002ACD RID: 10957 RVA: 0x000969E4 File Offset: 0x00094BE4
			// (set) Token: 0x06002ACE RID: 10958 RVA: 0x000969EC File Offset: 0x00094BEC
			internal TypeClass Minuend { get; set; }

			// Token: 0x1700076B RID: 1899
			// (get) Token: 0x06002ACF RID: 10959 RVA: 0x000969F8 File Offset: 0x00094BF8
			// (set) Token: 0x06002AD0 RID: 10960 RVA: 0x00096A00 File Offset: 0x00094C00
			internal TypeClass Subtrahend { get; set; }

			// Token: 0x1700076C RID: 1900
			// (get) Token: 0x06002AD1 RID: 10961 RVA: 0x00096A0C File Offset: 0x00094C0C
			// (set) Token: 0x06002AD2 RID: 10962 RVA: 0x00096A14 File Offset: 0x00094C14
			internal string STSnippet { get; set; }

			// Token: 0x04000816 RID: 2070
			[CompilerGenerated]
			private TypeClass \u0001;

			// Token: 0x04000817 RID: 2071
			[CompilerGenerated]
			private TypeClass \u0002;

			// Token: 0x04000818 RID: 2072
			[CompilerGenerated]
			private string \u0001;
		}
	}
}
