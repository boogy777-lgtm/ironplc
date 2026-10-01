using System;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0001
{
	// Token: 0x02000070 RID: 112
	internal sealed class \u0001 : StandardTraverser
	{
		// Token: 0x060008C7 RID: 2247 RVA: 0x000121A0 File Offset: 0x000103A0
		public \u0001(IExprementVisitorNoTraversion351300 \u009D, bool \u009E) : base(\u009D, \u009E)
		{
		}

		// Token: 0x060008C8 RID: 2248 RVA: 0x000121AC File Offset: 0x000103AC
		public override void visit(_IEnumDeclarationStatement eds)
		{
			this._expCalledForAll.visit(eds);
			if (eds._Value != null && !this.m_bNoInit)
			{
				eds._Value.Accept(this);
			}
		}

		// Token: 0x060008C9 RID: 2249 RVA: 0x000121D8 File Offset: 0x000103D8
		public override void visit(_ITypeDeclarationStatement tds)
		{
			this._expCalledForAll.visit(tds);
			tds.Declarations.Accept(this);
			if (tds.Initial != null && !this.m_bNoInit)
			{
				tds.Initial.Accept(this);
			}
			if (tds.Extends != null)
			{
				tds.Extends.Accept(this);
			}
		}

		// Token: 0x060008CA RID: 2250 RVA: 0x00012230 File Offset: 0x00010430
		public override void visit(_IPOUDeclarationStatement pds)
		{
			this._expCalledForAll.visit(pds);
			pds.Declarations.Accept(this);
			if (pds.Implements != null)
			{
				foreach (_IExpression iexpression in pds.Implements)
				{
					iexpression.Accept(this);
				}
			}
			if (pds.Extends != null)
			{
				foreach (_IExpression iexpression2 in pds.Extends)
				{
					iexpression2.Accept(this);
				}
			}
		}

		// Token: 0x060008CB RID: 2251 RVA: 0x000122E0 File Offset: 0x000104E0
		public override void visit(_IVariableDeclarationListStatement vdls)
		{
			this._expCalledForAll.visit(vdls);
			bool bNoInit = this.m_bNoInit;
			if (vdls.GetFlag(VarFlag.Constant))
			{
				this.m_bNoInit = false;
			}
			vdls.VariableDeclaration.Accept(this);
			this.m_bNoInit = bNoInit;
		}
	}
}
