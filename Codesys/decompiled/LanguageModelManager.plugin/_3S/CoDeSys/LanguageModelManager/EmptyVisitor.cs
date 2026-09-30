using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000E1 RID: 225
	public abstract class EmptyVisitor : IExprementVisitorNoTraversion
	{
		// Token: 0x170004D4 RID: 1236
		// (get) Token: 0x060010C0 RID: 4288 RVA: 0x00030F3B File Offset: 0x0002FF3B
		// (set) Token: 0x060010C1 RID: 4289 RVA: 0x00030F43 File Offset: 0x0002FF43
		public IStandardTraverser Traverser { get; set; }

		// Token: 0x060010C2 RID: 4290 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IVariableExpression variable, AccessFlag access)
		{
		}

		// Token: 0x060010C3 RID: 4291 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ICompiledPOU cpou)
		{
		}

		// Token: 0x060010C4 RID: 4292 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IWhileStatement whilst)
		{
		}

		// Token: 0x060010C5 RID: 4293 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IRepeatStatement repeat)
		{
		}

		// Token: 0x060010C6 RID: 4294 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IForStatement forloop)
		{
		}

		// Token: 0x060010C7 RID: 4295 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IExitStatement exit)
		{
		}

		// Token: 0x060010C8 RID: 4296 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IContinueStatement cont)
		{
		}

		// Token: 0x060010C9 RID: 4297 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ISequenceStatement seq)
		{
		}

		// Token: 0x060010CA RID: 4298 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IAssignmentExpression assign)
		{
		}

		// Token: 0x060010CB RID: 4299 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IIfStatement ifst)
		{
		}

		// Token: 0x060010CC RID: 4300 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IReturnStatement returnst)
		{
		}

		// Token: 0x060010CD RID: 4301 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IJumpStatement gotost)
		{
		}

		// Token: 0x060010CE RID: 4302 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ILabelStatement label)
		{
		}

		// Token: 0x060010CF RID: 4303 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ICommentStatement comment)
		{
		}

		// Token: 0x060010D0 RID: 4304 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IPragmaStatement pragma)
		{
		}

		// Token: 0x060010D1 RID: 4305 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IExpressionStatement expstat)
		{
		}

		// Token: 0x060010D2 RID: 4306 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ICallExpression call)
		{
		}

		// Token: 0x060010D3 RID: 4307 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IOperatorExpression op)
		{
		}

		// Token: 0x060010D4 RID: 4308 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ICastExpression cast)
		{
		}

		// Token: 0x060010D5 RID: 4309 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_INewExpression newexp)
		{
		}

		// Token: 0x060010D6 RID: 4310 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ITypeExpression newexp)
		{
		}

		// Token: 0x060010D7 RID: 4311 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IConversionExpression conv)
		{
		}

		// Token: 0x060010D8 RID: 4312 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IThisExpression thisexp)
		{
		}

		// Token: 0x060010D9 RID: 4313 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IBaseExpression baseexp)
		{
		}

		// Token: 0x060010DA RID: 4314 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ILiteralExpression literal)
		{
		}

		// Token: 0x060010DB RID: 4315 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IAddressExpression address)
		{
		}

		// Token: 0x060010DC RID: 4316 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IIndexAccessExpression indexaccess)
		{
		}

		// Token: 0x060010DD RID: 4317 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ICompoAccessExpression compo, AccessFlag access)
		{
		}

		// Token: 0x060010DE RID: 4318 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IDeRefAccessExpression deref)
		{
		}

		// Token: 0x060010DF RID: 4319 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ICopyScopeExpression copyexp)
		{
		}

		// Token: 0x060010E0 RID: 4320 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IGlobalScopeExpression globexp)
		{
		}

		// Token: 0x060010E1 RID: 4321 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ISystemScopeExpression systemscope)
		{
		}

		// Token: 0x060010E2 RID: 4322 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IEmptyStatement empty)
		{
		}

		// Token: 0x060010E3 RID: 4323 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ICaseRangeExpression caserange)
		{
		}

		// Token: 0x060010E4 RID: 4324 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ICaseLabelStatement caselabel)
		{
		}

		// Token: 0x060010E5 RID: 4325 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ICaseStatement casest)
		{
		}

		// Token: 0x060010E6 RID: 4326 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IErrorExpression errorexp)
		{
		}

		// Token: 0x060010E7 RID: 4327 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IErrorStatement errorst)
		{
		}

		// Token: 0x060010E8 RID: 4328 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_INullExpression errorexp)
		{
		}

		// Token: 0x060010E9 RID: 4329 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_INullStatement errorst)
		{
		}

		// Token: 0x060010EA RID: 4330 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IQualifiedNameExpression qne)
		{
		}

		// Token: 0x060010EB RID: 4331 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IVariableDeclarationStatement vds)
		{
		}

		// Token: 0x060010EC RID: 4332 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IVariableDeclarationListStatement vdls)
		{
		}

		// Token: 0x060010ED RID: 4333 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IPOUDeclarationStatement pds)
		{
		}

		// Token: 0x060010EE RID: 4334 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ITypeDeclarationStatement tds)
		{
		}

		// Token: 0x060010EF RID: 4335 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IEnumDeclarationStatement eds)
		{
		}

		// Token: 0x060010F0 RID: 4336 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IEnumDeclarationListStatement eds)
		{
		}

		// Token: 0x060010F1 RID: 4337 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IMultipleIndexInitialization errorst)
		{
		}

		// Token: 0x060010F2 RID: 4338 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IArrayInitialization errorexp)
		{
		}

		// Token: 0x060010F3 RID: 4339 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IStructureInitialization errorst)
		{
		}

		// Token: 0x060010F4 RID: 4340 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IDefineReference defref)
		{
		}

		// Token: 0x060010F5 RID: 4341 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IVariableReference varref)
		{
		}

		// Token: 0x060010F6 RID: 4342 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ITypeReference typeref)
		{
		}

		// Token: 0x060010F7 RID: 4343 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IPouReference pouref)
		{
		}

		// Token: 0x060010F8 RID: 4344 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ITaskReference taskref)
		{
		}

		// Token: 0x060010F9 RID: 4345 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IResourceReference resref)
		{
		}

		// Token: 0x060010FA RID: 4346 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IDefinedExpression defexp)
		{
		}

		// Token: 0x060010FB RID: 4347 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IPragmaOperatorExpression popexp)
		{
		}

		// Token: 0x060010FC RID: 4348 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IPragmaIfStatement pifst)
		{
		}

		// Token: 0x060010FD RID: 4349 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IBreakPointStatement bpstate)
		{
		}

		// Token: 0x060010FE RID: 4350 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IDefineStatement defstate)
		{
		}

		// Token: 0x060010FF RID: 4351 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IXRefExpression xref)
		{
		}

		// Token: 0x06001100 RID: 4352 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IHasTypeExpression hastype)
		{
		}

		// Token: 0x06001101 RID: 4353 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IIsEnumTypeExpression isenumtype)
		{
		}

		// Token: 0x06001102 RID: 4354 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IHasAttributeExpression hasattribute)
		{
		}

		// Token: 0x06001103 RID: 4355 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IHasValueExpression hasvalue)
		{
		}

		// Token: 0x06001104 RID: 4356 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IHasConstantValueExpression hasvalue)
		{
		}

		// Token: 0x06001105 RID: 4357 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_IPragmaAssertion assertion)
		{
		}

		// Token: 0x06001106 RID: 4358 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ICompilerVersionExpression compversion)
		{
		}

		// Token: 0x06001107 RID: 4359 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public virtual void visit(_ITryCatchStatement trycatch)
		{
		}

		// Token: 0x170004D5 RID: 1237
		// (get) Token: 0x06001108 RID: 4360 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool DoCallExpression
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170004D6 RID: 1238
		// (get) Token: 0x06001109 RID: 4361 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool DoAssignExpression
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170004D7 RID: 1239
		// (get) Token: 0x0600110A RID: 4362 RVA: 0x00005E58 File Offset: 0x00004E58
		public virtual bool bResolveCompoAccessExpression
		{
			get
			{
				return true;
			}
		}
	}
}
