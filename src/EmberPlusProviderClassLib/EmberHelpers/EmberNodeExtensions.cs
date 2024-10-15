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

        private const string MATRIX_ID_TARGETS = "targets";
        private const string MATRIX_ID_SOURCES = "sources";
        private const string MATRIX_ID_LABELS = "labels";
        private const string UNUSED = "(unused)";

        public static EmberNode AddSubNode(this Node node, int index, string identifier, EmberPlusProvider provider)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            return new EmberNode(index, node, identifier, provider);
        }

        public static StringParameter AddStringParameter(this Node node, int index, string identifier, EmberPlusProvider provider, bool isWritable, string value = "", string description = "", bool isPersistable = false, Func<string, StringParameter, bool> remoteSetter = null)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            return new StringParameter(index, node, identifier, provider.dispatcher, isWritable, isPersistable, remoteSetter) { Value = value, Description = description };
        }

        public static BooleanParameter AddBooleanParameter(this Node node, int index, string identifier, EmberPlusProvider provider, bool isWritable, bool value = false, string description = "", bool isPersistable = false, Func<bool, BooleanParameter, bool> remoteSetter = null)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            return new BooleanParameter(index, node, identifier, provider.dispatcher, isWritable, isPersistable, remoteSetter) { Value = value, Description = description };
        }

        public static IntegerParameter AddIntegerParameter(this Node node, int index, string identifier, EmberPlusProvider provider, bool isWritable, int value = 0, int min = 0, int max = 255, string description = "", bool isPersistable = false, Func<long, IntegerParameter, bool> remoteSetter = null)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            return new IntegerParameter(index, node, identifier, provider.dispatcher, min, max, isWritable, isPersistable, remoteSetter) { Value = value, Description = description };
        }

        public static EnumParameter AddEnumParameter(this Node node, int index, string identifier, EmberPlusProvider provider, bool isWritable = false, IEnumerable<string> enumValues = null, int value = 0, string description = "", bool isPersistable = false, Func<long, EnumParameter, bool> remoteSetter = null)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            enumValues ??= new List<string>();
            return new EnumParameter(index, node, identifier, provider.dispatcher, enumValues, isWritable, isPersistable, remoteSetter) { Value = value, Description = description };
        }

        public static RealParameter AddRealParameter(this Node node, int index, string identifier, EmberPlusProvider provider, bool isWritable, double value = 0, double min = 0, double max = 255, string description = "", bool isPersistable = false, Func<double, RealParameter, bool> remoteSetter = null)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            return new RealParameter(index, node, identifier, provider.dispatcher, min, max, isWritable, isPersistable, remoteSetter) { Value = value, Description = description };
        }

        public static void AddFunction(this Node node, int index, string identifier, Tuple<string, int>[] arguments, Tuple<string, int>[] result, Func<GlowValue[], Task<GlowValue[]>> coreFunc)
        {
            NodeAsserter.AssertIdentifierValid(identifier);
            _ = new Function(index, node, identifier, arguments, result, coreFunc);
        }

        public static OneToNMatrix AddMatrixOneToN(this Node node, int index, string identifier, string[] sourceNames, string[] targetNames, EmberPlusProvider provider, bool isWritable = true, string description = "", string matrixIdentifier = "matrix", Func<Signal, IEnumerable<Signal>, Matrix, bool> remoteConnector = null)
        {

            Node oneToN = new(index, node, identifier, description);
            Node labels = new(1, oneToN, MATRIX_ID_LABELS);
            Node targetLabels = new(1, labels, MATRIX_ID_TARGETS);
            Node sourceLabels = new(2, labels, MATRIX_ID_SOURCES);

            List<Signal> sources = new();
            for (int i = 0; i < sourceNames.Length; i++)
            {
                StringParameter sourceParameter = new(i, sourceLabels, $"s-{i}", provider.dispatcher, isWritable: true, value: sourceNames[i]);
                sources.Add(new Signal(i, sourceParameter));
            }

            List<Signal> targets = new();
            for (int i = 0; i < targetNames.Length; i++)
            {
                StringParameter targetParameter = new(i, targetLabels, $"t-{i}", provider.dispatcher, isWritable: true, value: targetNames[i]);
                targets.Add(new Signal(i, targetParameter));
            }

            return new OneToNMatrix(2, oneToN, matrixIdentifier, provider.dispatcher, targets, sources, labels, isWritable, remoteConnector: remoteConnector);

        }

        public static OneToNMatrix AddMatrixOneToN(this Node node, int index, string identifier, Dictionary<int, string> sourceNames, Dictionary<int, string> targetNames, EmberPlusProvider provider, bool isWritable = true, string description = "", string matrixIdentifier = "matrix", Func<Signal, IEnumerable<Signal>, Matrix, bool> remoteConnector = null)
        {

            Node oneToN = new(index, node, identifier, description);
            Node labels = new(1, oneToN, MATRIX_ID_LABELS);
            Node targetLabels = new(1, labels, MATRIX_ID_TARGETS);
            Node sourceLabels = new(2, labels, MATRIX_ID_SOURCES);

            List<Signal> sources = new();
            for (int i = 0; i < sourceNames.Keys.Max() + 1; i++)
            {
                bool unused = !sourceNames.TryGetValue(i, out string sourceName);
                if (unused)
                    sourceName = UNUSED;
                StringParameter sourceParameter = new(i, sourceLabels, $"s-{i}", provider.dispatcher, isWritable: true, value: sourceName);
                sources.Add(new Signal(i, sourceParameter, unused));
            }

            List<Signal> targets = new();
            for (int i = 0; i < targetNames.Keys.Max() + 1; i++)
            {
                bool unused = !targetNames.TryGetValue(i, out string targetName);
                if (unused)
                    targetName = UNUSED;
                StringParameter targetParameter = new(i, targetLabels, $"t-{i}", provider.dispatcher, isWritable: true, value: targetName);
                targets.Add(new Signal(i, targetParameter, unused));
            }

            return new OneToNMatrix(2, oneToN, matrixIdentifier, provider.dispatcher, targets, sources, labels, isWritable, remoteConnector: remoteConnector);

        }

        public static OneToNBlindSourceMatrix AddMatrixOneToNBlindSource(this Node node, int index, string identifier, string[] sourceNames, string[] targetNames, string blindSourceName, EmberPlusProvider provider, bool isWritable = true, string description = "", string matrixIdentifier = "matrix", Func<Signal, IEnumerable<Signal>, Matrix, bool> remoteConnector = null)
        {

            Node oneToN = new(index, node, identifier, description);
            Node labels = new(1, oneToN, MATRIX_ID_LABELS);
            Node targetLabels = new(1, labels, MATRIX_ID_TARGETS);
            Node sourceLabels = new(2, labels, MATRIX_ID_SOURCES);

            List<Signal> sources = new();

            int blindIndex = 0;
            StringParameter blindParameter = new(blindIndex, sourceLabels, $"b-{blindIndex}", provider.dispatcher, isWritable: true, value: blindSourceName);
            Signal blindSignal = new(blindIndex, blindParameter);
            sources.Add(blindSignal);
            int numberOfBlinds = sources.Count;

            for (int i = 0; i < sourceNames.Length; i++)
            {
                StringParameter sourceParameter = new(i + numberOfBlinds, sourceLabels, $"s-{i}", provider.dispatcher, isWritable: true, value: sourceNames[i]);
                sources.Add(new Signal(i + numberOfBlinds, sourceParameter));
            }

            List<Signal> targets = new();
            for (int i = 0; i < targetNames.Length; i++)
            {
                StringParameter targetParameter = new(i, targetLabels, $"t-{i}", provider.dispatcher, isWritable: true, value: targetNames[i]);
                targets.Add(new Signal(i, targetParameter));
            }

            return new OneToNBlindSourceMatrix(2, oneToN, matrixIdentifier, provider.dispatcher, targets, sources, blindSignal, labels, isWritable, remoteConnector: remoteConnector);
        
        }

        public static OneToNBlindSourceMatrix AddMatrixOneToNBlindSource(this Node node, int index, string identifier, Dictionary<int, string> sourceNames, Dictionary<int, string> targetNames, string blindSourceName, EmberPlusProvider provider, bool isWritable = true, string description = "", string matrixIdentifier = "matrix", Func<Signal, IEnumerable<Signal>, Matrix, bool> remoteConnector = null)
        {

            Node oneToN = new(index, node, identifier, description);
            Node labels = new(1, oneToN, MATRIX_ID_LABELS);
            Node targetLabels = new(1, labels, MATRIX_ID_TARGETS);
            Node sourceLabels = new(2, labels, MATRIX_ID_SOURCES);

            List<Signal> sources = new();

            int blindIndex = 0;
            StringParameter blindParameter = new(blindIndex, sourceLabels, $"b-{blindIndex}", provider.dispatcher, isWritable: true, value: blindSourceName);
            Signal blindSignal = new(blindIndex, blindParameter);
            sources.Add(blindSignal);
            int numberOfBlinds = sources.Count;

            for (int i = 0; i < sourceNames.Keys.Max() + 1; i++)
            {
                bool unused = !sourceNames.TryGetValue(i, out string sourceName);
                if (unused)
                    sourceName = UNUSED;
                StringParameter sourceParameter = new(i + numberOfBlinds, sourceLabels, $"s-{i}", provider.dispatcher, isWritable: true, value: sourceName);
                sources.Add(new Signal(i + numberOfBlinds, sourceParameter));
            }

            List<Signal> targets = new();
            for (int i = 0; i < sourceNames.Keys.Max() + 1; i++)
            {
                bool unused = !targetNames.TryGetValue(i, out string targetName);
                if (unused)
                    targetName = UNUSED;
                StringParameter targetParameter = new(i, targetLabels, $"t-{i}", provider.dispatcher, isWritable: true, value: targetName);
                targets.Add(new Signal(i, targetParameter));
            }

            return new OneToNBlindSourceMatrix(2, oneToN, matrixIdentifier, provider.dispatcher, targets, sources, blindSignal, labels, isWritable, remoteConnector: remoteConnector);

        }

        public static T GetParameter<T>(this Node node, int index) where T : ParameterBase
            => node.ResolveChild(new int[] { index }, out IDynamicPathHandler _) as T;

        public static bool UpdateParameter(this Node node, int index, string newValue)
        {
            var p = node.GetParameter<StringParameter>(index);
            if ((p != null) && (p.Value != newValue))
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
            if ((p != null) && (p.Value != newValue))
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
            if ((p != null) && (p.Value != newValue))
            {
                Debug.WriteLine($"Setting node '{p.IdentifierPath}' to '{newValue}'");
                p.Value = newValue;
                return true;
            }
            return false;
        }

    }
}