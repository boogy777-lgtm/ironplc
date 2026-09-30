using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using SmartAssembly.Attributes;
using _3S.CoDeSys.Core.Components;

// Token: 0x02000004 RID: 4
internal static class CodeAccessSecurity
{
	// Token: 0x06000003 RID: 3 RVA: 0x00002058 File Offset: 0x00001058
	[ObfuscateControlFlow]
	internal static bool CheckKeyFlagForType(Guid typeGuid, string stKeyFlag)
	{
		OwningPlugInComponent owningPlugInComponent = ComponentManager.Singleton.PlugInCache.GetTypeInformation(typeGuid, null).OwningComponent as OwningPlugInComponent;
		return owningPlugInComponent != null && CodeAccessSecurity.CheckKeyFlagForPlugIn(owningPlugInComponent.PlugInName, stKeyFlag);
	}

	// Token: 0x06000004 RID: 4 RVA: 0x00002094 File Offset: 0x00001094
	[ObfuscateControlFlow]
	internal static bool CheckKeyFlagForType(Guid typeGuid, Version version, string stKeyFlag)
	{
		TypeInformation typeInformation = ComponentManager.Singleton.PlugInCache.GetTypeInformation(typeGuid, new ExactVersionConstraint(version));
		if (typeInformation == null)
		{
			throw new TypeNotFoundException(typeGuid);
		}
		return typeInformation.OwningComponent is OwningPlugInComponent && CodeAccessSecurity.CheckKeyFlagForPlugIn(new PlugInName((typeInformation.OwningComponent as OwningPlugInComponent).PlugInName.Guid, ((OwningPlugInComponent)typeInformation.OwningComponent).PlugInName.Version), stKeyFlag);
	}

	// Token: 0x06000005 RID: 5 RVA: 0x00002108 File Offset: 0x00001108
	[ObfuscateControlFlow]
	internal static bool CheckKeyFlagForPlugIn(PlugInName plugInName, string stKeyFlag)
	{
		PlugInInformation plugInInformation = ComponentManager.Singleton.PlugInCache.GetPlugInInformation(plugInName);
		if (plugInInformation != null)
		{
			using (IEnumerator<string> enumerator = plugInInformation.PlugInKeyFlagsFast.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (string.Equals(enumerator.Current, stKeyFlag, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}
			}
			return false;
		}
		return false;
	}

	// Token: 0x06000006 RID: 6 RVA: 0x00002174 File Offset: 0x00001174
	[ObfuscateControlFlow]
	internal static void AssertCallerHasKeyFlag(string stKeyFlag, int iStartingLevel = 2)
	{
		StackFrame[] frames = new StackTrace(false).GetFrames();
		if (frames == null || frames.Length < iStartingLevel)
		{
			Environment.Exit(0);
		}
		Assembly assembly = frames[iStartingLevel - 1].GetMethod().DeclaringType.Assembly;
		Assembly assembly2 = null;
		for (int i = iStartingLevel; i < frames.Length; i++)
		{
			if (!(null == frames[i].GetMethod().DeclaringType))
			{
				assembly2 = frames[i].GetMethod().DeclaringType.Assembly;
				if (!string.Equals(assembly2.GetName().Name, "mscorlib", StringComparison.OrdinalIgnoreCase))
				{
					break;
				}
				assembly2 = null;
			}
		}
		if (assembly2 == null)
		{
			Environment.Exit(0);
		}
		if (assembly == assembly2)
		{
			return;
		}
		if (CodeAccessSecurity.TestLanguageModelBrowserPlugin(assembly2))
		{
			Environment.Exit(0);
		}
		foreach (string b in CodeAccessSecurity.GetPlugInKeyFlagsForAssembly(assembly2))
		{
			if (string.Equals(stKeyFlag, b, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
		}
		Environment.Exit(0);
	}

	// Token: 0x06000007 RID: 7 RVA: 0x0000227C File Offset: 0x0000127C
	[ObfuscateControlFlow]
	private static bool TestLanguageModelBrowserPlugin(Assembly assembly)
	{
		object[] customAttributes = assembly.GetCustomAttributes(typeof(PlugInKeyAttribute), false);
		return customAttributes != null && customAttributes.Length != 0 && customAttributes[0] is PlugInKeyAttribute && (customAttributes[0] as PlugInKeyAttribute).Key == "nRRVIy+0HoJkNeHIoS/i6CICP8Dn9SyDvVh5HnKZTxRXAUOZLaCgWPSAU0yFvbFg";
	}

	// Token: 0x06000008 RID: 8 RVA: 0x000022C8 File Offset: 0x000012C8
	[ObfuscateControlFlow]
	private static IEnumerable<string> GetPlugInKeyFlagsForAssembly(Assembly assembly)
	{
		Debug.Assert(assembly != null);
		object[] customAttributes = assembly.GetCustomAttributes(typeof(PlugInGuidAttribute), false);
		if (customAttributes == null || customAttributes.Length == 0)
		{
			return CodeAccessSecurity.EMPTY_STRING_ARRAY;
		}
		Guid guid = ((PlugInGuidAttribute)customAttributes[0]).Guid;
		Version version = assembly.GetName(false).Version;
		PlugInName plugInName = new PlugInName(guid, version);
		return ComponentManager.Singleton.PlugInCache.GetPlugInInformation(plugInName).PlugInKeyFlagsFast;
	}

	// Token: 0x04000001 RID: 1
	private static readonly string[] EMPTY_STRING_ARRAY = new string[0];
}
