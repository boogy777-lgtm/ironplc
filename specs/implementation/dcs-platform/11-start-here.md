# 11. Как передавать задания следующей LLM

Первый запрос — W01, затем W02/W04 и узкая регрессия W05. Не передавать 44 реализации одним неделимым заданием. Общие documents задают границы; конкретная карточка задаёт результат текущей итерации.

## Запрос для начала

```text
Репозиторий: boogy777-lgtm/ironplc.
Норма: specs/design/dcs-plc-production-platform-spec-ru.md, revision 3.0.
Implementation dossier: specs/implementation/dcs-platform/README.md.
Сначала выполни tasks/W01-baseline.md. Сверь текущий commit с source baseline
0fbc8e4fdb192c8dcb9be30a9ac5494c244ce9d3; не используй main по умолчанию.
Прочитай AGENTS.md, applicable steering и ironplc-dev перед compiler/**.
Результат: baseline/conflict map, boundaries, scoped ADR migration obligations
и проверка комплектности, без заявления, что platform уже реализована.
Не меняй нормативные инварианты ради совпадения с legacy code.
Исполняй repository PR workflow; передавай конкретный reviewable результат.
```

## Запрос на реализацию конкретного work package

```text
Выполни <W-ID> из specs/implementation/dcs-platform/tasks/.
Прочитай README, 02-modular-framework, 03-contracts-and-state,
05-evidence-and-llm-contract и саму карточку. Затем — только нужные § нормы.
Проверь, что predecessors доступны и совместимы; уточни их contract versions.
1. Зафиксируй source/spec SHAs, scope, affected owners и regression oracle.
2. По правилам repository подготовь contract/prefactor/implementation PRs.
3. Реализуй минимальный механизм, используя существующие crates и safe APIs.
4. Проверь W-A критерии и назначенные T/REQ/INV с правильным METHOD.
5. Заполни templates/evidence-report.md и templates/review.md.
6. Покажи actual commands/results, artifacts/hashes, N+1 diff и blockers.
Не считай compile/simulation доказательством physical timing/fencing/durability.
Не ослабляй gates и не создавай фиктивные passing tests. UNKNOWN остаётся UNKNOWN.
Продолжай доступную работу при hardware blocker, но qualification не объявляй.
```

## Размер выдаваемого контекста

Держать в active context: W-карточку, contract record owners, нужные § спецификации и relevant code. Полную traceability matrix использовать для проверки покрытия, а не копировать в каждый исходник. Review/implementation могут идти разными сессиями LLM; контекст handoff должен содержать точный commit и evidence, а не память предыдущего чата.

Это описание будущего процесса исполнения. Оно само по себе не запускает implementation agents и не разрешает merge/release без действующего repository workflow.
