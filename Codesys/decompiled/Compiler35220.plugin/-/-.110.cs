using System;
using System.Collections;
using System.Collections.Generic;
using \u0006;
using \u0011;
using \u0014;
using \u0017;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0084;

namespace \u0007
{
	// Token: 0x0200014A RID: 330
	internal class \u0005 : ICommonScope2, ICommonScope, _IScope2, _IScope, IScope5, IScope4, IScope3, IScope2, IScope, global::\u0017.\u0006
	{
		// Token: 0x060016FC RID: 5884 RVA: 0x00046184 File Offset: 0x00044384
		internal static IScope5 \u0001(ICompileContext \u0002)
		{
			if (\u0002 == null)
			{
				return null;
			}
			return global::\u0007.\u0005.\u0001(\u0002, Helper.InvalidId);
		}

		// Token: 0x060016FD RID: 5885 RVA: 0x00046198 File Offset: 0x00044398
		internal static IScope5 \u0002(ICompileContext \u0002)
		{
			return new global::\u0007.\u0005(\u0002 as _ICompileContext, null, true, true, true);
		}

		// Token: 0x060016FE RID: 5886 RVA: 0x000461AC File Offset: 0x000443AC
		internal static IScope5 \u0001(ICompileContext \u0002, int \u0003, int \u0004)
		{
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, \u0003);
			scope.MethodSignature = \u0002.GetSignatureById(\u0004);
			return scope;
		}

		// Token: 0x060016FF RID: 5887 RVA: 0x000461C4 File Offset: 0x000443C4
		internal static IScope5 \u0001(ICompileContext \u0002, int \u0003)
		{
			return global::\u0007.\u0005.\u0001(\u0002, \u0003, true);
		}

		// Token: 0x06001700 RID: 5888 RVA: 0x000461D0 File Offset: 0x000443D0
		internal static IScope5 \u0001(ICompileContext \u0002, int \u0003, bool \u0004)
		{
			global::\u0007.\u0005 u = new global::\u0007.\u0005(\u0002 as _ICompileContext, \u0003, \u0004);
			u.\u0001();
			return u;
		}

		// Token: 0x06001701 RID: 5889 RVA: 0x000461E8 File Offset: 0x000443E8
		public void \u0001()
		{
			if (this.\u0001 != null && this.\u0001.HasAttribute("friend_instance"))
			{
				try
				{
					string attributeValue = this.\u0001.GetAttributeValue("friend_instance");
					IExpression expression = new global::\u0011.\u0006(APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(attributeValue, false, false, false, false)).\u0001();
					ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(this, this.\u0001[0], false, null);
					(expression as _IExpression).Accept(ivisit);
					if (expression.Type is _IUserdefType)
					{
						_IUserdefType iuserdefType = expression.Type as _IUserdefType;
						this.\u0003 = (this[iuserdefType.SignatureId] as _ISignature);
					}
					return;
				}
				catch
				{
					this.\u0003 = null;
					return;
				}
			}
			this.\u0003 = null;
		}

		// Token: 0x06001702 RID: 5890 RVA: 0x000462B8 File Offset: 0x000444B8
		private _ISignature \u0001(int \u0002)
		{
			if (\u0002 == -1)
			{
				return null;
			}
			_ISignature isignature = null;
			for (int i = 0; i < this.\u0001.Length; i++)
			{
				isignature = this.\u0001[i][\u0002];
				if (isignature != null)
				{
					break;
				}
			}
			return isignature;
		}

		// Token: 0x06001703 RID: 5891 RVA: 0x000462F4 File Offset: 0x000444F4
		public ICompiledPOU \u0001(int \u0002)
		{
			if (\u0002 == -1)
			{
				return null;
			}
			ICompiledPOU compiledPOU = null;
			for (int i = 0; i < this.\u0001.Length; i++)
			{
				compiledPOU = this.\u0001[i].GetCompiledPOUById(\u0002);
				if (compiledPOU != null)
				{
					break;
				}
			}
			return compiledPOU;
		}

		// Token: 0x17000552 RID: 1362
		// (get) Token: 0x06001704 RID: 5892 RVA: 0x00046330 File Offset: 0x00044530
		public int PointerSize
		{
			get
			{
				if (this.Codegenerator is ICodegenerator3 && (this.Codegenerator as ICodegenerator3).GetProperty(CodegeneratorProperties.LWordPointer))
				{
					return 8;
				}
				if (this.\u0001 != null && this.\u0001.Length != 0 && this.\u0001[0].DeviceSpecificProperties != null)
				{
					return this.\u0001[0].DeviceSpecificProperties.PointerSize;
				}
				return 4;
			}
		}

		// Token: 0x06001705 RID: 5893 RVA: 0x00046394 File Offset: 0x00044594
		public int \u0001(IType \u0002)
		{
			return (\u0002 as ICompiledType).Size(this);
		}

		// Token: 0x06001706 RID: 5894 RVA: 0x000463A4 File Offset: 0x000445A4
		public int \u0001(IType \u0002, IRecursionGuard \u0003)
		{
			if (\u0002 is ITypeWithRecursiveTypeCheck)
			{
				bool flag;
				return ((ITypeWithRecursiveTypeCheck)\u0002).SizeWithRecursionCheck(this, \u0003, out flag);
			}
			return (\u0002 as ICompiledType).Size(this);
		}

		// Token: 0x06001707 RID: 5895 RVA: 0x000463D8 File Offset: 0x000445D8
		public bool \u0001(_IExpression \u0002)
		{
			return \u0002.IsConstant(this, true);
		}

		// Token: 0x06001708 RID: 5896 RVA: 0x000463E4 File Offset: 0x000445E4
		public bool \u0002(_IExpression \u0002)
		{
			return \u0002.IsConstant(this, false);
		}

		// Token: 0x06001709 RID: 5897 RVA: 0x000463F0 File Offset: 0x000445F0
		public _IVariable \u0001(_IExpression \u0002)
		{
			return (_IVariable)\u0002.GetVariable(this);
		}

		// Token: 0x0600170A RID: 5898 RVA: 0x00046400 File Offset: 0x00044600
		public _ISignature \u0001(_IExpression \u0002)
		{
			return (_ISignature)\u0002.GetSignature(this);
		}

		// Token: 0x0600170B RID: 5899 RVA: 0x00046410 File Offset: 0x00044610
		public _IExpression \u0001(_ISignature \u0002, _IVariable \u0003)
		{
			return \u0003._Initial;
		}

