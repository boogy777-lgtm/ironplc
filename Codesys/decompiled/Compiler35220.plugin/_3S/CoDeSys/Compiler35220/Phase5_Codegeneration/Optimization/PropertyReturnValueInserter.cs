using System;
using System.Linq;
using System.Runtime.CompilerServices;
using \u000E;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x020002BE RID: 702
	public class PropertyReturnValueInserter : IReplacer
	{
		// Token: 0x17000772 RID: 1906
		// (get) Token: 0x06002AE8 RID: 10984 RVA: 0x00096EAC File Offset: 0x000950AC
		private global::\u000E.\u0011 Context { get; }

		// Token: 0x06002AE9 RID: 10985 RVA: 0x00096EB4 File Offset: 0x000950B4
		internal PropertyReturnValueInserter(global::\u000E.\u0011 context)
		{
			this.Context = context;
		}

		// Token: 0x06002AEA RID: 10986 RVA: 0x00096EC4 File Offset: 0x000950C4
		private _IStatement \u0001(_ISignature \u0002, _ICompiledPOU \u0003, string \u0004)
		{
			if (\u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING) == CompileAttributes.ATTRIBUTEVALUE_VARIABLE)
			{
				_ISignature isignature = this.Context.Comcon[\u0002.ParentSignatureId];
				IVariable variable = (_IVariable)\u0002[\u0004];
				Operator u = Operator.Assign;
				if (variable.Type.Class == TypeClass.Reference)
				{
					u = Operator.RefAssign;
				}
				_IExpressionStatement iexpressionStatement = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(\u0019.\u0003.\u0001("__Watch_" + isignature.OrgName + "_" + \u0004), \u0019.\u0003.\u0001(\u0004), u));
				this.Context.Generator.\u0001<_IExpressionStatement>(iexpressionStatement, this.Context._Scope, \u0003);
				return iexpressionStatement;
			}
			return null;
		}

		// Token: 0x06002AEB RID: 10987 RVA: 0x00096F80 File Offset: 0x00095180
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			_ISignature isignature = this.Context.Comcon[cpou.SignatureId];
			if (!isignature.GetFlag(SignatureFlag.RawSTProperty))
			{
				return;
			}
			string attributeValue = isignature.GetAttributeValue(CompileAttributes.ATTRIBUTE_PROPERTY);
			_IStatement istatement = this.\u0001(isignature, cpou, attributeValue);
			bool flag = isignature.Name.StartsWith("__SET");
			_ISequenceStatement isequenceStatement = (_ISequenceStatement)cpou.GetParseTree();
			if (istatement != null && flag)
			{
				isequenceStatement.InsertStatement(0, \u0019.\u0003.\u0001("implicit off"));
				isequenceStatement.InsertStatement(0, istatement);
				isequenceStatement.InsertStatement(0, \u0019.\u0003.\u0001("implicit on"));
			}
			if (!isignature.Name.StartsWith("__GET"))
			{
				return;
			}
			if (isequenceStatement._StatementList.OfType<_IPragmaStatement>().FirstOrDefault(new Func<_IPragmaStatement, bool>(PropertyReturnValueInserter.<>c.<>9.\u0001)) == null)
			{
				IStatement state = \u0019.\u0003.\u0001("returnlabelposition");
				isequenceStatement.AddStatement(state);
			}
			isequenceStatement.AddStatement(\u0019.\u0003.\u0001("implicit on"));
			if (istatement != null)
			{
				isequenceStatement.AddStatement(istatement);
			}
			_IVariableExpression u = \u0019.\u0003.\u0001(isignature.OrgName);
			_IVariableExpression u2 = \u0019.\u0003.\u0001(attributeValue);
			IVariable variable = (_IVariable)isignature[attributeValue];
			_IAssignmentExpression iassignmentExpression = \u0019.\u0003.\u0001(u, u2);
			if (variable.Type.Class == TypeClass.Reference)
			{
				iassignmentExpression.KindOf = Operator.RefAssign;
			}
			_IExpressionStatement iexpressionStatement = \u0019.\u0003.\u0001(iassignmentExpression);
			this.Context.Generator.\u0001<_IExpressionStatement>(iexpressionStatement, this.Context._Scope, cpou);
			iexpressionStatement.SetFlag(StatementFlag.GenerateBP, true);
			isequenceStatement.AddStatement(iexpressionStatement);
			isequenceStatement.AddStatement(\u0019.\u0003.\u0001("implicit off"));
		}

		// Token: 0x0400081E RID: 2078
		[CompilerGenerated]
		private readonly global::\u000E.\u0011 \u0001;
	}
}
