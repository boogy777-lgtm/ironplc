using System;
using System.Runtime.CompilerServices;
using \u000E;
using \u0014;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0013
{
	// Token: 0x020002C0 RID: 704
	internal sealed class \u0007
	{
		// Token: 0x17000773 RID: 1907
		// (get) Token: 0x06002AEF RID: 10991 RVA: 0x0009715C File Offset: 0x0009535C
		private global::\u000E.\u0011 Context { get; }

		// Token: 0x06002AF0 RID: 10992 RVA: 0x00097164 File Offset: 0x00095364
		private \u0007(global::\u000E.\u0011 \u0083\u0005)
		{
			this.Context = \u0083\u0005;
		}

		// Token: 0x06002AF1 RID: 10993 RVA: 0x00097174 File Offset: 0x00095374
		public static _IStatement \u0001(_IAssignmentExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			return new global::\u0013.\u0007(\u0003).\u0001(\u0002);
		}

		// Token: 0x06002AF2 RID: 10994 RVA: 0x00097184 File Offset: 0x00095384
		public static bool \u0001(_IAssignmentExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			if (!global::\u0013.\u0007.\u0001(\u0002))
			{
				return false;
			}
			_IVariable ivariable = \u0002._LValue.GetVariable(\u0003._Scope) as _IVariable;
			return ivariable != null && ivariable.IsProperty;
		}

		// Token: 0x06002AF3 RID: 10995 RVA: 0x000971C4 File Offset: 0x000953C4
		private _IStatement \u0001(_IAssignmentExpression \u0002)
		{
			if (!global::\u0013.\u0007.\u0001(\u0002))
			{
				return null;
			}
			_IVariable ivariable = \u0002._LValue.GetVariable(this.Context._Scope) as _IVariable;
			if (ivariable == null)
			{
				return null;
			}
			if (!ivariable.IsProperty)
			{
				return null;
			}
			if (ivariable.HasAttribute(CompileAttributes.DEVICE_PARAMETER))
			{
				return this.\u0004(\u0002, ivariable);
			}
			if (ivariable.HasAttribute(CompileAttributes.SET_ACCESS))
			{
				return this.\u0003(\u0002, ivariable);
			}
			if (ivariable.Type.Class == TypeClass.Reference && \u0002.KindOf == Operator.Assign)
			{
				return this.\u0002(\u0002, ivariable);
			}
			return this.\u0001(\u0002, ivariable);
		}

		// Token: 0x06002AF4 RID: 10996 RVA: 0x00097264 File Offset: 0x00095464
		private static bool \u0001(_IAssignmentExpression \u0002)
		{
			bool result = !(\u0002._LValue is _IDeRefAccessExpression);
			if (\u0002._LValue is _IIndexAccessExpression)
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06002AF5 RID: 10997 RVA: 0x00097294 File Offset: 0x00095494
		private _IStatement \u0001(_IAssignmentExpression \u0002, _IVariable \u0003)
		{
			_IExpression iexpression = null;
			_ICompoAccessExpression icompoAccessExpression = \u0002._LValue as _ICompoAccessExpression;
			if (icompoAccessExpression != null)
			{
				iexpression = (icompoAccessExpression._Left.Duplicate() as _IExpression);
			}
			ISignature signature = this.Context._Scope[\u0002._LValue.SignatureId];
			bool flag = signature != null && (signature.POUType == Operator.VarGlobal || signature.POUType == Operator.Program);
			_ILanguageModelBuilder ilanguageModelBuilder = \u0019.\u0003.Builder;
			_ICompoAccessExpression icompoAccessExpression2;
			if (flag && iexpression == null)
			{
				icompoAccessExpression2 = ilanguageModelBuilder.CreateCompoAccessExpression(ilanguageModelBuilder.CreateVariableExpression(signature.Name));
			}
			else if (iexpression == null)
			{
				icompoAccessExpression2 = ilanguageModelBuilder.CreateCompoAccessExpression(ilanguageModelBuilder.CreateDeRefAccessExpression(ilanguageModelBuilder.CreateThisExpression()));
			}
			else
			{
				icompoAccessExpression2 = ilanguageModelBuilder.CreateCompoAccessExpression(iexpression);
			}
			icompoAccessExpression2._Right = ilanguageModelBuilder.CreateVariableExpression("__set" + \u0003.VersionedName);
			_ICallExpression icallExpression = ilanguageModelBuilder.CreateCallExpression(icompoAccessExpression2);
			icallExpression.AddParam(\u0002._RValue.Duplicate() as _IExpression, ilanguageModelBuilder.CreateVariableExpression(\u0003.VersionedName));
			_IExpressionStatement u = \u0019.\u0003.\u0001(icallExpression, Token.Empty);
			_IStatement istatement = this.Context.Generator.\u0001<_IExpressionStatement>(u, this.Context._Scope, this.Context.CompiledPOU);
			istatement.SetFlag(StatementFlag.GenerateFlow | StatementFlag.GenerateBP, true);
			this.Context.Generator.CopyPositionAndMessages(\u0002, istatement);
			return istatement;
		}

		// Token: 0x06002AF6 RID: 10998 RVA: 0x000973F0 File Offset: 0x000955F0
		private _ISignature \u0001()
		{
			_ISignature isignature = (_ISignature)this.Context.Comcon.GetSignatureById(this.Context._Scope.MostLocalSignatureId);
			if (isignature.POUType == Operator.FunctionBlock)
			{
				isignature = (isignature.GetSubSignature(IdentifierConstants.MainSignatureName) as _ISignature);
			}
			return isignature;
		}

		// Token: 0x06002AF7 RID: 10999 RVA: 0x00097448 File Offset: 0x00095648
		private _IStatement \u0002(_IAssignmentExpression \u0002, _IVariable \u0003)
		{
			string text = string.Empty;
			_ICompoAccessExpression icompoAccessExpression = \u0002._LValue as _ICompoAccessExpression;
			if (icompoAccessExpression != null)
			{
				_IExpression left = icompoAccessExpression._Left;
				text = ((left != null) ? left.ToString() : null) + ".";
			}
			ISignature signature = this.Context._Scope[\u0002._LValue.SignatureId];
			string text2;
			if (signature != null && (signature.POUType == Operator.VarGlobal || signature.POUType == Operator.Program) && string.IsNullOrEmpty(text))
			{
				text2 = string.Concat(new string[]
				{
					signature.Name,
					".",
					text,
					"__get",
					\u0003.VersionedName,
					"()"
				});
			}
			else
			{
				text2 = text + "__get" + \u0003.VersionedName + "()";
			}
			_ILanguageModelBuilder ilanguageModelBuilder = \u0019.\u0003.Builder;
			IReferenceType referenceType = \u0003.Type as IReferenceType;
			if (referenceType == null || referenceType.Base.Class != TypeClass.Userdef)
			{
				_IExpression iexpression = this.Context.Generator.GenerateExpression(string.Format("{0} := {1}", text2, \u0002.RValue), this.Context._Scope, this.Context.CompiledPOU);
				this.Context.Generator.CopyPositionAndMessages(\u0002, iexpression);
				return ilanguageModelBuilder.CreateExpressionStatement(iexpression);
			}
			_ISignature isignature = this.\u0001();
			string text3 = string.Format("__tmpProp_{0}_{1}", isignature.NextId, \u0003.Name);
			if (!\u0019.\u0001.\u0001(this.Context.Comcon, isignature, null, text3, \u0002._LValue._CompiledType))
			{
				throw new InvalidOperationException();
			}
			Locator.\u0001(this.Context.Comcon.DataManager, this.Context.Comcon, null, isignature, null);
			bool flag;
			_IExpression exp = this.Context.Generator.\u0001(text3 + " REF= " + text2, out flag);
			IAssignmentExpression expInner = ilanguageModelBuilder.CreateAssignmentExpression(null, ilanguageModelBuilder.CreateVariableExpression(null, text3), \u0002.RValue);
			_IStatement istatement = this.Context.Generator.\u0001<_IStatement>((_IStatement)ilanguageModelBuilder.CreateSequenceStatementEx(null, new IExpressionStatement[]
			{
				ilanguageModelBuilder.CreateExpressionStatement(exp),
				ilanguageModelBuilder.CreateExpressionStatement(expInner)
			}), this.Context._Scope, this.Context.CompiledPOU);
			this.Context.Generator.CopyPositionAndMessages(\u0002, istatement);
			this.Context.Generator.DisableFlowBPForallExceptFirst(istatement, \u0002._Position);
			return istatement;
		}

		// Token: 0x06002AF8 RID: 11000 RVA: 0x00097708 File Offset: 0x00095908
		private _IStatement \u0003(_IAssignmentExpression \u0002, _IVariable \u0003)
		{
			string text = \u0003.GetAttributeValue(CompileAttributes.SET_ACCESS);
			text = text.Replace(\u0003.OrgName, \u0002._RValue.ToString());
			text = Helper.\u0001(text, \u0002._LValue);
			_IExpression iexpression = this.Context.Generator.GenerateExpression(text, this.Context._Scope, this.Context.CompiledPOU);
			this.Context.Generator.CopyPositionAndMessages(\u0002, iexpression);
			return \u0019.\u0003.Builder.CreateExpressionStatement(iexpression);
		}

		// Token: 0x06002AF9 RID: 11001 RVA: 0x00097798 File Offset: 0x00095998
		private _IStatement \u0004(_IAssignmentExpression \u0002, _IVariable \u0003)
		{
			string stInput = global::\u0014.\u0013.\u0001(\u0003, -1, \u0002._RValue.ToString());
			_IExpression iexpression = this.Context.Generator.GenerateExpression(stInput, this.Context._Scope, this.Context.CompiledPOU);
			this.Context.Generator.CopyPositionAndMessages(\u0002, iexpression);
			return \u0019.\u0003.Builder.CreateExpressionStatement(iexpression);
		}

		// Token: 0x04000821 RID: 2081
		[CompilerGenerated]
		private readonly global::\u000E.\u0011 \u0001;
	}
}
