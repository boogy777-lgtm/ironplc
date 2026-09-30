using System;
using System.Diagnostics;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.ProjectInfoObject;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200001A RID: 26
	public struct ProjectInfoObjectEvaluator
	{
		// Token: 0x0600002F RID: 47 RVA: 0x0000247E File Offset: 0x0000147E
		public ProjectInfoObjectEvaluator(IObjectManager19 objectMgr)
		{
			this._objectMgr = objectMgr;
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00002488 File Offset: 0x00001488
		private bool HasBoolValue(IProjectInfoObject proInfo, string stKey)
		{
			if (proInfo.ContainsValue(stKey))
			{
				object value = proInfo.GetValue(stKey);
				return value is bool && (bool)value;
			}
			return false;
		}

		// Token: 0x06000031 RID: 49 RVA: 0x000024B8 File Offset: 0x000014B8
		private bool HasStringValue(IProjectInfoObject proInfo, string stKey, out string stValue)
		{
			stValue = null;
			bool flag = proInfo.ContainsValue(stKey);
			if (flag)
			{
				stValue = (proInfo.GetValue(stKey) as string);
			}
			return flag;
		}

		// Token: 0x06000032 RID: 50 RVA: 0x000024D8 File Offset: 0x000014D8
		private LibraryInfo EvaluateProjectInfoObject(int iProjectHandle, Guid gdProjectInfo)
		{
			LibraryInfo result = new LibraryInfo(string.Empty);
			if (this._objectMgr.ExistsObject(iProjectHandle, gdProjectInfo))
			{
				IProjectInfoObject proInfo = (IProjectInfoObject)this._objectMgr.GetObjectToRead(iProjectHandle, gdProjectInfo).Object;
				result.LinkInSimulation = this.HasBoolValue(proInfo, "LinkInSimulation");
				result.IsInterfaceLibrary = (this.HasBoolValue(proInfo, "IsInterfaceLibrary") && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35200);
				string text;
				result.QualifiedAccessOnly = (this.HasStringValue(proInfo, "LanguageModelAttribute", out text) && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34200 && CompileAttributes.ATTRIBUTE_QUALIFIED_ACCESS_ONLY.Equals(text));
				result.Support32BitOnly = (this.HasBoolValue(proInfo, "Support32BitOnly") && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35500);
				result.OnlineChangeable = this.HasBoolValue(proInfo, "OnlineChangeable");
				result.IgnoreLinkAll = this.HasBoolValue(proInfo, "IgnoreLinkAll");
				if (this.HasStringValue(proInfo, "UnitTestingDefine", out text) && text != null)
				{
					result.UnitTestingDefine = text;
				}
			}
			return result;
		}

		// Token: 0x06000033 RID: 51 RVA: 0x000025F4 File Offset: 0x000015F4
		private IProjectInfoObject GetProjectInfoObject(string stLibraryId)
		{
			try
			{
				int projectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(stLibraryId);
				if (this._objectMgr.ExistsObject(projectHandle, ProjectInfoObjectEvaluator.s_gdProjectInfo))
				{
					return this._objectMgr.GetObjectToRead(projectHandle, ProjectInfoObjectEvaluator.s_gdProjectInfo).Object as IProjectInfoObject;
				}
			}
			catch
			{
				return null;
			}
			return null;
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00002664 File Offset: 0x00001664
		public LibraryInfo GetLibraryInfo(string stLibraryId)
		{
			LibraryInfo result = new LibraryInfo(string.Empty);
			try
			{
				IProject projectByLibraryId = APEnvironmentFacade.Instance.GetProjectByLibraryId(stLibraryId);
				if (projectByLibraryId != null && projectByLibraryId.Library)
				{
					result = this.EvaluateProjectInfoObject(projectByLibraryId.Handle, ProjectInfoObjectEvaluator.s_gdProjectInfo);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("HaveToLinkInSimulation failed: " + ex.ToString());
			}
			return result;
		}

		// Token: 0x06000035 RID: 53 RVA: 0x000026D4 File Offset: 0x000016D4
		public bool IsProjectInfoObjectBoolFlagSet(int iProjectHandle, string stKey)
		{
			bool result = false;
			if (this._objectMgr.ExistsObject(iProjectHandle, ProjectInfoObjectEvaluator.s_gdProjectInfo))
			{
				IProjectInfoObject proInfo = (IProjectInfoObject)this._objectMgr.GetObjectToRead(iProjectHandle, ProjectInfoObjectEvaluator.s_gdProjectInfo).Object;
				result = this.HasBoolValue(proInfo, stKey);
			}
			return result;
		}

		// Token: 0x06000036 RID: 54 RVA: 0x0000271C File Offset: 0x0000171C
		public bool IsProjectInfoObjectBoolFlagSet(string stLibraryId, string stKey)
		{
			IProjectInfoObject projectInfoObject = this.GetProjectInfoObject(stLibraryId);
			bool result = false;
			if (projectInfoObject != null)
			{
				result = this.HasBoolValue(projectInfoObject, stKey);
			}
			return result;
		}

		// Token: 0x04000002 RID: 2
		private readonly IObjectManager19 _objectMgr;

		// Token: 0x04000003 RID: 3
		private static readonly Guid s_gdProjectInfo = new Guid("{11C0FC3A-9BCF-4dd8-AC38-EFB93363E521}");
	}
}
