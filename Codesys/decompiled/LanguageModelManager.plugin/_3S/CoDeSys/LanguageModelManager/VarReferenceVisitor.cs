using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000109 RID: 265
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Refactoring of the VarReferenceVisitor will be done by CDS-77707")]
	internal class VarReferenceVisitor : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x060013A8 RID: 5032 RVA: 0x00038DAB File Offset: 0x00037DAB
		public VarReferenceVisitor(IScope5 scope, _ICompileContext comcon, int nBaseAddress, int nArea, IVarReferenceGenerator generator)
		{
			this.m_generator = generator;
			this.m_generator.Init(scope, comcon);
			this.m_scope = scope;
			this.m_comcon = comcon;
			this.Push(nBaseAddress, nArea, null);
		}

		// Token: 0x060013A9 RID: 5033 RVA: 0x00038DEC File Offset: 0x00037DEC
		public VarReferenceVisitor(IScope5 scope, _ICompileContext comcon, VarRef varRefInstance, IVarReferenceGenerator generator)
		{
			this.m_generator = generator;
			this.m_generator.Init(scope, comcon);
			this.m_scope = scope;
			this.m_comcon = comcon;
			IAddressInfo addressInfo = varRefInstance.AddressInfo;
			if (addressInfo is IAbsoluteAddressInfo)
			{
				this.Push((addressInfo as IAbsoluteAddressInfo).Offset, (addressInfo as IAbsoluteAddressInfo).Area, null);
			}
			else
			{
				this.Push(0, -1, null);
			}
			this.m_varRefInstance = varRefInstance;
		}

		// Token: 0x17000572 RID: 1394
		// (get) Token: 0x060013AA RID: 5034 RVA: 0x00038E6B File Offset: 0x00037E6B
		public IScope5 Scope
		{
			get
			{
				return this.m_scope;
			}
		}

		// Token: 0x17000573 RID: 1395
		// (get) Token: 0x060013AB RID: 5035 RVA: 0x00038E73 File Offset: 0x00037E73
		internal VarReferenceVisitor.VRStackContent TopOfStack
		{
			get
			{
				return this.m_stackAttributes.Peek() as VarReferenceVisitor.VRStackContent;
			}
		}

		// Token: 0x060013AC RID: 5036 RVA: 0x00038E88 File Offset: 0x00037E88
		private void Push(int nAddress, int nArea, IAddressInfo addressinfo)
		{
			VarReferenceVisitor.VRStackContent vrstackContent = new VarReferenceVisitor.VRStackContent();
			vrstackContent.Address = nAddress;
			vrstackContent.Area = nArea;
			vrstackContent.AddressInfo = addressinfo;
			vrstackContent.InstancePath = null;
			vrstackContent.CurrentPOU = null;
			vrstackContent.SubExpressionsAllowed = (this.m_stackAttributes.Count > 0 && this.TopOfStack.SubExpressionsAllowed);
			this.m_stackAttributes.Push(vrstackContent);
		}

		// Token: 0x060013AD RID: 5037 RVA: 0x00038EEC File Offset: 0x00037EEC
		private VarReferenceVisitor.VRStackContent Pop()
		{
			return (VarReferenceVisitor.VRStackContent)this.m_stackAttributes.Pop();
		}

		// Token: 0x060013AE RID: 5038 RVA: 0x00038EFE File Offset: 0x00037EFE
		public void visit(_ICompiledPOU cpou)
		{
			cpou.GetParseTree().Accept(this);
		}

		// Token: 0x060013AF RID: 5039 RVA: 0x00038F0C File Offset: 0x00037F0C
		public void visit(_ISequenceStatement seq)
		{
			IList<_IStatement> statementList = seq._StatementList;
			for (int i = 0; i < statementList.Count; i++)
			{
				statementList[i].Accept(this);
			}
		}

		// Token: 0x060013B0 RID: 5040 RVA: 0x00038F3E File Offset: 0x00037F3E
		public void visit(_IWhileStatement whilst)
		{
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			whilst._Condition.Accept(this);
			this.Pop();
			whilst._Controlled.Accept(this);
		}

		// Token: 0x060013B1 RID: 5041 RVA: 0x00038F7C File Offset: 0x00037F7C
		public void visit(_IRepeatStatement repeat)
		{
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			repeat._Condition.Accept(this);
			this.Pop();
			repeat._Controlled.Accept(this);
		}

		// Token: 0x060013B2 RID: 5042 RVA: 0x00038FBC File Offset: 0x00037FBC
		public void visit(_IForStatement forloop)
		{
			forloop._CounterStart.Accept(this);
			if (forloop._Condition != null)
			{
				this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
				forloop._Condition.Accept(this);
				this.Pop();
			}
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			forloop._UpperBound.Accept(this);
			this.Pop();
			if (forloop._Counter != null)
			{
				this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
				forloop._Counter.Accept(this);
				this.Pop();
			}
			if (forloop._By != null)
			{
				this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
				forloop._By.Accept(this);
				this.Pop();
			}
			forloop._Controlled.Accept(this);
		}

		// Token: 0x060013B3 RID: 5043 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IExitStatement exit)
		{
		}

		// Token: 0x060013B4 RID: 5044 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IContinueStatement cont)
		{
		}

		// Token: 0x060013B5 RID: 5045 RVA: 0x000390BC File Offset: 0x000380BC
		public void visit(_IAssignmentExpression assign)
		{
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			assign._LValue.Accept(this);
			this.Pop();
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			assign._RValue.Accept(this);
			this.Pop();
		}

		// Token: 0x060013B6 RID: 5046 RVA: 0x00039129 File Offset: 0x00038129
		public void visit(_ITryCatchStatement tryCatch)
		{
			tryCatch.DefaultTraverse(this);
		}

		// Token: 0x060013B7 RID: 5047 RVA: 0x00039134 File Offset: 0x00038134
		public void visit(_IIfStatement ifst)
		{
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			ifst._Condition.Accept(this);
			this.Pop();
			ifst._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in ifst._ElseIf)
			{
				this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
				ielseIf._Condition.Accept(this);
				this.Pop();
				ielseIf._Controlled.Accept(this);
			}
			if (ifst._IfElse != null)
			{
				ifst._IfElse.Accept(this);
			}
		}

		// Token: 0x060013B8 RID: 5048 RVA: 0x00039204 File Offset: 0x00038204
		public void visit(_IReturnStatement returnst)
		{
			if (returnst._Condition != null)
			{
				this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
				returnst._Condition.Accept(this);
				this.Pop();
			}
		}

		// Token: 0x060013B9 RID: 5049 RVA: 0x0003923E File Offset: 0x0003823E
		public void visit(_IJumpStatement gotost)
		{
			if (gotost._Condition != null)
			{
				this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
				gotost._Condition.Accept(this);
				this.Pop();
			}
		}

		// Token: 0x060013BA RID: 5050 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_ILabelStatement label)
		{
		}

		// Token: 0x060013BB RID: 5051 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_ICommentStatement comment)
		{
		}

		// Token: 0x060013BC RID: 5052 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IPragmaStatement pragma)
		{
		}

		// Token: 0x060013BD RID: 5053 RVA: 0x00039278 File Offset: 0x00038278
		public void visit(_IExpressionStatement expstat)
		{
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			expstat._Expr.Accept(this);
			this.Pop();
		}

		// Token: 0x060013BE RID: 5054 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IVariableDeclarationStatement vds)
		{
		}

		// Token: 0x060013BF RID: 5055 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IVariableDeclarationListStatement vdls)
		{
		}

		// Token: 0x060013C0 RID: 5056 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IPOUDeclarationStatement pds)
		{
		}

		// Token: 0x060013C1 RID: 5057 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_ITypeDeclarationStatement tds)
		{
		}

		// Token: 0x060013C2 RID: 5058 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IEnumDeclarationStatement eds)
		{
		}

		// Token: 0x060013C3 RID: 5059 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IEnumDeclarationListStatement eds)
		{
		}

		// Token: 0x060013C4 RID: 5060 RVA: 0x000392AC File Offset: 0x000382AC
		public void visit(_ICallExpression call)
		{
			if (call._Condition != null)
			{
				this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
				call._Condition.Accept(this);
				this.Pop();
			}
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			call._Callee.Accept(this);
			int address = this.TopOfStack.Address;
			int area = this.TopOfStack.Area;
			IAddressInfo addressInfo = this.TopOfStack.AddressInfo;
			this.Pop();
			IAddressInfo[] array = this.VisitParameters(call);
			UserdefType userdefType = null;
			if (call._Callee.Type != null)
			{
				userdefType = (call._Callee.Type.DeRefType as UserdefType);
			}
			ISignature signature = (userdefType == null) ? null : userdefType.GetSignature(this.Scope);
			if (userdefType != null && signature != null)
			{
				if (signature.POUType == Operator.Program || signature.POUType == Operator.FunctionBlock)
				{
					this.CreateParameterVarReferences(call, address, area, addressInfo);
					return;
				}
				if (signature.POUType == Operator.Function && addressInfo is IFunctionAddressInfo && array.Length == (int)((IFunctionAddressInfo)addressInfo).InputParameterCount)
				{
					for (int i = 0; i < array.Length; i++)
					{
						((IFunctionAddressInfo)addressInfo).InputParameterAddressInfos[i] = array[i];
					}
					this.m_generator.AdaptFunctionCallInfoSourceposition(addressInfo, call);
					return;
				}
				if (signature.POUType == Operator.Function || signature.POUType == Operator.Method)
				{
					this.TopOfStack.Address = 0;
					this.TopOfStack.Area = -1;
					this.TopOfStack.AddressInfo = null;
					this.m_generator.Remove(addressInfo);
				}
			}
		}

		// Token: 0x060013C5 RID: 5061 RVA: 0x00039454 File Offset: 0x00038454
		private IAddressInfo[] VisitParameters(_ICallExpression call)
		{
			IList<_IExpression> paramExpressions = call.ParamExpressions;
			IAddressInfo[] array = new IAddressInfo[paramExpressions.Count];
			this.TopOfStack.SubExpressionsAllowed = true;
			for (int i = 0; i < paramExpressions.Count; i++)
			{
				if (paramExpressions[i] != null)
				{
					this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
					paramExpressions[i].Accept(this);
					array[i] = this.TopOfStack.AddressInfo;
					this.Pop();
				}
			}
			foreach (_IExpression iexpression in call.OutputExpressions)
			{
				if (iexpression != null)
				{
					this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
					iexpression.Accept(this);
					this.Pop();
				}
			}
			if (Array.Exists<IAddressInfo>(array, (IAddressInfo adrInfo) => adrInfo == null))
			{
				array = Array.Empty<IAddressInfo>();
			}
			return array;
		}

		// Token: 0x060013C6 RID: 5062 RVA: 0x00039574 File Offset: 0x00038574
		private void CreateParameterVarReferences(_ICallExpression call, int nCalleeAddress, int nCalleeArea, IAddressInfo addressinfo)
		{
			foreach (_IExpression iexpression in call.Inputs)
			{
				if (iexpression != null)
				{
					this.Push(nCalleeAddress, nCalleeArea, addressinfo);
					_ICompoAccessExpression icompoAccessExpression = LanguageModelBuilder.Singleton.CreateCompoAccessExpression(call._Callee);
					icompoAccessExpression._Right = iexpression;
					icompoAccessExpression._CompiledType = iexpression._CompiledType;
					this.TopOfStack.InstancePath = icompoAccessExpression;
					iexpression.Accept(this);
					this.Pop();
				}
			}
			foreach (_IExpression iexpression2 in call.Outputs)
			{
				if (iexpression2 != null)
				{
					this.Push(nCalleeAddress, nCalleeArea, addressinfo);
					_ICompoAccessExpression icompoAccessExpression2 = LanguageModelBuilder.Singleton.CreateCompoAccessExpression(call._Callee);
					icompoAccessExpression2._Right = iexpression2;
					icompoAccessExpression2._CompiledType = iexpression2._CompiledType;
					this.TopOfStack.InstancePath = icompoAccessExpression2;
					iexpression2.Accept(this);
					this.Pop();
				}
			}
		}

		// Token: 0x060013C7 RID: 5063 RVA: 0x0003968C File Offset: 0x0003868C
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Refactoring of the VarReferenceVisitor will be done by CDS-77707")]
		public void visit(_IOperatorExpression op)
		{
			IList<_IExpression> operandsList = op._OperandsList;
			IAddressInfo[] array = new IAddressInfo[operandsList.Count];
			bool flag = this.TopOfStack.SubExpressionsAllowed && operandsList.Count > 0 && (op.Code == Operator.Plus || op.Code == Operator.Minus || op.Code == Operator.Times || op.Code == Operator.Divide || op.Code == Operator.Mod);
			for (int i = 0; i < operandsList.Count; i++)
			{
				_IExpression iexpression = operandsList[i];
				if (iexpression.Type == null)
				{
					flag = false;
				}
				this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
				iexpression.Accept(this);
				array[i] = this.TopOfStack.AddressInfo;
				this.Pop();
				if (array[i] == null)
				{
					flag = false;
				}
				if (flag)
				{
					IType deRefType = iexpression.Type.DeRefType;
					if (TypeTable.IsInteger(deRefType.Class))
					{
						array[i] = this.InsertUnsignedToSignedCastIfNecessary(array[i], deRefType.Class);
					}
					else
					{
						flag = false;
					}
				}
			}
			if (flag)
			{
				this.TopOfStack.AddressInfo = this.m_generator.GenerateOperatorExpression(op, array);
				return;
			}
			if (Operator.Adr == op.Code)
			{
				IAbsoluteAddressInfo absoluteAddressInfo = array[0] as IAbsoluteAddressInfo;
				if (absoluteAddressInfo != null)
				{
					this.TopOfStack.AddressInfo = this.m_generator.GenerateAddress(absoluteAddressInfo.Area, absoluteAddressInfo.Offset);
					return;
				}
			}
			this.TopOfStack.Area = -1;
			this.TopOfStack.Address = 0;
			this.TopOfStack.AddressInfo = null;
		}

		// Token: 0x060013C8 RID: 5064 RVA: 0x00039828 File Offset: 0x00038828
		public void visit(_ICastExpression castexp)
		{
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			castexp.BaseExpression.Accept(this);
			int area = this.TopOfStack.Area;
			int address = this.TopOfStack.Address;
			IAddressInfo addressInfo = this.TopOfStack.AddressInfo;
			this.Pop();
			this.TopOfStack.Area = area;
			this.TopOfStack.Address = address;
			this.TopOfStack.AddressInfo = addressInfo;
		}

		// Token: 0x060013C9 RID: 5065 RVA: 0x000398B0 File Offset: 0x000388B0
		public void visit(_INewExpression typeref)
		{
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			typeref._Count.Accept(this);
			if (typeref._FBInitParams != null)
			{
				foreach (IAssignmentExpression assignmentExpression in typeref._FBInitParams)
				{
					(assignmentExpression as _IAssignmentExpression).Accept(this);
				}
			}
			this.Pop();
		}

		// Token: 0x060013CA RID: 5066 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_ITypeExpression typeexp)
		{
		}

		// Token: 0x060013CB RID: 5067 RVA: 0x00039938 File Offset: 0x00038938
		public void visit(_IConversionExpression conv)
		{
			int address = this.TopOfStack.Address;
			int area = this.TopOfStack.Area;
			IAddressInfo addressInfo = this.TopOfStack.AddressInfo;
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			conv._Exp.Accept(this);
			if (conv.Implicit)
			{
				address = this.TopOfStack.Address;
				area = this.TopOfStack.Area;
				addressInfo = this.TopOfStack.AddressInfo;
			}
			this.Pop();
			this.TopOfStack.Address = address;
			this.TopOfStack.Area = area;
			this.TopOfStack.AddressInfo = addressInfo;
		}

		// Token: 0x060013CC RID: 5068 RVA: 0x000399EC File Offset: 0x000389EC
		public void visit(_IThisExpression thisexp)
		{
			ISignature signature = this.m_scope.MethodSignature;
			ICompiledPOU cpou = null;
			if (signature == null && this.m_scope.LocalSignature != null && this.m_scope.LocalSignature.POUType == Operator.FunctionBlock)
			{
				signature = this.m_scope.LocalSignature.GetSubSignature(IdentifierConstants.MainSignatureName);
				cpou = this.m_comcon._GetCompiledPOUById(signature.Id);
			}
			IVarRef varRef = this.m_varRefInstance;
			if (this.m_varRefInstance != null && (this.m_varRefInstance.AddressInfo == null || (this.m_varRefInstance.AddressInfo is IAbsoluteAddressInfo && (this.m_varRefInstance.AddressInfo as IAbsoluteAddressInfo).Area == -1) || this.m_varRefInstance.AddressInfo is IStackRelativeAddressInfo))
			{
				varRef = null;
			}
			if (varRef == null && signature != null)
			{
				_IVariable ivariable = signature[IdentifierConstants.InstancePointer] as _IVariable;
				if (ivariable != null)
				{
					LanguageModelBuilder singleton = LanguageModelBuilder.Singleton;
					IExprementPosition pos = null;
					if (thisexp._Position != null)
					{
						singleton.CreateExprementPosition(thisexp._Position.EditorPosition, thisexp._Position.PositionOffset);
					}
					_IVariableExpression ivariableExpression = singleton.CreateVariableExpression(pos, ivariable.Name) as _IVariableExpression;
					CompilerProxy.TypifyExprement(ivariableExpression, this.Scope, this.m_comcon, null, false, false, null);
					ivariableExpression._CompiledType = ivariable._Type;
					int num = ivariable.DataLocation.Offset;
					bool flag = false;
					ICodegenerator3 codegenerator = this.m_comcon.Codegenerator as ICodegenerator3;
					if (codegenerator != null)
					{
						flag = codegenerator.GetProperty(CodegeneratorProperties.PositiveStackGrow);
					}
					if ((!flag && num >= 0) || (flag && num < 0))
					{
						num += this.m_comcon.Codegenerator.StackDisplacement;
					}
					this.TopOfStack.AddressInfo = this.m_generator.GenerateVarStackRelative(cpou, ivariableExpression, thisexp, num, ivariable._Type.Size(this.Scope));
				}
			}
		}

		// Token: 0x060013CD RID: 5069 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IBaseExpression baseexp)
		{
		}

		// Token: 0x060013CE RID: 5070 RVA: 0x00039BC0 File Offset: 0x00038BC0
		public void visit(_ILiteralExpression literal)
		{
			bool flag = false;
			int literalValue = 0;
			if (this.TopOfStack.SubExpressionsAllowed && literal.Type != null && TypeTable.IsInteger(literal.Type.Class))
			{
				ILiteralValue literalValue2 = literal.LiteralValue;
				if (literalValue2 != null)
				{
					literalValue = literalValue2.GetInt(out flag);
				}
			}
			if (flag)
			{
				this.TopOfStack.AddressInfo = this.m_generator.GenerateSignedConstant(literal, literalValue);
				return;
			}
			if (TypeClass.Pointer == literal.Type.Class)
			{
				ulong literalValue3 = 0UL;
				ILiteralValue literalValue4 = literal.LiteralValue;
				if (literalValue4 != null)
				{
					literalValue3 = (ulong)literalValue4.GetSignedLong(out flag);
				}
				if (flag)
				{
					this.TopOfStack.AddressInfo = this.m_generator.GenerateAddress(literalValue3);
				}
			}
		}

		// Token: 0x060013CF RID: 5071 RVA: 0x00039C6C File Offset: 0x00038C6C
		public void visit(_IAddressExpression address)
		{
			if (address.Type == null)
			{
				return;
			}
			IMessage message;
			bool flag;
			IDataLocation datloc = CompilerProxy.LocateAddress(this.m_comcon, out message, out flag, address.Position, address.DirectAddress, null);
			this.m_generator.GenerateDirectAddress(address, datloc, address.Type.Size(this.Scope));
		}

		// Token: 0x060013D0 RID: 5072 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IQualifiedNameExpression qne)
		{
		}

		// Token: 0x060013D1 RID: 5073 RVA: 0x00039CC0 File Offset: 0x00038CC0
		public void visit(_IVariableExpression variable)
		{
			IVarRef varRef = this.m_varRefInstance;
			if (this.m_varRefInstance != null && (this.m_varRefInstance.AddressInfo == null || (this.m_varRefInstance.AddressInfo is IAbsoluteAddressInfo && (this.m_varRefInstance.AddressInfo as IAbsoluteAddressInfo).Area == -1) || this.m_varRefInstance.AddressInfo is IStackRelativeAddressInfo))
			{
				varRef = null;
			}
			_IVariable ivariable = variable.GetVariable(this.Scope) as _IVariable;
			int num = this.TopOfStack.Address;
			int num2 = this.TopOfStack.Area;
			IAddressInfo addressInfo = this.TopOfStack.AddressInfo;
			_IExpression iexpression = this.TopOfStack.InstancePath;
			if (iexpression == null)
			{
				iexpression = variable;
			}
			int num3 = variable.SignatureId;
			int variableID = variable.VariableId;
			_ICompiledPOU cpou = this.m_comcon._GetCompiledPOUById(num3);
			if (addressInfo is IAddressInfo2 && ivariable != null && !ivariable.GetFlag(VarFlag.Static))
			{
				IAddressInfo2 addressInfo2 = (IAddressInfo2)addressInfo;
				num3 = addressInfo2.SignatureID;
				variableID = addressInfo2.VariableID;
			}
			IAddressInfo addressInfo3 = null;
			ILiteralValue literalValue = variable.Literal(this.Scope);
			if (ivariable != null && ivariable.Type != null && ivariable.Type.Class != TypeClass.Array && ivariable.Type.Class != TypeClass.Userdef && TypeTable.IsInteger(ivariable.Type.Class) && ivariable.GetFlag(VarFlag.Constant) && this.TopOfStack.SubExpressionsAllowed && literalValue != null)
			{
				bool flag;
				int @int = literalValue.GetInt(out flag);
				if (flag)
				{
					addressInfo3 = this.m_generator.GenerateSignedConstant(variable, @int);
					if (ivariable.DataLocation != null)
					{
						num2 = (int)ivariable.DataLocation.Area;
					}
				}
			}
			else if (ivariable != null && ivariable.Type != null && ivariable.IsProperty && ivariable.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING) == CompileAttributes.ATTRIBUTEVALUE_CALL)
			{
				if (ivariable.GetFlag(VarFlag.RelativeInstance) && addressInfo == null)
				{
					addressInfo3 = varRef.AddressInfo;
				}
				else
				{
					addressInfo3 = addressInfo;
				}
				addressInfo3 = this.m_generator.GeneratePropertyCall(variable, iexpression, addressInfo3, variable.Type.Size(this.Scope));
			}
			else if (((ivariable != null) ? ivariable.Type : null) != null && ivariable.DataLocation != null)
			{
				addressInfo3 = addressInfo;
				if (ivariable.GetFlag(VarFlag.RelativeInstance))
				{
					if (addressInfo3 == null && varRef != null)
					{
						addressInfo3 = varRef.AddressInfo;
						if (addressInfo3 is IMyAddressInfo)
						{
							addressInfo3 = (addressInfo3 as IMyAddressInfo).Duplicate();
						}
						iexpression = variable;
						if (addressInfo3 is AbsoluteAddressInfo)
						{
							num = (addressInfo3 as AbsoluteAddressInfo).Offset;
							num2 = (addressInfo3 as AbsoluteAddressInfo).Area;
						}
					}
					num += ivariable.DataLocation.Offset;
				}
				else if (ivariable.GetFlag(VarFlag.RelativeStack))
				{
					num = this.GetAddressForVariableInFunction(ivariable);
					num2 = -1;
				}
				else
				{
					num = ivariable.DataLocation.Offset;
					num2 = (int)ivariable.DataLocation.Area;
				}
				if (ivariable.GetFlag(VarFlag.ReplacedConstant) && ivariable.Initial != null)
				{
					ILiteralValue lv = ((_IExpression)ivariable.Initial).Literal(this.Scope, true);
					addressInfo3 = this.m_generator.GenerateLiteral(variable, iexpression, lv);
				}
				else if (addressInfo3 is StackRelativeAddressInfo)
				{
					addressInfo3 = this.m_generator.GenerateVarStackRelativeOffset(variable, iexpression, addressInfo3 as StackRelativeAddressInfo, num, variable.Type.Size(this.Scope));
				}
				else
				{
					CompoAddressInfo compoAddressInfo = addressInfo3 as CompoAddressInfo;
					if (compoAddressInfo != null)
					{
						addressInfo3 = this.GenerateAdressInfoForCompoAddress(variable, ivariable, num, num2, iexpression, compoAddressInfo);
					}
					else if (addressInfo3 is IArrayAccessAddressInfo2)
					{
						addressInfo3 = this.m_generator.GenerateCompoAccess(iexpression, addressInfo3, ivariable.DataLocation.Offset);
					}
					else if (ivariable.GetFlag(VarFlag.RelativeStack))
					{
						addressInfo3 = this.m_generator.GenerateVarStackRelative(cpou, variable, iexpression, num, variable.Type.Size(this.Scope));
					}
					else if (ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_MONITORING_INSTEAD) || (ivariable.HasAttribute(CompileAttributes.DEVICE_PARAMETER) && !ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_NOWATCH)))
					{
						addressInfo3 = this.HandleMonitoringInstead(variable, ivariable, iexpression, addressInfo3);
					}
					else if (ivariable.IsProperty && ivariable.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING) != CompileAttributes.ATTRIBUTEVALUE_VARIABLE)
					{
						if (ivariable.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING) == CompileAttributes.ATTRIBUTEVALUE_CALL)
						{
							addressInfo3 = this.m_generator.GeneratePropertyCall(variable, iexpression, addressInfo3, variable.Type.Size(this.Scope));
						}
						else
						{
							addressInfo3 = null;
						}
					}
					else if (ivariable.GetFlag(VarFlag.RelativeInstance) && varRef == null && addressInfo == null && this.m_scope != null && this.m_scope.LocalSignature != null && this.m_scope.LocalSignature.POUType == Operator.FunctionBlock)
					{
						LanguageModelBuilder singleton = LanguageModelBuilder.Singleton;
						IExprementPosition pos = singleton.CreateExprementPosition(variable.Position.Position, variable.Position.PositionOffset);
						_IDeRefAccessExpression ideRefAccessExpression = singleton.CreateDeRefAccessExpression(pos, singleton.CreateThisExpression(pos)) as _IDeRefAccessExpression;
						CompilerProxy.TypifyExprement(ideRefAccessExpression, this.m_scope, this.m_comcon, null, false, false, null);
						this.Push(this.TopOfStack.Address, this.TopOfStack.Area, this.TopOfStack.AddressInfo);
						ideRefAccessExpression.Accept(this);
						num = this.TopOfStack.Address;
						addressInfo3 = this.TopOfStack.AddressInfo;
						num2 = this.TopOfStack.Area;
						this.Pop();
						ICompoAccessExpression compoAccessExpression = singleton.CreateCompoAccessExpression(pos, ideRefAccessExpression, variable);
						(compoAccessExpression as _ICompoAccessExpression)._CompiledType = variable._CompiledType;
						this.m_generator.Remove(addressInfo3);
						if (addressInfo3 is DeRefAccessInfo)
						{
							(compoAccessExpression as _IExpression).PositionLength = variable.PositionLength;
							addressInfo3 = this.m_generator.GenerateCompoAccess(compoAccessExpression, addressInfo3, ivariable.DataLocation.Offset);
							if (ivariable != null && ivariable.DataLocation != null && ivariable.DataLocation.BitNr != 255)
							{
								_ICompoAccessExpression icompoAccessExpression = (compoAccessExpression as _IExprement).Duplicate() as _ICompoAccessExpression;
								icompoAccessExpression._Left._CompiledType = TypeTable.Byte;
								addressInfo3 = this.m_generator.GenerateBitAccess(icompoAccessExpression, addressInfo3, (int)ivariable.DataLocation.BitNr, TypeTable.Byte.Size(this.Scope));
							}
						}
						else
						{
							if (num == -1)
							{
								num = 0;
							}
							addressInfo3 = this.m_generator.GenerateVarAbsolut(variable, iexpression, num2, num, variable.Type.Size(this.Scope));
						}
					}
					else if (addressInfo3 is IDeRefAccessInfo && !ivariable.GetFlag(VarFlag.Static))
					{
						addressInfo3 = this.m_generator.GenerateCompoAccess(iexpression, addressInfo3, ivariable.DataLocation.Offset);
					}
					else
					{
						addressInfo3 = this.m_generator.GenerateVarAbsolut(variable, iexpression, num2, num, variable.Type.Size(this.Scope));
					}
				}
			}
			else
			{
				ISignature signature = variable.GetSignature(this.Scope);
				if (signature != null)
				{
					if (signature.HasAttribute(CompileAttributes.ATTRIBUTE_MONITORING))
					{
						if (signature.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING) == CompileAttributes.ATTRIBUTEVALUE_CALL)
						{
							ICompiledPOU compiledPOU = this.m_comcon.GetCompiledPOU(signature.ObjectGuid);
							if (compiledPOU != null && compiledPOU.GetFlag(CompiledPOUFlags.TopLevel))
							{
								addressInfo3 = this.m_generator.GenerateFunctionCall(variable, signature, iexpression, this.Scope);
							}
						}
					}
					else if (addressInfo is StackRelativeAddressInfo && (signature.POUType == Operator.Method || signature.POUType == Operator.Function))
					{
						addressInfo3 = this.m_generator.GenerateVarStackRelativeOffset(variable, iexpression, addressInfo as StackRelativeAddressInfo, num, variable.Type.Size(this.Scope));
					}
					else
					{
						addressInfo3 = this.m_generator.GenerateVarAbsolut(variable, iexpression, num2, num, variable.Type.Size(this.Scope));
					}
				}
			}
			if (addressInfo3 != null)
			{
				if (ivariable != null && ivariable.Type != null && ivariable.Type.Class == TypeClass.Reference)
				{
					this.m_generator.Remove(addressInfo3);
					addressInfo3 = this.m_generator.GenerateDeRefAccess(variable, addressInfo3, 0);
					num = -1;
					num2 = -1;
				}
				if (addressInfo3 is AddressInfoBase)
				{
					AddressInfoBase addressInfoBase = (AddressInfoBase)addressInfo3;
					addressInfoBase.SignatureID = num3;
					addressInfoBase.VariableID = variableID;
				}
			}
			else
			{
				num = -1;
				num2 = -1;
			}
			this.TopOfStack.Address = num;
			this.TopOfStack.Area = num2;
			this.TopOfStack.AddressInfo = addressInfo3;
		}

		// Token: 0x060013D2 RID: 5074 RVA: 0x0003A520 File Offset: 0x00039520
		private int GetAddressForVariableInFunction(_IVariable var)
		{
			bool flag = false;
			ICodegenerator3 codegenerator = this.m_comcon.Codegenerator as ICodegenerator3;
			if (codegenerator != null)
			{
				flag = codegenerator.GetProperty(CodegeneratorProperties.PositiveStackGrow);
			}
			int num = var.DataLocation.Offset;
			if ((!flag && num >= 0) || (flag && num < 0))
			{
				num += this.m_comcon.Codegenerator.StackDisplacement;
			}
			return num;
		}

		// Token: 0x060013D3 RID: 5075 RVA: 0x0003A57C File Offset: 0x0003957C
		private IAddressInfo GenerateAdressInfoForCompoAddress(_IVariableExpression variable, _IVariable var, int nCurrentAddress, int nArea, _IExpression expInstancePath, CompoAddressInfo compoAddressInfo)
		{
			IAddressInfo result;
			if (var.GetFlag(VarFlag.Static) && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352000)
			{
				result = this.m_generator.GenerateVarAbsolut(variable, expInstancePath, nArea, nCurrentAddress, variable.Type.Size(this.Scope));
			}
			else
			{
				result = this.m_generator.GenerateCompoAccess(expInstancePath, compoAddressInfo.Base, compoAddressInfo.Offset + var.DataLocation.Offset);
			}
			return result;
		}

		// Token: 0x060013D4 RID: 5076 RVA: 0x0003A5F8 File Offset: 0x000395F8
		private IAddressInfo HandleMonitoringInstead(_IVariableExpression variable, _IVariable var, _IExpression expInstancePath, IAddressInfo ainfo)
		{
			string text;
			if (var.HasAttribute(CompileAttributes.ATTRIBUTE_MONITORING_INSTEAD))
			{
				text = var.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING_INSTEAD);
				text = CompilerProxy.ReplacePlaceholders(text, expInstancePath);
			}
			else
			{
				text = CompilerProxy.GetParameterGetFunctionCall(var, -1);
			}
			_IExpression iexpression = CompilerProxy.CreateParser(APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(text, false, false, false, false)).ParseExpression() as _IExpression;
			if (iexpression != null)
			{
				CompilerProxy.TypifyExprement(iexpression, this.Scope, this.m_comcon, null, false, false, null);
				iexpression._Position = variable._Position;
				if (iexpression is _ICallExpression)
				{
					ISignature signatureById = this.m_comcon.GetSignatureById(((_ICallExpression)iexpression)._Callee.SignatureId);
					if (signatureById != null && signatureById.HasAttribute(CompileAttributes.ATTRIBUTE_MONITORING) && signatureById.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING) == CompileAttributes.ATTRIBUTEVALUE_CALL)
					{
						ICompiledPOU compiledPOU = this.m_comcon.GetCompiledPOU(signatureById.ObjectGuid);
						if (compiledPOU != null && compiledPOU.GetFlag(CompiledPOUFlags.TopLevel))
						{
							ainfo = this.m_generator.GenerateFunctionCall(variable, signatureById, expInstancePath, this.Scope);
							if (ainfo != null)
							{
								try
								{
									for (int i = 0; i < ((FunctionAddressInfo)ainfo).InputParameterAddressInfos.Length; i++)
									{
										_IExpression iexpression2 = ((_ICallExpression)iexpression).ParamExpressions[i];
										if (iexpression2 is _IIntegerLiteralExpression)
										{
											_IIntegerLiteralExpression iintegerLiteralExpression = iexpression2 as _IIntegerLiteralExpression;
											((FunctionAddressInfo)ainfo).InputParameterAddressInfos[i] = new SignedConstantAddressInfo(iintegerLiteralExpression.LongValue);
										}
										else
										{
											this.Push(0, -1, null);
											iexpression2.Accept(this);
											this.m_generator.Remove(this.TopOfStack.AddressInfo);
											((FunctionAddressInfo)ainfo).InputParameterAddressInfos[i] = this.TopOfStack.AddressInfo;
											this.Pop();
										}
									}
								}
								catch
								{
									ainfo = null;
								}
							}
						}
					}
				}
			}
			return ainfo;
		}

		// Token: 0x060013D5 RID: 5077 RVA: 0x0003A7E4 File Offset: 0x000397E4
		public void visit(_IIndexAccessExpression indexaccess)
		{
			IVariable variable;
			bool flag = VariableLengthArrayHelper.IsVariableLengthArray(indexaccess, this.m_scope, out variable);
			if (flag)
			{
				VariableLengthArrayHelper.HandleVariableLengthArray(indexaccess, this.m_scope);
			}
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, this.TopOfStack.AddressInfo);
			indexaccess._Var.Accept(this);
			int area = this.TopOfStack.Area;
			int address = this.TopOfStack.Address;
			IAddressInfo addressInfo = this.TopOfStack.AddressInfo;
			this.Pop();
			new IndexAccessAddressInfoCalculator(this.TopOfStack, this.Scope, this.m_generator, this).CalculateIndexAccessAddressInfo(indexaccess, flag, addressInfo, area, address);
		}

		// Token: 0x060013D6 RID: 5078 RVA: 0x0003A890 File Offset: 0x00039890
		internal void CreateIndexVarAddressInfo(_IIndexAccessExpression indexaccess, ICollection<_IExpression> exprs, out IAddressInfo[] aiIndices, out IArrayBounds[] aiBounds, ref bool bValid)
		{
			aiIndices = new IAddressInfo[exprs.Count];
			aiBounds = new IArrayBounds[exprs.Count];
			for (int i = 0; i < exprs.Count; i++)
			{
				_IExpression access = indexaccess.GetAccess(i);
				if (access.Type == null)
				{
					bValid = false;
					return;
				}
				IType deRefType = access.Type.DeRefType;
				this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
				this.TopOfStack.SubExpressionsAllowed = true;
				access.Accept(this);
				aiIndices[i] = this.TopOfStack.AddressInfo;
				this.Pop();
				if (aiIndices[i] == null)
				{
					bValid = false;
					return;
				}
				if (!TypeTable.IsInteger(deRefType.Class))
				{
					bValid = false;
					return;
				}
				aiIndices[i] = this.InsertUnsignedToSignedCastIfNecessary(aiIndices[i], deRefType.Class);
			}
		}

		// Token: 0x060013D7 RID: 5079 RVA: 0x0003A967 File Offset: 0x00039967
		private VarReferenceVisitor.VRStackContent Visit(int address, int area, IAddressInfo addressInfo, _IExpression expression)
		{
			this.Push(address, area, addressInfo);
			expression.Accept(this);
			return this.Pop();
		}

		// Token: 0x060013D8 RID: 5080 RVA: 0x0003A980 File Offset: 0x00039980
		private bool TryHandleBitAccess(_ICompoAccessExpression compo, IAddressInfo adrinfoLeft)
		{
			if (!compo._Left.Type.DeRefType.IsInteger)
			{
				return false;
			}
			ILiteralValue literalValue = compo._Right.Literal(this.Scope);
			if (literalValue == null)
			{
				if (adrinfoLeft is AbsoluteAddressInfo)
				{
					this.m_generator.Remove(adrinfoLeft);
				}
			}
			else
			{
				bool flag;
				int @int = literalValue.GetInt(out flag);
				if (flag)
				{
					IMyAddressInfo myAddressInfo = adrinfoLeft as IMyAddressInfo;
					IAddressInfo aiBase;
					if (myAddressInfo != null)
					{
						aiBase = myAddressInfo.Duplicate();
					}
					else
					{
						aiBase = null;
					}
					this.TopOfStack.AddressInfo = this.m_generator.GenerateBitAccess(compo, aiBase, @int, compo._Left.Type.DeRefType.Size(this.Scope));
				}
			}
			return true;
		}

		// Token: 0x060013D9 RID: 5081 RVA: 0x0003AA2C File Offset: 0x00039A2C
		public void visit(_ICompoAccessExpression compo)
		{
			VarReferenceVisitor.VRStackContent vrstackContent = this.Visit(this.TopOfStack.Address, this.TopOfStack.Area, null, compo._Left);
			if (compo._Left.Type == null)
			{
				return;
			}
			IAddressInfo addressInfo = vrstackContent.AddressInfo;
			if (this.TryHandleBitAccess(compo, addressInfo))
			{
				return;
			}
			int num;
			int num2;
			IAddressInfo addressInfo2;
			bool flag;
			this.TryHandleUserdefAccess(compo, vrstackContent, out num, out num2, out addressInfo2, out flag);
			if (num2 == -1 && flag && !(addressInfo2 is IStackRelativeAddressInfo))
			{
				this.TopOfStack.Address = -1;
				this.TopOfStack.Area = -1;
				this.m_generator.Remove(addressInfo);
				this.m_generator.Remove(addressInfo2);
				int bitOffset = VarReferenceVisitor.GetBitOffset(addressInfo2);
				this.TopOfStack.AddressInfo = this.m_generator.GenerateCompoAccess(compo, addressInfo, num);
				if (bitOffset >= 0)
				{
					this.TopOfStack.AddressInfo = this.m_generator.GenerateBitAccess(compo, this.TopOfStack.AddressInfo, bitOffset, TypeTable.Byte.Size(this.Scope));
					return;
				}
			}
			else
			{
				this.m_generator.Remove(addressInfo);
				this.TopOfStack.Address = num;
				this.TopOfStack.Area = num2;
				this.TopOfStack.AddressInfo = addressInfo2;
				if (compo.Right is IVariableExpression && compo.Right.DataLocation(this.Scope) != null && (this.TopOfStack.AddressInfo is StackRelativeAddressInfo || this.TopOfStack.AddressInfo is IDeRefAccessInfo || this.TopOfStack.AddressInfo is ICompoAddressInfo || this.TopOfStack.AddressInfo is IArrayAccessAddressInfo) && compo.Right.DataLocation(this.Scope).IsBitLocation && compo.Right.DataLocation(this.Scope).BitNr != 255)
				{
					this.m_generator.Remove(this.TopOfStack.AddressInfo);
					this.TopOfStack.AddressInfo = this.m_generator.GenerateBitAccess(compo, this.TopOfStack.AddressInfo, (int)compo.Right.DataLocation(this.Scope).BitNr, TypeTable.Byte.Size(this.Scope));
				}
			}
		}

		// Token: 0x060013DA RID: 5082 RVA: 0x0003AC6C File Offset: 0x00039C6C
		private void TryHandleUserdefAccess(_ICompoAccessExpression compo, VarReferenceVisitor.VRStackContent leftInfo, out int nRightAddress, out int nRightArea, out IAddressInfo adrinfoRight, out bool bSkipTheRightSide)
		{
			ICompiledType compiledType = compo._Left.Type.DeRefType;
			if (compo._Left.Type.DeRefType.Class == TypeClass.Pointer || compo._Left.Type.DeRefType.Class == TypeClass.Array)
			{
				compiledType = compo._Left.Type.DeRefType.BaseType;
			}
			nRightAddress = -1;
			nRightArea = -1;
			adrinfoRight = null;
			bSkipTheRightSide = true;
			if (compiledType.Class != TypeClass.Userdef)
			{
				return;
			}
			ISignature signature = ((UserdefType)compiledType).GetSignature(this.Scope);
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			if (signature != null)
			{
				flag = (signature.POUType == Operator.Method || signature.POUType == Operator.Function || signature.POUType == Operator.Program || signature.POUType == Operator.VarGlobal);
				flag2 = (signature.POUType == Operator.VarGlobal);
				flag3 = (signature.POUType == Operator.Program);
				flag4 = (signature.POUType == Operator.Function || signature.POUType == Operator.Method);
			}
			IAddressInfo addressInfo = leftInfo.AddressInfo;
			int area = leftInfo.Area;
			if (signature != null && (!flag2 && !flag3) && !flag4 && area == -1 && addressInfo == null)
			{
				return;
			}
			bSkipTheRightSide = (leftInfo.Area == -1 && !(addressInfo is IStackRelativeAddressInfo) && !(addressInfo is IDeRefAccessInfo) && !(addressInfo is IArrayAccessAddressInfo2) && !(addressInfo is ICompoAddressInfo) && !(addressInfo is IPropertyAddressInfoExtended) && !flag);
			if (bSkipTheRightSide)
			{
				this.Push(0, -1, null);
			}
			else
			{
				this.Push(leftInfo.Address, area, addressInfo);
			}
			this.TopOfStack.InstancePath = compo;
			if (flag4)
			{
				this.TopOfStack.CurrentPOU = this.m_comcon.GetCompiledPOUById(signature.Id);
			}
			compo._Right.Accept(this);
			nRightAddress = this.TopOfStack.Address;
			nRightArea = this.TopOfStack.Area;
			adrinfoRight = this.TopOfStack.AddressInfo;
			this.Pop();
		}

		// Token: 0x060013DB RID: 5083 RVA: 0x0003AE58 File Offset: 0x00039E58
		private static int GetBitOffset(IAddressInfo adrinfo)
		{
			if (adrinfo is IStackRelativeAddressInfo)
			{
				int bitOffset = (int)(adrinfo as IStackRelativeAddressInfo).BitOffset;
				if (bitOffset == 255)
				{
					return -1;
				}
				return bitOffset;
			}
			else if (adrinfo is IAbsoluteAddressInfo)
			{
				int bitOffset2 = (int)(adrinfo as IAbsoluteAddressInfo).BitOffset;
				if (bitOffset2 == 255)
				{
					return -1;
				}
				return bitOffset2;
			}
			else
			{
				if (adrinfo is IBitAddressInfo)
				{
					return (adrinfo as IBitAddressInfo).BitOffset;
				}
				return -1;
			}
		}

		// Token: 0x060013DC RID: 5084 RVA: 0x0003AEBC File Offset: 0x00039EBC
		public void visit(_IPartialAccessExpression partialAccessExpression)
		{
			VarReferenceVisitor.VRStackContent vrstackContent = this.Visit(this.TopOfStack.Address, this.TopOfStack.Area, null, partialAccessExpression._Left);
			if (!(vrstackContent.AddressInfo is LiteralAddressInfo))
			{
				int nOffset;
				int num;
				if (partialAccessExpression.GetByteOffsetAndSize(this.m_scope, out nOffset, out num))
				{
					this.m_generator.Remove(vrstackContent.AddressInfo);
					this.TopOfStack.AddressInfo = this.m_generator.GenerateCompoAccess(partialAccessExpression, vrstackContent.AddressInfo, nOffset);
					this.TopOfStack.Address = 0;
					this.TopOfStack.Area = vrstackContent.Area;
					return;
				}
				if (partialAccessExpression.PartSize == DirectVariableSize.X)
				{
					int partOffset = partialAccessExpression.PartOffset;
					IMyAddressInfo myAddressInfo = vrstackContent.AddressInfo as IMyAddressInfo;
					IAddressInfo aiBase = (myAddressInfo != null) ? myAddressInfo.Duplicate() : null;
					this.TopOfStack.AddressInfo = this.m_generator.GenerateBitAccess(partialAccessExpression, aiBase, partOffset, partialAccessExpression._Left.Type.DeRefType.Size(this.Scope));
				}
			}
		}

		// Token: 0x060013DD RID: 5085 RVA: 0x0003AFC0 File Offset: 0x00039FC0
		public void visit(_IDeRefAccessExpression deref)
		{
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, this.TopOfStack.AddressInfo);
			deref._Base.Accept(this);
			IAddressInfo addressInfo = this.TopOfStack.AddressInfo;
			int area = this.TopOfStack.Area;
			int address = this.TopOfStack.Address;
			this.Pop();
			if (addressInfo != null || area == -1 || address == -1)
			{
				this.m_generator.Remove(addressInfo);
				this.TopOfStack.AddressInfo = this.m_generator.GenerateDeRefAccess(deref, addressInfo, 0);
				this.TopOfStack.Address = -1;
				this.TopOfStack.Area = -1;
			}
		}

		// Token: 0x060013DE RID: 5086 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_ICopyScopeExpression copyexp)
		{
		}

		// Token: 0x060013DF RID: 5087 RVA: 0x0003B07C File Offset: 0x0003A07C
		public void visit(_IGlobalScopeExpression globexp)
		{
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			globexp._Base.Accept(this);
			IAddressInfo addressInfo = this.TopOfStack.AddressInfo;
			int area = this.TopOfStack.Area;
			int address = this.TopOfStack.Address;
			this.Pop();
			if (this.m_stackAttributes.Count > 0)
			{
				this.TopOfStack.AddressInfo = addressInfo;
				this.TopOfStack.Address = address;
				this.TopOfStack.Area = area;
			}
		}

		// Token: 0x060013E0 RID: 5088 RVA: 0x0003B10F File Offset: 0x0003A10F
		public void visit(_ISystemScopeExpression systemscope)
		{
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			systemscope._Base.Accept(this);
			this.Pop();
		}

		// Token: 0x060013E1 RID: 5089 RVA: 0x0003B141 File Offset: 0x0003A141
		public void visit(_IPoolScopeExpression poolscope)
		{
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			poolscope._Base.Accept(this);
			this.Pop();
		}

		// Token: 0x060013E2 RID: 5090 RVA: 0x0003B174 File Offset: 0x0003A174
		public void visit(_ICurrentTaskExpression currentTask)
		{
			_ISignature isignature = null;
			if (this.Scope.MethodSignature != null)
			{
				isignature = (this.Scope.MethodSignature as _ISignature);
			}
			else if (this.Scope.LocalSignature != null)
			{
				isignature = (this.Scope.LocalSignature as _ISignature);
			}
			_IVariable ivariable = ((isignature != null) ? isignature[IdentifierConstants.CurrentTaskInfoPointer] : null) as _IVariable;
			if (ivariable != null)
			{
				LanguageModelBuilder singleton = LanguageModelBuilder.Singleton;
				IExprementPosition pos = null;
				if (currentTask._Position != null)
				{
					pos = singleton.CreateExprementPosition(currentTask._Position.EditorPosition, currentTask._Position.PositionOffset);
				}
				_IVariableExpression ivariableExpression = singleton.CreateVariableExpression(pos, ivariable.Name) as _IVariableExpression;
				_IDeRefAccessExpression ideRefAccessExpression = singleton.CreateDeRefAccessExpression(pos, ivariableExpression) as _IDeRefAccessExpression;
				CompilerProxy.TypifyExprement(ideRefAccessExpression, this.Scope, this.m_comcon, null, false, false, null);
				int num = ivariable.DataLocation.Offset;
				bool flag = false;
				ICodegenerator3 codegenerator = this.m_comcon.Codegenerator as ICodegenerator3;
				if (codegenerator != null)
				{
					flag = codegenerator.GetProperty(CodegeneratorProperties.PositiveStackGrow);
				}
				if ((!flag && num >= 0) || (flag && num < 0))
				{
					num += this.m_comcon.Codegenerator.StackDisplacement;
				}
				IAddressInfo addressInfo = this.m_generator.GenerateVarStackRelative(this.m_comcon.GetCompiledPOUById(isignature.Id), ivariableExpression, currentTask, num, ivariable._Type.Size(this.Scope));
				this.m_generator.Remove(addressInfo);
				addressInfo = this.m_generator.GenerateDeRefAccess(ideRefAccessExpression, addressInfo, 0);
				this.Push(this.TopOfStack.Address, this.TopOfStack.Area, addressInfo);
				currentTask._Base.Accept(this);
				this.Pop();
			}
		}

		// Token: 0x060013E3 RID: 5091 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IEmptyStatement empty)
		{
		}

		// Token: 0x060013E4 RID: 5092 RVA: 0x0003B324 File Offset: 0x0003A324
		public void visit(_ICaseRangeExpression caserange)
		{
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			caserange._Low.Accept(this);
			this.Pop();
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			caserange._High.Accept(this);
			this.Pop();
		}

		// Token: 0x060013E5 RID: 5093 RVA: 0x0003B394 File Offset: 0x0003A394
		public void visit(_ICaseLabelStatement caselabel)
		{
			foreach (_IExprement iexprement in caselabel._cases)
			{
				this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
				iexprement.Accept(this);
				this.Pop();
			}
		}

		// Token: 0x060013E6 RID: 5094 RVA: 0x0003B404 File Offset: 0x0003A404
		public void visit(_ICaseStatement casest)
		{
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			casest._Switch.Accept(this);
			this.Pop();
			foreach (_ICase icase in casest._Cases)
			{
				icase._Label.Accept(this);
				icase._Controlled.Accept(this);
			}
			if (casest._Else != null)
			{
				casest._Else.Accept(this);
			}
		}

		// Token: 0x060013E7 RID: 5095 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IErrorExpression errorexp)
		{
		}

		// Token: 0x060013E8 RID: 5096 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IErrorStatement errorst)
		{
		}

		// Token: 0x060013E9 RID: 5097 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_INullExpression errorexp)
		{
		}

		// Token: 0x060013EA RID: 5098 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_INullStatement errorst)
		{
		}

		// Token: 0x060013EB RID: 5099 RVA: 0x0003B4A4 File Offset: 0x0003A4A4
		public void visit(_IMultipleIndexInitialization multiindex)
		{
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			multiindex._Number.Accept(this);
			this.Pop();
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			multiindex._Value.Accept(this);
			this.Pop();
		}

		// Token: 0x060013EC RID: 5100 RVA: 0x0003B514 File Offset: 0x0003A514
		public void visit(_IArrayInitialization arrayinit)
		{
			this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
			IList<_IExpression> initValues = arrayinit._InitValues;
			this.Pop();
			for (int i = 0; i < initValues.Count; i++)
			{
				this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
				initValues[i].Accept(this);
				this.Pop();
			}
		}

		// Token: 0x060013ED RID: 5101 RVA: 0x0003B590 File Offset: 0x0003A590
		public void visit(_IStructureInitialization structinit)
		{
			foreach (_IAssignmentExpression iassignmentExpression in structinit._CompoInits)
			{
				this.Push(this.TopOfStack.Address, this.TopOfStack.Area, null);
				iassignmentExpression._RValue.Accept(this);
				this.Pop();
			}
		}

		// Token: 0x060013EE RID: 5102 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IDefineReference defref)
		{
		}

		// Token: 0x060013EF RID: 5103 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IVariableReference varref)
		{
		}

		// Token: 0x060013F0 RID: 5104 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_ITypeReference typeref)
		{
		}

		// Token: 0x060013F1 RID: 5105 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IPouReference pouref)
		{
		}

		// Token: 0x060013F2 RID: 5106 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_ITaskReference taskref)
		{
		}

		// Token: 0x060013F3 RID: 5107 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IResourceReference resref)
		{
		}

		// Token: 0x060013F4 RID: 5108 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IDefinedExpression defexp)
		{
		}

		// Token: 0x060013F5 RID: 5109 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IPragmaOperatorExpression popexp)
		{
		}

		// Token: 0x060013F6 RID: 5110 RVA: 0x0003B604 File Offset: 0x0003A604
		public void visit(_IPragmaAssertion assertion)
		{
			assertion.Condition.Accept(this);
		}

		// Token: 0x060013F7 RID: 5111 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_ICompilerVersionExpression compiversionexp)
		{
		}

		// Token: 0x060013F8 RID: 5112 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IProjectDefinedExpression projectDefinedExpression)
		{
		}

		// Token: 0x060013F9 RID: 5113 RVA: 0x0003B614 File Offset: 0x0003A614
		public void visit(_IPragmaIfStatement pifst)
		{
			pifst.IfThen.Accept(this);
			foreach (_IPragmaElseIf ipragmaElseIf in pifst.ElseIf)
			{
				ipragmaElseIf.Controlled.Accept(this);
			}
			if (pifst.IfElse != null)
			{
				pifst.IfElse.Accept(this);
			}
		}

		// Token: 0x060013FA RID: 5114 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IBreakPointStatement bpstate)
		{
		}

		// Token: 0x060013FB RID: 5115 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IDefineStatement defstate)
		{
		}

		// Token: 0x060013FC RID: 5116 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IXRefExpression xref)
		{
		}

		// Token: 0x060013FD RID: 5117 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IHasTypeExpression hastype)
		{
		}

		// Token: 0x060013FE RID: 5118 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IIsEnumTypeExpression isenumtype)
		{
		}

		// Token: 0x060013FF RID: 5119 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IHasAttributeExpression hasattribute)
		{
		}

		// Token: 0x06001400 RID: 5120 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IHasValueExpression hasvalue)
		{
		}

		// Token: 0x06001401 RID: 5121 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IHasConstantValueExpression hasvalue)
		{
		}

		// Token: 0x06001402 RID: 5122 RVA: 0x0003B684 File Offset: 0x0003A684
		private IAddressInfo InsertUnsignedToSignedCastIfNecessary(IAddressInfo aiBase, TypeClass tcValue)
		{
			Debug.Assert(TypeTable.IsInteger(tcValue));
			if (aiBase == null)
			{
				return null;
			}
			if (aiBase is IOperatorAddressInfo || aiBase is ISignedConstantAddressInfo)
			{
				return aiBase;
			}
			if (!TypeTable.IsSigned(tcValue))
			{
				return OperatorAddressInfo.CreateUnsignedToSignedCast(aiBase, tcValue);
			}
			return aiBase;
		}

		// Token: 0x06001403 RID: 5123 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IHasConstantTypeExpression hasConstantTypeExpression)
		{
		}

		// Token: 0x06001404 RID: 5124 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_INamespaceAccessExpression namespaceaccess)
		{
		}

		// Token: 0x06001405 RID: 5125 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IRuntimeVersionExpression runtimeversionexp)
		{
		}

		// Token: 0x0400047D RID: 1149
		private readonly _ICompileContext m_comcon;

		// Token: 0x0400047E RID: 1150
		private readonly Stack m_stackAttributes = new Stack();

		// Token: 0x0400047F RID: 1151
		private readonly IVarReferenceGenerator m_generator;

		// Token: 0x04000480 RID: 1152
		private readonly IVarRef m_varRefInstance;

		// Token: 0x04000481 RID: 1153
		private readonly IScope5 m_scope;

		// Token: 0x020002A9 RID: 681
		internal class VRStackContent
		{
			// Token: 0x17000C09 RID: 3081
			// (get) Token: 0x06002B8F RID: 11151 RVA: 0x000739D1 File Offset: 0x000729D1
			// (set) Token: 0x06002B90 RID: 11152 RVA: 0x000739D9 File Offset: 0x000729D9
			public int Address
			{
				get
				{
					return this.nAddress;
				}
				set
				{
					this.nAddress = value;
				}
			}

			// Token: 0x17000C0A RID: 3082
			// (get) Token: 0x06002B91 RID: 11153 RVA: 0x000739E2 File Offset: 0x000729E2
			// (set) Token: 0x06002B92 RID: 11154 RVA: 0x000739EA File Offset: 0x000729EA
			public int Area
			{
				get
				{
					return this.nArea;
				}
				set
				{
					this.nArea = value;
				}
			}

			// Token: 0x17000C0B RID: 3083
			// (get) Token: 0x06002B93 RID: 11155 RVA: 0x000739F3 File Offset: 0x000729F3
			// (set) Token: 0x06002B94 RID: 11156 RVA: 0x000739FB File Offset: 0x000729FB
			public IAddressInfo AddressInfo
			{
				get
				{
					return this.addressinfo;
				}
				set
				{
					this.addressinfo = value;
				}
			}

			// Token: 0x17000C0C RID: 3084
			// (get) Token: 0x06002B95 RID: 11157 RVA: 0x00073A04 File Offset: 0x00072A04
			// (set) Token: 0x06002B96 RID: 11158 RVA: 0x00073A0C File Offset: 0x00072A0C
			public _IExpression InstancePath
			{
				get
				{
					return this.expInstance;
				}
				set
				{
					this.expInstance = value;
				}
			}

			// Token: 0x17000C0D RID: 3085
			// (get) Token: 0x06002B97 RID: 11159 RVA: 0x00073A15 File Offset: 0x00072A15
			// (set) Token: 0x06002B98 RID: 11160 RVA: 0x00073A1D File Offset: 0x00072A1D
			public ICompiledPOU CurrentPOU
			{
				get
				{
					return this.cpouCurrent;
				}
				set
				{
					this.cpouCurrent = value;
				}
			}

			// Token: 0x17000C0E RID: 3086
			// (get) Token: 0x06002B99 RID: 11161 RVA: 0x00073A26 File Offset: 0x00072A26
			// (set) Token: 0x06002B9A RID: 11162 RVA: 0x00073A2E File Offset: 0x00072A2E
			public bool SubExpressionsAllowed
			{
				get
				{
					return this._bSubExpressionsAllowed;
				}
				set
				{
					this._bSubExpressionsAllowed = value;
				}
			}

			// Token: 0x04000892 RID: 2194
			private int nAddress;

			// Token: 0x04000893 RID: 2195
			private int nArea;

			// Token: 0x04000894 RID: 2196
			private IAddressInfo addressinfo;

			// Token: 0x04000895 RID: 2197
			private _IExpression expInstance;

			// Token: 0x04000896 RID: 2198
			private ICompiledPOU cpouCurrent;

			// Token: 0x04000897 RID: 2199
			private bool _bSubExpressionsAllowed;
		}
	}
}
