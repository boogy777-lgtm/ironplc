using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{BC975E3E-6AB7-45A8-B112-68B526868F49}")]
	internal class LanguageModelBuilderHelper : ILanguageModelBuilderHelper
	{
		private ILanguageModelBuilder6 _lmb;

		private ILanguageModel _lm;

		private IExprementPosition _lastPosition;

		public ILanguageModelBuilder6 LMB => _lmb;

		public ILanguageModel LanguageModel => _lm;

		public LanguageModelBuilderHelper()
		{
		}

		public void Initialize(ILanguageModelBuilder6 lmb, int projectHandle, Guid objectGuid)
		{
			string libraryId = APEnvironmentFacade.Instance.GetLibraryId(projectHandle);
			Guid firstParentApplication = APEnvironmentFacade.Instance.GetFirstParentApplication(projectHandle, objectGuid);
			_lmb = lmb;
			_lm = _lmb.CreateLanguageModel(firstParentApplication, Guid.Empty, objectGuid, libraryId);
		}

		public LanguageModelBuilderHelper(ILanguageModelBuilder6 lmb, int projectHandle, Guid objectGuid)
		{
			Initialize(lmb, projectHandle, objectGuid);
		}

		public void SetLastPosition(IExprementPosition position)
		{
			_lastPosition = position;
		}

		public ICompiledType CreateSimpleType(string stType)
		{
			switch (stType)
			{
			case "BOOL":
				return _lmb.CreateSimpleType(TypeClass.Bool);
			case "BIT":
				return _lmb.CreateSimpleType(TypeClass.Bit);
			case "BYTE":
				return _lmb.CreateSimpleType(TypeClass.Byte);
			case "WORD":
				return _lmb.CreateSimpleType(TypeClass.Word);
			case "DWORD":
				return _lmb.CreateSimpleType(TypeClass.DWord);
			case "LWORD":
				return _lmb.CreateSimpleType(TypeClass.LWord);
			case "SINT":
				return _lmb.CreateSimpleType(TypeClass.SInt);
			case "INT":
				return _lmb.CreateSimpleType(TypeClass.Int);
			case "DINT":
				return _lmb.CreateSimpleType(TypeClass.DInt);
			case "LINT":
				return _lmb.CreateSimpleType(TypeClass.LInt);
			case "USINT":
				return _lmb.CreateSimpleType(TypeClass.USInt);
			case "UINT":
				return _lmb.CreateSimpleType(TypeClass.UInt);
			case "UDINT":
				return _lmb.CreateSimpleType(TypeClass.UDInt);
			case "ULINT":
				return _lmb.CreateSimpleType(TypeClass.ULInt);
			case "REAL":
				return _lmb.CreateSimpleType(TypeClass.Real);
			case "LREAL":
				return _lmb.CreateSimpleType(TypeClass.LReal);
			case "STRING":
				return _lmb.CreateSimpleType(TypeClass.String);
			case "WSTRING":
				return _lmb.CreateSimpleType(TypeClass.WString);
			default:
				return null;
			}
		}

		public IVariableExpression2 Var(string expr)
		{
			return _lmb.CreateVariableExpression(null, expr);
		}

		public IOperatorExpression Adr(IExpression expr)
		{
			return _lmb.CreateOperatorExpression(_lastPosition, Operator.Adr, expr);
		}

		public ILiteralExpression Lit(long l)
		{
			return _lmb.CreateLiteralExpression(_lastPosition, l);
		}

		public T Dup<T>(T exprement) where T : IExprement
		{
			return (T)_lmb.DuplicateExprement(exprement);
		}

		public IStatement CreateAssignToCompo(IExpression leftSide, string identRight, IExpression assignmentValue)
		{
			return _lmb.CreateAssignmentStatement(_lastPosition, Compo(leftSide, identRight), assignmentValue);
		}

		public IExpressionStatement Assign(IExpression expLeft, IExpression expRight)
		{
			return _lmb.CreateAssignmentStatement(_lastPosition, expLeft, expRight);
		}

		public IOperatorExpression Op(Operator op, IExpression expSingleOp)
		{
			return _lmb.CreateOperatorExpression(_lastPosition, op, expSingleOp);
		}

		public ICompoAccessExpression Compo(string varLeft, string varRight)
		{
			return _lmb.CreateCompoAccessExpression(_lastPosition, Var(varLeft), Var(varRight));
		}

		public ICompoAccessExpression Compo(IExpression varLeft, string varRight)
		{
			return CompoN(varLeft, varRight);
		}

		private ICompoAccessExpression CompoN(IExpression varLeft, string varRight)
		{
			ICompoAccessExpression compoAccessExpression = null;
			string[] array = Common.SplitAtDot(varRight);
			if (array.Length >= 2)
			{
				int num = 1;
				compoAccessExpression = _lmb.CreateCompoAccessExpression(_lastPosition, varLeft, Var(array[0]));
				while (num < array.Length)
				{
					compoAccessExpression = _lmb.CreateCompoAccessExpression(_lastPosition, compoAccessExpression, Var(array[num++]));
				}
			}
			else
			{
				compoAccessExpression = _lmb.CreateCompoAccessExpression(_lastPosition, varLeft, Var(varRight));
			}
			return compoAccessExpression;
		}

		public IExpression DerefCompo(IExpression lvalue, string varRight)
		{
			return CompoN(_lmb.CreateDeRefAccessExpression(_lastPosition, lvalue), varRight);
		}

		public IExpression DerefCompo(IExpression lvalue, IVariableExpression2 rvalue)
		{
			return _lmb.CreateCompoAccessExpression(_lastPosition, _lmb.CreateDeRefAccessExpression(_lastPosition, lvalue), rvalue);
		}

		public IExpression CreateLiteralArrayAccess(string array, int index)
		{
			IExpression expAccess = _lmb.CreateLiteralExpression(_lastPosition, index);
			IExpression expBase = Var(array);
			return _lmb.CreateIndexAccessExpression(_lastPosition, expBase, expAccess);
		}

		public IExpression CreateVariableArrayAccess(string array, string index)
		{
			IExpression expAccess = Var(index);
			IExpression expBase = Var(array);
			return _lmb.CreateIndexAccessExpression(_lastPosition, expBase, expAccess);
		}

		public void AddToLanguageModel(IHasVarDeclaration pou)
		{
			pou.AddToLanguageModel(_lastPosition, _lm);
		}

		public IStatement CreateCallInputsOnly(IExpression callee, params IExpression[] inputArgs)
		{
			return _lmb.CreateNonFormalCallStatement(_lastPosition, callee, null, null, inputArgs, new IAssignmentExpression[0]);
		}

		public IExpression CreateMethodCallExpression(IExpression objectExpr, string methodName, params IExpression[] arguments)
		{
			IExpression expCallee = _lmb.CreateCompoAccessExpression(_lastPosition, objectExpr, Var(methodName));
			return _lmb.CreateNonFormalCallExpression(_lastPosition, expCallee, null, null, arguments, new List<IAssignmentExpression>());
		}

		public IExpression CreateMethodCallExpression(string unqualifiedObject, string methodName, params IExpression[] arguments)
		{
			IExpression objectExpr = Var(unqualifiedObject);
			return CreateMethodCallExpression(objectExpr, methodName, arguments);
		}

		public IExpressionStatement Stmt(IExpression expr)
		{
			return _lmb.CreateExpressionStatement(expr);
		}
	}
}
