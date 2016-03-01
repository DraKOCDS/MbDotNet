﻿using System.Collections.Generic;
using System.Linq;
using MbDotNet.Interfaces;
using MbDotNet.Enums;
using MbDotNet.Models;

namespace MbDotNet
{
    public class MountebankClient : IClient
    {
        private readonly IRequestProxy _requestProxy;

        /// <summary>
        /// A collection of all of the current imposters. The imposters in this
        /// collection may or may not have been added to mountebank. See IImposter.PendingSubmission
        /// for more information.
        /// </summary>
        public ICollection<IImposter> Imposters { get; private set; }

        public MountebankClient() : this(new MountebankRequestProxy()) { }

		public MountebankClient(string mountebankUrl) : this(new MountebankRequestProxy(mountebankUrl)) { }

		public MountebankClient(IRequestProxy requestProxy)
        {
            Imposters = new List<IImposter>();
            _requestProxy = requestProxy;
        }

        /// <summary>
        /// Creates a new imposter on the specified port with the specified protocol. The Submit method
        /// must be called on the client in order to submit the imposter to mountebank.
        /// </summary>
        /// <param name="port">The port the imposter will be set up to receive requests on</param>
        /// <param name="protocol">The protocol the imposter will be set up to receive requests through</param>
        /// <returns>The newly created imposter</returns>
        public IImposter CreateImposter(int port, Protocol protocol)
        {
            var imposter = new Imposter(port, protocol);
            Imposters.Add(imposter);
            return imposter;
        }

        /// <summary>
        /// Deletes a single imposter from mountebank. Will also remove the imposter from the collection
        /// of imposters that the client maintains.
        /// </summary>
        /// <param name="port">The port number of the imposter to be removed</param>
        public void DeleteImposter(int port)
        {
            var imposter = Imposters.FirstOrDefault(imp => imp.Port == port);

            if (imposter != null)
            {
                _requestProxy.DeleteImposter(port);
                Imposters.Remove(imposter);
            }
        }

        /// <summary>
        /// Deletes all imposters from mountebank. Will also remove the imposters from the collection
        /// of imposters that the client maintains.
        /// </summary>
        public void DeleteAllImposters()
        {
            _requestProxy.DeleteAllImposters();
            Imposters = new List<IImposter>();
        }

        /// <summary>
        /// Submits all pending imposters to be created in mountebank. Will throw a MountebankException
        /// if unable to create the imposter for any reason.
        /// </summary>
        public void Submit()
        {
            foreach (var imposter in Imposters.Where(imp => imp.PendingSubmission))
            {
                _requestProxy.CreateImposter(imposter);
                imposter.PendingSubmission = false;
            }
        }
    }
}
