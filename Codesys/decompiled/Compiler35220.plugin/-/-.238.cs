using System;
using System.Runtime.CompilerServices;
using \u000E;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0002
{
	// Token: 0x02000297 RID: 663
	internal sealed class \u0007
	{
		// Token: 0x1700074A RID: 1866
		// (get) Token: 0x06002A1A RID: 10778 RVA: 0x00092D74 File Offset: 0x00090F74
		private global::\u000E.\u0011 Context { get; }

		// Token: 0x1700074B RID: 1867
		// (get) Token: 0x06002A1B RID: 10779 RVA: 0x00092D7C File Offset: 0x00090F7C
		private StructAndArrayInitReplacer Visitor { get; }

		// Token: 0x06002A1C RID: 10780 RVA: 0x00092D84 File Offset: 0x00090F84
		public \u0007(global::\u000E.\u0011 \u0083\u0005, StructAndArrayInitReplacer \u001E\u0006)
		{
			this.Context = \u0083\u0005;
			this.Visitor = \u001E\u0006;
		}

		// Token: 0x06002A1D RID: 10781 RVA: 0x00092D9C File Offset: 0x00090F9C
		public _IStatement \u0001(_IAssignmentExpression \u0002, bool \u0003)
		{
			_IStructureInitialization istructureInitialization = \u0002._RValue as _IStructureInitialization;
			if (istructureInitialization != null)
			{
				return this.\u0001(\u0002, \u0003, istructureInitialization);
			}
			return null;
		}

		// Token: 0x06002A1E RID: 10782 RVA: 0x00092DC4 File Offset: 0x00090FC4
		private _IStatement \u0001(_IAssignmentExpression \u0002, bool \u0003, _IStructureInitialization \u0004)
		{
			if (\u0002._LValue.GetVariable(this.Context._Scope).HasAttribute("no_default_init"))
			{
				_ISequenceStatement isequenceStatement = \u0019.\u0003.\u0001();
				foreach (_IAssignmentExpression iassignmentExpression in \u0004._CompoInits)
				{
					_ICompoAccessExpression icompoAccessExpression = \u0019.\u0003.\u0001();
					icompoAccessExpression._Left = (\u0002._LValue.Duplicate() as _IExpression);
					icompoAccessExpression._Right = (iassignmentExpression._LValue.Duplicate() as _IExpression);
					_IAssignmentExpression iassignmentExpression2 = \u0019.\u0003.\u0001();
					iassignmentExpression2._LValue = icompoAccessExpression;
					iassignmentExpression2._RValue = (iassignmentExpression._RValue.Duplicate() as _IExpression);
					isequenceStatement.Add(\u0019.\u0003.\u0001(iassignmentExpression2, Token.Empty));
				}
				return this.\u0001(isequenceStatement);
			}
			return this.\u0002(\u0002, \u0003, \u0004);
		}

		// Token: 0x06002A1F RID: 10783 RVA: 0x00092EB8 File Offset: 0x000910B8
		private _IStatement \u0001(_ISequenceStatement \u0002)
		{
			_ISequenceStatement isequenceStatement = this.Context.Generator.\u0001<_ISequenceStatement>(\u0002, this.Context._Scope, this.Context.CompiledPOU);
			this.Visitor.ReplaceInCode(isequenceStatement);
			return isequenceStatement;
		}

		// Token: 0x06002A20 RID: 10784 RVA: 0x00092F04 File Offset: 0x00091104
		private _IStatement \u0002(_IAssignmentExpression \u0002, bool \u0003, _IStructureInitialization \u0004)
		{
			_ISequenceStatement isequenceStatement = \u0019.\u0003.\u0001();
			foreach (_IAssignmentExpression iassignmentExpression in \u0004._CompoInits)
			{
				_IAssignmentExpression iassignmentExpression2 = \u0019.\u0003.\u0001();
				iassignmentExpression2._Position = iassignmentExpression._Position;
				if (iassignmentExpression._LValue.Type.Class == TypeClass.Reference)
				{
					iassignmentExpression2.KindOf = Operator.RefAssign;
				}
				_ICompoAccessExpression lvalue = \u0019.\u0003.\u0001((IExpression)\u0002._LValue.Duplicate(), (IVariableExpression2)iassignmentExpression._LValue.Duplicate());
				iassignmentExpression2._LValue = lvalue;
				iassignmentExpression2._RValue = (_IExpression)iassignmentExpression._RValue.Duplicate();
				isequenceStatement.Add(\u0019.\u0003.\u0001(iassignmentExpression2));
			}
			_IUserdefType iuserdefType = \u0002._LValue.Type as _IUserdefType;
			if (iuserdefType != null && \u0003)
			{
				ISignature signature = iuserdefType.GetSignature(this.Context._Scope);
				while (signature != null && signature.HasAttribute(CompileAttributes.ATTRIBUTE_CALL_AFTER_INIT))
				{
					foreach (ISignature signature2 in signature.SubSignatures)
					{
						if (signature2.HasAttribute(CompileAttributes.ATTRIBUTE_CALL_AFTER_INIT))
						{
							_ICallExpression u = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001((IExpression)\u0002._LValue.Duplicate(), \u0019.\u0003.\u0001(signature2.OrgName)));
							isequenceStatement.Add(\u0019.\u0003.\u0001(u));
						}
					}
					signature = this.Context._Scope[signature.BaseSignatureId];
				}
			}
			_IStatement istatement = this.\u0001(isequenceStatement);
			istatement.SetFlag(StatementFlag.GenerateFlow | StatementFlag.GenerateBP, false);
			return istatement;
		}

		// Token: 0x040007B2 RID: 1970
		[CompilerGenerated]
		private readonly global::\u000E.\u0011 \u0001;

		// Token: 0x040007B3 RID: 1971
		[CompilerGenerated]
		private readonly StructAndArrayInitReplacer \u0001;
	}
}
