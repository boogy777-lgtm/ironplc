"""Verification of the AST map coverage.

Prints how many interface rows exist, how many have a concrete red class, a
green twin, a red factory method and a parser producer, and *why* the others
are still empty.

Usage:  python tools\\verify_ast_map.py
"""
import csv, io, os, collections

ROOT = r"C:\Codesys"
AST = os.path.join(ROOT, "tables", "ast_nodes.csv")
RED = os.path.join(ROOT, "tables", "red_tree_nodes.csv")
AST_KINDS = ("statement", "expression", "type")


def read(path):
    with io.open(path, "r", encoding="utf-8-sig", newline="") as f:
        return list(csv.reader(f))


def main():
    rows = read(AST)
    head, data = rows[0], rows[1:]
    i = {k: n for n, k in enumerate(head)}

    total = len(data)
    kind = collections.Counter(r[i["kind"]] for r in data)
    have = {c: sum(1 for r in data if r[i[c]]) for c in
            ("concrete_impl", "green_impl", "red_factory_method", "producer")}

    print("=" * 62)
    print("AST map verification")
    print("=" * 62)
    print("ast_nodes rows          : %d" % total)
    print("  by kind               : " + ", ".join("%s=%d" % (k, kind[k]) for k in sorted(kind)))
    for c in ("concrete_impl", "green_impl", "red_factory_method", "producer"):
        print("  %-21s : %4d  (%5.1f%%)" % (c, have[c], 100.0 * have[c] / total))

    # producer split for AST nodes
    ast = [r for r in data if r[i["kind"]] in AST_KINDS]
    parser = [r for r in ast if r[i["producer"]] and not r[i["producer"]].startswith("нет")]
    typing = [r for r in ast if r[i["producer"]].startswith("нет")]
    print("AST nodes (stmt/expr/type) : %d  (concrete=%d, factory=%d, producer=%d)"
          % (len(ast),
             sum(1 for r in ast if r[i["concrete_impl"]]),
             sum(1 for r in ast if r[i["red_factory_method"]]),
             len(parser) + len(typing)))
    print("  producer: parser method  : %d" % len(parser))
    print("  producer: typing/decl    : %d" % len(typing))

    no_concrete = [r for r in data if not r[i["concrete_impl"]]]
    print("rows without concrete_impl : %d" % len(no_concrete))
    reasons = collections.Counter(r[i["note"]] for r in no_concrete)
    for reason, n in reasons.most_common():
        print("    %4d  %s" % (n, reason))

    bad = [r for r in ast if not r[i["concrete_impl"]]]
    print("AST nodes without concrete : %d" % len(bad))
    for r in bad:
        print("    %-32s %-10s %s" % (r[i["full_name"]].split(".")[-1], r[i["kind"]], r[i["note"]]))

    # red_tree_nodes table
    red = read(RED)
    print("red_tree_nodes rows        : %d" % (len(red) - 1))
    green = sum(1 for r in red[1:] if r[3])
    print("  with green_class         : %d" % green)

    # key sanity check requested by the task
    elseifs = [r for r in data if r[i["full_name"]].endswith(("IElseIf", "IElseIf2"))]
    for r in elseifs:
        print("  %-14s -> %s" % (r[i["full_name"]].split(".")[-1], r[i["concrete_impl"]] or "нет"))
    print("=" * 62)


if __name__ == "__main__":
    main()
