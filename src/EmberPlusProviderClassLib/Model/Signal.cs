﻿#region copyright
/*
 * This code is from the Lawo/ember-plus GitHub repository and is licensed with
 *
 * Boost Software License - Version 1.0 - August 17th, 2003
 *
 * Permission is hereby granted, free of charge, to any person or organization
 * obtaining a copy of the software and accompanying documentation covered by
 * this license (the "Software") to use, reproduce, display, distribute,
 * execute, and transmit the Software, and to prepare derivative works of the
 * Software, and to permit third-parties to whom the Software is furnished to
 * do so, all subject to the following:
 *
 * The copyright notices in the Software and this entire statement, including
 * the above license grant, this restriction and the following disclaimer,
 * must be included in all copies of the Software, in whole or in part, and
 * all derivative works of the Software, unless such copies or derivative
 * works are solely in the form of machine-executable object code generated by
 * a source language processor.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE, TITLE AND NON-INFRINGEMENT. IN NO EVENT
 * SHALL THE COPYRIGHT HOLDERS OR ANYONE DISTRIBUTING THE SOFTWARE BE LIABLE
 * FOR ANY DAMAGES OR OTHER LIABILITY, WHETHER IN CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
 #endregion

using System.Collections.Generic;
using System.Linq;
using EmberPlusProviderClassLib.Model.Parameters;

namespace EmberPlusProviderClassLib.Model
{
    public class Signal
    {
        public Signal(int number, StringParameter labelParameter, bool unused = false)
        {
            Number = number;
            LabelParameter = labelParameter;
            Unused = unused;
        }

        public int Number { get; }
        public StringParameter LabelParameter { get; }
        public bool Unused { get; }

        public IEnumerable<Signal> ConnectedSources => _connectedSources;

        public int ConnectedSourcesCount => _connectedSources.Count;

        public bool HasConnectedSources => _connectedSources.Count > 0;

        public void Connect(IEnumerable<Signal> sources, bool isAbsolute)
        {
            if (isAbsolute)
            {
                _connectedSources.Clear();
                _connectedSources.AddRange(sources.Where(s => !s.Unused));
            }
            else
            {
                foreach(var source in sources)
                {
                    if(_connectedSources.Contains(source) == false)
                        _connectedSources.Add(source);
                }
            }
        }

        public void Disconnect(IEnumerable<Signal> sources)
        {
            foreach(var signal in sources)
                _connectedSources.Remove(signal);
        }

        readonly List<Signal> _connectedSources = new List<Signal>();
    }
}