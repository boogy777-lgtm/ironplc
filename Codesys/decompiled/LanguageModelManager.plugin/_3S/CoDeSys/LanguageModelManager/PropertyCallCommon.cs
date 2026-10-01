using System;
using System.Diagnostics;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000FD RID: 253
	internal static class PropertyCallCommon
	{
		// Token: 0x0600125E RID: 4702 RVA: 0x00034684 File Offset: 0x00033684
		private static void DetermineGetterOffset(IScope5 localScope, _IVariable var, IMyPropertyAddressInfo pai, _IVirtualFunctionTable vftable, ISignature signParentCallee, bool bInterface, IScope5 scope)
		{
			ISignature signature = localScope.FindSignatureLocal(IdentifierConstants.CreateGetterName(var.VersionedName));
			if (signature != null)
			{
				IDataLocation fpdataLocation = signature.FPDataLocation;
				if (fpdataLocation != null)
				{
					pai.AreaProperty = (int)fpdataLocation.Area;
					pai.OffsetGet = fpdataLocation.Offset;
				}
				if (vftable != null)
				{
					int num = vftable[signature.Name];
					if (num != SignatureConstant.InvalidOffset)
					{
						pai.VFTableOffsetGet = num / scope.PointerSize;
						if (bInterface && signParentCallee.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE))
						{
							int vftableOffsetGet = pai.VFTableOffsetGet;
							pai.VFTableOffsetGet = vftableOffsetGet - 1;
						}
					}
				}
				IVariable variable = signature[signature.Name];
				Debug.Assert(variable != null);
				pai.OffsetValueGetter = variable.DataLocation.Offset;
			}
		}

		// Token: 0x0600125F RID: 4703 RVA: 0x00034740 File Offset: 0x00033740
		private static void DetermineSetterOffset(IScope5 localScope, _IVariableExpression varExp, _IVariable var, IMyPropertyAddressInfo pai, _IVirtualFunctionTable vftable, ISignature signParentCallee, bool bInterface, IScope5 scope)
		{
			ISignature signature = localScope.FindSignatureLocal(IdentifierConstants.CreateSetterName(var.VersionedName));
			if (signature != null)
			{
				IDataLocation fpdataLocation = signature.FPDataLocation;
				if (fpdataLocation != null)
				{
					pai.AreaProperty = (int)fpdataLocation.Area;
					pai.OffsetSet = fpdataLocation.Offset;
				}
				if (vftable != null)
				{
					int num = vftable[signature.Name];
					if (num != SignatureConstant.InvalidOffset)
					{
						pai.VFTableOffsetSet = num / scope.PointerSize;
						if (bInterface && signParentCallee.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE))
						{
							int vftableOffsetSet = pai.VFTableOffsetSet;
							pai.VFTableOffsetSet = vftableOffsetSet - 1;
						}
					}
				}
				IVariable variable = signature[varExp.Name];
				Debug.Assert(variable != null);
				pai.OffsetValueSetter = variable.DataLocation.Offset;
			}
		}

		// Token: 0x06001260 RID: 4704 RVA: 0x000347FC File Offset: 0x000337FC
		internal static void DetermineGetterSetterOffsets(_IVariableExpression varExp, _IVariable var, IMyPropertyAddressInfo pai, ISignature signParentCaller, ISignature signParentCallee, bool bInterface, IScope5 scope)
		{
			if (var.CompiledType.Class == TypeClass.Reference || var.CompiledType.Class == TypeClass.Pointer)
			{
				pai.IsReferenceType = true;
			}
			IScope5 localScope = scope.CreateLocalScope(signParentCallee as _ISignature);
			_IVirtualFunctionTable virtualFunctionTable = (signParentCaller as _ISignature2)._VirtualFunctionTable;
			if (signParentCaller.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
			{
				UserdefType userdefType = signParentCaller["__Interface"].CompiledType.BaseType as UserdefType;
				if (userdefType != null)
				{
					virtualFunctionTable = (userdefType.GetSignature(scope) as _ISignature2)._VirtualFunctionTable;
				}
			}
			PropertyCallCommon.DetermineGetterOffset(localScope, var, pai, virtualFunctionTable, signParentCallee, bInterface, scope);
			PropertyCallCommon.DetermineSetterOffset(localScope, varExp, var, pai, virtualFunctionTable, signParentCallee, bInterface, scope);
		}
	}
}
