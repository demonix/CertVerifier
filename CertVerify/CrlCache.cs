using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CertVerify.Certificates;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;

namespace CertVerify
{
    public class CrlCache
    {
        ReaderWriterLockSlim _crlCacheLocker = new ReaderWriterLockSlim();
        Dictionary<string, Crl> _crlCache = new Dictionary<string, Crl>();
        
        public bool IsRevoked(X509Certificate certificate)
        {
            string authorityId = certificate.GetAuthorityKeyIdentifier();
            Crl ce = GetCrlEntry(authorityId);
            if (ce == null)
                ce = CreateCrlEntry(authorityId, certificate.GetCrlDistributionPointAddresses());
            if (!ce.Valid)
                return true;
            return ce.Contains(certificate);
        }

        public void RemoveCrlEntries(IEnumerable<string> authorityIds)
        {
            foreach (var authorityId in authorityIds)
            {
                RemoveCrlEntry(authorityId);
            }
        }
        
        public void RemoveCrlEntry(string authorityId)
        {
            _crlCacheLocker.EnterUpgradeableReadLock();
            try
            {
                if (_crlCache.ContainsKey(authorityId))
                    try
                    {
                        _crlCacheLocker.EnterWriteLock();
                        _crlCache.Remove(authorityId);
                    }
                    finally
                    {
                        _crlCacheLocker.ExitWriteLock();
                    }
            }
            finally
            {
                _crlCacheLocker.ExitUpgradeableReadLock();
            }
        }


        private Crl CreateCrlEntry(string authorityId, List<string> cdpAddresses)
        {
            Crl cacheEntry = new Crl(cdpAddresses);
            _crlCacheLocker.EnterWriteLock();
            try
            {
                _crlCache.Add(authorityId, cacheEntry);
            }
            finally
            {
                _crlCacheLocker.ExitWriteLock();
            }
            return cacheEntry;
        }

        private Crl GetCrlEntry(string authorityId)
        {
            _crlCacheLocker.EnterReadLock();
            try
            {
                if (_crlCache.ContainsKey(authorityId))
                  return _crlCache[authorityId];
            }
            finally 
            {
                _crlCacheLocker.ExitReadLock();
            }
            return null;
        }
    }
}
