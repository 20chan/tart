using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CreamRoll.Routing;

namespace tart.Server {
    public class FrontEndServer {
        public string DirectoryPath;
        public string Index;

        public FrontEndServer(string path, string index) {
            DirectoryPath = path;
            Index = index;
        }

        [Get("/{filename}")]
        public Response RouteFile(Request req) {
            var path = req.Uri.AbsolutePath.Substring(1);
            if (string.IsNullOrEmpty(path)) {
                path = Index;
            }

            var fullPath = Path.Combine(DirectoryPath, path);
            if (!File.Exists(fullPath)) {
                return new TextResponse("file not found", status: StatusCode.NotFound);
            }

            return new FileResponse(fullPath);
        }
    }
}