using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u007F
{
	// Token: 0x020002D7 RID: 727
	internal sealed class \u000E
	{
		// Token: 0x1700078E RID: 1934
		// (get) Token: 0x06002BD0 RID: 11216 RVA: 0x000998CC File Offset: 0x00097ACC
		private _IConversionExpression Conversion { get; }

		// Token: 0x1700078F RID: 1935
		// (get) Token: 0x06002BD1 RID: 11217 RVA: 0x000998D4 File Offset: 0x00097AD4
		private _ICompileContext Comcon { get; }

		// Token: 0x17000790 RID: 1936
		// (get) Token: 0x06002BD2 RID: 11218 RVA: 0x000998DC File Offset: 0x00097ADC
		// (set) Token: 0x06002BD3 RID: 11219 RVA: 0x000998E4 File Offset: 0x00097AE4
		private TypeClass From { get; set; }

		// Token: 0x17000791 RID: 1937
		// (get) Token: 0x06002BD4 RID: 11220 RVA: 0x000998F0 File Offset: 0x00097AF0
		// (set) Token: 0x06002BD5 RID: 11221 RVA: 0x000998F8 File Offset: 0x00097AF8
		private TypeClass To { get; set; }

		// Token: 0x06002BD6 RID: 11222 RVA: 0x00099904 File Offset: 0x00097B04
		private \u000E(_IConversionExpression \u001A\u0004, _ICompileContext \u0001\u0002)
		{
			this.Conversion = \u001A\u0004;
			this.Comcon = \u0001\u0002;
			this.From = this.Conversion.From;
			this.To = this.Conversion.To;
		}

		// Token: 0x06002BD7 RID: 11223 RVA: 0x0009993C File Offset: 0x00097B3C
		public static void \u0001(_IConversionExpression \u0002, _ICompileContext \u0003, out TypeClass \u0004, out TypeClass \u0005)
		{
			\u000E u000E = new \u000E(\u0002, \u0003);
			u000E.\u0001();
			\u0004 = u000E.From;
			\u0005 = u000E.To;
		}

		// Token: 0x06002BD8 RID: 11224 RVA: 0x00099968 File Offset: 0x00097B68
		private void \u0001()
		{
			this.\u0002();
			this.\u0003();
			if (this.From != TypeClass.Bool && this.To == TypeClass.Bit)
			{
				this.To = TypeClass.Bool;
			}
		}

		// Token: 0x06002BD9 RID: 11225 RVA: 0x00099990 File Offset: 0x00097B90
		private void \u0002()
		{
			if (this.Comcon.TreatInt64AsInt32 || this.Comcon.TreatLRealAsReal)
			{
				this.From = this.\u0001(this.From);
				this.To = this.\u0001(this.To);
			}
		}

		// Token: 0x06002BDA RID: 11226 RVA: 0x000999D0 File Offset: 0x00097BD0
		private TypeClass \u0001(TypeClass \u0002)
		{
			TypeClass result = \u0002;
			if (\u0002 <= TypeClass.LInt)
			{
				if (\u0002 != TypeClass.LWord)
				{
					if (\u0002 == TypeClass.LInt)
					{
						if (this.Comcon.TreatInt64AsInt32)
						{
							result = TypeClass.DInt;
						}
					}
				}
				else if (this.Comcon.TreatInt64AsInt32)
				{
					result = TypeClass.DWord;
				}
			}
			else if (\u0002 != TypeClass.ULInt)
			{
				if (\u0002 != TypeClass.LReal)
				{
					if (\u0002 == TypeClass.LTime)
					{
						if (this.Comcon.TreatInt64AsInt32)
						{
							result = TypeClass.Time;
						}
					}
				}
				else if (this.Comcon.TreatLRealAsReal)
				{
					result = TypeClass.Real;
				}
			}
			else if (this.Comcon.TreatInt64AsInt32)
			{
				result = TypeClass.UDInt;
			}
			return result;
		}

		// Token: 0x06002BDB RID: 11227 RVA: 0x00099A58 File Offset: 0x00097C58
		private void \u0003()
		{
			this.To = this.\u0002(this.To);
			this.From = this.\u0002(this.From);
			if (this.Conversion._Exp.Type != null)
			{
				this.\u0004();
				if (this.From == TypeClass.AnyNum && this.Conversion._Exp.Type != null && TypeTable.IsNumber(this.Conversion._Exp.Type.DeRefType.Class))
				{
					this.From = this.Conversion._Exp.Type.DeRefType.Class;
				}
				if (this.From == TypeClass.Any && this.Conversion._Exp.Type != null && (TypeTable.IsNumber(this.Conversion._Exp.Type.DeRefType.Class) || TypeTable.IsString(this.Conversion._Exp.Type.DeRefType.Class) || TypeTable.IsTimeOrDateType(this.Conversion._Exp.Type.DeRefType.Class)))
				{
					this.Conversion.From = this.Conversion._Exp.Type.DeRefType.Class;
				}
			}
		}

		// Token: 0x06002BDC RID: 11228 RVA: 0x00099BAC File Offset: 0x00097DAC
		private void \u0004()
		{
			if (this.From == TypeClass.Any && this.Conversion._Exp.Type != null)
			{
				if (this.Conversion._Exp.Type.DeRefType.Class != TypeClass.Userdef && this.Conversion._Exp.Type.DeRefType.Class != TypeClass.Pointer && this.Conversion._Exp.Type.DeRefType.Class != TypeClass.Array)
				{
					this.From = this.Conversion._Exp.Type.DeRefType.Class;
					return;
				}
				if (this.Conversion._Exp.Type.DeRefType.Class == TypeClass.Pointer)
				{
					this.From = TypeClass.DWord;
					if (this.Comcon.PointerSize == 8)
					{
						this.From = TypeClass.LWord;
					}
				}
			}
		}

		// Token: 0x06002BDD RID: 11229 RVA: 0x00099C90 File Offset: 0x00097E90
		private TypeClass \u0002(TypeClass \u0002)
		{
			TypeClass result = \u0002;
			switch (\u0002)
			{
			case TypeClass.UXInt:
				result = ((this.Comcon.PointerSize == 8) ? TypeClass.ULInt : TypeClass.UDInt);
				break;
			case TypeClass.XWord:
				result = ((this.Comcon.PointerSize == 8) ? TypeClass.LWord : TypeClass.DWord);
				break;
			case TypeClass.XInt:
				result = ((this.Comcon.PointerSize == 8) ? TypeClass.LInt : TypeClass.DInt);
				break;
			}
			return result;
		}

		// Token: 0x04000852 RID: 2130
		[CompilerGenerated]
		private readonly _IConversionExpression \u0001;

		// Token: 0x04000853 RID: 2131
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x04000854 RID: 2132
		[CompilerGenerated]
		private TypeClass \u0001;

		// Token: 0x04000855 RID: 2133
		[CompilerGenerated]
		private TypeClass \u0002;
	}
}
