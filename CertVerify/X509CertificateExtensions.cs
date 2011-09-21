using System;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.X509;

namespace CertVerify
{
    public static class X509CertificateExtensions
    {
        public static bool IsSignedBy(this X509Certificate thisCertificate, X509Certificate signerCertificate)
        {
            var publicKey = signerCertificate.GetPublicKey();
            try
            {
                thisCertificate.Verify(publicKey);
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
            var publicKey = signerCertificate.GetPublicKey();
            try
            {
                thisCertificate.Verify(publicKey);
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
}