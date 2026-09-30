using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using \u0004;
using \u000E;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0003
{
	// Token: 0x02000263 RID: 611
	internal static class \u0010
	{
		// Token: 0x0600277F RID: 10111 RVA: 0x00088824 File Offset: 0x00086A24
		internal static _IStatement \u0001(_IAssignmentExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			_INewExpression inewExpression = \u0002._RValue as _INewExpression;
			if (inewExpression == null)
			{
				return null;
			}
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendFormat("{0} := __MemMan.Alloc(DINT_TO_DWORD(TO_DINT({1}) * TO_DINT({2})));", new object[]
			{
				\u0002._LValue.ToString(),
				inewExpression._Count,
				inewExpression._TypeToCast.Size(\u0003._Scope)
			});
			if (inewExpression._TypeToCast.Class == TypeClass.Userdef)
			{
				ISignature signature = inewExpression.GetSignature(\u0003._Scope);
				bool u;
				if (signature != null && global::\u0003.\u0010.\u0001(inewExpression, \u0003.Comcon, signature, out u))
				{
					lstringBuilder.AppendLine(string.Format("IF {0} <> 0 THEN", \u0002._LValue));
					global::\u0003.\u0010.\u0001(\u0002, u, signature, lstringBuilder);
					global::\u0003.\u0010.\u0001(\u0002, inewExpression, lstringBuilder);
					global::\u0003.\u0010.\u0001(lstringBuilder, (_ISignature)signature, (_IExpression)\u0002.LValue, \u0003);
					lstringBuilder.Append("END_IF");
				}
			}
			_ISequenceStatement isequenceStatement = (_ISequenceStatement)\u0003.Generator.\u0001(lstringBuilder.ToString(), \u0003._Scope, \u0003.CompiledPOU);
			\u0003.Generator.DisableFlowBPForallExceptFirst(isequenceStatement, \u0002._Position);
			return isequenceStatement;
		}

		// Token: 0x06002780 RID: 10112 RVA: 0x00088948 File Offset: 0x00086B48
		internal static bool \u0001(_IAssignmentExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			return \u0002._RValue is _INewExpression;
		}

		// Token: 0x06002781 RID: 10113 RVA: 0x00088958 File Offset: 0x00086B58
		private static void \u0001(_IAssignmentExpression \u0002, _INewExpression \u0003, LStringBuilder \u0004)
		{
			IEnumerable<IAssignmentExpression> fbinitParams = \u0003._FBInitParams;
			\u0004.AppendFormat("{0}^.FB_Init(bInitRetains := true, bInCopyCode := false", new object[]
			{
				\u0002._LValue
			});
			if (fbinitParams != null)
			{
				foreach (IAssignmentExpression assignmentExpression in fbinitParams)
				{
					\u0004.AppendFormat(", {0} := {1}", new object[]
					{
						assignmentExpression.LValue.ToString(),
						assignmentExpression.RValue.ToString()
					});
				}
			}
			\u0004.AppendLine(");");
		}

		// Token: 0x06002782 RID: 10114 RVA: 0x000889F8 File Offset: 0x00086BF8
		private static void \u0001(_IAssignmentExpression \u0002, bool \u0003, ISignature \u0004, LStringBuilder \u0005)
		{
			if (\u0003 || \u0004.GetSubSignature(IdentifierConstants.VFInitMethodName) != null)
			{
				\u0005.AppendLine(string.Format("{0}^.__VFInit();", \u0002._LValue));
			}
		}

		// Token: 0x06002783 RID: 10115 RVA: 0x00088A24 File Offset: 0x00086C24
		private static void \u0001(LStringBuilder \u0002, ISignature \u0003, _IExpression \u0004, global::\u000E.\u0011 \u0005)
		{
			_IExpression u = global::\u0019.\u0003.\u0001(\u0004);
			_ISequenceStatement isequenceStatement = global::\u0004.\u0018.\u0001((_ISignature)\u0003, u, \u0005._Scope, null);
			if (isequenceStatement != null && isequenceStatement.Statements.Length != 0)
			{
				foreach (_IStatement arg in isequenceStatement.StatementList.OfType<_IStatement>())
				{
					\u0002.AppendLine(string.Format("{0}", arg));
				}
			}
		}

		// Token: 0x06002784 RID: 10116 RVA: 0x00088AAC File Offset: 0x00086CAC
		private static bool \u0001(_INewExpression \u0002, _ICompileContext \u0003, ISignature \u0004, out bool \u0005)
		{
			bool flag = \u0004.GetFlag(SignatureFlag.Structure);
			\u0005 = (Operator.FunctionBlock == \u0004.POUType);
			global::\u0003.\u0010.\u0001(\u0002, \u0003, \u0004, ref \u0005, ref flag);
			return \u0005 || flag;
		}

		// Token: 0x06002785 RID: 10117 RVA: 0x00088AE0 File Offset: 0x00086CE0
		[ExcludeFromCodeCoverage]
		private static void \u0001(_INewExpression \u0002, _ICompileContext \u0003, ISignature \u0004, ref bool \u0005, ref bool \u0006)
		{
			if (!\u0006 && !\u0005 && \u0004.GetFlag(SignatureFlag.Alias))
			{
				_IUserdefType iuserdefType = \u0002._TypeToCast as _IUserdefType;
				if (iuserdefType != null)
				{
					ISignature signatureById = \u0003.GetSignatureById(iuserdefType.SignatureId);
					if (signatureById != null)
					{
						\u0006 = signatureById.GetFlag(SignatureFlag.Structure);
						\u0005 = (Operator.FunctionBlock == signatureById.POUType);
					}
				}
			}
		}
	}
}
