﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Security.Cryptography;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    public class SignatureValidator : ISignatureValidator
    {
        public bool Valid(byte[] signature, KeyReleaseWorkflowState workflow, byte[] data)
        {
            if (signature == null) throw new ArgumentNullException(nameof(signature));
            if (workflow == null) throw new ArgumentNullException(nameof(workflow));
            if (data == null) throw new ArgumentNullException(nameof(data));

            using var hmac = new HMACSHA256(Convert.FromBase64String(workflow.ConfirmationKey));
            var hash = hmac.ComputeHash(data);

            return hash.SequenceEqual(signature);
        }
    }
}