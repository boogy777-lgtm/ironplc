using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Options;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200010B RID: 267
	[TypeGuid("{7C0767B2-A6CB-4716-8697-D33A70B7E49F}")]
	public class CompileOptions : ILMCompileOptions3, _ICompileOptions2, _ICompileOptions, ILMCompileOptions, ILMCompileOptions2
	{
		// Token: 0x0600141E RID: 5150 RVA: 0x0003BC4F File Offset: 0x0003AC4F
		public CompileOptions()
		{
			this.AttachToEngine();
			this.AttachToOptionStorage();
		}

		// Token: 0x0600141F RID: 5151 RVA: 0x0003BC84 File Offset: 0x0003AC84
		private void AttachToOptionStorage()
		{
			APEnvironmentFacade.Instance.OptionCreated += this.OnOptionCreated;
			APEnvironmentFacade.Instance.OptionChanged += this.OnOptionChanged;
			APEnvironmentFacade.Instance.OptionDeleted += this.OnOptionDeleted;
		}

		// Token: 0x06001420 RID: 5152 RVA: 0x0003BCD3 File Offset: 0x0003ACD3
		private void AttachToEngine()
		{
			APEnvironmentFacade.Instance.BeforePrimaryProjectSwitched += this.OnBeforePrimaryProjectSwitched;
		}

		// Token: 0x06001421 RID: 5153 RVA: 0x0003BCEB File Offset: 0x0003ACEB
		public void OnBeforePrimaryProjectSwitched(IProject oldProject, IProject newProject)
		{
			this._bDoUpdate = true;
		}

		// Token: 0x06001422 RID: 5154 RVA: 0x0003BCF4 File Offset: 0x0003ACF4
		private void OnOptionCreated(object sender, OptionEventArgs e)
		{
			if (e.OptionKey != null && e.OptionKey.Name == "{E709B08B-B6E4-4966-8EED-D793A13114C6}")
			{
				this._bDoUpdate = true;
			}
		}

		// Token: 0x06001423 RID: 5155 RVA: 0x0003BD1C File Offset: 0x0003AD1C
		public void OnOptionChanged(object sender, OptionEventArgs e)
		{
			if (e.OptionKey != null && e.OptionKey.Name == "{E709B08B-B6E4-4966-8EED-D793A13114C6}")
			{
				this._bDoUpdate = true;
			}
			if (e.OptionKey != null && e.OptionKey.Name == SmartCodingOptionsHelper.SMARTCODING_SUB_KEY)
			{
				APEnvironmentFacade.Instance.LanguageModelMgr.ShowPrecomOptionChanged();
			}
		}

		// Token: 0x06001424 RID: 5156 RVA: 0x0003BCF4 File Offset: 0x0003ACF4
		private void OnOptionDeleted(object sender, OptionEventArgs e)
		{
			if (e.OptionKey != null && e.OptionKey.Name == "{E709B08B-B6E4-4966-8EED-D793A13114C6}")
			{
				this._bDoUpdate = true;
			}
		}

		// Token: 0x06001425 RID: 5157 RVA: 0x0003BD80 File Offset: 0x0003AD80
		internal void Update()
		{
			if (this._bDoUpdate)
			{
				this._bUnicodeIdentifiers = CompileOptions.LocalOptions.UnicodeIdentifiers;
				this._bReplaceConstants = CompileOptions.LocalOptions.ReplaceConstants;
				this._bEnableBreakpointLogging = CompileOptions.LocalOptions.EnableBreakpointLogging;
				this._nMaxCompilerWarnings = CompileOptions.LocalOptions.MaxCompilerWarnings;
				this._bUtf8Encoding = CompileOptions.LocalOptions.Utf8Encoding;
				this._projectDefines = CompileOptions.LocalOptions.ProjectDefines;
				this._bReportCompiledPousDuringIncrementalCompile = CompileOptions.LocalOptions.ReportCompiledPousDuringIncrementalCompile;
				this._bDoUpdate = false;
			}
		}

		// Token: 0x17000579 RID: 1401
		// (get) Token: 0x06001426 RID: 5158 RVA: 0x0003BDE9 File Offset: 0x0003ADE9
		// (set) Token: 0x06001427 RID: 5159 RVA: 0x0003BDF7 File Offset: 0x0003ADF7
		public bool ReplaceConstants
		{
			get
			{
				this.Update();
				return this._bReplaceConstants;
			}
			set
			{
				CompileOptions.LocalOptions.ReplaceConstants = value;
				this._bDoUpdate = true;
			}
		}

		// Token: 0x1700057A RID: 1402
		// (get) Token: 0x06001428 RID: 5160 RVA: 0x0003BE06 File Offset: 0x0003AE06
		// (set) Token: 0x06001429 RID: 5161 RVA: 0x0003BE14 File Offset: 0x0003AE14
		public bool UnicodeIdentifiers
		{
			get
			{
				this.Update();
				return this._bUnicodeIdentifiers;
			}
			set
			{
				CompileOptions.LocalOptions.UnicodeIdentifiers = value;
				this._bDoUpdate = true;
			}
		}

		// Token: 0x1700057B RID: 1403
		// (get) Token: 0x0600142A RID: 5162 RVA: 0x0003BE23 File Offset: 0x0003AE23
		// (set) Token: 0x0600142B RID: 5163 RVA: 0x0003BE31 File Offset: 0x0003AE31
		public int MaxCompilerWarnings
		{
			get
			{
				this.Update();
				return this._nMaxCompilerWarnings;
			}
			set
			{
				CompileOptions.LocalOptions.MaxCompilerWarnings = value;
				this._bDoUpdate = true;
			}
		}

		// Token: 0x1700057C RID: 1404
		// (get) Token: 0x0600142C RID: 5164 RVA: 0x0003BE40 File Offset: 0x0003AE40
		// (set) Token: 0x0600142D RID: 5165 RVA: 0x0003BE7B File Offset: 0x0003AE7B
		public bool EnableBreakpointLogging
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse() < new Version("3.5.5.0") && CompileOptions.LocalOptions.EnableBreakpointLogging)
				{
					CompileOptions.LocalOptions.EnableBreakpointLogging = false;
				}
				this.Update();
				return this._bEnableBreakpointLogging;
			}
			set
			{
				CompileOptions.LocalOptions.EnableBreakpointLogging = value;
				this._bDoUpdate = true;
			}
		}

		// Token: 0x1700057D RID: 1405
		// (get) Token: 0x0600142E RID: 5166 RVA: 0x0003BE8A File Offset: 0x0003AE8A
		// (set) Token: 0x0600142F RID: 5167 RVA: 0x0003BEC5 File Offset: 0x0003AEC5
		public bool UTF8Encoding
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse() < new Version("3.5.18.0") && CompileOptions.LocalOptions.Utf8Encoding)
				{
					CompileOptions.LocalOptions.Utf8Encoding = false;
				}
				this.Update();
				return this._bUtf8Encoding;
			}
			set
			{
				CompileOptions.LocalOptions.Utf8Encoding = value;
				this._bDoUpdate = true;
			}
		}

		// Token: 0x1700057E RID: 1406
		// (get) Token: 0x06001430 RID: 5168 RVA: 0x0003BED4 File Offset: 0x0003AED4
		// (set) Token: 0x06001431 RID: 5169 RVA: 0x0003BF28 File Offset: 0x0003AF28
		public string ProjectDefines
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse() < new Version(3, 5, 20, 0) && CompileOptions.LocalOptions.ProjectDefines != string.Empty)
				{
					CompileOptions.LocalOptions.ProjectDefines = string.Empty;
				}
				this.Update();
				return this._projectDefines;
			}
			set
			{
				CompileOptions.LocalOptions.ProjectDefines = value;
				this._projectDefines = value;
			}
		}

		// Token: 0x1700057F RID: 1407
		// (get) Token: 0x06001432 RID: 5170 RVA: 0x0003BF37 File Offset: 0x0003AF37
		// (set) Token: 0x06001433 RID: 5171 RVA: 0x0003BF72 File Offset: 0x0003AF72
		public bool ReportCompiledPousDuringIncrementalCompile
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse() < new Version("3.5.20.0") && !CompileOptions.LocalOptions.ReportCompiledPousDuringIncrementalCompile)
				{
					CompileOptions.LocalOptions.ReportCompiledPousDuringIncrementalCompile = true;
				}
				this.Update();
				return this._bReportCompiledPousDuringIncrementalCompile;
			}
			set
			{
				CompileOptions.LocalOptions.ReportCompiledPousDuringIncrementalCompile = value;
				this._bDoUpdate = true;
			}
		}

		// Token: 0x0400048B RID: 1163
		private bool _bUnicodeIdentifiers;

		// Token: 0x0400048C RID: 1164
		private bool _bReplaceConstants;

		// Token: 0x0400048D RID: 1165
		private bool _bEnableBreakpointLogging;

		// Token: 0x0400048E RID: 1166
		private bool _bDoUpdate = true;

		// Token: 0x0400048F RID: 1167
		private int _nMaxCompilerWarnings = 100;

		// Token: 0x04000490 RID: 1168
		private bool _bUtf8Encoding;

		// Token: 0x04000491 RID: 1169
		private bool _bReportCompiledPousDuringIncrementalCompile = true;

		// Token: 0x04000492 RID: 1170
		private string _projectDefines = string.Empty;

		// Token: 0x020002AB RID: 683
		internal abstract class LocalOptions
		{
			// Token: 0x06002B9F RID: 11167 RVA: 0x00073A4C File Offset: 0x00072A4C
			internal static bool GetCompileOption(string stKey, bool bDefault)
			{
				IOptionKey projectOptionKey = CompileOptions.LocalOptions.GetProjectOptionKey(false);
				if (projectOptionKey != null && projectOptionKey.HasValue(stKey, typeof(bool)))
				{
					return (bool)projectOptionKey[stKey];
				}
				return bDefault;
			}

			// Token: 0x06002BA0 RID: 11168 RVA: 0x00073A84 File Offset: 0x00072A84
			internal static int GetCompileOption(string stKey, int nDefault)
			{
				IOptionKey projectOptionKey = CompileOptions.LocalOptions.GetProjectOptionKey(false);
				if (projectOptionKey != null && projectOptionKey.HasValue(stKey, typeof(int)))
				{
					return (int)projectOptionKey[stKey];
				}
				return nDefault;
			}

			// Token: 0x06002BA1 RID: 11169 RVA: 0x00073ABC File Offset: 0x00072ABC
			internal static void SetCompileOption(string stKey, int nValue)
			{
				CompileOptions.LocalOptions.GetProjectOptionKey(true)[stKey] = nValue;
			}

			// Token: 0x06002BA2 RID: 11170 RVA: 0x00073AD0 File Offset: 0x00072AD0
			internal static void SetCompileOption(string stKey, bool bValue)
			{
				CompileOptions.LocalOptions.GetProjectOptionKey(true)[stKey] = bValue;
			}

			// Token: 0x06002BA3 RID: 11171 RVA: 0x00073AE4 File Offset: 0x00072AE4
			internal static string GetCompileOption(string stKey, string sDefault)
			{
				IOptionKey projectOptionKey = CompileOptions.LocalOptions.GetProjectOptionKey(false);
				if (projectOptionKey != null && projectOptionKey.HasValue(stKey, typeof(string)))
				{
					return (string)projectOptionKey[stKey];
				}
				return sDefault;
			}

			// Token: 0x06002BA4 RID: 11172 RVA: 0x00073B1C File Offset: 0x00072B1C
			internal static void SetCompileOption(string stKey, string sValue)
			{
				CompileOptions.LocalOptions.GetProjectOptionKey(true)[stKey] = sValue;
			}

			// Token: 0x06002BA5 RID: 11173 RVA: 0x00073B2B File Offset: 0x00072B2B
			private static IOptionKey GetProjectOptionKey(bool bCreate)
			{
				if (bCreate)
				{
					return APEnvironmentFacade.Instance.CreateSubKey(OptionRoot.Project, "{E709B08B-B6E4-4966-8EED-D793A13114C6}");
				}
				return APEnvironmentFacade.Instance.OpenSubKey(OptionRoot.Project, "{E709B08B-B6E4-4966-8EED-D793A13114C6}");
			}

			// Token: 0x17000C0F RID: 3087
			// (get) Token: 0x06002BA6 RID: 11174 RVA: 0x00073B51 File Offset: 0x00072B51
			// (set) Token: 0x06002BA7 RID: 11175 RVA: 0x00073B5E File Offset: 0x00072B5E
			internal static bool SaveParseTree
			{
				get
				{
					return CompileOptions.LocalOptions.GetCompileOption("SaveParseTree", false);
				}
				set
				{
					CompileOptions.LocalOptions.SetCompileOption("SaveParseTree", value);
				}
			}

			// Token: 0x17000C10 RID: 3088
			// (get) Token: 0x06002BA8 RID: 11176 RVA: 0x00073B6B File Offset: 0x00072B6B
			// (set) Token: 0x06002BA9 RID: 11177 RVA: 0x00073B78 File Offset: 0x00072B78
			internal static bool UnicodeIdentifiers
			{
				get
				{
					return CompileOptions.LocalOptions.GetCompileOption("UnicodeIdentifiers", false);
				}
				set
				{
					CompileOptions.LocalOptions.SetCompileOption("UnicodeIdentifiers", value);
				}
			}

			// Token: 0x17000C11 RID: 3089
			// (get) Token: 0x06002BAA RID: 11178 RVA: 0x00073B85 File Offset: 0x00072B85
			// (set) Token: 0x06002BAB RID: 11179 RVA: 0x00073B92 File Offset: 0x00072B92
			internal static bool ReplaceConstants
			{
				get
				{
					return CompileOptions.LocalOptions.GetCompileOption("ReplaceConstants", true);
				}
				set
				{
					CompileOptions.LocalOptions.SetCompileOption("ReplaceConstants", value);
				}
			}

			// Token: 0x17000C12 RID: 3090
			// (get) Token: 0x06002BAC RID: 11180 RVA: 0x00073B9F File Offset: 0x00072B9F
			// (set) Token: 0x06002BAD RID: 11181 RVA: 0x00073BAD File Offset: 0x00072BAD
			internal static int MaxCompilerWarnings
			{
				get
				{
					return CompileOptions.LocalOptions.GetCompileOption("MaxCompilerWarnings", 100);
				}
				set
				{
					CompileOptions.LocalOptions.SetCompileOption("MaxCompilerWarnings", value);
				}
			}

			// Token: 0x17000C13 RID: 3091
			// (get) Token: 0x06002BAE RID: 11182 RVA: 0x00073BBA File Offset: 0x00072BBA
			// (set) Token: 0x06002BAF RID: 11183 RVA: 0x00073BC7 File Offset: 0x00072BC7
			public static bool EnableBreakpointLogging
			{
				get
				{
					return CompileOptions.LocalOptions.GetCompileOption("EnableBreakpointLogging", true);
				}
				set
				{
					CompileOptions.LocalOptions.SetCompileOption("EnableBreakpointLogging", value);
				}
			}

			// Token: 0x17000C14 RID: 3092
			// (get) Token: 0x06002BB0 RID: 11184 RVA: 0x00073BD4 File Offset: 0x00072BD4
			// (set) Token: 0x06002BB1 RID: 11185 RVA: 0x00073BE1 File Offset: 0x00072BE1
			public static bool Utf8Encoding
			{
				get
				{
					return CompileOptions.LocalOptions.GetCompileOption("Utf8Encoding", false);
				}
				set
				{
					CompileOptions.LocalOptions.SetCompileOption("Utf8Encoding", value);
				}
			}

			// Token: 0x17000C15 RID: 3093
			// (get) Token: 0x06002BB2 RID: 11186 RVA: 0x00073BEE File Offset: 0x00072BEE
			// (set) Token: 0x06002BB3 RID: 11187 RVA: 0x00073BFB File Offset: 0x00072BFB
			internal static bool ReportCompiledPousDuringIncrementalCompile
			{
				get
				{
					return CompileOptions.LocalOptions.GetCompileOption("ReportCompiledPousDuringIncrementalCompile", false);
				}
				set
				{
					CompileOptions.LocalOptions.SetCompileOption("ReportCompiledPousDuringIncrementalCompile", value);
				}
			}

			// Token: 0x17000C16 RID: 3094
			// (get) Token: 0x06002BB4 RID: 11188 RVA: 0x00073C08 File Offset: 0x00072C08
			// (set) Token: 0x06002BB5 RID: 11189 RVA: 0x00073C19 File Offset: 0x00072C19
			public static string ProjectDefines
			{
				get
				{
					return CompileOptions.LocalOptions.GetCompileOption("ProjectDefines", string.Empty);
				}
				set
				{
					CompileOptions.LocalOptions.SetCompileOption("ProjectDefines", value);
				}
			}

			// Token: 0x0400089A RID: 2202
			internal const string SAVE_PARSE_TREE = "SaveParseTree";

			// Token: 0x0400089B RID: 2203
			internal const string UNICODE_IDENTIFIERS = "UnicodeIdentifiers";

			// Token: 0x0400089C RID: 2204
			internal const string REPLACE_CONSTANTS = "ReplaceConstants";

			// Token: 0x0400089D RID: 2205
			internal const string ENABLE_BREAKPOINT_LOGGING = "EnableBreakpointLogging";

			// Token: 0x0400089E RID: 2206
			internal const string MAX_COMPILER_WARNINGS = "MaxCompilerWarnings";

			// Token: 0x0400089F RID: 2207
			internal const string UTF8_ENCODING = "Utf8Encoding";

			// Token: 0x040008A0 RID: 2208
			internal const string REPORT_COMPILED_POUS_DURING_INCREMENTAL_COMPILE = "ReportCompiledPousDuringIncrementalCompile";

			// Token: 0x040008A1 RID: 2209
			internal const string PROJECT_DEFINES = "ProjectDefines";

			// Token: 0x040008A2 RID: 2210
			internal const string SUB_KEY_COMPILE = "{E709B08B-B6E4-4966-8EED-D793A13114C6}";
		}
	}
}
