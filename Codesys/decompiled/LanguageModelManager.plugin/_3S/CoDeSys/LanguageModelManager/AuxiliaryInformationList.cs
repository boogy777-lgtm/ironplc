using System;
using System.Collections;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200014F RID: 335
	[TypeGuid("{3966d598-3646-49d7-a64c-1b0623a3daed}")]
	[StorageVersion("3.3.0.0")]
	public class AuxiliaryInformationList : GenericObject2, IAuxiliaryCompileInformationList
	{
		// Token: 0x17000768 RID: 1896
		// (get) Token: 0x06001B7F RID: 7039 RVA: 0x0004DE7D File Offset: 0x0004CE7D
		// (set) Token: 0x06001B80 RID: 7040 RVA: 0x0004DE88 File Offset: 0x0004CE88
		[DefaultSerialization("List")]
		[StorageVersion("3.3.0.0")]
		private ArrayList ToSerialize
		{
			get
			{
				return this._alAll;
			}
			set
			{
				foreach (object obj in value)
				{
					AuxiliaryInformation aux = (AuxiliaryInformation)obj;
					this.Add(aux);
				}
			}
		}

		// Token: 0x06001B82 RID: 7042 RVA: 0x0004DF10 File Offset: 0x0004CF10
		public void Add(AuxiliaryInformation aux)
		{
			ArrayList arrayList = this._htByObjectGuid[aux.ObjectGuid] as ArrayList;
			if (arrayList == null)
			{
				arrayList = new ArrayList();
				this._htByObjectGuid[aux.ObjectGuid] = arrayList;
			}
			arrayList.Add(aux);
			this._htById[aux.Id] = aux;
			arrayList = (this._htByType[aux.Type] as ArrayList);
			if (arrayList == null)
			{
				arrayList = new ArrayList();
				this._htByType[aux.Type] = arrayList;
			}
			arrayList.Add(aux);
			this._alAll.Add(aux);
		}

		// Token: 0x06001B83 RID: 7043 RVA: 0x0004DFC0 File Offset: 0x0004CFC0
		public void Remove(Guid guidObject)
		{
			ArrayList arrayList = this._htByObjectGuid[guidObject] as ArrayList;
			if (arrayList == null)
			{
				return;
			}
			Hashtable hashtable = new Hashtable();
			foreach (object obj in arrayList)
			{
				AuxiliaryInformation auxiliaryInformation = (AuxiliaryInformation)obj;
				this._alAll.Remove(auxiliaryInformation);
				this._htById.Remove(auxiliaryInformation.Id);
				if (!hashtable.ContainsKey(auxiliaryInformation.Type))
				{
					ArrayList arrayList2 = this._htByType[auxiliaryInformation.Type] as ArrayList;
					for (int i = arrayList2.Count - 1; i >= 0; i--)
					{
						if ((arrayList2[i] as AuxiliaryInformation).ObjectGuid == guidObject)
						{
							arrayList2.RemoveAt(i);
						}
					}
					if (arrayList2.Count == 0)
					{
						this._htByType.Remove(auxiliaryInformation.Type);
					}
					hashtable[auxiliaryInformation.Type] = auxiliaryInformation.Type;
				}
			}
			this._htByObjectGuid.Remove(guidObject);
		}

		// Token: 0x06001B84 RID: 7044 RVA: 0x0004E0FC File Offset: 0x0004D0FC
		public AuxiliaryInformationList Duplicate()
		{
			AuxiliaryInformationList auxiliaryInformationList = new AuxiliaryInformationList();
			foreach (object obj in this._alAll)
			{
				AuxiliaryInformation aux = (AuxiliaryInformation)obj;
				auxiliaryInformationList.Add(aux);
			}
			return auxiliaryInformationList;
		}

		// Token: 0x17000769 RID: 1897
		public IAuxiliaryCompileInformation this[Guid id]
		{
			get
			{
				return this._htById[id] as IAuxiliaryCompileInformation;
			}
		}

		// Token: 0x06001B86 RID: 7046 RVA: 0x0004E174 File Offset: 0x0004D174
		public IAuxiliaryCompileInformation[] GetAllOfType(string stType)
		{
			ArrayList arrayList = this._htByType[stType] as ArrayList;
			if (arrayList == null)
			{
				return Array.Empty<IAuxiliaryCompileInformation>();
			}
			IAuxiliaryCompileInformation[] array = new IAuxiliaryCompileInformation[arrayList.Count];
			arrayList.CopyTo(array);
			return array;
		}

		// Token: 0x06001B87 RID: 7047 RVA: 0x0004E1B0 File Offset: 0x0004D1B0
		public IAuxiliaryCompileInformation[] GetAll()
		{
			IAuxiliaryCompileInformation[] array = new IAuxiliaryCompileInformation[this._alAll.Count];
			this._alAll.CopyTo(array);
			return array;
		}

		// Token: 0x040005CA RID: 1482
		[Obfuscation(Feature = "rename")]
		private Hashtable _htByObjectGuid = new Hashtable();

		// Token: 0x040005CB RID: 1483
		[Obfuscation(Feature = "rename")]
		private Hashtable _htById = new Hashtable();

		// Token: 0x040005CC RID: 1484
		[Obfuscation(Feature = "rename")]
		private Hashtable _htByType = new Hashtable();

		// Token: 0x040005CD RID: 1485
		[Obfuscation(Feature = "rename")]
		private ArrayList _alAll = new ArrayList();
	}
}
