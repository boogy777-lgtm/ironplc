using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200013F RID: 319
	[TypeGuid("{5fa3943b-6996-47da-b5a9-1a554611f5cb}")]
	[StorageVersion("3.3.0.0")]
	public class MySortedListWrapper : GenericObject2
	{
		// Token: 0x17000742 RID: 1858
		// (get) Token: 0x06001B13 RID: 6931 RVA: 0x0004D018 File Offset: 0x0004C018
		// (set) Token: 0x06001B14 RID: 6932 RVA: 0x0004D04C File Offset: 0x0004C04C
		[DefaultSerialization("CrossReferencesList")]
		[StorageVersion("3.3.0.0")]
		private AddressCrossReference[] CrossRefsToSave
		{
			get
			{
				AddressCrossReference[] array = new AddressCrossReference[this._slCrossRefs.Count];
				this._slCrossRefs.Values.CopyTo(array, 0);
				return array;
			}
			set
			{
				for (int i = 0; i < value.Length; i++)
				{
					AddressCrossReference addressCrossReference = value[i];
					this._slCrossRefs[addressCrossReference] = addressCrossReference;
				}
			}
		}

		// Token: 0x17000743 RID: 1859
		// (get) Token: 0x06001B15 RID: 6933 RVA: 0x0004D07A File Offset: 0x0004C07A
		public LSortedList<AddressCrossReference, AddressCrossReference> List
		{
			get
			{
				return this._slCrossRefs;
			}
		}

		// Token: 0x040005AA RID: 1450
		[Obfuscation(Feature = "rename")]
		private LSortedList<AddressCrossReference, AddressCrossReference> _slCrossRefs = new LSortedList<AddressCrossReference, AddressCrossReference>(1, new AddressCrossReferenceComparer());
	}
}
