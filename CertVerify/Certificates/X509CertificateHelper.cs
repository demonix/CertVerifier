using System;
using System.IO;
using Org.BouncyCastle.X509;

namespace CertVerify.Certificates
{
    public class X509CertificateHelper
    {
        public static X509Certificate CreateCertificateFromFile(string filename)
        {
            using (FileStream stream = new FileStream(filename,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
            {
                return CreateCertificateFromStream(stream);
            }
        }

        public static X509Certificate CreateCertificateFromStream(Stream stream)
        {
                return new X509CertificateParser().ReadCertificate(stream);
        }
        
        public static X509Certificate CreateCertificateFromBytes(byte[] bytes)
        {
            return new X509CertificateParser().ReadCertificate(bytes);
        }
    }
}