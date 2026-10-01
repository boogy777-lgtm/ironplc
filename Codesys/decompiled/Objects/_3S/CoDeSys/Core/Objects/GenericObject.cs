using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000D9 RID: 217
	[ReleasedClass]
	public abstract class GenericObject : IGenericObject, IArchivable, ICloneable, IComparable
	{
		// Token: 0x06000353 RID: 851 RVA: 0x0000576C File Offset: 0x0000396C
		public virtual object Clone()
		{
			return this.GenericObjectService.Clone(this);
		}

		// Token: 0x17000144 RID: 324
		// (get) Token: 0x06000354 RID: 852 RVA: 0x0000577A File Offset: 0x0000397A
		public virtual string[] SerializableValueNames
		{
			get
			{
				return this.GenericObjectService.GetSerializableValueNames(this);
			}
		}

		// Token: 0x06000355 RID: 853 RVA: 0x00005788 File Offset: 0x00003988
		public virtual object GetSerializableValue(string stValueName)
		{
			return this.GenericObjectService.GetSerializableValue(this, stValueName);
		}

		// Token: 0x06000356 RID: 854 RVA: 0x00005797 File Offset: 0x00003997
		public virtual void SetSerializableValue(string stValueName, object value)
		{
			this.GenericObjectService.SetSerializableValue(this, stValueName, value);
		}

		// Token: 0x06000357 RID: 855 RVA: 0x000057A7 File Offset: 0x000039A7
		public virtual void BeforeSerialize()
		{
		}

		// Token: 0x06000358 RID: 856 RVA: 0x000057A9 File Offset: 0x000039A9
		public virtual void AfterDeserialize()
		{
		}

		// Token: 0x06000359 RID: 857 RVA: 0x000057AB File Offset: 0x000039AB
		public virtual void AfterClone()
		{
		}

		// Token: 0x0600035A RID: 858 RVA: 0x000057AD File Offset: 0x000039AD
		public virtual int CompareTo(object obj)
		{
			return 0;
		}

		// Token: 0x17000145 RID: 325
		// (get) Token: 0x0600035B RID: 859 RVA: 0x000057B0 File Offset: 0x000039B0
		protected IGenericObjectService GenericObjectService
		{
			get
			{
				Type type = base.GetType();
				object obj = GenericObject.s_lockServices;
				IGenericObjectService genericObjectService;
				lock (obj)
				{
					GenericObject.s_genericObjectServices.TryGetValue(type, out genericObjectService);
				}
				if (genericObjectService == null)
				{
					if (GenericObject.s_genericObjectServiceFactory == null)
					{
						GenericObject.s_genericObjectServiceFactory = ComponentManager.Singleton.TryCreateInstance<IGenericObjectServiceFactory>(GenericObject.GUID_VERSIONABLEGENERICOBJECTSERVICEFACTORY);
						if (GenericObject.s_genericObjectServiceFactory == null)
						{
							GenericObject.s_genericObjectServiceFactory = new LegacyGenericObjectServiceFactory();
						}
					}
					genericObjectService = GenericObject.s_genericObjectServiceFactory.Create(type);
					obj = GenericObject.s_lockServices;
					lock (obj)
					{
						GenericObject.s_genericObjectServices[type] = genericObjectService;
					}
				}
				return genericObjectService;
			}
		}

		// Token: 0x04000135 RID: 309
		private static object s_lockServices = new object();

		// Token: 0x04000136 RID: 310
		private static IGenericObjectServiceFactory s_genericObjectServiceFactory = null;

		// Token: 0x04000137 RID: 311
		private static Dictionary<Type, IGenericObjectService> s_genericObjectServices = new Dictionary<Type, IGenericObjectService>();

		// Token: 0x04000138 RID: 312
		private static readonly Guid GUID_VERSIONABLEGENERICOBJECTSERVICEFACTORY = new Guid("{7D29F0A2-55DA-471F-81EE-9C11783C51D7}");
	}
}
