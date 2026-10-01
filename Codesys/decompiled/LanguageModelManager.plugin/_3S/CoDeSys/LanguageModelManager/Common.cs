using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000155 RID: 341
	public abstract class Common
	{
		// Token: 0x06001B99 RID: 7065 RVA: 0x0004E2B8 File Offset: 0x0004D2B8
		public static bool ImplementsInterface(Type c, string stInterfaceFullName)
		{
			bool flag = false;
			Type[] interfaces = c.GetInterfaces();
			if (interfaces != null)
			{
				int num = 0;
				while (!flag && num < interfaces.Length)
				{
					flag = stInterfaceFullName.Equals(interfaces[num].FullName);
					if (!flag)
					{
						num++;
					}
				}
			}
			return flag;
		}

		// Token: 0x06001B9A RID: 7066 RVA: 0x0004E2F5 File Offset: 0x0004D2F5
		public static IExpression RemoveConversions(IExpression expr, bool bImplicitOnly)
		{
			if (expr == null)
			{
				return null;
			}
			while (expr is IConversionExpression && (!bImplicitOnly || (expr as IConversionExpression).Implicit))
			{
				expr = (expr as IConversionExpression).Exp;
			}
			return expr;
		}

		// Token: 0x06001B9B RID: 7067 RVA: 0x0004E324 File Offset: 0x0004D324
		internal static bool IsLibraryWithPinnedStorageVersion()
		{
			return APEnvironmentFacade.Instance.ExistsPrimaryProject && APEnvironmentFacade.Instance.PrimaryProjectPath != null && APEnvironmentFacade.Instance.PrimaryProjectPath.ToLowerInvariant().EndsWith(".library") && APEnvironmentFacade.Instance.IsProjectInfoObjectBoolFlagSet(APEnvironmentFacade.Instance.PrimaryProjectHandle, "IsStorageFormatPinned");
		}

		// Token: 0x040005D3 RID: 1491
		internal static readonly int InvalidID = -1;
	}
}
