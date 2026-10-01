using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using \u0011;
using \u0019;
using \u001F;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace \u0080
{
	// Token: 0x020003B1 RID: 945
	internal static class \u001A
	{
		// Token: 0x06003670 RID: 13936 RVA: 0x000DBDA0 File Offset: 0x000D9FA0
		internal static _IExprement \u0001(_IVariable \u0002, bool \u0003, _IType \u0004, _IExpression \u0005, IScope5 \u0006, out bool \u0007, _ISignature \u0008, _ISignature \u000E, string \u000F, string \u0010, int \u0011, _ICompileContext \u0012)
		{
			_IVariableExpression u000F = \u0019.\u0003.\u0001(\u000F);
			_IVariableExpression u = \u0019.\u0003.\u0001(\u0010);
			return \u001A.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, out \u0007, \u0008, \u000E, u000F, u, \u0011, \u0012);
		}

		// Token: 0x06003671 RID: 13937 RVA: 0x000DBDD4 File Offset: 0x000D9FD4
		internal static _IExprement \u0001(_IVariable \u0002, bool \u0003, _IType \u0004, _IExpression \u0005, IScope5 \u0006, out bool \u0007, _ISignature \u0008, _ISignature \u000E, bool \u000F, bool \u0010, int \u0011, _ICompileContext \u0012)
		{
			_ILiteralExpression u000F = \u0019.\u0003.\u0001(\u000F);
			_ILiteralExpression u = \u0019.\u0003.\u0001(\u0010);
			return \u001A.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, out \u0007, \u0008, \u000E, u000F, u, \u0011, \u0012);
		}

		// Token: 0x06003672 RID: 13938 RVA: 0x000DBE08 File Offset: 0x000DA008
		internal static _IExprement \u0001(_IVariable \u0002, bool \u0003, _IType \u0004, _IExpression \u0005, IScope5 \u0006, out bool \u0007, _ISignature \u0008, _ISignature \u000E, _IExpression \u000F, _IExpression \u0010, int \u0011, _ICompileContext \u0012)
		{
			\u0007 = true;
			_IExprement iexprement = \u001A.\u0001(\u0002, ref \u0004);
			if (iexprement != null)
			{
				return iexprement.Duplicate();
			}
			switch (\u0004.Class)
			{
			case TypeClass.Bool:
			case TypeClass.Bit:
			case TypeClass.Byte:
			case TypeClass.Word:
			case TypeClass.DWord:
			case TypeClass.LWord:
			case TypeClass.SInt:
			case TypeClass.Int:
			case TypeClass.DInt:
			case TypeClass.LInt:
			case TypeClass.USInt:
			case TypeClass.UInt:
			case TypeClass.UDInt:
			case TypeClass.ULInt:
			case TypeClass.Time:
			case TypeClass.Date:
			case TypeClass.DateAndTime:
			case TypeClass.TimeOfDay:
			case TypeClass.LTime:
			case TypeClass.LDate:
			case TypeClass.LDateAndTime:
			case TypeClass.LTimeOfDay:
				return \u0019.\u0003.\u0001(0L, \u0004.Class);
			case TypeClass.Real:
			case TypeClass.LReal:
				return \u0019.\u0003.\u0001(0.0, \u0004.Class);
			case TypeClass.String:
			case TypeClass.WString:
				return \u001A.\u0001(\u0002, \u0004, \u0006, \u0012);
			case TypeClass.Pointer:
				break;
			case TypeClass.Reference:
				return null;
			case TypeClass.Subrange:
				return \u001A.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, out \u0007, \u0008, \u000E, \u000F, \u0010, \u0012);
			case TypeClass.Enum:
				return \u001A.\u0001(\u0004, \u0006);
			case TypeClass.Array:
				return \u001A.\u0002(\u0002, \u0003, \u0004, \u0005, \u0006, ref \u0007, \u0008, \u000E, \u000F, \u0010, \u0011, \u0012);
			case TypeClass.Params:
			case TypeClass.None:
			case TypeClass.Any:
			case TypeClass.AnyBit:
			case TypeClass.AnyDate:
			case TypeClass.AnyInt:
			case TypeClass.AnyNum:
			case TypeClass.AnyReal:
			case TypeClass.Lazy:
			case TypeClass.BitConst:
			case TypeClass.UXInt:
			case TypeClass.XWord:
			case TypeClass.XInt:
			case TypeClass.XString:
			case TypeClass.VarLenArray:
			case TypeClass.AnyString:
				goto IL_1CE;
			case TypeClass.Userdef:
			{
				_IUserdefType iuserdefType = \u0004 as _IUserdefType;
				_ISignature isignature = \u0006[iuserdefType.SignatureId] as _ISignature;
				if (isignature == null)
				{
					return null;
				}
				if (!isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
				{
					return \u001A.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, out \u0007, \u0008, \u000E, \u000F, \u0010, \u0011, \u0012, 0);
				}
				if (\u0003)
				{
					return null;
				}
				break;
			}
			case TypeClass.__Vector:
				return \u001A.\u0002(\u0004, \u0006);
			default:
				goto IL_1CE;
			}
			if (\u0012.GetCodegeneratorProperty(CodegeneratorProperties.LWordPointer))
			{
				return \u0019.\u0003.\u0001(0L, TypeClass.LWord);
			}
			return \u0019.\u0003.\u0001(0L, TypeClass.DWord);
			IL_1CE:
			return null;
		}

		// Token: 0x06003673 RID: 13939 RVA: 0x000DBFE4 File Offset: 0x000DA1E4
		internal static _IExprement \u0001(_IVariable \u0002, ref _IType \u0003)
		{
			if (\u0003 != null)
			{
				_IAliasType ialiasType = \u0003 as _IAliasType;
				if (ialiasType != null)
				{
					\u0003 = (_IType)ialiasType.EffectiveType;
					if (ialiasType._DefaultValue != null)
					{
						return ialiasType._DefaultValue;
					}
				}
			}
			return null;
		}

		// Token: 0x06003674 RID: 13940 RVA: 0x000DC020 File Offset: 0x000DA220
		private static _IExprement \u0002(_IVariable \u0002, bool \u0003, _IType \u0004, _IExpression \u0005, IScope5 \u0006, ref bool \u0007, _ISignature \u0008, _ISignature \u000E, _IExpression \u000F, _IExpression \u0010, int \u0011, _ICompileContext \u0012)
		{
			_IArrayType iarrayType = \u0004 as _IArrayType;
			int num = Helper.\u0001(iarrayType);
			if (num == 0)
			{
				return null;
			}
			bool flag;
			int numOfElements = iarrayType.GetNumOfElements(\u0006, out flag);
			if (flag && numOfElements == 0)
			{
				return null;
			}
			_IExpression u = \u001A.\u0001(flag, iarrayType, numOfElements);
			_IType itype = \u0084.\u0004.\u0001(iarrayType) as _IType;
			if (\u0008 != null)
			{
				Helper.\u0001(num, \u0008, \u000E);
			}
			bool flag2 = false;
			if (itype.Class == TypeClass.Userdef)
			{
				ISignature signature = null;
				if (!\u001A.\u0001(\u0006, iarrayType))
				{
					_IUserdefType iuserdefType = itype as _IUserdefType;
					if (iuserdefType != null)
					{
						signature = \u0006[iuserdefType.SignatureId];
					}
					flag2 = (signature != null && !signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion));
				}
				if (signature != null && \u001A.\u0001(\u0002, flag2, signature))
				{
					_ISequenceStatement isequenceStatement = \u0019.\u0003.\u0001();
					IEnumerable<_IExpression> enumerable = Helper.\u0001(\u0005, \u0006, iarrayType);
					int num2 = 0;
					int num3 = Helper.\u0001((_ISignature)signature.GetSubSignature(IdentifierConstants.InitMethodName)).Count<_IVariable>() - 3;
					foreach (_IExpression u2 in enumerable)
					{
						_IStatement sm = \u001A.\u0001(\u0002, \u0003, itype, u2, \u0006, out \u0007, \u0008, \u000E, \u000F, \u0010, \u0011, \u0012, num2) as _IStatement;
						isequenceStatement.Add(sm);
						num2 += num3;
					}
					return isequenceStatement;
				}
			}
			if (flag2)
			{
				_IExpression u3 = Helper.\u0001(\u0005, iarrayType, \u0011);
				_IStatement istatement = \u001A.\u0001(\u0002, \u0003, itype, u3, \u0006, out \u0007, \u0008, \u000E, \u000F, \u0010, \u0011, \u0012) as _IStatement;
				if (istatement == null)
				{
					return null;
				}
				return Helper.\u0001(iarrayType, istatement, \u0006, \u0011);
			}
			else
			{
				_IExpression iexpression = \u001A.\u0001(\u0002, \u0003, iarrayType.OriginalBaseType, \u0005, \u0006, out \u0007, \u0008, \u000E, \u000F, \u0010, \u0011, \u0012) as _IExpression;
				if (iexpression == null)
				{
					return null;
				}
				return \u001A.\u0001(u, iexpression);
			}
		}

		// Token: 0x06003675 RID: 13941 RVA: 0x000DC1FC File Offset: 0x000DA3FC
		private static bool \u0001(_IVariable \u0002, bool \u0003, ISignature \u0004)
		{
			return !\u0002.HasAttribute("old_input_assignments") && \u0003 && \u0004.GetSubSignature(IdentifierConstants.InitMethodName).Inputs.Count<IVariable>() > 3;
		}

		// Token: 0x06003676 RID: 13942 RVA: 0x000DC22C File Offset: 0x000DA42C
		private static _IExprement \u0001(_IExpression \u0002, _IExpression \u0003)
		{
			_IArrayInitialization iarrayInitialization = \u0019.\u0003.\u0001();
			_IMultipleIndexInitialization imultipleIndexInitialization = \u0019.\u0003.\u0001();
			imultipleIndexInitialization._Number = \u0002;
			imultipleIndexInitialization._Value = \u0003;
			iarrayInitialization.AddInitValue(imultipleIndexInitialization);
			return iarrayInitialization;
		}

		// Token: 0x06003677 RID: 13943 RVA: 0x000DC25C File Offset: 0x000DA45C
		private static bool \u0001(IScope5 \u0002, _IArrayType \u0003)
		{
			if (\u0003.OriginalBaseType != null)
			{
				_IAliasType ialiasType = \u0003.OriginalBaseType as _IAliasType;
				if (ialiasType != null && \u0002[ialiasType.SignatureId].All[0].Initial != null)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06003678 RID: 13944 RVA: 0x000DC2A0 File Offset: 0x000DA4A0
		private static _IExpression \u0001(bool \u0002, _IArrayType \u0003, int \u0004)
		{
			_IExpression result;
			if (!\u0002)
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < \u0003._Dimensions.Count; i++)
				{
					string value = string.Concat(new string[]
					{
						"(",
						\u0003._Dimensions[i].UpperBorder.ToString(),
						"-",
						\u0003._Dimensions[i].LowerBorder.ToString(),
						"+ 1)"
					});
					if (i > 0)
					{
						stringBuilder.Append(" * ");
					}
					stringBuilder.Append(value);
				}
				result = (global::\u0011.\u0006.\u0001(stringBuilder.ToString()) as _IExpression);
			}
			else
			{
				TypeClass u = TypeClass.Int;
				if (\u0004 < -32768 || \u0004 > 32767)
				{
					u = TypeClass.DInt;
				}
				result = \u0019.\u0003.\u0001((long)\u0004, u);
			}
			return result;
		}

		// Token: 0x06003679 RID: 13945 RVA: 0x000DC374 File Offset: 0x000DA574
		private static _IExprement \u0001(_IVariable \u0002, bool \u0003, _IType \u0004, _IExpression \u0005, IScope5 \u0006, out bool \u0007, _ISignature \u0008, _ISignature \u000E, _IExpression \u000F, _IExpression \u0010, _ICompileContext \u0011)
		{
			_ISubrangeType isubrangeType = \u0004 as _ISubrangeType;
			_IExprement iexprement = \u001A.\u0001(\u0002, \u0003, isubrangeType._Base, \u0005, \u0006, out \u0007, \u0008, \u000E, \u000F, \u0010, 0, \u0011);
			if (!(iexprement is _IExpression))
			{
				return iexprement;
			}
			_IExpression iexpression = iexprement as _IExpression;
			ILiteralValue literalValue = iexpression.Literal(\u0006, true);
			ILiteralValue literalValue2 = isubrangeType._LowerBorder.Literal(\u0006, true);
			ILiteralValue literalValue3 = isubrangeType._UpperBorder.Literal(\u0006, true);
			long num;
			ulong num2;
			if (TypeTable.IsSigned(isubrangeType._LowerBorder.Type.Class))
			{
				bool flag;
				num = literalValue2.GetSignedLong(out flag);
				num2 = (ulong)num;
			}
			else
			{
				bool flag;
				num2 = literalValue2.GetUnsignedLong(out flag);
				num = (long)num2;
			}
			long num3;
			ulong num4;
			if (TypeTable.IsSigned(isubrangeType._UpperBorder.Type.Class))
			{
				bool flag;
				num3 = literalValue3.GetSignedLong(out flag);
				num4 = (ulong)num3;
			}
			else
			{
				bool flag;
				num4 = literalValue3.GetUnsignedLong(out flag);
				num3 = (long)num4;
			}
			if (TypeTable.IsSigned(isubrangeType._Base.Class))
			{
				bool flag;
				long signedLong = literalValue.GetSignedLong(out flag);
				if (signedLong < num || signedLong > num3)
				{
					return isubrangeType._LowerBorder.Duplicate();
				}
			}
			else
			{
				bool flag;
				ulong unsignedLong = literalValue.GetUnsignedLong(out flag);
				if (unsignedLong < num2 || unsignedLong > num4)
				{
					return isubrangeType._LowerBorder.Duplicate();
				}
			}
			return iexpression;
		}

		// Token: 0x0600367A RID: 13946 RVA: 0x000DC4A8 File Offset: 0x000DA6A8
		private static _IExprement \u0001(_IVariable \u0002, _IType \u0003, IScope5 \u0004, _ICompileContext \u0005)
		{
			string text = string.Empty;
			if (\u0002 != null)
			{
				bool flag = \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_INIT_NAMESPACE);
				bool flag2 = \u0002.HasAttribute("init_namespace_lowercase");
				if (flag || flag2)
				{
					if (string.IsNullOrEmpty(\u0004.Name) && \u0004.LocalSignature != null && !string.IsNullOrEmpty(\u0004.LocalSignature.LibraryPath))
					{
						text = \u0005.GetLibraryNamespace(\u0004.LocalSignature.LibraryPath);
					}
					else
					{
						text = \u0004.Name;
					}
					if (flag2)
					{
						text = text.ToLowerInvariant();
					}
				}
			}
			return \u0019.\u0003.\u0001(text, \u0003.Class);
		}

		// Token: 0x0600367B RID: 13947 RVA: 0x000DC534 File Offset: 0x000DA734
		private static _IExprement \u0001(_IType \u0002, IScope5 \u0003)
		{
			_IEnumType ienumType = \u0002 as _IEnumType;
			_ISignature isignature = ienumType.GetSignature(\u0003) as _ISignature;
			if (ienumType != null && ienumType._DefaultValue != null && ienumType._DefaultValue != null)
			{
				foreach (_IVariable ivariable in isignature.AllVariables)
				{
					if (ivariable.Name.Equals(ienumType._DefaultValue.Name, StringComparison.CurrentCultureIgnoreCase))
					{
						return (ivariable.Initial as _IExpression).Duplicate();
					}
				}
			}
			bool flag = false;
			_IExpression iexpression = null;
			foreach (_IVariable ivariable2 in isignature.AllVariables)
			{
				if (ivariable2.Initial == null)
				{
					flag = true;
					break;
				}
				if (iexpression == null)
				{
					iexpression = (ivariable2.Initial as _IExpression);
				}
				ILiteralValue literalValue = ivariable2.Initial.Literal(\u0003);
				if (literalValue != null)
				{
					int num = 0;
					if (literalValue.GetInt(out num) && num == 0)
					{
						flag = true;
					}
				}
			}
			bool flag2 = false;
			if (iexpression != null && iexpression.MessagesList != null)
			{
				foreach (IMessage message in iexpression.MessagesList)
				{
					if (message.Severity == Severity.Error || message.Severity == Severity.FatalError)
					{
						flag2 = true;
					}
				}
			}
			if (flag || iexpression == null || flag2)
			{
				return \u0019.\u0003.\u0001(0L, (\u0002 as _IEnumType)._Base.Class);
			}
			return iexpression.Duplicate();
		}

		// Token: 0x0600367C RID: 13948 RVA: 0x000DC6EC File Offset: 0x000DA8EC
		private static _IExprement \u0002(_IType \u0002, IScope5 \u0003)
		{
			_IVectorType ivectorType = \u0002 as _IVectorType;
			Operator u = (ivectorType.BaseType.Class == TypeClass.LReal) ? Operator.__vcSetLReal : Operator.__vcSetReal;
			bool flag;
			int num = ivectorType.DimensionInt(\u0003, out flag);
			List<IExpression> list = new List<IExpression>();
			for (int i = 0; i < num; i++)
			{
				list.Add(\u0019.\u0003.\u0001(0.0, ivectorType.BaseType.Class));
			}
			return \u0019.\u0003.\u0001(u, list);
		}

		// Token: 0x0600367D RID: 13949 RVA: 0x000DC768 File Offset: 0x000DA968
		private static _IExprement \u0001(_IVariable \u0002, bool \u0003, _IType \u0004, _IExpression \u0005, IScope5 \u0006, out bool \u0007, _ISignature \u0008, _ISignature \u000E, _IExpression \u000F, _IExpression \u0010, int \u0011, _ICompileContext \u0012, int \u0013)
		{
			\u0007 = false;
			_IUserdefType iuserdefType = \u0004 as _IUserdefType;
			_ISignature isignature = \u0006[iuserdefType.SignatureId] as _ISignature;
			if (isignature == null)
			{
				return null;
			}
			if (isignature.GetSubSignature(IdentifierConstants.VFInitMethodName) != null || isignature.POUType == Operator.FunctionBlock)
			{
				bool minimalSystem = (\u0006.ApplicationContext as _ICompileContext).MinimalSystem;
				_IExpressionStatement iexpressionStatement = null;
				if (!minimalSystem)
				{
					_ICompoAccessExpression icompoAccessExpression = \u0019.\u0003.\u0001(\u0005.Duplicate() as _IExpression, Token.Empty);
					icompoAccessExpression._Right = \u0019.\u0003.\u0001(IdentifierConstants.VFInitMethodName);
					iexpressionStatement = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(icompoAccessExpression, Token.Empty), Token.Empty);
				}
				if (\u0003)
				{
					return iexpressionStatement;
				}
				_ISequenceStatement isequenceStatement = \u0019.\u0003.\u0001();
				if (iexpressionStatement != null)
				{
					isequenceStatement.Add(iexpressionStatement);
				}
				_ICompoAccessExpression icompoAccessExpression2 = \u0019.\u0003.\u0001(\u0005.Duplicate() as _IExpression, Token.Empty);
				icompoAccessExpression2._Right = \u0019.\u0003.\u0001(IdentifierConstants.InitMethodName);
				_ICallExpression icallExpression = \u0019.\u0003.\u0001(icompoAccessExpression2, Token.Empty);
				icallExpression.AddParam(\u000F);
				icallExpression.AddParam(\u0010);
				if (\u0002 != null && \u0002.InputAssignments != null)
				{
					IAssignmentExpression[] inputAssignments = \u0002.InputAssignments;
					_ISignature isignature2 = (_ISignature)isignature.GetSubSignature(IdentifierConstants.InitMethodName);
					if (isignature2.GetFlagInternal(SignatureFlagInternal.Overloaded))
					{
						isignature2 = \u001F.\u0010.\u0001((ICommonScope)\u0006, \u0012, inputAssignments, isignature2, (_ISignature4)isignature).FirstOrDefault<_ISignature>();
					}
					IVariable[] allInputs = isignature2.AllInputs;
					for (int i = 2; i < allInputs.Length - 1; i++)
					{
						if (!allInputs[i].HasAttribute(CompileAttributes.ATTRIBUTE_IMPLICIT_INPUT))
						{
							bool flag = false;
							int num = \u0013;
							while (num < inputAssignments.Length && !(inputAssignments[num].LValue is _INullExpression))
							{
								if ((allInputs[i] as _IVariable).VersionedName.ToUpperInvariant() == inputAssignments[num].LValue.ToString().ToUpperInvariant())
								{
									icallExpression.AddParam((inputAssignments[num] as _IAssignmentExpression)._RValue.Duplicate() as _IExpression);
									flag = true;
									break;
								}
								num++;
							}
							if (!flag)
							{
								int num2 = i - 2;
								int num3 = \u0013 + num2;
								icallExpression.AddParam((inputAssignments[num3] as _IAssignmentExpression)._RValue.Duplicate() as _IExpression);
							}
						}
					}
				}
				_IExpressionStatement sm = \u0019.\u0003.\u0001(icallExpression, Token.Empty);
				isequenceStatement.Add(sm);
				if (((\u0002 != null) ? \u0002.Initial : null) == null)
				{
					\u001A.\u0001(isignature, \u0005, \u0006, isequenceStatement);
				}
				return isequenceStatement;
			}
			else
			{
				if (isignature.GetFlag(SignatureFlag.Structure) && !\u0003)
				{
					_ICompoAccessExpression icompoAccessExpression3 = \u0019.\u0003.\u0001(\u0005, Token.Empty);
					icompoAccessExpression3._Right = \u0019.\u0003.\u0001(IdentifierConstants.InitMethodName);
					_ICallExpression icallExpression2 = \u0019.\u0003.\u0001(icompoAccessExpression3, Token.Empty);
					icallExpression2.AddParam(\u000F);
					icallExpression2.AddParam(\u0010);
					return \u0019.\u0003.\u0001(icallExpression2, Token.Empty);
				}
				return null;
			}
		}

		// Token: 0x0600367E RID: 13950 RVA: 0x000DCA1C File Offset: 0x000DAC1C
		internal static _ISequenceStatement \u0001(_ISignature \u0002, _IExpression \u0003, IScope5 \u0004, _ISequenceStatement \u0005 = null)
		{
			_ISignature isignature = \u0002;
			HashSet<string> u = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
			while (isignature != null && isignature.HasAttribute(CompileAttributes.ATTRIBUTE_CALL_AFTER_INIT))
			{
				foreach (object obj in isignature._SubSignatures)
				{
					ISignature u2 = (ISignature)obj;
					\u001A.\u0001(\u0003, ref \u0005, u, u2);
				}
				isignature = (\u0004[isignature.BaseSignatureId] as _ISignature);
			}
			return \u0005;
		}

		// Token: 0x0600367F RID: 13951 RVA: 0x000DCAB0 File Offset: 0x000DACB0
		private static void \u0001(_IExpression \u0002, ref _ISequenceStatement \u0003, HashSet<string> \u0004, ISignature \u0005)
		{
			if (!\u0005.HasAttribute(CompileAttributes.ATTRIBUTE_CALL_AFTER_INIT))
			{
				return;
			}
			if (\u0004.Contains(\u0005.Name))
			{
				return;
			}
			\u0004.Add(\u0005.Name);
			_IExpression u;
			if (\u0002 != null)
			{
				u = \u0019.\u0003.\u0001(\u0002.Duplicate() as _IExpression, \u0019.\u0003.\u0001(\u0005.OrgName));
			}
			else
			{
				u = \u0019.\u0003.\u0001(\u0005.OrgName);
			}
			_IExpressionStatement sm = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(u, Token.Empty), Token.Empty);
			if (\u0003 == null)
			{
				\u0003 = \u0019.\u0003.\u0001();
			}
			\u0003.Add(sm);
		}

		// Token: 0x04000A9B RID: 2715
		private const int \u0001 = 2;

		// Token: 0x04000A9C RID: 2716
		private const int \u0002 = 1;

		// Token: 0x04000A9D RID: 2717
		private const int \u0003 = 3;
	}
}
