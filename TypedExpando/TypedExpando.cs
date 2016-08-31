using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicExtensions
{
    /// <summary>
    /// A dynamic class that supports adding typed properties in runtime
    /// </summary>
    public class TypedExpando : DynamicExtensionContainer
    {
        /// <summary>
        /// Create a typed expando without property change notificaton
        /// </summary>
        public TypedExpando() : this(null)
        {

        }

        /// <summary>
        /// Create a typed expando with property change notificaton
        /// </summary>
        /// <param name="RaisePropertyChanged">The function to call when a property is modified. Can be null</param>
        public TypedExpando(PropertyChangedEventHandler RaisePropertyChanged) : base(RaisePropertyChanged)
        {
            this.extension = new TypedExpandoExtension();
            this.AddExtension(extension);
        }

        private readonly TypedExpandoExtension extension;

        /// <summary>
        /// Add a new property to the typed expando
        /// </summary>
        /// <param name="Name">Property name</param>
        /// <param name="Type">Property type</param>
        public void AddProperty(string Name, Type Type)
        {
            extension.AddProperty(Name, Type);
        }

        /// <summary>
        /// Remove a property from the typed expando
        /// </summary>
        /// <param name="Name">The name of the property to remove</param>
        public void RemoveProperty(string Name)
        {
            extension.RemoveProperty(Name);
        }

        /// <summary>
        /// Gets or sets a property by its name
        /// </summary>
        /// <param name="PropertyName">Property name</param>
        /// <returns></returns>
        public object this[string PropertyName]
        {
            get
            {
                return extension.Get(PropertyName);
            }
            set
            {
                extension.Set(PropertyName, value);
            }
        }
    }
}