		// Token: 0x0600170C RID: 5900 RVA: 0x00046418 File Offset: 0x00044618
		public global::\u0017.\u0006 \u0001(_IExpression \u0002, _IUserdefType \u0003)
		{
			_ISignature isignature = (_ISignature)this.\u0001(\u0003);
			if (isignature != null)
			{
				return (global::\u0017.\u0006)this.\u0001(isignature);
			}
			return this;
		}

		// Token: 0x0600170D RID: 5901 RVA: 0x00046444 File Offset: 0x00044644
		public global::\u0017.\u0006 \u0001(_ISignature \u0002)
		{
			return (global::\u0017.\u0006)this.\u0001(\u0002);
		}

		// Token: 0x17000553 RID: 1363
		// (get) Token: 0x0600170E RID: 5902 RVA: 0x00046454 File Offset: 0x00044654
		public bool ContainsCopyCode
		{
			get
			{
				return this.\u0001 != null && this.\u0001.Length != 0 && this.\u0001[0].DeviceSpecificProperties != null && this.\u0001[0].ContainsCopyCode;
			}
		}

		// Token: 0x0600170F RID: 5903 RVA: 0x00046488 File Offset: 0x00044688
		public void \u0001(_IExpression \u0002, out _IVariable \u0003, out _ISignature \u0004, out global::\u0017.\u0006 \u0005)
		{
			IVariable[] array;
			ISignature[] array2;
			IScope scope;
			this.\u0001(\u0002, out array, out array2, out scope);
			\u0005 = (global::\u0017.\u0006)scope;
			\u0003 = null;
			\u0004 = null;
			if (array != null && array.Length == 1)
			{
				\u0003 = (_IVariable)array[0];
			}
			if (array2 != null && array2.Length == 1)
			{
				\u0004 = (_ISignature)array2[0];
			}
		}

		// Token: 0x06001710 RID: 5904 RVA: 0x000464D8 File Offset: 0x000446D8
		public ISignature \u0001(IUserdefType \u0002)
		{
			return (\u0002 as _IUserdefType).GetSignature(this);
		}

		// Token: 0x06001711 RID: 5905 RVA: 0x000464E8 File Offset: 0x000446E8
		public ISignature \u0001(IEnumType \u0002)
		{
			return (\u0002 as _IEnumType).GetSignature(this);
		}

		// Token: 0x06001712 RID: 5906 RVA: 0x000464F8 File Offset: 0x000446F8
		public ILiteralValue \u0001(IExpression \u0002, bool \u0003)
		{
			return (\u0002 as _IExpression).Literal(this, \u0003);
		}

		// Token: 0x06001713 RID: 5907 RVA: 0x00046508 File Offset: 0x00044708
		public bool \u0001(ISignature \u0002, ISignature \u0003, ICommonScope \u0004)
		{
			return global::\u0006.\u0011.\u0001(\u0002, \u0003, this, \u0004 as IScope2);
		}

		// Token: 0x06001714 RID: 5908 RVA: 0x00046518 File Offset: 0x00044718
		public bool \u0001(IUserdefType \u0002, IUserdefType \u0003)
		{
			return global::\u0006.\u0011.\u0001(\u0002 as _IUserdefType, \u0003 as _IUserdefType, this, this);
		}

		// Token: 0x17000554 RID: 1364
		// (get) Token: 0x06001715 RID: 5909 RVA: 0x00046530 File Offset: 0x00044730
		public ICodegenerator Codegenerator
		{
			get
			{
				return this.\u0001[0].Codegenerator;
			}
		}

		// Token: 0x17000555 RID: 1365
		// (get) Token: 0x06001716 RID: 5910 RVA: 0x00046540 File Offset: 0x00044740
		public int MostLocalSignatureId
		{
			get
			{
				if (this.MethodSignature != null)
				{
					return this.MethodSignature.Id;
				}
				if (this.LocalSignature != null)
				{
					return this.LocalSignature.Id;
				}
				return Helper.InvalidId;
			}
		}

		// Token: 0x06001717 RID: 5911 RVA: 0x00046570 File Offset: 0x00044770
		internal void \u0001(_ICompileContext[] \u0002, _ISignature \u0003, bool \u0004, bool \u0005, bool \u0006)
		{
			this.\u0001 = \u0003;
			this.\u0001 = \u0002;
			this.\u0002 = \u0004;
			this.\u0004 = \u0005;
			if (this.\u0001 != null && this.\u0001.ParentSignatureId != Helper.InvalidId)
			{
				this.\u0002 = this.\u0001;
				this.\u0001 = this.\u0001(this.\u0001.ParentSignatureId);
			}
			if (!\u0006 || !(this.CurrentLibraryPath == string.Empty))
			{
				\u0084.\u0007 u = \u0002[0].CompiledSymbolTables as \u0084.\u0007;
				if (u != null)
				{
					this.\u0001 = u.\u0001(this.CurrentLibraryPath);
				}
			}
		}

		// Token: 0x06001718 RID: 5912 RVA: 0x00046614 File Offset: 0x00044814
		internal void \u0001(_ICompileContext[] \u0002, _ISignature \u0003, bool \u0004, bool \u0005)
		{
			this.\u0001(\u0002, \u0003, \u0004, \u0005, false);
		}

		// Token: 0x06001719 RID: 5913 RVA: 0x00046624 File Offset: 0x00044824
		internal void \u0001(_ICompileContext[] \u0002, int \u0003, bool \u0004)
		{
			this.\u0001 = \u0002;
			_ISignature u = this.\u0001(\u0003);
			this.\u0001(\u0002, u, \u0004, true);
		}

		// Token: 0x0600171A RID: 5914 RVA: 0x0004664C File Offset: 0x0004484C
		private static _ICompileContext[] \u0001(_ICompileContext \u0002)
		{
			int num = 0;
			_ICompileContext icompileContext;
			for (icompileContext = \u0002; icompileContext != null; icompileContext = icompileContext.ParentContext)
			{
				num++;
			}
			_ICompileContext[] array = new _ICompileContext[num];
			num = 0;
			icompileContext = \u0002;
			while (icompileContext != null)
			{
				array[num] = icompileContext;
				icompileContext = icompileContext.ParentContext;
				num++;
			}
			return array;
		}

		// Token: 0x0600171B RID: 5915 RVA: 0x00046690 File Offset: 0x00044890
		internal \u0005()
		{
		}

		// Token: 0x0600171C RID: 5916 RVA: 0x000466C8 File Offset: 0x000448C8
		internal \u0005(_ICompileContext \u0001\u0002, int \u0012\u0003, bool \u0013\u0003)
		{
			this.\u0001(global::\u0007.\u0005.\u0001(\u0001\u0002), \u0012\u0003, \u0013\u0003);
		}

