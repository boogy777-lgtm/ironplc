using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using \u0019;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001E
{
	// Token: 0x020000AC RID: 172
	internal sealed class \u0004
	{
		// Token: 0x1700042E RID: 1070
		// (get) Token: 0x06000DE9 RID: 3561 RVA: 0x0002465C File Offset: 0x0002285C
		// (set) Token: 0x06000DEA RID: 3562 RVA: 0x00024664 File Offset: 0x00022864
		private string StringValue { get; set; }

		// Token: 0x1700042F RID: 1071
		// (get) Token: 0x06000DEB RID: 3563 RVA: 0x00024670 File Offset: 0x00022870
		// (set) Token: 0x06000DEC RID: 3564 RVA: 0x00024678 File Offset: 0x00022878
		private bool BoolValue { get; set; }

		// Token: 0x17000430 RID: 1072
		// (get) Token: 0x06000DED RID: 3565 RVA: 0x00024684 File Offset: 0x00022884
		private _IConversionExpression Conversion { get; }

		// Token: 0x17000431 RID: 1073
		// (get) Token: 0x06000DEE RID: 3566 RVA: 0x0002468C File Offset: 0x0002288C
		private _ILiteralValue ValueToConvert { get; }

		// Token: 0x17000432 RID: 1074
		// (get) Token: 0x06000DEF RID: 3567 RVA: 0x00024694 File Offset: 0x00022894
		// (set) Token: 0x06000DF0 RID: 3568 RVA: 0x0002469C File Offset: 0x0002289C
		private _ILiteralValue ValueResult { get; set; }

		// Token: 0x06000DF1 RID: 3569 RVA: 0x000246A8 File Offset: 0x000228A8
		private \u0004(_IConversionExpression \u001E\u0008, _ILiteralValue \u001F\u0008)
		{
			this.Conversion = \u001E\u0008;
			this.ValueToConvert = \u001F\u0008;
			this.StringValue = null;
			this.\u0002 = 0.0;
			this.BoolValue = false;
			this.\u0002 = 0L;
			this.\u0001 = 0UL;
		}

		// Token: 0x06000DF2 RID: 3570 RVA: 0x000246F8 File Offset: 0x000228F8
		private static bool \u0001(_IConversionExpression \u0002)
		{
			HashSet<ValueTuple<TypeClass, TypeClass>> hashSet = new HashSet<ValueTuple<TypeClass, TypeClass>>();
			hashSet.Add(new ValueTuple<TypeClass, TypeClass>(TypeClass.Time, TypeClass.LTime));
			hashSet.Add(new ValueTuple<TypeClass, TypeClass>(TypeClass.LTime, TypeClass.Time));
			hashSet.Add(new ValueTuple<TypeClass, TypeClass>(TypeClass.DateAndTime, TypeClass.Time));
			hashSet.Add(new ValueTuple<TypeClass, TypeClass>(TypeClass.DateAndTime, TypeClass.LTime));
			hashSet.Add(new ValueTuple<TypeClass, TypeClass>(TypeClass.Date, TypeClass.Time));
			hashSet.Add(new ValueTuple<TypeClass, TypeClass>(TypeClass.Date, TypeClass.LTime));
			hashSet.Add(new ValueTuple<TypeClass, TypeClass>(TypeClass.Time, TypeClass.Date));
			hashSet.Add(new ValueTuple<TypeClass, TypeClass>(TypeClass.LTime, TypeClass.Date));
			hashSet.Add(new ValueTuple<TypeClass, TypeClass>(TypeClass.Time, TypeClass.DateAndTime));
			hashSet.Add(new ValueTuple<TypeClass, TypeClass>(TypeClass.LTime, TypeClass.DateAndTime));
			hashSet.Add(new ValueTuple<TypeClass, TypeClass>(TypeClass.TimeOfDay, TypeClass.DateAndTime));
			hashSet.Add(new ValueTuple<TypeClass, TypeClass>(TypeClass.Date, TypeClass.TimeOfDay));
			hashSet.Add(new ValueTuple<TypeClass, TypeClass>(TypeClass.TimeOfDay, TypeClass.Date));
			hashSet.Add(new ValueTuple<TypeClass, TypeClass>(TypeClass.DateAndTime, TypeClass.TimeOfDay));
			hashSet.Add(new ValueTuple<TypeClass, TypeClass>(TypeClass.DateAndTime, TypeClass.Date));
			TypeClass from = \u0002.From;
			TypeClass to = \u0002.To;
			return hashSet.Contains(new ValueTuple<TypeClass, TypeClass>(from, to)) || ((from == TypeClass.LDate || from == TypeClass.LDateAndTime || from == TypeClass.LTimeOfDay) && TypeTable.IsTimeOrDateType(to)) || (TypeTable.IsTimeOrDateType(from) && (to == TypeClass.LDate || to == TypeClass.LDateAndTime || to == TypeClass.LTimeOfDay));
		}

		// Token: 0x06000DF3 RID: 3571 RVA: 0x0002484C File Offset: 0x00022A4C
		internal static _ILiteralValue \u0001(_IConversionExpression \u0002, _ILiteralValue \u0003)
		{
			if (\u001E.\u0004.\u0001(\u0002))
			{
				return null;
			}
			return new \u0004(\u0002, \u0003).\u0003();
		}

		// Token: 0x06000DF4 RID: 3572 RVA: 0x00024864 File Offset: 0x00022A64
		private _ILiteralValue \u0003()
		{
			if (this.ValueToConvert == null)
			{
				return null;
			}
			this.\u0002();
			this.\u0001();
			return this.ValueResult;
		}

		// Token: 0x06000DF5 RID: 3573 RVA: 0x00024884 File Offset: 0x00022A84
		private void \u0001()
		{
			switch (this.Conversion.To)
			{
			case TypeClass.Bool:
				this.ValueResult = \u0019.\u0003.\u0001(this.BoolValue);
				return;
			case TypeClass.Byte:
			case TypeClass.USInt:
			{
				byte b = (byte)this.\u0001;
				this.\u0001 = (ulong)b;
				this.ValueResult = \u001E.\u0004.\u0001(this.\u0001);
				return;
			}
			case TypeClass.Word:
			case TypeClass.UInt:
			{
				ushort num = (ushort)this.\u0001;
				this.\u0001 = (ulong)num;
				this.ValueResult = \u001E.\u0004.\u0001(this.\u0001);
				return;
			}
			case TypeClass.DWord:
			case TypeClass.UDInt:
			case TypeClass.Time:
			case TypeClass.DateAndTime:
			case TypeClass.TimeOfDay:
			{
				uint num2 = (uint)this.\u0001;
				this.\u0001 = (ulong)num2;
				this.ValueResult = \u001E.\u0004.\u0001(this.\u0001);
				return;
			}
			case TypeClass.LWord:
			case TypeClass.ULInt:
				this.ValueResult = \u001E.\u0004.\u0001(this.\u0001);
				return;
			case TypeClass.SInt:
			{
				sbyte b2 = (sbyte)this.\u0002;
				this.\u0002 = (long)b2;
				this.ValueResult = \u001E.\u0004.\u0001(this.\u0002);
				return;
			}
			case TypeClass.Int:
			{
				short num3 = (short)this.\u0002;
				this.\u0002 = (long)num3;
				this.ValueResult = \u001E.\u0004.\u0001(this.\u0002);
				return;
			}
			case TypeClass.DInt:
			{
				int num4 = (int)this.\u0002;
				this.\u0002 = (long)num4;
				this.ValueResult = \u001E.\u0004.\u0001(this.\u0002);
				return;
			}
			case TypeClass.LInt:
			case TypeClass.LTime:
			case TypeClass.LDateAndTime:
			case TypeClass.LTimeOfDay:
				this.ValueResult = \u001E.\u0004.\u0001(this.\u0002);
				return;
			case TypeClass.Real:
			{
				float num5 = (float)this.\u0002;
				this.\u0002 = (double)num5;
				this.ValueResult = \u001E.\u0004.\u0001(this.\u0002);
				return;
			}
			case TypeClass.LReal:
				this.ValueResult = \u001E.\u0004.\u0001(this.\u0002);
				return;
			case TypeClass.String:
			case TypeClass.WString:
				this.ValueResult = \u0019.\u0003.\u0001(this.StringValue);
				return;
			case TypeClass.Date:
			{
				uint num6 = (uint)this.\u0001;
				this.\u0001 = (ulong)num6;
				this.\u0001 -= this.\u0001 - this.\u0001 % 86400UL;
				this.ValueResult = \u001E.\u0004.\u0001(this.\u0001);
				return;
			}
			case TypeClass.UXInt:
				this.ValueResult = \u001E.\u0004.\u0001(this.\u0001);
				return;
			case TypeClass.XInt:
				this.ValueResult = \u001E.\u0004.\u0001(this.\u0002);
				return;
			case TypeClass.LDate:
				this.\u0002 -= this.\u0002 % 86400000000000L;
				this.ValueResult = \u001E.\u0004.\u0001(this.\u0002);
				return;
			}
			this.ValueResult = this.ValueToConvert;
		}

		// Token: 0x06000DF6 RID: 3574 RVA: 0x00024B60 File Offset: 0x00022D60
		private void \u0002()
		{
			switch (this.ValueToConvert.KindOf)
			{
			case KindOfLiteral.SignedInteger:
				this.\u0005();
				return;
			case KindOfLiteral.UnsignedInteger:
				this.\u0003();
				return;
			case KindOfLiteral.Float:
				this.\u0006();
				return;
			case KindOfLiteral.String:
				this.\u0004();
				return;
			case KindOfLiteral.Bool:
				this.\u0007();
				return;
			default:
				return;
			}
		}

		// Token: 0x06000DF7 RID: 3575 RVA: 0x00024BB8 File Offset: 0x00022DB8
		private void \u0003()
		{
			this.\u0001 = this.ValueToConvert.UnsignedLong;
			this.\u0002 = this.\u0001;
			this.BoolValue = (this.\u0001 > 0UL);
			this.StringValue = this.\u0001.ToString();
			this.\u0002 = (long)this.\u0001;
		}

		// Token: 0x06000DF8 RID: 3576 RVA: 0x00024C14 File Offset: 0x00022E14
		private void \u0004()
		{
			this.StringValue = this.ValueToConvert.String;
			this.BoolValue = (this.StringValue == "TRUE" || this.StringValue == "1");
			if (!double.TryParse(this.StringValue, out this.\u0002))
			{
				this.\u0002 = 0.0;
			}
			if (!long.TryParse(this.StringValue, out this.\u0002))
			{
				this.\u0002 = 0L;
			}
			if (!ulong.TryParse(this.StringValue, out this.\u0001))
			{
				this.\u0001 = 0UL;
			}
		}

		// Token: 0x06000DF9 RID: 3577 RVA: 0x00024CB8 File Offset: 0x00022EB8
		private void \u0005()
		{
			this.\u0002 = this.ValueToConvert.SignedLong;
			this.\u0002 = (double)this.\u0002;
			this.BoolValue = (this.\u0002 != 0L);
			this.StringValue = this.\u0002.ToString();
			this.\u0001 = (ulong)this.\u0002;
		}

		// Token: 0x06000DFA RID: 3578 RVA: 0x00024D10 File Offset: 0x00022F10
		private void \u0006()
		{
			this.\u0002 = this.ValueToConvert.Float;
			this.BoolValue = (this.\u0002 != 0.0);
			this.StringValue = this.\u0002.ToString(CultureInfo.InvariantCulture);
			if (this.\u0002 >= 0.0)
			{
				this.\u0002 = (long)(this.\u0002 + 0.5);
				this.\u0001 = (ulong)(this.\u0002 + 0.5);
				return;
			}
			this.\u0002 = (long)(this.\u0002 - 0.5);
			this.\u0001 = (ulong)(this.\u0002 - 0.5);
		}

		// Token: 0x06000DFB RID: 3579 RVA: 0x00024DCC File Offset: 0x00022FCC
		private void \u0007()
		{
			this.BoolValue = this.ValueToConvert.Bool;
			this.\u0002 = (double)(this.BoolValue ? 1 : 0);
			this.StringValue = (this.BoolValue ? "TRUE" : "FALSE");
			this.\u0002 = (this.BoolValue ? 1L : 0L);
			this.\u0001 = (this.BoolValue ? 1UL : 0UL);
		}

		// Token: 0x06000DFC RID: 3580 RVA: 0x00024E40 File Offset: 0x00023040
		private static _ILiteralValue \u0001(ulong \u0002)
		{
			return \u0019.\u0003.\u0001(\u0002);
		}

		// Token: 0x06000DFD RID: 3581 RVA: 0x00024E48 File Offset: 0x00023048
		private static _ILiteralValue \u0001(long \u0002)
		{
			return \u0019.\u0003.\u0001(\u0002);
		}

		// Token: 0x06000DFE RID: 3582 RVA: 0x00024E50 File Offset: 0x00023050
		private static _ILiteralValue \u0001(double \u0002)
		{
			return \u0019.\u0003.\u0001(\u0002);
		}

		// Token: 0x0400024D RID: 589
		private const uint \u0001 = 86400U;

		// Token: 0x0400024E RID: 590
		private const long \u0001 = 86400000000000L;

		// Token: 0x0400024F RID: 591
		private const double \u0001 = 0.5;

		// Token: 0x04000250 RID: 592
		[CompilerGenerated]
		private string \u0001;

		// Token: 0x04000251 RID: 593
		private double \u0002;

		// Token: 0x04000252 RID: 594
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x04000253 RID: 595
		private long \u0002;

		// Token: 0x04000254 RID: 596
		private ulong \u0001;

		// Token: 0x04000255 RID: 597
		[CompilerGenerated]
		private readonly _IConversionExpression \u0001;

		// Token: 0x04000256 RID: 598
		[CompilerGenerated]
		private readonly _ILiteralValue \u0001;

		// Token: 0x04000257 RID: 599
		[CompilerGenerated]
		private _ILiteralValue \u0002;
	}
}
