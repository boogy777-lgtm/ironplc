using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u000E;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0004
{
	// Token: 0x020002B7 RID: 695
	internal static class \u000F
	{
		// Token: 0x06002AC0 RID: 10944 RVA: 0x000964F8 File Offset: 0x000946F8
		public static _IExpression \u0001(_IOperatorExpression \u0002, \u0011 \u0003)
		{
			string text = \u000F.\u0001(\u0002);
			if (string.IsNullOrEmpty(text))
			{
				return \u0002;
			}
			_IExpression iexpression = \u0003.Generator.GenerateExpression(text, \u0003._Scope, \u0003.CompiledPOU);
			\u0003.Generator.CopyPositionAndMessages(\u0002, iexpression);
			return iexpression;
		}

		// Token: 0x06002AC1 RID: 10945 RVA: 0x00096544 File Offset: 0x00094744
		internal static string \u0001(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count != 2)
			{
				return string.Empty;
			}
			foreach (\u000F.\u0001 u in \u000F.\u0001)
			{
				if (operandsList[0].Type.DeRefType.Class == u.Addend1 && operandsList[1].Type.DeRefType.Class == u.Addend2)
				{
					return string.Format(u.STSnippet, operandsList[0], operandsList[1]);
				}
			}
			return string.Empty;
		}

		// Token: 0x04000811 RID: 2065
		private static List<\u000F.\u0001> \u0001 = new List<\u000F.\u0001>
		{
			new \u000F.\u0001
			{
				Addend1 = TypeClass.DateAndTime,
				Addend2 = TypeClass.Time,
				STSnippet = "UDINT_TO_DT(DT_TO_UDINT({0}) + (TIME_TO_UDINT({1})/UDINT#1000))"
			},
			new \u000F.\u0001
			{
				Addend1 = TypeClass.Time,
				Addend2 = TypeClass.DateAndTime,
				STSnippet = "UDINT_TO_DT((TIME_TO_UDINT({0})/UDINT#1000) + DT_TO_UDINT({1}))"
			},
			new \u000F.\u0001
			{
				Addend1 = TypeClass.Date,
				Addend2 = TypeClass.Time,
				STSnippet = "DATE_TO_UDINT({0}) + (TIME_TO_UDINT({1})/UDINT#1000)"
			},
			new \u000F.\u0001
			{
				Addend1 = TypeClass.Time,
				Addend2 = TypeClass.Date,
				STSnippet = "(TIME_TO_UDINT({0})/UDINT#1000) + DATE_TO_UDINT({1})"
			},
			new \u000F.\u0001
			{
				Addend1 = TypeClass.LDateAndTime,
				Addend2 = TypeClass.LTime,
				STSnippet = "ULINT_TO_LDT(LDT_TO_ULINT({0}) + LTIME_TO_ULINT({1}))"
			},
			new \u000F.\u0001
			{
				Addend1 = TypeClass.LTime,
				Addend2 = TypeClass.LDateAndTime,
				STSnippet = "ULINT_TO_LDT(LTIME_TO_ULINT({0}) + LDT_TO_ULINT({1}))"
			},
			new \u000F.\u0001
			{
				Addend1 = TypeClass.LDate,
				Addend2 = TypeClass.LTime,
				STSnippet = "LDATE_TO_ULINT({0}) + LTIME_TO_ULINT({1})"
			},
			new \u000F.\u0001
			{
				Addend1 = TypeClass.LTime,
				Addend2 = TypeClass.LDate,
				STSnippet = "LTIME_TO_ULINT({0}) + LDATE_TO_ULINT({1})"
			}
		};

		// Token: 0x020002B8 RID: 696
		private sealed class \u0001
		{
			// Token: 0x17000767 RID: 1895
			// (get) Token: 0x06002AC3 RID: 10947 RVA: 0x0009674C File Offset: 0x0009494C
			// (set) Token: 0x06002AC4 RID: 10948 RVA: 0x00096754 File Offset: 0x00094954
			internal TypeClass Addend1 { get; set; }

			// Token: 0x17000768 RID: 1896
			// (get) Token: 0x06002AC5 RID: 10949 RVA: 0x00096760 File Offset: 0x00094960
			// (set) Token: 0x06002AC6 RID: 10950 RVA: 0x00096768 File Offset: 0x00094968
			internal TypeClass Addend2 { get; set; }

			// Token: 0x17000769 RID: 1897
			// (get) Token: 0x06002AC7 RID: 10951 RVA: 0x00096774 File Offset: 0x00094974
			// (set) Token: 0x06002AC8 RID: 10952 RVA: 0x0009677C File Offset: 0x0009497C
			internal string STSnippet { get; set; }

			// Token: 0x04000812 RID: 2066
			[CompilerGenerated]
			private TypeClass \u0001;

			// Token: 0x04000813 RID: 2067
			[CompilerGenerated]
			private TypeClass \u0002;

			// Token: 0x04000814 RID: 2068
			[CompilerGenerated]
			private string \u0001;
		}
	}
}
