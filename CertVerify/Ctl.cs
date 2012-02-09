using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CertVerify.Certificates;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace CertVerify
{
    public class Ctl
    {
        private const string TrustedCertificatesFolder = "trustedCertificates";
        private Dictionary<string, X509Certificate> _trustedIssuers = new Dictionary<string, X509Certificate>();

        private ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
        FileSystemWatcher _trustedCertificatesWatcher = new FileSystemWatcher();
        private readonly CrlCache _crlCache;

        public CrlCache CrlCache
        {
            get { return _crlCache; }
        }

        public Ctl()
        {
            _crlCache = new CrlCache(this);
            FillCtl();
            _trustedCertificatesWatcher.Path = TrustedCertificatesFolder;
            _trustedCertificatesWatcher.Changed += OnTrustedCertificatesFolderContentChanged;
            _trustedCertificatesWatcher.Created += OnTrustedCertificatesFolderContentChanged;
            _trustedCertificatesWatcher.Deleted += OnTrustedCertificatesFolderContentChanged;
            _trustedCertificatesWatcher.Renamed += OnTrustedCertificatesFolderContentChanged;
            _trustedCertificatesWatcher.EnableRaisingEvents = true;
        }

        private void OnTrustedCertificatesFolderContentChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                _trustedCertificatesWatcher.EnableRaisingEvents = false;
                Thread.Sleep(1000);
            }
            finally
            {
                _trustedCertificatesWatcher.EnableRaisingEvents = true;
            }

            FillCtl();
        }

        public void FillCtl()
        {
            Dictionary<string,X509Certificate> tmpTrustedIssuers = GetTrustedIssuersList();
            _rwLock.EnterWriteLock();
            try
            {
                _crlCache.RemoveCrlEntries(_trustedIssuers.Keys.Except(tmpTrustedIssuers.Keys));
                _trustedIssuers = tmpTrustedIssuers;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }

        }

        private Dictionary<string,X509Certificate> GetTrustedIssuersList()
        {
            Dictionary<string, X509Certificate> tmpTrustedIssuers = new Dictionary<string, X509Certificate>();
            List<string> certPaths =
                "*.crt|*.cer".Split('|').SelectMany(filter => Directory.GetFiles(TrustedCertificatesFolder, filter)).
                    ToList();
            List<X509Certificate> certificates =
                certPaths.Select(path => X509CertificateHelper.CreateCertificateFromFile(path)).ToList();
            List<X509Certificate> rootCertificates = new List<X509Certificate>();
            List<X509Certificate> intermediateCertificates = new List<X509Certificate>();
            rootCertificates = certificates.Where(cert => cert.IsRoot()).ToList();
            List<X509Certificate> certsToSearch = rootCertificates;
            while (certsToSearch.Count != 0)
            {
                List<X509Certificate> foundCerts =
                    certsToSearch.SelectMany(rootCer =>
                                             GetX509CertificatesIssuedBy(certificates, rootCer)).ToList();
                intermediateCertificates.AddRange(foundCerts);
                certsToSearch = foundCerts;
            }
            foreach (var certificate in rootCertificates.Union(intermediateCertificates))
            {
                string authorityId = certificate.GetSubjectKeyIdentifier();
                if (!tmpTrustedIssuers.ContainsKey(authorityId))
                    tmpTrustedIssuers.Add(authorityId,certificate);
                certificates.Remove(certificate);
            }
            if (certificates.Count != 0)
            {
                Console.WriteLine("Non-trusted certs found in trusted certificates folder:");
                foreach (var certificate in certificates)
                    Console.WriteLine(String.Format("DN: {0}\r\nIssuer:{1}\r\n",certificate.SubjectDN,certificate.IssuerDN));
            }
            return tmpTrustedIssuers;
        }

        private IEnumerable<X509Certificate> GetX509CertificatesIssuedBy(List<X509Certificate> certificates, X509Certificate rootCer)
        {
            var result = certificates.Where(cer =>
                                            cer.GetAuthorityKeyIdentifier() == rootCer.GetSubjectKeyIdentifier());
            return result;
        }


        public bool MayTrustTo(X509Certificate certificate)
        {
            Log.Write("Get authority key");
            string authorityId = certificate.GetAuthorityKeyIdentifier();
            Log.Write("Got authority key");
            Log.Write("Get issuer");
            X509Certificate issuerCertificate = GetIssuerCertificate(authorityId);
            Log.Write("Got issuer");
            Log.Write("Check signature");
            if (issuerCertificate != null && certificate.IsSignedBy(issuerCertificate))
                return true;
            return false;
        }

        public X509Certificate GetIssuerCertificate(string authorityId)
        {
            X509Certificate issuerCertifivate;
            _rwLock.EnterReadLock();
            try
            {
                _trustedIssuers.TryGetValue(authorityId, out issuerCertifivate);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return issuerCertifivate;
        }
    }
}