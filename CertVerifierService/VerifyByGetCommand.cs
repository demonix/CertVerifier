using System;
using System.Text;
using CertVerifierService.Commands;

namespace CertVerifierService
{
    internal class VerifyByGetCommand : Command
    {
        public VerifyByGetCommand(Parameters parameters)
            : base(parameters)
        {

        }

        public override string Execute()
        {
            if (parameters.HasParam("body"))
                return Verifier.GetInstance().Verify(Encoding.ASCII.GetBytes(parameters.GetParam("body"))).ToString();

            throw new Exception("Missing body parameter");



        }
    }
}