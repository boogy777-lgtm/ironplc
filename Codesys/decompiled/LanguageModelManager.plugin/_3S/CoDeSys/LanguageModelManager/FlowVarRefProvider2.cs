using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Online;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.OnlineExpressionInterpreter;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000029 RID: 41
	internal class FlowVarRefProvider2 : IOnlineVarRefProvider3, IOnlineVarRefProvider2, IOnlineVarRefProvider
	{
		// Token: 0x060001BA RID: 442 RVA: 0x00005F18 File Offset: 0x00004F18
		public FlowVarRefProvider2(Guid guidApplication, IScope scope, CompiledPOU cpou, ISourcePosition sourcepos, int nSignatureId, IVarRef varRefInstance, string stInstancePath, IBreakpoint bp, IBreakpoint bpLastListed, IDictionary<long, IBreakpoint> dicExpToBreakpoint)
		{
			this._guidApplication = guidApplication;
			this._scope = (scope as IScope5);
			this._cpou = cpou;
			this._sourcepos = sourcepos;
			this._nSignatureId = nSignatureId;
			this._varRefInstance = varRefInstance;
			this._stInstancePath = stInstancePath;
			this._bp = bp;
			this._bpLastListed = bpLastListed;
			this._dicExpToBreakpoint = dicExpToBreakpoint;
		}

		// Token: 0x060001BB RID: 443 RVA: 0x00005F80 File Offset: 0x00004F80
		public IOnlineVarRef GetVarRef(IOnlineVarRefCreateArguments variable)
		{
			_IExpression iexpression = null;
			if (variable is FlowVarRefCreateArguments && (variable as FlowVarRefCreateArguments).Expression != null)
			{
				iexpression = ((variable as FlowVarRefCreateArguments).Expression as _IExpression);
				if (iexpression is ICallExpression)
				{
					return this.GetVarRef(iexpression as ICallExpression);
				}
			}
			if (iexpression == null)
			{
				iexpression = (variable.ParseExpression() as _IExpression);
				CompilerProxy.TypifyExprement(iexpression, this._scope, APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(this._guidApplication) as CompileContext, null, false, false, this._cpou);
			}
			Flowposition flowposition = new Flowposition();
			flowposition.Position = this._sourcepos;
			flowposition.VarReference = (VarReferenceCreator.GetVarReference(iexpression, this._nSignatureId, this._guidApplication, this._varRefInstance as VarRef) as IVarRef2);
			flowposition.CompiledPOU = this._cpou;
			flowposition.InstancePath = this._stInstancePath;
			flowposition.Breakpoint = this._bp;
			flowposition.LastListedBreakpoint = this._bpLastListed;
			flowposition.ReadAccess = true;
			return APEnvironmentFacade.Instance.CreateWatch(flowposition);
		}

		// Token: 0x060001BC RID: 444 RVA: 0x00005F08 File Offset: 0x00004F08
		public IOnlineVarRefCreateArgumentsFactory GetFactory()
		{
			return new FlowVarRefCreateArgumentsFactory();
		}

		// Token: 0x060001BD RID: 445 RVA: 0x00005F0F File Offset: 0x00004F0F
		public IOnlineVarRef GetVarRef(string stVariable)
		{
			return null;
		}

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x060001BE RID: 446 RVA: 0x00005E58 File Offset: 0x00004E58
		public bool GenerateVarRefForCall
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060001BF RID: 447 RVA: 0x00006084 File Offset: 0x00005084
		public IOnlineVarRef GetVarRef(ICallExpression call)
		{
			ISignature[] array = this._scope.FindSignature(call.Callee);
			ISignature signature = null;
			if (array != null && array.Length == 1)
			{
				signature = array[0];
			}
			if (signature == null && call.Callee.Type is UserdefType)
			{
				signature = (call.Callee.Type as UserdefType).GetSignature(this._scope);
			}
			if (signature != null && (signature.POUType == Operator.Function || signature.POUType == Operator.Method) && signature.Outputs.Length != 0)
			{
				IBreakpoint breakpoint = this._dicExpToBreakpoint[call.Position.Position];
				if (breakpoint != null && breakpoint.StepInSuccessors != null && breakpoint.StepInSuccessors.Length != 0)
				{
					Flowposition flowposition = new Flowposition();
					flowposition.Position = call.Position;
					flowposition.CompiledPOU = this._cpou;
					flowposition.InstancePath = this._stInstancePath;
					flowposition.LastListedBreakpoint = breakpoint;
					flowposition.ReadAccess = true;
					flowposition.Breakpoint = breakpoint.StepInSuccessors[0].StepOutBreakpoint;
					IVariable variable = signature.Outputs[0];
					_IExpression iexpression = (call as _IExpression).Duplicate() as _IExpression;
					CompilerProxy.TypifyExprement(iexpression, this._scope, this._scope.ApplicationContext as _ICompileContext, null, false, false, this._cpou);
					iexpression._Position = (call as _IExpression)._Position;
					VarRef varRef = new VarRef(iexpression, this._guidApplication, this._scope);
					varRef.SetFlag(VarRefFlag.Invalid, false);
					varRef.AddressInfo = new StackRelativeAddressInfo(variable.DataLocation.Offset, byte.MaxValue, variable.CompiledType.Size(this._scope), (int)this._cpou.CompiledCode.Location.Area, this._cpou.CompiledCode.Location.Offset, this._cpou.CompiledCode.CodeSize, variable.CompiledType);
					(varRef.AddressInfo as StackRelativeAddressInfo).StackRelative = true;
					flowposition.VarReference = varRef;
					return APEnvironmentFacade.Instance.CreateWatch(flowposition);
				}
			}
			return null;
		}

		// Token: 0x060001C0 RID: 448 RVA: 0x00005F12 File Offset: 0x00004F12
		public object GetKeyOfExpression(IExpression exp)
		{
			return exp;
		}

		// Token: 0x04000042 RID: 66
		private Guid _guidApplication;

		// Token: 0x04000043 RID: 67
		private readonly IScope5 _scope;

		// Token: 0x04000044 RID: 68
		private readonly CompiledPOU _cpou;

		// Token: 0x04000045 RID: 69
		private readonly ISourcePosition _sourcepos;

		// Token: 0x04000046 RID: 70
		private readonly int _nSignatureId;

		// Token: 0x04000047 RID: 71
		private readonly IVarRef _varRefInstance;

		// Token: 0x04000048 RID: 72
		private readonly string _stInstancePath;

		// Token: 0x04000049 RID: 73
		private readonly IBreakpoint _bp;

		// Token: 0x0400004A RID: 74
		private readonly IBreakpoint _bpLastListed;

		// Token: 0x0400004B RID: 75
		private readonly IDictionary<long, IBreakpoint> _dicExpToBreakpoint;
	}
}
