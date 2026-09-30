using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000BD RID: 189
	internal sealed class InstancePathsContainer
	{
		// Token: 0x06000E6F RID: 3695 RVA: 0x00027598 File Offset: 0x00025798
		internal void \u0001(string \u0002, _IVariable \u0003, _ISignature \u0004)
		{
			InstancePathInformation item = new InstancePathInformation(\u0002, \u0003, \u0004);
			this.\u0001.Add(item);
		}

		// Token: 0x06000E70 RID: 3696 RVA: 0x000275BC File Offset: 0x000257BC
		internal void \u0001(IEnumerable<InstancePathInformation> \u0002)
		{
			foreach (InstancePathInformation instancePathInformation in \u0002)
			{
				this.\u0001(instancePathInformation.Path, instancePathInformation.DeclaredVariable, instancePathInformation.SignDeclarationLocation);
			}
		}

		// Token: 0x17000440 RID: 1088
		// (get) Token: 0x06000E71 RID: 3697 RVA: 0x00027618 File Offset: 0x00025818
		internal IEnumerable<string> InstancePaths
		{
			get
			{
				return this.InstancePathInformations.Select(new Func<InstancePathInformation, string>(InstancePathsContainer.<>c.<>9.\u0001));
			}
		}

		// Token: 0x17000441 RID: 1089
		// (get) Token: 0x06000E72 RID: 3698 RVA: 0x00027644 File Offset: 0x00025844
		internal IEnumerable<InstancePathInformation> InstancePathInformations
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x04000274 RID: 628
		private readonly HashSet<InstancePathInformation> \u0001 = new HashSet<InstancePathInformation>();
	}
}
