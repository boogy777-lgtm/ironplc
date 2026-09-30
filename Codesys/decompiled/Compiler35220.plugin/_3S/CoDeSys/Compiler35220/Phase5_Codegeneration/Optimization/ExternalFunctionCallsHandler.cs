using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using \u0019;
using _3S.CoDeSys.Compiler35220.Compile.Phase1_Typification.Code;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x020002A5 RID: 677
	public class ExternalFunctionCallsHandler
	{
		// Token: 0x1700075A RID: 1882
		// (get) Token: 0x06002A7E RID: 10878 RVA: 0x000949F0 File Offset: 0x00092BF0
		private ICodegenerator CodeGen { get; }

		// Token: 0x1700075B RID: 1883
		// (get) Token: 0x06002A7F RID: 10879 RVA: 0x000949F8 File Offset: 0x00092BF8
		private IScope5 Scope { get; }

		// Token: 0x1700075C RID: 1884
		// (get) Token: 0x06002A80 RID: 10880 RVA: 0x00094A00 File Offset: 0x00092C00
		private LateCodeGenerator Generator { get; }

		// Token: 0x1700075D RID: 1885
		// (get) Token: 0x06002A81 RID: 10881 RVA: 0x00094A08 File Offset: 0x00092C08
		private _ICompileContext ComCon { get; }

		// Token: 0x06002A82 RID: 10882 RVA: 0x00094A10 File Offset: 0x00092C10
		public ExternalFunctionCallsHandler(ICodegenerator codegenerator, _ICompileContext comcon, LateCodeGenerator generator, IScope5 scope)
		{
			this.CodeGen = codegenerator;
			this.Scope = scope;
			this.Generator = generator;
			this.ComCon = comcon;
		}

		// Token: 0x06002A83 RID: 10883 RVA: 0x00094A38 File Offset: 0x00092C38
		public bool HandleExternalFunctionCalls(_IOperatorExpression op, _ICompiledPOU cpou, out _IExpression replaced)
		{
			replaced = op;
			if (op.Code == Operator.__MemoryBarrier || op.Code == Operator.__AdrInst)
			{
				return false;
			}
			string empty = string.Empty;
			TypeClass u = TypeClass.None;
			if (this.CodeGen.NeedsExternalFunctionCall(op, op.Type, ref empty, ref u))
			{
				replaced = this.\u0001(empty, TypeClass.None, u, op.Operands);
				this.Generator.\u0001<_IExpression>(replaced, this.Scope, cpou);
				this.Generator.CopyPositionAndMessages(op, replaced);
				IList<ISignature> list = this.Scope[empty];
				if (list != null && list.Count == 1)
				{
					_ISignature isignature = (_ISignature)list[0];
					if (isignature.CalleeIds.Length == 0)
					{
						isignature.AddAttribute("DoLink", null);
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x06002A84 RID: 10884 RVA: 0x00094AF8 File Offset: 0x00092CF8
		public _IExpression HandleExternalFunctionCalls(_IConversionExpression conv, _ICompiledPOU cpou)
		{
			string empty = string.Empty;
			TypeClass typeClass = TypeClass.None;
			if (ImplicitFunctionCallsHandler.\u0001(this.ComCon, conv, this.Scope, ref empty, ref typeClass))
			{
				if (typeClass == TypeClass.Bit)
				{
					typeClass = TypeClass.Bool;
				}
				_IExpression iexpression = this.\u0001(empty, conv.To, typeClass, new IExpression[]
				{
					conv._Exp
				});
				this.Generator.\u0001<_IExpression>(iexpression, this.Scope, cpou);
				this.Generator.CopyPositionAndMessages(conv, iexpression);
				IList<ISignature> list = this.Scope[empty];
				if (list != null && list.Count == 1)
				{
					_ISignature isignature = (_ISignature)list[0];
					if (isignature.CalleeIds.Length == 0)
					{
						isignature.AddAttribute("DoLink", null);
					}
				}
				return iexpression;
			}
			return conv;
		}

		// Token: 0x06002A85 RID: 10885 RVA: 0x00094BB0 File Offset: 0x00092DB0
		private _IExpression \u0001(string \u0002, TypeClass \u0003, TypeClass \u0004, params IExpression[] \u0005)
		{
			IExpression[] array = this.\u0001(\u0005, \u0004);
			ISignature signature = this.\u0001(\u0002, array);
			_ICallExpression u = this.\u0001(\u0002, signature, array);
			return this.\u0001(signature, \u0003, u);
		}

		// Token: 0x06002A86 RID: 10886 RVA: 0x00094BE4 File Offset: 0x00092DE4
		private IExpression[] \u0001(IExpression[] \u0002, TypeClass \u0003)
		{
			IExpression[] array = \u0002;
			if (\u0003 != TypeClass.None)
			{
				_IExpression iexpression = \u0003.\u0001((long)\u0003);
				iexpression.Type = TypeTable.DWord;
				array = new IExpression[\u0002.Length + 1];
				\u0002.CopyTo(array, 0);
				array[\u0002.Length] = iexpression;
			}
			return array;
		}

		// Token: 0x06002A87 RID: 10887 RVA: 0x00094C28 File Offset: 0x00092E28
		private ISignature \u0001(string \u0002, IExpression[] \u0003)
		{
			ISignature signature = this.Scope.FindFirstSignature(\u0002);
			ExternalFunctionCallsHandler.\u0001(\u0002, \u0003, signature);
			return signature;
		}

		// Token: 0x06002A88 RID: 10888 RVA: 0x00094C4C File Offset: 0x00092E4C
		[ExcludeFromCodeCoverage]
		private static void \u0001(string \u0002, IExpression[] \u0003, ISignature \u0004)
		{
			if (\u0004 == null || \u0004.Inputs.Length != \u0003.Length)
			{
				string text = string.Format("Internal Error: Could not resolve external function {0} with {1} input(s)", \u0002, \u0003.Length);
				\u0003.\u0001(Token.Empty).AddError(text);
				throw new LateCompileErrorException(text);
			}
		}

		// Token: 0x06002A89 RID: 10889 RVA: 0x00094C94 File Offset: 0x00092E94
		private _ICallExpression \u0001(string \u0002, ISignature \u0003, IExpression[] \u0004)
		{
			_ICallExpression icallExpression = \u0003.\u0001(\u0003.\u0001(\u0002), Token.Empty);
			IVariable[] inputs = \u0003.Inputs;
			for (int i = 0; i < inputs.Length; i++)
			{
				if (\u0002 == CGConstants.test_and_set)
				{
					_IOperatorExpression exp = \u0003.\u0001(Operator.Adr, (_IExpression)((_IExprement)\u0004[i]).Duplicate());
					icallExpression.AddParam(exp);
				}
				else
				{
					_IVariableExpression expVariable = \u0003.\u0001(inputs[i].OrgName);
					icallExpression.AddParam((_IExpression)((_IExprement)\u0004[i]).Duplicate(), expVariable);
				}
			}
			return icallExpression;
		}

		// Token: 0x06002A8A RID: 10890 RVA: 0x00094D24 File Offset: 0x00092F24
		private _IExpression \u0001(ISignature \u0002, TypeClass \u0003, _ICallExpression \u0004)
		{
			_IExpression result = \u0004;
			if (\u0002.Outputs.Length != 0 && \u0003 != TypeClass.None && \u0002.Outputs[0].Type.Class != \u0003)
			{
				_IConversionExpression iconversionExpression = \u0003.\u0001(\u0002.Outputs[0].Type.Class, \u0003, Token.Empty);
				iconversionExpression._Exp = \u0004;
				result = iconversionExpression;
			}
			return result;
		}

		// Token: 0x040007F9 RID: 2041
		[CompilerGenerated]
		private readonly ICodegenerator \u0001;

		// Token: 0x040007FA RID: 2042
		[CompilerGenerated]
		private readonly IScope5 \u0001;

		// Token: 0x040007FB RID: 2043
		[CompilerGenerated]
		private readonly LateCodeGenerator \u0001;

		// Token: 0x040007FC RID: 2044
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;
	}
}
