using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Options;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000D6 RID: 214
	internal abstract class OptionsHelper
	{
		// Token: 0x06000F4F RID: 3919 RVA: 0x000293E0 File Offset: 0x000283E0
		public static IVersionConstraintToSave CreateVersionConstraintToSave(VersionConstraint constraint)
		{
			NewestVersionConstraint newestVersionConstraint = constraint as NewestVersionConstraint;
			if (newestVersionConstraint != null)
			{
				return new NewestVersionConstraintToSave(newestVersionConstraint);
			}
			ExactVersionConstraint exactVersionConstraint = constraint as ExactVersionConstraint;
			if (exactVersionConstraint != null)
			{
				return new ExactVersionConstraintToSave(exactVersionConstraint);
			}
			return null;
		}

		// Token: 0x06000F50 RID: 3920 RVA: 0x00029410 File Offset: 0x00028410
		public static void SetCompilerVersionConstraint(VersionConstraint constraint)
		{
			OptionsHelper.GetProjectOptionKey(true)["VersionConstraint"] = OptionsHelper.CreateVersionConstraintToSave(constraint);
		}

		// Token: 0x06000F51 RID: 3921 RVA: 0x00029428 File Offset: 0x00028428
		internal static void SetCompileOption(string stKey, bool bValue)
		{
			OptionsHelper.GetProjectOptionKey(true)[stKey] = bValue;
		}

		// Token: 0x06000F52 RID: 3922 RVA: 0x0002943C File Offset: 0x0002843C
		public static VersionConstraint GetCompilerVersionConstraint()
		{
			IOptionKey projectOptionKey = OptionsHelper.GetProjectOptionKey(false);
			string stValue = "VersionConstraint";
			if (projectOptionKey != null && projectOptionKey.HasValue(stValue, typeof(IVersionConstraintToSave)))
			{
				IVersionConstraintToSave versionConstraintToSave = (IVersionConstraintToSave)projectOptionKey[stValue];
				if (versionConstraintToSave != null)
				{
					return versionConstraintToSave.Constraint;
				}
			}
			return null;
		}

		// Token: 0x06000F53 RID: 3923 RVA: 0x00029484 File Offset: 0x00028484
		public static VersionConstraint GetCompilerVersionConstraint(int nProjectHandle)
		{
			IOptionKey projectOptionsRootKey = APEnvironmentFacade.Instance.GetProjectOptionsRootKey(nProjectHandle);
			if (projectOptionsRootKey != null)
			{
				IOptionKey optionKey = projectOptionsRootKey.OpenSubKey("{535658C0-5AF5-460d-99A4-BFFB984A829A}");
				if (optionKey != null)
				{
					string stValue = "VersionConstraint";
					if (optionKey.HasValue(stValue, typeof(IVersionConstraintToSave)))
					{
						IVersionConstraintToSave versionConstraintToSave = (IVersionConstraintToSave)optionKey[stValue];
						if (versionConstraintToSave != null)
						{
							return versionConstraintToSave.Constraint;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06000F54 RID: 3924 RVA: 0x000294E0 File Offset: 0x000284E0
		private static IOptionKey GetProjectOptionKey(bool bCreate)
		{
			if (bCreate)
			{
				return APEnvironmentFacade.Instance.CreateSubKey(OptionRoot.Project, "{535658C0-5AF5-460d-99A4-BFFB984A829A}");
			}
			return APEnvironmentFacade.Instance.OpenSubKey(OptionRoot.Project, "{535658C0-5AF5-460d-99A4-BFFB984A829A}");
		}

		// Token: 0x04000371 RID: 881
		public const string COMPILER_VERSION_CONSTRAINT_KEY = "VersionConstraint";

		// Token: 0x04000372 RID: 882
		internal const string REPLACE_CONSTANTS = "ReplaceConstants";

		// Token: 0x04000373 RID: 883
		internal const string ENABLE_BREAKPOINT_LOGGING = "EnableBreakpointLogging";

		// Token: 0x04000374 RID: 884
		public const string SUB_KEY = "{535658C0-5AF5-460d-99A4-BFFB984A829A}";
	}
}
