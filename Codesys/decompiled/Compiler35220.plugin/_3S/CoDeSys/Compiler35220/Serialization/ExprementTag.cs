using System;

namespace _3S.CoDeSys.Compiler35220.Serialization
{
	// Token: 0x0200008E RID: 142
	internal enum ExprementTag : uint
	{
		// Token: 0x040001BD RID: 445
		NULL,
		// Token: 0x040001BE RID: 446
		ErrorStatement,
		// Token: 0x040001BF RID: 447
		EmptyStatement,
		// Token: 0x040001C0 RID: 448
		WhileStatement,
		// Token: 0x040001C1 RID: 449
		RepeatStatement,
		// Token: 0x040001C2 RID: 450
		CaseRangeExpression,
		// Token: 0x040001C3 RID: 451
		CaseLabelStatement,
		// Token: 0x040001C4 RID: 452
		Case2,
		// Token: 0x040001C5 RID: 453
		CaseStatement,
		// Token: 0x040001C6 RID: 454
		ForStatement,
		// Token: 0x040001C7 RID: 455
		ExitStatement,
		// Token: 0x040001C8 RID: 456
		ContinueStatement,
		// Token: 0x040001C9 RID: 457
		SequenceStatement,
		// Token: 0x040001CA RID: 458
		AssignmentExpression,
		// Token: 0x040001CB RID: 459
		ElseIf,
		// Token: 0x040001CC RID: 460
		IfStatement,
		// Token: 0x040001CD RID: 461
		TryCatchStatement,
		// Token: 0x040001CE RID: 462
		ReturnStatement,
		// Token: 0x040001CF RID: 463
		JumpStatement,
		// Token: 0x040001D0 RID: 464
		LabelStatement,
		// Token: 0x040001D1 RID: 465
		CommentStatement,
		// Token: 0x040001D2 RID: 466
		PragmaStatement,
		// Token: 0x040001D3 RID: 467
		MessageGuidPragmaStatement,
		// Token: 0x040001D4 RID: 468
		WarningDisableRestorePragmaStatement,
		// Token: 0x040001D5 RID: 469
		ExpressionStatement,
		// Token: 0x040001D6 RID: 470
		ErrorExpression,
		// Token: 0x040001D7 RID: 471
		NamespaceAccessExpression,
		// Token: 0x040001D8 RID: 472
		CallExpression,
		// Token: 0x040001D9 RID: 473
		OperatorExpression,
		// Token: 0x040001DA RID: 474
		ConversionExpression,
		// Token: 0x040001DB RID: 475
		NewExpression,
		// Token: 0x040001DC RID: 476
		CastExpression,
		// Token: 0x040001DD RID: 477
		ThisExpression,
		// Token: 0x040001DE RID: 478
		BaseExpression,
		// Token: 0x040001DF RID: 479
		LiteralExpression,
		// Token: 0x040001E0 RID: 480
		IntegerLiteralExpression,
		// Token: 0x040001E1 RID: 481
		BasedIntegerLiteralExpression,
		// Token: 0x040001E2 RID: 482
		StringLiteralExpression,
		// Token: 0x040001E3 RID: 483
		FloatLiteralExpression,
		// Token: 0x040001E4 RID: 484
		TypeExpression,
		// Token: 0x040001E5 RID: 485
		AddressExpression,
		// Token: 0x040001E6 RID: 486
		VariableExpression,
		// Token: 0x040001E7 RID: 487
		IndexAccessExpression,
		// Token: 0x040001E8 RID: 488
		CompoAccessExpression,
		// Token: 0x040001E9 RID: 489
		DeRefAccessExpression,
		// Token: 0x040001EA RID: 490
		GlobalScopeExpression,
		// Token: 0x040001EB RID: 491
		SystemScopeExpression,
		// Token: 0x040001EC RID: 492
		PoolScopeExpression,
		// Token: 0x040001ED RID: 493
		CurrentTaskExpression,
		// Token: 0x040001EE RID: 494
		NullExpression,
		// Token: 0x040001EF RID: 495
		NullStatement,
		// Token: 0x040001F0 RID: 496
		HasValueExpression,
		// Token: 0x040001F1 RID: 497
		HasConstantValueExpression,
		// Token: 0x040001F2 RID: 498
		PragmaOperatorExpression,
		// Token: 0x040001F3 RID: 499
		PragmaAssertion2,
		// Token: 0x040001F4 RID: 500
		PragmaElseIf,
		// Token: 0x040001F5 RID: 501
		DefineStatement,
		// Token: 0x040001F6 RID: 502
		ArrayInitialization,
		// Token: 0x040001F7 RID: 503
		StructureInitialization,
		// Token: 0x040001F8 RID: 504
		MultipleIndexInitialization,
		// Token: 0x040001F9 RID: 505
		DefineReference,
		// Token: 0x040001FA RID: 506
		VariableReference,
		// Token: 0x040001FB RID: 507
		TypeReference,
		// Token: 0x040001FC RID: 508
		PouReference,
		// Token: 0x040001FD RID: 509
		TaskReference,
		// Token: 0x040001FE RID: 510
		ResourceReference,
		// Token: 0x040001FF RID: 511
		DefinedExpression,
		// Token: 0x04000200 RID: 512
		XRefExpression,
		// Token: 0x04000201 RID: 513
		CompilerVersionExpression,
		// Token: 0x04000202 RID: 514
		PragmaIfStatement,
		// Token: 0x04000203 RID: 515
		HasCompatibleTypeExpression,
		// Token: 0x04000204 RID: 516
		HasTypeExpression,
		// Token: 0x04000205 RID: 517
		IsEnumTypeExpression,
		// Token: 0x04000206 RID: 518
		HasAttributeExpression,
		// Token: 0x04000207 RID: 519
		RuntimeVersionExpression,
		// Token: 0x04000208 RID: 520
		HasConstantTypeExpression,
		// Token: 0x04000209 RID: 521
		PartialAccessExpression,
		// Token: 0x0400020A RID: 522
		ProjectDefinedExpression,
		// Token: 0x0400020B RID: 523
		TableContent,
		// Token: 0x0400020C RID: 524
		ImplicitConversionExpression,
		// Token: 0x0400020D RID: 525
		ImplicitCodeSectionPragmaStatement,
		// Token: 0x0400020E RID: 526
		LocalSignatureIdPragmaStatement
	}
}
