using System;
using System.Collections.Generic;
using System.Linq;
using \u0007;
using \u0019;
using \u001D;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0016
{
	// Token: 0x02000241 RID: 577
	internal sealed class \u0010
	{
		// Token: 0x060025C7 RID: 9671 RVA: 0x0008310C File Offset: 0x0008130C
		public \u0010(_ICompileContext \u0001\u0002)
		{
			this.\u0001 = \u0001\u0002;
		}

		// Token: 0x060025C8 RID: 9672 RVA: 0x0008311C File Offset: 0x0008131C
		public static string \u0001(_ICompiledPOU \u0002)
		{
			if (\u0002 == null)
			{
				return "ERROR";
			}
			return "return__label__" + \u0002.SignatureId.ToString();
		}

		// Token: 0x060025C9 RID: 9673 RVA: 0x0008314C File Offset: 0x0008134C
		private void \u0001(_ICompiledPOU \u0002, _ISequenceStatement \u0003)
		{
			if (\u0002 == null)
			{
				return;
			}
			int signatureId = \u0002.SignatureId;
			if (signatureId < 0)
			{
				return;
			}
			IScope5 scope = global::\u0007.\u0005.\u0001(this.\u0001, signatureId);
			if (scope == null)
			{
				return;
			}
			_ISignature isignature = scope[signatureId] as _ISignature;
			if (isignature == null)
			{
				return;
			}
			IList<_IVariable> allVariables = isignature.AllVariables;
			this.\u0001(allVariables, isignature, scope, \u0002, \u0003);
		}

		// Token: 0x060025CA RID: 9674 RVA: 0x000831A0 File Offset: 0x000813A0
		private bool \u0001(_IVariable \u0002)
		{
			return \u0002 != null && \u0002.HasFlag(VarFlag.RelativeStack) && !\u0002.HasFlag(VarFlag.Input) && !\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_NO_EXIT);
		}

		// Token: 0x060025CB RID: 9675 RVA: 0x000831D0 File Offset: 0x000813D0
		private void \u0001(IList<_IVariable> \u0002, _ISignature \u0003, IScope5 \u0004, _ICompiledPOU \u0005, _ISequenceStatement \u0006)
		{
			foreach (_IVariable u in \u0002.Reverse<_IVariable>().Where(new Func<_IVariable, bool>(this.\u0001)))
			{
				LStringBuilder lstringBuilder = new LStringBuilder();
				bool flag = true;
				\u001D.\u000F.\u0001(this.\u0001, lstringBuilder, \u0004, \u0003, false, string.Empty, u, false, 0, ref flag, null);
				if (flag && lstringBuilder.Length > 0)
				{
					_IStatement sm = this.\u0001(lstringBuilder, \u0004, \u0005);
					\u0006.Add(sm);
				}
			}
		}

		// Token: 0x060025CC RID: 9676 RVA: 0x0008326C File Offset: 0x0008146C
		private _IStatement \u0001(LStringBuilder \u0002, IScope5 \u0003, _ICompiledPOU \u0004)
		{
			return new LateCodeGenerator(this.\u0001).\u0001(\u0002.ToString(), \u0003, \u0004);
		}

		// Token: 0x060025CD RID: 9677 RVA: 0x00083288 File Offset: 0x00081488
		public _ISequenceStatement \u0001(_ICompiledPOU \u0002)
		{
			IMinimalPosition position = \u0019.\u0003.\u0001(\u0002.ImplicitReturnPositionPos, 0);
			_ISequenceStatement isequenceStatement = \u0019.\u0003.\u0001();
			isequenceStatement._Position = position;
			_ILabelStatement ilabelStatement = \u0019.\u0003.\u0001();
			ilabelStatement.Text = \u0016.\u0010.\u0001(\u0002);
			ilabelStatement.SetFlag(StatementFlag.GenerateBP, true);
			ilabelStatement._Position = \u0019.\u0003.\u0001(\u0002.ImplicitReturnPositionPos, 0);
			isequenceStatement.Add(ilabelStatement);
			this.\u0001(\u0002, isequenceStatement);
			return isequenceStatement;
		}

		// Token: 0x040006E4 RID: 1764
		private readonly _ICompileContext \u0001;
	}
}
