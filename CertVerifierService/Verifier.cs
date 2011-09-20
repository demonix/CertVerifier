using System;
using System.IO;
using System.Text;
using CertVerify;

namespace CertVerifierService
{
    public class Verifier
    {
        private static Verifier _instance;
        private static object _locker = new object();

        private CertVerifier _certVerifier;

        private Verifier()
        {
            _certVerifier = new CertVerifier();
        }

        public static Verifier GetInstance()
        {
            if (_instance == null)
                lock (_locker)
                {
                    if (_instance == null)
                        _instance = new Verifier();
                }
            return _instance;
        }

        public CertVerifyResult Verify(byte[] certificate)
        {
            return _certVerifier.Verify(certificate);
        }
    }
}