		// Token: 0x0600171D RID: 5917 RVA: 0x00046718 File Offset: 0x00044918
		internal \u0005(_ICompileContext \u0001\u0002, _ISignature \u0014\u0003, bool \u0013\u0003)
		{
			this.\u0001(global::\u0007.\u0005.\u0001(\u0001\u0002), \u0014\u0003, \u0013\u0003, true);
		}

		// Token: 0x0600171E RID: 5918 RVA: 0x0004676C File Offset: 0x0004496C
		internal \u0005(_ICompileContext \u0001\u0002, _ISignature \u0014\u0003, bool \u0013\u0003, bool \u0015\u0003)
		{
			this.\u0001(global::\u0007.\u0005.\u0001(\u0001\u0002), \u0014\u0003, \u0013\u0003, \u0015\u0003);
		}

		// Token: 0x0600171F RID: 5919 RVA: 0x000467C0 File Offset: 0x000449C0
		internal \u0005(_ICompileContext \u0001\u0002, _ISignature \u0014\u0003, bool \u0013\u0003, bool \u0015\u0003, bool \u0088\u0008)
		{
			this.\u0001(global::\u0007.\u0005.\u0001(\u0001\u0002), \u0014\u0003, \u0013\u0003, \u0015\u0003, \u0088\u0008);
		}

		// Token: 0x06001720 RID: 5920 RVA: 0x00046814 File Offset: 0x00044A14
		internal \u0005(_ICompileContext[] \u0016\u0003, int \u0012\u0003, bool \u0017\u0003)
		{
			this.\u0001(\u0016\u0003, \u0012\u0003, \u0017\u0003);
		}

		// Token: 0x06001721 RID: 5921 RVA: 0x00046854 File Offset: 0x00044A54
		public IScope5 \u0001(ISignature \u0002)
		{
			_ICompileContext u0001_u = null;
			for (int i = 0; i < this.\u0001.Length; i++)
			{
				if (this.\u0001[i][\u0002.Id] != null)
				{
					u0001_u = this.\u0001[i];
					break;
				}
			}
			return new global::\u0007.\u0005(u0001_u, \u0002 as _ISignature, true);
		}

		// Token: 0x06001722 RID: 5922 RVA: 0x000468A4 File Offset: 0x00044AA4
		public virtual _IScope \u0001(string \u0002)
		{
			global::\u0007.\u0005 u = new global::\u0007.\u0005(this.\u0001, Helper.InvalidId, this.\u0002);
			if (!string.IsNullOrEmpty(\u0002))
			{
				u.\u0001 = this.\u0001[0].GetContextByLibraryPath(this.CurrentLibraryPath);
			}
			return u;
		}

		// Token: 0x17000556 RID: 1366
		// (get) Token: 0x06001723 RID: 5923 RVA: 0x000468EC File Offset: 0x00044AEC
		private string CurrentLibraryIdNew
		{
			get
			{
				if (this.\u0001 != null)
				{
					return this.\u0001.LibraryId;
				}
				if (this.\u0001 != null)
				{
					return this.\u0001.LibraryId;
				}
				return string.Empty;
			}
		}

		// Token: 0x17000557 RID: 1367
		// (get) Token: 0x06001724 RID: 5924 RVA: 0x0004691C File Offset: 0x00044B1C
		private string CurrentLibraryPath
		{
			get
			{
				if (this.\u0001 != null)
				{
					return this.\u0001.LibraryPath;
				}
				if (this.\u0001 != null)
				{
					return this.\u0001.LibraryPath;
				}
				return string.Empty;
			}
		}

		// Token: 0x06001725 RID: 5925 RVA: 0x0004694C File Offset: 0x00044B4C
		public ISignature[] \u0001(string \u0002)
		{
			_ICompileContext icompileContext;
			ISignature signature = this.\u0001(\u0002, out icompileContext);
			if (signature != null)
			{
				return new ISignature[]
				{
					signature
				};
			}
			return null;
		}

		// Token: 0x06001726 RID: 5926 RVA: 0x00046974 File Offset: 0x00044B74
		protected internal ISignature \u0001(string \u0002, out _ICompileContext \u0003)
		{
			\u0003 = null;
			ISignature signature = this.\u0001[0].FindSuperGlobalSignature(\u0002);
			if (signature == null)
			{
				for (int i = 0; i < this.\u0001.Length; i++)
				{
					signature = this.\u0001[i][\u0002];
					if (signature != null)
					{
						\u0003 = this.\u0001[i];
						break;
					}
				}
			}
			else
			{
				\u0003 = this.\u0001[0];
			}
			return signature;
		}

		// Token: 0x06001727 RID: 5927 RVA: 0x000469D4 File Offset: 0x00044BD4
		public virtual ISignature[] \u0002(string \u0002)
		{
			IList<ISignature> list = this[\u0002];
			if (list == null)
			{
				return null;
			}
			return Enumerable.ToLList<ISignature>(list).ToArray();
		}

