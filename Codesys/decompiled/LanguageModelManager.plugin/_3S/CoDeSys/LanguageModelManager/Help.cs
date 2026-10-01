using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000135 RID: 309
	internal static class Help
	{
		// Token: 0x06001AA1 RID: 6817 RVA: 0x0004BFFA File Offset: 0x0004AFFA
		public static long GetAnyLong(ILiteralValue litval)
		{
			if (litval == null)
			{
				return 0L;
			}
			if (litval.KindOf == KindOfLiteral.UnsignedInteger)
			{
				return (long)litval.UnsignedLong;
			}
			return litval.SignedLong;
		}

		// Token: 0x06001AA2 RID: 6818 RVA: 0x0004C018 File Offset: 0x0004B018
		public static int CalculatePointerSize(Guid appGuid)
		{
			int result = 4;
			if (appGuid != Guid.Empty)
			{
				ICodegenerator codegenerator = CompilerProxy.CreateCodegenerator(APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(appGuid), appGuid, false, false);
				if (codegenerator is ICodegenerator3 && (codegenerator as ICodegenerator3).GetProperty(CodegeneratorProperties.LWordPointer))
				{
					result = 8;
				}
			}
			return result;
		}

		// Token: 0x06001AA3 RID: 6819 RVA: 0x0004C06C File Offset: 0x0004B06C
		internal static int CalculatePointerSize(IPreCompileContext precom)
		{
			Guid appGuid = Guid.Empty;
			if (precom != null && precom.ApplicationGuid != Guid.Empty)
			{
				appGuid = precom.ApplicationGuid;
			}
			return Help.CalculatePointerSize(appGuid);
		}

		// Token: 0x06001AA4 RID: 6820 RVA: 0x0004C0A4 File Offset: 0x0004B0A4
		public static int max(params int[] inputs)
		{
			if (inputs == null)
			{
				throw new ArgumentNullException("inputs");
			}
			if (inputs.Length == 0)
			{
				throw new ArgumentException("Inputs");
			}
			int num = inputs[0];
			for (int i = 1; i < inputs.Length; i++)
			{
				num = ((inputs[i] > num) ? inputs[i] : num);
			}
			return num;
		}

		// Token: 0x06001AA5 RID: 6821 RVA: 0x0004C0F0 File Offset: 0x0004B0F0
		public static int min(params int[] inputs)
		{
			if (inputs == null)
			{
				throw new ArgumentNullException("inputs");
			}
			if (inputs.Length == 0)
			{
				throw new ArgumentException("Inputs");
			}
			int num = inputs[0];
			for (int i = 1; i < inputs.Length; i++)
			{
				num = ((inputs[i] < num) ? inputs[i] : num);
			}
			return num;
		}
	}
}
