using System;
using System.Collections;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000E4 RID: 228
	internal class AddressInfoCollector : AddressInfoBuilderBase, IVarReferenceGenerator
	{
		// Token: 0x170004D8 RID: 1240
		// (get) Token: 0x06001121 RID: 4385 RVA: 0x00031993 File Offset: 0x00030993
		private IScope5 Scope
		{
			get
			{
				return this.m_scope;
			}
		}

		// Token: 0x170004D9 RID: 1241
		// (get) Token: 0x06001122 RID: 4386 RVA: 0x0003199B File Offset: 0x0003099B
		private ICompileContext ComCon
		{
			get
			{
				return this.m_context;
			}
		}

		// Token: 0x06001123 RID: 4387 RVA: 0x000319A3 File Offset: 0x000309A3
		public void Init(IScope5 scope, ICompileContext comcon)
		{
			this.m_scope = scope;
			this.m_context = comcon;
		}

		// Token: 0x170004DA RID: 1242
		// (get) Token: 0x06001124 RID: 4388 RVA: 0x000319B4 File Offset: 0x000309B4
		public IAddressInfo[] AddressInfo
		{
			get
			{
				IAddressInfo[] array = new IAddressInfo[this.m_alAddressInfos.Count];
				this.m_alAddressInfos.CopyTo(array);
				return array;
			}
		}

		// Token: 0x06001125 RID: 4389 RVA: 0x000319DF File Offset: 0x000309DF
		public void Remove(IAddressInfo aiInfo)
		{
			this.m_alAddressInfos.Remove(aiInfo);
		}

		// Token: 0x06001126 RID: 4390 RVA: 0x000319ED File Offset: 0x000309ED
		public void ClearDueToInvalidArrayIndex()
		{
			this.m_alAddressInfos.Clear();
		}

		// Token: 0x06001127 RID: 4391 RVA: 0x000319FC File Offset: 0x000309FC
		public IAddressInfo GenerateVarStackRelativeOffset(IVariableExpression varexp, IExpression expInstancePath, StackRelativeAddressInfo stinfo, int nAddress, int nSize)
		{
			varexp.DataLocation(this.Scope);
			IAddressInfo addressInfo = new StackRelativeAddressInfo(nAddress, byte.MaxValue, nSize, stinfo.AreaCode, stinfo.OffsetCode, stinfo.SizeCode, varexp.Type);
			this.m_alAddressInfos.Add(addressInfo);
			return addressInfo;
		}

		// Token: 0x06001128 RID: 4392 RVA: 0x00031A4C File Offset: 0x00030A4C
		public IAddressInfo GenerateVarStackRelative(ICompiledPOU cpou, IVariableExpression varexp, IExpression expInstancePath, int nAddress, int nSize)
		{
			if (cpou == null)
			{
				ISignature signature = this.Scope.MethodSignature;
				if (signature == null)
				{
					signature = this.Scope.LocalSignature;
				}
				if (signature == null)
				{
					signature = this.Scope[varexp.SignatureId];
				}
				if (signature == null)
				{
					return null;
				}
				cpou = this.ComCon.GetCompiledPOUById(signature.Id);
				if (cpou == null || cpou.CompiledCode == null || cpou.CompiledCode.Location == null)
				{
					return null;
				}
			}
			IDataLocation location = cpou.CompiledCode.Location;
			IAddressInfo addressInfo = new StackRelativeAddressInfo(nAddress, byte.MaxValue, nSize, (int)location.Area, location.Offset, cpou.CompiledCode.CodeSize, varexp.Type);
			this.m_alAddressInfos.Add(addressInfo);
			return addressInfo;
		}

		// Token: 0x06001129 RID: 4393 RVA: 0x00031B04 File Offset: 0x00030B04
		public IAddressInfo GenerateVarAbsolut(IVariableExpression varexp, IExpression expInstancePath, int iArea, int nAddress, int nSize)
		{
			IDataLocation dataLocation = varexp.DataLocation(this.Scope);
			byte nBitOffset = (dataLocation == null || !dataLocation.IsBitLocation) ? byte.MaxValue : dataLocation.BitNr;
			IAddressInfo addressInfo = new AbsoluteAddressInfo(iArea, nAddress, nBitOffset, nSize, varexp.Type);
			this.m_alAddressInfos.Add(addressInfo);
			return addressInfo;
		}

		// Token: 0x0600112A RID: 4394 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void AdaptFunctionCallInfoSourceposition(IAddressInfo addressInfo, _ICallExpression callexp)
		{
		}

		// Token: 0x0600112B RID: 4395 RVA: 0x00031B58 File Offset: 0x00030B58
		public IAddressInfo GenerateFunctionCall(_IVariableExpression variable, ISignature sign, _IExpression expInstancePath, IScope5 scope)
		{
			if (sign == null || expInstancePath == null)
			{
				return null;
			}
			int num = sign.Outputs[0].CompiledType.Size(scope);
			FunctionAddressInfo functionAddressInfo = new FunctionAddressInfo(num, sign.Outputs[0].CompiledType);
			functionAddressInfo.VariableID = -1;
			functionAddressInfo.SignatureID = sign.Id;
			functionAddressInfo.FunctionPointerArea = (int)sign.FPDataLocation.Area;
			functionAddressInfo.FunctionPointerOffset = sign.FPDataLocation.Offset;
			functionAddressInfo.ResultOffset = sign.Outputs[0].DataLocation.Offset;
			functionAddressInfo.ResultSize = num;
			if (variable.Name.ToUpperInvariant() == sign.Name)
			{
				functionAddressInfo.ResultCompiledType = sign.Outputs[0].CompiledType;
			}
			else
			{
				functionAddressInfo.ResultCompiledType = variable._CompiledType;
			}
			functionAddressInfo.ImplementationStyle = 0;
			if (sign.GetFlag(SignatureFlag.External))
			{
				functionAddressInfo.ImplementationStyle = 1;
			}
			functionAddressInfo.InputParameterCount = (short)sign.Inputs.Length;
			functionAddressInfo.InputParameterOffsets = new int[sign.Inputs.Length];
			functionAddressInfo.SignatureSize = sign.Size;
			for (int i = 0; i < sign.AllInputs.Length; i++)
			{
				functionAddressInfo.InputParameterOffsets[i] = sign.AllInputs[i].DataLocation.Offset;
			}
			functionAddressInfo.InputParameterAddressInfos = new IAddressInfo[sign.AllInputs.Length];
			this.m_alAddressInfos.Add(functionAddressInfo);
			return functionAddressInfo;
		}

		// Token: 0x0600112C RID: 4396 RVA: 0x00031CB8 File Offset: 0x00030CB8
		public IAddressInfo GeneratePropertyCall(_IVariableExpression varExp, IExpression expInstancePath, IAddressInfo adrInfoInstance, int nSize)
		{
			if (expInstancePath == null)
			{
				return null;
			}
			_IVariable ivariable = varExp.GetVariable(this.Scope) as _IVariable;
			if (ivariable == null)
			{
				return null;
			}
			if (ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE))
			{
				return null;
			}
			IAbsoluteAddressInfo absoluteAddressInfo = adrInfoInstance as IAbsoluteAddressInfo;
			ISignature signature = this.Scope[varExp.SignatureId];
			if (signature == null)
			{
				return null;
			}
			IUserdefType userdefType = new PropertyCallerSearcher().DetermineCallerType(expInstancePath);
			ISignature signature2 = (userdefType == null) ? signature : this.Scope[userdefType.SignatureId];
			if (signature2 == null)
			{
				return null;
			}
			if (signature.POUType == Operator.FunctionBlock && adrInfoInstance == null)
			{
				return null;
			}
			bool flag = signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion) || signature.POUType == Operator.Interface;
			IMyPropertyAddressInfo myPropertyAddressInfo;
			if (absoluteAddressInfo != null && !flag)
			{
				myPropertyAddressInfo = new PropertyAddressInfo(nSize, varExp.Type)
				{
					AreaInstance = absoluteAddressInfo.Area,
					OffsetInstance = absoluteAddressInfo.Offset
				};
			}
			else if (adrInfoInstance != null)
			{
				myPropertyAddressInfo = new PropertyAddressInfoExtended(nSize, varExp.Type)
				{
					InfoInstance = adrInfoInstance,
					InterfaceCall = flag
				};
			}
			else
			{
				myPropertyAddressInfo = new PropertyAddressInfo(nSize, varExp.Type);
			}
			PropertyCallCommon.DetermineGetterSetterOffsets(varExp, ivariable, myPropertyAddressInfo, signature2, signature, flag, this.m_scope);
			this.m_alAddressInfos.Add(myPropertyAddressInfo);
			return myPropertyAddressInfo;
		}

		// Token: 0x0600112D RID: 4397 RVA: 0x00031DEC File Offset: 0x00030DEC
		public IAddressInfo GenerateIndexAccess(IIndexAccessExpression indexaccess, int iArea, int nAddress, int nSize)
		{
			IAddressInfo addressInfo = new AbsoluteAddressInfo(iArea, nAddress, byte.MaxValue, nSize, indexaccess.Type);
			this.m_alAddressInfos.Add(addressInfo);
			return addressInfo;
		}

		// Token: 0x0600112E RID: 4398 RVA: 0x00031E1C File Offset: 0x00030E1C
		public IAddressInfo GenerateVariableIndexAccess(IIndexAccessExpression indexaccess, IAddressInfo aiWholeArray, int nBaseSize, IAddressInfo[] indexAccesses, IArrayBounds[] arrayBounds, IType t)
		{
			IAddressInfo addressInfo = this.DoGenerateVariableIndexAccess(indexaccess, aiWholeArray, nBaseSize, indexAccesses, arrayBounds, t);
			if (addressInfo != null)
			{
				this.m_alAddressInfos.Add(addressInfo);
			}
			return addressInfo;
		}

		// Token: 0x0600112F RID: 4399 RVA: 0x00031E4C File Offset: 0x00030E4C
		public IAddressInfo GenerateOperatorExpression(_IOperatorExpression opExp, IAddressInfo[] addrsOperands)
		{
			IAddressInfo addressInfo = this.DoGenerateOperatorExpression(opExp, addrsOperands);
			if (addressInfo != null)
			{
				this.m_alAddressInfos.Add(addressInfo);
			}
			return addressInfo;
		}

		// Token: 0x06001130 RID: 4400 RVA: 0x00031E74 File Offset: 0x00030E74
		public IAddressInfo GenerateSignedConstant(IExpression constExpr, int literalValue)
		{
			IAddressInfo addressInfo = this.DoGenerateSignedConstant(constExpr, literalValue);
			if (addressInfo != null)
			{
				this.m_alAddressInfos.Add(addressInfo);
			}
			return addressInfo;
		}

		// Token: 0x06001131 RID: 4401 RVA: 0x00031E9C File Offset: 0x00030E9C
		public IAddressInfo GenerateAddress(ulong literalValue)
		{
			IAddressInfo addressInfo = this.DoGenerateAddress(literalValue);
			if (addressInfo != null)
			{
				this.m_alAddressInfos.Add(addressInfo);
			}
			return addressInfo;
		}

		// Token: 0x06001132 RID: 4402 RVA: 0x00031EC4 File Offset: 0x00030EC4
		public IAddressInfo GenerateAddress(int iArea, int iOffset)
		{
			IAddressInfo addressInfo = this.DoGenerateAddress(iArea, iOffset);
			if (addressInfo != null)
			{
				this.m_alAddressInfos.Add(addressInfo);
			}
			return addressInfo;
		}

		// Token: 0x06001133 RID: 4403 RVA: 0x00031EEC File Offset: 0x00030EEC
		public IAddressInfo GenerateDirectAddress(IAddressExpression addr, IDataLocation datloc, int nSize)
		{
			IAddressInfo addressInfo;
			if (addr.DirectAddress.Size == DirectVariableSize.X)
			{
				int num = addr.DirectAddress.Components.Length;
				addressInfo = new AbsoluteAddressInfo((int)datloc.Area, datloc.Offset, datloc.BitNr, 0, addr.Type);
			}
			else
			{
				addressInfo = new AbsoluteAddressInfo((int)datloc.Area, datloc.Offset, byte.MaxValue, nSize, addr.Type);
			}
			this.m_alAddressInfos.Add(addressInfo);
			return addressInfo;
		}

		// Token: 0x06001134 RID: 4404 RVA: 0x00031F64 File Offset: 0x00030F64
		public IAddressInfo GenerateDeRefAccess(IExpression deref, IAddressInfo varrefelement, int nOffset)
		{
			if (varrefelement == null)
			{
				return null;
			}
			IAddressInfo addressInfo = new DeRefAccessInfo(varrefelement, deref.Type.DeRefType.Size(this.Scope), nOffset, deref.Type.DeRefType);
			this.m_alAddressInfos.Add(addressInfo);
			return addressInfo;
		}

		// Token: 0x06001135 RID: 4405 RVA: 0x00031FB0 File Offset: 0x00030FB0
		public IAddressInfo GenerateCompoAccess(IExpression compo, IAddressInfo varrefelement, int nOffset)
		{
			if (varrefelement == null)
			{
				return null;
			}
			IAddressInfo addressInfo = new CompoAddressInfo(varrefelement, compo.Type.DeRefType.Size(this.Scope), nOffset, compo.Type);
			this.m_alAddressInfos.Add(addressInfo);
			return addressInfo;
		}

		// Token: 0x06001136 RID: 4406 RVA: 0x00031FF4 File Offset: 0x00030FF4
		public IAddressInfo GenerateBitAccess(IExpression compo, ICompiledType leftType, ISourcePosition varRefPosition, IAddressInfo varrefelement, int nBitOffset, int accessedElementSize)
		{
			if (varrefelement == null)
			{
				return null;
			}
			BitAddressInfo bitAddressInfo = new BitAddressInfo(varrefelement, leftType.DeRefType.Size(this.Scope), nBitOffset, compo.Type);
			bitAddressInfo.AccessedElementSize = accessedElementSize;
			this.m_alAddressInfos.Add(bitAddressInfo);
			return bitAddressInfo;
		}

		// Token: 0x06001137 RID: 4407 RVA: 0x00032040 File Offset: 0x00031040
		public IAddressInfo GenerateLiteral(IVariableExpression varexp, IExpression expInstancePath, ILiteralValue lv)
		{
			IAddressInfo addressInfo = new LiteralAddressInfo(lv, varexp.Type);
			this.m_alAddressInfos.Add(addressInfo);
			return addressInfo;
		}

		// Token: 0x040003FC RID: 1020
		private readonly ArrayList m_alAddressInfos = new ArrayList();

		// Token: 0x040003FD RID: 1021
		private IScope5 m_scope;

		// Token: 0x040003FE RID: 1022
		private ICompileContext m_context;
	}
}
