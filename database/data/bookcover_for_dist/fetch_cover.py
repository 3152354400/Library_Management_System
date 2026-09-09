import os, time, re, requests
from io import BytesIO
from PIL import Image
from requests.adapters import HTTPAdapter, Retry

ISBN_TXT   = "ISBN.txt"
OUT_DIR    = "covers"
DEFAULT_PX = 96      # 过滤 1x1 / 10x10 等“空图”的阈值
PROXIES    = {"http": "http://127.0.0.1:7897",
              "https": "http://127.0.0.1:7897"}

# ---------- 工具函数 ----------
def clean_isbn(raw: str) -> str:
    "去掉连字符、空格，只保留数字与大写 X"
    return re.sub(r"[^0-9X]", "", raw.upper())

def save_image(content: bytes, fname: str):
    img = Image.open(BytesIO(content))
    if img.width <= DEFAULT_PX // 10 or img.height <= DEFAULT_PX // 10:
        return False
    img.save(fname)
    return True

def get_session(use_proxy=False, timeout=10):
    sess = requests.Session()
    if use_proxy:
        sess.proxies.update(PROXIES)
    retry = Retry(total=3, backoff_factor=0.5,
                  status_forcelist=[429, 500, 502, 503, 504])
    sess.mount("https://", HTTPAdapter(max_retries=retry))
    sess.timeout = timeout
    return sess

# ---------- 下载逻辑 ----------
def download_cover(isbn_raw: str, sess_no_proxy, sess_proxy):
    isbn = clean_isbn(isbn_raw)
    if not isbn:
        print(f"⚠️  跳过非法 ISBN: {isbn_raw}")
        return

    outfile = os.path.join(OUT_DIR, f"{isbn}.jpg")
    if os.path.exists(outfile):
        print(f"✅ 已存在: {outfile}")
        return

    def fetch(url, session):
        r = session.get(url, timeout=session.timeout)
        if r.status_code == 404:
            return None
        r.raise_for_status()
        return r.content

    # ① OpenLibrary
    ol_url = f"https://covers.openlibrary.org/b/isbn/{isbn}-M.jpg?default=false"
    for session in (sess_no_proxy, sess_proxy):
        if not session:  # 无代理就跳过
            continue
        try:
            data = fetch(ol_url, session)
            if data and save_image(data, outfile):
                print(f"✅ OpenLibrary 成功: {isbn}")
                return
        except Exception as e:
            print(f"⏳ OpenLibrary 超时({isbn}) - {e}")

    print(f"❌ 失败: {isbn}")

# ---------- 执行 ----------
if __name__ == "__main__":
    os.makedirs(OUT_DIR, exist_ok=True)

    with open(ISBN_TXT, encoding="utf-8") as f:
        isbn_list = [line.strip() for line in f if line.strip()]

    # 无代理 / 有代理 session
    sess_direct = get_session(use_proxy=False)
    sess_proxy  = get_session(use_proxy=True) if PROXIES["http"] else None

    for code in isbn_list:
        download_cover(code, sess_direct, sess_proxy)
        time.sleep(0.2)   # 轻微限速，避免触发对方反爬
