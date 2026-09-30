using System;
using System.Linq;
using System.Reflection;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000B7 RID: 183
	internal class LibraryPlaceholdersLegacy
	{
		// Token: 0x06000AB4 RID: 2740 RVA: 0x0001A1C8 File Offset: 0x000191C8
		internal static _IPreCompileContext ResolveLibraryPlaceholder(ITargetSettings tarset, Guid appObjectGuid, LibraryPlaceholder placeholder, IDeviceIdentification devid, bool cached, out bool bIgnoreUnresolvedLib, out string stResolvedName)
		{
			bIgnoreUnresolvedLib = false;
			stResolvedName = null;
			if (devid == null)
			{
				return LibraryPlaceholdersLegacy.ResolveLibraryPlaceholder(tarset, appObjectGuid, placeholder);
			}
			LibraryPlaceholdersLegacy.LibPlaceholderIdentification libPlaceholderIdentification = new LibraryPlaceholdersLegacy.LibPlaceholderIdentification(devid, placeholder);
			object obj = LibraryPlaceholdersLegacy.s_htResolvedPlaceholdersLock;
			lock (obj)
			{
				object obj2;
				if (cached && LibraryPlaceholdersLegacy.s_htResolvedPlaceholders.TryGetValue(libPlaceholderIdentification, ref obj2))
				{
					if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3131)
					{
						return obj2 as _IPreCompileContext;
					}
					stResolvedName = (obj2 as string);
					return APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetLibraryContext(stResolvedName, false);
				}
			}
			object resolverByTypeGuid = LibraryPlaceholdersLegacy.GetResolverByTypeGuid(placeholder.m_guidResolver);
			string empty = string.Empty;
			string text = null;
			ILibraryPlaceholderResolution3 libraryPlaceholderResolution = resolverByTypeGuid as ILibraryPlaceholderResolution3;
			if (libraryPlaceholderResolution != null)
			{
				string text2;
				bool flag2;
				text = libraryPlaceholderResolution.ResolvePlaceholder(tarset, appObjectGuid, placeholder.m_stName, placeholder.m_stDefaultLibraryId, true, out empty, out text2, out flag2);
			}
			else if (resolverByTypeGuid is ILibraryPlaceholderResolution)
			{
				text = ((ILibraryPlaceholderResolution)resolverByTypeGuid).ResolvePlaceholder(tarset, placeholder.m_stName, placeholder.m_stDefaultLibraryId, out empty);
			}
			else if (resolverByTypeGuid is ILibraryPlaceholderResolutionEx && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
			{
				text = ((ILibraryPlaceholderResolutionEx)resolverByTypeGuid).ResolvePlaceholder(appObjectGuid, placeholder.m_stName, placeholder.m_stDefaultLibraryId, out empty);
			}
			if (text != null)
			{
				bIgnoreUnresolvedLib = (text == string.Empty);
				text = LibraryPlaceholdersLegacy.MaybeApplyMappingResolution(appObjectGuid, text, placeholder.Resolver);
			}
			_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetLibraryContext(text, false);
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35100 && resolverByTypeGuid is ILibraryPlaceholderResolution3 && libraryContext == null)
			{
				string text3;
				bool flag3;
				text = ((ILibraryPlaceholderResolution3)resolverByTypeGuid).ResolvePlaceholder(tarset, Guid.Empty, placeholder.m_stName, placeholder.m_stDefaultLibraryId, true, out empty, out text3, out flag3);
				if (text != null)
				{
					bIgnoreUnresolvedLib = (text == string.Empty);
					text = LibraryPlaceholdersLegacy.MaybeApplyMappingResolution(appObjectGuid, text, placeholder.Resolver);
				}
				libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetLibraryContext(text, false);
			}
			if (libraryContext == null)
			{
				if (cached && text != null && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34400)
				{
					obj = LibraryPlaceholdersLegacy.s_htResolvedPlaceholdersLock;
					lock (obj)
					{
						LibraryPlaceholdersLegacy.s_htResolvedPlaceholders[libPlaceholderIdentification] = text;
					}
				}
				if (cached && !bIgnoreUnresolvedLib && !placeholder.Optional)
				{
					obj = LibraryPlaceholdersLegacy.s_htUnResolvedPlaceholdersLock;
					lock (obj)
					{
						LibraryPlaceholdersLegacy.s_htUnResolvedPlaceholders[placeholder.m_stName] = new LibraryPlaceholdersLegacy.UnresolvedLibraryPlaceholder(text, placeholder.LibManGuid, appObjectGuid);
					}
				}
				return null;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100)
			{
				libraryContext.Namespace = placeholder.Namespace;
			}
			else
			{
				libraryContext.Namespace = empty;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
			{
				if (!libraryContext.LinkAll)
				{
					libraryContext.LinkAll = placeholder.LinkAllContent;
				}
				if (!libraryContext.LinkInSimulation)
				{
					libraryContext.LinkInSimulation = placeholder.LinkInSimulation;
				}
			}
			if (cached)
			{
				obj = LibraryPlaceholdersLegacy.s_htResolvedPlaceholdersLock;
				lock (obj)
				{
					if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3131)
					{
						LibraryPlaceholdersLegacy.s_htResolvedPlaceholders[libPlaceholderIdentification] = libraryContext;
					}
					else
					{
						LibraryPlaceholdersLegacy.s_htResolvedPlaceholders[libPlaceholderIdentification] = libraryContext.LibraryPath;
					}
				}
			}
			return libraryContext;
		}

		// Token: 0x06000AB5 RID: 2741 RVA: 0x0001A548 File Offset: 0x00019548
		internal static _IPreCompileContext ResolveLibraryPlaceholder(ITargetSettings tarset, Guid appObjectGuid, LibraryPlaceholder placeholder)
		{
			object resolverByTypeGuid = LibraryPlaceholdersLegacy.GetResolverByTypeGuid(placeholder.m_guidResolver);
			string empty = string.Empty;
			string text = null;
			ILibraryPlaceholderResolution3 libraryPlaceholderResolution = resolverByTypeGuid as ILibraryPlaceholderResolution3;
			if (libraryPlaceholderResolution != null)
			{
				string text2;
				bool flag;
				text = libraryPlaceholderResolution.ResolvePlaceholder(tarset, appObjectGuid, placeholder.m_stName, placeholder.m_stDefaultLibraryId, true, out empty, out text2, out flag);
			}
			else if (resolverByTypeGuid is ILibraryPlaceholderResolution)
			{
				text = ((ILibraryPlaceholderResolution)resolverByTypeGuid).ResolvePlaceholder(tarset, placeholder.m_stName, placeholder.m_stDefaultLibraryId, out empty);
			}
			else if (resolverByTypeGuid is ILibraryPlaceholderResolutionEx && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
			{
				text = ((ILibraryPlaceholderResolutionEx)resolverByTypeGuid).ResolvePlaceholder(appObjectGuid, placeholder.m_stName, placeholder.m_stDefaultLibraryId, out empty);
			}
			if (!string.IsNullOrEmpty(text))
			{
				text = LibraryPlaceholdersLegacy.MaybeApplyMappingResolution(appObjectGuid, text, placeholder.m_guidResolver);
			}
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			_IPreCompileContext ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetLibraryContext(text, false);
			if (ipreCompileContext == null && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3204 && (tarset == null || tarset is IStubTargetSettings))
			{
				bool flag2 = APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000 && LibraryHelper.IsNewestLibrary(text);
				Version v = new Version(0, 0, 0, 0);
				foreach (_IPreCompileContext ipreCompileContext2 in APEnvironmentFacade.Instance.LanguageModelMgr.Libraries.ToArray<_IPreCompileContext>())
				{
					Version version;
					if (LibraryHelper.IsEqualLibraryNoVersion(text, ipreCompileContext2.LibraryPath, out version))
					{
						if (!flag2)
						{
							ipreCompileContext = ipreCompileContext2;
							break;
						}
						if (version > v)
						{
							v = version;
							ipreCompileContext = ipreCompileContext2;
						}
					}
				}
			}
			if (ipreCompileContext == null)
			{
				return null;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100)
			{
				ipreCompileContext.Namespace = placeholder.Namespace;
			}
			else if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34200 || (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34140 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34200))
			{
				if (tarset != null && string.IsNullOrEmpty(ipreCompileContext.Namespace))
				{
					ipreCompileContext.Namespace = empty;
				}
			}
			else
			{
				ipreCompileContext.Namespace = empty;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
			{
				if (!ipreCompileContext.LinkAll)
				{
					ipreCompileContext.LinkAll = placeholder.LinkAllContent;
				}
				if (!ipreCompileContext.LinkInSimulation)
				{
					ipreCompileContext.LinkInSimulation = placeholder.LinkInSimulation;
				}
			}
			return ipreCompileContext;
		}

		// Token: 0x06000AB6 RID: 2742 RVA: 0x0001A790 File Offset: 0x00019790
		internal static string ResolveLibraryPlaceholderName(ITargetSettings tarset, Guid appObjectGuid, LibraryPlaceholder placeholder, IDeviceIdentification devid)
		{
			if (devid == null)
			{
				if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
				{
					return null;
				}
				if (appObjectGuid != Guid.Empty)
				{
					return null;
				}
			}
			LibraryPlaceholdersLegacy.LibPlaceholderIdentification libPlaceholderIdentification = new LibraryPlaceholdersLegacy.LibPlaceholderIdentification(devid, placeholder);
			object obj = LibraryPlaceholdersLegacy.s_htResolvedPlaceholdersLock;
			lock (obj)
			{
				object obj2;
				if (LibraryPlaceholdersLegacy.s_htResolvedPlaceholders.TryGetValue(libPlaceholderIdentification, ref obj2))
				{
					if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3131)
					{
						return (obj2 as IPreCompileContext).LibraryPath;
					}
					return obj2 as string;
				}
			}
			object resolverByTypeGuid = LibraryPlaceholdersLegacy.GetResolverByTypeGuid(placeholder.m_guidResolver);
			string text = null;
			ILibraryPlaceholderResolution3 libraryPlaceholderResolution = resolverByTypeGuid as ILibraryPlaceholderResolution3;
			if (libraryPlaceholderResolution != null)
			{
				string text2;
				string text3;
				bool flag2;
				text = libraryPlaceholderResolution.ResolvePlaceholder(tarset, appObjectGuid, placeholder.m_stName, placeholder.m_stDefaultLibraryId, true, out text2, out text3, out flag2);
			}
			else
			{
				ILibraryPlaceholderResolution libraryPlaceholderResolution2 = resolverByTypeGuid as ILibraryPlaceholderResolution;
				if (libraryPlaceholderResolution2 != null)
				{
					string text2;
					text = libraryPlaceholderResolution2.ResolvePlaceholder(tarset, placeholder.m_stName, placeholder.m_stDefaultLibraryId, out text2);
				}
				else
				{
					ILibraryPlaceholderResolutionEx3 libraryPlaceholderResolutionEx = resolverByTypeGuid as ILibraryPlaceholderResolutionEx3;
					if (libraryPlaceholderResolutionEx != null && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
					{
						text = libraryPlaceholderResolutionEx.ResolvePlaceholder(appObjectGuid, placeholder.m_stName, placeholder.m_stDefaultLibraryId);
					}
					else
					{
						ILibraryPlaceholderResolutionEx libraryPlaceholderResolutionEx2 = resolverByTypeGuid as ILibraryPlaceholderResolutionEx;
						if (libraryPlaceholderResolutionEx2 != null && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
						{
							string text2;
							text = libraryPlaceholderResolutionEx2.ResolvePlaceholder(appObjectGuid, placeholder.m_stName, placeholder.m_stDefaultLibraryId, out text2);
						}
					}
				}
			}
			if (text != null)
			{
				text = LibraryPlaceholdersLegacy.MaybeApplyMappingResolution(appObjectGuid, text, placeholder.Resolver);
			}
			return text;
		}

		// Token: 0x06000AB7 RID: 2743 RVA: 0x0001A91C File Offset: 0x0001991C
		public static object GetResolverByTypeGuid(Guid guidType)
		{
			object obj = null;
			object obj2 = LibraryPlaceholdersLegacy.s_htResolverInstancesLock;
			lock (obj2)
			{
				if (LibraryPlaceholdersLegacy.s_htResolverInstances.TryGetValue(guidType, ref obj))
				{
					return obj;
				}
			}
			object result;
			try
			{
				obj = APEnvironmentFacade.Instance.TryCreateResolver(guidType);
				if (obj == null)
				{
					result = null;
				}
				else
				{
					obj2 = LibraryPlaceholdersLegacy.s_htResolverInstancesLock;
					lock (obj2)
					{
						LibraryPlaceholdersLegacy.s_htResolverInstances[guidType] = obj;
					}
					result = obj;
				}
			}
			catch
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000AB8 RID: 2744 RVA: 0x0001A9C8 File Offset: 0x000199C8
		private static string MaybeApplyMappingResolution(Guid appObjectGuid, string stLibraryId, Guid resolverGuid)
		{
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
			{
				return stLibraryId;
			}
			if (string.IsNullOrEmpty(stLibraryId))
			{
				return null;
			}
			if (APEnvironmentFacade.Instance.IsLibraryPlaceholderResolutionExGuid(resolverGuid))
			{
				return stLibraryId;
			}
			if (!LibraryPlaceholdersLegacy.s_bMappingResolutionInitialized)
			{
				try
				{
					LibraryPlaceholdersLegacy.s_bMappingResolutionInitialized = true;
					LibraryPlaceholdersLegacy.s_mappingResolution = APEnvironmentFacade.Instance.CreateLibraryPlaceholderResolutionEx();
				}
				catch
				{
				}
			}
			if (LibraryPlaceholdersLegacy.s_mappingResolution == null)
			{
				return stLibraryId;
			}
			if (LibraryPlaceholdersLegacy.s_mappingResolution is ILibraryPlaceholderResolutionEx3)
			{
				return (LibraryPlaceholdersLegacy.s_mappingResolution as ILibraryPlaceholderResolutionEx3).ResolvePlaceholder(appObjectGuid, stLibraryId, stLibraryId);
			}
			string text;
			return LibraryPlaceholdersLegacy.s_mappingResolution.ResolvePlaceholder(appObjectGuid, stLibraryId, stLibraryId, out text);
		}

		// Token: 0x06000AB9 RID: 2745 RVA: 0x0001AA6C File Offset: 0x00019A6C
		internal static void StartCompilation()
		{
			object obj;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3300)
			{
				obj = LibraryPlaceholdersLegacy.s_htResolvedPlaceholdersLock;
				lock (obj)
				{
					LibraryPlaceholdersLegacy.s_htResolvedPlaceholders.Clear();
				}
			}
			obj = LibraryPlaceholdersLegacy.s_htUnResolvedPlaceholdersLock;
			lock (obj)
			{
				LibraryPlaceholdersLegacy.s_htUnResolvedPlaceholders.Clear();
			}
		}

		// Token: 0x0400019F RID: 415
		private static readonly object s_htResolvedPlaceholdersLock = new object();

		// Token: 0x040001A0 RID: 416
		internal static readonly object s_htUnResolvedPlaceholdersLock = new object();

		// Token: 0x040001A1 RID: 417
		private static readonly object s_htResolverInstancesLock = new object();

		// Token: 0x040001A2 RID: 418
		[Obfuscation(Feature = "rename")]
		public static LDictionary<Guid, object> s_htResolverInstances = new LDictionary<Guid, object>();

		// Token: 0x040001A3 RID: 419
		internal static readonly Guid GUID_VISU_RESOLVER_GUID = new Guid("{2717EB6A-DD07-4c66-8D8D-CACEBD7B18AE}");

		// Token: 0x040001A4 RID: 420
		private static bool s_bMappingResolutionInitialized = false;

		// Token: 0x040001A5 RID: 421
		private static ILibraryPlaceholderResolutionEx s_mappingResolution = null;

		// Token: 0x040001A6 RID: 422
		private static readonly LDictionary<LibraryPlaceholdersLegacy.LibPlaceholderIdentification, object> s_htResolvedPlaceholders = new LDictionary<LibraryPlaceholdersLegacy.LibPlaceholderIdentification, object>();

		// Token: 0x040001A7 RID: 423
		internal static readonly LDictionary<string, LibraryPlaceholdersLegacy.UnresolvedLibraryPlaceholder> s_htUnResolvedPlaceholders = new LDictionary<string, LibraryPlaceholdersLegacy.UnresolvedLibraryPlaceholder>();

		// Token: 0x02000294 RID: 660
		internal class UnresolvedLibraryPlaceholder : IUnresolvedPlaceholder
		{
			// Token: 0x06002B34 RID: 11060 RVA: 0x00072B8D File Offset: 0x00071B8D
			internal UnresolvedLibraryPlaceholder(string stLibraryId, Guid guidLibMan, Guid guidApp)
			{
				this.LibraryId = stLibraryId;
				this.LibManGuid = guidLibMan;
				this.ApplicationGuid = guidApp;
			}

			// Token: 0x17000BFD RID: 3069
			// (get) Token: 0x06002B35 RID: 11061 RVA: 0x00072BAA File Offset: 0x00071BAA
			public string LibraryId { get; }

			// Token: 0x17000BFE RID: 3070
			// (get) Token: 0x06002B36 RID: 11062 RVA: 0x00072BB2 File Offset: 0x00071BB2
			public Guid LibManGuid { get; }

			// Token: 0x17000BFF RID: 3071
			// (get) Token: 0x06002B37 RID: 11063 RVA: 0x00072BBA File Offset: 0x00071BBA
			public Guid ApplicationGuid { get; }
		}

		// Token: 0x02000295 RID: 661
		public class LibPlaceholderIdentification : ILibPlaceholderIdentification
		{
			// Token: 0x06002B38 RID: 11064 RVA: 0x00072BC2 File Offset: 0x00071BC2
			public LibPlaceholderIdentification(IDeviceIdentification devid, ILibraryPlaceholder2 libplaceholder)
			{
				this._devid = devid;
				this._libplaceholder = (libplaceholder as LibraryPlaceholder);
			}

			// Token: 0x06002B39 RID: 11065 RVA: 0x00072BE0 File Offset: 0x00071BE0
			public override int GetHashCode()
			{
				int num = 0;
				if (this._devid != null)
				{
					if (this._devid.Id != null)
					{
						num ^= this._devid.Id.GetHashCode();
					}
					num ^= this._devid.Type.GetHashCode();
					if (this._devid.Version != null)
					{
						num ^= this._devid.Version.GetHashCode();
					}
				}
				if (this._libplaceholder.m_stName != null)
				{
					num ^= this._libplaceholder.m_stName.GetHashCode();
				}
				if (this._libplaceholder.m_stNamespace != null)
				{
					num ^= this._libplaceholder.m_stNamespace.GetHashCode();
				}
				if (this._libplaceholder.m_stDefaultLibraryId != null)
				{
					num ^= this._libplaceholder.m_stDefaultLibraryId.GetHashCode();
				}
				num ^= this._libplaceholder.m_guidResolver.GetHashCode();
				num ^= this._libplaceholder.m_bPublishSymbols.GetHashCode();
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
				{
					num ^= this._libplaceholder.m_bQualifiedOnly.GetHashCode();
				}
				num ^= this._libplaceholder.m_bLinkAllContent.GetHashCode();
				num ^= this._libplaceholder.m_bLinkInSimulation.GetHashCode();
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351040 || (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35980 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351000))
				{
					num ^= this._libplaceholder.LibManGuid.GetHashCode();
				}
				return num;
			}

			// Token: 0x06002B3A RID: 11066 RVA: 0x00072D74 File Offset: 0x00071D74
			public override bool Equals(object obj)
			{
				if (!(obj is LibraryPlaceholdersLegacy.LibPlaceholderIdentification))
				{
					return false;
				}
				LibraryPlaceholdersLegacy.LibPlaceholderIdentification libPlaceholderIdentification = obj as LibraryPlaceholdersLegacy.LibPlaceholderIdentification;
				if (libPlaceholderIdentification._devid == null && this._devid == null)
				{
					return libPlaceholderIdentification._libplaceholder.m_stName == this._libplaceholder.m_stName && libPlaceholderIdentification._libplaceholder.m_stNamespace == this._libplaceholder.m_stNamespace && libPlaceholderIdentification._libplaceholder.m_stDefaultLibraryId == this._libplaceholder.m_stDefaultLibraryId && libPlaceholderIdentification._libplaceholder.m_guidResolver == this._libplaceholder.m_guidResolver && libPlaceholderIdentification._libplaceholder.m_bPublishSymbols == this._libplaceholder.m_bPublishSymbols && (libPlaceholderIdentification._libplaceholder.m_bQualifiedOnly == this._libplaceholder.m_bQualifiedOnly || !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300) && libPlaceholderIdentification._libplaceholder.m_bLinkAllContent == this._libplaceholder.m_bLinkAllContent && libPlaceholderIdentification._libplaceholder.m_bLinkInSimulation == this._libplaceholder.m_bLinkInSimulation && (libPlaceholderIdentification._libplaceholder.LibManGuid == this._libplaceholder.LibManGuid || (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351040 && (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35980 || APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351000)));
				}
				return libPlaceholderIdentification._devid != null && this._devid != null && (libPlaceholderIdentification._devid.Id == this._devid.Id && libPlaceholderIdentification._devid.Type == this._devid.Type && libPlaceholderIdentification._devid.Version == this._devid.Version && libPlaceholderIdentification._libplaceholder.m_stName == this._libplaceholder.m_stName && libPlaceholderIdentification._libplaceholder.m_stNamespace == this._libplaceholder.m_stNamespace && libPlaceholderIdentification._libplaceholder.m_stDefaultLibraryId == this._libplaceholder.m_stDefaultLibraryId && libPlaceholderIdentification._libplaceholder.m_guidResolver == this._libplaceholder.m_guidResolver && libPlaceholderIdentification._libplaceholder.m_bPublishSymbols == this._libplaceholder.m_bPublishSymbols && (libPlaceholderIdentification._libplaceholder.m_bQualifiedOnly == this._libplaceholder.m_bQualifiedOnly || !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300) && libPlaceholderIdentification._libplaceholder.m_bLinkAllContent == this._libplaceholder.m_bLinkAllContent && libPlaceholderIdentification._libplaceholder.m_bLinkInSimulation == this._libplaceholder.m_bLinkInSimulation) && (libPlaceholderIdentification._libplaceholder.LibManGuid == this._libplaceholder.LibManGuid || (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351040 && (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35980 || APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351000)));
			}

			// Token: 0x04000860 RID: 2144
			private readonly IDeviceIdentification _devid;

			// Token: 0x04000861 RID: 2145
			private readonly LibraryPlaceholder _libplaceholder;
		}
	}
}
