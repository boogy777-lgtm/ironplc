using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.Legacy
{
	// Token: 0x02000271 RID: 625
	internal class GenericAttribute : IAttribute
	{
		// Token: 0x06002A3F RID: 10815 RVA: 0x0006B4FF File Offset: 0x0006A4FF
		public GenericAttribute(string name)
		{
			this._name = name;
		}

		// Token: 0x17000BDA RID: 3034
		// (get) Token: 0x06002A40 RID: 10816 RVA: 0x0006B50E File Offset: 0x0006A50E
		public string Name
		{
			get
			{
				return this._name;
			}
		}

		// Token: 0x04000803 RID: 2051
		private readonly string _name;
	}
}
