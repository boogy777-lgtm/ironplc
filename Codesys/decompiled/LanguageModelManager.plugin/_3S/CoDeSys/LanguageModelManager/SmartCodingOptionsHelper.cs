using System;
using _3S.CoDeSys.Core.Options;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000136 RID: 310
	public abstract class SmartCodingOptionsHelper
	{
		// Token: 0x17000713 RID: 1811
		// (get) Token: 0x06001AA6 RID: 6822 RVA: 0x0004C13A File Offset: 0x0004B13A
		internal static bool EnablePrecomCheck
		{
			get
			{
				return !SmartCodingOptionsHelper.OptionKey.HasValue(SmartCodingOptionsHelper.ENABLE_PRECOMCHECK, typeof(bool)) || (bool)SmartCodingOptionsHelper.OptionKey[SmartCodingOptionsHelper.ENABLE_PRECOMCHECK];
			}
		}

		// Token: 0x17000714 RID: 1812
		// (get) Token: 0x06001AA7 RID: 6823 RVA: 0x0004C16D File Offset: 0x0004B16D
		public static bool ShowAllInstanceVars
		{
			get
			{
				return SmartCodingOptionsHelper.OptionKey.HasValue(SmartCodingOptionsHelper.SHOWALLINSTANCEVARS, typeof(bool)) && (bool)SmartCodingOptionsHelper.OptionKey[SmartCodingOptionsHelper.SHOWALLINSTANCEVARS];
			}
		}

		// Token: 0x17000715 RID: 1813
		// (get) Token: 0x06001AA8 RID: 6824 RVA: 0x0004C1A0 File Offset: 0x0004B1A0
		// (set) Token: 0x06001AA9 RID: 6825 RVA: 0x0004C1EC File Offset: 0x0004B1EC
		public static int MaxDegreeOfParallelism
		{
			get
			{
				if (SmartCodingOptionsHelper.OptionKey.HasValue("MaxDegreeOfParallelism", typeof(int)))
				{
					return (int)SmartCodingOptionsHelper.OptionKey["MaxDegreeOfParallelism"];
				}
				return Math.Max(1, Environment.ProcessorCount / 2 - 1);
			}
			set
			{
				SmartCodingOptionsHelper.OptionKey["MaxDegreeOfParallelism"] = value;
			}
		}

		// Token: 0x17000716 RID: 1814
		// (get) Token: 0x06001AAA RID: 6826 RVA: 0x0004C203 File Offset: 0x0004B203
		private static IOptionKey OptionKey
		{
			get
			{
				return APEnvironmentFacade.Instance.CreateSubKey(OptionRoot.User, SmartCodingOptionsHelper.SMARTCODING_SUB_KEY);
			}
		}

		// Token: 0x0400058B RID: 1419
		internal static readonly string ENABLE_PRECOMCHECK = "EnablePrecomCheck";

		// Token: 0x0400058C RID: 1420
		internal static readonly string SHOWALLINSTANCEVARS = "ShowAllInstanceVars";

		// Token: 0x0400058D RID: 1421
		private const string MAX_DEGREE_OF_PARALLELISM = "MaxDegreeOfParallelism";

		// Token: 0x0400058E RID: 1422
		internal static readonly string SMARTCODING_SUB_KEY = "{24F0D403-55B9-41af-A07F-8C46BDAEEF8E}";
	}
}
