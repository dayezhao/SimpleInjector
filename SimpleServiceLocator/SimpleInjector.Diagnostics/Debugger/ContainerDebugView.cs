﻿#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace SimpleInjector.Diagnostics.Debugger
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using SimpleInjector.Diagnostics.Analyzers;
    
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "This type is loaded by the SimpleInjector.dll when running in the VS debugger.")]
    internal sealed class ContainerDebugView
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Container container;
        
        public ContainerDebugView(Container container)
        {
            this.container = container;

            this.Initialize();
        }

        public ContainerOptions Options
        {
            get { return this.container.Options; }
        }

        [DebuggerDisplay("")]
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public DebuggerViewItem[] Items { get; private set; }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = @"
                We must catch all exceptions here, because this constructor is called by the Visual Studio 
                debugger and it won't hide any failure in case of an exception. We catch and show the 
                exception in the debug view instead.")]
        private void Initialize()
        {
            if (!this.container.SuccesfullyVerified)
            {
                this.Items = new[]
                {
                    new DebuggerViewItem(
                        name: "How To View Diagnostic Info",
                        description: "Analysis info is available in this debug view after Verify() is " + 
                            "called on this container instance.")
                };

                return;
            }

            try
            {
                this.Items = this.GetAnalysisResults().ToArray();
            }
            catch (Exception ex)
            {
                this.Items = GetDebuggerTypeProxyFailureResults(ex);
            }
        }

        private DebuggerViewItem[] GetAnalysisResults()
        {
            var registrations = this.container.GetCurrentRegistrations();

            return new DebuggerViewItem[]
            {
                DebuggerGeneralWarningsContainerAnalyzer.Analyze(this.container),
                new DebuggerViewItem(
                    name: "Registrations",
                    description: "Count = " + registrations.Length,
                    value: registrations)
            };
        }

        private static DebuggerViewItem[] GetDebuggerTypeProxyFailureResults(Exception ex)
        {
            return new[] 
            {
                new DebuggerViewItem(
                    "Failure", 
                    "We're so so sorry. The Debugger Type Proxy failed to initialize.", 
                    ex)
            };
        }
    }
}