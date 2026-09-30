using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.Legacy;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000BE RID: 190
	[TypeGuid("{0F42BC3A-2CAE-4D8B-88FC-DE7CA378AED6}")]
	public class FBVisuCreatorAttributeProvider : IAttributeProvider
	{
		// Token: 0x170002C5 RID: 709
		// (get) Token: 0x06000B74 RID: 2932 RVA: 0x0001CE6C File Offset: 0x0001BE6C
		public IEnumerable<IAttribute> ProvidedAttributes
		{
			get
			{
				return new IAttribute[]
				{
					FBVisuCreatorAttributeProvider.A("FBVisuCreator"),
					FBVisuCreatorAttributeProvider.A("FBVisuCreatorTextCol"),
					FBVisuCreatorAttributeProvider.A("FBVisuCreatorTextStyleColor"),
					FBVisuCreatorAttributeProvider.A("FBVisuCreatorCustomName"),
					FBVisuCreatorAttributeProvider.A("FBVisuCreatorPrefix"),
					FBVisuCreatorAttributeProvider.A("FBVisuCreatorTemplate"),
					FBVisuCreatorAttributeProvider.A("FBVisuCreatorFormatString"),
					FBVisuCreatorAttributeProvider.A("FBVisuCreatorExcludeEntry"),
					FBVisuCreatorAttributeProvider.A("FBVisuCreatorTextColor"),
					FBVisuCreatorAttributeProvider.A("FBVisuCreatorTextStyleColor"),
					FBVisuCreatorAttributeProvider.A("FBVisuCreatorCustomName"),
					FBVisuCreatorAttributeProvider.A("FBVisuCreatorBoolColor"),
					FBVisuCreatorAttributeProvider.A("FBVisuCreatorBitField")
				};
			}
		}

		// Token: 0x06000B75 RID: 2933 RVA: 0x0001CF2D File Offset: 0x0001BF2D
		private static IAttribute A(string stName)
		{
			return new GenericAttribute(stName);
		}
	}
}
