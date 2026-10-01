using System;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200002B RID: 43
	internal class PropertyFlowpositionCreator
	{
		// Token: 0x1700005D RID: 93
		// (get) Token: 0x060001DB RID: 475 RVA: 0x00006785 File Offset: 0x00005785
		// (set) Token: 0x060001DC RID: 476 RVA: 0x0000678D File Offset: 0x0000578D
		private Guid _guidApplication { get; set; }

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x060001DD RID: 477 RVA: 0x00006796 File Offset: 0x00005796
		// (set) Token: 0x060001DE RID: 478 RVA: 0x0000679E File Offset: 0x0000579E
		private _ICompiledPOU _cpou { get; set; }

		// Token: 0x060001DF RID: 479 RVA: 0x000067A7 File Offset: 0x000057A7
		internal PropertyFlowpositionCreator(Guid guidApplication, _ICompiledPOU cpou)
		{
			this._guidApplication = guidApplication;
			this._cpou = cpou;
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x000067C0 File Offset: 0x000057C0
		private void DoSetPosition(_IExpression exp, IMinimalPosition pos)
		{
			if (exp is _ICompoAccessExpression)
			{
				this.DoSetPosition((exp as _ICompoAccessExpression)._Left, pos);
				return;
			}
			if (exp is _ICallExpression)
			{
				this.DoSetPosition((exp as _ICallExpression)._Callee, pos);
				return;
			}
			if (exp is _IIndexAccessExpression)
			{
				this.DoSetPosition((exp as _IIndexAccessExpression)._Var, pos);
				return;
			}
			if (exp is _IDeRefAccessExpression)
			{
				this.DoSetPosition((exp as _IDeRefAccessExpression)._Base, pos);
				return;
			}
			if (exp is _IGlobalScopeExpression)
			{
				this.DoSetPosition((exp as _IGlobalScopeExpression)._Base, pos);
				return;
			}
			if (exp is _ISystemScopeExpression)
			{
				this.DoSetPosition((exp as _ISystemScopeExpression)._Base, pos);
				return;
			}
			if (exp is _IPoolScopeExpression)
			{
				this.DoSetPosition((exp as _IPoolScopeExpression)._Base, pos);
				return;
			}
			exp._Position = pos;
		}

		// Token: 0x060001E1 RID: 481 RVA: 0x00006894 File Offset: 0x00005894
		internal bool ChangeFlowpositionForProperty(IScope5 _scope, IBreakpoint _bpCurrent, Flowposition flowpos, _IExpression exp, bool bReadAccess)
		{
			try
			{
				IVariable variable = exp.GetVariable(_scope);
				if (variable != null && (variable as _IVariable).IsProperty)
				{
					if (!bReadAccess)
					{
						if (variable.HasAttribute(CompileAttributes.ATTRIBUTE_MONITORING))
						{
							return true;
						}
						return false;
					}
					else
					{
						ISignature signPropertyMethod = PropertyFlowpositionCreator.FindSignatureForPropertyMethod(_scope, exp, variable);
						if (!this.ChangeFlowpositionInternal(_scope, _bpCurrent, flowpos, exp, variable, signPropertyMethod))
						{
							return false;
						}
					}
				}
			}
			catch
			{
			}
			return true;
		}

		// Token: 0x060001E2 RID: 482 RVA: 0x00006908 File Offset: 0x00005908
		private bool ChangeFlowpositionInternal(IScope5 _scope, IBreakpoint _bpCurrent, Flowposition flowpos, _IExpression exp, IVariable varProperty, ISignature signPropertyMethod)
		{
			if (_bpCurrent != null && signPropertyMethod != null)
			{
				string text = exp.ToString();
				if (!text.EndsWith(varProperty.OrgName))
				{
					return false;
				}
				_IExpression iexpression = this.CreateCallExpression(_scope, varProperty, signPropertyMethod, ref text);
				this.DoSetPosition(iexpression, exp._Position);
				VarRef varRef;
				IVariable varOut;
				this.CreateStackRelativeAddressInfoForReturnValue(_scope, signPropertyMethod, iexpression, out varRef, out varOut);
				PropertyFlowpositionCreator.ChangeAddressInfoForReferenceTypes(_scope, varRef, varOut);
				flowpos.VarReference = varRef;
				IBreakpoint breakpoint = PropertyFlowpositionCreator.FindMatchingStepOutBreakpoint(flowpos, exp);
				if (breakpoint == null)
				{
					return false;
				}
				flowpos.Breakpoint = breakpoint;
			}
			return true;
		}

		// Token: 0x060001E3 RID: 483 RVA: 0x00006988 File Offset: 0x00005988
		private static ISignature FindSignatureForPropertyMethod(IScope5 _scope, _IExpression exp, IVariable varHelp)
		{
			ISignature result = null;
			ISignature signature = _scope[exp.SignatureId];
			if (exp is _ICompoAccessExpression)
			{
				signature = _scope[(exp as _ICompoAccessExpression)._Right.SignatureId];
			}
			if (signature != null)
			{
				result = _scope.CreateLocalScope(signature).FindSignatureLocal(IdentifierConstants.CreateGetterName((varHelp as _IVariable).VersionedName));
			}
			return result;
		}

		// Token: 0x060001E4 RID: 484 RVA: 0x000069E4 File Offset: 0x000059E4
		private _IExpression CreateCallExpression(IScope5 _scope, IVariable varHelp, ISignature signHelp, ref string stHelp)
		{
			stHelp = stHelp.Remove(stHelp.Length - varHelp.OrgName.Length);
			stHelp = stHelp + signHelp.Name + "()";
			_IExpression iexpression = CompilerProxy.CreateParser(stHelp, true).ParseExpression() as _IExpression;
			CompilerProxy.TypifyExprement(iexpression, _scope, _scope.ApplicationContext as _ICompileContext, null, false, false, this._cpou);
			return iexpression;
		}

		// Token: 0x060001E5 RID: 485 RVA: 0x00006A54 File Offset: 0x00005A54
		private void CreateStackRelativeAddressInfoForReturnValue(IScope5 _scope, ISignature signHelp, _IExpression exp2, out VarRef varref, out IVariable varOut)
		{
			varref = new VarRef(exp2, this._guidApplication, _scope);
			varOut = signHelp.Outputs[0];
			varref.SetFlag(VarRefFlag.Invalid, false);
			varref.AddressInfo = new StackRelativeAddressInfo(varOut.DataLocation.Offset, byte.MaxValue, varOut.CompiledType.Size(_scope), (int)this._cpou.CompiledCode.Location.Area, this._cpou.CompiledCode.Location.Offset, this._cpou.CompiledCode.CodeSize, varOut.CompiledType);
			(varref.AddressInfo as StackRelativeAddressInfo).StackRelative = true;
		}

		// Token: 0x060001E6 RID: 486 RVA: 0x00006B08 File Offset: 0x00005B08
		private static void ChangeAddressInfoForReferenceTypes(IScope5 _scope, VarRef varref, IVariable varOut)
		{
			if (varOut.CompiledType.Class == TypeClass.Reference)
			{
				varref.AddressInfo = new DeRefAccessInfo(varref.AddressInfo, varOut.CompiledType.DeRefType.Size(_scope), 0, varOut.CompiledType.DeRefType);
				varref._WatchExpression._CompiledType = varOut.CompiledType.DeRefType;
			}
		}

		// Token: 0x060001E7 RID: 487 RVA: 0x00006B68 File Offset: 0x00005B68
		private static IBreakpoint FindMatchingStepOutBreakpoint(Flowposition flowpos, _IExpression exp)
		{
			IBreakpoint result = null;
			if (flowpos.Breakpoint.StepInSuccessors.Length != 0)
			{
				result = flowpos.Breakpoint.StepInSuccessors.Last<IStepInPosition>().StepOutBreakpoint;
				foreach (IStepInPosition stepInPosition in flowpos.Breakpoint.StepInSuccessors)
				{
					if (stepInPosition.StepOutBreakpoint != null && stepInPosition.StepOutBreakpoint.Position.PositionCombination == exp.Position.PositionCombination)
					{
						result = stepInPosition.StepOutBreakpoint;
					}
				}
			}
			return result;
		}
	}
}
