using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000FC RID: 252
	public static class TypeExtensions
	{
		// Token: 0x060010F0 RID: 4336 RVA: 0x00031824 File Offset: 0x0002FA24
		public static bool HasUnderlyingUserdefType(this IType type, out _IUserdefType udtype)
		{
			udtype = type.GetUnderlyingUserdefType();
			return udtype != null;
		}

		// Token: 0x060010F1 RID: 4337 RVA: 0x00031834 File Offset: 0x0002FA34
		public static _IUserdefType GetUnderlyingUserdefType(this IType type)
		{
			_IUserdefType iuserdefType = type as _IUserdefType;
			if (iuserdefType != null)
			{
				return iuserdefType;
			}
			_IArrayType iarrayType = type as _IArrayType;
			if (iarrayType != null)
			{
				return iarrayType._Base.GetUnderlyingUserdefType();
			}
			_IReferenceType ireferenceType = type as _IReferenceType;
			if (ireferenceType != null)
			{
				return ireferenceType._Base.GetUnderlyingUserdefType();
			}
			_IAliasType ialiasType = type as _IAliasType;
			if (ialiasType != null)
			{
				return ialiasType.BaseType.GetUnderlyingUserdefType();
			}
			_IPointerType ipointerType = type as _IPointerType;
			if (ipointerType != null)
			{
				return ipointerType.Base.GetUnderlyingUserdefType();
			}
			return null;
		}
	}
}
