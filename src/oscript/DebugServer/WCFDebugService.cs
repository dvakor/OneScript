/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using OneScript.DebugProtocol;
using OneScript.Language;
using ScriptEngine;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.HostedScript.Library.ValueTable;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using StackFrame = OneScript.DebugProtocol.StackFrame;
using Variable = OneScript.DebugProtocol.Variable;

namespace oscript.DebugServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    internal class WcfDebugService : IDebuggerService
    {
        private WcfDebugController Controller { get; }
        
        public WcfDebugService(WcfDebugController controller)
        {
            Controller = controller;
        }

        #region WCF Communication methods

        public void Execute(int threadId)
        {
            RegisterEventListener();
            if (threadId > 0)
            {
                var token = Controller.GetTokenForThread(threadId);
                token.Machine.PrepareDebugContinuation();
                token.ThreadEvent.Set();        
            }
            else
            {
                var tokens = Controller.GetAllTokens();
                foreach (var token in tokens)
                {
                    token.Machine.PrepareDebugContinuation();
                    token.ThreadEvent.Set();
                }
            }
        }

        private void RegisterEventListener()
        {
            var eventChannel = OperationContext.Current.
                   GetCallbackChannel<IDebugEventListener>();

            Controller.SetCallback(eventChannel);
        }

        public Breakpoint[] SetMachineBreakpoints(Breakpoint[] breaksToSet)
        {
            var confirmedBreakpoints = new List<Breakpoint>();

            var grouped = breaksToSet.GroupBy(g => g.Source);

            foreach (var item in grouped)
            {
                var lines = item
                    .Where(x => x.Line != 0)
                    .Select(x => x.Line)
                    .ToArray();

                foreach (var machine in Controller.GetAllTokens().Select(x=>x.Machine))
                {
                    machine.SetBreakpointsForModule(item.Key, lines);
                }

                foreach (var line in lines)
                {
                    confirmedBreakpoints.Add(new Breakpoint()
                    {
                        Line = line,
                        Source = item.Key
                    });
                }

            }

            return confirmedBreakpoints.ToArray();
        }

        public StackFrame[] GetStackFrames(int threadId)
        {
            var machine = Controller.GetTokenForThread(threadId).Machine;
            var frames = machine.GetExecutionFrames();
            var result = new StackFrame[frames.Count];
            int index = 0;
            foreach (var frameInfo in frames)
            {
                var frame = new StackFrame();
                frame.LineNumber = frameInfo.LineNumber;
                frame.Index = index++;
                frame.MethodName = frameInfo.MethodName;
                frame.Source = frameInfo.Source;
                result[frame.Index] = frame;

            }
            return result;
        }

        public OneScript.DebugProtocol.Variable[] GetVariables(int threadId, int frameId, int[] path)
        {
            var machine = Controller.GetTokenForThread(threadId).Machine;
            var locals = machine.GetFrameLocals(frameId);
            //var _locals = new List<IVariable>();
            foreach (var step in path)
            {
                var variable = locals[step];
                bool hasProps = HasProperties(variable);
                if (hasProps)
                {
                    var obj = variable.AsObject();
                    locals = new List<IVariable>();
                    var propsCount = obj.GetPropCount();
                    for (int i = 0; i < propsCount; i++)
                    {
                        string propName = obj.GetPropName(i);

                        try
                        {
                            locals.Add(ScriptEngine.Machine.Variable.Create(obj.GetPropValue(i), propName));
                        }
                        catch (Exception e)
                        {
                            locals.Add(ScriptEngine.Machine.Variable.Create(ValueFactory.Create(e.Message), propName));
                        }

                    }
                }
                //if (IsIndexed(variable))
                //{
                //    var obj = variable.AsObject();
                //    if (obj is IRuntimeContextInstance cntx)
                //    {
                //        if (obj is ICollectionContext collection)
                //        {
                //            if(!hasProps)
                //                locals = new List<IVariable>();
                //            for (int i = 0; i < collection.Count(); i++)
                //            {
                //                try
                //                {
                //                    var currentVal = cntx.GetIndexedValue(ValueFactory.Create(i));
                //                }
                //                catch (Exception)
                //                {
                //                    break;
                //                }
                //                try
                //                {
                //                    locals.Add(ScriptEngine.Machine.Variable.Create(obj.GetPropValue(i), i.ToString()));
                //                }
                //                catch (Exception e)
                //                {
                //                    locals.Add(ScriptEngine.Machine.Variable.Create(ValueFactory.Create(e.Message), i.ToString()));
                //                }
                //            }
                //        }
                //    }
                //}
            }

            var result = new OneScript.DebugProtocol.Variable[locals.Count];
            for (int i = 0; i < locals.Count; i++)
            {
                result[i] = new OneScript.DebugProtocol.Variable()
                {
                    Name = locals[i].Name,
                    IsStructured = HasProperties(locals[i]),
                    Presentation = locals[i].AsString(),
                    TypeName = locals[i].SystemType.Name
                };
            }

            return result;
        }



        public Variable Evaluate(int threadId, int contextFrame, string expression)
        {
            try
            {
               return new Variable()
               {
                    Name = "Test",
                    IsStructured = true,
                    Presentation = "Структура",
                    TypeName = "Структура"
               };
                var value = GetMachine(threadId).Evaluate(expression, true);
                //return GetVariable(value);
            }
            catch (ScriptException e)
            {
                return new Variable()
                {
                    Name = "$evalFault",
                    Presentation = e.ErrorDescription + e.StackTrace,
                    TypeName = "Ошибка",
                    IsStructured = false,
                    IsIndexed = false
                };
            }
        }

        public void Next(int threadId)
        {
            var t = Controller.GetTokenForThread(threadId);
            t.Machine.StepOver();
            t.ThreadEvent.Set();
        }

        public void StepIn(int threadId)
        {
            var t = Controller.GetTokenForThread(threadId);
            t.Machine.StepIn();
            t.ThreadEvent.Set();
        }

        public void StepOut(int threadId)
        {
            var t = Controller.GetTokenForThread(threadId);
            t.Machine.StepOut();
            t.ThreadEvent.Set();
        }

        public int[] GetThreads()
        {
            return Controller.GetAllThreadIds();
        }

        private MachineInstance GetMachine(int threadId)
        {
            return Controller.GetTokenForThread(threadId).Machine;
        }

        private Variable GetVariable(IValue value, string name = "$evalResult")
        {
            var variable = new Variable()
            {
                Name = name,
                Presentation = value.AsString(),
                TypeName = value.SystemType.Name,
                IsStructured = HasProperties(value),
                IsIndexed = IsIndexed(value)
            };
            if (variable.IsStructured)
                variable.NamedVariables = NamedVariables(value);
            if (variable.IsIndexed)
                variable.IndexedVariables = IndexedVariables(value);
            return variable;
        }

        private bool IsIndexed(IValue value)
        {
            var rawObj = value.GetRawValue();
            if (rawObj is IRuntimeContextInstance cntx)
                return cntx.IsIndexed;
            return false;
        }

        private List<Variable> IndexedVariables(IValue variable)
        {
            List<Variable> result = new List<Variable>();

            var rawObj = variable.GetRawValue();

            if (rawObj is IRuntimeContextInstance cntx)
            {
                if(rawObj is ICollectionContext collection)
                {
                    for (int i = 0; i < collection.Count(); i++)
                    {
                        try
                        {
                            var currentVal = cntx.GetIndexedValue(ValueFactory.Create(i));
                            result.Add(GetVariable(currentVal, i.ToString()));
                        }
                        catch (Exception e)
                        {
                            result.Add(new Variable() {
                                Name = i.ToString(),
                                Presentation = e.Message
                            });
                        }
                        
                    }
                }
            }

            return result;
        }

        private List<Variable> NamedVariables(IValue variable)
        {
            List<Variable> result = new List<Variable>();
            var obj = variable.AsObject();
            var props = obj.GetProperties();
            foreach (var prop in props)
            {
                var propVal = obj.GetPropValue(prop.Index);
                try
                {
                    var str = propVal.AsString();
                }
                catch (Exception)
                {
                    continue;
                }
                result.Add(GetVariable(propVal, prop.Identifier));
            }
            return result;
        }

        private static bool HasProperties(IValue variable)
        {
            if (variable.DataType == DataType.Object)
            {
                var obj = variable.AsObject();
                return obj.GetPropCount() > 0;
            }

            return false;
        }

        #endregion
    }
}
