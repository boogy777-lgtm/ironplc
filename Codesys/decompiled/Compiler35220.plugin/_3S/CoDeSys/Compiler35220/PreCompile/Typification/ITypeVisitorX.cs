using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile.Typification
{
	// Token: 0x02000191 RID: 401
	public interface ITypeVisitorX<out T>
	{
		// Token: 0x06001C2E RID: 7214
		T visit(_IBitConstType type);

		// Token: 0x06001C2F RID: 7215
		T visit(_IBitType type);

		// Token: 0x06001C30 RID: 7216
		T visit(_IBoolType type);

		// Token: 0x06001C31 RID: 7217
		T visit(_IByteType type);

		// Token: 0x06001C32 RID: 7218
		T visit(_ISIntType type);

		// Token: 0x06001C33 RID: 7219
		T visit(_IUSIntType type);

		// Token: 0x06001C34 RID: 7220
		T visit(_IIntType type);

		// Token: 0x06001C35 RID: 7221
		T visit(_IUIntType type);

		// Token: 0x06001C36 RID: 7222
		T visit(_IWordType type);

		// Token: 0x06001C37 RID: 7223
		T visit(_IDIntType type);

		// Token: 0x06001C38 RID: 7224
		T visit(_IUDIntType type);

		// Token: 0x06001C39 RID: 7225
		T visit(_IDWordType type);

		// Token: 0x06001C3A RID: 7226
		T visit(_ILIntType type);

		// Token: 0x06001C3B RID: 7227
		T visit(_IULIntType type);

		// Token: 0x06001C3C RID: 7228
		T visit(_ILWordType type);

		// Token: 0x06001C3D RID: 7229
		T visit(_IRealType type);

		// Token: 0x06001C3E RID: 7230
		T visit(_ILRealType type);

		// Token: 0x06001C3F RID: 7231
		T visit(_ILazyType type);

		// Token: 0x06001C40 RID: 7232
		T visit(_IUserdefType type);

		// Token: 0x06001C41 RID: 7233
		T visit(_IPointerType type);

		// Token: 0x06001C42 RID: 7234
		T visit(_IReferenceType type);

		// Token: 0x06001C43 RID: 7235
		T visit(_ISubrangeType type);

		// Token: 0x06001C44 RID: 7236
		T visit(_IEnumType type);

		// Token: 0x06001C45 RID: 7237
		T visit(IImplicitEnumerationType type);

		// Token: 0x06001C46 RID: 7238
		T visit(_IParamsType type);

		// Token: 0x06001C47 RID: 7239
		T visit(_IArrayType type);

		// Token: 0x06001C48 RID: 7240
		T visit(_IVectorType type);

		// Token: 0x06001C49 RID: 7241
		T visit(_IStringType type);

		// Token: 0x06001C4A RID: 7242
		T visit(_IWStringType type);

		// Token: 0x06001C4B RID: 7243
		T visit(_IAnyType type);

		// Token: 0x06001C4C RID: 7244
		T visit(_IAnyRealType type);

		// Token: 0x06001C4D RID: 7245
		T visit(_IAnyIntType type);

		// Token: 0x06001C4E RID: 7246
		T visit(_IAnyNumType type);

		// Token: 0x06001C4F RID: 7247
		T visit(_IAnyBitType type);

		// Token: 0x06001C50 RID: 7248
		T visit(_IAnyDateType type);

		// Token: 0x06001C51 RID: 7249
		T visit(_IAnyBitButBoolIsPreferred type);

		// Token: 0x06001C52 RID: 7250
		T visit(_IDateType type);

		// Token: 0x06001C53 RID: 7251
		T visit(_ITimeOfDayType type);

		// Token: 0x06001C54 RID: 7252
		T visit(_IDateAndTimeType type);

		// Token: 0x06001C55 RID: 7253
		T visit(_ITimeType type);

		// Token: 0x06001C56 RID: 7254
		T visit(_ILTimeType type);

		// Token: 0x06001C57 RID: 7255
		T visit(_IXIntType type);

		// Token: 0x06001C58 RID: 7256
		T visit(_IXWordType type);

		// Token: 0x06001C59 RID: 7257
		T visit(_IXUDIntType type);

		// Token: 0x06001C5A RID: 7258
		T visit(_IXULIntType type);

		// Token: 0x06001C5B RID: 7259
		T visit(_IXLIntType type);

		// Token: 0x06001C5C RID: 7260
		T visit(_IUXIntType type);

		// Token: 0x06001C5D RID: 7261
		T visit(_IXStringType type);

		// Token: 0x06001C5E RID: 7262
		T visit(_IVariableLengthArrayType type);

		// Token: 0x06001C5F RID: 7263
		T visit(_IAnyStringType type);

		// Token: 0x06001C60 RID: 7264
		T visit(_ILDateType type);

		// Token: 0x06001C61 RID: 7265
		T visit(_ILTimeOfDayType type);

		// Token: 0x06001C62 RID: 7266
		T visit(_ILDateAndTimeType type);

		// Token: 0x06001C63 RID: 7267
		T visit(_IAliasType type);

		// Token: 0x06001C64 RID: 7268
		T visit(_IXDIntType type);

		// Token: 0x06001C65 RID: 7269
		T visit(_IXDWordType type);

		// Token: 0x06001C66 RID: 7270
		T visit(_IXLWordType type);

		// Token: 0x06001C67 RID: 7271
		T visit(IGenericUserdefType type);
	}
}
