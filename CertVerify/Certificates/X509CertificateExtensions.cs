using System;
using System.Collections.Generic;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;

namespace CertVerify.Certificates
{
    public static class X509CertificateExtensions
    {
        public static string GetAuthorityKeyIdentifier(this X509Certificate certificate)
        {
            var x509Extension = certificate.CertificateStructure.TbsCertificate.Extensions.GetExtension(X509Extensions.AuthorityKeyIdentifier);
            if (x509Extension == null)
                return null;
            var authorityKeyIdExtension = AuthorityKeyIdentifier.GetInstance(x509Extension);
            return Hex.ToHexString(authorityKeyIdExtension.GetKeyIdentifier());
        }
        public static string GetSubjectKeyIdentifier(this X509Certificate certificate)
        {
            var x509Extension = certificate.CertificateStructure.TbsCertificate.Extensions.GetExtension(X509Extensions.SubjectKeyIdentifier);
            if (x509Extension == null)
                return null;
            var subjectKeyIdExtension = SubjectKeyIdentifier.GetInstance(x509Extension);
            return Hex.ToHexString(subjectKeyIdExtension.GetKeyIdentifier());
        }

        public static bool IsRoot(this X509Certificate certificate)
        {
            string cn1 = (string)certificate.IssuerDN.GetValueList(X509Name.CN)[0];
            string cn2 = (string)certificate.SubjectDN.GetValueList(X509Name.CN)[0];
            if (cn1 != cn2)
                return false;
            if (certificate.GetAuthorityKeyIdentifier() == null)
                return true;
            return false;
        }

        public static List<string> GetCrlDistributionPointAddresses(this X509Certificate certificate)
        {
            List<string> result = new List<string>();
            var ext = certificate.GetExtensionValue(X509Extensions.CrlDistributionPoints);
            if (ext == null)
                return result;
            CrlDistPoint crldp = CrlDistPoint.GetInstance(X509ExtensionUtilities.FromExtensionValue(ext));

            if (crldp != null)
            {
                DistributionPoint[] dps = null;
                try
                {
                    dps = crldp.GetDistributionPoints();
                }
                catch (Exception e)
                {
                    throw new Exception(
                        "Distribution points could not be read.", e);
                }
                for (int i = 0; i < dps.Length; i++)
                {
                    DistributionPointName dpn = dps[i].DistributionPointName;
                    // look for URIs in fullName
                    if (dpn != null)
                    {
                        if (dpn.PointType == DistributionPointName.FullName)
                        {
                            GeneralName[] genNames = GeneralNames.GetInstance(
                                dpn.Name).GetNames();
                            // look for an URI
                            for (int j = 0; j < genNames.Length; j++)
                            {
                                if (genNames[j].TagNo == GeneralName.UniformResourceIdentifier)
                                {
                                    string location = DerIA5String.GetInstance(
                                        genNames[j].Name).GetString();
                                    result.Add(location);
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}