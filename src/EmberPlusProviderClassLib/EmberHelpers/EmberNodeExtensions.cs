#region copyright
/*
 * Larkspur Ember Plus Provider
 *
 * Copyright (c) 2020 Roger Sandholm & Fredrik Bergholtz, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion copyright

using System;
using System.Linq;
using System.Threading.Tasks;
using EmberLib.Glow;
using EmberPlusProviderClassLib.Model;
using EmberPlusProviderClassLib.Model.Parameters;
using System.Diagnostics;
using System.Collections.Generic;

namespace EmberPlusProviderClassLib.EmberHelpers
{
    public static class EmberNodeExtensions
    {

        public static EmberNode AddSubNode(this Node node, int index, string identifier, EmberPlusProvider provider)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            return new EmberNode(index, node, identifier, provider);
        }

        public static StringParameter AddStringParameter(this Node node, int index, string identifier, EmberPlusProvider provider, bool isWritable, string value = "", string description = "", bool isPersistable = false)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            return new StringParameter(index, node, identifier, provider.dispatcher, isWritable, isPersistable) { Value = value, Description = description };
        }

        public static BooleanParameter AddBooleanParameter(this Node node, int index, string identifier, EmberPlusProvider provider, bool isWritable, bool value = false, string description = "", bool isPersistable = false)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            return new BooleanParameter(index, node, identifier, provider.dispatcher, isWritable, isPersistable) { Value = value, Description = description };
        }

        public static IntegerParameter AddIntegerParameter(this Node node, int index, string identifier, EmberPlusProvider provider, bool isWritable, int value = 0, int min = 0, int max = 255, string description = "", bool isPersistable = false)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            return new IntegerParameter(index, node, identifier, provider.dispatcher, min, max, isWritable, isPersistable) { Value = value, Description = description };
        }

        public static EnumParameter AddEnumParameter(this Node node, int index, string identifier, EmberPlusProvider provider, bool isWritable = false, IEnumerable<string> enumValues = null, int value = 0, string description = "", bool isPersistable = false)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            enumValues ??= new List<string>();
            return new EnumParameter(index, node, identifier, provider.dispatcher, enumValues, isWritable, isPersistable) { Value = value, Description = description };
        }

        public static RealParameter AddRealParameter(this Node node, int index, string identifier, EmberPlusProvider provider, bool isWritable, double value = 0, double min = 0, double max = 255, string description = "", bool isPersistable = false)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            return new RealParameter(index, node, identifier, provider.dispatcher, min, max, isWritable, isPersistable) { Value = value, Description = description };
        }

        public static void AddFunction(this Node node, int index, string identifier, Tuple<string, int>[] arguments, Tuple<string, int>[] result, Func<GlowValue[], Task<GlowValue[]>> coreFunc)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            new Function(index, node, identifier, arguments, result, coreFunc);
        }

        public static OneToNMatrix AddMatrixOneToN(this Node node, int index, string identifier, string[] sourceNames, string[] targetNames, EmberPlusProvider provider, bool isWritable = true, string description = "", string matrixIdentifier = "matrix")
        {
            
            var oneToN = new Node(index, node, identifier )
            {
                Description = description,
            };

            var labels = new Node(1, oneToN, "labels")
            {
                //SchemaIdentifier = "de.l-s-b.emberplus.matrix.labels"
            };

            var targetLabels = new Node(1, labels, "targets");
            var sourceLabels = new Node(2, labels, "sources");

            var targets = new List<Signal>();
            var sources = new List<Signal>();

            for (int number = 0; number < sourceNames.Length; number++)
            {
                var sourceParameter = new StringParameter(number, sourceLabels, $"s-{number}", provider.dispatcher, isWritable: true)
                {
                    Value = sourceNames[number]
                };

                sources.Add(new Signal(number, sourceParameter));
            }
            for (int number = 0; number < targetNames.Length; number++)
            {
                var targetParameter = new StringParameter(number, targetLabels, $"t-{number}", provider.dispatcher, isWritable: true)
                {
                    Value = targetNames[number]
                };

                targets.Add(new Signal(number, targetParameter));
            }
            var matrix = new OneToNMatrix(
               2,
               oneToN,
               matrixIdentifier,
               provider.dispatcher,
               targets,
               sources,
               labels,
               isWritable)
            {
                //SchemaIdentifier = "de.l-s-b.emberplus.matrix.oneToN"
            };

            //foreach (var target in matrix.Targets)
            //    matrix.Connect(target, new[] { matrix.GetSource(target.Number) }, null);
            return matrix;
        }

        public static OneToNBlindSourceMatrix AddMatrixOneToNBlindSource(this Node node, int index, string identifier, string[] sourceNames, string[] targetNames, string blindSourceName, EmberPlusProvider provider, bool isWritable = true, string description = "", string matrixIdentifier = "matrix")
        {

            var oneToN = new Node(index, node, identifier)
            {
                Description = description,
            };

            var labels = new Node(1, oneToN, "labels")
            {
                //SchemaIdentifier = "de.l-s-b.emberplus.matrix.labels"
            };

            var targetLabels = new Node(1, labels, "targets");
            var sourceLabels = new Node(2, labels, "sources");

            var targets = new List<Signal>();
            var sources = new List<Signal>();

            // Add the blind source
            var blindIndex = 0;
            var blindParameter = new StringParameter(blindIndex, sourceLabels, $"b-{blindIndex}", provider.dispatcher, isWritable: true)
            {
                Value = blindSourceName
            };
            var blindSignal = new Signal(blindIndex, blindParameter);
            sources.Add(blindSignal);

            // Add sources
            var numberOfBlinds = sources.Count();
            for (int number = 0; number < sourceNames.Length; number++)
            {
                var sourceParameter = new StringParameter(number + numberOfBlinds, sourceLabels, $"s-{number}", provider.dispatcher, isWritable: true)
                {
                    Value = sourceNames[number]
                };

                sources.Add(new Signal(number + numberOfBlinds, sourceParameter));
            }

            // Add targets
            for (int number = 0; number < targetNames.Length; number++)
            {
                var targetParameter = new StringParameter(number, targetLabels, $"t-{number}", provider.dispatcher, isWritable: true)
                {
                    Value = targetNames[number]
                };

                targets.Add(new Signal(number, targetParameter));
            }

            var matrix = new OneToNBlindSourceMatrix(
               2,
               oneToN,
               matrixIdentifier,
               provider.dispatcher,
               targets,
               sources,
               blindSignal,
               labels,
               isWritable)
            {
                //SchemaIdentifier = "de.l-s-b.emberplus.matrix.oneToN"
            };

            //foreach (var target in matrix.Targets)
            //    matrix.Connect(target, new[] { matrix.BlindSource }, null);

            return matrix;
        }

        public static T GetParameter<T>(this Node node, int index) where T : ParameterBase
        {
            IDynamicPathHandler dph;
            return node.ResolveChild(new int[] { index }, out dph) as T;
        }

        public static bool UpdateParameter(this Node node, int index, string newValue)
        {
            var p = node.GetParameter<StringParameter>(index);
            if (p != null && p.Value != newValue)
            {
                Debug.WriteLine($"Setting node '{p.IdentifierPath}' to '{newValue}'");
                p.Value = newValue;
                return true;
            }
            return false;
        }

        public static bool UpdateParameter(this Node node, int index, bool newValue)
        {
            var p = node.GetParameter<BooleanParameter>(index);
            if (p != null && p.Value != newValue)
            {
                Debug.WriteLine($"Setting node '{p.IdentifierPath}' to '{newValue}'");
                p.Value = newValue;
                return true;
            }
            return false;
        }

        public static bool UpdateParameter(this Node node, int index, long newValue)
        {
            var p = node.GetParameter<IntegerParameter>(index);
            if (p != null && p.Value != newValue)
            {
                Debug.WriteLine($"Setting node '{p.IdentifierPath}' to '{newValue}'");
                p.Value = newValue;
                return true;
            }
            return false;
        }

    }
}