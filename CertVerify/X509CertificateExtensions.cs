using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.X509;
using X509Certificate2 = System.Security.Cryptography.X509Certificates.X509Certificate2;

namespace CertVerify
{
    public static class X509CertificateExtensions
    {
        private const int X509_ASN_ENCODING = 0x1;
        private const int CRYPT_VERIFY_CERT_SIGN_SUBJECT_BLOB = 0x1;
        private const int CRYPT_VERIFY_CERT_SIGN_SUBJECT_CERT = 0x2;
        private const int CRYPT_VERIFY_CERT_SIGN_ISSUER_CERT = 0x2;

        public static bool IsSignedBy(this X509Certificate thisCertificate, X509Certificate signerCertificate)
        {
            X509Certificate2 c = new X509Certificate2(thisCertificate.GetTbsCertificate());
            X509Certificate2 i = new X509Certificate2(signerCertificate.GetTbsCertificate());
            X509Certificate2 c2 = new X509Certificate2(@"c:\temp\der.cer");
            X509Certificate2 i2 = new X509Certificate2(@"c:\temp\cader.cer");
            /*byte[] pvSubject = thisCertificate.GetTbsCertificate();
            byte[] pvIssuer = signerCertificate.GetTbsCertificate();
*/
            System.Text.Encoding.ASCII.GetString(c.RawData);
            IntPtr pvSubject = c.Handle;
            IntPtr pvIssuer = i.Handle;
            int res = SspiProvider.CryptVerifyCertificateSignatureEx(IntPtr.Zero, X509_ASN_ENCODING,
                                                           CRYPT_VERIFY_CERT_SIGN_SUBJECT_CERT, pvSubject,
                                                           CRYPT_VERIFY_CERT_SIGN_ISSUER_CERT, pvIssuer, 0,
                                                           IntPtr.Zero);
            Marshal.GetLastWin32Error();
            CmsSigner signer = new CmsSigner(i);
            SignedCms signedMessage = new SignedCms();
            // deserialize PKCS #7 byte array
          
            signedMessage.Decode(thisCertificate.GetTbsCertificate());
            Log.Write("Veryfy old");
            Log.Write("EndVeryfy old");
            Log.Write("Get signer's public key");
            var publicKey = signerCertificate.GetPublicKey();
            Log.Write("Got signer's public key");
            try
            {
                Log.Write("Veryfy signature");
                //TODO: log errors
                thisCertificate.Verify(publicKey);
                Log.Write("Verified");
            }
            catch (CertificateException)
            {
                return false;
            }
            catch (InvalidKeyException)
            {
                return false;
            }
            return true;
        }

        public static bool IsSignedBy(this X509Crl thisCertificate, X509Certificate signerCertificate)
        {
            Log.Write("Get signer's public key");
            var publicKey = signerCertificate.GetPublicKey();
            Log.Write("Got signer's public key");
            try
            {
                Log.Write("Veryfy signature");
                thisCertificate.Verify(publicKey);
                Log.Write("Veryfied");
            }
            catch (CrlException)
            {
                return false;
            }
            catch(SignatureException)
            {
                return false;
            }
            return true;
        }
    }
    internal sealed class SspiProvider
    {
        [DllImport(@"crypt32.dll", EntryPoint = "CryptVerifyCertificateSignatureEx", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern int CryptVerifyCertificateSignatureEx(IntPtr hCryptProv, int dwCertEncodingType, int dwSubjectType, IntPtr pvSubject, int dwIssuerType, IntPtr pvIssuer, int dwFlags, IntPtr pvReserved);
    }
}