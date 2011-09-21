using System;
using System.Collections.Generic;
using System.Threading;
using Org.BouncyCastle.X509;

namespace CertVerify
{
    internal class Crl
    {
        Timer _timer;
        private X509Certificate _issuerCertificate;
        TimeSpan _updateIntervalAfterNewCrlIssued = TimeSpan.FromMinutes(10);
        TimeSpan _updateIntervalAfterExpire = TimeSpan.FromSeconds(10);
        ReaderWriterLockSlim _rwLocker = new ReaderWriterLockSlim();
        HashSet<string> _revokedCerts = new HashSet<string>();
        DateTime _notBefore = DateTime.MaxValue;
        DateTime _notAfter = DateTime.MinValue;
        DateTime _nextPublish = DateTime.MinValue;
        private List<string> _cdpAddresses = new List<string>();

        
        public bool Valid
        { get { return !NotYetValid && !IsExpired; }}

        private bool NotYetValid
        { get { return DateTime.Now < _notBefore; } }

        private bool IsExpired
        { get {return  DateTime.Now > _notAfter; } }

        private bool WillExpireAt (DateTime dateTime)
        { return dateTime > _notAfter; }

        private TimeSpan TimeToNextPublish
        { get { return DateTime.Now > _nextPublish ? _nextPublish.Subtract(DateTime.Now) : new TimeSpan(0); } }



        public Crl(X509Certificate issuerCertificate, List<string> cdpAddresses)
        {
            _issuerCertificate = issuerCertificate;
            _cdpAddresses = cdpAddresses;
            _timer = new Timer(UpdateCrl);
            UpdateCrl(null);
        }

        private void UpdateCrl(object state)
        {
            Console.WriteLine("Updating Crl...");
            X509Crl crl = CrlDownloader.DownloadCrl(_cdpAddresses);
            if (crl == null)
            {
                Console.WriteLine("Can't update crl");
                ShedeuleNextUpdate();
                return;
            }
            if (!crl.IsSignedBy(_issuerCertificate))
            {
                Console.WriteLine("Downloaded CRL not issued by {0}", _issuerCertificate.SubjectDN);
                return;
            }
            _notBefore = new DateTime(crl.ThisUpdate.Ticks, DateTimeKind.Utc).ToLocalTime();
            _notAfter = new DateTime(crl.NextUpdate.Value.Ticks, DateTimeKind.Utc).ToLocalTime();
            _nextPublish = new DateTime(crl.NextUpdate.Value.Ticks, DateTimeKind.Utc).ToLocalTime();
            var revCerts = crl.GetRevokedCertificates();
            HashSet<string> tmpRevokedCerts = new HashSet<string>();
            foreach (X509CrlEntry revCert in revCerts)
            {
                if (!tmpRevokedCerts.Contains(revCert.SerialNumber.ToString(16)))
                    tmpRevokedCerts.Add(revCert.SerialNumber.ToString(16));
            }

            _rwLocker.EnterWriteLock();
            try
            {
                _revokedCerts = tmpRevokedCerts;
            }
            finally
            {
                _rwLocker.ExitWriteLock();
            }
            ShedeuleNextUpdate();

        }

        
        private void ShedeuleNextUpdate()
        {

            TimeSpan nextRun;
            if (IsExpired || WillExpireAt(DateTime.Now.Add(_updateIntervalAfterNewCrlIssued)))
            {
                if (WillExpireAt(DateTime.Now.Add(_updateIntervalAfterExpire)))
                    nextRun = _notAfter.Subtract(DateTime.Now).Subtract(TimeSpan.FromSeconds(5));
                else
                    nextRun = _updateIntervalAfterExpire;
            }
            else
            {
                if (TimeToNextPublish > _updateIntervalAfterNewCrlIssued)
                    nextRun = TimeToNextPublish;
                else
                    nextRun = _updateIntervalAfterNewCrlIssued;
            }
            _timer.Change(nextRun, TimeSpan.FromMilliseconds(-1));
        }

        public bool Contains(X509Certificate certificate)
        {
            _rwLocker.EnterReadLock();
            try
            {
                return _revokedCerts.Contains(certificate.SerialNumber.ToString(16));
            }
            finally 
            {
                _rwLocker.ExitReadLock();
            }
        }
    }
}