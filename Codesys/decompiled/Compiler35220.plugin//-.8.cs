using System;
using \u0019;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u007F
{
	// Token: 0x0200028D RID: 653
	internal readonly struct \u0008
	{
		// Token: 0x06002930 RID: 10544 RVA: 0x0008F8D0 File Offset: 0x0008DAD0
		public \u0008(_ICompileContext \u0001\u0002)
		{
			this.\u0001 = \u0001\u0002;
		}

		// Token: 0x06002931 RID: 10545 RVA: 0x0008F8DC File Offset: 0x0008DADC
		public _IDeRefAccessExpression \u0001(_IExpression \u0002, _IType \u0003)
		{
			_IDeRefAccessExpression ideRefAccessExpression = \u0003.\u0001(\u0002);
			ideRefAccessExpression.Type = \u0003;
			return ideRefAccessExpression;
		}

		// Token: 0x06002932 RID: 10546 RVA: 0x0008F8EC File Offset: 0x0008DAEC
		public _IVariableExpression \u0001(_IVariable \u0002, _ISignature \u0003)
		{
			_IVariableExpression ivariableExpression = \u0003.\u0001(\u0002, \u0003);
			ivariableExpression._CompiledType = \u0002.CompiledType;
			return ivariableExpression;
		}

		// Token: 0x06002933 RID: 10547 RVA: 0x0008F904 File Offset: 0x0008DB04
		public _IOperatorExpression \u0001(Operator \u0002, _IExpression \u0003, _IExpression \u0004, _IType \u0005)
		{
			_IOperatorExpression ioperatorExpression = \u0003.\u0001(\u0002, \u0003, \u0004);
			ioperatorExpression._CompiledType = \u0005;
			return ioperatorExpression;
		}

		// Token: 0x06002934 RID: 10548 RVA: 0x0008F918 File Offset: 0x0008DB18
		public _IOperatorExpression \u0001(Operator \u0002, _IExpression \u0003, _IType \u0004)
		{
			_IOperatorExpression ioperatorExpression = \u0003.\u0001(\u0002, \u0003);
			ioperatorExpression._CompiledType = \u0004;
			return ioperatorExpression;
		}

		// Token: 0x06002935 RID: 10549 RVA: 0x0008F928 File Offset: 0x0008DB28
		public _ICompoAccessExpression \u0001(_IExpression \u0002, _IVariableExpression \u0003, _IType \u0004)
		{
			_ICompoAccessExpression icompoAccessExpression = \u0003.\u0001(\u0002, \u0003);
			icompoAccessExpression.Type = \u0004;
			return icompoAccessExpression;
		}

		// Token: 0x06002936 RID: 10550 RVA: 0x0008F938 File Offset: 0x0008DB38
		private _IAssignmentExpression \u0001(_IExpression \u0002, _IExpression \u0003)
		{
			_IAssignmentExpression iassignmentExpression = \u0003.\u0001(\u0002, \u0003);
			iassignmentExpression.Type = \u0002.Type;
			return iassignmentExpression;
		}

		// Token: 0x06002937 RID: 10551 RVA: 0x0008F950 File Offset: 0x0008DB50
		public _IExpressionStatement \u0001(_IExpression \u0002, _IExpression \u0003)
		{
			return \u0003.\u0001(this.\u0001(\u0002, \u0003));
		}

		// Token: 0x06002938 RID: 10552 RVA: 0x0008F960 File Offset: 0x0008DB60
		public _IExpressionStatement \u0001(_IExpression \u0002, _ISignature \u0003, _IVariable \u0004, _IExpression \u0005)
		{
			return this.\u0001(this.\u0001((_IExpression)\u0002.Duplicate(), this.\u0001(\u0004, \u0003), \u0004._Type), \u0005);
		}

		// Token: 0x06002939 RID: 10553 RVA: 0x0008F98C File Offset: 0x0008DB8C
		public _IIndexAccessExpression \u0001(_IExpression \u0002, _IExpression \u0003, _IType \u0004)
		{
			_IIndexAccessExpression iindexAccessExpression = \u0003.\u0001(\u0002, \u0003);
			iindexAccessExpression.Type = \u0004;
			return iindexAccessExpression;
		}

		// Token: 0x0600293A RID: 10554 RVA: 0x0008F99C File Offset: 0x0008DB9C
		public _IConversionExpression \u0001(_IType \u0002, _IType \u0003, _IExpression \u0004)
		{
			_IConversionExpression iconversionExpression = \u0003.\u0001(\u0002.Class, \u0003.Class, \u0004);
			iconversionExpression.Type = \u0003;
			return iconversionExpression;
		}

		// Token: 0x0600293B RID: 10555 RVA: 0x0008F9B8 File Offset: 0x0008DBB8
		public _ILiteralExpression \u0001(long \u0002)
		{
			_ILiteralExpression iliteralExpression = \u0003.\u0001(\u0002, TypeClass.Int);
			iliteralExpression.Type = \u0003.\u0001();
			return iliteralExpression;
		}

		// Token: 0x0600293C RID: 10556 RVA: 0x0008F9CC File Offset: 0x0008DBCC
		public _IExpression \u0001(ICompiledType \u0002)
		{
			TypeClass typeClass = (this.\u0001.PointerSize == 8) ? TypeClass.LWord : TypeClass.DWord;
			_ILiteralExpression iliteralExpression = \u0003.\u0001(0L, typeClass);
			iliteralExpression.Type = TypeTable.Get(typeClass);
			return iliteralExpression;
		}

		// Token: 0x0400078E RID: 1934
		private readonly _ICompileContext \u0001;
	}
}
