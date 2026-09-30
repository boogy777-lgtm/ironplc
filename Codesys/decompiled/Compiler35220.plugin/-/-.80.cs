using System;
using System.Collections.Generic;
using \u0007;
using \u0019;
using _3S.CoDeSys.Compiler35220.Compile.Phase1_Typification.Code;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0012
{
	// Token: 0x0200010B RID: 267
	internal sealed class \u0007 : EmptyVisitor351900
	{
		// Token: 0x060013CE RID: 5070 RVA: 0x00038EDC File Offset: 0x000370DC
		private \u0007(int \u0087\u0002, _ICompileContext \u0001\u0002)
		{
			this.\u0001 = \u0087\u0002;
			this.\u0001 = \u0001\u0002;
			this.\u0001 = global::\u0007.\u0005.\u0001(\u0001\u0002);
		}

		// Token: 0x060013CF RID: 5071 RVA: 0x00038F0C File Offset: 0x0003710C
		public static LList<ISourcePosition> \u0001(_IStatement \u0002, int \u0003, _ICompileContext \u0004)
		{
			global::\u0012.\u0007 u = new global::\u0012.\u0007(\u0003, \u0004);
			IStandardTraverser ivisit = new StandardTraverser(u);
			\u0002.Accept(ivisit);
			return u.\u0001;
		}

		// Token: 0x060013D0 RID: 5072 RVA: 0x00038F34 File Offset: 0x00037134
		public override void visit(_IVariableExpression variable, AccessFlag access)
		{
			if (variable.SignatureId == this.\u0001 && variable.VariableId == -1 && access == AccessFlag.Call)
			{
				this.\u0001.Add(variable.Position);
			}
		}

		// Token: 0x060013D1 RID: 5073 RVA: 0x00038F64 File Offset: 0x00037164
		public override void visit(_IOperatorExpression op)
		{
			if (this.\u0001.Codegenerator == null || op.Type == null)
			{
				return;
			}
			string empty = string.Empty;
			TypeClass typeClass = TypeClass.None;
			Operator code = op.Code;
			if (op.Code == Operator.TruncInt)
			{
				op.Code = Operator.Trunc;
			}
			if (ImplicitFunctionCallsHandler.\u0001(this.\u0001, op, op.Type, ref empty, ref typeClass))
			{
				IList<ISignature> list = this.\u0001[empty];
				if (list != null && list.Count == 1 && list[0].Id == this.\u0001)
				{
					ISourcePosition position = op._OperandsList[0].Position;
					ISourcePosition sourcePosition = null;
					if (op._OperandsList.Count > 1)
					{
						sourcePosition = op._OperandsList[op._OperandsList.Count - 1].Position;
					}
					int num = (int)position.Length;
					if (sourcePosition != null && sourcePosition.PositionOffset > position.PositionOffset && sourcePosition.Length > 0)
					{
						num = (int)(sourcePosition.PositionOffset + sourcePosition.Length - position.PositionOffset);
					}
					this.\u0001.Add(\u0019.\u0003.\u0001(position.ProjectHandle, Guid.Empty, position.Position, position.PositionOffset, (short)num));
				}
			}
			op.Code = code;
		}

		// Token: 0x060013D2 RID: 5074 RVA: 0x000390B4 File Offset: 0x000372B4
		public override void visit(_IConversionExpression conv)
		{
			if (this.\u0001.Codegenerator == null)
			{
				return;
			}
			string empty = string.Empty;
			TypeClass typeClass = TypeClass.None;
			if (ImplicitFunctionCallsHandler.\u0001(this.\u0001, conv, this.\u0001, ref empty, ref typeClass))
			{
				IList<ISignature> list = this.\u0001[empty];
				if (list != null && list.Count == 1 && list[0].Id == this.\u0001)
				{
					this.\u0001.Add(conv.Position);
				}
			}
		}

		// Token: 0x04000355 RID: 853
		private LList<ISourcePosition> \u0001 = new LList<ISourcePosition>();

		// Token: 0x04000356 RID: 854
		private int \u0001;

		// Token: 0x04000357 RID: 855
		private _ICompileContext \u0001;

		// Token: 0x04000358 RID: 856
		private IScope5 \u0001;
	}
}
