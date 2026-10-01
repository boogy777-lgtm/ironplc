using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.CompilerVersion
{
	// Token: 0x02000080 RID: 128
	[TypeGuid("{22222222-c11d-4a41-9598-cbd21e32757c}")]
	public class CompilerVersionsProvider : _ICompilerVersionsProvider
	{
		// Token: 0x17000418 RID: 1048
		// (get) Token: 0x06000AED RID: 2797 RVA: 0x000180D0 File Offset: 0x000162D0
		public static Version V352200 { get; } = new Version(3, 5, 22, 0);

		// Token: 0x17000419 RID: 1049
		// (get) Token: 0x06000AEE RID: 2798 RVA: 0x000180D8 File Offset: 0x000162D8
		private static Version[] CompilerVersions { get; } = new Version[]
		{
			CompilerVersionsProvider.V352200
		};

		// Token: 0x1700041A RID: 1050
		// (get) Token: 0x06000AEF RID: 2799 RVA: 0x000180E0 File Offset: 0x000162E0
		IEnumerable<Version> _ICompilerVersionsProvider.CompilerVersions
		{
			get
			{
				return CompilerVersionsProvider.CompilerVersions;
			}
		}

		// Token: 0x1700041B RID: 1051
		// (get) Token: 0x06000AF0 RID: 2800 RVA: 0x000180E8 File Offset: 0x000162E8
		public ICompilerServiceFactory ServiceFactory { get; } = new CompilerServices();

		// Token: 0x06000AF1 RID: 2801 RVA: 0x000180F0 File Offset: 0x000162F0
		internal static bool \u0001(Version \u0002)
		{
			return CompilerVersionsProvider.CompilerVersions.Contains(\u0002);
		}

		// Token: 0x06000AF2 RID: 2802 RVA: 0x00018100 File Offset: 0x00016300
		public bool ProvidesVersion(Version version)
		{
			return CompilerVersionsProvider.\u0001(version);
		}

		// Token: 0x06000AF3 RID: 2803 RVA: 0x00018108 File Offset: 0x00016308
		public int GetStatusCode()
		{
			return 1;
		}

		// Token: 0x0400014E RID: 334
		[CompilerGenerated]
		private static readonly Version \u0001;

		// Token: 0x0400014F RID: 335
		[CompilerGenerated]
		private static readonly Version[] \u0001;

		// Token: 0x04000150 RID: 336
		[CompilerGenerated]
		private readonly ICompilerServiceFactory \u0001;
	}
}
