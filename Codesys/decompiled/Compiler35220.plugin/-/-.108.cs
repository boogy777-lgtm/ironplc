using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0012
{
	// Token: 0x02000140 RID: 320
	internal sealed class \u000E
	{
		// Token: 0x17000528 RID: 1320
		// (get) Token: 0x0600160B RID: 5643 RVA: 0x00041304 File Offset: 0x0003F504
		private _ICompileContext ComconNew { get; }

		// Token: 0x17000529 RID: 1321
		// (get) Token: 0x0600160C RID: 5644 RVA: 0x0004130C File Offset: 0x0003F50C
		private _ICompileContext ComconOld { get; }

		// Token: 0x1700052A RID: 1322
		// (get) Token: 0x0600160D RID: 5645 RVA: 0x00041314 File Offset: 0x0003F514
		private _IPreCompileContext3 Precomp { get; }

		// Token: 0x0600160E RID: 5646 RVA: 0x0004131C File Offset: 0x0003F51C
		internal \u000E(_ICompileContext \u001C\u0004, _ICompileContext \u001B\u0003, _IPreCompileContext3 \u001D\u0005)
		{
			this.ComconNew = \u001C\u0004;
			this.ComconOld = \u001B\u0003;
			this.Precomp = \u001D\u0005;
		}

		// Token: 0x0600160F RID: 5647 RVA: 0x0004133C File Offset: 0x0003F53C
		private _ISignature \u0001(_ISignature \u0002, _ISignature \u0003, _IPreCompileContext \u0004)
		{
			return this.ComconNew.CreateCompiledSignature(\u0002, \u0003, \u0004, this.ComconOld);
		}

		// Token: 0x06001610 RID: 5648 RVA: 0x00041354 File Offset: 0x0003F554
		private _ISignature \u0001(_ISignature \u0002, _IPreCompileContext \u0003)
		{
			string searchName = \u0002.GetSearchName(this.ComconNew);
			_ISignature isignature = null;
			if (this.ComconOld != null)
			{
				isignature = this.ComconOld[searchName];
			}
			_ISignature isignature2 = this.\u0001(\u0002, isignature, \u0003);
			bool flag = false;
			if (\u0002.IsLibraryObject)
			{
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(\u0002.LibraryPath);
				if (libraryContext != null && !string.IsNullOrEmpty(libraryContext.UnitTestingDefine) && this.ComconNew.IsDefined(libraryContext.UnitTestingDefine))
				{
					flag = true;
					isignature2.SetFlag(SignatureFlag.Internal, false);
					isignature2.SetFlag(SignatureFlag.Private, false);
					isignature2.SetFlag(SignatureFlag.Protected, false);
					isignature2.SetFlag(SignatureFlag.Final, false);
				}
			}
			this.ComconNew.AddSignature(isignature2, isignature, this.ComconOld);
			foreach (_ISignature isignature3 in isignature2.GetOrderedSubSignatures())
			{
				_ISignature signRef = null;
				if (isignature != null)
				{
					signRef = (isignature.GetSubSignature(isignature3.Name) as _ISignature);
				}
				isignature3.ParentSignatureId = isignature2.Id;
				if (flag)
				{
					isignature3.SetFlag(SignatureFlag.Internal, false);
					isignature3.SetFlag(SignatureFlag.Private, false);
					isignature3.SetFlag(SignatureFlag.Protected, false);
					isignature3.SetFlag(SignatureFlag.Final, false);
				}
				this.ComconNew.AddSignature(isignature3, signRef, this.ComconOld);
			}
			return isignature2;
		}

		// Token: 0x06001611 RID: 5649 RVA: 0x000414E0 File Offset: 0x0003F6E0
		internal void \u0001(_ISignature \u0002, _IPreCompileContext \u0003)
		{
			_ISignature isignature = this.\u0001(\u0002, \u0003);
			_ICompiledPOU pou = \u0003.GetPOU(\u0002.ObjectGuid);
			if (pou != null)
			{
				this.ComconNew.AddCompiledPOU(pou.CreateCompiledPOU(), isignature, this.ComconOld);
			}
			foreach (_ISignature isignature2 in isignature.GetSubSignatures())
			{
				if (!(isignature2.ObjectGuid == Guid.Empty))
				{
					pou = \u0003.GetPOU(isignature2.ObjectGuid);
					if (pou != null)
					{
						this.ComconNew.AddCompiledPOU(pou.CreateCompiledPOU(), isignature2, this.ComconOld);
					}
				}
			}
		}

		// Token: 0x06001612 RID: 5650 RVA: 0x00041578 File Offset: 0x0003F778
		internal static _ISignature \u0001(_ISignature \u0002, _IPreCompileContext \u0003, _ICompileContext \u0004, _ICompileContext \u0005)
		{
			return new \u000E(\u0004, \u0005, \u0003 as _IPreCompileContext3).\u0001(\u0002, \u0003);
		}

		// Token: 0x040003DE RID: 990
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x040003DF RID: 991
		[CompilerGenerated]
		private readonly _ICompileContext \u0002;

		// Token: 0x040003E0 RID: 992
		[CompilerGenerated]
		private readonly _IPreCompileContext3 \u0001;
	}
}
