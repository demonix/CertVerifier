namespace CertVerify
{
    public enum CertVerifyResult
    {
        Valid,
        Expired,
        NotYetValid,
        Revoked,
        NotTrusted
    }
}