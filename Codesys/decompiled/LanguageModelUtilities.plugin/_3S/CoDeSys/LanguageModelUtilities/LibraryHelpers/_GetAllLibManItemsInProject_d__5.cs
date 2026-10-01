using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LibManObject;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal static class LibraryHelpers
	{
		public static IManagedLibrary GetManagedLibFromLibManItem(ILibManItem lmi)
		{
			try
			{
				IPlaceholderLibManItem placeholderLibManItem = lmi as IPlaceholderLibManItem;
				IManagedLibManItem managedLibManItem = lmi as IManagedLibManItem;
				if (placeholderLibManItem != null && placeholderLibManItem.EffectiveResolution != null)
				{
					return placeholderLibManItem.EffectiveResolution;
				}
				if (managedLibManItem != null && managedLibManItem.ManagedLibrary != null)
				{
					return managedLibManItem.ManagedLibrary;
				}
			}
			catch
			{
			}
			return null;
		}

		private static bool ResolvesToLibFromDisplayName(ILibManItem lmi, string stDisplayName, IGetLibInformation libInfo)
		{
			IManagedLibrary managedLib = libInfo.GetManagedLib(lmi);
			if (managedLib != null)
			{
				return string.Compare(stDisplayName, managedLib.DisplayName, StringComparison.InvariantCultureIgnoreCase) == 0;
			}
			return false;
		}

		private static string GetProjectIdFromLibManItem(ILibManItem lmi, IGetLibInformation libInfo)
		{
			IManagedLibrary managedLib = libInfo.GetManagedLib(lmi);
			if (managedLib != null)
			{
				return managedLib.DisplayName;
			}
			return lmi.Name;
		}

		public static IProject GetProjectById(string stId)
		{
			return APEnvironmentFacade.Instance.GetLibraryById(stId);
		}

		public static IProject GetProjectFromLibManItem(ILibManItem lmi, IGetLibInformation libInfo)
		{
			IProject projectById = GetProjectById(GetProjectIdFromLibManItem(lmi, libInfo));
			if (projectById == null || !projectById.Library)
			{
				return null;
			}
			return projectById;
		}

		public static IEnumerable<ILibManItem> GetAllLibManItemsInProject(int nProj, Guid? gdApp)
		{
			Guid[] allObjects = APEnvironmentFacade.Instance.GetAllObjects(nProj);
			foreach (Guid objectGuid in allObjects)
			{
				IMetaObjectStub metaObjectStub = APEnvironmentFacade.Instance.GetMetaObjectStub(nProj, objectGuid);
				if (!typeof(ILibManObject).IsAssignableFrom(metaObjectStub.ObjectType))
				{
					continue;
				}
				int num;
				if (!gdApp.HasValue)
				{
					num = 1;
				}
				else
				{
					Guid parentObjectGuid = metaObjectStub.ParentObjectGuid;
					Guid? guid = gdApp;
					num = ((parentObjectGuid == guid) ? 1 : 0);
				}
				if (num == 0)
				{
					continue;
				}
				ILibManObject libManObject = (ILibManObject)APEnvironmentFacade.Instance.GetObjectToRead(nProj, objectGuid).Object;
				foreach (ILibManItem item in libManObject)
				{
					yield return item;
				}
			}
		}

		public static bool FindLibByDisplayName(int nProj, Guid? gdApp, string stDisplayName, Stack<int> itemPath, IGetLibInformation2 libInfo)
		{
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			if (stDisplayName == null)
			{
				return false;
			}
			if (stDisplayName == string.Empty)
			{
				return true;
			}
			Queue<Pair<Stack<int>, ILibManItem>> queue = new Queue<Pair<Stack<int>, ILibManItem>>();
			foreach (ILibManItem item in gdApp.HasValue ? libInfo.GetProjectLibs(nProj, gdApp.Value) : libInfo.GetProjectLibs(nProj))
			{
				queue.Enqueue(new Pair<Stack<int>, ILibManItem>(new Stack<int>(Fun.Tuple<int>(new int[1] { nProj })), item));
			}
			while (queue.Count > 0)
			{
				Pair<Stack<int>, ILibManItem> val = queue.Dequeue();
				ILibManItem second = val.second;
				if (ResolvesToLibFromDisplayName(second, stDisplayName, libInfo))
				{
					IProject projectFromLibManItem = GetProjectFromLibManItem(second, libInfo);
					if (projectFromLibManItem != null)
					{
						CloneStack(val.first, itemPath);
						itemPath.Push(projectFromLibManItem.Handle);
						return true;
					}
				}
				else
				{
					if (second.SystemLibrary)
					{
						continue;
					}
					IProject projectFromLibManItem2 = GetProjectFromLibManItem(second, libInfo);
					if (projectFromLibManItem2 == null || val.first.Contains(projectFromLibManItem2.Handle))
					{
						continue;
					}
					foreach (ILibManItem projectLib in libInfo.GetProjectLibs(projectFromLibManItem2.Handle))
					{
						Stack<int> stack = new Stack<int>(val.first);
						stack.Push(projectFromLibManItem2.Handle);
						queue.Enqueue(new Pair<Stack<int>, ILibManItem>(stack, projectLib));
					}
				}
			}
			return false;
		}

		private static void CloneStack<T>(Stack<T> src, Stack<T> dest)
		{
			foreach (T item in src)
			{
				dest.Push(item);
			}
		}

		public static Pair<int, int> FindLibByNamespace(int nProj, Guid? gdApp, DPath namespacePath, out IList<string> restSigs, IGetLibInformation2 libInfo)
		{
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			int num = nProj;
			restSigs = new List<string>(namespacePath.get_Components());
			foreach (string component in namespacePath.get_Components())
			{
				bool flag = false;
				foreach (ILibManItem item in gdApp.HasValue ? libInfo.GetProjectLibs(nProj, gdApp.Value) : libInfo.GetProjectLibs(nProj))
				{
					if (string.Compare(component, item.Namespace, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						gdApp = null;
						num = nProj;
						nProj = GetProjectFromLibManItem(item, libInfo).Handle;
						restSigs.Remove(component);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					break;
				}
			}
			return new Pair<int, int>(nProj, num);
		}

		public static IProject GetAttractingProject(int nContextProj, Guid gdContextApp, int nAttractedProj, IGetLibInformation libInfo)
		{
			Stack<ILibManItem> stack = new Stack<ILibManItem>();
			IProject projectFromHandle = GetProjectFromHandle(nContextProj);
			IProject projectFromHandle2 = GetProjectFromHandle(nAttractedProj);
			object libInfo2;
			if (!(libInfo is IGetLibInformation2))
			{
				IGetLibInformation2 getLibInformation = new GetLibInformation();
				libInfo2 = getLibInformation;
			}
			else
			{
				libInfo2 = (IGetLibInformation2)libInfo;
			}
			GetItemPathFromProjectRec(stack, projectFromHandle, gdContextApp, projectFromHandle2, (IGetLibInformation2)libInfo2);
			if (stack.Count > 1)
			{
				stack.Pop();
				IProject projectFromLibManItem = GetProjectFromLibManItem(stack.Peek(), libInfo);
				if (projectFromLibManItem == null)
				{
					return GetProjectFromHandle(nContextProj);
				}
				return projectFromLibManItem;
			}
			return GetProjectFromHandle(nContextProj);
		}

		private static IProject GetProjectFromHandle(int nProj)
		{
			return APEnvironmentFacade.Instance.GetProjectFromHandle(nProj);
		}

		public static IProject GetProjectByLibNamespacePath(int nProj, Guid? gdApp, string stPath, IGetLibInformation2 libInfo)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected O, but got Unknown
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			DPath namespacePath = new DPath(stPath);
			IList<string> restSigs;
			return GetProjectFromHandle(FindLibByNamespace(nProj, gdApp, namespacePath, out restSigs, libInfo).first);
		}

		public static bool GetItemPathFromProjectRec(Stack<ILibManItem> itemPath, IProject proToLookIn, Guid gdApp, IProject proToLookFor, IGetLibInformation2 libInfo)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			if (proToLookIn.Handle == proToLookFor.Handle)
			{
				return true;
			}
			Queue<Pair<Stack<ILibManItem>, ILibManItem>> queue = new Queue<Pair<Stack<ILibManItem>, ILibManItem>>();
			foreach (ILibManItem projectLib in libInfo.GetProjectLibs(proToLookIn.Handle, gdApp))
			{
				queue.Enqueue(new Pair<Stack<ILibManItem>, ILibManItem>(new Stack<ILibManItem>(), projectLib));
			}
			while (queue.Count > 0)
			{
				Pair<Stack<ILibManItem>, ILibManItem> val = queue.Dequeue();
				ILibManItem second = val.second;
				if (string.Compare(GetProjectIdFromLibManItem(second, libInfo), proToLookFor.Id, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					CloneStack(val.first, itemPath);
					itemPath.Push(second);
					return true;
				}
				if (second.SystemLibrary)
				{
					continue;
				}
				IProject projectFromLibManItem = GetProjectFromLibManItem(second, libInfo);
				if (projectFromLibManItem == null || val.first.Contains(second))
				{
					continue;
				}
				Stack<ILibManItem> stack = new Stack<ILibManItem>(val.first);
				stack.Push(second);
				foreach (ILibManItem projectLib2 in libInfo.GetProjectLibs(projectFromLibManItem.Handle))
				{
					queue.Enqueue(new Pair<Stack<ILibManItem>, ILibManItem>(stack, projectLib2));
				}
			}
			return false;
		}
	}
}
