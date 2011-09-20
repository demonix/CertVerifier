using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CertVerify;
using CertVerify.Certificates;
using Org.BouncyCastle.X509;

namespace CertVerifierApp
{
    class Program
    {
        static void Main(string[] args)
        {
            CertVerifier cv = new CertVerifier();
            X509Certificate certificate = X509CertificateHelper.CreateCertificateFromFile(args[0]);
            Console.WriteLine(cv.Verify(certificate));
            Console.ReadLine();
        }
    }
}
