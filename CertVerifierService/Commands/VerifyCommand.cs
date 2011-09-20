using System;

namespace CertVerifierService.Commands
{
    internal class VerifyCommand: Command
    {
        public VerifyCommand(Parameters parameters) : base(parameters)
        {
        }

        public override string Execute()
        {
            return Verifier.GetInstance().Verify(parameters.Body).ToString();
        }
    }
}