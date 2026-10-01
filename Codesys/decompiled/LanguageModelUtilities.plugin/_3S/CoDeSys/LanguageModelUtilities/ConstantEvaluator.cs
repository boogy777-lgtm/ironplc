using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "A class implementing the visitor pattern normally violates the class coupling metric")]
	internal sealed class ConstantEvaluator : IExprVisitor, IConstantEvaluator2, IConstantEvaluator
	{
		private ILiteralValue _LitValue;

		private ISignature _owningSignature;

		private bool _bFound;

		private IEvaluationContext4 _evalContext;

		private IEvaluationContext4 _evalOriginalContext;

		private string _currentIndices = "";

		private IExpression _originalExpression;

		internal ConstantEvaluator(IEvaluationContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (!(context is IEvaluationContext4))
			{
				if (!(context is IEvaluationContext2))
				{
					int primaryProjectHandle = APEnvironmentFacade.Instance.PrimaryProjectHandle;
					context = new EvaluationContext4(primaryProjectHandle, primaryProjectHandle, context.ApplicationGuid, context.ScopeIdentification, Guid.Empty, new GetLibInformation());
				}
				else
				{
					IGetLibInformation getLibInformation2;
					if (!(context is IEvaluationContext3))
					{
						IGetLibInformation getLibInformation = new GetLibInformation();
						getLibInformation2 = getLibInformation;
					}
					else
					{
						getLibInformation2 = ((IEvaluationContext3)context).LibInfo;
					}
					IGetLibInformation libInfo = getLibInformation2;
					context = new EvaluationContext4(((IEvaluationContext2)context).ProjectHandle, ((IEvaluationContext2)context).AttractingProjectHandle, context.ApplicationGuid, context.ScopeIdentification, Guid.Empty, libInfo);
				}
			}
			_evalContext = (IEvaluationContext4)context;
			_evalOriginalContext = (IEvaluationContext4)context;
		}

		public void visit(IWhileStatement whilst)
		{
		}

		public void visit(IRepeatStatement repeat)
		{
		}

		public void visit(IForStatement forloop)
		{
		}

		public void visit(IExitStatement exit)
		{
		}

		public void visit(IContinueStatement cont)
		{
		}

		public void visit(ISequenceStatement seq)
		{
		}

		public void visit(IAssignmentExpression assign)
		{
			_LitValue = GetAsLongIfValid(assign.RValue, null);
			_bFound = true;
		}

		public void visit(IIfStatement ifst)
		{
		}

		public void visit(IReturnStatement returnst)
		{
		}

		public void visit(IJumpStatement gotost)
		{
		}

		public void visit(ILabelStatement label)
		{
		}

		public void visit(ICommentStatement comment)
		{
		}

		public void visit(IPragmaStatement pragma)
		{
		}

		public void visit(IExpressionStatement expstat)
		{
		}

		public void visit(ICallExpression call)
		{
			IAssignmentExpression[] inputAssigns = call.InputAssigns;
			if (inputAssigns.Length != 2)
			{
				throw new ConstantEvaluationException(string.Format(Strings.InvalidFunction_Error, call.Callee.ToString()));
			}
			ILiteralValue asLongIfValid = GetAsLongIfValid(inputAssigns[0], null);
			ILiteralValue asLongIfValid2 = GetAsLongIfValid(inputAssigns[1], null);
			if (asLongIfValid == null || asLongIfValid2 == null || asLongIfValid.KindOf != asLongIfValid2.KindOf)
			{
				throw new ConstantEvaluationException(string.Format(Strings.InvalidFunction_Error, call.Callee.ToString()));
			}
			if (asLongIfValid.KindOf == KindOfLiteral.String)
			{
				string value = string.Empty;
				if (!asLongIfValid.GetString(out value))
				{
					throw new ConstantEvaluationException(string.Format(Strings.InvalidType_Error, inputAssigns[0].Type.ToString()));
				}
				string value2 = string.Empty;
				if (!asLongIfValid2.GetString(out value2))
				{
					throw new ConstantEvaluationException(string.Format(Strings.InvalidType_Error, inputAssigns[1].Type.ToString()));
				}
				if (!call.Callee.ToString().Contains("concat"))
				{
					throw new ConstantEvaluationException(string.Format(Strings.InvalidFunction_Error, call.Callee.ToString()));
				}
				_LitValue = LiteralValue.CreateString(value + value2);
				_bFound = true;
			}
		}

		public void visit(IOperatorExpression op)
		{
			switch (op.Operands.Length)
			{
			case 1:
				visitUnaryOperatorExpression(op);
				break;
			case 2:
				visitBinaryOperatorExpression(op);
				break;
			default:
				throw new ConstantEvaluationException(string.Format(Strings.InvalidOperator_Error, op.Code));
			}
		}

		public void visit(IConversionExpression conv)
		{
			conv.Exp.AcceptVisitor(this);
		}

		public void visit(IThisExpression thisexp)
		{
		}

		public void visit(IBaseExpression baseexp)
		{
		}

		public void visit(ILiteralExpression literal)
		{
			_LitValue = GetAsLongIfValid(literal.Literal(null), null);
			_bFound = true;
		}

		public void visit(IAddressExpression address)
		{
		}

		public void visit(IVariableExpression variable)
		{
			IPreCompileContext precompileContext = Helpers.GetPrecompileContext(_evalContext);
			VisitVariableExpression(variable, precompileContext);
			bool flag = precompileContext != null && precompileContext.ApplicationGuid == Guid.Empty;
			if (!_bFound && !flag)
			{
				IPreCompileContext precompileContext2 = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(Guid.Empty);
				VisitVariableExpression(variable, precompileContext2);
			}
		}

		private void VisitVariableExpression(IVariableExpression variable, IPreCompileContext pcc)
		{
			if (pcc == null)
			{
				return;
			}
			IIdentifierInfo[] identifierInfo = pcc.GetIdentifierInfo(_evalContext.ScopeIdentification, variable.Name);
			ISignature signature = identifierInfo[0].Signature;
			bool flag = identifierInfo != null && identifierInfo.Length != 0 && signature != null && identifierInfo[0].Variable != null;
			if (!flag && _evalContext.LocalScopeIdentification != Guid.Empty)
			{
				identifierInfo = pcc.GetIdentifierInfo(_evalContext.LocalScopeIdentification, variable.Name);
				signature = identifierInfo[0].Signature;
				flag = identifierInfo != null && identifierInfo.Length != 0 && signature != null && identifierInfo[0].Variable != null;
			}
			if (flag)
			{
				_evalContext = Helpers.GetContextFromSignature(_evalContext.AttractingProjectHandle, (ISignature2)signature, null, GetLibInfo());
				if (!identifierInfo[0].Variable.HasFlag(VarFlag.Constant))
				{
					_bFound = false;
					return;
				}
				if (_currentIndices != "")
				{
					if (InitialValue.Determine(_evalContext, variable.ToString() + _currentIndices, out var expInit, out var ctxExpInit) != 0)
					{
						return;
					}
					_evalContext = (IEvaluationContext4)ctxExpInit;
					_LitValue = GetAsLongIfValid(expInit, null);
				}
				else
				{
					_LitValue = DetermineConstantValue(signature, identifierInfo[0].Variable);
					_owningSignature = signature;
				}
				_bFound = true;
			}
			else
			{
				if (!(_originalExpression is IVariableExpression))
				{
					return;
				}
				ISignature[] gVLSignatures = pcc.GVLSignatures;
				if (gVLSignatures == null)
				{
					return;
				}
				bool flag2 = false;
				int num = 0;
				while (!flag2 && num < gVLSignatures.Length)
				{
					int num2 = 0;
					IVariable[] all = gVLSignatures[num].All;
					while (!flag2 && num2 < all.Length)
					{
						flag2 = all[num2].OrgName.Equals(variable.Name, StringComparison.OrdinalIgnoreCase) && all[num2].GetFlag(VarFlag.Constant);
						if (flag2)
						{
							_LitValue = DetermineConstantValue(gVLSignatures[num], all[num2]);
							_owningSignature = gVLSignatures[num];
							_bFound = true;
						}
						else
						{
							num2++;
						}
					}
					if (!flag2)
					{
						num++;
					}
				}
			}
		}

		public void visit(IIndexAccessExpression indexaccess)
		{
			if (indexaccess.Accesses != null && indexaccess.Accesses.Length != 0)
			{
				_currentIndices = "[";
				int num = 1;
				IExpression[] accesses = indexaccess.Accesses;
				foreach (IExpression expression in accesses)
				{
					_currentIndices = _currentIndices + expression.ToString() + ((num < indexaccess.Accesses.Length) ? ", " : "");
					num++;
				}
				_currentIndices += "]";
			}
			indexaccess.Var.AcceptVisitor(this);
		}

		public void visit(ICompoAccessExpression compo)
		{
			IExpression expression = FindInitializationExpresionForCompoAccess(compo);
			if (expression != null)
			{
				_LitValue = GetAsLongIfValid(expression, null);
				_bFound = true;
				return;
			}
			throw new ConstantEvaluationException(string.Format(Strings.NoInit_Error, compo.ToString()));
		}

		public IExpression FindInitializationExpresionForCompoAccess(ICompoAccessExpression compo)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Expected O, but got Unknown
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Expected O, but got Unknown
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			DPath namespacePath = new DPath(compo.Left.ToString().ToLowerInvariant());
			ISignature signature = null;
			IList<string> restSigs;
			Pair<int, int> val = LibraryHelpers.FindLibByNamespace(_evalContext.ProjectHandle, _evalContext.ApplicationGuid, namespacePath, out restSigs, GetLibInfo());
			IPreCompileContext4 pcc = APEnvironmentFacade.Instance.GetPreCompileContextForProject(val.first);
			if (pcc != null)
			{
				restSigs.Add(compo.Right.ToString());
				DPath val2 = new DPath((IEnumerable<string>)restSigs);
				for (int i = 0; i < restSigs.Count; i++)
				{
					string text = restSigs[i];
					IIdentifierInfo[] identifierInfo = pcc.GetIdentifierInfo(_evalContext.ScopeIdentification, text);
					if (identifierInfo != null && identifierInfo.Length != 0 && identifierInfo[0].Signature != null)
					{
						signature = identifierInfo[0].Signature;
						UpdatePccForUnqualifiedLibraryAccess(signature, ref pcc);
						if ((signature.POUType == Operator.VarGlobal || Operator.Program == signature.POUType) && string.Compare(signature.Name, text, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							restSigs.Remove(text);
							val2 = new DPath((IEnumerable<string>)restSigs);
						}
						break;
					}
				}
				if (signature == null)
				{
					return null;
				}
				IExpression expInit = null;
				_evalContext = Helpers.GetContextFromSignature(val.second, (ISignature2)signature, null, GetLibInfo());
				IEvaluationContext2 ctxExpInit;
				if (signature.GetFlag(SignatureFlag.Enum))
				{
					IIdentifierInfo[] identifierInfo2 = pcc.GetIdentifierInfo(signature.ObjectGuid, compo.Right.ToString());
					if (identifierInfo2 != null && identifierInfo2.Length != 0 && identifierInfo2[0].Signature != null && identifierInfo2[0].Variable != null)
					{
						_LitValue = DetermineConstantValue(signature, identifierInfo2[0].Variable);
						return Helpers.ParseExpression(_LitValue.SignedLong.ToString());
					}
					if (_evalContext.ApplicationGuid != Guid.Empty && APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(_evalContext.ApplicationGuid) is IPreCompileContext4 preCompileContext)
					{
						identifierInfo2 = preCompileContext.GetIdentifierInfo(signature.ObjectGuid, compo.Right.ToString());
						if (identifierInfo2 != null && identifierInfo2.Length != 0 && identifierInfo2[0].Signature != null && identifierInfo2[0].Variable != null)
						{
							_LitValue = DetermineConstantValue(signature, identifierInfo2[0].Variable);
							return Helpers.ParseExpression(_LitValue.SignedLong.ToString());
						}
					}
				}
				else if (signature.HasAttribute(CompileAttributes.ATTRIBUTE_PARAMETERLIST))
				{
					IIdentifierInfo[] identifierInfo3 = pcc.GetIdentifierInfo(signature.ObjectGuid, compo.Right.ToString());
					IVariable variable = null;
					if (identifierInfo3 != null && identifierInfo3.Length != 0 && identifierInfo3[0].Signature != null && identifierInfo3[0].Variable != null)
					{
						variable = identifierInfo3[0].Variable;
					}
					if (variable == null)
					{
						IVariable[] all = signature.All;
						foreach (IVariable variable2 in all)
						{
							if (variable2.GetFlag(VarFlag.Constant) && variable2.OrgName.ToUpperInvariant().Equals(compo.Right.ToString().ToUpperInvariant()))
							{
								variable = variable2;
							}
						}
					}
					if (variable != null)
					{
						_LitValue = DetermineConstantValue(signature, variable);
						return Helpers.ParseExpression(_LitValue.SignedLong.ToString());
					}
				}
				else if (InitialValue.Determine(_evalContext, ((object)val2).ToString() + _currentIndices, out expInit, out ctxExpInit) == InitialValueResult.OK)
				{
					return expInit;
				}
			}
			return null;
		}

		private static void UpdatePccForUnqualifiedLibraryAccess(ISignature sig, ref IPreCompileContext4 pcc)
		{
			if (sig.LibraryPath != pcc.LibraryPath)
			{
				int projectHandle = APEnvironmentFacade.Instance.GetProjectHandle(sig.LibraryPath);
				if (0 <= projectHandle)
				{
					pcc = APEnvironmentFacade.Instance.GetPreCompileContextForProject(projectHandle);
				}
			}
		}

		public void visit(IDeRefAccessExpression deref)
		{
			deref.Base.AcceptVisitor(this);
		}

		public void visit(IGlobalScopeExpression globexp)
		{
		}

		public void visit(IEmptyStatement empty)
		{
		}

		public void visit(ICaseRangeExpression caserange)
		{
		}

		public void visit(ICaseLabelStatement caselabel)
		{
		}

		public void visit(ICaseStatement casest)
		{
		}

		public void visit(IBreakPointStatement bpstate)
		{
		}

		public void visit(IDefineReference defref)
		{
		}

		public void visit(IVariableReference varref)
		{
		}

		public void visit(ITypeReference typeref)
		{
		}

		public void visit(IPouReference pouref)
		{
		}

		public void visit(IDefinedExpression defexp)
		{
		}

		public void visit(IPragmaOperatorExpression popexp)
		{
		}

		public void visit(IPragmaIfStatement pifst)
		{
		}

		public void visit(IDefineStatement defstate)
		{
		}

		public void visit(IHasTypeExpression hastype)
		{
		}

		public void visit(IHasAttributeExpression hasattribute)
		{
		}

		public void visit(IHasValueExpression hasvalue)
		{
		}

		public void visit(IPragmaAssertion assertion)
		{
		}

		public IGetLibInformation2 GetLibInfo()
		{
			if (_evalContext != null && _evalContext.LibInfo is IGetLibInformation2)
			{
				return (IGetLibInformation2)_evalContext.LibInfo;
			}
			return new GetLibInformation();
		}

		private IVariable[] GetAllConstantVariables(ISignature sig)
		{
			List<IVariable> list = new List<IVariable>();
			IVariable[] all = sig.All;
			foreach (IVariable variable in all)
			{
				if (variable.HasFlag(VarFlag.Constant))
				{
					list.Add(variable);
				}
			}
			return list.ToArray();
		}

		private ILiteralValue DetermineConstantValue(ISignature signature, IVariable constant)
		{
			object parameterValue = null;
			if (signature.HasAttribute(CompileAttributes.ATTRIBUTE_PARAMETERLIST))
			{
				parameterValue = EvaluateConstantValueParameterList(signature, constant);
			}
			IExpression initExpr = constant.Initial;
			if (initExpr == null)
			{
				if (constant.HasFlag(VarFlag.Enum))
				{
					return EvaluateConstantValueEnum(signature, constant, parameterValue, ref initExpr);
				}
				throw new ConstantEvaluationException(string.Format(Strings.NoInitialValue_Error, constant.Name));
			}
			return GetAsLongIfValid(initExpr, parameterValue);
		}

		private object EvaluateConstantValueParameterList(ISignature signature, IVariable constant)
		{
			if (APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(APEnvironmentFacade.Instance.ActiveApplicationGuid) is IPreCompileContext8 preCompileContext)
			{
				try
				{
					return preCompileContext.GetParameterValue(signature.LibraryPath, constant.Name);
				}
				catch (NullReferenceException)
				{
					return null;
				}
			}
			return null;
		}

		private ILiteralValue EvaluateConstantValueEnum(ISignature signature, IVariable constant, object parameterValue, ref IExpression initExpr)
		{
			long value = -1L;
			IVariable[] allConstantVariables = GetAllConstantVariables(signature);
			for (int i = 0; i < allConstantVariables.Length; i++)
			{
				initExpr = allConstantVariables[i].Initial;
				if (initExpr == null)
				{
					value++;
				}
				else
				{
					ILiteralValue asLongIfValid = GetAsLongIfValid(initExpr, parameterValue);
					if (asLongIfValid == null || !asLongIfValid.GetSignedLong(out value))
					{
						throw new ConstantEvaluationException(Strings.EnumEval_Error);
					}
				}
				if (allConstantVariables[i].Name.Equals(constant.Name))
				{
					break;
				}
			}
			return LiteralValue.CreateSignedInteger(value);
		}

		private ILiteralValue GetAsLongIfValid(IExpression expr, object parameterValue)
		{
			ConstantEvaluator constantEvaluator = new ConstantEvaluator(_evalContext);
			return constantEvaluator.GetAsLongIfValid(constantEvaluator.Evaluate(expr), parameterValue);
		}

		private ILiteralValue GetAsLongIfValid(ILiteralValue literalVal, object parameterValue)
		{
			if (parameterValue is IExpression)
			{
				literalVal = GetAsLongIfValid((IExpression)parameterValue, null);
			}
			else if (literalVal.KindOf == KindOfLiteral.SignedInteger)
			{
				if (parameterValue != null)
				{
					literalVal = LiteralValue.CreateSignedInteger(Convert.ToInt64(parameterValue));
				}
			}
			else if (literalVal.KindOf == KindOfLiteral.UnsignedInteger)
			{
				ulong num = 0uL;
				num = ((parameterValue != null) ? Convert.ToUInt64(parameterValue) : literalVal.UnsignedLong);
				if (num > long.MaxValue)
				{
					throw new OverflowException(string.Format(Strings.Overflow_Error, literalVal.UnsignedLong));
				}
				literalVal = LiteralValue.CreateUnsignedInteger(num);
			}
			return literalVal;
		}

		public ILiteralValue Evaluate(string stExpression)
		{
			ISignature owningSignature = null;
			return Evaluate(stExpression, out owningSignature);
		}

		public ILiteralValue Evaluate(IExpression expr)
		{
			ISignature owningSignature = null;
			return Evaluate(expr, out owningSignature);
		}

		public ILiteralValue Evaluate(string stExpression, out ISignature owningSignature)
		{
			IExpression expression = Helpers.ParseExpression(stExpression);
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 22, 0))
			{
				return Evaluate(expression, out owningSignature);
			}
			ILMPreCompileTypifier2 obj = (ILMPreCompileTypifier2)((APEnvironmentFacade.Instance.LMServiceProvider as ILMServiceProvider5).PreCompileService as ILMPreCompileService5).CreatePreCompileTypifier(APEnvironmentFacade.Instance.ActiveApplicationGuid);
			IExpressionStatement stmt = APEnvironmentFacade.Instance.LMServiceProvider.CreatorService.CreateLanguageModelBuilder().CreateExpressionStatement(expression);
			_IExpressionStatement iExpressionStatement = (_IExpressionStatement)obj.TypifyStatement(stmt, Guid.Empty, ConversionOptions.TypesOnly);
			return Evaluate(iExpressionStatement._Expr, out owningSignature);
		}

		public ILiteralValue Evaluate(IExpression expr, out ISignature owningSignature)
		{
			owningSignature = null;
			_owningSignature = null;
			_originalExpression = expr;
			_evalContext = _evalOriginalContext;
			_currentIndices = string.Empty;
			_LitValue = null;
			if (expr == null)
			{
				throw new ArgumentNullException("expr");
			}
			ILiteralValue literalValue = null;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 20, 0))
			{
				literalValue = GetLiteralValueOfExpression(expr);
			}
			_bFound = false;
			try
			{
				expr.AcceptVisitor(this);
			}
			catch (LanguageModelUtilitiesException)
			{
				if (literalValue == null)
				{
					throw;
				}
			}
			catch
			{
				_bFound = false;
			}
			if (!_bFound && literalValue == null)
			{
				throw new ConstantEvaluationException(string.Format(Strings.NoConstantValue_Error, expr.ToString()));
			}
			owningSignature = _owningSignature;
			if (literalValue != null)
			{
				return literalValue;
			}
			return _LitValue;
		}

		private ILiteralValue GetLiteralValueOfExpression(IExpression expression)
		{
			Guid guidApplication = ((Guid.Empty == _evalContext.ApplicationGuid) ? APEnvironmentFacade.Instance.ActiveApplicationGuid : _evalContext.ApplicationGuid);
			ILMPreCompileSet preCompileSet = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetPreCompileSet(guidApplication);
			if (preCompileSet == null)
			{
				return null;
			}
			if (Guid.Empty == _evalContext.ScopeIdentification)
			{
				return null;
			}
			IPrecompileScope precompileScope = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.CreatePrecompileScope(preCompileSet, _evalContext.ScopeIdentification);
			if (precompileScope == null)
			{
				return null;
			}
			if (!(expression is _IExpression iExpression))
			{
				return null;
			}
			return iExpression.Literal(precompileScope);
		}

		private void visitUnaryOperatorExpression(IOperatorExpression op)
		{
			IExpression[] operands = op.Operands;
			ILiteralValue asLongIfValid = GetAsLongIfValid(operands[0], null);
			if (asLongIfValid == null)
			{
				throw new ConstantEvaluationException(string.Format(Strings.InvalidOperator_Error, op.Code));
			}
			if (asLongIfValid.KindOf == KindOfLiteral.SignedInteger)
			{
				if (!asLongIfValid.GetSignedLong(out long value))
				{
					throw new ConstantEvaluationException(string.Format(Strings.InvalidType_Error, operands[0].Type.ToString()));
				}
				if (op.Code != Operator.Abs)
				{
					throw new ConstantEvaluationException(string.Format(Strings.InvalidOperator_Error, op.Code));
				}
				long lValue = Math.Abs(value);
				_LitValue = LiteralValue.CreateSignedInteger(lValue);
				_bFound = true;
			}
		}

		private void visitBinaryOperatorExpression(IOperatorExpression op)
		{
			IExpression[] operands = op.Operands;
			ILiteralValue asLongIfValid = GetAsLongIfValid(operands[0], null);
			ILiteralValue asLongIfValid2 = GetAsLongIfValid(operands[1], null);
			if (asLongIfValid == null || asLongIfValid2 == null || asLongIfValid.KindOf != asLongIfValid2.KindOf)
			{
				throw new ConstantEvaluationException(string.Format(Strings.InvalidOperator_Error, op.Code));
			}
			if (asLongIfValid.KindOf == KindOfLiteral.SignedInteger)
			{
				visitSignedIntegerOperatorExpression(op, asLongIfValid, asLongIfValid2);
			}
			else if (asLongIfValid.KindOf == KindOfLiteral.UnsignedInteger)
			{
				visitUnsignedIntegerOperatorExpression(op, asLongIfValid, asLongIfValid2);
			}
		}

		private void visitSignedIntegerOperatorExpression(IOperatorExpression op, ILiteralValue valOp1, ILiteralValue valOp2)
		{
			IExpression[] operands = op.Operands;
			if (!valOp1.GetSignedLong(out long value))
			{
				throw new ConstantEvaluationException(string.Format(Strings.InvalidType_Error, operands[0].Type.ToString()));
			}
			if (!valOp2.GetSignedLong(out long value2))
			{
				throw new ConstantEvaluationException(string.Format(Strings.InvalidType_Error, operands[1].Type.ToString()));
			}
			long lValue;
			switch (op.Code)
			{
			case Operator.Plus:
				lValue = value + value2;
				break;
			case Operator.Minus:
				lValue = value - value2;
				break;
			case Operator.Times:
				lValue = value * value2;
				break;
			case Operator.Divide:
				lValue = value / value2;
				break;
			case Operator.Min:
				lValue = Math.Min(value, value2);
				break;
			case Operator.Max:
				lValue = Math.Max(value, value2);
				break;
			case Operator.Mod:
				lValue = value % value2;
				break;
			default:
				throw new ConstantEvaluationException(string.Format(Strings.InvalidOperator_Error, op.Code));
			}
			_LitValue = LiteralValue.CreateSignedInteger(lValue);
			_bFound = true;
		}

		private void visitUnsignedIntegerOperatorExpression(IOperatorExpression op, ILiteralValue valOp1, ILiteralValue valOp2)
		{
			IExpression[] operands = op.Operands;
			if (!valOp1.GetUnsignedLong(out ulong value))
			{
				throw new ConstantEvaluationException(string.Format(Strings.InvalidType_Error, operands[0].Type.ToString()));
			}
			if (!valOp2.GetUnsignedLong(out ulong value2))
			{
				throw new ConstantEvaluationException(string.Format(Strings.InvalidType_Error, operands[1].Type.ToString()));
			}
			ulong ulValue;
			switch (op.Code)
			{
			case Operator.Plus:
				ulValue = value + value2;
				break;
			case Operator.Minus:
				ulValue = value - value2;
				break;
			case Operator.Times:
				ulValue = value * value2;
				break;
			case Operator.Divide:
				ulValue = value / value2;
				break;
			default:
				throw new ConstantEvaluationException(string.Format(Strings.InvalidOperator_Error, op.Code));
			}
			_LitValue = LiteralValue.CreateUnsignedInteger(ulValue);
			_bFound = true;
		}
	}
}
