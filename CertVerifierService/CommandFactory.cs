using System;
using CertVerifierService.Commands;

namespace CertVerifierService
{
    internal class CommandFactory
    {
        public static ICommand GetCommand(Parameters parameters)
        {
            string method = parameters.HasParam("method") ? parameters.GetParam("method").ToLower() : "";
            switch (method)
            {
                case "verify": return new VerifyCommand(parameters);
                case "verifyforjson": return new VerifyForJsonCommand(parameters);
                case "verifybyget": return new VerifyByGetCommand(parameters);
                default: throw new UnknownMethodException("unknown method: " + method);
            }
        }

        internal class UnknownMethodException : Exception
        {
            public UnknownMethodException(string message) : base(message) { }
        }
    }
}