using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001F
{
	// Token: 0x02000132 RID: 306
	internal sealed class \u0005
	{
		// Token: 0x060015BA RID: 5562 RVA: 0x0003F77C File Offset: 0x0003D97C
		internal \u0005(_IPreCompileContext \u0080\u0005)
		{
			this.PreCompileContext = \u0080\u0005;
		}

		// Token: 0x17000521 RID: 1313
		// (get) Token: 0x060015BB RID: 5563 RVA: 0x0003F78C File Offset: 0x0003D98C
		private _IPreCompileContext PreCompileContext { get; }

		// Token: 0x060015BC RID: 5564 RVA: 0x0003F794 File Offset: 0x0003D994
		internal ICompiledType \u0001(IVariable3 \u0002)
		{
			ICompiledType compiledType = \u0002.Type as ICompiledType;
			if (\u0005.\u0001(compiledType))
			{
				_IType itype = compiledType as _IType;
				if (itype != null)
				{
					itype = itype._Duplicate(true);
					compiledType = this.\u0001(itype);
				}
			}
			return compiledType;
		}

		// Token: 0x060015BD RID: 5565 RVA: 0x0003F7D0 File Offset: 0x0003D9D0
		private _IType \u0001(_IType \u0002)
		{
			switch (\u0002.Class)
			{
			case TypeClass.UXInt:
				if (this.PreCompileContext.PointerSize == 4)
				{
					return TypeTable.UDInt;
				}
				return TypeTable.ULInt;
			case TypeClass.XWord:
				if (this.PreCompileContext.PointerSize == 4)
				{
					return TypeTable.DWord;
				}
				return TypeTable.LWord;
			case TypeClass.XInt:
				if (this.PreCompileContext.PointerSize == 4)
				{
					return TypeTable.DInt;
				}
				return TypeTable.LInt;
			default:
				return \u0002;
			}
		}

		// Token: 0x060015BE RID: 5566 RVA: 0x0003F84C File Offset: 0x0003DA4C
		private static bool \u0001(ICompiledType \u0002)
		{
			switch (\u0002.Class)
			{
			case TypeClass.Pointer:
				return \u0005.\u0001(((_IPointerType)\u0002.DeRefType).BaseType);
			case TypeClass.Reference:
				return \u0005.\u0001(((_IReferenceType)\u0002).BaseType);
			case TypeClass.Array:
				return \u0005.\u0001(((_IArrayType)\u0002.DeRefType).BaseType);
			}
			return TypeTable.IsXType(\u0002);
		}

		// Token: 0x060015BF RID: 5567 RVA: 0x0003F8C4 File Offset: 0x0003DAC4
		private _IType \u0001(ICompiledType \u0002)
		{
			switch (\u0002.Class)
			{
			case TypeClass.Pointer:
			{
				_IPointerType ipointerType = (_IPointerType)\u0002.DeRefType;
				ipointerType._Base = this.\u0001(ipointerType.BaseType);
				return ipointerType;
			}
			case TypeClass.Reference:
			{
				_IReferenceType ireferenceType = (_IReferenceType)\u0002;
				ireferenceType._Base = this.\u0001(ireferenceType.BaseType);
				return ireferenceType;
			}
			case TypeClass.Array:
			{
				_IArrayType iarrayType = (_IArrayType)\u0002.DeRefType;
				iarrayType._Base = this.\u0001(iarrayType.BaseType);
				return iarrayType;
			}
			}
			return this.\u0001((_IType)\u0002);
		}

		// Token: 0x040003C2 RID: 962
		[CompilerGenerated]
		private readonly _IPreCompileContext \u0001;
	}
}