		// Token: 0x17000558 RID: 1368
		public virtual IList<ISignature> this[string \u0002]
		{
			get
			{
				ISignature signature = null;
				_ICompileContext[] u = this.\u0001;
				for (int i = 0; i < u.Length; i++)
				{
					signature = u[i].FindSuperGlobalSignature(\u0002);
					if (signature != null)
					{
						break;
					}
				}
				if (signature == null && this.\u0001 != null)
				{
					global::\u0007.\u0006 u2 = this.\u0001[\u0002];
					if (u2 != null && u2.\u0005())
					{
						return Enumerable.ToLList<ISignature>(u2.\u0001());
					}
				}
				if (signature == null)
				{
					string stName;
					if (this.\u0006)
					{
						stName = "@pool." + \u0002;
					}
					else if (this.\u0001[0].LibraryIsUnique(this.CurrentLibraryIdNew))
					{
						stName = APEnvironmentFacade.Instance.LanguageModelMgr.VersionFreeLibraryPath(this.CurrentLibraryIdNew) + "." + \u0002;
					}
					else
					{
						stName = \u0002;
						if (this.CurrentLibraryIdNew != string.Empty)
						{
							stName = this.CurrentLibraryIdNew + "." + \u0002;
						}
					}
					for (int j = 0; j < this.\u0001.Length; j++)
					{
						signature = this.\u0001[j][stName];
						if (signature != null)
						{
							break;
						}
					}
					if (signature == null && string.IsNullOrEmpty(this.CurrentLibraryIdNew))
					{
						stName = "@pool." + \u0002;
						for (int k = 0; k < this.\u0001.Length; k++)
						{
							signature = this.\u0001[k][stName];
							if (signature != null)
							{
								break;
							}
						}
					}
					if (signature == null)
					{
						for (int l = 0; l < this.\u0001.Length; l++)
						{
							string text = this.CurrentLibraryPath;
							IPreCompileContext[] referencedLibraries = this.\u0001[0].GetReferencedLibraries(text);
							_IPreCompileContext precomLocal;
							if (string.IsNullOrEmpty(text))
							{
								precomLocal = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(this.\u0001[l].ApplicationGuid);
							}
							else
							{
								precomLocal = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(text);
							}
							foreach (_IPreCompileContext ipreCompileContext in referencedLibraries)
							{
								if (ipreCompileContext != null && !this.\u0001[l]._LibraryTable.GetQualifiedOnly(precomLocal, ipreCompileContext.LibraryPath))
								{
									if (this.\u0001[0].LibraryIsUnique(ipreCompileContext))
									{
										stName = APEnvironmentFacade.Instance.LanguageModelMgr.VersionFreeLibraryPath(ipreCompileContext.LibraryPath) + "." + \u0002;
									}
									else
									{
										stName = ipreCompileContext.LibraryId + "." + \u0002;
									}
									signature = this.\u0001[l][stName];
									if (signature != null)
									{
										break;
									}
								}
							}
							if (signature != null)
							{
								break;
							}
						}
					}
					if (signature == null)
					{
						stName = "__SYSTEM." + \u0002;
						for (int m = 0; m < this.\u0001.Length; m++)
						{
							signature = this.\u0001[m][stName];
							if (signature != null)
							{
								break;
							}
						}
					}
				}
				if (signature != null)
				{
					return new ISignature[]
					{
						signature
					};
				}
				return null;
			}
		}

