using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Options;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000B5 RID: 181
	[TypeGuid("{1DC0DD5B-C861-4ECB-A527-F06D97947EBE}")]
	public class LibraryDevelopmentOptions : _ILibraryDevelopmentOptions, ILMLibraryDevelopmentOptions2, ILMLibraryDevelopmentOptions
	{
		// Token: 0x1700027B RID: 635
		// (get) Token: 0x06000A80 RID: 2688 RVA: 0x00017FD6 File Offset: 0x00016FD6
		// (set) Token: 0x06000A81 RID: 2689 RVA: 0x00017FDD File Offset: 0x00016FDD
		public string CompilerDefinesToUse
		{
			get
			{
				return LibraryDevelopmentOptions.LocalOptions.CompilerDefinesToUse;
			}
			set
			{
				LibraryDevelopmentOptions.LocalOptions.CompilerDefinesToUse = value;
			}
		}

		// Token: 0x1700027C RID: 636
		// (get) Token: 0x06000A82 RID: 2690 RVA: 0x00017FE5 File Offset: 0x00016FE5
		// (set) Token: 0x06000A83 RID: 2691 RVA: 0x00017FEC File Offset: 0x00016FEC
		public ECheckAllPoolObjectsTargetPointerSize CheckAllPoolObjectsTargetPointerSize
		{
			get
			{
				return (ECheckAllPoolObjectsTargetPointerSize)LibraryDevelopmentOptions.LocalOptions.CheckAllPoolObjectsTargetPointerSize;
			}
			set
			{
				LibraryDevelopmentOptions.LocalOptions.CheckAllPoolObjectsTargetPointerSize = (int)value;
			}
		}

		// Token: 0x02000293 RID: 659
		internal abstract class LocalOptions
		{
			// Token: 0x06002B2A RID: 11050 RVA: 0x00072A9C File Offset: 0x00071A9C
			internal static string GetLibraryDevelopmentOption(string stKey, string stDefault)
			{
				IOptionKey libraryDevelopmentOptionKey = LibraryDevelopmentOptions.LocalOptions.GetLibraryDevelopmentOptionKey(false);
				if (libraryDevelopmentOptionKey != null && libraryDevelopmentOptionKey.HasValue(stKey, typeof(string)))
				{
					return (string)libraryDevelopmentOptionKey[stKey];
				}
				return stDefault;
			}

			// Token: 0x06002B2B RID: 11051 RVA: 0x00072AD4 File Offset: 0x00071AD4
			private static int GetLibraryDevelopmentOption(string stKey, int iDefault)
			{
				IOptionKey libraryDevelopmentOptionKey = LibraryDevelopmentOptions.LocalOptions.GetLibraryDevelopmentOptionKey(false);
				if (libraryDevelopmentOptionKey != null && libraryDevelopmentOptionKey.HasValue(stKey, typeof(int)))
				{
					return (int)libraryDevelopmentOptionKey[stKey];
				}
				return iDefault;
			}

			// Token: 0x06002B2C RID: 11052 RVA: 0x00072B0C File Offset: 0x00071B0C
			internal static void SetLibraryDevelopmentOption(string stKey, string stValue)
			{
				LibraryDevelopmentOptions.LocalOptions.GetLibraryDevelopmentOptionKey(true)[stKey] = stValue;
			}

			// Token: 0x06002B2D RID: 11053 RVA: 0x00072B1B File Offset: 0x00071B1B
			private static void SetLibraryDevelopmentOption(string stKey, int iValue)
			{
				LibraryDevelopmentOptions.LocalOptions.GetLibraryDevelopmentOptionKey(true)[stKey] = iValue;
			}

			// Token: 0x06002B2E RID: 11054 RVA: 0x00072B2F File Offset: 0x00071B2F
			private static IOptionKey GetLibraryDevelopmentOptionKey(bool bCreate)
			{
				if (bCreate)
				{
					return APEnvironmentFacade.Instance.CreateSubKey(OptionRoot.Project, "{C93D6B18-20B5-4927-8643-48F176E8352A}");
				}
				return APEnvironmentFacade.Instance.OpenSubKey(OptionRoot.Project, "{C93D6B18-20B5-4927-8643-48F176E8352A}");
			}

			// Token: 0x17000BFB RID: 3067
			// (get) Token: 0x06002B2F RID: 11055 RVA: 0x00072B55 File Offset: 0x00071B55
			// (set) Token: 0x06002B30 RID: 11056 RVA: 0x00072B66 File Offset: 0x00071B66
			internal static string CompilerDefinesToUse
			{
				get
				{
					return LibraryDevelopmentOptions.LocalOptions.GetLibraryDevelopmentOption("CompilerDefinesToUse", "");
				}
				set
				{
					LibraryDevelopmentOptions.LocalOptions.SetLibraryDevelopmentOption("CompilerDefinesToUse", value);
				}
			}

			// Token: 0x17000BFC RID: 3068
			// (get) Token: 0x06002B31 RID: 11057 RVA: 0x00072B73 File Offset: 0x00071B73
			// (set) Token: 0x06002B32 RID: 11058 RVA: 0x00072B80 File Offset: 0x00071B80
			internal static int CheckAllPoolObjectsTargetPointerSize
			{
				get
				{
					return LibraryDevelopmentOptions.LocalOptions.GetLibraryDevelopmentOption("CheckAllPoolObjectsPointerSize", 4);
				}
				set
				{
					LibraryDevelopmentOptions.LocalOptions.SetLibraryDevelopmentOption("CheckAllPoolObjectsPointerSize", value);
				}
			}

			// Token: 0x0400085A RID: 2138
			internal const string COMPILER_DEFINES_TO_USE = "CompilerDefinesToUse";

			// Token: 0x0400085B RID: 2139
			internal const string CHECKALLPOOLOBJECTS_POINTER_SIZE = "CheckAllPoolObjectsPointerSize";

			// Token: 0x0400085C RID: 2140
			internal const string SUB_KEY_LIBRARYDEVELOPMENT = "{C93D6B18-20B5-4927-8643-48F176E8352A}";
		}
	}
}
