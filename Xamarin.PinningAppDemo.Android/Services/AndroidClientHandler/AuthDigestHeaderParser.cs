//
// Xamarin.Android SDK
//
// The MIT License (MIT)
//
// Copyright (c) .NET Foundation Contributors
//
// All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
//
// Adapted from:
//
// System.Net.DigestClient.cs
//                                      
// Authors:
//      Greg Reinacker (gregr@rassoc.com)
//      Sebastien Pouliot (spouliot@motus.com)
//      Gonzalo Paniagua Javier (gonzalo@ximian.com
//
// Copyright 2002-2003 Greg Reinacker, Reinacker & Associates, Inc. All rights reserved.
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (c) 2003 Novell, Inc. (http://www.novell.com)
//
// Original (server-side) source code available at
// http://www.rassoc.com/gregr/weblog/stories/2002/07/09/webServicesSecurityHttpDigestAuthenticationWithoutActiveDirectory.html
//
using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.PinningAppDemo.Droid.Services.AndroidClientHandler
{
	sealed class AuthDigestHeaderParser
	{
		const string REALM = "realm";
		const string OPAQUE = "opaque";
		const string NONCE = "nonce";
		const string ALGORITHM = "algorithm";
		const string QOP_ = "qop";

		static readonly Dictionary<string, string> keywords =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				[REALM] = null,
				[OPAQUE] = null,
				[NONCE] = null,
				[ALGORITHM] = null,
				[QOP_] = null
			};

		string header;
		int length;
		int pos;

		public string Realm
		{
			get { return keywords[REALM]; }
		}

		public string Opaque
		{
			get { return keywords[OPAQUE]; }
		}

		public string Nonce
		{
			get { return keywords[NONCE]; }
		}

		public string Algorithm
		{
			get { return keywords[ALGORITHM]; }
		}

		public string QOP
		{
			get { return keywords[QOP_]; }
		}

		public AuthDigestHeaderParser(string header)
		{
			this.header = header?.Trim();
		}

		public bool Parse()
		{
			if (header == null || !header.StartsWith("digest ", StringComparison.OrdinalIgnoreCase))
				return false;

			pos = "digest".Length;
			length = header.Length;
			while (pos < length)
			{
				string key, value;
				if (!GetKeywordAndValue(out key, out value))
					return false;

				SkipWhitespace();
				if (pos < length && header[pos] == ',')
					pos++;

				if (!keywords.ContainsKey(key))
					continue;

				if (keywords[key] != null)
					return false;

				keywords[key] = value;
			}

			if (Realm == null || Nonce == null)
				return false;

			return true;
		}

		void SkipWhitespace()
		{
			char c = ' ';
			while (pos < length && (c == ' ' || c == '\t' || c == '\r' || c == '\n'))
			{
				c = header[pos++];
			}

			pos--;
		}

		string GetKey()
		{
			SkipWhitespace();
			int begin = pos;
			while (pos < length && header[pos] != '=')
			{
				pos++;
			}

			return header.Substring(begin, pos - begin).Trim().ToLowerInvariant();
		}

		bool GetKeywordAndValue(out string key, out string value)
		{
			key = null;
			value = null;
			key = GetKey();
			if (pos >= length)
				return false;

			SkipWhitespace();
			if (pos + 1 >= length || header[pos++] != '=')
				return false;

			SkipWhitespace();
			// note: Apache doesn't use " in all case (like algorithm)
			if (pos + 1 >= length)
				return false;

			bool useQuote = false;
			if (header[pos] == '"')
			{
				pos++;
				useQuote = true;
			}

			int beginQ = pos;
			if (useQuote)
			{
				pos = header.IndexOf('"', pos);
				if (pos == -1)
					return false;
			}
			else
			{
				do
				{
					char c = header[pos];
					if (c == ',' || c == ' ' || c == '\t' || c == '\r' || c == '\n')
						break;
				} while (++pos < length);

				if (pos >= length && beginQ == pos)
					return false;
			}

			value = header.Substring(beginQ, pos - beginQ);
			pos += useQuote ? 2 : 1;
			return true;
		}
	}
}