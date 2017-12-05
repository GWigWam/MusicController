using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore {
    public static class LinkHelper {

        public static FileInfo GetLinkTarget(string linkFilePath) {
            //Who knows wtf this magic does...
            IWshRuntimeLibrary.IWshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(linkFilePath);
            if (File.Exists(shortcut.TargetPath)) {
                return new FileInfo(shortcut.TargetPath);
            }
            return null;
        }

        public static FileInfo GetLinkTarget(FileInfo linkFile) => GetLinkTarget(linkFile.FullName);
    }
}
