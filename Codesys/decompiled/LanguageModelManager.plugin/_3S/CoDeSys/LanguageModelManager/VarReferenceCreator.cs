using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.OnlineExpressionInterpreter;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000108 RID: 264
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes", Justification = "Will be fixed with CDS-89482")]
	internal static class VarReferenceCreator
	{
		// Token: 0x17000571 RID: 1393
		// (get) Token: 0x06001381 RID: 4993 RVA: 0x0003700C File Offset: 0x0003600C
		private static LanguageModelManagerConsolidated LMM
		{
			get
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr;
			}
		}

		// Token: 0x06001382 RID: 4994 RVA: 0x00037018 File Offset: 0x00036018
		internal static IVarRef GetVarReference(Guid guidApplication, string stPOUName, string stExpression)
		{
			if (guidApplication == Guid.Empty)
			{
				throw new ArgumentNullException("guidApplication");
			}
			if (stPOUName == null)
			{
				throw new ArgumentNullException("stPOUName");
			}
			if (stExpression == null)
			{
				throw new ArgumentNullException("stExpression");
			}
			_ICompileContext referenceContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContext(guidApplication);
			if (referenceContext == null)
			{
				throw new ArgumentException(Strings.ErrNoApplication);
			}
			_ISignature isignature = referenceContext[stPOUName];
			if (isignature == null)
			{
				throw new ArgumentException(Strings.ErrSignatureNotFound);
			}
			return VarReferenceCreator.GetVarReference(VarReferenceCreator.GetExpression(stExpression, referenceContext, CompilerProxy.CreateScope(referenceContext, isignature.Id)), isignature.Id, guidApplication);
		}

		// Token: 0x06001383 RID: 4995 RVA: 0x000370AD File Offset: 0x000360AD
		internal static IVarRef GetVarReference(Guid guidApplication, string stExpression)
		{
			return VarReferenceCreator.GetVarReference(guidApplication, stExpression, false);
		}

		// Token: 0x06001384 RID: 4996 RVA: 0x000370B7 File Offset: 0x000360B7
		internal static IVarRef GetVarReference(Guid guidApplication, string stExpression, bool bAllowShortExpressions)
		{
			if (guidApplication == Guid.Empty)
			{
				throw new ArgumentNullException("guidApplication");
			}
			if (stExpression == null)
			{
				throw new ArgumentNullException("stExpression");
			}
			return VarReferenceCreator.GetVarReferenceInternal(guidApplication, stExpression, bAllowShortExpressions);
		}

		// Token: 0x06001385 RID: 4997 RVA: 0x000370E8 File Offset: 0x000360E8
		internal static IVarRef GetVarReferenceInternal(Guid guidApplication, string stExpression, bool bAllowShortExpressions)
		{
			if (stExpression == null)
			{
				throw new ArgumentNullException("stExpression");
			}
			IScanner scanner = VarReferenceCreator.LMM.CreateScanner(stExpression, false, false, false, false);
			scanner.AllowMultipleUnderlines = true;
			_ICompileContext referenceContext = VarReferenceCreator.LMM.GetReferenceContext(guidApplication);
			if (referenceContext == null)
			{
				throw new ArgumentException(Strings.ErrNoApplication);
			}
			IToken position;
			int iSignId = VarReferenceCreator.ScanForSignatureId(scanner, referenceContext, out position);
			_IParser iparser = CompilerProxy.CreateParser(scanner);
			_IExpression iexpression = iparser.ParseExpression() as _IExpression;
			if (iexpression == null)
			{
				throw new ArgumentException(Strings.ErrInvalidExpression);
			}
			IVarRef varReference = VarReferenceCreator.GetVarReference(iexpression, iSignId, guidApplication, null, null);
			if (varReference == null)
			{
				VarRef varRef = new VarRef(iexpression, guidApplication, CompilerProxy.CreateGlobalScope(referenceContext));
				varRef.SetFlag(VarRefFlag.Invalid, true);
				return varRef;
			}
			scanner.SetPosition(position);
			bool flag;
			_IExpression iexpression2 = iparser.ParseSTOperand(out flag);
			if (!flag && !bAllowShortExpressions && varReference is VarRef)
			{
				VarReferenceCreator.GetVarReference(iexpression2, -1, guidApplication);
				((VarRef)varReference)._WatchExpression = iexpression2;
				if (iexpression._CompiledType != null)
				{
					iexpression2._CompiledType = iexpression._CompiledType;
				}
			}
			return varReference;
		}

		// Token: 0x06001386 RID: 4998 RVA: 0x000371E0 File Offset: 0x000361E0
		private static ISignature FindSignatureWithNamespace(IScope5 scope, IScanner scanner, string stPOUName)
		{
			IList<ISignature> list = scope[stPOUName];
			ISignature signature = (list != null && list.Count > 0) ? list[0] : null;
			if (signature == null)
			{
				IScope5 scope2 = scope.FindScope(new VariableExpression(stPOUName)) as IScope5;
				string text = null;
				IToken token;
				if (VarReferenceCreator.CheckForIdentifier(scanner, out token))
				{
					text = scanner.GetIdentifier(token);
				}
				if (scope2 != null && text != null)
				{
					signature = VarReferenceCreator.FindSignatureWithNamespace(scope2, scanner, text);
				}
			}
			return signature;
		}

		// Token: 0x06001387 RID: 4999 RVA: 0x0003724C File Offset: 0x0003624C
		private static int ScanForSignatureId(IScanner scanner, _ICompileContext comcon, out IToken tokenSaveProgramName)
		{
			int result = -1;
			if (VarReferenceCreator.CheckForIdentifier(scanner, out tokenSaveProgramName))
			{
				string identifier = scanner.GetIdentifier(tokenSaveProgramName);
				ISignature signature = VarReferenceCreator.FindSignatureWithNamespace(CompilerProxy.CreateGlobalScope(comcon), scanner, identifier);
				if (signature == null)
				{
					scanner.SetPosition(tokenSaveProgramName);
				}
				else
				{
					result = signature.Id;
					VarReferenceCreator.ScanForSubsignature(scanner, ref result, signature);
				}
			}
			else
			{
				scanner.SetPosition(tokenSaveProgramName);
			}
			return result;
		}

		// Token: 0x06001388 RID: 5000 RVA: 0x000372A4 File Offset: 0x000362A4
		private static void ScanForSubsignature(IScanner scanner, ref int iSignatureId, ISignature signFound)
		{
			IToken token;
			if (!VarReferenceCreator.CheckForIdentifier(scanner, out token))
			{
				scanner.SetPosition(token);
				return;
			}
			string identifier = scanner.GetIdentifier(token);
			if (signFound.GetSubSignature(identifier) == null)
			{
				scanner.SetPosition(token);
				return;
			}
			iSignatureId = signFound.GetSubSignature(identifier).Id;
		}

		// Token: 0x06001389 RID: 5001 RVA: 0x000372EC File Offset: 0x000362EC
		private static bool CheckForIdentifier(IScanner scanner, out IToken tokenSaveProgramName)
		{
			IToken token;
			return scanner.GetNext(out tokenSaveProgramName) == TokenType.Identifier && scanner.GetNext(out token) == TokenType.Operator && (scanner.GetOperator(token) == Operator.Period || scanner.GetOperator(token) == Operator.Hash);
		}

		// Token: 0x0600138A RID: 5002 RVA: 0x00037330 File Offset: 0x00036330
		internal static void ParseInstancePathForExplicitApplication(string stInstancePath, Guid guidExplicitApplication, out int iSignatureId, out _IExpression expr, out _IExpression exprFull, bool canExpectPoolOperator = false)
		{
			Guid guid;
			VarReferenceCreator.ParseInstancePath(stInstancePath, guidExplicitApplication, out iSignatureId, out guid, out expr, out exprFull, canExpectPoolOperator);
		}

		// Token: 0x0600138B RID: 5003 RVA: 0x0003734C File Offset: 0x0003634C
		internal static void ParseInstancePath(string stInstancePath, out int iSignatureId, out Guid guidApplication, out _IExpression expr, out _IExpression exprFull, bool canExpectPoolOperator = false)
		{
			VarReferenceCreator.ParseInstancePath(stInstancePath, Guid.Empty, out iSignatureId, out guidApplication, out expr, out exprFull, canExpectPoolOperator);
		}

		// Token: 0x0600138C RID: 5004 RVA: 0x00037360 File Offset: 0x00036360
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "")]
		private static void ParseInstancePath(string stInstancePath, Guid guidExplicitApplication, out int iSignatureId, out Guid guidApplication, out _IExpression expr, out _IExpression exprFull, bool canExpectPoolOperator)
		{
			if (stInstancePath == null)
			{
				throw new ArgumentNullException("stInstancePath");
			}
			Guid guid = Guid.Empty;
			IScanner scanner = VarReferenceCreator.LMM.CreateScanner(stInstancePath, false, false, false, false);
			scanner.AllowMultipleUnderlines = true;
			IToken token;
			scanner.GetNext(out token);
			string text = string.Empty;
			if (token.Type == TokenType.Identifier)
			{
				text = scanner.GetIdentifier(token);
			}
			IToken token2;
			IToken token3;
			if (scanner.GetNext(out token2) == TokenType.Operator && scanner.GetOperator(token2) == Operator.Period && scanner.GetNext(out token3) == TokenType.Identifier)
			{
				string identifier = scanner.GetIdentifier(token3);
				text = text + "." + identifier;
				guid = VarReferenceCreator.LMM.GetApplicationGuidByName(text);
				if (guid == Guid.Empty)
				{
					scanner.SetPosition(token);
				}
				else if (guidExplicitApplication != Guid.Empty && guid != guidExplicitApplication)
				{
					scanner.SetPosition(token);
				}
				else if (scanner.GetNext(out token3) != TokenType.Operator && (scanner.GetOperator(token3) != Operator.Period || scanner.GetOperator(token3) != Operator.Range))
				{
					throw new ArgumentException(Strings.ErrExpressionContainsNoAppName);
				}
			}
			else
			{
				scanner.SetPosition(token);
			}
			guidApplication = Guid.Empty;
			if (guidExplicitApplication == Guid.Empty && guid != Guid.Empty)
			{
				guidApplication = guid;
			}
			_ICompileContext referenceContext;
			if (guidExplicitApplication != Guid.Empty)
			{
				referenceContext = VarReferenceCreator.LMM.GetReferenceContext(guidExplicitApplication);
			}
			else
			{
				referenceContext = VarReferenceCreator.LMM.GetReferenceContext(guidApplication);
			}
			if (referenceContext == null)
			{
				throw new ArgumentException(Strings.ErrNoApplication);
			}
			IToken token4;
			TokenType next = scanner.GetNext(out token4);
			iSignatureId = Common.InvalidID;
			IScope5 scope;
			if (next == TokenType.Operator && canExpectPoolOperator && scanner.GetOperator(token4) == Operator.__PoolScope)
			{
				scope = ((_IScope2)CompilerProxy.CreateGlobalScope(referenceContext)).PoolScope;
				IToken token5;
				scanner.GetNext(out token5);
				next = scanner.GetNext(out token4);
			}
			else
			{
				scope = CompilerProxy.CreateGlobalScope(referenceContext);
			}
			if (next == TokenType.Identifier)
			{
				ISignature signature = null;
				token3 = token4;
				bool greaterEqualV = APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351700;
				ISignature[] array2;
				for (;;)
				{
					if (next == TokenType.Identifier)
					{
						string identifier2 = scanner.GetIdentifier(token3);
						if (greaterEqualV)
						{
							scope.FindListBeforeVariable = true;
						}
						IVariable[] array;
						IScope scope2;
						scope.FindDeclaration(identifier2, out array, out array2, out scope2);
						if (array != null && array.Length != 0)
						{
							goto IL_264;
						}
						if (array2 == null)
						{
							scope = (scope2 as IScope5);
						}
						if (array2 != null && array2.Length != 0)
						{
							break;
						}
						if (array2 == null && scope == null)
						{
							goto IL_264;
						}
					}
					if ((next = scanner.GetNext(out token3)) == TokenType.End)
					{
						goto IL_264;
					}
				}
				signature = array2[0];
				IL_264:
				if (signature == null)
				{
					scanner.SetPosition(token4);
				}
				else
				{
					iSignatureId = signature.Id;
					if (scanner.GetNext(out token2) == TokenType.Identifier && scanner.GetNext(out token3) == TokenType.Operator && scanner.GetOperator(token3) == Operator.Period)
					{
						string identifier3 = scanner.GetIdentifier(token2);
						if (signature.GetSubSignature(identifier3) == null)
						{
							scanner.SetPosition(token2);
						}
						else
						{
							iSignatureId = signature.GetSubSignature(identifier3).Id;
						}
					}
					else
					{
						scanner.SetPosition(token2);
					}
				}
			}
			else
			{
				scanner.SetPosition(token4);
			}
			expr = null;
			if (scanner.GetNext(out token3) == TokenType.Operator && scanner.GetOperator(token3) == Operator.Period)
			{
				_IParser iparser = CompilerProxy.CreateParser(scanner);
				expr = (iparser.ParseExpression() as _IExpression);
			}
			scanner.SetPosition(token4);
			_IParser iparser2 = CompilerProxy.CreateParser(scanner);
			exprFull = (iparser2.ParseExpression() as _IExpression);
		}

		// Token: 0x0600138D RID: 5005 RVA: 0x000376A8 File Offset: 0x000366A8
		internal static IFlowVarRef GetFlowVarReference(string stExpression, string stInstancePath, long lPosition)
		{
			int iSignId;
			Guid guidApplication;
			_IExpression iexpression;
			_IExpression iexpression2;
			VarReferenceCreator.ParseInstancePath(stInstancePath, out iSignId, out guidApplication, out iexpression, out iexpression2, !string.IsNullOrEmpty(stInstancePath));
			IVarRef varRef = null;
			if (iexpression != null)
			{
				varRef = VarReferenceCreator.GetVarReference(iexpression, iSignId, guidApplication, null);
			}
			_ICompileContext referenceContext = VarReferenceCreator.LMM.GetReferenceContext(guidApplication);
			IScanner scanner = VarReferenceCreator.LMM.CreateScanner(stExpression, false, false, false, false);
			scanner.AllowMultipleUnderlines = true;
			_IExpression iexpression3 = CompilerProxy.CreateParser(scanner).ParseExpression() as _IExpression;
			if (iexpression3 == null)
			{
				throw new ArgumentException(Strings.ErrInvalidExpression);
			}
			if (varRef != null && varRef.WatchExpression != null && varRef.WatchExpression.Type != null && varRef.WatchExpression.Type is UserdefType)
			{
				ISignature signature = (varRef.WatchExpression.Type as UserdefType).GetSignature(CompilerProxy.CreateGlobalScope(referenceContext));
				return VarReferenceCreator.GetFlowVarReference(iexpression3, signature.Id, guidApplication, lPosition, stInstancePath, varRef);
			}
			return VarReferenceCreator.GetFlowVarReference(iexpression3, iSignId, guidApplication, lPosition, stInstancePath, null);
		}

		// Token: 0x0600138E RID: 5006 RVA: 0x00037790 File Offset: 0x00036790
		internal static IVarRef GetVarReference(string stExpression)
		{
			if (stExpression == null)
			{
				throw new ArgumentNullException("stExpression");
			}
			string stExpression2;
			string stName = VarReferenceCreator.ExtractResourceFromExpression(stExpression, out stExpression2);
			return VarReferenceCreator.GetVarReferenceInternal(VarReferenceCreator.LMM.GetApplicationGuidByName(stName), stExpression2, false);
		}

		// Token: 0x0600138F RID: 5007 RVA: 0x000377C8 File Offset: 0x000367C8
		internal static IVarRef GetVarReference(string stExpression, string stInstancePath)
		{
			int iSignId;
			Guid guidApplication;
			_IExpression iexpression;
			_IExpression iexpression2;
			VarReferenceCreator.ParseInstancePath(stInstancePath, out iSignId, out guidApplication, out iexpression, out iexpression2, !string.IsNullOrEmpty(stInstancePath));
			IVarRef varRef = null;
			if (iexpression != null)
			{
				varRef = VarReferenceCreator.GetVarReference(iexpression, iSignId, guidApplication, null);
			}
			_ICompileContext referenceContext = VarReferenceCreator.LMM.GetReferenceContext(guidApplication);
			IScanner scanner = VarReferenceCreator.LMM.CreateScanner(stExpression, false, false, false, false);
			scanner.AllowMultipleUnderlines = true;
			_IExpression iexpression3 = CompilerProxy.CreateParser(scanner).ParseExpression() as _IExpression;
			if (iexpression3 == null)
			{
				throw new ArgumentException(Strings.ErrInvalidExpression);
			}
			if (varRef != null && varRef.WatchExpression != null && varRef.WatchExpression.Type != null && varRef.WatchExpression.Type is UserdefType)
			{
				ISignature signature = (varRef.WatchExpression.Type as UserdefType).GetSignature(CompilerProxy.CreateGlobalScope(referenceContext));
				return VarReferenceCreator.GetVarReference(iexpression3, signature.Id, guidApplication, varRef as VarRef, stInstancePath);
			}
			return VarReferenceCreator.GetVarReference(iexpression3, iSignId, guidApplication, varRef as VarRef);
		}

		// Token: 0x06001390 RID: 5008 RVA: 0x000378B8 File Offset: 0x000368B8
		internal static IFlowVarRef GetFlowVarReference(_IExpression exp, int iSignId, Guid guidApplication, long lPosition, string stInstancePath, IVarRef varrefInstance)
		{
			ISourcePosition sourcepos = new SourcePosition(-1, Guid.Empty, lPosition, 0, 0);
			_ICompileContext referenceContext = VarReferenceCreator.LMM.GetReferenceContext(guidApplication);
			if (referenceContext == null)
			{
				throw new ArgumentException(Strings.ErrNoApplication);
			}
			return referenceContext.GetFlowPositionBySourcePostion(exp, iSignId, sourcepos, stInstancePath, varrefInstance);
		}

		// Token: 0x06001391 RID: 5009 RVA: 0x000378F9 File Offset: 0x000368F9
		internal static IVarRef GetVarReference(_IExpression exp, int iSignId, Guid guidApplication)
		{
			return VarReferenceCreator.GetVarReference(exp, iSignId, guidApplication, null);
		}

		// Token: 0x06001392 RID: 5010 RVA: 0x00037904 File Offset: 0x00036904
		internal static IVarRef GetVarReference(_IExpression exp, int iSignId, Guid guidApplication, VarRef varrefInstancePath)
		{
			return VarReferenceCreator.GetVarReference(exp, iSignId, guidApplication, varrefInstancePath, null);
		}

		// Token: 0x06001393 RID: 5011 RVA: 0x00037910 File Offset: 0x00036910
		private static bool TypifyAndCheck(_IExpression exp, IScope scope, _ICompileContext comcon)
		{
			CompilerProxy.TypifyExprement(exp, scope, comcon, null, false, false, null);
			IMessage[] messages = CompilerProxy.TypifyAndCheckExprement(((exp != null) ? exp.Duplicate() : null) as _IExpression, scope, comcon);
			return !VarReferenceCreator.ContainsOnlineVarReferenceRelevantError(messages);
		}

		// Token: 0x06001394 RID: 5012 RVA: 0x00037950 File Offset: 0x00036950
		private static bool ContainsOnlineVarReferenceRelevantError(IEnumerable<IMessage> messages)
		{
			uint?[] array = new uint?[]
			{
				new uint?(178U),
				new uint?(37U),
				new uint?(38U),
				new uint?(192U),
				new uint?(552U),
				new uint?(576U)
			};
			foreach (IMessage message in messages)
			{
				if (Severity.Error == message.Severity || Severity.FatalError == message.Severity)
				{
					IEnumerable<uint?> source = array;
					IMessage4 message2 = message as IMessage4;
					if (!source.Contains((message2 != null) ? message2.Number : null))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001395 RID: 5013 RVA: 0x00037A38 File Offset: 0x00036A38
		private static ILiteralValue GetLiteralValue(IVariable varHelp, IScope scope, IExpression exp)
		{
			if (!((_IExpression)exp).IsConstant(scope, false))
			{
				return null;
			}
			_ICompoAccessExpression icompoAccessExpression = exp as _ICompoAccessExpression;
			bool flag;
			if (icompoAccessExpression == null)
			{
				flag = (null != null);
			}
			else
			{
				_IExpression left = icompoAccessExpression._Left;
				flag = (((left != null) ? left.Type : null) != null);
			}
			bool flag2 = flag && ((_ICompoAccessExpression)exp)._Left.Type.IsInteger;
			bool bAllocatedOK = false;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351700)
			{
				bAllocatedOK = true;
			}
			ILiteralValue literalValue = ((_IExpression)exp).Literal(scope, bAllocatedOK);
			if (literalValue == null && varHelp == null)
			{
				return null;
			}
			if (literalValue == null && !varHelp.GetFlag(VarFlag.ReplacedConstant))
			{
				return null;
			}
			if (literalValue == null && !flag2)
			{
				switch (varHelp.CompiledType.DeRefType.Class)
				{
				case TypeClass.Bool:
				case TypeClass.Bit:
				case TypeClass.BitConst:
					return new LiteralValue(false);
				case TypeClass.Byte:
				case TypeClass.Word:
				case TypeClass.DWord:
				case TypeClass.LWord:
				case TypeClass.USInt:
				case TypeClass.UInt:
				case TypeClass.UDInt:
				case TypeClass.ULInt:
				case TypeClass.Time:
				case TypeClass.Date:
				case TypeClass.DateAndTime:
				case TypeClass.TimeOfDay:
					return new LiteralValue(0UL);
				case TypeClass.SInt:
				case TypeClass.Int:
				case TypeClass.DInt:
				case TypeClass.LInt:
					return new LiteralValue(0L);
				case TypeClass.Real:
				case TypeClass.LReal:
					return new LiteralValue(0.0);
				case TypeClass.String:
				case TypeClass.WString:
					return new LiteralValue("");
				}
				literalValue = LiteralValue.Empty;
			}
			return literalValue;
		}

		// Token: 0x06001396 RID: 5014 RVA: 0x00037BEC File Offset: 0x00036BEC
		internal static IVarRef HandleComplexVarReference(_IExpression exp, int iSignId, Guid guidApplication, IScope5 scope, string stInstancePath)
		{
			_ICompileContext referenceContext = VarReferenceCreator.LMM.GetReferenceContext(guidApplication);
			VarRef varRef = new VarRef(exp, guidApplication, CompilerProxy.CreateScope(referenceContext, iSignId));
			varRef.AddressInfo = new ComplexAddressInfo();
			IOnlineExpressionInterpreter onlineExpressionInterpreter = APEnvironmentFacade.Instance.CreateOnlineExpressionInterpreter();
			OnlineExpressionException ex = null;
			string text = string.Empty;
			if (scope.LocalSignature != null)
			{
				text = scope.LocalSignature.Name;
			}
			if (scope.MethodSignature != null)
			{
				text = text + "." + scope.MethodSignature.Name;
			}
			if (stInstancePath != null)
			{
				text = stInstancePath;
			}
			IOnlineExpression onlineExpression = onlineExpressionInterpreter.Parse(guidApplication, text, exp.ToString(), TypeClass.Any, out ex);
			if (onlineExpression == null || ex != null)
			{
				return null;
			}
			(varRef.AddressInfo as ComplexAddressInfo).SetOnlineExpression(onlineExpression);
			return varRef;
		}

		// Token: 0x06001397 RID: 5015 RVA: 0x00037C9C File Offset: 0x00036C9C
		internal static IVarRef HandleTaskLocalVariables(IVarRef varref, _IExpression expInstance, _IExpression exp, IScope5 scope, Guid guidApplication)
		{
			if (exp != null && !TypeTable.IsBlock(exp._CompiledType.Class) && CheckForTaskLocalVariable.ContainsTaskLocalAccess(exp, scope))
			{
				VarReferenceCreator.LMM.GetReferenceContext(guidApplication);
				VarRef varRef = new VarRef(exp, guidApplication, scope);
				VarRef varRef2 = varref as VarRef;
				varRef.ConstantValue = varref.ConstantValue;
				varRef.InstancePathExpression = varRef2.InstancePathExpression;
				varRef.Position = varRef2.Position;
				varRef.SignatureId = varRef2.SignatureId;
				ComplexAddressInfo complexAddressInfo = new ComplexAddressInfo();
				TaskLocalVariableOnlineExpression onlineExpression = new TaskLocalVariableOnlineExpression(varref, guidApplication);
				complexAddressInfo.SetOnlineExpression(onlineExpression);
				varRef.AddressInfo = complexAddressInfo;
				return varRef;
			}
			return varref;
		}

		// Token: 0x06001398 RID: 5016 RVA: 0x00037D36 File Offset: 0x00036D36
		public static TypeClass GetCorrespondingUnsignedType(TypeClass signedType)
		{
			switch (signedType)
			{
			case TypeClass.SInt:
				return TypeClass.USInt;
			case TypeClass.Int:
				return TypeClass.UInt;
			case TypeClass.DInt:
				return TypeClass.UDInt;
			case TypeClass.LInt:
				return TypeClass.ULInt;
			default:
				return signedType;
			}
		}

		// Token: 0x06001399 RID: 5017 RVA: 0x00037D60 File Offset: 0x00036D60
		private static bool IsComplexExpression(_IExpression expression)
		{
			IOperatorExpression operatorExpression = expression as IOperatorExpression;
			if (operatorExpression != null && Operator.Adr != operatorExpression.Code)
			{
				return true;
			}
			if (expression is IConversionExpression)
			{
				IConversionExpression conversionExpression = (IConversionExpression)expression;
				return !TypeTable.IsEquivalent(VarReferenceCreator.GetCorrespondingUnsignedType(conversionExpression.From), VarReferenceCreator.GetCorrespondingUnsignedType(conversionExpression.To));
			}
			return false;
		}

		// Token: 0x0600139A RID: 5018 RVA: 0x00037DB4 File Offset: 0x00036DB4
		internal static IVarRef GetVarReference(_IExpression exp, int iSignId, Guid guidApplication, VarRef varrefInstancePath, string stInstancePath)
		{
			_ICompileContext referenceContext = VarReferenceCreator.LMM.GetReferenceContext(guidApplication);
			if (referenceContext == null)
			{
				throw new ArgumentException(Strings.ErrNoApplication);
			}
			_ISignature isignature = CompilerProxy.CreateGlobalScope(referenceContext)[iSignId] as _ISignature;
			if (isignature != null && isignature.POUType == Operator.FunctionBlock)
			{
				ISignature subSignature = isignature.GetSubSignature(IdentifierConstants.MainSignatureName);
				if (subSignature != null)
				{
					iSignId = subSignature.Id;
				}
			}
			IScope5 scope = CompilerProxy.CreateScope(referenceContext, iSignId);
			if (!VarReferenceCreator.TypifyAndCheck(exp, scope, referenceContext))
			{
				return null;
			}
			VarRef varRef = new VarRef(exp, guidApplication, scope);
			ILiteralValue literalValue = VarReferenceCreator.GetLiteralValue((exp != null) ? exp.GetVariable(scope) : null, scope, exp);
			if (literalValue != null)
			{
				varRef.SetConstantValue(literalValue);
				if (exp != null && !(exp is _ILiteralExpression))
				{
					varRef.AddressInfo = new LiteralAddressInfo(literalValue, exp.Type);
				}
			}
			else if (VarReferenceCreator.IsComplexExpression(exp))
			{
				return VarReferenceCreator.HandleComplexVarReference(exp, iSignId, guidApplication, scope, stInstancePath);
			}
			try
			{
				if (varRef.GetFlag(VarRefFlag.Invalid))
				{
					varRef.AddressInfo = null;
				}
				else if (varRef.AddressInfo == null)
				{
					varRef.AddressInfo = VarReferenceCreator.GetAddressInfo(varRef, scope, varrefInstancePath);
				}
			}
			catch
			{
				varRef.AddressInfo = null;
				varRef.SetFlag(VarRefFlag.Invalid, true);
			}
			return VarReferenceCreator.HandleTaskLocalVariables(varRef, null, exp, scope, guidApplication);
		}

		// Token: 0x0600139B RID: 5019 RVA: 0x00037EE0 File Offset: 0x00036EE0
		internal static IVarRef GetVarReference(Guid guidApplication, string stInstancePath, string stExpression, int nProjectHandle, Guid guidObject)
		{
			bool flag = false;
			string stText = stExpression;
			if (!string.IsNullOrEmpty(stInstancePath) && stExpression.StartsWith(stInstancePath) && stExpression.Length > stInstancePath.Length)
			{
				flag = true;
				stText = stExpression.Substring(stInstancePath.Length + 1);
			}
			string text = stInstancePath;
			if (string.IsNullOrEmpty(text))
			{
				text = stExpression;
			}
			int invalidID;
			_IExpression iexpression;
			_IExpression iexpression2;
			if (guidApplication != Guid.Empty)
			{
				VarReferenceCreator.ParseInstancePathForExplicitApplication(text, guidApplication, out invalidID, out iexpression, out iexpression2, !string.IsNullOrEmpty(stInstancePath));
			}
			else
			{
				VarReferenceCreator.ParseInstancePath(text, out invalidID, out guidApplication, out iexpression, out iexpression2, !string.IsNullOrEmpty(stInstancePath));
			}
			if (string.IsNullOrEmpty(stInstancePath))
			{
				iexpression = null;
			}
			_ICompileContext referenceContext = VarReferenceCreator.LMM.GetReferenceContext(guidApplication);
			IScope5 scope = CompilerProxy.CreateScope(referenceContext, invalidID);
			IVarRef varRef = null;
			if (iexpression != null)
			{
				varRef = VarReferenceCreator.GetVarReference(iexpression, invalidID, guidApplication, null);
			}
			_IExpression iexpression3;
			if (string.IsNullOrEmpty(stInstancePath))
			{
				iexpression3 = iexpression2;
			}
			else
			{
				IScanner scanner = VarReferenceCreator.LMM.CreateScanner(stText, false, false, false, false);
				scanner.AllowMultipleUnderlines = true;
				iexpression3 = (CompilerProxy.CreateParser(scanner).ParseExpression() as _IExpression);
				if (iexpression3 == null)
				{
					throw new ArgumentException(Strings.ErrInvalidExpression);
				}
				if (!flag && iexpression3.SignatureId == -1 && iexpression3.VariableId == -1 && iexpression != null)
				{
					ISignature signature = iexpression.GetSignature(scope);
					if (signature != null)
					{
						IScope5 scope2 = CompilerProxy.CreateScope(referenceContext, signature.Id);
						if (!VarReferenceCreator.TypifyAndCheck(iexpression3, scope2, referenceContext))
						{
							return null;
						}
						if (VarReferenceCreator.IsActionCall(iexpression3, scope2))
						{
							return null;
						}
					}
				}
			}
			if (iexpression == null)
			{
				if (string.IsNullOrEmpty(stInstancePath))
				{
					invalidID = Common.InvalidID;
				}
				return VarReferenceCreator.GetVarReference(iexpression3, invalidID, guidApplication, varRef as VarRef);
			}
			string stText2 = scope.LocalSignature.OrgName + "." + iexpression.ToString();
			IScanner scanner2 = VarReferenceCreator.LMM.CreateScanner(stText2, false, false, false, false);
			scanner2.AllowMultipleUnderlines = true;
			_IExpression iexpression4 = CompilerProxy.CreateParser(scanner2).ParseExpression() as _IExpression;
			if (iexpression4 == null)
			{
				throw new ArgumentException(Strings.ErrInvalidExpression);
			}
			CompilerProxy.TypifyExprement(iexpression4, scope, referenceContext, null, false, false, null);
			while (iexpression4 is ICompoAccessExpression)
			{
				IVariable variable = iexpression4.GetVariable(scope);
				if (variable != null && variable.Type != null && variable.Type is UserdefType && (variable.Type as UserdefType).GetSignature(scope) != null && (variable.Type as UserdefType).GetSignature(scope).POUType == Operator.FunctionBlock)
				{
					break;
				}
				if (iexpression4 is ICompoAccessExpression)
				{
					iexpression4 = (iexpression4 as _ICompoAccessExpression)._Left;
				}
			}
			CompilerProxy.TypifyExprement(iexpression, scope, referenceContext, null, false, false, true, null);
			ISignature signature2 = iexpression.GetSignatureEx(scope);
			if (varRef != null && varRef.WatchExpression != null && varRef.WatchExpression.Type != null && varRef.WatchExpression.Type is UserdefType)
			{
				signature2 = (varRef.WatchExpression.Type as UserdefType).GetSignature(scope);
			}
			if (signature2 != null && signature2.ObjectGuid != guidObject && signature2.ParentSignatureId != -1)
			{
				signature2 = InstancePathHelper.FindSignatureForInstancePath(referenceContext, nProjectHandle, guidObject, stInstancePath);
			}
			int iSignId = -1;
			if (stInstancePath == null)
			{
				stInstancePath = string.Empty;
			}
			if (signature2 != null)
			{
				iSignId = signature2.Id;
			}
			IVarRef varReference = VarReferenceCreator.GetVarReference(iexpression3, iSignId, guidApplication, varRef as VarRef, stInstancePath);
			if (varReference != null)
			{
				(varReference as VarRef).InstancePathExpression = iexpression4;
			}
			return varReference;
		}

		// Token: 0x0600139C RID: 5020 RVA: 0x00038218 File Offset: 0x00037218
		private static bool IsActionCall(_IExpression expr, IScope5 scope)
		{
			ISignature signature = (expr != null) ? expr.GetSignature(scope) : null;
			return signature != null && signature.POUType == Operator.Method && signature.GetFlag(SignatureFlag.Action);
		}

		// Token: 0x0600139D RID: 5021 RVA: 0x00038251 File Offset: 0x00037251
		internal static _IExpression GetExpression(string stExpression, _ICompileContext comcon)
		{
			return VarReferenceCreator.GetExpression(stExpression, comcon, CompilerProxy.CreateGlobalScope(comcon));
		}

		// Token: 0x0600139E RID: 5022 RVA: 0x00038260 File Offset: 0x00037260
		internal static _IExpression GetExpression(string stExpression, _ICompileContext comcon, IScope5 scope)
		{
			_IExpression expression = VarReferenceCreator.GetExpression(stExpression);
			CompilerProxy.TypifyExprement(expression, scope, comcon, null, false, false, null);
			return expression;
		}

		// Token: 0x0600139F RID: 5023 RVA: 0x00038274 File Offset: 0x00037274
		internal static _IExpression GetExpression(string stExpression)
		{
			if (stExpression == null)
			{
				throw new ArgumentNullException("stExpression");
			}
			bool flag = false;
			_IExpression result = CompilerProxy.CreateParser(stExpression, true).ParseSTOperand(out flag);
			if (flag)
			{
				throw new ArgumentException("Error in expression.");
			}
			return result;
		}

		// Token: 0x060013A0 RID: 5024 RVA: 0x000382B0 File Offset: 0x000372B0
		internal static IAddressInfo GetAddressInfo(Guid guidApplication, IExpression exp, IScope scope)
		{
			_ICompileContext referenceContext = VarReferenceCreator.LMM.GetReferenceContext(guidApplication);
			if (referenceContext == null)
			{
				throw new ArgumentException("Application or Resource not valid");
			}
			AddressInfoCollector addressInfoCollector = new AddressInfoCollector();
			VarReferenceVisitor ivisit = new VarReferenceVisitor(scope as IScope5, referenceContext, 0, -1, addressInfoCollector);
			_IExprement iexprement = (exp as _IExpression).Duplicate();
			CompilerProxy.TypifyExprement(iexprement, scope, referenceContext, null, false, false, true, null);
			iexprement.Accept(ivisit);
			IAddressInfo[] addressInfo = addressInfoCollector.AddressInfo;
			if (addressInfoCollector.AddressInfo.Length != 0 && addressInfoCollector.AddressInfo[0] is IFunctionAddressInfo)
			{
				return addressInfoCollector.AddressInfo[0];
			}
			if (addressInfo.Length != 0)
			{
				return addressInfo[addressInfo.Length - 1];
			}
			return null;
		}

		// Token: 0x060013A1 RID: 5025 RVA: 0x00038344 File Offset: 0x00037344
		internal static IAddressInfo GetAddressInfo(IVarRef varrefIn, IScope5 scope, VarRef varrefInstance)
		{
			VarRef varRef = varrefIn as VarRef;
			if (varRef == null)
			{
				throw new ArgumentNullException("varrefIn");
			}
			_ICompileContext referenceContext = VarReferenceCreator.LMM.GetReferenceContext(varRef.ApplicationGuid);
			if (referenceContext == null)
			{
				throw new ArgumentException("Application or Resource not valid");
			}
			if (varRef.GetFlag(VarRefFlag.Invalid))
			{
				throw new ArgumentException("Varreference not valid");
			}
			AddressInfoCollector addressInfoCollector = new AddressInfoCollector();
			VarReferenceVisitor ivisit;
			if (varrefInstance != null)
			{
				ivisit = new VarReferenceVisitor(scope, referenceContext, varrefInstance, addressInfoCollector);
			}
			else
			{
				ivisit = new VarReferenceVisitor(scope, referenceContext, 0, -1, addressInfoCollector);
			}
			_IExprement iexprement = varRef._WatchExpression.Duplicate();
			CompilerProxy.TypifyExprement(iexprement, scope, referenceContext, null, false, false, true, null);
			iexprement.Accept(ivisit);
			IAddressInfo[] addressInfo = addressInfoCollector.AddressInfo;
			if (addressInfoCollector.AddressInfo.Length != 0 && addressInfoCollector.AddressInfo[0] is IFunctionAddressInfo)
			{
				return addressInfoCollector.AddressInfo[0];
			}
			if (addressInfo.Length != 0)
			{
				return addressInfo[addressInfo.Length - 1];
			}
			return null;
		}

		// Token: 0x060013A2 RID: 5026 RVA: 0x00038414 File Offset: 0x00037414
		internal static IEnumerable<IFlowVarRef> GetAllFlowVarReferences(Guid objectguid, Guid guidExplicitApplicationGuid, string stInstance, long[] alPositionsOfInterest)
		{
			VarRef varRef = null;
			try
			{
				if (guidExplicitApplicationGuid != Guid.Empty)
				{
					string stExpression;
					VarReferenceCreator.ExtractResourceFromExpression(stInstance, out stExpression);
					varRef = (VarReferenceCreator.GetVarReference(guidExplicitApplicationGuid, stExpression) as VarRef);
				}
				else
				{
					varRef = (VarReferenceCreator.GetVarReference(stInstance) as VarRef);
				}
			}
			catch
			{
				return null;
			}
			_ICompileContext icompileContext = null;
			ISignature signature = null;
			_ISignature isignature = null;
			if (guidExplicitApplicationGuid != Guid.Empty)
			{
				icompileContext = VarReferenceCreator.LMM.GetReferenceContext(guidExplicitApplicationGuid);
			}
			else
			{
				icompileContext = VarReferenceCreator.LMM.GetReferenceContext(varRef.ApplicationGuid);
			}
			if (objectguid != Guid.Empty)
			{
				ISignature signature2 = icompileContext.GetSignature(objectguid);
				isignature = (signature2 as _ISignature);
				if (signature2 != null && (signature2.POUType == Operator.Method || signature2.POUType == Operator.Action))
				{
					signature = signature2;
					int length = stInstance.LastIndexOf('.');
					stInstance = stInstance.Substring(0, length);
					try
					{
						varRef = (VarReferenceCreator.GetVarReference(stInstance) as VarRef);
					}
					catch
					{
					}
					isignature = (icompileContext.GetSignatureById(signature.ParentSignatureId) as _ISignature);
					if (varRef.AddressInfo is AbsoluteAddressInfo && (varRef.AddressInfo as AbsoluteAddressInfo).Area == -1)
					{
						varRef.AddressInfo = null;
					}
				}
			}
			int id;
			if (signature != null)
			{
				id = signature.Id;
			}
			else
			{
				if (isignature == null)
				{
					throw new Exception("Signature not found");
				}
				id = isignature.Id;
			}
			return icompileContext.GetAllFlowPositions(id, alPositionsOfInterest, stInstance, varRef);
		}

		// Token: 0x060013A3 RID: 5027 RVA: 0x00038584 File Offset: 0x00037584
		internal static IEnumerable<IFlowVarRef> GetAllFlowVarReferences(string stInstance, long[] alPositionsOfInterest)
		{
			VarRef varRef = null;
			try
			{
				varRef = (VarReferenceCreator.GetVarReference(stInstance) as VarRef);
			}
			catch
			{
				return null;
			}
			_ICompileContext referenceContext = VarReferenceCreator.LMM.GetReferenceContext(varRef.ApplicationGuid);
			ISignature signature = null;
			if (varRef._WatchExpression.Type.Class == TypeClass.Userdef)
			{
				UserdefType userdefType = varRef._WatchExpression.Type as UserdefType;
				ISignature signatureById = referenceContext.GetSignatureById(userdefType.SignatureId);
				if (signatureById != null && (signatureById.POUType == Operator.Method || signatureById.POUType == Operator.Action))
				{
					signature = signatureById;
					int length = stInstance.LastIndexOf('.');
					stInstance = stInstance.Substring(0, length);
					try
					{
						varRef = (VarReferenceCreator.GetVarReference(stInstance) as VarRef);
					}
					catch
					{
						return null;
					}
				}
			}
			if (referenceContext == null)
			{
				throw new ArgumentException("instance not found", "stInstance");
			}
			CompiledPOU compiledPOU = null;
			_ISignature isignature = null;
			if (varRef._WatchExpression != null && varRef._WatchExpression.Type is UserdefType)
			{
				UserdefType userdefType2 = varRef._WatchExpression.Type as UserdefType;
				isignature = referenceContext[userdefType2.SignatureId];
				if (signature == null)
				{
					IPreCompileContext libraryContext = VarReferenceCreator.LMM.LibList.GetLibraryContext(isignature.LibraryPath);
					if (libraryContext == null)
					{
						compiledPOU = (VarReferenceCreator.LMM.FindPrecompiledPOU(isignature.ObjectGuid) as CompiledPOU);
					}
					else
					{
						compiledPOU = (libraryContext.GetCompiledPOU(isignature.ObjectGuid) as CompiledPOU);
					}
				}
				else
				{
					IPreCompileContext libraryContext2 = VarReferenceCreator.LMM.LibList.GetLibraryContext(signature.LibraryPath);
					if (libraryContext2 == null)
					{
						compiledPOU = (VarReferenceCreator.LMM.FindPrecompiledPOU(signature.ObjectGuid) as CompiledPOU);
					}
					else
					{
						compiledPOU = (libraryContext2.GetCompiledPOU(signature.ObjectGuid) as CompiledPOU);
					}
				}
			}
			if (compiledPOU == null)
			{
				throw new Exception(string.Format("no compile info for Instance '{0}' found", stInstance));
			}
			int nSignatureId = (signature != null) ? signature.Id : isignature.Id;
			return referenceContext.GetAllFlowPositions(nSignatureId, alPositionsOfInterest, stInstance, varRef);
		}

		// Token: 0x060013A4 RID: 5028 RVA: 0x0003877C File Offset: 0x0003777C
		internal static IEnumerable<IVarRef> GetAllVarReferences(string stInstance, long[] alPositionsOfInterest)
		{
			return VarReferenceCreator.GetAllVarReferences(Guid.Empty, Guid.Empty, stInstance, alPositionsOfInterest);
		}

		// Token: 0x060013A5 RID: 5029 RVA: 0x00038790 File Offset: 0x00037790
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-89482")]
		internal static IEnumerable<IVarRef> GetAllVarReferences(Guid objectguid, Guid guidExplicitApplicationGuid, string stInstance, long[] alPositionsOfInterest)
		{
			VarRef varRef = null;
			string text = stInstance;
			try
			{
				if (guidExplicitApplicationGuid != Guid.Empty)
				{
					string stExpression;
					VarReferenceCreator.ExtractResourceFromExpression(stInstance, out stExpression);
					varRef = (VarReferenceCreator.GetVarReference(guidExplicitApplicationGuid, stExpression) as VarRef);
				}
				else
				{
					varRef = (VarReferenceCreator.GetVarReference(stInstance) as VarRef);
				}
			}
			catch
			{
				return null;
			}
			_ICompileContext icompileContext = null;
			ISignature signature = null;
			_ISignature isignature = null;
			CompiledPOU compiledPOU = null;
			_ISignature isignature2 = null;
			if (guidExplicitApplicationGuid != Guid.Empty)
			{
				icompileContext = VarReferenceCreator.LMM.GetReferenceContext(guidExplicitApplicationGuid);
			}
			else
			{
				icompileContext = VarReferenceCreator.LMM.GetReferenceContext(varRef.ApplicationGuid);
			}
			bool flag = false;
			bool flag2 = varRef._WatchExpression.Type != null && varRef._WatchExpression.Type.Class == TypeClass.Userdef;
			if (flag2)
			{
				UserdefType userdefType = varRef._WatchExpression.Type as UserdefType;
				isignature = (icompileContext.GetSignatureById(userdefType.SignatureId) as _ISignature);
				if (isignature.ObjectGuid != objectguid)
				{
					flag = true;
				}
			}
			if (objectguid != Guid.Empty && flag)
			{
				_ISignature isignature3 = icompileContext.GetSignature(objectguid) as _ISignature;
				_ISignature isignature4 = isignature;
				_ISignature isignature5 = null;
				if (isignature3 != null)
				{
					if (isignature3.POUType == Operator.Method || isignature3.POUType == Operator.Action)
					{
						isignature5 = isignature3;
						int length = stInstance.LastIndexOf('.');
						stInstance = stInstance.Substring(0, length);
						try
						{
							varRef = (VarReferenceCreator.GetVarReference(stInstance) as VarRef);
						}
						catch
						{
						}
						isignature4 = (icompileContext.GetSignatureById(isignature5.ParentSignatureId) as _ISignature);
						if (varRef.AddressInfo is AbsoluteAddressInfo && (varRef.AddressInfo as AbsoluteAddressInfo).Area == -1)
						{
							varRef.AddressInfo = null;
						}
					}
					Guid objectGuid = isignature3.ObjectGuid;
					IPreCompileContext libraryContext = VarReferenceCreator.LMM.LibList.GetLibraryContext(APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352100 ? isignature3.LibraryPath : isignature4.LibraryPath);
					if (libraryContext == null)
					{
						compiledPOU = (VarReferenceCreator.LMM.FindPrecompiledPOU(objectGuid) as CompiledPOU);
					}
					else
					{
						compiledPOU = (libraryContext.GetCompiledPOU(objectGuid) as CompiledPOU);
					}
				}
				if (!isignature4.GetFlagInternal(SignatureFlagInternal.ContainsGenericConstants))
				{
					isignature2 = isignature4;
					isignature = isignature3;
					signature = isignature5;
				}
				else
				{
					signature = isignature;
					isignature2 = (icompileContext.GetSignatureById(signature.ParentSignatureId) as _ISignature);
				}
			}
			else
			{
				if (flag2 && !VarReferenceCreator.GetSignatureByUserdefType(ref stInstance, icompileContext, ref varRef, out isignature, ref signature))
				{
					return null;
				}
				if (icompileContext == null)
				{
					throw new ArgumentException("instance not found", "stInstance");
				}
				if (varRef._WatchExpression != null && varRef._WatchExpression.Type is UserdefType)
				{
					UserdefType userdefType2 = varRef._WatchExpression.Type as UserdefType;
					isignature2 = icompileContext[userdefType2.SignatureId];
					if (signature == null)
					{
						IPreCompileContext libraryContext2 = VarReferenceCreator.LMM.LibList.GetLibraryContext(isignature2.LibraryPath);
						if (libraryContext2 == null)
						{
							compiledPOU = (VarReferenceCreator.LMM.FindPrecompiledPOU(isignature2.ObjectGuid) as CompiledPOU);
						}
						else
						{
							compiledPOU = (libraryContext2.GetCompiledPOU(isignature2.ObjectGuid) as CompiledPOU);
						}
					}
					else
					{
						IPreCompileContext libraryContext3 = VarReferenceCreator.LMM.LibList.GetLibraryContext(signature.LibraryPath);
						if (libraryContext3 == null)
						{
							compiledPOU = (VarReferenceCreator.LMM.FindPrecompiledPOU(signature.ObjectGuid) as CompiledPOU);
						}
						else
						{
							compiledPOU = (libraryContext3.GetCompiledPOU(signature.ObjectGuid) as CompiledPOU);
						}
					}
				}
			}
			if (compiledPOU == null)
			{
				throw new Exception(string.Format("no compile info for Instance '{0}' found", stInstance));
			}
			if (objectguid != Guid.Empty)
			{
				ISignature signature2 = InstancePathHelper.FindSignatureForInstancePath(icompileContext, APEnvironmentFacade.Instance.PrimaryProjectHandle, objectguid, text);
				if (signature2 != null)
				{
					isignature = (_ISignature)signature2;
					isignature2 = (_ISignature)icompileContext.GetSignatureById(isignature.ParentSignatureId);
					if (isignature2 != null)
					{
						signature = isignature;
					}
					else
					{
						isignature2 = isignature;
					}
				}
			}
			if (isignature != null && isignature.POUType == Operator.FunctionBlock)
			{
				signature = isignature.GetSubSignature(IdentifierConstants.MainSignatureName);
				isignature2 = isignature;
			}
			IScope5 scope;
			if (signature != null && isignature2 != null)
			{
				scope = CompilerProxy.CreateScope(icompileContext, isignature2.Id, signature.Id);
			}
			else
			{
				if (isignature2 == null)
				{
					return Array.Empty<IVarRef>();
				}
				scope = CompilerProxy.CreateScope(icompileContext, isignature2.Id);
			}
			if (varRef.AddressInfo == null || (varRef.AddressInfo is IAbsoluteAddressInfo && (varRef.AddressInfo as IAbsoluteAddressInfo).Area == -1))
			{
				PointerToFBVarRefCreator.TestGetPointerToFBVarRef(text, ref varRef);
			}
			_IExpression watchExpression = varRef._WatchExpression;
			VarReferenceCollector varReferenceCollector = new VarReferenceCollector(varRef.AddressInfo, varRef.ApplicationGuid, alPositionsOfInterest, watchExpression);
			VarReferenceVisitor ivisit = new VarReferenceVisitor(scope, icompileContext, varRef, varReferenceCollector);
			try
			{
				_IStatement istatement = (compiledPOU.ParseTree as _IStatement).Duplicate() as _IStatement;
				bool bInterpretPragmas = APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 17, 0);
				CompilerProxy.TypifyExprement(istatement, scope, icompileContext, null, bInterpretPragmas, false, compiledPOU);
				istatement.Accept(ivisit);
			}
			catch
			{
				return Array.Empty<IVarRef>();
			}
			varReferenceCollector.HandleTaskLocalVarReferences();
			return varReferenceCollector.VarReferences;
		}

		// Token: 0x060013A6 RID: 5030 RVA: 0x00038C68 File Offset: 0x00037C68
		private static bool GetSignatureByUserdefType(ref string stInstance, _ICompileContext comcon, ref VarRef varrefHelp, out _ISignature sign, ref ISignature signMethod)
		{
			UserdefType userdefType = varrefHelp._WatchExpression.Type as UserdefType;
			sign = (comcon.GetSignatureById(userdefType.SignatureId) as _ISignature);
			if (sign != null && (sign.POUType == Operator.Method || sign.POUType == Operator.Action))
			{
				signMethod = sign;
				int length = stInstance.LastIndexOf('.');
				stInstance = stInstance.Substring(0, length);
				try
				{
					varrefHelp = (VarReferenceCreator.GetVarReference(stInstance) as VarRef);
				}
				catch
				{
					return false;
				}
				return true;
			}
			return true;
		}

		// Token: 0x060013A7 RID: 5031 RVA: 0x00038CF8 File Offset: 0x00037CF8
		private static string ExtractResourceFromExpression(string stExpression, out string stInstance)
		{
			stInstance = stExpression;
			IScanner scanner = VarReferenceCreator.LMM.CreateScanner(stExpression, false, false, false, false);
			scanner.AllowMultipleUnderlines = true;
			IToken token;
			if (scanner.GetNext(out token) != TokenType.Identifier)
			{
				throw new ArgumentException(Strings.ErrExpressionContainsNoResource);
			}
			string text = scanner.GetIdentifier(token);
			IToken token2;
			if (scanner.GetNext(out token2) == TokenType.Operator && scanner.GetOperator(token2) == Operator.Period && scanner.GetNext(out token) == TokenType.Identifier)
			{
				string identifier = scanner.GetIdentifier(token);
				text = text + "." + identifier;
				if (VarReferenceCreator.LMM.GetApplicationGuidByName(text) != Guid.Empty)
				{
					stInstance = stExpression.Substring(text.Length + 1);
				}
				else
				{
					text = string.Empty;
				}
			}
			return text;
		}
	}
}
