using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using \u000F;
using _3S.CoDeSys.Compiler35220.Serialization;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0082
{
	// Token: 0x02000083 RID: 131
	internal class \u0001 : ITypeVisitor3, ITypeVisitor2, ITypeVisitor, ITypeVisitor4
	{
		// Token: 0x1700041F RID: 1055
		// (get) Token: 0x06000B3D RID: 2877 RVA: 0x00018BB0 File Offset: 0x00016DB0
		internal static \u0001 Instance { get; } = new \u0001();

		// Token: 0x17000420 RID: 1056
		// (get) Token: 0x06000B3F RID: 2879 RVA: 0x00018BC4 File Offset: 0x00016DC4
		// (set) Token: 0x06000B3E RID: 2878 RVA: 0x00018BB8 File Offset: 0x00016DB8
		protected BinaryWriter Writer { get; set; }

		// Token: 0x06000B40 RID: 2880 RVA: 0x00018BCC File Offset: 0x00016DCC
		protected \u0001()
		{
		}

		// Token: 0x06000B41 RID: 2881 RVA: 0x00018BD4 File Offset: 0x00016DD4
		public void \u0001(BinaryWriter \u0002, _IType \u0003)
		{
			this.Writer = \u0002;
			this.\u0001(\u0003);
		}

		// Token: 0x06000B42 RID: 2882 RVA: 0x00018BE4 File Offset: 0x00016DE4
		protected void \u0001(\u0003 \u0002)
		{
			this.Writer.Write((ushort)\u0002);
		}

		// Token: 0x06000B43 RID: 2883 RVA: 0x00018BF4 File Offset: 0x00016DF4
		protected void \u0001(_IType \u0002)
		{
			if (this.\u0001(\u0002))
			{
				return;
			}
			\u0002.Accept(this);
		}

		// Token: 0x06000B44 RID: 2884 RVA: 0x00018C08 File Offset: 0x00016E08
		private bool \u0001(_IType \u0002)
		{
			if (!(\u0002 is _ISafeBoolType))
			{
				if (!(\u0002 is _ISafeSIntType))
				{
					if (!(\u0002 is _ISafeIntType))
					{
						if (!(\u0002 is _ISafeWordType))
						{
							if (!(\u0002 is _ISafeUDIntType))
							{
								if (!(\u0002 is _ISafeLIntType))
								{
									if (!(\u0002 is _ISafeLWordType))
									{
										if (!(\u0002 is _ISafeLRealType))
										{
											if (!(\u0002 is _ISafeTimeType))
											{
												if (!(\u0002 is _ISafeRealType))
												{
													if (!(\u0002 is _ISafeULIntType))
													{
														if (!(\u0002 is _ISafeDWordType))
														{
															if (!(\u0002 is _ISafeDIntType))
															{
																if (!(\u0002 is _ISafeUIntType))
																{
																	if (!(\u0002 is _ISafeUSIntType))
																	{
																		if (!(\u0002 is _ISafeByteType))
																		{
																			return false;
																		}
																		this.\u0001(\u0003.\u0015\u0002);
																	}
																	else
																	{
																		this.\u0001(\u0003.\u0014\u0002);
																	}
																}
																else
																{
																	this.\u0001(\u0003.\u0013\u0002);
																}
															}
															else
															{
																this.\u0001(\u0003.\u0012\u0002);
															}
														}
														else
														{
															this.\u0001(\u0003.\u0011\u0002);
														}
													}
													else
													{
														this.\u0001(\u0003.\u0010\u0002);
													}
												}
												else
												{
													this.\u0001(\u0003.\u000F\u0002);
												}
											}
											else
											{
												this.\u0001(\u0003.\u000E\u0002);
											}
										}
										else
										{
											this.\u0001(\u0003.\u0008\u0002);
										}
									}
									else
									{
										this.\u0001(\u0003.\u0007\u0002);
									}
								}
								else
								{
									this.\u0001(\u0003.\u0006\u0002);
								}
							}
							else
							{
								this.\u0001(\u0003.\u0005\u0002);
							}
						}
						else
						{
							this.\u0001(\u0003.\u0004\u0002);
						}
					}
					else
					{
						this.\u0001(\u0003.\u0003\u0002);
					}
				}
				else
				{
					this.\u0001(\u0003.\u0002\u0002);
				}
			}
			else
			{
				this.\u0001(\u0003.\u0001\u0002);
			}
			return true;
		}

		// Token: 0x06000B45 RID: 2885 RVA: 0x00018D78 File Offset: 0x00016F78
		private void \u0001(IList<_IArrayDimension> \u0002)
		{
			this.Writer.Write(\u0002.Count);
			for (int i = 0; i < \u0002.Count; i++)
			{
				this.\u0001(\u0002[i]);
			}
		}

		// Token: 0x06000B46 RID: 2886 RVA: 0x00018DB4 File Offset: 0x00016FB4
		private void \u0001(_IArrayDimension \u0002)
		{
			CompileContextSerializer.\u0001(this.Writer, \u0002._LowerBorder);
			CompileContextSerializer.\u0001(this.Writer, \u0002._UpperBorder);
		}

		// Token: 0x06000B47 RID: 2887 RVA: 0x00018DD8 File Offset: 0x00016FD8
		public void \u0001(_IBoolType \u0002)
		{
			this.\u0001(\u0003.\u0001);
		}

		// Token: 0x06000B48 RID: 2888 RVA: 0x00018DE4 File Offset: 0x00016FE4
		public void \u0001(_ISIntType \u0002)
		{
			this.\u0001(\u0003.\u0002);
		}

		// Token: 0x06000B49 RID: 2889 RVA: 0x00018DF0 File Offset: 0x00016FF0
		public void \u0001(_IIntType \u0002)
		{
			this.\u0001(\u0003.\u0003);
		}

		// Token: 0x06000B4A RID: 2890 RVA: 0x00018DFC File Offset: 0x00016FFC
		public void \u0001(_IWordType \u0002)
		{
			this.\u0001(\u0003.\u0004);
		}

		// Token: 0x06000B4B RID: 2891 RVA: 0x00018E08 File Offset: 0x00017008
		public void \u0001(_IUDIntType \u0002)
		{
			this.\u0001(\u0003.\u0005);
		}

		// Token: 0x06000B4C RID: 2892 RVA: 0x00018E14 File Offset: 0x00017014
		public void \u0001(_ILIntType \u0002)
		{
			this.\u0001(\u0003.\u0006);
		}

		// Token: 0x06000B4D RID: 2893 RVA: 0x00018E20 File Offset: 0x00017020
		public void \u0001(_ILWordType \u0002)
		{
			this.\u0001(\u0003.\u0007);
		}

		// Token: 0x06000B4E RID: 2894 RVA: 0x00018E2C File Offset: 0x0001702C
		public void \u0001(_ILRealType \u0002)
		{
			this.\u0001(\u0003.\u0008);
		}

		// Token: 0x06000B4F RID: 2895 RVA: 0x00018E38 File Offset: 0x00017038
		public virtual void \u0001(_IUserdefType \u0002)
		{
			this.\u0001(\u0003.\u000E);
			this.Writer.Write(\u0002.SignatureId);
			this.Writer.Write(\u0002.ScopeId);
			CompileContextSerializer.\u0001(this.Writer, (_IExpression)\u0002.NameExpression);
		}

		// Token: 0x06000B50 RID: 2896 RVA: 0x00018E84 File Offset: 0x00017084
		public virtual void \u0001(IGenericUserdefType \u0002)
		{
			this.\u0001(\u0003.\u000F);
			this.Writer.Write(\u0002.SignatureId);
			this.Writer.Write(\u0002.ScopeId);
			CompileContextSerializer.\u0001(this.Writer, (_IExpression)\u0002.NameExpression);
			CommonSerializer.SerializeList<_IExpression>(this.Writer, \u0002.GenericConstantsInitializations, new Action<BinaryWriter, _IExpression>(CompileContextSerializer.\u0001));
		}

		// Token: 0x06000B51 RID: 2897 RVA: 0x00018EF0 File Offset: 0x000170F0
		public virtual void \u0001(_IAliasType \u0002)
		{
			\u0002.BeforeSerialize();
			this.\u0001(\u0003.\u0010);
			this.Writer.Write(\u0002.SignatureId);
			this.Writer.Write(\u0002.ScopeId);
			CompileContextSerializer.\u0001(this.Writer, (_IExpression)\u0002.NameExpression);
			this.\u0001(\u0002.EffectiveType as _IType);
			this.Writer.Write(\u0002.IsCompiled);
			CompileContextSerializer.\u0001(this.Writer, \u0002._DefaultValue);
		}

		// Token: 0x06000B52 RID: 2898 RVA: 0x00018F78 File Offset: 0x00017178
		public void \u0001(_IReferenceType \u0002)
		{
			this.\u0001(\u0003.\u0011);
			this.\u0001(\u0002.BaseType as _IType);
		}

		// Token: 0x06000B53 RID: 2899 RVA: 0x00018F94 File Offset: 0x00017194
		public virtual void \u0001(_IEnumType \u0002)
		{
			\u0002.BeforeSerialize();
			this.\u0001(\u0003.\u0012);
			this.Writer.Write(\u0002.SignatureId);
			this.Writer.Write(\u0002.Name);
			this.\u0001(\u0002._Base);
			CompileContextSerializer.\u0001(this.Writer, \u0002._DefaultValue);
		}

		// Token: 0x06000B54 RID: 2900 RVA: 0x00018FF0 File Offset: 0x000171F0
		public void \u0001(_IParamsType \u0002)
		{
			this.\u0001(\u0003.\u0013);
			this.\u0001(\u0002._Base);
			CompileContextSerializer.\u0001(this.Writer, \u0002.Count);
		}

		// Token: 0x06000B55 RID: 2901 RVA: 0x00019018 File Offset: 0x00017218
		public void \u0001(_IVectorType \u0002)
		{
			this.\u0001(\u0003.\u0014);
			CompileContextSerializer.\u0001(this.Writer, \u0002._Dimension);
			this.\u0001(\u0002._Base);
		}

		// Token: 0x06000B56 RID: 2902 RVA: 0x00019040 File Offset: 0x00017240
		public void \u0001(_IWStringType \u0002)
		{
			this.\u0001(\u0003.\u0015);
			CompileContextSerializer.\u0001(this.Writer, \u0002.Length);
		}

		// Token: 0x06000B57 RID: 2903 RVA: 0x0001905C File Offset: 0x0001725C
		public void \u0001(_IAnyRealType \u0002)
		{
			this.\u0001(\u0003.\u0016);
		}

		// Token: 0x06000B58 RID: 2904 RVA: 0x00019068 File Offset: 0x00017268
		public void \u0001(_IAnyNumType \u0002)
		{
			this.\u0001(\u0003.\u0017);
		}

		// Token: 0x06000B59 RID: 2905 RVA: 0x00019074 File Offset: 0x00017274
		public void \u0001(_IAnyDateType \u0002)
		{
			this.\u0001(\u0003.\u0018);
		}

		// Token: 0x06000B5A RID: 2906 RVA: 0x00019080 File Offset: 0x00017280
		public void \u0001(_IDateType \u0002)
		{
			this.\u0001(\u0003.\u0019);
		}

		// Token: 0x06000B5B RID: 2907 RVA: 0x0001908C File Offset: 0x0001728C
		public void \u0001(_IDateAndTimeType \u0002)
		{
			this.\u0001(\u0003.\u001A);
		}

		// Token: 0x06000B5C RID: 2908 RVA: 0x00019098 File Offset: 0x00017298
		public void \u0001(_ILTimeType \u0002)
		{
			this.\u0001(\u0003.\u001B);
		}

		// Token: 0x06000B5D RID: 2909 RVA: 0x000190A4 File Offset: 0x000172A4
		public void \u0001(_IXWordType \u0002)
		{
			this.\u0001(\u0003.\u001C);
		}

		// Token: 0x06000B5E RID: 2910 RVA: 0x000190B0 File Offset: 0x000172B0
		public void \u0001(_IXDWordType \u0002)
		{
			this.\u0001(\u0003.\u001D);
		}

		// Token: 0x06000B5F RID: 2911 RVA: 0x000190BC File Offset: 0x000172BC
		public void \u0001(_IXLWordType \u0002)
		{
			this.\u0001(\u0003.\u001E);
		}

		// Token: 0x06000B60 RID: 2912 RVA: 0x000190C8 File Offset: 0x000172C8
		public void \u0001(_IXIntType \u0002)
		{
			this.\u0001(\u0003.\u0087);
		}

		// Token: 0x06000B61 RID: 2913 RVA: 0x000190D4 File Offset: 0x000172D4
		public void \u0001(_IXDIntType \u0002)
		{
			this.\u0001(\u0003.\u0083);
		}

		// Token: 0x06000B62 RID: 2914 RVA: 0x000190E0 File Offset: 0x000172E0
		public void \u0001(_IXLIntType \u0002)
		{
			this.\u0001(\u0003.\u0084);
		}

		// Token: 0x06000B63 RID: 2915 RVA: 0x000190EC File Offset: 0x000172EC
		public void \u0001(_IUXIntType \u0002)
		{
			this.\u0001(\u0003.\u007F);
		}

		// Token: 0x06000B64 RID: 2916 RVA: 0x000190F8 File Offset: 0x000172F8
		public void \u0001(_IXUDIntType \u0002)
		{
			this.\u0001(\u0003.\u0086);
		}

		// Token: 0x06000B65 RID: 2917 RVA: 0x00019104 File Offset: 0x00017304
		public void \u0001(_IXULIntType \u0002)
		{
			this.\u0001(\u0003.\u001F);
		}

		// Token: 0x06000B66 RID: 2918 RVA: 0x00019110 File Offset: 0x00017310
		public void \u0001(_IVariableLengthArrayType \u0002)
		{
			this.\u0001(\u0003.\u0080);
			this.\u0001(\u0002._Base);
			this.Writer.Write(\u0002.Dimensions);
		}

		// Token: 0x06000B67 RID: 2919 RVA: 0x00019138 File Offset: 0x00017338
		public void \u0001(_IAnyStringType \u0002)
		{
			this.\u0001(\u0003.\u0081);
		}

		// Token: 0x06000B68 RID: 2920 RVA: 0x00019144 File Offset: 0x00017344
		public void \u0001(_IXStringType \u0002)
		{
			this.\u0001(\u0003.\u0082);
			CompileContextSerializer.\u0001(this.Writer, \u0002.Length);
		}

		// Token: 0x06000B69 RID: 2921 RVA: 0x00019160 File Offset: 0x00017360
		public void \u0001(_ITimeType \u0002)
		{
			this.\u0001(\u0003.\u0088);
		}

		// Token: 0x06000B6A RID: 2922 RVA: 0x0001916C File Offset: 0x0001736C
		public void \u0001(_ITimeOfDayType \u0002)
		{
			this.\u0001(\u0003.\u0089);
		}

		// Token: 0x06000B6B RID: 2923 RVA: 0x00019178 File Offset: 0x00017378
		public void \u0001(_IAnyBitButBoolIsPreferred \u0002)
		{
			this.\u0001(\u0003.\u008A);
		}

		// Token: 0x06000B6C RID: 2924 RVA: 0x00019184 File Offset: 0x00017384
		public void \u0001(_IAnyBitType \u0002)
		{
			this.\u0001(\u0003.\u008B);
		}

		// Token: 0x06000B6D RID: 2925 RVA: 0x00019190 File Offset: 0x00017390
		public void \u0001(_IAnyIntType \u0002)
		{
			this.\u0001(\u0003.\u008C);
		}

		// Token: 0x06000B6E RID: 2926 RVA: 0x0001919C File Offset: 0x0001739C
		public void \u0001(_IAnyType \u0002)
		{
			this.\u0001(\u0003.\u008D);
		}

		// Token: 0x06000B6F RID: 2927 RVA: 0x000191A8 File Offset: 0x000173A8
		public void \u0001(_IStringType \u0002)
		{
			this.\u0001(\u0003.\u008E);
			CompileContextSerializer.\u0001(this.Writer, \u0002.Length);
		}

		// Token: 0x06000B70 RID: 2928 RVA: 0x000191C4 File Offset: 0x000173C4
		public void \u0001(_IArrayType \u0002)
		{
			this.\u0001(\u0003.\u008F);
			this.\u0001(\u0002._Dimensions);
			this.\u0001(\u0002._Base);
		}

		// Token: 0x06000B71 RID: 2929 RVA: 0x000191E8 File Offset: 0x000173E8
		public void \u0001(IImplicitEnumerationType \u0002)
		{
			this.\u0001(\u0003.\u0090);
		}

		// Token: 0x06000B72 RID: 2930 RVA: 0x000191F4 File Offset: 0x000173F4
		public void \u0001(_ISubrangeType \u0002)
		{
			this.\u0001(\u0003.\u0091);
			CompileContextSerializer.\u0001(this.Writer, \u0002._LowerBorder);
			CompileContextSerializer.\u0001(this.Writer, \u0002._UpperBorder);
			this.\u0001(\u0002._Base);
		}

		// Token: 0x06000B73 RID: 2931 RVA: 0x0001922C File Offset: 0x0001742C
		public void \u0001(_IPointerType \u0002)
		{
			this.\u0001(\u0003.\u0092);
			this.\u0001(\u0002.BaseType as _IType);
		}

		// Token: 0x06000B74 RID: 2932 RVA: 0x00019248 File Offset: 0x00017448
		public void \u0001(_ILazyType \u0002)
		{
			this.\u0001(\u0003.\u0093);
		}

		// Token: 0x06000B75 RID: 2933 RVA: 0x00019254 File Offset: 0x00017454
		public void \u0001(_IRealType \u0002)
		{
			this.\u0001(\u0003.\u0094);
		}

		// Token: 0x06000B76 RID: 2934 RVA: 0x00019260 File Offset: 0x00017460
		public void \u0001(_IULIntType \u0002)
		{
			this.\u0001(\u0003.\u0095);
		}

		// Token: 0x06000B77 RID: 2935 RVA: 0x0001926C File Offset: 0x0001746C
		public void \u0001(_IDWordType \u0002)
		{
			this.\u0001(\u0003.\u0096);
		}

		// Token: 0x06000B78 RID: 2936 RVA: 0x00019278 File Offset: 0x00017478
		public void \u0001(_IDIntType \u0002)
		{
			this.\u0001(\u0003.\u0097);
		}

		// Token: 0x06000B79 RID: 2937 RVA: 0x00019284 File Offset: 0x00017484
		public void \u0001(_IUIntType \u0002)
		{
			this.\u0001(\u0003.\u0098);
		}

		// Token: 0x06000B7A RID: 2938 RVA: 0x00019290 File Offset: 0x00017490
		public void \u0001(_IUSIntType \u0002)
		{
			this.\u0001(\u0003.\u0099);
		}

		// Token: 0x06000B7B RID: 2939 RVA: 0x0001929C File Offset: 0x0001749C
		public void \u0001(_IByteType \u0002)
		{
			this.\u0001(\u0003.\u009A);
		}

		// Token: 0x06000B7C RID: 2940 RVA: 0x000192A8 File Offset: 0x000174A8
		public void \u0001(_IBitType \u0002)
		{
			this.\u0001(\u0003.\u009B);
		}

		// Token: 0x06000B7D RID: 2941 RVA: 0x000192B4 File Offset: 0x000174B4
		public void \u0001(_IBitConstType \u0002)
		{
			this.\u0001(\u0003.\u009C);
		}

		// Token: 0x06000B7E RID: 2942 RVA: 0x000192C0 File Offset: 0x000174C0
		public void \u0001(_ILDateType \u0002)
		{
			this.\u0001(\u0003.\u009D);
		}

		// Token: 0x06000B7F RID: 2943 RVA: 0x000192CC File Offset: 0x000174CC
		public void \u0001(_ILTimeOfDayType \u0002)
		{
			this.\u0001(\u0003.\u009E);
		}

		// Token: 0x06000B80 RID: 2944 RVA: 0x000192D8 File Offset: 0x000174D8
		public void \u0001(_ILDateAndTimeType \u0002)
		{
			this.\u0001(\u0003.\u009F);
		}

		// Token: 0x04000154 RID: 340
		[CompilerGenerated]
		private static readonly \u0001 \u0001;

		// Token: 0x04000155 RID: 341
		[CompilerGenerated]
		private BinaryWriter \u0001;
	}
}
