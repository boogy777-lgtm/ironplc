using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0003;
using \u0006;
using \u0007;
using \u000E;
using \u0011;
using \u0014;
using \u001D;
using \u001F;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0004
{
	// Token: 0x0200031C RID: 796
	internal static class \u0011
	{
		// Token: 0x06002FAD RID: 12205 RVA: 0x000B3B90 File Offset: 0x000B1D90
		private static void \u0001(TypeCheckerVisitor \u0002, _IVariable \u0003, IScope5 \u0004)
		{
			_IArrayType iarrayType = \u0003.Type as _IArrayType;
			if (iarrayType != null && \u0003.Initial != null)
			{
				_IArrayInitialization iarrayInitialization = \u0003.Initial as _IArrayInitialization;
				if (iarrayInitialization != null)
				{
					global::\u0004.\u0011.\u0001(\u0002, iarrayInitialization, iarrayType, \u0004);
					return;
				}
			}
			_IStructureInitialization istructureInitialization = \u0003.Initial as _IStructureInitialization;
			if (istructureInitialization != null)
			{
				global::\u0004.\u0011.\u0001(\u0002, istructureInitialization, \u0004);
			}
		}

		// Token: 0x06002FAE RID: 12206 RVA: 0x000B3BE4 File Offset: 0x000B1DE4
		private static void \u0001(_ISignature \u0002, _IVariable \u0003, IScope5 \u0004)
		{
			global::\u0004.\u0011.\u0001.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06002FAF RID: 12207 RVA: 0x000B3BF0 File Offset: 0x000B1DF0
		private static void \u0001(TypeCheckerVisitor \u0002, _IArrayInitialization \u0003, _IArrayType \u0004, IScope5 \u0005)
		{
			IEnumerable<_IExpression> initValues = \u0003._InitValues;
			int num = 0;
			foreach (_IExpression iexpression in initValues)
			{
				_IMultipleIndexInitialization imultipleIndexInitialization = iexpression as _IMultipleIndexInitialization;
				if (imultipleIndexInitialization != null)
				{
					num = global::\u0004.\u0011.\u0001(\u0002, \u0005, num, imultipleIndexInitialization);
				}
				else
				{
					num++;
					_IStructureInitialization istructureInitialization = iexpression as _IStructureInitialization;
					if (istructureInitialization != null)
					{
						global::\u0004.\u0011.\u0001(\u0002, istructureInitialization, \u0005);
					}
					else
					{
						_IArrayInitialization iarrayInitialization = iexpression as _IArrayInitialization;
						if (iarrayInitialization != null)
						{
							_IArrayType iarrayType = \u0004._Base as _IArrayType;
							if (iarrayType == null)
							{
								break;
							}
							global::\u0004.\u0011.\u0001(\u0002, iarrayInitialization, iarrayType, \u0005);
						}
					}
				}
			}
			bool flag;
			if (num > \u0004.GetNumOfElements(\u0005, out flag) && flag)
			{
				\u0002.AddError(\u0003, MessageId.Err_TooManyInitializer, Array.Empty<object>());
			}
		}

		// Token: 0x06002FB0 RID: 12208 RVA: 0x000B3CB8 File Offset: 0x000B1EB8
		private static int \u0001(TypeCheckerVisitor \u0002, IScope5 \u0003, int \u0004, _IMultipleIndexInitialization \u0005)
		{
			IExpression number = \u0005.Number;
			ILiteralValue literalValue = ((_IExpression)number).Literal(\u0003, true);
			if (literalValue != null)
			{
				int num;
				if (literalValue.GetInt(out num))
				{
					\u0004 += num;
				}
			}
			else
			{
				\u0002.AddError(\u0005, MessageId.Err_ArrayInitialisationCountNoConstant, new object[]
				{
					number.ToString()
				});
			}
			return \u0004;
		}

		// Token: 0x06002FB1 RID: 12209 RVA: 0x000B3D0C File Offset: 0x000B1F0C
		private static void \u0001(TypeCheckerVisitor \u0002, _IStructureInitialization \u0003, IScope5 \u0004)
		{
			foreach (_IAssignmentExpression iassignmentExpression in \u0003._CompoInits)
			{
				_IArrayType iarrayType = iassignmentExpression.LValue.Type as _IArrayType;
				if (iarrayType != null)
				{
					_IArrayInitialization iarrayInitialization = iassignmentExpression.RValue as _IArrayInitialization;
					if (iarrayInitialization != null)
					{
						global::\u0004.\u0011.\u0001(\u0002, iarrayInitialization, iarrayType, \u0004);
						continue;
					}
				}
				_IStructureInitialization istructureInitialization = iassignmentExpression.RValue as _IStructureInitialization;
				if (istructureInitialization != null)
				{
					global::\u0004.\u0011.\u0001(\u0002, istructureInitialization, \u0004);
				}
			}
		}

		// Token: 0x06002FB2 RID: 12210 RVA: 0x000B3D9C File Offset: 0x000B1F9C
		private static void \u0001(_IVariable \u0002, IScope5 \u0003, TypeCheckerVisitor \u0004, ErrorVisitor \u0005, ref bool \u0006)
		{
			if (\u0002.GetFlag(VarFlag.ReplacedConstant) || \u0002.GetFlag(VarFlag.Constant))
			{
				bool flag;
				((_IExpression)\u0002.Initial).LiteralWithRecursionCheck(\u0003, new LDictionary<IVariable, IVariable>(), true, out flag);
				if (flag)
				{
					\u0004.AddError((_IExpression)\u0002.Initial, MessageId.Err_RecursiveConstantInitialisation, Array.Empty<object>());
					((_IExpression)\u0002.Initial).Accept(\u0005);
					\u0006 = false;
				}
			}
		}

		// Token: 0x06002FB3 RID: 12211 RVA: 0x000B3E0C File Offset: 0x000B200C
		private static bool \u0001(_IVariable \u0002, IScope5 \u0003, _ISignature \u0004, TypeCheckerVisitor \u0005, ICompiledType3 \u0006, _IExpression \u0007, ref global::\u0004.\u0011.\u0002 \u0008)
		{
			if (\u0006.Class == TypeClass.Reference)
			{
				ILiteralValue literalValue = \u0002.Initial.Literal(\u0003);
				int num;
				if (literalValue == null || !literalValue.GetInt(out num) || num != 0)
				{
					if (\u0006.DeRefType.Class == TypeClass.Bool && \u0007.Type is _IDirectAddressBitType)
					{
						\u0005.AddError(\u0007, MessageId.Err_NoReferenceToBits, new object[]
						{
							\u0007,
							\u0006.EffectiveType
						});
					}
					else if (!\u0007.IsLValue(\u0003, false))
					{
						\u0005.AddError(\u0007, MessageId.Err_IsNoLValue, new object[]
						{
							\u0007,
							\u0006.EffectiveType
						});
					}
					else if (!global::\u0006.\u0011.\u0002(\u0007.Type, \u0006.DeRefType, \u0003 as ICommonScope))
					{
						\u0005.AddError(\u0007, MessageId.Err_TypeMismatch, new object[]
						{
							\u0007.Type,
							\u0006.EffectiveType
						});
					}
					if (TypeClass.Userdef == \u0006.DeRefType.Class && \u0007.Type is _IUserdefType)
					{
						\u0008 |= global::\u0004.\u0011.\u0001(\u0002, \u0004, \u0005, \u0006, \u0007);
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x06002FB4 RID: 12212 RVA: 0x000B3F2C File Offset: 0x000B212C
		private static global::\u0004.\u0011.\u0002 \u0001(_IVariable \u0002, _ISignature \u0003, TypeCheckerVisitor \u0004, ICompiledType3 \u0005, _IExpression \u0006)
		{
			return global::\u0004.\u0011.\u0001(\u0002, \u0003, \u0004, \u0006, \u0005.EffectiveType.ToString());
		}

		// Token: 0x06002FB5 RID: 12213 RVA: 0x000B3F44 File Offset: 0x000B2144
		private static global::\u0004.\u0011.\u0002 \u0001(_IVariable \u0002, _ISignature \u0003, TypeCheckerVisitor \u0004, _IExpression \u0005, string \u0006)
		{
			global::\u0004.\u0011.\u0002 result = global::\u0004.\u0011.\u0002.\u0001;
			if (\u0002.GetFlag(VarFlag.Input) && (Operator.Function == \u0003.POUType || Operator.Method == \u0003.POUType))
			{
				\u0004.\u0001(\u0005, MessageId.Wrn_InvalidDefaultValue, new object[]
				{
					\u0006
				});
				result = global::\u0004.\u0011.\u0002.\u0002;
			}
			return result;
		}

		// Token: 0x06002FB6 RID: 12214 RVA: 0x000B3F8C File Offset: 0x000B218C
		private static void \u0001(_IVariable \u0002, _ISignature \u0003, TypeCheckerVisitor \u0004, ICompiledType3 \u0005, _IExpression \u0006, ref global::\u0004.\u0011.\u0002 \u0007)
		{
			\u0007 |= global::\u0004.\u0011.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x06002FB7 RID: 12215 RVA: 0x000B3FA0 File Offset: 0x000B21A0
		private static void \u0001(_IVariable \u0002, IScope5 \u0003, _ICompileContext \u0004, _ISignature \u0005, TypeCheckerVisitor \u0006, ICompiledType3 \u0007, _IExpression \u0008, ref global::\u0004.\u0011.\u0002 \u000E)
		{
			if (\u0007.EffectiveType.Class == TypeClass.Pointer)
			{
				if (!TypeTable.IsLikePointer(\u0008.Type, \u0004.PointerSize) && !(\u0008 is ILiteralExpression))
				{
					\u0006.AddError(\u0008, MessageId.Err_TypeMismatch, new object[]
					{
						\u0008.Type,
						\u0007.EffectiveType
					});
				}
				ILiteralValue literalValue = \u0002.Initial.Literal(\u0003);
				bool flag;
				if (literalValue == null)
				{
					flag = true;
				}
				else
				{
					int num;
					literalValue.GetInt(out num);
					flag = (num != 0 && -1 != num);
				}
				if (flag)
				{
					\u000E |= global::\u0004.\u0011.\u0001(\u0002, \u0005, \u0006, \u0007, \u0008);
				}
			}
		}

		// Token: 0x06002FB8 RID: 12216 RVA: 0x000B4044 File Offset: 0x000B2244
		private static void \u0001(_IVariable \u0002, IScope5 \u0003, _ISignature \u0004, TypeCheckerVisitor \u0005, ICompiledType3 \u0006, _IExpression \u0007, ICompiledType3 \u0008, ref global::\u0004.\u0011.\u0002 \u000E)
		{
			if (\u0008.Class == TypeClass.Userdef)
			{
				IUserdefType2 userdefType = (IUserdefType2)\u0008;
				if (userdefType.SignatureId == Helper.InvalidId)
				{
					ISignature[] array = \u0003.FindSignature(userdefType.NameExpression);
					if (array != null && array.Length == 1)
					{
						((_IUserdefType)userdefType).SignatureId = array[0].Id;
					}
				}
				\u000E |= global::\u0004.\u0011.\u0001(\u0002, \u0004, \u0005, \u0006, \u0007);
			}
		}

		// Token: 0x06002FB9 RID: 12217 RVA: 0x000B40B0 File Offset: 0x000B22B0
		private static void \u0002(_IVariable \u0002, IScope5 \u0003, _ICompileContext \u0004, _ISignature \u0005, TypeCheckerVisitor \u0006, ICompiledType3 \u0007, _IExpression \u0008, ref global::\u0004.\u0011.\u0002 \u000E)
		{
			if (TypeClass.Userdef == \u0002.CompiledType.Class && \u0008 is _ILiteralExpression)
			{
				_IUserdefType iuserdefType = (_IUserdefType)\u0002.CompiledType;
				ISignature signatureById = \u0004.GetSignatureById(iuserdefType.SignatureId);
				if (signatureById != null && signatureById.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
				{
					ILiteralValue literalValue = \u0002.Initial.Literal(\u0003);
					int num;
					if (literalValue != null && literalValue.GetInt(out num) && num != 0)
					{
						\u000E |= global::\u0004.\u0011.\u0001(\u0002, \u0005, \u0006, \u0007, \u0008);
					}
				}
			}
		}

		// Token: 0x06002FBA RID: 12218 RVA: 0x000B4130 File Offset: 0x000B2330
		private static void \u0001(_IVariable \u0002, _ICompileContext \u0003, _ISignature \u0004, TypeCheckerVisitor \u0005, _IExpression \u0006)
		{
			if (Helper.InvalidId != \u0004.ParentSignatureId && \u0002.GetFlag(VarFlag.Output))
			{
				ISignature signatureById = \u0003.GetSignatureById(\u0004.ParentSignatureId);
				if (Operator.Interface == signatureById.POUType || signatureById.GetFlag(SignatureFlag.Abstract))
				{
					\u0005.\u0001(\u0006, MessageId.Wrn_ObsoleteOutputInAbstractMethod, Array.Empty<object>());
				}
			}
		}

		// Token: 0x06002FBB RID: 12219 RVA: 0x000B4190 File Offset: 0x000B2390
		private static void \u0001(_IVariable \u0002, _ISignature \u0003, TypeCheckerVisitor \u0004, _IExpression \u0005, ref global::\u0004.\u0011.\u0002 \u0006)
		{
			if (\u0002.GetFlag(VarFlag.Lazy))
			{
				\u0006 |= global::\u0004.\u0011.\u0001(\u0002, \u0003, \u0004, \u0005, "__LAZY");
			}
		}

		// Token: 0x06002FBC RID: 12220 RVA: 0x000B41B8 File Offset: 0x000B23B8
		private static void \u0001(_IVariable \u0002, IScope5 \u0003, _ISignature \u0004, TypeCheckerVisitor \u0005, _IExpression \u0006, ref global::\u0004.\u0011.\u0002 \u0007)
		{
			if (\u0002.GetFlag(VarFlag.Input) && (Operator.Function == \u0004.POUType || Operator.Method == \u0004.POUType) && !\u0006.IsConstant(\u0003, true))
			{
				\u0005.\u0001(\u0006, MessageId.Wrn_DefaultValueNotConstant, Array.Empty<object>());
				\u0002.SetInitial(null);
				\u0007 |= global::\u0004.\u0011.\u0002.\u0003;
			}
		}

		// Token: 0x06002FBD RID: 12221 RVA: 0x000B4210 File Offset: 0x000B2410
		private static void \u0002(_IVariable \u0002, _ICompileContext \u0003, _ISignature \u0004, TypeCheckerVisitor \u0005, _IExpression \u0006)
		{
			if (\u0002.GetFlag(VarFlag.Input) && Operator.Function == \u0004.POUType && \u0003.GetCompiledPOUById(\u0004.Id).GetFlag(CompiledPOUFlags.TopLevel) && !\u001F.\u0004.\u0001(\u0004))
			{
				\u0005.\u0001(\u0006, MessageId.Wrn_DefaultValueTopLevel, Array.Empty<object>());
			}
		}

		// Token: 0x06002FBE RID: 12222 RVA: 0x000B4260 File Offset: 0x000B2460
		public static void \u0001(_IVariable \u0002, IScope \u0003, _IExpression \u0004, _ISignature \u0005, TypeCheckerVisitor \u0006)
		{
			if (!\u0002.HasFlag(VarFlag.AllocateInInstance))
			{
				return;
			}
			foreach (_IVariableExpression exp in \u001D.\u000E.\u0001(\u0004, \u0003, \u0005))
			{
				\u0006.AddError(exp, MessageId.Err_InvalidInitialisationForVarInst, Array.Empty<object>());
			}
		}

		// Token: 0x06002FBF RID: 12223 RVA: 0x000B42CC File Offset: 0x000B24CC
		private static void \u0001(_IVariable \u0002, _ISignature \u0003, ErrorVisitor \u0004)
		{
			if (\u0003.POUType != Operator.FunctionBlock || !\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_IMPLICIT_INST_VAR))
			{
				\u0003.AddMessages(\u0004.Messages);
			}
		}

		// Token: 0x06002FC0 RID: 12224 RVA: 0x000B42F4 File Offset: 0x000B24F4
		internal static void \u0001(_IVariable \u0002, IScope5 \u0003, _ICompileContext \u0004, _ISignature \u0005)
		{
			TypeCheckerVisitor typeCheckerVisitor = new TypeCheckerVisitor(\u0003 as global::\u0007.\u0005, \u0004)
			{
				CheckingInitialValue = true,
				InitialValueContextSignature = \u0005
			};
			if (\u0002.MessageGuid != Guid.Empty)
			{
				typeCheckerVisitor.MessageGuid = \u0002.MessageGuid;
			}
			ErrorVisitor errorVisitor = new ErrorVisitor();
			global::\u0014.\u0013.\u0001(\u0002, errorVisitor);
			ICompiledType3 compiledType = (ICompiledType3)\u0002.CompiledType;
			_IImplicitReferenceType iimplicitReferenceType = compiledType as _IImplicitReferenceType;
			if (iimplicitReferenceType != null)
			{
				compiledType = (ICompiledType3)iimplicitReferenceType.BaseType;
			}
			_IExpression iexpression = \u0002.Initial as _IExpression;
			bool flag = true;
			if (iexpression == null)
			{
				return;
			}
			global::\u0004.\u0011.\u0001(\u0002, \u0003, typeCheckerVisitor, errorVisitor, ref flag);
			if (!flag)
			{
				global::\u0004.\u0011.\u0001(\u0002, \u0005, errorVisitor);
				return;
			}
			iexpression.Accept(typeCheckerVisitor);
			global::\u0004.\u0011.\u0001(typeCheckerVisitor, \u0002, \u0003);
			global::\u0004.\u0011.\u0001(\u0005, \u0002, \u0003);
			if (iexpression.Type == null)
			{
				typeCheckerVisitor.AddError(\u0002.Initial as _IExpression, MessageId.Err_TypeMismatch, new object[]
				{
					global::\u0003.\u0006.\u0001(MessageId.Err_UnknownType, new object[]
					{
						\u0002.Initial.ToString()
					}),
					compiledType.EffectiveType
				});
				iexpression.Accept(errorVisitor);
				global::\u0004.\u0011.\u0001(\u0002, \u0005, errorVisitor);
				return;
			}
			global::\u0004.\u0011.\u0001(\u0002, \u0003, \u0005, compiledType, iexpression);
			global::\u0004.\u0011.\u0001(\u0002, \u0005, \u0003);
			global::\u0004.\u0011.\u0002 u = global::\u0004.\u0011.\u0002.\u0001;
			if (!global::\u0004.\u0011.\u0001(\u0002, \u0003, \u0005, typeCheckerVisitor, compiledType, iexpression, ref u))
			{
				global::\u0004.\u0011.\u0001(\u0002, \u0003, \u0004, \u0005, typeCheckerVisitor, compiledType, iexpression, ref u);
				ICompiledType3 compiledType2 = (ICompiledType3)iexpression.Type;
				compiledType2 = (compiledType2.EffectiveType as ICompiledType3);
				global::\u0004.\u0011.\u0001(\u0002, \u0003, \u0005, typeCheckerVisitor, compiledType, iexpression, compiledType2, ref u);
				global::\u0004.\u0011.\u0002(\u0002, \u0003, \u0004, \u0005, typeCheckerVisitor, compiledType, iexpression, ref u);
				global::\u0004.\u0011.\u0001(\u0002, \u0004, \u0005, typeCheckerVisitor, iexpression);
				global::\u0004.\u0011.\u0001(\u0002, \u0003, compiledType2, iexpression, typeCheckerVisitor);
				if (compiledType.EffectiveType.Class != TypeClass.Array || iexpression.Type.Class != TypeClass.Array)
				{
					TypeCheckerVisitor.\u0001(typeCheckerVisitor, iexpression, compiledType2, compiledType.EffectiveType, \u0003, ref iexpression);
				}
				else
				{
					global::\u0004.\u0011.\u0001(\u0002, \u0005, typeCheckerVisitor, compiledType, iexpression, ref u);
				}
				\u0002.Initial = iexpression;
				if ((global::\u0004.\u0011.\u0002.\u0002 & u) != global::\u0004.\u0011.\u0002.\u0001)
				{
					\u0002.Initial = null;
				}
				global::\u0004.\u0011.\u0001(\u0002, \u0003, iexpression);
				global::\u0004.\u0011.\u0001(\u0002, \u0003, \u0005, iexpression);
				global::\u0004.\u0011.\u0001(\u0002, \u0005, typeCheckerVisitor, iexpression, ref u);
				global::\u0004.\u0011.\u0001(\u0002, \u0003, iexpression, \u0005, typeCheckerVisitor);
			}
			if ((u & global::\u0004.\u0011.\u0002.\u0002) == global::\u0004.\u0011.\u0002.\u0001)
			{
				global::\u0004.\u0011.\u0001(\u0002, \u0003, \u0005, typeCheckerVisitor, iexpression, ref u);
			}
			if ((u & global::\u0004.\u0011.\u0002.\u0002) == global::\u0004.\u0011.\u0002.\u0001)
			{
				global::\u0004.\u0011.\u0002(\u0002, \u0004, \u0005, typeCheckerVisitor, iexpression);
			}
			iexpression.Accept(errorVisitor);
			global::\u0004.\u0011.\u0001(\u0002, \u0005, errorVisitor);
		}

		// Token: 0x06002FC1 RID: 12225 RVA: 0x000B4548 File Offset: 0x000B2748
		private static void \u0001(_IVariable \u0002, IScope5 \u0003, _IExpression \u0004)
		{
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_BLOBINIT) || \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_BLOBINITCONST))
			{
				global::\u0011.\u000E ivisit = new global::\u0011.\u000E(\u0003);
				\u0004.Accept(ivisit);
			}
		}

		// Token: 0x06002FC2 RID: 12226 RVA: 0x000B4580 File Offset: 0x000B2780
		private static void \u0001(_IVariable \u0002, IScope5 \u0003, _ISignature \u0004, ICompiledType3 \u0005, _IExpression \u0006)
		{
			if (!\u0002.HasFlag(VarFlag.Constant) && !\u0002.HasFlag(VarFlag.ReplacedConstant))
			{
				return;
			}
			bool flag = \u0006.IsConstant(\u0003, true) && !global::\u0004.\u0011.\u0001(\u0002, \u0003);
			if (\u0002.HasFlag(VarFlag.ReplacedConstant) && !flag)
			{
				\u0004.\u0001(\u0002.SourcePosition, Severity.Error, MessageId.Err_NoConstantInitialisationForValue, new object[]
				{
					\u0002.OrgName
				});
				return;
			}
			if (\u0002.HasFlag(VarFlag.Constant) && !TypeTable.IsBlock(\u0005.EffectiveType.Class) && !flag)
			{
				\u0004.\u0001(\u0002.SourcePosition, Severity.Error, MessageId.Err_NoConstantInitialisationForValue, new object[]
				{
					\u0002.OrgName
				});
			}
		}

		// Token: 0x06002FC3 RID: 12227 RVA: 0x000B4630 File Offset: 0x000B2830
		private static bool \u0001(_IVariable \u0002, IScope5 \u0003)
		{
			_IOperatorExpressionWithDetailedLiteralCheck ioperatorExpressionWithDetailedLiteralCheck = \u0002.Initial as _IOperatorExpressionWithDetailedLiteralCheck;
			if (ioperatorExpressionWithDetailedLiteralCheck == null)
			{
				return false;
			}
			bool flag;
			ILiteralValue literalValue = ioperatorExpressionWithDetailedLiteralCheck.Literal(\u0003, true, out flag);
			return literalValue == null && flag;
		}

		// Token: 0x06002FC4 RID: 12228 RVA: 0x000B4660 File Offset: 0x000B2860
		private static void \u0001(_IVariable \u0002, _ISignature \u0003, IScope5 \u0004)
		{
			if (\u0002.HasFlag(VarFlag.Static) && \u0002._Initial != null && !VarStatInitValueChecker.IsVarStatInit(\u0002._Initial, \u0004))
			{
				\u0003.\u0001(\u0002.SourcePosition, Severity.Error, MessageId.Err_NoStaticVariableInitialisationForValue, new object[]
				{
					\u0002.OrgName
				});
			}
		}

		// Token: 0x06002FC5 RID: 12229 RVA: 0x000B46B4 File Offset: 0x000B28B4
		private static void \u0001(_IVariable \u0002, IScope5 \u0003, _ISignature \u0004, _IExpression \u0005)
		{
			if (\u0002.HasFlag(VarFlag.LocalPersistent) && !\u0005.IsConstant(\u0003, false))
			{
				Severity severity = Messages.\u0001(\u0002, MessageId.Wrn_OnlyConstantInitialValueForMappedPersistentVar);
				\u0004.AddMessage(\u0002._SourcePosition, severity, MessageId.Wrn_OnlyConstantInitialValueForMappedPersistentVar, Array.Empty<object>());
			}
		}

		// Token: 0x06002FC6 RID: 12230 RVA: 0x000B4700 File Offset: 0x000B2900
		private static _IUserdefType \u0001(ICompiledType3 \u0002)
		{
			_IUserdefType iuserdefType = \u0002 as _IUserdefType;
			if (iuserdefType != null)
			{
				return iuserdefType;
			}
			_IArrayType iarrayType = \u0002 as _IArrayType;
			if (iarrayType != null)
			{
				_IUserdefType iuserdefType2 = iarrayType.BaseType as _IUserdefType;
				if (iuserdefType2 != null)
				{
					return iuserdefType2;
				}
			}
			return null;
		}

		// Token: 0x06002FC7 RID: 12231 RVA: 0x000B4738 File Offset: 0x000B2938
		private static void \u0001(IVariable \u0002, IScope5 \u0003, ICompiledType3 \u0004, _IExpression \u0005, TypeCheckerVisitor \u0006)
		{
			_IUserdefType iuserdefType = global::\u0004.\u0011.\u0001(\u0004);
			if (iuserdefType != null && iuserdefType.SignatureId != Helper.InvalidId)
			{
				ISignature signature = \u0003[iuserdefType.SignatureId];
				if (signature != null && (signature.HasAttribute(CompileAttributes.ATTRIBUTE_NOASSIGN) || signature.HasAttribute(CompileAttributes.ATTRIBUTE_NOASSIGN_WARNING)) && !(\u0005 is IStructureInitialization) && !(\u0005 is IArrayInitialization))
				{
					bool flag = false;
					if (\u0002.Type != null && \u0002.Type.Class == TypeClass.Userdef)
					{
						_IUserdefType iuserdefType2 = (_IUserdefType)\u0002.Type;
						ISignature signature2 = \u0003[iuserdefType2.SignatureId];
						if (signature2 != null && signature2.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
						{
							flag = true;
						}
					}
					if (!flag)
					{
						if (signature.HasAttribute(CompileAttributes.ATTRIBUTE_NOASSIGN))
						{
							\u0006.AddError(\u0005, MessageId.Err_NoAssign, new object[]
							{
								signature.OrgName
							});
							return;
						}
						if (signature.HasAttribute(CompileAttributes.ATTRIBUTE_NOASSIGN_WARNING))
						{
							\u0006.\u0001(\u0005, MessageId.Err_NoAssign, new object[]
							{
								signature.OrgName
							});
						}
					}
				}
			}
		}

		// Token: 0x06002FC8 RID: 12232 RVA: 0x000B4848 File Offset: 0x000B2A48
		internal static void \u0001(_ISignature \u0002, IScope5 \u0003, _IVariable \u0004, Dictionary<long, long> \u0005)
		{
			if (\u0002.GetFlag(SignatureFlag.Enum))
			{
				IntegerUnion integerUnion = new IntegerUnion
				{
					m_long = 0L
				};
				if (\u0004.Initial != null)
				{
					ILiteralValue literalValue = ((_IExpression)\u0004.Initial).Literal(\u0003, true);
					if (literalValue == null)
					{
						\u0002.\u0001(\u0004.SourcePosition, Severity.Error, MessageId.Err_NoValidEnumInit, new object[]
						{
							\u0004.Initial.ToString()
						});
					}
					else if (literalValue.KindOf == KindOfLiteral.SignedInteger)
					{
						integerUnion.m_long = literalValue.SignedLong;
					}
					else if (literalValue.KindOf == KindOfLiteral.UnsignedInteger)
					{
						integerUnion.m_ulong = literalValue.UnsignedLong;
					}
					else
					{
						\u0002.\u0001(\u0004.SourcePosition, Severity.Error, MessageId.Err_NoValidEnumInit, new object[]
						{
							\u0004.Initial.ToString()
						});
					}
				}
				if (\u0005.ContainsKey(integerUnion.m_long))
				{
					\u0002.\u0001(\u0004.SourcePosition, Messages.\u0001(\u0004, MessageId.Wrn_EnumValueDuplicate), MessageId.Wrn_EnumValueDuplicate, new object[]
					{
						integerUnion.m_long
					});
					return;
				}
				\u0005.Add(integerUnion.m_long, integerUnion.m_long);
			}
		}

		// Token: 0x0200031D RID: 797
		private sealed class \u0001 : EmptyVisitor351900
		{
			// Token: 0x06002FC9 RID: 12233 RVA: 0x000B4958 File Offset: 0x000B2B58
			private \u0001(_ISignature \u0008\u0005, IScope5 \u009B\u0002)
			{
				this.SignatureToCheck = \u0008\u0005;
				this.Scope = \u009B\u0002;
			}

			// Token: 0x170007EE RID: 2030
			// (get) Token: 0x06002FCA RID: 12234 RVA: 0x000B4970 File Offset: 0x000B2B70
			// (set) Token: 0x06002FCB RID: 12235 RVA: 0x000B4978 File Offset: 0x000B2B78
			private IScope5 Scope { get; set; }

			// Token: 0x170007EF RID: 2031
			// (get) Token: 0x06002FCC RID: 12236 RVA: 0x000B4984 File Offset: 0x000B2B84
			// (set) Token: 0x06002FCD RID: 12237 RVA: 0x000B498C File Offset: 0x000B2B8C
			private _ISignature SignatureToCheck { get; set; }

			// Token: 0x06002FCE RID: 12238 RVA: 0x000B4998 File Offset: 0x000B2B98
			public static void \u0001(_ISignature \u0002, _IVariable \u0003, IScope5 \u0004)
			{
				if (\u0002 == null || \u0003 == null || \u0004 == null)
				{
					return;
				}
				if (\u0003.Initial != null)
				{
					global::\u0004.\u0011.\u0001 ivisit = new global::\u0004.\u0011.\u0001(\u0002, \u0004);
					\u0003._Initial.Accept(ivisit);
				}
			}

			// Token: 0x06002FCF RID: 12239 RVA: 0x000B49CC File Offset: 0x000B2BCC
			public override void visit(_ICallExpression call)
			{
				_IUserdefType iuserdefType = call.Callee.Type as _IUserdefType;
				if (iuserdefType != null)
				{
					ISignature signature = iuserdefType.GetSignature(this.Scope);
					if (signature != null)
					{
						this.\u0001(call, signature);
					}
				}
			}

			// Token: 0x06002FD0 RID: 12240 RVA: 0x000B4A08 File Offset: 0x000B2C08
			private void \u0001(_ICallExpression \u0002, ISignature \u0003)
			{
				ISignature signature = this.SignatureToCheck;
				if (signature.ParentSignatureId != Helper.InvalidId)
				{
					signature = this.Scope[signature.ParentSignatureId];
				}
				if (\u0003 != null && \u0003.GetFlag(SignatureFlag.Private) && signature != null && signature.Id != \u0003.ParentSignatureId && signature.Id != \u0003.Id)
				{
					ISignature signature2 = this.Scope[\u0003.ParentSignatureId];
					string text = (signature2 == null) ? "???" : signature2.OrgName;
					this.\u0001(\u0002, MessageId.Err_CallOfPrivateMethod, new object[]
					{
						text,
						\u0003.OrgName
					});
				}
				if (\u0003 != null && \u0003.GetFlag(SignatureFlag.Protected))
				{
					bool flag = false;
					for (ISignature signature3 = signature; signature3 != null; signature3 = this.Scope[signature3.BaseSignatureId])
					{
						if (signature3.Id == \u0003.ParentSignatureId || signature3.Id == \u0003.Id)
						{
							flag = true;
						}
					}
					if (!flag)
					{
						ISignature signature4 = this.Scope[\u0003.ParentSignatureId];
						string text2 = (signature4 == null) ? "???" : signature4.OrgName;
						this.\u0001(\u0002, MessageId.Err_CallOfProtectedMethod, new object[]
						{
							text2,
							\u0003.OrgName
						});
					}
				}
			}

			// Token: 0x06002FD1 RID: 12241 RVA: 0x000B4B54 File Offset: 0x000B2D54
			private void \u0001(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
			{
				string format = \u0018.\u0001(\u0003);
				\u0002.AddWarning(string.Format(format, \u0004), \u0003);
			}

			// Token: 0x0400091B RID: 2331
			[CompilerGenerated]
			private IScope5 \u0001;

			// Token: 0x0400091C RID: 2332
			[CompilerGenerated]
			private _ISignature \u0001;
		}

		// Token: 0x0200031E RID: 798
		[Flags]
		private enum \u0002
		{
			// Token: 0x0400091E RID: 2334
			\u0001 = 0,
			// Token: 0x0400091F RID: 2335
			\u0002 = 1,
			// Token: 0x04000920 RID: 2336
			\u0003 = 2
		}
	}
}
