using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000123 RID: 291
	[TypeGuid("{2b2904f2-a760-4bc9-9411-77f8d40eac75}")]
	[StorageVersion("3.3.0.0")]
	public class ApplicationDeviceTable : GenericObject2, _IApplicationDeviceTable2, _IApplicationDeviceTable
	{
		// Token: 0x1700060D RID: 1549
		// (get) Token: 0x060018DE RID: 6366 RVA: 0x00047B29 File Offset: 0x00046B29
		public IDictionary<Guid, Guid> DeviceOfApplication
		{
			get
			{
				return new HashtableReadOnlyDictionaryWrapper<Guid, Guid>(this.m_htDeviceOfApplication);
			}
		}

		// Token: 0x1700060E RID: 1550
		// (get) Token: 0x060018DF RID: 6367 RVA: 0x00047B36 File Offset: 0x00046B36
		public IDictionary<Guid, ICollection> ApplicationsOfDevice
		{
			get
			{
				return new HashtableReadOnlyDictionaryWrapper<Guid, ICollection>(this.m_htApplicationsOfDevice);
			}
		}

		// Token: 0x1700060F RID: 1551
		// (get) Token: 0x060018E0 RID: 6368 RVA: 0x00047B43 File Offset: 0x00046B43
		public IDictionary<Guid, string> ApplicationNameTable
		{
			get
			{
				return new HashtableReadOnlyDictionaryWrapper<Guid, string>(this.m_htApplicationNameTable);
			}
		}

		// Token: 0x17000610 RID: 1552
		// (get) Token: 0x060018E1 RID: 6369 RVA: 0x00047B50 File Offset: 0x00046B50
		public IDictionary<Guid, string> SimulationApplicationNameTable
		{
			get
			{
				return new HashtableReadOnlyDictionaryWrapper<Guid, string>(this.m_htSimulationApplicationNameTable);
			}
		}

		// Token: 0x17000611 RID: 1553
		// (get) Token: 0x060018E2 RID: 6370 RVA: 0x00047B5D File Offset: 0x00046B5D
		public IDictionary<Guid, string> DeviceNameTable
		{
			get
			{
				return new HashtableReadOnlyDictionaryWrapper<Guid, string>(this.m_htDeviceNameTable);
			}
		}

		// Token: 0x17000612 RID: 1554
		// (get) Token: 0x060018E3 RID: 6371 RVA: 0x00047B6A File Offset: 0x00046B6A
		public IDictionary<Guid, IDeviceIdentification> TargetIdOfDevice
		{
			get
			{
				return new HashtableReadOnlyDictionaryWrapper<Guid, IDeviceIdentification>(this.m_htTargetIdOfDevice);
			}
		}

		// Token: 0x17000613 RID: 1555
		// (get) Token: 0x060018E4 RID: 6372 RVA: 0x00047B77 File Offset: 0x00046B77
		public IDictionary<Guid, ICollection> ClonesOfApplication
		{
			get
			{
				return new HashtableReadOnlyDictionaryWrapper<Guid, ICollection>(this.m_htClonesOfApplication);
			}
		}

		// Token: 0x17000614 RID: 1556
		// (get) Token: 0x060018E5 RID: 6373 RVA: 0x00047B84 File Offset: 0x00046B84
		public IDictionary<Guid, Guid> SubApplicationTable
		{
			get
			{
				return new HashtableReadOnlyDictionaryWrapper<Guid, Guid>(this.m_htSubApplicationTable);
			}
		}

		// Token: 0x17000615 RID: 1557
		// (get) Token: 0x060018E6 RID: 6374 RVA: 0x00047B91 File Offset: 0x00046B91
		public IDictionary<Guid, Guid> MemorySettingsProviderTable
		{
			get
			{
				return new HashtableReadOnlyDictionaryWrapper<Guid, Guid>(this.m_htMemorySettingsProviderTable);
			}
		}

		// Token: 0x060018E8 RID: 6376 RVA: 0x00047C18 File Offset: 0x00046C18
		public void Clear()
		{
			this.m_htDeviceOfApplication.Clear();
			this.m_htApplicationsOfDevice.Clear();
			this.m_htApplicationNameTable.Clear();
			this.m_htSimulationApplicationNameTable.Clear();
			this.m_htDeviceNameTable.Clear();
			this.m_htTargetIdOfDevice.Clear();
			this.m_htClonesOfApplication.Clear();
			this.m_htSubApplicationTable.Clear();
		}

		// Token: 0x060018E9 RID: 6377 RVA: 0x00047C7D File Offset: 0x00046C7D
		public void SetDeviceName(Guid guidDevice, string stName)
		{
			this.m_htDeviceNameTable[guidDevice] = stName;
		}

		// Token: 0x060018EA RID: 6378 RVA: 0x00047C91 File Offset: 0x00046C91
		public string GetDeviceName(Guid guidDevice)
		{
			return this.m_htDeviceNameTable[guidDevice] as string;
		}

		// Token: 0x060018EB RID: 6379 RVA: 0x00047CA9 File Offset: 0x00046CA9
		public void SetApplicationName(Guid guidApplication, string stName, bool bSimulation)
		{
			if (bSimulation)
			{
				this.m_htSimulationApplicationNameTable[guidApplication] = stName;
				return;
			}
			this.m_htApplicationNameTable[guidApplication] = stName;
		}

		// Token: 0x060018EC RID: 6380 RVA: 0x00047CD3 File Offset: 0x00046CD3
		public string GetApplicationName(Guid guidApplication, bool bSimulation)
		{
			if (bSimulation)
			{
				return this.m_htSimulationApplicationNameTable[guidApplication] as string;
			}
			return this.m_htApplicationNameTable[guidApplication] as string;
		}

		// Token: 0x060018ED RID: 6381 RVA: 0x00047D08 File Offset: 0x00046D08
		public Guid GetDeviceGuidByName(string stName)
		{
			Guid[] array = new Guid[this.m_htDeviceNameTable.Keys.Count];
			this.m_htDeviceNameTable.Keys.CopyTo(array, 0);
			foreach (Guid guid in array)
			{
				if (this.GetDeviceName(guid) == stName)
				{
					return guid;
				}
			}
			return Guid.Empty;
		}

		// Token: 0x060018EE RID: 6382 RVA: 0x00047D6C File Offset: 0x00046D6C
		public Guid GetApplicationGuidByName(string stName)
		{
			Guid[] array = new Guid[this.m_htApplicationNameTable.Keys.Count];
			this.m_htApplicationNameTable.Keys.CopyTo(array, 0);
			foreach (Guid guid in array)
			{
				if (this.GetApplicationNameByGuid(guid, false) == stName)
				{
					return guid;
				}
				if (this.GetApplicationNameByGuid(guid, true) == stName)
				{
					return guid;
				}
			}
			return Guid.Empty;
		}

		// Token: 0x060018EF RID: 6383 RVA: 0x00047DE2 File Offset: 0x00046DE2
		public Guid GetParentApplication(Guid guidSubApplication)
		{
			if (this.m_htSubApplicationTable.ContainsKey(guidSubApplication))
			{
				return (Guid)this.m_htSubApplicationTable[guidSubApplication];
			}
			return Guid.Empty;
		}

		// Token: 0x060018F0 RID: 6384 RVA: 0x00047E13 File Offset: 0x00046E13
		public void SetParentApplication(Guid guidParentApplication, Guid guidSubApplication)
		{
			this.m_htSubApplicationTable[guidSubApplication] = guidParentApplication;
		}

		// Token: 0x060018F1 RID: 6385 RVA: 0x00047E2C File Offset: 0x00046E2C
		public IEnumerable<Guid> GetChildApplications(Guid guidParentApplication, bool bRecursive)
		{
			LList<Guid> llist = new LList<Guid>();
			foreach (object obj in this.m_htSubApplicationTable.Keys)
			{
				Guid guid = (Guid)obj;
				if (this.GetParentApplication(guid) == guidParentApplication && bRecursive)
				{
					llist.AddRange(this.GetChildApplications(guid, true));
					llist.Add(guid);
				}
			}
			return llist;
		}

		// Token: 0x060018F2 RID: 6386 RVA: 0x00047EB4 File Offset: 0x00046EB4
		public string GetApplicationNameByGuid(Guid guidApplication, bool bSimulation)
		{
			Guid deviceOfApplication = this.GetDeviceOfApplication(guidApplication);
			string deviceName = this.GetDeviceName(deviceOfApplication);
			string applicationName = this.GetApplicationName(guidApplication, bSimulation);
			if (deviceName == null || applicationName == null)
			{
				return null;
			}
			return deviceName + "." + applicationName;
		}

		// Token: 0x060018F3 RID: 6387 RVA: 0x00047EF0 File Offset: 0x00046EF0
		public void AddApplicationDevice(Guid guidApplication, Guid guidDevice)
		{
			this.m_htDeviceOfApplication.Add(guidApplication, guidDevice);
			ArrayList arrayList = this.m_htApplicationsOfDevice[guidDevice] as ArrayList;
			if (arrayList == null)
			{
				arrayList = new ArrayList();
				this.m_htApplicationsOfDevice[guidDevice] = arrayList;
			}
			arrayList.Add(guidApplication);
		}

		// Token: 0x060018F4 RID: 6388 RVA: 0x00047F54 File Offset: 0x00046F54
		public Guid[] GetApplicationsOfDevice(Guid guidDevice)
		{
			ArrayList arrayList = this.m_htApplicationsOfDevice[guidDevice] as ArrayList;
			if (arrayList == null)
			{
				return Array.Empty<Guid>();
			}
			Guid[] array = new Guid[arrayList.Count];
			arrayList.CopyTo(array);
			return array;
		}

		// Token: 0x060018F5 RID: 6389 RVA: 0x00047F95 File Offset: 0x00046F95
		public Guid GetDeviceOfApplication(Guid guidApplication)
		{
			if (this.m_htDeviceOfApplication[guidApplication] == null)
			{
				return Guid.Empty;
			}
			return (Guid)this.m_htDeviceOfApplication[guidApplication];
		}

		// Token: 0x060018F6 RID: 6390 RVA: 0x00047FC6 File Offset: 0x00046FC6
		public IDeviceIdentification GetTargetIdOfDevice(Guid guidDevice)
		{
			return this.m_htTargetIdOfDevice[guidDevice] as IDeviceIdentification;
		}

		// Token: 0x060018F7 RID: 6391 RVA: 0x00047FDE File Offset: 0x00046FDE
		public void SetTargetIdOfDevice(Guid guidDevice, IDeviceIdentification devId)
		{
			this.m_htTargetIdOfDevice[guidDevice] = devId;
		}

		// Token: 0x060018F8 RID: 6392 RVA: 0x00047FF4 File Offset: 0x00046FF4
		public void AddCloneOfApplication(Guid guidApplication, Guid guidClone)
		{
			ArrayList arrayList = this.m_htClonesOfApplication[guidApplication] as ArrayList;
			if (arrayList == null)
			{
				arrayList = new ArrayList();
			}
			arrayList.Add(guidClone);
			this.m_htClonesOfApplication[guidApplication] = arrayList;
		}

		// Token: 0x060018F9 RID: 6393 RVA: 0x00048040 File Offset: 0x00047040
		public Guid[] GetClonesOfApplication(Guid guidApplication)
		{
			ArrayList arrayList = this.m_htClonesOfApplication[guidApplication] as ArrayList;
			if (arrayList == null)
			{
				return Array.Empty<Guid>();
			}
			Guid[] array = new Guid[arrayList.Count];
			arrayList.CopyTo(array);
			return array;
		}

		// Token: 0x060018FA RID: 6394 RVA: 0x00048084 File Offset: 0x00047084
		public Guid GetApplicationOfClone(Guid guidClone)
		{
			Guid[] array = new Guid[this.m_htClonesOfApplication.Keys.Count];
			this.m_htClonesOfApplication.Keys.CopyTo(array, 0);
			foreach (Guid guid in array)
			{
				Guid[] clonesOfApplication = this.GetClonesOfApplication(guid);
				for (int j = 0; j < clonesOfApplication.Length; j++)
				{
					if (clonesOfApplication[j] == guidClone)
					{
						return guid;
					}
				}
			}
			return Guid.Empty;
		}

		// Token: 0x060018FB RID: 6395 RVA: 0x00048108 File Offset: 0x00047108
		public Guid GetOriginalApplication(Guid guidApplication)
		{
			Guid applicationOfClone = this.GetApplicationOfClone(guidApplication);
			if (applicationOfClone == Guid.Empty)
			{
				return guidApplication;
			}
			return applicationOfClone;
		}

		// Token: 0x060018FC RID: 6396 RVA: 0x00048130 File Offset: 0x00047130
		public void RemoveByGuid(Guid guid)
		{
			this.m_htApplicationsOfDevice.Remove(guid);
			Guid[] array = new Guid[this.m_htApplicationsOfDevice.Keys.Count];
			this.m_htApplicationsOfDevice.Keys.CopyTo(array, 0);
			foreach (Guid guid2 in array)
			{
				ArrayList arrayList = this.m_htApplicationsOfDevice[guid2] as ArrayList;
				for (int j = arrayList.Count - 1; j >= 0; j--)
				{
					if ((Guid)arrayList[j] == guid)
					{
						arrayList.RemoveAt(j);
					}
				}
				if (arrayList.Count == 0)
				{
					this.m_htApplicationsOfDevice.Remove(guid2);
				}
			}
			this.m_htDeviceOfApplication.Remove(guid);
			array = new Guid[this.m_htDeviceOfApplication.Keys.Count];
			this.m_htDeviceOfApplication.Keys.CopyTo(array, 0);
			foreach (Guid guid3 in array)
			{
				if ((Guid)this.m_htDeviceOfApplication[guid3] == guid)
				{
					this.m_htApplicationsOfDevice.Remove(guid3);
				}
			}
			this.m_htTargetIdOfDevice.Remove(guid);
			this.m_htClonesOfApplication.Remove(guid);
			array = new Guid[this.m_htClonesOfApplication.Keys.Count];
			this.m_htClonesOfApplication.Keys.CopyTo(array, 0);
			foreach (Guid guid4 in array)
			{
				ArrayList arrayList2 = this.m_htClonesOfApplication[guid4] as ArrayList;
				using (IEnumerator enumerator = arrayList2.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if ((Guid)enumerator.Current == guid)
						{
							arrayList2.Remove(guid);
						}
					}
				}
				if (arrayList2.Count == 0)
				{
					this.m_htClonesOfApplication.Remove(guid4);
				}
			}
			this.m_htSubApplicationTable.Remove(guid);
			array = new Guid[this.m_htSubApplicationTable.Keys.Count];
			this.m_htSubApplicationTable.Keys.CopyTo(array, 0);
			foreach (Guid guid5 in array)
			{
				if ((Guid)this.m_htSubApplicationTable[guid5] == guid)
				{
					this.m_htSubApplicationTable.Remove(guid);
				}
			}
		}

		// Token: 0x060018FD RID: 6397 RVA: 0x000483F8 File Offset: 0x000473F8
		public Guid GetMemorySettingsProvider(Guid guidApplication)
		{
			if (this.m_htMemorySettingsProviderTable.ContainsKey(guidApplication))
			{
				return (Guid)this.m_htMemorySettingsProviderTable[guidApplication];
			}
			return Guid.Empty;
		}

		// Token: 0x060018FE RID: 6398 RVA: 0x00048429 File Offset: 0x00047429
		public void SetMemorySettingsProvider(Guid guidApplication, Guid guidMemorySettingsProvider)
		{
			if (guidMemorySettingsProvider != Guid.Empty)
			{
				this.m_htMemorySettingsProviderTable[guidApplication] = guidMemorySettingsProvider;
			}
		}

		// Token: 0x04000520 RID: 1312
		[DefaultSerialization("DeviceOfApplication")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Hashtable m_htDeviceOfApplication = new Hashtable();

		// Token: 0x04000521 RID: 1313
		[DefaultSerialization("ApplicationsOfDevice")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Hashtable m_htApplicationsOfDevice = new Hashtable();

		// Token: 0x04000522 RID: 1314
		[DefaultSerialization("ApplicationNameTable")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveHashtable m_htApplicationNameTable = new CaseInsensitiveHashtable();

		// Token: 0x04000523 RID: 1315
		[DefaultSerialization("SimulationApplicationNameTable")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveHashtable m_htSimulationApplicationNameTable = new CaseInsensitiveHashtable();

		// Token: 0x04000524 RID: 1316
		[DefaultSerialization("DeviceNameTable")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveHashtable m_htDeviceNameTable = new CaseInsensitiveHashtable();

		// Token: 0x04000525 RID: 1317
		[DefaultSerialization("TargetIdTable")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Hashtable m_htTargetIdOfDevice = new Hashtable();

		// Token: 0x04000526 RID: 1318
		[DefaultSerialization("ApplicationCloneTable")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Hashtable m_htClonesOfApplication = new Hashtable();

		// Token: 0x04000527 RID: 1319
		[DefaultSerialization("SubApplicationTable")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Hashtable m_htSubApplicationTable = new Hashtable();

		// Token: 0x04000528 RID: 1320
		private readonly Hashtable m_htMemorySettingsProviderTable = new Hashtable();
	}
}
