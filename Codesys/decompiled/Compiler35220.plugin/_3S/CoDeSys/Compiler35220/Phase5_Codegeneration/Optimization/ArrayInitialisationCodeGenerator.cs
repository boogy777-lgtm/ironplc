using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u0007;
using \u000E;
using \u0014;
using \u0019;
using \u001A;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x0200028F RID: 655
	public class ArrayInitialisationCodeGenerator
	{
		// Token: 0x06002941 RID: 10561 RVA: 0x0008FAB8 File Offset: 0x0008DCB8
		internal ArrayInitialisationCodeGenerator(StructAndArrayInitReplacer structAndArrayInitReplacer, global::\u000E.\u0011 context, bool emulateVectors)
		{
			this.Context = context;
			this.StructAndArrayInitReplacer = structAndArrayInitReplacer;
			this.\u0001 = emulateVectors;
		}

		// Token: 0x17000743 RID: 1859
		// (get) Token: 0x06002942 RID: 10562 RVA: 0x0008FAD8 File Offset: 0x0008DCD8
		private _ICompileContext ComCon
		{
			get
			{
				return this.Context.Comcon;
			}
		}

		// Token: 0x17000744 RID: 1860
		// (get) Token: 0x06002943 RID: 10563 RVA: 0x0008FAF4 File Offset: 0x0008DCF4
		private IScope5 _Scope
		{
			get
			{
				return this.Context._Scope;
			}
		}

		// Token: 0x17000745 RID: 1861
		// (get) Token: 0x06002944 RID: 10564 RVA: 0x0008FB10 File Offset: 0x0008DD10
		private global::\u000E.\u0011 Context { get; }

		// Token: 0x17000746 RID: 1862
		// (get) Token: 0x06002945 RID: 10565 RVA: 0x0008FB18 File Offset: 0x0008DD18
		private StructAndArrayInitReplacer StructAndArrayInitReplacer { get; }

		// Token: 0x17000747 RID: 1863
		// (get) Token: 0x06002946 RID: 10566 RVA: 0x0008FB20 File Offset: 0x0008DD20
		private Codegeneration Codegen
		{
			get
			{
				return this.Context.Codegeneration;
			}
		}

		// Token: 0x06002947 RID: 10567 RVA: 0x0008FB3C File Offset: 0x0008DD3C
		public static string GetPOUUniqueLabel(string stLabelBase)
		{
			return string.Format(stLabelBase, ArrayInitialisationCodeGenerator.\u0001++);
		}

		// Token: 0x06002948 RID: 10568 RVA: 0x0008FB58 File Offset: 0x0008DD58
		private _IStatement \u0001(_IAssignmentExpression \u0002)
		{
			_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
			IList<_IExpression> initValues = ((_IArrayInitialization)\u0002._RValue)._InitValues;
			for (int i = 0; i < initValues.Count; i++)
			{
				_IIndexAccessExpression iindexAccessExpression = global::\u0019.\u0003.\u0001();
				iindexAccessExpression.AddAccess(global::\u0019.\u0003.\u0001((long)i));
				iindexAccessExpression._Var = (\u0002._LValue.Duplicate() as _IExpression);
				_IAssignmentExpression iassignmentExpression = global::\u0019.\u0003.\u0001(iindexAccessExpression);
				iassignmentExpression._RValue = (initValues[i].Duplicate() as _IExpression);
				_IExpressionStatement sm = global::\u0019.\u0003.\u0001(iassignmentExpression, Token.Empty);
				isequenceStatement.Add(sm);
			}
			this.\u0001(isequenceStatement);
			return isequenceStatement;
		}

		// Token: 0x06002949 RID: 10569 RVA: 0x0008FBEC File Offset: 0x0008DDEC
		private void \u0001(_ISequenceStatement \u0002)
		{
			_ISequenceStatement statement = this.Context.Generator.\u0001<_ISequenceStatement>(\u0002, this._Scope, this.Context.CompiledPOU);
			this.StructAndArrayInitReplacer.ReplaceInCode(statement);
		}

		// Token: 0x0600294A RID: 10570 RVA: 0x0008FC30 File Offset: 0x0008DE30
		public _IStatement HandleArrayInitialisation(_IAssignmentExpression assign, _ISignature sign)
		{
			if (assign._LValue.GetVariable(this._Scope).HasAttribute("no_default_init"))
			{
				return this.\u0001(assign);
			}
			_IArrayInitialization iarrayInitialization = (_IArrayInitialization)assign._RValue;
			_IArrayType iarrayType = (_IArrayType)assign._LValue.Type.DeRefType;
			int count = iarrayType._Dimensions.Count;
			_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
			int[] array = new int[count];
			int[] array2 = new int[count];
			int[] array3 = new int[count];
			for (int i = 0; i < count; i++)
			{
				bool u;
				array[i] = iarrayType._Dimensions[i].LowerBorderInt(out u, this._Scope);
				Debug.\u0001(u);
				array2[i] = iarrayType._Dimensions[i].UpperBorderInt(out u, this._Scope);
				Debug.\u0001(u);
				array3[i] = array[i];
			}
			bool flag;
			int u2 = this.\u0001(isequenceStatement, assign, sign, ref array, ref array2, ref array3, out flag);
			this.\u0001(isequenceStatement, u2, assign, ref array, ref array2, ref array3, ref flag);
			if (!flag && !iarrayInitialization.DefaultInitializationDone)
			{
				this.\u0001(isequenceStatement, assign, sign, ref array, ref array2, ref array3, out flag);
			}
			this.\u0001(isequenceStatement, assign);
			this.StructAndArrayInitReplacer.CurrentArrayIndex += count;
			bool doCallAfterInitAttribute = this.StructAndArrayInitReplacer.DoCallAfterInitAttribute;
			this.StructAndArrayInitReplacer.DoCallAfterInitAttribute = false;
			this.\u0001(isequenceStatement);
			this.StructAndArrayInitReplacer.DoCallAfterInitAttribute = doCallAfterInitAttribute;
			this.StructAndArrayInitReplacer.CurrentArrayIndex -= count;
			return isequenceStatement;
		}

		// Token: 0x0600294B RID: 10571 RVA: 0x0008FDB4 File Offset: 0x0008DFB4
		private void \u0001(_ISequenceStatement \u0002, _IAssignmentExpression \u0003)
		{
			_IArrayType iarrayType = (_IArrayType)\u0003._LValue.Type.DeRefType;
			_ISequenceStatement u;
			this.\u0001(\u0003, iarrayType, out u);
			this.\u0001(\u0002, u, iarrayType);
		}

		// Token: 0x0600294C RID: 10572 RVA: 0x0008FDEC File Offset: 0x0008DFEC
		private void \u0001(_ISequenceStatement \u0002, _ISequenceStatement \u0003, _IArrayType \u0004)
		{
			if (\u0003 != null)
			{
				int count = \u0004._Dimensions.Count;
				int[] array = new int[count];
				int[] array2 = new int[count];
				int[] array3 = new int[count];
				for (int i = 0; i < count; i++)
				{
					bool u;
					array[i] = \u0004._Dimensions[i].LowerBorderInt(out u, this._Scope);
					Debug.\u0001(u);
					array2[i] = \u0004._Dimensions[i].UpperBorderInt(out u, this._Scope);
					Debug.\u0001(u);
					array3[i] = array[i];
				}
				bool flag;
				this.\u0001(\u0003, \u0002, -1, ref array, ref array2, ref array3, out flag, this.StructAndArrayInitReplacer.CurrentArrayIndex);
			}
		}

		// Token: 0x0600294D RID: 10573 RVA: 0x0008FEA0 File Offset: 0x0008E0A0
		private void \u0001(_IAssignmentExpression \u0002, _IArrayType \u0003, out _ISequenceStatement \u0004)
		{
			\u0004 = null;
			_IUserdefType iuserdefType = \u0003.BaseType as _IUserdefType;
			if (iuserdefType == null)
			{
				return;
			}
			ISignature signature = iuserdefType.GetSignature(this._Scope);
			while (signature != null && signature.HasAttribute(CompileAttributes.ATTRIBUTE_CALL_AFTER_INIT))
			{
				foreach (ISignature signature2 in signature.SubSignatures.Where(new Func<ISignature, bool>(ArrayInitialisationCodeGenerator.<>c.<>9.\u0001)))
				{
					if (\u0004 == null)
					{
						\u0004 = global::\u0019.\u0003.\u0001();
					}
					_ICallExpression u = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(this.\u0001(\u0003, \u0002._LValue.Duplicate() as _IExpression, this.StructAndArrayInitReplacer.CurrentArrayIndex), global::\u0019.\u0003.\u0001(signature2.OrgName)));
					\u0004.Add(global::\u0019.\u0003.\u0001(u));
				}
				signature = this._Scope[signature.BaseSignatureId];
			}
		}

		// Token: 0x0600294E RID: 10574 RVA: 0x0008FFA8 File Offset: 0x0008E1A8
		private void \u0001(_IStatement \u0002, _ISequenceStatement \u0003, int \u0004, ref int[] \u0005, ref int[] \u0006, ref int[] \u0007, out bool \u0008, int \u000E)
		{
			\u0008 = true;
			int num = \u0005.Length;
			int[] array = new int[num];
			bool flag = ArrayInitialisationCodeGenerator.\u0001(\u0004, \u0005, \u0006, \u0007, array, num);
			if (\u0004 > 0)
			{
				this.\u0001(\u0004 - 1, ref array, ref \u0006, ref \u0007, out \u0008);
			}
			bool u = !\u0008 && num > 1;
			string u2 = ArrayInitialisationCodeGenerator.\u0001(\u0003, \u0005, \u000E, flag);
			_IForStatement sm;
			_IForStatement iforStatement = this.\u0001(\u0005, \u0006, \u0007, \u000E, num, array, out sm);
			_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
			if (flag)
			{
				isequenceStatement.Add(global::\u0019.\u0003.\u0001(u2));
			}
			isequenceStatement.Add(\u0002);
			ArrayInitialisationCodeGenerator.\u0001(\u0004, \u000E, u, num, array, isequenceStatement);
			if (iforStatement != null)
			{
				iforStatement._Controlled = isequenceStatement;
			}
			if (\u0004 > 0)
			{
				array.CopyTo(\u0005, 0);
			}
			\u0003.Add(sm);
			this.\u0001(ref \u0005, ref \u0006, ref \u0007, out \u0008);
		}

		// Token: 0x0600294F RID: 10575 RVA: 0x00090080 File Offset: 0x0008E280
		private static void \u0001(int \u0002, int \u0003, bool \u0004, int \u0005, int[] \u0006, _ISequenceStatement \u0007)
		{
			if (\u0002 > 0 && \u0004)
			{
				_IIfStatement iifStatement = global::\u0019.\u0003.\u0001();
				_IOperatorExpression ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.And);
				for (int i = 0; i < \u0005; i++)
				{
					_IVariableExpression u = global::\u0019.\u0003.\u0001(IdentifierConstants.GetImplicitIndexVariable(i + \u0003));
					_ILiteralExpression u2 = global::\u0019.\u0003.\u0001((long)\u0006[i]);
					_IOperatorExpression exp = global::\u0019.\u0003.\u0001(Operator.Equal, u, u2);
					if (ioperatorExpression._OperandsList.Count == 2)
					{
						_IOperatorExpression ioperatorExpression2 = global::\u0019.\u0003.\u0001(Operator.And);
						ioperatorExpression2.AddOperand(ioperatorExpression);
						ioperatorExpression2.AddOperand(exp);
						ioperatorExpression = ioperatorExpression2;
					}
					else
					{
						ioperatorExpression.AddOperand(exp);
					}
				}
				iifStatement._Condition = ioperatorExpression;
				_IExitStatement sm = global::\u0019.\u0003.\u0001();
				_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
				isequenceStatement.Add(sm);
				iifStatement._IfThen = isequenceStatement;
				\u0007.Add(iifStatement);
			}
		}

		// Token: 0x06002950 RID: 10576 RVA: 0x0009013C File Offset: 0x0008E33C
		private _IForStatement \u0001(int[] \u0002, int[] \u0003, int[] \u0004, int \u0005, int \u0006, int[] \u0007, out _IForStatement \u0008)
		{
			_IForStatement iforStatement = null;
			\u0008 = null;
			if (\u0006 == 1)
			{
				_IVariableExpression ivariableExpression = global::\u0019.\u0003.\u0001(IdentifierConstants.GetImplicitIndexVariable(\u0005));
				_ILiteralExpression u = global::\u0019.\u0003.\u0001((long)\u0002[0]);
				iforStatement = global::\u0019.\u0003.\u0001();
				iforStatement._CounterStart = global::\u0019.\u0003.\u0001(ivariableExpression, u);
				iforStatement._UpperBound = global::\u0019.\u0003.\u0001((long)\u0007[0]);
				iforStatement._By = global::\u0019.\u0003.\u0001(1L);
				this.\u0001(iforStatement, ivariableExpression);
				\u0008 = iforStatement;
			}
			else
			{
				for (int i = 0; i < \u0002.Length; i++)
				{
					_IVariableExpression ivariableExpression2 = global::\u0019.\u0003.\u0001(IdentifierConstants.GetImplicitIndexVariable(i + \u0005));
					_ILiteralExpression u2 = global::\u0019.\u0003.\u0001((long)\u0004[i]);
					_IForStatement iforStatement2 = global::\u0019.\u0003.\u0001();
					iforStatement2._CounterStart = global::\u0019.\u0003.\u0001(ivariableExpression2, u2);
					iforStatement2._UpperBound = global::\u0019.\u0003.\u0001((long)\u0003[i]);
					iforStatement2._By = global::\u0019.\u0003.\u0001(1L);
					this.\u0001(iforStatement2, ivariableExpression2);
					if (iforStatement != null)
					{
						_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
						isequenceStatement.Add(iforStatement2);
						iforStatement._Controlled = isequenceStatement;
					}
					else
					{
						\u0008 = iforStatement2;
					}
					iforStatement = iforStatement2;
				}
			}
			return iforStatement;
		}

		// Token: 0x06002951 RID: 10577 RVA: 0x00090240 File Offset: 0x0008E440
		private static string \u0001(_ISequenceStatement \u0002, int[] \u0003, int \u0004, bool \u0005)
		{
			string text = null;
			if (\u0005)
			{
				for (int i = 0; i < \u0003.Length; i++)
				{
					_IVariableExpression u = global::\u0019.\u0003.\u0001(IdentifierConstants.GetImplicitIndexVariable(i + \u0004));
					_ILiteralExpression u2 = global::\u0019.\u0003.\u0001((long)\u0003[i]);
					\u0002.Add(global::\u0019.\u0003.\u0001(u, u2));
				}
				text = "Implicit__ArrayInitLabel__" + ArrayInitialisationCodeGenerator.\u0002++.ToString();
				\u0002.Add(global::\u0019.\u0003.\u0001(text));
			}
			return text;
		}

		// Token: 0x06002952 RID: 10578 RVA: 0x000902B4 File Offset: 0x0008E4B4
		private static bool \u0001(int \u0002, int[] \u0003, int[] \u0004, int[] \u0005, int[] \u0006, int \u0007)
		{
			if (\u0002 < 0)
			{
				\u0004.CopyTo(\u0006, 0);
			}
			else
			{
				\u0003.CopyTo(\u0006, 0);
			}
			bool result = false;
			if (\u0007 > 1)
			{
				for (int i = 0; i < \u0003.Length; i++)
				{
					if (\u0003[i] != \u0005[i])
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		// Token: 0x06002953 RID: 10579 RVA: 0x000902FC File Offset: 0x0008E4FC
		private _IIndexAccessExpression \u0001(ICompiledType \u0002, _IExpression \u0003, int \u0004)
		{
			if (\u0002.DeRefType.Class != TypeClass.Array)
			{
				return null;
			}
			IList<_IArrayDimension> dimensions = ((_IArrayType)\u0002)._Dimensions;
			List<IExpression> list = new List<IExpression>();
			for (int i = 0; i < dimensions.Count; i++)
			{
				list.Add(global::\u0019.\u0003.\u0001(IdentifierConstants.GetImplicitIndexVariable(\u0004++)));
			}
			return global::\u0019.\u0003.\u0001(\u0003, list);
		}

		// Token: 0x06002954 RID: 10580 RVA: 0x0009035C File Offset: 0x0008E55C
		private static _IIndexAccessExpression \u0001(ICompiledType \u0002, _IExpression \u0003, ref int[] \u0004, int \u0005)
		{
			if (\u0002.DeRefType.Class != TypeClass.Array)
			{
				return null;
			}
			IList<_IArrayDimension> dimensions = ((_IArrayType)\u0002)._Dimensions;
			List<IExpression> list = new List<IExpression>();
			for (int i = 0; i < dimensions.Count; i++)
			{
				list.Add(global::\u0019.\u0003.\u0001((long)\u0004[\u0005++]));
			}
			return global::\u0019.\u0003.\u0001(\u0003, list);
		}

		// Token: 0x06002955 RID: 10581 RVA: 0x000903BC File Offset: 0x0008E5BC
		private void \u0001(_IForStatement \u0002, _IVariableExpression \u0003)
		{
			_IOperatorExpression ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.Le);
			ioperatorExpression.AddOperand(\u0003.Duplicate() as _IExpression);
			ioperatorExpression.AddOperand(\u0002._UpperBound.Duplicate() as _IExpression);
			\u0002._Condition = ioperatorExpression;
			_IOperatorExpression ioperatorExpression2 = global::\u0019.\u0003.\u0001(Operator.Plus);
			ioperatorExpression2.AddOperand(\u0003.Duplicate() as _IExpression);
			ioperatorExpression2.AddOperand(\u0002._By.Duplicate() as _IExpression);
			_IAssignmentExpression iassignmentExpression = global::\u0019.\u0003.\u0001(\u0003.Duplicate() as _IExpression);
			iassignmentExpression._RValue = ioperatorExpression2;
			\u0002._Counter = iassignmentExpression;
		}

		// Token: 0x06002956 RID: 10582 RVA: 0x00090454 File Offset: 0x0008E654
		private _IExprement \u0001(_IVariable \u0002, bool \u0003, _IType \u0004, _IExpression \u0005, IScope5 \u0006, out bool \u0007, _ISignature \u0008, _ISignature \u000E, string \u000F, string \u0010, int \u0011, _ICompileContext \u0012)
		{
			if (this.\u0001 && \u0004.Class == TypeClass.__Vector)
			{
				return global::\u001A.\u0007.\u0001(\u0004, \u0006, \u0005, \u0012, \u0008, out \u0007);
			}
			return \u0080.\u001A.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, out \u0007, \u0008, \u000E, \u000F, \u0010, \u0011, \u0012);
		}

		// Token: 0x06002957 RID: 10583 RVA: 0x000904A0 File Offset: 0x0008E6A0
		private _IExprement \u0001(_IVariable \u0002, bool \u0003, _IType \u0004, _IExpression \u0005, IScope5 \u0006, out bool \u0007, _ISignature \u0008, _ISignature \u000E, _IExpression \u000F, _IExpression \u0010, int \u0011, _ICompileContext \u0012)
		{
			if (this.\u0001 && \u0004.Class == TypeClass.__Vector)
			{
				return global::\u001A.\u0007.\u0001(\u0004, \u0006, \u0005, \u0012, \u0008, out \u0007);
			}
			return \u0080.\u001A.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, out \u0007, \u0008, \u000E, \u000F, \u0010, \u0011, \u0012);
		}

		// Token: 0x06002958 RID: 10584 RVA: 0x000904EC File Offset: 0x0008E6EC
		private void \u0001(_ISequenceStatement \u0002, _IAssignmentExpression \u0003, _ISignature \u0004, ref int[] \u0005, ref int[] \u0006, ref int[] \u0007, out bool \u0008)
		{
			_IArrayType iarrayType = (_IArrayType)\u0003._LValue.Type.DeRefType;
			int count = iarrayType._Dimensions.Count;
			_IExpression iexpression = this.\u0001(iarrayType, \u0003._LValue.Duplicate() as _IExpression, this.StructAndArrayInitReplacer.CurrentArrayIndex);
			this.StructAndArrayInitReplacer.CurrentArrayIndex += count;
			IVariable[] array = this._Scope.FindVariable("__bInitRetains");
			bool flag = array != null && array.Length != 0;
			bool flag2;
			_IExprement iexprement;
			if (this._Scope.MethodSignature != null && (this._Scope.MethodSignature.Name == IdentifierConstants.GlobalInitName || this._Scope.MethodSignature.Name == "GLOBAL__INIT__X"))
			{
				iexprement = this.\u0001(null, false, iarrayType.BaseType as _IType, iexpression, this._Scope, out flag2, \u0004, null, "__bInitRetains", "__bInCopyCode", this.StructAndArrayInitReplacer.CurrentArrayIndex, this.ComCon);
			}
			else if ((this._Scope.LocalSignature != null && this._Scope.LocalSignature.POUType == Operator.Function) || (this._Scope.MethodSignature != null && this._Scope.MethodSignature.POUType == Operator.Method))
			{
				_IExpression u000F = global::\u0019.\u0003.\u0001(true);
				_IExpression u = global::\u0019.\u0003.\u0001(false);
				iexprement = this.\u0001(null, false, iarrayType.BaseType as _IType, iexpression, this._Scope, out flag2, \u0004, null, u000F, u, this.StructAndArrayInitReplacer.CurrentArrayIndex, this.ComCon);
			}
			else if (flag)
			{
				iexprement = this.\u0001(null, false, iarrayType.BaseType as _IType, iexpression, this._Scope, out flag2, \u0004, null, "__bInitRetains", "__bInCopyCode", this.StructAndArrayInitReplacer.CurrentArrayIndex, this.ComCon);
			}
			else
			{
				iexprement = this.\u0001(null, false, iarrayType.BaseType as _IType, iexpression, this._Scope, out flag2, \u0004, null, "bInitRetains", "bInCopyCode", this.StructAndArrayInitReplacer.CurrentArrayIndex, this.ComCon);
			}
			this.StructAndArrayInitReplacer.CurrentArrayIndex -= count;
			_IStatement u2;
			if (flag2)
			{
				Debug.\u0001(iexprement is _IExpression);
				u2 = global::\u0019.\u0003.\u0001(iexpression, iexprement as _IExpression);
			}
			else
			{
				Debug.\u0001(iexprement is _IStatement);
				u2 = (iexprement as _IStatement);
			}
			this.\u0001(u2, \u0002, -1, ref \u0005, ref \u0006, ref \u0007, out \u0008, this.StructAndArrayInitReplacer.CurrentArrayIndex);
		}

		// Token: 0x06002959 RID: 10585 RVA: 0x00090768 File Offset: 0x0008E968
		private void \u0001(_ISequenceStatement \u0002, int \u0003, _IAssignmentExpression \u0004, ref int[] \u0005, ref int[] \u0006, ref int[] \u0007, ref bool \u0008)
		{
			_IArrayInitialization iarrayInitialization = (_IArrayInitialization)\u0004._RValue;
			_IArrayType u = (_IArrayType)\u0004._LValue.Type.DeRefType;
			IList<_IExpression> initValues = iarrayInitialization._InitValues;
			for (int i = \u0003; i < initValues.Count; i++)
			{
				_IExpression iexpression = initValues[i];
				_IExpression u2 = iexpression.Duplicate() as _IExpression;
				_IMultipleIndexInitialization imultipleIndexInitialization = iexpression as _IMultipleIndexInitialization;
				if (imultipleIndexInitialization != null)
				{
					int u3;
					Debug.\u0001(imultipleIndexInitialization._Number.Literal(this._Scope, true).GetInt(out u3));
					u2 = imultipleIndexInitialization._Value;
					_IExpressionStatement u4 = global::\u0019.\u0003.\u0001(this.\u0001(u, \u0004._LValue.Duplicate() as _IExpression, this.StructAndArrayInitReplacer.CurrentArrayIndex), u2);
					this.\u0001(u4, \u0002, u3, ref \u0005, ref \u0006, ref \u0007, out \u0008, this.StructAndArrayInitReplacer.CurrentArrayIndex);
				}
				else
				{
					_IExpressionStatement sm = global::\u0019.\u0003.\u0001(ArrayInitialisationCodeGenerator.\u0001(u, \u0004._LValue.Duplicate() as _IExpression, ref \u0005, 0), u2);
					this.\u0001(ref \u0005, ref \u0006, ref \u0007, out \u0008);
					\u0002.Add(sm);
				}
			}
		}

		// Token: 0x0600295A RID: 10586 RVA: 0x0009087C File Offset: 0x0008EA7C
		public static bool CanPerformArrayInitMemCopy(IScope5 scope, _IVariable var)
		{
			int num;
			_ISignature isignature;
			return var != null && scope != null && ArrayInitialisationCodeGenerator.\u0001(scope, var, var.DataLocation, var._Initial, var.CompiledType as _IArrayType, out num, out isignature);
		}

		// Token: 0x0600295B RID: 10587 RVA: 0x000908B4 File Offset: 0x0008EAB4
		private static bool \u0001(IScope5 \u0002, _IVariable \u0003, IDataLocation \u0004, _IExpression \u0005, _IArrayType \u0006, out int \u0007, out _ISignature \u0008)
		{
			\u0007 = 0;
			\u0008 = null;
			if (\u0003 == null)
			{
				return false;
			}
			if (\u0006 == null)
			{
				return false;
			}
			if (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_NO_OPTIMIZED_ARRAY_INIT))
			{
				return false;
			}
			if (\u0004 == null)
			{
				return false;
			}
			if (\u0003.GetFlag(VarFlag.RelativeStack) || \u0003.GetFlag(VarFlag.RelativeInstance))
			{
				return false;
			}
			if (\u0006.BaseType.Class == TypeClass.Userdef || \u0006.BaseType.Class == TypeClass.Array || \u0006.BaseType.Class == TypeClass.String || \u0006.BaseType.Class == TypeClass.WString)
			{
				return false;
			}
			IList<_IExpression> initValues = ((_IArrayInitialization)\u0005)._InitValues;
			\u0007 = 0;
			while (\u0007 < initValues.Count && initValues[\u0007].Literal(\u0002) != null)
			{
				\u0007++;
			}
			return \u0007 >= 5;
		}

		// Token: 0x0600295C RID: 10588 RVA: 0x00090988 File Offset: 0x0008EB88
		private int \u0001(_ISequenceStatement \u0002, _IAssignmentExpression \u0003, _ISignature \u0004, ref int[] \u0005, ref int[] \u0006, ref int[] \u0007, out bool \u0008)
		{
			\u0008 = false;
			_IVariable ivariable = (_IVariable)\u0003._LValue.GetVariable(this._Scope);
			IDataLocation dataLocation = \u0003.LValue.DataLocation(this._Scope);
			_IArrayType iarrayType = (_IArrayType)\u0003._LValue.Type;
			int num;
			_ISignature isignature;
			if (!ArrayInitialisationCodeGenerator.\u0001(this._Scope, ivariable, dataLocation, \u0003._RValue, iarrayType, out num, out isignature))
			{
				return 0;
			}
			isignature = (this._Scope[\u0003._LValue.SignatureId] as _ISignature);
			if (isignature == null)
			{
				return 0;
			}
			this.\u0001(num, ref \u0005, ref \u0006, ref \u0007, out \u0008);
			string text = string.Format("__ArrayData__{0}__{1}__{2}", isignature.Id, ivariable.Id, dataLocation.Offset);
			_IArrayType iarrayType2 = global::\u0019.\u0003.\u0001(TypeTable.Byte);
			int num2 = iarrayType.BaseType.Size(this._Scope) * num;
			num2--;
			iarrayType2.AddDimension(global::\u0019.\u0003.\u0001(0L), global::\u0019.\u0003.\u0001((long)num2));
			bool flag = ivariable.GetFlag(VarFlag.OnlChangeInit);
			_ISignature isignature2;
			if (this.ComCon[text] != null && flag)
			{
				isignature2 = this.ComCon[text];
			}
			else
			{
				_ICompiledPOU cpou;
				isignature2 = this.\u0001(out cpou, num, \u0003, \u0004, isignature, ivariable, text);
				this.ComCon._GetCompiledPOUById(isignature2.Id);
				IScope5 scope = global::\u0007.\u0005.\u0001(this.ComCon, isignature2.Id);
				scope.LocalSignature = isignature2;
				global::\u0014.\u0013.\u0002(isignature2, scope, this.ComCon);
				this.ComCon.AddSignature(isignature2, null, null, true);
				this.ComCon.AddCompiledPOU(cpou, isignature2, null);
			}
			this.\u0001(isignature2, \u0004, isignature, ivariable, iarrayType2, dataLocation, \u0002);
			return num;
		}

		// Token: 0x0600295D RID: 10589 RVA: 0x00090B44 File Offset: 0x0008ED44
		private _ISignature \u0001(out _ICompiledPOU \u0002, int \u0003, _IAssignmentExpression \u0004, _ISignature \u0005, _ISignature \u0006, _IVariable \u0007, string \u0008)
		{
			_IArrayInitialization iarrayInitialization = (_IArrayInitialization)\u0004._RValue;
			_IArrayType u = (_IArrayType)\u0004._LValue.Type.DeRefType;
			ChunkedMemoryStream chunkedMemoryStream = new ChunkedMemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(chunkedMemoryStream);
			IList<_IExpression> initValues = iarrayInitialization._InitValues;
			this.\u0001(\u0003, initValues, u, binaryWriter);
			binaryWriter.Flush();
			binaryWriter.BaseStream.Flush();
			binaryWriter.BaseStream.Position = 0L;
			\u0002 = global::\u0019.\u0003.\u0001(\u0008);
			\u0002.SetFlag(CompiledPOUFlags.ToCompile | CompiledPOUFlags.Blob, true);
			\u0002.CompiledCode = global::\u0019.\u0003.\u0001(chunkedMemoryStream, \u0005.Id);
			_ICompiledPOU u3;
			bool u2 = this.\u0001(\u0002, \u0008, out u3);
			this.\u0001(\u0002, \u0006, \u0007, u3, u2);
			\u0002.SetParseTree(global::\u0019.\u0003.\u0001());
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendLine("VAR_GLOBAL");
			lstringBuilder.AppendLine("END_VAR");
			return ParserHelper.\u0001(\u0008, lstringBuilder.ToString(), true).CreateCompiledSignature(null, this.ComCon.HasByteSupport());
		}

		// Token: 0x0600295E RID: 10590 RVA: 0x00090C44 File Offset: 0x0008EE44
		private void \u0001(_ICompiledPOU \u0002, _ISignature \u0003, _IVariable \u0004, _ICompiledPOU \u0005, bool \u0006)
		{
			if (\u0005 != null && \u0006 && !this.ComCon.DataManager._MemorySettings.OnlineChangeInOwnSegment)
			{
				DataSegmentFlags u = DataSegmentFlags.Code;
				if (this.ComCon.DataManager.HasConstantSegment)
				{
					u = DataSegmentFlags.Constant;
				}
				\u0002.CompiledCode = \u0005.CompiledCode;
				MemoryCompiler.\u0002(this.ComCon.DataManager, \u0002.CompiledCode.Location.Area, \u0002.CompiledCode.Location.Offset, \u0002.CompiledCode.CodeSize, u);
				return;
			}
			ushort u2 = 0;
			int u3 = 0;
			DataSegmentFlags u4 = DataSegmentFlags.Code;
			if (this.ComCon.DataManager.HasConstantSegment)
			{
				u4 = DataSegmentFlags.Constant;
			}
			if (this.ComCon.DataManager._MemorySettings.OnlineChangeInOwnSegment)
			{
				u4 = DataSegmentFlags.Code;
			}
			if (!MemoryCompiler.\u0003(this.ComCon.DataManager, ref u2, ref u3, this.ComCon.DataManager._MemorySettings.PackMode, \u0002.CompiledCode.CodeSize, this.ComCon.DataManager.DataSegmentSize, u4))
			{
				string u5 = global::\u0003.\u0006.\u0001(MessageId.Err_NotEnoughMemoryForVariableInit, new object[]
				{
					\u0004.VersionedName,
					\u0002.CompiledCode.CodeSize
				});
				IMessageCategory messageCategory = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
				_ICompilerMessage icompilerMessage = global::\u0019.\u0003.\u0001(\u0004.SourcePosition, u5, Severity.Error, MessageId.Err_NotEnoughMemoryForVariableInit);
				icompilerMessage.ProjectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0003.LibraryPath);
				icompilerMessage.ObjectGuid = \u0003.ObjectGuid;
				APEnvironmentFacade.Instance.AddMessage(messageCategory, icompilerMessage);
			}
			\u0002.CompiledCode.Location = global::\u0019.\u0003.\u0001(u2, u3);
		}

		// Token: 0x0600295F RID: 10591 RVA: 0x00090DF8 File Offset: 0x0008EFF8
		private void \u0001(int \u0002, IList<_IExpression> \u0003, _IArrayType \u0004, BinaryWriter \u0005)
		{
			for (int i = 0; i < \u0002; i++)
			{
				_IExpression iexpression = \u0003[i];
				StringEncoding u = StringEncoding.Default;
				_IStringLiteralExpression2 istringLiteralExpression = iexpression as _IStringLiteralExpression2;
				if (istringLiteralExpression != null)
				{
					u = istringLiteralExpression.StringEncoding;
				}
				Helper.\u0001(iexpression.Literal(this._Scope) as _ILiteralValue, \u0004._Base, \u0005, this.Context.CodeGen.MotorolaByteOrder, this._Scope, u);
			}
		}

		// Token: 0x06002960 RID: 10592 RVA: 0x00090E64 File Offset: 0x0008F064
		private bool \u0001(_ICompiledPOU \u0002, string \u0003, out _ICompiledPOU \u0004)
		{
			bool result = false;
			\u0004 = null;
			if (this.Codegen.RefContext != null && this.Codegen.OnlineChange)
			{
				_ISignature isignature = this.Codegen.RefContext[\u0003];
				if (isignature != null)
				{
					\u0004 = (_ICompiledPOU)this.Codegen.RefContext.GetCompiledPOUById(isignature.Id);
					_ICompiledCodeData icompiledCodeData = \u0004.CompiledCode as _ICompiledCodeData;
					if (icompiledCodeData != null && icompiledCodeData.Equals((_ICompiledCodeData)\u0002.CompiledCode))
					{
						result = true;
					}
				}
			}
			return result;
		}

		// Token: 0x06002961 RID: 10593 RVA: 0x00090EE8 File Offset: 0x0008F0E8
		private void \u0001(ISignature \u0002, _ISignature \u0003, _ISignature \u0004, IVariable \u0005, _IType \u0006, IDataLocation \u0007, _ISequenceStatement \u0008)
		{
			_ICompiledPOU icompiledPOU = this.ComCon._GetCompiledPOUById(\u0002.Id);
			string pouuniqueLabel = ArrayInitialisationCodeGenerator.GetPOUUniqueLabel(ArrayInitialisationCodeGenerator.\u0001);
			_IVariable ivariable = global::\u0019.\u0003.\u0001(null);
			ivariable.Name = pouuniqueLabel;
			ivariable.DataLocation = \u0007;
			ivariable._Type = \u0006;
			if (\u0005.DataLocation.IsRelativ)
			{
				if (\u0005.GetFlag(VarFlag.RelativeInstance))
				{
					ivariable.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.RelativeInstance | VarFlag.NoInit | VarFlag.Implicit, true);
				}
				else
				{
					ivariable.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.RelativeStack | VarFlag.NoInit | VarFlag.Implicit, true);
				}
			}
			else
			{
				ivariable.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.Absolut | VarFlag.NoInit | VarFlag.Implicit, true);
			}
			if (!\u0007.IsRelativ)
			{
				ivariable.Id = \u0003.NextId;
				\u0003.AddVariable(ivariable);
			}
			else
			{
				ivariable.Id = \u0004.NextId;
				\u0004.AddVariable(ivariable);
			}
			string pouuniqueLabel2 = ArrayInitialisationCodeGenerator.GetPOUUniqueLabel(ArrayInitialisationCodeGenerator.\u0002);
			_IVariable ivariable2 = global::\u0019.\u0003.\u0001(null);
			ivariable2.Name = pouuniqueLabel2;
			ivariable2.DataLocation = icompiledPOU.CompiledCode.Location;
			ivariable2._Type = \u0006;
			ivariable2.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.Absolut | VarFlag.NoInit | VarFlag.Implicit, true);
			ivariable2.Id = \u0003.NextId;
			\u0003.AddVariable(ivariable2);
			IExpression u = global::\u0019.\u0003.\u0001(ivariable.OrgName);
			_IVariableExpression u2 = global::\u0019.\u0003.\u0001(ivariable2.OrgName);
			_IExpressionStatement sm = global::\u0019.\u0003.\u0001(u, u2);
			\u0008.Add(sm);
		}

		// Token: 0x06002962 RID: 10594 RVA: 0x00091030 File Offset: 0x0008F230
		private void \u0001(int \u0002, ref int[] \u0003, ref int[] \u0004, ref int[] \u0005, out bool \u0006)
		{
			\u0006 = false;
			for (int i = 0; i < \u0002; i++)
			{
				this.\u0001(ref \u0003, ref \u0004, ref \u0005, out \u0006);
				if (\u0006)
				{
					return;
				}
			}
		}

		// Token: 0x06002963 RID: 10595 RVA: 0x00091060 File Offset: 0x0008F260
		private void \u0001(ref int[] \u0002, ref int[] \u0003, ref int[] \u0004, out bool \u0005)
		{
			\u0005 = true;
			for (int i = \u0002.Length - 1; i >= 0; i--)
			{
				if (\u0002[i] != \u0003[i])
				{
					\u0002[i]++;
					\u0005 = false;
					return;
				}
				\u0002[i] = \u0004[i];
			}
		}

		// Token: 0x04000791 RID: 1937
		private static int \u0001 = 0;

		// Token: 0x04000792 RID: 1938
		private readonly bool \u0001;

		// Token: 0x04000793 RID: 1939
		[CompilerGenerated]
		private readonly global::\u000E.\u0011 \u0001;

		// Token: 0x04000794 RID: 1940
		[CompilerGenerated]
		private readonly StructAndArrayInitReplacer \u0001;

		// Token: 0x04000795 RID: 1941
		private static int \u0002 = 0;

		// Token: 0x04000796 RID: 1942
		private static readonly string \u0001 = "__Left__{0}";

		// Token: 0x04000797 RID: 1943
		private static readonly string \u0002 = "__Right__{0}";
	}
}
