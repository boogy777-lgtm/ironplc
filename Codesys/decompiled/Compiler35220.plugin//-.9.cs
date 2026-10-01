using System;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0007;
using \u0011;
using \u0014;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0083
{
	// Token: 0x0200034D RID: 845
	internal sealed class \u000E
	{
		// Token: 0x1700085C RID: 2140
		// (get) Token: 0x060032F8 RID: 13048 RVA: 0x000C5118 File Offset: 0x000C3318
		private _ISignature Signature { get; }

		// Token: 0x1700085D RID: 2141
		// (get) Token: 0x060032F9 RID: 13049 RVA: 0x000C5120 File Offset: 0x000C3320
		private _ICompileContext Comcon { get; }

		// Token: 0x1700085E RID: 2142
		// (get) Token: 0x060032FA RID: 13050 RVA: 0x000C5128 File Offset: 0x000C3328
		private _IVariable Variable { get; }

		// Token: 0x1700085F RID: 2143
		// (get) Token: 0x060032FB RID: 13051 RVA: 0x000C5130 File Offset: 0x000C3330
		private ICompiledType3 VariableType { get; }

		// Token: 0x17000860 RID: 2144
		// (get) Token: 0x060032FC RID: 13052 RVA: 0x000C5138 File Offset: 0x000C3338
		private _ISignature VariableTypeSignature { get; }

		// Token: 0x060032FD RID: 13053 RVA: 0x000C5140 File Offset: 0x000C3340
		internal \u000E(_ISignature \u001C\u0002, _ICompileContext \u0001\u0002, _IVariable \u001A\u0002, ICompiledType3 \u0084\u0005, _ISignature \u0086\u0005)
		{
			this.Signature = \u001C\u0002;
			this.Comcon = \u0001\u0002;
			this.Variable = \u001A\u0002;
			this.VariableType = \u0084\u0005;
			this.VariableTypeSignature = \u0086\u0005;
		}

		// Token: 0x060032FE RID: 13054 RVA: 0x000C5170 File Offset: 0x000C3370
		internal void \u0001(IScope5 \u0002)
		{
			this.\u0002(\u0002);
			this.\u0003(\u0002);
			this.\u0004(\u0002);
			this.\u0003();
			this.\u0002();
			this.\u0001();
		}

		// Token: 0x060032FF RID: 13055 RVA: 0x000C519C File Offset: 0x000C339C
		private void \u0001()
		{
			if (this.Variable.Type.Class == TypeClass.Pointer)
			{
				if (((_IPointerType)this.Variable.Type).BaseType.Class == TypeClass.Bit)
				{
					\u0083.\u000E.\u0001(this.Signature, this.Variable.SourcePosition, Severity.Error, MessageId.Err_NoPointerToBit);
					return;
				}
			}
			else if (this.Variable.Type.Class == TypeClass.Reference)
			{
				if (((_IReferenceType)this.Variable.Type).BaseType.Class == TypeClass.Bit)
				{
					\u0083.\u000E.\u0001(this.Signature, this.Variable.SourcePosition, Severity.Error, MessageId.Err_NoReferenceToBits);
					return;
				}
			}
			else if (this.Variable.Type.Class == TypeClass.Array && ((_IArrayType)this.Variable.Type).BaseType.Class == TypeClass.Bit)
			{
				\u0083.\u000E.\u0001(this.Signature, this.Variable.SourcePosition, Severity.Error, MessageId.Err_NoArrayOfBit);
			}
		}

		// Token: 0x06003300 RID: 13056 RVA: 0x000C5298 File Offset: 0x000C3498
		private void \u0002(IScope5 \u0002)
		{
			if (this.Variable.HasAttribute(CompileAttributes.DEVICE_PARAMETER))
			{
				for (int i = 0; i < 2; i++)
				{
					string u0087_u;
					if (i == 0)
					{
						u0087_u = global::\u0014.\u0013.\u0001(this.Variable, -1);
					}
					else
					{
						if (this.Variable.HasAttribute(CompileAttributes.ATTRIBUTE_READ_ONLY))
						{
							break;
						}
						string name = this.Variable.Name;
						u0087_u = global::\u0014.\u0013.\u0001(this.Variable, -1, name);
					}
					bool flag;
					_IExpression iexpression = new global::\u0011.\u0006(u0087_u, false).\u0002(out flag);
					ExpressionTypifierWithSpecialTasks expressionTypifierWithSpecialTasks = new ExpressionTypifierWithSpecialTasks(\u0002 as global::\u0007.\u0005, this.Comcon, false, null);
					expressionTypifierWithSpecialTasks.ContributeToCompile = true;
					TypeCheckerVisitor ivisit = new TypeCheckerVisitor(\u0002 as global::\u0007.\u0005, this.Comcon);
					iexpression.Accept(expressionTypifierWithSpecialTasks);
					iexpression.Accept(ivisit);
					ErrorVisitor errorVisitor = new ErrorVisitor();
					global::\u0014.\u0013.\u0001(this.Variable, errorVisitor);
					iexpression.Accept(errorVisitor);
					foreach (_ICompilerMessage icompilerMessage in errorVisitor.MessageList)
					{
						this.Signature.AddMessageString(this.Variable.SourcePosition as _ISourcePosition, icompilerMessage.Severity, icompilerMessage.Text, Array.Empty<object>());
					}
				}
			}
		}

		// Token: 0x06003301 RID: 13057 RVA: 0x000C53E4 File Offset: 0x000C35E4
		private void \u0002()
		{
			if (!this.Variable.GetFlag(VarFlag.ImplicitParamsStruct) || this.VariableTypeSignature == null)
			{
				return;
			}
			_ISignature isignature = this.VariableTypeSignature;
			foreach (_ICompilerMessage icompilerMessage in isignature.Messages.OfType<_ICompilerMessage>())
			{
				this.Signature.AddMessage(this.Signature.CreateCompilerMessage(this.Variable.SourcePosition, icompilerMessage.Text, icompilerMessage.Severity, (int)icompilerMessage.MessageId));
			}
			isignature.AddAttribute(CompileAttributes.ATTRIBUTE_SUPPRESS_MESSAGES, null);
		}

		// Token: 0x06003302 RID: 13058 RVA: 0x000C5494 File Offset: 0x000C3694
		private void \u0003(IScope5 \u0002)
		{
			if (!this.Variable.HasAttribute(CompileAttributes.SET_ACCESS))
			{
				return;
			}
			bool flag;
			_IExpression iexpression = new global::\u0011.\u0006(Helper.\u0001(this.Variable.GetAttributeValue(CompileAttributes.SET_ACCESS), null), false).\u0002(out flag);
			ExpressionTypifierWithSpecialTasks expressionTypifierWithSpecialTasks = new ExpressionTypifierWithSpecialTasks(\u0002 as global::\u0007.\u0005, this.Comcon, false, null);
			expressionTypifierWithSpecialTasks.ContributeToCompile = true;
			TypeCheckerVisitor ivisit = new TypeCheckerVisitor(\u0002 as global::\u0007.\u0005, this.Comcon);
			iexpression.Accept(expressionTypifierWithSpecialTasks);
			iexpression.Accept(ivisit);
			ErrorVisitor errorVisitor = new ErrorVisitor();
			global::\u0014.\u0013.\u0001(this.Variable, errorVisitor);
			iexpression.Accept(errorVisitor);
			foreach (_ICompilerMessage icompilerMessage in errorVisitor.MessageList)
			{
				this.Signature.AddMessageString(this.Variable.SourcePosition as _ISourcePosition, icompilerMessage.Severity, icompilerMessage.Text, Array.Empty<object>());
			}
		}

		// Token: 0x06003303 RID: 13059 RVA: 0x000C5598 File Offset: 0x000C3798
		private void \u0004(IScope5 \u0002)
		{
			if (!this.Variable.HasAttribute(CompileAttributes.GET_ACCESS))
			{
				return;
			}
			bool flag;
			_IExpression iexpression = new global::\u0011.\u0006(Helper.\u0001(this.Variable.GetAttributeValue(CompileAttributes.GET_ACCESS), null), false).\u0002(out flag);
			ExpressionTypifierWithSpecialTasks expressionTypifierWithSpecialTasks = new ExpressionTypifierWithSpecialTasks(\u0002 as global::\u0007.\u0005, this.Comcon, false, null);
			expressionTypifierWithSpecialTasks.ContributeToCompile = true;
			TypeCheckerVisitor ivisit = new TypeCheckerVisitor(\u0002 as global::\u0007.\u0005, this.Comcon);
			iexpression.Accept(expressionTypifierWithSpecialTasks);
			iexpression.Accept(ivisit);
			ErrorVisitor errorVisitor = new ErrorVisitor();
			global::\u0014.\u0013.\u0001(this.Variable, errorVisitor);
			iexpression.Accept(errorVisitor);
			foreach (_ICompilerMessage icompilerMessage in errorVisitor.MessageList)
			{
				this.Signature.AddMessageString(this.Variable.SourcePosition as _ISourcePosition, icompilerMessage.Severity, icompilerMessage.Text, Array.Empty<object>());
			}
		}

		// Token: 0x06003304 RID: 13060 RVA: 0x000C569C File Offset: 0x000C389C
		private void \u0003()
		{
			string attributeValue = this.Variable.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING);
			if (string.IsNullOrEmpty(attributeValue) || attributeValue != CompileAttributes.ATTRIBUTEVALUE_CALL)
			{
				return;
			}
			bool flag = false;
			ISignature signature = this.VariableTypeSignature;
			ICompiledType effectiveType = this.VariableType.EffectiveType;
			if (effectiveType.Class == TypeClass.Userdef && signature != null && !signature.GetFlag(SignatureFlag.Enum) && signature.POUType != Operator.Interface)
			{
				flag = true;
			}
			if (effectiveType.Class == TypeClass.Array)
			{
				flag = true;
			}
			if (flag && this.Signature.POUType != Operator.Method)
			{
				this.Signature.\u0001(this.Variable.SourcePosition, Messages.\u0001(this.Variable, MessageId.Wrn_StruturedTypePropertyNotMonitorable), MessageId.Wrn_StruturedTypePropertyNotMonitorable, new object[]
				{
					this.Variable.OrgName
				});
			}
		}

		// Token: 0x06003305 RID: 13061 RVA: 0x000C5764 File Offset: 0x000C3964
		private static void \u0001(_ISignature \u0002, ISourcePosition \u0003, Severity \u0004, MessageId \u0005)
		{
			foreach (_ICompilerMessage icompilerMessage in \u0002.GetMessages(true))
			{
				if (icompilerMessage.ObjectGuid == \u0002.MessageGuid && icompilerMessage.Position == \u0003.Position && icompilerMessage.MessageId == \u0005)
				{
					return;
				}
			}
			\u0002.\u0001(\u0003, \u0004, \u0005, Array.Empty<object>());
		}

		// Token: 0x040009A0 RID: 2464
		[CompilerGenerated]
		private readonly _ISignature \u0001;

		// Token: 0x040009A1 RID: 2465
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x040009A2 RID: 2466
		[CompilerGenerated]
		private readonly _IVariable \u0001;

		// Token: 0x040009A3 RID: 2467
		[CompilerGenerated]
		private readonly ICompiledType3 \u0001;

		// Token: 0x040009A4 RID: 2468
		[CompilerGenerated]
		private readonly _ISignature \u0002;
	}
}
