﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http.OData.Query;
using Newtonsoft.Json;

namespace NuGetGallery.OData.QueryFilter
{
    /// <summary>
    /// IODataQueryFilter interface
    /// </summary>
    public class ODataQueryFilter
    {
        [Flags]
        public enum ODataOperators
        {
            None = 0,
            Expand = 1,
            Filter = 1 << 1,
            Format = 1 << 2,
            InlineCount = 1 << 3,
            OrderBy = 1 << 4,
            Select = 1 << 5,
            Skip = 1 << 6,
            SkipToken = 1 << 7,
            Top = 1 << 8
        }

        private static readonly string ResourcesNamespace = "NuGetGallery.OData.QueryAllowed.Data";
        private HashSet<ODataOperators> _allowedOperatorPatterns = null;

        /// <summary>
        /// Initialization for a query filter.
        /// </summary>
        /// <param name="fileName"></param>
        public ODataQueryFilter(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            StreamReader sr = new StreamReader(assembly.GetManifestResourceStream($"{ResourcesNamespace}.{fileName}"));
            string json = sr.ReadToEnd(); 
            ODataQueryRequest data = JsonConvert.DeserializeObject<ODataQueryRequest>(json);
            _allowedOperatorPatterns = new HashSet<ODataOperators>(data.AllowedOperatorPatterns
                .Select( (op) => { return (ODataOperators)Enum.Parse(typeof(ODataOperators), op, true); } ));
            if (!_allowedOperatorPatterns.Contains(ODataOperators.None)) { _allowedOperatorPatterns.Add(ODataOperators.None); }
        }

        public ODataQueryFilter()
        {
        }

        /// <summary>
        /// Verifies if queryFormat is allowed.
        /// </summary>
        /// <param name="odataOptions">The <see cref="ODataQueryOptions"/> to be validated.</param>
        /// <returns>Returns true if the queryFormat is allowed.</returns>
        public virtual bool IsAllowed<T>(ODataQueryOptions<T> odataOptions)
        {
            return odataOptions == null ? true : _allowedOperatorPatterns.Contains(ODataOptionsMap(odataOptions));
        }

        /// <summary>
        /// The allowed operators for this API
        /// </summary>
        public HashSet<ODataOperators> AllowedOperatorPatterns => _allowedOperatorPatterns;

        /// <summary>
        /// Parses <paramref name="odataOptions"/> used parameters and returns <see cref="ODataOperators"/> 
        /// that represents the set of operators used by this odataOptions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="odataOptions"></param>
        /// <returns>The <see cref="ODataOperators"/> representation of the operators in the OData options. 
        /// If no operator is used the result will be <see cref="ODataOperators.none"/>.</returns>
        public static ODataOperators ODataOptionsMap<T>(ODataQueryOptions<T> odataOptions)
        {
            ODataOperators result = ODataOperators.None;
            if(odataOptions == null)
            {
                return 0;
            }

            foreach (var odataOperator in Enum.GetNames(typeof(ODataOperators)))
            {
                var rawValuesProperty = typeof(ODataRawQueryOptions).GetProperty(odataOperator);
                if (rawValuesProperty != null && rawValuesProperty.GetValue(odataOptions.RawValues, null) != null)
                {
                    result |= (ODataOperators)Enum.Parse(typeof(ODataOperators), odataOperator);
                }
            }

            return result;
        }
    }
}