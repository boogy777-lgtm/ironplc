using System;
using \u000E;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0084
{
	// Token: 0x020002A4 RID: 676
	internal static class \u0017
	{
		// Token: 0x06002A7D RID: 10877 RVA: 0x000948F4 File Offset: 0x00092AF4
		public static _IStatement \u0001(_IOperatorExpression \u0002, \u0011 \u0003)
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			_IPointerType ipointerType = \u0002[0].Type.DeRefType as _IPointerType;
			if (ipointerType == null)
			{
				return null;
			}
			_IUserdefType iuserdefType = ipointerType.BaseType as _IUserdefType;
			if (iuserdefType != null)
			{
				ISignature signature = iuserdefType.GetSignature(\u0003._Scope);
				if (signature != null && signature.GetSubSignature("FB_EXIT") != null)
				{
					lstringBuilder.AppendLine(string.Format("IF {0} <> 0 THEN", \u0002[0]));
					lstringBuilder.AppendLine(string.Format("{0}^.FB_Exit(false);", \u0002[0]));
					lstringBuilder.AppendLine("END_IF");
				}
			}
			lstringBuilder.AppendLine(string.Format("__MemMan.Free({0});", \u0002[0]));
			lstringBuilder.Append(string.Format("{0} := 0;", \u0002[0]));
			return \u0003.Generator.DisableFlowBPForallExceptFirst(\u0003.Generator.\u0001(lstringBuilder.ToString(), \u0003._Scope, \u0003.CompiledPOU), \u0002._Position);
		}
	}
}
