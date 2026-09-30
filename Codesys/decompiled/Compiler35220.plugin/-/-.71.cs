using System;
using System.Collections.Generic;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0019
{
	// Token: 0x020000FD RID: 253
	internal static class \u0003
	{
		// Token: 0x170004AA RID: 1194
		// (get) Token: 0x060010F2 RID: 4338 RVA: 0x000318AC File Offset: 0x0002FAAC
		internal static _ILanguageModelBuilder7 Builder
		{
			get
			{
				if (\u0003.\u0001 == null)
				{
					\u0003.\u0001 = (APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder() as _ILanguageModelBuilder7);
				}
				return \u0003.\u0001;
			}
		}

		// Token: 0x060010F3 RID: 4339 RVA: 0x000318D4 File Offset: 0x0002FAD4
		internal static _ICompiledPOU \u0001(string \u0002)
		{
			return \u0003.Builder.CreateCompiledPOU(\u0002);
		}

		// Token: 0x060010F4 RID: 4340 RVA: 0x000318E4 File Offset: 0x0002FAE4
		internal static _ISignature \u0001()
		{
			return \u0003.Builder.CreateSignature();
		}

		// Token: 0x060010F5 RID: 4341 RVA: 0x000318F0 File Offset: 0x0002FAF0
		internal static _IVariable \u0001(ISourcePosition \u0002)
		{
			return \u0003.Builder.CreateVariable(\u0002);
		}

		// Token: 0x060010F6 RID: 4342 RVA: 0x00031900 File Offset: 0x0002FB00
		internal static IDataLocation2 \u0001(ushort \u0002, int \u0003)
		{
			return \u0003.Builder.CreateDataLocation(\u0002, \u0003);
		}

		// Token: 0x060010F7 RID: 4343 RVA: 0x00031910 File Offset: 0x0002FB10
		internal static IDataLocation2 \u0001(int \u0002)
		{
			return \u0003.Builder.CreateRelativeDataLocation(\u0002);
		}

		// Token: 0x060010F8 RID: 4344 RVA: 0x00031920 File Offset: 0x0002FB20
		internal static IDataLocation2 \u0001(int \u0002, DataLocationFlag \u0003)
		{
			return \u0003.Builder.CreateRelativeDataLocation(\u0002, \u0003);
		}

		// Token: 0x060010F9 RID: 4345 RVA: 0x00031930 File Offset: 0x0002FB30
		internal static IDataLocation2 \u0001(int \u0002, byte \u0003)
		{
			return \u0003.Builder.CreateRelativeBitDataLocation(\u0002, \u0003);
		}

		// Token: 0x060010FA RID: 4346 RVA: 0x00031940 File Offset: 0x0002FB40
		internal static IDataLocation2 \u0001(ushort \u0002, int \u0003, byte \u0004)
		{
			return \u0003.Builder.CreateBitDataLocation(\u0002, \u0003, \u0004);
		}

		// Token: 0x060010FB RID: 4347 RVA: 0x00031950 File Offset: 0x0002FB50
		internal static _IStepInPosition \u0001(int \u0002, IBreakpoint \u0003)
		{
			return \u0003.Builder.CreateStepInPosition(\u0002, \u0003);
		}

		// Token: 0x060010FC RID: 4348 RVA: 0x00031960 File Offset: 0x0002FB60
		internal static _IArea \u0001(DataSegmentFlags \u0002, AreaFlags \u0003)
		{
			return \u0003.Builder.CreateArea(\u0002, \u0003);
		}

		// Token: 0x060010FD RID: 4349 RVA: 0x00031970 File Offset: 0x0002FB70
		internal static IPersistentArea \u0001(IArea \u0002, uint \u0003)
		{
			return \u0003.Builder.CreateAreaPersistent(\u0002, \u0003);
		}

		// Token: 0x060010FE RID: 4350 RVA: 0x00031980 File Offset: 0x0002FB80
		internal static ICodePosition \u0001(ISourcePosition \u0002, AccessFlag \u0003)
		{
			return \u0003.Builder.CreateCodePosition(\u0002, \u0003);
		}

		// Token: 0x060010FF RID: 4351 RVA: 0x00031990 File Offset: 0x0002FB90
		internal static ILibPlaceholderIdentification \u0001(IDeviceIdentification \u0002, ILibraryPlaceholder2 \u0003)
		{
			return \u0003.Builder.CreateLibPlaceholderIdentification(\u0002, \u0003);
		}

		// Token: 0x06001100 RID: 4352 RVA: 0x000319A0 File Offset: 0x0002FBA0
		internal static _ICompileContext \u0001(KindOfContext \u0002, _ICompileContext \u0003, _ICompileContext \u0004, Guid \u0005)
		{
			return \u0003.Builder.CreateCompileContext(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06001101 RID: 4353 RVA: 0x000319B0 File Offset: 0x0002FBB0
		internal static _ISourcePosition \u0001()
		{
			return (_ISourcePosition)\u0003.Builder.CreateSourcePosition(-1, Guid.Empty, 0L, 0, 0);
		}

		// Token: 0x06001102 RID: 4354 RVA: 0x000319CC File Offset: 0x0002FBCC
		internal static _ISourcePosition \u0001(int \u0002, Guid \u0003, long \u0004, short \u0005, short \u0006)
		{
			return (_ISourcePosition)\u0003.Builder.CreateSourcePosition(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x06001103 RID: 4355 RVA: 0x000319E4 File Offset: 0x0002FBE4
		internal static _ISourcePosition \u0002()
		{
			return \u0003.Builder.CreateFixedSourcePosition();
		}

		// Token: 0x06001104 RID: 4356 RVA: 0x000319F0 File Offset: 0x0002FBF0
		internal static IMinimalPosition \u0001(_ISourcePosition \u0002)
		{
			return \u0003.Builder.CreateMinimalPosition(\u0002);
		}

		// Token: 0x06001105 RID: 4357 RVA: 0x00031A00 File Offset: 0x0002FC00
		internal static IMinimalPosition \u0001(long \u0002, short \u0003)
		{
			return \u0003.Builder.CreateMinimalPosition(\u0002, \u0003);
		}

		// Token: 0x06001106 RID: 4358 RVA: 0x00031A10 File Offset: 0x0002FC10
		internal static _ILiteralValue \u0001(double \u0002)
		{
			return \u0003.Builder.CreateLiteralValue(\u0002);
		}

		// Token: 0x06001107 RID: 4359 RVA: 0x00031A20 File Offset: 0x0002FC20
		internal static _ILiteralValue \u0001(long \u0002)
		{
			return \u0003.Builder.CreateLiteralValue(\u0002);
		}

		// Token: 0x06001108 RID: 4360 RVA: 0x00031A30 File Offset: 0x0002FC30
		internal static _ILiteralValue \u0001(ulong \u0002)
		{
			return \u0003.Builder.CreateLiteralValue(\u0002);
		}

		// Token: 0x06001109 RID: 4361 RVA: 0x00031A40 File Offset: 0x0002FC40
		internal static _ILiteralValue \u0001(string \u0002)
		{
			return \u0003.Builder.CreateLiteralValue(\u0002);
		}

		// Token: 0x0600110A RID: 4362 RVA: 0x00031A50 File Offset: 0x0002FC50
		internal static _ILiteralValue \u0001(bool \u0002)
		{
			return \u0003.Builder.CreateLiteralValue(\u0002);
		}

		// Token: 0x0600110B RID: 4363 RVA: 0x00031A60 File Offset: 0x0002FC60
		internal static _ISafeBoolType \u0001()
		{
			return \u0003.Builder.CreateSafeBoolType();
		}

		// Token: 0x0600110C RID: 4364 RVA: 0x00031A6C File Offset: 0x0002FC6C
		internal static _ISafeByteType \u0001()
		{
			return \u0003.Builder.CreateSafeByteType();
		}

		// Token: 0x0600110D RID: 4365 RVA: 0x00031A78 File Offset: 0x0002FC78
		internal static _ISafeSIntType \u0001()
		{
			return \u0003.Builder.CreateSafeSIntType();
		}

		// Token: 0x0600110E RID: 4366 RVA: 0x00031A84 File Offset: 0x0002FC84
		internal static _ISafeUSIntType \u0001()
		{
			return \u0003.Builder.CreateSafeUSIntType();
		}

		// Token: 0x0600110F RID: 4367 RVA: 0x00031A90 File Offset: 0x0002FC90
		internal static _ISafeWordType \u0001()
		{
			return \u0003.Builder.CreateSafeWordType();
		}

		// Token: 0x06001110 RID: 4368 RVA: 0x00031A9C File Offset: 0x0002FC9C
		internal static _ISafeIntType \u0001()
		{
			return \u0003.Builder.CreateSafeIntType();
		}

		// Token: 0x06001111 RID: 4369 RVA: 0x00031AA8 File Offset: 0x0002FCA8
		internal static _ISafeUIntType \u0001()
		{
			return \u0003.Builder.CreateSafeUIntType();
		}

		// Token: 0x06001112 RID: 4370 RVA: 0x00031AB4 File Offset: 0x0002FCB4
		internal static _ISafeDWordType \u0001()
		{
			return \u0003.Builder.CreateSafeDWordType();
		}

		// Token: 0x06001113 RID: 4371 RVA: 0x00031AC0 File Offset: 0x0002FCC0
		internal static _ISafeDIntType \u0001()
		{
			return \u0003.Builder.CreateSafeDIntType();
		}

		// Token: 0x06001114 RID: 4372 RVA: 0x00031ACC File Offset: 0x0002FCCC
		internal static _ISafeUDIntType \u0001()
		{
			return \u0003.Builder.CreateSafeUDIntType();
		}

		// Token: 0x06001115 RID: 4373 RVA: 0x00031AD8 File Offset: 0x0002FCD8
		internal static _ISafeLWordType \u0001()
		{
			return \u0003.Builder.CreateSafeLWordType();
		}

		// Token: 0x06001116 RID: 4374 RVA: 0x00031AE4 File Offset: 0x0002FCE4
		internal static _ISafeLIntType \u0001()
		{
			return \u0003.Builder.CreateSafeLIntType();
		}

		// Token: 0x06001117 RID: 4375 RVA: 0x00031AF0 File Offset: 0x0002FCF0
		internal static _ISafeULIntType \u0001()
		{
			return \u0003.Builder.CreateSafeULIntType();
		}

		// Token: 0x06001118 RID: 4376 RVA: 0x00031AFC File Offset: 0x0002FCFC
		internal static _ISafeTimeType \u0001()
		{
			return \u0003.Builder.CreateSafeTimeType();
		}

		// Token: 0x06001119 RID: 4377 RVA: 0x00031B08 File Offset: 0x0002FD08
		internal static _ISafeRealType \u0001()
		{
			return \u0003.Builder.CreateSafeRealType();
		}

		// Token: 0x0600111A RID: 4378 RVA: 0x00031B14 File Offset: 0x0002FD14
		internal static _ISafeLRealType \u0001()
		{
			return \u0003.Builder.CreateSafeLRealType();
		}

		// Token: 0x0600111B RID: 4379 RVA: 0x00031B20 File Offset: 0x0002FD20
		internal static _IRetainBoolType \u0001()
		{
			return \u0003.Builder.CreateRetainBoolType();
		}

		// Token: 0x0600111C RID: 4380 RVA: 0x00031B2C File Offset: 0x0002FD2C
		internal static _IBool16Type \u0001()
		{
			return \u0003.Builder.CreateBool16Type();
		}

		// Token: 0x0600111D RID: 4381 RVA: 0x00031B38 File Offset: 0x0002FD38
		internal static _IRetainByteType \u0001()
		{
			return \u0003.Builder.CreateRetainByteType();
		}

		// Token: 0x0600111E RID: 4382 RVA: 0x00031B44 File Offset: 0x0002FD44
		internal static _IRetainSIntType \u0001()
		{
			return \u0003.Builder.CreateRetainSIntType();
		}

		// Token: 0x0600111F RID: 4383 RVA: 0x00031B50 File Offset: 0x0002FD50
		internal static _IRetainUSIntType \u0001()
		{
			return \u0003.Builder.CreateRetainUSIntType();
		}

		// Token: 0x06001120 RID: 4384 RVA: 0x00031B5C File Offset: 0x0002FD5C
		internal static _IXDWordType \u0001()
		{
			return \u0003.Builder.CreateXDWordType();
		}

		// Token: 0x06001121 RID: 4385 RVA: 0x00031B68 File Offset: 0x0002FD68
		internal static _IXLWordType \u0001()
		{
			return \u0003.Builder.CreateXLWordType();
		}

		// Token: 0x06001122 RID: 4386 RVA: 0x00031B74 File Offset: 0x0002FD74
		internal static _IXDIntType \u0001()
		{
			return \u0003.Builder.CreateXDIntType();
		}

		// Token: 0x06001123 RID: 4387 RVA: 0x00031B80 File Offset: 0x0002FD80
		internal static _IBoolType \u0001()
		{
			return \u0003.Builder.CreateBoolType();
		}

		// Token: 0x06001124 RID: 4388 RVA: 0x00031B8C File Offset: 0x0002FD8C
		internal static _IDirectAddressBitType \u0001()
		{
			return \u0003.Builder.CreateDirectAddressBitType();
		}

		// Token: 0x06001125 RID: 4389 RVA: 0x00031B98 File Offset: 0x0002FD98
		internal static _IByteType \u0001()
		{
			return \u0003.Builder.CreateByteType();
		}

		// Token: 0x06001126 RID: 4390 RVA: 0x00031BA4 File Offset: 0x0002FDA4
		internal static _ISIntType \u0001()
		{
			return \u0003.Builder.CreateSIntType();
		}

		// Token: 0x06001127 RID: 4391 RVA: 0x00031BB0 File Offset: 0x0002FDB0
		internal static _IUSIntType \u0001()
		{
			return \u0003.Builder.CreateUSIntType();
		}

		// Token: 0x06001128 RID: 4392 RVA: 0x00031BBC File Offset: 0x0002FDBC
		internal static _IIntType \u0001()
		{
			return \u0003.Builder.CreateIntType();
		}

		// Token: 0x06001129 RID: 4393 RVA: 0x00031BC8 File Offset: 0x0002FDC8
		internal static _IUIntType \u0001()
		{
			return \u0003.Builder.CreateUIntType();
		}

		// Token: 0x0600112A RID: 4394 RVA: 0x00031BD4 File Offset: 0x0002FDD4
		internal static _IWordType \u0001()
		{
			return \u0003.Builder.CreateWordType();
		}

		// Token: 0x0600112B RID: 4395 RVA: 0x00031BE0 File Offset: 0x0002FDE0
		internal static _IDIntType \u0001()
		{
			return \u0003.Builder.CreateDIntType();
		}

		// Token: 0x0600112C RID: 4396 RVA: 0x00031BEC File Offset: 0x0002FDEC
		internal static _IUDIntType \u0001()
		{
			return \u0003.Builder.CreateUDIntType();
		}

		// Token: 0x0600112D RID: 4397 RVA: 0x00031BF8 File Offset: 0x0002FDF8
		internal static _IDWordType \u0001()
		{
			return \u0003.Builder.CreateDWordType();
		}

		// Token: 0x0600112E RID: 4398 RVA: 0x00031C04 File Offset: 0x0002FE04
		internal static _ILIntType \u0001()
		{
			return \u0003.Builder.CreateLIntType();
		}

		// Token: 0x0600112F RID: 4399 RVA: 0x00031C10 File Offset: 0x0002FE10
		internal static _IULIntType \u0001()
		{
			return \u0003.Builder.CreateULIntType();
		}

		// Token: 0x06001130 RID: 4400 RVA: 0x00031C1C File Offset: 0x0002FE1C
		internal static _ILWordType \u0001()
		{
			return \u0003.Builder.CreateLWordType();
		}

		// Token: 0x06001131 RID: 4401 RVA: 0x00031C28 File Offset: 0x0002FE28
		internal static _IRealType \u0001()
		{
			return \u0003.Builder.CreateRealType();
		}

		// Token: 0x06001132 RID: 4402 RVA: 0x00031C34 File Offset: 0x0002FE34
		internal static _ILRealType \u0001()
		{
			return \u0003.Builder.CreateLRealType();
		}

		// Token: 0x06001133 RID: 4403 RVA: 0x00031C40 File Offset: 0x0002FE40
		internal static _ILazyType \u0001()
		{
			return \u0003.Builder.CreateLazyType();
		}

		// Token: 0x06001134 RID: 4404 RVA: 0x00031C4C File Offset: 0x0002FE4C
		internal static _IBitConstType \u0001()
		{
			return \u0003.Builder.CreateBitConstType();
		}

		// Token: 0x06001135 RID: 4405 RVA: 0x00031C58 File Offset: 0x0002FE58
		internal static _IBitType \u0001()
		{
			return \u0003.Builder.CreateBitType();
		}

		// Token: 0x06001136 RID: 4406 RVA: 0x00031C64 File Offset: 0x0002FE64
		internal static _IUXIntType \u0001()
		{
			return \u0003.Builder.CreateUXIntType();
		}

		// Token: 0x06001137 RID: 4407 RVA: 0x00031C70 File Offset: 0x0002FE70
		internal static _IXIntType \u0001()
		{
			return \u0003.Builder.CreateXIntType();
		}

		// Token: 0x06001138 RID: 4408 RVA: 0x00031C7C File Offset: 0x0002FE7C
		internal static _IXWordType \u0001()
		{
			return \u0003.Builder.CreateXWordType();
		}

		// Token: 0x06001139 RID: 4409 RVA: 0x00031C88 File Offset: 0x0002FE88
		internal static _IXUDIntType \u0001()
		{
			return \u0003.Builder.CreateXUDIntType();
		}

		// Token: 0x0600113A RID: 4410 RVA: 0x00031C94 File Offset: 0x0002FE94
		internal static _IXULIntType \u0001()
		{
			return \u0003.Builder.CreateXULIntType();
		}

		// Token: 0x0600113B RID: 4411 RVA: 0x00031CA0 File Offset: 0x0002FEA0
		internal static _IXLIntType \u0001()
		{
			return \u0003.Builder.CreateXLIntType();
		}

		// Token: 0x0600113C RID: 4412 RVA: 0x00031CAC File Offset: 0x0002FEAC
		internal static _IDateType \u0001()
		{
			return \u0003.Builder.CreateDateType();
		}

		// Token: 0x0600113D RID: 4413 RVA: 0x00031CB8 File Offset: 0x0002FEB8
		internal static _ITimeOfDayType \u0001()
		{
			return \u0003.Builder.CreateTimeOfDayType();
		}

		// Token: 0x0600113E RID: 4414 RVA: 0x00031CC4 File Offset: 0x0002FEC4
		internal static _IDateAndTimeType \u0001()
		{
			return \u0003.Builder.CreateDateAndTimeType();
		}

		// Token: 0x0600113F RID: 4415 RVA: 0x00031CD0 File Offset: 0x0002FED0
		internal static _ILDateType \u0001()
		{
			return \u0003.Builder.CreateLDateType();
		}

		// Token: 0x06001140 RID: 4416 RVA: 0x00031CDC File Offset: 0x0002FEDC
		internal static _ILTimeOfDayType \u0001()
		{
			return \u0003.Builder.CreateLTimeOfDayType();
		}

		// Token: 0x06001141 RID: 4417 RVA: 0x00031CE8 File Offset: 0x0002FEE8
		internal static _ILDateAndTimeType \u0001()
		{
			return \u0003.Builder.CreateLDateAndTimeType();
		}

		// Token: 0x06001142 RID: 4418 RVA: 0x00031CF4 File Offset: 0x0002FEF4
		internal static _ITimeType \u0001()
		{
			return \u0003.Builder.CreateTimeType();
		}

		// Token: 0x06001143 RID: 4419 RVA: 0x00031D00 File Offset: 0x0002FF00
		internal static _ILTimeType \u0001()
		{
			return \u0003.Builder.CreateLTimeType();
		}

		// Token: 0x06001144 RID: 4420 RVA: 0x00031D0C File Offset: 0x0002FF0C
		internal static _IAnyType \u0001()
		{
			return \u0003.Builder.CreateAnyType();
		}

		// Token: 0x06001145 RID: 4421 RVA: 0x00031D18 File Offset: 0x0002FF18
		internal static _IAnyRealType \u0001()
		{
			return \u0003.Builder.CreateAnyRealType();
		}

		// Token: 0x06001146 RID: 4422 RVA: 0x00031D24 File Offset: 0x0002FF24
		internal static _IAnyStringType \u0001()
		{
			return \u0003.Builder.CreateAnyStringType();
		}

		// Token: 0x06001147 RID: 4423 RVA: 0x00031D30 File Offset: 0x0002FF30
		internal static _IAnyIntType \u0001()
		{
			return \u0003.Builder.CreateAnyIntType();
		}

		// Token: 0x06001148 RID: 4424 RVA: 0x00031D3C File Offset: 0x0002FF3C
		internal static _IAnyNumType \u0001()
		{
			return \u0003.Builder.CreateAnyNumType();
		}

		// Token: 0x06001149 RID: 4425 RVA: 0x00031D48 File Offset: 0x0002FF48
		internal static _IAnyBitType \u0001()
		{
			return \u0003.Builder.CreateAnyBitType();
		}

		// Token: 0x0600114A RID: 4426 RVA: 0x00031D54 File Offset: 0x0002FF54
		internal static _IAnyDateType \u0001()
		{
			return \u0003.Builder.CreateAnyDateType();
		}

		// Token: 0x0600114B RID: 4427 RVA: 0x00031D60 File Offset: 0x0002FF60
		internal static _IAnyBitButBoolIsPreferred \u0001()
		{
			return \u0003.Builder.CreateAnyBitButBoolIsPreferredType();
		}

		// Token: 0x0600114C RID: 4428 RVA: 0x00031D6C File Offset: 0x0002FF6C
		internal static _IVariableLengthArrayType \u0001()
		{
			return \u0003.Builder.CreateVariableLengthArrayType();
		}

		// Token: 0x0600114D RID: 4429 RVA: 0x00031D78 File Offset: 0x0002FF78
		internal static _IArrayType \u0001()
		{
			return \u0003.Builder.CreateArrayType();
		}

		// Token: 0x0600114E RID: 4430 RVA: 0x00031D84 File Offset: 0x0002FF84
		internal static _IArrayType \u0001(_IType \u0002)
		{
			return \u0003.Builder.CreateArrayType(\u0002);
		}

		// Token: 0x0600114F RID: 4431 RVA: 0x00031D94 File Offset: 0x0002FF94
		internal static _IVectorType \u0001(_IType \u0002, _IExpression \u0003)
		{
			return \u0003.Builder.CreateVectorType(\u0002, \u0003);
		}

		// Token: 0x06001150 RID: 4432 RVA: 0x00031DA4 File Offset: 0x0002FFA4
		internal static _IRangeAwareAnyIntType \u0001()
		{
			return \u0003.Builder.CreateRangeAwareAnyIntType();
		}

		// Token: 0x06001151 RID: 4433 RVA: 0x00031DB0 File Offset: 0x0002FFB0
		internal static _IUserdefType \u0001()
		{
			return \u0003.Builder.CreateUserdefType();
		}

		// Token: 0x06001152 RID: 4434 RVA: 0x00031DBC File Offset: 0x0002FFBC
		internal static _IUserdefType \u0001(_IExpression \u0002)
		{
			return \u0003.Builder.CreateUserdefType(\u0002);
		}

		// Token: 0x06001153 RID: 4435 RVA: 0x00031DCC File Offset: 0x0002FFCC
		internal static IGenericUserdefType \u0001(_IExpression \u0002)
		{
			return \u0003.Builder.CreateGenericUserdefType(\u0002);
		}

		// Token: 0x06001154 RID: 4436 RVA: 0x00031DDC File Offset: 0x0002FFDC
		internal static _IUserdefType \u0001(string \u0002)
		{
			return \u0003.Builder.CreateUserdefType(\u0002);
		}

		// Token: 0x06001155 RID: 4437 RVA: 0x00031DEC File Offset: 0x0002FFEC
		internal static _IPointerType \u0001()
		{
			return \u0003.Builder.CreatePointerType();
		}

		// Token: 0x06001156 RID: 4438 RVA: 0x00031DF8 File Offset: 0x0002FFF8
		internal static _IPointerType \u0001(_IType \u0002)
		{
			return \u0003.Builder.CreatePointerType(\u0002);
		}

		// Token: 0x06001157 RID: 4439 RVA: 0x00031E08 File Offset: 0x00030008
		internal static _IReferenceType \u0001()
		{
			return \u0003.Builder.CreateReferenceType();
		}

		// Token: 0x06001158 RID: 4440 RVA: 0x00031E14 File Offset: 0x00030014
		internal static _IReferenceType \u0001(_IType \u0002)
		{
			return \u0003.Builder.CreateReferenceType(\u0002);
		}

		// Token: 0x06001159 RID: 4441 RVA: 0x00031E24 File Offset: 0x00030024
		internal static _IImplicitReferenceType \u0001(_IType \u0002)
		{
			return \u0003.Builder.CreateImplicitReferenceType(\u0002);
		}

		// Token: 0x0600115A RID: 4442 RVA: 0x00031E34 File Offset: 0x00030034
		internal static _IInOutReferenceType \u0001(_IType \u0002)
		{
			return \u0003.Builder.CreateInOutReferenceType(\u0002);
		}

		// Token: 0x0600115B RID: 4443 RVA: 0x00031E44 File Offset: 0x00030044
		internal static _IStringType \u0001()
		{
			return \u0003.Builder.CreateStringType();
		}

		// Token: 0x0600115C RID: 4444 RVA: 0x00031E50 File Offset: 0x00030050
		internal static _IWStringType \u0001()
		{
			return \u0003.Builder.CreateWStringType();
		}

		// Token: 0x0600115D RID: 4445 RVA: 0x00031E5C File Offset: 0x0003005C
		internal static _IAliasType \u0001(_IType \u0002)
		{
			return \u0003.Builder.CreateAliasType(\u0002);
		}

		// Token: 0x0600115E RID: 4446 RVA: 0x00031E6C File Offset: 0x0003006C
		internal static _IEnumType \u0001(string \u0002, int \u0003)
		{
			return \u0003.Builder.CreateEnumType(\u0002, \u0003);
		}

		// Token: 0x0600115F RID: 4447 RVA: 0x00031E7C File Offset: 0x0003007C
		internal static _IEnumType \u0001(string \u0002)
		{
			return \u0003.Builder.CreateEnumType(\u0002);
		}

		// Token: 0x06001160 RID: 4448 RVA: 0x00031E8C File Offset: 0x0003008C
		internal static _IParamsType \u0001(_IType \u0002, _IExpression \u0003)
		{
			return \u0003.Builder.CreateParamsType(\u0002, \u0003);
		}

		// Token: 0x06001161 RID: 4449 RVA: 0x00031E9C File Offset: 0x0003009C
		internal static IImplicitEnumerationType \u0001(_IEnumDeclarationListStatement \u0002, string \u0003)
		{
			return \u0003.Builder.CreateImplicitEnumerationType(\u0002, \u0003);
		}

		// Token: 0x06001162 RID: 4450 RVA: 0x00031EAC File Offset: 0x000300AC
		internal static _IVariableExpression \u0001(_IVariable \u0002, _ISignature \u0003)
		{
			return \u0003.Builder.CreateVariableExpression(\u0002, \u0003);
		}

		// Token: 0x06001163 RID: 4451 RVA: 0x00031EBC File Offset: 0x000300BC
		internal static _IWhileStatement \u0001(IExpression \u0002, ISequenceStatement2 \u0003)
		{
			return \u0003.Builder.CreateWhileStatement(null, \u0002, \u0003) as _IWhileStatement;
		}

		// Token: 0x06001164 RID: 4452 RVA: 0x00031ED0 File Offset: 0x000300D0
		internal static _IRepeatStatement \u0001(IExpression \u0002, ISequenceStatement2 \u0003)
		{
			return \u0003.Builder.CreateRepeatStatement(null, \u0002, \u0003) as _IRepeatStatement;
		}

		// Token: 0x06001165 RID: 4453 RVA: 0x00031EE4 File Offset: 0x000300E4
		internal static _ICaseRangeExpression \u0001(IExpression \u0002, IExpression \u0003)
		{
			return \u0003.Builder.CreateCaseRangeExpression(null, \u0002, \u0003) as _ICaseRangeExpression;
		}

		// Token: 0x06001166 RID: 4454 RVA: 0x00031EF8 File Offset: 0x000300F8
		internal static _ICaseLabelStatement \u0001(List<IExpression> \u0002)
		{
			return \u0003.Builder.CreateCaseLabelStatement(null, \u0002) as _ICaseLabelStatement;
		}

		// Token: 0x06001167 RID: 4455 RVA: 0x00031F0C File Offset: 0x0003010C
		internal static _ICase \u0001(ICaseLabelStatement \u0002, ISequenceStatement2 \u0003)
		{
			return \u0003.Builder.CreateCase(\u0002, \u0003) as _ICase;
		}

		// Token: 0x06001168 RID: 4456 RVA: 0x00031F20 File Offset: 0x00030120
		internal static _ICaseStatement \u0001(IExpression \u0002, List<ICase> \u0003, ISequenceStatement2 \u0004)
		{
			return \u0003.Builder.CreateCaseStatement(null, \u0002, \u0003, \u0004) as _ICaseStatement;
		}

		// Token: 0x06001169 RID: 4457 RVA: 0x00031F38 File Offset: 0x00030138
		internal static _IForStatement \u0001(IAssignmentExpression \u0002, IExpression \u0003, IExpression \u0004, IStatement \u0005)
		{
			return \u0003.Builder.CreateForStatement(null, \u0002, \u0003, \u0004, \u0005) as _IForStatement;
		}

		// Token: 0x0600116A RID: 4458 RVA: 0x00031F50 File Offset: 0x00030150
		internal static _IForStatement \u0001()
		{
			return \u0003.Builder.CreateForStatement();
		}

		// Token: 0x0600116B RID: 4459 RVA: 0x00031F5C File Offset: 0x0003015C
		internal static IDirectVariable \u0001(DirectVariableLocation \u0002)
		{
			return \u0003.Builder.CreateIncompleteDirectVariable(\u0002);
		}

		// Token: 0x0600116C RID: 4460 RVA: 0x00031F6C File Offset: 0x0003016C
		internal static _IForStatement \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateForStatement(\u0002);
		}

		// Token: 0x0600116D RID: 4461 RVA: 0x00031F7C File Offset: 0x0003017C
		internal static _IExitStatement \u0001(IExprementPosition \u0002)
		{
			return \u0003.Builder.CreateExitStatement(\u0002) as _IExitStatement;
		}

		// Token: 0x0600116E RID: 4462 RVA: 0x00031F90 File Offset: 0x00030190
		internal static _IContinueStatement \u0001(IExprementPosition \u0002)
		{
			return \u0003.Builder.CreateContinueStatement(\u0002) as _IContinueStatement;
		}

		// Token: 0x0600116F RID: 4463 RVA: 0x00031FA4 File Offset: 0x000301A4
		internal static _ISequenceStatement \u0001(IExprementPosition \u0002)
		{
			return \u0003.Builder.CreateSequenceStatement(\u0002) as _ISequenceStatement;
		}

		// Token: 0x06001170 RID: 4464 RVA: 0x00031FB8 File Offset: 0x000301B8
		internal static _ISequenceStatement \u0001(List<IStatement> \u0002)
		{
			return \u0003.Builder.CreateSequenceStatement(null, \u0002) as _ISequenceStatement;
		}

		// Token: 0x06001171 RID: 4465 RVA: 0x00031FCC File Offset: 0x000301CC
		internal static _IAssignmentExpression \u0001(IExpression \u0002, IExpression \u0003)
		{
			return \u0003.Builder.CreateAssignmentExpression(null, \u0002, \u0003) as _IAssignmentExpression;
		}

		// Token: 0x06001172 RID: 4466 RVA: 0x00031FE0 File Offset: 0x000301E0
		internal static _IExpressionStatement \u0001(IExpression \u0002, IExpression \u0003)
		{
			return \u0003.Builder.CreateAssignmentStatement(null, \u0002, \u0003) as _IExpressionStatement;
		}

		// Token: 0x06001173 RID: 4467 RVA: 0x00031FF4 File Offset: 0x000301F4
		internal static _IAssignmentExpression \u0001(IExpression \u0002, IExpression \u0003, Operator \u0004)
		{
			return \u0003.Builder.CreateAssignmentExpression(null, \u0002, \u0003, \u0004) as _IAssignmentExpression;
		}

		// Token: 0x06001174 RID: 4468 RVA: 0x0003200C File Offset: 0x0003020C
		internal static _IIfStatement \u0001(IExpression \u0002, ISequenceStatement2 \u0003)
		{
			return \u0003.Builder.CreateIfStatement(null, \u0002, \u0003) as _IIfStatement;
		}

		// Token: 0x06001175 RID: 4469 RVA: 0x00032020 File Offset: 0x00030220
		internal static _IIfStatement \u0001(IExpression \u0002, ISequenceStatement2 \u0003, ISequenceStatement2 \u0004)
		{
			return \u0003.Builder.CreateIfStatement(null, \u0002, \u0003, \u0004) as _IIfStatement;
		}

		// Token: 0x06001176 RID: 4470 RVA: 0x00032038 File Offset: 0x00030238
		internal static _IReturnStatement \u0001(IExpression \u0002)
		{
			return \u0003.Builder.CreateReturnStatement(null, \u0002) as _IReturnStatement;
		}

		// Token: 0x06001177 RID: 4471 RVA: 0x0003204C File Offset: 0x0003024C
		internal static _IJumpStatement \u0001(IExpression \u0002, string \u0003)
		{
			return \u0003.Builder.CreateJumpStatement(null, \u0002, \u0003) as _IJumpStatement;
		}

		// Token: 0x06001178 RID: 4472 RVA: 0x00032060 File Offset: 0x00030260
		internal static _ILabelStatement \u0001(string \u0002)
		{
			return \u0003.Builder.CreateLabelStatement(\u0002);
		}

		// Token: 0x06001179 RID: 4473 RVA: 0x00032070 File Offset: 0x00030270
		internal static _ICommentStatement \u0001(string \u0002)
		{
			return \u0003.Builder.CreateCommentStatement(\u0002);
		}

		// Token: 0x0600117A RID: 4474 RVA: 0x00032080 File Offset: 0x00030280
		internal static _IPragmaStatement \u0001(Guid \u0002)
		{
			return \u0003.Builder.CreateMessageGuidPragmaStatement(\u0002) as _IPragmaStatement;
		}

		// Token: 0x0600117B RID: 4475 RVA: 0x00032094 File Offset: 0x00030294
		internal static _IImplicitCodeSectionPragma \u0001(string \u0002, bool \u0003)
		{
			return \u0003.Builder.CreateImplicitCodeSectionPragma(Token.Empty, \u0002, \u0003);
		}

		// Token: 0x0600117C RID: 4476 RVA: 0x000320A8 File Offset: 0x000302A8
		internal static _ILocalSignatureIdPragma \u0001(string \u0002, int \u0003)
		{
			return \u0003.Builder.CreateLocalSignatureIdPragma(\u0003, \u0002);
		}

		// Token: 0x0600117D RID: 4477 RVA: 0x000320B8 File Offset: 0x000302B8
		internal static _IExpressionStatement \u0001(IExpression \u0002)
		{
			return \u0003.Builder.CreateExpressionStatement(\u0002) as _IExpressionStatement;
		}

		// Token: 0x0600117E RID: 4478 RVA: 0x000320CC File Offset: 0x000302CC
		internal static _IPOUDeclarationStatement \u0001(Operator \u0002, string \u0003, ICompiledType \u0004, List<IVariableDeclarationListStatement> \u0005, List<IExpression> \u0006, List<IExpression> \u0007, SignatureFlag \u0008)
		{
			return \u0003.Builder.CreatePOUDeclarationStatement(null, \u0002, \u0003, \u0004, \u0005, \u0006, \u0007, \u0008) as _IPOUDeclarationStatement;
		}

		// Token: 0x0600117F RID: 4479 RVA: 0x000320F4 File Offset: 0x000302F4
		internal static _IPOUDeclarationStatement \u0001(Operator \u0002, string \u0003)
		{
			return \u0003.Builder.CreatePOUDeclarationStatement(null, \u0002, \u0003) as _IPOUDeclarationStatement;
		}

		// Token: 0x06001180 RID: 4480 RVA: 0x00032108 File Offset: 0x00030308
		internal static _IVariableDeclarationListStatement \u0001(VarFlag \u0002, ISequenceStatement \u0003)
		{
			return \u0003.Builder.CreateVariableDeclarationListStatement(null, \u0002, \u0003) as _IVariableDeclarationListStatement;
		}

		// Token: 0x06001181 RID: 4481 RVA: 0x0003211C File Offset: 0x0003031C
		internal static _IVariableDeclarationStatement \u0001(string \u0002, ICompiledType \u0003, IExpression \u0004, IDirectVariable \u0005)
		{
			return \u0003.Builder.CreateVariableDeclarationStatement(null, \u0002, \u0003, \u0004, \u0005) as _IVariableDeclarationStatement;
		}

		// Token: 0x06001182 RID: 4482 RVA: 0x00032134 File Offset: 0x00030334
		internal static _IVariableDeclarationStatement \u0001(List<string> \u0002, ICompiledType \u0003, IExpression \u0004)
		{
			return \u0003.Builder.CreateVariableDeclarationStatement(null, \u0002, \u0003, \u0004) as _IVariableDeclarationStatement;
		}

		// Token: 0x06001183 RID: 4483 RVA: 0x0003214C File Offset: 0x0003034C
		internal static _IVariableDeclarationStatement \u0001(string \u0002, ICompiledType \u0003, IExpression \u0004, IDirectVariable \u0005, List<IAssignmentExpression> \u0006)
		{
			return \u0003.Builder.CreateInstanceVariableDeclarationStatement(null, \u0002, \u0003, \u0004, \u0005, \u0006) as _IVariableDeclarationStatement;
		}

		// Token: 0x06001184 RID: 4484 RVA: 0x00032164 File Offset: 0x00030364
		internal static _ITypeDeclarationStatement \u0001(ICompiledType \u0002, string \u0003, IExpression \u0004, SignatureFlag \u0005)
		{
			return \u0003.Builder.CreateAliasDeclaration(null, \u0002, \u0003, \u0004, \u0005) as _ITypeDeclarationStatement;
		}

		// Token: 0x06001185 RID: 4485 RVA: 0x0003217C File Offset: 0x0003037C
		internal static _ITypeDeclarationStatement \u0001(string \u0002, IExpression \u0003, IExpression \u0004, ISequenceStatement \u0005, SignatureFlag \u0006)
		{
			return \u0003.Builder.CreateStructDeclaration(null, \u0002, \u0003, \u0004, \u0005, \u0006) as _ITypeDeclarationStatement;
		}

		// Token: 0x06001186 RID: 4486 RVA: 0x00032194 File Offset: 0x00030394
		internal static _ITypeDeclarationStatement \u0002(string \u0002, IExpression \u0003, IExpression \u0004, ISequenceStatement \u0005, SignatureFlag \u0006)
		{
			return \u0003.Builder.CreateUnionDeclaration(null, \u0002, \u0003, \u0004, \u0005, \u0006) as _ITypeDeclarationStatement;
		}

		// Token: 0x06001187 RID: 4487 RVA: 0x000321AC File Offset: 0x000303AC
		internal static _ITypeDeclarationStatement \u0001(string \u0002, IExpression \u0003, IEnumDeclarationListStatement \u0004, SignatureFlag \u0005)
		{
			return \u0003.Builder.CreateEnumTypeDeclaration(null, \u0002, \u0003, \u0004, \u0005) as _ITypeDeclarationStatement;
		}

		// Token: 0x06001188 RID: 4488 RVA: 0x000321C4 File Offset: 0x000303C4
		internal static _IEnumDeclarationListStatement \u0001(ICompiledType \u0002, List<IEnumDeclarationStatement> \u0003)
		{
			return \u0003.Builder.CreateEnumDeclarationListStatement(null, \u0002, \u0003) as _IEnumDeclarationListStatement;
		}

		// Token: 0x06001189 RID: 4489 RVA: 0x000321D8 File Offset: 0x000303D8
		internal static _IEnumDeclarationListStatement \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateEnumDeclarationListStatement(\u0002);
		}

		// Token: 0x0600118A RID: 4490 RVA: 0x000321E8 File Offset: 0x000303E8
		internal static _ITypeDeclarationStatement \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateTypeDeclarationStatement(\u0002);
		}

		// Token: 0x0600118B RID: 4491 RVA: 0x000321F8 File Offset: 0x000303F8
		internal static _IEnumDeclarationStatement \u0001(string \u0002, IExpression \u0003)
		{
			return \u0003.Builder.CreateEnumDeclarationStatement(null, \u0002, \u0003) as _IEnumDeclarationStatement;
		}

		// Token: 0x0600118C RID: 4492 RVA: 0x0003220C File Offset: 0x0003040C
		internal static _ICallExpression \u0001(IExpression \u0002, IExpression \u0003, ICompiledType \u0004, List<IAssignmentExpression> \u0005, List<IAssignmentExpression> \u0006)
		{
			return \u0003.Builder.CreateCallExpression(null, \u0002, \u0003, \u0004, \u0005, \u0006) as _ICallExpression;
		}

		// Token: 0x0600118D RID: 4493 RVA: 0x00032224 File Offset: 0x00030424
		internal static _IExpressionStatement \u0001(IExpression \u0002, IExpression \u0003, ICompiledType \u0004, List<IAssignmentExpression> \u0005, List<IAssignmentExpression> \u0006)
		{
			return \u0003.Builder.CreateCallStatement(null, \u0002, \u0003, \u0004, \u0005, \u0006) as _IExpressionStatement;
		}

		// Token: 0x0600118E RID: 4494 RVA: 0x0003223C File Offset: 0x0003043C
		internal static _IOperatorExpression \u0001(Operator \u0002, IExpression \u0003, IExpression \u0004)
		{
			return \u0003.Builder.CreateOperatorExpression(null, \u0002, \u0003, \u0004) as _IOperatorExpression;
		}

		// Token: 0x0600118F RID: 4495 RVA: 0x00032254 File Offset: 0x00030454
		internal static _IOperatorExpression \u0001(Operator \u0002, IExpression \u0003)
		{
			return \u0003.Builder.CreateOperatorExpression(null, \u0002, \u0003) as _IOperatorExpression;
		}

		// Token: 0x06001190 RID: 4496 RVA: 0x00032268 File Offset: 0x00030468
		internal static _IOperatorExpression \u0001(Operator \u0002, List<IExpression> \u0003)
		{
			return \u0003.Builder.CreateOperatorExpression(null, \u0002, \u0003) as _IOperatorExpression;
		}

		// Token: 0x06001191 RID: 4497 RVA: 0x0003227C File Offset: 0x0003047C
		internal static _IConversionExpression \u0001(TypeClass \u0002, TypeClass \u0003, IExpression \u0004)
		{
			return \u0003.Builder.CreateConversionExpression(null, \u0002, \u0003, \u0004) as _IConversionExpression;
		}

		// Token: 0x06001192 RID: 4498 RVA: 0x00032294 File Offset: 0x00030494
		internal static _IThisExpression \u0001(IExprementPosition \u0002)
		{
			return \u0003.Builder.CreateThisExpression(\u0002) as _IThisExpression;
		}

		// Token: 0x06001193 RID: 4499 RVA: 0x000322A8 File Offset: 0x000304A8
		internal static _IBaseExpression \u0001(IExprementPosition \u0002)
		{
			return \u0003.Builder.CreateSuperExpression(\u0002) as _IBaseExpression;
		}

		// Token: 0x06001194 RID: 4500 RVA: 0x000322BC File Offset: 0x000304BC
		internal static _ILiteralExpression \u0001(long \u0002, TypeClass \u0003)
		{
			return \u0003.Builder.CreateLiteralExpression(null, \u0002, \u0003) as _ILiteralExpression;
		}

		// Token: 0x06001195 RID: 4501 RVA: 0x000322D0 File Offset: 0x000304D0
		internal static _ILiteralExpression \u0001(long \u0002, TypeClass \u0003, int \u0004)
		{
			return \u0003.Builder.CreateLiteralExpression(null, \u0002, \u0003, \u0004) as _ILiteralExpression;
		}

		// Token: 0x06001196 RID: 4502 RVA: 0x000322E8 File Offset: 0x000304E8
		internal static _ILiteralExpression \u0001(ulong \u0002, TypeClass \u0003)
		{
			return \u0003.Builder.CreateLiteralExpression(null, \u0002, \u0003) as _ILiteralExpression;
		}

		// Token: 0x06001197 RID: 4503 RVA: 0x000322FC File Offset: 0x000304FC
		internal static _ILiteralExpression \u0001(string \u0002, TypeClass \u0003)
		{
			return \u0003.Builder.CreateLiteralExpression(null, \u0002, \u0003) as _ILiteralExpression;
		}

		// Token: 0x06001198 RID: 4504 RVA: 0x00032310 File Offset: 0x00030510
		internal static _ILiteralExpression \u0001(double \u0002, TypeClass \u0003)
		{
			return \u0003.Builder.CreateLiteralExpression(null, \u0002, \u0003) as _ILiteralExpression;
		}

		// Token: 0x06001199 RID: 4505 RVA: 0x00032324 File Offset: 0x00030524
		internal static _ILiteralExpression \u0001(long \u0002)
		{
			return \u0003.Builder.CreateLiteralExpression(null, \u0002) as _ILiteralExpression;
		}

		// Token: 0x0600119A RID: 4506 RVA: 0x00032338 File Offset: 0x00030538
		internal static _ILiteralExpression \u0001(ulong \u0002)
		{
			return \u0003.Builder.CreateLiteralExpression(null, \u0002) as _ILiteralExpression;
		}

		// Token: 0x0600119B RID: 4507 RVA: 0x0003234C File Offset: 0x0003054C
		internal static _ILiteralExpression \u0001(string \u0002)
		{
			return \u0003.Builder.CreateLiteralExpression(null, \u0002) as _ILiteralExpression;
		}

		// Token: 0x0600119C RID: 4508 RVA: 0x00032360 File Offset: 0x00030560
		internal static _ILiteralExpression \u0001(double \u0002)
		{
			return \u0003.Builder.CreateLiteralExpression(null, \u0002) as _ILiteralExpression;
		}

		// Token: 0x0600119D RID: 4509 RVA: 0x00032374 File Offset: 0x00030574
		internal static _ILiteralExpression \u0001(bool \u0002)
		{
			return \u0003.Builder.CreateLiteralExpression(null, \u0002) as _ILiteralExpression;
		}

		// Token: 0x0600119E RID: 4510 RVA: 0x00032388 File Offset: 0x00030588
		internal static _ILiteralExpression \u0001(TypeClass \u0002)
		{
			return \u0003.Builder.CreateDefaultLiteralExpression(\u0002);
		}

		// Token: 0x0600119F RID: 4511 RVA: 0x00032398 File Offset: 0x00030598
		internal static _ILiteralExpression \u0001(long \u0002, TypeClass \u0003, IToken \u0004)
		{
			return \u0003.Builder.CreateLiteralExpression(\u0002, \u0003, \u0004);
		}

		// Token: 0x060011A0 RID: 4512 RVA: 0x000323A8 File Offset: 0x000305A8
		internal static _ILiteralExpression \u0001(long \u0002, TypeClass \u0003, IToken \u0004, int \u0005)
		{
			return \u0003.Builder.CreateLiteralExpression(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x060011A1 RID: 4513 RVA: 0x000323B8 File Offset: 0x000305B8
		internal static _ILiteralExpression \u0001(long \u0002, TypeClass \u0003, IToken \u0004, int \u0005, bool \u0006)
		{
			return \u0003.Builder.CreateLiteralExpression(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x060011A2 RID: 4514 RVA: 0x000323CC File Offset: 0x000305CC
		internal static _ILiteralExpression \u0001(ulong \u0002, TypeClass \u0003, IToken \u0004)
		{
			return \u0003.Builder.CreateLiteralExpression(\u0002, \u0003, \u0004);
		}

		// Token: 0x060011A3 RID: 4515 RVA: 0x000323DC File Offset: 0x000305DC
		internal static _ILiteralExpression \u0001(string \u0002, TypeClass \u0003, IToken \u0004)
		{
			return \u0003.Builder.CreateLiteralExpression(\u0002, \u0003, \u0004);
		}

		// Token: 0x060011A4 RID: 4516 RVA: 0x000323EC File Offset: 0x000305EC
		internal static _ILiteralExpression \u0001(string \u0002, TypeClass \u0003, IToken \u0004, StringEncoding \u0005)
		{
			return \u0003.Builder.CreateLiteralExpression(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x060011A5 RID: 4517 RVA: 0x000323FC File Offset: 0x000305FC
		internal static _ILiteralExpression \u0001(double \u0002, TypeClass \u0003, IToken \u0004)
		{
			return \u0003.Builder.CreateLiteralExpression(\u0002, \u0003, \u0004);
		}

		// Token: 0x060011A6 RID: 4518 RVA: 0x0003240C File Offset: 0x0003060C
		internal static _ITypeExpression \u0001(ICompiledType \u0002)
		{
			return \u0003.Builder.CreateTypeExpression(null, \u0002) as _ITypeExpression;
		}

		// Token: 0x060011A7 RID: 4519 RVA: 0x00032420 File Offset: 0x00030620
		internal static _ITypeExpression \u0001(ICompiledType \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateTypeExpression(\u0002, \u0003);
		}

		// Token: 0x060011A8 RID: 4520 RVA: 0x00032430 File Offset: 0x00030630
		internal static _INewExpression \u0001(ICompiledType \u0002, IExpression \u0003)
		{
			return \u0003.Builder.CreateNewExpression(null, \u0002, \u0003) as _INewExpression;
		}

		// Token: 0x060011A9 RID: 4521 RVA: 0x00032444 File Offset: 0x00030644
		internal static _ICastExpression \u0001(IExpression \u0002, IExpression \u0003)
		{
			return \u0003.Builder.CreateCastExpression(null, \u0002, \u0003) as _ICastExpression;
		}

		// Token: 0x060011AA RID: 4522 RVA: 0x00032458 File Offset: 0x00030658
		internal static _ICastExpression \u0001(ICompiledType \u0002, IExpression \u0003)
		{
			return \u0003.Builder.CreateCastExpression(null, \u0002, \u0003) as _ICastExpression;
		}

		// Token: 0x060011AB RID: 4523 RVA: 0x0003246C File Offset: 0x0003066C
		internal static _IAddressExpression \u0001(IDirectVariable \u0002)
		{
			return \u0003.Builder.CreateAddressExpression(\u0002);
		}

		// Token: 0x060011AC RID: 4524 RVA: 0x0003247C File Offset: 0x0003067C
		internal static _IVariableExpression \u0001(string \u0002)
		{
			return \u0003.Builder.CreateVariableExpression(\u0002);
		}

		// Token: 0x060011AD RID: 4525 RVA: 0x0003248C File Offset: 0x0003068C
		internal static _IIndexAccessExpression \u0001(IExpression \u0002, IExpression \u0003)
		{
			return \u0003.Builder.CreateIndexAccessExpression(null, \u0002, \u0003) as _IIndexAccessExpression;
		}

		// Token: 0x060011AE RID: 4526 RVA: 0x000324A0 File Offset: 0x000306A0
		internal static _IIndexAccessExpression \u0001(IExpression \u0002, List<IExpression> \u0003)
		{
			return \u0003.Builder.CreateIndexAccessExpression(null, \u0002, \u0003) as _IIndexAccessExpression;
		}

		// Token: 0x060011AF RID: 4527 RVA: 0x000324B4 File Offset: 0x000306B4
		internal static _ICompoAccessExpression \u0001(IExpression \u0002, IVariableExpression2 \u0003)
		{
			return \u0003.Builder.CreateCompoAccessExpression(null, \u0002, \u0003) as _ICompoAccessExpression;
		}

		// Token: 0x060011B0 RID: 4528 RVA: 0x000324C8 File Offset: 0x000306C8
		internal static _ICompoAccessExpression \u0001(IExpression \u0002, ILiteralExpression \u0003)
		{
			return \u0003.Builder.CreateBitAccessExpression(null, \u0002, \u0003) as _ICompoAccessExpression;
		}

		// Token: 0x060011B1 RID: 4529 RVA: 0x000324DC File Offset: 0x000306DC
		internal static _IDeRefAccessExpression \u0001(IExpression \u0002)
		{
			return \u0003.Builder.CreateDeRefAccessExpression(null, \u0002) as _IDeRefAccessExpression;
		}

		// Token: 0x060011B2 RID: 4530 RVA: 0x000324F0 File Offset: 0x000306F0
		internal static _IGlobalScopeExpression \u0001(IExpression \u0002)
		{
			return \u0003.Builder.CreateGlobalScopeExpression(null, \u0002) as _IGlobalScopeExpression;
		}

		// Token: 0x060011B3 RID: 4531 RVA: 0x00032504 File Offset: 0x00030704
		internal static _ISystemScopeExpression \u0001(IExpression \u0002)
		{
			return \u0003.Builder.CreateSystemScopeExpression(null, \u0002) as _ISystemScopeExpression;
		}

		// Token: 0x060011B4 RID: 4532 RVA: 0x00032518 File Offset: 0x00030718
		internal static _IMultipleIndexInitialization \u0001(IExpression \u0002, IExpression \u0003)
		{
			return \u0003.Builder.CreateMultipleIndexInitialisation(null, \u0002, \u0003) as _IMultipleIndexInitialization;
		}

		// Token: 0x060011B5 RID: 4533 RVA: 0x0003252C File Offset: 0x0003072C
		internal static _IArrayInitialization \u0001(List<IExpression> \u0002)
		{
			return \u0003.Builder.CreateArrayInitialisation(null, \u0002) as _IArrayInitialization;
		}

		// Token: 0x060011B6 RID: 4534 RVA: 0x00032540 File Offset: 0x00030740
		internal static _IStructureInitialization \u0001(List<IAssignmentExpression> \u0002)
		{
			return \u0003.Builder.CreateStructureInitialisation(null, \u0002) as _IStructureInitialization;
		}

		// Token: 0x060011B7 RID: 4535 RVA: 0x00032554 File Offset: 0x00030754
		internal static _IDefineReference \u0001(string \u0002)
		{
			return \u0003.Builder.CreateDefineReference(null, \u0002);
		}

		// Token: 0x060011B8 RID: 4536 RVA: 0x00032564 File Offset: 0x00030764
		internal static _IDefineReference \u0001(IToken \u0002, string \u0003)
		{
			return \u0003.Builder.CreateDefineReference(\u0002, \u0003);
		}

		// Token: 0x060011B9 RID: 4537 RVA: 0x00032574 File Offset: 0x00030774
		internal static _IVariableReference \u0001(IExpression \u0002)
		{
			return \u0003.Builder.CreateVariableReference(null, \u0002) as _IVariableReference;
		}

		// Token: 0x060011BA RID: 4538 RVA: 0x00032588 File Offset: 0x00030788
		internal static _ITypeReference \u0001(IExpression \u0002)
		{
			return \u0003.Builder.CreateTypeReference(null, \u0002) as _ITypeReference;
		}

		// Token: 0x060011BB RID: 4539 RVA: 0x0003259C File Offset: 0x0003079C
		internal static _IPouReference \u0001(IExpression \u0002)
		{
			return \u0003.Builder.CreatePouReference(null, \u0002) as _IPouReference;
		}

		// Token: 0x060011BC RID: 4540 RVA: 0x000325B0 File Offset: 0x000307B0
		internal static _IDefinedExpression \u0001(IExpression \u0002)
		{
			return \u0003.Builder.CreateDefinedExpression(null, \u0002) as _IDefinedExpression;
		}

		// Token: 0x060011BD RID: 4541 RVA: 0x000325C4 File Offset: 0x000307C4
		internal static _ICompilerVersionExpression \u0001(Version \u0002, Operator \u0003)
		{
			return \u0003.Builder.CreateCompilerVersioExpression(null, \u0002, \u0003) as _ICompilerVersionExpression;
		}

		// Token: 0x060011BE RID: 4542 RVA: 0x000325D8 File Offset: 0x000307D8
		internal static _IHasTypeExpression \u0001(IVariableReference \u0002, ICompiledType \u0003)
		{
			return \u0003.Builder.CreateHasTypeExpression(null, \u0002, \u0003) as _IHasTypeExpression;
		}

		// Token: 0x060011BF RID: 4543 RVA: 0x000325EC File Offset: 0x000307EC
		internal static _IIsEnumTypeExpression \u0001(ICompiledType \u0002)
		{
			return \u0003.Builder.CreateIsEnumTypeExpression(null, \u0002) as _IIsEnumTypeExpression;
		}

		// Token: 0x060011C0 RID: 4544 RVA: 0x00032600 File Offset: 0x00030800
		internal static _IHasTypeExpression \u0002(IVariableReference \u0002, ICompiledType \u0003)
		{
			return \u0003.Builder.CreateHasCompatibleTypeExpression(null, \u0002, \u0003) as _IHasTypeExpression;
		}

		// Token: 0x060011C1 RID: 4545 RVA: 0x00032614 File Offset: 0x00030814
		internal static _IHasAttributeExpression \u0001(IExpression \u0002, string \u0003)
		{
			return \u0003.Builder.CreateHasAttributeExpression(null, \u0002, \u0003) as _IHasAttributeExpression;
		}

		// Token: 0x060011C2 RID: 4546 RVA: 0x00032628 File Offset: 0x00030828
		internal static _IHasValueExpression \u0001(string \u0002, string \u0003)
		{
			return \u0003.Builder.CreateHasValueExpression(null, \u0002, \u0003);
		}

		// Token: 0x060011C3 RID: 4547 RVA: 0x00032638 File Offset: 0x00030838
		internal static _IPragmaOperatorExpression \u0001(Operator \u0002, IExpression \u0003, IExpression \u0004)
		{
			return \u0003.Builder.CreatePragmaOperatorExpression(null, \u0002, \u0003, \u0004) as _IPragmaOperatorExpression;
		}

		// Token: 0x060011C4 RID: 4548 RVA: 0x00032650 File Offset: 0x00030850
		internal static _IPragmaAssertion \u0001(IExpression \u0002, string \u0003)
		{
			return \u0003.Builder.CreatePragmaAssertion(null, \u0002, \u0003) as _IPragmaAssertion;
		}

		// Token: 0x060011C5 RID: 4549 RVA: 0x00032664 File Offset: 0x00030864
		internal static _IPragmaIfStatement \u0001(IExpression \u0002, ISequenceStatement2 \u0003, ISequenceStatement2 \u0004)
		{
			return \u0003.Builder.CreatePragmaIfStatement(null, \u0002, \u0003, \u0004) as _IPragmaIfStatement;
		}

		// Token: 0x060011C6 RID: 4550 RVA: 0x0003267C File Offset: 0x0003087C
		internal static _IBreakPointStatement \u0001(long \u0002, long \u0003)
		{
			return \u0003.Builder.CreateBreakpointStatement(null, \u0002, \u0003) as _IBreakPointStatement;
		}

		// Token: 0x060011C7 RID: 4551 RVA: 0x00032690 File Offset: 0x00030890
		internal static _IDefineStatement \u0001(bool \u0002, string \u0003, string \u0004)
		{
			return \u0003.Builder.CreateDefineStatement(null, \u0002, \u0003, \u0004);
		}

		// Token: 0x060011C8 RID: 4552 RVA: 0x000326A0 File Offset: 0x000308A0
		internal static _ICompilerMessage \u0001()
		{
			return \u0003.Builder.CreateCompilerMessage();
		}

		// Token: 0x060011C9 RID: 4553 RVA: 0x000326AC File Offset: 0x000308AC
		internal static _ICompilerMessage \u0001(IExprementPosition \u0002, IExprement \u0003, string \u0004, Severity \u0005, ShowAttribute \u0006, uint \u0007, string \u0008)
		{
			return \u0003.Builder.CreateCompilerMessage(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, \u0008) as _ICompilerMessage;
		}

		// Token: 0x060011CA RID: 4554 RVA: 0x000326C8 File Offset: 0x000308C8
		internal static _ICompilerMessage \u0001(ISourcePosition \u0002, string \u0003, Severity \u0004, MessageId \u0005)
		{
			return \u0003.Builder.CreateCompilerMessage(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x060011CB RID: 4555 RVA: 0x000326D8 File Offset: 0x000308D8
		internal static _ICompilerMessage \u0001(IMinimalPosition \u0002, string \u0003, Severity \u0004, short \u0005, MessageId \u0006)
		{
			return \u0003.Builder.CreateCompilerMessage(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x060011CC RID: 4556 RVA: 0x000326EC File Offset: 0x000308EC
		internal static _ISequenceStatement \u0001(IEnumerable<IStatement> \u0002)
		{
			return \u0003.Builder.CreateSequenceStatementEx(null, \u0002) as _ISequenceStatement;
		}

		// Token: 0x060011CD RID: 4557 RVA: 0x00032700 File Offset: 0x00030900
		internal static _IArrayInitialization \u0001(IEnumerable<IExpression> \u0002)
		{
			return \u0003.Builder.CreateArrayInitialisationEx(null, \u0002) as _IArrayInitialization;
		}

		// Token: 0x060011CE RID: 4558 RVA: 0x00032714 File Offset: 0x00030914
		internal static _IStructureInitialization \u0001(IEnumerable<IAssignmentExpression> \u0002)
		{
			return \u0003.Builder.CreateStructureInitialisationEx(null, \u0002) as _IStructureInitialization;
		}

		// Token: 0x060011CF RID: 4559 RVA: 0x00032728 File Offset: 0x00030928
		internal static _IStatement \u0001(string \u0002)
		{
			return \u0003.Builder.CreatePragmaStatement2(null, \u0002) as _IStatement;
		}

		// Token: 0x060011D0 RID: 4560 RVA: 0x0003273C File Offset: 0x0003093C
		internal static _ICallExpression \u0001(IExpression \u0002, IExpression \u0003, ICompiledType \u0004, IEnumerable<IExpression> \u0005, IEnumerable<IAssignmentExpression> \u0006)
		{
			return \u0003.Builder.CreateNonFormalCallExpression(null, \u0002, \u0003, \u0004, \u0005, \u0006) as _ICallExpression;
		}

		// Token: 0x060011D1 RID: 4561 RVA: 0x00032754 File Offset: 0x00030954
		internal static _IExpressionStatement \u0001(IExpression \u0002, IExpression \u0003, ICompiledType \u0004, IEnumerable<IExpression> \u0005, IEnumerable<IAssignmentExpression> \u0006)
		{
			return \u0003.Builder.CreateNonFormalCallStatement(null, \u0002, \u0003, \u0004, \u0005, \u0006) as _IExpressionStatement;
		}

		// Token: 0x060011D2 RID: 4562 RVA: 0x0003276C File Offset: 0x0003096C
		internal static _IPragmaStatement \u0001(string \u0002, string \u0003)
		{
			return \u0003.Builder.CreatePragmaAttributeStatement(null, \u0002, \u0003) as _IPragmaStatement;
		}

		// Token: 0x060011D3 RID: 4563 RVA: 0x00032780 File Offset: 0x00030980
		internal static _INullExpression \u0001(IExprementPosition \u0002)
		{
			return \u0003.Builder.CreateNullExpression(\u0002) as _INullExpression;
		}

		// Token: 0x060011D4 RID: 4564 RVA: 0x00032794 File Offset: 0x00030994
		internal static _IHasCompatibleTypeExpression \u0001()
		{
			return \u0003.Builder.CreateHasCompatibleTypeExpression();
		}

		// Token: 0x060011D5 RID: 4565 RVA: 0x000327A0 File Offset: 0x000309A0
		internal static _IXStringType \u0001()
		{
			return \u0003.Builder.CreateXStringtype();
		}

		// Token: 0x060011D6 RID: 4566 RVA: 0x000327AC File Offset: 0x000309AC
		internal static _ISubrangeType \u0001(_IExpression \u0002, _IExpression \u0003)
		{
			return \u0003.Builder.CreateSubrangeType(\u0002, \u0003);
		}

		// Token: 0x060011D7 RID: 4567 RVA: 0x000327BC File Offset: 0x000309BC
		internal static _IExitStatement \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateExitStatement(\u0002);
		}

		// Token: 0x060011D8 RID: 4568 RVA: 0x000327CC File Offset: 0x000309CC
		internal static _IContinueStatement \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateContinueStatement(\u0002);
		}

		// Token: 0x060011D9 RID: 4569 RVA: 0x000327DC File Offset: 0x000309DC
		internal static _IErrorStatement \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateErrorStatement(\u0002);
		}

		// Token: 0x060011DA RID: 4570 RVA: 0x000327EC File Offset: 0x000309EC
		internal static _IErrorStatement \u0001()
		{
			return \u0003.Builder.CreateErrorStatement();
		}

		// Token: 0x060011DB RID: 4571 RVA: 0x000327F8 File Offset: 0x000309F8
		internal static _IEmptyStatement \u0001()
		{
			return \u0003.Builder.CreateEmptyStatement();
		}

		// Token: 0x060011DC RID: 4572 RVA: 0x00032804 File Offset: 0x00030A04
		internal static _IEmptyStatement \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateEmptyStatement(\u0002);
		}

		// Token: 0x060011DD RID: 4573 RVA: 0x00032814 File Offset: 0x00030A14
		internal static _IVarInitialEmptyStatement \u0001(_IVariable \u0002, _ISignature \u0003)
		{
			return \u0003.Builder.CreateVarInitialEmptyStatement(\u0002, \u0003);
		}

		// Token: 0x060011DE RID: 4574 RVA: 0x00032824 File Offset: 0x00030A24
		internal static _IWhileStatement \u0001()
		{
			return \u0003.Builder.CreateWhileStatement();
		}

		// Token: 0x060011DF RID: 4575 RVA: 0x00032830 File Offset: 0x00030A30
		internal static _IWhileStatement \u0001(_IExpression \u0002, _IStatement \u0003)
		{
			return \u0003.Builder.CreateWhileStatement(\u0002, \u0003);
		}

		// Token: 0x060011E0 RID: 4576 RVA: 0x00032840 File Offset: 0x00030A40
		internal static _IWhileStatement \u0001(_IExpression \u0002, _IStatement \u0003, IToken \u0004)
		{
			return \u0003.Builder.CreateWhileStatement(\u0002, \u0003, \u0004);
		}

		// Token: 0x060011E1 RID: 4577 RVA: 0x00032850 File Offset: 0x00030A50
		internal static _IRepeatStatement \u0001()
		{
			return \u0003.Builder.CreateRepeatStatement();
		}

		// Token: 0x060011E2 RID: 4578 RVA: 0x0003285C File Offset: 0x00030A5C
		internal static _IRepeatStatement \u0001(_IExpression \u0002, _IStatement \u0003)
		{
			return \u0003.Builder.CreateRepeatStatement(\u0002, \u0003);
		}

		// Token: 0x060011E3 RID: 4579 RVA: 0x0003286C File Offset: 0x00030A6C
		internal static _IRepeatStatement \u0001(_IExpression \u0002, _IStatement \u0003, IToken \u0004)
		{
			return \u0003.Builder.CreateRepeatStatement(\u0002, \u0003, \u0004);
		}

		// Token: 0x060011E4 RID: 4580 RVA: 0x0003287C File Offset: 0x00030A7C
		internal static _ICaseRangeExpression \u0001()
		{
			return \u0003.Builder.CreateCaseRangeExpression();
		}

		// Token: 0x060011E5 RID: 4581 RVA: 0x00032888 File Offset: 0x00030A88
		internal static _ICaseRangeExpression \u0001(_IExpression \u0002, _IExpression \u0003, IToken \u0004)
		{
			return \u0003.Builder.CreateCaseRangeExpression(\u0002, \u0003, \u0004);
		}

		// Token: 0x060011E6 RID: 4582 RVA: 0x00032898 File Offset: 0x00030A98
		internal static _ICaseLabelStatement \u0001()
		{
			return \u0003.Builder.CreateCaseLabelStatement();
		}

		// Token: 0x060011E7 RID: 4583 RVA: 0x000328A4 File Offset: 0x00030AA4
		internal static _ICaseLabelStatement \u0001(_IExpression \u0002)
		{
			return \u0003.Builder.CreateCaseLabelStatement(\u0002);
		}

		// Token: 0x060011E8 RID: 4584 RVA: 0x000328B4 File Offset: 0x00030AB4
		internal static _ICaseLabelStatement \u0001(_IExpression \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateCaseLabelStatement(\u0002, \u0003);
		}

		// Token: 0x060011E9 RID: 4585 RVA: 0x000328C4 File Offset: 0x00030AC4
		internal static _ICaseLabelStatement \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateCaseLabelStatement(\u0002);
		}

		// Token: 0x060011EA RID: 4586 RVA: 0x000328D4 File Offset: 0x00030AD4
		internal static _ICase \u0001()
		{
			return \u0003.Builder.CreateCase();
		}

		// Token: 0x060011EB RID: 4587 RVA: 0x000328E0 File Offset: 0x00030AE0
		internal static _ICase \u0001(_ICaseLabelStatement \u0002, _IStatement \u0003)
		{
			return \u0003.Builder.CreateCase(\u0002, \u0003);
		}

		// Token: 0x060011EC RID: 4588 RVA: 0x000328F0 File Offset: 0x00030AF0
		internal static _ICaseStatement \u0001()
		{
			return \u0003.Builder.CreateCaseStatement();
		}

		// Token: 0x060011ED RID: 4589 RVA: 0x000328FC File Offset: 0x00030AFC
		internal static _ICaseStatement \u0001(_IExpression \u0002)
		{
			return \u0003.Builder.CreateCaseStatement(\u0002);
		}

		// Token: 0x060011EE RID: 4590 RVA: 0x0003290C File Offset: 0x00030B0C
		internal static _ICaseStatement \u0001(_IExpression \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateCaseStatement(\u0002, \u0003);
		}

		// Token: 0x060011EF RID: 4591 RVA: 0x0003291C File Offset: 0x00030B1C
		internal static _IExitStatement \u0001()
		{
			return \u0003.Builder.CreateExitStatement();
		}

		// Token: 0x060011F0 RID: 4592 RVA: 0x00032928 File Offset: 0x00030B28
		internal static _IContinueStatement \u0001()
		{
			return \u0003.Builder.CreateContinueStatement();
		}

		// Token: 0x060011F1 RID: 4593 RVA: 0x00032934 File Offset: 0x00030B34
		internal static _ISequenceStatement \u0001()
		{
			return \u0003.Builder.CreateSequenceStatement();
		}

		// Token: 0x060011F2 RID: 4594 RVA: 0x00032940 File Offset: 0x00030B40
		internal static _ISequenceStatement \u0001(int \u0002)
		{
			return \u0003.Builder.CreateSequenceStatement(\u0002);
		}

		// Token: 0x060011F3 RID: 4595 RVA: 0x00032950 File Offset: 0x00030B50
		internal static _ISequenceStatement \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateSequenceStatement(\u0002);
		}

		// Token: 0x060011F4 RID: 4596 RVA: 0x00032960 File Offset: 0x00030B60
		internal static _ISubRoutineStatement \u0001()
		{
			return \u0003.Builder.CreateSubRoutineStatement();
		}

		// Token: 0x060011F5 RID: 4597 RVA: 0x0003296C File Offset: 0x00030B6C
		internal static _IAssignmentExpression \u0001()
		{
			return \u0003.Builder.CreateAssignmentExpression();
		}

		// Token: 0x060011F6 RID: 4598 RVA: 0x00032978 File Offset: 0x00030B78
		internal static _IAssignmentExpression \u0001(_IExpression \u0002)
		{
			return \u0003.Builder.CreateAssignmentExpression(\u0002);
		}

		// Token: 0x060011F7 RID: 4599 RVA: 0x00032988 File Offset: 0x00030B88
		internal static _IAssignmentExpression \u0001(_IExpression \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateAssignmentExpression(\u0002, \u0003);
		}

		// Token: 0x060011F8 RID: 4600 RVA: 0x00032998 File Offset: 0x00030B98
		internal static _IElseIf \u0001(_IExpression \u0002, _IStatement \u0003)
		{
			return \u0003.Builder.CreateElseIf(\u0002, \u0003);
		}

		// Token: 0x060011F9 RID: 4601 RVA: 0x000329A8 File Offset: 0x00030BA8
		internal static _IElseIf \u0001()
		{
			return \u0003.Builder.CreateElseIf();
		}

		// Token: 0x060011FA RID: 4602 RVA: 0x000329B4 File Offset: 0x00030BB4
		internal static _IIfStatement \u0001()
		{
			return \u0003.Builder.CreateIfStatement();
		}

		// Token: 0x060011FB RID: 4603 RVA: 0x000329C0 File Offset: 0x00030BC0
		internal static _IIfStatement \u0001(_IExpression \u0002)
		{
			return \u0003.Builder.CreateIfStatement(\u0002);
		}

		// Token: 0x060011FC RID: 4604 RVA: 0x000329D0 File Offset: 0x00030BD0
		internal static _IIfStatement \u0001(_IExpression \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateIfStatement(\u0002, \u0003);
		}

		// Token: 0x060011FD RID: 4605 RVA: 0x000329E0 File Offset: 0x00030BE0
		internal static _ITryCatchStatement \u0001()
		{
			return \u0003.Builder.CreateTryCatchStatement();
		}

		// Token: 0x060011FE RID: 4606 RVA: 0x000329EC File Offset: 0x00030BEC
		internal static _ITryCatchStatement \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateTryCatchStatement(\u0002);
		}

		// Token: 0x060011FF RID: 4607 RVA: 0x000329FC File Offset: 0x00030BFC
		internal static _IReturnStatement \u0001()
		{
			return \u0003.Builder.CreateReturnStatement();
		}

		// Token: 0x06001200 RID: 4608 RVA: 0x00032A08 File Offset: 0x00030C08
		internal static _IReturnStatement \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateReturnStatement(\u0002);
		}

		// Token: 0x06001201 RID: 4609 RVA: 0x00032A18 File Offset: 0x00030C18
		internal static _IJumpStatement \u0001()
		{
			return \u0003.Builder.CreateJumpStatement();
		}

		// Token: 0x06001202 RID: 4610 RVA: 0x00032A24 File Offset: 0x00030C24
		internal static _IJumpStatement \u0001(string \u0002)
		{
			return \u0003.Builder.CreateJumpStatement(\u0002);
		}

		// Token: 0x06001203 RID: 4611 RVA: 0x00032A34 File Offset: 0x00030C34
		internal static _IJumpStatement \u0001(string \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateJumpStatement(\u0002, \u0003);
		}

		// Token: 0x06001204 RID: 4612 RVA: 0x00032A44 File Offset: 0x00030C44
		internal static _ILabelStatement \u0001()
		{
			return \u0003.Builder.CreateLabelStatement();
		}

		// Token: 0x06001205 RID: 4613 RVA: 0x00032A50 File Offset: 0x00030C50
		internal static _ILabelStatement \u0001(string \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateLabelStatement(\u0002, \u0003);
		}

		// Token: 0x06001206 RID: 4614 RVA: 0x00032A60 File Offset: 0x00030C60
		internal static _ICommentStatement \u0001()
		{
			return \u0003.Builder.CreateCommentStatement();
		}

		// Token: 0x06001207 RID: 4615 RVA: 0x00032A6C File Offset: 0x00030C6C
		internal static _ICommentStatement \u0001(string \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateCommentStatement(\u0002, \u0003);
		}

		// Token: 0x06001208 RID: 4616 RVA: 0x00032A7C File Offset: 0x00030C7C
		internal static _IPragmaStatement \u0001()
		{
			return \u0003.Builder.CreatePragmaStatement();
		}

		// Token: 0x06001209 RID: 4617 RVA: 0x00032A88 File Offset: 0x00030C88
		internal static _IPragmaStatement \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreatePragmaStatement(\u0002);
		}

		// Token: 0x0600120A RID: 4618 RVA: 0x00032A98 File Offset: 0x00030C98
		internal static _IPragmaStatement \u0001(string \u0002)
		{
			_IPragmaStatement ipragmaStatement = \u0003.\u0001();
			ipragmaStatement.Text = \u0002;
			return ipragmaStatement;
		}

		// Token: 0x0600120B RID: 4619 RVA: 0x00032AA8 File Offset: 0x00030CA8
		internal static _IMessageGuidPragmaStatement \u0001(IToken \u0002, Guid \u0003)
		{
			return \u0003.Builder.CreateMessageGuidPragmaStatement(\u0002, \u0003);
		}

		// Token: 0x0600120C RID: 4620 RVA: 0x00032AB8 File Offset: 0x00030CB8
		internal static _IWarningDisableRestorePragmaStatement \u0001()
		{
			return \u0003.Builder.CreateWarningDisableRestorePragmaStatement();
		}

		// Token: 0x0600120D RID: 4621 RVA: 0x00032AC4 File Offset: 0x00030CC4
		internal static _IWarningDisableRestorePragmaStatement \u0001(IToken \u0002, bool \u0003, string \u0004)
		{
			return \u0003.Builder.CreateWarningDisableRestorePragmaStatement(\u0002, \u0003, \u0004);
		}

		// Token: 0x0600120E RID: 4622 RVA: 0x00032AD4 File Offset: 0x00030CD4
		internal static _IExpressionStatement \u0001()
		{
			return \u0003.Builder.CreateExpressionStatement();
		}

		// Token: 0x0600120F RID: 4623 RVA: 0x00032AE0 File Offset: 0x00030CE0
		internal static _IExpressionStatement \u0001(_IExpression \u0002)
		{
			return \u0003.Builder.CreateExpressionStatement(\u0002);
		}

		// Token: 0x06001210 RID: 4624 RVA: 0x00032AF0 File Offset: 0x00030CF0
		internal static _IExpressionStatement \u0001(_IExpression \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateExpressionStatement(\u0002, \u0003);
		}

		// Token: 0x06001211 RID: 4625 RVA: 0x00032B00 File Offset: 0x00030D00
		internal static _IPOUDeclarationStatement \u0001()
		{
			return \u0003.Builder.CreatePOUDeclarationStatement();
		}

		// Token: 0x06001212 RID: 4626 RVA: 0x00032B0C File Offset: 0x00030D0C
		internal static _IPOUDeclarationStatement \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreatePOUDeclarationStatement(\u0002);
		}

		// Token: 0x06001213 RID: 4627 RVA: 0x00032B1C File Offset: 0x00030D1C
		internal static _IVariableDeclarationListStatement \u0001()
		{
			return \u0003.Builder.CreateVariableDeclarationListStatement();
		}

		// Token: 0x06001214 RID: 4628 RVA: 0x00032B28 File Offset: 0x00030D28
		internal static _IVariableDeclarationListStatement \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateVariableDeclarationListStatement(\u0002);
		}

		// Token: 0x06001215 RID: 4629 RVA: 0x00032B38 File Offset: 0x00030D38
		internal static _IVariableDeclarationStatement \u0001()
		{
			return \u0003.Builder.CreateVariableDeclarationStatement();
		}

		// Token: 0x06001216 RID: 4630 RVA: 0x00032B44 File Offset: 0x00030D44
		internal static _IVariableDeclarationStatement \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateVariableDeclarationStatement(\u0002);
		}

		// Token: 0x06001217 RID: 4631 RVA: 0x00032B54 File Offset: 0x00030D54
		internal static _IErrorExpression \u0001()
		{
			return \u0003.Builder.CreateErrorExpression();
		}

		// Token: 0x06001218 RID: 4632 RVA: 0x00032B60 File Offset: 0x00030D60
		internal static _IErrorExpression \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateErrorExpression(\u0002);
		}

		// Token: 0x06001219 RID: 4633 RVA: 0x00032B70 File Offset: 0x00030D70
		internal static _IProgramCounterExpression \u0001()
		{
			return \u0003.Builder.CreateProgramCounterExpression();
		}

		// Token: 0x0600121A RID: 4634 RVA: 0x00032B7C File Offset: 0x00030D7C
		internal static _IFramePointerExpression \u0001()
		{
			return \u0003.Builder.CreateFramePointerExpression();
		}

		// Token: 0x0600121B RID: 4635 RVA: 0x00032B88 File Offset: 0x00030D88
		internal static _ICallInstanceExpression \u0001(bool \u0002, ICompiledType \u0003, IIntermediateValueLocation \u0004)
		{
			return \u0003.Builder.CreateCallInstanceExpression(\u0002, \u0003, \u0004);
		}

		// Token: 0x0600121C RID: 4636 RVA: 0x00032B98 File Offset: 0x00030D98
		internal static _ICallExpression \u0001()
		{
			return \u0003.Builder.CreateCallExpression();
		}

		// Token: 0x0600121D RID: 4637 RVA: 0x00032BA4 File Offset: 0x00030DA4
		internal static _ICallExpression \u0001(_IExpression \u0002)
		{
			return \u0003.Builder.CreateCallExpression(\u0002);
		}

		// Token: 0x0600121E RID: 4638 RVA: 0x00032BB4 File Offset: 0x00030DB4
		internal static _ICallExpression \u0001(_IExpression \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateCallExpression(\u0002, \u0003);
		}

		// Token: 0x0600121F RID: 4639 RVA: 0x00032BC4 File Offset: 0x00030DC4
		internal static _IOperatorExpression \u0001()
		{
			return \u0003.Builder.CreateOperatorExpression();
		}

		// Token: 0x06001220 RID: 4640 RVA: 0x00032BD0 File Offset: 0x00030DD0
		internal static _IOperatorExpression \u0001(Operator \u0002)
		{
			return \u0003.Builder.CreateOperatorExpression(\u0002);
		}

		// Token: 0x06001221 RID: 4641 RVA: 0x00032BE0 File Offset: 0x00030DE0
		internal static _IOperatorExpression \u0001(Operator \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateOperatorExpression(\u0002, \u0003);
		}

		// Token: 0x06001222 RID: 4642 RVA: 0x00032BF0 File Offset: 0x00030DF0
		internal static _IConversionExpression \u0001()
		{
			return \u0003.Builder.CreateConversionExpression();
		}

		// Token: 0x06001223 RID: 4643 RVA: 0x00032BFC File Offset: 0x00030DFC
		internal static _IConversionExpression \u0001(TypeClass \u0002, TypeClass \u0003)
		{
			return \u0003.Builder.CreateConversionExpression(\u0002, \u0003);
		}

		// Token: 0x06001224 RID: 4644 RVA: 0x00032C0C File Offset: 0x00030E0C
		internal static _IConversionExpression \u0001(TypeClass \u0002, TypeClass \u0003, IToken \u0004)
		{
			return \u0003.Builder.CreateConversionExpression(\u0002, \u0003, \u0004);
		}

		// Token: 0x06001225 RID: 4645 RVA: 0x00032C1C File Offset: 0x00030E1C
		internal static _IImplicitConversionExpression \u0001()
		{
			return \u0003.Builder.CreateImplicitConversionExpression();
		}

		// Token: 0x06001226 RID: 4646 RVA: 0x00032C28 File Offset: 0x00030E28
		internal static _IImplicitConversionExpression \u0001(TypeClass \u0002, TypeClass \u0003)
		{
			return \u0003.Builder.CreateImplicitConversionExpression(\u0002, \u0003);
		}

		// Token: 0x06001227 RID: 4647 RVA: 0x00032C38 File Offset: 0x00030E38
		internal static _IImplicitConversionExpression \u0001(TypeClass \u0002, TypeClass \u0003, IToken \u0004)
		{
			return \u0003.Builder.CreateImplicitConversionExpression(\u0002, \u0003, \u0004);
		}

		// Token: 0x06001228 RID: 4648 RVA: 0x00032C48 File Offset: 0x00030E48
		internal static _INewExpression \u0001()
		{
			return \u0003.Builder.CreateNewExpression();
		}

		// Token: 0x06001229 RID: 4649 RVA: 0x00032C54 File Offset: 0x00030E54
		internal static _INewExpression \u0001(_IType \u0002, _IExpression \u0003)
		{
			return \u0003.Builder.CreateNewExpression(\u0002, \u0003);
		}

		// Token: 0x0600122A RID: 4650 RVA: 0x00032C64 File Offset: 0x00030E64
		internal static _INewExpression \u0001(_IType \u0002, _IExpression \u0003, IToken \u0004)
		{
			return \u0003.Builder.CreateNewExpression(\u0002, \u0003, \u0004);
		}

		// Token: 0x0600122B RID: 4651 RVA: 0x00032C74 File Offset: 0x00030E74
		internal static _ICastExpression \u0001()
		{
			return \u0003.Builder.CreateCastExpression();
		}

		// Token: 0x0600122C RID: 4652 RVA: 0x00032C80 File Offset: 0x00030E80
		internal static _ICastExpression \u0001(_IExpression \u0002, _IExpression \u0003)
		{
			return \u0003.Builder.CreateCastExpression(\u0002, \u0003);
		}

		// Token: 0x0600122D RID: 4653 RVA: 0x00032C90 File Offset: 0x00030E90
		internal static _IThisExpression \u0001()
		{
			return \u0003.Builder.CreateThisExpression();
		}

		// Token: 0x0600122E RID: 4654 RVA: 0x00032C9C File Offset: 0x00030E9C
		internal static _IThisExpression \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateThisExpression(\u0002);
		}

		// Token: 0x0600122F RID: 4655 RVA: 0x00032CAC File Offset: 0x00030EAC
		internal static _IBaseExpression \u0001()
		{
			return \u0003.Builder.CreateBaseExpression();
		}

		// Token: 0x06001230 RID: 4656 RVA: 0x00032CB8 File Offset: 0x00030EB8
		internal static _IBaseExpression \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateBaseExpression(\u0002);
		}

		// Token: 0x06001231 RID: 4657 RVA: 0x00032CC8 File Offset: 0x00030EC8
		internal static _ITypeExpression \u0001()
		{
			return \u0003.Builder.CreateTypeExpression();
		}

		// Token: 0x06001232 RID: 4658 RVA: 0x00032CD4 File Offset: 0x00030ED4
		internal static _IAddressExpression \u0001()
		{
			return \u0003.Builder.CreateAddressExpression();
		}

		// Token: 0x06001233 RID: 4659 RVA: 0x00032CE0 File Offset: 0x00030EE0
		internal static _IAddressExpression \u0001(IDirectVariable \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateAddressExpression(\u0002, \u0003);
		}

		// Token: 0x06001234 RID: 4660 RVA: 0x00032CF0 File Offset: 0x00030EF0
		internal static _IVariableExpression \u0001(string \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateVariableExpression(\u0002, \u0003);
		}

		// Token: 0x06001235 RID: 4661 RVA: 0x00032D00 File Offset: 0x00030F00
		internal static _ICompoAccessExpression \u0001()
		{
			return \u0003.Builder.CreateCompoAccessExpression();
		}

		// Token: 0x06001236 RID: 4662 RVA: 0x00032D0C File Offset: 0x00030F0C
		internal static _ICompoAccessExpression \u0001(_IExpression \u0002)
		{
			return \u0003.Builder.CreateCompoAccessExpression(\u0002);
		}

		// Token: 0x06001237 RID: 4663 RVA: 0x00032D1C File Offset: 0x00030F1C
		internal static _ICompoAccessExpression \u0001(_IExpression \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateCompoAccessExpression(\u0002, \u0003);
		}

		// Token: 0x06001238 RID: 4664 RVA: 0x00032D2C File Offset: 0x00030F2C
		internal static _IDeRefAccessExpression \u0001()
		{
			return \u0003.Builder.CreateDeRefAccessExpression();
		}

		// Token: 0x06001239 RID: 4665 RVA: 0x00032D38 File Offset: 0x00030F38
		internal static _IDeRefAccessExpression \u0001(_IExpression \u0002)
		{
			return \u0003.Builder.CreateDeRefAccessExpression(\u0002);
		}

		// Token: 0x0600123A RID: 4666 RVA: 0x00032D48 File Offset: 0x00030F48
		internal static _IDeRefAccessExpression \u0001(_IExpression \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateDeRefAccessExpression(\u0002, \u0003);
		}

		// Token: 0x0600123B RID: 4667 RVA: 0x00032D58 File Offset: 0x00030F58
		internal static _IIndexAccessExpression \u0001()
		{
			return \u0003.Builder.CreateIndexAccessExpression();
		}

		// Token: 0x0600123C RID: 4668 RVA: 0x00032D64 File Offset: 0x00030F64
		internal static _IIndexAccessExpression \u0001(_IExpression \u0002)
		{
			return \u0003.Builder.CreateIndexAccessExpression(\u0002);
		}

		// Token: 0x0600123D RID: 4669 RVA: 0x00032D74 File Offset: 0x00030F74
		internal static _IIndexAccessExpression \u0001(_IExpression \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateIndexAccessExpression(\u0002, \u0003);
		}

		// Token: 0x0600123E RID: 4670 RVA: 0x00032D84 File Offset: 0x00030F84
		internal static _IPartialAccessExpression \u0001(_IExpression \u0002, DirectVariableSize \u0003, int \u0004)
		{
			return \u0003.Builder.CreatePartialAccessExpression(\u0002, \u0003, \u0004);
		}

		// Token: 0x0600123F RID: 4671 RVA: 0x00032D94 File Offset: 0x00030F94
		internal static _IPartialAccessExpression \u0001(IToken \u0002, _IExpression \u0003, DirectVariableSize \u0004, int \u0005)
		{
			return \u0003.Builder.CreatePartialAccessExpression(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06001240 RID: 4672 RVA: 0x00032DA4 File Offset: 0x00030FA4
		internal static _ICopyScopeExpression \u0001()
		{
			return \u0003.Builder.CreateCopyScopeExpression();
		}

		// Token: 0x06001241 RID: 4673 RVA: 0x00032DB0 File Offset: 0x00030FB0
		internal static _ICopyScopeExpression \u0001(_IExpression \u0002)
		{
			return \u0003.Builder.CreateCopyScopeExpression(\u0002);
		}

		// Token: 0x06001242 RID: 4674 RVA: 0x00032DC0 File Offset: 0x00030FC0
		internal static _ICopyScopeExpression \u0001(_IExpression \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateCopyScopeExpression(\u0002, \u0003);
		}

		// Token: 0x06001243 RID: 4675 RVA: 0x00032DD0 File Offset: 0x00030FD0
		internal static _IGlobalScopeExpression \u0001()
		{
			return \u0003.Builder.CreateGlobalScopeExpression();
		}

		// Token: 0x06001244 RID: 4676 RVA: 0x00032DDC File Offset: 0x00030FDC
		internal static _IGlobalScopeExpression \u0001(_IExpression \u0002)
		{
			return \u0003.Builder.CreateGlobalScopeExpression(\u0002);
		}

		// Token: 0x06001245 RID: 4677 RVA: 0x00032DEC File Offset: 0x00030FEC
		internal static _IGlobalScopeExpression \u0001(_IExpression \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateGlobalScopeExpression(\u0002, \u0003);
		}

		// Token: 0x06001246 RID: 4678 RVA: 0x00032DFC File Offset: 0x00030FFC
		internal static _ISystemScopeExpression \u0001()
		{
			return \u0003.Builder.CreateSystemScopeExpression();
		}

		// Token: 0x06001247 RID: 4679 RVA: 0x00032E08 File Offset: 0x00031008
		internal static _ISystemScopeExpression \u0001(string \u0002)
		{
			return \u0003.Builder.CreateSystemScopeExpression(\u0002);
		}

		// Token: 0x06001248 RID: 4680 RVA: 0x00032E18 File Offset: 0x00031018
		internal static _ISystemScopeExpression \u0001(_IExpression \u0002)
		{
			return \u0003.Builder.CreateSystemScopeExpression(\u0002);
		}

		// Token: 0x06001249 RID: 4681 RVA: 0x00032E28 File Offset: 0x00031028
		internal static _ISystemScopeExpression \u0001(_IExpression \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateSystemScopeExpression(\u0002, \u0003);
		}

		// Token: 0x0600124A RID: 4682 RVA: 0x00032E38 File Offset: 0x00031038
		internal static _IPoolScopeExpression \u0001(_IExpression \u0002)
		{
			return \u0003.Builder.CreatePoolScopeExpression(\u0002);
		}

		// Token: 0x0600124B RID: 4683 RVA: 0x00032E48 File Offset: 0x00031048
		internal static _IPoolScopeExpression \u0001(_IExpression \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreatePoolScopeExpression(\u0002, \u0003);
		}

		// Token: 0x0600124C RID: 4684 RVA: 0x00032E58 File Offset: 0x00031058
		internal static _INamespaceAccessExpression \u0001(_IExpression \u0002, _IExpression \u0003)
		{
			return \u0003.Builder.CreateNamespaceAccessExpression(\u0002, \u0003);
		}

		// Token: 0x0600124D RID: 4685 RVA: 0x00032E68 File Offset: 0x00031068
		internal static _INamespaceAccessExpression \u0001(_IExpression \u0002, _IExpression \u0003, IToken \u0004)
		{
			return \u0003.Builder.CreateNamespaceAccessExpression(\u0002, \u0003, \u0004);
		}

		// Token: 0x0600124E RID: 4686 RVA: 0x00032E78 File Offset: 0x00031078
		internal static _ICurrentTaskExpression \u0001(_IExpression \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreateCurrentTaskExpression(\u0002, \u0003);
		}

		// Token: 0x0600124F RID: 4687 RVA: 0x00032E88 File Offset: 0x00031088
		internal static _INullExpression \u0001()
		{
			return \u0003.Builder.CreateNullExpression();
		}

		// Token: 0x06001250 RID: 4688 RVA: 0x00032E94 File Offset: 0x00031094
		internal static _INullExpression \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateNullExpression(\u0002);
		}

		// Token: 0x06001251 RID: 4689 RVA: 0x00032EA4 File Offset: 0x000310A4
		internal static _INullStatement \u0001()
		{
			return \u0003.Builder.CreateNullStatement();
		}

		// Token: 0x06001252 RID: 4690 RVA: 0x00032EB0 File Offset: 0x000310B0
		internal static _INullStatement \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateNullStatement(\u0002);
		}

		// Token: 0x06001253 RID: 4691 RVA: 0x00032EC0 File Offset: 0x000310C0
		internal static _IMultipleIndexInitialization \u0001()
		{
			return \u0003.Builder.CreateMultipleIndexInitialisation();
		}

		// Token: 0x06001254 RID: 4692 RVA: 0x00032ECC File Offset: 0x000310CC
		internal static _IMultipleIndexInitialization \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateMultipleIndexInitialisation(\u0002);
		}

		// Token: 0x06001255 RID: 4693 RVA: 0x00032EDC File Offset: 0x000310DC
		internal static _IArrayInitialization \u0001()
		{
			return \u0003.Builder.CreateArrayInitialisation();
		}

		// Token: 0x06001256 RID: 4694 RVA: 0x00032EE8 File Offset: 0x000310E8
		internal static _IArrayInitialization \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateArrayInitialisation(\u0002);
		}

		// Token: 0x06001257 RID: 4695 RVA: 0x00032EF8 File Offset: 0x000310F8
		internal static _IStructureInitialization \u0001()
		{
			return \u0003.Builder.CreateStructureInitialisation();
		}

		// Token: 0x06001258 RID: 4696 RVA: 0x00032F04 File Offset: 0x00031104
		internal static _IStructureInitialization \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateStructureInitialisation(\u0002);
		}

		// Token: 0x06001259 RID: 4697 RVA: 0x00032F14 File Offset: 0x00031114
		internal static _IDefineReference \u0001()
		{
			return \u0003.Builder.CreateDefineReference();
		}

		// Token: 0x0600125A RID: 4698 RVA: 0x00032F20 File Offset: 0x00031120
		internal static _IDefineReference \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateDefineReference(\u0002);
		}

		// Token: 0x0600125B RID: 4699 RVA: 0x00032F30 File Offset: 0x00031130
		internal static _IVariableReference \u0001()
		{
			return \u0003.Builder.CreateVariableReference();
		}

		// Token: 0x0600125C RID: 4700 RVA: 0x00032F3C File Offset: 0x0003113C
		internal static _IVariableReference \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateVariableReference(\u0002);
		}

		// Token: 0x0600125D RID: 4701 RVA: 0x00032F4C File Offset: 0x0003114C
		internal static _ITypeReference \u0001()
		{
			return \u0003.Builder.CreateTypeReference();
		}

		// Token: 0x0600125E RID: 4702 RVA: 0x00032F58 File Offset: 0x00031158
		internal static _ITypeReference \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateTypeReference(\u0002);
		}

		// Token: 0x0600125F RID: 4703 RVA: 0x00032F68 File Offset: 0x00031168
		internal static _ITypeReference \u0001(IToken \u0002, _IExpression \u0003)
		{
			return \u0003.Builder.CreateTypeReference(\u0002, \u0003);
		}

		// Token: 0x06001260 RID: 4704 RVA: 0x00032F78 File Offset: 0x00031178
		internal static _IPouReference \u0001()
		{
			return \u0003.Builder.CreatePouReference();
		}

		// Token: 0x06001261 RID: 4705 RVA: 0x00032F84 File Offset: 0x00031184
		internal static _IPouReference \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreatePouReference(\u0002);
		}

		// Token: 0x06001262 RID: 4706 RVA: 0x00032F94 File Offset: 0x00031194
		internal static _IPouReference \u0001(IToken \u0002, _IExpression \u0003)
		{
			return \u0003.Builder.CreatePouReference(\u0002, \u0003);
		}

		// Token: 0x06001263 RID: 4707 RVA: 0x00032FA4 File Offset: 0x000311A4
		internal static _ITaskReference \u0001()
		{
			return \u0003.Builder.CreateTaskReference();
		}

		// Token: 0x06001264 RID: 4708 RVA: 0x00032FB0 File Offset: 0x000311B0
		internal static _ITaskReference \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateTaskReference(\u0002);
		}

		// Token: 0x06001265 RID: 4709 RVA: 0x00032FC0 File Offset: 0x000311C0
		internal static _ITaskReference \u0001(IToken \u0002, string \u0003)
		{
			return \u0003.Builder.CreateTaskReference(\u0002, \u0003);
		}

		// Token: 0x06001266 RID: 4710 RVA: 0x00032FD0 File Offset: 0x000311D0
		internal static _IResourceReference \u0001()
		{
			return \u0003.Builder.CreateResourceReference();
		}

		// Token: 0x06001267 RID: 4711 RVA: 0x00032FDC File Offset: 0x000311DC
		internal static _IResourceReference \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateResourceReference(\u0002);
		}

		// Token: 0x06001268 RID: 4712 RVA: 0x00032FEC File Offset: 0x000311EC
		internal static _IResourceReference \u0001(IToken \u0002, string \u0003)
		{
			return \u0003.Builder.CreateResourceReference(\u0002, \u0003);
		}

		// Token: 0x06001269 RID: 4713 RVA: 0x00032FFC File Offset: 0x000311FC
		internal static _IXRefExpression \u0001()
		{
			return \u0003.Builder.CreateXRefExpression();
		}

		// Token: 0x0600126A RID: 4714 RVA: 0x00033008 File Offset: 0x00031208
		internal static _IXRefExpression \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateXRefExpression(\u0002);
		}

		// Token: 0x0600126B RID: 4715 RVA: 0x00033018 File Offset: 0x00031218
		internal static _IXRefExpression \u0001(IToken \u0002, _IItemReference \u0003, _IItemReference \u0004)
		{
			return \u0003.Builder.CreateXRefExpression(\u0002, \u0003, \u0004);
		}

		// Token: 0x0600126C RID: 4716 RVA: 0x00033028 File Offset: 0x00031228
		internal static _IDefinedExpression \u0001()
		{
			return \u0003.Builder.CreateDefinedExpression();
		}

		// Token: 0x0600126D RID: 4717 RVA: 0x00033034 File Offset: 0x00031234
		internal static _IDefinedExpression \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateDefinedExpression(\u0002);
		}

		// Token: 0x0600126E RID: 4718 RVA: 0x00033044 File Offset: 0x00031244
		internal static _IProjectDefinedExpression \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateProjectDefinedExpression(\u0002);
		}

		// Token: 0x0600126F RID: 4719 RVA: 0x00033054 File Offset: 0x00031254
		internal static _IDefinedExpression \u0001(IToken \u0002, _IItemReference \u0003)
		{
			return \u0003.Builder.CreateDefinedExpression(\u0002, \u0003);
		}

		// Token: 0x06001270 RID: 4720 RVA: 0x00033064 File Offset: 0x00031264
		internal static _ICompilerVersionExpression \u0001()
		{
			return \u0003.Builder.CreateCompilerVersionExpression();
		}

		// Token: 0x06001271 RID: 4721 RVA: 0x00033070 File Offset: 0x00031270
		internal static _ICompilerVersionExpression \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateCompilerVersionExpression(\u0002);
		}

		// Token: 0x06001272 RID: 4722 RVA: 0x00033080 File Offset: 0x00031280
		internal static _ICompilerVersionExpression \u0001(IToken \u0002, Version \u0003, Operator \u0004)
		{
			return \u0003.Builder.CreateCompilerVersionExpression(\u0002, \u0003, \u0004);
		}

		// Token: 0x06001273 RID: 4723 RVA: 0x00033090 File Offset: 0x00031290
		internal static _IRuntimeVersionExpression \u0001()
		{
			return \u0003.Builder.CreateRuntimeVersionExpression();
		}

		// Token: 0x06001274 RID: 4724 RVA: 0x0003309C File Offset: 0x0003129C
		internal static _IRuntimeVersionExpression \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateRuntimeVersionExpression(\u0002);
		}

		// Token: 0x06001275 RID: 4725 RVA: 0x000330AC File Offset: 0x000312AC
		internal static _IRuntimeVersionExpression \u0001(IToken \u0002, Version \u0003, Operator \u0004)
		{
			return \u0003.Builder.CreateRuntimeVersionExpression(\u0002, \u0003, \u0004);
		}

		// Token: 0x06001276 RID: 4726 RVA: 0x000330BC File Offset: 0x000312BC
		internal static _IHasTypeExpression \u0001()
		{
			return \u0003.Builder.CreateHasTypeExpression();
		}

		// Token: 0x06001277 RID: 4727 RVA: 0x000330C8 File Offset: 0x000312C8
		internal static _IHasTypeExpression \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateHasTypeExpression(\u0002);
		}

		// Token: 0x06001278 RID: 4728 RVA: 0x000330D8 File Offset: 0x000312D8
		internal static _IIsEnumTypeExpression \u0001()
		{
			return \u0003.Builder.CreateIsEnumTypeExpression();
		}

		// Token: 0x06001279 RID: 4729 RVA: 0x000330E4 File Offset: 0x000312E4
		internal static _IIsEnumTypeExpression \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateIsEnumTypeExpression(\u0002);
		}

		// Token: 0x0600127A RID: 4730 RVA: 0x000330F4 File Offset: 0x000312F4
		internal static _IHasAttributeExpression \u0001()
		{
			return \u0003.Builder.CreateHasAttributeExpression();
		}

		// Token: 0x0600127B RID: 4731 RVA: 0x00033100 File Offset: 0x00031300
		internal static _IHasAttributeExpression \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateHasAttributeExpression(\u0002);
		}

		// Token: 0x0600127C RID: 4732 RVA: 0x00033110 File Offset: 0x00031310
		internal static _IHasAttributeExpression \u0001(IToken \u0002, _IItemReference \u0003, string \u0004)
		{
			return \u0003.Builder.CreateHasAttributeExpression(\u0002, \u0003, \u0004);
		}

		// Token: 0x0600127D RID: 4733 RVA: 0x00033120 File Offset: 0x00031320
		internal static _IHasValueExpression \u0001()
		{
			return \u0003.Builder.CreateHasValueExpression();
		}

		// Token: 0x0600127E RID: 4734 RVA: 0x0003312C File Offset: 0x0003132C
		internal static _IHasValueExpression \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateHasValueExpression(\u0002);
		}

		// Token: 0x0600127F RID: 4735 RVA: 0x0003313C File Offset: 0x0003133C
		internal static _IHasValueExpression \u0001(IToken \u0002, string \u0003, string \u0004)
		{
			return \u0003.Builder.CreateHasValueExpression(\u0002, \u0003, \u0004);
		}

		// Token: 0x06001280 RID: 4736 RVA: 0x0003314C File Offset: 0x0003134C
		internal static _IHasConstantValueExpression \u0001()
		{
			return \u0003.Builder.CreateHasConstantValueExpression();
		}

		// Token: 0x06001281 RID: 4737 RVA: 0x00033158 File Offset: 0x00031358
		internal static _IHasConstantValueExpression \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateHasConstantValueExpression(\u0002);
		}

		// Token: 0x06001282 RID: 4738 RVA: 0x00033168 File Offset: 0x00031368
		internal static _IHasConstantValueExpression \u0001(IToken \u0002, _IExpression \u0003, _IExpression \u0004, Operator \u0005)
		{
			return \u0003.Builder.CreateHasConstantValueExpression(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06001283 RID: 4739 RVA: 0x00033178 File Offset: 0x00031378
		internal static _IHasConstantTypeExpression \u0001()
		{
			return \u0003.Builder.CreateHasConstantTypeExpression();
		}

		// Token: 0x06001284 RID: 4740 RVA: 0x00033184 File Offset: 0x00031384
		internal static _IHasConstantTypeExpression \u0001(IToken \u0002)
		{
			return \u0003.Builder.CreateHasConstantTypeExpression(\u0002);
		}

		// Token: 0x06001285 RID: 4741 RVA: 0x00033194 File Offset: 0x00031394
		internal static _IHasConstantTypeExpression \u0001(IToken \u0002, _IExpression \u0003, bool \u0004)
		{
			return \u0003.Builder.CreateHasConstantTypeExpression(\u0002, \u0003, \u0004);
		}

		// Token: 0x06001286 RID: 4742 RVA: 0x000331A4 File Offset: 0x000313A4
		internal static _IPragmaOperatorExpression \u0001()
		{
			return \u0003.Builder.CreatePragmaOperatorExpression();
		}

		// Token: 0x06001287 RID: 4743 RVA: 0x000331B0 File Offset: 0x000313B0
		internal static _IPragmaOperatorExpression \u0001(PragmaOperator \u0002)
		{
			return \u0003.Builder.CreatePragmaOperatorExpression(\u0002);
		}

		// Token: 0x06001288 RID: 4744 RVA: 0x000331C0 File Offset: 0x000313C0
		internal static _IPragmaOperatorExpression \u0001(PragmaOperator \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreatePragmaOperatorExpression(\u0002, \u0003);
		}

		// Token: 0x06001289 RID: 4745 RVA: 0x000331D0 File Offset: 0x000313D0
		internal static _IPragmaAssertion \u0001()
		{
			return \u0003.Builder.CreatePragmaAssertion();
		}

		// Token: 0x0600128A RID: 4746 RVA: 0x000331DC File Offset: 0x000313DC
		internal static _IPragmaAssertion \u0001(_IExpression \u0002, string \u0003)
		{
			return \u0003.Builder.CreatePragmaAssertion(\u0002, \u0003);
		}

		// Token: 0x0600128B RID: 4747 RVA: 0x000331EC File Offset: 0x000313EC
		internal static _IPragmaAssertion \u0001(_IExpression \u0002, string \u0003, IToken \u0004)
		{
			return \u0003.Builder.CreatePragmaAssertion(\u0002, \u0003, \u0004);
		}

		// Token: 0x0600128C RID: 4748 RVA: 0x000331FC File Offset: 0x000313FC
		internal static _IPragmaElseIf \u0001()
		{
			return \u0003.Builder.CreatePragmaElseIf();
		}

		// Token: 0x0600128D RID: 4749 RVA: 0x00033208 File Offset: 0x00031408
		internal static _IPragmaElseIf \u0001(_IPragmaExpression \u0002, _IStatement \u0003)
		{
			return \u0003.Builder.CreatePragmaElseIf(\u0002, \u0003);
		}

		// Token: 0x0600128E RID: 4750 RVA: 0x00033218 File Offset: 0x00031418
		internal static _IPragmaIfStatement \u0001()
		{
			return \u0003.Builder.CreatePragmaIfStatement();
		}

		// Token: 0x0600128F RID: 4751 RVA: 0x00033224 File Offset: 0x00031424
		internal static _IPragmaIfStatement \u0001(_IExpression \u0002)
		{
			return \u0003.Builder.CreatePragmaIfStatement(\u0002);
		}

		// Token: 0x06001290 RID: 4752 RVA: 0x00033234 File Offset: 0x00031434
		internal static _IPragmaIfStatement \u0001(_IExpression \u0002, IToken \u0003)
		{
			return \u0003.Builder.CreatePragmaIfStatement(\u0002, \u0003);
		}

		// Token: 0x06001291 RID: 4753 RVA: 0x00033244 File Offset: 0x00031444
		internal static _IBreakPointStatement \u0001()
		{
			return \u0003.Builder.CreateBreakPointStatement();
		}

		// Token: 0x06001292 RID: 4754 RVA: 0x00033250 File Offset: 0x00031450
		internal static _IBreakPointStatement \u0001(IToken \u0002, long \u0003, long \u0004)
		{
			return \u0003.Builder.CreateBreakPointStatement(\u0002, \u0003, \u0004);
		}

		// Token: 0x06001293 RID: 4755 RVA: 0x00033260 File Offset: 0x00031460
		internal static _IDefineStatement \u0001()
		{
			return \u0003.Builder.CreateDefineStatement();
		}

		// Token: 0x06001294 RID: 4756 RVA: 0x0003326C File Offset: 0x0003146C
		internal static _IDefineStatement \u0001(IToken \u0002, bool \u0003, string \u0004, string \u0005)
		{
			return \u0003.Builder.CreateDefineStatement(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06001295 RID: 4757 RVA: 0x0003327C File Offset: 0x0003147C
		internal static _IDefineStatement \u0001(IToken \u0002, bool \u0003, string \u0004)
		{
			return \u0003.Builder.CreateDefineStatement(\u0002, \u0003, \u0004);
		}

		// Token: 0x06001296 RID: 4758 RVA: 0x0003328C File Offset: 0x0003148C
		internal static _IBitAccess \u0001()
		{
			return \u0003.Builder.CreateBitAccess();
		}

		// Token: 0x06001297 RID: 4759 RVA: 0x00033298 File Offset: 0x00031498
		internal static _IBitAccess \u0001(_IExpression \u0002, byte \u0003)
		{
			return \u0003.Builder.CreateBitAccess(\u0002, \u0003);
		}

		// Token: 0x06001298 RID: 4760 RVA: 0x000332A8 File Offset: 0x000314A8
		internal static _IAddressCodePosition \u0001(ISourcePosition \u0002, AccessFlag \u0003, int \u0004)
		{
			return \u0003.Builder.CreateAddressCodePosition(\u0002, \u0003, \u0004);
		}

		// Token: 0x06001299 RID: 4761 RVA: 0x000332B8 File Offset: 0x000314B8
		internal static _IPreCompileContext \u0001(string \u0002, Guid \u0003, KindOfContext \u0004)
		{
			return \u0003.Builder.CreatePrecompileContext(\u0002, \u0003, \u0004);
		}

		// Token: 0x0600129A RID: 4762 RVA: 0x000332C8 File Offset: 0x000314C8
		internal static ICompiledCode \u0001(ICompiledCode \u0002)
		{
			return \u0003.Builder.CreateCompiledCodeDataStub(\u0002);
		}

		// Token: 0x0600129B RID: 4763 RVA: 0x000332D8 File Offset: 0x000314D8
		internal static _ICompiledCodeData \u0001(ChunkedMemoryStream \u0002, int \u0003)
		{
			return \u0003.Builder.CreateCompiledCodeData(\u0002, \u0003);
		}

		// Token: 0x0600129C RID: 4764 RVA: 0x000332E8 File Offset: 0x000314E8
		internal static ICompiledCode \u0001(int \u0002, IDataLocation \u0003)
		{
			return \u0003.Builder.CreateCompiledCodeDataPlaceholder(\u0002, \u0003);
		}

		// Token: 0x0600129D RID: 4765 RVA: 0x000332F8 File Offset: 0x000314F8
		internal static ICompiledCode \u0001(ChunkedMemoryStream \u0002, bool \u0003)
		{
			return \u0003.Builder.CreateCompiledCodeDataReloc(\u0002, \u0003);
		}

		// Token: 0x0600129E RID: 4766 RVA: 0x00033308 File Offset: 0x00031508
		internal static ICompiledCode \u0001(byte[] \u0002, bool \u0003)
		{
			return \u0003.Builder.CreateCompiledCodeDataReloc(\u0002, \u0003);
		}

		// Token: 0x0600129F RID: 4767 RVA: 0x00033318 File Offset: 0x00031518
		internal static _IRelocationList \u0001()
		{
			return \u0003.Builder.CreateRelocationList();
		}

		// Token: 0x060012A0 RID: 4768 RVA: 0x00033324 File Offset: 0x00031524
		public static _ICompilerAttribute \u0001(string \u0002, string \u0003)
		{
			return \u0003.Builder.CreateCompilerAttribute(\u0002, \u0003);
		}

		// Token: 0x060012A1 RID: 4769 RVA: 0x00033334 File Offset: 0x00031534
		internal static _IDataSegment \u0001(ushort \u0002, int \u0003, int \u0004, DataSegmentFlags \u0005)
		{
			return \u0003.Builder.CreateDataSegment(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x060012A2 RID: 4770 RVA: 0x00033344 File Offset: 0x00031544
		internal static _IDataManager \u0001()
		{
			return \u0003.Builder.CreateDataManager();
		}

		// Token: 0x060012A3 RID: 4771 RVA: 0x00033350 File Offset: 0x00031550
		internal static _IMemoryManager \u0001(int \u0002, int \u0003)
		{
			return \u0003.Builder.CreateMemMan(\u0002, \u0003);
		}

		// Token: 0x060012A4 RID: 4772 RVA: 0x00033360 File Offset: 0x00031560
		internal static IBitWriteAccess \u0001(int \u0002, int \u0003, int \u0004, byte \u0005, IMinimalPosition \u0006, string \u0007)
		{
			return \u0003.Builder.CreateBitWriteAccess(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007);
		}

		// Token: 0x060012A5 RID: 4773 RVA: 0x00033374 File Offset: 0x00031574
		internal static ILanguageModelManagerTargetSettings4 \u0001()
		{
			return (ILanguageModelManagerTargetSettings4)\u0003.Builder.CreateTargetSettings();
		}

		// Token: 0x0400031D RID: 797
		private static _ILanguageModelBuilder7 \u0001;
	}
}
