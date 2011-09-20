using System;
using System.IO;
using System.Text;
using CertVerify.Certificates;
using Org.BouncyCastle.X509;

namespace CertVerify
{
    public class CertVerifier
    {
        private Ctl _ctl = new Ctl();
        private readonly byte[] _pemCertificateHeader;
        private readonly byte[] _pemCertificateEnding;

        public CertVerifier()
        {
            _pemCertificateHeader = Encoding.ASCII.GetBytes("-----BEGIN CERTIFICATE-----\r\n");
            _pemCertificateEnding = Encoding.ASCII.GetBytes("\r\n-----END CERTIFICATE-----");
        }

        public CertVerifyResult Verify(byte[] certificate)
        {
            if (certificate[0] != 0x30 && certificate[0] != 0x2D)
                AddHeaderAndFooter(ref certificate);
            return Verify(X509CertificateHelper.CreateCertificateFromBytes(certificate));
        }

        private void AddHeaderAndFooter(ref byte[] certificate)
        {
            byte[] correctCer;
            correctCer = new byte[_pemCertificateHeader.Length + _pemCertificateEnding.Length + certificate.Length];
            Array.Copy(_pemCertificateHeader, 0, correctCer, 0, _pemCertificateHeader.Length);
            Array.Copy(certificate, 0, correctCer, _pemCertificateHeader.Length, certificate.Length);
            Array.Copy(_pemCertificateEnding, 0, correctCer, _pemCertificateHeader.Length + certificate.Length, _pemCertificateEnding.Length);
            certificate = correctCer;
        }

        public CertVerifyResult Verify(X509Certificate certificate)
        {
            if (certificate.NotAfter < DateTime.UtcNow)
                return CertVerifyResult.Expired;
            if (certificate.NotBefore > DateTime.UtcNow)
                return CertVerifyResult.NotYetValid;
            if (!_ctl.MayTrustTo(certificate))
                return CertVerifyResult.NotTrusted;
            if (_ctl.CrlCache.IsRevoked(certificate))
                return CertVerifyResult.Revoked;
            return CertVerifyResult.Valid;
        }
    }
}