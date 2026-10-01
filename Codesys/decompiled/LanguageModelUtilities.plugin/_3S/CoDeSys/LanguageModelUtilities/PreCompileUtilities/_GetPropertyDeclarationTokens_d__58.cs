using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelUtilities.Legacy;
using _3S.CoDeSys.LibManObject;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Released interface cannot be changed anymore")]
	public class PreCompileUtilities : IPreCompileUtilities14, IPreCompileUtilities13, IPreCompileUtilities12, IPreCompileUtilities11, IPreCompileUtilities10, IPreCompileUtilities9, IPreCompileUtilities8, IPreCompileUtilities7, IPreCompileUtilities6, IPreCompileUtilities5, IPreCompileUtilities4, IPreCompileUtilities3, IPreCompileUtilities2, IPreCompileUtilities
	{
		private List<IAttributedString> _lstOnlyEmptyStringTokenContainingList;

		public PreCompileUtilities()
		{
			_lstOnlyEmptyStringTokenContainingList = new List<IAttributedString>();
			_lstOnlyEmptyStringTokenContainingList.Add(new PlainTextAttributedString(string.Empty));
		}

		public IEvaluationContext CreateContext(Guid gdApplication, Guid gdScope)
		{
			return new EvaluationContext(gdApplication, gdScope);
		}

		public IEvaluationContext2 CreateContext2(int nProj, int nAttrProj, Guid gdScope)
		{
			return new EvaluationContext2(nProj, nAttrProj, gdScope);
		}

		public IEvaluationContext3 CreateContext3(int nProj, int nAttrProj, Guid gdScope, IGetLibInformation libInfo)
		{
			return new EvaluationContext3(nProj, nAttrProj, gdScope, libInfo);
		}

		public IEvaluationContext4 CreateContext4(int nProj, int nAttrProj, Guid gdScope, Guid gdLocalScope, IGetLibInformation libInfo)
		{
			return new EvaluationContext4(nProj, nAttrProj, gdScope, gdLocalScope, libInfo);
		}

		public IEvaluationContext3 CreateAppSpecificContext3(int nProj, int nAttrProj, Guid gdApp, Guid gdScope, IGetLibInformation libInfo)
		{
			return new EvaluationContext3(nProj, nAttrProj, gdApp, gdScope, libInfo);
		}

		public IConstantEvaluator CreateConstantEvaluator(IEvaluationContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			return new ConstantEvaluator(context);
		}

		public IArrayDimensionEvaluator CreateArrayDimensionEvaluator(IEvaluationContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			return new ArrayDimensionEvaluator(context);
		}

		public Guid GetDeclaringSignature(IEvaluationContext context, string stAccessPath)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (stAccessPath == null)
			{
				throw new ArgumentNullException("stAccessPath");
			}
			IPreCompileContext precompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(context.ApplicationGuid);
			if (precompileContext != null)
			{
				IIdentifierInfo[] identifierInfo = precompileContext.GetIdentifierInfo(Guid.Empty, stAccessPath);
				if (identifierInfo != null && identifierInfo.Length != 0 && identifierInfo[0].Signature != null)
				{
					return identifierInfo[0].Signature.ObjectGuid;
				}
			}
			return Guid.Empty;
		}

		public InitialValueResult DetermineInitialValue(IEvaluationContext ctxBase, string stAccessPath, out IExpression expInit, out IEvaluationContext ctxExpInit)
		{
			IEvaluationContext2 ctxExpInit2;
			InitialValueResult result = InitialValue.Determine(ctxBase, stAccessPath, out expInit, out ctxExpInit2);
			ctxExpInit = ctxExpInit2;
			return result;
		}

		public ISignature2 FindTypeSignature2(IEvaluationContext context, string stTypeName)
		{
			return Helpers.FindTypeSignature2(context, stTypeName);
		}

		public IPreCompileContext4 GetPreCompileContextForProject(int nProj)
		{
			return APEnvironmentFacade.Instance.GetPreCompileContextForProject(nProj);
		}

		public bool IsPrimitiveType(IType t)
		{
			return Helpers.IsPrimitiveType(t);
		}

		public ICompiledType2 GetCompiledType(string stTypeName)
		{
			return Helpers.GetCompiledType(stTypeName);
		}

		public IEvaluationContext GetContextFromSignature(int nProjAttracting, ISignature sig)
		{
			if (sig == null || !(sig is ISignature2))
			{
				throw new ArgumentException("sig");
			}
			return Helpers.GetContextFromSignature(nProjAttracting, (ISignature2)sig, null, new GetLibInformation());
		}

		public IEvaluationContext3 GetContextFromSignature(int nProjAttracting, ISignature sig, IGetLibInformation libInfo)
		{
			if (sig == null || !(sig is ISignature2))
			{
				throw new ArgumentException("sig");
			}
			return Helpers.GetContextFromSignature(nProjAttracting, (ISignature2)sig, null, libInfo);
		}

		public IEvaluationContext4 GetContextFromSignature(int nProjAttracting, ISignature sig, ISignature sigLocal, IGetLibInformation libInfo)
		{
			if (sig == null || !(sig is ISignature2))
			{
				throw new ArgumentException("sig");
			}
			return Helpers.GetContextFromSignature(nProjAttracting, (ISignature2)sig, (ISignature2)sigLocal, libInfo);
		}

		public IProject GetAttractingProject(int nContextProj, Guid gdContextApp, int nAttractedProj)
		{
			Stack<ILibManItem> stack = new Stack<ILibManItem>();
			LibraryHelpers.GetItemPathFromProjectRec(stack, GetProjectFromHandle(nContextProj), gdContextApp, GetProjectFromHandle(nAttractedProj), new GetLibInformation());
			if (stack.Count > 1)
			{
				stack.Pop();
				IProject projectFromLibManItem = GetProjectFromLibManItem(stack.Peek());
				if (projectFromLibManItem == null)
				{
					return GetProjectFromHandle(nContextProj);
				}
				return projectFromLibManItem;
			}
			return GetProjectFromHandle(nContextProj);
		}

		public bool GetItemPathFromProjectRec(Stack<ILibManItem> itemPath, IProject proToLookIn, Guid gdApp, IProject proToLookFor)
		{
			return LibraryHelpers.GetItemPathFromProjectRec(itemPath, proToLookIn, gdApp, proToLookFor, new GetLibInformation());
		}

		private static IManagedLibrary GetManagedLibFromLibManItem(ILibManItem lmi)
		{
			return LibraryHelpers.GetManagedLibFromLibManItem(lmi);
		}

		private static IProject GetProjectFromHandle(int nProj)
		{
			return APEnvironmentFacade.Instance.GetProjectFromHandle(nProj);
		}

		public IProject GetProjectFromLibManItem(ILibManItem item)
		{
			return LibraryHelpers.GetProjectFromLibManItem(item, new GetLibInformation());
		}

		public bool GetProjectPathByLibDisplayName(int nProj, string stDisplayName, Stack<int> itemPath)
		{
			return LibraryHelpers.FindLibByDisplayName(nProj, null, stDisplayName, itemPath, new GetLibInformation());
		}

		public IProject GetProjectByLibNamespacePath(int nProj, string stPath)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected O, but got Unknown
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			DPath namespacePath = new DPath(stPath);
			IList<string> restSigs;
			return GetProjectFromHandle(LibraryHelpers.FindLibByNamespace(nProj, null, namespacePath, out restSigs, new GetLibInformation()).first);
		}

		public IManagedLibrary GetManagedLib(ILibManItem lmi)
		{
			return GetManagedLibFromLibManItem(lmi);
		}

		public IEnumerable<ILibManItem> GetAllLibManItemsInProject(int nProj)
		{
			return LibraryHelpers.GetAllLibManItemsInProject(nProj, null);
		}

		public IVariable FindVar(ISignature2 sig, string stName, out ISignature2 foundSig)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			Pair<ISignature2, IVariable> val = InitialValue.FindVar(sig, stName);
			foundSig = val.first;
			return val.second;
		}

		public bool GetItemPathFromProjectRec(Stack<ILibManItem> itemPath, IProject proToLookIn, Guid gdApp, IProject proToLookFor, IGetLibInformation libInfo)
		{
			object libInfo2;
			if (!(libInfo is IGetLibInformation2))
			{
				IGetLibInformation2 getLibInformation = new GetLibInformation();
				libInfo2 = getLibInformation;
			}
			else
			{
				libInfo2 = (IGetLibInformation2)libInfo;
			}
			return LibraryHelpers.GetItemPathFromProjectRec(itemPath, proToLookIn, gdApp, proToLookFor, (IGetLibInformation2)libInfo2);
		}

		public bool GetProjectPathByLibDisplayName(int nProj, string stDisplayName, Stack<int> itemPath, IGetLibInformation libInfo)
		{
			object libInfo2;
			if (!(libInfo is IGetLibInformation2))
			{
				IGetLibInformation2 getLibInformation = new GetLibInformation();
				libInfo2 = getLibInformation;
			}
			else
			{
				libInfo2 = (IGetLibInformation2)libInfo;
			}
			return LibraryHelpers.FindLibByDisplayName(nProj, null, stDisplayName, itemPath, (IGetLibInformation2)libInfo2);
		}

		public IProject GetProjectByLibNamespacePath(int nProj, string stPath, IGetLibInformation libInfo)
		{
			object libInfo2;
			if (!(libInfo is IGetLibInformation2))
			{
				IGetLibInformation2 getLibInformation = new GetLibInformation();
				libInfo2 = getLibInformation;
			}
			else
			{
				libInfo2 = (IGetLibInformation2)libInfo;
			}
			return LibraryHelpers.GetProjectByLibNamespacePath(nProj, null, stPath, (IGetLibInformation2)libInfo2);
		}

		public IProject GetProjectFromLibManItem(ILibManItem item, IGetLibInformation libInfo)
		{
			return LibraryHelpers.GetProjectFromLibManItem(item, libInfo);
		}

		public IGetLibInformation CreateBufferLibInfo()
		{
			return new GetLibInformationBuffered();
		}

		public IGetLibInformation CreateLibInfo()
		{
			return new GetLibInformation();
		}

		public void DoStandardTreeTraversal(IExprementVisitorNoTraversion visitor, ISignature4 localSignature, IPreCompileContext9 precomApp, IExprement expToVisit)
		{
			if (precomApp == null)
			{
				throw new ArgumentNullException("precomApp");
			}
			if (visitor == null)
			{
				throw new ArgumentNullException("visitor");
			}
			if (expToVisit == null)
			{
				throw new ArgumentNullException("expToVisit");
			}
			StandardTraverser visitor2 = new StandardTraverser(visitor, localSignature, precomApp);
			expToVisit.AcceptVisitor(visitor2);
			visitor.Traverser = null;
		}

		public void DoStatementTreeTraversal(IStatementVisitorNoTraversion visitor, IStatement statementToVisit)
		{
			if (visitor == null)
			{
				throw new ArgumentNullException("visitor");
			}
			if (statementToVisit == null)
			{
				throw new ArgumentNullException("statementToVisit");
			}
			StatementTraverser visitor2 = new StatementTraverser(visitor);
			statementToVisit.AcceptVisitor(visitor2);
		}

		public void VisitAllVariables(IVariableVisitor visitor, ISignature4 localSignature, IPreCompileContext9 precomApp, IExprement expToVisit)
		{
			if (precomApp == null)
			{
				throw new ArgumentNullException("precomApp");
			}
			if (visitor == null)
			{
				throw new ArgumentNullException("visitor");
			}
			if (expToVisit == null)
			{
				throw new ArgumentNullException("expToVisit");
			}
			StandardTraverser visitor2 = new StandardTraverser(new VariableVisitor(visitor), localSignature, precomApp);
			expToVisit.AcceptVisitor(visitor2);
		}

		public IType ResolveAliasType(IEvaluationContext context, IType aliasType)
		{
			TypeTraverser typeTraverser = new TypeTraverser();
			typeTraverser.TraverseType(aliasType, this, context);
			string resolvedType = typeTraverser.GetResolvedType();
			if (resolvedType == null)
			{
				return aliasType;
			}
			return Helpers.GetCompiledType(resolvedType);
		}

		public void EnforcePresenceOfAllPreCompileContexts(IProgressCallback callback)
		{
			if (!APEnvironmentFacade.Instance.ExistsPrimaryProject)
			{
				throw new InvalidOperationException();
			}
			EnsureProjectsLoaded(callback);
			if (!callback.Aborting)
			{
				APEnvironmentFacade.Instance.LanguageModelMgr.ProcessQueuedLibraryPreCompileContexts(callback);
			}
		}

		private static void EnsureProjectsLoaded(IProgressCallback callback)
		{
			if (!APEnvironmentFacade.Instance.ExistsPrimaryProject)
			{
				return;
			}
			HashSet<int> hashSet = new HashSet<int> { APEnvironmentFacade.Instance.PrimaryProjectHandle };
			HashSet<int> hashSet2 = new HashSet<int>();
			ObjectEventHandler value = delegate
			{
				callback.TaskProgress("");
			};
			APEnvironmentFacade.Instance.ObjectLoaded += value;
			try
			{
				while (hashSet.Count > 0 && !callback.Aborting)
				{
					int num = hashSet.First();
					if (!APEnvironmentFacade.Instance.IsLoadProjectFinished(num, out var nObjectsRemaining))
					{
						string stTask;
						if (APEnvironmentFacade.Instance.PrimaryProjectHandle == num)
						{
							stTask = Strings.Progress_FinishProjectLoad;
						}
						else
						{
							string libraryId = APEnvironmentFacade.Instance.GetLibraryId(num);
							stTask = string.Format(Strings.Progress_FinishLibraryLoad, libraryId);
						}
						callback.NextTask(stTask, nObjectsRemaining, "");
						APEnvironmentFacade.Instance.FinishLoadProject(num);
					}
					hashSet2.Add(num);
					hashSet.Remove(num);
					foreach (int item in APEnvironmentFacade.Instance.GetAllProjectsWithAttribute(ProjectAttributes.ProvidesLanguageModel))
					{
						if (!hashSet2.Contains(item))
						{
							hashSet.Add(item);
						}
					}
				}
			}
			finally
			{
				APEnvironmentFacade.Instance.ObjectLoaded -= value;
			}
		}

		public string AddDeviceApplicationPrefix(string stWatchExpression)
		{
			Guid activeApplicationGuid = APEnvironmentFacade.Instance.ActiveApplicationGuid;
			if (activeApplicationGuid == Guid.Empty)
			{
				return stWatchExpression;
			}
			return AddDeviceApplicationPrefix(stWatchExpression, activeApplicationGuid);
		}

		public string AddDeviceApplicationPrefix(string stWatchExpression, Guid applicationGuid)
		{
			string activeApplicationPrefix = APEnvironmentFacade.Instance.GetActiveApplicationPrefix(applicationGuid);
			if (!string.IsNullOrEmpty(activeApplicationPrefix) && !stWatchExpression.StartsWith(activeApplicationPrefix))
			{
				return activeApplicationPrefix + "." + stWatchExpression;
			}
			return stWatchExpression;
		}

		internal static int _GetIntValue(IExpression expr, IPrecompileScope scope, out bool bValid)
		{
			ILiteralValue literalValue = (expr as IExpression5).Literal(scope);
			if (literalValue != null)
			{
				if (literalValue.KindOf == KindOfLiteral.SignedInteger)
				{
					bValid = true;
					return (int)literalValue.SignedLong;
				}
				if (literalValue.KindOf == KindOfLiteral.UnsignedInteger)
				{
					bValid = true;
					return (int)literalValue.UnsignedLong;
				}
			}
			bValid = false;
			return 0;
		}

		public int GetIntValue(IExpression expr, IPrecompileScope scope, out bool bValid)
		{
			return _GetIntValue(expr, scope, out bValid);
		}

		internal static ISignature _ResolveAlias(ISignature signAlias, IPrecompileScope6 scope, int nDepth)
		{
			if (nDepth > 5 || !signAlias.GetFlag(SignatureFlag.Alias))
			{
				return null;
			}
			if (signAlias.All.Length != 0 && signAlias.All[0].Type != null && signAlias.All[0].Type.Class == TypeClass.Userdef)
			{
				IUserdefType2 userdefType = (signAlias.All[0].Type as IUserdefType) as IUserdefType2;
				ISignature signature = scope.FindSignatureGlobal(userdefType.NameExpression);
				if (signature == null)
				{
					return null;
				}
				if (signature.GetFlag(SignatureFlag.Alias))
				{
					return _ResolveAlias(signature, scope, ++nDepth);
				}
				return signature;
			}
			return null;
		}

		public ISignature ResolveAlias(ISignature signAlias, IPrecompileScope6 scope)
		{
			return _ResolveAlias(signAlias, scope, 0);
		}

		public IIdentifierInfo[] FindSubelements(Guid guidSignature, IPreCompileContext11 precom, string stAccessPathOrType, FindSubelementsFlags flags, out bool bError)
		{
			IPreCompileContext11 preCompileContext;
			if (guidSignature == Guid.Empty)
			{
				preCompileContext = precom;
			}
			else
			{
				preCompileContext = null;
				foreach (IPreCompileContext11 item in APEnvironmentFacade.Instance.LanguageModelMgr.AllPreCompileContexts(bWithDevices: true, bWithLibraries: false).OfType<IPreCompileContext11>())
				{
					if (item.GetSignature(guidSignature) != null)
					{
						preCompileContext = item;
						break;
					}
				}
				if (preCompileContext == null)
				{
					foreach (IPreCompileContext11 item2 in APEnvironmentFacade.Instance.LanguageModelMgr.AllPreCompileContexts(bWithDevices: true, bWithLibraries: false).OfType<IPreCompileContext11>())
					{
						if (item2.ApplicationGuid == guidSignature)
						{
							preCompileContext = item2;
							break;
						}
					}
				}
				if (preCompileContext == null)
				{
					preCompileContext = FindPrecompileContextOfProperty(guidSignature, preCompileContext);
				}
				if (preCompileContext == null)
				{
					preCompileContext = precom;
				}
			}
			if (preCompileContext == null)
			{
				bError = true;
				return new IIdentifierInfo[0];
			}
			return new FindSubelementsHelper(guidSignature, preCompileContext, stAccessPathOrType).FindSubelements(flags, out bError);
		}

		private static IPreCompileContext11 FindPrecompileContextOfProperty(Guid guidSignature, IPreCompileContext11 precomToUse)
		{
			foreach (IPreCompileContext11 item in APEnvironmentFacade.Instance.LanguageModelMgr.AllPreCompileContexts(bWithDevices: true, bWithLibraries: false).OfType<IPreCompileContext11>())
			{
				ISignature[] allSignatures = item.AllSignatures;
				for (int i = 0; i < allSignatures.Length; i++)
				{
					IVariable[] all = allSignatures[i].All;
					foreach (IVariable variable in all)
					{
						if (variable.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY) && variable.HasAttribute(CompileAttributes.ATTRIBUTE_MESSAGE_GUID) && Guid.Parse(variable.GetAttributeValue(CompileAttributes.ATTRIBUTE_MESSAGE_GUID)) == guidSignature)
						{
							precomToUse = item;
							break;
						}
					}
				}
			}
			return precomToUse;
		}

		public string GetScopeDeclarationIdentifier(IVariable variable)
		{
			Operator op;
			if (variable.HasFlag(VarFlag.Local))
			{
				op = Operator.Var;
			}
			else if (variable.HasFlag(VarFlag.Input))
			{
				op = Operator.VarInput;
			}
			else if (variable.HasFlag(VarFlag.Output))
			{
				op = Operator.VarOutput;
			}
			else if (variable.HasFlag(VarFlag.Inout))
			{
				op = Operator.VarInOut;
			}
			else if (variable.HasFlag(VarFlag.External))
			{
				op = Operator.VarExternal;
			}
			else if (variable.HasFlag(VarFlag.VarConfig))
			{
				op = Operator.VarConfig;
			}
			else if (variable.HasFlag(VarFlag.Global))
			{
				op = Operator.VarGlobal;
			}
			else if (variable.HasFlag(VarFlag.Temp))
			{
				op = Operator.VarTemp;
			}
			else if (variable.HasFlag(VarFlag.Static))
			{
				op = Operator.VarStat;
			}
			else if (variable.HasFlag(VarFlag.AllocateInInstance))
			{
				op = Operator.VarInst;
			}
			else
			{
				if (!variable.HasFlag(VarFlag.VarAccess))
				{
					return "ERROR";
				}
				op = Operator.VarAccess;
			}
			return Common.SCANNER.GetOperatorText(op);
		}

		public bool ExpressionHasSideEffects(IExpression expression, ISignature4 signature, IPreCompileContext9 pcc)
		{
			SideEffectVisitor sideEffectVisitor = new SideEffectVisitor();
			DoStandardTreeTraversal(sideEffectVisitor, signature, pcc, expression);
			return sideEffectVisitor.HasSideEffects;
		}

		public string MakeValidIdentifier(string stInput, bool bAllowUnicodeCharacters)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			if (stInput == null)
			{
				return null;
			}
			LStringBuilder val = new LStringBuilder();
			bool flag = false;
			foreach (char c in stInput)
			{
				if (IsIdentifierCharacter(c, bAllowUnicodeCharacters))
				{
					bool num = c == '_';
					if (!(num && flag))
					{
						val.Append(c);
					}
					flag = num;
				}
				else
				{
					if (!flag)
					{
						val.Append('_');
					}
					flag = true;
				}
			}
			if (val.get_Length() > 0 && !IsIdentifierStartCharacter(val.get_Item(0), bAllowUnicodeCharacters))
			{
				val.Insert(0, "_");
			}
			return ((object)val).ToString();
		}

		private static bool IsIdentifierStartCharacter(char c, bool bAllowUnicodeCharacters)
		{
			if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_')
			{
				return true;
			}
			if (bAllowUnicodeCharacters)
			{
				return char.IsLetter(c);
			}
			return false;
		}

		internal static bool IsIdentifierCharacter(char c, bool bAllowUnicodeCharacters)
		{
			if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_')
			{
				return true;
			}
			if (bAllowUnicodeCharacters)
			{
				return char.IsLetterOrDigit(c);
			}
			return false;
		}

		public bool IsQualifiedOnly(IUserdefType type, IPreCompileContext12 pcc, out string stNamespace)
		{
			stNamespace = null;
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (pcc == null)
			{
				throw new ArgumentNullException("pcc");
			}
			ISignature signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(type.SignatureId);
			if (signatureForPrecompileID != null && pcc.LibraryTable is ILibraryTable4 libraryTable && !string.IsNullOrEmpty(signatureForPrecompileID.LibraryPath))
			{
				bool? qualifiedOnlyRecursive = libraryTable.GetQualifiedOnlyRecursive(pcc, signatureForPrecompileID.LibraryPath);
				if (qualifiedOnlyRecursive.HasValue && qualifiedOnlyRecursive.Value)
				{
					stNamespace = libraryTable.GetLocalLibraryNamespaceRecursive(pcc, signatureForPrecompileID.LibraryPath);
					return true;
				}
			}
			return false;
		}

		public ILocalCodeWriter CreateLocalCodeWriter(IPreCompileContext localContext)
		{
			return new LocalCodeWriter((IPreCompileContext3)localContext);
		}

		public IEnumerable<IAttributedString> GetSignatureDeclarationTokens(ISignature sign, Guid appGuid, ILMPreCompileSet preCompileSet, ISignature signatureForCreatingScope)
		{
			IEnumerable<IAttributedString> declarationTokens = GetDeclarationTokens(sign, preCompileSet, signatureForCreatingScope);
			return CompactTokens(declarationTokens);
		}

		public IEnumerable<IAttributedString> GetTypeDeclarationTokens(IType type, ILMPreCompileSet preCompileSet, ISignature signatureForCreatingScope)
		{
			IEnumerable<IAttributedString> enumerable = null;
			if (type == null)
			{
				return _lstOnlyEmptyStringTokenContainingList;
			}
			enumerable = CreateAttributedString(type, preCompileSet, signatureForCreatingScope);
			return CompactTokens(enumerable);
		}

		public IEnumerable<IAttributedString> GetExpressionTokens(IExpression expr, ILMPreCompileSet preCompileSet, ISignature signatureForCreatingScope)
		{
			IEnumerable<IAttributedString> enumerable = null;
			if (expr == null)
			{
				return _lstOnlyEmptyStringTokenContainingList;
			}
			IPrecompileScope2 precompileScope = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.CreatePrecompileScope(preCompileSet, signatureForCreatingScope.ObjectGuid) as IPrecompileScope2;
			enumerable = (IEnumerable<IAttributedString>)UserdefTypeCollector.Instance.CollectUserdefTypes(expr, precompileScope);
			return CompactTokens(enumerable);
		}

		public IEnumerable<IAttributedString> GetPropertyDeclarationTokens(IVariable var, ISignature signGetter, ISignature signSetter, ILMPreCompileSet preCompileSet, ISignature signatureForCreatingScope)
		{
			yield return new PlainTextAttributedString($"PROPERTY {var.OrgName}: ");
			IEnumerable<AttributedString> enumerable = CreateAttributedString(var.Type, preCompileSet, signatureForCreatingScope);
			foreach (AttributedString item in enumerable)
			{
				yield return item;
			}
			string arg = string.Empty;
			if (var.HasAttribute(CompileAttributes.ATTRIBUTE_GET))
			{
				arg = ("GET " + Common.GetVisibilityString(Common.ExtractVisibilityFlags(signGetter))).Trim();
			}
			string text = string.Empty;
			if (var.HasAttribute(CompileAttributes.ATTRIBUTE_SET))
			{
				text = ("SET " + Common.GetVisibilityString(Common.ExtractVisibilityFlags(signSetter))).Trim();
			}
			string text2 = ((var.HasAttribute(CompileAttributes.ATTRIBUTE_GET) && var.HasAttribute(CompileAttributes.ATTRIBUTE_SET)) ? $"({arg}, {text})" : (var.HasAttribute(CompileAttributes.ATTRIBUTE_GET) ? $"({arg})" : ((!var.HasAttribute(CompileAttributes.ATTRIBUTE_SET)) ? string.Empty : $"({text})")));
			yield return new PlainTextAttributedString(" " + text2);
		}

		public string GetPOUTypeString(ISignature signature)
		{
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Expected O, but got Unknown
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Expected O, but got Unknown
			if (signature == null)
			{
				return string.Empty;
			}
			if (signature.POUType == Operator.Type)
			{
				if (signature.GetFlag(SignatureFlag.Union))
				{
					return "UNION";
				}
				if (signature.GetFlag(SignatureFlag.Structure))
				{
					return "STRUCT";
				}
				if (signature.GetFlag(SignatureFlag.Alias))
				{
					return "ALIAS";
				}
				return "TYPE";
			}
			if (signature.POUType == Operator.VarGlobal)
			{
				if (signature.GetFlag(SignatureFlag.Enum))
				{
					return "ENUM";
				}
				LStringBuilder val = new LStringBuilder();
				val.Append("VAR_GLOBAL");
				if (signature.All.Length != 0)
				{
					IVariable obj = signature.All[0];
					if (obj.GetFlag(VarFlag.Constant))
					{
						val.Append(" CONSTANT");
					}
					if (obj.GetFlag(VarFlag.Retain))
					{
						val.Append(" RETAIN");
					}
					if (obj.GetFlag(VarFlag.Persistent))
					{
						val.Append(" PERSISTENT");
					}
				}
				return ((object)val).ToString();
			}
			LStringBuilder val2 = new LStringBuilder();
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(string.Empty, bIncludeComments: false, bIncludeEndOfLines: false, bIncludePragmas: false, bIncludeWhitespaces: false);
			val2.Append(scanner.GetOperatorText(signature.POUType, bShort: true));
			return ((object)val2).ToString();
		}

		private static string QualifyStringType(IType type, string sizeExpression, string qualificationNamespace)
		{
			IExpression expression = Helpers.ParseExpression(sizeExpression);
			if (expression == null)
			{
				return sizeExpression;
			}
			string text = LegacySwitch.DumpQualifiedExpressionText(expression, qualificationNamespace);
			switch (type.Class)
			{
			case TypeClass.String:
				return "STRING(" + text + ")";
			case TypeClass.WString:
				return "WSTRING(" + text + ")";
			case TypeClass.XString:
				return "__XSTRING(" + text + ")";
			default:
				return sizeExpression;
			}
		}

		private static string ExtractSizeExpressionIf(string stType)
		{
			int num = stType.IndexOf("(");
			int num2 = stType.LastIndexOf(")");
			if (num >= 0 && num2 >= 0 && num2 > num + 1)
			{
				return stType.Substring(num + 1, num2 - num - 1);
			}
			return null;
		}

		public string PrependNamespaceToVariableTypename(int projHandle, Guid objGuid, IType type, string stVarName)
		{
			if (type == null)
			{
				return string.Empty;
			}
			string text = type.ToString();
			string text2 = ExtractSizeExpressionIf(text);
			bool flag = false;
			if (text2 != null)
			{
				flag = true;
				text = text2;
			}
			Guid applicationGuid = APEnvironmentFacade.Instance.GetApplicationGuid(objGuid, projHandle);
			ISignature signature = FindVarSignature(applicationGuid, stVarName);
			if (signature != null)
			{
				string namespaceFromLibProject = APEnvironmentFacade.Instance.GetNamespaceFromLibProject(projHandle, applicationGuid, signature.LibraryPath);
				if (!string.IsNullOrWhiteSpace(namespaceFromLibProject))
				{
					if (flag)
					{
						return QualifyStringType(type, text, namespaceFromLibProject);
					}
					if (type.Class == TypeClass.Enum || type.Class == TypeClass.Userdef)
					{
						return string.Join(Common.GetNamespaceDelimiterConsideringCompilerversion3_5_21_10(), namespaceFromLibProject, text);
					}
				}
			}
			return type.ToString();
		}

		public IEnumerable<ISignature2> GetImplementedInterfaces(ISignature2 sign, ILMPreCompileSet precom)
		{
			return InterfaceHelper.CollectFBInterfaces(sign, precom);
		}

		private ISignature FindVarSignature(Guid gdApp, string stVar)
		{
			IPreCompileContext precompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(gdApp);
			if (precompileContext != null)
			{
				IIdentifierInfo[] identifierInfo = precompileContext.GetIdentifierInfo(Guid.Empty, stVar);
				if (identifierInfo != null && identifierInfo.Length != 0)
				{
					return identifierInfo[0]?.Signature;
				}
			}
			return null;
		}

		private IEnumerable<IAttributedString> CompactTokens(IEnumerable<IAttributedString> tokens)
		{
			LStringBuilder sbNormalTextCaption = new LStringBuilder();
			foreach (IAttributedString token in tokens)
			{
				if (token is IPlainTextAttributedString)
				{
					sbNormalTextCaption.Append(token.Text);
					continue;
				}
				if (0 < sbNormalTextCaption.get_Length())
				{
					yield return new PlainTextAttributedString(((object)sbNormalTextCaption).ToString());
					sbNormalTextCaption.Clear();
				}
				yield return token;
			}
			if (0 < sbNormalTextCaption.get_Length())
			{
				yield return new PlainTextAttributedString(((object)sbNormalTextCaption).ToString());
			}
		}

		private IEnumerable<IAttributedString> GetDeclarationTokens(ISignature sign, ILMPreCompileSet preCompileSet, ISignature signatureForCreatingScope)
		{
			List<IAttributedString> list = new List<IAttributedString>();
			list.Add(new PlainTextAttributedString(GetPOUTypeString(sign) + " " + Common.GetVisibilityString(sign) + " " + sign.OrgName));
			AddReturnValueDeclarationTokens(list, sign, preCompileSet, signatureForCreatingScope);
			AddBaseExpressionsDeclarationTokens(list, sign, preCompileSet, signatureForCreatingScope);
			AddInterfaceExpressionsDeclarationTokens(list, sign, preCompileSet, signatureForCreatingScope);
			AddAliasDeclarationTokens(list, sign, preCompileSet, signatureForCreatingScope);
			return list;
		}

		private void AddReturnValueDeclarationTokens(List<IAttributedString> lstDeclarationTokens, ISignature sign, ILMPreCompileSet preCompileSet, ISignature signatureForCreatingScope)
		{
			IVariable variable = Array.Find(sign.Outputs, (IVariable v) => v.Name == sign.Name);
			if (variable == null)
			{
				return;
			}
			lstDeclarationTokens.Add(new PlainTextAttributedString(": "));
			foreach (AttributedString item in CreateAttributedString(variable.Type, preCompileSet, signatureForCreatingScope))
			{
				lstDeclarationTokens.Add(item);
			}
		}

		private void AddBaseExpressionsDeclarationTokens(List<IAttributedString> lstDeclarationTokens, ISignature sign, ILMPreCompileSet preCompileSet, ISignature signatureForCreatingScope)
		{
			IExpression[] baseExpressions = GetBaseExpressions(sign);
			AddExpressionsDeclarationTokens(lstDeclarationTokens, baseExpressions, "EXTENDS", preCompileSet, signatureForCreatingScope);
		}

		private void AddInterfaceExpressionsDeclarationTokens(List<IAttributedString> lstDeclarationTokens, ISignature sign, ILMPreCompileSet preCompileSet, ISignature signatureForCreatingScope)
		{
			IExpression[] interfaceExpressions = GetInterfaceExpressions(sign);
			AddExpressionsDeclarationTokens(lstDeclarationTokens, interfaceExpressions, "IMPLEMENTS", preCompileSet, signatureForCreatingScope);
		}

		private void AddExpressionsDeclarationTokens(List<IAttributedString> lstDeclarationTokens, IExpression[] expressions, string stKeyword, ILMPreCompileSet preCompileSet, ISignature signatureForCreatingScope)
		{
			for (int i = 0; i < expressions.Length; i++)
			{
				lstDeclarationTokens.Add(new PlainTextAttributedString((i == 0) ? (" " + stKeyword + " ") : ", "));
				foreach (AttributedString item in CreateAttributedString(expressions[i], preCompileSet, signatureForCreatingScope))
				{
					lstDeclarationTokens.Add(item);
				}
			}
		}

		private void AddAliasDeclarationTokens(List<IAttributedString> lstDeclarationTokens, ISignature sign, ILMPreCompileSet preCompileSet, ISignature signatureForCreatingScope)
		{
			if (!sign.GetFlag(SignatureFlag.Alias) || sign.All.Length == 0)
			{
				return;
			}
			lstDeclarationTokens.Add(new PlainTextAttributedString(" : "));
			foreach (AttributedString item in CreateAttributedString(sign.All[0].Type, preCompileSet, signatureForCreatingScope))
			{
				lstDeclarationTokens.Add(item);
			}
		}

		private IExpression[] GetBaseExpressions(ISignature sign)
		{
			if (!(sign is ISignature2 signature))
			{
				return Array.Empty<IExpression>();
			}
			if (Operator.Interface == sign.POUType)
			{
				List<IExpression> list = new List<IExpression>();
				if (signature.BaseExpression != null)
				{
					list.Add(signature.BaseExpression);
				}
				if (signature.InterfaceExpressions != null)
				{
					list.AddRange(signature.InterfaceExpressions);
				}
				return list.ToArray();
			}
			if (signature.BaseExpression == null)
			{
				return Array.Empty<IExpression>();
			}
			return new IExpression[1] { signature.BaseExpression };
		}

		private IExpression[] GetInterfaceExpressions(ISignature sign)
		{
			if (Operator.Interface == sign.POUType)
			{
				return Array.Empty<IExpression>();
			}
			if (sign is ISignature2 signature && signature.InterfaceExpressions != null)
			{
				return signature.InterfaceExpressions;
			}
			return Array.Empty<IExpression>();
		}

		private static IEnumerable<AttributedString> CreateAttributedString(IType type, ILMPreCompileSet preCompileSet, ISignature signatureForCreatingScope)
		{
			IPrecompileScope2 precompileScope = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.CreatePrecompileScope(preCompileSet, signatureForCreatingScope.ObjectGuid) as IPrecompileScope2;
			LList<AttributedString> val = UserdefTypeCollector.Instance.CollectUserdefTypes(type, precompileScope);
			foreach (AttributedString item in val)
			{
				yield return item;
			}
		}

		private static IEnumerable<AttributedString> CreateAttributedString(IExpression typeExpr, ILMPreCompileSet preCompileSet, ISignature signatureForCreatingScope)
		{
			if (typeExpr.Type != null)
			{
				return CreateAttributedString(typeExpr.Type, preCompileSet, signatureForCreatingScope);
			}
			bool flag = false;
			if (typeExpr is ISystemScopeExpression systemScopeExpression && systemScopeExpression.Base is IVariableExpression variableExpression)
			{
				flag = "IQueryInterface".Equals(variableExpression.ToString(), StringComparison.InvariantCultureIgnoreCase);
			}
			if (!flag)
			{
				ISignature signature = (APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.CreatePrecompileScope(preCompileSet, signatureForCreatingScope.ObjectGuid) as IPrecompileScope2)?.FindSignatureGlobal(typeExpr);
				bool bAvailable = signature != null;
				return new ExpressionAttributedString[1]
				{
					new ExpressionAttributedString(typeExpr, bAvailable)
				};
			}
			return new PlainTextAttributedString[1]
			{
				new PlainTextAttributedString(typeExpr.ToString())
			};
		}

		public bool TryGetEnumerationItems(ILMPreCompileSet rootPcc, IPrecompileScope2 typeScope, IType type, out IEnumerable<string> enumMemberNames)
		{
			return EnumInfoService.TryGetEnumerationItems(rootPcc, typeScope, type, out enumMemberNames);
		}
	}
}
