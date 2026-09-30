using System;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0007;
using \u000E;
using \u0017;
using \u0018;
using \u0019;
using \u001F;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase1_Typification
{
	// Token: 0x0200033B RID: 827
	internal sealed class GenericTypeCompiler
	{
		// Token: 0x1700081D RID: 2077
		// (get) Token: 0x060031D1 RID: 12753 RVA: 0x000C0AD4 File Offset: 0x000BECD4
		private global::\u000E.\u0017 Context { get; }

		// Token: 0x1700081E RID: 2078
		// (get) Token: 0x060031D2 RID: 12754 RVA: 0x000C0ADC File Offset: 0x000BECDC
		private IScope Scope
		{
			get
			{
				return this.Context.Scope;
			}
		}

		// Token: 0x1700081F RID: 2079
		// (get) Token: 0x060031D3 RID: 12755 RVA: 0x000C0AEC File Offset: 0x000BECEC
		private IErrorVisitor Errorvisitor
		{
			get
			{
				return this.Context.Errorvisitor;
			}
		}

		// Token: 0x17000820 RID: 2080
		// (get) Token: 0x060031D4 RID: 12756 RVA: 0x000C0AFC File Offset: 0x000BECFC
		private _IExpressionTypifier Expressiontypifier
		{
			get
			{
				return this.Context.Expressiontypifier;
			}
		}

		// Token: 0x17000821 RID: 2081
		// (get) Token: 0x060031D5 RID: 12757 RVA: 0x000C0B0C File Offset: 0x000BED0C
		private \u001F.\u0007 Typechecker
		{
			get
			{
				return this.Context.Typechecker;
			}
		}

		// Token: 0x17000822 RID: 2082
		// (get) Token: 0x060031D6 RID: 12758 RVA: 0x000C0B1C File Offset: 0x000BED1C
		private global::\u0019.\u0012 UDTypeCompiler { get; }

		// Token: 0x17000823 RID: 2083
		// (get) Token: 0x060031D7 RID: 12759 RVA: 0x000C0B24 File Offset: 0x000BED24
		private _ISignature UserdefSignature
		{
			get
			{
				return this.UDTypeCompiler.UserdefSignature;
			}
		}

		// Token: 0x17000824 RID: 2084
		// (get) Token: 0x060031D8 RID: 12760 RVA: 0x000C0B34 File Offset: 0x000BED34
		internal _IType GeneratedType
		{
			get
			{
				return this.UDTypeCompiler.GeneratedType;
			}
		}

		// Token: 0x17000825 RID: 2085
		// (get) Token: 0x060031D9 RID: 12761 RVA: 0x000C0B44 File Offset: 0x000BED44
		private GenericTypeChecker GenericTypeChecker { get; }

		// Token: 0x060031DA RID: 12762 RVA: 0x000C0B4C File Offset: 0x000BED4C
		internal GenericTypeCompiler(global::\u000E.\u0017 context, bool bReferencingCrossReferences)
		{
			this.GenericTypeChecker = this.\u0002();
			this.UDTypeCompiler = new global::\u0019.\u0012(context, bReferencingCrossReferences, this.GenericTypeChecker);
			this.Context = context;
		}

		// Token: 0x060031DB RID: 12763 RVA: 0x000C0B7C File Offset: 0x000BED7C
		internal GenericTypeChecker \u0002()
		{
			return new GenericTypeChecker(new Action<_IExpression, string, MessageId>(this.\u0001));
		}

		// Token: 0x060031DC RID: 12764 RVA: 0x000C0B90 File Offset: 0x000BED90
		public void \u0001(IGenericUserdefType \u0002)
		{
			this.UDTypeCompiler.\u0001(\u0002);
			if (this.UserdefSignature == null)
			{
				return;
			}
			if (this.Context.SignDecl != null)
			{
				this.Context.SignDecl.SetFlagInternal(SignatureFlagInternal.ContainsGenericInstanceVar, true);
			}
			IGenericUserdefType2 genericUserdefType = \u0002 as IGenericUserdefType2;
			if (genericUserdefType != null)
			{
				genericUserdefType.OriginalDeclaration = \u0002.ToString();
			}
			_IVariable[] array = this.UserdefSignature.AllVariables.Where(new Func<_IVariable, bool>(GenericTypeCompiler.<>c.<>9.\u0001)).ToArray<_IVariable>();
			int num = 0;
			if (!this.\u0001(\u0002))
			{
				return;
			}
			if (!this.\u0001(array, \u0002))
			{
				return;
			}
			this.\u0001(array, \u0002);
			foreach (_IExpression iexpression in \u0002.GenericConstantsInitializations)
			{
				if (num >= array.Length)
				{
					break;
				}
				_IVariable ivariable = array[num];
				if (this.Context.SignDecl != null)
				{
					ivariable.AddCrossReference(this.Context.SignDecl.Id, null);
				}
				iexpression.Accept(this.Expressiontypifier);
				this.GenericTypeChecker.\u0001(iexpression, ivariable, (global::\u0017.\u0006)this.Scope);
				_IExpression iexpression2 = iexpression;
				this.Typechecker.\u0001(iexpression, iexpression._CompiledType, ivariable.OriginalType, this.Scope as global::\u0007.\u0005, ref iexpression2);
				iexpression2.Accept(this.Errorvisitor);
				num++;
			}
			if (this.Context.InterfaceCompiler != null)
			{
				_ISignature isignature = this.Context.InterfaceCompiler.ConstGenericController.\u0001(this.UserdefSignature, \u0002, this.Context);
				((_IUserdefType)this.GeneratedType).SignatureId = isignature.Id;
				return;
			}
			string stName = global::\u0018.\u0003.\u0001(this.UserdefSignature.Name, \u0002.GenericConstantsInitializations);
			ISignature[] array2 = this.Context.Scope.FindSignature(stName);
			if (array2 != null && array2.Length == 1)
			{
				((_IUserdefType)this.GeneratedType).SignatureId = array2[0].Id;
			}
		}

		// Token: 0x060031DD RID: 12765 RVA: 0x000C0DB4 File Offset: 0x000BEFB4
		private void \u0001(_IVariable[] \u0002, IGenericUserdefType \u0003)
		{
			_IExpression[] array = new _IExpression[\u0002.Length];
			bool flag = false;
			foreach (_IAssignmentExpression iassignmentExpression in \u0003.GenericConstantsInitializations.OfType<_IAssignmentExpression>())
			{
				int num = this.\u0001(\u0002, iassignmentExpression);
				if (num < 0)
				{
					return;
				}
				array[num] = iassignmentExpression._RValue;
				flag = true;
			}
			if (!flag)
			{
				return;
			}
			for (int i = 0; i < array.Length; i++)
			{
				\u0003.SetGenericConstantInitialization(array[i], i);
			}
		}

		// Token: 0x060031DE RID: 12766 RVA: 0x000C0E4C File Offset: 0x000BF04C
		private int \u0001(_IVariable[] \u0002, _IAssignmentExpression \u0003)
		{
			for (int i = 0; i < \u0002.Length; i++)
			{
				if (string.Equals(\u0002[i].Name, \u0003.LValue.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x060031DF RID: 12767 RVA: 0x000C0E88 File Offset: 0x000BF088
		private bool \u0001(_IVariable[] \u0002, IGenericUserdefType \u0003)
		{
			return this.GenericTypeChecker.\u0001(\u0002, \u0003);
		}

		// Token: 0x060031E0 RID: 12768 RVA: 0x000C0E98 File Offset: 0x000BF098
		private bool \u0001(IGenericUserdefType \u0002)
		{
			return this.GenericTypeChecker.\u0001(\u0002);
		}

		// Token: 0x060031E1 RID: 12769 RVA: 0x000C0EA8 File Offset: 0x000BF0A8
		private void \u0001(_IExpression \u0002, string \u0003, MessageId \u0004)
		{
			\u0002.AddError(\u0003, \u0004);
			\u0002.Accept(this.Errorvisitor);
		}

		// Token: 0x04000968 RID: 2408
		[CompilerGenerated]
		private readonly global::\u000E.\u0017 \u0001;

		// Token: 0x04000969 RID: 2409
		[CompilerGenerated]
		private readonly global::\u0019.\u0012 \u0001;

		// Token: 0x0400096A RID: 2410
		[CompilerGenerated]
		private readonly GenericTypeChecker \u0001;
	}
}
