using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Infrastructure.AppSettings.Model
{
    public class PuppeteerPdfSetting
    {
        public string Path { get; set; } // A path for the downloads folder.
        public int Product { get; set; } // Product.  Browser to use (Chrome or Firefox).
        public string Version { get; set; } // Downloads the revision of browser
        public string ExecutablePath { get; set; } // Path to a Chromium or Chrome executable to run instead of bundled Chromium. If executablePath is a relative path, then it is resolved relative to current working directory.
        public string Host { get; set; } // Host download of browser
    }
}
