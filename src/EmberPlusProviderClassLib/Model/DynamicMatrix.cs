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
using EmberLib.Glow;

namespace EmberPlusProviderClassLib.Model
{
    public class DynamicMatrix : Matrix, IDynamicPathHandler
    {
        public DynamicMatrix(int number,
                            Element parent,
                            string identifier,
                            Dispatcher dispatcher,
                            IEnumerable<Signal> targets,
                            IEnumerable<Signal> sources,
                            Node labelsNode,
                            bool? isWritable = true)
        : base(number, parent, identifier, dispatcher, targets, sources, labelsNode, isWritable, null, null, null)
        {
            _xpointParameters = new Dictionary<int,Dictionary<int,XpointParams>>();
            foreach(var target in targets)
            {
                var dict = new Dictionary<int, XpointParams>();

                foreach(var source in sources)
                    dict.Add(source.Number, new XpointParams());

                _xpointParameters.Add(target.Number, dict);
            }
        }

        public int ParametersSubIdentifier => s_parametersSubIdentifier;

        protected override bool ConnectOverride(Signal target, IEnumerable<Signal> sources, ConnectOperation operation)
        {
            if(operation == ConnectOperation.Disconnect)
                target.Disconnect(sources);
            else
                target.Connect(sources, operation == ConnectOperation.Absolute);

            return true;
        }

        public override TResult Accept<TState, TResult>(IElementVisitor<TState, TResult> visitor, TState state)
        {
            return visitor.Visit(this, state);
        }

        static readonly int s_parametersSubIdentifier = 0;
        Dictionary<int, Dictionary<int, XpointParams>> _xpointParameters;

        #region IDynamicPathHandler Members
        void IDynamicPathHandler.HandleParameter(GlowParameterBase parameter, int[] path, Client source)
        {
            var offset = Path.Length;

            if(path.Length == offset + 5
            && path[offset + 0] == ParametersSubIdentifier
            && path[offset + 1] == 3) // connections
            {
                Dictionary<int, XpointParams> dict;

                if(_xpointParameters.TryGetValue(path[offset + 2], out dict)) // target
                {
                    XpointParams xpointParams;

                    if(dict.TryGetValue(path[offset + 3], out xpointParams)) // source
                    {
                        if(path[offset + 4] == 1) // gain
                        {
                            var value = parameter.Value;

                            if(value != null
                            && value.Type == GlowParameterType.Real)
                            {
                                xpointParams.Gain = value.Real;

                                Dispatcher.NotifyParameterValueChanged(path, new GlowValue(xpointParams.Gain));
                            }
                        }
                    }
                }
            }
        }

        void IDynamicPathHandler.HandleCommand(GlowCommand command, int[] path, Client source)
        {
            if(command.Number == GlowCommandType.GetDirectory)
            {
                var offset = Path.Length;

                if(path.Length == offset + 4
                && path[offset + 0] == ParametersSubIdentifier
                && path[offset + 1] == 3) // connections
                {
                    Dictionary<int, XpointParams> dict;

                    if(_xpointParameters.TryGetValue(path[offset + 2], out dict)) // target
                    {
                        XpointParams xpointParams;

                        if(dict.TryGetValue(path[offset + 3], out xpointParams)) // source
                        {
                            var gainPath = path.Concat(new[] { 1 }).ToArray();

                            var glow = new GlowQualifiedParameter(gainPath)
                            {
                                Identifier = "dynamicGain",
                                Value = new GlowValue(xpointParams.Gain),
                                Minimum = new GlowMinMax(XpointParams.MinimumGain),
                                Maximum = new GlowMinMax(XpointParams.MaximumGain),
                            };

                            var root = GlowRootElementCollection.CreateRoot();
                            root.Insert(glow);
                            source.Write(root);
                        }
                    }
                }
            }
        }
        #endregion
    }

    class XpointParams
    {
        public const double MinimumGain = -128;
        public const double MaximumGain = 15;

        public XpointParams()
        {
            Gain = MinimumGain;
        }

        public double Gain { get; set; }
    }
}