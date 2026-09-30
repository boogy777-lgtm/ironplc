using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u000F;
using \u0013;
using \u0017;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.PreCompile.Typification
{
	// Token: 0x02000195 RID: 405
	public class PreCompileTypifier : ILMPreCompileTypifier2, ILMPreCompileTypifier
	{
		// Token: 0x170005C9 RID: 1481
		// (get) Token: 0x06001D0B RID: 7435 RVA: 0x0005EE74 File Offset: 0x0005D074
		// (set) Token: 0x06001D0C RID: 7436 RVA: 0x0005EE7C File Offset: 0x0005D07C
		private _IPreCompileContext PreComLocal { get; set; }

		// Token: 0x170005CA RID: 1482
		// (get) Token: 0x06001D0D RID: 7437 RVA: 0x0005EE88 File Offset: 0x0005D088
		// (set) Token: 0x06001D0E RID: 7438 RVA: 0x0005EE90 File Offset: 0x0005D090
		private global::\u000F.\u0007 Scope { get; set; }

		// Token: 0x06001D0F RID: 7439 RVA: 0x0005EE9C File Offset: 0x0005D09C
		public PreCompileTypifier(_IPreCompileContext precom)
		{
			this.PreComLocal = precom;
			this.Scope = new global::\u000F.\u0007(precom);
		}

		// Token: 0x06001D10 RID: 7440 RVA: 0x0005EEB8 File Offset: 0x0005D0B8
		public bool CheckPOUCode(Guid guidObject, out IEnumerable<IMessage> messages)
		{
			messages = new LList<IMessage>();
			LList<IMessage4> llist = new LList<IMessage4>();
			bool result = this.PreComLocal.CheckPOUCode(guidObject, llist);
			messages = llist;
			return result;
		}

		// Token: 0x06001D11 RID: 7441 RVA: 0x0005EEE4 File Offset: 0x0005D0E4
		public bool CheckSignature(Guid guidSignature, out IEnumerable<IMessage> messages)
		{
			messages = null;
			_ISignature isignature = this.PreComLocal.GetSignature(guidSignature) as _ISignature;
			if (isignature == null)
			{
				return false;
			}
			bool result = this.PreComLocal.CheckSignature(isignature);
			messages = isignature.Messages;
			return result;
		}

		// Token: 0x06001D12 RID: 7442 RVA: 0x0005EF20 File Offset: 0x0005D120
		public IStatement CreateTypifiedParseTree(Guid guidObject, bool bAddImplicitConversions)
		{
			if (bAddImplicitConversions)
			{
				return this.CreateTypifiedParseTree(guidObject, ConversionOptions.AddExplicitConversions);
			}
			return this.CreateTypifiedParseTree(guidObject, ConversionOptions.TypesOnly);
		}

		// Token: 0x06001D13 RID: 7443 RVA: 0x0005EF38 File Offset: 0x0005D138
		public IStatement CreateTypifiedParseTree(Guid guidObject, ConversionOptions options)
		{
			_ICompiledPOU icompiledPOU = this.PreComLocal.GetCompiledPOU(guidObject) as _ICompiledPOU;
			if (icompiledPOU == null)
			{
				return null;
			}
			global::\u0017.\u000E u000E = new global::\u0017.\u000E(this.PreComLocal, this.Scope);
			u000E.\u0001(icompiledPOU);
			IStatement statement = u000E.TypifiedRedParseTree;
			if (options != ConversionOptions.TypesOnly)
			{
				new global::\u0013.\u0005(this.Scope, options == ConversionOptions.AddExplicitConversionsAndExplicitReferences).\u0001(statement);
			}
			return statement;
		}

		// Token: 0x06001D14 RID: 7444 RVA: 0x0005EF94 File Offset: 0x0005D194
		private _ISignature \u0001(Guid \u0002)
		{
			_ISignature isignature = this.PreComLocal[\u0002];
			if (isignature != null)
			{
				return isignature;
			}
			return APEnvironmentFacade.Instance.LanguageModelMgr.Pool[\u0002];
		}

		// Token: 0x06001D15 RID: 7445 RVA: 0x0005EFC8 File Offset: 0x0005D1C8
		public IStatement TypifyStatement(IStatement stmt, Guid guidObject, bool bAddImplicitConversions)
		{
			if (bAddImplicitConversions)
			{
				return this.TypifyStatement(stmt, guidObject, ConversionOptions.AddExplicitConversions);
			}
			return this.TypifyStatement(stmt, guidObject, ConversionOptions.TypesOnly);
		}

		// Token: 0x06001D16 RID: 7446 RVA: 0x0005EFE0 File Offset: 0x0005D1E0
		public IStatement TypifyStatement(IStatement stmt, Guid guidObject, ConversionOptions options)
		{
			_ISignature isignature = this.\u0001(guidObject);
			_IStatement istatement = (_IStatement)((_IStatement)stmt).Duplicate();
			SimpleTypeChecker ivisit = new SimpleTypeChecker(isignature, APEnvironmentFacade.Instance.PrimaryProjectHandle, this.PreComLocal, false);
			istatement.Accept(ivisit);
			new global::\u0017.\u000E(this.PreComLocal, this.Scope).\u0001(istatement, isignature);
			if (options != ConversionOptions.TypesOnly)
			{
				new global::\u0013.\u0005(this.Scope, options == ConversionOptions.AddExplicitConversionsAndExplicitReferences).\u0001(istatement);
			}
			return istatement;
		}

		// Token: 0x040004CF RID: 1231
		[CompilerGenerated]
		private _IPreCompileContext \u0001;

		// Token: 0x040004D0 RID: 1232
		[CompilerGenerated]
		private global::\u000F.\u0007 \u0001;
	}
}
