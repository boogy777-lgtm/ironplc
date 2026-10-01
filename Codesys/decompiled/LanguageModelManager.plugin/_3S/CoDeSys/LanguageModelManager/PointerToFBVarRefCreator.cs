using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000FC RID: 252
	internal static class PointerToFBVarRefCreator
	{
		// Token: 0x06001258 RID: 4696 RVA: 0x00034494 File Offset: 0x00033494
		internal static void TestGetPointerToFBVarRef(string stInstance, ref VarRef varrefInOut)
		{
			_ICompileContext compileContextByInstancePath = PointerToFBVarRefCreator.GetCompileContextByInstancePath(stInstance);
			if (compileContextByInstancePath == null)
			{
				return;
			}
			ISignature signatureOfWatchExpression = PointerToFBVarRefCreator.GetSignatureOfWatchExpression(compileContextByInstancePath, varrefInOut._WatchExpression);
			if (signatureOfWatchExpression == null || signatureOfWatchExpression.POUType != Operator.FunctionBlock)
			{
				return;
			}
			varrefInOut = (APEnvironmentFacade.Instance.LMServiceProvider.MonitoringService.GetVarReference(stInstance + "." + IdentifierConstants.InstancePointer + "^") as VarRef);
		}

		// Token: 0x06001259 RID: 4697 RVA: 0x000344F8 File Offset: 0x000334F8
		private static ISignature GetSignatureOfWatchExpression(_ICompileContext comcon, _IExpression expression)
		{
			return PointerToFBVarRefCreator.GetSignatureOfWatchExpression(CompilerProxy.CreateGlobalScope(comcon), expression);
		}

		// Token: 0x0600125A RID: 4698 RVA: 0x00034508 File Offset: 0x00033508
		private static ISignature GetSignatureOfWatchExpression(IScope5 scope, _IExpression expression)
		{
			_ICompoAccessExpression icompoAccessExpression = expression as _ICompoAccessExpression;
			if (icompoAccessExpression != null)
			{
				return PointerToFBVarRefCreator.GetSignatureOfCompoExpression(scope, icompoAccessExpression);
			}
			_INamespaceAccessExpression inamespaceAccessExpression = expression as _INamespaceAccessExpression;
			if (inamespaceAccessExpression != null)
			{
				return PointerToFBVarRefCreator.GetSignatureOfNamespaceAccessExpression(scope, inamespaceAccessExpression);
			}
			IVariableExpression variableExpression = expression as IVariableExpression;
			if (variableExpression == null)
			{
				return null;
			}
			return variableExpression.GetSignature(scope);
		}

		// Token: 0x0600125B RID: 4699 RVA: 0x00034550 File Offset: 0x00033550
		private static ISignature GetSignatureOfCompoExpression(IScope5 scope, _ICompoAccessExpression compo)
		{
			ISignature signatureOfWatchExpression = PointerToFBVarRefCreator.GetSignatureOfWatchExpression(scope, compo._Right);
			if (signatureOfWatchExpression != null && signatureOfWatchExpression.POUType == Operator.Method)
			{
				signatureOfWatchExpression = PointerToFBVarRefCreator.GetSignatureOfWatchExpression(scope, compo._Left);
			}
			return signatureOfWatchExpression;
		}

		// Token: 0x0600125C RID: 4700 RVA: 0x00034588 File Offset: 0x00033588
		private static ISignature GetSignatureOfNamespaceAccessExpression(IScope5 scope, _INamespaceAccessExpression name)
		{
			ISignature signatureOfWatchExpression = PointerToFBVarRefCreator.GetSignatureOfWatchExpression(scope, name._Access);
			if (signatureOfWatchExpression != null && signatureOfWatchExpression.POUType == Operator.Method)
			{
				signatureOfWatchExpression = PointerToFBVarRefCreator.GetSignatureOfWatchExpression(scope, name._Namespace);
			}
			return signatureOfWatchExpression;
		}

		// Token: 0x0600125D RID: 4701 RVA: 0x000345C0 File Offset: 0x000335C0
		private static _ICompileContext GetCompileContextByInstancePath(string stInstance)
		{
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(stInstance, false, false, false, false);
			scanner.AllowMultipleUnderlines = true;
			IToken token;
			if (scanner.GetNext(out token) != TokenType.Identifier)
			{
				return null;
			}
			string text = scanner.GetIdentifier(token);
			IToken token2;
			if (scanner.GetNext(out token2) == TokenType.Operator && scanner.GetOperator(token2) == Operator.Period && scanner.GetNext(out token) == TokenType.Identifier)
			{
				string identifier = scanner.GetIdentifier(token);
				text = text + "." + identifier;
			}
			else
			{
				scanner.SetPosition(token2);
			}
			Guid applicationGuidByName = APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationGuidByName(text);
			if (scanner.GetNext(out token) != TokenType.Operator || scanner.GetOperator(token) != Operator.Period)
			{
				return null;
			}
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContext(applicationGuidByName);
		}
	}
}
