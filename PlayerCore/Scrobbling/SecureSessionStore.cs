using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CredentialManagement;

namespace PlayerCore.Scrobbling
{
    internal static class SecureSessionStore
    {
#if !DEBUG
        private const string TargetName = "MusicController_LastFM";
#else
        private const string TargetName = "MusicController_LastFM_DBG";
#endif

        public static void Save(string user, string sessionToken)
        {
            using var cred = new Credential { Target = TargetName, Username = user, Password = sessionToken, PersistanceType = PersistanceType.LocalComputer };
            if (!cred.Save())
            {
                throw new Exception("Saving credential failed");
            }
        }

        public static (string usr, string sessionToken) Load()
        {
            using var cred = new Credential { Target = TargetName };
            if (cred.Load())
            {
                return (cred.Username, cred.Password);
            }
            else
            {
                throw new Exception("Failed to load LastFM credentials from Windows credentials store");
            }
        }

        public static void Clear()
        {
            using var cred = new Credential { Target = TargetName };
            cred.Delete();
        }
    }
}
