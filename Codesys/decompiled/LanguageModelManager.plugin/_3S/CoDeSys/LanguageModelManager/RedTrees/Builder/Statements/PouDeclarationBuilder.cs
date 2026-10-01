using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Compiler.LanguageModelBuilder;
using _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements
{
	// Token: 0x0200026B RID: 619
	public class PouDeclarationBuilder : IPouDeclarationBuilder, IPouDeclarationBuilderOptionalPosStep, ILmbPositional<IPouDeclarationBuilderClass>, IPouDeclarationBuilderClass, IPouDeclarationBuilderOptionalExtends, IPouDeclarationBuilderExtends, IPouDeclarationBuilderOptionalImplements, IPouDeclarationBuilderImplements, IPouDeclarationBuilderOptionalReturns, IPouDeclarationBuilderReturns, IPouDeclarationBuilderDeclaration, ILmbExprementBuilder<IPOUDeclarationStatement>, IPouDeclarationBuilderOptionalAccess, IPouDeclarationBuilderAccess, IPouDeclarationBuilderName
	{
		// Token: 0x06002A07 RID: 10759 RVA: 0x0006B088 File Offset: 0x0006A088
		public IPouDeclarationBuilderClass At(IExprementPosition position)
		{
			this._pos = position;
			return this;
		}

		// Token: 0x06002A08 RID: 10760 RVA: 0x0006B092 File Offset: 0x0006A092
		public IPouDeclarationBuilderOptionalAccess AsProgram()
		{
			this._operator = Operator.Program;
			return this;
		}

		// Token: 0x06002A09 RID: 10761 RVA: 0x0006B09D File Offset: 0x0006A09D
		public IPouDeclarationBuilderOptionalAccess AsFunction()
		{
			this._operator = Operator.Function;
			return this;
		}

		// Token: 0x06002A0A RID: 10762 RVA: 0x0006B0A8 File Offset: 0x0006A0A8
		public IPouDeclarationBuilderOptionalAccess AsFunctionBlock()
		{
			this._operator = Operator.FunctionBlock;
			return this;
		}

		// Token: 0x06002A0B RID: 10763 RVA: 0x0006B0B3 File Offset: 0x0006A0B3
		public IPouDeclarationBuilderOptionalAccess AsInterface()
		{
			this._operator = Operator.Interface;
			return this;
		}

		// Token: 0x06002A0C RID: 10764 RVA: 0x0006B0BE File Offset: 0x0006A0BE
		public IPouDeclarationBuilderOptionalAccess AsMethod()
		{
			this._operator = Operator.Method;
			return this;
		}

		// Token: 0x06002A0D RID: 10765 RVA: 0x0006B0C9 File Offset: 0x0006A0C9
		public IPouDeclarationBuilderOptionalAccess AsAction()
		{
			this._operator = Operator.Action;
			return this;
		}

		// Token: 0x06002A0E RID: 10766 RVA: 0x0006B0D4 File Offset: 0x0006A0D4
		public IPouDeclarationBuilderName Access(params SignatureFlag[] flags)
		{
			SignatureFlag signatureFlag = SignatureFlag.None;
			foreach (SignatureFlag signatureFlag2 in flags)
			{
				signatureFlag |= signatureFlag2;
			}
			this._signatureFlag = signatureFlag;
			return this;
		}

		// Token: 0x06002A0F RID: 10767 RVA: 0x0006B104 File Offset: 0x0006A104
		public IPouDeclarationBuilderOptionalExtends Name(string name)
		{
			this._name = name;
			return this;
		}

		// Token: 0x06002A10 RID: 10768 RVA: 0x0006B10E File Offset: 0x0006A10E
		public IPouDeclarationBuilderOptionalImplements Extends(params IExpression[] expressions)
		{
			this._extends = expressions;
			return this;
		}

		// Token: 0x06002A11 RID: 10769 RVA: 0x0006B118 File Offset: 0x0006A118
		public IPouDeclarationBuilderOptionalReturns Implements(params IExpression[] expressions)
		{
			this._implements = expressions;
			return this;
		}

		// Token: 0x06002A12 RID: 10770 RVA: 0x0006B122 File Offset: 0x0006A122
		public IPouDeclarationBuilderDeclaration Returns(ICompiledType type)
		{
			this._returns = type;
			return this;
		}

		// Token: 0x06002A13 RID: 10771 RVA: 0x0006B12C File Offset: 0x0006A12C
		public IPouDeclarationBuilderDeclaration AddVariableDeclarations(IVariableDeclarationListStatement variableDeclarationListStatement)
		{
			this._decls.Add(variableDeclarationListStatement);
			return this;
		}

		// Token: 0x06002A14 RID: 10772 RVA: 0x0006B13C File Offset: 0x0006A13C
		public IPOUDeclarationStatement Build()
		{
			if (this._pos == null)
			{
				this._pos = new ExprementPosition(0L, 0);
			}
			if (this._extends == null)
			{
				this._extends = Array.Empty<IExpression>();
			}
			if (this._implements == null)
			{
				this._implements = Array.Empty<IExpression>();
			}
			return LanguageModelBuilder.Singleton.CreatePOUDeclarationStatement(this._pos, this._operator, this._name, this._returns, this._decls, this._extends.ToList<IExpression>(), this._implements.ToList<IExpression>(), this._signatureFlag);
		}

		// Token: 0x06002A15 RID: 10773 RVA: 0x0006B1C9 File Offset: 0x0006A1C9
		public static IPouDeclarationBuilderOptionalPosStep Init()
		{
			return new PouDeclarationBuilder();
		}

		// Token: 0x040007E9 RID: 2025
		private readonly List<IVariableDeclarationListStatement> _decls = new List<IVariableDeclarationListStatement>();

		// Token: 0x040007EA RID: 2026
		private IExpression[] _extends;

		// Token: 0x040007EB RID: 2027
		private IExpression[] _implements;

		// Token: 0x040007EC RID: 2028
		private string _name;

		// Token: 0x040007ED RID: 2029
		private Operator _operator;

		// Token: 0x040007EE RID: 2030
		private IExprementPosition _pos;

		// Token: 0x040007EF RID: 2031
		private ICompiledType _returns;

		// Token: 0x040007F0 RID: 2032
		private SignatureFlag _signatureFlag;
	}
}
