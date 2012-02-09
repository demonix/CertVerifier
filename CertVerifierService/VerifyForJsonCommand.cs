using System;
using System.Text;
using CertVerifierService.Commands;

namespace CertVerifierService
{
    internal class VerifyForJsonCommand : Command
    {
        private bool _jsonParsed;
        private string _cert;
        public VerifyForJsonCommand(Parameters parameters): base(parameters)
        {
            string body = Encoding.UTF8.GetString(parameters.Body);
            try
            {
                _cert = body.Split(':')[0].Trim(new[] { '}', ' ', '"' });
                _jsonParsed = true;
            }
            catch (Exception ex)
            {
            }
        }

        public override string Execute()
        {
            if (!_jsonParsed)
                throw new Exception("invalidJsonInput");
            return "{ \"result\": \"" +Verifier.GetInstance().Verify(Encoding.ASCII.GetBytes(_cert))+"\"}";
        }
    }
}