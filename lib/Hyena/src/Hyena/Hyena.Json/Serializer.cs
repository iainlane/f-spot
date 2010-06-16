//
// Serializer.cs
//
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
//
// Authors:
//   Sandy Armstrong <sanfordarmstrong@gmail.com>
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hyena.Json
{
    public class Serializer
    {
        private const string serializedNull = "null";
        private const string serializedTrue = "true";
        private const string serializedFalse = "false";

        private object input;

        public Serializer () { }
        public Serializer (object input) { SetInput (input); }

        public void SetInput (object input)
        {
            this.input = input;
        }

        // TODO: Support serialize to stream?

        public string Serialize ()
        {
            return Serialize (input);
        }

        private string SerializeBool (bool val)
        {
            return val ? serializedTrue : serializedFalse;
        }

        private string SerializeInt (int val)
        {
            return val.ToString ();
        }

        private string SerializeDouble (double val)
        {
            return val.ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        // TODO: exponent stuff

        private string SerializeString (string val)
        {
            // TODO: More work, escaping, etc
            return "\"" +
                val.Replace ("\\", "\\\\").
                    Replace ("\"", "\\\"").
                    Replace ("\b", "\\b").
                    Replace ("\f", "\\f").
                    Replace ("\n", "\\n").
                    Replace ("\r", "\\r").
                    Replace ("\t", "\\t") +
                "\"";
        }

        private string SerializeEnumerable (IEnumerable array)
        {
            StringBuilder builder = new StringBuilder ("[");
            int i = 0;
            foreach (var obj in array) {
                builder.Append (Serialize (obj));
                builder.Append (",");
                i++;
            }
            // Get rid of trailing comma
            if (i > 0)
                builder.Remove (builder.Length - 1, 1);
            builder.Append ("]");
            return builder.ToString ();
        }

        private string SerializeDictionary (Dictionary<string, object> obj)
        {
            StringBuilder builder = new StringBuilder ("{");
            foreach (var pair in obj) {
                builder.Append (SerializeString (pair.Key));
                builder.Append (":");
                builder.Append (Serialize (pair.Value));
                builder.Append (",");
            }
            // Get rid of trailing comma
            if (obj.Count > 0)
                builder.Remove (builder.Length - 1, 1);
            builder.Append ("}");
            return builder.ToString ();
        }

        private string Serialize (object unknownObj)
        {
            if (unknownObj == null)
                return serializedNull;

            bool? b = unknownObj as bool?;
            if (b.HasValue)
                return SerializeBool (b.Value);

            int? i = unknownObj as int?;
            if (i.HasValue)
                return SerializeInt (i.Value);

            double? d = unknownObj as double?;
            if (d.HasValue)
                return SerializeDouble (d.Value);

            string s = unknownObj as string;
            if (s != null)
                return SerializeString (s);

            var o = unknownObj as Dictionary<string, object>;
            if (o != null)
                return SerializeDictionary (o);

            var a = unknownObj as IEnumerable;
            if (a != null)
                return SerializeEnumerable (a);

            throw new ArgumentException ("Cannot serialize anything but doubles, integers, strings, JsonObjects, and JsonArrays");
        }
    }
}
