using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000E0 RID: 224
	[ReleasedInterface]
	public interface IMetaObject : IGenericObject, IArchivable, ICloneable, IComparable
	{
		// Token: 0x1700014E RID: 334
		// (get) Token: 0x0600037B RID: 891
		Guid ObjectGuid { get; }

		// Token: 0x1700014F RID: 335
		// (get) Token: 0x0600037C RID: 892
		int ProjectHandle { get; }

		// Token: 0x17000150 RID: 336
		// (get) Token: 0x0600037D RID: 893
		IObject Object { get; }

		// Token: 0x17000151 RID: 337
		// (get) Token: 0x0600037E RID: 894
		string Name { get; }

		// Token: 0x17000152 RID: 338
		// (get) Token: 0x0600037F RID: 895
		Guid Namespace { get; }

		// Token: 0x17000153 RID: 339
		// (get) Token: 0x06000380 RID: 896
		Guid[] SubObjectGuids { get; }

		// Token: 0x17000154 RID: 340
		// (get) Token: 0x06000381 RID: 897
		bool OrderedSubObjects { get; }

		// Token: 0x17000155 RID: 341
		// (get) Token: 0x06000382 RID: 898
		int Index { get; }

		// Token: 0x17000156 RID: 342
		// (get) Token: 0x06000383 RID: 899
		Guid ParentObjectGuid { get; }

		// Token: 0x17000157 RID: 343
		// (get) Token: 0x06000384 RID: 900
		bool IsToModify { get; }

		// Token: 0x17000158 RID: 344
		// (get) Token: 0x06000385 RID: 901
		IObjectProperty[] Properties { get; }

		// Token: 0x06000386 RID: 902
		IObjectProperty GetProperty(Guid propertyGuid);

		// Token: 0x06000387 RID: 903
		void AddProperty(IObjectProperty property);

		// Token: 0x06000388 RID: 904
		void RemoveProperty(Guid propertyGuid);
	}
}
