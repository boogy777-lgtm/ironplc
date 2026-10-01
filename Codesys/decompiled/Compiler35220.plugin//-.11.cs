using System;
using System.Collections.Generic;
using \u0003;
using \u000E;
using \u0012;
using \u0018;
using \u0019;
using \u001C;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0080;

namespace \u007F
{
	// Token: 0x0200030E RID: 782
	internal static class \u0010
	{
		// Token: 0x06002F3A RID: 12090 RVA: 0x000B1A9C File Offset: 0x000AFC9C
		private static void \u0001(object \u0002, MessageId \u0003, params object[] \u0004)
		{
			_ISignature isignature = \u0002 as _ISignature;
			if (isignature != null)
			{
				isignature.AddMessage(Severity.Error, \u0003, \u0004);
			}
		}

		// Token: 0x06002F3B RID: 12091 RVA: 0x000B1ABC File Offset: 0x000AFCBC
		private static void \u0001(object \u0002, _ICompilerMessage \u0003)
		{
			_ISignature isignature = \u0002 as _ISignature;
			if (isignature != null)
			{
				isignature.AddMessage(\u0003);
			}
		}

		// Token: 0x06002F3C RID: 12092 RVA: 0x000B1ADC File Offset: 0x000AFCDC
		internal static void \u0001(_ISignature \u0002, _ICompileContext \u0003)
		{
			\u0080.\u0014 u = new \u0080.\u0014(\u0003);
			ErrorReportingService u2 = new ErrorReportingService(new global::\u000E.\u0016(\u007F.\u0010.\u0001), new global::\u0012.\u0013(\u007F.\u0010.\u0001));
			\u007F.\u0010.\u0001((_ISignature4)\u0002, u, u2);
		}

		// Token: 0x06002F3D RID: 12093 RVA: 0x000B1B1C File Offset: 0x000AFD1C
		internal static void \u0001(_ISignature \u0002, _IPreCompileContext \u0003, object \u0004, global::\u000E.\u0016 \u0005, global::\u0012.\u0013 \u0006)
		{
			global::\u0018.\u000F u = new global::\u0018.\u000F(\u0003, new List<ICompiledType>());
			\u007F.\u0010.\u0001((_ISignature4)\u0002, u, new ErrorReportingService(\u0005, \u0006));
		}

		// Token: 0x06002F3E RID: 12094 RVA: 0x000B1B4C File Offset: 0x000AFD4C
		private static bool \u0001(Operator \u0002)
		{
			return \u0002 == Operator.Interface || \u0002 == Operator.FunctionBlock || \u0002 == Operator.Program;
		}

		// Token: 0x06002F3F RID: 12095 RVA: 0x000B1B60 File Offset: 0x000AFD60
		private static void \u0001(_ISignature4 \u0002, \u001C.\u0011 \u0003, ErrorReportingService \u0004)
		{
			if (\u0002 != null && \u007F.\u0010.\u0001(\u0002.POUType))
			{
				IEnumerable<string> enumerable = \u0003.\u0001(\u0002);
				_ISignature4 isignature = \u0003.\u0001(\u0002);
				foreach (string u in enumerable)
				{
					IList<_ISignature> u2 = \u0003.\u0001(\u0002, u);
					\u007F.\u0010.\u0001(u2, u, \u0004);
					\u007F.\u0010.\u0001(u2, u, \u0003, \u0004);
					if (isignature != null)
					{
						\u007F.\u0010.\u0001(isignature, u, \u0004);
					}
				}
			}
		}

		// Token: 0x06002F40 RID: 12096 RVA: 0x000B1BE4 File Offset: 0x000AFDE4
		private static void \u0001(_ISignature4 \u0002, string \u0003, ErrorReportingService \u0004)
		{
			if (\u0002 != null)
			{
				_ISignature isignature = \u0002.GetSubSignature(\u0003) as _ISignature;
				if (isignature != null && !isignature.GetFlagInternal(SignatureFlagInternal.Overloaded))
				{
					\u007F.\u0010.\u0001(isignature, \u0003, \u0004);
				}
			}
		}

		// Token: 0x06002F41 RID: 12097 RVA: 0x000B1C1C File Offset: 0x000AFE1C
		private static void \u0001(IList<_ISignature> \u0002, string \u0003, \u001C.\u0011 \u0004, ErrorReportingService \u0005)
		{
			for (int i = 0; i < \u0002.Count; i++)
			{
				for (int j = i + 1; j < \u0002.Count; j++)
				{
					\u007F.\u0010.\u0001(\u0002[i], \u0002[j], \u0003, \u0004, \u0005);
				}
			}
		}

		// Token: 0x06002F42 RID: 12098 RVA: 0x000B1C64 File Offset: 0x000AFE64
		private static void \u0001(_ISignature \u0002, _ISignature \u0003, string \u0004, \u001C.\u0011 \u0005, ErrorReportingService \u0006)
		{
			if (\u0005.\u0001(\u0002, \u0003))
			{
				\u007F.\u0010.\u0001(\u0002, \u0003, \u0004, \u0006);
				\u007F.\u0010.\u0001(\u0003, \u0002, \u0004, \u0006);
			}
		}

		// Token: 0x06002F43 RID: 12099 RVA: 0x000B1C84 File Offset: 0x000AFE84
		private static void \u0001(_ISignature \u0002, _ISignature \u0003, string \u0004, ErrorReportingService \u0005)
		{
			\u0005.AddError(\u0002, MessageId.Err_OverloadWithSameInputs, new object[]
			{
				\u0004
			});
			\u007F.\u0010.\u0001(\u0002, \u0003, \u0005);
		}

		// Token: 0x06002F44 RID: 12100 RVA: 0x000B1CA4 File Offset: 0x000AFEA4
		private static void \u0001(_ISignature \u0002, _ISignature \u0003, ErrorReportingService \u0004)
		{
			ISourcePosition u = global::\u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0003.LibraryPath), \u0003.ObjectGuid, 0L, 0, 0);
			string u2 = global::\u0003.\u0006.\u0001(MessageId.Inf_RelatedPosition, Array.Empty<object>());
			_ICompilerMessage cm = global::\u0019.\u0003.\u0001(u, u2, Severity.Information, MessageId.Inf_RelatedPosition);
			\u0004.AddInformation(\u0002, cm);
		}

		// Token: 0x06002F45 RID: 12101 RVA: 0x000B1D00 File Offset: 0x000AFF00
		private static void \u0001(IList<_ISignature> \u0002, string \u0003, ErrorReportingService \u0004)
		{
			foreach (_ISignature u in \u0002)
			{
				\u007F.\u0010.\u0001(u, \u0003, \u0004);
			}
		}

		// Token: 0x06002F46 RID: 12102 RVA: 0x000B1D48 File Offset: 0x000AFF48
		private static void \u0001(_ISignature \u0002, string \u0003, ErrorReportingService \u0004)
		{
			if (!\u0002.HasAttribute("overloaded"))
			{
				\u0004.AddError(\u0002, MessageId.Err_OverloadNeedsAttribute, new object[]
				{
					\u0003
				});
			}
		}
	}
}
