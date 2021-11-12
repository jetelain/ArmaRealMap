using System.Net;
using SRTM.Logging;

// The MIT License (MIT)

// Copyright (c) 2019, Tadas Juščius

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace SRTM.Sources.NASA
{
    /// <summary>
    /// Defines a NASA source of data.
    /// </summary>
    public class NASASource : ISRTMSource
    {
        private NetworkCredential _credentials;

        public NASASource(NetworkCredential credentials)
        {
            _credentials = credentials;
        }
        
        /// <summary>
        /// The source of the data.
        /// </summary>
        public const string SOURCE = @"http://e4ftl01.cr.usgs.gov/MEASURES/SRTMGL1.003/2000.02.11/";
 
        /// <summary>
        /// Gets the missing cell.
        /// </summary>
        public bool GetMissingCell(string path, string name)
        {
            var filename = name + ".SRTMGL1.hgt.zip";
            var local = System.IO.Path.Combine(path, name + ".hgt.zip");
            
            var Logger = LogProvider.For<SRTMData>();
            Logger.Info("Downloading {0} ...", name);

            return SourceHelpers.DownloadWithCredentials(_credentials, local, SOURCE + filename);
        }
    }
}