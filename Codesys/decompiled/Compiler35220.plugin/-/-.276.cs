using System;
using \u0019;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0082;

namespace \u0011
{
	// Token: 0x020002DF RID: 735
	internal static class \u0013
	{
		// Token: 0x06002C1C RID: 11292 RVA: 0x0009ACF0 File Offset: 0x00098EF0
		internal static _IStatement \u0001(_IPragmaIfStatement \u0002, IScope5 \u0003, _ICompileContext \u0004, bool \u0005 = false)
		{
			if (global::\u0011.\u0013.\u0001(\u0002, \u0003, \u0004, \u0005))
			{
				return null;
			}
			_IPragmaExpression ipragmaExpression = \u0002.Condition as _IPragmaExpression;
			_IStatement result = null;
			Debug.\u0001(ipragmaExpression != null);
			if (global::\u0011.\u0013.\u0001(\u0002.IfThen, ipragmaExpression, out result))
			{
				return result;
			}
			foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
			{
				ipragmaExpression = (ipragmaElseIf.Condition as _IPragmaExpression);
				if (global::\u0011.\u0013.\u0001(ipragmaElseIf.Controlled, ipragmaExpression, out result))
				{
					return result;
				}
			}
			if (\u0002.IfElse != null)
			{
				return \u0002.IfElse;
			}
			return result;
		}

		// Token: 0x06002C1D RID: 11293 RVA: 0x0009AD9C File Offset: 0x00098F9C
		private static bool \u0001(_IPragmaIfStatement \u0002, IScope5 \u0003, _ICompileContext \u0004, bool \u0005)
		{
			bool flag = global::\u0011.\u0013.\u0001(\u0002.Condition as _IPragmaExpression, \u0003, \u0004, \u0005);
			foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
			{
				flag = (global::\u0011.\u0013.\u0001(ipragmaElseIf.Condition as _IPragmaExpression, \u0003, \u0004, \u0005) || flag);
			}
			return flag;
		}

		// Token: 0x06002C1E RID: 11294 RVA: 0x0009AE0C File Offset: 0x0009900C
		private static bool \u0001(_IPragmaExpression \u0002, IScope5 \u0003, _ICompileContext \u0004, bool \u0005)
		{
			if (\u0002 == null)
			{
				return true;
			}
			bool flag = false;
			if (\u0005 || \u0002.ValueStillUndecided)
			{
				flag = \u0082.\u0012.\u0001(\u0002, \u0003, \u0004, \u0005);
			}
			if (flag)
			{
				\u0002.ValueStillUndecided = true;
				return true;
			}
			return false;
		}

		// Token: 0x06002C1F RID: 11295 RVA: 0x0009AE44 File Offset: 0x00099044
		private static bool \u0001(_IStatement \u0002, _IPragmaExpression \u0003, out _IStatement \u0004)
		{
			if (\u0003 == null)
			{
				\u0004 = \u0019.\u0003.\u0001();
				return false;
			}
			if (\u0003.Value)
			{
				\u0004 = \u0002;
				return true;
			}
			\u0004 = \u0019.\u0003.\u0001();
			return false;
		}
	}
}
