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

using System;
using System.Linq;
using System.Threading.Tasks;
using EmberLib.Glow;
using System.Diagnostics;

namespace EmberPlusProviderClassLib.Model
{
    public class Function : Element
    {
        public Function(int number, Element parent, string identifier, Tuple<string, int>[] arguments, Tuple<string, int>[] result, Func<GlowValue[], Task<GlowValue[]>> coreFunc)
        : base(number, parent, identifier)
        {
            Arguments = arguments;
            Result = result;
            _coreFunc = coreFunc;
        }

        public async Task<GlowInvocationResult> Invoke(GlowInvocation invocation)
        {
            GlowValue[] arguments;
            int? invocationId;

            if (invocation == null)
            {
                if (Arguments != null)
                    throw new ArgumentException("Function with parameters called without arguments!");

                arguments = null;
                invocationId = null;
            }
            else
            {
                var argumentValues = invocation.ArgumentValues;

                arguments = argumentValues != null
                            ? argumentValues.ToArray()
                            : null;

                // **********************************************
                // Added code
                // Functions without parameters can be defined in two (2) ways. Either 'args=null' or 'args=empty list'
                // - EmberViewer v1.6.2 sends args=null when function parameter is missing
                // - EmberViewer v2.4.0 sends args=empty list when function parameter is missing
                // So that both models will work, we create an empty list if args=null
                // and we create args=null if args=empty list
                if (Arguments != null && Arguments.Length == 0 && arguments == null)
                {
                    arguments = new GlowValue[0];
                }
                if (Arguments == null && arguments != null && arguments.Length == 0)
                {
                    arguments = null;
                }
                // ************************************************

                invocationId = invocation.InvocationId;

                AssertValueTypes(arguments, Arguments);
            }

            if (invocationId == null && HasResult)
                throw new ArgumentException("Function with result called without invocation id!");

            Debug.WriteLine($"Invoking function {IdentifierPath}");

            var result = await _coreFunc(arguments);

            AssertValueTypes(result, Result);

            if (invocationId != null)
            {
                var invocationResult = GlowInvocationResult.CreateRoot(invocationId.Value);

                if (result != null)
                    invocationResult.ResultValues = result;

                return invocationResult;
            }

            return null;
        }

        public Tuple<string, int>[] Arguments { get; private set; }
        public Tuple<string, int>[] Result { get; private set; }

        public bool HasResult => Result != null;

        public override TResult Accept<TState, TResult>(IElementVisitor<TState, TResult> visitor, TState state)
        {
            return visitor.Visit(this, state);
        }

        public static Tuple<string, int> CreateStringArgument(string name)
        {
            return Tuple.Create(name, GlowParameterType.String);
        }

        public static Tuple<string, int> CreateBooleanArgument(string name)
        {
            return Tuple.Create(name, GlowParameterType.Boolean);
        }

        public static Tuple<string, int> CreateIntegerArgument(string name)
        {
            return Tuple.Create(name, GlowParameterType.Integer);
        }

        public static GlowValue CreateArgumentValue(bool value)
        {
            return new GlowValue(value);
        }

        public static GlowValue CreateArgumentValue(int value)
        {
            return new GlowValue(value);
        }

        public static GlowValue[] CreateResult(bool value)
        {
            return new[] { CreateArgumentValue(value) };
        }

        readonly Func<GlowValue[], Task<GlowValue[]>> _coreFunc;

        void AssertValueTypes(GlowValue[] values, Tuple<string, int>[] expected)
        {
            if (expected == null)
            {
                if (values != null)
                    throw new ArgumentException();
            }
            else
            {
                if (values.Length != expected.Length)
                    throw new ArgumentException();

                for(int index = 0; index < values.Length; index++)
                {
                    if (values[index].Type != expected[index].Item2)
                        throw new ArgumentException();
                }
            }
        }
    }
}