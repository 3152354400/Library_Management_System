import requests, pathlib, csv, html

DEFAULT_STOCK = 5
API  = "https://openlibrary.org/api/books"
PROXIES = {"http": "http://127.0.0.1:7897", "https": "http://127.0.0.1:7897"}

def esc(txt, limit):
    return html.escape(txt.replace("'", "''"))[:limit]

def fetch_isbn(isbn: str):
    try:
        r = requests.get(API,
                         params={"bibkeys": f"ISBN:{isbn}",
                                 "format": "json",
                                 "jscmd": "data"},
                         proxies=PROXIES, timeout=10)
        r.raise_for_status()
        return r.json().get(f"ISBN:{isbn}")      # dict 或 None
    except Exception as e:
        print(f"[❌] {isbn}: {e}")
        return None

def main():
    base_dir = pathlib.Path(__file__).parent      # 保证路径正确
    isbns = [l.strip() for l in (base_dir / "isbns.txt")
                            .read_text(encoding="utf-8-sig").splitlines() if l.strip()]

    sql_fp  = (base_dir / "bookinfo_insert.sql").open("w", encoding="utf-8")
    csv_fp  = (base_dir / "isbn_subject.csv").open("w", newline="", encoding="utf-8")
    csv_out = csv.writer(csv_fp); csv_out.writerow(["ISBN", "Subject"])

    for i, isbn in enumerate(isbns, 1):
        data = fetch_isbn(isbn)
        if not data:
            print(f"[{i}/{len(isbns)}] ⚠ 无数据: {isbn}")
            continue

        # ---------- BookInfo ----------
        title  = esc(data.get("title", ""), 100)
        author = esc("、".join(a["name"] for a in data.get("authors", [])) or "佚名", 40)
        sql_fp.write(f"INSERT INTO BookInfo (ISBN, Title, Author, Stock) "
                     f"VALUES ('{isbn}','{title}','{author}',{DEFAULT_STOCK});\n")

        # ---------- Subject ----------
        for s in data.get("subjects", []):
            if isinstance(s, dict):          # 兼容 dict 和 str
                s = s.get("name", "")
            csv_out.writerow([isbn, esc(str(s), 100)])

        print(f"[{i}/{len(isbns)}] ✅ {isbn}")

    sql_fp.close(); csv_fp.close()
    print("✔ 完成：生成 bookinfo_insert.sql 与 isbn_subject.csv")

if __name__ == "__main__":
    main()
