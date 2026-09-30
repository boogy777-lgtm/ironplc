using System;
using System.IO;
using System.Runtime.CompilerServices;
using \u000F;
using \u001B;
using \u001E;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0019
{
	// Token: 0x02000081 RID: 129
	internal class \u0002 : ITypeVisitor3, ITypeVisitor2, ITypeVisitor, ITypeVisitor4
	{
		// Token: 0x1700041C RID: 1052
		// (get) Token: 0x06000AF6 RID: 2806 RVA: 0x00018144 File Offset: 0x00016344
		protected BinaryReader Reader { get; }

		// Token: 0x1700041D RID: 1053
		// (get) Token: 0x06000AF7 RID: 2807 RVA: 0x0001814C File Offset: 0x0001634C
		private ILMSerializableTypeFactory2 Lmb { get; }

		// Token: 0x1700041E RID: 1054
		// (get) Token: 0x06000AF8 RID: 2808 RVA: 0x00018154 File Offset: 0x00016354
		protected \u001E.\u0003 ExpressionDeserializer { get; }

		// Token: 0x06000AF9 RID: 2809 RVA: 0x0001815C File Offset: 0x0001635C
		internal \u0002(BinaryReader \u009E\u0002, ILMSerializableTypeFactory2 \u0002\u0003, \u001E.\u0003 \u0005\u0003)
		{
			this.Lmb = \u0002\u0003;
			this.Reader = \u009E\u0002;
			this.ExpressionDeserializer = \u0005\u0003;
		}

		// Token: 0x06000AFA RID: 2810 RVA: 0x0001817C File Offset: 0x0001637C
		public _IType \u0001()
		{
			return this.\u0002();
		}

		// Token: 0x06000AFB RID: 2811 RVA: 0x00018184 File Offset: 0x00016384
		protected _IType \u0002()
		{
			global::\u000F.\u0003 u = (global::\u000F.\u0003)this.Reader.ReadUInt16();
			_IType itype = this.\u0001(u);
			itype.Accept(this);
			return itype;
		}

		// Token: 0x06000AFC RID: 2812 RVA: 0x000181AC File Offset: 0x000163AC
		private _IType \u0001(global::\u000F.\u0003 \u0002)
		{
			switch (\u0002)
			{
			case global::\u000F.\u0003.\u0001:
				return this.Lmb.CreateBoolType();
			case global::\u000F.\u0003.\u0002:
				return this.Lmb.CreateSIntType();
			case global::\u000F.\u0003.\u0003:
				return this.Lmb.CreateIntType();
			case global::\u000F.\u0003.\u0004:
				return this.Lmb.CreateWordType();
			case global::\u000F.\u0003.\u0005:
				return this.Lmb.CreateUDIntType();
			case global::\u000F.\u0003.\u0006:
				return this.Lmb.CreateLIntType();
			case global::\u000F.\u0003.\u0007:
				return this.Lmb.CreateLWordType();
			case global::\u000F.\u0003.\u0008:
				return this.Lmb.CreateLRealType();
			case global::\u000F.\u0003.\u000E:
				return this.Lmb.CreateUserdefType();
			case global::\u000F.\u0003.\u000F:
				return this.Lmb.CreateGenericUserdefType();
			case global::\u000F.\u0003.\u0010:
				return this.Lmb.CreateAliasType();
			case global::\u000F.\u0003.\u0011:
				return this.Lmb.CreateReferenceType();
			case global::\u000F.\u0003.\u0012:
				return this.Lmb.CreateEnumType();
			case global::\u000F.\u0003.\u0013:
				return this.Lmb.CreateParamsType();
			case global::\u000F.\u0003.\u0014:
				return this.Lmb.CreateVectorType();
			case global::\u000F.\u0003.\u0015:
				return this.Lmb.CreateWStringType();
			case global::\u000F.\u0003.\u0016:
				return this.Lmb.CreateAnyRealType();
			case global::\u000F.\u0003.\u0017:
				return this.Lmb.CreateAnyNumType();
			case global::\u000F.\u0003.\u0018:
				return this.Lmb.CreateAnyDateType();
			case global::\u000F.\u0003.\u0019:
				return this.Lmb.CreateDateType();
			case global::\u000F.\u0003.\u001A:
				return this.Lmb.CreateDateAndTimeType();
			case global::\u000F.\u0003.\u001B:
				return this.Lmb.CreateLTimeType();
			case global::\u000F.\u0003.\u001C:
				return this.Lmb.CreateXWordType();
			case global::\u000F.\u0003.\u001D:
				return this.Lmb.CreateXDWordType();
			case global::\u000F.\u0003.\u001E:
				return this.Lmb.CreateXLWordType();
			case global::\u000F.\u0003.\u001F:
				return this.Lmb.CreateXULIntType();
			case global::\u000F.\u0003.\u007F:
				return this.Lmb.CreateUXIntType();
			case global::\u000F.\u0003.\u0080:
				return this.Lmb.CreateVariableLengthArrayType();
			case global::\u000F.\u0003.\u0081:
				return this.Lmb.CreateAnyStringType();
			case global::\u000F.\u0003.\u0082:
				return this.Lmb.CreateXStringType();
			case global::\u000F.\u0003.\u0083:
				return this.Lmb.CreateXDIntType();
			case global::\u000F.\u0003.\u0084:
				return this.Lmb.CreateXLIntType();
			case global::\u000F.\u0003.\u0086:
				return this.Lmb.CreateXUDIntType();
			case global::\u000F.\u0003.\u0087:
				return this.Lmb.CreateXIntType();
			case global::\u000F.\u0003.\u0088:
				return this.Lmb.CreateTimeType();
			case global::\u000F.\u0003.\u0089:
				return this.Lmb.CreateTimeOfDayType();
			case global::\u000F.\u0003.\u008A:
				return this.Lmb.CreateAnyBitButBoolIsPreferred();
			case global::\u000F.\u0003.\u008B:
				return this.Lmb.CreateAnyBitType();
			case global::\u000F.\u0003.\u008C:
				return this.Lmb.CreateAnyIntType();
			case global::\u000F.\u0003.\u008D:
				return this.Lmb.CreateAnyType();
			case global::\u000F.\u0003.\u008E:
				return this.Lmb.CreateStringType();
			case global::\u000F.\u0003.\u008F:
				return this.Lmb.CreateArrayType();
			case global::\u000F.\u0003.\u0090:
				return this.Lmb.CreateImplicitEnumerationType();
			case global::\u000F.\u0003.\u0091:
				return this.Lmb.CreateSubrangeType();
			case global::\u000F.\u0003.\u0092:
				return this.Lmb.CreatePointerType();
			case global::\u000F.\u0003.\u0093:
				return this.Lmb.CreateLazyType();
			case global::\u000F.\u0003.\u0094:
				return this.Lmb.CreateRealType();
			case global::\u000F.\u0003.\u0095:
				return this.Lmb.CreateULIntType();
			case global::\u000F.\u0003.\u0096:
				return this.Lmb.CreateDWordType();
			case global::\u000F.\u0003.\u0097:
				return this.Lmb.CreateDIntType();
			case global::\u000F.\u0003.\u0098:
				return this.Lmb.CreateUIntType();
			case global::\u000F.\u0003.\u0099:
				return this.Lmb.CreateUSIntType();
			case global::\u000F.\u0003.\u009A:
				return this.Lmb.CreateByteType();
			case global::\u000F.\u0003.\u009B:
				return this.Lmb.CreateBitType();
			case global::\u000F.\u0003.\u009C:
				return this.Lmb.CreateBitConstType();
			case global::\u000F.\u0003.\u009D:
				return this.Lmb.CreateLDateType();
			case global::\u000F.\u0003.\u009E:
				return this.Lmb.CreateLTimeOfDayType();
			case global::\u000F.\u0003.\u009F:
				return this.Lmb.CreateLDateAndTimeType();
			case global::\u000F.\u0003.\u0001\u0002:
				return this.Lmb.CreateSafeBoolType();
			case global::\u000F.\u0003.\u0002\u0002:
				return this.Lmb.CreateSafeSIntType();
			case global::\u000F.\u0003.\u0003\u0002:
				return this.Lmb.CreateSafeIntType();
			case global::\u000F.\u0003.\u0004\u0002:
				return this.Lmb.CreateSafeWordType();
			case global::\u000F.\u0003.\u0005\u0002:
				return this.Lmb.CreateSafeUDIntType();
			case global::\u000F.\u0003.\u0006\u0002:
				return this.Lmb.CreateSafeLIntType();
			case global::\u000F.\u0003.\u0007\u0002:
				return this.Lmb.CreateSafeLWordType();
			case global::\u000F.\u0003.\u0008\u0002:
				return this.Lmb.CreateSafeLRealType();
			case global::\u000F.\u0003.\u000E\u0002:
				return this.Lmb.CreateSafeTimeType();
			case global::\u000F.\u0003.\u000F\u0002:
				return this.Lmb.CreateSafeRealType();
			case global::\u000F.\u0003.\u0010\u0002:
				return this.Lmb.CreateSafeULIntType();
			case global::\u000F.\u0003.\u0011\u0002:
				return this.Lmb.CreateSafeDWordType();
			case global::\u000F.\u0003.\u0012\u0002:
				return this.Lmb.CreateSafeDIntType();
			case global::\u000F.\u0003.\u0013\u0002:
				return this.Lmb.CreateSafeUIntType();
			case global::\u000F.\u0003.\u0014\u0002:
				return this.Lmb.CreateSafeUSIntType();
			case global::\u000F.\u0003.\u0015\u0002:
				return this.Lmb.CreateSafeByteType();
			default:
				Debug.\u0001(false);
				return null;
			}
		}

		// Token: 0x06000AFD RID: 2813 RVA: 0x0001866C File Offset: 0x0001686C
		private void \u0001(ref _IArrayType \u0002)
		{
			int num = this.Reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				_IExpression expLower = this.ExpressionDeserializer.\u0002<_IExpression>(this.Reader);
				_IExpression expUpper = this.ExpressionDeserializer.\u0002<_IExpression>(this.Reader);
				\u0002.AddDimension(expLower, expUpper);
			}
		}

		// Token: 0x06000AFE RID: 2814 RVA: 0x000186C0 File Offset: 0x000168C0
		public void \u0001(_IBoolType \u0002)
		{
		}

		// Token: 0x06000AFF RID: 2815 RVA: 0x000186C4 File Offset: 0x000168C4
		public void \u0001(_ISIntType \u0002)
		{
		}

		// Token: 0x06000B00 RID: 2816 RVA: 0x000186C8 File Offset: 0x000168C8
		public void \u0001(_IIntType \u0002)
		{
		}

		// Token: 0x06000B01 RID: 2817 RVA: 0x000186CC File Offset: 0x000168CC
		public void \u0001(_IWordType \u0002)
		{
		}

		// Token: 0x06000B02 RID: 2818 RVA: 0x000186D0 File Offset: 0x000168D0
		public void \u0001(_IUDIntType \u0002)
		{
		}

		// Token: 0x06000B03 RID: 2819 RVA: 0x000186D4 File Offset: 0x000168D4
		public void \u0001(_ILIntType \u0002)
		{
		}

		// Token: 0x06000B04 RID: 2820 RVA: 0x000186D8 File Offset: 0x000168D8
		public void \u0001(_ILWordType \u0002)
		{
		}

		// Token: 0x06000B05 RID: 2821 RVA: 0x000186DC File Offset: 0x000168DC
		public void \u0001(_ILRealType \u0002)
		{
		}

		// Token: 0x06000B06 RID: 2822 RVA: 0x000186E0 File Offset: 0x000168E0
		public virtual void \u0001(_IUserdefType \u0002)
		{
			\u0002.SignatureId = this.Reader.ReadInt32();
			\u0002.ScopeId = this.Reader.ReadInt32();
			\u0002.NameExpression = this.ExpressionDeserializer.\u0002<IExpression>(this.Reader);
		}

		// Token: 0x06000B07 RID: 2823 RVA: 0x0001871C File Offset: 0x0001691C
		public virtual void \u0001(IGenericUserdefType \u0002)
		{
			\u0002.SignatureId = this.Reader.ReadInt32();
			\u0002.ScopeId = this.Reader.ReadInt32();
			\u0002.NameExpression = this.ExpressionDeserializer.\u0002<IExpression>(this.Reader);
			foreach (_IExpression exp in \u001B.\u0001.\u0002<_IExpression>(this.Reader, new Func<BinaryReader, _IExpression>(this.ExpressionDeserializer.\u0002<_IExpression>)))
			{
				\u0002.AddGenericConstantInitialization(exp);
			}
		}

		// Token: 0x06000B08 RID: 2824 RVA: 0x000187B8 File Offset: 0x000169B8
		public virtual void \u0001(_IAliasType \u0002)
		{
			\u0002.SignatureId = this.Reader.ReadInt32();
			\u0002.ScopeId = this.Reader.ReadInt32();
			\u0002.NameExpression = this.ExpressionDeserializer.\u0002<IExpression>(this.Reader);
			((IAliasTypeSerializable)\u0002).SetEffectiveType(this.\u0002());
			\u0002.IsCompiled = this.Reader.ReadBoolean();
			\u0002._DefaultValue = this.ExpressionDeserializer.\u0002<_IExpression>(this.Reader);
		}

		// Token: 0x06000B09 RID: 2825 RVA: 0x00018838 File Offset: 0x00016A38
		public void \u0001(_IReferenceType \u0002)
		{
			\u0002._Base = this.\u0002();
		}

		// Token: 0x06000B0A RID: 2826 RVA: 0x00018848 File Offset: 0x00016A48
		public virtual void \u0001(_IEnumType \u0002)
		{
			\u0002.SignatureId = this.Reader.ReadInt32();
			((IEnumTypeSerializable)\u0002).Name = this.Reader.ReadString();
			\u0002._Base = this.\u0002();
			\u0002._DefaultValue = this.ExpressionDeserializer.\u0002<_IVariableExpression>(this.Reader);
			\u0002.AfterDeserialize();
		}

		// Token: 0x06000B0B RID: 2827 RVA: 0x000188A8 File Offset: 0x00016AA8
		public void \u0001(_IParamsType \u0002)
		{
			\u0002._Base = this.\u0002();
			\u0002.Count = this.ExpressionDeserializer.\u0002<_IExpression>(this.Reader);
		}

		// Token: 0x06000B0C RID: 2828 RVA: 0x000188D0 File Offset: 0x00016AD0
		public void \u0001(_IVectorType \u0002)
		{
			\u0002._Dimension = this.ExpressionDeserializer.\u0002<_IExpression>(this.Reader);
			\u0002._Base = this.\u0002();
		}

		// Token: 0x06000B0D RID: 2829 RVA: 0x000188F8 File Offset: 0x00016AF8
		public void \u0001(_IWStringType \u0002)
		{
			\u0002.Length = this.ExpressionDeserializer.\u0002<_IExpression>(this.Reader);
		}

		// Token: 0x06000B0E RID: 2830 RVA: 0x00018914 File Offset: 0x00016B14
		public void \u0001(_IAnyRealType \u0002)
		{
		}

		// Token: 0x06000B0F RID: 2831 RVA: 0x00018918 File Offset: 0x00016B18
		public void \u0001(_IAnyNumType \u0002)
		{
		}

		// Token: 0x06000B10 RID: 2832 RVA: 0x0001891C File Offset: 0x00016B1C
		public void \u0001(_IAnyDateType \u0002)
		{
		}

		// Token: 0x06000B11 RID: 2833 RVA: 0x00018920 File Offset: 0x00016B20
		public void \u0001(_IDateType \u0002)
		{
		}

		// Token: 0x06000B12 RID: 2834 RVA: 0x00018924 File Offset: 0x00016B24
		public void \u0001(_IDateAndTimeType \u0002)
		{
		}

		// Token: 0x06000B13 RID: 2835 RVA: 0x00018928 File Offset: 0x00016B28
		public void \u0001(_ILTimeType \u0002)
		{
		}

		// Token: 0x06000B14 RID: 2836 RVA: 0x0001892C File Offset: 0x00016B2C
		public void \u0001(_IXWordType \u0002)
		{
		}

		// Token: 0x06000B15 RID: 2837 RVA: 0x00018930 File Offset: 0x00016B30
		public void \u0001(_IXDWordType \u0002)
		{
		}

		// Token: 0x06000B16 RID: 2838 RVA: 0x00018934 File Offset: 0x00016B34
		public void \u0001(_IXLWordType \u0002)
		{
		}

		// Token: 0x06000B17 RID: 2839 RVA: 0x00018938 File Offset: 0x00016B38
		public void \u0001(_IXULIntType \u0002)
		{
		}

		// Token: 0x06000B18 RID: 2840 RVA: 0x0001893C File Offset: 0x00016B3C
		public void \u0001(_IUXIntType \u0002)
		{
		}

		// Token: 0x06000B19 RID: 2841 RVA: 0x00018940 File Offset: 0x00016B40
		public void \u0001(_IVariableLengthArrayType \u0002)
		{
			\u0002._Base = this.\u0002();
			\u0002.Dimensions = this.Reader.ReadInt32();
		}

		// Token: 0x06000B1A RID: 2842 RVA: 0x00018960 File Offset: 0x00016B60
		public void \u0001(_IAnyStringType \u0002)
		{
		}

		// Token: 0x06000B1B RID: 2843 RVA: 0x00018964 File Offset: 0x00016B64
		public void \u0001(_IXStringType \u0002)
		{
			\u0002.Length = this.ExpressionDeserializer.\u0002<_IExpression>(this.Reader);
		}

		// Token: 0x06000B1C RID: 2844 RVA: 0x00018980 File Offset: 0x00016B80
		public void \u0001(_IXLIntType \u0002)
		{
		}

		// Token: 0x06000B1D RID: 2845 RVA: 0x00018984 File Offset: 0x00016B84
		public void \u0001(_IXUDIntType \u0002)
		{
		}

		// Token: 0x06000B1E RID: 2846 RVA: 0x00018988 File Offset: 0x00016B88
		public void \u0001(_IXIntType \u0002)
		{
		}

		// Token: 0x06000B1F RID: 2847 RVA: 0x0001898C File Offset: 0x00016B8C
		public void \u0001(_IXDIntType \u0002)
		{
		}

		// Token: 0x06000B20 RID: 2848 RVA: 0x00018990 File Offset: 0x00016B90
		public void \u0001(_ITimeType \u0002)
		{
		}

		// Token: 0x06000B21 RID: 2849 RVA: 0x00018994 File Offset: 0x00016B94
		public void \u0001(_ITimeOfDayType \u0002)
		{
		}

		// Token: 0x06000B22 RID: 2850 RVA: 0x00018998 File Offset: 0x00016B98
		public void \u0001(_IAnyBitButBoolIsPreferred \u0002)
		{
		}

		// Token: 0x06000B23 RID: 2851 RVA: 0x0001899C File Offset: 0x00016B9C
		public void \u0001(_IAnyBitType \u0002)
		{
		}

		// Token: 0x06000B24 RID: 2852 RVA: 0x000189A0 File Offset: 0x00016BA0
		public void \u0001(_IAnyIntType \u0002)
		{
		}

		// Token: 0x06000B25 RID: 2853 RVA: 0x000189A4 File Offset: 0x00016BA4
		public void \u0001(_IAnyType \u0002)
		{
		}

		// Token: 0x06000B26 RID: 2854 RVA: 0x000189A8 File Offset: 0x00016BA8
		public void \u0001(_IStringType \u0002)
		{
			\u0002.Length = this.ExpressionDeserializer.\u0002<_IExpression>(this.Reader);
		}

		// Token: 0x06000B27 RID: 2855 RVA: 0x000189C4 File Offset: 0x00016BC4
		public void \u0001(_IArrayType \u0002)
		{
			this.\u0001(ref \u0002);
			\u0002._Base = this.\u0002();
		}

		// Token: 0x06000B28 RID: 2856 RVA: 0x000189DC File Offset: 0x00016BDC
		public void \u0001(IImplicitEnumerationType \u0002)
		{
		}

		// Token: 0x06000B29 RID: 2857 RVA: 0x000189E0 File Offset: 0x00016BE0
		public void \u0001(_ISubrangeType \u0002)
		{
			\u0002._LowerBorder = this.ExpressionDeserializer.\u0002<_IExpression>(this.Reader);
			\u0002._UpperBorder = this.ExpressionDeserializer.\u0002<_IExpression>(this.Reader);
			\u0002._Base = this.\u0002();
		}

		// Token: 0x06000B2A RID: 2858 RVA: 0x00018A1C File Offset: 0x00016C1C
		public void \u0001(_IPointerType \u0002)
		{
			\u0002._Base = this.\u0002();
		}

		// Token: 0x06000B2B RID: 2859 RVA: 0x00018A2C File Offset: 0x00016C2C
		public void \u0001(_ILazyType \u0002)
		{
		}

		// Token: 0x06000B2C RID: 2860 RVA: 0x00018A30 File Offset: 0x00016C30
		public void \u0001(_IRealType \u0002)
		{
		}

		// Token: 0x06000B2D RID: 2861 RVA: 0x00018A34 File Offset: 0x00016C34
		public void \u0001(_IULIntType \u0002)
		{
		}

		// Token: 0x06000B2E RID: 2862 RVA: 0x00018A38 File Offset: 0x00016C38
		public void \u0001(_IDWordType \u0002)
		{
		}

		// Token: 0x06000B2F RID: 2863 RVA: 0x00018A3C File Offset: 0x00016C3C
		public void \u0001(_IDIntType \u0002)
		{
		}

		// Token: 0x06000B30 RID: 2864 RVA: 0x00018A40 File Offset: 0x00016C40
		public void \u0001(_IUIntType \u0002)
		{
		}

		// Token: 0x06000B31 RID: 2865 RVA: 0x00018A44 File Offset: 0x00016C44
		public void \u0001(_IUSIntType \u0002)
		{
		}

		// Token: 0x06000B32 RID: 2866 RVA: 0x00018A48 File Offset: 0x00016C48
		public void \u0001(_IByteType \u0002)
		{
		}

		// Token: 0x06000B33 RID: 2867 RVA: 0x00018A4C File Offset: 0x00016C4C
		public void \u0001(_IBitType \u0002)
		{
		}

		// Token: 0x06000B34 RID: 2868 RVA: 0x00018A50 File Offset: 0x00016C50
		public void \u0001(_IBitConstType \u0002)
		{
		}

		// Token: 0x06000B35 RID: 2869 RVA: 0x00018A54 File Offset: 0x00016C54
		public void \u0001(_ILDateType \u0002)
		{
		}

		// Token: 0x06000B36 RID: 2870 RVA: 0x00018A58 File Offset: 0x00016C58
		public void \u0001(_ILTimeOfDayType \u0002)
		{
		}

		// Token: 0x06000B37 RID: 2871 RVA: 0x00018A5C File Offset: 0x00016C5C
		public void \u0001(_ILDateAndTimeType \u0002)
		{
		}

		// Token: 0x04000151 RID: 337
		[CompilerGenerated]
		private readonly BinaryReader \u0001;

		// Token: 0x04000152 RID: 338
		[CompilerGenerated]
		private readonly ILMSerializableTypeFactory2 \u0001;

		// Token: 0x04000153 RID: 339
		[CompilerGenerated]
		private readonly \u001E.\u0003 \u0001;
	}
}
