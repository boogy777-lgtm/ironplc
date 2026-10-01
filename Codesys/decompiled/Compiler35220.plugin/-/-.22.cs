using System;
using System.Collections.Generic;
using \u0003;
using \u0019;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001F
{
	// Token: 0x02000074 RID: 116
	internal sealed class \u0002 : ITypeVisitor3, ITypeVisitor2, ITypeVisitor
	{
		// Token: 0x06000944 RID: 2372 RVA: 0x000128FC File Offset: 0x00010AFC
		internal \u0002(_ICompileContext \u0001\u0002, ISignature \u0002\u0002, ISourcePosition \u0003\u0002)
		{
			this.\u0001 = \u0001\u0002;
			this.\u0001 = (_ISignature)\u0002\u0002;
			this.\u0001 = \u0003\u0002;
			this.\u0001 = new Stack<TypeClass>();
		}

		// Token: 0x06000945 RID: 2373 RVA: 0x0001292C File Offset: 0x00010B2C
		public void \u0001(_IBitConstType \u0002)
		{
		}

		// Token: 0x06000946 RID: 2374 RVA: 0x00012930 File Offset: 0x00010B30
		public void \u0001(_IBitType \u0002)
		{
		}

		// Token: 0x06000947 RID: 2375 RVA: 0x00012934 File Offset: 0x00010B34
		public void \u0001(_IBoolType \u0002)
		{
		}

		// Token: 0x06000948 RID: 2376 RVA: 0x00012938 File Offset: 0x00010B38
		public void \u0001(_IByteType \u0002)
		{
		}

		// Token: 0x06000949 RID: 2377 RVA: 0x0001293C File Offset: 0x00010B3C
		public void \u0001(_ISIntType \u0002)
		{
		}

		// Token: 0x0600094A RID: 2378 RVA: 0x00012940 File Offset: 0x00010B40
		public void \u0001(_IUSIntType \u0002)
		{
		}

		// Token: 0x0600094B RID: 2379 RVA: 0x00012944 File Offset: 0x00010B44
		public void \u0001(_IIntType \u0002)
		{
		}

		// Token: 0x0600094C RID: 2380 RVA: 0x00012948 File Offset: 0x00010B48
		public void \u0001(_IUIntType \u0002)
		{
		}

		// Token: 0x0600094D RID: 2381 RVA: 0x0001294C File Offset: 0x00010B4C
		public void \u0001(_IWordType \u0002)
		{
		}

		// Token: 0x0600094E RID: 2382 RVA: 0x00012950 File Offset: 0x00010B50
		public void \u0001(_IDIntType \u0002)
		{
		}

		// Token: 0x0600094F RID: 2383 RVA: 0x00012954 File Offset: 0x00010B54
		public void \u0001(_IUDIntType \u0002)
		{
		}

		// Token: 0x06000950 RID: 2384 RVA: 0x00012958 File Offset: 0x00010B58
		public void \u0001(_IDWordType \u0002)
		{
		}

		// Token: 0x06000951 RID: 2385 RVA: 0x0001295C File Offset: 0x00010B5C
		public void \u0001(_ILIntType \u0002)
		{
		}

		// Token: 0x06000952 RID: 2386 RVA: 0x00012960 File Offset: 0x00010B60
		public void \u0001(_IULIntType \u0002)
		{
		}

		// Token: 0x06000953 RID: 2387 RVA: 0x00012964 File Offset: 0x00010B64
		public void \u0001(_ILWordType \u0002)
		{
		}

		// Token: 0x06000954 RID: 2388 RVA: 0x00012968 File Offset: 0x00010B68
		public void \u0001(_IRealType \u0002)
		{
		}

		// Token: 0x06000955 RID: 2389 RVA: 0x0001296C File Offset: 0x00010B6C
		public void \u0001(_ILRealType \u0002)
		{
		}

		// Token: 0x06000956 RID: 2390 RVA: 0x00012970 File Offset: 0x00010B70
		public void \u0001(_ILazyType \u0002)
		{
		}

		// Token: 0x06000957 RID: 2391 RVA: 0x00012974 File Offset: 0x00010B74
		public void \u0001(IImplicitEnumerationType \u0002)
		{
		}

		// Token: 0x06000958 RID: 2392 RVA: 0x00012978 File Offset: 0x00010B78
		public void \u0001(_IEnumType \u0002)
		{
		}

		// Token: 0x06000959 RID: 2393 RVA: 0x0001297C File Offset: 0x00010B7C
		public void \u0001(_IAnyType \u0002)
		{
		}

		// Token: 0x0600095A RID: 2394 RVA: 0x00012980 File Offset: 0x00010B80
		public void \u0001(_IAnyRealType \u0002)
		{
		}

		// Token: 0x0600095B RID: 2395 RVA: 0x00012984 File Offset: 0x00010B84
		public void \u0001(_IAnyIntType \u0002)
		{
		}

		// Token: 0x0600095C RID: 2396 RVA: 0x00012988 File Offset: 0x00010B88
		public void \u0001(_IAnyNumType \u0002)
		{
		}

		// Token: 0x0600095D RID: 2397 RVA: 0x0001298C File Offset: 0x00010B8C
		public void \u0001(_IAnyBitType \u0002)
		{
		}

		// Token: 0x0600095E RID: 2398 RVA: 0x00012990 File Offset: 0x00010B90
		public void \u0001(_IAnyDateType \u0002)
		{
		}

		// Token: 0x0600095F RID: 2399 RVA: 0x00012994 File Offset: 0x00010B94
		public void \u0001(_IAnyStringType \u0002)
		{
		}

		// Token: 0x06000960 RID: 2400 RVA: 0x00012998 File Offset: 0x00010B98
		public void \u0001(_IAnyBitButBoolIsPreferred \u0002)
		{
		}

		// Token: 0x06000961 RID: 2401 RVA: 0x0001299C File Offset: 0x00010B9C
		public void \u0001(_IDateType \u0002)
		{
		}

		// Token: 0x06000962 RID: 2402 RVA: 0x000129A0 File Offset: 0x00010BA0
		public void \u0001(_ITimeOfDayType \u0002)
		{
		}

		// Token: 0x06000963 RID: 2403 RVA: 0x000129A4 File Offset: 0x00010BA4
		public void \u0001(_IDateAndTimeType \u0002)
		{
		}

		// Token: 0x06000964 RID: 2404 RVA: 0x000129A8 File Offset: 0x00010BA8
		public void \u0001(_ILDateType \u0002)
		{
		}

		// Token: 0x06000965 RID: 2405 RVA: 0x000129AC File Offset: 0x00010BAC
		public void \u0001(_ILTimeOfDayType \u0002)
		{
		}

		// Token: 0x06000966 RID: 2406 RVA: 0x000129B0 File Offset: 0x00010BB0
		public void \u0001(_ILDateAndTimeType \u0002)
		{
		}

		// Token: 0x06000967 RID: 2407 RVA: 0x000129B4 File Offset: 0x00010BB4
		public void \u0001(_ITimeType \u0002)
		{
		}

		// Token: 0x06000968 RID: 2408 RVA: 0x000129B8 File Offset: 0x00010BB8
		public void \u0001(_ILTimeType \u0002)
		{
		}

		// Token: 0x06000969 RID: 2409 RVA: 0x000129BC File Offset: 0x00010BBC
		public void \u0001(_IXIntType \u0002)
		{
		}

		// Token: 0x0600096A RID: 2410 RVA: 0x000129C0 File Offset: 0x00010BC0
		public void \u0001(_IXDIntType \u0002)
		{
		}

		// Token: 0x0600096B RID: 2411 RVA: 0x000129C4 File Offset: 0x00010BC4
		public void \u0001(_IXLIntType \u0002)
		{
		}

		// Token: 0x0600096C RID: 2412 RVA: 0x000129C8 File Offset: 0x00010BC8
		public void \u0001(_IXWordType \u0002)
		{
		}

		// Token: 0x0600096D RID: 2413 RVA: 0x000129CC File Offset: 0x00010BCC
		public void \u0001(_IXDWordType \u0002)
		{
		}

		// Token: 0x0600096E RID: 2414 RVA: 0x000129D0 File Offset: 0x00010BD0
		public void \u0001(_IXLWordType \u0002)
		{
		}

		// Token: 0x0600096F RID: 2415 RVA: 0x000129D4 File Offset: 0x00010BD4
		public void \u0001(_IXUDIntType \u0002)
		{
		}

		// Token: 0x06000970 RID: 2416 RVA: 0x000129D8 File Offset: 0x00010BD8
		public void \u0001(_IXULIntType \u0002)
		{
		}

		// Token: 0x06000971 RID: 2417 RVA: 0x000129DC File Offset: 0x00010BDC
		public void \u0001(_IUXIntType \u0002)
		{
		}

		// Token: 0x06000972 RID: 2418 RVA: 0x000129E0 File Offset: 0x00010BE0
		public void \u0001(_IXStringType \u0002)
		{
		}

		// Token: 0x06000973 RID: 2419 RVA: 0x000129E4 File Offset: 0x00010BE4
		public void \u0001(_IParamsType \u0002)
		{
		}

		// Token: 0x06000974 RID: 2420 RVA: 0x000129E8 File Offset: 0x00010BE8
		public void \u0001(_IStringType \u0002)
		{
		}

		// Token: 0x06000975 RID: 2421 RVA: 0x000129EC File Offset: 0x00010BEC
		public void \u0001(_IWStringType \u0002)
		{
		}

		// Token: 0x06000976 RID: 2422 RVA: 0x000129F0 File Offset: 0x00010BF0
		public void \u0001(_IVectorType \u0002)
		{
		}

		// Token: 0x06000977 RID: 2423 RVA: 0x000129F4 File Offset: 0x00010BF4
		public void \u0001(_IUserdefType \u0002)
		{
			_ISignature isignature = this.\u0001.GetSignatureById(\u0002.SignatureId) as _ISignature;
			if (isignature != null && isignature.GetFlag(SignatureFlag.Abstract))
			{
				bool flag = false;
				if (0 < this.\u0001.Count)
				{
					TypeClass typeClass = this.\u0001.Peek();
					flag = (TypeClass.Pointer == typeClass || TypeClass.Reference == typeClass);
				}
				if (!flag)
				{
					this.\u0001.AddMessage(\u0019.\u0003.\u0001(this.\u0001, global::\u0003.\u0006.\u0001(MessageId.Err_AbstractFunctionBlockInstance, new object[]
					{
						\u0002.NameExpression.ToString(),
						Scanner.GetTextOfOperator(isignature.POUType)
					}), Severity.Error, MessageId.Err_AbstractFunctionBlockInstance));
				}
			}
		}

		// Token: 0x06000978 RID: 2424 RVA: 0x00012AA4 File Offset: 0x00010CA4
		public void \u0001(_IAliasType \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06000979 RID: 2425 RVA: 0x00012AB0 File Offset: 0x00010CB0
		public void \u0001(_IPointerType \u0002)
		{
			this.\u0001.Push(TypeClass.Pointer);
			\u0002._Base.Accept(this);
			this.\u0001.Pop();
		}

		// Token: 0x0600097A RID: 2426 RVA: 0x00012AD8 File Offset: 0x00010CD8
		public void \u0001(_IReferenceType \u0002)
		{
			this.\u0001.Push(TypeClass.Reference);
			\u0002._Base.Accept(this);
			this.\u0001.Pop();
		}

		// Token: 0x0600097B RID: 2427 RVA: 0x00012B00 File Offset: 0x00010D00
		public void \u0001(_ISubrangeType \u0002)
		{
			this.\u0001.Push(TypeClass.Subrange);
			\u0002._Base.Accept(this);
			this.\u0001.Pop();
		}

		// Token: 0x0600097C RID: 2428 RVA: 0x00012B28 File Offset: 0x00010D28
		public void \u0001(_IVariableLengthArrayType \u0002)
		{
			this.\u0001.Push(TypeClass.VarLenArray);
			\u0002._Base.Accept(this);
			this.\u0001.Pop();
		}

		// Token: 0x0600097D RID: 2429 RVA: 0x00012B50 File Offset: 0x00010D50
		public void \u0001(_IArrayType \u0002)
		{
			this.\u0001.Push(TypeClass.Array);
			\u0002._Base.Accept(this);
			this.\u0001.Pop();
		}

		// Token: 0x04000133 RID: 307
		private _ICompileContext \u0001;

		// Token: 0x04000134 RID: 308
		private _ISignature \u0001;

		// Token: 0x04000135 RID: 309
		private ISourcePosition \u0001;

		// Token: 0x04000136 RID: 310
		private Stack<TypeClass> \u0001;
	}
}
