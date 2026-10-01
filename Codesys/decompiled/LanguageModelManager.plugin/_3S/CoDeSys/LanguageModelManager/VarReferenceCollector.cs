using System;
using System.Collections.Generic;
using System.Diagnostics;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000107 RID: 263
	internal class VarReferenceCollector : AddressInfoBuilderBase, IVarReferenceGenerator
	{
		// Token: 0x1700056D RID: 1389
		// (get) Token: 0x06001361 RID: 4961 RVA: 0x00035FFA File Offset: 0x00034FFA
		// (set) Token: 0x06001362 RID: 4962 RVA: 0x00036002 File Offset: 0x00035002
		private IScope5 Scope { get; set; }

		// Token: 0x1700056E RID: 1390
		// (get) Token: 0x06001363 RID: 4963 RVA: 0x0003600B File Offset: 0x0003500B
		// (set) Token: 0x06001364 RID: 4964 RVA: 0x00036013 File Offset: 0x00035013
		private ICompileContext ComCon { get; set; }

		// Token: 0x06001365 RID: 4965 RVA: 0x0003601C File Offset: 0x0003501C
		public void Init(IScope5 scope, ICompileContext comcon)
		{
			this.Scope = scope;
			this.ComCon = comcon;
		}

		// Token: 0x06001366 RID: 4966 RVA: 0x0003602C File Offset: 0x0003502C
		public VarReferenceCollector(IAddressInfo aiBase, Guid guidApplication, long[] alPositionsOfInterest, _IExpression expInstance)
		{
			this.m_guidApplication = guidApplication;
			this.m_alPositionsOfInterest = alPositionsOfInterest;
			this.m_expInstance = expInstance;
		}

		// Token: 0x06001367 RID: 4967 RVA: 0x00036058 File Offset: 0x00035058
		private bool CheckPosition(ISourcePosition sourcepos)
		{
			if (sourcepos == null)
			{
				return false;
			}
			if (this.m_alPositionsOfInterest == null)
			{
				return true;
			}
			long[] alPositionsOfInterest = this.m_alPositionsOfInterest;
			for (int i = 0; i < alPositionsOfInterest.Length; i++)
			{
				if (alPositionsOfInterest[i] == sourcepos.Position)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x1700056F RID: 1391
		// (get) Token: 0x06001368 RID: 4968 RVA: 0x00036097 File Offset: 0x00035097
		private Guid ApplicationGuid
		{
			get
			{
				return this.m_guidApplication;
			}
		}

		// Token: 0x17000570 RID: 1392
		// (get) Token: 0x06001369 RID: 4969 RVA: 0x0003609F File Offset: 0x0003509F
		public IEnumerable<IVarRef> VarReferences
		{
			get
			{
				return this.m_alVarReferences;
			}
		}

		// Token: 0x0600136A RID: 4970 RVA: 0x000360A8 File Offset: 0x000350A8
		public void HandleTaskLocalVarReferences()
		{
			LList<VarRef> llist = new LList<VarRef>();
			foreach (IVarRef varRef in this.m_alVarReferences)
			{
				if (CheckForTaskLocalVariable.ContainsTaskLocalAccess(varRef.WatchExpression as _IExpression, this.Scope))
				{
					_IExpression exp = varRef.WatchExpression as _IExpression;
					if ((varRef as VarRef)._OrgExpression != null)
					{
						exp = (varRef as VarRef)._OrgExpression;
					}
					VarRef varRef2 = VarReferenceCreator.HandleTaskLocalVariables(varRef, this.m_expInstance, exp, this.Scope, this.ApplicationGuid) as VarRef;
					llist.Add(varRef2);
				}
				else
				{
					llist.Add(varRef as VarRef);
				}
			}
			this.m_alVarReferences.Clear();
			this.m_alVarReferences.AddRange(llist);
		}

		// Token: 0x0600136B RID: 4971 RVA: 0x00036184 File Offset: 0x00035184
		public void Remove(IAddressInfo aiInfo)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
			{
				for (int i = this.m_alVarReferences.Count - 1; i >= 0; i--)
				{
					if (this.m_alVarReferences[i].AddressInfo == aiInfo)
					{
						this.m_alVarReferences.RemoveAt(i);
						return;
					}
				}
				return;
			}
			foreach (VarRef varRef in this.m_alVarReferences)
			{
				if (varRef.AddressInfo == aiInfo)
				{
					this.m_alVarReferences.Remove(varRef);
					break;
				}
			}
		}

		// Token: 0x0600136C RID: 4972 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void ClearDueToInvalidArrayIndex()
		{
		}

		// Token: 0x0600136D RID: 4973 RVA: 0x00036230 File Offset: 0x00035230
		public IAddressInfo GenerateVarStackRelativeOffset(IVariableExpression varexp, IExpression expInstancePath, StackRelativeAddressInfo stinfo, int nAddress, int nSize)
		{
			VarRef varRef = new VarRef(expInstancePath as _IExpression, this.ApplicationGuid, this.Scope, this.m_expInstance);
			varRef.Position = varexp.Position;
			varRef.AddressInfo = new StackRelativeAddressInfo(nAddress, byte.MaxValue, nSize, stinfo.AreaCode, stinfo.OffsetCode, stinfo.SizeCode, varexp.Type);
			this.m_alVarReferences.Add(varRef);
			return varRef.AddressInfo;
		}

		// Token: 0x0600136E RID: 4974 RVA: 0x000362A8 File Offset: 0x000352A8
		public IAddressInfo GenerateVarStackRelative(ICompiledPOU cpou, IVariableExpression varexp, IExpression expInstancePath, int nAddress, int nSize)
		{
			if (!this.CheckPosition(varexp.Position))
			{
				return null;
			}
			_IVariable ivariable = varexp.GetVariable(this.Scope) as _IVariable;
			if (ivariable == null)
			{
				return null;
			}
			if (VarReferenceCollector.IsImplicitVar(ivariable) && ivariable.Name != IdentifierConstants.InstancePointer && ivariable.Name != IdentifierConstants.CurrentTaskInfoPointer && !VarReferenceCollector.IsVarLenArrayDimInfo(ivariable))
			{
				return null;
			}
			ISignature signature = this.Scope.MethodSignature;
			if (signature == null)
			{
				signature = this.Scope.LocalSignature;
			}
			if (signature == null)
			{
				return null;
			}
			if (cpou == null)
			{
				cpou = this.ComCon.GetCompiledPOUById(signature.Id);
				if (cpou == null || cpou.CompiledCode == null || cpou.CompiledCode.Location == null)
				{
					return null;
				}
			}
			IDataLocation location = cpou.CompiledCode.Location;
			VarRef varRef = new VarRef(expInstancePath as _IExpression, this.ApplicationGuid, this.Scope, this.m_expInstance);
			varRef.Position = varexp.Position;
			varRef.AddressInfo = new StackRelativeAddressInfo(nAddress, byte.MaxValue, nSize, (int)location.Area, location.Offset, cpou.CompiledCode.CodeSize, varexp.Type);
			if (ivariable.GetFlag(VarFlag.ReplacedConstant) && ivariable.Initial != null)
			{
				varRef.SetConstantValue(ivariable.Initial.Literal(this.Scope));
			}
			this.m_alVarReferences.Add(varRef);
			return varRef.AddressInfo;
		}

		// Token: 0x0600136F RID: 4975 RVA: 0x00036405 File Offset: 0x00035405
		private static bool IsImplicitVar(_IVariable var)
		{
			return var.GetFlag(VarFlag.Implicit) || var.VersionedName.IndexOf("__", StringComparison.OrdinalIgnoreCase) != -1;
		}

		// Token: 0x06001370 RID: 4976 RVA: 0x00036430 File Offset: 0x00035430
		public IAddressInfo GenerateVarAbsolut(IVariableExpression varexp, IExpression expInstancePath, int iArea, int nAddress, int nSize)
		{
			if (!this.CheckPosition(varexp.Position))
			{
				return null;
			}
			_IVariable ivariable = varexp.GetVariable(this.Scope) as _IVariable;
			ISignature signature = varexp.GetSignature(this.Scope);
			int nSize2;
			if (ivariable != null)
			{
				nSize2 = ivariable._Type.Size(this.Scope);
			}
			else
			{
				nSize2 = varexp.Type.DeRefType.Size(this.Scope);
			}
			if ((ivariable != null && !VarReferenceCollector.IsImplicitVar(ivariable)) || (ivariable == null && signature != null) || VarReferenceCollector.IsVarLenArrayDimInfo(ivariable))
			{
				IExpression expression = expInstancePath;
				if (this.m_expInstance != null)
				{
					expression = CompilerProxy.CreateParser(this.m_expInstance.ToString() + "." + expInstancePath.ToString()).ParseOperand();
					if (expression == null)
					{
						expression = expInstancePath;
					}
					else
					{
						CompilerProxy.TypifyExprement(expression as _IExpression, this.Scope, this.ComCon as _ICompileContext, null, false, false, null);
					}
					if (expression.Type == null)
					{
						expression = expInstancePath;
					}
					(expression as _IExpression)._CompiledType = (varexp as _IExpression)._CompiledType;
				}
				VarRef varRef = new VarRef(expression as _IExpression, this.ApplicationGuid, this.Scope, this.m_expInstance);
				varRef._OrgExpression = (varexp as _IExpression);
				varRef.Position = varexp.Position;
				IDataLocation dataLocation = varexp.DataLocation(this.Scope);
				byte b = (dataLocation == null || !dataLocation.IsBitLocation) ? byte.MaxValue : dataLocation.BitNr;
				this.m_alVarReferences.Add(varRef);
				if (b != 255)
				{
					nSize2 = 0;
				}
				if (ivariable != null && ivariable.GetFlag(VarFlag.ReplacedConstant) && ivariable.Initial != null)
				{
					ILiteralValue literalValue = ivariable.Initial.Literal(this.Scope);
					varRef.SetConstantValue(literalValue);
					varRef.SetFlag(VarRefFlag.Constant, true);
					varRef.AddressInfo = new LiteralAddressInfo(literalValue, varexp.Type);
				}
				else
				{
					varRef.AddressInfo = new AbsoluteAddressInfo(iArea, nAddress, b, nSize2, varRef.WatchExpression.Type);
				}
				if (ivariable != null)
				{
					varRef.SetFlag(VarRefFlag.Constant, ivariable.HasFlag(VarFlag.ReplacedConstant | VarFlag.Constant));
				}
				return varRef.AddressInfo;
			}
			return null;
		}

		// Token: 0x06001371 RID: 4977 RVA: 0x00036635 File Offset: 0x00035635
		private static bool IsVarLenArrayDimInfo(_IVariable var)
		{
			return var != null && var.Name.EndsWith("__ARRAY__INFO");
		}

		// Token: 0x06001372 RID: 4978 RVA: 0x0003664C File Offset: 0x0003564C
		public void AdaptFunctionCallInfoSourceposition(IAddressInfo addressInfo, _ICallExpression callexp)
		{
			foreach (VarRef varRef in this.m_alVarReferences)
			{
				if (varRef.AddressInfo == addressInfo)
				{
					varRef.Position = callexp.Position;
				}
			}
		}

		// Token: 0x06001373 RID: 4979 RVA: 0x000366A8 File Offset: 0x000356A8
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
				if (!this.CheckForValidParameterType(sign.AllInputs[i].CompiledType))
				{
					return null;
				}
				functionAddressInfo.InputParameterOffsets[i] = sign.AllInputs[i].DataLocation.Offset;
			}
			functionAddressInfo.InputParameterAddressInfos = new IAddressInfo[sign.AllInputs.Length];
			VarRef varRef = new VarRef(expInstancePath, this.ApplicationGuid, this.Scope, this.m_expInstance);
			varRef.Position = variable.Position;
			varRef.AddressInfo = functionAddressInfo;
			this.m_alVarReferences.Add(varRef);
			return functionAddressInfo;
		}

		// Token: 0x06001374 RID: 4980 RVA: 0x0003684A File Offset: 0x0003584A
		public bool CheckForValidParameterType(ICompiledType ctype)
		{
			return !TypeTable.IsBlock(ctype.Class) || TypeTable.IsString(ctype.Class);
		}

		// Token: 0x06001375 RID: 4981 RVA: 0x00036868 File Offset: 0x00035868
		public IAddressInfo GeneratePropertyCall(_IVariableExpression varExp, IExpression expInstancePath, IAddressInfo adrInfoInstance, int nSize)
		{
			if (expInstancePath == null)
			{
				return null;
			}
			if (!this.CheckPosition(varExp.Position))
			{
				return null;
			}
			_IVariable ivariable = varExp.GetVariable(this.Scope) as _IVariable;
			if (ivariable == null)
			{
				return null;
			}
			ICompoAccessExpression compoAccessExpression = expInstancePath as ICompoAccessExpression;
			if (compoAccessExpression != null && (compoAccessExpression.Left == null || compoAccessExpression.Right == null))
			{
				return null;
			}
			IAbsoluteAddressInfo absoluteAddressInfo = adrInfoInstance as IAbsoluteAddressInfo;
			if (adrInfoInstance == null)
			{
				absoluteAddressInfo = new AbsoluteAddressInfo(-1, 0, byte.MaxValue, nSize, varExp.Type);
				if (this.Scope != null && this.Scope.LocalSignature != null && this.Scope.LocalSignature.POUType == Operator.FunctionBlock && this.m_expInstance != null)
				{
					IDataLocation dataLocation = this.m_expInstance.DataLocation(this.Scope);
					if (dataLocation != null && !dataLocation.IsRelativ)
					{
						absoluteAddressInfo = new AbsoluteAddressInfo((int)dataLocation.Area, dataLocation.Offset, byte.MaxValue, nSize, varExp.Type);
					}
				}
			}
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
			else
			{
				if (adrInfoInstance == null)
				{
					return null;
				}
				myPropertyAddressInfo = new PropertyAddressInfoExtended(nSize, varExp.Type)
				{
					InfoInstance = adrInfoInstance,
					InterfaceCall = (signature.POUType == Operator.Interface || signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
				};
			}
			PropertyCallCommon.DetermineGetterSetterOffsets(varExp, ivariable, myPropertyAddressInfo, signature2, signature, flag, this.Scope);
			if (!ivariable.GetFlag(VarFlag.Implicit))
			{
				IExpression expression = expInstancePath;
				if (this.m_expInstance != null)
				{
					expression = CompilerProxy.CreateParser(this.m_expInstance.ToString() + "." + expInstancePath.ToString()).ParseOperand();
					if (expression == null)
					{
						expression = expInstancePath;
					}
					else
					{
						CompilerProxy.TypifyExprement(expression as _IExpression, this.Scope, this.ComCon as _ICompileContext, null, false, false, null);
					}
					if (expression.Type == null)
					{
						expression = expInstancePath;
					}
					(expression as _IExpression)._CompiledType = (expInstancePath as _IExpression)._CompiledType;
				}
				VarRef varRef = new VarRef(expression as _IExpression, this.ApplicationGuid, this.Scope, this.m_expInstance);
				varRef._OrgExpression = varExp;
				varRef.Position = varExp.Position;
				varRef.AddressInfo = myPropertyAddressInfo;
				this.m_alVarReferences.Add(varRef);
				return myPropertyAddressInfo;
			}
			return null;
		}

		// Token: 0x06001376 RID: 4982 RVA: 0x00036B18 File Offset: 0x00035B18
		public IAddressInfo GenerateIndexAccess(IIndexAccessExpression indexaccess, int iArea, int nAddress, int nSize)
		{
			if (!this.CheckPosition(indexaccess.Position))
			{
				return null;
			}
			VarRef varRef = new VarRef(indexaccess as _IExpression, this.ApplicationGuid, this.Scope, this.m_expInstance);
			varRef.Position = indexaccess.Position;
			varRef.AddressInfo = new AbsoluteAddressInfo(iArea, nAddress, byte.MaxValue, nSize, indexaccess.Type);
			this.m_alVarReferences.Add(varRef);
			return varRef.AddressInfo;
		}

		// Token: 0x06001377 RID: 4983 RVA: 0x00036B8C File Offset: 0x00035B8C
		public IAddressInfo GenerateVariableIndexAccess(IIndexAccessExpression indexaccess, IAddressInfo aiWholeArray, int nBaseSize, IAddressInfo[] indexAccesses, IArrayBounds[] arrayBounds, IType t)
		{
			if (!this.CheckPosition(indexaccess.Position))
			{
				return null;
			}
			IAddressInfo addressInfo = this.DoGenerateVariableIndexAccess(indexaccess, aiWholeArray, nBaseSize, indexAccesses, arrayBounds, t);
			if (addressInfo == null)
			{
				return null;
			}
			VarRef varRef = new VarRef(indexaccess as _IExpression, this.ApplicationGuid, this.Scope, this.m_expInstance);
			varRef.Position = indexaccess.Position;
			varRef.AddressInfo = addressInfo;
			this.m_alVarReferences.Add(varRef);
			return varRef.AddressInfo;
		}

		// Token: 0x06001378 RID: 4984 RVA: 0x00036C01 File Offset: 0x00035C01
		public IAddressInfo GenerateOperatorExpression(_IOperatorExpression opExp, IAddressInfo[] addrsOperands)
		{
			return this.DoGenerateOperatorExpression(opExp, addrsOperands);
		}

		// Token: 0x06001379 RID: 4985 RVA: 0x00036C0B File Offset: 0x00035C0B
		public IAddressInfo GenerateSignedConstant(IExpression constExpr, int literalValue)
		{
			return this.DoGenerateSignedConstant(constExpr, literalValue);
		}

		// Token: 0x0600137A RID: 4986 RVA: 0x00036C15 File Offset: 0x00035C15
		public IAddressInfo GenerateAddress(ulong literalValue)
		{
			return this.DoGenerateAddress(literalValue);
		}

		// Token: 0x0600137B RID: 4987 RVA: 0x00036C1E File Offset: 0x00035C1E
		public IAddressInfo GenerateAddress(int iArea, int iOffset)
		{
			return this.DoGenerateAddress(iArea, iOffset);
		}

		// Token: 0x0600137C RID: 4988 RVA: 0x00036C28 File Offset: 0x00035C28
		public IAddressInfo GenerateDirectAddress(IAddressExpression addr, IDataLocation datloc, int nSize)
		{
			if (!this.CheckPosition(addr.Position))
			{
				return null;
			}
			VarRef varRef = new VarRef(addr as _IExpression, this.ApplicationGuid, this.Scope, this.m_expInstance);
			varRef.Position = addr.Position;
			if (addr.DirectAddress.Size == DirectVariableSize.X)
			{
				varRef.AddressInfo = new AbsoluteAddressInfo((int)datloc.Area, datloc.Offset, datloc.BitNr, 0, addr.Type);
			}
			else
			{
				varRef.AddressInfo = new AbsoluteAddressInfo((int)datloc.Area, datloc.Offset, byte.MaxValue, nSize, addr.Type);
			}
			this.m_alVarReferences.Add(varRef);
			return varRef.AddressInfo;
		}

		// Token: 0x0600137D RID: 4989 RVA: 0x00036CD8 File Offset: 0x00035CD8
		public IAddressInfo GenerateDeRefAccess(IExpression deref, IAddressInfo varrefelement, int nOffset)
		{
			if (!this.CheckPosition(deref.Position))
			{
				return null;
			}
			if (varrefelement == null)
			{
				return null;
			}
			VarRef varRef = new VarRef(deref as _IExpression, this.ApplicationGuid, this.Scope, this.m_expInstance);
			(deref as _IExpression)._CompiledType = deref.Type.DeRefType;
			varRef.Position = deref.Position;
			if (deref.Type.DeRefType.Size(this.Scope) != 0)
			{
				varRef.AddressInfo = new DeRefAccessInfo(varrefelement, deref.Type.DeRefType.Size(this.Scope), nOffset, deref.Type.DeRefType);
				this.m_alVarReferences.Add(varRef);
			}
			return varRef.AddressInfo;
		}

		// Token: 0x0600137E RID: 4990 RVA: 0x00036D94 File Offset: 0x00035D94
		public IAddressInfo GenerateCompoAccess(IExpression compo, IAddressInfo varrefelement, int nOffset)
		{
			if (!this.CheckPosition(compo.Position))
			{
				return null;
			}
			if (varrefelement == null)
			{
				return null;
			}
			VarRef varRef = new VarRef(compo as _IExpression, this.ApplicationGuid, this.Scope, this.m_expInstance);
			ICompoAccessExpression compoAccessExpression = compo as ICompoAccessExpression;
			if (compoAccessExpression != null)
			{
				varRef.Position = compoAccessExpression.Right.Position;
			}
			else
			{
				varRef.Position = compo.Position;
			}
			varRef.AddressInfo = new CompoAddressInfo(varrefelement, compo.Type.DeRefType.Size(this.Scope), nOffset, compo.Type);
			this.m_alVarReferences.Add(varRef);
			return varRef.AddressInfo;
		}

		// Token: 0x0600137F RID: 4991 RVA: 0x00036E38 File Offset: 0x00035E38
		public IAddressInfo GenerateBitAccess(IExpression compo, ICompiledType leftType, ISourcePosition varRefPosition, IAddressInfo varrefelement, int nBitOffset, int accessedElementSize)
		{
			if (!this.CheckPosition(compo.Position))
			{
				return null;
			}
			if (varrefelement == null)
			{
				return null;
			}
			VarRef varRef = new VarRef(compo as _IExpression, this.ApplicationGuid, this.Scope, this.m_expInstance);
			varRef.Position = varRefPosition;
			BitAddressInfo bitAddressInfo = new BitAddressInfo(varrefelement, leftType.DeRefType.Size(this.Scope), nBitOffset, compo.Type);
			bitAddressInfo.AccessedElementSize = accessedElementSize;
			varRef.AddressInfo = bitAddressInfo;
			this.m_alVarReferences.Add(varRef);
			return bitAddressInfo;
		}

		// Token: 0x06001380 RID: 4992 RVA: 0x00036EC0 File Offset: 0x00035EC0
		public IAddressInfo GenerateLiteral(IVariableExpression varexp, IExpression expInstancePath, ILiteralValue lv)
		{
			if (!this.CheckPosition(varexp.Position))
			{
				return null;
			}
			_IVariable ivariable = varexp.GetVariable(this.Scope) as _IVariable;
			Debug.Assert(ivariable != null && ivariable.GetFlag(VarFlag.ReplacedConstant) && ivariable.Initial != null);
			Debug.Assert(!ivariable.GetFlag(VarFlag.Implicit) && ivariable.VersionedName.IndexOf("__", StringComparison.OrdinalIgnoreCase) == -1);
			IExpression expression = expInstancePath;
			if (this.m_expInstance != null)
			{
				expression = CompilerProxy.CreateParser(this.m_expInstance.ToString() + "." + expInstancePath.ToString()).ParseOperand();
				if (expression == null)
				{
					expression = expInstancePath;
				}
				else
				{
					CompilerProxy.TypifyExprement(expression as _IExpression, this.Scope, this.ComCon as _ICompileContext, null, false, false, null);
				}
				if (expression.Type == null)
				{
					expression = expInstancePath;
				}
				(expression as _IExpression)._CompiledType = (varexp as _IExpression)._CompiledType;
			}
			VarRef varRef = new VarRef(expression as _IExpression, this.ApplicationGuid, this.Scope, this.m_expInstance);
			varRef.Position = varexp.Position;
			varRef.SetConstantValue(lv);
			varRef.SetFlag(VarRefFlag.Constant, true);
			varRef.AddressInfo = new LiteralAddressInfo(lv, varexp.Type);
			this.m_alVarReferences.Add(varRef);
			return varRef.AddressInfo;
		}

		// Token: 0x04000477 RID: 1143
		private readonly LList<VarRef> m_alVarReferences = new LList<VarRef>();

		// Token: 0x04000478 RID: 1144
		private Guid m_guidApplication;

		// Token: 0x04000479 RID: 1145
		private readonly long[] m_alPositionsOfInterest;

		// Token: 0x0400047A RID: 1146
		private readonly _IExpression m_expInstance;
	}
}
