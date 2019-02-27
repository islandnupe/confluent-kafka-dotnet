// Copyright 2019 Confluent Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Refer to LICENSE for more information.


namespace Confluent.Kafka
{
    /// <summary>
    ///     Context relevant to a serialization or deserialization operation.
    /// </summary>
    public struct SerializationContext
    {
        /// <summary>
        ///     Create a new SerializationContext object instance.
        /// </summary>
        /// <param name="component">
        ///     The component of the message the serialization operation relates to.
        /// </param>
        /// <param name="topic">
        ///     The topic the data is being written to or read from.
        /// </param>
        public SerializationContext(MessageComponentType component, string topic)
        {
            Component = component;
            Topic = topic;
        }

        /// <summary>
        ///     The topic the data is being written to or read from.
        /// </summary>
        public string Topic { get; private set; }
        
        /// <summary>
        ///     The component of the message the serialization operation relates to.
        /// </summary>
        public MessageComponentType Component { get; private set; }
    }
}
