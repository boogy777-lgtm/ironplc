using System;
using System.Collections.Generic;
using _3S.CoDeSys.ApplicationObject;
using _3S.CoDeSys.Controls.Controls;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManagerConfigurators.OnlineChange;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	internal class Utilities
	{
		internal static readonly Guid GUID_BUILDPROPERTY = new Guid("{24568A24-C491-472c-A21F-EE5D33859FAB}");

		internal static bool MemoryReserveSupported => APEnvironment.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 12, 0);

		internal static int GetMemoryReserveForPOU(int nProjectHandle, Guid objectGuid)
		{
			IBuildProperty5 buildProperty = APEnvironment.ObjectMgr.GetObjectToRead(nProjectHandle, objectGuid).GetProperty(GUID_BUILDPROPERTY) as IBuildProperty5;
			int result = 0;
			if (buildProperty != null)
			{
				result = buildProperty.MemoryReserveForOnlineChange;
			}
			return result;
		}

		internal static bool StoreNewReserveValueInObject(int nProjectHandle, Guid pouGuid, int newValue)
		{
			bool result = false;
			try
			{
				IMetaObject objectToModify = APEnvironment.ObjectMgr.GetObjectToModify(nProjectHandle, pouGuid);
				IBuildProperty5 buildProperty = objectToModify.GetProperty(GUID_BUILDPROPERTY) as IBuildProperty5;
				if (buildProperty == null)
				{
					buildProperty = APEnvironment.CreateBuildProperty();
				}
				buildProperty.MemoryReserveForOnlineChange = newValue;
				objectToModify.AddProperty(buildProperty);
				APEnvironment.ObjectMgr.SetObject(objectToModify, bCommit: true, null);
				result = true;
				return result;
			}
			catch (Exception)
			{
				return result;
			}
		}

		internal static int GetCompiledSizeOfSignature(Guid appGuid, IPreCompileContext14 precom, ISignature sign)
		{
			return ((APEnvironment.LanguageModelMgr.GetCompileContext(appGuid) as ICompileContext20)?.GetSignature(sign.ObjectGuid))?.Size ?? 0;
		}

		internal static int GetRemainingMemReserveSizeOfSignature(Guid appGuid, IPreCompileContext14 precom, ISignature sign)
		{
			return ((APEnvironment.LanguageModelMgr.GetCompileContext(appGuid) as ICompileContext20)?.GetSignature(sign.ObjectGuid) as ISignature7)?.RemainingSizeOfMemoryReserve ?? 0;
		}

		internal static int GetInstanceCount(Guid appGuid, IPreCompileContext14 precom, ISignature sign)
		{
			if (!(APEnvironment.LanguageModelMgr.GetCompileContext(appGuid) is ICompileContext20 compileContext))
			{
				return 0;
			}
			ISignature signature = compileContext.GetSignature(sign.ObjectGuid);
			IVariable[] varInstances = null;
			ISignature[] declaringSignatures = null;
			return compileContext.InstancePaths(signature, out varInstances, out declaringSignatures, bWithNamespace: false, bWithStackVariables: false, bWithDerivedClasses: false).Length;
		}

		internal static int CalculateTotalAdditionalSizeForMemoryReserve(AllocationPlusModel model)
		{
			int num = 0;
			for (int i = 0; i < ((AbstractTreeTableModel)model).get_Sentinel().get_ChildCount(); i++)
			{
				if (((AbstractTreeTableModel)model).get_Sentinel().GetChild(i) is ModelNode modelNode)
				{
					num += modelNode.MemoryReserveSizeAllInstances;
				}
			}
			return num;
		}

		internal static IList<ApplicationOption> CollectApplications()
		{
			LList<ApplicationOption> val = new LList<ApplicationOption>();
			if (APEnvironment.Engine.Projects.PrimaryProject == null)
			{
				return (IList<ApplicationOption>)val;
			}
			int handle = APEnvironment.Engine.Projects.PrimaryProject.Handle;
			Guid[] allObjects = APEnvironment.ObjectMgr.GetAllObjects(handle);
			foreach (Guid guid in allObjects)
			{
				if (APEnvironment.ObjectMgr.ExistsObject(handle, guid))
				{
					IMetaObjectStub metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(handle, guid);
					if (typeof(IApplicationObject).IsAssignableFrom(metaObjectStub.ObjectType))
					{
						string fullName = APEnvironment.ObjectMgr.GetFullName(handle, guid);
						ApplicationOption applicationOption = new ApplicationOption
						{
							Name = fullName,
							Value = guid
						};
						val.Add(applicationOption);
					}
				}
			}
			return (IList<ApplicationOption>)val;
		}
	}
}
