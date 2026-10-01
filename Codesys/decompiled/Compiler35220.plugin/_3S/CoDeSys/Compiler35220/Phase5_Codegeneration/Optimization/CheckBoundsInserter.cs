using System;
using \u0011;
using \u0019;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000272 RID: 626
	public class CheckBoundsInserter
	{
		// Token: 0x060027DE RID: 10206 RVA: 0x0008A9F4 File Offset: 0x00088BF4
		public CheckBoundsInserter(CheckFunctionReplacerContext checkFunctionReplacerContext)
		{
			this.\u0001 = checkFunctionReplacerContext;
		}

		// Token: 0x060027DF RID: 10207 RVA: 0x0008AA04 File Offset: 0x00088C04
		public _IExpression ReplaceIndexAccess(_IIndexAccessExpression indexAccessExpression)
		{
			this.\u0001(indexAccessExpression);
			this.\u0002(indexAccessExpression);
			return indexAccessExpression;
		}

		// Token: 0x060027E0 RID: 10208 RVA: 0x0008AA18 File Offset: 0x00088C18
		private void \u0001(_IIndexAccessExpression \u0002)
		{
			_IArrayType iarrayType = \u0002._Var.Type.DeRefType as _IArrayType;
			for (int i = 0; i < \u0002.NumAccesses; i++)
			{
				if (iarrayType != null)
				{
					this.\u0001(\u0002, iarrayType, i);
				}
			}
		}

		// Token: 0x060027E1 RID: 10209 RVA: 0x0008AA58 File Offset: 0x00088C58
		private void \u0001(_IIndexAccessExpression \u0002, _IArrayType \u0003, int \u0004)
		{
			if (\u0002[\u0004].Literal(this.\u0001._Scope) != null)
			{
				return;
			}
			if (this.\u0001.CheckFunctions.m_stCheckBoundsFun != null && this.\u0001.DoChecks())
			{
				bool flag;
				int u = \u0003._Dimensions[\u0004].LowerBorderInt(out flag, this.\u0001._Scope);
				bool flag2;
				int u2 = \u0003._Dimensions[\u0004].UpperBorderInt(out flag2, this.\u0001._Scope);
				_IExpression u3 = \u0002[\u0004];
				if (flag && flag2 && this.\u0001.CheckFunctions.CheckForCheckFunHide(this.\u0001._Scope, this.\u0001.CheckFunctions.m_stCheckBoundsFun))
				{
					this.\u0001(\u0002, \u0004, u3, u, u2);
				}
			}
		}

		// Token: 0x060027E2 RID: 10210 RVA: 0x0008AB2C File Offset: 0x00088D2C
		private void \u0001(_IIndexAccessExpression \u0002, int \u0003, _IExpression \u0004, int \u0005, int \u0006)
		{
			_ILiteralExpression u = \u0019.\u0003.\u0001((long)\u0005);
			_ILiteralExpression u2 = \u0019.\u0003.\u0001((long)\u0006);
			this.\u0001(\u0002, \u0003, \u0004, u, u2);
		}

		// Token: 0x060027E3 RID: 10211 RVA: 0x0008AB58 File Offset: 0x00088D58
		private void \u0001(_IIndexAccessExpression \u0002, int \u0003, _IExpression \u0004, IVariable \u0005)
		{
			_ICompoAccessExpression u = this.\u0001(\u0005, \u0003, "diLower");
			_ICompoAccessExpression u2 = this.\u0001(\u0005, \u0003, "diUpper");
			this.\u0001(\u0002, \u0003, \u0004, u, u2);
		}

		// Token: 0x060027E4 RID: 10212 RVA: 0x0008AB90 File Offset: 0x00088D90
		private void \u0001(_IIndexAccessExpression \u0002, int \u0003, _IExpression \u0004, _IExpression \u0005, _IExpression \u0006)
		{
			_ICallExpression icallExpression = \u0019.\u0003.\u0001(new global::\u0011.\u0006(this.\u0001.CheckFunctions.m_stCheckBoundsFun).\u0002() as _IExpression, Token.Empty);
			icallExpression.AddParam(\u0004.Duplicate() as _IExpression);
			icallExpression.AddParam(\u0005);
			icallExpression.AddParam(\u0006);
			icallExpression = this.\u0001.Generator.\u0001<_ICallExpression>(icallExpression, this.\u0001._Scope, this.\u0001.CompiledPOU);
			icallExpression.SetPositionIntern(\u0004._Position);
			OptionalInputsProvider.\u0001(this.\u0001.\u0001, icallExpression);
			\u0002[\u0003] = icallExpression;
		}

		// Token: 0x060027E5 RID: 10213 RVA: 0x0008AC38 File Offset: 0x00088E38
		private _ICompoAccessExpression \u0001(IVariable \u0002, int \u0003, string \u0004)
		{
			IExpression u = \u0019.\u0003.\u0001(\u0002.OrgName + "__Array__Info");
			_IVariableExpression u2 = \u0019.\u0003.\u0001(\u0004);
			_ILiteralExpression u3 = \u0019.\u0003.\u0001((long)(1 + \u0003));
			return \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(u, u3), u2);
		}

		// Token: 0x060027E6 RID: 10214 RVA: 0x0008AC78 File Offset: 0x00088E78
		public static bool IsVarLenArray(_IIndexAccessExpression indexaccess, IScope scope, out IVariable varLenArrayVar)
		{
			varLenArrayVar = null;
			if (indexaccess.Var is _IIndexAccessExpression)
			{
				return false;
			}
			if (indexaccess._Var.Type.DeRefType is _IPointerType)
			{
				varLenArrayVar = ((_IExpression)indexaccess.Var).GetVariable(scope);
				return varLenArrayVar != null && varLenArrayVar.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY);
			}
			return false;
		}

		// Token: 0x060027E7 RID: 10215 RVA: 0x0008ACD8 File Offset: 0x00088ED8
		internal void \u0002(_IIndexAccessExpression \u0002)
		{
			IVariable u;
			if (CheckBoundsInserter.IsVarLenArray(\u0002, this.\u0001._Scope, out u))
			{
				for (int i = 0; i < \u0002.NumAccesses; i++)
				{
					this.\u0001(u, \u0002, i);
				}
			}
		}

		// Token: 0x060027E8 RID: 10216 RVA: 0x0008AD14 File Offset: 0x00088F14
		private void \u0001(IVariable \u0002, _IIndexAccessExpression \u0003, int \u0004)
		{
			if (this.\u0001.CheckFunctions.m_stCheckBoundsFun != null && this.\u0001.DoChecks() && this.\u0001.CheckFunctions.CheckForCheckFunHide(this.\u0001._Scope, this.\u0001.CheckFunctions.m_stCheckBoundsFun))
			{
				this.\u0001(\u0003, \u0004, \u0003[\u0004], \u0002);
			}
		}

		// Token: 0x04000761 RID: 1889
		private CheckFunctionReplacerContext \u0001;
	}
}
