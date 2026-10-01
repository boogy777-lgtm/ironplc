using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.OnlineExpressionInterpreter;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000025 RID: 37
	internal class FlowPosVisitor : EmptyVisitor, IFlowPosVisitor351300, IFlowPosVisitor, IExprementVisitorNoTraversion, IExprementVisitorNoTraversion351300, IExprementVisitorNoTraversion3590
	{
		// Token: 0x06000196 RID: 406 RVA: 0x000053AC File Offset: 0x000043AC
		internal FlowPosVisitor()
		{
		}

		// Token: 0x06000197 RID: 407 RVA: 0x000053D4 File Offset: 0x000043D4
		internal static Flowposition GetFlowpositionOfExpression(_IExpression exptofind, ISourcePosition sourceposIn, _IStatement state, _IBreakpointList bplist, int nSignatureId, Guid guidApplication, CompiledPOU cpou, string stInstancePath, IVarRef varrefInstance, IScope scope)
		{
			if (state == null || sourceposIn == null)
			{
				return null;
			}
			FlowPosVisitor flowPosVisitor = new FlowPosVisitor();
			flowPosVisitor._sourcepos = sourceposIn;
			flowPosVisitor._expToFind = exptofind;
			flowPosVisitor._nSignatureId = nSignatureId;
			flowPosVisitor._guidApplication = guidApplication;
			flowPosVisitor._cpou = cpou;
			flowPosVisitor._stInstancePath = stInstancePath;
			flowPosVisitor._varrefInstance = (varrefInstance as VarRef);
			flowPosVisitor._scope = (scope as IScope5);
			IExprementVisitor ivisit = CompilerProxy.CreateFlowTraverser(flowPosVisitor, bplist);
			state.Accept(ivisit);
			return flowPosVisitor._flowFound;
		}

		// Token: 0x06000198 RID: 408 RVA: 0x0000544C File Offset: 0x0000444C
		private bool TestPosition(IExpression exp, bool bReadAccess)
		{
			if (exp == null)
			{
				return false;
			}
			if (exp.Position == null)
			{
				return false;
			}
			if (!this._dicExpToBreakpoint.ContainsKey(exp.Position.Position))
			{
				this._dicExpToBreakpoint.Add(exp.Position.Position, this._bpCurrent);
			}
			if (this._flowFound != null)
			{
				return false;
			}
			ExpressionComparer expressionComparer = new ExpressionComparer(exp as _IExpression);
			this._expToFind.Accept(expressionComparer);
			if (!expressionComparer.CodeEqual)
			{
				return false;
			}
			if (exp.Position.Position == this._sourcepos.Position && exp.Position.PositionOffset == this._sourcepos.PositionOffset)
			{
				this._flowFound = new Flowposition();
				this._flowFound.Position = exp.Position;
				this._flowFound.VarReference = (VarReferenceCreator.GetVarReference(exp as _IExpression, this._nSignatureId, this._guidApplication, this._varrefInstance) as IVarRef2);
				IExpression watchExpression = this._flowFound.WatchExpression;
				bool flag;
				if (watchExpression == null)
				{
					flag = false;
				}
				else
				{
					ICompiledType type = watchExpression.Type;
					TypeClass? typeClass = (type != null) ? new TypeClass?(type.Class) : null;
					TypeClass typeClass2 = TypeClass.Reference;
					flag = (typeClass.GetValueOrDefault() == typeClass2 & typeClass != null);
				}
				if (flag)
				{
					(this._flowFound.WatchExpression as _IExpression)._CompiledType = this._flowFound.WatchExpression.Type.DeRefType;
				}
				this._flowFound.CompiledPOU = this._cpou;
				this._flowFound.InstancePath = this._stInstancePath;
				this._flowFound.Breakpoint = this._bpCurrent;
				this._flowFound.LastListedBreakpoint = this.LastListedBreakpoint;
				this._flowFound.ReadAccess = bReadAccess;
				if (this._flowFound.VarReference.GetFlag(VarRefFlag.Extensible) && exp.Type.Class != TypeClass.Pointer)
				{
					Flowposition flowposition = new Flowposition();
					flowposition.Position = exp.Position;
					_IVariableExpression ivariableExpression = LanguageModelBuilder.Singleton.CreateVariableExpression("___XXX???");
					new StringType().Length = (LanguageModelBuilder.Singleton.CreateLiteralExpression(null, 1L) as _IExpression);
					ivariableExpression._CompiledType = TypeTable.Byte;
					VarRef varRef = new VarRef(ivariableExpression, this._guidApplication, this._scope);
					flowposition.VarReference = varRef;
					IVariable[] array;
					ISignature[] array2;
					IScope scope;
					if (!this._scope.FindDeclaration("__ApplicationName", out array, out array2, out scope))
					{
						return false;
					}
					IDataLocation dataLocation = array2[0].All[0].DataLocation;
					varRef.AddressInfo = new AbsoluteAddressInfo((int)dataLocation.Area, dataLocation.Offset, byte.MaxValue, TypeTable.ByteSize, TypeTable.Byte);
					varRef.SetFlag(VarRefFlag.Invalid, false);
					varRef.SetFlag(VarRefFlag.ReachedOnly, true);
					flowposition.CompiledPOU = this._cpou;
					flowposition.InstancePath = this._stInstancePath;
					flowposition.Breakpoint = this._bpCurrent;
					flowposition.LastListedBreakpoint = this.LastListedBreakpoint;
					flowposition.ReadAccess = true;
					this._flowFound = flowposition;
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000199 RID: 409 RVA: 0x0000574C File Offset: 0x0000474C
		public override void visit(_IVariableExpression variable, AccessFlag access)
		{
			bool bReadAccess = (access & AccessFlag.Write) == AccessFlag.None;
			this.TestPosition(variable, bReadAccess);
		}

		// Token: 0x0600019A RID: 410 RVA: 0x0000576C File Offset: 0x0000476C
		public override void visit(_ICallExpression call)
		{
			if (!this._dicExpToBreakpoint.ContainsKey(call.Position.Position))
			{
				this._dicExpToBreakpoint.Add(call.Position.Position, this._bpCurrent);
			}
			if (this._expToFind is _ICallExpression && call.Position.Position == this._sourcepos.Position && call.Position.PositionOffset == this._sourcepos.PositionOffset)
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
					this._flowFound = new Flowposition();
					this._flowFound.Position = call.Position;
					this._flowFound.CompiledPOU = this._cpou;
					this._flowFound.InstancePath = this._stInstancePath;
					this._flowFound.LastListedBreakpoint = this.LastListedBreakpoint;
					this._flowFound.ReadAccess = true;
					if (this._bpCurrent != null && this._bpCurrent.StepInSuccessors != null && this._bpCurrent.StepInSuccessors.Length != 0)
					{
						this._flowFound.Breakpoint = this._bpCurrent.StepInSuccessors[0].StepOutBreakpoint;
					}
					else
					{
						this._flowFound.Breakpoint = this._bpCurrent;
					}
					IVariable variable = signature.Outputs[0];
					_IExpression iexpression = call.Duplicate() as _IExpression;
					CompilerProxy.TypifyExprement(iexpression, this._scope, this._scope.ApplicationContext as _ICompileContext, null, false, false, this._cpou);
					iexpression._Position = call._Position;
					VarRef varRef = new VarRef(iexpression, this._guidApplication, this._scope);
					varRef.SetFlag(VarRefFlag.Invalid, false);
					varRef.AddressInfo = new StackRelativeAddressInfo(variable.DataLocation.Offset, byte.MaxValue, variable.CompiledType.Size(this._scope), (int)this._cpou.CompiledCode.Location.Area, this._cpou.CompiledCode.Location.Offset, this._cpou.CompiledCode.CodeSize, variable.CompiledType);
					(varRef.AddressInfo as StackRelativeAddressInfo).StackRelative = true;
					this._flowFound.VarReference = varRef;
				}
			}
		}

		// Token: 0x0600019B RID: 411 RVA: 0x00005A04 File Offset: 0x00004A04
		public override void visit(_IAddressExpression address)
		{
			this.TestPosition(address, false);
		}

		// Token: 0x0600019C RID: 412 RVA: 0x00005A10 File Offset: 0x00004A10
		public override void visit(_IConversionExpression conv)
		{
			if (this._expToFind is _IConversionExpression && conv.Position.Position == this._sourcepos.Position)
			{
				Flowposition flowposition = new Flowposition();
				flowposition.Position = this._sourcepos;
				flowposition.CompiledPOU = this._cpou;
				flowposition.InstancePath = this._stInstancePath;
				flowposition.Breakpoint = this._bpCurrent;
				flowposition.LastListedBreakpoint = this.LastListedBreakpoint;
				flowposition.ReadAccess = true;
				_IExpression iexpression = conv.Duplicate() as _IExpression;
				CompilerProxy.TypifyExprement(iexpression, this._scope, this._scope.ApplicationContext as _ICompileContext, null, false, false, this._cpou);
				VarRef varRef = new VarRef(iexpression, this._guidApplication, this._scope);
				varRef.AddressInfo = new ComplexAddressInfo();
				IOnlineExpressionInterpreter3 onlineExpressionInterpreter = APEnvironmentFacade.Instance.CreateOnlineExpressionInterpreter();
				OnlineExpressionException ex = null;
				IOnlineVarRefProvider2 onlineVarRefProvider = new FlowVarRefProvider2(this._guidApplication, this._scope, this._cpou, this._sourcepos, this._nSignatureId, this._varrefInstance, this._stInstancePath, this._bpCurrent, this.LastListedBreakpoint, this._dicExpToBreakpoint);
				IOnlineExpression2 flowOnlineExpression = onlineExpressionInterpreter.GetFlowOnlineExpression(this._guidApplication, iexpression, onlineVarRefProvider, out ex);
				(varRef.AddressInfo as ComplexAddressInfo).SetOnlineExpression(flowOnlineExpression);
				flowposition.VarReference = varRef;
				this._flowFound = flowposition;
			}
		}

		// Token: 0x0600019D RID: 413 RVA: 0x00005B64 File Offset: 0x00004B64
		public override void visit(_IOperatorExpression op)
		{
			ExpressionComparer expressionComparer = new ExpressionComparer(this._expToFind);
			op.Accept(expressionComparer);
			if (this._expToFind is _IOperatorExpression && op.Position.Position == this._sourcepos.Position && expressionComparer.CodeEqual)
			{
				Flowposition flowposition = new Flowposition();
				flowposition.Position = this._sourcepos;
				flowposition.CompiledPOU = this._cpou;
				flowposition.InstancePath = this._stInstancePath;
				flowposition.Breakpoint = this._bpCurrent;
				flowposition.LastListedBreakpoint = this.LastListedBreakpoint;
				flowposition.ReadAccess = true;
				_IExpression iexpression = op.Duplicate() as _IExpression;
				CompilerProxy.TypifyExprement(iexpression, this._scope, this._scope.ApplicationContext as _ICompileContext, null, false, false, this._cpou);
				iexpression = this.TryExchangeByLiteralExpression(iexpression);
				VarRef varRef = new VarRef(iexpression, this._guidApplication, this._scope);
				varRef.AddressInfo = new ComplexAddressInfo();
				IOnlineExpressionInterpreter3 onlineExpressionInterpreter = APEnvironmentFacade.Instance.CreateOnlineExpressionInterpreter();
				OnlineExpressionException ex = null;
				IOnlineVarRefProvider2 onlineVarRefProvider = new FlowVarRefProvider2(this._guidApplication, this._scope, this._cpou, this._sourcepos, this._nSignatureId, this._varrefInstance, this._stInstancePath, this._bpCurrent, this.LastListedBreakpoint, this._dicExpToBreakpoint);
				IOnlineExpression2 flowOnlineExpression = onlineExpressionInterpreter.GetFlowOnlineExpression(this._guidApplication, iexpression, onlineVarRefProvider, out ex);
				(varRef.AddressInfo as ComplexAddressInfo).SetOnlineExpression(flowOnlineExpression);
				if (flowOnlineExpression != null && ex == null)
				{
					flowposition.VarReference = varRef;
					this._flowFound = flowposition;
				}
			}
		}

		// Token: 0x0600019E RID: 414 RVA: 0x00005CEC File Offset: 0x00004CEC
		private _IExpression TryExchangeByLiteralExpression(_IExpression exp)
		{
			_IExpression iexpression = exp;
			ILiteralValue2 literalValue = exp.Literal(this._scope) as ILiteralValue2;
			if (literalValue != null)
			{
				_IExpression iexpression2 = LanguageModelBuilder.Singleton.CreateLiteralExpression(literalValue, exp.Type.Class);
				if (iexpression2 != null)
				{
					iexpression = iexpression2;
					CompilerProxy.TypifyExprement(iexpression, this._scope, this._scope.ApplicationContext as _ICompileContext, null, false, false, this._cpou);
				}
			}
			return iexpression;
		}

		// Token: 0x0600019F RID: 415 RVA: 0x00005D54 File Offset: 0x00004D54
		public virtual void visitCompoAccess(_ICompoAccessExpression compo, AccessFlag access)
		{
			bool bReadAccess = (access & AccessFlag.Write) == AccessFlag.None;
			this.TestPosition(compo, bReadAccess);
		}

		// Token: 0x060001A0 RID: 416 RVA: 0x00005D74 File Offset: 0x00004D74
		public virtual void visitIndexAccess(_IIndexAccessExpression index, AccessFlag access)
		{
			bool bReadAccess = (access & AccessFlag.Write) == AccessFlag.None;
			this.TestPosition(index, bReadAccess);
		}

		// Token: 0x060001A1 RID: 417 RVA: 0x00005D94 File Offset: 0x00004D94
		public virtual void visitDeRefAccess(_IDeRefAccessExpression deref, AccessFlag access)
		{
			bool bReadAccess = (access & AccessFlag.Write) == AccessFlag.None;
			this.TestPosition(deref, bReadAccess);
		}

		// Token: 0x060001A2 RID: 418 RVA: 0x00005DB1 File Offset: 0x00004DB1
		public IBreakpoint GetCurrentBreakpoint()
		{
			return this._bpCurrent;
		}

		// Token: 0x060001A3 RID: 419 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void GenerateDummyFlow(IBreakpoint bp)
		{
		}

		// Token: 0x060001A4 RID: 420 RVA: 0x00005DB9 File Offset: 0x00004DB9
		public virtual void SetCurrentBreakpoint(IBreakpoint bp)
		{
			if (bp != null)
			{
				this._bpCurrent = bp;
			}
		}

		// Token: 0x060001A5 RID: 421 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IPoolScopeExpression poolscope)
		{
		}

		// Token: 0x060001A6 RID: 422 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_ICurrentTaskExpression currentTaskExp)
		{
		}

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x060001A7 RID: 423 RVA: 0x00005DC5 File Offset: 0x00004DC5
		// (set) Token: 0x060001A8 RID: 424 RVA: 0x00005DCD File Offset: 0x00004DCD
		public IBreakpoint LastListedBreakpoint { get; set; }

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x060001A9 RID: 425 RVA: 0x00005DD6 File Offset: 0x00004DD6
		public IScope IScope
		{
			get
			{
				return this._scope;
			}
		}

		// Token: 0x04000032 RID: 50
		private ISourcePosition _sourcepos;

		// Token: 0x04000033 RID: 51
		internal IExpression _expFound;

		// Token: 0x04000034 RID: 52
		internal _IExpression _expToFind;

		// Token: 0x04000035 RID: 53
		private int _nSignatureId = -1;

		// Token: 0x04000036 RID: 54
		private Guid _guidApplication = Guid.Empty;

		// Token: 0x04000037 RID: 55
		private CompiledPOU _cpou;

		// Token: 0x04000038 RID: 56
		private Flowposition _flowFound;

		// Token: 0x04000039 RID: 57
		private IBreakpoint _bpCurrent;

		// Token: 0x0400003A RID: 58
		private string _stInstancePath;

		// Token: 0x0400003B RID: 59
		private VarRef _varrefInstance;

		// Token: 0x0400003C RID: 60
		private IScope5 _scope;

		// Token: 0x0400003D RID: 61
		private readonly LDictionary<long, IBreakpoint> _dicExpToBreakpoint = new LDictionary<long, IBreakpoint>();
	}
}
