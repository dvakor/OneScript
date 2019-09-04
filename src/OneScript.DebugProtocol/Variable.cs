/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OneScript.DebugProtocol
{
    [DataContract]
    public class Variable
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsStructured { get; set; }

        [DataMember]
        public string Presentation { get; set; }

        [DataMember]
        public string TypeName { get; set; }

        [DataMember]
        public List<Variable> ChildVariables { get; set; }

        public int ChildrenHandleID { get; set; }

        
    }
}
