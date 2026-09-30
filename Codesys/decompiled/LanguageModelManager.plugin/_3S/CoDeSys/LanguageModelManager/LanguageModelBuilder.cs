using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;
using _3S.CoDeSys.LanguageModelManager.GreenTrees;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LanguageModelManager.Signature;
using _3S.CoDeSys.LanguageModelManager.Variable;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000119 RID: 281
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "The class is a factory for all LM-Items. No brain overload expected.")]
	internal class LanguageModelBuilder : _ILanguageModelBuilder8, _ILanguageModelBuilder7, _ILanguageModelBuilder6, _ILanguageModelBuilder5, _ILanguageModelBuilder4, _ILanguageModelBuilder3, _ILanguageModelBuilder2, _ILanguageModelBuilder, ILanguageModelBuilder12, ILanguageModelBuilder11, ILanguageModelBuilder10, ILanguageModelBuilder9, ILanguageModelBuilder8, ILanguageModelBuilder7, ILanguageModelBuilder6, ILanguageModelBuilder5, ILanguageModelBuilder4, ILanguageModelBuilder3, ILanguageModelBuilder2, ILanguageModelBuilder, ILMSerializableTypeFactory2, ILMSerializableTypeFactory
	{
		// Token: 0x060014E9 RID: 5353 RVA: 0x00002476 File Offset: 0x00001476
		private LanguageModelBuilder()
		{
		}

		// Token: 0x170005CE RID: 1486
		// (get) Token: 0x060014EA RID: 5354 RVA: 0x0003C951 File Offset: 0x0003B951
		public static LanguageModelBuilder Singleton { get; } = new LanguageModelBuilder();

		// Token: 0x060014EB RID: 5355 RVA: 0x0003C958 File Offset: 0x0003B958
		public _IVariable CreateVariable(ISourcePosition sp)
		{
			return new Variable(sp as SourcePosition);
		}

		// Token: 0x060014EC RID: 5356 RVA: 0x0003C965 File Offset: 0x0003B965
		public _IVariable CreateVariable(string stName, _IType type)
		{
			return new Variable
			{
				Name = stName,
				_Type = type
			};
		}

		// Token: 0x060014ED RID: 5357 RVA: 0x0003C97A File Offset: 0x0003B97A
		public _IImplicitReferenceVariable CreateImplicitReferenceVariable(int SignatureId, _IVariable var)
		{
			return new ImplicitReferenceVariable
			{
				SignatureId = SignatureId,
				Var = var
			};
		}

		// Token: 0x060014EE RID: 5358 RVA: 0x0003C98F File Offset: 0x0003B98F
		public IDataLocation2 CreateDataLocation(ushort usArea, int iOffset)
		{
			return new DataLocation(usArea, iOffset);
		}

		// Token: 0x060014EF RID: 5359 RVA: 0x0003C998 File Offset: 0x0003B998
		public IDataLocation2 CreateRelativeDataLocation(int iOffset)
		{
			return new RelativeLocation(iOffset);
		}

		// Token: 0x060014F0 RID: 5360 RVA: 0x0003C9A0 File Offset: 0x0003B9A0
		public IDataLocation2 CreateRelativeDataLocation(int iOffset, DataLocationFlag dlf)
		{
			return new RelativeLocation(iOffset, dlf);
		}

		// Token: 0x060014F1 RID: 5361 RVA: 0x0003C9A9 File Offset: 0x0003B9A9
		public IDataLocation2 CreateRelativeBitDataLocation(int iOffset, byte byBitLocation)
		{
			return new RelativeBitLocation(iOffset, byBitLocation);
		}

		// Token: 0x060014F2 RID: 5362 RVA: 0x0003C9B2 File Offset: 0x0003B9B2
		public IDataLocation2 CreateBitDataLocation(ushort usArea, int iOffset, byte byBitLocation)
		{
			return new BitDataLocation(usArea, iOffset, byBitLocation);
		}

		// Token: 0x060014F3 RID: 5363 RVA: 0x0003C9BC File Offset: 0x0003B9BC
		public IProcessImageLocation CreateProcessImageLocation(IDataLocation datloc, int nSize, DirectVariableLocation dirvarlocation)
		{
			return new ProcessImageLocation(datloc, nSize, dirvarlocation);
		}

		// Token: 0x060014F4 RID: 5364 RVA: 0x0003C9C6 File Offset: 0x0003B9C6
		public _IArea CreateArea()
		{
			return new Area();
		}

		// Token: 0x060014F5 RID: 5365 RVA: 0x0003C9CD File Offset: 0x0003B9CD
		public _IArea CreateArea(DataSegmentFlags dsf, AreaFlags af)
		{
			return new Area(dsf, af);
		}

		// Token: 0x060014F6 RID: 5366 RVA: 0x0003C9D6 File Offset: 0x0003B9D6
		public IPersistentArea CreateAreaPersistent(IArea area, uint uiChecksum)
		{
			return new AreaPersistent(area, uiChecksum);
		}

		// Token: 0x060014F7 RID: 5367 RVA: 0x0003C9DF File Offset: 0x0003B9DF
		public ICodePosition CreateCodePosition(ISourcePosition sp, AccessFlag access)
		{
			return new CodePosition(sp, access);
		}

		// Token: 0x060014F8 RID: 5368 RVA: 0x0003C9E8 File Offset: 0x0003B9E8
		public ILibPlaceholderIdentification CreateLibPlaceholderIdentification(IDeviceIdentification devid, ILibraryPlaceholder2 libplc)
		{
			return new LibraryPlaceholdersLegacy.LibPlaceholderIdentification(devid, libplc);
		}

		// Token: 0x060014F9 RID: 5369 RVA: 0x0003C9F1 File Offset: 0x0003B9F1
		public _IStepInPosition CreateStepInPosition(int nSignId, IBreakpoint stepoutBreakpoint)
		{
			return new StepInPosition(nSignId, stepoutBreakpoint);
		}

		// Token: 0x060014FA RID: 5370 RVA: 0x0003C9FA File Offset: 0x0003B9FA
		public _ICompileContext CreateCompileContext(KindOfContext kindof, _ICompileContext comconOld, _ICompileContext comconParent, Guid objectGuid)
		{
			return new CompileContext(kindof, comconOld as CompileContext, comconParent as CompileContext, objectGuid);
		}

		// Token: 0x060014FB RID: 5371 RVA: 0x0003CA10 File Offset: 0x0003BA10
		public _ISignature CreateSignature()
		{
			return new Signature();
		}

		// Token: 0x060014FC RID: 5372 RVA: 0x0003CA17 File Offset: 0x0003BA17
		public _ICompiledPOU CreateCompiledPOU(string stName)
		{
			return new CompiledPOU(stName);
		}

		// Token: 0x060014FD RID: 5373 RVA: 0x0003CA20 File Offset: 0x0003BA20
		public ILanguageModel CreateLanguageModelOfXml(string stContent, List<List<string>> stringlistTable)
		{
			IList<IList<string>> list = null;
			if (stringlistTable != null)
			{
				list = new List<IList<string>>();
				foreach (List<string> item in stringlistTable)
				{
					list.Add(item);
				}
			}
			return LanguageModelHandling.CreateLanguageModelOfXml(stContent, list);
		}

		// Token: 0x060014FE RID: 5374 RVA: 0x0003CA80 File Offset: 0x0003BA80
		public ILateCompileContext CreateLateCompileContext(_ICompileContext comconNew, _ICompileContext comconRef)
		{
			return new LateCompileContext(comconNew, comconRef);
		}

		// Token: 0x060014FF RID: 5375 RVA: 0x0003CA89 File Offset: 0x0003BA89
		public _IMemoryManager CreateMemMan(int nSize, int nBaseAddress)
		{
			return new MemMan(nSize, nBaseAddress);
		}

		// Token: 0x06001500 RID: 5376 RVA: 0x0003CA92 File Offset: 0x0003BA92
		public ILanguageModelList CreateLanguageModelList()
		{
			return new LanguageModelList();
		}

		// Token: 0x06001501 RID: 5377 RVA: 0x0003CA99 File Offset: 0x0003BA99
		public _ILiteralValue CreateLiteralValue(double d)
		{
			return new LiteralValue(d);
		}

		// Token: 0x06001502 RID: 5378 RVA: 0x0003CAA6 File Offset: 0x0003BAA6
		public _ILiteralValue CreateLiteralValue(long l)
		{
			return new LiteralValue(l);
		}

		// Token: 0x06001503 RID: 5379 RVA: 0x0003CAB3 File Offset: 0x0003BAB3
		public _ILiteralValue CreateLiteralValue(ulong ul)
		{
			return new LiteralValue(ul);
		}

		// Token: 0x06001504 RID: 5380 RVA: 0x0003CAC0 File Offset: 0x0003BAC0
		public _ILiteralValue CreateLiteralValue(string s)
		{
			return new LiteralValue(s);
		}

		// Token: 0x06001505 RID: 5381 RVA: 0x0003CACD File Offset: 0x0003BACD
		public _ILiteralValue CreateLiteralValue(bool b)
		{
			return new LiteralValue(b);
		}

		// Token: 0x06001506 RID: 5382 RVA: 0x0003CADA File Offset: 0x0003BADA
		public ILanguageModel CreateApplicationLanguageModel(Guid applicationGuid, Guid deviceGuid)
		{
			return new LanguageModel(applicationGuid, deviceGuid, Guid.Empty, null);
		}

		// Token: 0x06001507 RID: 5383 RVA: 0x0003CAE9 File Offset: 0x0003BAE9
		public ILanguageModel CreateLibraryLanguageModel(string stLibraryId)
		{
			return new LanguageModel(Guid.Empty, Guid.Empty, Guid.Empty, stLibraryId);
		}

		// Token: 0x06001508 RID: 5384 RVA: 0x0003CB00 File Offset: 0x0003BB00
		public ILanguageModel CreateLanguageModel(Guid applicationGuid, Guid deviceGuid, Guid languageModelControlObjectGuid, string stLibraryId)
		{
			return new LanguageModel(applicationGuid, deviceGuid, languageModelControlObjectGuid, stLibraryId);
		}

		// Token: 0x06001509 RID: 5385 RVA: 0x0003CB0C File Offset: 0x0003BB0C
		public IDeviceIdentification CreateDeviceIdentification(int nType, string stId, string stVersion)
		{
			return new DeviceIdentification(nType, stId, stVersion);
		}

		// Token: 0x0600150A RID: 5386 RVA: 0x0003CB16 File Offset: 0x0003BB16
		public ILMDevice CreateLMForDevice(string stName, IDeviceIdentification devid)
		{
			return new LMDevice(stName, devid);
		}

		// Token: 0x0600150B RID: 5387 RVA: 0x0003CB1F File Offset: 0x0003BB1F
		public ILMApplication CreateLMForApplication(Guid parentApp, string stDeviceName, string stApplicationName, IDeviceIdentification devid)
		{
			return new LMApplication(parentApp, stDeviceName, stApplicationName, devid);
		}

		// Token: 0x0600150C RID: 5388 RVA: 0x0003CB2B File Offset: 0x0003BB2B
		public ITaskInfo CreateTaskInfo(Guid guidTask, string stTaskName)
		{
			return new TaskInfo(Guid.Empty, guidTask, stTaskName, null);
		}

		// Token: 0x0600150D RID: 5389 RVA: 0x0003CB3A File Offset: 0x0003BB3A
		public ITaskInfo2 CreateTaskInfo(Guid guidTaskInfo, string stName, string stParentTaskName)
		{
			return new TaskInfo(Guid.Empty, guidTaskInfo, stName, stParentTaskName);
		}

		// Token: 0x0600150E RID: 5390 RVA: 0x0003CB49 File Offset: 0x0003BB49
		public ILMTaskList CreateTaskList(Guid guidTaskConfig)
		{
			return new LMTaskList(guidTaskConfig);
		}

		// Token: 0x0600150F RID: 5391 RVA: 0x0003CB51 File Offset: 0x0003BB51
		public ILibParameterTable CreateLibraryParameterTable()
		{
			return new LibParameterTable();
		}

		// Token: 0x06001510 RID: 5392 RVA: 0x0003CB58 File Offset: 0x0003BB58
		public ILMLibraryInfo CreateLibInfo(string stId, string stDefaultNamespace, string stNamespace, bool bSystemLibrary, bool bPublishSymbols, bool bLinkAllContent, bool bLinkInSimulation, bool bQualifiedOnly)
		{
			return new LMLibraryInfo
			{
				Identification = stId,
				DefaultNamespace = stDefaultNamespace,
				Namespace = stNamespace,
				SystemLibrary = bSystemLibrary,
				PublishSymbols = bPublishSymbols,
				LinkAllContent = bLinkAllContent,
				LinkInSimulation = bLinkInSimulation,
				QualifiedOnly = bQualifiedOnly
			};
		}

		// Token: 0x06001511 RID: 5393 RVA: 0x0003CBA7 File Offset: 0x0003BBA7
		public ILMPlaceholderInfo CreateLibraryPlaceholder(string stName, string stDefaultLibraryId, string stNamespace, bool bPublishSymbols, bool bLinkAllContent, bool bLinkInSimulation, Guid guidResolver)
		{
			return new LMPlaceholderInfo(new LibraryPlaceholder(stName, stDefaultLibraryId, stNamespace, bPublishSymbols, bLinkAllContent, bLinkInSimulation, guidResolver));
		}

		// Token: 0x06001512 RID: 5394 RVA: 0x0003CBBE File Offset: 0x0003BBBE
		public ILMPlaceholderInfo CreateLibraryPlaceholder(string stName, string stDefaultLibraryId, string stNamespace, bool bPublishSymbols, bool bLinkAllContent, bool bLinkInSimulation, Guid guidResolver, Guid libManGuid)
		{
			return new LMPlaceholderInfo(new LibraryPlaceholder(stName, stDefaultLibraryId, stNamespace, bPublishSymbols, bLinkAllContent, bLinkInSimulation, guidResolver, libManGuid));
		}

		// Token: 0x06001513 RID: 5395 RVA: 0x0003CBD7 File Offset: 0x0003BBD7
		public ILMLibraryList CreateLibraryList(Guid guidLibMan)
		{
			return new LMLibraryList(guidLibMan);
		}

		// Token: 0x06001514 RID: 5396 RVA: 0x0003CBDF File Offset: 0x0003BBDF
		public ILMLibraryList CreateLibraryList(Guid guidLibMan, string stLibraryId)
		{
			return new LMLibraryList(guidLibMan, stLibraryId);
		}

		// Token: 0x06001515 RID: 5397 RVA: 0x0003CBE8 File Offset: 0x0003BBE8
		public ILMPOU CreatePou(string stName, Guid guidPOU)
		{
			return new LMPOU(stName, guidPOU);
		}

		// Token: 0x06001516 RID: 5398 RVA: 0x0003CBF1 File Offset: 0x0003BBF1
		public ILMGlobVarlist CreateGlobVarlist(string stName, Guid guidGVL)
		{
			return new LMGlobVarlist(stName, guidGVL);
		}

		// Token: 0x06001517 RID: 5399 RVA: 0x0003CBFA File Offset: 0x0003BBFA
		public ILMDataType CreateDataType(string stName, Guid guidDUT)
		{
			return new LMDataType(stName, guidDUT);
		}

		// Token: 0x06001518 RID: 5400 RVA: 0x0003CC03 File Offset: 0x0003BC03
		public IExprement DuplicateExprement(IExprement expIn)
		{
			return (expIn as Exprement).Duplicate();
		}

		// Token: 0x06001519 RID: 5401 RVA: 0x0003CC10 File Offset: 0x0003BC10
		public ISequenceStatement2 ParseSTSnippet(string stSnippet)
		{
			return this.ParseSTSnippet(stSnippet, true);
		}

		// Token: 0x0600151A RID: 5402 RVA: 0x0003CC1A File Offset: 0x0003BC1A
		public IExpression ParseInitialisation(string stExpression)
		{
			return this.ParseInitialisation(stExpression, true);
		}

		// Token: 0x0600151B RID: 5403 RVA: 0x0003CC24 File Offset: 0x0003BC24
		public ICompiledType ParseType(string stType)
		{
			return this.ParseType(stType, true);
		}

		// Token: 0x0600151C RID: 5404 RVA: 0x0003CC2E File Offset: 0x0003BC2E
		public IExprementPosition CreateExprementPosition(long lPosition)
		{
			return new ExprementPosition(lPosition, 0);
		}

		// Token: 0x0600151D RID: 5405 RVA: 0x0003CC37 File Offset: 0x0003BC37
		public IExprementPosition CreateExprementPosition(long lPosition, short sPositionOffset)
		{
			return new ExprementPosition(lPosition, sPositionOffset);
		}

		// Token: 0x0600151E RID: 5406 RVA: 0x0003CC40 File Offset: 0x0003BC40
		public IMessage CreateCompilerMessage(IExprementPosition position, IExprement exp, string stMessage, Severity severity, ShowAttribute showatt)
		{
			IMinimalPosition position2 = null;
			if (position != null)
			{
				position2 = MinimalPosition.CreateMinimalPosition(position.Position, position.PositionOffset);
			}
			CompilerMessage compilerMessage = new CompilerMessage(position2, stMessage, severity, (short)stMessage.Length, MessageId.None);
			compilerMessage.ShowAttribute = showatt;
			(exp as Exprement).AddMessage(compilerMessage);
			return compilerMessage;
		}

		// Token: 0x0600151F RID: 5407 RVA: 0x0003CC8C File Offset: 0x0003BC8C
		public IMessage4 CreateCompilerMessage(IExprementPosition position, IExprement exp, string stMessage, Severity severity, ShowAttribute showatt, uint uiMessageId, string stMessagePrefix)
		{
			IMinimalPosition position2 = null;
			if (position != null)
			{
				position2 = MinimalPosition.CreateMinimalPosition(position.Position, position.PositionOffset);
			}
			SpecialCompilerMessage specialCompilerMessage = new SpecialCompilerMessage(position2, stMessage, severity, (short)stMessage.Length, uiMessageId, stMessagePrefix);
			specialCompilerMessage.ShowAttribute = showatt;
			(exp as Exprement).AddMessage(specialCompilerMessage);
			return specialCompilerMessage;
		}

		// Token: 0x06001520 RID: 5408 RVA: 0x0003CCDC File Offset: 0x0003BCDC
		public IMessage4 CreateCompilerMessage(IExprementPosition position, IExprement exp, string stMessage, Guid gdObject, short sLength, Severity severity, ShowAttribute showatt)
		{
			IMinimalPosition position2 = null;
			if (position != null)
			{
				position2 = MinimalPosition.CreateMinimalPosition(position.Position, position.PositionOffset);
			}
			CompilerMessage compilerMessage = new CompilerMessage(position2, stMessage, severity, sLength, MessageId.None);
			compilerMessage.ShowAttribute = showatt;
			compilerMessage.ObjectGuid = gdObject;
			(exp as Exprement).AddMessage(compilerMessage);
			return compilerMessage;
		}

		// Token: 0x06001521 RID: 5409 RVA: 0x0003CD2B File Offset: 0x0003BD2B
		public _ICompilerMessage CreateCompilerMessage()
		{
			return new CompilerMessage();
		}

		// Token: 0x06001522 RID: 5410 RVA: 0x0003CD32 File Offset: 0x0003BD32
		public _ICompilerMessage CreateCompilerMessage(ISourcePosition position, string stError, Severity severity, MessageId Number)
		{
			return new CompilerMessage(position, stError, severity, Number);
		}

		// Token: 0x06001523 RID: 5411 RVA: 0x0003CD3E File Offset: 0x0003BD3E
		public _ICompilerMessage CreateCompilerMessage(IMinimalPosition position, string stError, Severity severity, short sLength, MessageId Number)
		{
			return new CompilerMessage(position, stError, severity, sLength, Number);
		}

		// Token: 0x06001524 RID: 5412 RVA: 0x0003CD4C File Offset: 0x0003BD4C
		public IDirectVariable CreateDirectVariable(DirectVariableLocation location, DirectVariableSize size, int nOffset)
		{
			return new DirectVariable(location, size, new int[]
			{
				nOffset
			});
		}

		// Token: 0x06001525 RID: 5413 RVA: 0x0003CD5F File Offset: 0x0003BD5F
		public IDirectVariable CreateDirectVariable(DirectVariableLocation location, DirectVariableSize size, int nOffset, int nBitOffset)
		{
			return new DirectVariable(location, size, new int[]
			{
				nOffset,
				nBitOffset
			});
		}

		// Token: 0x06001526 RID: 5414 RVA: 0x0003CD77 File Offset: 0x0003BD77
		public IDirectVariable CreateDirectVariable(DirectVariableLocation location, DirectVariableSize size, int[] nComponents)
		{
			return new DirectVariable(location, size, nComponents);
		}

		// Token: 0x06001527 RID: 5415 RVA: 0x0003CD81 File Offset: 0x0003BD81
		public IDirectVariable CreateIncompleteDirectVariable(DirectVariableLocation dirvarlocation)
		{
			return new DirectVariable(dirvarlocation, true);
		}

		// Token: 0x06001528 RID: 5416 RVA: 0x0003CD8A File Offset: 0x0003BD8A
		public ICompiledType CreateSimpleType(TypeClass type)
		{
			return TypeTable.Get(type);
		}

		// Token: 0x06001529 RID: 5417 RVA: 0x0003CD92 File Offset: 0x0003BD92
		public ICompiledType CreateComplexType(string stType, out IMessage message)
		{
			return CompilerProxy.CreateParser(stType, true).ParseTypeDeclaration(out message);
		}

		// Token: 0x0600152A RID: 5418 RVA: 0x0003CDA1 File Offset: 0x0003BDA1
		public void SetExprementPosition(IExprementPosition expPos, IExprement exp)
		{
			if (expPos != null)
			{
				(exp as Exprement).SetPositionIntern(MinimalPosition.CreateMinimalPosition(expPos.Position, expPos.PositionOffset));
			}
		}

		// Token: 0x0600152B RID: 5419 RVA: 0x0003CDC4 File Offset: 0x0003BDC4
		public IEmptyStatement CreateEmptyStatement(IExprementPosition pos)
		{
			EmptyStatement emptyStatement = new EmptyStatement();
			this.SetExprementPosition(pos, emptyStatement);
			return emptyStatement;
		}

		// Token: 0x0600152C RID: 5420 RVA: 0x0003CDE0 File Offset: 0x0003BDE0
		public IWhileStatement CreateWhileStatement(IExprementPosition pos, IExpression expCondition, ISequenceStatement2 seqControlled)
		{
			if (expCondition == null)
			{
				throw new ArgumentNullException("expCondition");
			}
			if (seqControlled == null)
			{
				throw new ArgumentNullException("seqControlled");
			}
			IWhileStatement whileStatement = new WhileStatement(expCondition as Expression, seqControlled as Statement);
			this.SetExprementPosition(pos, whileStatement);
			return whileStatement;
		}

		// Token: 0x0600152D RID: 5421 RVA: 0x0003CE24 File Offset: 0x0003BE24
		public IRepeatStatement CreateRepeatStatement(IExprementPosition pos, IExpression expCondition, ISequenceStatement2 seqControlled)
		{
			if (expCondition == null)
			{
				throw new ArgumentNullException("expCondition");
			}
			if (seqControlled == null)
			{
				throw new ArgumentNullException("seqControlled");
			}
			RepeatStatement repeatStatement = new RepeatStatement(expCondition as Expression, seqControlled as Statement);
			this.SetExprementPosition(pos, repeatStatement);
			return repeatStatement;
		}

		// Token: 0x0600152E RID: 5422 RVA: 0x0003CE68 File Offset: 0x0003BE68
		public ICaseRangeExpression CreateCaseRangeExpression(IExprementPosition pos, IExpression expLow, IExpression expHigh)
		{
			if (expLow == null)
			{
				throw new ArgumentNullException("expLow");
			}
			if (expHigh == null)
			{
				throw new ArgumentNullException("expHigh");
			}
			CaseRangeExpression caseRangeExpression = new CaseRangeExpression(expLow as Expression, expHigh as Expression);
			this.SetExprementPosition(pos, caseRangeExpression);
			return caseRangeExpression;
		}

		// Token: 0x0600152F RID: 5423 RVA: 0x0003CEAC File Offset: 0x0003BEAC
		public ICaseLabelStatement CreateCaseLabelStatement(IExprementPosition pos, List<IExpression> cases)
		{
			if (cases == null)
			{
				throw new ArgumentNullException("cases");
			}
			CaseLabelStatement caseLabelStatement = new CaseLabelStatement();
			this.SetExprementPosition(pos, caseLabelStatement);
			foreach (IExpression expression in cases)
			{
				caseLabelStatement.AddCase(expression as Expression);
			}
			return caseLabelStatement;
		}

		// Token: 0x06001530 RID: 5424 RVA: 0x0003CF1C File Offset: 0x0003BF1C
		public ICase CreateCase(ICaseLabelStatement caslabst, ISequenceStatement2 stateControlled)
		{
			if (caslabst == null)
			{
				throw new ArgumentNullException("caslabst");
			}
			if (stateControlled == null)
			{
				throw new ArgumentNullException("stateControlled");
			}
			return new Case(caslabst as CaseLabelStatement, stateControlled as Statement);
		}

		// Token: 0x06001531 RID: 5425 RVA: 0x0003CF4C File Offset: 0x0003BF4C
		public ICaseStatement CreateCaseStatement(IExprementPosition pos, IExpression expSwitch, List<ICase> cases, ISequenceStatement2 stateElse)
		{
			if (expSwitch == null)
			{
				throw new ArgumentNullException("expSwitch");
			}
			if (cases == null)
			{
				throw new ArgumentNullException("cases");
			}
			CaseStatement caseStatement = new CaseStatement();
			this.SetExprementPosition(pos, caseStatement);
			foreach (ICase @case in cases)
			{
				caseStatement.AddCase(@case as Case);
			}
			caseStatement._Switch = (expSwitch as Expression);
			caseStatement._Else = (stateElse as Statement);
			return caseStatement;
		}

		// Token: 0x06001532 RID: 5426 RVA: 0x0003CFE4 File Offset: 0x0003BFE4
		public _IForStatement CreateForStatement()
		{
			return new ForStatement();
		}

		// Token: 0x06001533 RID: 5427 RVA: 0x0003CFEC File Offset: 0x0003BFEC
		public IForStatement CreateForStatement(IExprementPosition pos, IAssignmentExpression assCounterStart, IExpression expUpper, IExpression expBy, IStatement stateControlled)
		{
			if (assCounterStart == null)
			{
				throw new ArgumentNullException("assCounterStart");
			}
			if (expUpper == null)
			{
				throw new ArgumentNullException("expUpper");
			}
			if (expBy == null)
			{
				throw new ArgumentNullException("expBy");
			}
			if (stateControlled == null)
			{
				throw new ArgumentNullException("stateControlled");
			}
			ForStatement forStatement = new ForStatement();
			this.SetExprementPosition(pos, forStatement);
			forStatement._By = (expBy as Expression);
			forStatement._CounterStart = (assCounterStart as Expression);
			forStatement._UpperBound = (expUpper as Expression);
			forStatement._Controlled = (stateControlled as Statement);
			AssignmentExpression assignmentExpression = assCounterStart as AssignmentExpression;
			if (assignmentExpression != null)
			{
				_IExpression lvalue = assignmentExpression._LValue;
				OperatorExpression operatorExpression;
				if (forStatement._By.IsLiteral)
				{
					if ((expBy as LiteralExpression).Negative)
					{
						operatorExpression = new OperatorExpression(Operator.Ge);
					}
					else
					{
						operatorExpression = new OperatorExpression(Operator.Le);
					}
					operatorExpression.AddOperand(lvalue.Duplicate() as Expression);
					operatorExpression.AddOperand(forStatement._UpperBound.Duplicate() as Expression);
				}
				else
				{
					operatorExpression = new OperatorExpression(Operator.Or);
					OperatorExpression operatorExpression2 = new OperatorExpression(Operator.And);
					OperatorExpression operatorExpression3 = new OperatorExpression(Operator.Ge);
					operatorExpression3.AddOperand(forStatement._By.Duplicate() as Expression);
					operatorExpression3.AddOperand(LiteralExpression.CreateLiteralExpression(0L));
					OperatorExpression operatorExpression4 = new OperatorExpression(Operator.Le);
					operatorExpression4.AddOperand(lvalue.Duplicate() as Expression);
					operatorExpression4.AddOperand(forStatement._UpperBound.Duplicate() as Expression);
					operatorExpression2.AddOperand(operatorExpression3);
					operatorExpression2.AddOperand(operatorExpression4);
					OperatorExpression operatorExpression5 = new OperatorExpression(Operator.And);
					OperatorExpression operatorExpression6 = new OperatorExpression(Operator.Lt);
					operatorExpression6.AddOperand(forStatement._By.Duplicate() as Expression);
					operatorExpression6.AddOperand(LiteralExpression.CreateLiteralExpression(0L));
					OperatorExpression operatorExpression7 = new OperatorExpression(Operator.Ge);
					operatorExpression7.AddOperand(lvalue.Duplicate() as Expression);
					operatorExpression7.AddOperand(forStatement._UpperBound.Duplicate() as Expression);
					operatorExpression5.AddOperand(operatorExpression6);
					operatorExpression5.AddOperand(operatorExpression7);
					operatorExpression.AddOperand(operatorExpression2);
					operatorExpression.AddOperand(operatorExpression5);
				}
				forStatement._Condition = operatorExpression;
				OperatorExpression operatorExpression8 = new OperatorExpression(Operator.Plus);
				operatorExpression8.AddOperand(lvalue.Duplicate() as Expression);
				operatorExpression8.AddOperand(forStatement._By.Duplicate() as Expression);
				forStatement._Counter = new AssignmentExpression(lvalue.Duplicate() as Expression)
				{
					_RValue = operatorExpression8
				};
			}
			return forStatement;
		}

		// Token: 0x06001534 RID: 5428 RVA: 0x0003D268 File Offset: 0x0003C268
		public IExitStatement CreateExitStatement(IExprementPosition pos)
		{
			ExitStatement exitStatement = new ExitStatement();
			this.SetExprementPosition(pos, exitStatement);
			return exitStatement;
		}

		// Token: 0x06001535 RID: 5429 RVA: 0x0003D284 File Offset: 0x0003C284
		public IContinueStatement CreateContinueStatement(IExprementPosition pos)
		{
			ContinueStatement continueStatement = new ContinueStatement();
			this.SetExprementPosition(pos, continueStatement);
			return continueStatement;
		}

		// Token: 0x06001536 RID: 5430 RVA: 0x0003D2A0 File Offset: 0x0003C2A0
		public ISequenceStatement2 CreateSequenceStatement(IExprementPosition pos)
		{
			SequenceStatement sequenceStatement = new SequenceStatement();
			this.SetExprementPosition(pos, sequenceStatement);
			return sequenceStatement;
		}

		// Token: 0x06001537 RID: 5431 RVA: 0x0003D2BC File Offset: 0x0003C2BC
		public ISequenceStatement2 CreateSequenceStatement(IExprementPosition pos, List<IStatement> statements)
		{
			if (statements == null)
			{
				throw new ArgumentNullException("statements");
			}
			SequenceStatement sequenceStatement = new SequenceStatement(statements.Count);
			foreach (IStatement state in statements)
			{
				sequenceStatement.AddStatement(state);
			}
			this.SetExprementPosition(pos, sequenceStatement);
			return sequenceStatement;
		}

		// Token: 0x06001538 RID: 5432 RVA: 0x0003D330 File Offset: 0x0003C330
		public IAssignmentExpression CreateAssignmentExpression(IExprementPosition pos, IExpression expLeft, IExpression expRight)
		{
			return this.CreateAssignmentExpression(pos, expLeft, expRight, Operator.Assign);
		}

		// Token: 0x06001539 RID: 5433 RVA: 0x0003D340 File Offset: 0x0003C340
		public IExpressionStatement CreateAssignmentStatement(IExprementPosition pos, IExpression expLeft, IExpression expRight)
		{
			return this.CreateExpressionStatement(this.CreateAssignmentExpression(pos, expLeft, expRight, Operator.Assign));
		}

		// Token: 0x0600153A RID: 5434 RVA: 0x0003D358 File Offset: 0x0003C358
		public IAssignmentExpression CreateAssignmentExpression(IExprementPosition pos, IExpression expLValue, IExpression expRValue, Operator kindof)
		{
			if (kindof != Operator.Assign && kindof != Operator.FupAssign && kindof != Operator.AssignOut && kindof != Operator.SetAssign && kindof != Operator.RefAssign && kindof != Operator.ResetAssign)
			{
				throw new ArgumentException("No valid Operator enumeration value for assignment", "kindof");
			}
			if (expLValue == null)
			{
				throw new ArgumentNullException("expLValue");
			}
			if (expRValue == null)
			{
				throw new ArgumentNullException("expRValue");
			}
			AssignmentExpression assignmentExpression = new AssignmentExpression(expLValue as Expression);
			assignmentExpression._RValue = (expRValue as Expression);
			this.SetExprementPosition(pos, assignmentExpression);
			assignmentExpression.KindOf = kindof;
			return assignmentExpression;
		}

		// Token: 0x0600153B RID: 5435 RVA: 0x0003D3F0 File Offset: 0x0003C3F0
		public IElseIf2 CreateElseIf(IExprementPosition pos, IExpression expCondition, ISequenceStatement2 seqControlled)
		{
			if (expCondition == null)
			{
				throw new ArgumentNullException("expCondition");
			}
			if (seqControlled == null)
			{
				throw new ArgumentNullException("seqControlled");
			}
			ElseIf elseIf = new ElseIf(expCondition as Expression, seqControlled as Statement);
			this.SetExprementPosition(pos, elseIf);
			return elseIf;
		}

		// Token: 0x0600153C RID: 5436 RVA: 0x0003D434 File Offset: 0x0003C434
		public IIfStatement CreateIfStatement(IExprementPosition pos, IExpression expCondition, ISequenceStatement2 seqThen)
		{
			return this.CreateIfStatement(pos, expCondition, seqThen, null, null);
		}

		// Token: 0x0600153D RID: 5437 RVA: 0x0003D441 File Offset: 0x0003C441
		public IIfStatement CreateIfStatement(IExprementPosition pos, IExpression expCondition, ISequenceStatement2 seqThen, ISequenceStatement2 seqElse)
		{
			return this.CreateIfStatement(pos, expCondition, seqThen, seqElse, null);
		}

		// Token: 0x0600153E RID: 5438 RVA: 0x0003D450 File Offset: 0x0003C450
		public IIfStatement CreateIfStatement(IExprementPosition pos, IExpression expCondition, ISequenceStatement2 seqThen, ISequenceStatement2 seqElse, List<IElseIf2> elsifs)
		{
			if (expCondition == null)
			{
				throw new ArgumentNullException("expCondition");
			}
			if (seqThen == null)
			{
				throw new ArgumentNullException("seqThen");
			}
			IfStatement ifStatement = new IfStatement();
			this.SetExprementPosition(pos, ifStatement);
			ifStatement._Condition = (expCondition as Expression);
			if (elsifs != null)
			{
				foreach (IElseIf2 elseIf in elsifs)
				{
					ifStatement.AddElseIf(elseIf as _IElseIf);
				}
			}
			ifStatement._IfElse = (seqElse as Statement);
			ifStatement._IfThen = (seqThen as Statement);
			return ifStatement;
		}

		// Token: 0x0600153F RID: 5439 RVA: 0x0003D4F8 File Offset: 0x0003C4F8
		public IReturnStatement CreateReturnStatement(IExprementPosition pos, IExpression expCondition)
		{
			ReturnStatement returnStatement = new ReturnStatement();
			returnStatement._Condition = (expCondition as Expression);
			this.SetExprementPosition(pos, returnStatement);
			return returnStatement;
		}

		// Token: 0x06001540 RID: 5440 RVA: 0x0003D520 File Offset: 0x0003C520
		public IJumpStatement CreateJumpStatement(IExprementPosition pos, IExpression expCondition, string stLabel)
		{
			if (stLabel == null)
			{
				throw new ArgumentNullException("stLabel");
			}
			JumpStatement jumpStatement = new JumpStatement(stLabel);
			jumpStatement._Condition = (expCondition as Expression);
			this.SetExprementPosition(pos, jumpStatement);
			return jumpStatement;
		}

		// Token: 0x06001541 RID: 5441 RVA: 0x0003D558 File Offset: 0x0003C558
		public ILabelStatement CreateLabelStatement(IExprementPosition pos, string stLabel)
		{
			if (string.IsNullOrEmpty(stLabel))
			{
				throw new ArgumentException("Label null or empty string", "stLabel");
			}
			LabelStatement labelStatement = new LabelStatement(stLabel);
			this.SetExprementPosition(pos, labelStatement);
			return labelStatement;
		}

		// Token: 0x06001542 RID: 5442 RVA: 0x0003D590 File Offset: 0x0003C590
		public ICommentStatement CreateCommentStatement(IExprementPosition pos, string stComment)
		{
			if (stComment == null)
			{
				throw new ArgumentNullException("stComment");
			}
			CommentStatement commentStatement = new CommentStatement();
			commentStatement.Text = stComment;
			this.SetExprementPosition(pos, commentStatement);
			return commentStatement;
		}

		// Token: 0x06001543 RID: 5443 RVA: 0x0003D5C4 File Offset: 0x0003C5C4
		public IPragmaStatement CreatePragmaStatement(IExprementPosition pos, string stPragma)
		{
			if (stPragma == null)
			{
				throw new ArgumentNullException("stPragma");
			}
			PragmaStatement pragmaStatement = new PragmaStatement();
			pragmaStatement.Text = stPragma;
			this.SetExprementPosition(pos, pragmaStatement);
			return pragmaStatement;
		}

		// Token: 0x06001544 RID: 5444 RVA: 0x0003D5F8 File Offset: 0x0003C5F8
		public IStatement CreatePragmaStatement2(IExprementPosition pos, string pragma)
		{
			if (pragma == null)
			{
				throw new ArgumentNullException("pragma");
			}
			_IParser iparser = CompilerProxy.CreateParser(pragma, false);
			IMinimalPosition errorpos = (pos != null) ? MinimalPosition.CreateMinimalPosition(pos.Position, pos.PositionOffset) : null;
			bool flag;
			IStatement statement = iparser.ParsePragma(out flag, errorpos, pragma);
			if (statement == null)
			{
				statement = new PragmaStatement
				{
					Text = pragma
				};
			}
			this.SetExprementPosition(pos, statement);
			return statement;
		}

		// Token: 0x06001545 RID: 5445 RVA: 0x0003D655 File Offset: 0x0003C655
		public IPragmaStatement CreateMessageGuidPragmaStatement(Guid guidMessage)
		{
			return new MessageGuidPragmaStatement
			{
				MessageGuid = guidMessage
			};
		}

		// Token: 0x06001546 RID: 5446 RVA: 0x0003D663 File Offset: 0x0003C663
		public _IMessageGuidPragmaStatement CreateMessageGuidPragmaStatement(IToken token, Guid guid)
		{
			return new MessageGuidPragmaStatement(token, guid);
		}

		// Token: 0x06001547 RID: 5447 RVA: 0x0003D66C File Offset: 0x0003C66C
		public _IImplicitCodeSectionPragma CreateImplicitCodeSectionPragma(IToken token, string stText, bool bOn)
		{
			return new ImplicitCodeSectionPragmaStatement(token, stText, bOn);
		}

		// Token: 0x06001548 RID: 5448 RVA: 0x0003D676 File Offset: 0x0003C676
		public _ILocalSignatureIdPragma CreateLocalSignatureIdPragma(int nId, string stText)
		{
			return new LocalSignatureIdPragma(Token.Empty, stText, nId);
		}

		// Token: 0x06001549 RID: 5449 RVA: 0x0003D689 File Offset: 0x0003C689
		public _IEmbeddedLanguageStatement CreateEmbeddedLanguageStatement()
		{
			return new EmbeddedLanguageStatement();
		}

		// Token: 0x0600154A RID: 5450 RVA: 0x0003D690 File Offset: 0x0003C690
		public IPragmaStatement CreatePragmaAttributeStatement(IExprementPosition pos, string attributeName, string attributeValue)
		{
			if (attributeName == null)
			{
				throw new ArgumentNullException("attributeName");
			}
			if (attributeName == string.Empty)
			{
				throw new ArgumentException("attributeName");
			}
			string arg = string.Empty;
			if (!string.IsNullOrEmpty(attributeValue))
			{
				arg = string.Format(" := '{0}'", attributeValue);
			}
			return this.CreatePragmaStatement(pos, string.Format("attribute '{0}'{1}", attributeName, arg));
		}

		// Token: 0x0600154B RID: 5451 RVA: 0x0003D6F0 File Offset: 0x0003C6F0
		public IExpressionStatement CreateExpressionStatement(IExpression expInner)
		{
			if (expInner == null)
			{
				throw new ArgumentNullException("expInner");
			}
			return new ExpressionStatement(expInner as Expression)
			{
				_Position = (expInner as Expression)._Position
			};
		}

		// Token: 0x0600154C RID: 5452 RVA: 0x0003D71C File Offset: 0x0003C71C
		public IPOUDeclarationStatement CreatePOUDeclarationStatement(IExprementPosition pos, Operator opClass, string stName, ICompiledType typeReturnValue, List<IVariableDeclarationListStatement> vardecls, List<IExpression> extends, List<IExpression> implements, SignatureFlag accessflags)
		{
			POUDeclarationStatement poudeclarationStatement = new POUDeclarationStatement();
			this.SetExprementPosition(pos, poudeclarationStatement);
			poudeclarationStatement.Class = opClass;
			poudeclarationStatement.Name = stName;
			poudeclarationStatement.NameExpression = new VariableExpression(stName);
			poudeclarationStatement.Type = (typeReturnValue as _IType);
			poudeclarationStatement.DeclarationLists = vardecls;
			poudeclarationStatement.ExtendsList = extends;
			poudeclarationStatement.ImplementsList = implements;
			poudeclarationStatement.Access = accessflags;
			return poudeclarationStatement;
		}

		// Token: 0x0600154D RID: 5453 RVA: 0x0003D780 File Offset: 0x0003C780
		public IPOUDeclarationStatement CreatePOUDeclarationStatement(IExprementPosition pos, Operator opClass, string stName)
		{
			POUDeclarationStatement poudeclarationStatement = new POUDeclarationStatement();
			this.SetExprementPosition(pos, poudeclarationStatement);
			poudeclarationStatement.Class = opClass;
			poudeclarationStatement.Name = stName;
			poudeclarationStatement.NameExpression = new VariableExpression(stName);
			return poudeclarationStatement;
		}

		// Token: 0x0600154E RID: 5454 RVA: 0x0003D7B8 File Offset: 0x0003C7B8
		public IVariableDeclarationListStatement CreateVariableDeclarationListStatement(IExprementPosition pos, VarFlag varflag, ISequenceStatement seq)
		{
			VariableDeclarationListStatement variableDeclarationListStatement = new VariableDeclarationListStatement();
			this.SetExprementPosition(pos, variableDeclarationListStatement);
			if ((varflag & VarFlag.Global) == VarFlag.Global)
			{
				varflag |= VarFlag.Absolut;
			}
			if ((varflag & VarFlag.Static) == VarFlag.Static)
			{
				varflag |= VarFlag.Absolut;
			}
			if ((varflag & VarFlag.Union) == VarFlag.Union)
			{
				varflag = (varflag | VarFlag.Structure | VarFlag.Local);
			}
			if ((varflag & VarFlag.Structure) == VarFlag.Structure)
			{
				varflag |= VarFlag.Local;
			}
			if ((varflag & VarFlag.External) == VarFlag.External)
			{
				varflag |= VarFlag.Absolut;
			}
			variableDeclarationListStatement.Flags = varflag;
			variableDeclarationListStatement.VariableDeclaration = (seq as Statement);
			return variableDeclarationListStatement;
		}

		// Token: 0x0600154F RID: 5455 RVA: 0x0003D862 File Offset: 0x0003C862
		public IVariableDeclarationStatement CreateSimpleVariableDeclarationStatement(IExprementPosition pos, string stName, ICompiledType type)
		{
			return this.CreateInstanceVariableDeclarationStatement(pos, stName, type, null, null, null);
		}

		// Token: 0x06001550 RID: 5456 RVA: 0x0003D870 File Offset: 0x0003C870
		public IVariableDeclarationStatement CreateVariableDeclarationStatement(IExprementPosition pos, string stName, ICompiledType type, IExpression expInitial, IDirectVariable diraddr)
		{
			return this.CreateInstanceVariableDeclarationStatement(pos, stName, type, expInitial, diraddr, null);
		}

		// Token: 0x06001551 RID: 5457 RVA: 0x0003D880 File Offset: 0x0003C880
		public IVariableDeclarationStatement CreateVariableDeclarationStatement(IExprementPosition pos, List<string> stNames, ICompiledType type, IExpression expInitial)
		{
			return this.CreateInstanceVariableDeclarationStatementX(pos, stNames, type, expInitial, null, null);
		}

		// Token: 0x06001552 RID: 5458 RVA: 0x0003D890 File Offset: 0x0003C890
		public IVariableDeclarationStatement CreateInstanceVariableDeclarationStatement(IExprementPosition pos, string stName, ICompiledType type, IExpression expInitial, IDirectVariable diraddr, List<IAssignmentExpression> expInitMethodAssignments)
		{
			return this.CreateInstanceVariableDeclarationStatementX(pos, new List<string>
			{
				stName
			}, type, expInitial, diraddr, expInitMethodAssignments);
		}

		// Token: 0x06001553 RID: 5459 RVA: 0x0003D8BC File Offset: 0x0003C8BC
		internal IVariableDeclarationStatement CreateInstanceVariableDeclarationStatementX(IExprementPosition pos, List<string> stNames, ICompiledType type, IExpression expInitial, IDirectVariable diraddr, List<IAssignmentExpression> expInitMethodAssignments)
		{
			VariableDeclarationStatement variableDeclarationStatement = new VariableDeclarationStatement();
			this.SetExprementPosition(pos, variableDeclarationStatement);
			LList<_IExpression> llist = new LList<_IExpression>();
			foreach (string stName in stNames)
			{
				llist.Add(this.CreateVariableExpression(pos, stName) as _IExpression);
			}
			variableDeclarationStatement.NameList = llist;
			variableDeclarationStatement.Type = (type as _IType);
			variableDeclarationStatement.Address = diraddr;
			variableDeclarationStatement.Initial = (expInitial as Expression);
			if (expInitMethodAssignments != null)
			{
				foreach (IAssignmentExpression assignmentExpression in expInitMethodAssignments)
				{
					variableDeclarationStatement.AddParam(assignmentExpression.RValue as Expression, assignmentExpression.LValue as Expression);
				}
			}
			return variableDeclarationStatement;
		}

		// Token: 0x06001554 RID: 5460 RVA: 0x0003D9B0 File Offset: 0x0003C9B0
		public ITypeDeclarationStatement CreateAliasDeclaration(IExprementPosition pos, ICompiledType type, string stName, IExpression expInitial, SignatureFlag sfAccess)
		{
			TypeDeclarationStatement typeDeclarationStatement = new TypeDeclarationStatement();
			this.SetExprementPosition(pos, typeDeclarationStatement);
			typeDeclarationStatement.Flags = (SignatureFlag.Alias | sfAccess);
			typeDeclarationStatement.Type = (type as _IType);
			typeDeclarationStatement.Name = stName;
			typeDeclarationStatement.Initial = (expInitial as Expression);
			return typeDeclarationStatement;
		}

		// Token: 0x06001555 RID: 5461 RVA: 0x0003D9F8 File Offset: 0x0003C9F8
		public ITypeDeclarationStatement CreateStructDeclaration(IExprementPosition pos, string stName, IExpression expExtends, IExpression expInitial, ISequenceStatement seq, SignatureFlag sfAccess)
		{
			TypeDeclarationStatement typeDeclarationStatement = new TypeDeclarationStatement();
			this.SetExprementPosition(pos, typeDeclarationStatement);
			typeDeclarationStatement.Extends = (expExtends as Expression);
			typeDeclarationStatement.Flags = (SignatureFlag.Structure | sfAccess);
			typeDeclarationStatement.Name = stName;
			typeDeclarationStatement.Declarations = (seq as Statement);
			typeDeclarationStatement.Initial = (expInitial as Expression);
			return typeDeclarationStatement;
		}

		// Token: 0x06001556 RID: 5462 RVA: 0x0003DA4C File Offset: 0x0003CA4C
		public ITypeDeclarationStatement CreateUnionDeclaration(IExprementPosition pos, string stName, IExpression expExtends, IExpression expInitial, ISequenceStatement seq, SignatureFlag sfAccess)
		{
			TypeDeclarationStatement typeDeclarationStatement = new TypeDeclarationStatement();
			this.SetExprementPosition(pos, typeDeclarationStatement);
			typeDeclarationStatement.Extends = (expExtends as Expression);
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35400)
			{
				typeDeclarationStatement.Flags = (SignatureFlag.Structure | SignatureFlag.Union | sfAccess);
			}
			else
			{
				typeDeclarationStatement.Flags = (SignatureFlag.Union | sfAccess);
			}
			typeDeclarationStatement.Name = stName;
			typeDeclarationStatement.Declarations = (seq as Statement);
			typeDeclarationStatement.Initial = (expInitial as Expression);
			return typeDeclarationStatement;
		}

		// Token: 0x06001557 RID: 5463 RVA: 0x0003DAC8 File Offset: 0x0003CAC8
		public ITypeDeclarationStatement CreateEnumTypeDeclaration(IExprementPosition pos, string stName, IExpression expInitial, IEnumDeclarationListStatement enumdecllist, SignatureFlag sfAccess)
		{
			TypeDeclarationStatement typeDeclarationStatement = new TypeDeclarationStatement();
			this.SetExprementPosition(pos, typeDeclarationStatement);
			typeDeclarationStatement.Flags = (SignatureFlag.Enum | sfAccess);
			typeDeclarationStatement.Name = stName;
			typeDeclarationStatement.Declarations = (enumdecllist as Statement);
			typeDeclarationStatement.Initial = (expInitial as Expression);
			return typeDeclarationStatement;
		}

		// Token: 0x06001558 RID: 5464 RVA: 0x0003DB10 File Offset: 0x0003CB10
		public IEnumDeclarationListStatement CreateEnumDeclarationListStatement(IExprementPosition pos, ICompiledType basetype, List<IEnumDeclarationStatement> enums)
		{
			EnumDeclarationListStatement enumDeclarationListStatement = new EnumDeclarationListStatement();
			this.SetExprementPosition(pos, enumDeclarationListStatement);
			enumDeclarationListStatement._BaseType = (basetype as _IType);
			enumDeclarationListStatement.Enumerations = enums;
			return enumDeclarationListStatement;
		}

		// Token: 0x06001559 RID: 5465 RVA: 0x0003DB40 File Offset: 0x0003CB40
		public IEnumDeclarationStatement CreateEnumDeclarationStatement(IExprementPosition pos, string stName, IExpression expValue)
		{
			EnumDeclarationStatement enumDeclarationStatement = new EnumDeclarationStatement();
			this.SetExprementPosition(pos, enumDeclarationStatement);
			enumDeclarationStatement._Value = (expValue as Expression);
			enumDeclarationStatement.Name = stName;
			return enumDeclarationStatement;
		}

		// Token: 0x0600155A RID: 5466 RVA: 0x0003DB6F File Offset: 0x0003CB6F
		public IExpressionStatement CreateCallStatement(IExprementPosition pos, IExpression expCallee, IExpression expCondition, ICompiledType typeExpected, List<IAssignmentExpression> inputassignments, List<IAssignmentExpression> outputassignments)
		{
			return this.CreateExpressionStatement(this.CreateCallExpression(pos, expCallee, expCondition, typeExpected, inputassignments, outputassignments));
		}

		// Token: 0x0600155B RID: 5467 RVA: 0x0003DB88 File Offset: 0x0003CB88
		public ICallExpression2 CreateCallExpression(IExprementPosition pos, IExpression expCallee, IExpression expCondition, ICompiledType typeExpected, List<IAssignmentExpression> inputassignments, List<IAssignmentExpression> outputassignments)
		{
			CallExpression callExpression = new CallExpression();
			callExpression._Callee = (expCallee as Expression);
			callExpression._Condition = (expCondition as Expression);
			callExpression.ExpectedType = (typeExpected as _IType);
			this.SetExprementPosition(pos, callExpression._Callee);
			foreach (IAssignmentExpression assignmentExpression in inputassignments)
			{
				callExpression.AddParam(assignmentExpression.RValue as Expression, assignmentExpression.LValue as Expression);
			}
			foreach (IAssignmentExpression assignmentExpression2 in outputassignments)
			{
				callExpression.AddOutput(assignmentExpression2.LValue as Expression, assignmentExpression2.RValue as Expression);
			}
			return callExpression;
		}

		// Token: 0x0600155C RID: 5468 RVA: 0x0003DC78 File Offset: 0x0003CC78
		public IExpressionStatement CreateNonFormalCallStatement(IExprementPosition pos, IExpression expCallee, IExpression expCondition, ICompiledType typeExpected, IEnumerable<IExpression> inputarguments, IEnumerable<IAssignmentExpression> outputassignments)
		{
			return this.CreateExpressionStatement(this.CreateNonFormalCallExpression(pos, expCallee, expCondition, typeExpected, inputarguments, outputassignments));
		}

		// Token: 0x0600155D RID: 5469 RVA: 0x0003DC90 File Offset: 0x0003CC90
		public ICallExpression2 CreateNonFormalCallExpression(IExprementPosition pos, IExpression expCallee, IExpression expCondition, ICompiledType typeExpected, IEnumerable<IExpression> inputarguments, IEnumerable<IAssignmentExpression> outputassignments)
		{
			CallExpression callExpression = new CallExpression();
			this.SetExprementPosition(pos, callExpression);
			callExpression._Callee = (expCallee as Expression);
			callExpression._Condition = (expCondition as Expression);
			callExpression.ExpectedType = (typeExpected as _IType);
			foreach (IExpression expression in inputarguments)
			{
				callExpression.AddParam(expression as Expression);
			}
			foreach (IAssignmentExpression assignmentExpression in outputassignments)
			{
				callExpression.AddOutput(assignmentExpression.LValue as Expression, assignmentExpression.RValue as Expression);
			}
			return callExpression;
		}

		// Token: 0x0600155E RID: 5470 RVA: 0x0003DD64 File Offset: 0x0003CD64
		public IOperatorExpression CreateOperatorExpression(IExprementPosition pos, Operator op, IExpression expSingleOp)
		{
			OperatorExpression operatorExpression = new OperatorExpression(op);
			this.SetExprementPosition(pos, operatorExpression);
			operatorExpression.AddOperand(expSingleOp as Expression);
			return operatorExpression;
		}

		// Token: 0x0600155F RID: 5471 RVA: 0x0003DD90 File Offset: 0x0003CD90
		public IOperatorExpression CreateOperatorExpression(IExprementPosition pos, Operator op, IExpression expOp1, IExpression expOp2)
		{
			OperatorExpression operatorExpression = new OperatorExpression(op);
			this.SetExprementPosition(pos, operatorExpression);
			operatorExpression.AddOperand(expOp1 as Expression);
			operatorExpression.AddOperand(expOp2 as Expression);
			return operatorExpression;
		}

		// Token: 0x06001560 RID: 5472 RVA: 0x0003DDC8 File Offset: 0x0003CDC8
		public IOperatorExpression CreateOperatorExpression(IExprementPosition pos, Operator op, List<IExpression> expOperands)
		{
			OperatorExpression operatorExpression = new OperatorExpression(op);
			this.SetExprementPosition(pos, operatorExpression);
			if (op == Operator.Add || op == Operator.Mul || op == Operator.And || op == Operator.And_Then || op == Operator.Or || op == Operator.Or_Else || op == Operator.Xor || op == Operator.Plus || op == Operator.Times || op == Operator.Max || op == Operator.Min)
			{
				if (expOperands.Count > 2)
				{
					IExpression expOp = expOperands[expOperands.Count - 1];
					List<IExpression> range = expOperands.GetRange(0, expOperands.Count - 1);
					IExpression expOp2 = this.CreateOperatorExpression(pos, op, range);
					return (OperatorExpression)this.CreateOperatorExpression(pos, op, expOp2, expOp);
				}
				using (List<IExpression>.Enumerator enumerator = expOperands.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						IExpression expression = enumerator.Current;
						operatorExpression.AddOperand(expression as Expression);
					}
					return operatorExpression;
				}
			}
			foreach (IExpression expression2 in expOperands)
			{
				operatorExpression.AddOperand(expression2 as Expression);
			}
			return operatorExpression;
		}

		// Token: 0x06001561 RID: 5473 RVA: 0x0003DF00 File Offset: 0x0003CF00
		public IConversionExpression CreateConversionExpression(IExprementPosition pos, TypeClass tcFrom, TypeClass tcTo, IExpression exp)
		{
			ConversionExpression conversionExpression = new ConversionExpression(tcFrom, tcTo);
			this.SetExprementPosition(pos, conversionExpression);
			conversionExpression._Exp = (exp as Expression);
			return conversionExpression;
		}

		// Token: 0x06001562 RID: 5474 RVA: 0x0003DF2C File Offset: 0x0003CF2C
		public IThisExpression CreateThisExpression(IExprementPosition pos)
		{
			ThisExpression thisExpression = new ThisExpression();
			this.SetExprementPosition(pos, thisExpression);
			return thisExpression;
		}

		// Token: 0x06001563 RID: 5475 RVA: 0x0003DF48 File Offset: 0x0003CF48
		public IBaseExpression CreateSuperExpression(IExprementPosition pos)
		{
			BaseExpression baseExpression = new BaseExpression();
			this.SetExprementPosition(pos, baseExpression);
			return baseExpression;
		}

		// Token: 0x06001564 RID: 5476 RVA: 0x0003DF64 File Offset: 0x0003CF64
		public _ILiteralExpression CreateDefaultLiteralExpression(TypeClass tc)
		{
			return LiteralExpression.CreateDefaultLiteralExpression(tc);
		}

		// Token: 0x06001565 RID: 5477 RVA: 0x0003DF6C File Offset: 0x0003CF6C
		public LiteralExpression CreateLiteralExpression(ILiteralValue2 litVal, TypeClass tc)
		{
			return LiteralExpression.CreateLiteralExpression(litVal, tc);
		}

		// Token: 0x06001566 RID: 5478 RVA: 0x0003DF78 File Offset: 0x0003CF78
		public ILiteralExpression CreateLiteralExpression(IExprementPosition pos, long lVal, TypeClass tc)
		{
			ILiteralExpression literalExpression = LiteralExpression.CreateLiteralExpression(lVal, tc);
			this.SetExprementPosition(pos, literalExpression);
			return literalExpression;
		}

		// Token: 0x06001567 RID: 5479 RVA: 0x0003DF98 File Offset: 0x0003CF98
		public ILiteralExpression CreateLiteralExpression(IExprementPosition pos, long lVal, TypeClass tc, int nBase)
		{
			ILiteralExpression literalExpression = LiteralExpression.CreateLiteralExpression(lVal, tc, nBase);
			this.SetExprementPosition(pos, literalExpression);
			return literalExpression;
		}

		// Token: 0x06001568 RID: 5480 RVA: 0x0003DFB8 File Offset: 0x0003CFB8
		public ILiteralExpression CreateLiteralExpression(IExprementPosition pos, ulong ulVal, TypeClass tc)
		{
			ILiteralExpression literalExpression = LiteralExpression.CreateLiteralExpression(ulVal, tc);
			this.SetExprementPosition(pos, literalExpression);
			return literalExpression;
		}

		// Token: 0x06001569 RID: 5481 RVA: 0x0003DFD8 File Offset: 0x0003CFD8
		public ILiteralExpression CreateLiteralExpression(IExprementPosition pos, string stVal, TypeClass tc)
		{
			ILiteralExpression literalExpression = LiteralExpression.CreateLiteralExpression(stVal, tc);
			this.SetExprementPosition(pos, literalExpression);
			return literalExpression;
		}

		// Token: 0x0600156A RID: 5482 RVA: 0x0003DFF8 File Offset: 0x0003CFF8
		public ILiteralExpression CreateLiteralExpression(IExprementPosition pos, double dVal, TypeClass tc)
		{
			ILiteralExpression literalExpression = LiteralExpression.CreateLiteralExpression(dVal, tc);
			this.SetExprementPosition(pos, literalExpression);
			return literalExpression;
		}

		// Token: 0x0600156B RID: 5483 RVA: 0x0003E018 File Offset: 0x0003D018
		public ILiteralExpression CreateLiteralExpression(IExprementPosition pos, long lVal)
		{
			ILiteralExpression literalExpression = LiteralExpression.CreateLiteralExpression(lVal);
			this.SetExprementPosition(pos, literalExpression);
			return literalExpression;
		}

		// Token: 0x0600156C RID: 5484 RVA: 0x0003E038 File Offset: 0x0003D038
		public ILiteralExpression CreateLiteralExpression(IExprementPosition pos, ulong ulVal)
		{
			ILiteralExpression literalExpression = LiteralExpression.CreateLiteralExpression(ulVal);
			this.SetExprementPosition(pos, literalExpression);
			return literalExpression;
		}

		// Token: 0x0600156D RID: 5485 RVA: 0x0003E058 File Offset: 0x0003D058
		public ILiteralExpression CreateLiteralExpression(IExprementPosition pos, string stVal)
		{
			ILiteralExpression literalExpression = LiteralExpression.CreateLiteralExpression(stVal);
			this.SetExprementPosition(pos, literalExpression);
			return literalExpression;
		}

		// Token: 0x0600156E RID: 5486 RVA: 0x0003E078 File Offset: 0x0003D078
		public ILiteralExpression CreateLiteralExpression(IExprementPosition pos, double dVal)
		{
			ILiteralExpression literalExpression = LiteralExpression.CreateLiteralExpression(dVal);
			this.SetExprementPosition(pos, literalExpression);
			return literalExpression;
		}

		// Token: 0x0600156F RID: 5487 RVA: 0x0003E098 File Offset: 0x0003D098
		public ILiteralExpression CreateLiteralExpression(IExprementPosition pos, bool bVal)
		{
			ILiteralExpression literalExpression = LiteralExpression.CreateLiteralExpression(bVal);
			this.SetExprementPosition(pos, literalExpression);
			return literalExpression;
		}

		// Token: 0x06001570 RID: 5488 RVA: 0x0003E0B5 File Offset: 0x0003D0B5
		public _ILiteralExpression CreateLiteralExpression(long lVal, TypeClass tc, IToken token)
		{
			return LiteralExpression.CreateLiteralExpression(lVal, tc, token);
		}

		// Token: 0x06001571 RID: 5489 RVA: 0x0003E0BF File Offset: 0x0003D0BF
		public _ILiteralExpression CreateLiteralExpression(long lVal, TypeClass tc, IToken token, int nBase)
		{
			return LiteralExpression.CreateLiteralExpression(lVal, tc, token, nBase);
		}

		// Token: 0x06001572 RID: 5490 RVA: 0x0003E0CB File Offset: 0x0003D0CB
		public _ILiteralExpression CreateLiteralExpression(long lVal, TypeClass tc, IToken token, int nBase, bool negative)
		{
			return LiteralExpression.CreateLiteralExpression(lVal, tc, token, nBase, negative);
		}

		// Token: 0x06001573 RID: 5491 RVA: 0x0003E0D9 File Offset: 0x0003D0D9
		public _ILiteralExpression CreateLiteralExpression(ulong ulVal, TypeClass tc, IToken token)
		{
			return LiteralExpression.CreateLiteralExpression(ulVal, tc, token);
		}

		// Token: 0x06001574 RID: 5492 RVA: 0x0003E0E3 File Offset: 0x0003D0E3
		public _ILiteralExpression CreateLiteralExpression(string stVal, TypeClass tc, IToken token)
		{
			return LiteralExpression.CreateLiteralExpression(stVal, tc, token);
		}

		// Token: 0x06001575 RID: 5493 RVA: 0x0003E0ED File Offset: 0x0003D0ED
		public _ILiteralExpression CreateLiteralExpression(string stVal, TypeClass tc, IToken token, StringEncoding stringEncoding)
		{
			return LiteralExpression.CreateLiteralExpression(stVal, tc, token, stringEncoding);
		}

		// Token: 0x06001576 RID: 5494 RVA: 0x0003E0F9 File Offset: 0x0003D0F9
		public _ILiteralExpression CreateLiteralExpression(double dVal, TypeClass tc, IToken token)
		{
			return LiteralExpression.CreateLiteralExpression(dVal, tc, token);
		}

		// Token: 0x06001577 RID: 5495 RVA: 0x0003E103 File Offset: 0x0003D103
		public _ITypeExpression CreateTypeExpression(ICompiledType ctype, IToken token)
		{
			return new TypeExpression(ctype, token);
		}

		// Token: 0x06001578 RID: 5496 RVA: 0x0003E10C File Offset: 0x0003D10C
		public ITypeExpression CreateTypeExpression(IExprementPosition pos, ICompiledType ctype)
		{
			TypeExpression typeExpression = new TypeExpression();
			typeExpression._CompiledType = ctype;
			this.SetExprementPosition(pos, typeExpression);
			return typeExpression;
		}

		// Token: 0x06001579 RID: 5497 RVA: 0x0003E130 File Offset: 0x0003D130
		public INewExpression CreateNewExpression(IExprementPosition pos, ICompiledType ctypeIn, IExpression expCount)
		{
			if (expCount == null)
			{
				expCount = new IntegerLiteralExpression(1L);
			}
			NewExpression newExpression = new NewExpression(ctypeIn as _IType, expCount as Expression);
			this.SetExprementPosition(pos, newExpression);
			return newExpression;
		}

		// Token: 0x0600157A RID: 5498 RVA: 0x0003E164 File Offset: 0x0003D164
		public ICastExpression CreateCastExpression(IExprementPosition pos, IExpression expWithType, IExpression expBase)
		{
			CastExpression castExpression = new CastExpression(expWithType as Expression, expBase as Expression);
			this.SetExprementPosition(pos, castExpression);
			return castExpression;
		}

		// Token: 0x0600157B RID: 5499 RVA: 0x0003E18C File Offset: 0x0003D18C
		public ICastExpression CreateCastExpression(IExprementPosition pos, ICompiledType type, IExpression expBase)
		{
			CastExpression castExpression = new CastExpression(null, expBase as Expression);
			this.SetExprementPosition(pos, castExpression);
			castExpression.ExplicitelySpecifiedType = type;
			return castExpression;
		}

		// Token: 0x0600157C RID: 5500 RVA: 0x0003E1B8 File Offset: 0x0003D1B8
		public IAddressExpression CreateAddressExpression(IExprementPosition pos, IDirectVariable dirvar)
		{
			AddressExpression addressExpression = new AddressExpression(dirvar);
			this.SetExprementPosition(pos, addressExpression);
			return addressExpression;
		}

		// Token: 0x0600157D RID: 5501 RVA: 0x0003E1D5 File Offset: 0x0003D1D5
		public _IVariableExpression CreateVariableExpression(_IVariable var, _ISignature sign)
		{
			return new VariableExpression(var, sign);
		}

		// Token: 0x0600157E RID: 5502 RVA: 0x0003E1E0 File Offset: 0x0003D1E0
		public IVariableExpression2 CreateVariableExpression(IExprementPosition pos, string stName)
		{
			VariableExpression variableExpression = new VariableExpression(stName);
			this.SetExprementPosition(pos, variableExpression);
			return variableExpression;
		}

		// Token: 0x0600157F RID: 5503 RVA: 0x0003E200 File Offset: 0x0003D200
		public IIndexAccessExpression CreateIndexAccessExpression(IExprementPosition pos, IExpression expBase, IExpression expAccess)
		{
			IndexAccessExpression indexAccessExpression = new IndexAccessExpression();
			indexAccessExpression._Var = (expBase as Expression);
			indexAccessExpression.AddAccess(expAccess as Expression);
			this.SetExprementPosition(pos, indexAccessExpression);
			return indexAccessExpression;
		}

		// Token: 0x06001580 RID: 5504 RVA: 0x0003E234 File Offset: 0x0003D234
		public IIndexAccessExpression CreateIndexAccessExpression(IExprementPosition pos, IExpression expBase, List<IExpression> expAccesses)
		{
			IndexAccessExpression indexAccessExpression = new IndexAccessExpression();
			indexAccessExpression._Var = (expBase as Expression);
			foreach (IExpression expression in expAccesses)
			{
				indexAccessExpression.AddAccess(expression as Expression);
			}
			this.SetExprementPosition(pos, indexAccessExpression);
			return indexAccessExpression;
		}

		// Token: 0x06001581 RID: 5505 RVA: 0x0003E2A4 File Offset: 0x0003D2A4
		public ICompoAccessExpression CreateCompoAccessExpression(IExprementPosition pos, IExpression expLeft, IVariableExpression2 expRight)
		{
			CompoAccessExpression compoAccessExpression = new CompoAccessExpression();
			compoAccessExpression._Left = (expLeft as Expression);
			compoAccessExpression._Right = (expRight as Expression);
			this.SetExprementPosition(pos, compoAccessExpression);
			return compoAccessExpression;
		}

		// Token: 0x06001582 RID: 5506 RVA: 0x0003E2D8 File Offset: 0x0003D2D8
		public ICompoAccessExpression CreateBitAccessExpression(IExprementPosition pos, IExpression expLeft, ILiteralExpression expRight)
		{
			CompoAccessExpression compoAccessExpression = new CompoAccessExpression();
			compoAccessExpression._Left = (expLeft as Expression);
			compoAccessExpression._Right = (expRight as Expression);
			this.SetExprementPosition(pos, compoAccessExpression);
			return compoAccessExpression;
		}

		// Token: 0x06001583 RID: 5507 RVA: 0x0003E30C File Offset: 0x0003D30C
		public IDeRefAccessExpression CreateDeRefAccessExpression(IExprementPosition pos, IExpression expBase)
		{
			DeRefAccessExpression deRefAccessExpression = new DeRefAccessExpression();
			deRefAccessExpression._Base = (expBase as Expression);
			this.SetExprementPosition(pos, deRefAccessExpression);
			return deRefAccessExpression;
		}

		// Token: 0x06001584 RID: 5508 RVA: 0x0003E334 File Offset: 0x0003D334
		public IGlobalScopeExpression CreateGlobalScopeExpression(IExprementPosition pos, IExpression expBase)
		{
			GlobalScopeExpression globalScopeExpression = new GlobalScopeExpression();
			globalScopeExpression._Base = (expBase as Expression);
			this.SetExprementPosition(pos, globalScopeExpression);
			return globalScopeExpression;
		}

		// Token: 0x06001585 RID: 5509 RVA: 0x0003E35C File Offset: 0x0003D35C
		public ISystemScopeExpression CreateSystemScopeExpression(IExprementPosition pos, IExpression expBase)
		{
			SystemScopeExpression systemScopeExpression = new SystemScopeExpression();
			systemScopeExpression._Base = (expBase as Expression);
			this.SetExprementPosition(pos, systemScopeExpression);
			return systemScopeExpression;
		}

		// Token: 0x06001586 RID: 5510 RVA: 0x0003E384 File Offset: 0x0003D384
		public IMultipleIndexInitialization CreateMultipleIndexInitialisation(IExprementPosition pos, IExpression expNumber, IExpression expValue)
		{
			MultipleIndexInitialisation multipleIndexInitialisation = new MultipleIndexInitialisation();
			multipleIndexInitialisation._Number = (expNumber as Expression);
			multipleIndexInitialisation._Value = (expValue as Expression);
			this.SetExprementPosition(pos, multipleIndexInitialisation);
			return multipleIndexInitialisation;
		}

		// Token: 0x06001587 RID: 5511 RVA: 0x0003E3B8 File Offset: 0x0003D3B8
		public IArrayInitialization CreateArrayInitialisation(IExprementPosition pos, List<IExpression> expInitvalues)
		{
			ArrayInitialisation arrayInitialisation = new ArrayInitialisation();
			foreach (IExpression expression in expInitvalues)
			{
				arrayInitialisation.AddInitValue(expression as Expression);
			}
			this.SetExprementPosition(pos, arrayInitialisation);
			return arrayInitialisation;
		}

		// Token: 0x06001588 RID: 5512 RVA: 0x0003E41C File Offset: 0x0003D41C
		public IStructureInitialization CreateStructureInitialisation(IExprementPosition pos, List<IAssignmentExpression> initAssigns)
		{
			StructureInitialisation structureInitialisation = new StructureInitialisation();
			foreach (IAssignmentExpression assignmentExpression in initAssigns)
			{
				structureInitialisation.AddInitValue(assignmentExpression as AssignmentExpression);
			}
			this.SetExprementPosition(pos, structureInitialisation);
			return structureInitialisation;
		}

		// Token: 0x06001589 RID: 5513 RVA: 0x0003E480 File Offset: 0x0003D480
		public IDefineReference CreateDefineReference(IExprementPosition pos, string stDefine)
		{
			DefineReference defineReference = new DefineReference();
			defineReference.Define = stDefine;
			this.SetExprementPosition(pos, defineReference);
			return defineReference;
		}

		// Token: 0x0600158A RID: 5514 RVA: 0x0003E4A4 File Offset: 0x0003D4A4
		public IVariableReference CreateVariableReference(IExprementPosition pos, IExpression expInstancePath)
		{
			VariableReference variableReference = new VariableReference();
			variableReference.InstancePath = (expInstancePath as Expression);
			this.SetExprementPosition(pos, variableReference);
			return variableReference;
		}

		// Token: 0x0600158B RID: 5515 RVA: 0x0003E4CC File Offset: 0x0003D4CC
		public ITypeReference2 CreateTypeReference(IExprementPosition pos, IExpression expInstancePath)
		{
			TypeReference typeReference = new TypeReference();
			typeReference.InstancePath = (expInstancePath as Expression);
			this.SetExprementPosition(pos, typeReference);
			return typeReference;
		}

		// Token: 0x0600158C RID: 5516 RVA: 0x0003E4F4 File Offset: 0x0003D4F4
		public IPouReference2 CreatePouReference(IExprementPosition pos, IExpression expInstancePath)
		{
			PouReference pouReference = new PouReference();
			pouReference.InstancePath = (expInstancePath as Expression);
			this.SetExprementPosition(pos, pouReference);
			return pouReference;
		}

		// Token: 0x0600158D RID: 5517 RVA: 0x0003E51C File Offset: 0x0003D51C
		public IDefinedExpression CreateDefinedExpression(IExprementPosition pos, IExpression expItemReference)
		{
			if (expItemReference == null)
			{
				throw new ArgumentNullException("expItemReference");
			}
			if (!(expItemReference is ItemReference))
			{
				throw new ArgumentException("Wrong type for item reference", "expItemReference");
			}
			DefinedExpression definedExpression = new DefinedExpression();
			definedExpression.ItemReference = (expItemReference as ItemReference);
			this.SetExprementPosition(pos, definedExpression);
			return definedExpression;
		}

		// Token: 0x0600158E RID: 5518 RVA: 0x0003E56C File Offset: 0x0003D56C
		public ICompilerVersionExpression CreateCompilerVersioExpression(IExprementPosition pos, Version version, Operator opComparison)
		{
			CompilerVersionExpression compilerVersionExpression = new CompilerVersionExpression(version, opComparison);
			this.SetExprementPosition(pos, compilerVersionExpression);
			return compilerVersionExpression;
		}

		// Token: 0x0600158F RID: 5519 RVA: 0x0003E58C File Offset: 0x0003D58C
		public IRuntimeVersionExpression CreateRuntimeVersionExpression(IExprementPosition pos, Version version, Operator opComparison)
		{
			RuntimeVersionExpression runtimeVersionExpression = new RuntimeVersionExpression(version, opComparison);
			this.SetExprementPosition(pos, runtimeVersionExpression);
			return runtimeVersionExpression;
		}

		// Token: 0x06001590 RID: 5520 RVA: 0x0003E5AC File Offset: 0x0003D5AC
		public IIsEnumTypeExpression CreateIsEnumTypeExpression(IExprementPosition pos, ICompiledType type)
		{
			IsEnumTypeExpression isEnumTypeExpression = new IsEnumTypeExpression();
			isEnumTypeExpression.ReferencedType = type;
			this.SetExprementPosition(pos, isEnumTypeExpression);
			return isEnumTypeExpression;
		}

		// Token: 0x06001591 RID: 5521 RVA: 0x0003E5D0 File Offset: 0x0003D5D0
		public IHasTypeExpression CreateHasCompatibleTypeExpression(IExprementPosition pos, IVariableReference varref, ICompiledType type)
		{
			HasCompatibleTypeExpression hasCompatibleTypeExpression = new HasCompatibleTypeExpression();
			hasCompatibleTypeExpression.Variable = (varref as VariableReference);
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35900)
			{
				hasCompatibleTypeExpression.ReferencedType = type;
			}
			else
			{
				hasCompatibleTypeExpression.Type = type;
			}
			this.SetExprementPosition(pos, hasCompatibleTypeExpression);
			return hasCompatibleTypeExpression;
		}

		// Token: 0x06001592 RID: 5522 RVA: 0x0003E61C File Offset: 0x0003D61C
		public IHasAttributeExpression CreateHasAttributeExpression(IExprementPosition pos, IExpression expItemReference, string stAttribute)
		{
			HasAttributeExpression hasAttributeExpression = new HasAttributeExpression();
			hasAttributeExpression.ItemReference = (expItemReference as ItemReference);
			hasAttributeExpression.Attribute = stAttribute;
			this.SetExprementPosition(pos, hasAttributeExpression);
			return hasAttributeExpression;
		}

		// Token: 0x06001593 RID: 5523 RVA: 0x0003E64C File Offset: 0x0003D64C
		public IHasValueExpression CreateHasValueExpression(IExprementPosition pos, string stDefine, string stValue)
		{
			HasValueExpression hasValueExpression = new HasValueExpression();
			hasValueExpression.Define = stDefine;
			hasValueExpression.DefineValue = stValue;
			this.SetExprementPosition(pos, hasValueExpression);
			return hasValueExpression;
		}

		// Token: 0x06001594 RID: 5524 RVA: 0x0003E676 File Offset: 0x0003D676
		public IHasConstantValueExpression CreateHasConstantValueExpression(IExprementPosition pos, IExpression constant, ILiteralExpression value)
		{
			return this.CreateHasConstantValueExpression(pos, constant, value, Operator.Equal);
		}

		// Token: 0x06001595 RID: 5525 RVA: 0x0003E688 File Offset: 0x0003D688
		public IHasConstantValueExpression2 CreateHasConstantValueExpression(IExprementPosition pos, IExpression constant, ILiteralExpression value, Operator comparison)
		{
			HasConstantValueExpression hasConstantValueExpression = new HasConstantValueExpression();
			hasConstantValueExpression._Constant = (constant as Expression);
			hasConstantValueExpression._ConstantValue = (value as LiteralExpression);
			hasConstantValueExpression._OpComparison = comparison;
			this.SetExprementPosition(pos, hasConstantValueExpression);
			return hasConstantValueExpression;
		}

		// Token: 0x06001596 RID: 5526 RVA: 0x0003E6C4 File Offset: 0x0003D6C4
		public _IHasConstantTypeExpression CreateHasConstantTypeExpression()
		{
			return new HasConstantTypeExpression();
		}

		// Token: 0x06001597 RID: 5527 RVA: 0x0003E6CB File Offset: 0x0003D6CB
		public _IHasConstantTypeExpression CreateHasConstantTypeExpression(IToken token)
		{
			return new HasConstantTypeExpression(token);
		}

		// Token: 0x06001598 RID: 5528 RVA: 0x0003E6D3 File Offset: 0x0003D6D3
		public _IHasConstantTypeExpression CreateHasConstantTypeExpression(IToken token, _IExpression constant, bool bConstantTypeReplaced)
		{
			return new HasConstantTypeExpression(token, constant, bConstantTypeReplaced);
		}

		// Token: 0x06001599 RID: 5529 RVA: 0x0003E6E0 File Offset: 0x0003D6E0
		public IPragmaOperatorExpression CreatePragmaOperatorExpression(IExprementPosition pos, Operator op, IExpression expOperand1, IExpression expOperand2)
		{
			PragmaOperatorExpression pragmaOperatorExpression = new PragmaOperatorExpression();
			this.SetExprementPosition(pos, pragmaOperatorExpression);
			pragmaOperatorExpression.AddOperand(expOperand1 as Expression);
			if (op != Operator.Not)
			{
				pragmaOperatorExpression.AddOperand(expOperand2 as Expression);
			}
			pragmaOperatorExpression.Operator = op;
			return pragmaOperatorExpression;
		}

		// Token: 0x0600159A RID: 5530 RVA: 0x0003E724 File Offset: 0x0003D724
		public IPragmaAssertion CreatePragmaAssertion(IExprementPosition pos, IExpression expCondition, string stAssertionText)
		{
			PragmaAssertion pragmaAssertion = new PragmaAssertion(expCondition as Expression, stAssertionText);
			this.SetExprementPosition(pos, pragmaAssertion);
			return pragmaAssertion;
		}

		// Token: 0x0600159B RID: 5531 RVA: 0x0003E748 File Offset: 0x0003D748
		public IPragmaIfStatement CreatePragmaIfStatement(IExprementPosition pos, IExpression expCondition, ISequenceStatement2 seqThen, ISequenceStatement2 seqElse)
		{
			PragmaIfStatement pragmaIfStatement = new PragmaIfStatement();
			pragmaIfStatement.Condition = (expCondition as Expression);
			pragmaIfStatement.IfThen = (seqThen as Statement);
			pragmaIfStatement.IfElse = (seqElse as Statement);
			this.SetExprementPosition(pos, pragmaIfStatement);
			return pragmaIfStatement;
		}

		// Token: 0x0600159C RID: 5532 RVA: 0x0003E789 File Offset: 0x0003D789
		public IWarningDisableRestorePragmaStatement CreateWarningDisableRestorePragmaStatement(bool bRestore, string stId)
		{
			return new WarningDisableRestorePragmaStatement
			{
				Restore = bRestore,
				Id = stId
			};
		}

		// Token: 0x0600159D RID: 5533 RVA: 0x0003E7A0 File Offset: 0x0003D7A0
		public IBreakPointStatement CreateBreakpointStatement(IExprementPosition pos, long lBPPosition, long lSuccessorPosition)
		{
			BreakPointStatement breakPointStatement = new BreakPointStatement();
			breakPointStatement.BPPosition = lBPPosition;
			breakPointStatement.SuccessorPosition = lSuccessorPosition;
			this.SetExprementPosition(pos, breakPointStatement);
			return breakPointStatement;
		}

		// Token: 0x0600159E RID: 5534 RVA: 0x0003E7CC File Offset: 0x0003D7CC
		public IDefineStatement CreateDefineStatement(IExprementPosition pos, bool bDefine, string stIdent, string stValue)
		{
			DefineStatement defineStatement = new DefineStatement();
			defineStatement.Define = bDefine;
			defineStatement.Ident = stIdent;
			defineStatement.Value = stValue;
			this.SetExprementPosition(pos, defineStatement);
			return defineStatement;
		}

		// Token: 0x0600159F RID: 5535 RVA: 0x0003E800 File Offset: 0x0003D800
		public ISequenceStatement2 CreateSequenceStatementEx(IExprementPosition pos, IEnumerable<IStatement> statements)
		{
			if (statements == null)
			{
				throw new ArgumentNullException("statements");
			}
			SequenceStatement sequenceStatement = new SequenceStatement();
			foreach (IStatement state in statements)
			{
				sequenceStatement.AddStatement(state);
			}
			this.SetExprementPosition(pos, sequenceStatement);
			return sequenceStatement;
		}

		// Token: 0x060015A0 RID: 5536 RVA: 0x0003E868 File Offset: 0x0003D868
		public IArrayInitialization CreateArrayInitialisationEx(IExprementPosition pos, IEnumerable<IExpression> expInitvalues)
		{
			ArrayInitialisation arrayInitialisation = new ArrayInitialisation();
			foreach (IExpression expression in expInitvalues)
			{
				arrayInitialisation.AddInitValue(expression as Expression);
			}
			this.SetExprementPosition(pos, arrayInitialisation);
			return arrayInitialisation;
		}

		// Token: 0x060015A1 RID: 5537 RVA: 0x0003E8C4 File Offset: 0x0003D8C4
		public IStructureInitialization CreateStructureInitialisationEx(IExprementPosition pos, IEnumerable<IAssignmentExpression> initAssigns)
		{
			StructureInitialisation structureInitialisation = new StructureInitialisation();
			foreach (IAssignmentExpression assignmentExpression in initAssigns)
			{
				structureInitialisation.AddInitValue(assignmentExpression as AssignmentExpression);
			}
			this.SetExprementPosition(pos, structureInitialisation);
			return structureInitialisation;
		}

		// Token: 0x060015A2 RID: 5538 RVA: 0x0003E920 File Offset: 0x0003D920
		public INullExpression CreateNullExpression(IExprementPosition pos)
		{
			NullExpression nullExpression = new NullExpression();
			this.SetExprementPosition(pos, nullExpression);
			return nullExpression;
		}

		// Token: 0x060015A3 RID: 5539 RVA: 0x0003E93C File Offset: 0x0003D93C
		public ISequenceStatement ParseInterfaceSnippet(string snippet, bool allowImplicit)
		{
			return CompilerProxy.CreateParser(snippet, allowImplicit).ParseInterfaceSnippet() as ISequenceStatement;
		}

		// Token: 0x060015A4 RID: 5540 RVA: 0x0003E94F File Offset: 0x0003D94F
		public IExpression ParseInitialisation(string stExpression, bool allowImplicit)
		{
			return CompilerProxy.CreateParser(stExpression, allowImplicit).ParseInitialisation();
		}

		// Token: 0x060015A5 RID: 5541 RVA: 0x0003E95D File Offset: 0x0003D95D
		public ISequenceStatement2 ParseSTSnippet(string stSnippet, bool allowImplicit)
		{
			return CompilerProxy.CreateParser(stSnippet, allowImplicit).ParseStatement() as ISequenceStatement2;
		}

		// Token: 0x060015A6 RID: 5542 RVA: 0x0003E970 File Offset: 0x0003D970
		public IExpression ParseExpression(string stExpression)
		{
			return this.ParseExpression(stExpression, true);
		}

		// Token: 0x060015A7 RID: 5543 RVA: 0x0003E97C File Offset: 0x0003D97C
		public IExpression ParseExpression(IExprementPosition pos, string stExpression, bool allowImplicit)
		{
			IExpression expression = this.ParseExpression(stExpression, allowImplicit);
			if (expression != null)
			{
				this.SetExprementPosition(pos, expression);
			}
			return expression;
		}

		// Token: 0x060015A8 RID: 5544 RVA: 0x0003E99E File Offset: 0x0003D99E
		public _IScanner CreateScanner(string st)
		{
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(st, true, false, true, false);
			scanner.AllowNestedComments = APEnvironmentFacade.Instance.LanguageModelMgr.AllowNestedComments;
			return (_IScanner)scanner;
		}

		// Token: 0x060015A9 RID: 5545 RVA: 0x0003E9CE File Offset: 0x0003D9CE
		public _IParser CreateParser(string st, bool allowImplicit)
		{
			return CompilerProxy.CreateParser(st, allowImplicit);
		}

		// Token: 0x060015AA RID: 5546 RVA: 0x0003E9D8 File Offset: 0x0003D9D8
		public IExpression ParseExpression(string stExpression, bool allowImplicit)
		{
			IExpression expression = CompilerProxy.CreateParser(stExpression, allowImplicit).ParseExpression();
			if (expression == null)
			{
				return CompilerProxy.CreateParser(stExpression, allowImplicit).ParseInitialisation();
			}
			return expression;
		}

		// Token: 0x060015AB RID: 5547 RVA: 0x0003EA04 File Offset: 0x0003DA04
		public IExpression ParseExpression(IExprementPosition pos, string stExpression, bool allowImplicit, bool bGenerateErrorForAdditionalToken, out string stRestText)
		{
			stRestText = stExpression;
			_IParser iparser = CompilerProxy.CreateParser(stExpression, allowImplicit);
			IExpression expression = iparser.ParseExpression();
			if (expression == null)
			{
				iparser = CompilerProxy.CreateParser(stExpression, allowImplicit);
				expression = iparser.ParseInitialisation();
			}
			if (expression != null)
			{
				if (stExpression.Length > iparser.UsedScanner.SourceOffset)
				{
					stRestText = stExpression.Substring(iparser.UsedScanner.SourceOffset);
				}
				IToken token;
				if (bGenerateErrorForAdditionalToken && iparser.UsedScanner.GetNext(out token) != TokenType.End)
				{
					iparser.AddErrorST(expression as Expression, token, MessageId.Err_UnexpectedTokenFound, new object[]
					{
						iparser.UsedScanner.GetTokenText(token)
					});
				}
				this.SetExprementPosition(pos, expression);
			}
			return expression;
		}

		// Token: 0x060015AC RID: 5548 RVA: 0x0003EAA2 File Offset: 0x0003DAA2
		public ICompiledType ParseType(string stType, bool allowImplicit)
		{
			return CompilerProxy.CreateParser(stType, allowImplicit).ParseType();
		}

		// Token: 0x060015AD RID: 5549 RVA: 0x0003EAB0 File Offset: 0x0003DAB0
		public bool AddTemporaryVariableBeforeCompile(ISignature signToModify, IExprementPosition pos, string stVariableName, ICompiledType ctype, IExpression expInitValue)
		{
			SourcePosition sp = SourcePosition.Empty;
			if (pos != null)
			{
				sp = new SourcePosition(-1, Guid.Empty, pos.Position, pos.PositionOffset, (short)stVariableName.Length);
			}
			_IVariable ivariable = new Variable(sp);
			ivariable.Name = stVariableName;
			ivariable._Type = (ctype as _IType);
			if (signToModify.POUType == Operator.Function || signToModify.POUType == Operator.Method)
			{
				ivariable.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.Implicit, true);
			}
			else
			{
				ivariable.SetFlag(VarFlag.IsCompiled | VarFlag.Implicit | VarFlag.Temp, true);
			}
			ivariable.Id = (signToModify as Signature).NextId;
			ivariable._Initial = (expInitValue as Expression);
			return (signToModify as Signature).AddVariable(ivariable);
		}

		// Token: 0x060015AE RID: 5550 RVA: 0x0003EB5A File Offset: 0x0003DB5A
		public bool AddTemporaryVariableAfterCompile(ICompileContext comcon, ISignature signToModify, IExprementPosition pos, string stVariableName, ICompiledType ctype)
		{
			return CompilerProxy.AddTemporaryVariableAfterCompile(comcon as _ICompileContext, signToModify, pos, stVariableName, ctype);
		}

		// Token: 0x060015AF RID: 5551 RVA: 0x0003EB6D File Offset: 0x0003DB6D
		public _ISourcePosition CreateFixedSourcePosition()
		{
			return new FixedSourcePosition();
		}

		// Token: 0x060015B0 RID: 5552 RVA: 0x0003EB74 File Offset: 0x0003DB74
		public ISourcePosition CreateSourcePosition(int nProjectHandle, Guid objectGuid, long nPosition, short sPositionOffset, short nLength)
		{
			return new SourcePosition(nProjectHandle, objectGuid, nPosition, sPositionOffset, nLength);
		}

		// Token: 0x060015B1 RID: 5553 RVA: 0x0003EB82 File Offset: 0x0003DB82
		public IMinimalPosition CreateMinimalPosition(_ISourcePosition sp)
		{
			return MinimalPosition.CreateMinimalPosition(sp);
		}

		// Token: 0x060015B2 RID: 5554 RVA: 0x0003EB8A File Offset: 0x0003DB8A
		public IMinimalPosition CreateMinimalPosition(long nPosition, short sPositionOffset)
		{
			return MinimalPosition.CreateMinimalPosition(nPosition, sPositionOffset);
		}

		// Token: 0x060015B3 RID: 5555 RVA: 0x0003EB93 File Offset: 0x0003DB93
		public _IHasCompatibleTypeExpression CreateHasCompatibleTypeExpression()
		{
			return new HasCompatibleTypeExpression();
		}

		// Token: 0x060015B4 RID: 5556 RVA: 0x0003EB9A File Offset: 0x0003DB9A
		public _IDefineReference CreateDefineReference(IToken token, string stDefine)
		{
			return new DefineReference(token, stDefine);
		}

		// Token: 0x060015B5 RID: 5557 RVA: 0x0003EBA3 File Offset: 0x0003DBA3
		public _IXStringType CreateXStringtype()
		{
			return new XStringType();
		}

		// Token: 0x060015B6 RID: 5558 RVA: 0x0003EBAA File Offset: 0x0003DBAA
		public _ISubrangeType CreateSubrangeType(_IExpression expLower, _IExpression expUpper)
		{
			return new SubrangeType(expLower, expUpper);
		}

		// Token: 0x060015B7 RID: 5559 RVA: 0x0003EBB3 File Offset: 0x0003DBB3
		public _IExitStatement CreateExitStatement(IToken token)
		{
			return new ExitStatement(token);
		}

		// Token: 0x060015B8 RID: 5560 RVA: 0x0003EBBB File Offset: 0x0003DBBB
		public _IContinueStatement CreateContinueStatement(IToken token)
		{
			return new ContinueStatement(token);
		}

		// Token: 0x060015B9 RID: 5561 RVA: 0x0003EBC3 File Offset: 0x0003DBC3
		public _IErrorStatement CreateErrorStatement(IToken token)
		{
			return new ErrorStatement(token);
		}

		// Token: 0x060015BA RID: 5562 RVA: 0x0003EBCB File Offset: 0x0003DBCB
		public _IEnumDeclarationListStatement CreateEnumDeclarationListStatement(IToken token)
		{
			return new EnumDeclarationListStatement(token);
		}

		// Token: 0x060015BB RID: 5563 RVA: 0x0003EBD3 File Offset: 0x0003DBD3
		public _ITypeDeclarationStatement CreateTypeDeclarationStatement(IToken token)
		{
			return new TypeDeclarationStatement(token);
		}

		// Token: 0x060015BC RID: 5564 RVA: 0x0003EBDB File Offset: 0x0003DBDB
		public _IErrorStatement CreateErrorStatement()
		{
			return new ErrorStatement();
		}

		// Token: 0x060015BD RID: 5565 RVA: 0x0003EBE2 File Offset: 0x0003DBE2
		public _IEmptyStatement CreateEmptyStatement()
		{
			return new EmptyStatement();
		}

		// Token: 0x060015BE RID: 5566 RVA: 0x0003EBE9 File Offset: 0x0003DBE9
		public _IEmptyStatement CreateEmptyStatement(IToken token)
		{
			return new EmptyStatement(token);
		}

		// Token: 0x060015BF RID: 5567 RVA: 0x0003EBF1 File Offset: 0x0003DBF1
		public _IVarInitialEmptyStatement CreateVarInitialEmptyStatement(_IVariable varInitial, _ISignature signInitial)
		{
			return new VarInitialEmptyStatement(varInitial, signInitial);
		}

		// Token: 0x060015C0 RID: 5568 RVA: 0x0003EBFA File Offset: 0x0003DBFA
		public _IWhileStatement CreateWhileStatement()
		{
			return new WhileStatement();
		}

		// Token: 0x060015C1 RID: 5569 RVA: 0x0003EC01 File Offset: 0x0003DC01
		public _IWhileStatement CreateWhileStatement(_IExpression expCond, _IStatement stateControlled)
		{
			return new WhileStatement(expCond, stateControlled);
		}

		// Token: 0x060015C2 RID: 5570 RVA: 0x0003EC0A File Offset: 0x0003DC0A
		public _IWhileStatement CreateWhileStatement(_IExpression expCond, _IStatement stateControlled, IToken token)
		{
			return new WhileStatement(expCond, stateControlled, token);
		}

		// Token: 0x060015C3 RID: 5571 RVA: 0x0003EC14 File Offset: 0x0003DC14
		public _IRepeatStatement CreateRepeatStatement()
		{
			return new RepeatStatement();
		}

		// Token: 0x060015C4 RID: 5572 RVA: 0x0003EC1B File Offset: 0x0003DC1B
		public _IRepeatStatement CreateRepeatStatement(_IExpression expCond, _IStatement stateControlled)
		{
			return new RepeatStatement(expCond, stateControlled);
		}

		// Token: 0x060015C5 RID: 5573 RVA: 0x0003EC24 File Offset: 0x0003DC24
		public _IRepeatStatement CreateRepeatStatement(_IExpression expCond, _IStatement stateControlled, IToken token)
		{
			return new RepeatStatement(expCond, stateControlled, token);
		}

		// Token: 0x060015C6 RID: 5574 RVA: 0x0003EC2E File Offset: 0x0003DC2E
		public _ICaseRangeExpression CreateCaseRangeExpression()
		{
			return new CaseRangeExpression();
		}

		// Token: 0x060015C7 RID: 5575 RVA: 0x0003EC35 File Offset: 0x0003DC35
		public _ICaseRangeExpression CreateCaseRangeExpression(_IExpression expLow, _IExpression expHigh, IToken token)
		{
			return new CaseRangeExpression(expLow, expHigh, token);
		}

		// Token: 0x060015C8 RID: 5576 RVA: 0x0003EC3F File Offset: 0x0003DC3F
		public _ICaseLabelStatement CreateCaseLabelStatement()
		{
			return new CaseLabelStatement();
		}

		// Token: 0x060015C9 RID: 5577 RVA: 0x0003EC46 File Offset: 0x0003DC46
		public _ICaseLabelStatement CreateCaseLabelStatement(_IExpression exp)
		{
			return new CaseLabelStatement(exp);
		}

		// Token: 0x060015CA RID: 5578 RVA: 0x0003EC4E File Offset: 0x0003DC4E
		public _ICaseLabelStatement CreateCaseLabelStatement(_IExpression exp, IToken token)
		{
			return new CaseLabelStatement(exp, token);
		}

		// Token: 0x060015CB RID: 5579 RVA: 0x0003EC57 File Offset: 0x0003DC57
		public _ICaseLabelStatement CreateCaseLabelStatement(IToken token)
		{
			return new CaseLabelStatement(token);
		}

		// Token: 0x060015CC RID: 5580 RVA: 0x0003EC5F File Offset: 0x0003DC5F
		public _ICase CreateCase()
		{
			return new Case();
		}

		// Token: 0x060015CD RID: 5581 RVA: 0x0003EC66 File Offset: 0x0003DC66
		public _ICase CreateCase(_ICaseLabelStatement caselabel, _IStatement statement)
		{
			if (caselabel == null)
			{
				caselabel = new CaseLabelStatement(new ErrorExpression());
			}
			return new Case(caselabel, statement);
		}

		// Token: 0x060015CE RID: 5582 RVA: 0x0003EC7E File Offset: 0x0003DC7E
		public _ICaseStatement CreateCaseStatement()
		{
			return new CaseStatement();
		}

		// Token: 0x060015CF RID: 5583 RVA: 0x0003EC85 File Offset: 0x0003DC85
		public _ICaseStatement CreateCaseStatement(_IExpression expSwitch)
		{
			return new CaseStatement(expSwitch);
		}

		// Token: 0x060015D0 RID: 5584 RVA: 0x0003EC8D File Offset: 0x0003DC8D
		public _ICaseStatement CreateCaseStatement(_IExpression expSwitch, IToken token)
		{
			return new CaseStatement(expSwitch, token);
		}

		// Token: 0x060015D1 RID: 5585 RVA: 0x0003EC96 File Offset: 0x0003DC96
		public _IExitStatement CreateExitStatement()
		{
			return new ExitStatement();
		}

		// Token: 0x060015D2 RID: 5586 RVA: 0x0003EC9D File Offset: 0x0003DC9D
		public _IContinueStatement CreateContinueStatement()
		{
			return new ContinueStatement();
		}

		// Token: 0x060015D3 RID: 5587 RVA: 0x0003ECA4 File Offset: 0x0003DCA4
		public _ISequenceStatement CreateSequenceStatement()
		{
			return new SequenceStatement();
		}

		// Token: 0x060015D4 RID: 5588 RVA: 0x0003ECAB File Offset: 0x0003DCAB
		public _ISequenceStatement CreateSequenceStatement(int nCount)
		{
			return new SequenceStatement(nCount);
		}

		// Token: 0x060015D5 RID: 5589 RVA: 0x0003ECB3 File Offset: 0x0003DCB3
		public _IForStatement CreateForStatement(IToken token)
		{
			return new ForStatement(token);
		}

		// Token: 0x060015D6 RID: 5590 RVA: 0x0003ECBB File Offset: 0x0003DCBB
		public _ISequenceStatement CreateSequenceStatement(IToken token)
		{
			return new SequenceStatement(token);
		}

		// Token: 0x060015D7 RID: 5591 RVA: 0x0003ECC3 File Offset: 0x0003DCC3
		public _ISubRoutineStatement CreateSubRoutineStatement()
		{
			return new SubRoutineStatement();
		}

		// Token: 0x060015D8 RID: 5592 RVA: 0x0003ECCA File Offset: 0x0003DCCA
		public _IAssignmentExpression CreateAssignmentExpression()
		{
			return new AssignmentExpression();
		}

		// Token: 0x060015D9 RID: 5593 RVA: 0x0003ECD1 File Offset: 0x0003DCD1
		public _IAssignmentExpression CreateAssignmentExpression(_IExpression expLValue)
		{
			return new AssignmentExpression(expLValue);
		}

		// Token: 0x060015DA RID: 5594 RVA: 0x0003ECD9 File Offset: 0x0003DCD9
		public _IAssignmentExpression CreateAssignmentExpression(_IExpression expLValue, IToken token)
		{
			return new AssignmentExpression(expLValue, token);
		}

		// Token: 0x060015DB RID: 5595 RVA: 0x0003ECE2 File Offset: 0x0003DCE2
		public _IElseIf CreateElseIf(_IExpression expCondition, _IStatement stControlled)
		{
			return new ElseIf(expCondition, stControlled);
		}

		// Token: 0x060015DC RID: 5596 RVA: 0x0003ECEB File Offset: 0x0003DCEB
		public _IElseIf CreateElseIf()
		{
			return new ElseIf();
		}

		// Token: 0x060015DD RID: 5597 RVA: 0x0003ECF2 File Offset: 0x0003DCF2
		public _IIfStatement CreateIfStatement()
		{
			return new IfStatement();
		}

		// Token: 0x060015DE RID: 5598 RVA: 0x0003ECF9 File Offset: 0x0003DCF9
		public _IIfStatement CreateIfStatement(_IExpression expCond)
		{
			return new IfStatement(expCond);
		}

		// Token: 0x060015DF RID: 5599 RVA: 0x0003ED01 File Offset: 0x0003DD01
		public _IIfStatement CreateIfStatement(_IExpression expCond, IToken token)
		{
			return new IfStatement(expCond, token);
		}

		// Token: 0x060015E0 RID: 5600 RVA: 0x0003ED0A File Offset: 0x0003DD0A
		public _ITryCatchStatement CreateTryCatchStatement()
		{
			return new TryCatchStatement();
		}

		// Token: 0x060015E1 RID: 5601 RVA: 0x0003ED11 File Offset: 0x0003DD11
		public _ITryCatchStatement CreateTryCatchStatement(IToken token)
		{
			return new TryCatchStatement(token);
		}

		// Token: 0x060015E2 RID: 5602 RVA: 0x0003ED19 File Offset: 0x0003DD19
		public _IReturnStatement CreateReturnStatement()
		{
			return new ReturnStatement();
		}

		// Token: 0x060015E3 RID: 5603 RVA: 0x0003ED20 File Offset: 0x0003DD20
		public _IReturnStatement CreateReturnStatement(IToken token)
		{
			return new ReturnStatement(token);
		}

		// Token: 0x060015E4 RID: 5604 RVA: 0x0003ED28 File Offset: 0x0003DD28
		public _IJumpStatement CreateJumpStatement()
		{
			return new JumpStatement();
		}

		// Token: 0x060015E5 RID: 5605 RVA: 0x0003ED2F File Offset: 0x0003DD2F
		public _IJumpStatement CreateJumpStatement(string stLabel)
		{
			return new JumpStatement(stLabel);
		}

		// Token: 0x060015E6 RID: 5606 RVA: 0x0003ED37 File Offset: 0x0003DD37
		public _IJumpStatement CreateJumpStatement(string stLabel, IToken token)
		{
			return new JumpStatement(stLabel, token);
		}

		// Token: 0x060015E7 RID: 5607 RVA: 0x0003ED40 File Offset: 0x0003DD40
		public _ILabelStatement CreateLabelStatement()
		{
			return new LabelStatement();
		}

		// Token: 0x060015E8 RID: 5608 RVA: 0x0003ED47 File Offset: 0x0003DD47
		public _ILabelStatement CreateLabelStatement(string stLabel)
		{
			return new LabelStatement(stLabel);
		}

		// Token: 0x060015E9 RID: 5609 RVA: 0x0003ED4F File Offset: 0x0003DD4F
		public _ILabelStatement CreateLabelStatement(string stLabel, IToken token)
		{
			return new LabelStatement(stLabel, token);
		}

		// Token: 0x060015EA RID: 5610 RVA: 0x0003ED58 File Offset: 0x0003DD58
		public _ICommentStatement CreateCommentStatement()
		{
			return new CommentStatement();
		}

		// Token: 0x060015EB RID: 5611 RVA: 0x0003ED5F File Offset: 0x0003DD5F
		public _ICommentStatement CreateCommentStatement(string stText)
		{
			return new CommentStatement(stText);
		}

		// Token: 0x060015EC RID: 5612 RVA: 0x0003ED67 File Offset: 0x0003DD67
		public _ICommentStatement CreateCommentStatement(string stText, IToken token)
		{
			return new CommentStatement(stText, token);
		}

		// Token: 0x060015ED RID: 5613 RVA: 0x0003ED70 File Offset: 0x0003DD70
		public _IPragmaStatement CreatePragmaStatement()
		{
			return new PragmaStatement();
		}

		// Token: 0x060015EE RID: 5614 RVA: 0x0003ED77 File Offset: 0x0003DD77
		public _IPragmaStatement CreatePragmaStatement(IToken token)
		{
			return new PragmaStatement(token);
		}

		// Token: 0x060015EF RID: 5615 RVA: 0x0003ED7F File Offset: 0x0003DD7F
		public _IWarningDisableRestorePragmaStatement CreateWarningDisableRestorePragmaStatement()
		{
			return new WarningDisableRestorePragmaStatement();
		}

		// Token: 0x060015F0 RID: 5616 RVA: 0x0003ED86 File Offset: 0x0003DD86
		public _IWarningDisableRestorePragmaStatement CreateWarningDisableRestorePragmaStatement(IToken token, bool bRestore, string stId)
		{
			return new WarningDisableRestorePragmaStatement(token, bRestore, stId);
		}

		// Token: 0x060015F1 RID: 5617 RVA: 0x0003ED90 File Offset: 0x0003DD90
		public _IExpressionStatement CreateExpressionStatement()
		{
			return new ExpressionStatement();
		}

		// Token: 0x060015F2 RID: 5618 RVA: 0x0003ED97 File Offset: 0x0003DD97
		public _IExpressionStatement CreateExpressionStatement(_IExpression exp)
		{
			return new ExpressionStatement(exp);
		}

		// Token: 0x060015F3 RID: 5619 RVA: 0x0003ED9F File Offset: 0x0003DD9F
		public _IExpressionStatement CreateExpressionStatement(_IExpression exp, IToken token)
		{
			return new ExpressionStatement(exp, token);
		}

		// Token: 0x060015F4 RID: 5620 RVA: 0x0003EDA8 File Offset: 0x0003DDA8
		public _IPOUDeclarationStatement CreatePOUDeclarationStatement()
		{
			return new POUDeclarationStatement();
		}

		// Token: 0x060015F5 RID: 5621 RVA: 0x0003EDAF File Offset: 0x0003DDAF
		public _IPOUDeclarationStatement CreatePOUDeclarationStatement(IToken token)
		{
			return new POUDeclarationStatement(token);
		}

		// Token: 0x060015F6 RID: 5622 RVA: 0x0003EDB7 File Offset: 0x0003DDB7
		public _IMethodDeclarationStatement CreateMethodDeclarationStatement(IToken token)
		{
			return new MethodDeclarationStatement(token);
		}

		// Token: 0x060015F7 RID: 5623 RVA: 0x0003EDBF File Offset: 0x0003DDBF
		public _IVariableDeclarationListStatement CreateVariableDeclarationListStatement()
		{
			return new VariableDeclarationListStatement();
		}

		// Token: 0x060015F8 RID: 5624 RVA: 0x0003EDC6 File Offset: 0x0003DDC6
		public _IVariableDeclarationListStatement CreateVariableDeclarationListStatement(IToken token)
		{
			return new VariableDeclarationListStatement(token);
		}

		// Token: 0x060015F9 RID: 5625 RVA: 0x0003EDCE File Offset: 0x0003DDCE
		public _IVariableDeclarationStatement CreateVariableDeclarationStatement()
		{
			return new VariableDeclarationStatement();
		}

		// Token: 0x060015FA RID: 5626 RVA: 0x0003EDD5 File Offset: 0x0003DDD5
		public _IVariableDeclarationStatement CreateVariableDeclarationStatement(IToken token)
		{
			return new VariableDeclarationStatement(token);
		}

		// Token: 0x060015FB RID: 5627 RVA: 0x0003EDDD File Offset: 0x0003DDDD
		public _IErrorExpression CreateErrorExpression()
		{
			return new ErrorExpression();
		}

		// Token: 0x060015FC RID: 5628 RVA: 0x0003EDE4 File Offset: 0x0003DDE4
		public _IErrorExpression CreateErrorExpression(IToken token)
		{
			return new ErrorExpression(token);
		}

		// Token: 0x060015FD RID: 5629 RVA: 0x0003EDEC File Offset: 0x0003DDEC
		public _IProgramCounterExpression CreateProgramCounterExpression()
		{
			return new ProgramCounterExpression();
		}

		// Token: 0x060015FE RID: 5630 RVA: 0x0003EDF3 File Offset: 0x0003DDF3
		public _IFramePointerExpression CreateFramePointerExpression()
		{
			return new FramePointerExpression();
		}

		// Token: 0x060015FF RID: 5631 RVA: 0x0003EDFA File Offset: 0x0003DDFA
		public _ICallInstanceExpression CreateCallInstanceExpression(bool bWriteAccess, ICompiledType ctype, IIntermediateValueLocation ivl)
		{
			return new CallInstanceExpression(bWriteAccess, ctype, ivl);
		}

		// Token: 0x06001600 RID: 5632 RVA: 0x0003EE04 File Offset: 0x0003DE04
		public _ICallExpression CreateCallExpression()
		{
			return new CallExpression();
		}

		// Token: 0x06001601 RID: 5633 RVA: 0x0003EE0B File Offset: 0x0003DE0B
		public _ICallExpression CreateCallExpression(_IExpression expCallee)
		{
			return new CallExpression(expCallee);
		}

		// Token: 0x06001602 RID: 5634 RVA: 0x0003EE13 File Offset: 0x0003DE13
		public _ICallExpression CreateCallExpression(_IExpression expCallee, IToken token)
		{
			return new CallExpression(expCallee, token);
		}

		// Token: 0x06001603 RID: 5635 RVA: 0x0003EE1C File Offset: 0x0003DE1C
		public _IOperatorExpression CreateOperatorExpression()
		{
			return new OperatorExpression();
		}

		// Token: 0x06001604 RID: 5636 RVA: 0x0003EE23 File Offset: 0x0003DE23
		public _IOperatorExpression CreateOperatorExpression(Operator op)
		{
			return new OperatorExpression(op);
		}

		// Token: 0x06001605 RID: 5637 RVA: 0x0003EE2B File Offset: 0x0003DE2B
		public _IOperatorExpression CreateOperatorExpression(Operator op, IToken token)
		{
			return new OperatorExpression(op, token);
		}

		// Token: 0x06001606 RID: 5638 RVA: 0x0003EE34 File Offset: 0x0003DE34
		public _IConversionExpression CreateConversionExpression()
		{
			return new ConversionExpression();
		}

		// Token: 0x06001607 RID: 5639 RVA: 0x0003EE3B File Offset: 0x0003DE3B
		public _IConversionExpression CreateConversionExpression(TypeClass tcFrom, TypeClass tcTo)
		{
			return new ConversionExpression(tcFrom, tcTo);
		}

		// Token: 0x06001608 RID: 5640 RVA: 0x0003EE44 File Offset: 0x0003DE44
		public _IConversionExpression CreateConversionExpression(TypeClass tcFrom, TypeClass tcTo, IToken token)
		{
			return new ConversionExpression(tcFrom, tcTo, token);
		}

		// Token: 0x06001609 RID: 5641 RVA: 0x0003EE4E File Offset: 0x0003DE4E
		public _IImplicitConversionExpression CreateImplicitConversionExpression()
		{
			return new ImplicitConversionExpression();
		}

		// Token: 0x0600160A RID: 5642 RVA: 0x0003EE55 File Offset: 0x0003DE55
		public _IImplicitConversionExpression CreateImplicitConversionExpression(TypeClass tcFrom, TypeClass tcTo)
		{
			return new ImplicitConversionExpression(tcFrom, tcTo);
		}

		// Token: 0x0600160B RID: 5643 RVA: 0x0003EE5E File Offset: 0x0003DE5E
		public _IImplicitConversionExpression CreateImplicitConversionExpression(TypeClass tcFrom, TypeClass tcTo, IToken token)
		{
			return new ImplicitConversionExpression(tcFrom, tcTo, token);
		}

		// Token: 0x0600160C RID: 5644 RVA: 0x0003EE68 File Offset: 0x0003DE68
		public _INewExpression CreateNewExpression()
		{
			return new NewExpression();
		}

		// Token: 0x0600160D RID: 5645 RVA: 0x0003EE6F File Offset: 0x0003DE6F
		public _INewExpression CreateNewExpression(_IType typeIn, _IExpression expCount)
		{
			return new NewExpression(typeIn, expCount);
		}

		// Token: 0x0600160E RID: 5646 RVA: 0x0003EE78 File Offset: 0x0003DE78
		public _INewExpression CreateNewExpression(_IType typeIn, _IExpression expCount, IToken token)
		{
			return new NewExpression(typeIn, expCount, token);
		}

		// Token: 0x0600160F RID: 5647 RVA: 0x0003EE82 File Offset: 0x0003DE82
		public _ICastExpression CreateCastExpression()
		{
			return new CastExpression();
		}

		// Token: 0x06001610 RID: 5648 RVA: 0x0003EE89 File Offset: 0x0003DE89
		public _ICastExpression CreateCastExpression(_IExpression expWithType, _IExpression expBase)
		{
			return new CastExpression(expWithType, expBase);
		}

		// Token: 0x06001611 RID: 5649 RVA: 0x0003EE92 File Offset: 0x0003DE92
		public _IThisExpression CreateThisExpression()
		{
			return new ThisExpression();
		}

		// Token: 0x06001612 RID: 5650 RVA: 0x0003EE99 File Offset: 0x0003DE99
		public _IThisExpression CreateThisExpression(IToken token)
		{
			return new ThisExpression(token);
		}

		// Token: 0x06001613 RID: 5651 RVA: 0x0003EEA1 File Offset: 0x0003DEA1
		public _IBaseExpression CreateBaseExpression()
		{
			return new BaseExpression();
		}

		// Token: 0x06001614 RID: 5652 RVA: 0x0003EEA8 File Offset: 0x0003DEA8
		public _IBaseExpression CreateBaseExpression(IToken token)
		{
			return new BaseExpression(token);
		}

		// Token: 0x06001615 RID: 5653 RVA: 0x0003EEB0 File Offset: 0x0003DEB0
		public _ITypeExpression CreateTypeExpression()
		{
			return new TypeExpression();
		}

		// Token: 0x06001616 RID: 5654 RVA: 0x0003EEB7 File Offset: 0x0003DEB7
		public _ITypeExpression CreateTypeExpression(ICompiledType cType)
		{
			return new TypeExpression(cType);
		}

		// Token: 0x06001617 RID: 5655 RVA: 0x0003EEBF File Offset: 0x0003DEBF
		public _IAddressExpression CreateAddressExpression()
		{
			return new AddressExpression();
		}

		// Token: 0x06001618 RID: 5656 RVA: 0x0003EEC6 File Offset: 0x0003DEC6
		public _IAddressExpression CreateAddressExpression(IDirectVariable dirvar)
		{
			return new AddressExpression(dirvar);
		}

		// Token: 0x06001619 RID: 5657 RVA: 0x0003EECE File Offset: 0x0003DECE
		public _IAddressExpression CreateAddressExpression(IDirectVariable dirvar, IToken token)
		{
			return new AddressExpression(dirvar, token);
		}

		// Token: 0x0600161A RID: 5658 RVA: 0x0003EED7 File Offset: 0x0003DED7
		public _IVariableExpression CreateVariableExpression(string stName)
		{
			return new VariableExpression(stName);
		}

		// Token: 0x0600161B RID: 5659 RVA: 0x0003EEDF File Offset: 0x0003DEDF
		public _IVariableExpression CreateVariableExpression(string stName, IToken token)
		{
			return new VariableExpression(stName, token);
		}

		// Token: 0x0600161C RID: 5660 RVA: 0x0003EEE8 File Offset: 0x0003DEE8
		public _ICompoAccessExpression CreateCompoAccessExpression()
		{
			return new CompoAccessExpression();
		}

		// Token: 0x0600161D RID: 5661 RVA: 0x0003EEEF File Offset: 0x0003DEEF
		public _ICompoAccessExpression CreateCompoAccessExpression(_IExpression expLeft)
		{
			return new CompoAccessExpression(expLeft);
		}

		// Token: 0x0600161E RID: 5662 RVA: 0x0003EEF7 File Offset: 0x0003DEF7
		public _ICompoAccessExpression CreateCompoAccessExpression(_IExpression expLeft, IToken token)
		{
			return new CompoAccessExpression(expLeft, token);
		}

		// Token: 0x0600161F RID: 5663 RVA: 0x0003EF00 File Offset: 0x0003DF00
		public _IPartialAccessExpression CreatePartialAccessExpression(_IExpression left, DirectVariableSize partSize, int partOffset)
		{
			return new PartialAccessExpression(left)
			{
				PartSize = partSize,
				PartOffset = partOffset
			};
		}

		// Token: 0x06001620 RID: 5664 RVA: 0x0003EF16 File Offset: 0x0003DF16
		public _IPartialAccessExpression CreatePartialAccessExpression(IToken token, _IExpression left, DirectVariableSize partSize, int partOffset)
		{
			return new PartialAccessExpression(left, token)
			{
				PartSize = partSize,
				PartOffset = partOffset
			};
		}

		// Token: 0x06001621 RID: 5665 RVA: 0x0003EF2E File Offset: 0x0003DF2E
		public _IDeRefAccessExpression CreateDeRefAccessExpression()
		{
			return new DeRefAccessExpression();
		}

		// Token: 0x06001622 RID: 5666 RVA: 0x0003EF35 File Offset: 0x0003DF35
		public _IDeRefAccessExpression CreateDeRefAccessExpression(_IExpression exp)
		{
			return new DeRefAccessExpression(exp);
		}

		// Token: 0x06001623 RID: 5667 RVA: 0x0003EF3D File Offset: 0x0003DF3D
		public _IDeRefAccessExpression CreateDeRefAccessExpression(_IExpression exp, IToken token)
		{
			return new DeRefAccessExpression(exp, token);
		}

		// Token: 0x06001624 RID: 5668 RVA: 0x0003EF46 File Offset: 0x0003DF46
		public IIntermediateValueLocation CreateIntermediateValueLocation()
		{
			return new IntermediateValueLocation();
		}

		// Token: 0x06001625 RID: 5669 RVA: 0x0003EF4D File Offset: 0x0003DF4D
		public _IDeRefAccessExpression CreateImplicitDeRefAccessExpression(_IExpression etoken)
		{
			return new ImplicitDeRefAccessExpression(etoken);
		}

		// Token: 0x06001626 RID: 5670 RVA: 0x0003EF55 File Offset: 0x0003DF55
		public _IIndexAccessExpression CreateIndexAccessExpression()
		{
			return new IndexAccessExpression();
		}

		// Token: 0x06001627 RID: 5671 RVA: 0x0003EF5C File Offset: 0x0003DF5C
		public _IIndexAccessExpression CreateIndexAccessExpression(_IExpression expBase)
		{
			return new IndexAccessExpression(expBase);
		}

		// Token: 0x06001628 RID: 5672 RVA: 0x0003EF64 File Offset: 0x0003DF64
		public _IIndexAccessExpression CreateIndexAccessExpression(_IExpression expBase, IToken token)
		{
			return new IndexAccessExpression(expBase, token);
		}

		// Token: 0x06001629 RID: 5673 RVA: 0x0003EF6D File Offset: 0x0003DF6D
		public _ICopyScopeExpression CreateCopyScopeExpression()
		{
			return new CopyScopeExpression();
		}

		// Token: 0x0600162A RID: 5674 RVA: 0x0003EF74 File Offset: 0x0003DF74
		public _ICopyScopeExpression CreateCopyScopeExpression(_IExpression expBase)
		{
			return new CopyScopeExpression(expBase);
		}

		// Token: 0x0600162B RID: 5675 RVA: 0x0003EF7C File Offset: 0x0003DF7C
		public _ICopyScopeExpression CreateCopyScopeExpression(_IExpression expBase, IToken token)
		{
			return new CopyScopeExpression(expBase, token);
		}

		// Token: 0x0600162C RID: 5676 RVA: 0x0003EF85 File Offset: 0x0003DF85
		public _IGlobalScopeExpression CreateGlobalScopeExpression()
		{
			return new GlobalScopeExpression();
		}

		// Token: 0x0600162D RID: 5677 RVA: 0x0003EF8C File Offset: 0x0003DF8C
		public _IGlobalScopeExpression CreateGlobalScopeExpression(_IExpression expBase)
		{
			return new GlobalScopeExpression(expBase);
		}

		// Token: 0x0600162E RID: 5678 RVA: 0x0003EF94 File Offset: 0x0003DF94
		public _IGlobalScopeExpression CreateGlobalScopeExpression(_IExpression expBase, IToken token)
		{
			return new GlobalScopeExpression(expBase, token);
		}

		// Token: 0x0600162F RID: 5679 RVA: 0x0003EF9D File Offset: 0x0003DF9D
		public _ISystemScopeExpression CreateSystemScopeExpression()
		{
			return new SystemScopeExpression();
		}

		// Token: 0x06001630 RID: 5680 RVA: 0x0003EFA4 File Offset: 0x0003DFA4
		public _ISystemScopeExpression CreateSystemScopeExpression(string stBaseName)
		{
			return new SystemScopeExpression(stBaseName);
		}

		// Token: 0x06001631 RID: 5681 RVA: 0x0003EFAC File Offset: 0x0003DFAC
		public _ISystemScopeExpression CreateSystemScopeExpression(_IExpression expBase)
		{
			return new SystemScopeExpression(expBase);
		}

		// Token: 0x06001632 RID: 5682 RVA: 0x0003EFB4 File Offset: 0x0003DFB4
		public _ISystemScopeExpression CreateSystemScopeExpression(_IExpression expBase, IToken token)
		{
			return new SystemScopeExpression(expBase, token);
		}

		// Token: 0x06001633 RID: 5683 RVA: 0x0003EFBD File Offset: 0x0003DFBD
		public _IPoolScopeExpression CreatePoolScopeExpression(_IExpression expBase)
		{
			return new PoolScopeExpression(expBase);
		}

		// Token: 0x06001634 RID: 5684 RVA: 0x0003EFC5 File Offset: 0x0003DFC5
		public _IPoolScopeExpression CreatePoolScopeExpression(_IExpression expBase, IToken token)
		{
			return new PoolScopeExpression(expBase, token);
		}

		// Token: 0x06001635 RID: 5685 RVA: 0x0003EFCE File Offset: 0x0003DFCE
		public _INamespaceAccessExpression CreateNamespaceAccessExpression(_IExpression expNamespace, _IExpression expAccess)
		{
			return new NamespaceAccessExpression(expNamespace, expAccess);
		}

		// Token: 0x06001636 RID: 5686 RVA: 0x0003EFD7 File Offset: 0x0003DFD7
		public _INamespaceAccessExpression CreateNamespaceAccessExpression(_IExpression expNamespace, _IExpression expAccess, IToken token)
		{
			return new NamespaceAccessExpression(expNamespace, expAccess, token);
		}

		// Token: 0x06001637 RID: 5687 RVA: 0x0003EFE1 File Offset: 0x0003DFE1
		public _ICurrentTaskExpression CreateCurrentTaskExpression(_IExpression expBase, IToken token)
		{
			return new CurrentTaskExpression(expBase, token);
		}

		// Token: 0x06001638 RID: 5688 RVA: 0x0003EFEA File Offset: 0x0003DFEA
		public _INullExpression CreateNullExpression()
		{
			return new NullExpression();
		}

		// Token: 0x06001639 RID: 5689 RVA: 0x0003EFF1 File Offset: 0x0003DFF1
		public _INullExpression CreateNullExpression(IToken token)
		{
			return new NullExpression(token);
		}

		// Token: 0x0600163A RID: 5690 RVA: 0x0003EFF9 File Offset: 0x0003DFF9
		public _INullStatement CreateNullStatement()
		{
			return new NullStatement();
		}

		// Token: 0x0600163B RID: 5691 RVA: 0x0003F000 File Offset: 0x0003E000
		public _INullStatement CreateNullStatement(IToken token)
		{
			return new NullStatement(token);
		}

		// Token: 0x0600163C RID: 5692 RVA: 0x0003F008 File Offset: 0x0003E008
		public _IMultipleIndexInitialization CreateMultipleIndexInitialisation()
		{
			return new MultipleIndexInitialisation();
		}

		// Token: 0x0600163D RID: 5693 RVA: 0x0003F00F File Offset: 0x0003E00F
		public _IMultipleIndexInitialization CreateMultipleIndexInitialisation(IToken token)
		{
			return new MultipleIndexInitialisation(token);
		}

		// Token: 0x0600163E RID: 5694 RVA: 0x0003F017 File Offset: 0x0003E017
		public _IArrayInitialization CreateArrayInitialisation()
		{
			return new ArrayInitialisation();
		}

		// Token: 0x0600163F RID: 5695 RVA: 0x0003F01E File Offset: 0x0003E01E
		public _IArrayInitialization CreateArrayInitialisation(IToken token)
		{
			return new ArrayInitialisation(token);
		}

		// Token: 0x06001640 RID: 5696 RVA: 0x0003F026 File Offset: 0x0003E026
		public _IStructureInitialization CreateStructureInitialisation()
		{
			return new StructureInitialisation();
		}

		// Token: 0x06001641 RID: 5697 RVA: 0x0003F02D File Offset: 0x0003E02D
		public _IStructureInitialization CreateStructureInitialisation(IToken token)
		{
			return new StructureInitialisation(token);
		}

		// Token: 0x06001642 RID: 5698 RVA: 0x0003F035 File Offset: 0x0003E035
		public _IDefineReference CreateDefineReference()
		{
			return new DefineReference();
		}

		// Token: 0x06001643 RID: 5699 RVA: 0x0003F03C File Offset: 0x0003E03C
		public _IDefineReference CreateDefineReference(IToken token)
		{
			return new DefineReference(token);
		}

		// Token: 0x06001644 RID: 5700 RVA: 0x0003F044 File Offset: 0x0003E044
		public _IVariableReference CreateVariableReference()
		{
			return new VariableReference();
		}

		// Token: 0x06001645 RID: 5701 RVA: 0x0003F04B File Offset: 0x0003E04B
		public _IVariableReference CreateVariableReference(IToken token)
		{
			return new VariableReference(token);
		}

		// Token: 0x06001646 RID: 5702 RVA: 0x0003F053 File Offset: 0x0003E053
		public _ITypeReference CreateTypeReference()
		{
			return new TypeReference();
		}

		// Token: 0x06001647 RID: 5703 RVA: 0x0003F05A File Offset: 0x0003E05A
		public _ITypeReference CreateTypeReference(IToken token)
		{
			return new TypeReference(token);
		}

		// Token: 0x06001648 RID: 5704 RVA: 0x0003F062 File Offset: 0x0003E062
		public _ITypeReference CreateTypeReference(IToken token, _IExpression expPath)
		{
			return new TypeReference(token, expPath);
		}

		// Token: 0x06001649 RID: 5705 RVA: 0x0003F06B File Offset: 0x0003E06B
		public _IPouReference CreatePouReference()
		{
			return new PouReference();
		}

		// Token: 0x0600164A RID: 5706 RVA: 0x0003F072 File Offset: 0x0003E072
		public _IPouReference CreatePouReference(IToken token)
		{
			return new PouReference(token);
		}

		// Token: 0x0600164B RID: 5707 RVA: 0x0003F07A File Offset: 0x0003E07A
		public _IPouReference CreatePouReference(IToken token, _IExpression expPath)
		{
			return new PouReference(token, expPath);
		}

		// Token: 0x0600164C RID: 5708 RVA: 0x0003F083 File Offset: 0x0003E083
		public _ITaskReference CreateTaskReference()
		{
			return new TaskReference();
		}

		// Token: 0x0600164D RID: 5709 RVA: 0x0003F08A File Offset: 0x0003E08A
		public _ITaskReference CreateTaskReference(IToken token)
		{
			return new TaskReference(token);
		}

		// Token: 0x0600164E RID: 5710 RVA: 0x0003F092 File Offset: 0x0003E092
		public _ITaskReference CreateTaskReference(IToken token, string stTaskName)
		{
			return new TaskReference(token, stTaskName);
		}

		// Token: 0x0600164F RID: 5711 RVA: 0x0003F09B File Offset: 0x0003E09B
		public _IResourceReference CreateResourceReference()
		{
			return new ResourceReference();
		}

		// Token: 0x06001650 RID: 5712 RVA: 0x0003F0A2 File Offset: 0x0003E0A2
		public _IResourceReference CreateResourceReference(IToken token)
		{
			return new ResourceReference(token);
		}

		// Token: 0x06001651 RID: 5713 RVA: 0x0003F0AA File Offset: 0x0003E0AA
		public _IResourceReference CreateResourceReference(IToken token, string stResourceName)
		{
			return new ResourceReference(token, stResourceName);
		}

		// Token: 0x06001652 RID: 5714 RVA: 0x0003F0B3 File Offset: 0x0003E0B3
		public _IXRefExpression CreateXRefExpression()
		{
			return new XRefExpression();
		}

		// Token: 0x06001653 RID: 5715 RVA: 0x0003F0BA File Offset: 0x0003E0BA
		public _IXRefExpression CreateXRefExpression(IToken token)
		{
			return new XRefExpression(token);
		}

		// Token: 0x06001654 RID: 5716 RVA: 0x0003F0C2 File Offset: 0x0003E0C2
		public _IXRefExpression CreateXRefExpression(IToken token, _IItemReference itref, _IItemReference itrefFrom)
		{
			return new XRefExpression(token, itref, itrefFrom);
		}

		// Token: 0x06001655 RID: 5717 RVA: 0x0003F0CC File Offset: 0x0003E0CC
		public _IDefinedExpression CreateDefinedExpression()
		{
			return new DefinedExpression();
		}

		// Token: 0x06001656 RID: 5718 RVA: 0x0003F0D3 File Offset: 0x0003E0D3
		public _IDefinedExpression CreateDefinedExpression(IToken token)
		{
			return new DefinedExpression(token);
		}

		// Token: 0x06001657 RID: 5719 RVA: 0x0003F0DB File Offset: 0x0003E0DB
		public _IDefinedExpression CreateDefinedExpression(IToken token, _IItemReference itref)
		{
			return new DefinedExpression(token, itref);
		}

		// Token: 0x06001658 RID: 5720 RVA: 0x0003F0E4 File Offset: 0x0003E0E4
		public _IProjectDefinedExpression CreateProjectDefinedExpression(IToken token)
		{
			return new ProjectDefinedExpression(token);
		}

		// Token: 0x06001659 RID: 5721 RVA: 0x0003F0EC File Offset: 0x0003E0EC
		public _ICompilerVersionExpression CreateCompilerVersionExpression()
		{
			return new CompilerVersionExpression();
		}

		// Token: 0x0600165A RID: 5722 RVA: 0x0003F0F3 File Offset: 0x0003E0F3
		public _ICompilerVersionExpression CreateCompilerVersionExpression(IToken token)
		{
			return new CompilerVersionExpression(token);
		}

		// Token: 0x0600165B RID: 5723 RVA: 0x0003F0FB File Offset: 0x0003E0FB
		public _ICompilerVersionExpression CreateCompilerVersionExpression(IToken token, Version versionToTest, Operator opComparison)
		{
			return new CompilerVersionExpression(token, versionToTest, opComparison);
		}

		// Token: 0x0600165C RID: 5724 RVA: 0x0003F105 File Offset: 0x0003E105
		public _IRuntimeVersionExpression CreateRuntimeVersionExpression()
		{
			return new RuntimeVersionExpression();
		}

		// Token: 0x0600165D RID: 5725 RVA: 0x0003F10C File Offset: 0x0003E10C
		public _IRuntimeVersionExpression CreateRuntimeVersionExpression(IToken token)
		{
			return new RuntimeVersionExpression(token);
		}

		// Token: 0x0600165E RID: 5726 RVA: 0x0003F114 File Offset: 0x0003E114
		public _IRuntimeVersionExpression CreateRuntimeVersionExpression(IToken token, Version versionToTest, Operator opComparison)
		{
			return new RuntimeVersionExpression(token, versionToTest, opComparison);
		}

		// Token: 0x0600165F RID: 5727 RVA: 0x0003F11E File Offset: 0x0003E11E
		public _IHasTypeExpression CreateHasTypeExpression()
		{
			return new HasTypeExpression();
		}

		// Token: 0x06001660 RID: 5728 RVA: 0x0003F125 File Offset: 0x0003E125
		public _IHasTypeExpression CreateHasTypeExpression(IToken token)
		{
			return new HasTypeExpression(token);
		}

		// Token: 0x06001661 RID: 5729 RVA: 0x0003F130 File Offset: 0x0003E130
		public IHasTypeExpression CreateHasTypeExpression(IExprementPosition pos, IVariableReference varref, ICompiledType type)
		{
			HasTypeExpression hasTypeExpression = new HasTypeExpression();
			hasTypeExpression.Variable = (varref as VariableReference);
			hasTypeExpression.ReferencedType = type;
			this.SetExprementPosition(pos, hasTypeExpression);
			return hasTypeExpression;
		}

		// Token: 0x06001662 RID: 5730 RVA: 0x0003F15F File Offset: 0x0003E15F
		public _IIsEnumTypeExpression CreateIsEnumTypeExpression()
		{
			return new IsEnumTypeExpression();
		}

		// Token: 0x06001663 RID: 5731 RVA: 0x0003F166 File Offset: 0x0003E166
		public _IIsEnumTypeExpression CreateIsEnumTypeExpression(IToken token)
		{
			return new IsEnumTypeExpression(token);
		}

		// Token: 0x06001664 RID: 5732 RVA: 0x0003F16E File Offset: 0x0003E16E
		public _IHasAttributeExpression CreateHasAttributeExpression()
		{
			return new HasAttributeExpression();
		}

		// Token: 0x06001665 RID: 5733 RVA: 0x0003F175 File Offset: 0x0003E175
		public _IHasAttributeExpression CreateHasAttributeExpression(IToken token)
		{
			return new HasAttributeExpression(token);
		}

		// Token: 0x06001666 RID: 5734 RVA: 0x0003F17D File Offset: 0x0003E17D
		public _IHasAttributeExpression CreateHasAttributeExpression(IToken token, _IItemReference itref, string stAttribute)
		{
			return new HasAttributeExpression(token, itref, stAttribute);
		}

		// Token: 0x06001667 RID: 5735 RVA: 0x0003F187 File Offset: 0x0003E187
		public _IHasValueExpression CreateHasValueExpression()
		{
			return new HasValueExpression();
		}

		// Token: 0x06001668 RID: 5736 RVA: 0x0003F18E File Offset: 0x0003E18E
		public _IHasValueExpression CreateHasValueExpression(IToken token)
		{
			return new HasValueExpression(token);
		}

		// Token: 0x06001669 RID: 5737 RVA: 0x0003F196 File Offset: 0x0003E196
		public _IHasValueExpression CreateHasValueExpression(IToken token, string stDefine, string stValue)
		{
			return new HasValueExpression(token, stDefine, stValue);
		}

		// Token: 0x0600166A RID: 5738 RVA: 0x0003F1A0 File Offset: 0x0003E1A0
		public _IHasConstantValueExpression CreateHasConstantValueExpression()
		{
			return new HasConstantValueExpression();
		}

		// Token: 0x0600166B RID: 5739 RVA: 0x0003F1A7 File Offset: 0x0003E1A7
		public _IHasConstantValueExpression CreateHasConstantValueExpression(IToken token)
		{
			return new HasConstantValueExpression(token);
		}

		// Token: 0x0600166C RID: 5740 RVA: 0x0003F1AF File Offset: 0x0003E1AF
		public _IHasConstantValueExpression CreateHasConstantValueExpression(IToken token, _IExpression constant, _IExpression value, Operator comparison)
		{
			return new HasConstantValueExpression(token, constant, value);
		}

		// Token: 0x0600166D RID: 5741 RVA: 0x0003F1B9 File Offset: 0x0003E1B9
		public _IPragmaOperatorExpression CreatePragmaOperatorExpression()
		{
			return new PragmaOperatorExpression();
		}

		// Token: 0x0600166E RID: 5742 RVA: 0x0003F1C0 File Offset: 0x0003E1C0
		public _IPragmaOperatorExpression CreatePragmaOperatorExpression(PragmaOperator op)
		{
			return new PragmaOperatorExpression(op);
		}

		// Token: 0x0600166F RID: 5743 RVA: 0x0003F1C8 File Offset: 0x0003E1C8
		public _IPragmaOperatorExpression CreatePragmaOperatorExpression(PragmaOperator op, IToken token)
		{
			return new PragmaOperatorExpression(op, token);
		}

		// Token: 0x06001670 RID: 5744 RVA: 0x0003F1D1 File Offset: 0x0003E1D1
		public _IPragmaAssertion CreatePragmaAssertion()
		{
			return new PragmaAssertion();
		}

		// Token: 0x06001671 RID: 5745 RVA: 0x0003F1D8 File Offset: 0x0003E1D8
		public _IPragmaAssertion CreatePragmaAssertion(_IExpression expCondition, string stErrorOutput)
		{
			return new PragmaAssertion(expCondition, stErrorOutput);
		}

		// Token: 0x06001672 RID: 5746 RVA: 0x0003F1E1 File Offset: 0x0003E1E1
		public _IPragmaAssertion CreatePragmaAssertion(_IExpression expCondition, string stErrorOutput, IToken token)
		{
			return new PragmaAssertion(expCondition, stErrorOutput, token);
		}

		// Token: 0x06001673 RID: 5747 RVA: 0x0003F1EB File Offset: 0x0003E1EB
		public _IPragmaElseIf CreatePragmaElseIf()
		{
			return new PragmaElseIf();
		}

		// Token: 0x06001674 RID: 5748 RVA: 0x0003F1F2 File Offset: 0x0003E1F2
		public _IPragmaElseIf CreatePragmaElseIf(_IPragmaExpression expCondition, _IStatement stControlled)
		{
			return new PragmaElseIf(expCondition, stControlled);
		}

		// Token: 0x06001675 RID: 5749 RVA: 0x0003F1FB File Offset: 0x0003E1FB
		public _IPragmaIfStatement CreatePragmaIfStatement()
		{
			return new PragmaIfStatement();
		}

		// Token: 0x06001676 RID: 5750 RVA: 0x0003F202 File Offset: 0x0003E202
		public _IPragmaIfStatement CreatePragmaIfStatement(_IExpression expCond)
		{
			return new PragmaIfStatement(expCond);
		}

		// Token: 0x06001677 RID: 5751 RVA: 0x0003F20A File Offset: 0x0003E20A
		public _IPragmaIfStatement CreatePragmaIfStatement(_IExpression expCond, IToken token)
		{
			return new PragmaIfStatement(expCond, token);
		}

		// Token: 0x06001678 RID: 5752 RVA: 0x0003F213 File Offset: 0x0003E213
		public _IBreakPointStatement CreateBreakPointStatement()
		{
			return new BreakPointStatement();
		}

		// Token: 0x06001679 RID: 5753 RVA: 0x0003F21A File Offset: 0x0003E21A
		public _IBreakPointStatement CreateBreakPointStatement(IToken token, long lPosition, long lSuccessorPosition)
		{
			return new BreakPointStatement(token, lPosition, lSuccessorPosition);
		}

		// Token: 0x0600167A RID: 5754 RVA: 0x0003F224 File Offset: 0x0003E224
		public _IDefineStatement CreateDefineStatement()
		{
			return new DefineStatement();
		}

		// Token: 0x0600167B RID: 5755 RVA: 0x0003F22B File Offset: 0x0003E22B
		public _IDefineStatement CreateDefineStatement(IToken token, bool bDefine, string stIdent, string stValue)
		{
			return new DefineStatement(token, bDefine, stIdent, stValue);
		}

		// Token: 0x0600167C RID: 5756 RVA: 0x0003F237 File Offset: 0x0003E237
		public _IDefineStatement CreateDefineStatement(IToken token, bool bDefine, string stIdent)
		{
			return new DefineStatement(token, bDefine, stIdent);
		}

		// Token: 0x0600167D RID: 5757 RVA: 0x0003F241 File Offset: 0x0003E241
		public _IBitAccess CreateBitAccess()
		{
			return new BitAccess();
		}

		// Token: 0x0600167E RID: 5758 RVA: 0x0003F248 File Offset: 0x0003E248
		public _IBitAccess CreateBitAccess(_IExpression expBase, byte byBitNr)
		{
			return new BitAccess(expBase, byBitNr);
		}

		// Token: 0x0600167F RID: 5759 RVA: 0x0003F251 File Offset: 0x0003E251
		public IBitWriteAccess CreateBitWriteAccess(int nSignatureId, int nArea, int nOffset, byte byBitNr, IMinimalPosition position, string stSymbol)
		{
			return new BitWriteAccess(nSignatureId, nArea, nOffset, byBitNr, position, stSymbol);
		}

		// Token: 0x06001680 RID: 5760 RVA: 0x0003F261 File Offset: 0x0003E261
		public _ICompilerAttribute CreateCompilerAttribute(string stName, string stValue)
		{
			return new CompilerAttribute(stName, stValue);
		}

		// Token: 0x06001681 RID: 5761 RVA: 0x0003F26A File Offset: 0x0003E26A
		public _IArrayType CreateArrayType()
		{
			return new ArrayType();
		}

		// Token: 0x06001682 RID: 5762 RVA: 0x0003F271 File Offset: 0x0003E271
		public _IArrayType CreateArrayType(_IType typeBase)
		{
			return new ArrayType(typeBase);
		}

		// Token: 0x06001683 RID: 5763 RVA: 0x0003F279 File Offset: 0x0003E279
		public _IVectorType CreateVectorType(_IType typeBase, _IExpression expDim)
		{
			return new VectorType(typeBase, expDim);
		}

		// Token: 0x06001684 RID: 5764 RVA: 0x0003F282 File Offset: 0x0003E282
		public _IRangeAwareAnyIntType CreateRangeAwareAnyIntType()
		{
			return new RangeAwareAnyIntType();
		}

		// Token: 0x06001685 RID: 5765 RVA: 0x0003F289 File Offset: 0x0003E289
		public _IUserdefType CreateUserdefType()
		{
			return new UserdefType();
		}

		// Token: 0x06001686 RID: 5766 RVA: 0x0003F290 File Offset: 0x0003E290
		public _IUserdefType CreateUserdefType(_IExpression expname)
		{
			return new UserdefType(expname);
		}

		// Token: 0x06001687 RID: 5767 RVA: 0x0003F298 File Offset: 0x0003E298
		public _IUserdefType CreateUserdefType(string stname)
		{
			return new UserdefType(stname);
		}

		// Token: 0x06001688 RID: 5768 RVA: 0x0003F2A0 File Offset: 0x0003E2A0
		public _IPointerType CreatePointerType()
		{
			return new PointerType();
		}

		// Token: 0x06001689 RID: 5769 RVA: 0x0003F2A7 File Offset: 0x0003E2A7
		public _IPointerType CreatePointerType(_IType typeBase)
		{
			return new PointerType(typeBase);
		}

		// Token: 0x0600168A RID: 5770 RVA: 0x0003F2AF File Offset: 0x0003E2AF
		public _IReferenceType CreateReferenceType()
		{
			return new ReferenceType();
		}

		// Token: 0x0600168B RID: 5771 RVA: 0x0003F2B6 File Offset: 0x0003E2B6
		public _IReferenceType CreateReferenceType(_IType typeBase)
		{
			return new ReferenceType(typeBase);
		}

		// Token: 0x0600168C RID: 5772 RVA: 0x0003F2BE File Offset: 0x0003E2BE
		public _IImplicitReferenceType CreateImplicitReferenceType(_IType typeBase)
		{
			return new ImplicitReferenceType(typeBase);
		}

		// Token: 0x0600168D RID: 5773 RVA: 0x0003F2C6 File Offset: 0x0003E2C6
		public _IInOutReferenceType CreateInOutReferenceType(_IType typeBase)
		{
			return new InOutReferenceType(typeBase);
		}

		// Token: 0x0600168E RID: 5774 RVA: 0x0003F2CE File Offset: 0x0003E2CE
		public _IStringType CreateStringType()
		{
			return new StringType();
		}

		// Token: 0x0600168F RID: 5775 RVA: 0x0003F2D5 File Offset: 0x0003E2D5
		public _IWStringType CreateWStringType()
		{
			return new WStringType();
		}

		// Token: 0x06001690 RID: 5776 RVA: 0x0003F2DC File Offset: 0x0003E2DC
		public _IAliasType CreateAliasType(_IType orgType)
		{
			return new AliasType(orgType);
		}

		// Token: 0x06001691 RID: 5777 RVA: 0x0003F2E4 File Offset: 0x0003E2E4
		public _IEnumType CreateEnumType(string stName)
		{
			return new EnumType(stName);
		}

		// Token: 0x06001692 RID: 5778 RVA: 0x0003F2EC File Offset: 0x0003E2EC
		public _IEnumType CreateEnumType(string stName, int idSignature)
		{
			return new EnumType(stName, idSignature);
		}

		// Token: 0x06001693 RID: 5779 RVA: 0x0003F2F5 File Offset: 0x0003E2F5
		public _IParamsType CreateParamsType(_IType typeBase, _IExpression count)
		{
			return new ParamsType(typeBase, count);
		}

		// Token: 0x06001694 RID: 5780 RVA: 0x0003F2FE File Offset: 0x0003E2FE
		public _ISafeBoolType CreateSafeBoolType()
		{
			return new SafeBoolType();
		}

		// Token: 0x06001695 RID: 5781 RVA: 0x0003F305 File Offset: 0x0003E305
		public _ISafeByteType CreateSafeByteType()
		{
			return new SafeByteType();
		}

		// Token: 0x06001696 RID: 5782 RVA: 0x0003F30C File Offset: 0x0003E30C
		public _ISafeSIntType CreateSafeSIntType()
		{
			return new SafeSIntType();
		}

		// Token: 0x06001697 RID: 5783 RVA: 0x0003F313 File Offset: 0x0003E313
		public _ISafeUSIntType CreateSafeUSIntType()
		{
			return new SafeUSIntType();
		}

		// Token: 0x06001698 RID: 5784 RVA: 0x0003F31A File Offset: 0x0003E31A
		public _ISafeWordType CreateSafeWordType()
		{
			return new SafeWordType();
		}

		// Token: 0x06001699 RID: 5785 RVA: 0x0003F321 File Offset: 0x0003E321
		public _ISafeIntType CreateSafeIntType()
		{
			return new SafeIntType();
		}

		// Token: 0x0600169A RID: 5786 RVA: 0x0003F328 File Offset: 0x0003E328
		public _ISafeUIntType CreateSafeUIntType()
		{
			return new SafeUIntType();
		}

		// Token: 0x0600169B RID: 5787 RVA: 0x0003F32F File Offset: 0x0003E32F
		public _ISafeDWordType CreateSafeDWordType()
		{
			return new SafeDWordType();
		}

		// Token: 0x0600169C RID: 5788 RVA: 0x0003F336 File Offset: 0x0003E336
		public _ISafeDIntType CreateSafeDIntType()
		{
			return new SafeDIntType();
		}

		// Token: 0x0600169D RID: 5789 RVA: 0x0003F33D File Offset: 0x0003E33D
		public _ISafeUDIntType CreateSafeUDIntType()
		{
			return new SafeUDIntType();
		}

		// Token: 0x0600169E RID: 5790 RVA: 0x0003F344 File Offset: 0x0003E344
		public _ISafeLWordType CreateSafeLWordType()
		{
			return new SafeLWordType();
		}

		// Token: 0x0600169F RID: 5791 RVA: 0x0003F34B File Offset: 0x0003E34B
		public _ISafeLIntType CreateSafeLIntType()
		{
			return new SafeLIntType();
		}

		// Token: 0x060016A0 RID: 5792 RVA: 0x0003F352 File Offset: 0x0003E352
		public _ISafeULIntType CreateSafeULIntType()
		{
			return new SafeULIntType();
		}

		// Token: 0x060016A1 RID: 5793 RVA: 0x0003F359 File Offset: 0x0003E359
		public _ISafeTimeType CreateSafeTimeType()
		{
			return new SafeTimeType();
		}

		// Token: 0x060016A2 RID: 5794 RVA: 0x0003F360 File Offset: 0x0003E360
		public _ISafeRealType CreateSafeRealType()
		{
			return new SafeRealType();
		}

		// Token: 0x060016A3 RID: 5795 RVA: 0x0003F367 File Offset: 0x0003E367
		public _ISafeLRealType CreateSafeLRealType()
		{
			return new SafeLRealType();
		}

		// Token: 0x060016A4 RID: 5796 RVA: 0x0003F36E File Offset: 0x0003E36E
		public _IRetainBoolType CreateRetainBoolType()
		{
			return new RetainBoolType();
		}

		// Token: 0x060016A5 RID: 5797 RVA: 0x0003F375 File Offset: 0x0003E375
		public _IBool16Type CreateBool16Type()
		{
			return new Bool16Type();
		}

		// Token: 0x060016A6 RID: 5798 RVA: 0x0003F37C File Offset: 0x0003E37C
		public _IRetainByteType CreateRetainByteType()
		{
			return new RetainByteType();
		}

		// Token: 0x060016A7 RID: 5799 RVA: 0x0003F383 File Offset: 0x0003E383
		public _IRetainSIntType CreateRetainSIntType()
		{
			return new RetainSIntType();
		}

		// Token: 0x060016A8 RID: 5800 RVA: 0x0003F38A File Offset: 0x0003E38A
		public _IRetainUSIntType CreateRetainUSIntType()
		{
			return new RetainUSIntType();
		}

		// Token: 0x060016A9 RID: 5801 RVA: 0x0003F391 File Offset: 0x0003E391
		public _IXDWordType CreateXDWordType()
		{
			return new XDWordType();
		}

		// Token: 0x060016AA RID: 5802 RVA: 0x0003F398 File Offset: 0x0003E398
		public _IXLWordType CreateXLWordType()
		{
			return new XLWordType();
		}

		// Token: 0x060016AB RID: 5803 RVA: 0x0003F39F File Offset: 0x0003E39F
		public _IXDIntType CreateXDIntType()
		{
			return new XDIntType();
		}

		// Token: 0x060016AC RID: 5804 RVA: 0x0003F3A6 File Offset: 0x0003E3A6
		public _IBoolType CreateBoolType()
		{
			return new BoolType();
		}

		// Token: 0x060016AD RID: 5805 RVA: 0x0003F3AD File Offset: 0x0003E3AD
		public _IDirectAddressBitType CreateDirectAddressBitType()
		{
			return new DirectAdressBitType();
		}

		// Token: 0x060016AE RID: 5806 RVA: 0x0003F3B4 File Offset: 0x0003E3B4
		public _IByteType CreateByteType()
		{
			return new ByteType();
		}

		// Token: 0x060016AF RID: 5807 RVA: 0x0003F3BB File Offset: 0x0003E3BB
		public _ISIntType CreateSIntType()
		{
			return new SIntType();
		}

		// Token: 0x060016B0 RID: 5808 RVA: 0x0003F3C2 File Offset: 0x0003E3C2
		public _IUSIntType CreateUSIntType()
		{
			return new USIntType();
		}

		// Token: 0x060016B1 RID: 5809 RVA: 0x0003F3C9 File Offset: 0x0003E3C9
		public _IIntType CreateIntType()
		{
			return new IntType();
		}

		// Token: 0x060016B2 RID: 5810 RVA: 0x0003F3D0 File Offset: 0x0003E3D0
		public _IUIntType CreateUIntType()
		{
			return new UIntType();
		}

		// Token: 0x060016B3 RID: 5811 RVA: 0x0003F3D7 File Offset: 0x0003E3D7
		public _IWordType CreateWordType()
		{
			return new WordType();
		}

		// Token: 0x060016B4 RID: 5812 RVA: 0x0003F3DE File Offset: 0x0003E3DE
		public _IDIntType CreateDIntType()
		{
			return new DIntType();
		}

		// Token: 0x060016B5 RID: 5813 RVA: 0x0003F3E5 File Offset: 0x0003E3E5
		public _IUDIntType CreateUDIntType()
		{
			return new UDIntType();
		}

		// Token: 0x060016B6 RID: 5814 RVA: 0x0003F3EC File Offset: 0x0003E3EC
		public _IDWordType CreateDWordType()
		{
			return new DWordType();
		}

		// Token: 0x060016B7 RID: 5815 RVA: 0x0003F3F3 File Offset: 0x0003E3F3
		public _ILIntType CreateLIntType()
		{
			return new LIntType();
		}

		// Token: 0x060016B8 RID: 5816 RVA: 0x0003F3FA File Offset: 0x0003E3FA
		public _IULIntType CreateULIntType()
		{
			return new ULIntType();
		}

		// Token: 0x060016B9 RID: 5817 RVA: 0x0003F401 File Offset: 0x0003E401
		public _ILWordType CreateLWordType()
		{
			return new LWordType();
		}

		// Token: 0x060016BA RID: 5818 RVA: 0x0003F408 File Offset: 0x0003E408
		public _IRealType CreateRealType()
		{
			return new RealType();
		}

		// Token: 0x060016BB RID: 5819 RVA: 0x0003F40F File Offset: 0x0003E40F
		public _ILRealType CreateLRealType()
		{
			return new LRealType();
		}

		// Token: 0x060016BC RID: 5820 RVA: 0x0003F416 File Offset: 0x0003E416
		public _ILazyType CreateLazyType()
		{
			return new LazyType();
		}

		// Token: 0x060016BD RID: 5821 RVA: 0x0003F41D File Offset: 0x0003E41D
		public _IBitConstType CreateBitConstType()
		{
			return new BitConstType();
		}

		// Token: 0x060016BE RID: 5822 RVA: 0x0003F424 File Offset: 0x0003E424
		public _IBitType CreateBitType()
		{
			return new BitType();
		}

		// Token: 0x060016BF RID: 5823 RVA: 0x0003F42B File Offset: 0x0003E42B
		public _IUXIntType CreateUXIntType()
		{
			return new UXIntType();
		}

		// Token: 0x060016C0 RID: 5824 RVA: 0x0003F432 File Offset: 0x0003E432
		public _IXIntType CreateXIntType()
		{
			return new XIntType();
		}

		// Token: 0x060016C1 RID: 5825 RVA: 0x0003F439 File Offset: 0x0003E439
		public _IXWordType CreateXWordType()
		{
			return new XWordType();
		}

		// Token: 0x060016C2 RID: 5826 RVA: 0x0003F440 File Offset: 0x0003E440
		public _IXUDIntType CreateXUDIntType()
		{
			return new XUDIntType();
		}

		// Token: 0x060016C3 RID: 5827 RVA: 0x0003F447 File Offset: 0x0003E447
		public _IXULIntType CreateXULIntType()
		{
			return new XULIntType();
		}

		// Token: 0x060016C4 RID: 5828 RVA: 0x0003F44E File Offset: 0x0003E44E
		public _IXLIntType CreateXLIntType()
		{
			return new XLIntType();
		}

		// Token: 0x060016C5 RID: 5829 RVA: 0x0003F455 File Offset: 0x0003E455
		public _IDateType CreateDateType()
		{
			return new DateType();
		}

		// Token: 0x060016C6 RID: 5830 RVA: 0x0003F45C File Offset: 0x0003E45C
		public _ITimeOfDayType CreateTimeOfDayType()
		{
			return new TimeOfDayType();
		}

		// Token: 0x060016C7 RID: 5831 RVA: 0x0003F463 File Offset: 0x0003E463
		public _IDateAndTimeType CreateDateAndTimeType()
		{
			return new DateAndTimeType();
		}

		// Token: 0x060016C8 RID: 5832 RVA: 0x0003F46A File Offset: 0x0003E46A
		public _ILDateType CreateLDateType()
		{
			return new LDateType();
		}

		// Token: 0x060016C9 RID: 5833 RVA: 0x0003F471 File Offset: 0x0003E471
		public _ILTimeOfDayType CreateLTimeOfDayType()
		{
			return new LTimeOfDayType();
		}

		// Token: 0x060016CA RID: 5834 RVA: 0x0003F478 File Offset: 0x0003E478
		public _ILDateAndTimeType CreateLDateAndTimeType()
		{
			return new LDateAndTimeType();
		}

		// Token: 0x060016CB RID: 5835 RVA: 0x0003F47F File Offset: 0x0003E47F
		public _ITimeType CreateTimeType()
		{
			return new TimeType();
		}

		// Token: 0x060016CC RID: 5836 RVA: 0x0003F486 File Offset: 0x0003E486
		public _ILTimeType CreateLTimeType()
		{
			return new LTimeType();
		}

		// Token: 0x060016CD RID: 5837 RVA: 0x0003F48D File Offset: 0x0003E48D
		public _IAnyType CreateAnyType()
		{
			return new AnyType();
		}

		// Token: 0x060016CE RID: 5838 RVA: 0x0003F494 File Offset: 0x0003E494
		public _IAnyRealType CreateAnyRealType()
		{
			return new AnyRealType();
		}

		// Token: 0x060016CF RID: 5839 RVA: 0x0003F49B File Offset: 0x0003E49B
		public _IAnyStringType CreateAnyStringType()
		{
			return new AnyStringType();
		}

		// Token: 0x060016D0 RID: 5840 RVA: 0x0003F4A2 File Offset: 0x0003E4A2
		public _IAnyIntType CreateAnyIntType()
		{
			return new AnyIntType();
		}

		// Token: 0x060016D1 RID: 5841 RVA: 0x0003F4A9 File Offset: 0x0003E4A9
		public _IAnyNumType CreateAnyNumType()
		{
			return new AnyNumType();
		}

		// Token: 0x060016D2 RID: 5842 RVA: 0x0003F4B0 File Offset: 0x0003E4B0
		public _IAnyBitType CreateAnyBitType()
		{
			return new AnyBitType();
		}

		// Token: 0x060016D3 RID: 5843 RVA: 0x0003F4B7 File Offset: 0x0003E4B7
		public _IAnyDateType CreateAnyDateType()
		{
			return new AnyDateType();
		}

		// Token: 0x060016D4 RID: 5844 RVA: 0x0003F4BE File Offset: 0x0003E4BE
		public _IAnyBitButBoolIsPreferred CreateAnyBitButBoolIsPreferredType()
		{
			return new AnyBitButBoolIsPreferred();
		}

		// Token: 0x060016D5 RID: 5845 RVA: 0x0003F4C5 File Offset: 0x0003E4C5
		public _IVariableLengthArrayType CreateVariableLengthArrayType()
		{
			return new VariableLengthArrayType();
		}

		// Token: 0x060016D6 RID: 5846 RVA: 0x0003F4CC File Offset: 0x0003E4CC
		public IImplicitEnumerationType CreateImplicitEnumerationType(_IEnumDeclarationListStatement enumdecls, string stImplicitName)
		{
			return new ImplicitEnumerationType(enumdecls, stImplicitName);
		}

		// Token: 0x060016D7 RID: 5847 RVA: 0x0003F4D5 File Offset: 0x0003E4D5
		public _IAddressCodePosition CreateAddressCodePosition(ISourcePosition sp, AccessFlag access, int nTypeSize)
		{
			return new AddressCodePosition(sp, access, nTypeSize);
		}

		// Token: 0x060016D8 RID: 5848 RVA: 0x0003F4DF File Offset: 0x0003E4DF
		public _IPreCompileContext CreatePrecompileContext(string stLibraryPath, Guid applicationGuid, KindOfContext kindof)
		{
			return new PreCompileContext(stLibraryPath, applicationGuid, kindof);
		}

		// Token: 0x060016D9 RID: 5849 RVA: 0x0003F4E9 File Offset: 0x0003E4E9
		public ICompiledCode CreateCompiledCodeDataStub(ICompiledCode compiledcode)
		{
			return new CompiledCode(compiledcode);
		}

		// Token: 0x060016DA RID: 5850 RVA: 0x0003F4F1 File Offset: 0x0003E4F1
		public _ICompiledCodeData CreateCompiledCodeData(Stream stream, int nRelatedId)
		{
			return new CompiledCodeData(stream, nRelatedId);
		}

		// Token: 0x060016DB RID: 5851 RVA: 0x0003F4FA File Offset: 0x0003E4FA
		public ICompiledCode CreateCompiledCodeDataPlaceholder(int nSize, IDataLocation datloc)
		{
			return new CompiledCodeDataPlaceholder(nSize, datloc);
		}

		// Token: 0x060016DC RID: 5852 RVA: 0x0003F503 File Offset: 0x0003E503
		public ICompiledCode CreateCompiledCodeDataReloc(Stream stream, bool bMotorola)
		{
			return new CompiledCodeDataReloc(stream, bMotorola);
		}

		// Token: 0x060016DD RID: 5853 RVA: 0x0003F50C File Offset: 0x0003E50C
		public ICompiledCode CreateCompiledCodeDataReloc(byte[] bytes, bool bMotorolaByteOrder)
		{
			return new CompiledCodeDataReloc(bytes, bMotorolaByteOrder);
		}

		// Token: 0x060016DE RID: 5854 RVA: 0x0003F515 File Offset: 0x0003E515
		public _IRelocationList CreateRelocationList()
		{
			return new RelocationList();
		}

		// Token: 0x060016DF RID: 5855 RVA: 0x0003F51C File Offset: 0x0003E51C
		public _IDataSegment CreateDataSegment(ushort usArea, int nAddress, int nSize, DataSegmentFlags flags)
		{
			return new DataSegment(usArea, nAddress, nSize, flags);
		}

		// Token: 0x060016E0 RID: 5856 RVA: 0x0003F528 File Offset: 0x0003E528
		public _IDataManager CreateDataManager()
		{
			return new DataManager();
		}

		// Token: 0x060016E1 RID: 5857 RVA: 0x0003F52F File Offset: 0x0003E52F
		public ILanguageModelManagerTargetSettings CreateTargetSettings()
		{
			return new LocalTargetSettingsObject();
		}

		// Token: 0x060016E2 RID: 5858 RVA: 0x0003F536 File Offset: 0x0003E536
		public IEnumerable<KeyValuePair<string, string>> GetAttributesDefinedAtTopOfSequence(ISequenceStatement stmt)
		{
			return CompilerProxy.GetAttributesDefinedAtTopOfSequence(stmt);
		}

		// Token: 0x060016E3 RID: 5859 RVA: 0x0003F540 File Offset: 0x0003E540
		public IVariable5 AddLocalStackVariableToCompiledSignature(string stVariableName, VarFlag vf, ICompiledType ctype, IExpression expInit, ISignature sign)
		{
			_IVariable ivariable = this.CreateVariable(null);
			ivariable.Name = stVariableName;
			ivariable._Type = (ctype as _IType);
			if (sign.POUType == Operator.FunctionBlock || sign.POUType == Operator.Program)
			{
				ivariable.SetFlag(VarFlag.IsCompiled | VarFlag.Implicit | VarFlag.Temp | vf, true);
			}
			else
			{
				ivariable.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.Implicit | vf, true);
			}
			ivariable.Id = (sign as _ISignature).NextId;
			(sign as _ISignature).AddVariable(ivariable);
			ivariable.Initial = expInit;
			return ivariable;
		}

		// Token: 0x060016E4 RID: 5860 RVA: 0x0003F5C8 File Offset: 0x0003E5C8
		public void SetType(IExpression exp, ICompiledType type)
		{
			_IExpression iexpression = exp as _IExpression;
			if (iexpression != null)
			{
				iexpression.Type = type;
			}
		}

		// Token: 0x060016E5 RID: 5861 RVA: 0x0003F5E6 File Offset: 0x0003E5E6
		public IPointerType CreatePointerType(IType typeBase)
		{
			return this.CreatePointerType((_IType)typeBase);
		}

		// Token: 0x060016E6 RID: 5862 RVA: 0x0003F5F4 File Offset: 0x0003E5F4
		public IReferenceType CreateReferenceType(IType typeBase)
		{
			return this.CreateReferenceType((_IType)typeBase);
		}

		// Token: 0x060016E7 RID: 5863 RVA: 0x0003F602 File Offset: 0x0003E602
		public ICompactedParseTreeInformation CreateCompactedParseTreeInformation()
		{
			return new CompactedPrecompileParseTreeInformation();
		}

		// Token: 0x060016E8 RID: 5864 RVA: 0x0003F609 File Offset: 0x0003E609
		public ICompactedCompiledParseTreeInformation CreateCompactedCompiledParseTreeInformation()
		{
			return new CompactedCompiledParseTreeInformation();
		}

		// Token: 0x060016E9 RID: 5865 RVA: 0x0003F610 File Offset: 0x0003E610
		public IGenericUserdefType CreateGenericUserdefType(_IExpression expname)
		{
			return new GenericUserdefType(expname);
		}

		// Token: 0x060016EA RID: 5866 RVA: 0x0003F618 File Offset: 0x0003E618
		public IGenericUserdefType CreateGenericUserdefType()
		{
			return new GenericUserdefType();
		}

		// Token: 0x060016EB RID: 5867 RVA: 0x0003F61F File Offset: 0x0003E61F
		public IStaticMemorySegment CreateStaticMemorySegment(Guid guidSubApplication, int offset, int size)
		{
			return new StaticMemorySegment(guidSubApplication, offset, size);
		}

		// Token: 0x060016EC RID: 5868 RVA: 0x0003F629 File Offset: 0x0003E629
		public IVirtualFunctionTableSerializable CreateVirtualFunctionTable()
		{
			return new VFTable();
		}

		// Token: 0x060016ED RID: 5869 RVA: 0x0003F630 File Offset: 0x0003E630
		public _IFunctionPointerEntry CreateVirtualFunctionTableFunctionPointerEntry()
		{
			return new FunctionPointerEntry();
		}

		// Token: 0x060016EE RID: 5870 RVA: 0x0003F637 File Offset: 0x0003E637
		public IInterfaceOffsetEntrySerializable CreateVirtualFunctionTableInterfaceOffsetEntry()
		{
			return new InterfaceOffsetEntry();
		}

		// Token: 0x060016EF RID: 5871 RVA: 0x0003F63E File Offset: 0x0003E63E
		public _ISubrangeType CreateSubrangeType()
		{
			return new SubrangeType();
		}

		// Token: 0x060016F0 RID: 5872 RVA: 0x0003F645 File Offset: 0x0003E645
		public IImplicitEnumerationType CreateImplicitEnumerationType()
		{
			return new ImplicitEnumerationType();
		}

		// Token: 0x060016F1 RID: 5873 RVA: 0x0003F4BE File Offset: 0x0003E4BE
		public _IAnyBitButBoolIsPreferred CreateAnyBitButBoolIsPreferred()
		{
			return new AnyBitButBoolIsPreferred();
		}

		// Token: 0x060016F2 RID: 5874 RVA: 0x0003EBA3 File Offset: 0x0003DBA3
		public _IXStringType CreateXStringType()
		{
			return new XStringType();
		}

		// Token: 0x060016F3 RID: 5875 RVA: 0x0003F64C File Offset: 0x0003E64C
		public _IVectorType CreateVectorType()
		{
			return new VectorType();
		}

		// Token: 0x060016F4 RID: 5876 RVA: 0x0003F653 File Offset: 0x0003E653
		public _IParamsType CreateParamsType()
		{
			return new ParamsType();
		}

		// Token: 0x060016F5 RID: 5877 RVA: 0x0003F65A File Offset: 0x0003E65A
		public _IEnumType CreateEnumType()
		{
			return new EnumType();
		}

		// Token: 0x060016F6 RID: 5878 RVA: 0x0003F661 File Offset: 0x0003E661
		public _IAliasType CreateAliasType()
		{
			return new AliasType();
		}

		// Token: 0x060016F7 RID: 5879 RVA: 0x0003F668 File Offset: 0x0003E668
		public ICrossReferenceSerializable CreateCrossReference()
		{
			return new CrossReference();
		}

		// Token: 0x060016F8 RID: 5880 RVA: 0x0003F66F File Offset: 0x0003E66F
		public IBreakpointSerializable CreateBreakpoint()
		{
			return new Breakpoint();
		}

		// Token: 0x060016F9 RID: 5881 RVA: 0x0003F676 File Offset: 0x0003E676
		public IBreakpointListSerializable CreateBreakpointList()
		{
			return new BreakpointList();
		}

		// Token: 0x060016FA RID: 5882 RVA: 0x000207CD File Offset: 0x0001F7CD
		public ICompileOptionsSerializable CreateCompileOptions()
		{
			return new CompileOptionsSerializable();
		}

		// Token: 0x060016FB RID: 5883 RVA: 0x0003F67D File Offset: 0x0003E67D
		public ILibInfoSerializable CreateLibInfo()
		{
			return new LibInfoNew();
		}

		// Token: 0x060016FC RID: 5884 RVA: 0x0003F684 File Offset: 0x0003E684
		public _ISlotPOUList2 CreateSlotPOUList()
		{
			return new SlotPOUList();
		}

		// Token: 0x060016FD RID: 5885 RVA: 0x0003F68B File Offset: 0x0003E68B
		public IDirectVariableCrossRefTableSerializable CreateDirVariableCrossRefTable()
		{
			return new DirectVariableCrossRefTable();
		}

		// Token: 0x060016FE RID: 5886 RVA: 0x0003F692 File Offset: 0x0003E692
		public IAddressCrossReferenceSerializable CreateAddressCrossReference(int nCodeId)
		{
			return new AddressCrossReference(nCodeId);
		}

		// Token: 0x060016FF RID: 5887 RVA: 0x0003F69A File Offset: 0x0003E69A
		public IAddressCodePositionSerializable CreateAddressCodePosition()
		{
			return new AddressCodePosition();
		}

		// Token: 0x06001700 RID: 5888 RVA: 0x0003F6A1 File Offset: 0x0003E6A1
		public _IImplicitReferenceVariable CreateImplitReferenceVariable(int nSignatureId, _IVariable variable)
		{
			return new ImplicitReferenceVariable(nSignatureId, variable);
		}

		// Token: 0x06001701 RID: 5889 RVA: 0x0003F6AA File Offset: 0x0003E6AA
		public _ITaskList CreateTaskList()
		{
			return new TaskList();
		}

		// Token: 0x06001702 RID: 5890 RVA: 0x0003F6B1 File Offset: 0x0003E6B1
		public _ILibraryTable2 CreateLibraryTable()
		{
			return new LibraryTableWithoutPlaceholders();
		}

		// Token: 0x06001703 RID: 5891 RVA: 0x0003F6B8 File Offset: 0x0003E6B8
		public IMemoryManagerSerializable CreateMemMan()
		{
			return new MemMan();
		}

		// Token: 0x06001704 RID: 5892 RVA: 0x0003F6BF File Offset: 0x0003E6BF
		public IBreakpointSerializable CreateTryCatchBreakpoint()
		{
			return new TryCatchBreakpoint();
		}

		// Token: 0x06001705 RID: 5893 RVA: 0x0003F6C6 File Offset: 0x0003E6C6
		public ICompiledCodeSerializable CreateCompiledCodeDataStub()
		{
			return new CompiledCode();
		}

		// Token: 0x06001706 RID: 5894 RVA: 0x0003F6CD File Offset: 0x0003E6CD
		public ICompiledCodeDataSerializable CreateCompiledCodeData()
		{
			return new CompiledCodeData();
		}

		// Token: 0x06001707 RID: 5895 RVA: 0x0003F6D4 File Offset: 0x0003E6D4
		public ICompiledCodeDataRelocSerializable CreateCompiledCodeDataReloc()
		{
			return new CompiledCodeDataReloc();
		}
	}
}
