using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u0006;
using \u0007;
using \u000E;
using \u0019;
using \u001F;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase1_Typification
{
	// Token: 0x0200033D RID: 829
	internal sealed class TypeCompiler : ITypeVisitor3, ITypeVisitor2, ITypeVisitor, ITypeVisitor4, ITypeCompiler
	{
		// Token: 0x17000826 RID: 2086
		// (get) Token: 0x060031E5 RID: 12773 RVA: 0x000C0EE8 File Offset: 0x000BF0E8
		private global::\u000E.\u0017 Context { get; }

		// Token: 0x17000827 RID: 2087
		// (get) Token: 0x060031E6 RID: 12774 RVA: 0x000C0EF0 File Offset: 0x000BF0F0
		private IScope Scope
		{
			get
			{
				return this.Context.Scope;
			}
		}

		// Token: 0x17000828 RID: 2088
		// (get) Token: 0x060031E7 RID: 12775 RVA: 0x000C0F00 File Offset: 0x000BF100
		private _ICompileContext Comcon
		{
			get
			{
				return this.Context.Comcon;
			}
		}

		// Token: 0x17000829 RID: 2089
		// (get) Token: 0x060031E8 RID: 12776 RVA: 0x000C0F10 File Offset: 0x000BF110
		private \u001F.\u0007 Typechecker
		{
			get
			{
				return this.Context.Typechecker;
			}
		}

		// Token: 0x1700082A RID: 2090
		// (get) Token: 0x060031E9 RID: 12777 RVA: 0x000C0F20 File Offset: 0x000BF120
		private _IExpressionTypifier Expressiontypifier
		{
			get
			{
				return this.Context.Expressiontypifier;
			}
		}

		// Token: 0x1700082B RID: 2091
		// (get) Token: 0x060031EA RID: 12778 RVA: 0x000C0F30 File Offset: 0x000BF130
		private IErrorVisitor Errorvisitor
		{
			get
			{
				return this.Context.Errorvisitor;
			}
		}

		// Token: 0x1700082C RID: 2092
		// (get) Token: 0x060031EB RID: 12779 RVA: 0x000C0F40 File Offset: 0x000BF140
		private ISourcePosition SourcePos
		{
			get
			{
				return this.Context.SourcePos;
			}
		}

		// Token: 0x1700082D RID: 2093
		// (get) Token: 0x060031EC RID: 12780 RVA: 0x000C0F50 File Offset: 0x000BF150
		// (set) Token: 0x060031ED RID: 12781 RVA: 0x000C0F58 File Offset: 0x000BF158
		private bool ReferencingCrossReference { get; set; }

		// Token: 0x1700082E RID: 2094
		// (get) Token: 0x060031EE RID: 12782 RVA: 0x000C0F64 File Offset: 0x000BF164
		// (set) Token: 0x060031EF RID: 12783 RVA: 0x000C0F6C File Offset: 0x000BF16C
		private _IType GeneratedType { get; set; }

		// Token: 0x060031F0 RID: 12784 RVA: 0x000C0F78 File Offset: 0x000BF178
		internal TypeCompiler()
		{
		}

		// Token: 0x060031F1 RID: 12785 RVA: 0x000C0F80 File Offset: 0x000BF180
		private TypeCompiler(global::\u000E.\u0017 context)
		{
			this.Context = context;
			this.GeneratedType = null;
		}

		// Token: 0x060031F2 RID: 12786 RVA: 0x000C0F98 File Offset: 0x000BF198
		public _IType \u0001(_IType \u0002, IScope \u0003, _ICompileContext \u0004, ISourcePosition \u0005, _ISignature \u0006, ref IExpression \u0007)
		{
			return this.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x060031F3 RID: 12787 RVA: 0x000C0FA8 File Offset: 0x000BF1A8
		private _IType \u0001(_IType \u0002, IScope \u0003, _ICompileContext \u0004, ISourcePosition \u0005, _ISignature \u0006)
		{
			global::\u000E.\u0017 u = this.Context;
			TypeCompiler typeCompiler = new TypeCompiler(global::\u000E.\u0017.\u0001(\u0003, \u0004, \u0005, \u0006, (u != null) ? u.VarWithType : null));
			\u0002.Accept(typeCompiler);
			return typeCompiler.GeneratedType;
		}

		// Token: 0x060031F4 RID: 12788 RVA: 0x000C0FE8 File Offset: 0x000BF1E8
		public static _IType \u0001(_IType \u0002, IScope \u0003, _ICompileContext \u0004, \u001F.\u0007 \u0005, _IExpressionTypifier \u0006, _IVariable \u0007, _ISignature \u0008)
		{
			TypeCompiler typeCompiler = new TypeCompiler(global::\u000E.\u0017.\u0001(\u0003, \u0004, \u0005, \u0006, \u0007.SourcePosition, \u0008, \u0007));
			\u0002.Accept(typeCompiler);
			return typeCompiler.GeneratedType;
		}

		// Token: 0x060031F5 RID: 12789 RVA: 0x000C1020 File Offset: 0x000BF220
		public static _IType \u0001(_IType \u0002, IScope \u0003, _ICompileContext \u0004, \u001F.\u0007 \u0005, _IExpressionTypifier \u0006, ISourcePosition \u0007)
		{
			TypeCompiler typeCompiler = new TypeCompiler(global::\u000E.\u0017.\u0001(\u0003, \u0004, \u0005, \u0006, \u0007));
			\u0002.Accept(typeCompiler);
			return typeCompiler.GeneratedType;
		}

		// Token: 0x060031F6 RID: 12790 RVA: 0x000C104C File Offset: 0x000BF24C
		public static _IType \u0001(_IType \u0002, IScope \u0003, _ICompileContext \u0004, \u001F.\u0007 \u0005, _IExpressionTypifier \u0006, ISourcePosition \u0007, out IEnumerable<_ICompilerMessage> \u0008)
		{
			global::\u000E.\u0017 u = global::\u000E.\u0017.\u0001(\u0003, \u0004, \u0005, \u0006, \u0007);
			TypeCompiler typeCompiler = new TypeCompiler(u);
			\u0002.Accept(typeCompiler);
			\u0008 = u.Errorvisitor.MessageList;
			return typeCompiler.GeneratedType;
		}

		// Token: 0x060031F7 RID: 12791 RVA: 0x000C1088 File Offset: 0x000BF288
		internal static _IType \u0001(_IType \u0002, global::\u000E.\u0017 \u0003, bool \u0004)
		{
			TypeCompiler typeCompiler = new TypeCompiler(\u0003)
			{
				ReferencingCrossReference = \u0004
			};
			\u0002.Accept(typeCompiler);
			return typeCompiler.GeneratedType;
		}

		// Token: 0x060031F8 RID: 12792 RVA: 0x000C10B0 File Offset: 0x000BF2B0
		private void \u0002(_IType \u0002)
		{
			if (!this.Comcon.TypeIsSupported(\u0002.Class))
			{
				this.Errorvisitor.MessageList.Add(\u0019.\u0003.\u0001(this.SourcePos, global::\u0003.\u0006.\u0001(MessageId.Err_UnsupportedType, new object[]
				{
					\u0002.ToString()
				}), Severity.Error, MessageId.Err_UnsupportedType));
			}
			this.GeneratedType = \u0002;
		}

		// Token: 0x060031F9 RID: 12793 RVA: 0x000C110C File Offset: 0x000BF30C
		private void \u0001(_IArrayDimension \u0002)
		{
			\u0002._LowerBorder.Accept(this.Expressiontypifier);
			if (!\u0002._LowerBorder.IsConstant(this.Scope, true))
			{
				\u0002._LowerBorder.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_ArrayBorderIsNoConstant, new object[]
				{
					\u0002._LowerBorder.ToString()
				}), MessageId.Err_ArrayBorderIsNoConstant);
			}
			_IExpression iexpression = \u0002._LowerBorder;
			this.Typechecker.\u0001(\u0002._LowerBorder, \u0002._LowerBorder._CompiledType, TypeClass.AnyInt, this.Scope as global::\u0007.\u0005, ref iexpression);
			\u0002._LowerBorder = iexpression;
			\u0002._UpperBorder.Accept(this.Expressiontypifier);
			if (!\u0002._UpperBorder.IsConstant(this.Scope, true))
			{
				\u0002._UpperBorder.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_ArrayBorderIsNoConstant, new object[]
				{
					\u0002._UpperBorder.ToString()
				}), MessageId.Err_ArrayBorderIsNoConstant);
			}
			iexpression = \u0002._UpperBorder;
			this.Typechecker.\u0001(\u0002._UpperBorder, \u0002._UpperBorder._CompiledType, TypeClass.AnyInt, this.Scope as global::\u0007.\u0005, ref iexpression);
			\u0002._UpperBorder = iexpression;
			this.\u0001(\u0002._LowerBorder);
			this.\u0001(\u0002._UpperBorder);
			\u0002._LowerBorder.Accept(this.Errorvisitor);
			\u0002._UpperBorder.Accept(this.Errorvisitor);
		}

		// Token: 0x060031FA RID: 12794 RVA: 0x000C1270 File Offset: 0x000BF470
		private void \u0001(_IExpression \u0002)
		{
			_IOperatorExpression ioperatorExpression = \u0002 as _IOperatorExpression;
			if (ioperatorExpression == null)
			{
				return;
			}
			Operator code = ioperatorExpression.Code;
			if (code - Operator.Add <= 4 || code - Operator.Plus <= 2 || code == Operator.Divide)
			{
				foreach (ICompiledType compiledType in ioperatorExpression._OperandsList.Select(new Func<_IExpression, ICompiledType>(TypeCompiler.<>c.<>9.\u0001)))
				{
					if (compiledType != null && compiledType.Class == TypeClass.Enum)
					{
						ISignature signature = ((_IScope)this.Scope).FindSignature((_IEnumType)compiledType);
						if (signature != null && signature.HasAttribute("strict"))
						{
							this.Typechecker.\u0001(\u0002, MessageId.Err_StrictEnumNoArithmeticAllowed, new object[]
							{
								signature.OrgName
							});
						}
					}
				}
			}
		}

		// Token: 0x060031FB RID: 12795 RVA: 0x000C1360 File Offset: 0x000BF560
		public void \u0001(_IBitConstType \u0002)
		{
		}

		// Token: 0x060031FC RID: 12796 RVA: 0x000C1364 File Offset: 0x000BF564
		public void \u0001(_IBitType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x060031FD RID: 12797 RVA: 0x000C1370 File Offset: 0x000BF570
		public void \u0001(_IBoolType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x060031FE RID: 12798 RVA: 0x000C137C File Offset: 0x000BF57C
		public void \u0001(_IByteType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x060031FF RID: 12799 RVA: 0x000C1388 File Offset: 0x000BF588
		public void \u0001(_ISIntType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x06003200 RID: 12800 RVA: 0x000C1394 File Offset: 0x000BF594
		public void \u0001(_IUSIntType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x06003201 RID: 12801 RVA: 0x000C13A0 File Offset: 0x000BF5A0
		public void \u0001(_IIntType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x06003202 RID: 12802 RVA: 0x000C13AC File Offset: 0x000BF5AC
		public void \u0001(_IUIntType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x06003203 RID: 12803 RVA: 0x000C13B8 File Offset: 0x000BF5B8
		public void \u0001(_IWordType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x06003204 RID: 12804 RVA: 0x000C13C4 File Offset: 0x000BF5C4
		public void \u0001(_IDIntType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x06003205 RID: 12805 RVA: 0x000C13D0 File Offset: 0x000BF5D0
		public void \u0001(_IUDIntType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x06003206 RID: 12806 RVA: 0x000C13DC File Offset: 0x000BF5DC
		public void \u0001(_IDWordType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x06003207 RID: 12807 RVA: 0x000C13E8 File Offset: 0x000BF5E8
		public void \u0001(_ILIntType \u0002)
		{
			if (this.Comcon.TreatInt64AsInt32)
			{
				this.GeneratedType = TypeTable.Get(Operator.DInt);
				return;
			}
			this.\u0002(\u0002);
		}

		// Token: 0x06003208 RID: 12808 RVA: 0x000C140C File Offset: 0x000BF60C
		public void \u0001(_IULIntType \u0002)
		{
			if (this.Comcon.TreatInt64AsInt32)
			{
				this.GeneratedType = TypeTable.Get(Operator.UDInt);
				return;
			}
			this.\u0002(\u0002);
		}

		// Token: 0x06003209 RID: 12809 RVA: 0x000C1430 File Offset: 0x000BF630
		public void \u0001(_ILWordType \u0002)
		{
			if (this.Comcon.TreatInt64AsInt32)
			{
				this.GeneratedType = TypeTable.Get(Operator.DWord);
				return;
			}
			this.\u0002(\u0002);
		}

		// Token: 0x0600320A RID: 12810 RVA: 0x000C1454 File Offset: 0x000BF654
		public void \u0001(_IRealType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x0600320B RID: 12811 RVA: 0x000C1460 File Offset: 0x000BF660
		public void \u0001(_ILRealType \u0002)
		{
			if (this.Comcon.TreatLRealAsReal)
			{
				this.GeneratedType = TypeTable.Get(Operator.Real);
				return;
			}
			this.\u0002(\u0002);
		}

		// Token: 0x0600320C RID: 12812 RVA: 0x000C1484 File Offset: 0x000BF684
		public void \u0001(_ILazyType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x0600320D RID: 12813 RVA: 0x000C1490 File Offset: 0x000BF690
		public void \u0001(IImplicitEnumerationType \u0002)
		{
			\u0002.Enumerations.AddError(\u0018.\u0001(MessageId.Err_ImplicitEnumerationTypeNotExpected), MessageId.Err_ImplicitEnumerationTypeNotExpected);
			\u0002.Enumerations.Accept(this.Errorvisitor);
			this.GeneratedType = \u0002;
		}

		// Token: 0x0600320E RID: 12814 RVA: 0x000C14C4 File Offset: 0x000BF6C4
		public void \u0001(_IUserdefType \u0002)
		{
			GenericTypeCompiler genericTypeCompiler = new GenericTypeCompiler(this.Context, this.ReferencingCrossReference);
			\u0019.\u0012 u = new \u0019.\u0012(this.Context, this.ReferencingCrossReference, genericTypeCompiler.\u0002());
			u.\u0001(\u0002);
			this.GeneratedType = u.GeneratedType;
		}

		// Token: 0x0600320F RID: 12815 RVA: 0x000C1510 File Offset: 0x000BF710
		public void \u0001(_IPointerType \u0002)
		{
			\u0002._Base = TypeCompiler.\u0001(\u0002._Base, this.Context, true);
			this.GeneratedType = \u0002;
		}

		// Token: 0x06003210 RID: 12816 RVA: 0x000C1534 File Offset: 0x000BF734
		public void \u0001(_IReferenceType \u0002)
		{
			\u0002._Base = TypeCompiler.\u0001(\u0002._Base, this.Context, true);
			this.GeneratedType = \u0002;
		}

		// Token: 0x06003211 RID: 12817 RVA: 0x000C1558 File Offset: 0x000BF758
		public void \u0001(_ISubrangeType \u0002)
		{
			\u0002._LowerBorder.Accept(this.Expressiontypifier);
			if (!\u0002._LowerBorder.IsConstant(this.Scope, true))
			{
				\u0002._LowerBorder.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_ArrayBorderIsNoConstant, new object[]
				{
					\u0002._LowerBorder.ToString()
				}), MessageId.Err_ArrayBorderIsNoConstant);
			}
			_IExpression iexpression = \u0002._LowerBorder;
			_ILiteralExpression iliteralExpression = \u0002._LowerBorder as _ILiteralExpression;
			if (iliteralExpression != null)
			{
				if (!global::\u0006.\u0011.\u0001(iliteralExpression, iliteralExpression.Type, \u0002._Base, this.Scope as ICommonScope, this.Scope as ICommonScope))
				{
					this.Typechecker.\u0001(iliteralExpression, MessageId.Err_TypeMismatch, new object[]
					{
						iliteralExpression,
						\u0002._Base
					});
				}
			}
			else
			{
				this.Typechecker.\u0001(\u0002._LowerBorder, \u0002._LowerBorder.Type, \u0002._Base, this.Scope as global::\u0007.\u0005, ref iexpression);
				this.\u0001(\u0002._LowerBorder);
			}
			\u0002._LowerBorder.Accept(this.Errorvisitor);
			\u0002._UpperBorder.Accept(this.Expressiontypifier);
			if (!\u0002._UpperBorder.IsConstant(this.Scope, true))
			{
				\u0002._UpperBorder.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_ArrayBorderIsNoConstant, new object[]
				{
					\u0002._UpperBorder.ToString()
				}), MessageId.Err_ArrayBorderIsNoConstant);
			}
			iexpression = \u0002._UpperBorder;
			_ILiteralExpression iliteralExpression2 = \u0002._UpperBorder as _ILiteralExpression;
			if (iliteralExpression2 != null)
			{
				if (!global::\u0006.\u0011.\u0001(iliteralExpression2, iliteralExpression2.Type, \u0002._Base, this.Scope as ICommonScope, this.Scope as ICommonScope))
				{
					this.Typechecker.\u0001(iliteralExpression2, MessageId.Err_TypeMismatch, new object[]
					{
						iliteralExpression2,
						\u0002._Base
					});
				}
			}
			else
			{
				this.Typechecker.\u0001(\u0002._UpperBorder, \u0002._UpperBorder.Type, \u0002._Base, this.Scope as IScope5, ref iexpression);
				this.\u0001(\u0002._UpperBorder);
			}
			\u0002._UpperBorder.Accept(this.Errorvisitor);
			\u0002._Base = TypeCompiler.\u0001(\u0002._Base, this.Context, this.ReferencingCrossReference);
			this.GeneratedType = \u0002;
		}

		// Token: 0x06003212 RID: 12818 RVA: 0x000C1790 File Offset: 0x000BF990
		public void \u0001(_IEnumType \u0002)
		{
			if (\u0002.SignatureId == Helper.InvalidId)
			{
				\u0002.SignatureId = this.Scope.LocalSignature.Id;
			}
			this.GeneratedType = \u0002;
		}

		// Token: 0x06003213 RID: 12819 RVA: 0x000C17BC File Offset: 0x000BF9BC
		public void \u0001(_IParamsType \u0002)
		{
			\u0002._Base = TypeCompiler.\u0001(\u0002._Base, this.Context, this.ReferencingCrossReference);
			this.GeneratedType = \u0002;
		}

		// Token: 0x06003214 RID: 12820 RVA: 0x000C17E4 File Offset: 0x000BF9E4
		public void \u0001(_IVariableLengthArrayType \u0002)
		{
			\u0002._Base = TypeCompiler.\u0001(\u0002._Base, this.Context, this.ReferencingCrossReference);
			this.GeneratedType = \u0002;
		}

		// Token: 0x06003215 RID: 12821 RVA: 0x000C180C File Offset: 0x000BFA0C
		public void \u0001(_IArrayType \u0002)
		{
			foreach (_IArrayDimension u in \u0002._Dimensions)
			{
				this.\u0001(u);
			}
			\u0002._Base = TypeCompiler.\u0001(\u0002._Base, this.Context, this.ReferencingCrossReference);
			this.GeneratedType = \u0002;
		}

		// Token: 0x06003216 RID: 12822 RVA: 0x000C1880 File Offset: 0x000BFA80
		public void \u0001(_IVectorType \u0002)
		{
			\u0002._Dimension.Accept(this.Expressiontypifier);
			if (!\u0002._Dimension.IsConstant(this.Scope, true))
			{
				\u0002._Dimension.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_VectorSizeIsNoConstant, new object[]
				{
					\u0002._Dimension.ToString()
				}), MessageId.Err_VectorSizeIsNoConstant);
			}
			_IExpression dimension = \u0002._Dimension;
			this.Typechecker.\u0001(\u0002._Dimension, \u0002._Dimension._CompiledType, TypeClass.AnyInt, this.Scope as global::\u0007.\u0005, ref dimension);
			\u0002._Dimension = dimension;
			\u0002._Dimension.Accept(this.Errorvisitor);
			\u0002._Base = TypeCompiler.\u0001(\u0002._Base, this.Context, this.ReferencingCrossReference);
			if (\u0002._Base.Class != TypeClass.Real && \u0002._Base.Class != TypeClass.LReal)
			{
				this.Errorvisitor.MessageList.Add(\u0019.\u0003.\u0001(this.SourcePos, global::\u0003.\u0006.\u0001(MessageId.Err_VectorBaseMustBeRealType, Array.Empty<object>()), Severity.Error, MessageId.Err_VectorBaseMustBeRealType));
			}
			this.GeneratedType = \u0002;
		}

		// Token: 0x06003217 RID: 12823 RVA: 0x000C19A0 File Offset: 0x000BFBA0
		public void \u0001(_IStringType \u0002)
		{
			if (!this.Comcon.TypeIsSupported(\u0002.Class))
			{
				this.Errorvisitor.MessageList.Add(\u0019.\u0003.\u0001(this.SourcePos, global::\u0003.\u0006.\u0001(MessageId.Err_UnsupportedType, new object[]
				{
					\u0002.ToString()
				}), Severity.Error, MessageId.Err_UnsupportedType));
			}
			if (\u0002.Length != null)
			{
				\u0002.Length.Accept(this.Expressiontypifier);
				if (!\u0002.Length.IsConstant(this.Scope, true))
				{
					if (\u0002.Length.Type == null)
					{
						\u0002.Length.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_IdentNotDefined, new object[]
						{
							\u0002.Length.ToString()
						}), MessageId.Err_IdentNotDefined);
					}
					\u0002.Length.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_StringLengthIsNoConstant, new object[]
					{
						\u0002.Length.ToString()
					}), MessageId.Err_StringLengthIsNoConstant);
				}
				_IExpression length = \u0002.Length;
				this.Typechecker.\u0001(\u0002.Length, \u0002.Length._CompiledType, TypeClass.AnyInt, this.Scope as global::\u0007.\u0005, ref length);
				\u0002.Length = length;
				this.\u0001(length);
				\u0002.Length.Accept(this.Errorvisitor);
			}
			this.GeneratedType = \u0002;
		}

		// Token: 0x06003218 RID: 12824 RVA: 0x000C1AE0 File Offset: 0x000BFCE0
		public void \u0001(_IWStringType \u0002)
		{
			if (!this.Comcon.TypeIsSupported(\u0002.Class))
			{
				this.Errorvisitor.MessageList.Add(\u0019.\u0003.\u0001(this.SourcePos, global::\u0003.\u0006.\u0001(MessageId.Err_UnsupportedType, new object[]
				{
					\u0002.ToString()
				}), Severity.Error, MessageId.Err_UnsupportedType));
			}
			if (\u0002.Length != null)
			{
				\u0002.Length.Accept(this.Expressiontypifier);
				if (!\u0002.Length.IsConstant(this.Scope, true))
				{
					if (\u0002.Length.Type == null)
					{
						\u0002.Length.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_IdentNotDefined, new object[]
						{
							\u0002.Length.ToString()
						}), MessageId.Err_IdentNotDefined);
					}
					\u0002.Length.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_StringLengthIsNoConstant, new object[]
					{
						\u0002.Length.ToString()
					}), MessageId.Err_StringLengthIsNoConstant);
				}
				_IExpression length = \u0002.Length;
				this.Typechecker.\u0001(\u0002.Length, \u0002.Length._CompiledType, TypeClass.AnyInt, this.Scope as global::\u0007.\u0005, ref length);
				\u0002.Length = length;
				\u0002.Length.Accept(this.Errorvisitor);
			}
			this.GeneratedType = \u0002;
		}

		// Token: 0x06003219 RID: 12825 RVA: 0x000C1C1C File Offset: 0x000BFE1C
		public void \u0001(_IAnyType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x0600321A RID: 12826 RVA: 0x000C1C28 File Offset: 0x000BFE28
		public void \u0001(_IAnyRealType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x0600321B RID: 12827 RVA: 0x000C1C34 File Offset: 0x000BFE34
		public void \u0001(_IAnyIntType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x0600321C RID: 12828 RVA: 0x000C1C40 File Offset: 0x000BFE40
		public void \u0001(_IAnyNumType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x0600321D RID: 12829 RVA: 0x000C1C4C File Offset: 0x000BFE4C
		public void \u0001(_IAnyBitType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x0600321E RID: 12830 RVA: 0x000C1C58 File Offset: 0x000BFE58
		public void \u0001(_IAnyDateType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x0600321F RID: 12831 RVA: 0x000C1C64 File Offset: 0x000BFE64
		public void \u0001(_IAnyStringType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x06003220 RID: 12832 RVA: 0x000C1C70 File Offset: 0x000BFE70
		public void \u0001(_IAnyBitButBoolIsPreferred \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x06003221 RID: 12833 RVA: 0x000C1C7C File Offset: 0x000BFE7C
		public void \u0001(_IDateType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x06003222 RID: 12834 RVA: 0x000C1C88 File Offset: 0x000BFE88
		public void \u0001(_ITimeOfDayType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x06003223 RID: 12835 RVA: 0x000C1C94 File Offset: 0x000BFE94
		public void \u0001(_IDateAndTimeType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x06003224 RID: 12836 RVA: 0x000C1CA0 File Offset: 0x000BFEA0
		public void \u0001(_ILDateType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x06003225 RID: 12837 RVA: 0x000C1CAC File Offset: 0x000BFEAC
		public void \u0001(_ILTimeOfDayType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x06003226 RID: 12838 RVA: 0x000C1CB8 File Offset: 0x000BFEB8
		public void \u0001(_ILDateAndTimeType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x06003227 RID: 12839 RVA: 0x000C1CC4 File Offset: 0x000BFEC4
		public void \u0001(_ITimeType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x06003228 RID: 12840 RVA: 0x000C1CD0 File Offset: 0x000BFED0
		public void \u0001(_ILTimeType \u0002)
		{
			if (this.Comcon.TreatInt64AsInt32)
			{
				this.GeneratedType = TypeTable.Get(Operator.Time);
				return;
			}
			this.\u0002(\u0002);
		}

		// Token: 0x06003229 RID: 12841 RVA: 0x000C1CF4 File Offset: 0x000BFEF4
		public void \u0001(_IXIntType \u0002)
		{
			if (this.Comcon.PointerSize == 8)
			{
				this.GeneratedType = TypeTable.XLInt;
				return;
			}
			this.GeneratedType = TypeTable.XDInt;
		}

		// Token: 0x0600322A RID: 12842 RVA: 0x000C1D1C File Offset: 0x000BFF1C
		public void \u0001(_IXWordType \u0002)
		{
			if (this.Comcon.PointerSize == 8)
			{
				this.GeneratedType = TypeTable.XLWord;
				return;
			}
			this.GeneratedType = TypeTable.XDWord;
		}

		// Token: 0x0600322B RID: 12843 RVA: 0x000C1D44 File Offset: 0x000BFF44
		public void \u0001(_IXUDIntType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x0600322C RID: 12844 RVA: 0x000C1D50 File Offset: 0x000BFF50
		public void \u0001(_IXULIntType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x0600322D RID: 12845 RVA: 0x000C1D5C File Offset: 0x000BFF5C
		public void \u0001(_IXLIntType \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x0600322E RID: 12846 RVA: 0x000C1D68 File Offset: 0x000BFF68
		public void \u0001(_IUXIntType \u0002)
		{
			if (this.Comcon.PointerSize == 8)
			{
				this.GeneratedType = TypeTable.XULInt;
				return;
			}
			this.GeneratedType = TypeTable.XUDInt;
		}

		// Token: 0x0600322F RID: 12847 RVA: 0x000C1D90 File Offset: 0x000BFF90
		public void \u0001(_IXStringType \u0002)
		{
			if (this.Comcon.IsDefined("NO_UNICODE_SUPPORT"))
			{
				_IStringType istringType = \u0019.\u0003.\u0001();
				istringType.Length = \u0002.Length;
				this.GeneratedType = istringType;
				return;
			}
			_IWStringType iwstringType = \u0019.\u0003.\u0001();
			iwstringType.Length = \u0002.Length;
			this.GeneratedType = iwstringType;
		}

		// Token: 0x06003230 RID: 12848 RVA: 0x000C1DE4 File Offset: 0x000BFFE4
		public void \u0001(IGenericUserdefType \u0002)
		{
			GenericTypeCompiler genericTypeCompiler = new GenericTypeCompiler(this.Context, this.ReferencingCrossReference);
			genericTypeCompiler.\u0001(\u0002);
			this.GeneratedType = genericTypeCompiler.GeneratedType;
		}

		// Token: 0x06003231 RID: 12849 RVA: 0x000C1E18 File Offset: 0x000C0018
		public void \u0001(_IAliasType \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06003232 RID: 12850 RVA: 0x000C1E24 File Offset: 0x000C0024
		public void \u0001(_IXDIntType \u0002)
		{
			this.\u0001(\u0002 as _IDIntType);
		}

		// Token: 0x06003233 RID: 12851 RVA: 0x000C1E34 File Offset: 0x000C0034
		public void \u0001(_IXDWordType \u0002)
		{
			this.\u0001(\u0002 as _IDWordType);
		}

		// Token: 0x06003234 RID: 12852 RVA: 0x000C1E44 File Offset: 0x000C0044
		public void \u0001(_IXLWordType \u0002)
		{
			this.\u0001(\u0002 as _ILWordType);
		}

		// Token: 0x0400096D RID: 2413
		[CompilerGenerated]
		private readonly global::\u000E.\u0017 \u0001;

		// Token: 0x0400096E RID: 2414
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x0400096F RID: 2415
		[CompilerGenerated]
		private _IType \u0001;
	}
}
