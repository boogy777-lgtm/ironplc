using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Online;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes", Justification = "Class cannot be divided into subclasses because of released interfaces")]
	public class CompileUtilities : ICompileUtilities7, ICompileUtilities6, ICompileUtilities5, ICompileUtilities4, ICompileUtilities3, ICompileUtilities2, ICompileUtilities
	{
		public IAddressInfoCreator CreateAddressInfoCreator()
		{
			return new XAddressInfoCreator();
		}

		public IExpression ParseAndTypifyExpression(Guid guidApplication, string stExpression, int iSignatureId, out IEnumerable<IMessage> mess)
		{
			return ParseAndTypifyExpression2(guidApplication, stExpression, iSignatureId, out mess, bUsedForVarReference: false);
		}

		public IExpression ParseAndTypifyExpression2(Guid guidApplication, string stExpression, int iSignatureId, out IEnumerable<IMessage> mess, bool bUsedForVarReference)
		{
			mess = (IEnumerable<IMessage>)new LList<IMessage>();
			try
			{
				if (!(APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder() is ILanguageModelBuilder3 languageModelBuilder))
				{
					return null;
				}
				IExpression expression = languageModelBuilder.ParseExpression(stExpression);
				if (!(APEnvironmentFacade.Instance.LanguageModelMgr.CreateTypifier(guidApplication, iSignatureId, bContributeToCompile: false, bInterpretPragmas: false) is IExpressionTypifier6 expressionTypifier))
				{
					return null;
				}
				expressionTypifier.TypifyAndCheckExpression(expression, out mess);
				if (bUsedForVarReference)
				{
					if (ContainsOnlineVarReferenceRelevantError(mess))
					{
						return null;
					}
				}
				else
				{
					foreach (IMessage item in mess)
					{
						if (item.Severity == Severity.Error || item.Severity == Severity.FatalError)
						{
							return null;
						}
					}
				}
				return expression;
			}
			catch
			{
				return null;
			}
		}

		public IDataLocationInformation GetInformationForDataLocation(Guid guidApplication, IDataLocation codeLocation, IDataLocation instanceLocation, ICallStackEntry2 oldLocation, bool bException)
		{
			DataLocationInformation dataLocationInformation = new DataLocationInformation();
			if (oldLocation != null && oldLocation.LocationInformation != null && codeLocation.IsEqual(oldLocation.LocationPOU))
			{
				dataLocationInformation.CompiledPOU = oldLocation.LocationInformation.CompiledPOU;
				dataLocationInformation.Breakpoint = oldLocation.LocationInformation.Breakpoint;
			}
			IDataLocationInformation informationForDataLocation = GetInformationForDataLocation(dataLocationInformation, guidApplication, codeLocation, instanceLocation, bException);
			if (instanceLocation != null && oldLocation != null && oldLocation.LocationInstance != null && instanceLocation.IsEqual(oldLocation.LocationInstance) && oldLocation.LocationInformation != null && dataLocationInformation.CompiledPOU.SignatureId == oldLocation.LocationInformation.CompiledPOU.SignatureId && (!(oldLocation.LocationInformation is IDataLocationInformation2) || ((IDataLocationInformation2)oldLocation.LocationInformation).InstancePathAvailableImmediate))
			{
				dataLocationInformation.InstancePath = oldLocation.LocationInformation.InstancePath;
			}
			return informationForDataLocation;
		}

		public IDataLocationInformation GetInformationForDataLocation(Guid guidApplication, IDataLocation codeLocation, IDataLocation instanceLocation, bool bException)
		{
			DataLocationInformation info = new DataLocationInformation();
			return GetInformationForDataLocation(info, guidApplication, codeLocation, instanceLocation, bException);
		}

		private IDataLocationInformation GetInformationForDataLocation(DataLocationInformation info, Guid guidApplication, IDataLocation codeLocation, IDataLocation instanceLocation, bool bException)
		{
			if (info.CompiledPOU == null)
			{
				ICompiledPOU cpou = null;
				ICompileContext compileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(guidApplication);
				if (compileContext == null)
				{
					return null;
				}
				if (bException)
				{
					if (compileContext is ICompileContext2 compileContext2)
					{
						info.Breakpoint = compileContext2.FindBreakpointByCodePosition(codeLocation.Area, (uint)codeLocation.Offset, bBackward: true, out cpou);
					}
				}
				else
				{
					info.Breakpoint = compileContext.GetBreakpointByCodePosition(codeLocation.Area, (uint)codeLocation.Offset, out cpou);
					if (info.Breakpoint == null && compileContext is ICompileContext2 compileContext3)
					{
						info.Breakpoint = compileContext3.FindBreakpointByCodePosition(codeLocation.Area, (uint)codeLocation.Offset, bBackward: true, out cpou);
					}
				}
				if (cpou == null)
				{
					return null;
				}
				info.CompiledPOU = cpou;
			}
			info.InstanceLocation = instanceLocation;
			info.ApplicationGuid = guidApplication;
			return info;
		}

		public ISignature GetCalledSignatureForStackVariable(IVarRef2 varRef, out IExpression callingExpression)
		{
			ISignature signature = null;
			callingExpression = null;
			ICompileContext compileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(varRef.ApplicationGuid);
			if (compileContext != null)
			{
				IScope scope = compileContext.CreateIScope(varRef.SignatureId);
				IExpression5 expression = (IExpression5)(callingExpression = GetMethodOrFunctionExpression(varRef.WatchExpression, scope));
				if (varRef.WatchExpression is ICompoAccessExpression && (varRef.WatchExpression as ICompoAccessExpression).Right is IVariableExpression)
				{
					signature = scope[((varRef.WatchExpression as ICompoAccessExpression).Right as IVariableExpression).SignatureId];
					if (signature.POUType == Operator.Type)
					{
						signature = expression.GetSignatureEx(scope);
					}
				}
				else
				{
					signature = expression.GetSignatureEx(scope);
				}
			}
			return signature;
		}

		private static IExpression5 GetMethodOrFunctionExpression(IExpression exp, IScope scope)
		{
			IExpression5 expression = exp as IExpression5;
			ISignature signature = expression.GetSignature(scope);
			if (signature != null && (signature.POUType == Operator.Function || signature.POUType == Operator.Method))
			{
				return expression;
			}
			if (exp is ICompoAccessExpression)
			{
				expression = GetMethodOrFunctionExpression((exp as ICompoAccessExpression).Left, scope);
			}
			else if (exp is IDeRefAccessExpression)
			{
				expression = GetMethodOrFunctionExpression((exp as IDeRefAccessExpression).Base, scope);
			}
			else if (exp is IIndexAccessExpression)
			{
				expression = GetMethodOrFunctionExpression((exp as IIndexAccessExpression).Var, scope);
			}
			return expression;
		}

		public ISignature GetSignature(ICompileContext comcon, int iProjectHandle, Guid gdObject)
		{
			if (!APEnvironmentFacade.Instance.ExistProject(iProjectHandle))
			{
				return null;
			}
			string projectId = GetProjectId(iProjectHandle);
			ISignature signature = (comcon as ICompileContext21).GetSignature(gdObject);
			if (signature != null && IsLibraryPathMatch(signature.LibraryPath, projectId))
			{
				return signature;
			}
			ISignature[] allSignatures = comcon.AllSignatures;
			foreach (ISignature signature2 in allSignatures)
			{
				if (signature2.ObjectGuid == gdObject && IsLibraryPathMatch(signature2.LibraryPath, projectId))
				{
					return signature2;
				}
				ISignature[] subSignatures = signature2.SubSignatures;
				foreach (ISignature signature3 in subSignatures)
				{
					if (signature3.ObjectGuid == gdObject && IsLibraryPathMatch(signature3.LibraryPath, projectId))
					{
						return signature3;
					}
				}
			}
			return null;
		}

		private static string GetProjectId(int iProjectHandle)
		{
			if (APEnvironmentFacade.Instance.IsPrimaryProject(iProjectHandle))
			{
				return string.Empty;
			}
			IProject projectFromHandle = APEnvironmentFacade.Instance.GetProjectFromHandle(iProjectHandle);
			if (projectFromHandle == null)
			{
				return string.Empty;
			}
			string result = string.Empty;
			if (projectFromHandle.Library)
			{
				result = projectFromHandle.Id;
			}
			if (projectFromHandle.Id.StartsWith("Reloaded:"))
			{
				result = projectFromHandle.Id.Substring(9);
			}
			return result;
		}

		private static bool IsLibraryPathMatch(string libraryPath, string projectId)
		{
			bool num = string.IsNullOrEmpty(libraryPath);
			bool flag = string.IsNullOrEmpty(projectId);
			if (num)
			{
				return flag;
			}
			if (flag)
			{
				return false;
			}
			return string.Equals(libraryPath, projectId, StringComparison.OrdinalIgnoreCase);
		}

		public ISignature FindSignature(ICompileContext comcon, int nProjectHandle, Guid guidObject)
		{
			ISignature signature = GetSignature(comcon, nProjectHandle, guidObject);
			if (signature != null)
			{
				return signature;
			}
			Guid parentObject = APEnvironmentFacade.Instance.GetParentObject(nProjectHandle, guidObject);
			while (parentObject != Guid.Empty)
			{
				if (GetSignature(comcon, nProjectHandle, parentObject) != null)
				{
					ISignature[] subSignatures = comcon.GetSubSignatures(parentObject);
					if (subSignatures != null)
					{
						ISignature[] array = subSignatures;
						foreach (ISignature signature2 in array)
						{
							if (signature2.ObjectGuid == guidObject)
							{
								return signature2;
							}
						}
					}
				}
				parentObject = APEnvironmentFacade.Instance.GetParentObject(nProjectHandle, parentObject);
			}
			return null;
		}

		public bool GetAddressFromExpression(Guid guidApplication, string stExpression, out int iArea, out int iOffset)
		{
			iArea = -1;
			iOffset = 0;
			IEnumerable<IMessage> mess;
			IExpression expression = ParseAndTypifyExpression(guidApplication, stExpression, -1, out mess);
			if (expression == null || mess.Count() != 0)
			{
				return false;
			}
			AddressFromCompileContext addressFromCompileContext = new AddressFromCompileContext(guidApplication);
			expression.AcceptVisitor(addressFromCompileContext);
			return addressFromCompileContext.GetAddress(out iArea, out iOffset);
		}

		public void VisitAllVariables(IVariableVisitor visitor, IExprement expToVisit)
		{
			if (visitor == null)
			{
				throw new ArgumentNullException("visitor");
			}
			if (expToVisit == null)
			{
				throw new ArgumentNullException("expToVisit");
			}
			VariableTraverser visitor2 = new VariableTraverser(new VariableVisitor(visitor));
			expToVisit.AcceptVisitor(visitor2);
		}

		public bool ContainsOnlineVarReferenceRelevantError(IEnumerable<IMessage> messages)
		{
			return GetOnlineVarReferenceRelevantError(messages) != null;
		}

		public IMessage GetOnlineVarReferenceRelevantError(IEnumerable<IMessage> messages)
		{
			uint?[] source = new uint?[5] { 178u, 37u, 38u, 192u, 576u };
			foreach (IMessage4 message in messages)
			{
				if ((Severity.Error == message.Severity || Severity.FatalError == message.Severity) && !source.Contains(message.Number))
				{
					return message;
				}
			}
			return null;
		}
	}
}
