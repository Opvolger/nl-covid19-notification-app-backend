﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping
{
    public static class Mapper
    {
        public static TemporaryExposureKeyEntity[] ToEntities(this TemporaryExposureKeyArgs[] items)
        {
            var content = items.Select(x =>
                new TemporaryExposureKeyEntity
                {
                    KeyData = Convert.FromBase64String(x.KeyData),
                    TransmissionRiskLevel = 0,
                    RollingPeriod = x.RollingPeriod,
                    RollingStartNumber = x.RollingStartNumber,
                }).ToArray();

            return content;
        }
    }
}