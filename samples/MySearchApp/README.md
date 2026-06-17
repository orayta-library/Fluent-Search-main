# MyMapsApp (sample)

דוגמת ממשק מפות מבוסס Avalonia. הממשק מדגים רשימת שכבות, קנבס מפות (placeholder), וכלי ניווט/זום; המימוש הממשי של מפות (tile provider, GIS, חיפוש כתובות) יתווסף בנפרד.

להרצה:

```bash
dotnet restore
dotnet build
dotnet run --project samples/MySearchApp
```

Notes:
- Place your MBTiles file at `samples/MySearchApp/data/map.mbtiles` before running. The app will start a small local HTTP server on `http://127.0.0.1:5000/` which serves `index.html` and tile requests.
- The sample currently opens the map page in your default browser when you click "Open Map". To embed the page inside the app you can add a WebView control (e.g., WebView2) and point it at `http://127.0.0.1:5000/`.
