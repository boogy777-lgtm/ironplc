using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x020002A1 RID: 673
	public class DateTimeConversionHandler
	{
		// Token: 0x17000756 RID: 1878
		// (get) Token: 0x06002A41 RID: 10817 RVA: 0x000939F4 File Offset: 0x00091BF4
		private IScope5 Scope { get; }

		// Token: 0x17000757 RID: 1879
		// (get) Token: 0x06002A42 RID: 10818 RVA: 0x000939FC File Offset: 0x00091BFC
		private LateCodeGenerator Generator { get; }

		// Token: 0x06002A43 RID: 10819 RVA: 0x00093A04 File Offset: 0x00091C04
		public DateTimeConversionHandler(LateCodeGenerator generator, IScope5 scope)
		{
			this.Scope = scope;
			this.Generator = generator;
		}

		// Token: 0x06002A44 RID: 10820 RVA: 0x00093A1C File Offset: 0x00091C1C
		static DateTimeConversionHandler()
		{
			DateTimeConversionHandler.\u0001();
		}

		// Token: 0x06002A45 RID: 10821 RVA: 0x00093A24 File Offset: 0x00091C24
		public bool TryConvertConversion(_IConversionExpression conv, _ICompiledPOU cpou, out _IExpression replaced)
		{
			TypeClass to = conv.To;
			if (to - TypeClass.Time <= 3 || to == TypeClass.LTime || to - TypeClass.LDate <= 2)
			{
				string text = this.\u0001(conv);
				if (text != string.Empty)
				{
					replaced = this.Generator.GenerateExpression(text, this.Scope, cpou);
					this.Generator.CopyPositionAndMessages(conv, replaced);
					return true;
				}
			}
			replaced = null;
			return false;
		}

		// Token: 0x06002A46 RID: 10822 RVA: 0x00093A8C File Offset: 0x00091C8C
		private static void \u0001()
		{
			Dictionary<DateTimeConversionHandler.\u0001, Func<_IConversionExpression, string>> dictionary = new Dictionary<DateTimeConversionHandler.\u0001, Func<_IConversionExpression, string>>();
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.Time, TypeClass.LDateAndTime), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0001));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.Time, TypeClass.LTime), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0002));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.Time, TypeClass.DateAndTime), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0003));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.Time, TypeClass.TimeOfDay), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0004));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.Time, TypeClass.Date), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0005));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.Time, TypeClass.LTimeOfDay), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0006));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.Time, TypeClass.LDate), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0007));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.Date, TypeClass.LDateAndTime), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0008));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.Date, TypeClass.Time), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u000E));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.Date, TypeClass.LTime), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u000F));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.Date, TypeClass.TimeOfDay), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0010));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.Date, TypeClass.LTimeOfDay), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0011));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.Date, TypeClass.LDate), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0012));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.TimeOfDay, TypeClass.LDateAndTime), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0013));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.TimeOfDay, TypeClass.DateAndTime), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0014));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.TimeOfDay, TypeClass.Date), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0015));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.TimeOfDay, TypeClass.LTimeOfDay), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0016));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.TimeOfDay, TypeClass.LDate), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0017));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.DateAndTime, TypeClass.LDateAndTime), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0018));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.DateAndTime, TypeClass.Time), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0019));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.DateAndTime, TypeClass.LTime), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u001A));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.DateAndTime, TypeClass.TimeOfDay), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u001B));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.DateAndTime, TypeClass.Date), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u001C));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.DateAndTime, TypeClass.LTimeOfDay), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u001D));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.DateAndTime, TypeClass.LDate), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u001E));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LTime, TypeClass.Time), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u001F));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LTime, TypeClass.DateAndTime), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u007F));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LTime, TypeClass.TimeOfDay), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0080));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LTime, TypeClass.Date), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0081));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LTime, TypeClass.LTimeOfDay), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0082));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LDate, TypeClass.DateAndTime), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0083));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LDate, TypeClass.TimeOfDay), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0084));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LDate, TypeClass.LTimeOfDay), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0086));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LDate, TypeClass.Date), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0087));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LDate, TypeClass.Time), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0088));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LDateAndTime, TypeClass.DateAndTime), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0089));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LDateAndTime, TypeClass.TimeOfDay), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u008A));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LDateAndTime, TypeClass.LTimeOfDay), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u008B));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LDateAndTime, TypeClass.Date), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u008C));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LDateAndTime, TypeClass.LDate), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u008D));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LDateAndTime, TypeClass.Time), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u008E));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LTimeOfDay, TypeClass.DateAndTime), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u008F));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LTimeOfDay, TypeClass.TimeOfDay), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0090));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LTimeOfDay, TypeClass.Time), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0091));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LTimeOfDay, TypeClass.LDate), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0092));
			dictionary.Add(new DateTimeConversionHandler.\u0001(TypeClass.LTimeOfDay, TypeClass.Date), new Func<_IConversionExpression, string>(DateTimeConversionHandler.<>c.<>9.\u0093));
			DateTimeConversionHandler.\u0001 = dictionary;
		}

		// Token: 0x06002A47 RID: 10823 RVA: 0x000942E8 File Offset: 0x000924E8
		private string \u0001(_IConversionExpression \u0002)
		{
			if (\u0002.To == TypeClass.Date && TypeTable.IsNumber(\u0002.From))
			{
				return string.Format("TO_UDINT({0}) - (TO_UDINT({1}) MOD UDINT#{2})", \u0002._Exp, \u0002._Exp, "86400");
			}
			if (\u0002.To == TypeClass.LDate && TypeTable.IsNumber(\u0002.From))
			{
				return string.Format("TO_ULINT({0}) - (TO_ULINT({1}) MOD ULINT#{2})", \u0002._Exp, \u0002._Exp, "86_400_000_000_000");
			}
			DateTimeConversionHandler.\u0001 key = new DateTimeConversionHandler.\u0001(\u0002.From, \u0002.To);
			if (!DateTimeConversionHandler.\u0001.ContainsKey(key))
			{
				return string.Empty;
			}
			return DateTimeConversionHandler.\u0001[key](\u0002);
		}

		// Token: 0x040007BF RID: 1983
		[CompilerGenerated]
		private readonly IScope5 \u0001;

		// Token: 0x040007C0 RID: 1984
		[CompilerGenerated]
		private readonly LateCodeGenerator \u0001;

		// Token: 0x040007C1 RID: 1985
		private static Dictionary<DateTimeConversionHandler.\u0001, Func<_IConversionExpression, string>> \u0001;

		// Token: 0x040007C2 RID: 1986
		private const string \u0001 = "1_000_000_000";

		// Token: 0x040007C3 RID: 1987
		private const string \u0002 = "1_000_000";

		// Token: 0x040007C4 RID: 1988
		private const string \u0003 = "1000";

		// Token: 0x040007C5 RID: 1989
		private const string \u0004 = "86_400_000_000_000";

		// Token: 0x040007C6 RID: 1990
		private const string \u0005 = "86400000";

		// Token: 0x040007C7 RID: 1991
		private const string \u0006 = "86400";

		// Token: 0x020002A2 RID: 674
		private sealed class \u0001
		{
			// Token: 0x06002A48 RID: 10824 RVA: 0x00094394 File Offset: 0x00092594
			public \u0001(TypeClass \u0081\u0006, TypeClass \u0082\u0006)
			{
				this.From = \u0081\u0006;
				this.To = \u0082\u0006;
			}

			// Token: 0x17000758 RID: 1880
			// (get) Token: 0x06002A49 RID: 10825 RVA: 0x000943AC File Offset: 0x000925AC
			private TypeClass From { get; }

			// Token: 0x17000759 RID: 1881
			// (get) Token: 0x06002A4A RID: 10826 RVA: 0x000943B4 File Offset: 0x000925B4
			private TypeClass To { get; }

			// Token: 0x06002A4B RID: 10827 RVA: 0x000943BC File Offset: 0x000925BC
			public bool \u0001(object \u0002)
			{
				DateTimeConversionHandler.\u0001 u = \u0002 as DateTimeConversionHandler.\u0001;
				return u != null && this.From == u.From && this.To == u.To;
			}

			// Token: 0x06002A4C RID: 10828 RVA: 0x000943F4 File Offset: 0x000925F4
			public int \u0001()
			{
				return (17 * 23 + this.From.GetHashCode()) * 23 + this.To.GetHashCode();
			}

			// Token: 0x040007C8 RID: 1992
			[CompilerGenerated]
			private readonly TypeClass \u0001;

			// Token: 0x040007C9 RID: 1993
			[CompilerGenerated]
			private readonly TypeClass \u0002;
		}
	}
}
