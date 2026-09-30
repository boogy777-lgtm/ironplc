using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{95738fa6-6255-41b2-a4e7-86210afeb1e9}")]
	public class MonitoringUtilities : IMonitoringUtilities
	{
		private readonly IScanner _scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner("", bIncludeComments: false, bIncludeEndOfLines: false, bIncludePragmas: false, bIncludeWhitespaces: false);

		private readonly IParser _parser;

		private const int MAX_LAST_FOUND_APP_CACHE = 32;

		private readonly Queue<Guid> _lastFoundApp = new Queue<Guid>(32);

		public MonitoringUtilities()
		{
			_parser = APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(_scanner);
		}

		public Guid GetApplicationGuid(string stDevice, string stApplication)
		{
			if (!APEnvironmentFacade.Instance.ExistsPrimaryProject)
			{
				return Guid.Empty;
			}
			int primaryProjectHandle = APEnvironmentFacade.Instance.PrimaryProjectHandle;
			foreach (Guid item in _lastFoundApp)
			{
				if (APEnvironmentFacade.Instance.ExistsObject(primaryProjectHandle, item) && APEnvironmentFacade.Instance.IsApplicationWithDevice(stDevice, stApplication, primaryProjectHandle, item))
				{
					return item;
				}
			}
			Guid[] allObjects = APEnvironmentFacade.Instance.GetAllObjects(primaryProjectHandle);
			foreach (Guid guid in allObjects)
			{
				if (APEnvironmentFacade.Instance.IsApplicationWithDevice(stDevice, stApplication, primaryProjectHandle, guid))
				{
					_lastFoundApp.Enqueue(guid);
					if (_lastFoundApp.Count > 32)
					{
						_lastFoundApp.Dequeue();
					}
					return guid;
				}
			}
			return Guid.Empty;
		}

		private bool IsDirectAddress(string stExpression, out IDirectVariable dirVar, out ICompileContext comcon)
		{
			comcon = null;
			dirVar = null;
			_scanner.Initialize(stExpression);
			IToken token = null;
			_scanner.AllowMultipleUnderlines = true;
			if (_scanner.GetNext(out token) != TokenType.Identifier)
			{
				return false;
			}
			string identifier = _scanner.GetIdentifier(token);
			string empty = string.Empty;
			if (_scanner.GetNext(out token) == TokenType.Operator && _scanner.GetOperator(token) == Operator.Period && _scanner.GetNext(out token) == TokenType.Identifier)
			{
				empty = _scanner.GetIdentifier(token);
				if (_scanner.GetNext(out token) != TokenType.Operator || _scanner.GetOperator(token) != Operator.Period)
				{
					return false;
				}
				bool result = false;
				IExpression expression = _parser.ParseOperand();
				if (expression != null && expression is IAddressExpression)
				{
					IAddressExpression addressExpression = expression as IAddressExpression;
					dirVar = addressExpression.DirectAddress;
					result = true;
				}
				Guid applicationGuid = GetApplicationGuid(identifier, empty);
				comcon = GetReferenceContext(applicationGuid);
				return result;
			}
			return false;
		}

		public ICompileContext GetReferenceContext(Guid guidApplication)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetReferenceContextIfAvailable(guidApplication);
		}

		public void CheckValidAddress(string stExpression)
		{
			bool flag = true;
			IDirectVariable dirVar = null;
			ICompileContext comcon = null;
			if (IsDirectAddress(stExpression, out dirVar, out comcon))
			{
				if (dirVar != null && comcon != null)
				{
					IAddressCalculation addressCalculation = APEnvironmentFacade.Instance.LanguageModelMgr.CreateAddressCalculaton(comcon);
					bool bError = false;
					addressCalculation.CalculateAddress(dirVar, out bError);
					if (bError)
					{
						flag = false;
					}
				}
				else
				{
					flag = false;
				}
			}
			if (!flag)
			{
				throw new InvalidOperationException(Strings.AddressOutOfRange);
			}
		}

		public void CheckValidValue(ICompiledType type, string stValueText, Guid applicationGuid)
		{
			ICompiledType compiledType = type;
			if (compiledType.Class == TypeClass.Reference)
			{
				compiledType = type.BaseType;
			}
			IConverterFromIEC converterFromIEC = APEnvironmentFacade.Instance.LanguageModelMgr.GetConverterFromIEC();
			if (compiledType.IsInteger)
			{
				string text = stValueText.Trim();
				if (compiledType.Class == TypeClass.Bit)
				{
					converterFromIEC.GetBoolean(text);
					return;
				}
				object value;
				TypeClass typeClass;
				if (compiledType.Class != TypeClass.Enum || !(compiledType is IEnumType2))
				{
					string stIEC = ValueWithIecPrefix(compiledType, text);
					try
					{
						converterFromIEC.GetInteger(stIEC, out value, out typeClass);
						return;
					}
					catch
					{
						if (compiledType.Class == TypeClass.UDInt && text.Length == 1)
						{
							return;
						}
						throw;
					}
				}
				if (!decimal.TryParse(text, out var _))
				{
					ICompileContext compileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(applicationGuid);
					if (compileContext != null && compileContext.CreateGlobalIScope() is IScope4 scope)
					{
						ISignature signature = scope[((IEnumType2)compiledType).SignatureId];
						if (signature != null && signature[text] != null)
						{
							return;
						}
					}
					converterFromIEC.GetInteger(compiledType.ToString() + "#" + text, out value, out typeClass);
				}
				converterFromIEC.GetInteger(compiledType.BaseType.ToString() + "#" + text, out value, out typeClass);
			}
			else if (compiledType.Class == TypeClass.Subrange)
			{
				bool flag = false;
				try
				{
					flag = TryToConvertSubrangeValue(ValueWithIecPrefix(compiledType.BaseType, stValueText.Trim()), compiledType, applicationGuid);
				}
				catch (Exception)
				{
				}
				if (flag)
				{
					string arg = type.ToString();
					throw new ApplicationException(string.Format(Strings.IncompatibleValue, stValueText, arg));
				}
			}
		}

		private string ValueWithIecPrefix(ICompiledType type, string stValue)
		{
			string empty = string.Empty;
			if (APEnvironmentFacade.Instance.LanguageModelMgr.TypeInfo is ITypeInfo3 typeInfo)
			{
				return typeInfo.GetIecName(type.Class) + "#" + stValue;
			}
			return type.ToString() + "#" + stValue;
		}

		public void CheckValidValue(IVarRef varRef, string stValueText, Guid applicationGuid)
		{
			if (varRef.WatchExpression != null && varRef.WatchExpression.Type != null && !CheckForUnicodeCharacter(varRef, stValueText, applicationGuid))
			{
				CheckValidValue(varRef.WatchExpression.Type, stValueText, applicationGuid);
			}
		}

		private bool CheckForUnicodeCharacter(IVarRef varRef, string stValueText, Guid applicationGuid)
		{
			if (varRef.WatchExpression != null && varRef.WatchExpression.Type != null && varRef.WatchExpression.Type.Class == TypeClass.UDInt && stValueText.Length == 1 && varRef.WatchExpression is IExpression3 expression)
			{
				ICompileContext compileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(applicationGuid);
				if (compileContext != null)
				{
					IScope scope = compileContext.CreateGlobalIScope();
					IVariable variable = expression.GetVariable(scope);
					if (variable != null && HasUnicodeCharAttribute(variable, scope))
					{
						return true;
					}
				}
			}
			return false;
		}

		private static bool HasUnicodeCharAttribute(IVariable variable, IScope scope)
		{
			string attributeValue = variable.GetAttributeValue("monitoring_encoding");
			if (attributeValue != null && attributeValue.Equals("UnicodeCharacter", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
			if (variable.OriginalType.Class == TypeClass.Userdef)
			{
				IUserdefType userdefType = variable.OriginalType as IUserdefType;
				ISignature signature = scope[userdefType.SignatureId];
				if (signature != null && signature.GetFlag(SignatureFlag.Alias))
				{
					attributeValue = signature.GetAttributeValue("monitoring_encoding");
					if (attributeValue != null && attributeValue.Equals("UnicodeCharacter", StringComparison.InvariantCultureIgnoreCase))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool TryToConvertSubrangeValue(string stValue, ICompiledType type, Guid application)
		{
			IConverterFromIEC converterFromIEC = APEnvironmentFacade.Instance.LanguageModelMgr.GetConverterFromIEC();
			ITypeInfo typeInfo = APEnvironmentFacade.Instance.LanguageModelMgr.TypeInfo;
			ISubrangeType subrangeType = type as ISubrangeType;
			ICompileContext compileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(application);
			if (compileContext == null)
			{
				return false;
			}
			bool result = false;
			IScope scope = compileContext.CreateGlobalIScope();
			converterFromIEC.GetInteger(stValue, out var value, out var _);
			ILiteralValue literalValue = ((_IExpression)subrangeType.LowerBorder).Literal(scope, bAllocatedOK: true);
			ILiteralValue literalValue2 = ((_IExpression)subrangeType.UpperBorder).Literal(scope, bAllocatedOK: true);
			if (literalValue == null || literalValue2 == null)
			{
				return true;
			}
			if (typeInfo.IsSigned(type.BaseType.Class))
			{
				long num = ((literalValue.KindOf == KindOfLiteral.SignedInteger) ? literalValue.SignedLong : ((long)literalValue.UnsignedLong));
				long num2 = ((literalValue2.KindOf == KindOfLiteral.SignedInteger) ? literalValue2.SignedLong : ((long)literalValue2.UnsignedLong));
				long num3 = Convert.ToInt64(value);
				if (num3 < num || num3 > num2)
				{
					result = true;
				}
			}
			else
			{
				ulong num4 = ((literalValue.KindOf == KindOfLiteral.SignedInteger) ? ((ulong)literalValue.SignedLong) : literalValue.UnsignedLong);
				ulong num5 = ((literalValue2.KindOf == KindOfLiteral.SignedInteger) ? ((ulong)literalValue2.SignedLong) : literalValue2.UnsignedLong);
				ulong num6 = Convert.ToUInt64(value);
				if (num6 < num4 || num6 > num5)
				{
					result = true;
				}
			}
			return result;
		}
	}
}
