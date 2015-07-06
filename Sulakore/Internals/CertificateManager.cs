﻿/* Copyright

    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    .NET library for creating Habbo Hotel related desktop applications.
    Copyright (C) 2015 ArachisH

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    See License.txt in the project root for license information.
*/

using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Sulakore
{
    internal class CertificateManager
    {
        private const string CERT_CREATE_FORMAT =
            "-ss {0} -n \"CN={1}, O={2}\" -sky {3} -cy {4} -m 120 -a sha256 -eku 1.3.6.1.5.5.7.3.1 -b {5:MM/dd/yyyy} {6}";

        private readonly IDictionary<string, X509Certificate2> _certificateCache;

        public string Issuer { get; private set; }
        public string RootCertificateName { get; private set; }

        public X509Store MyStore { get; private set; }
        public X509Store RootStore { get; private set; }

        public CertificateManager(string issuer, string rootCertificateName)
        {
            Issuer = issuer;
            RootCertificateName = rootCertificateName;

            MyStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            RootStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser);

            _certificateCache = new Dictionary<string, X509Certificate2>();
        }

        public bool CreateTrustedRootCertificate()
        {
            X509Certificate2 rootCertificate =
                CreateCertificate(RootStore, RootCertificateName);

            return rootCertificate != null;
        }
        public bool DestroyTrustedRootCertificate()
        {
            return DestroyCertificate(RootStore, RootCertificateName);
        }

        public X509Certificate2Collection FindCertificates(string certificateSubject)
        {
            return FindCertificates(MyStore, certificateSubject);
        }
        protected virtual X509Certificate2Collection FindCertificates(X509Store store, string certificateSubject)
        {
            X509Certificate2Collection discoveredCertificates = store.Certificates
                .Find(X509FindType.FindBySubjectDistinguishedName, certificateSubject, false);

            return discoveredCertificates.Count > 0 ?
                discoveredCertificates : null;
        }

        public X509Certificate2 CreateCertificate(string certificateName)
        {
            return CreateCertificate(MyStore, certificateName);
        }
        protected virtual X509Certificate2 CreateCertificate(X509Store store, string certificateName)
        {
            if (_certificateCache.ContainsKey(certificateName))
                return _certificateCache[certificateName];

            lock (store)
            {
                X509Certificate2 certificate = null;
                try
                {
                    store.Open(OpenFlags.ReadWrite);
                    string certificateSubject = string.Format("CN={0}, O={1}", certificateName, Issuer);

                    X509Certificate2Collection certificates = FindCertificates(store, certificateSubject);

                    if (certificates != null)
                        certificate = FindCertificates(store, certificateSubject)[0];

                    if (certificate == null)
                    {
                        string[] args = new[] {
                            GetCertificateCreateArgs(store, certificateName) };

                        CreateCertificate(args);
                        certificates = FindCertificates(store, certificateSubject);

                        return certificates != null ?
                            certificates[0] : null;
                    }
                    return certificate;
                }
                finally
                {
                    store.Close();

                    if (certificate != null && !_certificateCache.ContainsKey(certificateName))
                        _certificateCache.Add(certificateName, certificate);
                }
            }
        }
        protected virtual void CreateCertificate(string[] args)
        {
            using (var process = new Process())
            {
                if (!File.Exists("makecert.exe"))
                    throw new Exception("Unable to locate 'makecert.exe'.");

                process.StartInfo.Verb = "runas";
                process.StartInfo.Arguments = args != null ? args[0] : string.Empty;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = "makecert.exe";

                process.Start();
                process.WaitForExit();
            }
        }

        public bool DestroyCertificate(string certificateName)
        {
            return DestroyCertificate(MyStore, certificateName);
        }
        protected virtual bool DestroyCertificate(X509Store store, string certificateName)
        {
            lock (store)
            {
                X509Certificate2Collection certificates = null;
                try
                {
                    store.Open(OpenFlags.ReadWrite);
                    string certificateSubject = string.Format("CN={0}, O={1}", certificateName, Issuer);

                    certificates = FindCertificates(store, certificateSubject);
                    if (certificates != null)
                    {
                        store.RemoveRange(certificates);
                        certificates = FindCertificates(store, certificateSubject);
                    }
                    return certificates == null;
                }
                finally
                {
                    store.Close();

                    if (certificates == null &&
                        _certificateCache.ContainsKey(certificateName))
                    {
                        _certificateCache.Remove(certificateName);
                    }
                }
            }
        }

        protected virtual string GetCertificateCreateArgs(X509Store store, string certificateName)
        {
            bool isRootCertificate =
                (certificateName == RootCertificateName);

            string certCreatArgs = string.Format(CERT_CREATE_FORMAT,
                store.Name, certificateName, Issuer,
                isRootCertificate ? "signature" : "exchange",
                isRootCertificate ? "authority" : "end", DateTime.Now,
                isRootCertificate ? "-h 1 -r" : string.Format("-pe -in \"{0}\" -is Root", RootCertificateName));

            return certCreatArgs;
        }
    }
}