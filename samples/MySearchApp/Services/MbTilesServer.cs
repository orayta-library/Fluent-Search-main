using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MySearchApp.Services
{
    // Very small HTTP server that serves static files from www and tiles from an MBTiles file.
    public class MbTilesServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly string _wwwRoot;
        private readonly string _mbtilesPath;
        private SqliteConnection? _db;

        public string Url { get; }

        public MbTilesServer(string prefix, string wwwRoot, string mbtilesPath)
        {
            Url = prefix.TrimEnd('/');
            _wwwRoot = wwwRoot;
            _mbtilesPath = mbtilesPath;
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
        }

        public void Start()
        {
            if (File.Exists(_mbtilesPath))
            {
                _db = new SqliteConnection($"Data Source={_mbtilesPath}");
                _db.Open();
            }

            _listener.Start();
            Task.Run(() => Loop());
        }

        private async Task Loop()
        {
            while (_listener.IsListening)
            {
                try
                {
                    var ctx = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequest(ctx));
                }
                catch { break; }
            }
        }

        private void HandleRequest(HttpListenerContext ctx)
        {
            try
            {
                var req = ctx.Request;
                var res = ctx.Response;
                var path = req.Url.LocalPath.TrimStart('/');

                if (string.IsNullOrEmpty(path) || path == "index.html")
                {
                    var index = Path.Combine(_wwwRoot, "index.html");
                    if (File.Exists(index))
                        ServeFile(res, index, "text/html");
                    else
                        res.StatusCode = 404;
                    return;
                }

                if (path.StartsWith("tiles/"))
                {
                    // expected format: tiles/{z}/{x}/{y}.png or .pbf
                    var parts = path.Split('/');
                    if (parts.Length == 4)
                    {
                        var z = int.Parse(parts[1]);
                        var x = int.Parse(parts[2]);
                        var yPart = parts[3];
                        var ext = Path.GetExtension(yPart).TrimStart('.').ToLowerInvariant();
                        var yStr = Path.GetFileNameWithoutExtension(yPart);
                        var y = int.Parse(yStr);
                        var bytes = GetTile(z, x, y);
                        if (bytes != null)
                        {
                            var contentType = ext == "pbf" ? "application/x-protobuf" : "image/png";
                            ServeBytes(res, bytes, contentType);
                            return;
                        }
                        res.StatusCode = 404;
                        return;
                    }
                }

                // fallback: serve static file from www
                var file = Path.Combine(_wwwRoot, path.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(file))
                {
                    var ct = GetContentType(Path.GetExtension(file));
                    ServeFile(res, file, ct);
                    return;
                }

                res.StatusCode = 404;
            }
            catch
            {
                try { ctx.Response.StatusCode = 500; } catch { }
            }
            finally
            {
                try { ctx.Response.OutputStream.Close(); } catch { }
            }
        }

        private byte[]? GetTile(int z, int x, int y)
        {
            if (_db == null) return null;
            // MBTiles uses TMS tile_row often; try both y and flipped y
            var candidates = new[] { y, ((1 << z) - 1) - y };
            foreach (var yy in candidates)
            {
                using var cmd = _db.CreateCommand();
                cmd.CommandText = "SELECT tile_data FROM tiles WHERE zoom_level = $z AND tile_column = $x AND tile_row = $y LIMIT 1";
                cmd.Parameters.AddWithValue("$z", z);
                cmd.Parameters.AddWithValue("$x", x);
                cmd.Parameters.AddWithValue("$y", yy);
                using var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    return (byte[])r[0];
                }
            }
            return null;
        }

        private static void ServeFile(HttpListenerResponse res, string path, string contentType)
        {
            var bytes = File.ReadAllBytes(path);
            ServeBytes(res, bytes, contentType);
        }

        private static void ServeBytes(HttpListenerResponse res, byte[] bytes, string contentType)
        {
            res.ContentType = contentType;
            res.ContentLength64 = bytes.Length;
            res.OutputStream.Write(bytes, 0, bytes.Length);
        }

        private static string GetContentType(string ext)
        {
            ext = ext.TrimStart('.').ToLowerInvariant();
            return ext switch
            {
                "js" => "application/javascript",
                "css" => "text/css",
                "png" => "image/png",
                "html" => "text/html",
                "json" => "application/json",
                _ => "application/octet-stream",
            };
        }

        public void Dispose()
        {
            try { _listener.Stop(); } catch { }
            try { _db?.Close(); } catch { }
        }
    }
}
