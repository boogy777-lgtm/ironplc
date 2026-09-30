using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.MonitoringByteCode
{
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes", Justification = "The visitor pattern normally violates the class coupling metric")]
	internal class InterpreterCodeGenerator : IExprVisitor2, IExprVisitor
	{
		private class BitAccessInfo
		{
			private readonly int _lenChars;

			private readonly int _bit;

			public int LenChars => _lenChars;

			public int Bit => _bit;

			public BitAccessInfo(int lenChars, int bit)
			{
				_lenChars = lenChars;
				_bit = bit;
			}
		}

		private readonly IScope _scope;

		private readonly ITypeInfo3 _typeInfo;

		private readonly IStatement _state;

		private readonly ICompileContext _comcon;

		private readonly ByteProgramCreator _bpc;

		private readonly ByteCmdCreator _bcc;

		private readonly FStack _fstack;

		private readonly InterpreterCodeAdapter _adapter;

		private readonly CodeTranslator _codeTranslator;

		private Guid _guidApplication;

		private readonly Stack<StackContent> _resultMode = new Stack<StackContent>();

		private readonly Stack<ByteProgramCreator.ECompileMode> _variableAccessMode = new Stack<ByteProgramCreator.ECompileMode>();

		private readonly Stack<BitAccessInfo> _writeBitAccess = new Stack<BitAccessInfo>();

		internal InterpreterCodeGenerator(Guid guidApplication, ByteProgramCreator bpc, ByteCmdCreator bcc, IScope scope, IExpressionTypifier6 typifier, IStatement state)
		{
			_guidApplication = guidApplication;
			_scope = scope;
			_typeInfo = APEnvironmentFacade.Instance.LanguageModelMgr.TypeInfo as ITypeInfo3;
			APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder();
			_state = state;
			_bpc = bpc;
			_bcc = bcc;
			_fstack = new FStack(_bcc);
			_comcon = APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(_guidApplication);
			_adapter = new InterpreterCodeAdapter();
			_codeTranslator = new CodeTranslator(_comcon, typifier, _scope);
		}

		internal bool CreateCode(out string stErrorMsg)
		{
			bool result = false;
			stErrorMsg = string.Empty;
			try
			{
				_state.AcceptVisitor(this);
				_bpc.EndCompile();
				return result;
			}
			catch (UnsupportedInterpreterCodeException ex)
			{
				result = true;
				stErrorMsg = ExecutionpointStrings.UnsupportedStatementInBreakpointCode;
				stErrorMsg = stErrorMsg + Environment.NewLine + ex.Message;
				return result;
			}
			catch (Exception)
			{
				result = true;
				stErrorMsg = ExecutionpointStrings.UnsupportedBreakpointCode;
				return result;
			}
		}

		internal bool CreateReadCode(out string stErrorMsg)
		{
			bool result = false;
			stErrorMsg = string.Empty;
			try
			{
				ReadVariable();
				_state.AcceptVisitor(this);
				EnsureAddressOnStack(TypeClass.Bool);
				_bpc.EndCompile();
				return result;
			}
			catch (UnsupportedInterpreterCodeException ex)
			{
				result = true;
				stErrorMsg = ExecutionpointStrings.UnsupportedStatementInBreakpointCode;
				stErrorMsg = stErrorMsg + Environment.NewLine + ex.Message;
				return result;
			}
			catch (Exception)
			{
				result = true;
				stErrorMsg = ExecutionpointStrings.UnsupportedBreakpointCode;
				return result;
			}
		}

		private void EnsureValueOnStack(TypeClass requiredType)
		{
			if ((_resultMode.Pop() ?? throw new UnsupportedInterpreterCodeException(ExecutionpointStrings.NoArgumentsOnTheInterpreterStack)).ResultMode == ByteProgramCreator.EResultMode.Address)
			{
				ByteProgramCreator.EValueType eValueType = ByteProgramCreator.EValueType.Raw;
				if (TH.IsSignedInteger(requiredType))
				{
					eValueType = ByteProgramCreator.EValueType.Signed;
				}
				int size = _typeInfo.GetSize(requiredType, _scope);
				_bpc.AddressToValue(size, eValueType);
			}
		}

		private int EnsureAddressOnStack(TypeClass targetType)
		{
			StackContent stackContent = _resultMode.Pop();
			if (stackContent == null)
			{
				throw new UnsupportedInterpreterCodeException(ExecutionpointStrings.NoArgumentsOnTheInterpreterStack);
			}
			if (stackContent.ResultMode == ByteProgramCreator.EResultMode.Value)
			{
				int size = _typeInfo.GetSize(targetType, _scope);
				ValueToAddress(size);
				return size;
			}
			return stackContent.ValueSizeInBytes;
		}

		private int ValueToAddress(int iSizeBytes)
		{
			if (iSizeBytes != 1 && iSizeBytes != 2 && iSizeBytes != 4 && iSizeBytes != 8)
			{
				throw new UnsupportedInterpreterCodeException(string.Format(ExecutionpointStrings.ValueSizeCannotBeUsed, iSizeBytes));
			}
			if (iSizeBytes > _bcc.PointerSizeBytes)
			{
				_bcc.FLd64();
				_bcc.FAddrE();
				return iSizeBytes;
			}
			_bcc.FLd((byte)iSizeBytes);
			_bcc.FAddrE();
			return _fstack.Align(iSizeBytes);
		}

		private void LeavingValueOnStack()
		{
			_resultMode.Push(new StackContent(ByteProgramCreator.EResultMode.Value, 0));
		}

		private void LeavingAddressOnStack(int valueSize)
		{
			_resultMode.Push(new StackContent(ByteProgramCreator.EResultMode.Address, valueSize));
		}

		private void ReadVariable()
		{
			_variableAccessMode.Push(ByteProgramCreator.ECompileMode.Read);
		}

		private void WriteVariable()
		{
			_variableAccessMode.Push(ByteProgramCreator.ECompileMode.Write);
		}

		private ByteProgramCreator.ECompileMode GetVariableAccessMode()
		{
			return _variableAccessMode.Peek();
		}

		private void VariableAccessDone()
		{
			if (_variableAccessMode.Count > 0)
			{
				_variableAccessMode.Pop();
			}
		}

		private void WriteBit(int sizeBytes, int bit)
		{
			BitAccessInfo item = new BitAccessInfo(sizeBytes, bit);
			_writeBitAccess.Push(item);
		}

		private BitAccessInfo GetLValueBitToWrite()
		{
			BitAccessInfo result = null;
			if (_writeBitAccess.Count > 0)
			{
				result = _writeBitAccess.Pop();
			}
			return result;
		}

		public void visit(ISequenceStatement seq)
		{
			IStatement[] statements = seq.Statements;
			for (int i = 0; i < statements.Length; i++)
			{
				statements[i].AcceptVisitor(this);
			}
		}

		public void visit(IExpressionStatement expstat)
		{
			expstat.Expr.AcceptVisitor(this);
		}

		public void visit(IAssignmentExpression assign)
		{
			ICallExpression callExpression = _codeTranslator.CreatePropertyWriteCall(assign.LValue, assign.RValue);
			if (callExpression != null)
			{
				callExpression.AcceptVisitor(this);
				return;
			}
			_fstack.BeginTemporaryArea();
			WriteVariable();
			assign.LValue.AcceptVisitor(this);
			VariableAccessDone();
			int val = EnsureAddressOnStack(assign.LValue.Type.Class);
			BitAccessInfo lValueBitToWrite = GetLValueBitToWrite();
			ReadVariable();
			assign.RValue.AcceptVisitor(this);
			VariableAccessDone();
			if (lValueBitToWrite != null)
			{
				EnsureValueOnStack(assign.RValue.Type.Class);
				_bcc.LdS(lValueBitToWrite.Bit);
				_bcc.SetBit((byte)lValueBitToWrite.LenChars);
			}
			else
			{
				int val2 = EnsureAddressOnStack(assign.RValue.Type.Class);
				val2 = Math.Min(val2, val);
				_bcc.Ld32(Convert.ToUInt32(val2));
				_bcc.Cpy();
			}
			_fstack.EndTemporaryArea();
		}

		public void visit(IOperatorExpression op)
		{
			IOperatorExpression operatorExpression = _codeTranslator.ReduceToPrimitiveLogicOperations(op);
			if (operatorExpression != null)
			{
				operatorExpression.AcceptVisitor(this);
			}
			else
			{
				GenerateCodeForOperator(op);
			}
		}

		private void GenerateCodeForOperator(IOperatorExpression op)
		{
			if (_adapter.IsLogicOperator(op.Code) && op.Type.Class == TypeClass.Bool)
			{
				LogicOperator(op);
				return;
			}
			if (_adapter.IsArithmeticOperator(op.Code))
			{
				ArithmeticOperator(op);
				return;
			}
			if (_adapter.IsAddressOperator(op.Code))
			{
				AddressOperator(op);
				return;
			}
			throw new UnsupportedInterpreterCodeException(string.Format(Strings.Err_UnsupportedOperator_1, op.Code));
		}

		private void LogicOperator(IOperatorExpression op)
		{
			IExpression[] operands = op.Operands;
			foreach (IExpression expression in operands)
			{
				ReadVariable();
				expression.AcceptVisitor(this);
				VariableAccessDone();
				EnsureValueOnStack(expression.Type.DeRefType.Class);
			}
			switch (op.Code)
			{
			case Operator.LessEqual:
			case Operator.NotEqual:
			{
				ICompiledType deRefType = op.Operands[0].Type.DeRefType;
				CreateSubtractionCmd(deRefType.Class, op);
				if (deRefType.Size(_scope) <= _bcc.PointerSizeBytes)
				{
					_bcc.Pop();
				}
				else
				{
					_bcc.Pop64();
				}
				if (op.Code == Operator.LessEqual)
				{
					if (TH.IsSignedInteger(deRefType.Class) || TH.IsFloat(deRefType.Class))
					{
						_bcc.SetLE();
					}
					else
					{
						_bcc.SetBE();
					}
				}
				else
				{
					_bcc.SetNZ();
				}
				break;
			}
			case Operator.And:
				_bcc.MulS();
				break;
			case Operator.Or:
				_bcc.Add();
				_bcc.Pop();
				_bcc.SetNZ();
				break;
			default:
				throw new UnsupportedInterpreterCodeException(string.Format(ExecutionpointStrings.InterpreterOperatorIsNotSupported, op.Code.ToString()));
			}
			LeavingValueOnStack();
		}

		private void ArithmeticOperator(IOperatorExpression op)
		{
			IExpression[] operands = op.Operands;
			foreach (IExpression expression in operands)
			{
				ReadVariable();
				expression.AcceptVisitor(this);
				VariableAccessDone();
				EnsureValueOnStack(expression.Type.DeRefType.Class);
			}
			if (op.Code == Operator.Times || op.Code == Operator.Divide || op.Code == Operator.Mod)
			{
				ICompiledType deRefType = op.Type.DeRefType;
				if (!TH.IsSignedInteger(deRefType.Class) || deRefType.Size(null) > _bcc.PointerSizeBytes)
				{
					throw new UnsupportedInterpreterCodeException(string.Format(ExecutionpointStrings.InterpreterOperatorNotSupportedForType, op.Code, deRefType));
				}
			}
			switch (op.Code)
			{
			case Operator.Plus:
				CreateAdditionCmd(op.Type.DeRefType.Class);
				break;
			case Operator.Minus:
				CreateSubtractionCmd(op.Type.DeRefType.Class, op);
				break;
			case Operator.Times:
				_bcc.MulS();
				break;
			case Operator.Divide:
				_bcc.DivS();
				break;
			case Operator.Mod:
				_bcc.ModS();
				break;
			default:
				throw new UnsupportedInterpreterCodeException(string.Format(ExecutionpointStrings.InterpreterOperatorIsNotSupported, op.Code));
			}
			LeavingValueOnStack();
		}

		private void AddressOperator(IOperatorExpression op)
		{
			ReadVariable();
			op.Operands[0].AcceptVisitor(this);
			VariableAccessDone();
			if (op.Code == Operator.Adr)
			{
				EnsureAddressOnStack(op.Operands[0].Type.DeRefType.Class);
			}
			else if (op.Code == Operator.DeRef)
			{
				EnsureValueOnStack(op.Operands[0].Type.DeRefType.Class);
			}
			LeavingValueOnStack();
		}

		private void CreateAdditionCmd(TypeClass tc)
		{
			if (TH.IsInteger32(tc) || (TH.IsInteger(tc) && _bcc.PointerSizeBytes == 8))
			{
				_bcc.Add();
			}
			else if (TH.IsInteger64(tc))
			{
				_bcc.Add64();
			}
			else if (TH.IsReal(tc))
			{
				_bcc.AddR();
			}
			else if (TH.IsLReal(tc))
			{
				_bcc.AddLR();
			}
		}

		private void CreateSubtractionCmd(TypeClass tc, IExpression exp)
		{
			if (!TH.IsInteger(tc) && !TH.IsFloat(tc) && tc != TypeClass.Pointer)
			{
				throw new UnsupportedInterpreterCodeException(exp.ToString());
			}
			if (TH.IsInteger32(tc) || TH.IsPointer32(tc, _bcc.PointerSizeBytes))
			{
				_bcc.Sub();
			}
			else if (TH.IsInteger64(tc) || TH.IsPointer64(tc, _bcc.PointerSizeBytes))
			{
				_bcc.Sub64();
			}
			else if (TH.IsReal(tc))
			{
				_bcc.SubR();
			}
			else if (TH.IsLReal(tc))
			{
				_bcc.SubLR();
			}
		}

		public void visit(IConversionExpression conv)
		{
			TypeClass from = conv.From;
			TypeClass to = conv.To;
			ICallExpression2 callExpression = _codeTranslator.CreateExternalFuncCallForConversion(conv);
			if (callExpression != null)
			{
				callExpression.AcceptVisitor(this);
				return;
			}
			conv.Exp.AcceptVisitor(this);
			EnsureValueOnStack(from);
			if (TH.IsDateTime(from) && TH.IsDateTime(to) && from != to)
			{
				throw new UnsupportedInterpreterCodeException(string.Format(Strings.Err_UnsupportedConversion_2, from, to));
			}
			if (TH.IsInteger(from) && TH.IsInteger(to))
			{
				CvtInt(from, to);
			}
			else
			{
				if (!TH.IsFloat(from) && !TH.IsFloat(to))
				{
					throw new UnsupportedInterpreterCodeException(string.Format(Strings.Err_UnsupportedConversion_2, from, to));
				}
				CvtFloat(from, to);
			}
			LeavingValueOnStack();
		}

		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-95535")]
		private void CvtInt(TypeClass tcFr, TypeClass tcTo)
		{
			ICompiledType typeFromClass = TMH.GetTypeFromClass(tcFr);
			ICompiledType typeFromClass2 = TMH.GetTypeFromClass(tcTo);
			int num = typeFromClass.Size(null);
			int num2 = typeFromClass2.Size(null);
			if ((TH.IsInteger32(tcFr) && TH.IsInteger32(tcTo)) || _bcc.PointerSizeBytes == 8)
			{
				if (num < num2)
				{
					if (TH.IsSignedInteger(tcFr))
					{
						_bcc.Es((byte)(num * 8));
					}
					return;
				}
				if (num > num2)
				{
					if (TH.IsBitOrBool(tcTo))
					{
						_bcc.LdU(0uL);
						_bcc.Sub();
						_bcc.Pop();
						_bcc.SetNZ();
					}
					else
					{
						ulong ulImmediate = (ulong)((1L << num2 * 8) - 1);
						_bcc.LdU(ulImmediate);
						_bcc.And();
					}
				}
				if (TH.IsSignedInteger(tcTo))
				{
					_bcc.Es((byte)(num2 * 8));
				}
			}
			else
			{
				if (TH.IsInteger64(tcFr) && TH.IsInteger64(tcTo))
				{
					return;
				}
				if (TH.IsInteger64(tcFr))
				{
					_bcc.C64To32();
					CvtInt(TypeClass.UDInt, tcTo);
				}
				else if (TH.IsInteger64(tcTo))
				{
					_bcc.C32To64();
					if (TH.IsSignedInteger(tcTo))
					{
						_bcc.Es64(32);
					}
				}
			}
		}

		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-95535")]
		private void CvtFloat(TypeClass tcFr, TypeClass tcTo)
		{
			if (TH.IsFloat(tcFr) && TH.IsFloat(tcTo))
			{
				if (TH.IsReal(tcFr) && TH.IsLReal(tcTo))
				{
					_bcc.CRToLR();
				}
				else if (TH.IsLReal(tcFr) && TH.IsReal(tcTo))
				{
					_bcc.CLRToR();
				}
				return;
			}
			if (TH.IsInteger(tcFr))
			{
				if (TH.IsInteger32(tcFr) || _bcc.PointerSizeBytes == 8)
				{
					if (TH.IsReal(tcTo))
					{
						_bcc.CIntToR(TH.IsSignedInteger(tcFr));
					}
					else
					{
						_bcc.CIntToLR(TH.IsSignedInteger(tcFr));
					}
				}
				else if (TH.IsReal(tcTo))
				{
					_bcc.CIntToR64(TH.IsSignedInteger(tcFr));
				}
				else
				{
					_bcc.CIntToLR64(TH.IsSignedInteger(tcFr));
				}
				return;
			}
			if (TH.IsInteger(tcTo))
			{
				if (TH.IsInteger32(tcTo) || _bcc.PointerSizeBytes == 8)
				{
					if (TH.IsReal(tcFr))
					{
						_bcc.CRToInt(TH.IsSignedInteger(tcTo));
					}
					else
					{
						_bcc.CLRToInt(TH.IsSignedInteger(tcTo));
					}
					TypeClass tcFr2 = ((_bcc.PointerSizeBytes != 8) ? (TH.IsSignedInteger(tcTo) ? TypeClass.DInt : TypeClass.UDInt) : (TH.IsSignedInteger(tcTo) ? TypeClass.LInt : TypeClass.ULInt));
					CvtInt(tcFr2, tcTo);
				}
				else if (TH.IsReal(tcFr))
				{
					_bcc.CRToInt64(TH.IsSignedInteger(tcTo));
				}
				else
				{
					_bcc.CLRToInt64(TH.IsSignedInteger(tcTo));
				}
				return;
			}
			throw new UnsupportedInterpreterCodeException(string.Format(Strings.Err_UnsupportedConversion_2, tcFr, tcTo));
		}

		public void visit(IIfStatement ifst)
		{
			ReadVariable();
			ifst.Condition.AcceptVisitor(this);
			VariableAccessDone();
			_bcc.BnZ(8);
			_bcc.LdU(1uL);
			int count = _bcc.ByteCode.Count;
			_bcc.BnZ(0);
			ifst.IfThen.AcceptVisitor(this);
			int num = _bcc.ByteCode.Count - count;
			if (num > 32767 || num < -32768)
			{
				throw new UnsupportedInterpreterCodeException("IF: Distance too long");
			}
			_bcc.ByteCode[count + 1] = (byte)((ushort)num & 0xFFu);
			_bcc.ByteCode[count + 2] = (byte)((ushort)num >> 8);
			if (ifst.IfElse != null)
			{
				throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(ifst.IfElse));
			}
		}

		internal void CreateByAddressInfo(IExpression exp)
		{
			IExpression expression = _codeTranslator.CreatePropertyReadCall(exp);
			if (expression != null)
			{
				expression.AcceptVisitor(this);
				return;
			}
			ByteProgramCreator.ECompileMode variableAccessMode = GetVariableAccessMode();
			IAddressInfo addressInfo = APEnvironmentFacade.Instance.LanguageModelMgr.GetAddressInfo(_guidApplication, exp, _scope);
			if (addressInfo is ILiteralAddressInfo)
			{
				ILiteralAddressInfo literalAddressInfo = addressInfo as ILiteralAddressInfo;
				TypeClass @class = literalAddressInfo.Type.Class;
				if (literalAddressInfo.Type is ICompiledType)
				{
					@class = (literalAddressInfo.Type as ICompiledType).DeRefType.Class;
				}
				LoadLiteralValue(literalAddressInfo.Value, @class);
				return;
			}
			if (addressInfo is IBitAddressInfo && variableAccessMode == ByteProgramCreator.ECompileMode.Write)
			{
				IBitAddressInfo bitAddressInfo = addressInfo as IBitAddressInfo;
				_bpc.Translate(bitAddressInfo.Base, ByteProgramCreator.ECompileMode.Read);
				_bpc.PrepareBitAccess(bitAddressInfo.BitOffset, bitAddressInfo.Base.Size, out var iBitOffset, out var iSizeBytes);
				WriteBit(iSizeBytes, iBitOffset);
				LeavingAddressOnStack(addressInfo.Size);
				return;
			}
			if (variableAccessMode == ByteProgramCreator.ECompileMode.Write && addressInfo is IAbsoluteAddressInfo && (addressInfo as IAbsoluteAddressInfo).BitOffset != byte.MaxValue)
			{
				IAbsoluteAddressInfo absoluteAddressInfo = addressInfo as IAbsoluteAddressInfo;
				_bcc.Rao((byte)absoluteAddressInfo.Area, absoluteAddressInfo.Offset);
				WriteBit(MonH.GetByteSize(absoluteAddressInfo, _bcc.CharBit == 8), absoluteAddressInfo.BitOffset);
				LeavingAddressOnStack(addressInfo.Size);
				return;
			}
			ByteProgramCreator.TrResult trResult = _bpc.Translate(addressInfo, ByteProgramCreator.ECompileMode.Read);
			if (!trResult.Success)
			{
				throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(exp));
			}
			if (trResult.Mode == ByteProgramCreator.EResultMode.Address)
			{
				LeavingAddressOnStack(addressInfo.Size);
			}
			else
			{
				LeavingValueOnStack();
			}
		}

		public void visit(IAddressExpression address)
		{
			CreateByAddressInfo(address);
		}

		public void visit(IVariableExpression variable)
		{
			CreateByAddressInfo(variable);
		}

		public void visit(IThisExpression thisexp)
		{
			CreateByAddressInfo(thisexp);
		}

		public void visit(IIndexAccessExpression indexaccess)
		{
			CreateByAddressInfo(indexaccess);
		}

		public void visit(ICompoAccessExpression compo)
		{
			CreateByAddressInfo(compo);
		}

		public void visit(IDeRefAccessExpression deref)
		{
			deref.Base.AcceptVisitor(this);
			EnsureValueOnStack(TypeClass.Pointer);
			LeavingAddressOnStack(_typeInfo.GetSize(deref.Type.Class, _scope));
		}

		public void visit(IGlobalScopeExpression globexp)
		{
			CreateByAddressInfo(globexp);
		}

		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-95535")]
		[SuppressMessage("Critical Code Smell", "S927:Parameter names should match base declaration and other partial definitions", Justification = "Will be fixed with CDS-95535")]
		public void visit(ICallExpression callOrig)
		{
			ISequenceStatement2 inputAssignSeq;
			ISequenceStatement2 outputAssignSeq;
			ICallExpression callExpression = _codeTranslator.CreateCompleteCallStatement(callOrig, out inputAssignSeq, out outputAssignSeq);
			if (callExpression != null)
			{
				inputAssignSeq?.AcceptVisitor(this);
			}
			else
			{
				callExpression = callOrig;
			}
			ISignature signatureFromCallee = _codeTranslator.GetSignatureFromCallee(callExpression.Callee);
			if (!signatureFromCallee.GetFlag(SignatureFlag.Located))
			{
				throw new UnsupportedInterpreterCodeException(string.Format(ExecutionpointStrings.EPCode_POUNotAvailable, signatureFromCallee.OrgName));
			}
			if (!signatureFromCallee.GetFlag(SignatureFlag.SuperGlobal) && !CodeTranslator.IsPOUCallable(signatureFromCallee, _comcon))
			{
				throw new UnsupportedInterpreterCodeException(string.Format(ExecutionpointStrings.EPCode_POUNotCallable, signatureFromCallee.OrgName));
			}
			int size = signatureFromCallee.Size;
			int num = signatureFromCallee.Outputs.Length;
			int num2 = ((num > 0) ? signatureFromCallee.Outputs[num - 1].DataLocation.Offset : size);
			int num3 = size - num2;
			_fstack.Alloc(num3);
			IAssignmentExpression[] inputAssigns = callExpression.InputAssigns;
			Array.Reverse(inputAssigns);
			IScope3 scope = _comcon.CreateIScope(signatureFromCallee.Id) as IScope3;
			int num4 = num2;
			foreach (IAssignmentExpression assignmentExpression in inputAssigns)
			{
				int offset = assignmentExpression.LValue.DataLocation(scope).Offset;
				int num5 = num4 - offset;
				_fstack.Alloc(num5);
				_bcc.FAddrE();
				_fstack.BeginTemporaryArea();
				IExpression3 expression = assignmentExpression.RValue as IExpression3;
				if ((assignmentExpression.LValue as IVariableExpression).GetVariable(scope).GetFlag(VarFlag.Inout))
				{
					expression = _codeTranslator.VarToInOut(assignmentExpression.RValue) as IExpression3;
				}
				ReadVariable();
				expression.AcceptVisitor(this);
				VariableAccessDone();
				int num6 = EnsureAddressOnStack(expression.Type.Class);
				if (num6 > num5)
				{
					num6 = num5;
				}
				_bcc.Cpy(num6);
				_fstack.EndTemporaryArea();
				num4 = offset;
			}
			IDataLocation fPDataLocation = signatureFromCallee.FPDataLocation;
			if (fPDataLocation == null)
			{
				throw new UnsupportedInterpreterCodeException(string.Format(ExecutionpointStrings.BPCodeFuncUnavailable, callExpression.Callee));
			}
			_bcc.Rao((byte)fPDataLocation.Area, fPDataLocation.Offset);
			_bcc.Der((byte)_bcc.PointerSizeBytes);
			ByteCmdCreator.ECallMode eCallMode = ByteCmdCreator.ECallMode.IEC;
			if (signatureFromCallee.GetFlag(SignatureFlag.External))
			{
				eCallMode = ByteCmdCreator.ECallMode.C;
			}
			_bcc.Call((ushort)num2, (ushort)num3, eCallMode);
			if (signatureFromCallee.POUType == Operator.FunctionBlock)
			{
				outputAssignSeq?.AcceptVisitor(this);
			}
			else if (num > 0)
			{
				_bcc.FAddrE();
				LeavingAddressOnStack(num3);
			}
		}

		public void visit(ILiteralExpression literal)
		{
			LoadLiteralValue(literal.LiteralValue, literal.Type.Class);
		}

		private void LoadLiteralValue(ILiteralValue lv, TypeClass tc)
		{
			switch (lv.KindOf)
			{
			default:
				return;
			case KindOfLiteral.Bool:
				_bcc.Ld8((byte)(lv.Bool ? 1u : 0u));
				break;
			case KindOfLiteral.UnsignedInteger:
				LdU_T(lv.UnsignedLong, tc);
				break;
			case KindOfLiteral.SignedInteger:
				LdS_T(lv.SignedLong, tc);
				break;
			case KindOfLiteral.Float:
				if (tc == TypeClass.Real)
				{
					_bcc.LdR((float)lv.Float);
				}
				else
				{
					_bcc.LdLR(lv.Float);
				}
				break;
			case KindOfLiteral.String:
			{
				int valueSize = new InterpreterStringLiteral(_bcc).StoreStringLiteral(lv, tc);
				LeavingAddressOnStack(valueSize);
				return;
			}
			case KindOfLiteral.None:
				throw new UnsupportedInterpreterCodeException("Invalid literal (KindOfLiteral.None)");
			}
			LeavingValueOnStack();
		}

		private void LdU_T(ulong ulImmediate, TypeClass tc)
		{
			bool flag = false;
			int size = _typeInfo.GetSize(tc, _scope);
			switch (size)
			{
			case 1:
				_bcc.Ld8((byte)ulImmediate);
				break;
			case 2:
				_bcc.Ld16((ushort)ulImmediate);
				break;
			case 4:
				_bcc.Ld32((uint)ulImmediate);
				break;
			case 8:
				_bcc.Ld64(ulImmediate);
				flag = true;
				break;
			default:
				throw new UnsupportedInterpreterCodeException(string.Format(ExecutionpointStrings.UnsupportedSizeOfVariable, size));
			}
			if (!flag && _bcc.PointerSizeBytes != 8 && TH.IsInteger64(tc))
			{
				_bcc.C32To64();
			}
		}

		public void LdS_T(long lImmediate, TypeClass tc)
		{
			bool flag = false;
			int size = _typeInfo.GetSize(tc, _scope);
			switch (size)
			{
			case 1:
				_bcc.Ld8((byte)lImmediate);
				_bcc.Es(8);
				break;
			case 2:
				_bcc.Ld16((ushort)lImmediate);
				_bcc.Es(16);
				break;
			case 4:
				_bcc.Ld32((uint)lImmediate);
				_bcc.Es(32);
				break;
			case 8:
				_bcc.Ld64((ulong)lImmediate);
				flag = true;
				break;
			default:
				throw new UnsupportedInterpreterCodeException(string.Format(ExecutionpointStrings.UnsupportedSizeOfVariable, size));
			}
			if (!flag && _bcc.PointerSizeBytes != 8 && TH.IsInteger64(tc))
			{
				_bcc.C32To64();
				_bcc.Es64(32);
			}
		}

		public void visit(IEmptyStatement empty)
		{
		}

		public void visit(ICommentStatement comment)
		{
		}

		public void visit(IReturnStatement returnst)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(returnst));
		}

		public void visit(IJumpStatement gotost)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(gotost));
		}

		public void visit(ILabelStatement label)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(label));
		}

		public void visit(IPragmaStatement pragma)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(pragma));
		}

		public void visit(IHasConstantValueExpression hasvalue)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(hasvalue));
		}

		public void visit(IWhileStatement whilst)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(whilst));
		}

		public void visit(IRepeatStatement repeat)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(repeat));
		}

		public void visit(IForStatement forloop)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(forloop));
		}

		public void visit(IExitStatement exit)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(exit));
		}

		public void visit(IContinueStatement cont)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(cont));
		}

		public void visit(IBaseExpression baseexp)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(baseexp));
		}

		public void visit(ICaseRangeExpression caserange)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(caserange));
		}

		public void visit(ICaseLabelStatement caselabel)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(caselabel));
		}

		public void visit(ICaseStatement casest)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(casest));
		}

		public void visit(IBreakPointStatement bpstate)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(bpstate));
		}

		public void visit(IDefineReference defref)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(defref));
		}

		public void visit(IVariableReference varref)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(varref));
		}

		public void visit(ITypeReference typeref)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(typeref));
		}

		public void visit(IPouReference pouref)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(pouref));
		}

		public void visit(IDefinedExpression defexp)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(defexp));
		}

		public void visit(IPragmaOperatorExpression popexp)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(popexp));
		}

		public void visit(IPragmaIfStatement pifst)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(pifst));
		}

		public void visit(IDefineStatement defstate)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(defstate));
		}

		public void visit(IHasTypeExpression hastype)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(hastype));
		}

		public void visit(IHasAttributeExpression hasattribute)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(hasattribute));
		}

		public void visit(IHasValueExpression hasvalue)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(hasvalue));
		}

		public void visit(IPragmaAssertion assertion)
		{
			throw new UnsupportedInterpreterCodeException(ErrorMessageFromExprement(assertion));
		}

		[SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "Will be fixed with CDS-95535")]
		private string ErrorMessageFromExprement(IExprement exp)
		{
			string text = exp.ToString().Split(Environment.NewLine.ToCharArray()).FirstOrDefault();
			return text.Substring(0, Math.Min(50, text.Length)) + "...";
		}
	}
}
