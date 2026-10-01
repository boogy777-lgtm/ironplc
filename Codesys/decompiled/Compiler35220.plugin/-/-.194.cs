using System;
using System.Collections.Generic;
using \u0004;
using \u0011;
using \u0017;
using \u0018;
using \u0019;
using \u001A;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001B
{
	// Token: 0x0200022E RID: 558
	internal sealed class \u0006
	{
		// Token: 0x170006B9 RID: 1721
		// (get) Token: 0x06002516 RID: 9494 RVA: 0x00080CC8 File Offset: 0x0007EEC8
		private ICodegenerator m_codegen
		{
			get
			{
				return this.\u0001.\u0001;
			}
		}

		// Token: 0x06002517 RID: 9495 RVA: 0x00080CD8 File Offset: 0x0007EED8
		public \u0006(global::\u0004.\u000E \u0010\u0006)
		{
			this.\u0001 = \u0010\u0006;
		}

		// Token: 0x170006BA RID: 1722
		// (get) Token: 0x06002518 RID: 9496 RVA: 0x00080CE8 File Offset: 0x0007EEE8
		private global::\u0011.\u0007 TopOfStack
		{
			get
			{
				return this.\u0001.TopOfStack;
			}
		}

		// Token: 0x170006BB RID: 1723
		// (get) Token: 0x06002519 RID: 9497 RVA: 0x00080CF8 File Offset: 0x0007EEF8
		private IScope5 _Scope
		{
			get
			{
				return this.\u0001._Scope;
			}
		}

		// Token: 0x0600251A RID: 9498 RVA: 0x00080D08 File Offset: 0x0007EF08
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			ICollection<_IExpression> accesses = \u0002._Accesses;
			int num = 0;
			bool flag = true;
			bool flag2 = false;
			\u0002.Type = global::\u0004.\u000E.\u0001(\u0002.Type);
			ICompiledType deRefType = \u0002._Var.Type.DeRefType;
			global::\u0017.\u0011 u = new global::\u0017.\u0011();
			\u0002.Info = new global::\u0018.\u0005();
			\u0002.Info.DoGeneration = false;
			\u0002.Info.HasSideEffect = false;
			ICompiledType u2 = this.TopOfStack.CurrentType;
			TypeClass @class = deRefType.Class;
			if (@class != TypeClass.Pointer)
			{
				if (@class == TypeClass.Array)
				{
					this.\u0001(\u0002, accesses, ref num, ref flag, ref flag2, deRefType, u, ref u2);
					goto IL_2E4;
				}
			}
			else
			{
				_IDeRefAccessExpression ideRefAccessExpression = global::\u0019.\u0003.\u0001(\u0002._Var.Duplicate() as _IExpression, Token.Empty);
				ideRefAccessExpression._Base.Type = deRefType;
				ideRefAccessExpression.Type = ((_IPointerType)deRefType).BaseType;
				IVariable variable = \u0002._Var.GetVariable(this._Scope);
				\u0002._Var = ideRefAccessExpression;
				if (variable != null && variable.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY))
				{
					IDeRefAccessExpression deRefAccessExpression = \u0002._Var as IDeRefAccessExpression;
					if (deRefAccessExpression == null || !(deRefAccessExpression.Base is IIndexAccessExpression))
					{
						this.\u0002(\u0002);
						u = this.TopOfStack.IndexInfo;
						goto IL_2E4;
					}
				}
			}
			int num2 = this.\u0001(deRefType, ref u2);
			_IExpression access = \u0002.GetAccess(0);
			if (access.IsConstant(this._Scope, false) && (this.TopOfStack.IndexInfo == null || this.TopOfStack.IndexInfo.IndexExpressionRaw == null))
			{
				int @int = access.Literal(this._Scope).GetInt(out flag2);
				if (flag2)
				{
					num = @int * num2;
					this.TopOfStack.Offset += num;
				}
			}
			else
			{
				if (this.m_codegen.\u0001(CodegeneratorProperties.WordAddressing) && num2 > 1)
				{
					num2 /= 2;
				}
				if ((num2 == 2 || num2 == 4 || num2 == 8) && this.TopOfStack.IndexInfo == null)
				{
					u.BaseSize = num2;
					num2 = 1;
				}
				_IExpression iexpression;
				if (num2 == 1)
				{
					iexpression = access;
				}
				else
				{
					_IOperatorExpression ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.Mul, Token.Empty);
					ioperatorExpression.AddOperand(access.Duplicate() as _IExpression);
					ioperatorExpression.AddOperand(global::\u0019.\u0003.\u0001((long)num2));
					iexpression = ioperatorExpression;
				}
				if (this.TopOfStack.IndexInfo != null && this.TopOfStack.IndexInfo.IndexExpressionRaw != null)
				{
					_IOperatorExpression ioperatorExpression2 = global::\u0019.\u0003.\u0001(Operator.Mul, Token.Empty);
					ioperatorExpression2.AddOperand(this.TopOfStack.IndexInfo.IndexExpressionRaw);
					ioperatorExpression2.AddOperand(global::\u0019.\u0003.\u0001((long)this.TopOfStack.IndexInfo.BaseSize));
					_IOperatorExpression ioperatorExpression3 = global::\u0019.\u0003.\u0001(Operator.Add, Token.Empty);
					ioperatorExpression3.AddOperand(ioperatorExpression2);
					ioperatorExpression3.AddOperand(iexpression);
					u.IndexExpressionRaw = ioperatorExpression3;
					this.TopOfStack.IndexInfo = u;
				}
				else
				{
					u.IndexExpressionRaw = iexpression;
					this.TopOfStack.IndexInfo = u;
				}
			}
			IL_2E4:
			int u3 = this.TopOfStack.CurrentPackMode;
			this.\u0001.\u0001(this.TopOfStack.Offset, u, u2, this.TopOfStack.AccessModeFlags);
			this.TopOfStack.CurrentPackMode = u3;
			\u0002._Var.Accept(this.\u0001);
			this.\u0001.\u0001();
			_IExpression iexpression2 = u.IndexExpression as _IExpression;
			if (iexpression2 != null && iexpression2.Info != null && iexpression2.Info.HasSideEffect)
			{
				\u0002.Info.HasSideEffect = true;
			}
			this.TopOfStack.VirtualFunctionCall = false;
		}

		// Token: 0x0600251B RID: 9499 RVA: 0x00081098 File Offset: 0x0007F298
		private int \u0001(IType \u0002, ref ICompiledType \u0003)
		{
			TypeClass @class = \u0002.Class;
			if (@class <= TypeClass.WString)
			{
				if (@class == TypeClass.String)
				{
					if (\u0003 == null)
					{
						\u0003 = TypeTable.Byte;
					}
					return 1;
				}
				if (@class == TypeClass.WString)
				{
					if (\u0003 == null)
					{
						\u0003 = TypeTable.Word;
					}
					return 2;
				}
			}
			else
			{
				if (@class == TypeClass.Pointer)
				{
					_IPointerType ipointerType = (_IPointerType)\u0002;
					if (\u0003 == null)
					{
						\u0003 = ipointerType._Base;
					}
					return ipointerType._Base.Size(this._Scope);
				}
				if (@class == TypeClass.__Vector)
				{
					_IVectorType ivectorType = (_IVectorType)\u0002;
					if (\u0003 == null)
					{
						\u0003 = ivectorType._Base;
					}
					return ivectorType._Base.Size(this._Scope);
				}
			}
			Debug.\u0001(false);
			return 1;
		}

		// Token: 0x0600251C RID: 9500 RVA: 0x00081134 File Offset: 0x0007F334
		private void \u0001(_IIndexAccessExpression \u0002, ICollection<_IExpression> \u0003, ref int \u0004, ref bool \u0005, ref bool \u0006, ICompiledType \u0007, global::\u0017.\u0011 \u0008, ref ICompiledType \u000E)
		{
			_IArrayType iarrayType = \u0007 as _IArrayType;
			if (iarrayType != null)
			{
				IList<_IArrayDimension> dimensions = iarrayType._Dimensions;
				int num = iarrayType.BaseType.Size(this._Scope);
				for (int i = dimensions.Count - 1; i >= 0; i--)
				{
					_IExpression access = \u0002.GetAccess(i);
					\u0005 = false;
					if (!access.IsConstant(this._Scope, false))
					{
						break;
					}
					ILiteralValue literalValue = access.Literal(this._Scope);
					if (literalValue == null)
					{
						break;
					}
					int @int = literalValue.GetInt(out \u0006);
					if (!\u0006)
					{
						break;
					}
					int num2 = dimensions[i].LowerBorderInt(out \u0006, this._Scope);
					if (!\u0006)
					{
						break;
					}
					int num3 = dimensions[i].Range(out \u0006, this._Scope);
					if (!\u0006)
					{
						break;
					}
					\u0004 += (@int - num2) * num;
					num *= num3;
					\u0005 = true;
				}
			}
			if (\u0005 && (this.TopOfStack.IndexInfo == null || this.TopOfStack.IndexInfo.IndexExpressionRaw == null))
			{
				this.TopOfStack.Offset += \u0004;
			}
			else
			{
				if (iarrayType == null)
				{
					throw new InvalidOperationException();
				}
				IList<_IArrayDimension> dimensions2 = iarrayType._Dimensions;
				int num4 = iarrayType.BaseType.Size(this._Scope);
				if (this.m_codegen.\u0001(CodegeneratorProperties.WordAddressing) && num4 > 1)
				{
					num4 /= 2;
				}
				_IExpression iexpression = null;
				_IOperatorExpression ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.Add, Token.Empty);
				if ((num4 == 2 || num4 == 4 || num4 == 8) && this.TopOfStack.IndexInfo == null)
				{
					\u0008.BaseSize = num4;
					num4 = 1;
				}
				for (int j = \u0003.Count - 1; j >= 0; j--)
				{
					_IArrayDimension iarrayDimension = dimensions2[j];
					int num5 = iarrayDimension.LowerBorderInt(out \u0006, this._Scope);
					int num6 = iarrayDimension.Range(out \u0006, this._Scope);
					Debug.\u0001(\u0006);
					_IExpression iexpression2 = \u0002.GetAccess(j).Duplicate() as _IExpression;
					if (num5 != 0)
					{
						_IOperatorExpression ioperatorExpression2 = global::\u0019.\u0003.\u0001(Operator.Minus, Token.Empty);
						ioperatorExpression2.AddOperand(iexpression2);
						ioperatorExpression2.AddOperand(global::\u0019.\u0003.\u0001((long)num5));
						iexpression2 = ioperatorExpression2;
					}
					if (num4 != 1)
					{
						_IOperatorExpression ioperatorExpression3 = global::\u0019.\u0003.\u0001(Operator.Times, Token.Empty);
						ioperatorExpression3.AddOperand(iexpression2);
						ioperatorExpression3.AddOperand(global::\u0019.\u0003.\u0001((long)num4));
						iexpression2 = ioperatorExpression3;
					}
					num4 *= num6;
					if (ioperatorExpression._OperandsList.Count >= 2)
					{
						_IOperatorExpression ioperatorExpression4 = global::\u0019.\u0003.\u0001(Operator.Add, Token.Empty);
						ioperatorExpression4.AddOperand(ioperatorExpression);
						ioperatorExpression = ioperatorExpression4;
					}
					ioperatorExpression.AddOperand(iexpression2);
					if (iexpression == null)
					{
						iexpression = iexpression2;
					}
					else
					{
						iexpression = ioperatorExpression;
					}
				}
				if (this.TopOfStack.IndexInfo != null && this.TopOfStack.IndexInfo.IndexExpressionRaw != null)
				{
					_IOperatorExpression ioperatorExpression5 = global::\u0019.\u0003.\u0001(Operator.Mul, Token.Empty);
					ioperatorExpression5.AddOperand(this.TopOfStack.IndexInfo.IndexExpressionRaw);
					ioperatorExpression5.AddOperand(global::\u0019.\u0003.\u0001((long)this.TopOfStack.IndexInfo.BaseSize));
					_IOperatorExpression ioperatorExpression6 = global::\u0019.\u0003.\u0001(Operator.Add, Token.Empty);
					ioperatorExpression6.AddOperand(ioperatorExpression5);
					ioperatorExpression6.AddOperand(iexpression);
					\u0008.IndexExpressionRaw = ioperatorExpression6;
					this.TopOfStack.IndexInfo = \u0008;
				}
				else
				{
					\u0008.IndexExpressionRaw = iexpression;
					this.TopOfStack.IndexInfo = \u0008;
				}
			}
			if (\u000E == null)
			{
				if (iarrayType == null)
				{
					throw new InvalidOperationException();
				}
				\u000E = iarrayType.BaseType;
			}
		}

		// Token: 0x0600251D RID: 9501 RVA: 0x00081484 File Offset: 0x0007F684
		private void \u0002(_IIndexAccessExpression \u0002)
		{
			IVariable variable = \u0002._Var.GetVariable(this._Scope);
			ISignature signature;
			IVariable variable2 = this._Scope.FindVariableLocal(variable.Name + "__Array__Info", out signature);
			_IArrayType iarrayType = variable2.Type as _IArrayType;
			_IType @base = (variable.Type as _IPointerType)._Base;
			bool flag;
			Debug.\u0001(iarrayType._Dimensions[0].Range(out flag, this._Scope) == \u0002.NumAccesses && flag);
			global::\u0017.\u0011 u = new global::\u0017.\u0011();
			int num = @base.Size(this._Scope);
			if (this.m_codegen.\u0001(CodegeneratorProperties.WordAddressing) && num > 1)
			{
				num /= 2;
			}
			_IExpression iexpression = null;
			_IOperatorExpression ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.Plus, Token.Empty);
			if ((num == 2 || num == 4 || num == 8) && this.TopOfStack.IndexInfo == null)
			{
				u.BaseSize = num;
				num = 1;
			}
			_IExpression iexpression2 = null;
			if (num != 1)
			{
				iexpression2 = global::\u0019.\u0003.\u0001((long)num);
			}
			for (int i = \u0002._Accesses.Count - 1; i >= 0; i--)
			{
				IExpression u2 = global::\u0019.\u0003.\u0001(variable2 as _IVariable, signature as _ISignature);
				_ILiteralExpression u3 = global::\u0019.\u0003.\u0001((long)(i + 1));
				_IIndexAccessExpression iindexAccessExpression = global::\u0019.\u0003.\u0001(u2, u3);
				_IVariableExpression u4 = global::\u0019.\u0003.\u0001("diLower");
				_IVariableExpression u5 = global::\u0019.\u0003.\u0001("diUpper");
				_ICompoAccessExpression icompoAccessExpression = global::\u0019.\u0003.\u0001(iindexAccessExpression, u4);
				_ICompoAccessExpression u6 = global::\u0019.\u0003.\u0001(iindexAccessExpression.Duplicate() as _IExpression, u5);
				_IOperatorExpression u7 = global::\u0019.\u0003.\u0001(Operator.Minus, u6, icompoAccessExpression);
				_ILiteralExpression u8 = global::\u0019.\u0003.\u0001(1L);
				_IOperatorExpression ioperatorExpression2 = global::\u0019.\u0003.\u0001(Operator.Plus, u7, u8);
				_IExpression iexpression3 = \u0002.GetAccess(i).Duplicate() as _IExpression;
				iexpression3 = global::\u0019.\u0003.\u0001(Operator.Minus, iexpression3, icompoAccessExpression.Duplicate() as _IExpression);
				if (iexpression2 != null)
				{
					_IOperatorExpression ioperatorExpression3 = global::\u0019.\u0003.\u0001(Operator.Times, Token.Empty);
					ioperatorExpression3.AddOperand(iexpression3);
					ioperatorExpression3.AddOperand(iexpression2);
					iexpression3 = ioperatorExpression3;
					iexpression2 = global::\u0019.\u0003.\u0001(Operator.Times, iexpression2, ioperatorExpression2);
				}
				else
				{
					iexpression2 = ioperatorExpression2;
				}
				if (ioperatorExpression._OperandsList.Count >= 2)
				{
					_IOperatorExpression ioperatorExpression4 = global::\u0019.\u0003.\u0001(Operator.Plus, Token.Empty);
					ioperatorExpression4.AddOperand(ioperatorExpression);
					ioperatorExpression = ioperatorExpression4;
				}
				ioperatorExpression.AddOperand(iexpression3);
				if (iexpression == null)
				{
					iexpression = iexpression3;
				}
				else
				{
					iexpression = ioperatorExpression;
				}
			}
			if (this.TopOfStack.IndexInfo != null && this.TopOfStack.IndexInfo.IndexExpressionRaw != null)
			{
				_IOperatorExpression ioperatorExpression5 = global::\u0019.\u0003.\u0001(Operator.Mul, Token.Empty);
				ioperatorExpression5.AddOperand(this.TopOfStack.IndexInfo.IndexExpressionRaw);
				ioperatorExpression5.AddOperand(global::\u0019.\u0003.\u0001((long)this.TopOfStack.IndexInfo.BaseSize));
				_IOperatorExpression ioperatorExpression6 = global::\u0019.\u0003.\u0001(Operator.Add, Token.Empty);
				ioperatorExpression6.AddOperand(ioperatorExpression5);
				ioperatorExpression6.AddOperand(iexpression);
				u.IndexExpressionRaw = ioperatorExpression6;
				this.TopOfStack.IndexInfo = u;
				return;
			}
			u.IndexExpressionRaw = iexpression;
			this.TopOfStack.IndexInfo = u;
		}

		// Token: 0x0400069C RID: 1692
		private readonly global::\u0004.\u000E \u0001;
	}
}
