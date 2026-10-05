# AutoMate A3 팸플릿

- `AutoMate_Pamphlet.html`: 편집 가능한 원본
- `AutoMate_Pamphlet_A3.pdf`: 제출 및 인쇄용 A3 세로 PDF
- `AutoMate_Pamphlet_preview.png`: 빠른 확인용 미리보기

HTML을 수정한 뒤 프로젝트 루트에서 아래 명령으로 PDF를 다시 만들 수 있습니다.

```bash
"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome" \
  --headless=new --disable-gpu --no-sandbox \
  --allow-file-access-from-files --no-pdf-header-footer \
  --print-to-pdf="$(pwd)/docs/pamphlet/AutoMate_Pamphlet_A3.pdf" \
  "file://$(pwd)/docs/pamphlet/AutoMate_Pamphlet.html"
```
