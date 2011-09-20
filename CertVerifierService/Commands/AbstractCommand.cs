using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace CertVerifierService.Commands
{
    
        internal struct Parameters
        {
            public Parameters(byte[] body, NameValueCollection parameters)
            {
                this.body = body;
                //TODO: make case insensitive
                queryParameters = parameters;
            }

            public byte[] Body { get { return body; } }

            public string GetParam(string paramName)
            {
                return queryParameters[paramName];
            }
            public bool HasParam(string paramName)
            {
                return queryParameters[paramName] != null;
            }

            public NameValueCollection QueryParameters
            {
                get { return queryParameters; }
            }

            private readonly byte[] body;
            private readonly NameValueCollection queryParameters;
        }

        internal interface ICommand
        {
            string Execute();
        }

        abstract class Command : ICommand
        {
            protected Command(Parameters parameters)
            {
                this.parameters = parameters;
            }

            public abstract string Execute();
            protected Parameters parameters;
        }
    
}