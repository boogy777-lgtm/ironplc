using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities.MonitoringByteCode
{
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes", Justification = "Will be fixed with CDS-95535")]
	internal class ByteProgramCreator
	{
		public enum ECompileMode
		{
			Read,
			Write,
			Force
		}

		public enum EResultMode
		{
			Value,
			Address
		}

		public enum EValueType
		{
			Signed,
			Raw
		}

		public struct TrResult
		{
			private static readonly TrResult s_riAddr = new TrResult(bSuccess: true, EResultMode.Address, "");

			private static readonly TrResult s_riValue = new TrResult(bSuccess: true, EResultMode.Value, "");

			private readonly EResultMode m_mode;

			private readonly bool m_bSuccess;

			private readonly string m_stMsg;

			public bool Success => m_bSuccess;

			public EResultMode Mode => m_mode;

			public string ErrorMsg => m_stMsg;

			private TrResult(bool bSuccess, EResultMode mode, string stMsg)
			{
				m_bSuccess = bSuccess;
				m_mode = mode;
				m_stMsg = stMsg;
			}

			public static TrResult Error(string stMsg)
			{
				return new TrResult(bSuccess: false, EResultMode.Address, stMsg);
			}

			public static TrResult Address()
			{
				return s_riAddr;
			}

			public static TrResult Value()
			{
				return s_riValue;
			}
		}

		private readonly ByteCmdCreator m_bc;

		public ByteCmdCreator CmdCreator => m_bc;

		public ByteProgramCreator(ByteCmdCreator bc)
		{
			m_bc = bc;
		}

		public bool CompileRead(IAddressInfo ai, out string stErrorMsg)
		{
			return Compile(ai, 0, ECompileMode.Read, out stErrorMsg);
		}

		public bool CompileWrite(IAddressInfo ai, out string stErrorMsg)
		{
			return Compile(ai, m_bc.PointerSizeBytes + 8, ECompileMode.Write, out stErrorMsg);
		}

		public bool CompileForce(IAddressInfo ai, out string stErrorMsg)
		{
			return Compile(ai, m_bc.PointerSizeBytes + 8, ECompileMode.Force, out stErrorMsg);
		}

		private bool Compile(IAddressInfo ai, int iSpaceForInputsBytes, ECompileMode cm, out string stErrorMsg)
		{
			if (iSpaceForInputsBytes != 0)
			{
				m_bc.FAlloc(iSpaceForInputsBytes);
			}
			TrResult trResult = Translate(ai, cm);
			stErrorMsg = trResult.ErrorMsg;
			bool success = trResult.Success;
			if (success && trResult.Mode == EResultMode.Value)
			{
				TrResult trResult2 = ValueToAddress(GetAddressInfoType(ai).Size(null));
				success = trResult2.Success;
				stErrorMsg = trResult2.ErrorMsg;
			}
			if (success)
			{
				EndCompile();
			}
			return success;
		}

		public void EndCompile()
		{
			while (m_bc.ByteCode.Count % (m_bc.CharBit / 8) != 0)
			{
				m_bc.Halt();
			}
		}

		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-95535")]
		public TrResult Translate(IAddressInfo ai, ECompileMode mode)
		{
			if (ai == null)
			{
				return TrResult.Error("Translate: address info is null");
			}
			if (ai is IAbsoluteAddressInfo aiAbs)
			{
				return TrAbsoluteAddressInfo(aiAbs, mode);
			}
			if (ai is IStackRelativeAddressInfo aiSRel)
			{
				if (ai is IStackRelativeAddressInfo3 aiSRel2 && (ai as IStackRelativeAddressInfo3).BasePointer != 0L)
				{
					return TrStackAbsoluteAddressInfo(aiSRel2, mode);
				}
				return TrStackRelativeAddressInfo(aiSRel, mode);
			}
			if (ai is ICompoAddressInfo aiCompo)
			{
				return TrCompoAddressInfo(aiCompo, mode);
			}
			if (ai is ISignedConstantAddressInfo aiConst)
			{
				return TrSignedConstantAddressInfo(aiConst, mode);
			}
			if (ai is IDeRefAccessInfo2 aiDeref)
			{
				return TrDeRefAccessInfo(aiDeref, mode);
			}
			if (ai is IBitAddressInfo aiBit)
			{
				return TrBitAddressInfo(aiBit, mode);
			}
			if (ai is IArrayAccessAddressInfo2 aiArr)
			{
				return TrArrayAccessAddressInfo(aiArr, mode);
			}
			if (ai is IOperatorAddressInfo aiOp)
			{
				return TrOperatorAddressInfo(aiOp, mode);
			}
			if (ai is IPropertyAddressInfo aiProp)
			{
				return TrPropertyAddressInfo(aiProp, mode);
			}
			if (ai is IPropertyAddressInfoExtended aiPropExt)
			{
				return TrPropertyAddressInfoExtended(aiPropExt, mode);
			}
			if (ai is IFunctionAddressInfo aiFun)
			{
				return TrFunctionAddressInfo(aiFun, mode);
			}
			if (ai is IParameterAddressInfo aiParameter)
			{
				return TrParameterAddressInfo(aiParameter, mode);
			}
			if (ai is IConversionAddressInfo cai)
			{
				return TrConversionAddressInfo(cai, mode);
			}
			if (ai is ILiteralAddressInfo lai)
			{
				return TrLiteralAddressInfo(lai, mode);
			}
			if (ai is IAddressAddressInfo aiAddr)
			{
				return TrAddressAddressInfo(aiAddr);
			}
			return TrResult.Error($"Unknown type of address info: '{ai.GetType().FullName}'");
		}

		private TrResult TrAbsoluteAddressInfo(IAbsoluteAddressInfo aiAbs, ECompileMode mode)
		{
			if (aiAbs.BitOffset != byte.MaxValue && aiAbs.BitOffset > 7)
			{
				return TrResult.Error($"Invalid bit offset {aiAbs.BitOffset} for absolute address info");
			}
			m_bc.Rao((byte)aiAbs.Area, aiAbs.Offset);
			if (aiAbs.BitOffset == byte.MaxValue)
			{
				HandleLValueWriteAndForce(mode, aiAbs);
				if (mode == ECompileMode.Read)
				{
					return TrResult.Address();
				}
				WriteValue();
				return TrResult.Address();
			}
			HandleLValueWriteAndForceBit(mode, aiAbs.BitOffset, GetByteSize(aiAbs), out var iBitOffsetNew, out var iByteSizeNew);
			if (mode == ECompileMode.Read)
			{
				ReadBitOffset(iBitOffsetNew, iByteSizeNew);
				return TrResult.Value();
			}
			SetBit(iByteSizeNew, (byte)iBitOffsetNew);
			return TrResult.Address();
		}

		private int GetByteSize(IAddressInfo ai)
		{
			return MonH.GetByteSize(ai, m_bc.CharBit == 8);
		}

		private void ReadBitOffset(int iBitOffset, int iByteSize)
		{
			m_bc.Der((byte)iByteSize);
			m_bc.Ld8((byte)iBitOffset);
			m_bc.Bit();
		}

		private void HandleLValueWriteAndForce(ECompileMode cm, IAbsoluteAddressInfo aiAbs)
		{
			if (cm == ECompileMode.Write || cm == ECompileMode.Force)
			{
				PublishAddr(EExpressionType.LValue);
			}
			if (cm == ECompileMode.Force)
			{
				LoadMode();
				m_bc.Ld8(2);
				m_bc.Sub();
				m_bc.BnZ(4);
				m_bc.Halt();
			}
		}

		private void HandleLValueWriteAndForceBit(ECompileMode cm, int iBitOffset, int iByteSize, out int iBitOffsetNew, out int iByteSizeNew)
		{
			iBitOffsetNew = iBitOffset;
			iByteSizeNew = iByteSize;
			switch (cm)
			{
			case ECompileMode.Write:
				PublishAddr(EExpressionType.LValue);
				break;
			case ECompileMode.Force:
				PrepareBitAccess(iBitOffset, iByteSize, out iBitOffsetNew, out iByteSizeNew);
				LoadMode();
				m_bc.BnZ(8);
				m_bc.FLd((byte)m_bc.PointerSizeBytes);
				m_bc.Ld8((byte)iByteSizeNew);
				m_bc.FAddrE();
				m_bc.Der((byte)m_bc.PointerSizeBytes);
				m_bc.Ld8((byte)iBitOffsetNew);
				m_bc.Ld8(1);
				m_bc.Halt();
				LoadMode();
				m_bc.Ld8(2);
				m_bc.Sub();
				m_bc.BnZ(12);
				ReadBitOffset(iBitOffsetNew, iByteSizeNew);
				ValueToAddress(1);
				m_bc.Halt();
				break;
			}
			if (cm != ECompileMode.Force)
			{
				PrepareBitAccess(iBitOffset, iByteSize, out iBitOffsetNew, out iByteSizeNew);
			}
		}

		private TrResult TrStackRelativeAddressInfo(IStackRelativeAddressInfo aiSRel, ECompileMode mode)
		{
			if (mode == ECompileMode.Force)
			{
				return TrResult.Error("Cannot force stack relative adresses");
			}
			m_bc.Rao((byte)aiSRel.AreaCode, aiSRel.OffsetCode);
			m_bc.LdS(m_bc.ByteToCharOffset(aiSRel.Offset));
			m_bc.LdU((uint)m_bc.ByteToCharSize(aiSRel.SizeCode));
			bool bRelativeToSP = aiSRel is IStackRelativeAddressInfo2 stackRelativeAddressInfo && stackRelativeAddressInfo.StackRelative;
			m_bc.Rst(bRelativeToSP);
			if (mode == ECompileMode.Write)
			{
				PublishAddr(EExpressionType.LValue);
				WriteValue();
			}
			return TrResult.Address();
		}

		private TrResult TrStackAbsoluteAddressInfo(IStackRelativeAddressInfo3 aiSRel, ECompileMode mode)
		{
			if (mode == ECompileMode.Force)
			{
				return TrResult.Error("Cannot force stack relative adresses");
			}
			m_bc.LdU(aiSRel.BasePointer);
			m_bc.LdS(m_bc.ByteToCharOffset(aiSRel.Offset));
			m_bc.Add();
			if (mode == ECompileMode.Write)
			{
				PublishAddr(EExpressionType.LValue);
				WriteValue();
			}
			return TrResult.Address();
		}

		private TrResult TrCompoAddressInfo(ICompoAddressInfo aiCompo, ECompileMode mode)
		{
			if (mode == ECompileMode.Force)
			{
				return TrResult.Error("Cannot force compo address infos");
			}
			TrResult result = Translate(aiCompo.Base, ECompileMode.Read);
			if (!result.Success)
			{
				return result;
			}
			if (aiCompo.Offset != 0)
			{
				m_bc.Dup();
				m_bc.BnZ(5);
				m_bc.Der(1);
				m_bc.LdS(m_bc.ByteToCharOffset(aiCompo.Offset));
				m_bc.Add();
			}
			if (mode == ECompileMode.Write)
			{
				PublishAddr(EExpressionType.LValue);
				WriteValue();
			}
			return TrResult.Address();
		}

		private TrResult TrSignedConstantAddressInfo(ISignedConstantAddressInfo aiConst, ECompileMode mode)
		{
			if (mode != 0)
			{
				return TrResult.Error("Constants can only be read");
			}
			m_bc.LdS((int)aiConst.Value);
			return TrResult.Value();
		}

		private TrResult TrDeRefAccessInfo(IDeRefAccessInfo2 aiDeref, ECompileMode mode)
		{
			if (mode == ECompileMode.Force)
			{
				return TrResult.Error("Cannot force deref address infos");
			}
			TrResult result = Translate(aiDeref.Base, ECompileMode.Read);
			if (!result.Success)
			{
				return result;
			}
			if (result.Mode == EResultMode.Value)
			{
				TrResult result2 = ValueToAddress(aiDeref.Base.Size);
				if (!result2.Success)
				{
					return result2;
				}
			}
			m_bc.Der((byte)m_bc.PointerSizeBytes);
			if (aiDeref.Offset != 0)
			{
				m_bc.Dup();
				m_bc.BnZ(5);
				m_bc.Der(1);
				m_bc.LdS(m_bc.ByteToCharOffset(aiDeref.Offset));
				m_bc.Add();
			}
			if (mode == ECompileMode.Write)
			{
				PublishAddr(EExpressionType.LValue);
				WriteValue();
			}
			return TrResult.Address();
		}

		private TrResult TrBitAddressInfo(IBitAddressInfo aiBit, ECompileMode mode)
		{
			if (mode == ECompileMode.Force && (!(aiBit.Base is IAbsoluteAddressInfo) || CmdCreator.RuntimeIdentification < Constants.RTS_VERSION_3550))
			{
				return TrResult.Error(Strings.ErrForceBitOfNonAbsoluteAddressInfo);
			}
			TrResult result = Translate(aiBit.Base, ECompileMode.Read);
			if (!result.Success)
			{
				return result;
			}
			int iByteSize = GetByteSize(aiBit.Base);
			if (aiBit is IBitAddressInfo2)
			{
				iByteSize = (aiBit as IBitAddressInfo2).AccessedElementSize;
			}
			HandleLValueWriteAndForceBit(mode, aiBit.BitOffset, iByteSize, out var iBitOffsetNew, out var iByteSizeNew);
			if (mode == ECompileMode.Read)
			{
				ReadBitOffset(iBitOffsetNew, iByteSizeNew);
				return TrResult.Value();
			}
			SetBit(iByteSizeNew, (byte)iBitOffsetNew);
			return TrResult.Address();
		}

		internal void PrepareBitAccess(int iBitOffsetOrig, int iSizeBytesOrig, out int iBitOffset, out int iSizeBytes)
		{
			int num = (iSizeBytesOrig + (m_bc.PointerSizeBytes - 1)) / m_bc.PointerSizeBytes;
			int num2 = iBitOffsetOrig / (m_bc.PointerSizeBytes * 8);
			int num3 = ((m_bc.ByteOrder == ByteOrder.Motorola) ? (num - 1 - num2) : num2);
			if (num3 != 0)
			{
				m_bc.LdU((uint)m_bc.ByteToCharSize(num3 * m_bc.PointerSizeBytes));
				m_bc.Add();
			}
			iBitOffset = iBitOffsetOrig % (m_bc.PointerSizeBytes * 8);
			iSizeBytes = Math.Min(iSizeBytesOrig, m_bc.PointerSizeBytes);
		}

		private TrResult TrArrayAccessAddressInfo(IArrayAccessAddressInfo2 aiArr, ECompileMode mode)
		{
			if (mode == ECompileMode.Force)
			{
				return TrResult.Error("Cannot force array access infos");
			}
			int num = aiArr.Indexes.Length;
			int[] array = new int[num];
			array[num - 1] = aiArr.BaseSize;
			for (int num2 = num - 2; num2 >= 0; num2--)
			{
				IArrayBounds arrayBounds = aiArr.Bounds[num2 + 1];
				array[num2] = array[num2 + 1] * (int)(arrayBounds.UpperBound - arrayBounds.LowerBound + 1);
			}
			TrResult result = Translate(aiArr.Base, ECompileMode.Read);
			if (!result.Success)
			{
				return result;
			}
			for (int i = 0; i != num; i++)
			{
				IAddressInfo addressInfo = aiArr.Indexes[i];
				TrResult result2 = Translate(addressInfo, ECompileMode.Read);
				if (!result2.Success)
				{
					return result2;
				}
				if (result2.Mode == EResultMode.Address)
				{
					TrResult result3 = AddressToValue(addressInfo.Size, EValueType.Signed);
					if (!result3.Success)
					{
						return result3;
					}
				}
				m_bc.LdS((int)aiArr.Bounds[i].LowerBound);
				m_bc.LdS((int)aiArr.Bounds[i].UpperBound);
				m_bc.LdU((uint)m_bc.ByteToCharSize(array[i]));
				m_bc.Arr();
			}
			if (mode == ECompileMode.Write)
			{
				PublishAddr(EExpressionType.LValue);
				WriteValue();
			}
			return TrResult.Address();
		}

		private TrResult TrOperatorAddressInfo(IOperatorAddressInfo aiOp, ECompileMode mode)
		{
			if (mode != 0)
			{
				return TrResult.Error("An operator address info can only be read");
			}
			if (IsLogicOperator(aiOp.Operator) && GetAddressInfoType(aiOp).Class == TypeClass.Bool)
			{
				return TrLogicOperator(aiOp);
			}
			if (IsArithmeticOperator(aiOp.Operator))
			{
				return TrArithmeticOperator(aiOp);
			}
			return TrResult.Error(string.Format(Strings.Err_UnsupportedOperator_1, aiOp.Operator));
		}

		public static bool IsArithmeticOperator(Operator op)
		{
			return OperatorMapping.GetOperatorCode(op) != OperatorCodes.None;
		}

		public static bool IsLogicOperator(Operator op)
		{
			switch (op)
			{
			case Operator.And:
			case Operator.Or:
			case Operator.Not:
			case Operator.Less:
			case Operator.Greater:
			case Operator.LessEqual:
			case Operator.GreaterEqual:
			case Operator.Equal:
			case Operator.NotEqual:
				return true;
			default:
				return false;
			}
		}

		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-95535")]
		private TrResult TrArithmeticOperator(IOperatorAddressInfo aiOp)
		{
			OperatorCodes operatorCode = OperatorMapping.GetOperatorCode(aiOp.Operator);
			if (operatorCode == OperatorCodes.CastUnsignedToSigned)
			{
				IAddressInfo addressInfo = aiOp.Operands[0];
				TrResult result = Translate(addressInfo, ECompileMode.Read);
				if (!result.Success)
				{
					return result;
				}
				if (result.Mode == EResultMode.Address)
				{
					TrResult result2 = AddressToValue(addressInfo.Size, EValueType.Raw);
					if (!result2.Success)
					{
						return result2;
					}
				}
			}
			else
			{
				IType addressInfoType = GetAddressInfoType(aiOp.Operands[0]);
				switch (operatorCode)
				{
				case OperatorCodes.Addition:
					if (!TH.IsInteger(addressInfoType.Class) || (!TH.IsInteger32(addressInfoType.Class) && m_bc.PointerSizeBytes != 8))
					{
						return TrResult.Error(Strings.Err_AdditionOnlyUpToPointerSize);
					}
					break;
				case OperatorCodes.Subtraction:
					if (m_bc.RuntimeIdentification < new Version(3, 5, 3, 0))
					{
						if (!TH.IsInteger(addressInfoType.Class) || (!TH.IsInteger32(addressInfoType.Class) && m_bc.PointerSizeBytes != 8))
						{
							return TrResult.Error(Strings.Err_SubtractionOnlyUpToPointerSize);
						}
					}
					else if (!TH.IsInteger(addressInfoType.Class) && !TH.IsFloat(addressInfoType.Class))
					{
						return TrResult.Error(string.Format(Strings.Err_InvSubtractionType_1, addressInfoType.ToString()));
					}
					break;
				case OperatorCodes.Multiplication:
				case OperatorCodes.Division:
				case OperatorCodes.Modulo:
					if (!TH.IsSignedInteger(addressInfoType.Class) || (!TH.IsInteger32(addressInfoType.Class) && m_bc.PointerSizeBytes != 8))
					{
						return TrResult.Error(string.Format(Strings.Err_OperatorOnlySignedUpToPointerSize_1, operatorCode));
					}
					break;
				default:
					return TrResult.Error(string.Format(Strings.Err_UnsupportedOperator_1, operatorCode));
				}
				IAddressInfo[] operands = aiOp.Operands;
				foreach (IAddressInfo addressInfo2 in operands)
				{
					TrResult result3 = Translate(addressInfo2, ECompileMode.Read);
					if (!result3.Success)
					{
						return result3;
					}
					if (result3.Mode == EResultMode.Address)
					{
						TrResult result4 = AddressToValue(addressInfo2.Size, GetValueType(addressInfo2));
						if (!result4.Success)
						{
							return result4;
						}
					}
				}
				switch (operatorCode)
				{
				case OperatorCodes.Addition:
					m_bc.Add();
					break;
				case OperatorCodes.Subtraction:
					CreateSubtractionCmd(addressInfoType.Class);
					break;
				case OperatorCodes.Multiplication:
					m_bc.MulS();
					break;
				case OperatorCodes.Division:
					m_bc.DivS();
					break;
				case OperatorCodes.Modulo:
					m_bc.ModS();
					break;
				}
			}
			return TrResult.Value();
		}

		private void CreateSubtractionCmd(TypeClass tc)
		{
			if (TH.IsInteger32(tc) || (TH.IsInteger(tc) && m_bc.PointerSizeBytes == 8))
			{
				m_bc.Sub();
			}
			else if (TH.IsInteger64(tc))
			{
				m_bc.Sub64();
			}
			else if (TH.IsReal(tc))
			{
				m_bc.SubR();
			}
			else if (TH.IsLReal(tc))
			{
				m_bc.SubLR();
			}
		}

		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-95535")]
		private TrResult TrLogicOperator(IOperatorAddressInfo aiOp)
		{
			if (m_bc.RuntimeIdentification < new Version(3, 5, 3, 0))
			{
				return TrResult.Error(Strings.Err_LogicOperatorsRequire3530);
			}
			IType type = ((IAddressInfo3)aiOp).Type;
			switch (aiOp.Operator)
			{
			case Operator.Greater:
				return TrLogicOperator(OperatorAI(Operator.Not, type, OperatorAI(Operator.LessEqual, type, aiOp.Operands)));
			case Operator.Less:
				return TrLogicOperator(OperatorAI(Operator.Not, type, OperatorAI(Operator.LessEqual, type, Fun.ToArr<IAddressInfo>(Fun.Reversed<IAddressInfo>((IEnumerable<IAddressInfo>)aiOp.Operands)))));
			case Operator.GreaterEqual:
				return TrLogicOperator(OperatorAI(Operator.LessEqual, type, Fun.ToArr<IAddressInfo>(Fun.Reversed<IAddressInfo>((IEnumerable<IAddressInfo>)aiOp.Operands))));
			case Operator.Equal:
				return TrLogicOperator(OperatorAI(Operator.Not, type, OperatorAI(Operator.NotEqual, type, aiOp.Operands)));
			case Operator.Not:
				m_bc.LdU(1uL);
				break;
			default:
				return TrResult.Error("Internal error");
			case Operator.And:
			case Operator.Or:
			case Operator.LessEqual:
			case Operator.NotEqual:
				break;
			}
			IAddressInfo[] operands = aiOp.Operands;
			foreach (IAddressInfo addressInfo in operands)
			{
				TrResult result = Translate(addressInfo, ECompileMode.Read);
				if (!result.Success)
				{
					return result;
				}
				if (result.Mode == EResultMode.Address)
				{
					TrResult result2 = AddressToValue(addressInfo.Size, GetValueType(addressInfo));
					if (!result2.Success)
					{
						return result2;
					}
				}
			}
			switch (aiOp.Operator)
			{
			case Operator.LessEqual:
			case Operator.NotEqual:
			{
				ICompiledType addressInfoType = GetAddressInfoType(aiOp.Operands[0]);
				ICompiledType addressInfoType2 = GetAddressInfoType(aiOp.Operands[1]);
				APEnvironmentFacade.Instance.LanguageModelMgr.TypeInfo.IsEquivalent(addressInfoType.Class, addressInfoType2.Class);
				CreateSubtractionCmd(addressInfoType.Class);
				if (addressInfoType.Size(null) <= m_bc.PointerSizeBytes)
				{
					m_bc.Pop();
				}
				else
				{
					m_bc.Pop64();
				}
				if (aiOp.Operator == Operator.LessEqual)
				{
					if (TH.IsSignedInteger(addressInfoType.Class) || TH.IsFloat(addressInfoType.Class))
					{
						m_bc.SetLE();
					}
					else
					{
						m_bc.SetBE();
					}
				}
				else
				{
					m_bc.SetNZ();
				}
				break;
			}
			case Operator.And:
				m_bc.MulS();
				break;
			case Operator.Or:
				m_bc.Add();
				m_bc.Pop();
				m_bc.SetNZ();
				break;
			case Operator.Not:
				m_bc.Sub();
				break;
			default:
				return TrResult.Error("Internal error");
			}
			return TrResult.Value();
		}

		private static IOperatorAddressInfo OperatorAI(Operator op, IType t, params IAddressInfo[] operands)
		{
			return APEnvironmentFacade.Instance.AddressInfoFactory.CreateOperator(operands, op, t);
		}

		private static ICompiledType GetAddressInfoType(IAddressInfo ai)
		{
			ICompiledType compiledType = (ICompiledType)((IAddressInfo3)ai).Type;
			if (compiledType != null && compiledType.Class == TypeClass.Enum)
			{
				return compiledType.BaseType;
			}
			return compiledType;
		}

		private static EValueType GetValueType(IAddressInfo aiOperand)
		{
			IType addressInfoType = GetAddressInfoType(aiOperand);
			if (addressInfoType == null)
			{
				return EValueType.Raw;
			}
			if (TH.IsSignedInteger(addressInfoType.Class))
			{
				return EValueType.Signed;
			}
			return EValueType.Raw;
		}

		private int PropPaddedReturnSize(IAddressInfo aiProp)
		{
			int size = aiProp.Size;
			if (aiProp is IPropertyAddressInfoAdditional propertyAddressInfoAdditional && propertyAddressInfoAdditional.IsReferenceType)
			{
				return m_bc.PointerSizeBytes;
			}
			return (size + 7) / 8 * 8;
		}

		private TrResult TrPropertyRead(IPropertyAddressInfo aiProp, ECompileMode mode)
		{
			if (aiProp.OffsetGet < 0)
			{
				return TrResult.Error(Strings.Err_ReadPropNoGet);
			}
			int num = PropPaddedReturnSize(aiProp);
			m_bc.FAlloc(num);
			bool flag = aiProp is IPropertyAddressInfoAdditional propertyAddressInfoAdditional && propertyAddressInfoAdditional.IsReferenceType;
			int num2;
			if (aiProp is IPropertyAdressInfoWithOffset && (aiProp as IPropertyAdressInfoWithOffset).OffsetValueGetter >= 0)
			{
				num2 = (aiProp as IPropertyAdressInfoWithOffset).OffsetValueGetter;
				if (m_bc.PointerSizeBytes == 4)
				{
					m_bc.FAlloc(4);
				}
			}
			else if (!flag && aiProp.Size == 8 && m_bc.PointerSizeBytes != 8)
			{
				num2 = 8;
				m_bc.FAlloc(4);
			}
			else
			{
				num2 = m_bc.PointerSizeBytes;
			}
			if (aiProp.AreaInstance == -1)
			{
				m_bc.LdU(0uL);
			}
			else
			{
				m_bc.Rao((byte)aiProp.AreaInstance, aiProp.OffsetInstance);
			}
			m_bc.FLd((byte)m_bc.PointerSizeBytes);
			m_bc.Rao((byte)aiProp.AreaProperty, aiProp.OffsetGet);
			m_bc.Der((byte)m_bc.PointerSizeBytes);
			m_bc.Call((ushort)num2, (ushort)num, ByteCmdCreator.ECallMode.IEC);
			m_bc.FAddrE();
			return TrResult.Address();
		}

		private TrResult TrPropertyWrite(IPropertyAddressInfo aiProp, ECompileMode mode)
		{
			if (aiProp.OffsetSet < 0)
			{
				return TrResult.Error(Strings.Err_WritePropNoSet);
			}
			int num = 0;
			PublishAddr(EExpressionType.PropertyCall);
			m_bc.FAlloc(aiProp.Size);
			m_bc.FAddrE();
			WriteValue();
			int size = aiProp.Size;
			IPropertyAdressInfoWithOffset propertyAdressInfoWithOffset = aiProp as IPropertyAdressInfoWithOffset;
			if (propertyAdressInfoWithOffset != null && propertyAdressInfoWithOffset.OffsetValueSetter == 4)
			{
				size += 4;
			}
			else if (propertyAdressInfoWithOffset != null && propertyAdressInfoWithOffset.OffsetValueSetter == 8)
			{
				size += 8;
				if (m_bc.PointerSizeBytes == 4)
				{
					m_bc.FAlloc(4);
				}
			}
			else if (aiProp.Size == 8 && m_bc.PointerSizeBytes != 8)
			{
				size += m_bc.PointerSizeBytes + 4;
				m_bc.FAlloc(4);
			}
			else
			{
				size += m_bc.PointerSizeBytes;
			}
			if (aiProp.AreaInstance == -1)
			{
				m_bc.LdU(0uL);
			}
			else
			{
				m_bc.Rao((byte)aiProp.AreaInstance, aiProp.OffsetInstance);
			}
			m_bc.FLd((byte)m_bc.PointerSizeBytes);
			m_bc.Rao((byte)aiProp.AreaProperty, aiProp.OffsetSet);
			m_bc.Der((byte)m_bc.PointerSizeBytes);
			m_bc.Call((ushort)size, (ushort)num, ByteCmdCreator.ECallMode.IEC);
			return TrResult.Address();
		}

		private TrResult TrPropertyForce(IPropertyAddressInfo aiProp, ECompileMode mode)
		{
			if (aiProp.OffsetSet < 0)
			{
				return TrResult.Error(Strings.Err_ForcePropNoSet);
			}
			int num = 0;
			int val = PropPaddedReturnSize(aiProp);
			m_bc.FAlloc(Math.Max(val, aiProp.Size));
			m_bc.FAlloc(aiProp.Size);
			m_bc.FAddrE();
			WriteValue();
			int size = aiProp.Size;
			IPropertyAdressInfoWithOffset propertyAdressInfoWithOffset = aiProp as IPropertyAdressInfoWithOffset;
			if (propertyAdressInfoWithOffset != null && propertyAdressInfoWithOffset.OffsetValueSetter == 4)
			{
				size += 4;
			}
			else if (propertyAdressInfoWithOffset != null && propertyAdressInfoWithOffset.OffsetValueSetter == 8)
			{
				size += 8;
				if (m_bc.PointerSizeBytes == 4)
				{
					m_bc.FAlloc(4);
				}
			}
			else if (aiProp.Size == 8 && m_bc.PointerSizeBytes != 8)
			{
				size += m_bc.PointerSizeBytes + 4;
				m_bc.FAlloc(4);
			}
			else
			{
				size += m_bc.PointerSizeBytes;
			}
			m_bc.Rao((byte)aiProp.AreaProperty, aiProp.OffsetGet);
			m_bc.Rao((byte)aiProp.AreaProperty, aiProp.OffsetSet);
			if (aiProp.AreaInstance == -1)
			{
				m_bc.LdU(0uL);
			}
			else
			{
				m_bc.Rao((byte)aiProp.AreaInstance, aiProp.OffsetInstance);
			}
			m_bc.Dup();
			m_bc.FLd((byte)m_bc.PointerSizeBytes);
			PublishAddr(EExpressionType.PropertyCall);
			m_bc.Pop();
			LoadMode();
			m_bc.Ld8(2);
			m_bc.Sub();
			m_bc.BnZ(14);
			m_bc.Pop();
			m_bc.Der((byte)m_bc.PointerSizeBytes);
			m_bc.Call((ushort)(size - aiProp.Size), (ushort)aiProp.Size, ByteCmdCreator.ECallMode.IEC);
			m_bc.FAddrE();
			m_bc.Halt();
			m_bc.Der((byte)m_bc.PointerSizeBytes);
			m_bc.Call((ushort)size, (ushort)num, ByteCmdCreator.ECallMode.IEC);
			return TrResult.Address();
		}

		private TrResult TrPropertyAddressInfo(IPropertyAddressInfo aiProp, ECompileMode mode)
		{
			switch (mode)
			{
			default:
				return TrPropertyRead(aiProp, mode);
			case ECompileMode.Write:
				return TrPropertyWrite(aiProp, mode);
			case ECompileMode.Force:
				return TrPropertyForce(aiProp, mode);
			}
		}

		private TrResult TrPropertyExtRead(IPropertyAddressInfoExtended aiPropExt, ECompileMode mode)
		{
			if (aiPropExt.VFTableOffsetGet < 0)
			{
				return TrResult.Error(Strings.Err_ReadPropNoGet);
			}
			int num = PropPaddedReturnSize(aiPropExt);
			m_bc.FAlloc(num);
			IPropertyAdressInfoWithOffset propertyAdressInfoWithOffset = aiPropExt as IPropertyAdressInfoWithOffset;
			int num2;
			if (propertyAdressInfoWithOffset != null && propertyAdressInfoWithOffset.OffsetValueGetter == 4)
			{
				num2 = 4;
			}
			else if (propertyAdressInfoWithOffset != null && propertyAdressInfoWithOffset.OffsetValueSetter == 8)
			{
				num2 = 8;
				if (m_bc.PointerSizeBytes == 4)
				{
					m_bc.FAlloc(4);
				}
			}
			else if (aiPropExt.Size == 8 && m_bc.PointerSizeBytes != 8)
			{
				num2 = 8;
				m_bc.FAlloc(4);
			}
			else
			{
				num2 = m_bc.PointerSizeBytes;
			}
			TrResult result = Translate(aiPropExt.InfoInstance, ECompileMode.Read);
			if (!result.Success)
			{
				return result;
			}
			if (aiPropExt.InterfaceCall)
			{
				m_bc.Der((byte)m_bc.PointerSizeBytes);
			}
			m_bc.VFTab((ushort)aiPropExt.VFTableOffsetGet);
			if (aiPropExt.InterfaceCall)
			{
				m_bc.Itf();
			}
			m_bc.FLd((byte)m_bc.PointerSizeBytes);
			if (!m_bc.NewVFTable)
			{
				m_bc.Der((byte)m_bc.PointerSizeBytes);
			}
			m_bc.Call((ushort)num2, (ushort)num, ByteCmdCreator.ECallMode.IEC);
			m_bc.FAddrE();
			return TrResult.Address();
		}

		private TrResult TrPropertyExtWrite(IPropertyAddressInfoExtended aiPropExt, ECompileMode mode)
		{
			if (aiPropExt.VFTableOffsetSet < 0)
			{
				return TrResult.Error(Strings.Err_WritePropNoSet);
			}
			int num = 0;
			PublishAddr(EExpressionType.PropertyCall);
			m_bc.FAlloc(aiPropExt.Size);
			int size = aiPropExt.Size;
			bool flag = false;
			IPropertyAdressInfoWithOffset propertyAdressInfoWithOffset = aiPropExt as IPropertyAdressInfoWithOffset;
			if (propertyAdressInfoWithOffset != null && propertyAdressInfoWithOffset.OffsetValueSetter == 4)
			{
				size += 4;
			}
			else if (propertyAdressInfoWithOffset != null && propertyAdressInfoWithOffset.OffsetValueSetter == 8)
			{
				size += 8;
				if (m_bc.PointerSizeBytes == 4)
				{
					flag = true;
				}
			}
			else if (aiPropExt.Size == 8 && m_bc.PointerSizeBytes != 8)
			{
				size += m_bc.PointerSizeBytes + 4;
				flag = true;
			}
			else
			{
				size += m_bc.PointerSizeBytes;
			}
			TrResult result = Translate(aiPropExt.InfoInstance, ECompileMode.Read);
			if (!result.Success)
			{
				return result;
			}
			if (aiPropExt.InterfaceCall)
			{
				m_bc.Der((byte)m_bc.PointerSizeBytes);
			}
			m_bc.VFTab((ushort)aiPropExt.VFTableOffsetSet);
			if (aiPropExt.InterfaceCall)
			{
				m_bc.Itf();
			}
			m_bc.FAddrE();
			WriteValue();
			if (flag)
			{
				m_bc.FAlloc(4);
			}
			m_bc.FLd((byte)m_bc.PointerSizeBytes);
			if (!m_bc.NewVFTable)
			{
				m_bc.Der((byte)m_bc.PointerSizeBytes);
			}
			m_bc.Call((ushort)size, (ushort)num, ByteCmdCreator.ECallMode.IEC);
			return TrResult.Address();
		}

		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-95535")]
		private TrResult TrPropertyExtForce(IPropertyAddressInfoExtended aiPropExt, ECompileMode mode)
		{
			if (aiPropExt.VFTableOffsetSet < 0)
			{
				return TrResult.Error(Strings.Err_ForcePropNoSet);
			}
			int num = 0;
			int val = PropPaddedReturnSize(aiPropExt);
			m_bc.FAlloc(Math.Max(val, aiPropExt.Size));
			m_bc.FAlloc(aiPropExt.Size);
			int size = aiPropExt.Size;
			bool flag = false;
			IPropertyAdressInfoWithOffset propertyAdressInfoWithOffset = aiPropExt as IPropertyAdressInfoWithOffset;
			if (propertyAdressInfoWithOffset != null && propertyAdressInfoWithOffset.OffsetValueSetter == 4)
			{
				size += 4;
			}
			else if (propertyAdressInfoWithOffset != null && propertyAdressInfoWithOffset.OffsetValueSetter == 8)
			{
				size += 8;
				if (m_bc.PointerSizeBytes == 4)
				{
					flag = true;
				}
			}
			else if (aiPropExt.Size == 8 && m_bc.PointerSizeBytes != 8)
			{
				size += m_bc.PointerSizeBytes + 4;
				flag = true;
			}
			else
			{
				size += m_bc.PointerSizeBytes;
			}
			TrResult result = Translate(aiPropExt.InfoInstance, ECompileMode.Read);
			if (!result.Success)
			{
				return result;
			}
			if (aiPropExt.InterfaceCall)
			{
				m_bc.Der((byte)m_bc.PointerSizeBytes);
			}
			m_bc.VFTab((ushort)aiPropExt.VFTableOffsetGet);
			m_bc.VFTab((ushort)aiPropExt.VFTableOffsetSet);
			if (aiPropExt.InterfaceCall)
			{
				m_bc.Itf();
			}
			PublishAddr(EExpressionType.PropertyCall);
			m_bc.FAddrE();
			WriteValue();
			if (flag)
			{
				m_bc.FAlloc(4);
			}
			m_bc.FLd((byte)m_bc.PointerSizeBytes);
			short s = 14;
			if (m_bc.NewVFTable)
			{
				s = 12;
			}
			LoadMode();
			m_bc.Ld8(2);
			m_bc.Sub();
			m_bc.BnZ(s);
			m_bc.Pop();
			if (!m_bc.NewVFTable)
			{
				m_bc.Der((byte)m_bc.PointerSizeBytes);
			}
			m_bc.Call((ushort)(size - aiPropExt.Size), (ushort)aiPropExt.Size, ByteCmdCreator.ECallMode.IEC);
			m_bc.FAddrE();
			m_bc.Halt();
			if (!m_bc.NewVFTable)
			{
				m_bc.Der((byte)m_bc.PointerSizeBytes);
			}
			m_bc.Call((ushort)size, (ushort)num, ByteCmdCreator.ECallMode.IEC);
			return TrResult.Address();
		}

		private TrResult TrPropertyAddressInfoExtended(IPropertyAddressInfoExtended aiPropExt, ECompileMode mode)
		{
			switch (mode)
			{
			default:
				return TrPropertyExtRead(aiPropExt, mode);
			case ECompileMode.Write:
				return TrPropertyExtWrite(aiPropExt, mode);
			case ECompileMode.Force:
				return TrPropertyExtForce(aiPropExt, mode);
			}
		}

		private TrResult TrFunctionAddressInfo(IFunctionAddressInfo aiFun, ECompileMode mode)
		{
			if (mode != 0)
			{
				return TrResult.Error("Function calls can only be read");
			}
			int signatureSize = aiFun.SignatureSize;
			m_bc.FAlloc(signatureSize - aiFun.ResultOffset);
			int num = aiFun.ResultOffset;
			for (int num2 = aiFun.InputParameterCount - 1; num2 >= 0; num2--)
			{
				int num3 = aiFun.InputParameterOffsets[num2];
				int num4 = num - num3;
				m_bc.FAlloc(num4);
				m_bc.FAddrE();
				int fStackTopBytes = m_bc.FStackTopBytes;
				IAddressInfo addressInfo = aiFun.InputParameterAddressInfos[num2];
				TrResult result = Translate(addressInfo, ECompileMode.Read);
				if (!result.Success)
				{
					return result;
				}
				if (result.Mode == EResultMode.Value)
				{
					TrResult result2 = ValueToAddress(addressInfo.Size);
					if (!result2.Success)
					{
						return result2;
					}
				}
				m_bc.Cpy(num4);
				int fStackTopBytes2 = m_bc.FStackTopBytes;
				if (fStackTopBytes2 != fStackTopBytes)
				{
					m_bc.FAlloc(fStackTopBytes - fStackTopBytes2);
				}
				num = num3;
			}
			m_bc.Rao((byte)aiFun.FunctionPointerArea, aiFun.FunctionPointerOffset);
			m_bc.Der((byte)m_bc.PointerSizeBytes);
			ByteCmdCreator.ECallMode eCallMode = ((aiFun.ImplementationStyle != 0) ? ByteCmdCreator.ECallMode.C : ByteCmdCreator.ECallMode.IEC);
			m_bc.Call((ushort)aiFun.ResultOffset, (ushort)(aiFun.SignatureSize - aiFun.ResultOffset), eCallMode);
			m_bc.FAddrE();
			return TrResult.Address();
		}

		private TrResult TrParameterAddressInfo(IParameterAddressInfo aiParameter, ECompileMode mode)
		{
			if (mode == ECompileMode.Force)
			{
				return TrResult.Error("Parameters cannot be forced");
			}
			int size = aiParameter.Size;
			switch (mode)
			{
			case ECompileMode.Read:
			{
				int iSizeBytes = (size + 7) / 8 * 8;
				m_bc.FAlloc(iSizeBytes);
				m_bc.FAddrE();
				break;
			}
			case ECompileMode.Write:
				PublishAddr(EExpressionType.IoParameter);
				LoadValueAddress();
				break;
			}
			m_bc.LdU((uint)aiParameter.ModuleType);
			m_bc.LdU((uint)aiParameter.ModuleInstance);
			m_bc.LdU((uint)aiParameter.ParameterId);
			m_bc.LdU((uint)aiParameter.BitOffset);
			m_bc.LdU((uint)aiParameter.BitSize);
			switch (mode)
			{
			case ECompileMode.Read:
				m_bc.IoPR();
				break;
			case ECompileMode.Write:
				m_bc.IoPW();
				break;
			}
			return TrResult.Address();
		}

		[SuppressMessage("Major Code Smell", "S1871:Two branches in a conditional structure should not have exactly the same implementation", Justification = "Will be fixed with CDS-95535")]
		private TrResult TrConversionAddressInfo(IConversionAddressInfo cai, ECompileMode mode)
		{
			if (m_bc.RuntimeIdentification < new Version(3, 5, 3, 0))
			{
				return TrResult.Error(Strings.Err_ConversionsRequire3530);
			}
			if (mode != 0)
			{
				return TrResult.Error("A type conversion can only be read");
			}
			TrResult result = Translate(cai.Base, mode);
			if (!result.Success)
			{
				return result;
			}
			if (result.Mode == EResultMode.Address)
			{
				TrResult result2 = AddressToValue(cai.Base.Size, GetValueType(cai.Base));
				if (!result2.Success)
				{
					return result2;
				}
			}
			TypeClass from = cai.From;
			TypeClass to = cai.To;
			if (TH.IsDateTime(from) && TH.IsDateTime(to) && from != to)
			{
				return TrResult.Error(string.Format(Strings.Err_UnsupportedConversion_2, from, to));
			}
			if (TH.IsInteger(from) && TH.IsInteger(to))
			{
				return CvtInt(from, to);
			}
			if (TH.IsFloat(from) || TH.IsFloat(to))
			{
				return CvtFloat(from, to);
			}
			return TrResult.Error(string.Format(Strings.Err_UnsupportedConversion_2, from, to));
		}

		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-95535")]
		private TrResult CvtInt(TypeClass tcFr, TypeClass tcTo)
		{
			ICompiledType typeFromClass = TMH.GetTypeFromClass(tcFr);
			ICompiledType typeFromClass2 = TMH.GetTypeFromClass(tcTo);
			int num = typeFromClass.Size(null);
			int num2 = typeFromClass2.Size(null);
			if ((TH.IsInteger32(tcFr) && TH.IsInteger32(tcTo)) || m_bc.PointerSizeBytes == 8)
			{
				if (num < num2)
				{
					if (TH.IsSignedInteger(tcFr))
					{
						m_bc.Es((byte)(num * 8));
					}
				}
				else
				{
					if (num > num2)
					{
						if (TH.IsBitOrBool(tcTo))
						{
							m_bc.LdU(0uL);
							m_bc.Sub();
							m_bc.Pop();
							m_bc.SetNZ();
						}
						else
						{
							ulong ulImmediate = (ulong)((1L << num2 * 8) - 1);
							m_bc.LdU(ulImmediate);
							m_bc.And();
						}
					}
					if (TH.IsSignedInteger(tcTo))
					{
						m_bc.Es((byte)(num2 * 8));
					}
				}
			}
			else if (!TH.IsInteger64(tcFr) || !TH.IsInteger64(tcTo))
			{
				if (TH.IsInteger64(tcFr))
				{
					m_bc.C64To32();
					return CvtInt(TypeClass.UDInt, tcTo);
				}
				if (!TH.IsInteger64(tcTo))
				{
					return TrResult.Error("Internal error");
				}
				m_bc.C32To64();
				if (TH.IsSignedInteger(tcTo))
				{
					m_bc.Es64(32);
				}
			}
			return TrResult.Value();
		}

		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-95535")]
		private TrResult CvtFloat(TypeClass tcFr, TypeClass tcTo)
		{
			if (TH.IsFloat(tcFr) && TH.IsFloat(tcTo))
			{
				if (TH.IsReal(tcFr) && TH.IsLReal(tcTo))
				{
					m_bc.CRToLR();
				}
				else if (TH.IsLReal(tcFr) && TH.IsReal(tcTo))
				{
					m_bc.CLRToR();
				}
				else if (tcFr != tcTo)
				{
					return TrResult.Error("Internal error");
				}
			}
			else if (TH.IsInteger(tcFr))
			{
				if (TH.IsInteger32(tcFr) || m_bc.PointerSizeBytes == 8)
				{
					if (TH.IsReal(tcTo))
					{
						m_bc.CIntToR(TH.IsSignedInteger(tcFr));
					}
					else
					{
						m_bc.CIntToLR(TH.IsSignedInteger(tcFr));
					}
				}
				else if (TH.IsReal(tcTo))
				{
					m_bc.CIntToR64(TH.IsSignedInteger(tcFr));
				}
				else
				{
					m_bc.CIntToLR64(TH.IsSignedInteger(tcFr));
				}
			}
			else
			{
				if (!TH.IsInteger(tcTo))
				{
					return TrResult.Error(string.Format(Strings.Err_UnsupportedConversion_2, tcFr, tcTo));
				}
				if (TH.IsInteger32(tcTo) || m_bc.PointerSizeBytes == 8)
				{
					if (TH.IsReal(tcFr))
					{
						m_bc.CRToInt(TH.IsSignedInteger(tcTo));
					}
					else
					{
						m_bc.CLRToInt(TH.IsSignedInteger(tcTo));
					}
					TypeClass tcFr2 = ((m_bc.PointerSizeBytes != 8) ? (TH.IsSignedInteger(tcTo) ? TypeClass.DInt : TypeClass.UDInt) : (TH.IsSignedInteger(tcTo) ? TypeClass.LInt : TypeClass.ULInt));
					return CvtInt(tcFr2, tcTo);
				}
				if (TH.IsReal(tcFr))
				{
					m_bc.CRToInt64(TH.IsSignedInteger(tcTo));
				}
				else
				{
					m_bc.CLRToInt64(TH.IsSignedInteger(tcTo));
				}
			}
			return TrResult.Value();
		}

		private TrResult TrLiteralAddressInfo(ILiteralAddressInfo lai, ECompileMode mode)
		{
			if (m_bc.RuntimeIdentification < new Version(3, 5, 3, 0))
			{
				return TrResult.Error(Strings.Err_LiteralsRequire3530);
			}
			if (mode != 0)
			{
				return TrResult.Error("Literals can only be read");
			}
			ILiteralValue value = lai.Value;
			switch (value.KindOf)
			{
			case KindOfLiteral.Bool:
				m_bc.Ld8((byte)(value.Bool ? 1u : 0u));
				break;
			case KindOfLiteral.UnsignedInteger:
				m_bc.LdU_T(value.UnsignedLong, lai.Type.Class);
				break;
			case KindOfLiteral.SignedInteger:
				m_bc.LdS_T(value.SignedLong, lai.Type.Class);
				break;
			case KindOfLiteral.Float:
				if (lai.Type.Class == TypeClass.Real)
				{
					m_bc.LdR((float)value.Float);
				}
				else
				{
					m_bc.LdLR(value.Float);
				}
				break;
			case KindOfLiteral.String:
				return TrResult.Error(Strings.Err_StringLiteralsNotSupported);
			case KindOfLiteral.None:
				return TrResult.Error("Invalid literal (KindOfLiteral.None)");
			default:
				return TrResult.Error("Internal error");
			}
			return TrResult.Value();
		}

		private TrResult TrAddressAddressInfo(IAddressAddressInfo aiAddr)
		{
			m_bc.LoadAddressOntoStack(aiAddr.Address);
			return TrResult.Address();
		}

		public TrResult AddressToValue(int iSizeBytes, EValueType eValueType)
		{
			if (iSizeBytes != 1 && iSizeBytes != 2 && iSizeBytes != 4 && iSizeBytes != 8)
			{
				return TrResult.Error($"Cannot load a value of size {iSizeBytes} onto the stack");
			}
			if (iSizeBytes > m_bc.PointerSizeBytes)
			{
				m_bc.Der64();
				return TrResult.Value();
			}
			m_bc.Der((byte)iSizeBytes);
			if (eValueType == EValueType.Signed && iSizeBytes != m_bc.PointerSizeBytes)
			{
				m_bc.Es((byte)(iSizeBytes * 8));
			}
			return TrResult.Value();
		}

		private TrResult ValueToAddress(int iSizeBytes)
		{
			if (iSizeBytes != 1 && iSizeBytes != 2 && iSizeBytes != 4 && iSizeBytes != 8)
			{
				return TrResult.Error($"Cannot push a value of size {iSizeBytes} onto the f-stack");
			}
			if (iSizeBytes > m_bc.PointerSizeBytes)
			{
				m_bc.FLd64();
				m_bc.FAddrE();
				return TrResult.Value();
			}
			m_bc.FLd((byte)iSizeBytes);
			m_bc.FAddrE();
			return TrResult.Value();
		}

		private void SetBit(int iSizeBytes, byte byBitOffset)
		{
			LoadValueAddress();
			m_bc.Der(1);
			m_bc.LdU(byBitOffset);
			m_bc.SetBit((byte)iSizeBytes);
		}

		private void PublishAddr(EExpressionType et)
		{
			LoadMode();
			m_bc.BnZ(6);
			m_bc.Ld8((byte)et);
			m_bc.Halt();
		}

		private void WriteValue()
		{
			LoadValueAddress();
			LoadValueLength();
			m_bc.Cpy();
		}

		private void LoadValueAddress()
		{
			m_bc.FAddrB((byte)m_bc.PointerSizeBytes);
			m_bc.Der((byte)m_bc.PointerSizeBytes);
		}

		private void LoadValueLength()
		{
			m_bc.FAddrB((byte)(m_bc.PointerSizeBytes + 4));
			m_bc.Der(4);
		}

		private void LoadMode()
		{
			m_bc.FAddrB((byte)(m_bc.PointerSizeBytes + 8));
			m_bc.Der(4);
		}
	}
}
