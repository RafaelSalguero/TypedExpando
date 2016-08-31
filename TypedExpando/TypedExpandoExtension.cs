using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypedExpando
{
    /// <summary>
    /// Dynamic extension that supports adding and removing basic get/set properties
    /// </summary>
    class TypedExpandoExtension : IDynamicExtension
    {
        class Property
        {
            public Property(Type PropertyType)
            {
                this.PropertyType = PropertyType;
                //Set the default property value
                this.Value = PropertyType.IsValueType ? Activator.CreateInstance(PropertyType) : null;
            }

            /// <summary>
            /// Property type
            /// </summary>
            public Type PropertyType { get; private set; }

            /// <summary>
            /// Property value
            /// </summary>
            public object Value { get; set; }
        }

        /// <summary>
        /// Property value dictionary
        /// </summary>
        private readonly Dictionary<string, Property> Properties = new Dictionary<string, Property>();

        /// <summary>
        /// Add a new property. Throws an ArgumentException if the property already exists
        /// </summary>
        /// <param name="PropertyName">The property name</param>
        /// <param name="PropertyType">The property type</param>
        public void AddProperty(string PropertyName, Type PropertyType)
        {
            if (Properties.ContainsKey(PropertyName))
                throw new ArgumentException("The property already exists");
            Properties.Add(PropertyName, new Property(PropertyType));
        }

        /// <summary>
        /// Remove a property. Throws an ArgumentException if the property does not exists
        /// </summary>
        /// <param name="PropertyName">The property to remove</param>
        public void RemoveProperty(string PropertyName)
        {
            if (!Properties.ContainsKey(PropertyName))
                throw new ArgumentException("The property does not exists");

            Properties.Remove(PropertyName);
        }

        public IEnumerable<string> MemberNames => Properties.Select(x => x.Key);
        bool IDynamicExtension.CanRead(string PropertyName) => true;
        bool IDynamicExtension.CanWrite(string PropertyName) => true;
        public object Get(string PropertyName) => Properties[PropertyName].Value;
        public void Set(string PropertyName, object Value) => Properties[PropertyName].Value = Value;
        Type IDynamicExtension.GetPropertyType(string PropertyName) => Properties[PropertyName].PropertyType;
        
    }
}
