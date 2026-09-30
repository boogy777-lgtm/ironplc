using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0008;
using \u0011;
using \u0013;
using \u0015;
using \u0017;
using _3S.CoDeSys.Compiler35220.ImplicitCode;
using _3S.CoDeSys.Compiler35220.PreCompile.Typification;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Scopes
{
	// Token: 0x02000142 RID: 322
	internal sealed class CheckerScope : ICommonScope2, ICommonScope, _IPrecompileScope, IPrecompileScope6, IPrecompileScope5, IPrecompileScope4, IPrecompileScope3, IPrecompileScope2, IPrecompileScope, IPrecompileScope7, IPrecompileScopeWithAliasService, _IPrecompileScope3, _IPrecompileScope2, IPrecompileScope8, global::\u0017.\u0006, global::\u0015.\u0002
	{
		// Token: 0x06001628 RID: 5672 RVA: 0x00041DE4 File Offset: 0x0003FFE4
		public CheckerScope(_ISignature signLocal, _IPreCompileContext precomLocal, _IPreCompileContext precomPool)
		{
			this.\u0001 = APEnvironmentFacade.Instance.LanguageModelMgr;
			this.\u0001 = signLocal;
			this.\u0001 = precomPool;
			this.\u0002 = precomLocal;
			this.\u0001 = this.\u0002.ApplicationGuid;
			this.RootApplicationGuid = this.\u0001;
			this.\u0001 = ((precomLocal != null) ? precomLocal._GetLibraryTable(this.\u0001) : null);
			_ILibraryTable u = this.\u0001;
			if (this.\u0001 != null && this.\u0001.ParentObjectGuid != Guid.Empty)
			{
				this.\u0002 = this.\u0001;
				if (precomLocal != null)
				{
					this.\u0001 = precomLocal[this.\u0001.ParentObjectGuid];
				}
				if (this.\u0001 == null && precomPool != null)
				{
					this.\u0001 = precomPool[this.\u0002.ParentObjectGuid];
				}
			}
			this.\u0001();
			if (precomLocal.ApplicationGuid != Guid.Empty)
			{
				this.\u0003 = precomLocal;
				return;
			}
			if (CheckerScope.ActiveApplicationGuid != Guid.Empty)
			{
				this.\u0003 = (APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(CheckerScope.ActiveApplicationGuid) as _IPreCompileContext);
			}
		}

		// Token: 0x06001629 RID: 5673 RVA: 0x00041F20 File Offset: 0x00040120
		public CheckerScope(Guid applicationGuid, _ISignature signLocal, _IPreCompileContext precomLocal, _IPreCompileContext precomPool)
		{
			this.\u0001 = APEnvironmentFacade.Instance.LanguageModelMgr;
			this.\u0001 = signLocal;
			this.\u0001 = precomPool;
			this.\u0002 = precomLocal;
			this.\u0001 = applicationGuid;
			this.RootApplicationGuid = applicationGuid;
			this.\u0001 = precomLocal._GetLibraryTable(this.\u0001);
			_ILibraryTable u = this.\u0001;
			if (this.\u0001 != null && this.\u0001.ParentObjectGuid != Guid.Empty)
			{
				this.\u0002 = this.\u0001;
				if (precomLocal != null)
				{
					this.\u0001 = precomLocal[this.\u0001.ParentObjectGuid];
				}
				if (this.\u0001 == null)
				{
					this.\u0001 = precomPool[this.\u0002.ParentObjectGuid];
				}
			}
			this.\u0001();
			if (precomLocal.ApplicationGuid != Guid.Empty)
			{
				this.\u0003 = precomLocal;
				return;
			}
			if (CheckerScope.ActiveApplicationGuid != Guid.Empty)
			{
				this.\u0003 = (APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(CheckerScope.ActiveApplicationGuid) as _IPreCompileContext);
			}
		}

		// Token: 0x0600162A RID: 5674 RVA: 0x00042044 File Offset: 0x00040244
		internal CheckerScope(int nPointerSize, _ILibraryTable libraryTable, _ISignature signLocal, _IPreCompileContext precomLocal, _IPreCompileContext precomPool)
		{
			this.\u0001 = APEnvironmentFacade.Instance.LanguageModelMgr;
			this.\u0001 = signLocal;
			this.\u0001 = precomPool;
			this.\u0002 = precomLocal;
			this.\u0001 = this.\u0002.ApplicationGuid;
			this.RootApplicationGuid = this.\u0001;
			if (libraryTable != null)
			{
				this.\u0001 = libraryTable;
			}
			else if (precomLocal != null)
			{
				this.\u0001 = precomLocal._GetLibraryTable(this.\u0001);
			}
			if (this.\u0001 != null && this.\u0001.ParentObjectGuid != Guid.Empty)
			{
				this.\u0002 = this.\u0001;
				if (precomLocal != null)
				{
					this.\u0001 = precomLocal[this.\u0002.ParentObjectGuid];
				}
				if (this.\u0001 == null && precomPool != null)
				{
					this.\u0001 = precomPool[this.\u0002.ParentObjectGuid];
				}
			}
			this.PointerSize = nPointerSize;
			if (precomLocal != null && precomLocal.ApplicationGuid != Guid.Empty)
			{
				this.\u0003 = precomLocal;
				return;
			}
			if (CheckerScope.ActiveApplicationGuid != Guid.Empty)
			{
				this.\u0003 = (APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(CheckerScope.ActiveApplicationGuid) as _IPreCompileContext);
			}
		}

		// Token: 0x0600162B RID: 5675 RVA: 0x00042190 File Offset: 0x00040390
		internal static global::\u0015.\u0002 \u0001(int \u0002, _ILibraryTable \u0003, _ISignature \u0004, _IPreCompileContext \u0005, _IPreCompileContext \u0006)
		{
			if (\u0005 != null && string.IsNullOrEmpty(\u0005.LibraryPath) && \u0005.ApplicationGuid == Guid.Empty)
			{
				return \u0081.\u0007.\u0001(\u0004, \u0002);
			}
			return new CheckerScope(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x17000532 RID: 1330
		// (get) Token: 0x0600162C RID: 5676 RVA: 0x000421C8 File Offset: 0x000403C8
		internal _IPreCompileContext LocalContext
		{
			get
			{
				return this.\u0002;
			}
		}

		// Token: 0x17000533 RID: 1331
		// (get) Token: 0x0600162D RID: 5677 RVA: 0x000421D0 File Offset: 0x000403D0
		// (set) Token: 0x0600162E RID: 5678 RVA: 0x000421D8 File Offset: 0x000403D8
		public bool IgnoreImplicitEnumMembers { get; set; }

		// Token: 0x17000534 RID: 1332
		// (get) Token: 0x0600162F RID: 5679 RVA: 0x000421E4 File Offset: 0x000403E4
		// (set) Token: 0x06001630 RID: 5680 RVA: 0x000421EC File Offset: 0x000403EC
		public bool IgnoreActions { get; set; }

		// Token: 0x06001631 RID: 5681 RVA: 0x000421F8 File Offset: 0x000403F8
		internal int \u0001()
		{
			if (this.\u0002 == null)
			{
				return -1;
			}
			return this.\u0001.LibList.GetProjectHandle(this.\u0002.LibraryId);
		}

		// Token: 0x17000535 RID: 1333
		// (get) Token: 0x06001632 RID: 5682 RVA: 0x00042220 File Offset: 0x00040420
		private static Guid ActiveApplicationGuid
		{
			get
			{
				return APEnvironmentFacade.Instance.ActiveApplicationGuid;
			}
		}

		// Token: 0x06001633 RID: 5683 RVA: 0x0004222C File Offset: 0x0004042C
		public void \u0001()
		{
			this.PointerSize = 4;
			Guid guid = Guid.Empty;
			if (this.\u0002 != null && this.\u0002.ApplicationGuid != Guid.Empty)
			{
				guid = this.\u0002.ApplicationGuid;
			}
			else if (this.RootApplicationGuid != Guid.Empty)
			{
				guid = this.RootApplicationGuid;
			}
			else
			{
				guid = CheckerScope.ActiveApplicationGuid;
			}
			if (guid != Guid.Empty)
			{
				ICodegenerator codegenerator = CompilerServicesInternal.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(guid), guid, false, false);
				if (codegenerator is ICodegenerator3 && (codegenerator as ICodegenerator3).GetProperty(CodegeneratorProperties.LWordPointer))
				{
					this.PointerSize = 8;
				}
			}
		}

		// Token: 0x06001634 RID: 5684 RVA: 0x000422DC File Offset: 0x000404DC
		public global::\u0015.\u0002 \u0001(IExpression \u0002)
		{
			ISignature[] array = this.\u0001(\u0002);
			_ISignature isignature = null;
			if (array != null && array.Length != 0)
			{
				isignature = (array[0] as _ISignature);
			}
			if (isignature != null && isignature.GetFlag(SignatureFlag.Alias))
			{
				IVariable variable = null;
				if (isignature.AllVariables.Count<_IVariable>() == 1)
				{
					variable = isignature.AllVariables[0];
				}
				if (variable != null && variable.Type.Class == TypeClass.Userdef)
				{
					CheckerScope checkerScope = this;
					if (isignature.IsLibraryObject)
					{
						_IPreCompileContext ipreCompileContext = (_IPreCompileContext)APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(isignature);
						_ILibraryTable libraryTable = ipreCompileContext._GetLibraryTable(this.ApplicationGuid);
						checkerScope = new CheckerScope(this.PointerSize, libraryTable, isignature, ipreCompileContext, this.\u0001);
					}
					isignature = (checkerScope.\u0001(variable.Type as IUserdefType) as _ISignature);
				}
			}
			return this.\u0001(isignature);
		}

		// Token: 0x06001635 RID: 5685 RVA: 0x000423AC File Offset: 0x000405AC
		public global::\u0015.\u0002 \u0001(_ISignature \u0002)
		{
			_IPreCompileContext ipreCompileContext = this.\u0002;
			_IPreCompileContext precomPool = this.\u0001;
			_IPreCompileContext u = null;
			if (\u0002 != null && !string.IsNullOrEmpty(\u0002.LibraryPath))
			{
				ipreCompileContext = this.\u0001.GetLibraryContext(\u0002.LibraryPath);
				if (this.\u0003 != null)
				{
					u = this.\u0003;
				}
				else if (this.\u0002.ApplicationGuid != Guid.Empty)
				{
					u = this.\u0002;
				}
				precomPool = null;
				if (ipreCompileContext == null)
				{
					ipreCompileContext = this.\u0002;
				}
			}
			return new CheckerScope(this.PointerSize, this.\u0001, \u0002, ipreCompileContext, precomPool)
			{
				RootApplicationGuid = this.RootApplicationGuid,
				\u0003 = u
			};
		}

		// Token: 0x06001636 RID: 5686 RVA: 0x00042450 File Offset: 0x00040650
		public global::\u0015.\u0002 \u0001(_IPreCompileContext \u0002)
		{
			global::\u0015.\u0002 u = CheckerScope.\u0001(this.PointerSize, this.\u0001, null, \u0002, null);
			CheckerScope checkerScope = u as CheckerScope;
			if (checkerScope != null)
			{
				checkerScope.RootApplicationGuid = this.RootApplicationGuid;
			}
			return u;
		}

		// Token: 0x06001637 RID: 5687 RVA: 0x00042488 File Offset: 0x00040688
		public global::\u0015.\u0002 \u0002(_ISignature \u0002)
		{
			_IPreCompileContext ipreCompileContext = this.\u0001(\u0002);
			if (ipreCompileContext == null)
			{
				return null;
			}
			return new CheckerScope(this.PointerSize, this.\u0001, null, ipreCompileContext, null)
			{
				RootApplicationGuid = this.RootApplicationGuid
			};
		}

		// Token: 0x06001638 RID: 5688 RVA: 0x000424C4 File Offset: 0x000406C4
		public global::\u0015.\u0002 \u0001()
		{
			return new CheckerScope(this.PointerSize, this.\u0001, null, this.\u0002, this.\u0001);
		}

		// Token: 0x06001639 RID: 5689 RVA: 0x000424E4 File Offset: 0x000406E4
		private global::\u0015.\u0002 \u0003(_ISignature \u0002)
		{
			return new CheckerScope(this.PointerSize, this.\u0001, \u0002, this.\u0002, this.\u0001);
		}

		// Token: 0x17000536 RID: 1334
		// (get) Token: 0x0600163A RID: 5690 RVA: 0x00042504 File Offset: 0x00040704
		// (set) Token: 0x0600163B RID: 5691 RVA: 0x0004250C File Offset: 0x0004070C
		public int PointerSize { get; private set; } = 4;

		// Token: 0x0600163C RID: 5692 RVA: 0x00042518 File Offset: 0x00040718
		public int \u0001(IType \u0002)
		{
			int size = TypeTable.GetSize2(\u0002.Class, this);
			if (size == -1)
			{
				return this.\u0002.CalculateTypeSize(this.\u0001, \u0002);
			}
			return size;
		}

		// Token: 0x0600163D RID: 5693 RVA: 0x0004254C File Offset: 0x0004074C
		public int \u0001(IType \u0002, IRecursionGuard \u0003)
		{
			int size = TypeTable.GetSize2(\u0002.Class, this);
			if (size == -1)
			{
				return ((_IPreCompileContext2)this.\u0002).CalculateTypeSize(this.\u0001, \u0002, \u0003);
			}
			return size;
		}

		// Token: 0x0600163E RID: 5694 RVA: 0x00042584 File Offset: 0x00040784
		public bool \u0001(_IExpression \u0002)
		{
			_IVariable ivariable = this.\u0001(\u0002);
			return (ivariable != null && ivariable.HasFlag(VarFlag.ReplacedConstant | VarFlag.Constant)) || global::\u0013.\u0004.\u0001((_IExprement3)\u0002, true);
		}

		// Token: 0x0600163F RID: 5695 RVA: 0x000425B8 File Offset: 0x000407B8
		public bool \u0002(_IExpression \u0002)
		{
			_IVariable ivariable = this.\u0001(\u0002);
			if (ivariable != null)
			{
				return ivariable.HasFlag(VarFlag.ReplacedConstant);
			}
			return global::\u0013.\u0004.\u0001((_IExprement3)\u0002, false);
		}

		// Token: 0x06001640 RID: 5696 RVA: 0x000425E8 File Offset: 0x000407E8
		public _IVariable \u0001(_IExpression \u0002)
		{
			return (_IVariable)\u0002.GetVariable(this);
		}

		// Token: 0x06001641 RID: 5697 RVA: 0x000425F8 File Offset: 0x000407F8
		public _ISignature \u0001(_IExpression \u0002)
		{
			if (\u0002.PrecompileSignatureId != Helper.InvalidId)
			{
				return (_ISignature)APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(\u0002.PrecompileSignatureId);
			}
			_IVariableExpression ivariableExpression = \u0002 as _IVariableExpression;
			if (ivariableExpression != null)
			{
				IVariable[] array;
				ISignature[] array2;
				global::\u0015.\u0002 u;
				this.\u0001(ivariableExpression.Name, out array, out array2, out u);
				if (array2.Length == 1)
				{
					return (_ISignature)array2[0];
				}
			}
			return null;
		}

		// Token: 0x06001642 RID: 5698 RVA: 0x0004265C File Offset: 0x0004085C
		public static _IExpression \u0001(_IPrecompileScope3 \u0002, _ISignature \u0003, _IVariable \u0004)
		{
			_IExpression iexpression = null;
			Guid rootApplicationGuid = \u0002.RootApplicationGuid;
			if (rootApplicationGuid != Guid.Empty && \u0003 != null && !string.IsNullOrEmpty(\u0003.LibraryPath) && \u0003.HasAttribute(CompileAttributes.ATTRIBUTE_PARAMETERLIST))
			{
				ICaseInsensitiveDictionary<IExpression> caseInsensitiveDictionary = ((_IPreCompileContext)APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(rootApplicationGuid)).ParameterTable(\u0003.LibraryPath);
				if (caseInsensitiveDictionary != null && caseInsensitiveDictionary.ContainsKey(\u0004.Name))
				{
					iexpression = (_IExpression)caseInsensitiveDictionary[\u0004.Name];
				}
			}
			if (iexpression == null)
			{
				iexpression = \u0004._Initial;
			}
			return iexpression;
		}

		// Token: 0x06001643 RID: 5699 RVA: 0x000426EC File Offset: 0x000408EC
		public _IExpression \u0001(_ISignature \u0002, _IVariable \u0003)
		{
			return CheckerScope.\u0001(this, \u0002, \u0003);
		}

		// Token: 0x06001644 RID: 5700 RVA: 0x000426F8 File Offset: 0x000408F8
		public global::\u0017.\u0006 \u0001(_IExpression \u0002, _IUserdefType \u0003)
		{
			_ISignature isignature = (_ISignature)this.\u0001(\u0003);
			if (isignature != null || \u0002.PrecompileSignatureId == Helper.InvalidId)
			{
				return (global::\u0017.\u0006)this.\u0001(isignature);
			}
			_ISignature isignature2 = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(\u0002.PrecompileSignatureId) as _ISignature;
			if (isignature2 == null)
			{
				return this;
			}
			isignature = (_ISignature)this.\u0001(isignature2).FindSignature(\u0003);
			if (isignature != null)
			{
				return (global::\u0017.\u0006)this.\u0001(isignature);
			}
			return this;
		}

		// Token: 0x06001645 RID: 5701 RVA: 0x00042774 File Offset: 0x00040974
		public global::\u0017.\u0006 \u0001(_ISignature \u0002)
		{
			return (global::\u0017.\u0006)this.\u0001(\u0002);
		}

		// Token: 0x17000537 RID: 1335
		// (get) Token: 0x06001646 RID: 5702 RVA: 0x00042784 File Offset: 0x00040984
		public bool ContainsCopyCode
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06001647 RID: 5703 RVA: 0x00042788 File Offset: 0x00040988
		public void \u0001(_IExpression \u0002, out _IVariable \u0003, out _ISignature \u0004, out global::\u0017.\u0006 \u0005)
		{
			IVariable variable;
			ISignature signature;
			IPrecompileScope precompileScope;
			this.\u0001(\u0002.ToString(), out variable, out signature, out precompileScope);
			\u0005 = (global::\u0017.\u0006)precompileScope;
			\u0003 = (_IVariable)variable;
			\u0004 = (_ISignature)signature;
		}

		// Token: 0x06001648 RID: 5704 RVA: 0x000427C4 File Offset: 0x000409C4
		public ISignature \u0001(IUserdefType \u0002)
		{
			_IUserdefType iuserdefType = \u0002 as _IUserdefType;
			if (iuserdefType == null)
			{
				return null;
			}
			ISignature[] array = this.\u0001(iuserdefType.NameExpression);
			if (array != null && array.Count<ISignature>() == 1)
			{
				return array[0] as _ISignature;
			}
			return null;
		}

		// Token: 0x06001649 RID: 5705 RVA: 0x00042800 File Offset: 0x00040A00
		public ISignature \u0001(IEnumType \u0002)
		{
			if (\u0002 == null)
			{
				return null;
			}
			_IEnumType ienumType = \u0002 as _IEnumType;
			if (ienumType != null && ienumType.SignatureId != Helper.InvalidId)
			{
				ISignature signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(ienumType.SignatureId);
				if (signatureForPrecompileID != null)
				{
					return signatureForPrecompileID;
				}
			}
			ISignature[] array = this.\u0001(\u0002.Name);
			if (array.Count<ISignature>() == 1)
			{
				return array[0] as _ISignature;
			}
			return null;
		}

		// Token: 0x0600164A RID: 5706 RVA: 0x00042864 File Offset: 0x00040A64
		public ILiteralValue \u0001(IExpression \u0002, bool \u0003)
		{
			bool flag;
			return (\u0002 as _IExpression).LiteralWithRecursionCheck(this, new LDictionary<IVariable, IVariable>(), \u0003, out flag);
		}

		// Token: 0x0600164B RID: 5707 RVA: 0x00042888 File Offset: 0x00040A88
		public bool \u0001(IUserdefType \u0002, IUserdefType \u0003)
		{
			_IUserdefType iuserdefType = \u0002 as _IUserdefType;
			_IUserdefType iuserdefType2 = \u0003 as _IUserdefType;
			if (iuserdefType.SignatureId != -1 && iuserdefType2.SignatureId != -1)
			{
				return iuserdefType.SignatureId == iuserdefType2.SignatureId;
			}
			string[] source = iuserdefType.NameExpression.ToString().Split(new char[]
			{
				'.'
			});
			string[] source2 = iuserdefType2.NameExpression.ToString().Split(new char[]
			{
				'.'
			});
			return source.Last<string>().Equals(source2.Last<string>(), StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x0600164C RID: 5708 RVA: 0x00042910 File Offset: 0x00040B10
		public bool \u0001(ISignature \u0002, ISignature \u0003, ICommonScope \u0004)
		{
			_ISignature isignature = \u0002 as _ISignature;
			_ISignature isignature2 = \u0003 as _ISignature;
			if (isignature == null || isignature2 == null)
			{
				return false;
			}
			if (isignature.ObjectGuid == isignature2.ObjectGuid)
			{
				return true;
			}
			if (isignature.BaseExpression != null)
			{
				_ISignature isignature3 = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(isignature._BaseSignature.PrecompileSignatureId) as _ISignature;
				return isignature3 == null || this.\u0001(isignature3, \u0003, \u0004);
			}
			if (isignature2.POUType == Operator.Interface)
			{
				using (IEnumerator<_IExpression> enumerator = isignature.InterfaceExpressions.OfType<_IExpression>().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.PrecompileSignatureId == isignature2.PrecompileId)
						{
							return true;
						}
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x17000538 RID: 1336
		// (get) Token: 0x0600164D RID: 5709 RVA: 0x000429DC File Offset: 0x00040BDC
		// (set) Token: 0x0600164E RID: 5710 RVA: 0x000429E4 File Offset: 0x00040BE4
		public bool LocalScope { get; set; }

		// Token: 0x0600164F RID: 5711 RVA: 0x000429F0 File Offset: 0x00040BF0
		public ISignature[] \u0001(IExpression \u0002)
		{
			IVariableExpression variableExpression = \u0002 as IVariableExpression;
			if (variableExpression != null)
			{
				return this.\u0001(variableExpression.Name);
			}
			_ICompoAccessExpression icompoAccessExpression = \u0002 as _ICompoAccessExpression;
			if (icompoAccessExpression == null)
			{
				_ISystemScopeExpression isystemScopeExpression = \u0002 as _ISystemScopeExpression;
				if (isystemScopeExpression != null)
				{
					return ((ICommonScope)new CheckerScope(null, this.\u0001._SystemContext, null)).FindSignature(isystemScopeExpression.Base);
				}
				_IPoolScopeExpression ipoolScopeExpression = \u0002 as _IPoolScopeExpression;
				if (ipoolScopeExpression != null)
				{
					return ((ICommonScope)new CheckerScope(null, this.\u0001.Pool, null)).FindSignature(ipoolScopeExpression.Base);
				}
				_INamespaceAccessExpression inamespaceAccessExpression = \u0002 as _INamespaceAccessExpression;
				if (inamespaceAccessExpression == null)
				{
					_ITypeExpression itypeExpression = \u0002 as _ITypeExpression;
					if (itypeExpression == null)
					{
						return null;
					}
					_IUserdefType iuserdefType = itypeExpression._CompiledType as _IUserdefType;
					if (iuserdefType != null)
					{
						return new ISignature[]
						{
							this.\u0001(iuserdefType)
						};
					}
					return null;
				}
				else
				{
					global::\u0015.\u0002 u = this.\u0002(inamespaceAccessExpression._Namespace);
					if (u != null)
					{
						return u.FindSignature(inamespaceAccessExpression._Access);
					}
					return null;
				}
			}
			else
			{
				global::\u0015.\u0002 u2 = this.\u0002(icompoAccessExpression.Left);
				if (u2 != null)
				{
					return u2.FindSignature(icompoAccessExpression.Right);
				}
				return null;
			}
		}

		// Token: 0x06001650 RID: 5712 RVA: 0x00042AFC File Offset: 0x00040CFC
		public global::\u0015.\u0002 \u0001(string \u0002)
		{
			_IPreCompileContext libraryContextByNamespace;
			if (string.IsNullOrEmpty(this.\u0002.LibraryPath))
			{
				libraryContextByNamespace = this.\u0001.GetLibraryContextByNamespace(\u0002);
			}
			else
			{
				libraryContextByNamespace = this.\u0001.GetLibraryContextByNamespace(\u0002, this.\u0002);
			}
			if (libraryContextByNamespace == null)
			{
				return null;
			}
			return this.\u0001(libraryContextByNamespace);
		}

		// Token: 0x06001651 RID: 5713 RVA: 0x00042B4C File Offset: 0x00040D4C
		public IPrecompileScope \u0001(IExpression \u0002)
		{
			return this.\u0002(\u0002);
		}

		// Token: 0x06001652 RID: 5714 RVA: 0x00042B58 File Offset: 0x00040D58
		public global::\u0015.\u0002 \u0002(IExpression \u0002)
		{
			if (\u0002 is IVariableExpression)
			{
				string text = \u0002.ToString();
				_IPreCompileContext ipreCompileContext;
				if (string.IsNullOrEmpty(this.\u0002.LibraryPath))
				{
					ipreCompileContext = this.\u0001.GetLibraryContextByNamespace(text);
				}
				else
				{
					ipreCompileContext = this.\u0001.GetLibraryContextByNamespace(text, this.\u0002);
				}
				if (this.\u0002.Namespace != null && ipreCompileContext == null && text.Equals(this.\u0002.Namespace, StringComparison.InvariantCultureIgnoreCase))
				{
					ipreCompileContext = this.\u0002;
				}
				if (ipreCompileContext == null)
				{
					return null;
				}
				return this.\u0001(ipreCompileContext);
			}
			else if (\u0002 is ICompoAccessExpression)
			{
				ICompoAccessExpression compoAccessExpression = \u0002 as ICompoAccessExpression;
				global::\u0015.\u0002 u = this.\u0002(compoAccessExpression.Left);
				if (u == null)
				{
					return null;
				}
				return u.\u0002(compoAccessExpression.Right);
			}
			else
			{
				if (!(\u0002 is _INamespaceAccessExpression))
				{
					return null;
				}
				_INamespaceAccessExpression inamespaceAccessExpression = \u0002 as _INamespaceAccessExpression;
				global::\u0015.\u0002 u2 = this.\u0002(inamespaceAccessExpression._Namespace);
				if (u2 == null)
				{
					return null;
				}
				return u2.\u0002(inamespaceAccessExpression._Access);
			}
		}

		// Token: 0x17000539 RID: 1337
		public ISignature this[Guid \u0002]
		{
			get
			{
				return this.\u0002[\u0002];
			}
		}

		// Token: 0x06001654 RID: 5716 RVA: 0x00042C50 File Offset: 0x00040E50
		private IVariable \u0001(string \u0002, _ISignature \u0003, out ISignature \u0004)
		{
			\u0004 = null;
			if (this.IgnoreImplicitEnumMembers)
			{
				return null;
			}
			_IPreCompileContext ipreCompileContext = this.\u0001(\u0003);
			foreach (_IVariable ivariable in \u0003.AllVariables)
			{
				if (ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_IMPLICIT_ENUM_TYPE))
				{
					string stName = string.Empty;
					_IType itype = null;
					if (ivariable._Type is _IArrayType)
					{
						itype = (ivariable._Type as _IArrayType)._Base;
					}
					else if (ivariable._Type is _IEnumType)
					{
						itype = ivariable._Type;
					}
					else if (ivariable._Type is _IUserdefType)
					{
						itype = ivariable._Type;
					}
					if (itype != null)
					{
						stName = itype.ToString();
						_ISignature isignature = ipreCompileContext.GetSignature(stName) as _ISignature;
						if (isignature != null)
						{
							IVariable variable = isignature[\u0002];
							if (variable != null)
							{
								\u0004 = isignature;
								return variable;
							}
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06001655 RID: 5717 RVA: 0x00042D54 File Offset: 0x00040F54
		private Tuple<ISignature, IVariable> \u0001(IExpression \u0002, string \u0003, global::\u0015.\u0002 \u0004, LHashSet<int> \u0005)
		{
			ISignature[] array = \u0004.FindSignature(\u0002);
			if (array != null && array.Count<ISignature>() == 1)
			{
				_ISignature isignature = array[0] as _ISignature;
				_IPreCompileContext ipreCompileContext = this.\u0001(isignature);
				global::\u0015.\u0002 u;
				if (isignature.IsLibraryObject)
				{
					u = this.\u0001(ipreCompileContext);
				}
				else
				{
					u = CheckerScope.\u0001(this.PointerSize, this.\u0001, null, ipreCompileContext, this.\u0001);
				}
				Tuple<ISignature, IVariable> tuple = this.\u0001(\u0003, u, isignature, \u0005);
				if (tuple != null)
				{
					return tuple;
				}
			}
			return null;
		}

		// Token: 0x06001656 RID: 5718 RVA: 0x00042DCC File Offset: 0x00040FCC
		private IVariable \u0001(string \u0002, _ISignature \u0003)
		{
			IVariable variable = \u0003[\u0002];
			if (variable != null)
			{
				return variable;
			}
			IEnumerable<ISignature> enumerable = ((ILMPreCompileSet)this.\u0002).FindSubSignatureSet(\u0003.Name);
			if (enumerable == null)
			{
				return null;
			}
			string b = ("__GET" + \u0002).ToUpperInvariant();
			string b2 = ("__SET" + \u0002).ToUpperInvariant();
			ISignature signature = null;
			ISignature signature2 = null;
			foreach (ISignature signature3 in enumerable.Where(new Func<ISignature, bool>(CheckerScope.<>c.<>9.\u0001)))
			{
				if (signature3.Name == b2)
				{
					signature2 = signature3;
				}
				else if (signature3.Name == b)
				{
					signature = signature3;
				}
				if (signature2 != null && signature != null)
				{
					break;
				}
			}
			FBImplicitPropertyVariableGenerator.PropertyInfo propertyInfo = FBImplicitPropertyVariableGenerator.PropertyInfo.Create(\u0002, signature, signature2);
			if (propertyInfo == null)
			{
				return null;
			}
			_IVariable ivariable = FBImplicitPropertyVariableGenerator.CreatePropertyVariable(\u0003, propertyInfo);
			\u0003.AddVariable(ivariable);
			return ivariable;
		}

		// Token: 0x06001657 RID: 5719 RVA: 0x00042EE4 File Offset: 0x000410E4
		private Tuple<ISignature, IVariable> \u0001(string \u0002, global::\u0015.\u0002 \u0003, _ISignature \u0004, LHashSet<int> \u0005)
		{
			if (\u0005.Contains(\u0004.PrecompileId))
			{
				return null;
			}
			\u0005.Add(\u0004.PrecompileId);
			IVariable variable = this.\u0001(\u0002, \u0004);
			if (variable == null)
			{
				if (\u0004.HasAttribute("contains_implicit_enum"))
				{
					ISignature item;
					variable = this.\u0001(\u0002, \u0004, out item);
					if (variable != null)
					{
						return new Tuple<ISignature, IVariable>(item, variable);
					}
				}
				if (\u0004.BaseExpression != null)
				{
					Tuple<ISignature, IVariable> tuple = this.\u0001(\u0004.BaseExpression, \u0002, \u0003, \u0005);
					if (tuple != null)
					{
						return tuple;
					}
				}
				if (\u0004.POUType == Operator.Interface && \u0004.InterfaceExpressions != null)
				{
					foreach (IExpression u in \u0004.InterfaceExpressions)
					{
						Tuple<ISignature, IVariable> tuple2 = this.\u0001(u, \u0002, \u0003, \u0005);
						if (tuple2 != null)
						{
							return tuple2;
						}
					}
				}
				if (\u0004.GetFlag(SignatureFlag.Alias))
				{
					global::\u0015.\u0002 u2;
					\u0004 = \u0003.\u0001(\u0004, out u2);
					if (\u0004 != null)
					{
						Tuple<ISignature, IVariable> tuple3 = this.\u0001(\u0002, \u0003, \u0004, \u0005);
						if (tuple3 != null)
						{
							return tuple3;
						}
					}
				}
				return null;
			}
			if (this.\u0002 != null && this.\u0002.POUType != Operator.Action && variable.GetFlag(VarFlag.Temp))
			{
				return null;
			}
			return new Tuple<ISignature, IVariable>(\u0004, variable);
		}

		// Token: 0x06001658 RID: 5720 RVA: 0x00043000 File Offset: 0x00041200
		public IVariable \u0001(string \u0002, out ISignature \u0003)
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
				if (this.\u0002.HasAttribute("contains_implicit_enum"))
				{
					variable = this.\u0001(\u0002, this.\u0002, out \u0003);
					if (variable != null)
					{
						return variable;
					}
				}
			}
			if (this.\u0001 != null)
			{
				Tuple<ISignature, IVariable> tuple = this.\u0001(\u0002, this, this.\u0001, new LHashSet<int>());
				if (tuple != null)
				{
					\u0003 = tuple.Item1;
					return tuple.Item2;
				}
			}
			return null;
		}

		// Token: 0x06001659 RID: 5721 RVA: 0x00043088 File Offset: 0x00041288
		public IEnumerable<IVariable> \u0001(string \u0002, out ISignature[] \u0003)
		{
			LList<ISignature> llist = new LList<ISignature>();
			LList<IVariable> llist2 = new LList<IVariable>();
			\u0003 = null;
			this.\u0001(\u0002, llist, llist2);
			this.\u0002(\u0002, llist, llist2);
			this.\u0003(\u0002, llist, llist2);
			this.\u0004(\u0002, llist, llist2);
			if (llist2.Count > 0)
			{
				\u0003 = new ISignature[llist.Count];
				llist.CopyTo(\u0003);
				return llist2;
			}
			return null;
		}

		// Token: 0x0600165A RID: 5722 RVA: 0x000430EC File Offset: 0x000412EC
		private void \u0001(string \u0002, LList<ISignature> \u0003, LList<IVariable> \u0004)
		{
			Guid parentApplicationGuid;
			for (_IPreCompileContext ipreCompileContext = this.\u0002; ipreCompileContext != null; ipreCompileContext = (APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(parentApplicationGuid) as _IPreCompileContext))
			{
				int[] signsForGlobalVar = ipreCompileContext.GetSignsForGlobalVar(\u0002);
				if (signsForGlobalVar.Length != 0)
				{
					CheckerScope.\u0001(\u0002, \u0003, \u0004, signsForGlobalVar);
				}
				if (\u0004.Count > 0 || ipreCompileContext.ApplicationGuid == Guid.Empty)
				{
					break;
				}
				parentApplicationGuid = APEnvironmentFacade.Instance.LMServiceProvider.LanguageModelProviderService.GetParentApplicationGuid(ipreCompileContext.ApplicationGuid);
				if (!(parentApplicationGuid != Guid.Empty))
				{
					break;
				}
			}
		}

		// Token: 0x0600165B RID: 5723 RVA: 0x00043174 File Offset: 0x00041374
		private static void \u0001(string \u0002, LList<ISignature> \u0003, LList<IVariable> \u0004, int[] \u0005)
		{
			foreach (int precompileId in \u0005)
			{
				_ISignature isignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(precompileId) as _ISignature;
				if (!isignature.HasAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY))
				{
					IVariable variable = isignature[\u0002];
					if (variable != null)
					{
						\u0003.Add(isignature);
						\u0004.Add(variable);
					}
				}
			}
		}

		// Token: 0x0600165C RID: 5724 RVA: 0x000431D4 File Offset: 0x000413D4
		private void \u0002(string \u0002, LList<ISignature> \u0003, LList<IVariable> \u0004)
		{
			if (\u0004.Count == 0)
			{
				foreach (_IPreCompileContext2 ipreCompileContext in this.\u0001.GetVisibleLibraries(this.\u0002).OfType<_IPreCompileContext2>())
				{
					if (!this.\u0001.GetQualifiedOnly(this.\u0002, ipreCompileContext.LibraryPath))
					{
						IEnumerable<int> signsForGlobalVar = ipreCompileContext.GetSignsForGlobalVar(\u0002);
						if (signsForGlobalVar.Any<int>())
						{
							CheckerScope.\u0001(\u0002, \u0003, \u0004, signsForGlobalVar);
						}
					}
				}
			}
		}

		// Token: 0x0600165D RID: 5725 RVA: 0x00043264 File Offset: 0x00041464
		private static void \u0001(string \u0002, LList<ISignature> \u0003, LList<IVariable> \u0004, IEnumerable<int> \u0005)
		{
			foreach (int precompileId in \u0005)
			{
				_ISignature isignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(precompileId) as _ISignature;
				if (!isignature.HasAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY) && (!isignature.IsLibraryObject || !isignature.GetFlag(SignatureFlag.Internal)))
				{
					IVariable variable = isignature[\u0002];
					if (variable != null)
					{
						\u0003.Add(isignature);
						\u0004.Add(variable);
					}
				}
			}
		}

		// Token: 0x0600165E RID: 5726 RVA: 0x000432FC File Offset: 0x000414FC
		private void \u0003(string \u0002, LList<ISignature> \u0003, LList<IVariable> \u0004)
		{
			if (this.\u0001 != null && \u0004.Count == 0)
			{
				IEnumerable<int> signsForGlobalVar = this.\u0001.GetSignsForGlobalVar(\u0002);
				if (signsForGlobalVar.Any<int>())
				{
					foreach (int precompileId in signsForGlobalVar)
					{
						_ISignature isignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(precompileId) as _ISignature;
						if (!isignature.HasAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY))
						{
							IVariable variable = isignature[\u0002];
							if (variable != null)
							{
								\u0003.Add(isignature);
								\u0004.Add(variable);
							}
						}
					}
				}
			}
		}

		// Token: 0x0600165F RID: 5727 RVA: 0x000433A4 File Offset: 0x000415A4
		private void \u0004(string \u0002, LList<ISignature> \u0003, LList<IVariable> \u0004)
		{
			if (this.\u0001 != null && \u0004.Count == 0)
			{
				foreach (_IPreCompileContext ipreCompileContext in this.\u0001.GetVisibleLibraries(this.\u0001))
				{
					if (!this.\u0001.GetQualifiedOnly(this.\u0001, ipreCompileContext.LibraryPath))
					{
						IEnumerable<int> signsForGlobalVar = ipreCompileContext.GetSignsForGlobalVar(\u0002);
						if (signsForGlobalVar.Any<int>())
						{
							CheckerScope.\u0002(\u0002, \u0003, \u0004, signsForGlobalVar);
						}
					}
				}
			}
		}

		// Token: 0x06001660 RID: 5728 RVA: 0x00043438 File Offset: 0x00041638
		private static void \u0002(string \u0002, LList<ISignature> \u0003, LList<IVariable> \u0004, IEnumerable<int> \u0005)
		{
			foreach (int precompileId in \u0005)
			{
				_ISignature isignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(precompileId) as _ISignature;
				if (!isignature.HasAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY))
				{
					IVariable variable = isignature[\u0002];
					if (variable != null)
					{
						\u0003.Add(isignature);
						\u0004.Add(variable);
					}
				}
			}
		}

		// Token: 0x06001661 RID: 5729 RVA: 0x000434B8 File Offset: 0x000416B8
		public ISignature[] \u0001(string \u0002)
		{
			return this[\u0002];
		}

		// Token: 0x1700053A RID: 1338
		public ISignature[] this[string \u0002]
		{
			get
			{
				if (this.\u0003 != null)
				{
					ISignature signature = this.\u0003[\u0002];
					if (signature != null && signature.GetFlag(SignatureFlag.SuperGlobal))
					{
						return new ISignature[]
						{
							signature
						};
					}
				}
				if (this.\u0001.Pool != null)
				{
					ISignature signature2 = this.\u0001.Pool[\u0002];
					if (signature2 != null && signature2.GetFlag(SignatureFlag.SuperGlobal))
					{
						return new ISignature[]
						{
							signature2
						};
					}
				}
				ISignature signature3 = this.\u0001(\u0002);
				if (signature3 != null)
				{
					return new ISignature[]
					{
						signature3
					};
				}
				ISignature signature4 = this.\u0002(\u0002);
				if (signature4 != null)
				{
					return new ISignature[]
					{
						signature4
					};
				}
				return Array.Empty<ISignature>();
			}
		}

		// Token: 0x06001663 RID: 5731 RVA: 0x00043570 File Offset: 0x00041770
		public ISignature \u0001(string \u0002)
		{
			string u = \u0002.ToUpperInvariant();
			if (this.\u0001 == null)
			{
				return null;
			}
			LHashSet<_ISignature> lhashSet = new LHashSet<_ISignature>();
			LStack<Tuple<global::\u0015.\u0002, _IPreCompileContext, _ISignature>> lstack = new LStack<Tuple<global::\u0015.\u0002, _IPreCompileContext, _ISignature>>();
			lstack.Push(new Tuple<global::\u0015.\u0002, _IPreCompileContext, _ISignature>(this, this.\u0002, this.\u0001));
			while (lstack.Count > 0)
			{
				Tuple<global::\u0015.\u0002, _IPreCompileContext, _ISignature> tuple = lstack.Pop();
				global::\u0015.\u0002 item = tuple.Item1;
				_IPreCompileContext item2 = tuple.Item2;
				_ISignature item3 = tuple.Item3;
				if (item2 != null && lhashSet.Add(item3))
				{
					ISignature result;
					if (this.\u0001(u, item2, item3, out result))
					{
						return result;
					}
					this.\u0001(lstack, item, item3);
					this.\u0001(lstack, item, item2, item3);
				}
			}
			return null;
		}

		// Token: 0x06001664 RID: 5732 RVA: 0x00043610 File Offset: 0x00041810
		private bool \u0001(string \u0002, _IPreCompileContext \u0003, _ISignature \u0004, out ISignature \u0005)
		{
			\u0005 = null;
			IEnumerable<ISignature> enumerable = ((ILMPreCompileSet)\u0003).FindSubSignatureSet(\u0004.Name);
			if (enumerable == null)
			{
				return false;
			}
			foreach (ISignature signature in enumerable)
			{
				if ((Operator.Action != signature.POUType || !this.IgnoreActions) && signature.Name == \u0002)
				{
					\u0005 = signature;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001665 RID: 5733 RVA: 0x00043698 File Offset: 0x00041898
		private void \u0001(LStack<Tuple<global::\u0015.\u0002, _IPreCompileContext, _ISignature>> \u0002, global::\u0015.\u0002 \u0003, _ISignature \u0004)
		{
			if (\u0004.BaseExpression == null)
			{
				return;
			}
			_ISignature isignature = \u0003.FindSignatureGlobal(\u0004.BaseExpression) as _ISignature;
			if (isignature != null)
			{
				_IPreCompileContext ipreCompileContext = this.\u0001(isignature);
				global::\u0015.\u0002 item;
				if (!string.IsNullOrEmpty(isignature.LibraryPath))
				{
					item = this.\u0001(ipreCompileContext);
				}
				else
				{
					item = this.\u0003(isignature);
				}
				\u0002.Push(new Tuple<global::\u0015.\u0002, _IPreCompileContext, _ISignature>(item, ipreCompileContext, isignature));
			}
		}

		// Token: 0x06001666 RID: 5734 RVA: 0x000436FC File Offset: 0x000418FC
		private void \u0001(LStack<Tuple<global::\u0015.\u0002, _IPreCompileContext, _ISignature>> \u0002, global::\u0015.\u0002 \u0003, _IPreCompileContext \u0004, _ISignature \u0005)
		{
			if (\u0005.POUType == Operator.Interface && \u0005.InterfaceExpressions != null)
			{
				foreach (IExpression qne in \u0005.InterfaceExpressions)
				{
					ISignature signature = \u0003.FindSignatureGlobal(qne);
					if (signature != null)
					{
						\u0002.Push(new Tuple<global::\u0015.\u0002, _IPreCompileContext, _ISignature>(this.\u0001(\u0004), this.\u0001((_ISignature)signature), signature as _ISignature));
					}
				}
			}
		}

		// Token: 0x06001667 RID: 5735 RVA: 0x00043768 File Offset: 0x00041968
		private _IPreCompileContext \u0001(_ISignature \u0002)
		{
			if (!string.IsNullOrEmpty(\u0002.LibraryPath))
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(\u0002.LibraryPath);
			}
			if (this.\u0002 != null && this.\u0002[\u0002.ObjectGuid] != null)
			{
				return this.\u0002;
			}
			if (APEnvironmentFacade.Instance.LanguageModelMgr.Pool[\u0002.ObjectGuid] != null)
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.Pool;
			}
			if (APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext[\u0002.ObjectGuid] != null)
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext;
			}
			if (\u0002.GetFlag(SignatureFlag.SuperGlobal))
			{
				_IPreCompileContext u = this.\u0003;
				if (((u != null) ? u[\u0002.ObjectGuid] : null) != null)
				{
					return this.\u0003;
				}
			}
			return null;
		}

		// Token: 0x06001668 RID: 5736 RVA: 0x00043844 File Offset: 0x00041A44
		public string \u0001(string \u0002)
		{
			if (this.\u0002 != null)
			{
				return this.\u0001.GetLocalLibraryNamespaceRecursive(this.\u0002, \u0002);
			}
			return null;
		}

		// Token: 0x06001669 RID: 5737 RVA: 0x00043864 File Offset: 0x00041A64
		public string \u0001()
		{
			_IPreCompileContext u = this.\u0002;
			if (u == null)
			{
				return null;
			}
			return u.LibraryId;
		}

		// Token: 0x0600166A RID: 5738 RVA: 0x00043878 File Offset: 0x00041A78
		public bool \u0001(string \u0002, out IVariable[] \u0003, out ISignature[] \u0004, out global::\u0015.\u0002 \u0005)
		{
			ISignature signature = null;
			\u0003 = null;
			\u0004 = null;
			\u0005 = null;
			IVariable variable = this.\u0001(\u0002, out signature);
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
				IEnumerable<IVariable> enumerable = this.\u0001(\u0002, out \u0004);
				\u0003 = ((enumerable != null) ? enumerable.ToArray<IVariable>() : null);
				if (\u0003 != null)
				{
					return true;
				}
				if (this.\u0001)
				{
					\u0004 = this[\u0002];
				}
			}
			if (\u0004 != null && \u0004.Length != 0)
			{
				return true;
			}
			if (!this.LocalScope)
			{
				signature = this.\u0002(\u0002);
				if (signature != null)
				{
					\u0004 = new ISignature[1];
					\u0004[0] = signature;
					return true;
				}
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

		// Token: 0x1700053B RID: 1339
		// (get) Token: 0x0600166B RID: 5739 RVA: 0x00043960 File Offset: 0x00041B60
		// (set) Token: 0x0600166C RID: 5740 RVA: 0x00043968 File Offset: 0x00041B68
		public ISignature LocalSignature
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = (value as _ISignature);
			}
		}

		// Token: 0x1700053C RID: 1340
		// (get) Token: 0x0600166D RID: 5741 RVA: 0x00043978 File Offset: 0x00041B78
		public ISignature MostLocalSignature
		{
			get
			{
				ISignature u = this.\u0002;
				return u ?? this.LocalSignature;
			}
		}

		// Token: 0x0600166E RID: 5742 RVA: 0x00043998 File Offset: 0x00041B98
		public IIdentifierInfo[] \u0001(string \u0002)
		{
			bool flag = false;
			_IExpression iexpression = new global::\u0011.\u0006(\u0002).\u0002(out flag);
			if (iexpression == null || flag)
			{
				return null;
			}
			SimpleTypeInferrer simpleTypeInferrer = new SimpleTypeInferrer(this);
			iexpression.Accept(simpleTypeInferrer);
			string u0018_u = string.Empty;
			IdentifierInfoFlag identifierInfoFlag = IdentifierInfoFlag.None;
			if (simpleTypeInferrer.DerivedVariable != null)
			{
				identifierInfoFlag = IdentifierInfoFlag.Variable;
				u0018_u = simpleTypeInferrer.DerivedVariable.Comment;
				if (simpleTypeInferrer.DerivedVariable.GetFlag(VarFlag.Global))
				{
					identifierInfoFlag |= IdentifierInfoFlag.Global;
				}
				if (simpleTypeInferrer.DerivedVariable.GetFlag(VarFlag.External))
				{
					identifierInfoFlag |= IdentifierInfoFlag.External;
				}
				if (simpleTypeInferrer.DerivedVariable.GetFlag(VarFlag.Inout))
				{
					identifierInfoFlag |= IdentifierInfoFlag.Inout;
				}
				if (simpleTypeInferrer.DerivedVariable.GetFlag(VarFlag.Input))
				{
					identifierInfoFlag |= IdentifierInfoFlag.Input;
				}
				if (simpleTypeInferrer.DerivedVariable.GetFlag(VarFlag.Local))
				{
					identifierInfoFlag |= IdentifierInfoFlag.Local;
				}
			}
			return new global::\u0008.\u0006[]
			{
				new global::\u0008.\u0006(\u0002, u0018_u, identifierInfoFlag, simpleTypeInferrer.DerivedType)
				{
					Variable = simpleTypeInferrer.DerivedVariable,
					Signature = simpleTypeInferrer.DerivedSignature
				}
			};
		}

		// Token: 0x1700053D RID: 1341
		// (get) Token: 0x0600166F RID: 5743 RVA: 0x00043AA4 File Offset: 0x00041CA4
		// (set) Token: 0x06001670 RID: 5744 RVA: 0x00043AAC File Offset: 0x00041CAC
		public Guid ApplicationGuid
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
				_IPreCompileContext ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(this.\u0001) as _IPreCompileContext;
				this.\u0001 = ipreCompileContext._GetLibraryTable(this.\u0001);
				_ILibraryTable u = this.\u0001;
			}
		}

		// Token: 0x1700053E RID: 1342
		// (get) Token: 0x06001671 RID: 5745 RVA: 0x00043AF4 File Offset: 0x00041CF4
		// (set) Token: 0x06001672 RID: 5746 RVA: 0x00043AFC File Offset: 0x00041CFC
		public Guid RootApplicationGuid { get; private set; }

		// Token: 0x06001673 RID: 5747 RVA: 0x00043B08 File Offset: 0x00041D08
		public ISignature2 \u0001(IExpression \u0002, out string \u0003)
		{
			\u0003 = null;
			ISignature2 signature = this.\u0001(\u0002) as ISignature2;
			if (signature == null)
			{
				return null;
			}
			\u0003 = this.\u0002(signature.LibraryPath);
			return signature;
		}

		// Token: 0x06001674 RID: 5748 RVA: 0x00043B3C File Offset: 0x00041D3C
		public string \u0002(string \u0002)
		{
			IList<IPreCompileContext> visibleILibraries = this.\u0001.GetVisibleILibraries(this.\u0002);
			for (int i = 0; i < visibleILibraries.Count; i++)
			{
				if (visibleILibraries[i].LibraryPath.ToUpperInvariant() == \u0002.ToUpperInvariant())
				{
					return this.\u0001.GetNamespaceOfLibrary(this.\u0002, \u0002);
				}
			}
			return null;
		}

		// Token: 0x06001675 RID: 5749 RVA: 0x00043BA0 File Offset: 0x00041DA0
		public _IPrecompileScope2 \u0001()
		{
			return new CheckerScope(this.PointerSize, this.\u0001, null, this.\u0001._SystemContext, this.\u0001);
		}

		// Token: 0x06001676 RID: 5750 RVA: 0x00043BC8 File Offset: 0x00041DC8
		public _IPrecompileScope2 \u0002()
		{
			return new CheckerScope(this.PointerSize, this.\u0001, null, this.\u0001.Pool, this.\u0001);
		}

		// Token: 0x06001677 RID: 5751 RVA: 0x00043BF0 File Offset: 0x00041DF0
		public ISignature \u0001(IExpression \u0002)
		{
			if (\u0002 is _ICompoAccessExpression)
			{
				_ICompoAccessExpression icompoAccessExpression = \u0002 as _ICompoAccessExpression;
				global::\u0015.\u0002 u = this.\u0002(icompoAccessExpression._Left);
				if (u != null)
				{
					return u.FindSignatureGlobal(icompoAccessExpression._Right);
				}
				return null;
			}
			else
			{
				if (\u0002 is _ISystemScopeExpression)
				{
					IPrecompileScope precompileScope = this.\u0001() as global::\u0015.\u0002;
					_ISystemScopeExpression isystemScopeExpression = \u0002 as _ISystemScopeExpression;
					return precompileScope.FindSignatureGlobal(isystemScopeExpression._Base.ToString());
				}
				if (\u0002 is _IPoolScopeExpression)
				{
					IPrecompileScope precompileScope2 = this.\u0002() as global::\u0015.\u0002;
					_IPoolScopeExpression ipoolScopeExpression = \u0002 as _IPoolScopeExpression;
					return precompileScope2.FindSignatureGlobal(ipoolScopeExpression._Base.ToString());
				}
				if (\u0002 is _INamespaceAccessExpression)
				{
					_INamespaceAccessExpression inamespaceAccessExpression = \u0002 as _INamespaceAccessExpression;
					global::\u0015.\u0002 u2 = this.\u0002(inamespaceAccessExpression._Namespace);
					if (u2 != null)
					{
						return u2.FindSignatureGlobal(inamespaceAccessExpression._Access);
					}
					return null;
				}
				else
				{
					if (\u0002 is IVariableExpression)
					{
						return this.\u0002(\u0002.ToString());
					}
					ITypeExpression typeExpression = \u0002 as ITypeExpression;
					if (typeExpression != null && typeExpression.ExpressionType != null)
					{
						return this.\u0002(typeExpression.ExpressionType as _IUserdefType);
					}
					return null;
				}
			}
		}

		// Token: 0x06001678 RID: 5752 RVA: 0x00043CF4 File Offset: 0x00041EF4
		public ISignature \u0001(IQualifiedNameExpression \u0002)
		{
			return null;
		}

		// Token: 0x06001679 RID: 5753 RVA: 0x00043CF8 File Offset: 0x00041EF8
		public ISignature \u0002(string \u0002)
		{
			if (this.\u0003 != null)
			{
				ISignature signature = this.\u0003[\u0002];
				if (signature != null && signature.GetFlag(SignatureFlag.SuperGlobal))
				{
					return signature;
				}
			}
			_ILibraryTable u = this.LocalContext._GetLibraryTable(this.ApplicationGuid);
			foreach (_IPreCompileContext u2 in this.\u0001())
			{
				ISignature result;
				if (this.\u0001(\u0002, u, u2, out result))
				{
					return result;
				}
			}
			return null;
		}

		// Token: 0x0600167A RID: 5754 RVA: 0x00043D90 File Offset: 0x00041F90
		private bool \u0001(string \u0002, _ILibraryTable \u0003, _IPreCompileContext \u0004, out ISignature \u0005)
		{
			\u0005 = \u0004[\u0002];
			if (\u0005 == null)
			{
				return false;
			}
			if (this.LocalContext != \u0004)
			{
				if (\u0003.GetQualifiedOnly(this.LocalContext, \u0004.LibraryId))
				{
					return false;
				}
				if (\u0005.GetFlag(SignatureFlag.SystemNamespaceForced))
				{
					return false;
				}
				if (\u0005.GetFlag(SignatureFlag.Internal))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600167B RID: 5755 RVA: 0x00043DFC File Offset: 0x00041FFC
		public ISignature \u0002(IUserdefType \u0002)
		{
			if (\u0002 == null)
			{
				return null;
			}
			return this.\u0001((\u0002 as _IUserdefType).NameExpression);
		}

		// Token: 0x0600167C RID: 5756 RVA: 0x00043E14 File Offset: 0x00042014
		private IPrecompileScope \u0001(_IExpression \u0002, out string \u0003)
		{
			\u0003 = null;
			ICompoAccessExpression compoAccessExpression = \u0002 as ICompoAccessExpression;
			if (compoAccessExpression != null)
			{
				\u0003 = compoAccessExpression.Right.ToString();
				return this.\u0001(compoAccessExpression.Left);
			}
			_ISystemScopeExpression isystemScopeExpression = \u0002 as _ISystemScopeExpression;
			if (isystemScopeExpression != null)
			{
				\u0003 = isystemScopeExpression._Base.ToString();
				return new CheckerScope(null, APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext, null);
			}
			_IPoolScopeExpression ipoolScopeExpression = \u0002 as _IPoolScopeExpression;
			if (ipoolScopeExpression != null)
			{
				\u0003 = ipoolScopeExpression._Base.ToString();
				return new CheckerScope(null, APEnvironmentFacade.Instance.LanguageModelMgr.Pool, null);
			}
			_INamespaceAccessExpression inamespaceAccessExpression = \u0002 as _INamespaceAccessExpression;
			if (inamespaceAccessExpression != null)
			{
				\u0003 = inamespaceAccessExpression._Access.ToString();
				return this.\u0001(inamespaceAccessExpression.Namespace);
			}
			return null;
		}

		// Token: 0x0600167D RID: 5757 RVA: 0x00043ECC File Offset: 0x000420CC
		public bool \u0001(string \u0002, out IVariable \u0003, out ISignature \u0004, out IPrecompileScope \u0005)
		{
			\u0003 = null;
			\u0004 = null;
			_IExpression u = (_IExpression)global::\u0011.\u0006.\u0001(\u0002);
			string stIdent;
			IPrecompileScope precompileScope = this.\u0001(u, out stIdent);
			if (precompileScope != null)
			{
				return precompileScope.FindDeclaration(stIdent, out \u0003, out \u0004, out \u0005);
			}
			IVariable[] array;
			ISignature[] array2;
			global::\u0015.\u0002 u2;
			bool result = this.\u0001(\u0002, out array, out array2, out u2);
			if (array != null && array.Length == 1)
			{
				\u0003 = array[0];
			}
			if (array2 != null && array2.Length == 1)
			{
				\u0004 = array2[0];
			}
			\u0005 = u2;
			return result;
		}

		// Token: 0x0600167E RID: 5758 RVA: 0x00043F38 File Offset: 0x00042138
		private IEnumerable<_IPreCompileContext> \u0001()
		{
			Guid guid = this.LocalContext.ApplicationGuid;
			while (guid != Guid.Empty)
			{
				_IPreCompileContext ipreCompileContext = this.\u0001._GetPrecompileContext(guid);
				if (ipreCompileContext == null)
				{
					break;
				}
				yield return ipreCompileContext;
				IList<_IPreCompileContext> visibleLibraries = this.\u0001.GetVisibleLibraries(ipreCompileContext);
				foreach (_IPreCompileContext ipreCompileContext2 in visibleLibraries)
				{
					if (ipreCompileContext2 != null)
					{
						yield return ipreCompileContext2;
					}
				}
				IEnumerator<_IPreCompileContext> enumerator = null;
				guid = this.\u0001.ApplicationDeviceTable.GetParentApplication(guid);
				ipreCompileContext = null;
			}
			if (APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext != null)
			{
				yield return APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext;
			}
			if (this.\u0002 != null)
			{
				yield return this.\u0002;
				IList<_IPreCompileContext> visibleLibraries2 = this.\u0001.GetVisibleLibraries(this.\u0002);
				foreach (_IPreCompileContext ipreCompileContext3 in visibleLibraries2)
				{
					if (ipreCompileContext3 != null)
					{
						yield return ipreCompileContext3;
					}
				}
				IEnumerator<_IPreCompileContext> enumerator = null;
			}
			if (this.\u0001 != null)
			{
				yield return this.\u0001;
				IList<_IPreCompileContext> visibleLibraries3 = this.\u0001.GetVisibleLibraries(this.\u0001);
				foreach (_IPreCompileContext ipreCompileContext4 in visibleLibraries3)
				{
					if (ipreCompileContext4 != null)
					{
						yield return ipreCompileContext4;
					}
				}
				IEnumerator<_IPreCompileContext> enumerator = null;
			}
			yield break;
			yield break;
		}

		// Token: 0x0600167F RID: 5759 RVA: 0x00043F48 File Offset: 0x00042148
		public IPrecompileScope \u0001()
		{
			return this.\u0001();
		}

		// Token: 0x06001680 RID: 5760 RVA: 0x00043F50 File Offset: 0x00042150
		public IPrecompileScope \u0001(string \u0002)
		{
			ISignature[] array = this.\u0001(\u0002);
			if (array.Length == 1)
			{
				return this.\u0001(array[0]);
			}
			return null;
		}

		// Token: 0x06001681 RID: 5761 RVA: 0x00043F78 File Offset: 0x00042178
		public IPrecompileScope \u0001(ISignature \u0002)
		{
			return this.\u0001(\u0002 as _ISignature);
		}

		// Token: 0x06001682 RID: 5762 RVA: 0x00043F88 File Offset: 0x00042188
		public IIdentifierInfo[] \u0001()
		{
			return this.\u0001(true, false);
		}

		// Token: 0x06001683 RID: 5763 RVA: 0x00043F94 File Offset: 0x00042194
		public IIdentifierInfo[] \u0001(bool \u0002, bool \u0003)
		{
			EWhichDeclarations ewhichDeclarations = EWhichDeclarations.Namespaces | EWhichDeclarations.POUsFromSubLibraries;
			if (\u0002)
			{
				ewhichDeclarations |= EWhichDeclarations.Locals;
			}
			if (\u0003)
			{
				ewhichDeclarations |= EWhichDeclarations.SystemLibraries;
			}
			return this.\u0001(ewhichDeclarations).ToArray<IIdentifierInfo>();
		}

		// Token: 0x06001684 RID: 5764 RVA: 0x00043FC0 File Offset: 0x000421C0
		public IEnumerable<IIdentifierInfo> \u0001(EWhichDeclarations \u0002)
		{
			CaseInsensitiveDictionary<IIdentifierInfo> caseInsensitiveDictionary = new CaseInsensitiveDictionary<IIdentifierInfo>();
			ICaseInsensitiveDictionary<string> caseInsensitiveDictionary2 = new CaseInsensitiveDictionary<string>();
			bool flag = !string.IsNullOrEmpty(this.\u0002.LibraryPath);
			if ((EWhichDeclarations.Locals & \u0002) > EWhichDeclarations.None)
			{
				this.\u0001(caseInsensitiveDictionary, caseInsensitiveDictionary2, flag);
			}
			foreach (IIdentifierInfo identifierInfo in this.\u0001(ref caseInsensitiveDictionary2))
			{
				if (!caseInsensitiveDictionary.ContainsKey(identifierInfo.Name) && !caseInsensitiveDictionary2.ContainsKey(identifierInfo.Name) && (!identifierInfo.Signature.GetFlag(SignatureFlag.Internal) || !flag) && !APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(identifierInfo.Signature, GUIHidingFlags.EvaluateAttributes))
				{
					if (identifierInfo.Variable == null || !APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenVariable(identifierInfo.Variable, GUIHidingFlags.EvaluateAttributes))
					{
						caseInsensitiveDictionary.Add(identifierInfo.Name, identifierInfo);
					}
					else
					{
						caseInsensitiveDictionary2[identifierInfo.Name] = identifierInfo.Name;
					}
				}
			}
			if (this.\u0001)
			{
				this.\u0001(\u0002, caseInsensitiveDictionary, caseInsensitiveDictionary2, flag);
			}
			return caseInsensitiveDictionary.Values.ToArray<IIdentifierInfo>();
		}

		// Token: 0x06001685 RID: 5765 RVA: 0x000440F0 File Offset: 0x000422F0
		private void \u0001(EWhichDeclarations \u0002, CaseInsensitiveDictionary<IIdentifierInfo> \u0003, ICaseInsensitiveDictionary<string> \u0004, bool \u0005)
		{
			bool u = (EWhichDeclarations.SystemLibraries & \u0002) > EWhichDeclarations.None;
			bool flag = string.IsNullOrEmpty(this.\u0002.LibraryPath);
			foreach (IIdentifierInfo identifierInfo in this.\u0001(ref \u0004, u, flag))
			{
				if (!\u0003.ContainsKey(identifierInfo.Name) && !APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(identifierInfo.Signature, GUIHidingFlags.EvaluateAttributes) && (!identifierInfo.Signature.GetFlag(SignatureFlag.Internal) || !\u0005) && !\u0004.ContainsKey(identifierInfo.Name))
				{
					\u0003.Add(identifierInfo.Name, identifierInfo);
				}
				else
				{
					\u0004[identifierInfo.Name] = identifierInfo.Name;
				}
			}
			this.\u0002(\u0002, \u0003, \u0004, !flag);
		}

		// Token: 0x06001686 RID: 5766 RVA: 0x000441C8 File Offset: 0x000423C8
		private void \u0001(CaseInsensitiveDictionary<IIdentifierInfo> \u0002, ICaseInsensitiveDictionary<string> \u0003, bool \u0004)
		{
			foreach (IIdentifierInfo identifierInfo in this.\u0002())
			{
				if (!\u0002.ContainsKey(identifierInfo.Name))
				{
					if (identifierInfo.Variable == null || !APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenVariable(identifierInfo.Variable, GUIHidingFlags.EvaluateAttributes))
					{
						\u0002.Add(identifierInfo.Name, identifierInfo);
					}
					else
					{
						\u0003[identifierInfo.Name] = identifierInfo.Name;
					}
				}
			}
			foreach (IIdentifierInfo identifierInfo2 in this.\u0003())
			{
				if (!\u0002.ContainsKey(identifierInfo2.Name) && !\u0003.ContainsKey(identifierInfo2.Name) && (!identifierInfo2.Signature.GetFlag(SignatureFlag.Internal) || !\u0004) && !APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(identifierInfo2.Signature, GUIHidingFlags.EvaluateAttributes))
				{
					\u0002.Add(identifierInfo2.Name, identifierInfo2);
				}
				else
				{
					\u0003[identifierInfo2.Name] = identifierInfo2.Name;
				}
			}
		}

		// Token: 0x06001687 RID: 5767 RVA: 0x000442D8 File Offset: 0x000424D8
		private void \u0002(EWhichDeclarations \u0002, CaseInsensitiveDictionary<IIdentifierInfo> \u0003, ICaseInsensitiveDictionary<string> \u0004, bool \u0005)
		{
			IList<_IPreCompileContext> visibleLibraries = this.\u0001.GetVisibleLibraries(this.\u0002);
			for (int i = 0; i < visibleLibraries.Count; i++)
			{
				if (visibleLibraries[i] != null)
				{
					this.\u0001(\u0002, \u0003, \u0004, visibleLibraries[i]);
					_ILibraryTable2 ilibraryTable = this.\u0001 as _ILibraryTable2;
					bool flag = ilibraryTable != null && ilibraryTable.GetPublishSymbols(this.\u0002, visibleLibraries[i].LibraryPath);
					if ((!\u0005 || flag) && !this.\u0001.GetQualifiedOnly(this.\u0002, visibleLibraries[i].LibraryPath) && (EWhichDeclarations.POUsFromSubLibraries & \u0002) != EWhichDeclarations.None)
					{
						CheckerScope.\u0001(\u0003, \u0004, visibleLibraries[i]);
					}
				}
			}
		}

		// Token: 0x06001688 RID: 5768 RVA: 0x0004438C File Offset: 0x0004258C
		private void \u0001(EWhichDeclarations \u0002, CaseInsensitiveDictionary<IIdentifierInfo> \u0003, ICaseInsensitiveDictionary<string> \u0004, _IPreCompileContext \u0005)
		{
			if ((EWhichDeclarations.Namespaces & \u0002) == EWhichDeclarations.None)
			{
				return;
			}
			string libraryPath = \u0005.LibraryPath;
			global::\u0008.\u0006 u = new global::\u0008.\u0006(this.\u0001.GetNamespaceOfLibrary(this.\u0002, libraryPath), "", IdentifierInfoFlag.Scope, null);
			if (!\u0003.ContainsKey(u.Name) && !\u0004.ContainsKey(u.Name))
			{
				\u0003[u.Name] = u;
			}
		}

		// Token: 0x06001689 RID: 5769 RVA: 0x000443F0 File Offset: 0x000425F0
		private static void \u0001(CaseInsensitiveDictionary<IIdentifierInfo> \u0002, ICaseInsensitiveDictionary<string> \u0003, _IPreCompileContext \u0004)
		{
			foreach (_ISignature isignature in \u0004.AllSignatures.OfType<_ISignature>())
			{
				if (\u0002.ContainsKey(isignature.OrgName) || \u0003.ContainsKey(isignature.OrgName) || APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(isignature, (GUIHidingFlags)18446462598733561856UL))
				{
					if (isignature.HasAttribute(CompileAttributes.ATTRIBUTE_HIDE))
					{
						\u0003[isignature.OrgName] = isignature.OrgName;
					}
				}
				else
				{
					IdentifierInfoFlag identifierInfoFlag = IdentifierInfoFlag.Signature;
					Operator poutype = isignature.POUType;
					if (poutype <= Operator.FunctionBlock)
					{
						if (poutype != Operator.Action)
						{
							if (poutype != Operator.Function)
							{
								if (poutype == Operator.FunctionBlock)
								{
									identifierInfoFlag |= IdentifierInfoFlag.Functionblock;
								}
							}
							else
							{
								identifierInfoFlag |= IdentifierInfoFlag.Function;
							}
						}
						else
						{
							identifierInfoFlag |= IdentifierInfoFlag.Action;
						}
					}
					else if (poutype != Operator.Program)
					{
						if (poutype != Operator.VarGlobal)
						{
							if (poutype == Operator.Method)
							{
								identifierInfoFlag |= IdentifierInfoFlag.Method;
							}
						}
						else
						{
							identifierInfoFlag |= IdentifierInfoFlag.Global;
						}
					}
					else
					{
						identifierInfoFlag |= IdentifierInfoFlag.Program;
					}
					global::\u0008.\u0006 u = new global::\u0008.\u0006(isignature, isignature.OrgName, isignature.Comment, identifierInfoFlag, null);
					\u0002.Add(u.Name, u);
				}
			}
		}

		// Token: 0x0600168A RID: 5770 RVA: 0x00044540 File Offset: 0x00042740
		public IIdentifierInfo[] \u0002()
		{
			CaseInsensitiveDictionary<IIdentifierInfo> caseInsensitiveDictionary = new CaseInsensitiveDictionary<IIdentifierInfo>();
			int num = 0;
			if (this.\u0002 != null)
			{
				num = this.\u0002.AllVariables.Count;
			}
			if (this.\u0001 != null)
			{
				num += this.\u0001.AllVariables.Count;
			}
			_IVariable[] array = new _IVariable[num];
			_ISignature[] array2 = new _ISignature[num];
			int num2 = 0;
			if (this.\u0002 != null)
			{
				IList<_IVariable> allVariables = this.\u0002.AllVariables;
				allVariables.CopyTo(array, 0);
				num2 += allVariables.Count;
				for (int i = 0; i < allVariables.Count; i++)
				{
					array2[i] = this.\u0002;
				}
			}
			if (this.\u0001 != null)
			{
				IList<_IVariable> allVariables2 = this.\u0001.AllVariables;
				allVariables2.CopyTo(array, num2);
				for (int j = 0; j < allVariables2.Count; j++)
				{
					array2[num2 + j] = this.\u0001;
				}
			}
			for (int k = 0; k < array.Length; k++)
			{
				_IVariable ivariable = array[k];
				_ISignature u001B_u = array2[k];
				if (!caseInsensitiveDictionary.ContainsKey(ivariable.OrgName) && !APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenVariable(ivariable, GUIHidingFlags.VarImplicit | GUIHidingFlags.EvaluateFlags | GUIHidingFlags.EvaluateImplicitNames))
				{
					IdentifierInfoFlag identifierInfoFlag = IdentifierInfoFlag.Variable;
					if (ivariable.GetFlag(VarFlag.Input))
					{
						identifierInfoFlag |= IdentifierInfoFlag.Input;
					}
					if (ivariable.GetFlag(VarFlag.Output))
					{
						identifierInfoFlag |= IdentifierInfoFlag.Output;
					}
					if (ivariable.GetFlag(VarFlag.Inout))
					{
						identifierInfoFlag |= IdentifierInfoFlag.Inout;
					}
					if (ivariable.GetFlag(VarFlag.External))
					{
						identifierInfoFlag |= IdentifierInfoFlag.External;
					}
					if (ivariable.GetFlag(VarFlag.Local))
					{
						identifierInfoFlag |= IdentifierInfoFlag.Local;
					}
					global::\u0008.\u0006 u = new global::\u0008.\u0006(ivariable, u001B_u, ivariable.OrgName, ivariable.Comment, identifierInfoFlag, ivariable.Type);
					caseInsensitiveDictionary.Add(ivariable.OrgName, u);
				}
			}
			IIdentifierInfo[] array3 = new IIdentifierInfo[caseInsensitiveDictionary.Values.Count];
			caseInsensitiveDictionary.Values.CopyTo(array3, 0);
			return array3;
		}

		// Token: 0x0600168B RID: 5771 RVA: 0x00044730 File Offset: 0x00042930
		public IIdentifierInfo[] \u0003()
		{
			_ISignature isignature = this.\u0001;
			CaseInsensitiveDictionary<IIdentifierInfo> caseInsensitiveDictionary = new CaseInsensitiveDictionary<IIdentifierInfo>();
			LDictionary<_ISignature, _ISignature> ldictionary = new LDictionary<_ISignature, _ISignature>();
			while (isignature != null && !ldictionary.ContainsKey(isignature))
			{
				IList<_ISignature> list = this.\u0002._GetSubSignatures(isignature.ObjectGuid);
				if (list != null)
				{
					foreach (_ISignature isignature2 in list)
					{
						if (!caseInsensitiveDictionary.ContainsKey(isignature2.OrgName) && !isignature2.GetFlag(SignatureFlag.Generated) && !isignature2.GetFlag(SignatureFlag.ImplicitInterfaceUnion) && isignature2.OrgName.IndexOf("__", StringComparison.OrdinalIgnoreCase) < 0)
						{
							IdentifierInfoFlag identifierInfoFlag = IdentifierInfoFlag.Signature;
							Operator poutype = isignature2.POUType;
							if (poutype != Operator.Action)
							{
								if (poutype == Operator.Method)
								{
									identifierInfoFlag |= IdentifierInfoFlag.Method;
								}
							}
							else
							{
								identifierInfoFlag |= IdentifierInfoFlag.Action;
							}
							global::\u0008.\u0006 u = new global::\u0008.\u0006(isignature2, isignature2.OrgName, isignature2.Comment, identifierInfoFlag, null);
							caseInsensitiveDictionary.Add(isignature2.OrgName, u);
						}
					}
				}
				if (isignature.BaseExpression == null)
				{
					break;
				}
				ldictionary[isignature] = isignature;
				isignature = (this.\u0001(isignature.BaseExpression) as _ISignature);
			}
			IIdentifierInfo[] array = new IIdentifierInfo[caseInsensitiveDictionary.Values.Count];
			caseInsensitiveDictionary.Values.CopyTo(array, 0);
			return array;
		}

		// Token: 0x0600168C RID: 5772 RVA: 0x0004489C File Offset: 0x00042A9C
		public IIdentifierInfo[] \u0001(ref ICaseInsensitiveDictionary<string> \u0002, bool \u0003, bool \u0004)
		{
			CaseInsensitiveDictionary<IIdentifierInfo> caseInsensitiveDictionary = new CaseInsensitiveDictionary<IIdentifierInfo>();
			IList<_IPreCompileContext> list = new LList<_IPreCompileContext>(this.\u0001.GetVisibleLibraries(this.\u0002));
			if (this.\u0002 != null)
			{
				list.Add(this.\u0002);
			}
			foreach (_IPreCompileContext ipreCompileContext in list)
			{
				if ((\u0004 || string.IsNullOrEmpty(ipreCompileContext.LibraryPath) || ipreCompileContext == this.\u0002) && !this.\u0001.GetQualifiedOnly(this.\u0002, ipreCompileContext.LibraryPath))
				{
					foreach (_ISignature isignature in ipreCompileContext._AllSignatures)
					{
						if (isignature.POUType == Operator.Program || isignature.POUType == Operator.FunctionBlock || isignature.POUType == Operator.Function || isignature.POUType == Operator.Interface || isignature.POUType == Operator.Type || isignature.POUType == Operator.VarGlobal)
						{
							if (caseInsensitiveDictionary.ContainsKey(isignature.OrgName) || \u0002.ContainsKey(isignature.OrgName) || APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(isignature, (GUIHidingFlags)18446462598733037568UL))
							{
								if (isignature.HasAttribute(CompileAttributes.ATTRIBUTE_HIDE))
								{
									\u0002[isignature.OrgName] = isignature.OrgName;
								}
							}
							else
							{
								global::\u0008.\u0006 u = new global::\u0008.\u0006(isignature, isignature.OrgName, string.Empty, IdentifierInfoFlag.Signature | IdentifierInfoFlag.Global, null);
								caseInsensitiveDictionary.Add(isignature.OrgName, u);
							}
						}
					}
				}
			}
			IIdentifierInfo[] array = new IIdentifierInfo[caseInsensitiveDictionary.Values.Count];
			caseInsensitiveDictionary.Values.CopyTo(array, 0);
			return array;
		}

		// Token: 0x0600168D RID: 5773 RVA: 0x00044A98 File Offset: 0x00042C98
		public IIdentifierInfo[] \u0001(ref ICaseInsensitiveDictionary<string> \u0002)
		{
			CaseInsensitiveDictionary<IIdentifierInfo> caseInsensitiveDictionary = new CaseInsensitiveDictionary<IIdentifierInfo>();
			IList<_IPreCompileContext> list = new LList<_IPreCompileContext>();
			if (this.\u0002 != null)
			{
				list.Add(this.\u0002);
			}
			if (this.\u0001 != null)
			{
				list.Add(this.\u0001);
			}
			foreach (_IPreCompileContext ipreCompileContext in list)
			{
				if (!this.\u0001.GetQualifiedOnly(this.\u0002, ipreCompileContext.LibraryPath))
				{
					foreach (_ISignature isignature in ipreCompileContext._GVLSignatures)
					{
						if (!isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion) && !isignature.HasAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY))
						{
							IList<_IVariable> allVariables = isignature.AllVariables;
							if (APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(isignature, GUIHidingFlags.EvaluateAttributes))
							{
								using (IEnumerator<_IVariable> enumerator3 = allVariables.GetEnumerator())
								{
									while (enumerator3.MoveNext())
									{
										_IVariable ivariable = enumerator3.Current;
										new global::\u0008.\u0006(ivariable, isignature, ivariable.OrgName, ivariable.Comment, IdentifierInfoFlag.Variable | IdentifierInfoFlag.Global, ivariable.Type).Signature = isignature;
										\u0002[ivariable.OrgName] = ivariable.OrgName;
									}
									continue;
								}
							}
							foreach (_IVariable ivariable2 in allVariables)
							{
								if (!APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenVariable(ivariable2, GUIHidingFlags.VarImplicit | GUIHidingFlags.EvaluateFlags | GUIHidingFlags.EvaluateImplicitNames) && !caseInsensitiveDictionary.ContainsKey(ivariable2.OrgName))
								{
									global::\u0008.\u0006 u = new global::\u0008.\u0006(ivariable2, isignature, ivariable2.OrgName, ivariable2.Comment, IdentifierInfoFlag.Variable | IdentifierInfoFlag.Global, ivariable2.Type);
									u.Signature = isignature;
									caseInsensitiveDictionary.Add(ivariable2.OrgName, u);
								}
							}
						}
					}
				}
			}
			IIdentifierInfo[] array = new IIdentifierInfo[caseInsensitiveDictionary.Values.Count];
			caseInsensitiveDictionary.Values.CopyTo(array, 0);
			return array;
		}

		// Token: 0x0600168E RID: 5774 RVA: 0x00044D28 File Offset: 0x00042F28
		public ISignature \u0001(ISignature \u0002, out IPrecompileScope7 \u0003)
		{
			global::\u0015.\u0002 u;
			_ISignature isignature = this.\u0001(\u0002 as _ISignature, out u);
			if (isignature != null)
			{
				\u0003 = u;
				return isignature;
			}
			\u0003 = this;
			return \u0002;
		}

		// Token: 0x0600168F RID: 5775 RVA: 0x00044D50 File Offset: 0x00042F50
		public ICompiledType \u0001(ICompiledType \u0002, out IPrecompileScope7 \u0003)
		{
			global::\u0015.\u0002 u;
			ICompiledType result = this.\u0001(\u0002 as _IType, out u);
			\u0003 = u;
			return result;
		}

		// Token: 0x1700053F RID: 1343
		// (get) Token: 0x06001690 RID: 5776 RVA: 0x00044D70 File Offset: 0x00042F70
		public bool IsPrecompileScope
		{
			get
			{
				return true;
			}
		}

		// Token: 0x040003E3 RID: 995
		private readonly _ILanguageModelManagerConsolidated \u0001;

		// Token: 0x040003E4 RID: 996
		private _ISignature \u0001;

		// Token: 0x040003E5 RID: 997
		private readonly _ISignature \u0002;

		// Token: 0x040003E6 RID: 998
		private readonly _IPreCompileContext \u0001;

		// Token: 0x040003E7 RID: 999
		private readonly _IPreCompileContext \u0002;

		// Token: 0x040003E8 RID: 1000
		private _IPreCompileContext \u0003;

		// Token: 0x040003E9 RID: 1001
		private readonly bool \u0001 = true;

		// Token: 0x040003EA RID: 1002
		private _ILibraryTable \u0001;

		// Token: 0x040003EB RID: 1003
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x040003EC RID: 1004
		[CompilerGenerated]
		private bool \u0003;

		// Token: 0x040003ED RID: 1005
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x040003EE RID: 1006
		[CompilerGenerated]
		private bool \u0004;

		// Token: 0x040003EF RID: 1007
		private Guid \u0001;

		// Token: 0x040003F0 RID: 1008
		[CompilerGenerated]
		private Guid \u0002;
	}
}