		// Token: 0x17000559 RID: 1369
		// (get) Token: 0x06001729 RID: 5929 RVA: 0x00046CBC File Offset: 0x00044EBC
		// (set) Token: 0x0600172A RID: 5930 RVA: 0x00046CC4 File Offset: 0x00044EC4
		public int Id
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
				this.\u0002();
			}
		}

		// Token: 0x1700055A RID: 1370
		// (get) Token: 0x0600172B RID: 5931 RVA: 0x00046CD4 File Offset: 0x00044ED4
		// (set) Token: 0x0600172C RID: 5932 RVA: 0x00046CE4 File Offset: 0x00044EE4
		public string Name
		{
			get
			{
				return this.\u0001.ToUpperInvariant();
			}
			set
			{
				this.\u0001 = value;
				this.\u0002();
			}
		}

		// Token: 0x1700055B RID: 1371
		// (get) Token: 0x0600172D RID: 5933 RVA: 0x00046CF4 File Offset: 0x00044EF4
		public string DisplayName
		{
			get
			{
				if (!string.IsNullOrEmpty(this.Name))
				{
					return this.Name;
				}
				return this.CurrentLibraryIdNew;
			}
		}

		// Token: 0x1700055C RID: 1372
		public ISignature this[int \u0002]
		{
			get
			{
				if (this.\u0001 != null && this.\u0001.Id == \u0002)
				{
					return this.\u0001;
				}
				if (this.\u0002 != null && this.\u0002.Id == \u0002)
				{
					return this.\u0002;
				}
				return this.\u0001(\u0002);
			}
		}

		// Token: 0x1700055D RID: 1373
		// (get) Token: 0x0600172F RID: 5935 RVA: 0x00046D60 File Offset: 0x00044F60
		public ISignature[] All
		{
			get
			{
				IList<_ISignature> allSignatureList = this.\u0001[0].AllSignatureList;
				_ISignature[] array = new _ISignature[allSignatureList.Count];
				allSignatureList.CopyTo(array, 0);
				return array;
			}
		}

		// Token: 0x06001730 RID: 5936 RVA: 0x00046D90 File Offset: 0x00044F90
		public virtual IVariable \u0001(string \u0002, out ISignature \u0003)
		{
			\u0003 = null;
			if (this.\u0003 != null)
			{
				IVariable variable = this.\u0003[\u0002];
				if (variable != null)
				{
					\u0003 = this.\u0003;
					return variable;
				}
			}
			return null;
		}

		// Token: 0x06001731 RID: 5937 RVA: 0x00046DC8 File Offset: 0x00044FC8
		public static IVariable \u0001(global::\u0007.\u0005 \u0002, ISignature \u0003, string \u0004, out ISignature \u0005, LHashSet<ISignature> \u0006 = null)
		{
			\u0005 = null;
			if (\u0006 == null)
			{
				\u0006 = new LHashSet<ISignature>();
			}
			if (\u0006.Contains(\u0003))
			{
				return null;
			}
			\u0006.Add(\u0003);
			IVariable variable = \u0003[\u0004];
			if (variable != null)
			{
				\u0005 = \u0003;
				return variable;
			}
			if (\u0003.BaseSignatureId != Helper.InvalidId)
			{
				_ISignature u = \u0002[\u0003.BaseSignatureId] as _ISignature;
				variable = global::\u0007.\u0005.\u0001(\u0002, u, \u0004, out \u0005, \u0006);
				if (variable != null)
				{
					return variable;
				}
			}
			foreach (int u2 in \u0003.InterfaceIds)
			{
				_ISignature u3 = \u0002[u2] as _ISignature;
				variable = global::\u0007.\u0005.\u0001(\u0002, u3, \u0004, out \u0005, \u0006);
				if (variable != null)
				{
					return variable;
				}
			}
			return null;
		}

		// Token: 0x06001732 RID: 5938 RVA: 0x00046E74 File Offset: 0x00045074
		public virtual IVariable \u0002(string \u0002, out ISignature \u0003)
		{
			\u0003 = null;
			if (this.\u0002 != null)
			{
				IVariable variable = this.\u0002[\u0002];
				if (variable != null)
				{
					\u0003 = this.\u0002;
					return variable;
				}
			}
			if (this.\u0001 != null)
			{
				IVariable variable = global::\u0007.\u0005.\u0001(this, this.\u0001, \u0002, out \u0003, null);
				if (variable != null)
				{
					if (this.\u0002 != null && this.\u0001.POUType == Operator.Program && variable.GetFlag(VarFlag.Temp))
					{
						return null;
					}
					return variable;
				}
			}
			return this.\u0001(\u0002, out \u0003);
		}

		// Token: 0x06001733 RID: 5939 RVA: 0x00046EF4 File Offset: 0x000450F4
		private void \u0002()
		{
			this.\u0001.Clear();
		}

		// Token: 0x06001734 RID: 5940 RVA: 0x00046F04 File Offset: 0x00045104
		public virtual IVariable[] \u0001(string \u0002, out ISignature[] \u0003)
		{
			global::\u0007.\u0005.\u0001 u;
			if (this.\u0001.TryGetValue(\u0002, ref u))
			{
				\u0003 = u.\u0001;
				return u.\u0001;
			}
			if (this.\u0001 != null)
			{
				global::\u0007.\u0006 u2 = this.\u0001[\u0002];
				if (u2 != null && u2.\u0004())
				{
					ISignature[] array = u2.\u0001();
					\u0003 = array;
					return u2.\u0001();
				}
				\u0003 = null;
				return null;
			}
			else
			{
				LList<ISignature> llist = new LList<ISignature>();
				LList<IVariable> llist2 = new LList<IVariable>();
				\u0003 = null;
				for (int i = 0; i < this.\u0001.Length; i++)
				{
					foreach (_ISignature isignature in this.\u0001[i].GetLocalGVLSignatures(this.CurrentLibraryPath))
					{
						if (!isignature.HasAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY))
						{
							IVariable variable = isignature[\u0002];
							if (variable != null)
							{
								llist.Add(isignature);
								llist2.Add(variable);
							}
						}
					}
					if (llist2.Count != 0)
					{
						break;
					}
				}
				if (llist2.Count == 0)
				{
					for (int j = 0; j < this.\u0001.Length; j++)
					{
						foreach (_ISignature isignature2 in this.\u0001[j].GetLibraryGVLSignatures(this.CurrentLibraryPath))
						{
							if (!isignature2.HasAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY))
							{
								IVariable variable2 = isignature2[\u0002];
								if (variable2 != null)
								{
									llist.Add(isignature2);
									llist2.Add(variable2);
								}
							}
						}
						if (llist2.Count != 0)
						{
							break;
						}
					}
				}
				if (llist2.Count > 0)
				{
					IVariable[] array2 = new IVariable[llist2.Count];
					llist2.CopyTo(array2);
					\u0003 = new ISignature[llist.Count];
					llist.CopyTo(\u0003);
					global::\u0007.\u0005.\u0001 u3 = new global::\u0007.\u0005.\u0001();
					u3.\u0001 = array2;
					u3.\u0001 = \u0003;
					this.\u0001.Add(\u0002, u3);
					return array2;
				}
				return null;
			}
		}

		// Token: 0x06001735 RID: 5941 RVA: 0x00047118 File Offset: 0x00045318
		public IVariable[] \u0002(string \u0002, out ISignature[] \u0003)
		{
			\u0003 = null;
			ISignature signature = null;
			IVariable variable = this.\u0002(\u0002, out signature);
			if (variable != null)
			{
				IVariable[] array = new IVariable[]
				{
					variable
				};
				\u0003 = new ISignature[1];
				\u0003[0] = signature;
				return array;
			}
			if (!this.LocalScope)
			{
				IVariable[] array = this.\u0001(\u0002, out \u0003);
				if (array != null)
				{
					return array;
				}
			}
			return null;
		}

		// Token: 0x06001736 RID: 5942 RVA: 0x0004716C File Offset: 0x0004536C
		public ISignature \u0001(ISignature \u0002, string \u0003)
		{
			if (\u0002 == null)
			{
				return null;
			}
			ISignature signature = \u0002.GetSubSignature(\u0003);
			if (signature != null)
			{
				return signature;
			}
			if (\u0002.BaseSignatureId != Helper.InvalidId)
			{
				signature = this.\u0001(this[\u0002.BaseSignatureId], \u0003);
			}
			if (signature != null)
			{
				return signature;
			}
			if (\u0002.POUType == Operator.Interface)
			{
				foreach (int u in \u0002.InterfaceIds)
				{
					ISignature u2 = this[u];
					signature = this.\u0001(u2, \u0003);
					if (signature != null)
					{
						return signature;
					}
				}
			}
			return null;
		}

		// Token: 0x06001737 RID: 5943 RVA: 0x000471F0 File Offset: 0x000453F0
		public ISignature \u0001(string \u0002)
		{
			_ISignature u = this.\u0001;
			return this.\u0001(u, \u0002);
		}

		// Token: 0x06001738 RID: 5944 RVA: 0x0004720C File Offset: 0x0004540C
		public IVariable[] \u0001(string \u0002)
		{
			ISignature[] array;
			return this.\u0002(\u0002, out array);
		}

		// Token: 0x06001739 RID: 5945 RVA: 0x00047224 File Offset: 0x00045424
		public virtual bool \u0001(IExpression \u0002, out IVariable[] \u0003, out ISignature[] \u0004, out IScope \u0005)
		{
			\u0003 = null;
			\u0004 = null;
			\u0005 = null;
			if (\u0002 is _IVariableExpression)
			{
				return this.\u0001(\u0002.ToString(), out \u0003, out \u0004, out \u0005);
			}
			if (\u0002 is _ICompoAccessExpression)
			{
				ICompoAccessExpression compoAccessExpression = \u0002 as _ICompoAccessExpression;
				\u0005 = this.\u0001(compoAccessExpression.Left);
				return \u0005 != null && \u0005.FindDeclaration(compoAccessExpression.Right.ToString(), out \u0003, out \u0004, out \u0005);
			}
			if (\u0002 is _ISystemScopeExpression)
			{
				_ISystemScopeExpression isystemScopeExpression = \u0002 as _ISystemScopeExpression;
				\u0005 = this.SystemScope;
				return \u0005.FindDeclaration(isystemScopeExpression._Base.ToString(), out \u0003, out \u0004, out \u0005);
			}
			if (\u0002 is _IPoolScopeExpression)
			{
				_IPoolScopeExpression ipoolScopeExpression = \u0002 as _IPoolScopeExpression;
				\u0005 = this.PoolScope;
				return \u0005.FindDeclaration(ipoolScopeExpression._Base.ToString(), out \u0003, out \u0004, out \u0005);
			}
			if (\u0002 is _INamespaceAccessExpression)
			{
				_INamespaceAccessExpression inamespaceAccessExpression = \u0002 as _INamespaceAccessExpression;
				\u0005 = this.\u0001(inamespaceAccessExpression._Namespace);
				return \u0005 != null && \u0005.FindDeclaration(inamespaceAccessExpression._Access.ToString(), out \u0003, out \u0004, out \u0005);
			}
			return false;
		}

		// Token: 0x1700055E RID: 1374
		// (get) Token: 0x0600173A RID: 5946 RVA: 0x00047330 File Offset: 0x00045530
		// (set) Token: 0x0600173B RID: 5947 RVA: 0x00047338 File Offset: 0x00045538
		public bool FindListBeforeVariable
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x0600173C RID: 5948 RVA: 0x00047344 File Offset: 0x00045544
		internal virtual bool \u0001(string \u0002, out IVariable[] \u0003, out ISignature[] \u0004)
		{
			ISignature signature = null;
			\u0003 = null;
			\u0004 = null;
			IVariable variable = this.\u0002(\u0002, out signature);
			if (variable != null && !variable.HasFlag(VarFlag.External))
			{
				\u0003 = new IVariable[1];
				\u0003[0] = variable;
				\u0004 = new ISignature[1];
				\u0004[0] = signature;
				return true;
			}
			signature = this.\u0001(\u0002);
			if (signature != null)
			{
				\u0004 = new ISignature[1];
				\u0004[0] = signature;
				return true;
			}
			if (!this.LocalScope)
			{
				if (this.\u0001 != null && !this.FindListBeforeVariable)
				{
					global::\u0007.\u0006 u = this.\u0001[\u0002];
					if (u != null && (u.\u0004() || u.\u0005()))
					{
						if (u.\u0004())
						{
							IVariable[] array = u.\u0001();
							\u0003 = array;
						}
						if (u.\u0006())
						{
							ISignature[] array2 = u.\u0001();
							\u0004 = array2;
						}
						return true;
					}
				}
				if (this.FindListBeforeVariable)
				{
					if (this.\u0002)
					{
						\u0004 = this.\u0002(\u0002);
					}
					if (\u0004 == null)
					{
						\u0003 = this.\u0001(\u0002, out \u0004);
					}
				}
				else
				{
					\u0003 = this.\u0001(\u0002, out \u0004);
					if (\u0003 != null)
					{
						return true;
					}
					if (this.\u0002)
					{
						\u0004 = this.\u0002(\u0002);
					}
				}
			}
			return \u0004 != null;
		}

		// Token: 0x1700055F RID: 1375
		// (get) Token: 0x0600173D RID: 5949 RVA: 0x0004745C File Offset: 0x0004565C
		public IScope GlobalScope
		{
			get
			{
				return this._GlobalScope;
			}
		}

		// Token: 0x17000560 RID: 1376
		// (get) Token: 0x0600173E RID: 5950 RVA: 0x00047464 File Offset: 0x00045664
		public virtual global::\u0007.\u0005 _CopyScope
		{
			get
			{
				_ICompileContext referenceContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContext(this.\u0001[0].ApplicationGuid);
				if (referenceContext == null || referenceContext == this.\u0001[0])
				{
					return this;
				}
				int nId = Helper.InvalidId;
				int nId2 = Helper.InvalidId;
				if (this.LocalSignature != null)
				{
					nId = this.LocalSignature.Id;
				}
				if (this.MethodSignature != null)
				{
					nId2 = this.MethodSignature.Id;
				}
				global::\u0007.\u0005 u = (global::\u0007.\u0005)referenceContext.CreateGlobalIScope();
				_ISignature isignature = referenceContext[nId];
				_ISignature isignature2 = referenceContext[nId2];
				if (isignature == null && this.LocalSignature != null)
				{
					u.LocalSignature = this.LocalSignature;
				}
				else
				{
					u.LocalSignature = isignature;
				}
				if (isignature2 == null && this.MethodSignature != null)
				{
					u.MethodSignature = this.MethodSignature;
				}
				else
				{
					u.MethodSignature = isignature2;
				}
				u.CopyScope = true;
				return u;
			}
		}

		// Token: 0x17000561 RID: 1377
		// (get) Token: 0x0600173F RID: 5951 RVA: 0x0004753C File Offset: 0x0004573C
		public virtual global::\u0007.\u0005 _GlobalScope
		{
			get
			{
				global::\u0007.\u0005 u = new global::\u0007.\u0005(this.\u0001, Helper.InvalidId, this.\u0002);
				if (!string.IsNullOrEmpty(this.CurrentLibraryPath))
				{
					u.\u0001 = this.\u0001[0].GetContextByLibraryPath(this.CurrentLibraryPath);
				}
				return u;
			}
		}

		// Token: 0x17000562 RID: 1378
		// (get) Token: 0x06001740 RID: 5952 RVA: 0x00047588 File Offset: 0x00045788
		public virtual _IScope PoolScope
		{
			get
			{
				global::\u0007.\u0005 u = new global::\u0007.\u0005(this.\u0001, Helper.InvalidId, true);
				\u0084.\u0007 u2 = this.\u0001[0].CompiledSymbolTables as \u0084.\u0007;
				if (u2 != null)
				{
					u.\u0001 = u2.\u0001();
				}
				u.\u0006 = true;
				return u;
			}
		}

		// Token: 0x17000563 RID: 1379
		// (get) Token: 0x06001741 RID: 5953 RVA: 0x000475D4 File Offset: 0x000457D4
		public virtual IScope5 SystemScope
		{
			get
			{
				return new global::\u0007.\u0005(this.\u0001, Helper.InvalidId, true);
			}
		}

		// Token: 0x06001742 RID: 5954 RVA: 0x000475E8 File Offset: 0x000457E8
		public virtual IScope2 \u0001(IExpression \u0002)
		{
			if (\u0002 is ICompoAccessExpression)
			{
				ICompoAccessExpression compoAccessExpression = \u0002 as ICompoAccessExpression;
				IScope2 scope = this.\u0001(compoAccessExpression.Left);
				if (scope != null)
				{
					return scope.FindScope(compoAccessExpression.Right);
				}
				return null;
			}
			else if (\u0002 is _INamespaceAccessExpression)
			{
				_INamespaceAccessExpression inamespaceAccessExpression = \u0002 as _INamespaceAccessExpression;
				IScope2 scope2 = this.\u0001(inamespaceAccessExpression._Namespace);
				if (scope2 != null)
				{
					return scope2.FindScope(inamespaceAccessExpression._Access);
				}
				return null;
			}
			else
			{
				if (\u0002 is IVariableExpression)
				{
					return this.\u0001(\u0002.ToString()) as IScope2;
				}
				return null;
			}
		}

		// Token: 0x06001743 RID: 5955 RVA: 0x0004766C File Offset: 0x0004586C
		private void \u0001(_IPreCompileContext \u0002)
		{
			this.\u0001 = \u0002;
			if (this.\u0001[0].CompiledSymbolTables != null)
			{
				this.\u0001 = (this.\u0001[0].CompiledSymbolTables as \u0084.\u0007).\u0001(this.CurrentLibraryPath);
			}
		}

		// Token: 0x06001744 RID: 5956 RVA: 0x000476A8 File Offset: 0x000458A8
		public virtual IScope \u0001(string \u0002)
		{
			_ICompileContext icompileContext = null;
			_IPreCompileContext ipreCompileContext = null;
			_IPreCompileContext ipreCompileContext2 = this.\u0001;
			if (ipreCompileContext2 == null && this.CurrentLibraryPath != string.Empty)
			{
				ipreCompileContext2 = this.\u0001[0].GetContextByLibraryPath(this.CurrentLibraryPath);
			}
			if (ipreCompileContext2 != null)
			{
				if (ipreCompileContext2.Namespace == \u0002.ToUpperInvariant())
				{
					this.Id = this.\u0001[0].GetIdOfLibraryReference(ipreCompileContext2, \u0002);
					return this;
				}
				ipreCompileContext = this.\u0001[0]._LibraryTable.GetLibraryContextByNamespace(\u0002, ipreCompileContext2);
				icompileContext = this.\u0001[0];
			}
			else
			{
				for (int i = 0; i < this.\u0001.Length; i++)
				{
					icompileContext = this.\u0001[i];
					ipreCompileContext = this.\u0001[i].GetLibraryByName(\u0002);
					if (ipreCompileContext != null)
					{
						break;
					}
				}
			}
			if (ipreCompileContext != null)
			{
				global::\u0007.\u0005 u = global::\u0007.\u0005.\u0001(icompileContext, Helper.InvalidId) as global::\u0007.\u0005;
				u.Name = \u0002;
				while (icompileContext != null)
				{
					u.Id = icompileContext.GetIdOfLibraryReference(ipreCompileContext, \u0002);
					if (u.Id != -1)
					{
						break;
					}
					icompileContext = icompileContext.ParentContext;
				}
				u.\u0004 = false;
				u.\u0001(ipreCompileContext);
				return u;
			}
			return null;
		}

		// Token: 0x06001745 RID: 5957 RVA: 0x000477BC File Offset: 0x000459BC
		public virtual bool \u0001(string \u0002, out IVariable[] \u0003, out ISignature[] \u0004, out IScope \u0005)
		{
			\u0005 = null;
			if (this.\u0001(\u0002, out \u0003, out \u0004))
			{
				return true;
			}
			if (!this.LocalScope)
			{
				\u0005 = this.\u0001(\u0002);
				if (\u0005 != null)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001746 RID: 5958 RVA: 0x000477EC File Offset: 0x000459EC
		public ISignature \u0002(string \u0002)
		{
			ISignature[] array = this.\u0002(\u0002);
			if (array == null || array.Length == 0)
			{
				return null;
			}
			return array[0];
		}

		// Token: 0x06001747 RID: 5959 RVA: 0x00047810 File Offset: 0x00045A10
		[Obsolete("_IQualifiedNameExpression is no longer used, function will return null. Use FindSignature in IScope2 instead")]
		public ISignature[] \u0001(IQualifiedNameExpression \u0002)
		{
			return null;
		}

		// Token: 0x06001748 RID: 5960 RVA: 0x00047814 File Offset: 0x00045A14
		public ISignature[] \u0001(IExpression \u0002)
		{
			if (\u0002 is IVariableExpression)
			{
				return this.\u0003(\u0002.ToString());
			}
			if (\u0002 is ICompoAccessExpression)
			{
				ICompoAccessExpression compoAccessExpression = \u0002 as ICompoAccessExpression;
				IScope2 scope = this.\u0001(compoAccessExpression.Left);
				if (scope != null)
				{
					return scope.FindSignature(compoAccessExpression.Right);
				}
				return null;
			}
			else
			{
				if (\u0002 is _ISystemScopeExpression)
				{
					_ISystemScopeExpression isystemScopeExpression = \u0002 as _ISystemScopeExpression;
					return this.SystemScope.FindSignature(isystemScopeExpression._Base.ToString());
				}
				if (\u0002 is _IPoolScopeExpression)
				{
					_IPoolScopeExpression ipoolScopeExpression = \u0002 as _IPoolScopeExpression;
					return this.PoolScope.FindSignature(ipoolScopeExpression._Base.ToString());
				}
				if (!(\u0002 is _INamespaceAccessExpression))
				{
					return null;
				}
				_INamespaceAccessExpression inamespaceAccessExpression = \u0002 as _INamespaceAccessExpression;
				IScope2 scope2 = this.\u0001(inamespaceAccessExpression._Namespace);
				if (scope2 != null)
				{
					return scope2.FindSignature(inamespaceAccessExpression._Access);
				}
				return null;
			}
		}

		// Token: 0x06001749 RID: 5961 RVA: 0x000478E8 File Offset: 0x00045AE8
		public ISignature[] \u0003(string \u0002)
		{
			IList<ISignature> list = this[\u0002];
			if (list == null || list.Count == 0)
			{
				return null;
			}
			if (list.Count == 1)
			{
				return new ISignature[]
				{
					list[0]
				};
			}
			return Enumerable.ToLList<ISignature>(list).ToArray();
		}

		// Token: 0x17000564 RID: 1380
		// (get) Token: 0x0600174A RID: 5962 RVA: 0x00047930 File Offset: 0x00045B30
		// (set) Token: 0x0600174B RID: 5963 RVA: 0x00047938 File Offset: 0x00045B38
		public ISignature LocalSignature
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = (value as _ISignature);
				this.\u0002();
			}
		}

		// Token: 0x0600174C RID: 5964 RVA: 0x0004794C File Offset: 0x00045B4C
		public void \u0002(ISignature \u0002)
		{
			this.LocalSignature = \u0002;
		}

		// Token: 0x17000565 RID: 1381
		// (get) Token: 0x0600174D RID: 5965 RVA: 0x00047958 File Offset: 0x00045B58
		// (set) Token: 0x0600174E RID: 5966 RVA: 0x00047960 File Offset: 0x00045B60
		public ISignature MethodSignature
		{
			get
			{
				return this.\u0002;
			}
			set
			{
				this.\u0002 = (value as _ISignature);
				this.\u0002();
			}
		}

		// Token: 0x0600174F RID: 5967 RVA: 0x00047974 File Offset: 0x00045B74
		public virtual IScope \u0001(int \u0002)
		{
			if (\u0002 == this.Id && !this.\u0004)
			{
				return this;
			}
			_IPreCompileContext ipreCompileContext = null;
			_ICompileContext icompileContext = null;
			string text = string.Empty;
			for (int i = 0; i < this.\u0001.Length; i++)
			{
				ipreCompileContext = this.\u0001[i].GetLibraryById(\u0002);
				text = this.\u0001[i].GetNameOfLibrary(\u0002);
				icompileContext = this.\u0001[i];
				if (ipreCompileContext != null)
				{
					break;
				}
			}
			if (ipreCompileContext == null)
			{
				return null;
			}
			global::\u0007.\u0005 u = global::\u0007.\u0005.\u0001(icompileContext, Helper.InvalidId) as global::\u0007.\u0005;
			u.Name = text;
			u.Id = icompileContext.GetIdOfLibraryReference(ipreCompileContext, text);
			u.\u0004 = false;
			u.\u0001(ipreCompileContext);
			return u;
		}

		// Token: 0x06001750 RID: 5968 RVA: 0x00047A14 File Offset: 0x00045C14
		public void \u0001(string \u0002, string \u0003)
		{
			if (this.DefineTable.ContainsKey(\u0002))
			{
				this.DefineTable[\u0002] = \u0003;
				return;
			}
			this.DefineTable.Add(\u0002, \u0003);
		}

		// Token: 0x06001751 RID: 5969 RVA: 0x00047A40 File Offset: 0x00045C40
		public void \u0002(string \u0002)
		{
			this.DefineTable.Remove(\u0002);
		}

		// Token: 0x06001752 RID: 5970 RVA: 0x00047A50 File Offset: 0x00045C50
		public bool \u0001(string \u0002)
		{
			for (int i = 0; i < this.\u0001.Length; i++)
			{
				if (this.\u0001[i].IsDefined(\u0002))
				{
					return true;
				}
			}
			return this.\u0001 != null && this.DefineTable.ContainsKey(\u0002);
		}

		// Token: 0x06001753 RID: 5971 RVA: 0x00047A98 File Offset: 0x00045C98
		public bool \u0001(string \u0002, string \u0003)
		{
			for (int i = 0; i < this.\u0001.Length; i++)
			{
				if (this.\u0001[i].DefineHasValue(\u0002, \u0003))
				{
					return true;
				}
			}
			return this.\u0001 != null && this.DefineTable.ContainsKey(\u0002) && (string)this.DefineTable[\u0002] == \u0003;
		}

		// Token: 0x17000566 RID: 1382
		// (get) Token: 0x06001754 RID: 5972 RVA: 0x00047AFC File Offset: 0x00045CFC
		protected Hashtable DefineTable
		{
			get
			{
				if (this.\u0001 == null)
				{
					this.\u0001 = new Hashtable();
				}
				return this.\u0001;
			}
		}

		// Token: 0x17000567 RID: 1383
		// (get) Token: 0x06001755 RID: 5973 RVA: 0x00047B18 File Offset: 0x00045D18
		// (set) Token: 0x06001756 RID: 5974 RVA: 0x00047B20 File Offset: 0x00045D20
		public bool SearchLocalScope
		{
			get
			{
				return this.\u0004;
			}
			set
			{
				this.\u0004 = value;
				this.\u0002();
			}
		}

		// Token: 0x17000568 RID: 1384
		// (get) Token: 0x06001757 RID: 5975 RVA: 0x00047B30 File Offset: 0x00045D30
		public ICompileContext ApplicationContext
		{
			get
			{
				return this.\u0001[0];
			}
		}

		// Token: 0x17000569 RID: 1385
		// (get) Token: 0x06001758 RID: 5976 RVA: 0x00047B3C File Offset: 0x00045D3C
		// (set) Token: 0x06001759 RID: 5977 RVA: 0x00047B44 File Offset: 0x00045D44
		public bool LocalScope
		{
			get
			{
				return this.\u0003;
			}
			set
			{
				this.\u0003 = value;
				this.\u0002();
			}
		}

		// Token: 0x0600175A RID: 5978 RVA: 0x00047B54 File Offset: 0x00045D54
		internal void \u0003()
		{
			this.\u0001(null);
			this.\u0002();
		}

		// Token: 0x1700056A RID: 1386
		// (get) Token: 0x0600175B RID: 5979 RVA: 0x00047B64 File Offset: 0x00045D64
		// (set) Token: 0x0600175C RID: 5980 RVA: 0x00047B6C File Offset: 0x00045D6C
		public bool CopyScope
		{
			get
			{
				return this.\u0005;
			}
			set
			{
				this.\u0005 = value;
			}
		}

		// Token: 0x1700056B RID: 1387
		// (get) Token: 0x0600175D RID: 5981 RVA: 0x00047B78 File Offset: 0x00045D78
		public bool IsPrecompileScope
		{
			get
			{
				return false;
			}
		}

		// Token: 0x040003FE RID: 1022
		private global::\u0014.\u0005 \u0001;

		// Token: 0x040003FF RID: 1023
		private LDictionary<string, global::\u0007.\u0005.\u0001> \u0001 = new LDictionary<string, global::\u0007.\u0005.\u0001>();

		// Token: 0x04000400 RID: 1024
		private bool \u0001;

		// Token: 0x04000401 RID: 1025
		protected _IPreCompileContext \u0001;

		// Token: 0x04000402 RID: 1026
		protected _ICompileContext[] \u0001;

		// Token: 0x04000403 RID: 1027
		protected Hashtable \u0001;

		// Token: 0x04000404 RID: 1028
		protected bool \u0002 = true;

		// Token: 0x04000405 RID: 1029
		protected _ISignature \u0001;

		// Token: 0x04000406 RID: 1030
		protected _ISignature \u0002;

		// Token: 0x04000407 RID: 1031
		protected _ISignature \u0003;

		// Token: 0x04000408 RID: 1032
		protected string \u0001 = string.Empty;

		// Token: 0x04000409 RID: 1033
		protected bool \u0003;

		// Token: 0x0400040A RID: 1034
		protected bool \u0004 = true;

		// Token: 0x0400040B RID: 1035
		protected int \u0001 = Helper.InvalidId;

		// Token: 0x0400040C RID: 1036
		private bool \u0005;

		// Token: 0x0400040D RID: 1037
		private bool \u0006;

		// Token: 0x0200014B RID: 331
		private sealed class \u0001
		{
			// Token: 0x0400040E RID: 1038
			public IVariable[] \u0001;

			// Token: 0x0400040F RID: 1039
			public ISignature[] \u0001;
		}
	}
}
